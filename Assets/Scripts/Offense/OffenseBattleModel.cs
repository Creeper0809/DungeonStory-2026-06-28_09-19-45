using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum OffenseBattleTeam
{
    Allies,
    Enemies
}

public enum OffenseBattleActionType
{
    BasicAttack,
    Guard,
    Ability,
    Retreat
}

public enum OffenseBattleOutcome
{
    InProgress,
    Victory,
    Defeat,
    Retreated,
    AbortedOwnerDeath
}

public enum OffenseBattleStatusType
{
    Guard,
    Vulnerability,
    DamageOverTime,
    AttackModifier
}

[Serializable]
public sealed class OffenseBattleStats
{
    public OffenseBattleStats(
        float maxHealth,
        float attack,
        float strength,
        float toughness,
        float dexterity,
        float moveSpeed)
    {
        MaxHealth = Mathf.Max(1f, maxHealth);
        Attack = Mathf.Max(0f, attack);
        Strength = Mathf.Max(0f, strength);
        Toughness = Mathf.Max(0f, toughness);
        Dexterity = Mathf.Max(0f, dexterity);
        MoveSpeed = Mathf.Max(0f, moveSpeed);
    }

    public float MaxHealth { get; }
    public float Attack { get; }
    public float Strength { get; }
    public float Toughness { get; }
    public float Dexterity { get; }
    public float MoveSpeed { get; }
}

[Serializable]
public sealed class OffenseBattleStatus
{
    public OffenseBattleStatus(
        string id,
        OffenseBattleStatusType type,
        float value,
        int remainingTurns,
        string sourceId)
    {
        Id = id ?? string.Empty;
        Type = type;
        Value = Mathf.Max(0f, value);
        RemainingTurns = Mathf.Max(1, remainingTurns);
        SourceId = sourceId ?? string.Empty;
    }

    public string Id { get; }
    public OffenseBattleStatusType Type { get; }
    public float Value { get; private set; }
    public int RemainingTurns { get; private set; }
    public string SourceId { get; }

    internal void Refresh(float value, int turns)
    {
        Value = Mathf.Max(Value, Mathf.Max(0f, value));
        RemainingTurns = Mathf.Max(RemainingTurns, Mathf.Max(1, turns));
    }

    internal bool ConsumeTurn()
    {
        RemainingTurns = Mathf.Max(0, RemainingTurns - 1);
        return RemainingTurns <= 0;
    }
}

public sealed class OffenseBattleCombatant
{
    private readonly List<CharacterCombatAbilityDefinition> abilities;
    private readonly IReadOnlyList<CharacterCombatAbilityDefinition> abilitiesView;
    private readonly List<OffenseBattleStatus> statuses = new List<OffenseBattleStatus>();
    private readonly IReadOnlyList<OffenseBattleStatus> statusesView;
    private readonly Dictionary<string, int> cooldowns = new Dictionary<string, int>(StringComparer.Ordinal);

    public OffenseBattleCombatant(
        string persistentId,
        string displayName,
        string speciesTag,
        OffenseBattleTeam team,
        OffenseBattleStats stats,
        float currentHealth,
        IEnumerable<CharacterCombatAbilityDefinition> abilities = null,
        int portraitDataId = -1,
        OffenseFormationSlot formation = OffenseFormationSlot.Front)
    {
        PersistentId = string.IsNullOrWhiteSpace(persistentId)
            ? throw new ArgumentException("A combatant requires a persistent ID.", nameof(persistentId))
            : persistentId;
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? persistentId : displayName;
        SpeciesTag = speciesTag ?? string.Empty;
        Team = team;
        Stats = stats ?? throw new ArgumentNullException(nameof(stats));
        CurrentHealth = Mathf.Clamp(currentHealth, 0f, stats.MaxHealth);
        this.abilities = abilities?
            .Where(ability => ability != null && ability.IsValid)
            .GroupBy(ability => ability.Id, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToList() ?? new List<CharacterCombatAbilityDefinition>();
        abilitiesView = this.abilities.AsReadOnly();
        statusesView = statuses.AsReadOnly();
        PortraitDataId = portraitDataId;
        Formation = formation;
    }

    public string PersistentId { get; }
    public string DisplayName { get; }
    public string SpeciesTag { get; }
    public OffenseBattleTeam Team { get; }
    public OffenseBattleStats Stats { get; }
    public float CurrentHealth { get; private set; }
    public float HealthRatio => CurrentHealth / Mathf.Max(1f, Stats.MaxHealth);
    public bool IsDead => CurrentHealth <= 0f;
    public float InitiativePenalty { get; private set; }
    public float Initiative => Mathf.Max(0f, Stats.Dexterity * 2f + Stats.MoveSpeed - InitiativePenalty);
    public float TotalDamageTaken { get; private set; }
    public int PortraitDataId { get; }
    public OffenseFormationSlot Formation { get; private set; }
    public int TurnsStarted { get; private set; }
    public IReadOnlyList<CharacterCombatAbilityDefinition> Abilities => abilitiesView;
    public IReadOnlyList<OffenseBattleStatus> Statuses => statusesView;

    public int GetCooldown(string abilityId)
    {
        return !string.IsNullOrWhiteSpace(abilityId)
            && cooldowns.TryGetValue(abilityId, out int value)
                ? Mathf.Max(0, value)
                : 0;
    }

    internal void SetCooldown(string abilityId, int turns)
    {
        if (!string.IsNullOrWhiteSpace(abilityId))
        {
            cooldowns[abilityId] = Mathf.Max(0, turns);
        }
    }

    internal void AdjustCooldowns(int delta)
    {
        foreach (string id in cooldowns.Keys.ToArray())
        {
            cooldowns[id] = delta <= -99
                ? 0
                : Mathf.Max(0, cooldowns[id] + delta);
        }
    }

    internal IReadOnlyDictionary<string, int> GetCooldownSnapshot()
    {
        return new Dictionary<string, int>(cooldowns, StringComparer.Ordinal);
    }

    internal void RestoreCooldowns(IEnumerable<KeyValuePair<string, int>> values)
    {
        cooldowns.Clear();
        foreach (KeyValuePair<string, int> pair in values)
        {
            if (!string.IsNullOrWhiteSpace(pair.Key) && pair.Value > 0)
            {
                cooldowns[pair.Key] = pair.Value;
            }
        }
    }

    internal void BeginTurn()
    {
        if (TurnsStarted > 0)
        {
            foreach (string id in cooldowns.Keys.ToArray())
            {
                cooldowns[id] = Mathf.Max(0, cooldowns[id] - 1);
            }
        }

        TurnsStarted++;
    }

    internal void RestoreTurnsStarted(int turns)
    {
        TurnsStarted = Mathf.Max(0, turns);
    }

    internal float ApplyRawDamage(float amount)
    {
        float before = CurrentHealth;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - Mathf.Max(0f, amount));
        float applied = before - CurrentHealth;
        TotalDamageTaken += applied;
        return applied;
    }

