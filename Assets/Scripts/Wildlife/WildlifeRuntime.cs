using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer.Unity;

public sealed class WildlifeRuntime :
    IWildlifeRuntime,
    IInitializable,
    ITickable,
    IDisposable
{
    private const int InitialWildlifeTargetCount = 7;
    private const float CarcassFreshnessSeconds = 360f;
    private const float CarcassTickInterval = 2f;

    private readonly IGridSystemProvider gridSystemProvider;
    private readonly IWildlifeSpeciesCatalogProvider speciesCatalog;
    private readonly IGameDataProvider gameDataProvider;
    private readonly IWildlifeEcosystemRuntime ecosystemRuntime;
    private readonly ICombatResolutionService combatResolution;
    private readonly ICombatEquipmentRuntime combatEquipmentRuntime;
    private readonly ICharacterBodyHealthRuntime bodyHealthRuntime;
    private readonly ICombatLineOfSightService lineOfSightService;
    private readonly ICombatCoverQuery coverQuery;
    private readonly ICombatAmmoResupplyRuntime ammoResupplyRuntime;
    private readonly IMainCameraProvider mainCameraProvider;
    private readonly List<WildlifeActor> wildlife = new List<WildlifeActor>();
    private readonly Dictionary<string, WildlifeCarcassFreshnessSaveData> carcassFreshness =
        new Dictionary<string, WildlifeCarcassFreshnessSaveData>(StringComparer.Ordinal);
    private readonly Dictionary<string, float> nextBehaviorTickByWildlifeId =
        new Dictionary<string, float>(StringComparer.Ordinal);
    private WorldItemStackSnapshot[] cachedItemStacks = Array.Empty<WorldItemStackSnapshot>();
    private int cachedItemStackVersion = -1;
    private WorldItemStackSnapshot cachedBestButcherCarcass;
    private int cachedBestButcherCarcassVersion = -1;
    private int nextSequence = 1;
    private bool initialSpawnCompleted;
    private float nextCarcassTickAt;

    public WildlifeRuntime(
        IGridSystemProvider gridSystemProvider,
        IWildlifeSpeciesCatalogProvider speciesCatalog,
        IGameDataProvider gameDataProvider = null,
        IWildlifeEcosystemRuntime ecosystemRuntime = null,
        ICombatResolutionService combatResolution = null,
        ICombatEquipmentRuntime combatEquipmentRuntime = null,
        ICharacterBodyHealthRuntime bodyHealthRuntime = null,
        ICombatLineOfSightService lineOfSightService = null,
        ICombatCoverQuery coverQuery = null,
        ICombatAmmoResupplyRuntime ammoResupplyRuntime = null,
        IMainCameraProvider mainCameraProvider = null)
    {
        this.gridSystemProvider = gridSystemProvider ?? throw new ArgumentNullException(nameof(gridSystemProvider));
        this.speciesCatalog = speciesCatalog ?? throw new ArgumentNullException(nameof(speciesCatalog));
        this.gameDataProvider = gameDataProvider;
        this.ecosystemRuntime = ecosystemRuntime;
        this.combatResolution = combatResolution
            ?? new CombatResolutionService(new UnityCombatRandomSource());
        this.combatEquipmentRuntime = combatEquipmentRuntime;
        this.bodyHealthRuntime = bodyHealthRuntime;
        this.lineOfSightService = lineOfSightService ?? new GridCombatLineOfSightService();
        this.coverQuery = coverQuery ?? new GridCombatCoverQuery();
        this.ammoResupplyRuntime = ammoResupplyRuntime;
        this.mainCameraProvider = mainCameraProvider;
    }

    public static WildlifeRuntime Active { get; private set; }
    public IReadOnlyList<WildlifeActor> Wildlife => wildlife;

    public void Initialize()
    {
        Active = this;
    }

    public void Dispose()
    {
        if (Active == this)
        {
            Active = null;
        }
    }

    public void Tick()
    {
        if (DungeonDebugRuntimeRules.IsEnabled(DungeonDebugCheat.PauseWildlifeAi))
        {
            return;
        }

        if (!gridSystemProvider.TryGetGrid(out Grid grid))
        {
            return;
        }

        if (!initialSpawnCompleted)
        {
            ecosystemRuntime?.EnsureInitialized(grid);
            SpawnInitialWildlife(grid);
            initialSpawnCompleted = true;
        }
        else
        {
            ecosystemRuntime?.EnsureInitialized(grid);
        }

        float now = Time.time;
        Camera mainCamera = mainCameraProvider != null ? mainCameraProvider.Camera : null;
        for (int i = wildlife.Count - 1; i >= 0; i--)
        {
            WildlifeActor actor = wildlife[i];
            if (actor == null || !actor.IsAlive)
            {
                wildlife.RemoveAt(i);
                if (actor != null)
                {
                    nextBehaviorTickByWildlifeId.Remove(actor.WildlifeId);
                }
                continue;
            }

            if (!IsValidCurrentWildlifePosition(grid, actor))
            {
                if (!TryFindNearestInitialSpawnCell(grid, actor.GridPosition, out Vector2Int safePosition))
                {
                    wildlife.RemoveAt(i);
                    nextBehaviorTickByWildlifeId.Remove(actor.WildlifeId);
                    DestroyWildlifeActor(actor);
                    continue;
                }

                actor.WarpTo(safePosition);
                actor.SetTerritoryCenter(safePosition);
                actor.SetHerdAnchor(safePosition);
            }

            actor.Tick(Time.deltaTime);
            ecosystemRuntime?.TickAnimal(actor, grid, Time.deltaTime);
            if (ecosystemRuntime != null && ecosystemRuntime.ShouldRemoveLeavingAnimal(actor, grid))
            {
                wildlife.RemoveAt(i);
                nextBehaviorTickByWildlifeId.Remove(actor.WildlifeId);
                DestroyWildlifeActor(actor);
                continue;
            }

            if (!ShouldTickBehavior(actor, now, mainCamera))
            {
                continue;
            }

            TryResolvePredatorWildlifeContact(actor);
            TickBehavior(actor, grid, now);
        }

        TryRespawnWildlife(grid, now);

        if (now >= nextCarcassTickAt)
        {
            nextCarcassTickAt = now + CarcassTickInterval;
            TickCarcassFreshness();
        }
    }

    public DungeonWildlifeSaveData Capture()
    {
        return new DungeonWildlifeSaveData
        {
            version = DungeonWildlifeSaveData.CurrentVersion,
            nextSequence = Mathf.Max(1, nextSequence),
            wildlife = wildlife
                .Where(actor => actor != null && actor.IsAlive)
                .Select(actor => actor.Capture())
                .ToList(),
            carcasses = carcassFreshness.Values
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.stackId))
                .Select(entry => new WildlifeCarcassFreshnessSaveData
                {
                    stackId = entry.stackId,
                    speciesId = entry.speciesId,
                    remainingFreshnessSeconds = Mathf.Max(0f, entry.remainingFreshnessSeconds)
                })
                .ToList(),
            ecosystem = ecosystemRuntime?.Capture() ?? new DungeonWildlifeEcosystemSaveData()
        };
    }

    public void Restore(DungeonWildlifeSaveData saveData, DungeonGameRestoreReport report = null)
    {
        if (!gridSystemProvider.TryGetGrid(out Grid grid))
        {
            report?.AddWarning("Wildlife runtime could not restore because the grid is not ready.");
            return;
        }

        ClearWildlife();
        carcassFreshness.Clear();
        DungeonWildlifeSaveData source = saveData ?? new DungeonWildlifeSaveData();
        ecosystemRuntime?.Restore(source.ecosystem ?? new DungeonWildlifeEcosystemSaveData());
        nextSequence = Mathf.Max(1, source.nextSequence);
        foreach (WildlifeSaveData entry in source.wildlife ?? Enumerable.Empty<WildlifeSaveData>())
        {
            if (entry == null
                || !speciesCatalog.TryGetSpecies(entry.speciesId, out WildlifeSpeciesDefinition species))
            {
                continue;
            }

            Vector2Int position = new Vector2Int(entry.gridX, entry.gridY);
            if (!CanSpawnAt(grid, position, species.CanEnterDungeon))
            {
                report?.AddWarning($"Wildlife {entry.wildlifeId} had an invalid saved position and was skipped.");
                continue;
            }

            SpawnActor(grid, species, position, entry.wildlifeId, entry);
        }

        foreach (WildlifeCarcassFreshnessSaveData entry in source.carcasses
                     ?? Enumerable.Empty<WildlifeCarcassFreshnessSaveData>())
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.stackId))
            {
                continue;
            }

            carcassFreshness[entry.stackId] = new WildlifeCarcassFreshnessSaveData
            {
                stackId = entry.stackId,
                speciesId = entry.speciesId,
                remainingFreshnessSeconds = Mathf.Max(0f, entry.remainingFreshnessSeconds)
            };
        }

        initialSpawnCompleted = true;
    }

    public bool DebugSpawn(
        string speciesId,
        int amount,
        Vector2Int position,
        out int spawned,
        out string message)
    {
        spawned = 0;
        message = string.Empty;
        if (!gridSystemProvider.TryGetGrid(out Grid grid))
        {
            message = "그리드가 준비되지 않았습니다.";
            return false;
        }

        if (!speciesCatalog.TryGetSpecies(speciesId, out WildlifeSpeciesDefinition species))
        {
            message = "야생동물 종을 찾을 수 없습니다.";
            return false;
        }

        int requested = Mathf.Clamp(amount, 1, 50);
        Vector2Int anchor = position;
        for (int index = 0; index < requested; index++)
        {
            Vector2Int candidate = index == 0
                ? anchor
                : FindNearbySpawnPosition(grid, anchor);
            if (!CanInitialSpawnAt(grid, candidate))
            {
                continue;
            }

            SpawnActor(grid, species, candidate, NextWildlifeId(), null);
            spawned++;
        }

        message = spawned > 0
            ? $"{species.DisplayName} {spawned}마리를 소환했습니다."
            : "선택 칸 주변에 유효한 외부 스폰 칸이 없습니다.";
        return spawned > 0;
    }

    public bool DebugDelete(string wildlifeId)
    {
        WildlifeActor actor = wildlife.FirstOrDefault(candidate => candidate != null
            && string.Equals(candidate.WildlifeId, wildlifeId, StringComparison.Ordinal));
        if (actor == null)
        {
            return false;
        }

        wildlife.Remove(actor);
        nextBehaviorTickByWildlifeId.Remove(actor.WildlifeId);
        DestroyWildlifeActor(actor);
        return true;
    }

    public int DebugDeleteAll()
    {
        int count = wildlife.Count(actor => actor != null);
        ClearWildlife();
        return count;
    }

    public bool HasAvailableHuntJob(CharacterActor actor)
    {
        return TryFindBestHuntTarget(actor, out _);
    }

    public bool TryReserveBestHuntJob(
        CharacterActor actor,
        out WildlifeHuntJob job,
        out string reason)
    {
        job = default;
        reason = string.Empty;
        if (actor == null)
        {
            reason = "사냥할 직원이 없습니다.";
            return false;
        }

        if (!TryFindBestHuntTarget(actor, out WildlifeActor target))
        {
            reason = "지정된 사냥감이 없습니다.";
            return false;
        }

        if (!target.TryReserve(actor))
        {
            reason = "이미 다른 사냥꾼이 추적 중입니다.";
            return false;
        }

        job = new WildlifeHuntJob(target);
        return true;
    }

    public void ReleaseHuntReservation(string wildlifeId, CharacterActor actor)
    {
        if (TryGetWildlife(wildlifeId, out WildlifeActor target))
        {
            target.ReleaseReservation(actor);
        }
    }

    public bool DesignateHunt(string wildlifeId, bool designated, bool priority = false)
    {
        if (!TryGetWildlife(wildlifeId, out WildlifeActor target))
        {
            return false;
        }

        target.SetHuntDesignation(designated, priority);
        return true;
    }

    public bool ApplyHuntHit(CharacterActor hunter, string wildlifeId, out string message)
    {
        return ApplyHuntHitWithCombatCore(hunter, wildlifeId, out message);
    }

    public bool CanAttackHuntTargetFrom(
        CharacterActor hunter,
        WildlifeActor target,
        Grid grid,
        Vector2Int attackerCell)
    {
        if (hunter == null || target == null || !target.IsAlive || grid == null)
        {
            return false;
        }

        ICombatEquipmentRuntime equipment = combatEquipmentRuntime ?? CombatEquipmentRuntime.Active;
        CombatWeaponSnapshot weapon = CombatWeaponSnapshot.CreateUnarmed();
        if (equipment != null)
        {
            equipment.TryGetActiveWeapon(GetCharacterId(hunter), out weapon);
        }
        weapon ??= CombatWeaponSnapshot.CreateUnarmed();

        int distance = Manhattan(attackerCell, target.GridPosition);
        if (!weapon.IsRanged)
        {
            return attackerCell.y == target.GridPosition.y
                && Mathf.Abs(attackerCell.x - target.GridPosition.x) == 1;
        }

        if (distance <= 0 || distance > weapon.MaximumRange)
        {
            return false;
        }

        CombatRangeBand band = CombatRangeRules.GetBand(distance);
        if (weapon.GetAccuracyMultiplier(band) <= 0f
            || weapon.GetDamageMultiplier(band) <= 0f)
        {
            return false;
        }

        CombatLineOfSightResult sight = lineOfSightService.Evaluate(
            grid,
            attackerCell,
            target.GridPosition,
            GetCharacterId(hunter),
            "wildlife:" + target.WildlifeId);
        return sight.HasLineOfSight && !sight.FriendlyFireRisk;
    }

    public bool NeedsHuntReload(CharacterActor hunter)
    {
        ICombatEquipmentRuntime equipment = combatEquipmentRuntime ?? CombatEquipmentRuntime.Active;
        return hunter != null
            && equipment != null
            && equipment.TryGetActiveWeapon(GetCharacterId(hunter), out CombatWeaponSnapshot weapon)
            && weapon != null
            && weapon.RequiresAmmo
            && weapon.LoadedAmmo <= 0;
    }

    public float GetHuntReloadDuration(CharacterActor hunter)
    {
        ICombatEquipmentRuntime equipment = combatEquipmentRuntime ?? CombatEquipmentRuntime.Active;
        if (hunter == null
            || equipment == null
            || !equipment.TryGetActiveWeapon(GetCharacterId(hunter), out CombatWeaponSnapshot weapon)
            || weapon == null)
        {
            return 0f;
        }

        CharacterBodyHealthSnapshot body =
            (bodyHealthRuntime ?? CharacterBodyHealthRuntime.Active)?.GetSnapshot(hunter)
            ?? CreateHealthyBodySnapshot();
        return combatResolution.CalculateReloadTime(
            CreateHunterCombatStats(hunter, body),
            weapon);
    }

    public bool TryReloadHuntWeapon(CharacterActor hunter, out string message)
    {
        message = string.Empty;
        ICombatEquipmentRuntime equipment = combatEquipmentRuntime ?? CombatEquipmentRuntime.Active;
        if (hunter == null
            || equipment == null
            || !equipment.TryGetActiveWeapon(GetCharacterId(hunter), out CombatWeaponSnapshot weapon)
            || weapon == null
            || !weapon.RequiresAmmo)
        {
            return true;
        }

        if (weapon.LoadedAmmo > 0)
        {
            return true;
        }

        if (!equipment.TryReloadFromCharacterInventory(
                GetCharacterId(hunter),
                weapon.InstanceId,
                out int consumed)
            || consumed <= 0)
        {
            if (ammoResupplyRuntime?.TryRequestAmmoResupply(hunter, out string resupplyMessage)
                == true)
            {
                message = string.IsNullOrWhiteSpace(resupplyMessage)
                    ? "창고 탄약 재보급을 시작합니다."
                    : resupplyMessage;
                return false;
            }

            message = $"{weapon.AmmunitionItemId} 탄약이 없습니다.";
            return false;
        }

        message = $"{consumed}발 장전";
        return true;
    }

    public float GetHuntAttackInterval(CharacterActor hunter)
    {
        ICombatEquipmentRuntime equipment = combatEquipmentRuntime ?? CombatEquipmentRuntime.Active;
        CombatWeaponSnapshot weapon = CombatWeaponSnapshot.CreateUnarmed();
        CharacterCombatLoadoutProfile profile = null;
        if (hunter != null && equipment != null)
        {
            string hunterId = GetCharacterId(hunter);
            equipment.TryGetActiveWeapon(hunterId, out weapon);
            profile = equipment.GetActiveProfileSnapshot(hunterId);
        }
        weapon ??= CombatWeaponSnapshot.CreateUnarmed();

        CharacterBodyHealthSnapshot body =
            (bodyHealthRuntime ?? CharacterBodyHealthRuntime.Active)?.GetSnapshot(hunter)
            ?? CreateHealthyBodySnapshot();
        return combatResolution.CalculateAttackInterval(
            CreateHunterCombatStats(hunter, body),
            weapon,
            ResolveSupportedFireMode(weapon, profile?.fireMode ?? CombatFireMode.Aimed));
    }

    private bool ApplyHuntHitWithCombatCore(
        CharacterActor hunter,
        string wildlifeId,
        out string message)
    {
        message = string.Empty;
        if (hunter == null
            || !TryGetWildlife(wildlifeId, out WildlifeActor target)
            || !target.IsAlive)
        {
            message = "사냥 대상이 사라졌습니다.";
            return false;
        }

        if (!gridSystemProvider.TryGetGrid(out Grid grid))
        {
            message = "전투 격자를 찾지 못했습니다.";
            return false;
        }

        ICombatEquipmentRuntime equipment = combatEquipmentRuntime ?? CombatEquipmentRuntime.Active;
        ICharacterBodyHealthRuntime health = bodyHealthRuntime ?? CharacterBodyHealthRuntime.Active;
        string hunterId = GetCharacterId(hunter);
        CombatWeaponSnapshot weapon = CombatWeaponSnapshot.CreateUnarmed();
        if (equipment != null)
        {
            equipment.TryGetActiveWeapon(hunterId, out weapon);
        }
        weapon ??= CombatWeaponSnapshot.CreateUnarmed();

        CharacterCombatLoadoutProfile profile = equipment?.GetActiveProfileSnapshot(hunterId);
        if (weapon.IsRanged && profile?.holdFire == true)
        {
            message = "사격 중지 상태입니다.";
            return false;
        }

        int distance = Manhattan(hunter.GetNowXY(), target.GridPosition);
        if (!weapon.IsRanged
            && (hunter.GetNowXY().y != target.GridPosition.y
                || Mathf.Abs(hunter.GetNowXY().x - target.GridPosition.x) != 1))
        {
            message = "근접 공격은 같은 층의 바로 옆 칸에서만 가능합니다.";
            return false;
        }

        CombatLineOfSightResult sight = weapon.IsRanged
            ? lineOfSightService.Evaluate(
                grid,
                hunter.GetNowXY(),
                target.GridPosition,
                hunterId,
                "wildlife:" + target.WildlifeId)
            : new CombatLineOfSightResult(
                true,
                false,
                default,
                Array.Empty<Vector2Int>(),
                string.Empty);
        CombatFireMode fireMode = ResolveSupportedFireMode(
            weapon,
            profile?.fireMode ?? CombatFireMode.Aimed);
        CharacterBodyHealthSnapshot hunterBody = health?.GetSnapshot(hunter)
            ?? CreateHealthyBodySnapshot();
        CombatAttackResult result = combatResolution.Resolve(new CombatAttackRequest(
            $"hunt:{hunterId}:{target.WildlifeId}:{Time.frameCount}",
            hunterId,
            "wildlife:" + target.WildlifeId,
            CreateHunterCombatStats(hunter, hunterBody),
            CreateWildlifeCombatStats(target),
            weapon,
            distance,
            fireMode,
            weapon.IsRanged
                ? coverQuery.GetCover(grid, hunter.GetNowXY(), target.GridPosition)
                : default,
            hasLineOfSight: sight.HasLineOfSight,
            friendlyFireRisk: sight.FriendlyFireRisk,
            defenderMeleeLocked: distance <= 1,
            attackerSuppression: hunterBody.Suppression,
            attackPowerMultiplier: hunter.GetCombatPowerMultiplier()));
        if (!result.Executed)
        {
            message = ResolveHuntFailureMessage(weapon, distance, sight);
            return false;
        }

        PresentHuntAttack(hunter, target, weapon);
        ConsumeHuntWeapon(equipment, weapon, target.GridPosition);
        if (result.CoverBlocked)
        {
            CombatCoverDurability.TryApplyDamage(result.CoverSourceId, result.CoverDamage);
        }

        target.RegisterThreat(hunter.GetNowXY(), result.Hit ? 0.75f : 0.35f);
        target.SetHuntDesignation(true, target.PriorityHunt);
        int applied = result.Hit ? target.ApplyCombatDamage(result, hunter) : 0;
        bool killed = !target.IsAlive;
        hunter.AddActivity(CharacterActivityEvent.Create(
            CharacterActivityKinds.Combat,
            killed ? CharacterActivityOutcomes.Completed : CharacterActivityOutcomes.Progress,
            killed
                ? $"{GetCharacterDisplayName(hunter)}이(가) {target.DisplayName} 사냥을 끝냈다."
                : result.Hit
                    ? $"{GetCharacterDisplayName(hunter)}이(가) {target.DisplayName}의 {GetBodyPartName(result.BodyPart)}에 {applied} 피해를 입혔다."
                    : result.CoverBlocked
                        ? $"{GetCharacterDisplayName(hunter)}의 공격이 엄폐물에 막혔다."
                        : $"{GetCharacterDisplayName(hunter)}의 공격을 {target.DisplayName}이(가) 피했다.",
            actionId: "survival/hunt",
            targetId: "wildlife:" + target.WildlifeId,
            targetName: target.DisplayName,
            value: applied,
            sentiment: killed ? 0.45f : result.Hit ? 0.1f : -0.1f,
            bubbleEligible: true));

        if (target.RetaliationDamage > 0
            && !killed
            && target.Aggression > 0.45f
            && distance <= 1)
        {
            ApplyWildlifeRetaliation(target, hunter, equipment, health);
        }

        if (killed)
        {
            ecosystemRuntime?.NotifyWildlifeKilled(target, byHunt: true);
            hunter.Progression?.AddExperience(target.IsDangerous ? 20 : 10);
            RecordHuntNarrative(hunter, target);
            SpawnCarcass(target);
            wildlife.Remove(target);
            if (target != null)
            {
                DestroyWildlifeActor(target);
            }
        }

        message = killed
            ? "사냥감 처치"
            : result.Hit
                ? $"{GetBodyPartName(result.BodyPart)} 명중"
                : result.CoverBlocked
                    ? "엄폐물에 막힘"
                    : result.Evaded
                        ? "사냥감이 회피"
                        : "빗나감";
        return true;
    }

    private void ApplyWildlifeRetaliation(
        WildlifeActor wildlifeActor,
        CharacterActor hunter,
        ICombatEquipmentRuntime equipment,
        ICharacterBodyHealthRuntime health)
    {
        if (wildlifeActor == null || hunter == null || hunter.IsDead)
        {
            return;
        }

        string hunterId = GetCharacterId(hunter);
        CharacterBodyHealthSnapshot hunterBody = health?.GetSnapshot(hunter)
            ?? CreateHealthyBodySnapshot();
        CombatWeaponSnapshot naturalWeapon = CreateWildlifeNaturalWeapon(wildlifeActor);
        CombatAttackResult retaliation = combatResolution.Resolve(new CombatAttackRequest(
            $"wildlife-retaliation:{wildlifeActor.WildlifeId}:{hunterId}:{Time.frameCount}",
            "wildlife:" + wildlifeActor.WildlifeId,
            hunterId,
            CreateWildlifeCombatStats(wildlifeActor),
            CreateHunterCombatStats(hunter, hunterBody),
            naturalWeapon,
            1,
            CombatFireMode.Aimed,
            default,
            defenderDowned: hunterBody.Downed,
            defenderMeleeLocked: true,
            defenderSuppression: hunterBody.Suppression,
            defenderArmor: equipment?.GetArmor(hunterId),
            defenderShield: equipment?.GetShield(hunterId) ?? default));
        if (!retaliation.Executed)
        {
            return;
        }

        DefenseCombatPresentation.Ensure(hunter)?.PlayHit(retaliation.AppliedDamage);
        if (retaliation.Hit)
        {
            if (health != null)
            {
                health.ApplyCombatResult(
                    hunter,
                    retaliation,
                    $"{wildlifeActor.DisplayName}의 반격");
            }
            else
            {
                hunter.ApplyDamage(retaliation.AppliedDamage, wildlifeActor.DisplayName + "의 반격");
            }

            ApplyArmorDurabilityDamage(equipment, retaliation);
            hunter.ApplyMoodFactor(
                "survival:hunt:retaliation",
                $"{wildlifeActor.DisplayName}에게 반격당함",
                -4f,
                180f,
                1);
        }
        else if (health != null)
        {
            health.AddSuppression(hunter, retaliation.Suppression);
        }
    }

    private static CombatWeaponSnapshot CreateWildlifeNaturalWeapon(WildlifeActor actor)
    {
        float baseDamage = Mathf.Max(2f, actor?.RetaliationDamage ?? 2);
        return new CombatWeaponSnapshot(
            "combat:wildlife-natural",
            string.Empty,
            CombatEquipmentKind.MeleeWeapon,
            new MeleeStrikeVerb
            {
                attackTime = 1.05f,
                baseDamage = baseDamage,
                penetration = Mathf.Max(0f, baseDamage * 0.2f),
                damageType = CombatDamageType.Pierce,
                tracking = 0.08f
            },
            new[]
            {
                new CombatRangeProfile
                {
                    band = CombatRangeBand.Contact,
                    accuracyMultiplier = 1f,
                    damageMultiplier = 1f
                }
            },
            1,
            CombatEquipmentQuality.Normal,
            string.Empty,
            0,
            0,
            0f,
            false,
            false,
            false);
    }

    private static CombatStatSnapshot CreateHunterCombatStats(
        CharacterActor hunter,
        CharacterBodyHealthSnapshot body)
    {
        if (hunter == null)
        {
            return default;
        }

        float healthRatio = Mathf.Clamp01(hunter.CurrentHealth / Mathf.Max(1f, hunter.MaxHealth));
        float bodyEfficiency = Mathf.Min(
            body.Consciousness,
            Mathf.Lerp(0.5f, 1f, body.Manipulation));
        return new CombatStatSnapshot(
            hunter.GetCharacterStat(CharacterStatType.Attack),
            hunter.GetCharacterStat(CharacterStatType.Shooting),
            hunter.GetCharacterStat(CharacterStatType.Evasion),
            hunter.GetCharacterStat(CharacterStatType.MoveSpeed) * body.Mobility,
            hunter.GetCharacterStat(CharacterStatType.Strength),
            hunter.GetCharacterStat(CharacterStatType.Toughness),
            hunter.GetCharacterStat(CharacterStatType.Dexterity) * body.Manipulation,
            healthRatio * bodyEfficiency);
    }

    private static CombatStatSnapshot CreateWildlifeCombatStats(WildlifeActor actor)
    {
        if (actor == null)
        {
            return default;
        }

        float speed = Mathf.Max(0.5f, actor.Species?.MoveSpeed ?? 1f);
        float mobility = actor.CombatMobility;
        float health = Mathf.Clamp01(actor.CurrentHealth / Mathf.Max(1f, actor.MaxHealth));
        return new CombatStatSnapshot(
            melee: Mathf.Clamp(3f + actor.RetaliationDamage * 0.45f, 2f, 14f),
            shooting: 0f,
            evasion: Mathf.Clamp(2f + speed * 3f, 2f, 14f) * mobility,
            moveSpeed: Mathf.Clamp(3f + speed * 3f, 3f, 14f) * mobility,
            strength: Mathf.Clamp(2f + actor.RetaliationDamage * 0.5f, 2f, 15f),
            toughness: Mathf.Clamp(actor.MaxHealth * 0.12f, 1f, 16f),
            dexterity: Mathf.Clamp(2f + speed * 2.5f, 2f, 14f) * mobility,
            healthMultiplier: health);
    }

    private static CharacterBodyHealthSnapshot CreateHealthyBodySnapshot()
    {
        return new CharacterBodyHealthSnapshot(
            Array.Empty<CharacterBodyPartHealthState>(),
            0f,
            0f,
            1f,
            1f,
            1f,
            false);
    }

    private static CombatFireMode ResolveSupportedFireMode(
        CombatWeaponSnapshot weapon,
        CombatFireMode requested)
    {
        if (weapon == null)
        {
            return CombatFireMode.Aimed;
        }

        return requested switch
        {
            CombatFireMode.Rapid when weapon.SupportsRapid => CombatFireMode.Rapid,
            CombatFireMode.Suppressive when weapon.SupportsSuppressive => CombatFireMode.Suppressive,
            _ => CombatFireMode.Aimed
        };
    }

    private static string ResolveHuntFailureMessage(
        CombatWeaponSnapshot weapon,
        int distance,
        CombatLineOfSightResult sight)
    {
        if (weapon == null)
        {
            return "사용할 무기가 없습니다.";
        }

        if (distance > weapon.MaximumRange || (!weapon.IsRanged && distance > 1))
        {
            return "무기 사거리 밖입니다.";
        }

        if (weapon.IsRanged && !sight.HasLineOfSight)
        {
            return "사선이 막혔습니다.";
        }

        if (weapon.IsRanged && sight.FriendlyFireRisk)
        {
            return "아군이 사선에 있어 사격을 보류합니다.";
        }

        if (weapon.RequiresAmmo && weapon.LoadedAmmo <= 0)
        {
            return "장전된 탄약이 없습니다.";
        }

        return "공격할 수 없습니다.";
    }

    private static void PresentHuntAttack(
        CharacterActor hunter,
        WildlifeActor target,
        CombatWeaponSnapshot weapon)
    {
        if (hunter == null || target == null)
        {
            return;
        }

        DefenseCombatPresentation.Ensure(hunter)?.PlayAttack(target.transform.position);
        if (!weapon.IsRanged)
        {
            return;
        }

        float projectileSpeed = weapon.Verb switch
        {
            ProjectileVerb projectile => projectile.projectileSpeed,
            RecoverableThrowVerb recoverable => recoverable.projectileSpeed,
            _ => 12f
        };
        CombatProjectilePresentation.Launch(
            hunter.transform.position,
            target.transform.position,
            projectileSpeed,
            weapon.Verb?.damageType ?? CombatDamageType.Pierce);
    }

    private static void ConsumeHuntWeapon(
        ICombatEquipmentRuntime equipment,
        CombatWeaponSnapshot weapon,
        Vector2Int impactPosition)
    {
        if (equipment == null || weapon == null)
        {
            return;
        }

        if (weapon.RequiresAmmo && !string.IsNullOrWhiteSpace(weapon.InstanceId))
        {
            equipment.TryConsumeLoadedAmmo(weapon.InstanceId);
            return;
        }

        if (weapon.Verb?.DropsWeaponOnUse != true
            || string.IsNullOrWhiteSpace(weapon.InstanceId)
            || string.IsNullOrWhiteSpace(weapon.DefinitionId)
            || WorldItemStackRuntime.Active == null
            || !WorldItemStackRuntime.Active.SpawnUniqueItemAt(
                DungeonItemCatalogSO.EquipmentItemId(weapon.DefinitionId),
                impactPosition,
                WorldItemStackState.Loose,
                string.Empty,
                out string stackId))
        {
            return;
        }

        equipment.TryLinkToWorldStack(
            weapon.InstanceId,
            stackId,
            CombatEquipmentWorldState.Loose);
    }

    private static void ApplyArmorDurabilityDamage(
        ICombatEquipmentRuntime equipment,
        CombatAttackResult result)
    {
        if (equipment == null)
        {
            return;
        }

        if (result.ArmorDurabilityHits.Count > 0)
        {
            for (int i = 0; i < result.ArmorDurabilityHits.Count; i++)
            {
                CombatArmorDurabilityHit hit = result.ArmorDurabilityHits[i];
                equipment.TryApplyDurabilityDamage(hit.InstanceId, hit.Damage);
            }

            return;
        }

        if (!string.IsNullOrWhiteSpace(result.ArmorInstanceId))
        {
            equipment.TryApplyDurabilityDamage(
                result.ArmorInstanceId,
                result.ArmorDurabilityDamage);
        }
    }

    private static string GetCharacterId(CharacterActor actor)
    {
        string persistentId = actor?.Identity?.PersistentId;
        return !string.IsNullOrWhiteSpace(persistentId)
            ? persistentId
            : $"scene-actor:{actor?.GetInstanceID() ?? 0}";
    }

    private static string GetCharacterDisplayName(CharacterActor actor)
    {
        string displayName = actor?.Identity?.DisplayName;
        return !string.IsNullOrWhiteSpace(displayName)
            ? displayName
            : actor != null ? actor.name : "사냥꾼";
    }

    private static string GetBodyPartName(CombatBodyPart bodyPart)
    {
        return bodyPart switch
        {
            CombatBodyPart.Head => "머리",
            CombatBodyPart.Torso => "몸통",
            CombatBodyPart.LeftArm => "왼앞다리",
            CombatBodyPart.RightArm => "오른앞다리",
            CombatBodyPart.LeftLeg => "왼뒷다리",
            CombatBodyPart.RightLeg => "오른뒷다리",
            _ => "몸"
        };
    }

    private static int Manhattan(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public bool TryButcherNextCarcass(
        CharacterActor butcher,
        BuildableObject building,
        out int produced,
        out string message)
    {
        produced = 0;
        message = string.Empty;
        if (WorldItemStackRuntime.Active == null)
        {
            message = "아이템 런타임이 없습니다.";
            return false;
        }

        WorldItemStackSnapshot carcass = FindBestButcherCarcass();
        if (carcass == null)
        {
            message = "도축할 사체가 없습니다.";
            return false;
        }

        if (string.Equals(carcass.ItemId, DarkSurvivalItemDefinitions.HumanoidCorpseItemId, StringComparison.Ordinal))
        {
            return TryButcherHumanoidCorpse(butcher, building, carcass, out produced, out message);
        }

        if (!WildlifeItemDefinitions.TryGetSpeciesIdFromCarcass(carcass.ItemId, out string speciesId)
            || !speciesCatalog.TryGetSpecies(speciesId, out WildlifeSpeciesDefinition species))
        {
            message = "알 수 없는 사체입니다.";
            return false;
        }

        if (!WorldItemStackRuntime.Active.DeleteStack(carcass.StackId))
        {
            message = "사체 스택을 소비하지 못했습니다.";
            return false;
        }

        carcassFreshness.Remove(carcass.StackId);
        Vector2Int outputPosition = building != null ? building.centerPos : carcass.Position;
        foreach (WildlifeButcherYield yieldItem in species.ButcherYields)
        {
            if (yieldItem == null || yieldItem.amount <= 0)
            {
                continue;
            }

            if (WorldItemStackRuntime.Active.SpawnItemAt(
                    yieldItem.itemId,
                    yieldItem.amount,
                    outputPosition,
                    WorldItemStackState.Loose,
                    string.Empty,
                    out int spawned))
            {
                produced += spawned;
            }
        }

        butcher?.AddActivity(CharacterActivityEvent.Work(
            FacilityWorkType.Butcher,
            produced > 0 ? CharacterActivityOutcomes.Completed : CharacterActivityOutcomes.Failed,
            produced > 0
                ? $"{species.DisplayName} 사체를 손질해 식량과 부산물을 얻었다."
                : $"{species.DisplayName} 사체 손질에 실패했다.",
            building,
            reasonCode: "wildlife-butchered",
            quantity: produced,
            bubbleEligible: produced <= 0));
        message = produced > 0 ? "도축 완료" : "도축 산출 없음";
        return produced > 0;
    }

    public bool HasButcherWorkAvailable(BuildableObject building)
    {
        return building != null && FindBestButcherCarcass() != null;
    }

    public float GetButcherWorkUrgency()
    {
        int carcasses = WorldItemStackRuntime.Active == null
            ? 0
            : GetCachedItemStacks()
                .Count(stack => stack != null
                    && (WildlifeItemDefinitions.TryGetSpeciesIdFromCarcass(stack.ItemId, out _)
                        || (string.Equals(stack.ItemId, DarkSurvivalItemDefinitions.HumanoidCorpseItemId, StringComparison.Ordinal)
                            && stack.EmergencyButcheryAllowed)));
        return Mathf.Clamp(carcasses * 22f, 0f, 90f);
    }

    private static bool TryButcherHumanoidCorpse(
        CharacterActor butcher,
        BuildableObject building,
        WorldItemStackSnapshot carcass,
        out int produced,
        out string message)
    {
        produced = 0;
        message = string.Empty;
        if (carcass == null || !carcass.EmergencyButcheryAllowed || WorldItemStackRuntime.Active == null)
        {
            message = "비상 도축이 허용되지 않은 사체입니다.";
            return false;
        }

        if (!WorldItemStackRuntime.Active.DeleteStack(carcass.StackId))
        {
            message = "사체를 소비하지 못했습니다.";
            return false;
        }

        Vector2Int outputPosition = building != null ? building.centerPos : carcass.Position;
        if (WorldItemStackRuntime.Active.SpawnItemAt(
                DarkSurvivalItemDefinitions.HumanoidMeatItemId,
                4,
                outputPosition,
                WorldItemStackState.Loose,
                string.Empty,
                out int meat))
        {
            produced += meat;
        }

        if (WorldItemStackRuntime.Active.SpawnItemAt(
                DarkSurvivalItemDefinitions.BoneItemId,
                2,
                outputPosition,
                WorldItemStackState.Loose,
                string.Empty,
                out int bone))
        {
            produced += bone;
        }

        if (butcher != null)
        {
            bool sameSpecies = string.Equals(
                butcher.SpeciesTag,
                carcass.SourceSpeciesTag,
                StringComparison.OrdinalIgnoreCase);
            butcher.ApplyMoodFactor(
                "survival:emergency-butchery",
                sameSpecies ? "동족의 사체를 손질함" : "인간형 사체를 손질함",
                sameSpecies ? -16f : -9f,
                900f,
                1);
            butcher.ChangesStat(CharacterCondition.HYGIENE, -18f);
            CharacterDeprivationRuntime.Active?.RecordTaboo(
                butcher,
                $"{(string.IsNullOrWhiteSpace(carcass.SourceDisplayName) ? "이름 모를 자" : carcass.SourceDisplayName)}의 사체를 비상 도축했다");
            butcher.Progression?.RecordNarrative(
                CharacterNarrativeDomain.Survival,
                "survival/taboo-butchery",
                carcass.SourceCharacterId,
                sameSpecies ? "same-species" : "humanoid",
                produced,
                0);
            CharacterDeprivationRuntime.Active?.RecordTabooWitnesses(
                butcher,
                outputPosition,
                "인간형 사체의 비상 도축을 목격함",
                sameSpecies ? -10f : -7f);
        }

        message = produced > 0 ? "비상 도축 완료" : "도축 산출 없음";
        return produced > 0;
    }

    private void SpawnInitialWildlife(Grid grid)
    {
        if (wildlife.Count >= InitialWildlifeTargetCount)
        {
            return;
        }

        List<Vector2Int> candidates = GetSpawnCandidates(grid).ToList();
        int attempts = 0;
        while (wildlife.Count < InitialWildlifeTargetCount && candidates.Count > 0 && attempts < 80)
        {
            attempts++;
            WildlifeSpeciesDefinition species = speciesCatalog.GetRandomSpecies();
            int herdCount = Mathf.Clamp(species.HerdSize, 1, InitialWildlifeTargetCount - wildlife.Count);
            Vector2Int anchor = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            for (int i = 0; i < herdCount && wildlife.Count < InitialWildlifeTargetCount; i++)
            {
                Vector2Int position = i == 0
                    ? anchor
                    : FindNearbySpawnPosition(grid, anchor);
                if (!CanInitialSpawnAt(grid, position))
                {
                    continue;
                }

                SpawnActor(grid, species, position, NextWildlifeId(), null);
            }
        }
    }

    private void TryRespawnWildlife(Grid grid, float now)
    {
        if (ecosystemRuntime == null
            || wildlife.Count >= InitialWildlifeTargetCount + 6
            || !ecosystemRuntime.TryConsumeRespawnOpportunity(
                now,
                wildlife.Count(actor => actor != null && actor.IsAlive),
                speciesCatalog.All,
                out WildlifeSpeciesDefinition species))
        {
            return;
        }

        List<Vector2Int> candidates = GetSpawnCandidates(grid).ToList();
        if (candidates.Count == 0)
        {
            return;
        }

        for (int attempt = 0; attempt < 24; attempt++)
        {
            Vector2Int position = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            if (!CanInitialSpawnAt(grid, position))
            {
                continue;
            }

            WildlifeActor actor = SpawnActor(grid, species, position, NextWildlifeId(), null);
            actor.SetIntent(WildlifeIntent.ReturnToTerritory, "서식지가 회복되어 돌아옴");
            return;
        }
    }

    private WildlifeActor SpawnActor(
        Grid grid,
        WildlifeSpeciesDefinition species,
        Vector2Int position,
        string wildlifeId,
        WildlifeSaveData saveData)
    {
        GameObject gameObject = new GameObject("Wildlife");
        DungeonRuntimeHierarchy.Parent(gameObject, DungeonRuntimeHierarchy.Wildlife);
        WildlifeActor actor = gameObject.AddComponent<WildlifeActor>();
        actor.Initialize(grid, species, wildlifeId, position, saveData);
        wildlife.Add(actor);
        return actor;
    }

    private void ClearWildlife()
    {
        for (int i = wildlife.Count - 1; i >= 0; i--)
        {
            WildlifeActor actor = wildlife[i];
            if (actor != null)
            {
                DestroyWildlifeActor(actor);
            }
        }

        wildlife.Clear();
        nextBehaviorTickByWildlifeId.Clear();
    }

    private static void DestroyWildlifeActor(WildlifeActor actor)
    {
        if (actor == null)
        {
            return;
        }

        actor.PrepareForDespawn();
        UnityEngine.Object.Destroy(actor.gameObject);
    }

    private bool ShouldTickBehavior(WildlifeActor actor, float now, Camera mainCamera)
    {
        if (actor == null || !actor.IsAlive)
        {
            return false;
        }

        string id = actor.WildlifeId;
        if (string.IsNullOrWhiteSpace(id))
        {
            return true;
        }

        bool urgent = actor.HuntDesignated
            || actor.State == WildlifeState.Hunted
            || actor.State == WildlifeState.Fleeing
            || actor.State == WildlifeState.Retaliating
            || actor.State == WildlifeState.PredatorStalking
            || actor.IsDangerous;
        bool visible = IsVisible(mainCamera, actor);
        float interval = urgent
            ? 0.25f
            : visible ? 0.75f : UnityEngine.Random.Range(1.5f, 3.5f);

        if (nextBehaviorTickByWildlifeId.TryGetValue(id, out float nextTickAt)
            && now < nextTickAt)
        {
            return false;
        }

        nextBehaviorTickByWildlifeId[id] = now + interval;
        return true;
    }

    private static bool IsVisible(Camera camera, WildlifeActor actor)
    {
        if (camera == null || actor == null)
        {
            return true;
        }

        Vector3 viewport = camera.WorldToViewportPoint(actor.transform.position);
        return viewport.z > 0f
            && viewport.x >= -0.1f
            && viewport.x <= 1.1f
            && viewport.y >= -0.1f
            && viewport.y <= 1.1f;
    }

    private string NextWildlifeId()
    {
        return "wild:" + nextSequence++;
    }

    private bool TryFindBestHuntTarget(CharacterActor hunter, out WildlifeActor target)
    {
        target = null;
        if (hunter == null || !gridSystemProvider.TryGetGrid(out Grid grid))
        {
            return false;
        }

        Vector2Int start = hunter.GetNowXY();
        if (!GridPathSearchBroker.TryGetSearch(grid, start, () => true, out GridPathSearchResult search))
        {
            return false;
        }

        string hunterId = hunter.Identity != null ? hunter.Identity.PersistentId : hunter.name;
        int bestPriority = -1;
        int bestDistance = int.MaxValue;
        bool bestDangerous = false;
        foreach (WildlifeActor candidate in wildlife)
        {
            if (candidate == null
                || !candidate.IsAlive
                || !candidate.HuntDesignated
                || (!string.IsNullOrWhiteSpace(candidate.ReservedByPersistentId)
                    && candidate.ReservedByPersistentId != hunterId)
                || !search.ContainsVisitableOccupant(candidate))
            {
                continue;
            }

            int priority = candidate.PriorityHunt ? 1 : 0;
            int distance = search.GetMoveDistanceTo(candidate);
            bool dangerous = candidate.IsDangerous;
            if (target == null
                || priority > bestPriority
                || (priority == bestPriority && distance < bestDistance)
                || (priority == bestPriority && distance == bestDistance && dangerous && !bestDangerous))
            {
                target = candidate;
                bestPriority = priority;
                bestDistance = distance;
                bestDangerous = dangerous;
            }
        }

        return target != null;
    }

    private bool TryGetWildlife(string wildlifeId, out WildlifeActor target)
    {
        string normalized = wildlifeId?.Trim() ?? string.Empty;
        target = wildlife.FirstOrDefault(candidate =>
            candidate != null
            && string.Equals(candidate.WildlifeId, normalized, StringComparison.Ordinal));
        return target != null;
    }

    private void TickBehavior(WildlifeActor actor, Grid grid, float now)
    {
        if (actor == null || !actor.IsAlive || !actor.CanRepath(now))
        {
            return;
        }

        Vector2Int target = actor.State switch
        {
            WildlifeState.Fleeing => ChooseFleePosition(actor, grid),
            WildlifeState.Hunted => ChooseHuntedMovePosition(actor, grid),
            WildlifeState.Retaliating => ChooseHuntedMovePosition(actor, grid),
            WildlifeState.PredatorStalking => ChooseEcologyOrPredatorPosition(actor, grid),
            WildlifeState.Leaving => ChooseLeavingPosition(actor, grid),
            _ => ChooseEcologyOrWanderPosition(actor, grid)
        };

        actor.TrySetPath(target, now);
    }

    private Vector2Int ChooseEcologyOrPredatorPosition(WildlifeActor actor, Grid grid)
    {
        if (ecosystemRuntime != null
            && ecosystemRuntime.TryChooseEcologyTarget(
                actor,
                grid,
                wildlife,
                GetCachedItemStacks(),
                out Vector2Int target,
                out WildlifeIntent intent,
                out string reason))
        {
            actor.SetIntent(intent, reason);
            if (intent == WildlifeIntent.LeaveMap)
            {
                actor.MarkLeaving();
            }
            else if (intent != WildlifeIntent.HuntPrey)
            {
                actor.SetGrazing();
            }

            return target;
        }

        return ChoosePredatorPosition(actor, grid);
    }

    private Vector2Int ChooseLeavingPosition(WildlifeActor actor, Grid grid)
    {
        int exitX = actor.GridPosition.x < grid.width * 0.5f ? 0 : grid.width - 1;
        Vector2Int target = new Vector2Int(exitX, actor.GridPosition.y);
        if (CanWildlifeRoamTargetAt(grid, target, actor.CanEnterDungeon))
        {
            actor.SetIntent(WildlifeIntent.LeaveMap, "먹이와 물을 찾아 지역을 떠남");
            return target;
        }

        return ChooseReachablePosition(actor, grid, minDistance: 4, maxDistance: 10, preferAwayFrom: actor.TerritoryCenter);
    }

    private Vector2Int ChooseEcologyOrWanderPosition(WildlifeActor actor, Grid grid)
    {
        if (ecosystemRuntime != null
            && ecosystemRuntime.TryChooseEcologyTarget(
                actor,
                grid,
                wildlife,
                GetCachedItemStacks(),
                out Vector2Int target,
                out WildlifeIntent intent,
                out string reason))
        {
            actor.SetIntent(intent, reason);
            switch (intent)
            {
                case WildlifeIntent.Forage:
                case WildlifeIntent.Drink:
                    actor.SetGrazing();
                    break;
                case WildlifeIntent.HuntPrey:
                    actor.SetPredatorStalking();
                    break;
                case WildlifeIntent.LeaveMap:
                    actor.MarkLeaving();
                    break;
                default:
                    actor.SetIdle();
                    break;
            }

            return target;
        }

        return ChooseWanderPosition(actor, grid);
    }

    private Vector2Int ChooseWanderPosition(WildlifeActor actor, Grid grid)
    {
        if (actor.Fear >= 4f || (actor.HasLastThreatPosition && actor.LastThreatAge < 12f))
        {
            actor.SetIntent(WildlifeIntent.Flee, "위협을 피해 도망");
            return ChooseFleePosition(actor, grid);
        }

        if (actor.Species != null
            && actor.Species.IsPredator
            && (actor.Hunger >= 0.55f || UnityEngine.Random.value < actor.Species.Aggression * 0.18f))
        {
            actor.SetIntent(WildlifeIntent.HuntPrey, "먹잇감을 찾는 중");
            actor.SetPredatorStalking();
            return ChoosePredatorPosition(actor, grid);
        }

        actor.SetIntent(WildlifeIntent.Wander, "영역 안을 배회");
        actor.SetGrazing();
        return ChooseReachablePosition(actor, grid, minDistance: 2, maxDistance: 6, preferAwayFrom: null);
    }

    private Vector2Int ChooseHuntedMovePosition(WildlifeActor actor, Grid grid)
    {
        CharacterActor hunter = FindCharacterByPersistentId(actor.ReservedByPersistentId);
        if (hunter != null)
        {
            return ChooseReachablePosition(actor, grid, minDistance: 3, maxDistance: 8, preferAwayFrom: hunter.GetNowXY());
        }

        return ChooseFleePosition(actor, grid);
    }

    private Vector2Int ChooseFleePosition(WildlifeActor actor, Grid grid)
    {
        CharacterActor nearest = FindNearestWorker(actor.GridPosition);
        Vector2Int? awayFrom = actor.HasLastThreatPosition && actor.LastThreatAge < 20f
            ? actor.LastThreatPosition
            : nearest != null ? nearest.GetNowXY() : null;
        return ChooseReachablePosition(actor, grid, minDistance: 4, maxDistance: 10, preferAwayFrom: awayFrom);
    }

    private Vector2Int ChoosePredatorPosition(WildlifeActor actor, Grid grid)
    {
        CharacterActor target = FindBestPredatorTarget(actor);
        if (target == null)
        {
            return ChooseReachablePosition(actor, grid, minDistance: 2, maxDistance: 6, preferAwayFrom: null);
        }

        return target.GetNowXY();
    }

    private Vector2Int ChooseReachablePosition(
        WildlifeActor actor,
        Grid grid,
        int minDistance,
        int maxDistance,
        Vector2Int? preferAwayFrom)
    {
        Vector2Int origin = actor.GridPosition;
        Vector2Int best = origin;
        float bestScore = float.NegativeInfinity;
        int samples = 0;
        int clampedMin = Mathf.Max(1, minDistance);
        int clampedMax = Mathf.Max(clampedMin, maxDistance);
        for (int distance = clampedMin; distance <= clampedMax; distance++)
        {
            for (int direction = -1; direction <= 1; direction += 2)
            {
                Vector2Int candidate = new Vector2Int(origin.x + direction * distance, origin.y);
                if (!CanWildlifeRoamTargetAt(grid, candidate, actor.CanEnterDungeon))
                {
                    continue;
                }

                float score = ScoreWildlifeMovePosition(actor, grid, candidate, preferAwayFrom);
                if (score > bestScore)
                {
                    best = candidate;
                    bestScore = score;
                }

                samples++;
            }
        }

        if (samples <= 0)
        {
            return origin;
        }

        Vector2Int selected = best;
        float accumulatedWeight = 0f;
        const float viableScoreRange = 5f;
        for (int distance = clampedMin; distance <= clampedMax; distance++)
        {
            for (int direction = -1; direction <= 1; direction += 2)
            {
                Vector2Int candidate = new Vector2Int(origin.x + direction * distance, origin.y);
                if (!CanWildlifeRoamTargetAt(grid, candidate, actor.CanEnterDungeon))
                {
                    continue;
                }

                float score = ScoreWildlifeMovePosition(actor, grid, candidate, preferAwayFrom);
                if (score < bestScore - viableScoreRange)
                {
                    continue;
                }

                float weight = Mathf.Exp((score - bestScore) * 0.55f);
                accumulatedWeight += weight;
                if (UnityEngine.Random.value * accumulatedWeight <= weight)
                {
                    selected = candidate;
                }
            }
        }

        return selected;
    }

    private static float ScoreWildlifeMovePosition(
        WildlifeActor actor,
        Grid grid,
        Vector2Int position,
        Vector2Int? preferAwayFrom)
    {
        float score = 0f;
        GridCell cell = grid.GetGridCell(position);
        GridCellAreaType areaType = cell != null ? cell.AreaType : GridCellAreaType.BlockedExterior;
        if (preferAwayFrom.HasValue)
        {
            Vector2Int threat = preferAwayFrom.Value;
            int distanceFromThreat = Mathf.Abs(position.x - threat.x) + Mathf.Abs(position.y - threat.y);
            score += distanceFromThreat * 4f;
        }
        else
        {
            int territoryDistance = Mathf.Abs(position.x - actor.TerritoryCenter.x)
                + Mathf.Abs(position.y - actor.TerritoryCenter.y);
            int herdDistance = Mathf.Abs(position.x - actor.HerdAnchorPosition.x)
                + Mathf.Abs(position.y - actor.HerdAnchorPosition.y);
            score += Mathf.Clamp(12f - territoryDistance, -8f, 12f);
            score += Mathf.Clamp(7f - herdDistance, -4f, 7f);
            score += actor.Hunger * (areaType == GridCellAreaType.ExteriorPath ? 4f : 1f);

            int direction = Mathf.RoundToInt(Mathf.Sign(position.x - actor.GridPosition.x));
            if (direction != 0 && actor.LastHorizontalDirection != 0)
            {
                score += direction == actor.LastHorizontalDirection ? 2.6f : -2.1f;
            }
        }

        if (areaType == GridCellAreaType.Entrance)
        {
            score -= actor.CanEnterDungeon ? 1.5f : 7f;
        }
        else if (areaType == GridCellAreaType.DropZone)
        {
            score -= 2f;
        }
        else if (areaType == GridCellAreaType.DungeonInterior && !actor.CanEnterDungeon)
        {
            score -= 30f;
        }

        score -= CountNearbyCharacters(position, 3) * (actor.Species != null && actor.Species.IsPredator ? 0.8f : 2.6f);
        return score;
    }

    private IEnumerable<Vector2Int> GetSpawnCandidates(Grid grid)
    {
        return grid.GetCells()
            .Where(cell => IsInitialWildlifeSpawnCell(grid, cell))
            .Select(cell => cell.Position);
    }

    private Vector2Int FindNearbySpawnPosition(Grid grid, Vector2Int anchor)
    {
        for (int radius = 1; radius <= 4; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (Mathf.Abs(dx) + Mathf.Abs(dy) > radius)
                    {
                        continue;
                    }

                    Vector2Int candidate = anchor + new Vector2Int(dx, dy);
                    if (CanInitialSpawnAt(grid, candidate))
                    {
                        return candidate;
                    }
                }
            }
        }

        return CanInitialSpawnAt(grid, anchor)
            ? anchor
            : GetSpawnCandidates(grid).FirstOrDefault();
    }

    public static bool IsInitialWildlifeSpawnCell(Grid grid, GridCell cell)
    {
        return cell != null
            && grid != null
            && cell.AreaType == GridCellAreaType.ExteriorPath
            && grid.IsWalkable(cell.Position)
            && IsOutdoorSurfaceCell(grid, cell)
            && !cell.HasOccupantInLayer(GridLayer.Wildlife);
    }

    private bool CanInitialSpawnAt(Grid grid, Vector2Int position)
    {
        return IsInitialWildlifeSpawnCell(grid, grid?.GetGridCell(position));
    }

    private static bool IsValidCurrentWildlifePosition(Grid grid, WildlifeActor actor)
    {
        if (grid == null || actor == null || !grid.IsWalkable(actor.GridPosition))
        {
            return false;
        }

        GridCell cell = grid.GetGridCell(actor.GridPosition);
        if (cell == null || cell.AreaType == GridCellAreaType.BlockedExterior)
        {
            return false;
        }

        if (cell.AreaType == GridCellAreaType.ExteriorPath
            && !IsOutdoorSurfaceCell(grid, cell))
        {
            return false;
        }

        return actor.CanEnterDungeon || cell.AreaType != GridCellAreaType.DungeonInterior;
    }

    private bool TryFindNearestInitialSpawnCell(Grid grid, Vector2Int origin, out Vector2Int position)
    {
        position = default;
        if (grid == null)
        {
            return false;
        }

        int maxRadius = Mathf.Max(grid.width, grid.height);
        for (int radius = 0; radius <= maxRadius; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (Mathf.Abs(dx) + Mathf.Abs(dy) != radius)
                    {
                        continue;
                    }

                    Vector2Int candidate = origin + new Vector2Int(dx, dy);
                    if (!grid.IsValidGridPos(candidate) || !CanInitialSpawnAt(grid, candidate))
                    {
                        continue;
                    }

                    position = candidate;
                    return true;
                }
            }
        }

        GridCell fallback = grid.GetCells().FirstOrDefault(cell => IsInitialWildlifeSpawnCell(grid, cell));
        if (fallback == null)
        {
            return false;
        }

        position = fallback.Position;
        return true;
    }

    private bool CanWildlifeRoamTargetAt(Grid grid, Vector2Int position, bool canEnterDungeon)
    {
        GridCell cell = grid?.GetGridCell(position);
        if (cell == null
            || cell.AreaType == GridCellAreaType.DropZone
            || cell.AreaType == GridCellAreaType.Entrance)
        {
            return false;
        }

        return CanSpawnAt(grid, position, canEnterDungeon);
    }

    private bool CanSpawnAt(Grid grid, Vector2Int position, bool canEnterDungeon)
    {
        GridCell cell = grid?.GetGridCell(position);
        if (cell == null || !grid.IsWalkable(position) || cell.HasOccupantInLayer(GridLayer.Wildlife))
        {
            return false;
        }

        if (cell.AreaType == GridCellAreaType.BlockedExterior)
        {
            return false;
        }

        if (cell.AreaType == GridCellAreaType.ExteriorPath
            && !IsOutdoorSurfaceCell(grid, cell))
        {
            return false;
        }

        return canEnterDungeon || cell.AreaType != GridCellAreaType.DungeonInterior;
    }

    public static bool IsOutdoorSurfaceCell(Grid grid, GridCell cell)
    {
        if (grid == null || cell == null || cell.AreaType != GridCellAreaType.ExteriorPath)
        {
            return false;
        }

        if (cell.Position.y > 0)
        {
            return false;
        }

        Vector2Int belowPosition = new Vector2Int(cell.Position.x, cell.Position.y - 1);
        GridCell below = grid.GetGridCell(belowPosition);
        return below == null || below.AreaType == GridCellAreaType.BlockedExterior;
    }

    private void SpawnCarcass(WildlifeActor target)
    {
        if (target == null || target.Species == null || WorldItemStackRuntime.Active == null)
        {
            return;
        }

        Vector2Int position = target.GridPosition;
        string itemId = target.Species.CarcassItemId;
        if (!WorldItemStackRuntime.Active.SpawnItemAt(
                itemId,
                1,
                position,
                WorldItemStackState.Loose,
                string.Empty,
                out int spawned)
            || spawned <= 0)
        {
            return;
        }

        WorldItemStackSnapshot stack = WorldItemStackRuntime.Active
            .GetStacksAt(position, includeStored: true)
            .LastOrDefault(candidate => candidate != null
                && string.Equals(candidate.ItemId, itemId, StringComparison.Ordinal)
                && !carcassFreshness.ContainsKey(candidate.StackId));
        if (stack == null)
        {
            return;
        }

        carcassFreshness[stack.StackId] = new WildlifeCarcassFreshnessSaveData
        {
            stackId = stack.StackId,
            speciesId = target.SpeciesId,
            remainingFreshnessSeconds = CarcassFreshnessSeconds
        };
    }

    private void TickCarcassFreshness()
    {
        if (WorldItemStackRuntime.Active == null || carcassFreshness.Count == 0)
        {
            return;
        }

        float delta = CarcassTickInterval;
        List<string> expired = null;
        foreach (WildlifeCarcassFreshnessSaveData entry in carcassFreshness.Values)
        {
            entry.remainingFreshnessSeconds -= delta;
            if (entry.remainingFreshnessSeconds <= 0f)
            {
                expired ??= new List<string>();
                expired.Add(entry.stackId);
            }
        }

        if (expired == null)
        {
            return;
        }

        foreach (string stackId in expired)
        {
            WorldItemStackSnapshot stack = GetCachedItemStacks()
                .FirstOrDefault(candidate => candidate != null
                    && string.Equals(candidate.StackId, stackId, StringComparison.Ordinal));
            carcassFreshness.Remove(stackId);
            if (stack == null)
            {
                continue;
            }

            Vector2Int position = stack.Position;
            WorldItemStackRuntime.Active.DeleteStack(stackId);
            WorldItemStackRuntime.Active.SpawnItemAt(
                WildlifeItemDefinitions.RotItemId,
                1,
                position,
                WorldItemStackState.Loose,
                string.Empty,
                out _);
        }
    }

    private WorldItemStackSnapshot FindBestButcherCarcass()
    {
        if (WorldItemStackRuntime.Active == null)
        {
            return null;
        }

        if (cachedBestButcherCarcassVersion == WorldItemStackRuntime.Active.ItemStackVersion)
        {
            return cachedBestButcherCarcass;
        }

        cachedBestButcherCarcassVersion = WorldItemStackRuntime.Active.ItemStackVersion;
        cachedBestButcherCarcass = GetCachedItemStacks()
            .Where(stack => stack != null
                && !stack.Forbidden
                && !stack.IsReserved
                && (WildlifeItemDefinitions.TryGetSpeciesIdFromCarcass(stack.ItemId, out _)
                    || (string.Equals(stack.ItemId, DarkSurvivalItemDefinitions.HumanoidCorpseItemId, StringComparison.Ordinal)
                        && stack.EmergencyButcheryAllowed))
                && (stack.State == WorldItemStackState.Stored
                    || stack.State == WorldItemStackState.Loose
                    || stack.State == WorldItemStackState.FacilityBuffer))
            .OrderBy(stack => stack.State == WorldItemStackState.Stored ? 0 : 1)
            .ThenBy(stack => carcassFreshness.TryGetValue(stack.StackId, out WildlifeCarcassFreshnessSaveData fresh)
                ? fresh.remainingFreshnessSeconds
                : CarcassFreshnessSeconds)
            .FirstOrDefault();
        return cachedBestButcherCarcass;
    }

    private void RecordHuntNarrative(CharacterActor hunter, WildlifeActor target)
    {
        int day = 0;
        if (gameDataProvider != null && gameDataProvider.TryGetGameData(out GameData data))
        {
            day = data.day != null ? data.day.Value : 0;
        }

        hunter.Progression?.RecordNarrative(
            CharacterNarrativeDomain.Survival,
            "survival/hunt",
            target != null ? "wildlife:" + target.SpeciesId : "wildlife",
            target != null && target.IsDangerous ? "dangerous-hunt" : "hunt",
            target != null ? target.MaxHealth : 0f,
            day);
    }

    private static CharacterActor FindCharacterByPersistentId(string persistentId)
    {
        if (string.IsNullOrWhiteSpace(persistentId))
        {
            return null;
        }

        IReadOnlyList<CharacterActor> actors = CharacterAiWorldRegistry.Characters;
        for (int i = 0; i < actors.Count; i++)
        {
            CharacterActor actor = actors[i];
            if (actor != null
                && actor.Identity != null
                && string.Equals(actor.Identity.PersistentId, persistentId, StringComparison.Ordinal))
            {
                return actor;
            }
        }

        return null;
    }

    private static CharacterActor FindNearestWorker(Vector2Int position)
    {
        CharacterActor best = null;
        int bestDistance = int.MaxValue;
        IReadOnlyList<CharacterActor> actors = CharacterAiWorldRegistry.Characters;
        for (int i = 0; i < actors.Count; i++)
        {
            CharacterActor actor = actors[i];
            if (actor == null || actor.IsDead || !CharacterWorkRoleUtility.TryGetWork(actor, out _))
            {
                continue;
            }

            Vector2Int actorPosition = actor.GetNowXY();
            int distance = Mathf.Abs(actorPosition.x - position.x) + Mathf.Abs(actorPosition.y - position.y);
            if (best != null && distance >= bestDistance)
            {
                continue;
            }

            best = actor;
            bestDistance = distance;
        }

        return best;
    }

    private static CharacterActor FindBestPredatorTarget(WildlifeActor predator)
    {
        if (predator == null)
        {
            return null;
        }

        CharacterActor best = null;
        float bestScore = float.MinValue;
        IReadOnlyList<CharacterActor> actors = CharacterAiWorldRegistry.Characters;
        for (int i = 0; i < actors.Count; i++)
        {
            CharacterActor actor = actors[i];
            if (actor == null || actor.IsDead)
            {
                continue;
            }

            Vector2Int actorPosition = actor.GetNowXY();
            int distance = Mathf.Abs(actorPosition.x - predator.GridPosition.x)
                + Mathf.Abs(actorPosition.y - predator.GridPosition.y);
            if (distance > 10)
            {
                continue;
            }

            float healthWeakness = actor.MaxHealth > 0
                ? Mathf.Clamp01(1f - actor.CurrentHealth / Mathf.Max(1f, actor.MaxHealth))
                : 0f;
            float workerPenalty = CharacterWorkRoleUtility.TryGetWork(actor, out _) ? 0.2f : 0f;
            float score = healthWeakness * 5f
                + Mathf.Clamp(10f - distance, 0f, 10f) * 0.45f
                + predator.Hunger * 3f
                - workerPenalty;
            if (best == null || score > bestScore)
            {
                best = actor;
                bestScore = score;
            }
        }

        return best;
    }

    private bool TryResolvePredatorWildlifeContact(WildlifeActor predator)
    {
        if (predator == null
            || !predator.IsAlive
            || predator.Species == null
            || predator.Species.Diet != WildlifeDietType.Carnivore
            || predator.Hunger < 0.45f)
        {
            return false;
        }

        WildlifeActor prey = null;
        float bestScore = float.NegativeInfinity;
        for (int i = 0; i < wildlife.Count; i++)
        {
            WildlifeActor candidate = wildlife[i];
            if (candidate == null
                || candidate == predator
                || !candidate.IsAlive
                || candidate.Species == null
                || candidate.Species.Diet == WildlifeDietType.Carnivore
                || !IsAdjacentCell(predator.GridPosition, candidate.GridPosition))
            {
                continue;
            }

            float weakness = candidate.MaxHealth > 0
                ? 1f - (candidate.CurrentHealth / (float)candidate.MaxHealth)
                : 0f;
            float score = weakness * 5f
                + Mathf.Clamp(predator.MaxHealth - candidate.MaxHealth, -8f, 12f)
                - (candidate.IsDangerous ? 6f : 0f);
            if (prey == null || score > bestScore)
            {
                prey = candidate;
                bestScore = score;
            }
        }

        if (prey == null)
        {
            return false;
        }

        int damage = Mathf.Max(
            1,
            Mathf.RoundToInt((predator.RetaliationDamage * 0.75f) + (predator.MaxHealth * 0.12f)));
        prey.RegisterThreat(predator.GridPosition, 0.65f);
        prey.ApplyDamage(damage, null);
        predator.SetIntent(WildlifeIntent.HuntPrey, prey.IsAlive ? "먹잇감을 몰아붙이는 중" : "먹잇감을 쓰러뜨림");
        predator.ChangeHunger(-0.18f);
        if (prey.IsAlive)
        {
            return true;
        }

        ecosystemRuntime?.NotifyWildlifeKilled(prey, byHunt: false);
        SpawnCarcass(prey);
        wildlife.Remove(prey);
        nextBehaviorTickByWildlifeId.Remove(prey.WildlifeId);
        UnityEngine.Object.Destroy(prey.gameObject);
        predator.ChangeHunger(-0.45f);
        return true;
    }

    private static bool IsAdjacentCell(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) <= 1
            && a != b;
    }

    private IReadOnlyList<WorldItemStackSnapshot> GetCachedItemStacks()
    {
        WorldItemStackRuntime runtime = WorldItemStackRuntime.Active;
        if (runtime == null)
        {
            cachedItemStackVersion = -1;
            cachedItemStacks = Array.Empty<WorldItemStackSnapshot>();
            cachedBestButcherCarcass = null;
            cachedBestButcherCarcassVersion = -1;
            return cachedItemStacks;
        }

        if (cachedItemStackVersion == runtime.ItemStackVersion)
        {
            return cachedItemStacks;
        }

        cachedItemStackVersion = runtime.ItemStackVersion;
        cachedItemStacks = runtime.GetAllStacks()
            .Where(stack => stack != null)
            .ToArray();
        cachedBestButcherCarcass = null;
        cachedBestButcherCarcassVersion = -1;
        return cachedItemStacks;
    }

    private static int CountNearbyCharacters(Vector2Int position, int radius)
    {
        int count = 0;
        IReadOnlyList<CharacterActor> actors = CharacterAiWorldRegistry.Characters;
        for (int i = 0; i < actors.Count; i++)
        {
            CharacterActor actor = actors[i];
            if (actor == null || actor.IsDead)
            {
                continue;
            }

            Vector2Int actorPosition = actor.GetNowXY();
            int distance = Mathf.Abs(actorPosition.x - position.x) + Mathf.Abs(actorPosition.y - position.y);
            if (distance <= radius)
            {
                count++;
            }
        }

        return count;
    }
}

