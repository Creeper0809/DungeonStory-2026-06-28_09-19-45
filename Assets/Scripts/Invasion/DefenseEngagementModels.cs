using System;
using System.Collections.Generic;
using UnityEngine;

public enum DefenseEngagementState
{
    Dispatching,
    InterceptPlanned,
    Engaged,
    ReserveWaiting,
    Switching,
    Retreating,
    FrontCollapsed,
    Completed
}

public enum DefenseResponsePolicyKind
{
    Standard,
    SurvivalFirst,
    HoldTheLine,
    Custom
}

[Serializable]
public sealed class DefenseResponsePolicyData
{
    public string id = string.Empty;
    public string displayName = string.Empty;
    public DefenseResponsePolicyKind kind = DefenseResponsePolicyKind.Custom;
    public bool autoRespond = true;
    [Range(0f, 1f)] public float minimumDispatchHealthRatio = 0.4f;
    [Range(0f, 1f)] public float retreatHealthRatio = 0.2f;
    public bool holdWithoutReplacement = true;
    [Range(0f, 1f)] public float rejoinHealthRatio = 0.6f;

    public DefenseResponsePolicyData Clone()
    {
        return new DefenseResponsePolicyData
        {
            id = id,
            displayName = displayName,
            kind = kind,
            autoRespond = autoRespond,
            minimumDispatchHealthRatio = Mathf.Clamp01(minimumDispatchHealthRatio),
            retreatHealthRatio = Mathf.Clamp01(retreatHealthRatio),
            holdWithoutReplacement = holdWithoutReplacement,
            rejoinHealthRatio = Mathf.Clamp01(rejoinHealthRatio)
        };
    }

    public void Normalize()
    {
        id = id?.Trim() ?? string.Empty;
        displayName = displayName?.Trim() ?? string.Empty;
        minimumDispatchHealthRatio = Mathf.Clamp01(minimumDispatchHealthRatio);
        retreatHealthRatio = Mathf.Clamp01(retreatHealthRatio);
        rejoinHealthRatio = Mathf.Clamp(
            rejoinHealthRatio,
            minimumDispatchHealthRatio,
            1f);
    }
}

public readonly struct DefenseInterceptPlan
{
    public DefenseInterceptPlan(
        Vector2Int intruderStopCell,
        Vector2Int guardCell,
        Vector2Int reserveCell,
        Queue<GridMoveStep> leadPath,
        int intruderSteps)
    {
        IntruderStopCell = intruderStopCell;
        GuardCell = guardCell;
        ReserveCell = reserveCell;
        LeadPath = leadPath ?? new Queue<GridMoveStep>();
        IntruderSteps = Mathf.Max(0, intruderSteps);
    }

    public Vector2Int IntruderStopCell { get; }
    public Vector2Int GuardCell { get; }
    public Vector2Int ReserveCell { get; }
    public Queue<GridMoveStep> LeadPath { get; }
    public int IntruderSteps { get; }
}

public sealed class DefenseEngagement
{
    public string Id { get; internal set; } = string.Empty;
    public InvasionIntruderRuntime Intruder { get; internal set; }
    public CharacterActor LeadGuard { get; internal set; }
    public CharacterActor ReserveGuard { get; internal set; }
    public CharacterActor RangedGuard { get; internal set; }
    public CharacterActor SecondaryRangedGuard { get; internal set; }
    public DefenseEngagementState State { get; internal set; }
    public Vector2Int IntruderStopCell { get; internal set; }
    public Vector2Int GuardCell { get; internal set; }
    public Vector2Int ReserveCell { get; internal set; }
    public Vector2Int RangedCell { get; internal set; }
    public Vector2Int SecondaryRangedCell { get; internal set; }
    public bool HasReserveCell { get; internal set; }
    public bool IsOwnerFinalDefense { get; internal set; }
    public bool Forced { get; internal set; }
    public float NextGuardAttackAt { get; internal set; }
    public float NextIntruderAttackAt { get; internal set; }
    public float NextRangedAttackAt { get; internal set; }
    public float NextRangedReplanAt { get; internal set; }
    public float NextSecondaryRangedAttackAt { get; internal set; }
    public float NextSecondaryRangedReplanAt { get; internal set; }
    public int ExchangeCount { get; internal set; }
    public string StatusText { get; internal set; } = string.Empty;
    internal Coroutine LeadMovement { get; set; }
    internal Coroutine ReserveMovement { get; set; }
    internal Coroutine RangedMovement { get; set; }
    internal Coroutine SecondaryRangedMovement { get; set; }
    internal bool LeadArrived { get; set; }
    internal bool ReserveArrived { get; set; }
    internal bool RangedArrived { get; set; }
    internal bool SecondaryRangedArrived { get; set; }

    public CharacterActor IntruderActor => Intruder != null ? Intruder.IntruderActor : null;
    public bool IsActive => State != DefenseEngagementState.Completed;
}

public interface IDefenseResponsePolicyRuntime
{
    IReadOnlyList<DefenseResponsePolicyData> Policies { get; }
    DefenseResponsePolicyData GetPolicy(CharacterActor actor);
    string GetAssignedPolicyId(CharacterActor actor);
    bool AssignPolicy(CharacterActor actor, string policyId);
    bool TryCreatePolicy(string displayName, out DefenseResponsePolicyData policy);
    bool TryDuplicatePolicy(string sourcePolicyId, string displayName, out DefenseResponsePolicyData policy);
    bool TryUpdatePolicy(DefenseResponsePolicyData source);
    bool TryDeletePolicy(string policyId, bool reassignToStandard);
    DefenseResponsePolicySaveSnapshot Capture();
    void Restore(DefenseResponsePolicySaveSnapshot snapshot, IList<string> warnings);
}