    internal float Heal(float amount)
    {
        if (IsDead)
        {
            return 0f;
        }

        float before = CurrentHealth;
        CurrentHealth = Mathf.Min(Stats.MaxHealth, CurrentHealth + Mathf.Max(0f, amount));
        return CurrentHealth - before;
    }

    internal void RestoreHealth(float currentHealth, float totalDamageTaken)
    {
        CurrentHealth = Mathf.Clamp(currentHealth, 0f, Stats.MaxHealth);
        TotalDamageTaken = Mathf.Max(0f, totalDamageTaken);
    }

    internal void AddStatus(OffenseBattleStatus status)
    {
        if (status == null)
        {
            return;
        }

        OffenseBattleStatus existing = statuses.FirstOrDefault(value => value.Id == status.Id);
        if (existing != null)
        {
            existing.Refresh(status.Value, status.RemainingTurns);
            return;
        }

        statuses.Add(status);
    }

    internal void RestoreStatuses(IEnumerable<OffenseBattleStatus> values)
    {
        statuses.Clear();
        statuses.AddRange(values?.Where(value => value != null) ?? Array.Empty<OffenseBattleStatus>());
    }

    internal void RemoveStatus(OffenseBattleStatus status)
    {
        statuses.Remove(status);
    }

    internal int RemoveStatuses(Func<OffenseBattleStatus, bool> predicate, int maximum)
    {
        if (predicate == null || maximum <= 0)
        {
            return 0;
        }

        int removed = 0;
        for (int i = statuses.Count - 1; i >= 0 && removed < maximum; i--)
        {
            if (predicate(statuses[i]))
            {
                statuses.RemoveAt(i);
                removed++;
            }
        }

        return removed;
    }

    internal void SetFormation(OffenseFormationSlot formation)
    {
        Formation = formation;
    }

    internal void AddInitiativePenalty(float amount)
    {
        InitiativePenalty = Mathf.Max(0f, InitiativePenalty + Mathf.Max(0f, amount));
    }

    internal void RestoreInitiativePenalty(float value)
    {
        InitiativePenalty = Mathf.Max(0f, value);
    }

    internal void RestoreFormation(OffenseFormationSlot formation)
    {
        Formation = formation;
    }
}

public sealed class OffenseBattleCommand
{
    public OffenseBattleCommand(
        long commandId,
        string actorId,
        OffenseBattleActionType actionType,
        string targetId = "",
        string abilityId = "")
    {
        CommandId = commandId;
        ActorId = actorId ?? string.Empty;
        ActionType = actionType;
        TargetId = targetId ?? string.Empty;
        AbilityId = abilityId ?? string.Empty;
    }

    public long CommandId { get; }
    public string ActorId { get; }
    public OffenseBattleActionType ActionType { get; }
    public string TargetId { get; }
    public string AbilityId { get; }
}

public sealed class OffenseBattleCommandResult
{
    public OffenseBattleCommandResult(bool accepted, string message, float amount = 0f)
    {
        Accepted = accepted;
        Message = message ?? string.Empty;
        Amount = Mathf.Max(0f, amount);
    }

    public bool Accepted { get; }
    public string Message { get; }
    public float Amount { get; }
}

[Serializable]
public sealed class OffenseBattlePersistenceState
{
    public string battleId = string.Empty;
    public string expeditionId = string.Empty;
    public string targetId = string.Empty;
    public string targetTitle = string.Empty;
    public DungeonDifficulty difficulty = DungeonDifficulty.Normal;
    public OffenseBattleOutcome outcome = OffenseBattleOutcome.InProgress;
    public int roundNumber = 1;
    public int currentOrderIndex;
    public long lastProcessedCommandId;
    public List<string> initiativeOrder = new List<string>();
    public List<string> log = new List<string>();
    public List<OffenseBattleCombatantPersistenceState> combatants =
        new List<OffenseBattleCombatantPersistenceState>();
}

[Serializable]
public sealed class OffenseBattleCombatantPersistenceState
{
    public string persistentId = string.Empty;
    public float currentHealth;
    public float totalDamageTaken;
    public float initiativePenalty;
    public int turnsStarted;
    public OffenseFormationSlot formation;
    public List<OffenseBattleCooldownPersistenceState> cooldowns =
        new List<OffenseBattleCooldownPersistenceState>();
    public List<OffenseBattleStatusPersistenceState> statuses =
        new List<OffenseBattleStatusPersistenceState>();
}

[Serializable]
public sealed class OffenseBattleCooldownPersistenceState
{
    public string abilityId = string.Empty;
    public int remainingTurns;
}

