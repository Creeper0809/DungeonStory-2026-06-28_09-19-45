using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum StartPartyRerollGroup
{
    Identity,
    Aptitude,
    Skill
}

public static class StartPartyCommitDiagnostics
{
    public static string LastReport { get; internal set; } = string.Empty;
}

public sealed class StartPartyMemberPreparation
{
    internal GameObject previewObject;
    internal GameObject prefetchedSkillObject;
    internal CharacterProgression prefetchedSkillProgression;
    internal int rollSerial;

    public int Index { get; internal set; }
    public bool IsOwner { get; internal set; }
    public CharacterSO CharacterData { get; internal set; }
    public CharacterProgression Progression { get; internal set; }
    public int IdentityRerollsRemaining { get; internal set; } = 3;
    public int AptitudeRerollsRemaining { get; internal set; } = 3;
    public int SkillRerollsRemaining { get; internal set; } = 3;

    public string RoleLabel => IsOwner ? "사장" : $"직원 {Index}";
    public bool HasReadyFirstActive => Progression != null
        && Progression.Drafts.Any(draft => draft != null
            && draft.kind == CharacterSkillKind.Active
            && draft.unlockLevel == 1
            && draft.isReady);
    public bool HasFirstPassive => Progression != null && Progression.PassiveSkills.Count > 0;
    public bool HasSelectedFirstActive => Progression != null && Progression.ActiveSkills.Count > 0;
    public bool IsReadyToStart => HasReadyFirstActive && HasFirstPassive && HasSelectedFirstActive;
}

public interface IStartPartyPreparationService
{
    bool IsPreparing { get; }
    IReadOnlyList<StartPartyMemberPreparation> Members { get; }
    event Action Changed;

    bool Begin(CharacterSO ownerData, out string message);
    bool TryFullReroll(int memberIndex, out string message);
    bool TryPartialReroll(int memberIndex, StartPartyRerollGroup group, out string message);
    bool TryChooseFirstActive(int memberIndex, int candidateIndex, out string message);
    bool TryCommit(out string message);
    void Cancel();
}

public sealed class StartPartyPreparationService : IStartPartyPreparationService, IDisposable
{
    private const int PartialRerollCharge = 3;
    private const string TraitResourcePath = "SO/Character/Traits";

    private static readonly string[] GivenNames =
    {
        "아린", "보라", "다온", "라온", "미르", "세린", "유나", "하람",
        "도윤", "시온", "루카", "네아", "카인", "레나", "이안", "소마"
    };

    private static readonly string[] Origins =
    {
        "버려진 성채의 생존자",
        "변방 시장의 잔심부름꾼",
        "몰락한 길드의 견습생",
        "지하 수로의 길잡이",
        "국경 수비대의 낙오자",
        "떠돌이 상단의 기록원"
    };

    private readonly ICharacterSkillGenerationService skillGenerationService;
    private readonly ICharacterSkillSystemSettingsProvider settingsProvider;
    private readonly IRunCharacterCatalog characterCatalog;
    private readonly IResourcesAssetLoader resourcesAssetLoader;
    private readonly IOwnerRunManagerProvider ownerRunManagerProvider;
    private readonly ICharacterSpawnerProvider characterSpawnerProvider;
    private readonly ICharacterSpawnObjectFactory characterObjectFactory;
    private readonly IGridSystemProvider gridSystemProvider;
    private readonly IRunVariableRuntimeProvider runVariableRuntimeProvider;
    private readonly List<StartPartyMemberPreparation> members = new List<StartPartyMemberPreparation>(3);
    private readonly IReadOnlyList<StartPartyMemberPreparation> membersView;
    private readonly System.Random random = new System.Random();

    private CharacterTraitSO[] traitPool;
    private int seedSerial;

    public bool IsPreparing { get; private set; }
    public IReadOnlyList<StartPartyMemberPreparation> Members => membersView;
    public event Action Changed;

