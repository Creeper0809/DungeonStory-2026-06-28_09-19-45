using System;
using System.Collections.Generic;
using System.Linq;

public static class CodexTextFormatter
{
    public static string FormatEvolutionMutationTags(IReadOnlyList<string> mutationTags)
    {
        if (mutationTags == null || mutationTags.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(", ", mutationTags
            .Where((tag) => !string.IsNullOrWhiteSpace(tag))
            .Distinct());
    }

    public static string FormatFacilityRoles(FacilityRole roles)
    {
        return string.Join(", ", Enum.GetValues(typeof(FacilityRole))
            .Cast<FacilityRole>()
            .Where((role) => role != FacilityRole.None && (roles & role) != 0)
            .Select(FormatFacilityRole));
    }

    public static string FormatWorkTypes(FacilityWorkType workTypes)
    {
        return string.Join(", ", Enum.GetValues(typeof(FacilityWorkType))
            .Cast<FacilityWorkType>()
            .Where((workType) => workType != FacilityWorkType.None && (workTypes & workType) != 0)
            .Select(FormatWorkType));
    }

    public static string FormatDefenseConcept(DefenseAttackConcept concept)
    {
        return concept switch
        {
            DefenseAttackConcept.Physical => "물리",
            DefenseAttackConcept.Poison => "독",
            DefenseAttackConcept.Fire => "화염",
            DefenseAttackConcept.Lightning => "번개",
            DefenseAttackConcept.Ice => "냉기",
            DefenseAttackConcept.Guard => "경비",
            _ => "없음"
        };
    }

    public static string FormatTriggerTimings(DefenseTriggerTiming timings)
    {
        if (timings == DefenseTriggerTiming.None)
        {
            return "없음";
        }

        return string.Join(", ", Enum.GetValues(typeof(DefenseTriggerTiming))
            .Cast<DefenseTriggerTiming>()
            .Where((timing) => timing != DefenseTriggerTiming.None && (timings & timing) != 0)
            .Select((timing) => timing switch
            {
                DefenseTriggerTiming.OnEnter => "입장 시",
                DefenseTriggerTiming.Periodic => "머무는 동안",
                DefenseTriggerTiming.Cooldown => "쿨타임",
                DefenseTriggerTiming.GuardResponse => "경비 반응",
                _ => timing.ToString()
            }));
    }

    public static string FormatTargetRule(DefenseTargetRule targetRule)
    {
        return targetRule switch
        {
            DefenseTargetRule.EnteringIntruder => "입장한 침입자",
            DefenseTargetRule.IntrudersInRoom => "방 안 침입자",
            DefenseTargetRule.AllIntrudersInRoom => "방 안 모든 침입자",
            DefenseTargetRule.GuardTarget => "경비 대상",
            _ => targetRule.ToString()
        };
    }

    public static IEnumerable<string> FormatDefenseEffects(DefenseFacilityData defense)
    {
        IEnumerable<DefenseEffectData> effectData = defense.effects ?? Array.Empty<DefenseEffectData>();
        if (defense.effectAssets != null && defense.effectAssets.Length > 0)
        {
            return defense.effectAssets
                .Where((effect) => effect != null)
                .Select((effect) => FormatEffect(effect.Kind, effect.Amount, effect.Duration, effect.Stacks));
        }

        return effectData.Select((effect) => FormatEffect(effect.kind, effect.amount, effect.duration, effect.stacks));
    }

    private static string FormatFacilityRole(FacilityRole role)
    {
        return role switch
        {
            FacilityRole.Meal => "식사",
            FacilityRole.Purchase => "구매",
            FacilityRole.Rest => "휴식",
            FacilityRole.Training => "훈련",
            FacilityRole.Research => "연구",
            FacilityRole.Mana => "마력",
            FacilityRole.Logistics => "물류",
            FacilityRole.Toilet => "Toilet",
            FacilityRole.Hygiene => "Hygiene",
            _ => role.ToString()
        };
    }

    private static string FormatWorkType(FacilityWorkType workType)
    {
        return workType switch
        {
            FacilityWorkType.Operate => "근무",
            FacilityWorkType.Restock => "보충",
            FacilityWorkType.Repair => "수리",
            FacilityWorkType.Clean => "청소",
            FacilityWorkType.Research => "연구",
            FacilityWorkType.Guard => "경비",
            FacilityWorkType.Rescue => "구조",
            FacilityWorkType.Rest => "휴식",
            _ => workType.ToString()
        };
    }

    private static string FormatEffect(DefenseEffectKind kind, float amount, float duration, int stacks)
    {
        string name = kind switch
        {
            DefenseEffectKind.Damage => "피해",
            DefenseEffectKind.Corrosion => "방어력 감소",
            DefenseEffectKind.Burn => "지속 피해",
            DefenseEffectKind.Charge => "축전",
            DefenseEffectKind.Slow => "감속",
            DefenseEffectKind.GuardAttack => "근접 교전",
            _ => kind.ToString()
        };

        List<string> parts = new List<string> { name };
        if (amount > 0f)
        {
            parts.Add($"{amount:0.#}");
        }

        if (duration > 0f)
        {
            parts.Add($"{duration:0.#}초");
        }

        if (stacks > 1)
        {
            parts.Add($"{stacks}중첩");
        }

        return string.Join(" ", parts);
    }
}