[Serializable]
public sealed class OffenseBattleStatusPersistenceState
{
    public string id = string.Empty;
    public OffenseBattleStatusType type;
    public float value;
    public int remainingTurns;
    public string sourceId = string.Empty;
}

public sealed class OffenseBattleSession
{
    private const int MaxLogEntries = 60;
    private readonly List<OffenseBattleCombatant> combatants;
    private readonly IReadOnlyList<OffenseBattleCombatant> combatantsView;
    private readonly List<string> initiativeOrder = new List<string>();
    private readonly IReadOnlyList<string> initiativeOrderView;
    private readonly List<string> log = new List<string>();
    private readonly IReadOnlyList<string> logView;
    private int currentOrderIndex = -1;

    public OffenseBattleSession(
        string battleId,
        string expeditionId,
        string targetId,
        string targetTitle,
        DungeonDifficulty difficulty,
        IEnumerable<OffenseBattleCombatant> combatants)
        : this(
            battleId,
            expeditionId,
            targetId,
            targetTitle,
            difficulty,
            combatants,
            true)
    {
    }

    private OffenseBattleSession(
        string battleId,
        string expeditionId,
        string targetId,
        string targetTitle,
        DungeonDifficulty difficulty,
        IEnumerable<OffenseBattleCombatant> combatants,
        bool startImmediately)
    {
        BattleId = string.IsNullOrWhiteSpace(battleId) ? Guid.NewGuid().ToString("N") : battleId;
        ExpeditionId = expeditionId ?? string.Empty;
        TargetId = targetId ?? string.Empty;
        TargetTitle = targetTitle ?? string.Empty;
        Difficulty = difficulty;
        this.combatants = combatants?
            .Where(combatant => combatant != null)
            .GroupBy(combatant => combatant.PersistentId, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToList() ?? new List<OffenseBattleCombatant>();
        combatantsView = this.combatants.AsReadOnly();
        initiativeOrderView = initiativeOrder.AsReadOnly();
        logView = log.AsReadOnly();

        Outcome = startImmediately ? ResolveOutcome() : OffenseBattleOutcome.InProgress;
        if (startImmediately && Outcome == OffenseBattleOutcome.InProgress)
        {
            RoundNumber = 1;
            BuildInitiativeOrder();
            currentOrderIndex = 0;
            PrepareCurrentTurn();
            AddLog($"{TargetTitle} 전투가 시작되었습니다.");
        }
    }

    public string BattleId { get; }
    public string ExpeditionId { get; }
    public string TargetId { get; }
    public string TargetTitle { get; }
    public DungeonDifficulty Difficulty { get; }
    public OffenseBattleOutcome Outcome { get; private set; }
    public int RoundNumber { get; private set; }
    public long LastProcessedCommandId { get; private set; }
    public int CurrentOrderIndex => currentOrderIndex;
    public IReadOnlyList<OffenseBattleCombatant> Combatants => combatantsView;
    public IReadOnlyList<string> InitiativeOrder => initiativeOrderView;
    public IReadOnlyList<string> Log => logView;
    public bool IsComplete => Outcome != OffenseBattleOutcome.InProgress;
    public OffenseBattleCombatant CurrentActor => currentOrderIndex >= 0
        && currentOrderIndex < initiativeOrder.Count
            ? FindCombatant(initiativeOrder[currentOrderIndex])
            : null;

    public OffenseBattleCombatant FindCombatant(string persistentId)
    {
        return combatants.FirstOrDefault(combatant => string.Equals(
            combatant.PersistentId,
            persistentId,
            StringComparison.Ordinal));
    }

    public OffenseBattlePersistenceState CapturePersistentState()
    {
        return new OffenseBattlePersistenceState
        {
            battleId = BattleId,
            expeditionId = ExpeditionId,
            targetId = TargetId,
            targetTitle = TargetTitle,
            difficulty = Difficulty,
            outcome = Outcome,
            roundNumber = RoundNumber,
            currentOrderIndex = currentOrderIndex,
            lastProcessedCommandId = LastProcessedCommandId,
            initiativeOrder = initiativeOrder.ToList(),
            log = log.ToList(),
            combatants = combatants.Select(combatant => new OffenseBattleCombatantPersistenceState
            {
                persistentId = combatant.PersistentId,
                currentHealth = combatant.CurrentHealth,
                totalDamageTaken = combatant.TotalDamageTaken,
                initiativePenalty = combatant.InitiativePenalty,
                turnsStarted = combatant.TurnsStarted,
                formation = combatant.Formation,
                cooldowns = combatant.GetCooldownSnapshot()
                    .Where(pair => pair.Value > 0)
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => new OffenseBattleCooldownPersistenceState
                    {
                        abilityId = pair.Key,
                        remainingTurns = pair.Value
                    })
                    .ToList(),
                statuses = combatant.Statuses.Select(status => new OffenseBattleStatusPersistenceState
                {
                    id = status.Id,
                    type = status.Type,
                    value = status.Value,
                    remainingTurns = status.RemainingTurns,
                    sourceId = status.SourceId
                }).ToList()
            }).ToList()
        };
    }

    public static OffenseBattleSession Restore(
        OffenseBattlePersistenceState state,
        IEnumerable<OffenseBattleCombatant> configuredCombatants)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        OffenseBattleSession session = new OffenseBattleSession(
            state.battleId,
            state.expeditionId,
            state.targetId,
            state.targetTitle,
            state.difficulty,
            configuredCombatants,
            false);
        foreach (OffenseBattleCombatantPersistenceState saved in state.combatants
            ?? new List<OffenseBattleCombatantPersistenceState>())
        {
            OffenseBattleCombatant combatant = session.FindCombatant(saved?.persistentId);
            if (combatant == null) continue;

            combatant.RestoreHealth(saved.currentHealth, saved.totalDamageTaken);
            combatant.RestoreInitiativePenalty(saved.initiativePenalty);
            combatant.RestoreTurnsStarted(saved.turnsStarted);
            combatant.RestoreFormation(saved.formation);
            combatant.RestoreCooldowns((saved.cooldowns
                    ?? new List<OffenseBattleCooldownPersistenceState>())
                .Where(value => value != null)
                .Select(value => new KeyValuePair<string, int>(value.abilityId, value.remainingTurns)));
            combatant.RestoreStatuses((saved.statuses
                    ?? new List<OffenseBattleStatusPersistenceState>())
                .Where(value => value != null)
                .Select(value => new OffenseBattleStatus(
                    value.id,
                    value.type,
                    value.value,
                    value.remainingTurns,
                    value.sourceId)));
        }