    public StartPartyPreparationService(
        ICharacterSkillGenerationService skillGenerationService,
        ICharacterSkillSystemSettingsProvider settingsProvider,
        IRunCharacterCatalog characterCatalog,
        IResourcesAssetLoader resourcesAssetLoader,
        IOwnerRunManagerProvider ownerRunManagerProvider,
        ICharacterSpawnerProvider characterSpawnerProvider,
        ICharacterSpawnObjectFactory characterObjectFactory,
        IGridSystemProvider gridSystemProvider,
        IRunVariableRuntimeProvider runVariableRuntimeProvider)
    {
        this.skillGenerationService = skillGenerationService
            ?? throw new ArgumentNullException(nameof(skillGenerationService));
        this.settingsProvider = settingsProvider
            ?? throw new ArgumentNullException(nameof(settingsProvider));
        this.characterCatalog = characterCatalog
            ?? throw new ArgumentNullException(nameof(characterCatalog));
        this.resourcesAssetLoader = resourcesAssetLoader
            ?? throw new ArgumentNullException(nameof(resourcesAssetLoader));
        this.ownerRunManagerProvider = ownerRunManagerProvider
            ?? throw new ArgumentNullException(nameof(ownerRunManagerProvider));
        this.characterSpawnerProvider = characterSpawnerProvider
            ?? throw new ArgumentNullException(nameof(characterSpawnerProvider));
        this.characterObjectFactory = characterObjectFactory
            ?? throw new ArgumentNullException(nameof(characterObjectFactory));
        this.gridSystemProvider = gridSystemProvider
            ?? throw new ArgumentNullException(nameof(gridSystemProvider));
        this.runVariableRuntimeProvider = runVariableRuntimeProvider
            ?? throw new ArgumentNullException(nameof(runVariableRuntimeProvider));
        membersView = members.AsReadOnly();
    }

    public bool Begin(CharacterSO ownerData, out string message)
    {
        if (ownerData == null || !ownerData.IsOwnerCandidate)
        {
            message = "사장 후보가 올바르지 않습니다.";
            return false;
        }

        CharacterSO staffData = characterCatalog.Characters
            .Where(candidate => candidate != null
                && candidate.characterType == CharacterType.Customer
                && string.Equals(candidate.SpeciesTag, ownerData.SpeciesTag, StringComparison.OrdinalIgnoreCase))
            .OrderBy(candidate => candidate.id)
            .FirstOrDefault();
        if (staffData == null)
        {
            message = $"{ownerData.SpeciesTag} 종족의 직원 원형을 찾지 못했습니다.";
            return false;
        }

        Cancel();
        traitPool ??= resourcesAssetLoader
            .LoadAllRequired<CharacterTraitSO>(TraitResourcePath)
            .Where(trait => trait != null)
            .OrderBy(trait => trait.id)
            .ToArray();

        members.Add(CreateMember(0, true, ownerData));
        members.Add(CreateMember(1, false, staffData));
        members.Add(CreateMember(2, false, staffData));
        IsPreparing = true;
        message = "사장과 직원 2명의 준비를 시작했습니다.";
        Changed?.Invoke();
        return true;
    }

    public bool TryFullReroll(int memberIndex, out string message)
    {
        if (!TryGetMember(memberIndex, out StartPartyMemberPreparation member, out message))
        {
            return false;
        }

        CharacterPreparedIdentity identity = RollIdentity(member.CharacterData, member.Index);
        CharacterStatBlock stats = CharacterGrowthRules.RollInitialStats(settingsProvider.Settings, random);
        CharacterPotentialGrade potential = CharacterGrowthRules.RollPotential(settingsProvider.Settings, random);
        ReplaceCurrentProgression(member, identity, stats, potential);
        member.IdentityRerollsRemaining = PartialRerollCharge;
        member.AptitudeRerollsRemaining = PartialRerollCharge;
        member.SkillRerollsRemaining = PartialRerollCharge;
        message = $"{member.RoleLabel}의 전체 구성을 다시 만들었습니다.";
        Changed?.Invoke();
        return true;
    }