public interface IDefenseEngagementRuntime
{
    IReadOnlyList<DefenseEngagement> ActiveEngagements { get; }
    bool TryGetEngagement(InvasionIntruderRuntime intruder, out DefenseEngagement engagement);
    bool IsCellReservedForOther(CharacterActor actor, Vector2Int cell);
    bool ShouldHoldIntruder(InvasionIntruderRuntime intruder);
    bool CanIntruderAdvanceTo(InvasionIntruderRuntime intruder, Vector2Int nextCell);
    bool TryAssignManual(CharacterActor defender, InvasionIntruderRuntime intruder, out string failureReason);
    bool TryBeginOwnerFinalDefense(InvasionIntruderRuntime intruder, CharacterActor owner);
    void NotifyActorDowned(CharacterActor actor);
    void NotifyIntruderFinished(InvasionIntruderRuntime intruder);
    DefenseEngagementSaveSnapshot Capture();
    void Restore(DefenseEngagementSaveSnapshot snapshot, IList<string> warnings);
}

public interface IInvasionOwnerEvacuationService
{
    bool IsEvacuating { get; }
    bool HasReachedTarget { get; }
    CharacterActor Owner { get; }
    Vector2Int TargetCell { get; }
    string StatusText { get; }
    OwnerEvacuationSaveSnapshot Capture();
    void Restore(OwnerEvacuationSaveSnapshot snapshot, IList<string> warnings);
}

[Serializable]
public sealed class DefenseResponsePolicySaveSnapshot
{
    public List<DefenseResponsePolicyData> policies = new List<DefenseResponsePolicyData>();
    public List<DefensePolicyAssignmentSaveData> assignments = new List<DefensePolicyAssignmentSaveData>();
}

[Serializable]
public sealed class DefensePolicyAssignmentSaveData
{
    public string characterId = string.Empty;
    public string policyId = string.Empty;
}

[Serializable]
public sealed class DefenseEngagementSaveSnapshot
{
    public List<DefenseEngagementSaveData> engagements = new List<DefenseEngagementSaveData>();
}

[Serializable]
public sealed class DefenseEngagementSaveData
{
    public string id = string.Empty;
    public string intruderId = string.Empty;
    public string leadGuardId = string.Empty;
    public string reserveGuardId = string.Empty;
    public string rangedGuardId = string.Empty;
    public string secondaryRangedGuardId = string.Empty;
    public DefenseEngagementState state;
    public int intruderStopX;
    public int intruderStopY;
    public int guardX;
    public int guardY;
    public int reserveX;
    public int reserveY;
    public int rangedX;
    public int rangedY;
    public int secondaryRangedX;
    public int secondaryRangedY;
    public bool hasReserveCell;
    public bool ownerFinalDefense;
    public bool forced;
    public float guardAttackRemaining;
    public float intruderAttackRemaining;
    public float rangedAttackRemaining;
    public float secondaryRangedAttackRemaining;
    public int exchangeCount;
}

[Serializable]
public sealed class OwnerEvacuationSaveSnapshot
{
    public bool active;
    public int targetX;
    public int targetY;
    public bool usedAdministrationRoom;
    public string statusText = string.Empty;
}

public static class DefenseCombatFormula
{
    public static float CalculateDamage(CharacterActor attacker, CharacterActor defender, float attackMultiplier = 1f)
    {
        if (attacker == null || defender == null)
        {
            return 0f;
        }

        return CalculateDamage(
            attacker.GetCharacterStat(CharacterStatType.Attack),
            attacker.GetCharacterStat(CharacterStatType.Strength),
            attacker.GetCombatPowerMultiplier(),
            defender.GetCharacterStat(CharacterStatType.Toughness),
            attackMultiplier);
    }

    public static float CalculateDamage(
        float attack,
        float strength,
        float combatPowerMultiplier,
        float defenderToughness,
        float attackMultiplier = 1f)
    {
        float raw = 4f + Mathf.Max(0f, attack) * 1.2f + Mathf.Max(0f, strength) * 0.6f;
        float mitigation = Mathf.Clamp(Mathf.Max(0f, defenderToughness) * 0.025f, 0f, 0.45f);
        return Mathf.Max(1f, raw
            * Mathf.Max(0.01f, combatPowerMultiplier)
            * Mathf.Max(0.01f, attackMultiplier)
            * (1f - mitigation));
    }

    public static float CalculateAttackInterval(CharacterActor attacker, float attackSpeedMultiplier = 1f)
    {
        if (attacker == null)
        {
            return 1.2f;
        }

        return CalculateAttackInterval(
            attacker.GetCharacterStat(CharacterStatType.Dexterity),
            attackSpeedMultiplier);
    }

    public static float CalculateAttackInterval(float dexterity, float attackSpeedMultiplier = 1f)
    {
        float interval = Mathf.Clamp(1.25f - Mathf.Max(0f, dexterity) * 0.05f, 0.55f, 1.2f);
        return Mathf.Clamp(interval / Mathf.Max(0.1f, attackSpeedMultiplier), 0.35f, 1.5f);
    }
}