        session.RestoreTurnState(
            state.roundNumber,
            state.initiativeOrder,
            state.currentOrderIndex,
            state.lastProcessedCommandId,
            state.outcome,
            state.log);
        return session;
    }

    public bool TryExecuteCommand(
        OffenseBattleCommand command,
        out OffenseBattleCommandResult result)
    {
        if (command == null)
        {
            result = new OffenseBattleCommandResult(false, "명령이 없습니다.");
            return false;
        }

        if (IsComplete)
        {
            result = new OffenseBattleCommandResult(false, "이미 끝난 전투입니다.");
            return false;
        }

        if (command.CommandId <= LastProcessedCommandId)
        {
            result = new OffenseBattleCommandResult(false, "이미 처리한 명령입니다.");
            return false;
        }

        OffenseBattleCombatant actor = CurrentActor;
        if (actor == null || actor.IsDead
            || !string.Equals(actor.PersistentId, command.ActorId, StringComparison.Ordinal))
        {
            result = new OffenseBattleCommandResult(false, "현재 행동할 캐릭터가 아닙니다.");
            return false;
        }

        if (!TryResolveCommand(actor, command, out result))
        {
            return false;
        }

        LastProcessedCommandId = command.CommandId;
        Outcome = ResolveOutcome();
        if (!IsComplete)
        {
            AdvanceTurn();
        }

        return true;
    }

    public OffenseBattleCommand CreateEnemyCommand(long commandId)
    {
        OffenseBattleCombatant actor = CurrentActor;
        if (actor == null || actor.Team != OffenseBattleTeam.Enemies || actor.IsDead)
        {
            return null;
        }

        List<OffenseBattleCombatant> targets = combatants
            .Where(target => target.Team == OffenseBattleTeam.Allies && !target.IsDead)
            .Where(IsReachableByBasicAttack)
            .OrderBy(target => WouldBasicAttackKill(actor, target) ? 0 : 1)
            .ThenBy(target => target.HealthRatio)
            .ThenByDescending(target => target.Stats.Attack)
            .ThenBy(target => target.PersistentId, StringComparer.Ordinal)
            .ToList();
        OffenseBattleCombatant target = targets.FirstOrDefault();
        if (target == null)
        {
            return null;
        }

        CharacterCombatAbilityDefinition bestAbility = actor.Abilities
            .Where(ability => actor.GetCooldown(ability.Id) <= 0
                && ability.TargetRule == OffenseBattleTargetRule.Enemy
                && IsPositionAllowed(ability.UsableFrom, actor.Formation)
                && combatants.Any(candidate => IsAbilityTargetValid(actor, candidate, ability)))
            .OrderByDescending(EstimateAbilityDamageMultiplier)
            .ThenBy(ability => ability.Id, StringComparer.Ordinal)
            .FirstOrDefault();
        if (bestAbility != null && EstimateAbilityDamageMultiplier(bestAbility) > 1.05f)
        {
            OffenseBattleCombatant abilityTarget = combatants
                .Where(candidate => IsAbilityTargetValid(actor, candidate, bestAbility))
                .OrderBy(candidate => candidate.HealthRatio)
                .ThenBy(candidate => candidate.PersistentId, StringComparer.Ordinal)
                .First();
            return new OffenseBattleCommand(
                commandId,
                actor.PersistentId,
                OffenseBattleActionType.Ability,
                abilityTarget.PersistentId,
                bestAbility.Id);
        }

        if (actor.HealthRatio < 0.25f)
        {
            return new OffenseBattleCommand(
                commandId,
                actor.PersistentId,
                OffenseBattleActionType.Guard,
                actor.PersistentId);
        }

        return new OffenseBattleCommand(
            commandId,
            actor.PersistentId,
            OffenseBattleActionType.BasicAttack,
            target.PersistentId);
    }

    public float CalculateBasicDamage(OffenseBattleCombatant source, OffenseBattleCombatant target)
    {
        if (source == null || target == null)
        {
            return 0f;
        }

        float attackMultiplier = 1f + source.Statuses
            .Where(status => status.Type == OffenseBattleStatusType.AttackModifier)
            .Sum(status => status.Value);
        return Mathf.Max(
            1f,
            source.Stats.Attack * Mathf.Max(0.1f, attackMultiplier) * 2f
            + source.Stats.Strength
            - target.Stats.Toughness * 0.75f);
    }

    internal float ApplyDamage(
        OffenseBattleCombatant source,
        OffenseBattleCombatant target,
        float rawAmount)
    {
        if (target == null || target.IsDead)
        {
            return 0f;
        }

        float guard = target.Statuses
            .Where(status => status.Type == OffenseBattleStatusType.Guard)
            .Select(status => status.Value)
            .DefaultIfEmpty(0f)
            .Max();
        float vulnerability = target.Statuses
            .Where(status => status.Type == OffenseBattleStatusType.Vulnerability)
            .Select(status => status.Value)
            .DefaultIfEmpty(0f)
            .Max();
        float finalAmount = Mathf.Max(1f, rawAmount * (1f - Mathf.Clamp01(guard)) * (1f + vulnerability));
        float applied = target.ApplyRawDamage(finalAmount);
        if (target.IsDead)
        {
            AddLog($"{target.DisplayName}이(가) 쓰러졌습니다.");
            CompactFormation(target.Team);
        }

        return applied;
    }

    internal float Heal(OffenseBattleCombatant target, float amount)
    {
        return target?.Heal(amount) ?? 0f;
    }

    internal void AddStatus(
        OffenseBattleCombatant target,
        OffenseBattleStatusType type,
        float value,
        int turns,
        string sourceId,
        string statusId)
    {
        target?.AddStatus(new OffenseBattleStatus(statusId, type, value, turns, sourceId));
    }

    internal void Delay(OffenseBattleCombatant target, float amount)
    {
        target?.AddInitiativePenalty(amount);
    }

    internal int Cleanse(OffenseBattleCombatant target, int maximum)
    {
        return target?.RemoveStatuses(
            status => status.Type == OffenseBattleStatusType.Vulnerability
                || status.Type == OffenseBattleStatusType.DamageOverTime
                || (status.Type == OffenseBattleStatusType.AttackModifier && status.Value < 0f),
            maximum) ?? 0;
    }

    internal void AdjustCooldowns(OffenseBattleCombatant target, int delta)
    {
        target?.AdjustCooldowns(delta);
    }

    internal IReadOnlyList<OffenseBattleCombatant> GetLivingTeam(OffenseBattleTeam team)
    {
        return combatants.Where(combatant => combatant.Team == team && !combatant.IsDead).ToArray();
    }

    internal void Reposition(OffenseBattleCombatant target, int offset)
    {
        if (target == null || offset == 0)
        {
            return;
        }

        int next = Mathf.Clamp((int)target.Formation + offset, 0, 2);
        target.SetFormation((OffenseFormationSlot)next);
    }

    public void AbortForOwnerDeath()
    {
        if (IsComplete)
        {
            return;
        }

        Outcome = OffenseBattleOutcome.AbortedOwnerDeath;
        AddLog("사장이 쓰러져 원정 전투가 중단되었습니다.");
    }

    internal void RestoreTurnState(
        int roundNumber,
        IEnumerable<string> restoredInitiativeOrder,
        int restoredCurrentOrderIndex,
        long lastProcessedCommandId,
        OffenseBattleOutcome outcome,
        IEnumerable<string> restoredLog)
    {
        RoundNumber = Mathf.Max(1, roundNumber);
        initiativeOrder.Clear();
        initiativeOrder.AddRange((restoredInitiativeOrder ?? Array.Empty<string>())
            .Where(id => FindCombatant(id) != null)
            .Distinct(StringComparer.Ordinal));
        if (initiativeOrder.Count == 0 && outcome == OffenseBattleOutcome.InProgress)
        {
            BuildInitiativeOrder();
        }

        currentOrderIndex = Mathf.Clamp(restoredCurrentOrderIndex, 0, Mathf.Max(0, initiativeOrder.Count - 1));
        LastProcessedCommandId = Math.Max(0, lastProcessedCommandId);
        Outcome = outcome;
        log.Clear();
        log.AddRange((restoredLog ?? Array.Empty<string>()).TakeLast(MaxLogEntries));
    }

    private bool TryResolveCommand(
        OffenseBattleCombatant actor,
        OffenseBattleCommand command,
        out OffenseBattleCommandResult result)
    {
        switch (command.ActionType)
        {
            case OffenseBattleActionType.BasicAttack:
                return TryBasicAttack(actor, command.TargetId, out result);
            case OffenseBattleActionType.Guard:
                AddStatus(
                    actor,
                    OffenseBattleStatusType.Guard,
                    0.5f,
                    1,
                    actor.PersistentId,
                    $"common-guard:{actor.PersistentId}");
                AddLog($"{actor.DisplayName}이(가) 방어 태세를 취했습니다.");
                result = new OffenseBattleCommandResult(true, "방어 태세");
                return true;
            case OffenseBattleActionType.Retreat:
                if (actor.Team != OffenseBattleTeam.Allies)
                {
                    result = new OffenseBattleCommandResult(false, "적은 후퇴 명령을 사용할 수 없습니다.");
                    return false;
                }

                Outcome = OffenseBattleOutcome.Retreated;
                AddLog($"{actor.DisplayName}의 지시로 원정대가 후퇴했습니다.");
                result = new OffenseBattleCommandResult(true, "후퇴했습니다.");
                return true;
            case OffenseBattleActionType.Ability:
                return TryUseAbility(actor, command.TargetId, command.AbilityId, out result);
            default:
                result = new OffenseBattleCommandResult(false, "지원하지 않는 행동입니다.");
                return false;
        }
    }

    private bool TryBasicAttack(
        OffenseBattleCombatant actor,
        string targetId,
        out OffenseBattleCommandResult result)
    {
        OffenseBattleCombatant target = FindCombatant(targetId);
        if (!IsValidTarget(actor, target, OffenseBattleTargetRule.Enemy)
            || !IsReachableByBasicAttack(target))
        {
            result = new OffenseBattleCommandResult(false, "공격할 수 없는 대상입니다.");
            return false;
        }

        float damage = ApplyDamage(actor, target, CalculateBasicDamage(actor, target));
        AddLog($"{actor.DisplayName}이(가) {target.DisplayName}에게 {damage:0.#} 피해를 줬습니다.");
        result = new OffenseBattleCommandResult(true, "공격 성공", damage);
        return true;
    }

    private bool TryUseAbility(
        OffenseBattleCombatant actor,
        string targetId,
        string abilityId,
        out OffenseBattleCommandResult result)
    {
        CharacterCombatAbilityDefinition ability = actor.Abilities.FirstOrDefault(value => string.Equals(
            value.Id,
            abilityId,
            StringComparison.Ordinal));
        if (ability == null)
        {
            result = new OffenseBattleCommandResult(false, "사용할 수 없는 능력입니다.");
            return false;
        }

        int cooldown = actor.GetCooldown(ability.Id);
        if (cooldown > 0)
        {
            result = new OffenseBattleCommandResult(false, $"재사용까지 {cooldown}턴 남았습니다.");
            return false;
        }

        if (!IsPositionAllowed(ability.UsableFrom, actor.Formation))
        {
            result = new OffenseBattleCommandResult(
                false,
                $"{OffenseFormationUtility.GetDisplayName(actor.Formation)}에서는 이 능력을 사용할 수 없습니다.");
            return false;
        }

        OffenseBattleCombatant target = ability.TargetRule == OffenseBattleTargetRule.Self
            ? actor
            : FindCombatant(targetId);
        if (!IsAbilityTargetValid(actor, target, ability))
        {
            result = new OffenseBattleCommandResult(false, "능력을 사용할 수 없는 대상입니다.");
            return false;
        }

        OffenseBattleEffectContext context = new OffenseBattleEffectContext(this, actor, target);
        foreach (OffenseCombatEffectModule effect in ability.Effects)
        {
            effect?.Apply(context);
        }

        actor.SetCooldown(ability.Id, ability.CooldownTurns);
        AddLog($"{actor.DisplayName}이(가) {ability.DisplayName}을(를) 사용했습니다.");
        result = new OffenseBattleCommandResult(true, ability.DisplayName, context.DamageDealt);
        return true;
    }

    private void AdvanceTurn()
    {
        int attempts = 0;
        do
        {
            currentOrderIndex++;
            if (currentOrderIndex >= initiativeOrder.Count)
            {
                RoundNumber++;
                BuildInitiativeOrder();
                currentOrderIndex = 0;
            }

            PrepareCurrentTurn();
            Outcome = ResolveOutcome();
            attempts++;
        }
        while (!IsComplete
            && (CurrentActor == null || CurrentActor.IsDead)
            && attempts <= combatants.Count * 2);
    }

    private void PrepareCurrentTurn()
    {
        OffenseBattleCombatant actor = CurrentActor;
        if (actor == null || actor.IsDead)
        {
            return;
        }

        actor.BeginTurn();
        foreach (OffenseBattleStatus status in actor.Statuses.ToArray())
        {
            if (status.Type == OffenseBattleStatusType.DamageOverTime)
            {
                OffenseBattleCombatant source = FindCombatant(status.SourceId);
                float damage = ApplyDamage(source, actor, status.Value);
                AddLog($"{actor.DisplayName}이(가) 지속 피해 {damage:0.#}을 받았습니다.");
            }

            if (status.ConsumeTurn())
            {
                actor.RemoveStatus(status);
            }
        }
    }

    private void BuildInitiativeOrder()
    {
        initiativeOrder.Clear();
        initiativeOrder.AddRange(combatants
            .Where(combatant => !combatant.IsDead)
            .OrderByDescending(combatant => combatant.Initiative)
            .ThenBy(combatant => combatant.PersistentId, StringComparer.Ordinal)
            .Select(combatant => combatant.PersistentId));
    }

    private OffenseBattleOutcome ResolveOutcome()
    {
        bool alliesAlive = combatants.Any(combatant => combatant.Team == OffenseBattleTeam.Allies && !combatant.IsDead);
        bool enemiesAlive = combatants.Any(combatant => combatant.Team == OffenseBattleTeam.Enemies && !combatant.IsDead);
        if (!alliesAlive) return OffenseBattleOutcome.Defeat;
        if (!enemiesAlive) return OffenseBattleOutcome.Victory;
        return Outcome is OffenseBattleOutcome.Retreated or OffenseBattleOutcome.AbortedOwnerDeath
            ? Outcome
            : OffenseBattleOutcome.InProgress;
    }

    private bool IsValidTarget(
        OffenseBattleCombatant actor,
        OffenseBattleCombatant target,
        OffenseBattleTargetRule rule)
    {
        if (actor == null || target == null || target.IsDead)
        {
            return false;
        }

        return rule switch
        {
            OffenseBattleTargetRule.Self => ReferenceEquals(actor, target),
            OffenseBattleTargetRule.Ally => actor.Team == target.Team,
            OffenseBattleTargetRule.Enemy => actor.Team != target.Team,
            _ => false
        };
    }

    private bool IsAbilityTargetValid(
        OffenseBattleCombatant actor,
        OffenseBattleCombatant target,
        CharacterCombatAbilityDefinition ability)
    {
        return ability != null
            && IsValidTarget(actor, target, ability.TargetRule)
            && IsPositionAllowed(ability.TargetPositions, target.Formation);
    }

    private bool IsReachableByBasicAttack(OffenseBattleCombatant target)
    {
        if (target == null || target.IsDead)
        {
            return false;
        }

        bool hasForwardTarget = combatants.Any(candidate => candidate.Team == target.Team
            && !candidate.IsDead
            && candidate.Formation != OffenseFormationSlot.Rear);
        return !hasForwardTarget || target.Formation != OffenseFormationSlot.Rear;
    }

    private static bool IsPositionAllowed(OffenseFormationMask mask, OffenseFormationSlot slot)
    {
        return (mask & OffenseFormationUtility.ToMask(slot)) != 0;
    }

    private void CompactFormation(OffenseBattleTeam team)
    {
        OffenseBattleCombatant[] survivors = combatants
            .Where(combatant => combatant.Team == team && !combatant.IsDead)
            .OrderBy(combatant => combatant.Formation)
            .ThenBy(combatant => combatant.PersistentId, StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        for (int index = 0; index < survivors.Length; index++)
        {
            survivors[index].RestoreFormation((OffenseFormationSlot)index);
        }
    }

    private bool WouldBasicAttackKill(OffenseBattleCombatant source, OffenseBattleCombatant target)
    {
        return CalculateBasicDamage(source, target) >= target.CurrentHealth;
    }

    private static float EstimateAbilityDamageMultiplier(CharacterCombatAbilityDefinition ability)
    {
        float estimate = 0f;
        foreach (OffenseCombatEffectModule effect in ability?.Effects ?? Array.Empty<OffenseCombatEffectModule>())
        {
            if (effect is OffenseDamageEffect)
            {
                estimate += 1.1f;
            }
            else if (effect is OffenseVulnerabilityEffect || effect is OffenseDamageOverTimeEffect)
            {
                estimate += 0.3f;
            }
        }

        return estimate;
    }

    private void AddLog(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        log.Add(message);
        if (log.Count > MaxLogEntries)
        {
            log.RemoveRange(0, log.Count - MaxLogEntries);
        }
    }
}