    public bool TryPartialReroll(
        int memberIndex,
        StartPartyRerollGroup group,
        out string message)
    {
        if (!TryGetMember(memberIndex, out StartPartyMemberPreparation member, out message))
        {
            return false;
        }

        switch (group)
        {
            case StartPartyRerollGroup.Identity:
                if (member.IdentityRerollsRemaining <= 0)
                {
                    message = "정체성 부분 리롤을 모두 사용했습니다.";
                    return false;
                }

                member.IdentityRerollsRemaining--;
                ReplaceCurrentProgression(
                    member,
                    RollIdentity(member.CharacterData, member.Index),
                    member.Progression.GrowthState.initialBaseStats,
                    member.Progression.PotentialGrade);
                message = $"{member.RoleLabel}의 정체성을 다시 만들었습니다.";
                break;

            case StartPartyRerollGroup.Aptitude:
                if (member.AptitudeRerollsRemaining <= 0)
                {
                    message = "재능 부분 리롤을 모두 사용했습니다.";
                    return false;
                }

                member.AptitudeRerollsRemaining--;
                ReplaceCurrentProgression(
                    member,
                    ReadIdentity(member.Progression),
                    CharacterGrowthRules.RollInitialStats(settingsProvider.Settings, random),
                    CharacterGrowthRules.RollPotential(settingsProvider.Settings, random));
                message = $"{member.RoleLabel}의 재능을 다시 만들었습니다.";
                break;

            case StartPartyRerollGroup.Skill:
                if (member.SkillRerollsRemaining <= 0)
                {
                    message = "스킬 부분 리롤을 모두 사용했습니다.";
                    return false;
                }

                member.SkillRerollsRemaining--;
                UsePrefetchedSkills(member);
                message = $"{member.RoleLabel}의 스킬 후보를 다시 만들었습니다.";
                break;

            default:
                message = "지원하지 않는 리롤 묶음입니다.";
                return false;
        }

        Changed?.Invoke();
        return true;
    }

    public bool TryChooseFirstActive(int memberIndex, int candidateIndex, out string message)
    {
        if (!TryGetMember(memberIndex, out StartPartyMemberPreparation member, out message))
        {
            return false;
        }

        bool selected = member.Progression.TryChooseActiveSkill(
            unlockLevel: 1,
            candidateIndex,
            confirmed: true,
            out message);
        Changed?.Invoke();
        return selected;
    }

    public bool TryCommit(out string message)
    {
        if (!IsPreparing || members.Count != 3)
        {
            message = "준비 중인 시작 파티가 없습니다.";
            return false;
        }

        StartPartyMemberPreparation incomplete = members.FirstOrDefault(member => !member.IsReadyToStart);
        if (incomplete != null)
        {
            message = $"{incomplete.RoleLabel}의 첫 액티브 선택과 패시브 준비가 필요합니다.";
            return false;
        }

        if (!ownerRunManagerProvider.TryGetManager(out OwnerRunManager manager)
            || !characterSpawnerProvider.TryGetSpawner(out CharacterSpawner spawner)
            || spawner.characterPrefab == null)
        {
            message = "시작 파티를 월드에 배치할 런타임 구성이 없습니다.";
            return false;
        }

        List<CharacterActor> preparedStaff = new List<CharacterActor>(2);
        List<string> commitDiagnostics = new List<string>();
        for (int i = 1; i < members.Count; i++)
        {
            CharacterActor staff = CreateStaffActor(members[i], spawner, i);
            if (staff == null)
            {
                foreach (CharacterActor prepared in preparedStaff)
                {
                    characterObjectFactory.Destroy(prepared.gameObject);
                }

                message = $"직원 {i}의 월드 캐릭터를 만들지 못했습니다.";
                return false;
            }

            preparedStaff.Add(staff);
            commitDiagnostics.Add(DescribeActor($"created-{i}", staff));
        }

        CharacterProgressionSnapshot ownerSnapshot = members[0].Progression.CapturePersistentState();
        manager.SelectOwner(
            members[0].CharacterData,
            members[0].Progression.GrowthState.displayName);
        CharacterActor owner = manager.CurrentOwnerActor;
        if (owner == null || owner.Progression == null)
        {
            foreach (CharacterActor prepared in preparedStaff)
            {
                characterObjectFactory.Destroy(prepared.gameObject);
            }

            message = "사장 캐릭터를 월드에 배치하지 못했습니다.";
            return false;
        }

        owner.Progression.RestorePersistentState(ownerSnapshot);
        owner.gameObject.name = owner.Progression.GrowthState.displayName;
        PlaceParty(owner, preparedStaff, spawner);
        RemoveDuplicateStartingStaff(preparedStaff, "before-enable", commitDiagnostics);
        foreach (CharacterActor staff in preparedStaff)
        {
            staff.PrepareForPersistentRestore();
            staff.gameObject.SetActive(true);
            staff.SetLifecycleState(CharacterLifecycleState.Active);
            staff.characterType = CharacterType.NPC;
            staff.Brain?.UseStaffWorkActions();
            staff.Brain?.RequestImmediateReplan(clearFailures: true);
        }
        RemoveDuplicateStartingStaff(preparedStaff, "after-enable", commitDiagnostics);
        commitDiagnostics.AddRange(preparedStaff.Select(actor => DescribeActor("retained-final", actor)));
        StartPartyCommitDiagnostics.LastReport = string.Join(" || ", commitDiagnostics);

        string ownerName = owner.Progression.GrowthState.displayName;
        CleanupPreviews();
        IsPreparing = false;
        message = $"{ownerName}와 직원 2명으로 던전을 시작합니다.";
        Changed?.Invoke();
        return true;
    }

