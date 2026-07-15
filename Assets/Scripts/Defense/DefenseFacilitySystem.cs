using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Flags]
public enum DefenseTriggerTiming
{
    None = 0,
    OnEnter = 1 << 0,
    Periodic = 1 << 1,
    Cooldown = 1 << 2,
    GuardResponse = 1 << 3
}

public enum DefenseAttackConcept
{
    None,
    Physical,
    Poison,
    Fire,
    Lightning,
    Ice,
    Guard
}

public enum DefenseTargetRule
{
    EnteringIntruder,
    IntrudersInRoom,
    AllIntrudersInRoom,
    GuardTarget
}

public enum DefenseEffectKind
{
    Damage,
    Corrosion,
    Burn,
    Charge,
    Slow,
    GuardAttack
}

public enum DefenseStatusKind
{
    Corrosion,
    Burn,
    Charge,
    Slow
}

[Serializable]
public class DefenseEffectData
{
    public DefenseEffectKind kind;
    [Min(0f)] public float amount;
    [Min(0f)] public float duration;
    [Min(1)] public int stacks = 1;
    public string logTag;

    public DefenseEffectData Clone()
    {
        return new DefenseEffectData
        {
            kind = kind,
            amount = amount,
            duration = duration,
            stacks = stacks,
            logTag = logTag
        };
    }
}

[Serializable]
public class DefenseFacilityData
{
    public bool enabled;
    public DefenseAttackConcept concept;
    public DefenseTriggerTiming triggerTimings;
    public DefenseTargetRule targetRule;
    [Min(0f)] public float cooldownSeconds;
    [Min(0f)] public float periodicIntervalSeconds;
    [Min(0)] public int range;
    [Min(1)] public int star = 1;
    public string combatLogText;
    public DefenseEffectSO[] effectAssets = Array.Empty<DefenseEffectSO>();
    public DefenseEffectData[] effects = Array.Empty<DefenseEffectData>();

    public bool IsDefenseFacility => enabled && concept != DefenseAttackConcept.None;

    public bool SupportsTrigger(DefenseTriggerTiming timing)
    {
        return timing != DefenseTriggerTiming.None && (triggerTimings & timing) != 0;
    }
}

public class DefenseActivationReport
{
    private readonly List<string> effectTags = new List<string>();

    public DefenseActivationReport(DefenseFacility facility, CharacterActor target, DefenseTriggerTiming timing)
    {
        Facility = facility;
        TargetActor = target;
        Timing = timing;
        Concept = facility != null && facility.BuildingData != null
            ? facility.BuildingData.Defense.concept
            : DefenseAttackConcept.None;
    }

    public DefenseFacility Facility { get; }
    public CharacterActor TargetActor { get; }
    public DefenseTriggerTiming Timing { get; }
    public DefenseAttackConcept Concept { get; }
    public float TotalDamage { get; private set; }
    public float MovementDelaySeconds { get; private set; }
    public IReadOnlyList<string> EffectTags => effectTags;
    public bool Triggered => Facility != null && TargetActor != null;

    public void AddDamage(float amount)
    {
        TotalDamage += Mathf.Max(0f, amount);
    }

    public void AddMovementDelay(float seconds)
    {
        MovementDelaySeconds = Mathf.Max(MovementDelaySeconds, seconds);
    }

    public void AddEffectTag(string tag)
    {
        if (!string.IsNullOrWhiteSpace(tag) && !effectTags.Contains(tag))
        {
            effectTags.Add(tag);
        }
    }

    public string FormatSummary()
    {
        string facilityName = Facility != null && Facility.BuildingData != null
            ? Facility.BuildingData.objectName
            : "방어 시설";
        string damageText = TotalDamage > 0f ? $" 피해 {TotalDamage:0.#}" : string.Empty;
        string tags = effectTags.Count > 0 ? $" [{string.Join(", ", effectTags)}]" : string.Empty;
        return $"{facilityName} 발동{damageText}{tags}";
    }
}

public struct DefenseFacilityTriggeredEvent
{
    public DefenseActivationReport report;

    public DefenseFacilityTriggeredEvent(DefenseActivationReport report)
    {
        this.report = report;
    }

    private static DefenseFacilityTriggeredEvent e;

    public static void Trigger(DefenseActivationReport report)
    {
        e.report = report;
        EventObserver.TriggerEvent(e);
    }
}

public class DefenseFacility : Facility
{
    private float nextTriggerTime;

    public DefenseFacilityData Defense => BuildingData != null ? BuildingData.Defense : null;
    public float CooldownRemaining => Mathf.Max(0f, nextTriggerTime - Time.time);