public static class OffenseEncounterCatalog
{
    public static string GetEnemySummary(int campaignOrder)
    {
        return Mathf.Clamp(campaignOrder, 1, 6) switch
        {
            1 => "농장 집행관",
            2 => "상단 경비 + 석궁병",
            3 => "무기고 경비 2명 + 대장",
            4 => "마력 파수기 2명 + 마나 이상체",
            5 => "경쟁 던전 3종족 파티",
            _ => "봉인 수호자 2명 + 진실의 감시자"
        };
    }

    public static OffenseBattleCombatant CreateAlly(
        CharacterActor actor,
        string persistentId,
        OffenseFormationSlot formation = OffenseFormationSlot.Front,
        float stress = 0f,
        ExpeditionEquipmentStatBlock equipment = null)
    {
        if (actor == null)
        {
            throw new ArgumentNullException(nameof(actor));
        }

        actor.EnsureRuntimeState();
        CharacterIdentity identity = actor.Identity;
        float stressMultiplier = Mathf.Lerp(1f, 0.65f, Mathf.Clamp01(stress / 100f));
        equipment ??= ExpeditionEquipmentStatBlock.Empty;
        float maxHealth = Mathf.Max(1f, actor.MaxHealth + equipment.maxHealth);
        return new OffenseBattleCombatant(
            persistentId,
            identity != null ? identity.DisplayName : actor.name,
            identity != null ? identity.SpeciesTag : string.Empty,
            OffenseBattleTeam.Allies,
            new OffenseBattleStats(
                maxHealth,
                actor.GetCharacterStat(CharacterStatType.Attack) * stressMultiplier + equipment.attack,
                actor.GetCharacterStat(CharacterStatType.Strength) * stressMultiplier + equipment.strength,
                actor.GetCharacterStat(CharacterStatType.Toughness) * stressMultiplier + equipment.toughness,
                actor.GetCharacterStat(CharacterStatType.Dexterity) * stressMultiplier + equipment.dexterity,
                actor.GetCharacterStat(CharacterStatType.MoveSpeed) * stressMultiplier + equipment.moveSpeed),
            Mathf.Clamp(actor.CurrentHealth + equipment.maxHealth, 0f, maxHealth),
            CharacterCombatAbilityCatalog.GetAbilities(actor),
            identity?.Data != null ? identity.Data.id : -1,
            formation);
    }