    private void RemoveDuplicateStartingStaff(
        IReadOnlyCollection<CharacterActor> preparedStaff,
        string phase,
        ICollection<string> diagnostics)
    {
        CharacterActor[] retained = preparedStaff?
            .Where(actor => actor != null)
            .ToArray() ?? Array.Empty<CharacterActor>();
        HashSet<int> retainedGameObjectIds = new HashSet<int>(retained
            .Select(actor => actor.gameObject.GetInstanceID()));
        HashSet<string> retainedIds = new HashSet<string>(retained
            .Select(actor => actor.Identity?.PersistentId)
            .Where(id => !string.IsNullOrWhiteSpace(id)),
            StringComparer.Ordinal);
        IReadOnlyList<CharacterActor> actors = CharacterActorCollection.DistinctByGameObject(
            UnityEngine.Object.FindObjectsByType<CharacterActor>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None));
        foreach (CharacterActor actor in actors)
        {
            string persistentId = actor != null ? actor.Identity?.PersistentId : string.Empty;
            bool retainedActor = actor != null
                && retainedGameObjectIds.Contains(actor.gameObject.GetInstanceID());
            if (actor != null && persistentId.StartsWith("staff:", StringComparison.Ordinal))
            {
                diagnostics?.Add(
                    $"scan-{phase}:name={actor.name};instance={actor.GetInstanceID()};go={actor.gameObject.GetInstanceID()};components={actor.gameObject.GetComponents<CharacterActor>().Length};id={persistentId};active={actor.gameObject.activeInHierarchy};retained={retainedActor}");
            }

            if (actor == null
                || retainedActor
                || !retainedIds.Contains(persistentId)
                || !persistentId.StartsWith("staff:", StringComparison.Ordinal))
            {
                continue;
            }

            actor.gameObject.SetActive(false);
            characterObjectFactory.Destroy(actor.gameObject);
        }
    }

    private static string DescribeActor(string phase, CharacterActor actor)
    {
        return actor == null
            ? $"{phase}:null"
            : $"{phase}:name={actor.name};instance={actor.GetInstanceID()};go={actor.gameObject.GetInstanceID()};components={actor.gameObject.GetComponents<CharacterActor>().Length};id={actor.Identity?.PersistentId};active={actor.gameObject.activeInHierarchy}";
    }

    public void Cancel()
    {
        CleanupPreviews();
        members.Clear();
        IsPreparing = false;
        Changed?.Invoke();
    }

    public void Dispose()
    {
        Cancel();
    }

    private StartPartyMemberPreparation CreateMember(int index, bool isOwner, CharacterSO data)
    {
        StartPartyMemberPreparation member = new StartPartyMemberPreparation
        {
            Index = index,
            IsOwner = isOwner,
            CharacterData = data
        };
        CharacterPreparedIdentity identity = RollIdentity(data, index);
        ReplaceCurrentProgression(
            member,
            identity,
            CharacterGrowthRules.RollInitialStats(settingsProvider.Settings, random),
            CharacterGrowthRules.RollPotential(settingsProvider.Settings, random));
        return member;
    }

    private void ReplaceCurrentProgression(
        StartPartyMemberPreparation member,
        CharacterPreparedIdentity identity,
        CharacterStatBlock stats,
        CharacterPotentialGrade potential)
    {
        DestroyPreview(member.Progression, member.previewObject);
        DestroyPreview(member.prefetchedSkillProgression, member.prefetchedSkillObject);
        member.previewObject = CreatePreviewObject($"StartParty_{member.Index}_Current", out CharacterProgression progression);
        member.Progression = progression;
        progression.DraftReady += _ => Changed?.Invoke();
        progression.Changed += HandleProgressionChanged;
        ApplyRoll(progression, member, identity, stats, potential);
        TryBeginSkillPrefetch(member);
    }

    private void UsePrefetchedSkills(StartPartyMemberPreparation member)
    {
        CharacterPreparedIdentity identity = ReadIdentity(member.Progression);
        CharacterStatBlock stats = CharacterSkillModelUtility.CopyStats(member.Progression.GrowthState.initialBaseStats);
        CharacterPotentialGrade potential = member.Progression.PotentialGrade;
        DestroyPreview(member.Progression, member.previewObject);

        if (member.prefetchedSkillProgression != null)
        {
            member.Progression = member.prefetchedSkillProgression;
            member.previewObject = member.prefetchedSkillObject;
            member.prefetchedSkillProgression = null;
            member.prefetchedSkillObject = null;
            member.Progression.DraftReady += _ => Changed?.Invoke();
            member.Progression.Changed += HandleProgressionChanged;
        }
        else
        {
            member.previewObject = CreatePreviewObject($"StartParty_{member.Index}_Current", out CharacterProgression progression);
            member.Progression = progression;
            progression.DraftReady += _ => Changed?.Invoke();
            progression.Changed += HandleProgressionChanged;
            ApplyRoll(progression, member, identity, stats, potential);
        }

        TryBeginSkillPrefetch(member);
    }

    private void TryBeginSkillPrefetch(StartPartyMemberPreparation member)
    {
        if (member == null
            || member.Progression == null
            || !member.HasReadyFirstActive
            || !member.HasFirstPassive
            || member.prefetchedSkillProgression != null)
        {
            return;
        }

        DestroyPreview(member.prefetchedSkillProgression, member.prefetchedSkillObject);
        member.prefetchedSkillObject = CreatePreviewObject(
            $"StartParty_{member.Index}_Prefetch",
            out CharacterProgression progression);
        member.prefetchedSkillProgression = progression;
        ApplyRoll(
            progression,
            member,
            ReadIdentity(member.Progression),
            member.Progression.GrowthState.initialBaseStats,
            member.Progression.PotentialGrade);
    }

    private void ApplyRoll(
        CharacterProgression progression,
        StartPartyMemberPreparation member,
        CharacterPreparedIdentity identity,
        CharacterStatBlock stats,
        CharacterPotentialGrade potential)
    {
        member.rollSerial++;
        progression.ApplyPreparedIdentity(
            identity.displayName,
            identity.origin,
            identity.traitIds,
            stats,
            potential,
            NextSeed(member),
            autoChooseDrafts: false);
    }

    private GameObject CreatePreviewObject(string objectName, out CharacterProgression progression)
    {
        GameObject preview = new GameObject(objectName);
        preview.hideFlags = HideFlags.HideAndDontSave;
        progression = preview.AddComponent<CharacterProgression>();
        progression.ConstructCharacterProgression(skillGenerationService, settingsProvider);
        progression.SetPublicSkillNotificationsSuppressed(true);
        return preview;
    }

    private CharacterPreparedIdentity RollIdentity(CharacterSO data, int memberIndex)
    {
        List<int> traitIds = new List<int>(3);
        foreach (CharacterTraitSO trait in traitPool.OrderBy(_ => random.Next()))
        {
            if (traitIds.Count >= 3)
            {
                break;
            }

            if (!ConflictsWithSelected(trait.id, traitIds))
            {
                traitIds.Add(trait.id);
            }
        }

        string baseName = GivenNames[random.Next(GivenNames.Length)];
        string displayName = members.Any(member => member.Progression != null
                && string.Equals(member.Progression.GrowthState.displayName, baseName, StringComparison.Ordinal))
            ? $"{baseName}{memberIndex + 1}"
            : baseName;
        return new CharacterPreparedIdentity
        {
            displayName = displayName,
            origin = $"{data.SpeciesTag} · {Origins[random.Next(Origins.Length)]}",
            traitIds = traitIds
        };
    }

    private bool ConflictsWithSelected(int traitId, IReadOnlyCollection<int> selected)
    {
        return settingsProvider.Settings.traitConflicts.Any(rule => rule != null
            && ((rule.firstTraitId == traitId && selected.Contains(rule.secondTraitId))
                || (rule.secondTraitId == traitId && selected.Contains(rule.firstTraitId))));
    }

    private CharacterActor CreateStaffActor(
        StartPartyMemberPreparation member,
        CharacterSpawner spawner,
        int staffIndex)
    {
        GameObject staffObject = characterObjectFactory.Create(spawner.characterPrefab);
        staffObject.SetActive(false);
        if (staffObject.GetComponent<AbilityWork>() == null)
        {
            staffObject.AddComponent<AbilityWork>();
        }

        characterObjectFactory.Inject(staffObject);
        CharacterActor actor = CharacterActorCollection.GetCanonical(
            staffObject.GetComponent<CharacterActor>());
        if (actor == null)
        {
            characterObjectFactory.Destroy(staffObject);
            return null;
        }

        actor.Initialize(member.CharacterData);
        int runSeed = runVariableRuntimeProvider.TryGetRuntime(out RunVariableRuntime runtime)
            ? runtime.RunSeed
            : 0;
        actor.Identity.SetPersistentId($"staff:{runSeed}:{staffIndex:D2}");
        actor.characterType = CharacterType.NPC;
        actor.Progression.RestorePersistentState(member.Progression.CapturePersistentState());
        actor.gameObject.name = actor.Progression.GrowthState.displayName;
        actor.RefreshAbilityCache();
        if (actor.TryGetAbility(out AbilityWork _))
        {
            return actor;
        }

        characterObjectFactory.Destroy(staffObject);
        return null;
    }

    private void PlaceParty(
        CharacterActor owner,
        IReadOnlyList<CharacterActor> staff,
        CharacterSpawner spawner)
    {
        if (!gridSystemProvider.TryGetGrid(out Grid grid))
        {
            for (int i = 0; i < staff.Count; i++)
            {
                staff[i].transform.position = spawner.GetEntryDoorWorldPosition() + Vector3.right * (i + 1);
            }

            return;
        }

        Vector2Int ownerPosition = grid.GetXY(owner.transform.position);
        HashSet<Vector2Int> used = new HashSet<Vector2Int> { ownerPosition };
        List<Vector2Int> walkable = grid.GetCells()
            .Where(cell => cell != null && grid.IsWalkable(cell.Position))
            .Select(cell => cell.Position)
            .OrderBy(position => position.y == ownerPosition.y ? 0 : 1)
            .ThenBy(position => Mathf.Abs(position.x - ownerPosition.x) + Mathf.Abs(position.y - ownerPosition.y))
            .ThenBy(position => position.x)
            .ToList();
        foreach (CharacterActor actor in staff)
        {
            Vector2Int? available = walkable
                .Where(candidate => !used.Contains(candidate))
                .Select(candidate => (Vector2Int?)candidate)
                .FirstOrDefault();
            if (!available.HasValue)
            {
                actor.transform.position = spawner.GetEntryDoorWorldPosition();
                continue;
            }

            Vector2Int position = available.Value;
            used.Add(position);
            actor.transform.position = grid.GetWorldPos(position);
        }
    }

    private bool TryGetMember(
        int memberIndex,
        out StartPartyMemberPreparation member,
        out string message)
    {
        member = members.FirstOrDefault(candidate => candidate.Index == memberIndex);
        if (!IsPreparing || member == null || member.Progression == null)
        {
            message = "준비 중인 캐릭터를 찾지 못했습니다.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    private int NextSeed(StartPartyMemberPreparation member)
    {
        seedSerial++;
        return CharacterGrowthRules.StableHash(
            $"start:{member.Index}:{member.rollSerial}:{seedSerial}:{random.Next()}");
    }

    private static CharacterPreparedIdentity ReadIdentity(CharacterProgression progression)
    {
        return new CharacterPreparedIdentity
        {
            displayName = progression.GrowthState.displayName,
            origin = progression.GrowthState.origin,
            traitIds = progression.GrowthState.traitIds.ToList()
        };
    }

    private void HandleProgressionChanged()
    {
        foreach (StartPartyMemberPreparation member in members)
        {
            TryBeginSkillPrefetch(member);
        }

        Changed?.Invoke();
    }

    private void CleanupPreviews()
    {
        foreach (StartPartyMemberPreparation member in members)
        {
            DestroyPreview(member.Progression, member.previewObject);
            DestroyPreview(member.prefetchedSkillProgression, member.prefetchedSkillObject);
            member.Progression = null;
            member.previewObject = null;
            member.prefetchedSkillProgression = null;
            member.prefetchedSkillObject = null;
        }
    }

    private void DestroyPreview(CharacterProgression progression, GameObject preview)
    {
        if (progression != null)
        {
            progression.Changed -= HandleProgressionChanged;
            skillGenerationService.CancelRequests(progression);
        }

        if (preview != null)
        {
            UnityEngine.Object.Destroy(preview);
        }
    }

    private sealed class CharacterPreparedIdentity
    {
        public string displayName;
        public string origin;
        public List<int> traitIds = new List<int>();
    }
}