    public bool CanTrigger(DefenseTriggerTiming timing, out string failureReason)
    {
        failureReason = string.Empty;
        DefenseFacilityData defense = Defense;
        if (defense == null || !defense.IsDefenseFacility)
        {
            failureReason = "방어 시설 아님";
            return false;
        }

        if (isDestroy)
        {
            failureReason = "시설 파괴됨";
            return false;
        }

        if (IsDamaged && (Facility == null || Facility.disabledWhenDamaged))
        {
            failureReason = "시설 파손";
            return false;
        }

        if (!defense.SupportsTrigger(timing))
        {
            failureReason = "발동 조건 불일치";
            return false;
        }

        if (Time.time < nextTriggerTime)
        {
            failureReason = "쿨타임";
            return false;
        }

        return true;
    }

    public DefenseActivationReport Trigger(
        CharacterActor intruder,
        DefenseTriggerTiming timing,
        IDefenseStatusRuntimeService statusRuntimeService)
    {
        if (intruder == null || !CanTrigger(timing, out _))
        {
            return null;
        }

        DefenseActivationReport report = new DefenseActivationReport(this, intruder, timing);
        nextTriggerTime = Time.time + Mathf.Max(0f, Defense.cooldownSeconds);
        DefenseEffectResolver.ApplyEffects(this, intruder, report, statusRuntimeService);
        intruder.AddLog(report.FormatSummary());
        DefenseFacilityTriggeredEvent.Trigger(report);
        return report;
    }

}

public static class DefenseFacilityResolver
{
    public static List<DefenseActivationReport> TriggerAt(
        Grid grid,
        CharacterActor intruder,
        Vector2Int position,
        DefenseTriggerTiming timing,
        IDefenseStatusRuntimeService statusRuntimeService)
    {
        if (statusRuntimeService == null)
        {
            throw new ArgumentNullException(nameof(statusRuntimeService));
        }

        List<DefenseActivationReport> reports = new List<DefenseActivationReport>();
        if (grid == null || intruder == null || !grid.IsValidGridPos(position))
        {
            return reports;
        }

        GridCell cell = grid.GetGridCell(position);
        if (cell == null)
        {
            return reports;
        }

        IEnumerable<DefenseFacility> defenses = cell.GetAllOccupants()
            .OfType<DefenseFacility>()
            .Distinct();
        foreach (DefenseFacility defense in defenses)
        {
            DefenseActivationReport report = defense.Trigger(intruder, timing, statusRuntimeService);
            if (report != null)
            {
                reports.Add(report);
            }
        }

        return reports;
    }

}

public static class DefenseEffectResolver
{
    private const int ChargeDischargeThreshold = 3;

    public static void ApplyEffects(
        DefenseFacility facility,
        CharacterActor target,
        DefenseActivationReport report,
        IDefenseStatusRuntimeService statusRuntimeService)
    {
        if (facility == null || target == null || report == null || facility.Defense == null)
        {
            return;
        }

        if (statusRuntimeService == null)
        {
            throw new ArgumentNullException(nameof(statusRuntimeService));
        }

        DefenseFacilityData defense = facility.Defense;
        DefenseStatusRuntime statusRuntime = statusRuntimeService.GetOrAdd(target)
            ?? throw new InvalidOperationException($"{nameof(IDefenseStatusRuntimeService)} could not provide {nameof(DefenseStatusRuntime)}.");
        DefenseEffectSO[] effectAssets = defense.effectAssets ?? Array.Empty<DefenseEffectSO>();
        if (effectAssets.Length > 0)
        {
            foreach (DefenseEffectSO effectAsset in effectAssets)
            {
                if (effectAsset == null)
                {
                    continue;
                }

                effectAsset.Apply(target, statusRuntime, report, defense);
            }

            return;
        }

        foreach (DefenseEffectData effect in defense.effects ?? Array.Empty<DefenseEffectData>())
        {
            if (effect == null)
            {
                continue;
            }

            ApplyEffect(effect, target, statusRuntime, report, defense);
        }
    }

    public static void ApplyEffect(
        DefenseEffectData effect,
        CharacterActor target,
        DefenseStatusRuntime statusRuntime,
        DefenseActivationReport report,
        DefenseFacilityData defense)
    {
        string logTag = !string.IsNullOrWhiteSpace(effect.logTag)
            ? effect.logTag
            : effect.kind.ToString();

        switch (effect.kind)
        {
            case DefenseEffectKind.Damage:
                ApplyDamage(target, statusRuntime, report, effect.amount, defense.combatLogText);
                report.AddEffectTag(logTag);
                break;
            case DefenseEffectKind.Corrosion:
                statusRuntime.ApplyStatus(DefenseStatusKind.Corrosion, effect.amount, effect.duration, effect.stacks);
                report.AddEffectTag(logTag);
                break;
            case DefenseEffectKind.Burn:
                statusRuntime.ApplyStatus(DefenseStatusKind.Burn, effect.amount, effect.duration, effect.stacks);
                report.AddEffectTag(logTag);
                break;
            case DefenseEffectKind.Charge:
                int chargeStacks = statusRuntime.ApplyStatus(DefenseStatusKind.Charge, effect.amount, effect.duration, effect.stacks);
                report.AddEffectTag($"{logTag} {chargeStacks}");
                if (chargeStacks >= ChargeDischargeThreshold)
                {
                    statusRuntime.ClearStatus(DefenseStatusKind.Charge);
                    ApplyDamage(target, statusRuntime, report, effect.amount, "축전 방전");
                    report.AddEffectTag("축전 방전");
                }
                break;
            case DefenseEffectKind.Slow:
                statusRuntime.ApplyStatus(DefenseStatusKind.Slow, effect.amount, effect.duration, effect.stacks);
                report.AddMovementDelay(effect.amount);
                report.AddEffectTag(logTag);
                break;
            case DefenseEffectKind.GuardAttack:
                ApplyDamage(target, statusRuntime, report, effect.amount, "경비실 교전");
                report.AddEffectTag(logTag);
                break;
        }
    }