    public static IReadOnlyList<OffenseBattleCombatant> CreateEnemies(
        OffenseTargetDefinition target,
        DungeonDifficulty difficulty,
        OffenseRouteNode routeNode = null)
    {
        int stage = Mathf.Clamp(target?.campaignOrder ?? 1, 1, 6);
        DungeonDifficultyMultipliers multipliers = DungeonDifficultyRules.GetOffenseMultipliers(difficulty);
        List<EnemyTemplate> templates = stage switch
        {
            1 => new List<EnemyTemplate>
            {
                Enemy("farm-enforcer", "농장 집행관", "Human", 82f, 7f, 6f, 5f, 5f, 4f)
            },
            2 => new List<EnemyTemplate>
            {
                Enemy("caravan-guard", "상단 경비", "Human", 88f, 7f, 7f, 7f, 5f, 4f),
                Enemy("caravan-crossbow", "석궁병", "Human", 68f, 9f, 5f, 4f, 8f, 5f, AimedShot())
            },
            3 => new List<EnemyTemplate>
            {
                Enemy("armory-guard-a", "무기고 경비 A", "Human", 92f, 8f, 7f, 8f, 5f, 4f),
                Enemy("armory-guard-b", "무기고 경비 B", "Human", 92f, 8f, 7f, 8f, 5f, 4f),
                Enemy("armory-captain", "무기고 대장", "Human", 118f, 10f, 9f, 9f, 7f, 5f, CaptainBreak())
            },
            4 => new List<EnemyTemplate>
            {
                Enemy("arcane-sentry-a", "마력 파수기 A", "Construct", 96f, 9f, 6f, 9f, 7f, 4f, ArcaneBurn()),
                Enemy("arcane-sentry-b", "마력 파수기 B", "Construct", 96f, 9f, 6f, 9f, 7f, 4f, ArcaneBurn()),
                Enemy("mana-anomaly", "마나 이상체", "Arcane", 125f, 11f, 8f, 7f, 9f, 6f, ManaRend())
            },
            5 => new List<EnemyTemplate>
            {
                Enemy("rival-slime", "경쟁 던전 슬라임", "Slime", 120f, 10f, 7f, 10f, 6f, 4f, CharacterCombatAbilityCatalog.CreateSlimeBarrier()),
                Enemy("rival-orc", "경쟁 던전 오크", "Orc", 145f, 12f, 11f, 11f, 7f, 5f, CharacterCombatAbilityCatalog.CreateOrcCrush()),
                Enemy("rival-vampire", "경쟁 던전 뱀파이어", "Vampire", 112f, 11f, 8f, 7f, 12f, 7f, CharacterCombatAbilityCatalog.CreateVampireDrain())
            },
            _ => new List<EnemyTemplate>
            {
                Enemy("sealkeeper-a", "봉인 수호자 A", "Truth", 132f, 12f, 9f, 11f, 9f, 5f, SealBrand()),
                Enemy("sealkeeper-b", "봉인 수호자 B", "Truth", 132f, 12f, 9f, 11f, 9f, 5f, SealBrand()),
                Enemy("truth-warden", "진실의 감시자", "Truth", 210f, 15f, 13f, 13f, 11f, 6f, TruthRend())
            }
        };

        int enemyCount = routeNode == null || routeNode.IsBoss
            ? templates.Count
            : Mathf.Min(templates.Count, routeNode.Depth <= 1 ? 1 : 2);
        float encounterScale = routeNode == null || routeNode.IsBoss
            ? 1f
            : Mathf.Clamp(routeNode.DangerMultiplier, 0.65f, 1.1f);
        return templates.Take(enemyCount).Select((template, index) => new OffenseBattleCombatant(
            $"enemy:{target?.id ?? "unknown"}:{template.Id}:{index}",
            template.Name,
            template.Species,
            OffenseBattleTeam.Enemies,
            new OffenseBattleStats(
                template.Health * multipliers.EnemyHealth * encounterScale,
                template.Attack * multipliers.EnemyAttack * encounterScale,
                template.Strength * multipliers.EnemyAttack * encounterScale,
                template.Toughness * encounterScale,
                template.Dexterity * multipliers.EnemyInitiative * encounterScale,
                template.MoveSpeed * multipliers.EnemyInitiative * encounterScale),
            template.Health * multipliers.EnemyHealth * encounterScale,
            template.Abilities,
            formation: (OffenseFormationSlot)Mathf.Clamp(index, 0, 2))).ToArray();
    }