public sealed class SurvivalFoodRuntime :
    ISurvivalFoodRuntime,
    IInitializable,
    IDisposable,
    UtilEventListener<OperatingDayStartedEvent>
{
    private const float DefaultFreshnessSeconds = 360f;
    private const float PreservedFreshnessSeconds = 1440f;
    private const float FreshnessWarningThresholdSeconds = 90f;
    private const int DailyFuelDemand = 1;
    private const float TreatmentMedicineHeal = 16f;

    private readonly IGridSystemProvider gridSystemProvider;
    private readonly IWildlifeSpeciesCatalogProvider speciesCatalog;
    private DungeonSurvivalSaveData state = new DungeonSurvivalSaveData();
    private WorldItemStackSnapshot[] cachedItemStacks = Array.Empty<WorldItemStackSnapshot>();
    private int cachedItemStackVersion = -1;

    public SurvivalFoodRuntime(
        IGridSystemProvider gridSystemProvider,
        IWildlifeSpeciesCatalogProvider speciesCatalog)
    {
        this.gridSystemProvider = gridSystemProvider ?? throw new ArgumentNullException(nameof(gridSystemProvider));
        this.speciesCatalog = speciesCatalog ?? throw new ArgumentNullException(nameof(speciesCatalog));
    }

    public static SurvivalFoodRuntime Active { get; private set; }

    public int GetStoredStockCount(StockCategory category)
    {
        return CountStoredStock(category);
    }

    public int TryConsumeStoredStock(StockCategory category, int amount)
    {
        return WithdrawStock(category, Mathf.Max(0, amount));
    }

    public void Initialize()
    {
        Active = this;
        this.EventStartListening<OperatingDayStartedEvent>();
    }

    public void Dispose()
    {
        this.EventStopListening<OperatingDayStartedEvent>();
        if (Active == this)
        {
            Active = null;
        }
    }

    public void OnTriggerEvent(OperatingDayStartedEvent eventType)
    {
        if (eventType.day <= 0 || state.lastProcessedDay == eventType.day)
        {
            return;
        }

        ProcessDailySurvival(eventType.day);
    }

    private void ProcessDailySurvival(int day)
    {
        EnsureStateLists();
        UpdateWeather(day);
        ProcessSpoilage(advanceTime: true);
        ConsumeDailyFood(day);
        ConsumeDailyWater(day);
        ConsumeDailyFuel();
        RefreshSurvivalRisks();
        ApplyHealthConsequences();
    }

    public DungeonSurvivalSaveData Capture()
    {
        return new DungeonSurvivalSaveData
        {
            version = DungeonSurvivalSaveData.CurrentVersion,
            lastProcessedDay = state.lastProcessedDay,
            lastNeededFood = state.lastNeededFood,
            lastConsumedFood = state.lastConsumedFood,
            lastMissingFood = state.lastMissingFood,
            lastNeededWater = state.lastNeededWater,
            lastConsumedWater = state.lastConsumedWater,
            lastMissingWater = state.lastMissingWater,
            consecutiveFoodShortageDays = state.consecutiveFoodShortageDays,
            consecutiveWaterShortageDays = state.consecutiveWaterShortageDays,
            lastConsumedFuel = state.lastConsumedFuel,
            lastMissingFuel = state.lastMissingFuel,
            currentWeather = state.currentWeather,
            weatherDay = state.weatherDay,
            outdoorTemperature = state.outdoorTemperature,
            sanitationRisk = state.sanitationRisk,
            diseaseRisk = state.diseaseRisk,
            exteriorNightDanger = state.exteriorNightDanger,
            spoilage = (state.spoilage ?? new List<SurvivalFoodSpoilageSaveData>())
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.stackId))
                .Select(entry => new SurvivalFoodSpoilageSaveData
                {
                    stackId = entry.stackId,
                    itemId = entry.itemId,
                    remainingFreshnessSeconds = entry.remainingFreshnessSeconds,
                    preserved = entry.preserved,
                    contaminated = entry.contaminated
                })
                .ToList(),
            health = (state.health ?? new List<SurvivalHealthSaveData>())
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.persistentId))
                .Select(entry => new SurvivalHealthSaveData
                {
                    persistentId = entry.persistentId,
                    state = entry.state,
                    severity = entry.severity,
                    remainingSeconds = entry.remainingSeconds,
                    source = entry.source
                })
                .ToList()
        };
    }

    public void DebugSetWeather(SurvivalWeatherType weather)
    {
        EnsureStateLists();
        state.currentWeather = weather;
        state.outdoorTemperature = weather switch
        {
            SurvivalWeatherType.ColdSnap => -6f,
            SurvivalWeatherType.HeatWave => 34f,
            SurvivalWeatherType.Storm => 12f,
            SurvivalWeatherType.Rain => 14f,
            SurvivalWeatherType.Fog => 16f,
            _ => 18f
        };
        RefreshSurvivalRisks();
    }

    public void DebugAdvanceSpoilage(float seconds)
    {
        EnsureStateLists();
        float advance = Mathf.Max(0f, seconds);
        foreach (SurvivalFoodSpoilageSaveData entry in state.spoilage)
        {
            if (entry != null && !entry.preserved)
            {
                entry.remainingFreshnessSeconds = Mathf.Max(
                    0f,
                    entry.remainingFreshnessSeconds - advance);
            }
        }

        ProcessSpoilage(advanceTime: false);
    }

    public void DebugResetSpoilage()
    {
        EnsureStateLists();
        foreach (SurvivalFoodSpoilageSaveData entry in state.spoilage)
        {
            if (entry != null)
            {
                entry.remainingFreshnessSeconds = entry.preserved
                    ? PreservedFreshnessSeconds
                    : DefaultFreshnessSeconds;
                entry.contaminated = false;
            }
        }
    }

    public void Restore(DungeonSurvivalSaveData saveData)
    {
        state = saveData ?? new DungeonSurvivalSaveData();
        state.version = DungeonSurvivalSaveData.CurrentVersion;
        state.spoilage ??= new List<SurvivalFoodSpoilageSaveData>();
        state.health ??= new List<SurvivalHealthSaveData>();
    }

    public SurvivalFoodOverview GetOverview()
    {
        EnsureStateLists();
        ProcessSpoilage();
        RefreshSurvivalRisks();

        int required = GetSurvivalConsumers().Count();
        int stored = CountStoredFood();
        int looseFood = CountLooseFood();
        int carcasses = CountCarcasses(out int pendingFood);
        int shortageDays = required <= 0
            ? int.MaxValue
            : Mathf.FloorToInt((stored + looseFood + pendingFood) / (float)required);
        int storedWater = CountStoredStock(StockCategory.Water);
        int looseWater = CountLooseStock(StockCategory.Water);
        int storedFuel = CountStoredStock(StockCategory.Fuel);
        int storedMedicine = CountStoredStock(StockCategory.Medicine);
        int sickCount = state.health?.Count(entry => entry != null
            && entry.state is SurvivalHealthState.Sick or SurvivalHealthState.Infected
            && entry.remainingSeconds > 0f) ?? 0;
        int untreatedCount = state.health?.Count(entry => entry != null
            && entry.state is SurvivalHealthState.Sick or SurvivalHealthState.Infected
            && entry.severity >= 0.5f
            && entry.remainingSeconds > 0f) ?? 0;
        int spoilageWarnings = CountSpoilageWarnings();
        return new SurvivalFoodOverview(
            required,
            stored,
            looseFood,
            carcasses,
            pendingFood,
            shortageDays,
            required,
            storedWater,
            looseWater,
            storedFuel,
            storedMedicine,
            spoilageWarnings,
            state.currentWeather,
            state.outdoorTemperature,
            state.sanitationRisk,
            state.diseaseRisk,
            state.exteriorNightDanger,
            sickCount,
            untreatedCount);
    }

    public bool TryGetItemStatus(string stackId, string itemId, out SurvivalItemStatus status)
    {
        EnsureStateLists();
        string normalizedStackId = stackId?.Trim() ?? string.Empty;
        string normalizedItemId = itemId?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedItemId)
            || !ShouldTrackSpoilage(normalizedItemId))
        {
            status = default;
            return false;
        }

        SurvivalFoodSpoilageSaveData entry = state.spoilage
            .FirstOrDefault(candidate => candidate != null
                && string.Equals(candidate.stackId, normalizedStackId, StringComparison.Ordinal));
        if (entry == null)
        {
            entry = CreateSpoilageEntry(normalizedStackId, normalizedItemId);
            if (!string.IsNullOrWhiteSpace(normalizedStackId))
            {
                state.spoilage.Add(entry);
            }
        }

        float baseFreshness = entry.preserved ? PreservedFreshnessSeconds : DefaultFreshnessSeconds;
        string label = entry.contaminated
            ? "오염됨"
            : entry.remainingFreshnessSeconds <= FreshnessWarningThresholdSeconds
                ? "부패 임박"
                : entry.preserved
                    ? "보존됨"
                    : "신선함";
        status = new SurvivalItemStatus(
            tracked: true,
            preserved: entry.preserved,
            contaminated: entry.contaminated,
            freshness01: entry.remainingFreshnessSeconds / Mathf.Max(1f, baseFreshness),
            remainingFreshnessSeconds: entry.remainingFreshnessSeconds,
            label: label);
        return true;
    }

    public bool TryGetCharacterStatus(CharacterActor actor, out SurvivalCharacterStatus status)
    {
        EnsureStateLists();
        status = default;
        if (actor == null)
        {
            return false;
        }

        string persistentId = actor.Identity?.PersistentId;
        if (string.IsNullOrWhiteSpace(persistentId))
        {
            persistentId = actor.name;
        }

        List<SurvivalHealthSaveData> activeEntries = state.health
            .Where(entry => entry != null
                && entry.remainingSeconds > 0f
                && entry.state != SurvivalHealthState.Healthy
                && (string.Equals(entry.persistentId, persistentId, StringComparison.Ordinal)
                    || string.Equals(entry.persistentId, actor.name, StringComparison.Ordinal)))
            .OrderByDescending(entry => entry.state == SurvivalHealthState.Infected ? 3 : 0)
            .ThenByDescending(entry => entry.state == SurvivalHealthState.Sick ? 2 : 0)
            .ThenByDescending(entry => entry.state == SurvivalHealthState.Exposed ? 1 : 0)
            .ThenByDescending(entry => entry.severity)
            .ToList();

        SurvivalHealthSaveData primary = activeEntries.FirstOrDefault();
        float temperatureComfort = GetTemperatureComfort01(state.outdoorTemperature);
        status = new SurvivalCharacterStatus(
            hasStatus: primary != null || state.consecutiveWaterShortageDays > 0 || state.consecutiveFoodShortageDays > 0,
            primaryState: primary?.state ?? SurvivalHealthState.Healthy,
            severity01: primary?.severity ?? 0f,
            remainingSeconds: primary?.remainingSeconds ?? 0f,
            source: primary?.source ?? string.Empty,
            activeIssueCount: activeEntries.Count,
            temperatureComfort01: temperatureComfort,
            waterSummary: state.consecutiveWaterShortageDays > 0
                ? $"물 부족 {state.consecutiveWaterShortageDays}일"
                : "물 정상",
            foodSummary: state.consecutiveFoodShortageDays > 0
                ? $"식량 부족 {state.consecutiveFoodShortageDays}일"
                : "식량 정상");
        return true;
    }

    public bool TryApplySurvivalWork(
        CharacterActor actor,
        BuildableObject building,
        FacilityWorkType workType,
        out int amount,
        out string message)
    {
        EnsureStateLists();
        amount = 0;
        message = string.Empty;
        if (building == null)
        {
            message = "대상 시설이 없습니다.";
            return false;
        }

        switch (workType)
        {
            case FacilityWorkType.DrawWater:
                return TryApplyDrawWater(actor, building, out amount, out message);
            case FacilityWorkType.Cook:
                return TryApplyCook(actor, building, out amount, out message);
            case FacilityWorkType.Treat:
                return TryApplyTreat(actor, building, out amount, out message);
            case FacilityWorkType.Refuel:
                return TryApplyRefuel(actor, building, out amount, out message);
            default:
                message = "생존 작업이 아닙니다.";
                return false;
        }
    }

    public bool HasSurvivalWorkAvailable(BuildableObject building, FacilityWorkType workType)
    {
        if (building?.BuildingData == null || building.isDestroy)
        {
            return false;
        }

        return workType switch
        {
            FacilityWorkType.DrawWater => building.BuildingData.GetAbility<BuildingWaterSourceAbility>() != null
                && CanDrawWater(building),
            FacilityWorkType.Cook => building.BuildingData.GetAbility<BuildingCookingAbility>() is { } cooking
                && CountStoredStock(StockCategory.Food) >= Mathf.Max(1, cooking.inputFood)
                && (!cooking.requiresFuel || CountStoredStock(StockCategory.Fuel) > 0),
            FacilityWorkType.Treat => building.BuildingData.GetAbility<BuildingMedicalAbility>() != null
                && HasTreatableHealth()
                && (building.BuildingData.GetAbility<BuildingMedicalAbility>()?.requiresMedicine != true
                    || CountStoredStock(StockCategory.Medicine) > 0),
            FacilityWorkType.Refuel => building.BuildingData.GetAbility<BuildingFuelConsumerAbility>() != null
                && CountStoredStock(StockCategory.Fuel) > 0,
            _ => false
        };
    }

    public float GetSurvivalWorkUrgency(BuildableObject building, FacilityWorkType workType)
    {
        if (building == null || !HasSurvivalWorkAvailable(building, workType))
        {
            return 0f;
        }

        SurvivalFoodOverview overview = GetOverview();
        return workType switch
        {
            FacilityWorkType.DrawWater => Mathf.Clamp(80f - (overview.WaterShortageDays * 15f), 10f, 90f)
                + (state.lastMissingWater > 0 ? 25f : 0f),
            FacilityWorkType.Cook => Mathf.Clamp(70f - (overview.ShortageDays * 12f), 8f, 80f)
                + (overview.SpoilageWarningCount > 0 ? 15f : 0f),
            FacilityWorkType.Treat => 35f + (overview.UntreatedCount * 25f) + Mathf.Clamp(overview.DiseaseRisk * 0.35f, 0f, 35f),
            FacilityWorkType.Refuel => state.currentWeather == SurvivalWeatherType.ColdSnap
                ? 75f
                : Mathf.Clamp(overview.ExteriorNightDanger * 0.45f, 10f, 55f),
            _ => 0f
        };
    }

    private void ConsumeDailyFood(int day)
    {
        List<CharacterActor> consumers = GetSurvivalConsumers().ToList();
        int need = consumers.Count;
        int consumed = WithdrawFood(need);
        int missing = Mathf.Max(0, need - consumed);
        state.lastProcessedDay = day;
        state.lastNeededFood = need;
        state.lastConsumedFood = consumed;
        state.lastMissingFood = missing;
        state.consecutiveFoodShortageDays = missing > 0
            ? state.consecutiveFoodShortageDays + 1
            : 0;

        if (missing <= 0)
        {
            return;
        }

        for (int i = 0; i < missing && i < consumers.Count; i++)
        {
            CharacterActor actor = consumers[i];
            actor.ApplyMoodFactor(
                "survival:hungry-day",
                "굶주린 하루",
                -6f,
                180f,
                1);
            actor.AddActivity(CharacterActivityEvent.Create(
                CharacterActivityKinds.Lifecycle,
                CharacterActivityOutcomes.Blocked,
                $"{actor.name}이 식량 부족으로 허기를 넘겼다.",
                actionId: "survival/food",
                reasonCode: "food-shortage",
                sentiment: -0.55f,
                bubbleEligible: true));
        }

        EventAlertService.Raise(
            "식량이 부족합니다",
            $"오늘 필요 식량 {need}개 중 {consumed}개만 확보했습니다. 사냥하거나 사체를 도축해야 합니다.",
            EventAlertImportance.High,
            "생존");
    }

    private IEnumerable<CharacterActor> GetSurvivalConsumers()
    {
        IReadOnlyList<CharacterActor> actors = CharacterAiWorldRegistry.Characters;
        for (int i = 0; i < actors.Count; i++)
        {
            CharacterActor actor = actors[i];
            if (actor != null
                && !actor.IsDead
                && (actor.Role == CharacterRole.Owner || CharacterWorkRoleUtility.TryGetWork(actor, out _)))
            {
                yield return actor;
            }
        }
    }

    private int CountStoredFood()
    {
        return CountStoredStock(StockCategory.Food);
    }

    private int CountLooseFood()
    {
        return CountLooseStock(StockCategory.Food);
    }

    private int CountCarcasses(out int pendingFood)
    {
        pendingFood = 0;
        if (WorldItemStackRuntime.Active == null || WildlifeRuntime.Active == null)
        {
            return 0;
        }

        int count = 0;
        foreach (WorldItemStackSnapshot stack in GetCachedItemStacks())
        {
            if (stack == null
                || !WildlifeItemDefinitions.TryGetSpeciesIdFromCarcass(stack.ItemId, out string speciesId))
            {
                continue;
            }

            count++;
            if (speciesCatalog.TryGetSpecies(speciesId, out WildlifeSpeciesDefinition species))
            {
                pendingFood += species.ButcherYields
                    .Where(yieldItem => yieldItem != null
                        && string.Equals(
                            yieldItem.itemId,
                            DungeonItemCatalogSO.StockItemId(StockCategory.Food),
                            StringComparison.Ordinal))
                    .Sum(yieldItem => yieldItem.amount);
            }
        }

        return count;
    }

    private int WithdrawFood(int amount)
    {
        return WithdrawStock(StockCategory.Food, amount);
    }

    private void EnsureStateLists()
    {
        state ??= new DungeonSurvivalSaveData();
        state.spoilage ??= new List<SurvivalFoodSpoilageSaveData>();
        state.health ??= new List<SurvivalHealthSaveData>();
    }

    private void UpdateWeather(int day)
    {
        if (state.weatherDay == day)
        {
            return;
        }

        int roll = Mathf.Abs((day * 73) + 17) % 100;
        SurvivalWeatherType previous = state.currentWeather;
        state.currentWeather = roll switch
        {
            < 10 => SurvivalWeatherType.Storm,
            < 24 => SurvivalWeatherType.Rain,
            < 34 => SurvivalWeatherType.Fog,
            < 44 => SurvivalWeatherType.ColdSnap,
            < 54 => SurvivalWeatherType.HeatWave,
            _ => SurvivalWeatherType.Clear
        };
        state.weatherDay = day;
        state.outdoorTemperature = state.currentWeather switch
        {
            SurvivalWeatherType.ColdSnap => -6f,
            SurvivalWeatherType.HeatWave => 34f,
            SurvivalWeatherType.Storm => 12f,
            SurvivalWeatherType.Rain => 14f,
            SurvivalWeatherType.Fog => 16f,
            _ => 18f
        };

        if (state.currentWeather != previous
            && (state.currentWeather == SurvivalWeatherType.ColdSnap
                || state.currentWeather == SurvivalWeatherType.HeatWave
                || state.currentWeather == SurvivalWeatherType.Storm))
        {
            EventAlertService.Raise(
                "날씨가 위험해집니다",
                $"{FormatWeather(state.currentWeather)} 예보입니다. 연료, 조명, 외부 작업 상태를 확인하세요.",
                EventAlertImportance.Medium,
                "생존");
        }
    }

    private void ProcessSpoilage(bool advanceTime = false)
    {
        EnsureStateLists();
        if (WorldItemStackRuntime.Active == null)
        {
            state.spoilage.Clear();
            return;
        }

        WorldItemStackSnapshot[] stacks = GetCachedItemStacks()
            .Where(stack => stack != null && stack.State != WorldItemStackState.Carried)
            .ToArray();
        HashSet<string> validStackIds = new HashSet<string>(
            stacks.Select(stack => stack.StackId),
            StringComparer.Ordinal);

        foreach (WorldItemStackSnapshot stack in stacks)
        {
            if (ShouldTrackSpoilage(stack.ItemId))
            {
                TrackSpoilageIfNeeded(stack);
            }
        }

        state.spoilage.RemoveAll(entry => entry == null
            || string.IsNullOrWhiteSpace(entry.stackId)
            || !validStackIds.Contains(entry.stackId));

        if (!advanceTime)
        {
            return;
        }

        float weatherMultiplier = state.currentWeather == SurvivalWeatherType.HeatWave
            ? 1.35f
            : state.currentWeather == SurvivalWeatherType.ColdSnap
                ? 0.45f
                : 1f;
        float dailyDelta = 180f * weatherMultiplier;
        List<SurvivalFoodSpoilageSaveData> expired = null;
        foreach (SurvivalFoodSpoilageSaveData entry in state.spoilage)
        {
            if (entry.preserved)
            {
                entry.remainingFreshnessSeconds -= dailyDelta * 0.25f;
            }
            else
            {
                entry.remainingFreshnessSeconds -= dailyDelta;
            }

            if (entry.remainingFreshnessSeconds <= 0f || entry.contaminated)
            {
                expired ??= new List<SurvivalFoodSpoilageSaveData>();
                expired.Add(entry);
            }
        }

        if (expired == null)
        {
            return;
        }

        foreach (SurvivalFoodSpoilageSaveData entry in expired)
        {
            WorldItemStackSnapshot stack = stacks.FirstOrDefault(candidate =>
                string.Equals(candidate.StackId, entry.stackId, StringComparison.Ordinal));
            state.spoilage.Remove(entry);
            if (stack == null)
            {
                continue;
            }

            Vector2Int position = stack.Position;
            int rotAmount = Mathf.Max(1, stack.Quantity);
            WorldItemStackRuntime.Active.DeleteStack(stack.StackId);
            WorldItemStackRuntime.Active.SpawnItemAt(
                WildlifeItemDefinitions.RotItemId,
                rotAmount,
                position,
                WorldItemStackState.Loose,
                string.Empty,
                out _);
        }
    }

    private void ConsumeDailyWater(int day)
    {
        int need = GetSurvivalConsumers().Count();
        int available = CountStoredStock(StockCategory.Water) + CountLooseStock(StockCategory.Water);
        int consumed = Mathf.Min(need, available);
        int missing = Mathf.Max(0, need - consumed);
        state.lastNeededWater = need;
        state.lastConsumedWater = consumed;
        state.lastMissingWater = missing;
        // Personal thirst is now restored only when each character actually drinks.
        // These daily values remain a stock forecast for the survival dashboard.
        state.consecutiveWaterShortageDays = missing > 0
            ? state.consecutiveWaterShortageDays + 1
            : 0;
    }

    private void ConsumeDailyFuel()
    {
        int need = DailyFuelDemand;
        if (state.currentWeather == SurvivalWeatherType.ColdSnap)
        {
            need += 1;
        }

        int consumed = WithdrawStock(StockCategory.Fuel, need);
        state.lastConsumedFuel = consumed;
        state.lastMissingFuel = Mathf.Max(0, need - consumed);
        if (state.lastMissingFuel <= 0)
        {
            return;
        }

        EventAlertService.Raise(
            "연료가 부족합니다",
            "조명과 난방이 약해집니다. 밤 외부 위험과 추위 위험이 함께 오릅니다.",
            EventAlertImportance.Medium,
            "생존");
    }

    private void RefreshSurvivalRisks()
    {
        int rotStacks = CountLooseRotStacks();
        float ventilationBonus = SumBuildingAbilityValue<BuildingVentilationAbility>(
            ability => ability.hygieneRiskReduction);
        float lightSafety = SumBuildingAbilityValue<BuildingFuelConsumerAbility>(
            ability => ability.lightSafety);
        state.sanitationRisk = Mathf.Clamp(
            (rotStacks * 12f) + (state.lastMissingWater * 8f) - ventilationBonus,
            0f,
            100f);
        state.diseaseRisk = Mathf.Clamp(
            (state.sanitationRisk * 0.55f)
            + (state.consecutiveFoodShortageDays * 7f)
            + (state.consecutiveWaterShortageDays * 12f),
            0f,
            100f);
        float weatherDanger = state.currentWeather switch
        {
            SurvivalWeatherType.Storm => 35f,
            SurvivalWeatherType.Fog => 25f,
            SurvivalWeatherType.Rain => 18f,
            SurvivalWeatherType.ColdSnap => 16f,
            _ => 10f
        };
        state.exteriorNightDanger = Mathf.Clamp(
            weatherDanger + (state.lastMissingFuel * 18f) + (rotStacks * 4f) - lightSafety,
            0f,
            100f);
    }

    private static float GetTemperatureComfort01(float temperature)
    {
        float distanceFromComfort = Mathf.Abs(temperature - 20f);
        return Mathf.Clamp01(1f - (distanceFromComfort / 22f));
    }

    private void ApplyHealthConsequences()
    {
        EnsureStateLists();
        foreach (SurvivalHealthSaveData entry in state.health)
        {
            entry.remainingSeconds -= 180f;
            entry.severity = Mathf.Clamp01(entry.severity - 0.05f);
        }

        state.health.RemoveAll(entry => entry == null
            || entry.remainingSeconds <= 0f
            || entry.severity <= 0.01f
            || entry.state == SurvivalHealthState.Healthy);

        if (state.diseaseRisk < 55f)
        {
            return;
        }

        CharacterActor patient = GetSurvivalConsumers()
            .OrderBy(actor => actor.Identity?.PersistentId ?? actor.name)
            .FirstOrDefault(actor => !HasActiveHealth(actor, SurvivalHealthState.Sick)
                && !HasActiveHealth(actor, SurvivalHealthState.Infected));
        if (patient == null)
        {
            return;
        }

        RegisterOrRefreshHealth(patient, SurvivalHealthState.Sick, state.diseaseRisk / 100f, 360f, "sanitation-risk");
        patient.ApplyMoodFactor(
            "survival:sick",
            "몸 상태가 좋지 않음",
            -4f,
            240f,
            1);
    }

    private bool TryApplyDrawWater(
        CharacterActor actor,
        BuildableObject building,
        out int amount,
        out string message)
    {
        amount = 0;
        BuildingWaterSourceAbility ability = building.BuildingData?.GetAbility<BuildingWaterSourceAbility>();
        if (ability == null)
        {
            message = "물을 얻을 수 있는 시설이 아닙니다.";
            return false;
        }

        if (!CanDrawWater(building))
        {
            message = "추위 때문에 물길이 막혔습니다.";
            return false;
        }

        amount = Mathf.Max(1, ability.waterPerWork);
        bool spawned = WorldItemStackRuntime.Active != null
            && WorldItemStackRuntime.Active.SpawnItemAt(
                DungeonItemCatalogSO.StockItemId(StockCategory.Water),
                amount,
                building.centerPos,
                WorldItemStackState.Loose,
                string.Empty,
                out int spawnedAmount)
            && spawnedAmount > 0;
        if (!spawned)
        {
            amount = ModularFacilityRuntimeEffects.Produce(building, StockCategory.Water, amount);
        }

        actor?.AddActivity(CharacterActivityEvent.Work(
            FacilityWorkType.DrawWater,
            amount > 0 ? CharacterActivityOutcomes.Completed : CharacterActivityOutcomes.Failed,
            amount > 0
                ? $"{GetBuildingName(building)}에서 물 {amount}개를 길었다."
                : "물을 담을 곳을 찾지 못했다.",
            building,
            reasonCode: amount > 0 ? "water-drawn" : "water-output-failed",
            quantity: amount,
            bubbleEligible: amount <= 0));
        message = amount > 0 ? "물을 길었습니다." : "물 생산 실패";
        return amount > 0;
    }

    private bool TryApplyCook(
        CharacterActor actor,
        BuildableObject building,
        out int amount,
        out string message)
    {
        amount = 0;
        BuildingCookingAbility cooking = building.BuildingData?.GetAbility<BuildingCookingAbility>();
        if (cooking == null)
        {
            message = "조리 가능한 시설이 아닙니다.";
            return false;
        }

        int input = Mathf.Max(1, cooking.inputFood);
        if (CountStoredStock(StockCategory.Food) < input)
        {
            message = "조리할 식재료가 부족합니다.";
            return false;
        }

        if (cooking.requiresFuel && CountStoredStock(StockCategory.Fuel) <= 0)
        {
            message = "조리에 쓸 연료가 부족합니다.";
            return false;
        }

        WithdrawStock(StockCategory.Food, input);
        if (cooking.requiresFuel)
        {
            WithdrawStock(StockCategory.Fuel, 1);
        }

        BuildingPreservationAbility preservation = FindRoomPreservationAbility(building);
        string outputId = preservation != null
            ? SurvivalItemDefinitions.PreservedFoodItemId
            : SurvivalItemDefinitions.CookedMealItemId;
        amount = preservation != null
            ? Mathf.Max(1, preservation.preservedMealsPerCook)
            : Mathf.Max(1, cooking.cookedMeals);
        bool spawned = WorldItemStackRuntime.Active != null
            && WorldItemStackRuntime.Active.SpawnItemAt(
                outputId,
                amount,
                building.centerPos,
                WorldItemStackState.Loose,
                string.Empty,
                out int spawnedAmount)
            && spawnedAmount > 0;
        if (!spawned)
        {
            ModularFacilityRuntimeEffects.Produce(building, StockCategory.Food, amount);
        }

        actor?.ApplyMoodFactor(
            "survival:cooked-meal-work",
            "따뜻한 식사를 준비함",
            2f,
            120f,
            1);
        actor?.AddActivity(CharacterActivityEvent.Work(
            FacilityWorkType.Cook,
            CharacterActivityOutcomes.Completed,
            preservation != null
                ? $"{GetBuildingName(building)}에서 오래 둘 수 있는 보존 식량을 만들었다."
                : $"{GetBuildingName(building)}에서 따뜻한 식사를 만들었다.",
            building,
            reasonCode: preservation != null ? "food-preserved" : "food-cooked",
            quantity: amount));
        message = "조리를 완료했습니다.";
        return true;
    }

    private bool TryApplyTreat(
        CharacterActor actor,
        BuildableObject building,
        out int amount,
        out string message)
    {
        amount = 0;
        BuildingMedicalAbility medical = building.BuildingData?.GetAbility<BuildingMedicalAbility>();
        if (medical == null)
        {
            message = "치료 가능한 시설이 아닙니다.";
            return false;
        }

        SurvivalHealthSaveData patientEntry = FindMostSevereHealthEntry();
        if (patientEntry == null)
        {
            message = "치료할 대상이 없습니다.";
            return false;
        }

        if (medical.requiresMedicine && WithdrawStock(StockCategory.Medicine, 1) <= 0)
        {
            message = "약품이 부족합니다.";
            return false;
        }

        patientEntry.severity = Mathf.Clamp01(patientEntry.severity - Mathf.Max(0f, medical.severityReduction));
        patientEntry.remainingSeconds = Mathf.Max(0f, patientEntry.remainingSeconds - 180f);
        if (patientEntry.severity <= 0.05f || patientEntry.remainingSeconds <= 0f)
        {
            state.health.Remove(patientEntry);
        }
        else
        {
            patientEntry.state = SurvivalHealthState.Recovering;
        }

        CharacterActor patient = FindActorByPersistentId(patientEntry.persistentId);
        patient?.Heal(TreatmentMedicineHeal);
        patient?.ApplyMoodFactor(
            "survival:treated",
            "제때 치료받음",
            3f,
            180f,
            1);
        actor?.AddActivity(CharacterActivityEvent.Work(
            FacilityWorkType.Treat,
            CharacterActivityOutcomes.Completed,
            $"{GetBuildingName(building)}에서 {GetActorName(patient, patientEntry.persistentId)}의 상태를 살폈다.",
            building,
            reasonCode: "survival-treated"));
        amount = 1;
        message = "치료를 완료했습니다.";
        return true;
    }

    private bool TryApplyRefuel(
        CharacterActor actor,
        BuildableObject building,
        out int amount,
        out string message)
    {
        amount = 0;
        BuildingFuelConsumerAbility fuel = building.BuildingData?.GetAbility<BuildingFuelConsumerAbility>();
        if (fuel == null)
        {
            message = "연료를 쓰는 시설이 아닙니다.";
            return false;
        }

        int needed = Mathf.Max(1, fuel.fuelPerRefuel);
        amount = WithdrawStock(StockCategory.Fuel, needed);
        if (amount <= 0)
        {
            message = "연료가 부족합니다.";
            return false;
        }

        state.lastMissingFuel = 0;
        actor?.AddActivity(CharacterActivityEvent.Work(
            FacilityWorkType.Refuel,
            CharacterActivityOutcomes.Completed,
            $"{GetBuildingName(building)}에 연료를 보충했다.",
            building,
            reasonCode: "survival-refueled",
            quantity: amount));
        message = "연료를 보충했습니다.";
        return true;
    }

    private static BuildingPreservationAbility FindRoomPreservationAbility(BuildableObject building)
    {
        if (building == null)
        {
            return null;
        }

        try
        {
            return building.GetRoomOperationalProfile()
                .Parts
                .Where(part => part != null && part.BuildingData != null)
                .Select(part => part.BuildingData.GetAbility<BuildingPreservationAbility>())
                .FirstOrDefault(ability => ability != null);
        }
        catch (InvalidOperationException)
        {
            return building.BuildingData?.GetAbility<BuildingPreservationAbility>();
        }
    }

    private int CountStoredStock(StockCategory category)
    {
        if (!gridSystemProvider.TryGetGrid(out Grid grid))
        {
            return 0;
        }

        IReadOnlyList<IWarehouseFacility> warehouses = CharacterAiWorldRegistry.Warehouses;
        if (warehouses.Count > 0)
        {
            return warehouses
                .Where(warehouse => IsWarehouseOnGrid(warehouse, grid)
                    && warehouse.HasWarehouseInventory
                    && warehouse.Inventory != null)
                .Sum(warehouse => warehouse.Inventory.GetStock(category));
        }

        return grid.FindAllOccupants(null)
            .OfType<IWarehouseFacility>()
            .Where(warehouse => warehouse != null && warehouse.HasWarehouseInventory && warehouse.Inventory != null)
            .Sum(warehouse => warehouse.Inventory.GetStock(category));
    }

    private int CountLooseStock(StockCategory category)
    {
        if (WorldItemStackRuntime.Active == null)
        {
            return 0;
        }

        return GetCachedItemStacks()
            .Where(stack => stack != null
                && !stack.Forbidden
                && stack.State != WorldItemStackState.Carried
                && DungeonItemCatalogSO.TryGetStockCategoryFromItemId(stack.ItemId, out StockCategory parsed)
                && parsed == category
                && !SurvivalItemDefinitions.IsContaminated(stack.ItemId))
            .Sum(stack => stack.Quantity);
    }

    private int WithdrawStock(StockCategory category, int amount)
    {
        if (amount <= 0 || !gridSystemProvider.TryGetGrid(out Grid grid))
        {
            return 0;
        }

        int remaining = amount;
        int withdrawn = 0;
        IEnumerable<IWarehouseFacility> warehouses = CharacterAiWorldRegistry.Warehouses.Count > 0
            ? CharacterAiWorldRegistry.Warehouses.Where(warehouse => IsWarehouseOnGrid(warehouse, grid))
            : grid.FindAllOccupants(null).OfType<IWarehouseFacility>();
        foreach (IWarehouseFacility warehouse in warehouses
                     .Where(warehouse => warehouse != null
                         && warehouse.HasWarehouseInventory
                         && warehouse.Inventory != null))
        {
            if (remaining <= 0)
            {
                break;
            }

            int amountFromWarehouse = warehouse.Inventory.Withdraw(category, remaining);
            remaining -= amountFromWarehouse;
            withdrawn += amountFromWarehouse;
        }

        return withdrawn;
    }

    private bool CanDrawWater(BuildableObject building)
    {
        BuildingWaterSourceAbility ability = building?.BuildingData?.GetAbility<BuildingWaterSourceAbility>();
        return ability != null
            && (!ability.blockedByFreezingWeather || state.currentWeather != SurvivalWeatherType.ColdSnap);
    }

    private bool HasTreatableHealth()
    {
        EnsureStateLists();
        return state.health.Any(entry => entry != null
            && entry.remainingSeconds > 0f
            && (entry.state == SurvivalHealthState.Sick
                || entry.state == SurvivalHealthState.Infected
                || entry.state == SurvivalHealthState.Exposed
                || entry.state == SurvivalHealthState.Recovering));
    }

    private SurvivalHealthSaveData FindMostSevereHealthEntry()
    {
        EnsureStateLists();
        return state.health
            .Where(entry => entry != null
                && entry.remainingSeconds > 0f
                && entry.state != SurvivalHealthState.Healthy)
            .OrderByDescending(entry => entry.state == SurvivalHealthState.Infected ? 1 : 0)
            .ThenByDescending(entry => entry.severity)
            .FirstOrDefault();
    }

    private void RegisterOrRefreshHealth(
        CharacterActor actor,
        SurvivalHealthState healthState,
        float severity,
        float durationSeconds,
        string source)
    {
        if (actor == null)
        {
            return;
        }

        EnsureStateLists();
        string persistentId = actor.Identity?.PersistentId;
        if (string.IsNullOrWhiteSpace(persistentId))
        {
            persistentId = actor.name;
        }

        SurvivalHealthSaveData entry = state.health.FirstOrDefault(candidate =>
            candidate != null
            && string.Equals(candidate.persistentId, persistentId, StringComparison.Ordinal)
            && candidate.state == healthState);
        if (entry == null)
        {
            state.health.Add(new SurvivalHealthSaveData
            {
                persistentId = persistentId,
                state = healthState,
                severity = Mathf.Clamp01(severity),
                remainingSeconds = Mathf.Max(1f, durationSeconds),
                source = source ?? string.Empty
            });
            return;
        }

        entry.severity = Mathf.Clamp01(Mathf.Max(entry.severity, severity));
        entry.remainingSeconds = Mathf.Max(entry.remainingSeconds, durationSeconds);
        entry.source = source ?? entry.source;
    }

    private bool HasActiveHealth(CharacterActor actor, SurvivalHealthState healthState)
    {
        string persistentId = actor?.Identity?.PersistentId;
        if (string.IsNullOrWhiteSpace(persistentId))
        {
            return false;
        }

        return state.health.Any(entry => entry != null
            && entry.state == healthState
            && entry.remainingSeconds > 0f
            && string.Equals(entry.persistentId, persistentId, StringComparison.Ordinal));
    }

    private CharacterActor FindActorByPersistentId(string persistentId)
    {
        if (string.IsNullOrWhiteSpace(persistentId))
        {
            return null;
        }

        return GetSurvivalConsumers().FirstOrDefault(actor =>
            string.Equals(actor.Identity?.PersistentId, persistentId, StringComparison.Ordinal)
            || string.Equals(actor.name, persistentId, StringComparison.Ordinal));
    }

    private void TrackSpoilageIfNeeded(WorldItemStackSnapshot stack)
    {
        if (stack == null
            || string.IsNullOrWhiteSpace(stack.StackId)
            || state.spoilage.Any(entry => entry != null
                && string.Equals(entry.stackId, stack.StackId, StringComparison.Ordinal)))
        {
            return;
        }

        state.spoilage.Add(CreateSpoilageEntry(stack.StackId, stack.ItemId));
    }

    private SurvivalFoodSpoilageSaveData CreateSpoilageEntry(string stackId, string itemId)
    {
        bool preserved = SurvivalItemDefinitions.IsPreserved(itemId);
        return new SurvivalFoodSpoilageSaveData
        {
            stackId = stackId ?? string.Empty,
            itemId = itemId ?? string.Empty,
            preserved = preserved,
            contaminated = SurvivalItemDefinitions.IsContaminated(itemId),
            remainingFreshnessSeconds = preserved ? PreservedFreshnessSeconds : DefaultFreshnessSeconds
        };
    }

    private static bool ShouldTrackSpoilage(string itemId)
    {
        return SurvivalItemDefinitions.IsFoodLike(itemId)
            && !string.Equals(
                itemId?.Trim(),
                DungeonItemCatalogSO.StockItemId(StockCategory.Food),
                StringComparison.Ordinal)
            && !SurvivalItemDefinitions.IsContaminated(itemId);
    }

    private int CountSpoilageWarnings()
    {
        EnsureStateLists();
        ProcessSpoilage();
        return state.spoilage.Count(entry => entry != null
            && (entry.contaminated
                || entry.remainingFreshnessSeconds <= FreshnessWarningThresholdSeconds));
    }

    private int CountLooseRotStacks()
    {
        if (WorldItemStackRuntime.Active == null)
        {
            return 0;
        }

        return GetCachedItemStacks()
            .Count(stack => stack != null
                && !stack.Forbidden
                && stack.State != WorldItemStackState.Carried
                && string.Equals(stack.ItemId, WildlifeItemDefinitions.RotItemId, StringComparison.Ordinal));
    }

    private float SumBuildingAbilityValue<TAbility>(Func<TAbility, float> selector)
        where TAbility : BuildingAbility
    {
        if (selector == null || !gridSystemProvider.TryGetGrid(out Grid grid))
        {
            return 0f;
        }

        IReadOnlyList<BuildableObject> registeredBuildings = CharacterAiWorldRegistry.Buildings;
        IEnumerable<BuildableObject> buildings = registeredBuildings.Count > 0
            ? registeredBuildings.Where(building => building != null && building.Grid == grid)
            : grid.FindAllOccupants(null).OfType<BuildableObject>();

        return buildings
            .Where(building => building != null && !building.isDestroy && building.BuildingData != null)
            .Select(building => building.BuildingData.GetAbility<TAbility>())
            .Where(ability => ability != null)
            .Sum(selector);
    }

    private static bool IsWarehouseOnGrid(IWarehouseFacility warehouse, Grid grid)
    {
        if (warehouse == null)
        {
            return false;
        }

        BuildableObject building = warehouse as BuildableObject;
        return building == null || building.Grid == grid;
    }

    private IReadOnlyList<WorldItemStackSnapshot> GetCachedItemStacks()
    {
        WorldItemStackRuntime runtime = WorldItemStackRuntime.Active;
        if (runtime == null)
        {
            cachedItemStackVersion = -1;
            cachedItemStacks = Array.Empty<WorldItemStackSnapshot>();
            return cachedItemStacks;
        }

        if (cachedItemStackVersion == runtime.ItemStackVersion)
        {
            return cachedItemStacks;
        }

        cachedItemStackVersion = runtime.ItemStackVersion;
        cachedItemStacks = runtime.GetAllStacks()
            .Where(stack => stack != null)
            .ToArray();
        return cachedItemStacks;
    }

    private static string FormatWeather(SurvivalWeatherType weather)
    {
        return weather switch
        {
            SurvivalWeatherType.Rain => "비",
            SurvivalWeatherType.Fog => "안개",
            SurvivalWeatherType.HeatWave => "폭염",
            SurvivalWeatherType.ColdSnap => "한파",
            SurvivalWeatherType.Storm => "폭우",
            _ => "맑음"
        };
    }

    private static string GetBuildingName(BuildableObject building)
    {
        return string.IsNullOrWhiteSpace(building?.BuildingData?.objectName)
            ? building != null ? building.name : "시설"
            : building.BuildingData.objectName;
    }

    private static string GetActorName(CharacterActor actor, string fallback)
    {
        return actor != null && !string.IsNullOrWhiteSpace(actor.name)
            ? actor.name
            : string.IsNullOrWhiteSpace(fallback) ? "대상" : fallback;
    }
}