    public static float TickStatuses(
        CharacterActor target,
        float deltaSeconds,
        IDefenseStatusRuntimeService statusRuntimeService)
    {
        if (target == null || target.IsDead)
        {
            return 0f;
        }

        if (statusRuntimeService == null)
        {
            throw new ArgumentNullException(nameof(statusRuntimeService));
        }

        return statusRuntimeService.TickStatuses(target, deltaSeconds);
    }

    private static void ApplyDamage(
        CharacterActor target,
        DefenseStatusRuntime statusRuntime,
        DefenseActivationReport report,
        float amount,
        string reason)
    {
        if (amount <= 0f || target == null || target.IsDead)
        {
            return;
        }

        float finalDamage = amount * statusRuntime.GetIncomingDamageMultiplier();
        target.ApplyDamage(finalDamage, reason);
        report.AddDamage(finalDamage);
    }
}

public readonly struct DefenseStatusSnapshot
{
    public DefenseStatusSnapshot(DefenseStatusKind kind, float value, float remainingSeconds, int stacks)
    {
        Kind = kind;
        Value = value;
        RemainingSeconds = Mathf.Max(0f, remainingSeconds);
        Stacks = Mathf.Max(0, stacks);
    }

    public DefenseStatusKind Kind { get; }
    public float Value { get; }
    public float RemainingSeconds { get; }
    public int Stacks { get; }
}

public class DefenseStatusRuntime : MonoBehaviour
{
    private readonly List<DefenseRuntimeStatus> statuses = new List<DefenseRuntimeStatus>();

    public IReadOnlyList<DefenseStatusSnapshot> ActiveStatuses => statuses
        .Select((status) => new DefenseStatusSnapshot(
            status.kind,
            status.value,
            status.remainingSeconds,
            status.stacks))
        .ToArray();

    public int ApplyStatus(DefenseStatusKind kind, float value, float duration, int stacks)
    {
        DefenseRuntimeStatus status = statuses.FirstOrDefault((item) => item.kind == kind);
        if (status == null)
        {
            status = new DefenseRuntimeStatus(kind);
            statuses.Add(status);
        }

        status.value = Mathf.Max(status.value, value);
        status.remainingSeconds = Mathf.Max(status.remainingSeconds, duration);
        status.stacks = kind == DefenseStatusKind.Charge
            ? Mathf.Max(0, status.stacks) + Mathf.Max(1, stacks)
            : Mathf.Max(1, stacks);
        return status.stacks;
    }

    public void ClearStatus(DefenseStatusKind kind)
    {
        statuses.RemoveAll((status) => status.kind == kind);
    }

    public float GetIncomingDamageMultiplier()
    {
        DefenseRuntimeStatus corrosion = statuses.FirstOrDefault((status) => status.kind == DefenseStatusKind.Corrosion);
        return corrosion != null ? 1f + Mathf.Max(0f, corrosion.value) : 1f;
    }

    public float Tick(CharacterActor target, float deltaSeconds)
    {
        if (target == null || deltaSeconds <= 0f)
        {
            return 0f;
        }

        float burnDamage = 0f;
        foreach (DefenseRuntimeStatus status in statuses.ToArray())
        {
            if (status.kind == DefenseStatusKind.Burn && status.value > 0f)
            {
                burnDamage += status.value * deltaSeconds;
            }

            status.remainingSeconds -= deltaSeconds;
            if (status.remainingSeconds <= 0f)
            {
                statuses.Remove(status);
            }
        }

        if (burnDamage > 0f && !target.IsDead)
        {
            target.ApplyDamage(burnDamage, "연소");
        }

        return burnDamage;
    }

    private sealed class DefenseRuntimeStatus
    {
        public DefenseRuntimeStatus(DefenseStatusKind kind)
        {
            this.kind = kind;
        }

        public DefenseStatusKind kind;
        public float value;
        public float remainingSeconds;
        public int stacks;
    }
}