    private static EnemyTemplate Enemy(
        string id,
        string name,
        string species,
        float health,
        float attack,
        float strength,
        float toughness,
        float dexterity,
        float moveSpeed,
        params CharacterCombatAbilityDefinition[] abilities)
    {
        return new EnemyTemplate(
            id,
            name,
            species,
            health,
            attack,
            strength,
            toughness,
            dexterity,
            moveSpeed,
            abilities);
    }

    private static CharacterCombatAbilityDefinition AimedShot()
    {
        return new CharacterCombatAbilityDefinition(
            "enemy.aimed-shot",
            "조준 사격",
            "강한 피해를 주고 행동을 늦춥니다.",
            2,
            OffenseBattleTargetRule.Enemy,
            new OffenseDamageEffect(1.25f),
            new OffenseDelayEffect(2f));
    }

    private static CharacterCombatAbilityDefinition CaptainBreak()
    {
        return new CharacterCombatAbilityDefinition(
            "enemy.captain-break",
            "갑옷 파괴",
            "피해와 취약을 적용합니다.",
            2,
            OffenseBattleTargetRule.Enemy,
            new OffenseDamageEffect(1.3f),
            new OffenseVulnerabilityEffect(0.2f));
    }

    private static CharacterCombatAbilityDefinition ArcaneBurn()
    {
        return new CharacterCombatAbilityDefinition(
            "enemy.arcane-burn",
            "비전 화상",
            "지속 피해를 남깁니다.",
            2,
            OffenseBattleTargetRule.Enemy,
            new OffenseDamageEffect(0.8f),
            new OffenseDamageOverTimeEffect(5f, 2));
    }

    private static CharacterCombatAbilityDefinition ManaRend()
    {
        return new CharacterCombatAbilityDefinition(
            "enemy.mana-rend",
            "마력 절단",
            "큰 피해와 행동 지연을 줍니다.",
            2,
            OffenseBattleTargetRule.Enemy,
            new OffenseDamageEffect(1.45f),
            new OffenseDelayEffect(3f));
    }

    private static CharacterCombatAbilityDefinition SealBrand()
    {
        return new CharacterCombatAbilityDefinition(
            "enemy.seal-brand",
            "봉인의 낙인",
            "취약과 지속 피해를 남깁니다.",
            2,
            OffenseBattleTargetRule.Enemy,
            new OffenseDamageEffect(0.9f),
            new OffenseVulnerabilityEffect(0.2f),
            new OffenseDamageOverTimeEffect(6f, 2));
    }

    private static CharacterCombatAbilityDefinition TruthRend()
    {
        return new CharacterCombatAbilityDefinition(
            "enemy.truth-rend",
            "진실 절단",
            "강한 피해를 주고 방어를 무너뜨립니다.",
            2,
            OffenseBattleTargetRule.Enemy,
            new OffenseDamageEffect(1.55f),
            new OffenseVulnerabilityEffect(0.3f));
    }

    private sealed class EnemyTemplate
    {
        public EnemyTemplate(
            string id,
            string name,
            string species,
            float health,
            float attack,
            float strength,
            float toughness,
            float dexterity,
            float moveSpeed,
            IEnumerable<CharacterCombatAbilityDefinition> abilities)
        {
            Id = id;
            Name = name;
            Species = species;
            Health = health;
            Attack = attack;
            Strength = strength;
            Toughness = toughness;
            Dexterity = dexterity;
            MoveSpeed = moveSpeed;
            Abilities = abilities?.Where(ability => ability != null).ToArray()
                ?? Array.Empty<CharacterCombatAbilityDefinition>();
        }

        public string Id { get; }
        public string Name { get; }
        public string Species { get; }
        public float Health { get; }
        public float Attack { get; }
        public float Strength { get; }
        public float Toughness { get; }
        public float Dexterity { get; }
        public float MoveSpeed { get; }
        public IReadOnlyList<CharacterCombatAbilityDefinition> Abilities { get; }
    }
}
