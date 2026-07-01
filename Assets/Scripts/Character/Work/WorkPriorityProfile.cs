using System;
using System.Collections.Generic;
using UnityEngine;

public enum WorkPriorityLevel
{
    Off = 0,
    Priority1 = 1,
    Priority2 = 2,
    Priority3 = 3
}

public static class WorkPriorityLevelExtensions
{
    public static WorkPriorityLevel Next(this WorkPriorityLevel priority)
    {
        return priority switch
        {
            WorkPriorityLevel.Priority1 => WorkPriorityLevel.Priority2,
            WorkPriorityLevel.Priority2 => WorkPriorityLevel.Priority3,
            WorkPriorityLevel.Priority3 => WorkPriorityLevel.Off,
            _ => WorkPriorityLevel.Priority1
        };
    }

    public static float GetBaseScore(this WorkPriorityLevel priority)
    {
        return priority switch
        {
            WorkPriorityLevel.Priority1 => 300f,
            WorkPriorityLevel.Priority2 => 200f,
            WorkPriorityLevel.Priority3 => 100f,
            _ => float.NegativeInfinity
        };
    }

    public static string ToDisplayText(this WorkPriorityLevel priority)
    {
        return priority switch
        {
            WorkPriorityLevel.Priority1 => "1",
            WorkPriorityLevel.Priority2 => "2",
            WorkPriorityLevel.Priority3 => "3",
            _ => "꺼짐"
        };
    }
}

public static class WorkTaskCatalog
{
    public static readonly FacilityWorkType[] TaskTypes =
    {
        FacilityWorkType.Operate,
        FacilityWorkType.Restock,
        FacilityWorkType.Repair,
        FacilityWorkType.Clean,
        FacilityWorkType.Research,
        FacilityWorkType.Guard,
        FacilityWorkType.Rescue,
        FacilityWorkType.Rest
    };

    public static string GetDisplayName(FacilityWorkType workType)
    {
        return workType switch
        {
            FacilityWorkType.Operate => "근무",
            FacilityWorkType.Restock => "보충",
            FacilityWorkType.Repair => "수리",
            FacilityWorkType.Clean => "청소",
            FacilityWorkType.Research => "연구",
            FacilityWorkType.Guard => "경비",
            FacilityWorkType.Rescue => "구조/응급",
            FacilityWorkType.Rest => "휴식",
            _ => "작업"
        };
    }

    public static IEnumerable<FacilityWorkType> GetSingleTypes(FacilityWorkType workTypes)
    {
        foreach (FacilityWorkType type in TaskTypes)
        {
            if ((workTypes & type) != 0)
            {
                yield return type;
            }
        }
    }
}

public static class WorkCommandResolver
{
    public static bool TryResolveFacilityCommand(
        Character actor,
        BuildableObject target,
        out FacilityWorkType workType,
        out string failureReason)
    {
        workType = FacilityWorkType.None;
        failureReason = string.Empty;

        if (actor == null)
        {
            failureReason = "선택된 캐릭터가 없습니다";
            return false;
        }

        if (target == null)
        {
            failureReason = "우선 지정할 대상이 없습니다";
            return false;
        }

        if (target.isDestroy || target.Facility == null)
        {
            failureReason = "작업 가능한 시설이 아닙니다";
            return false;
        }

        FacilityWorkType supportedTypes = target.Facility.supportedWorkTypes;
        if (supportedTypes == FacilityWorkType.None)
        {
            failureReason = "지원하는 작업이 없습니다";
            return false;
        }

        if (target.IsDamaged && TryUse(target, supportedTypes, FacilityWorkType.Repair, out workType))
        {
            return true;
        }

        if (NeedsRestock(target) && TryUse(target, supportedTypes, FacilityWorkType.Restock, out workType))
        {
            return true;
        }

        if (TryUse(target, supportedTypes, FacilityWorkType.Research, out workType))
        {
            return true;
        }

        List<FacilityWorkType> assignableTypes = new List<FacilityWorkType>();
        foreach (FacilityWorkType candidate in WorkTaskCatalog.GetSingleTypes(supportedTypes))
        {
            if (TryUse(target, supportedTypes, candidate, out workType))
            {
                assignableTypes.Add(workType);
            }
        }

        if (assignableTypes.Count == 1)
        {
            workType = assignableTypes[0];
            return true;
        }

        if (assignableTypes.Count > 1)
        {
            workType = FacilityWorkType.None;
            failureReason = "작업 종류를 명시해야 합니다.";
            return false;
        }

        failureReason = "수행 가능한 작업이 없습니다";
        return false;
    }

    public static bool IsSuppressTarget(Character target)
    {
        return target != null
            && (target.characterType == CharacterType.Intruder
                || (StaffDiscontentRuntime.Instance != null
                    && StaffDiscontentRuntime.Instance.IsRebellionTarget(target)));
    }

    public static bool TryResolveSuppressCommand(
        Character actor,
        Character target,
        out string failureReason)
    {
        failureReason = string.Empty;

        if (actor == null)
        {
            failureReason = "선택된 캐릭터가 없습니다";
            return false;
        }

        if (target == null)
        {
            failureReason = "제압할 대상이 없습니다";
            return false;
        }

        if (actor == target)
        {
            failureReason = "자기 자신은 제압할 수 없습니다";
            return false;
        }

        if (actor.IsDead)
        {
            failureReason = "선택한 캐릭터가 행동할 수 없습니다";
            return false;
        }

        if (target.IsDead)
        {
            failureReason = "이미 제압된 대상입니다";
            return false;
        }

        if (!IsSuppressTarget(target))
        {
            failureReason = "제압 대상이 아닙니다";
            return false;
        }

        return true;
    }

    private static bool TryUse(
        BuildableObject target,
        FacilityWorkType supportedTypes,
        FacilityWorkType candidate,
        out FacilityWorkType result)
    {
        result = FacilityWorkType.None;
        if ((supportedTypes & candidate) == 0)
        {
            return false;
        }

        if (!target.CanAssignWork(candidate, out _))
        {
            return false;
        }

        result = candidate;
        return true;
    }

    private static bool NeedsRestock(BuildableObject target)
    {
        if (target == null || target.Facility == null || target is not IStockedFacility stocked)
        {
            return false;
        }

        if (target.Facility.requiresStock && !stocked.HasAvailableStock)
        {
            return true;
        }

        return target.Facility.internalStockMax > 0
            && stocked.CurrentStock <= target.Facility.restockRequestThreshold;
    }
}

[Serializable]
public class WorkPriorityProfile
{
    public WorkPriorityLevel operate = WorkPriorityLevel.Priority1;
    public WorkPriorityLevel restock = WorkPriorityLevel.Priority2;
    public WorkPriorityLevel repair = WorkPriorityLevel.Priority2;
    public WorkPriorityLevel clean = WorkPriorityLevel.Priority3;
    public WorkPriorityLevel research = WorkPriorityLevel.Priority3;
    public WorkPriorityLevel guard = WorkPriorityLevel.Priority3;
    public WorkPriorityLevel rescue = WorkPriorityLevel.Priority2;
    public WorkPriorityLevel rest = WorkPriorityLevel.Priority3;

    public static WorkPriorityProfile CreateDefault()
    {
        return new WorkPriorityProfile();
    }

    public WorkPriorityProfile Clone()
    {
        return new WorkPriorityProfile
        {
            operate = operate,
            restock = restock,
            repair = repair,
            clean = clean,
            research = research,
            guard = guard,
            rescue = rescue,
            rest = rest
        };
    }

    public WorkPriorityLevel GetPriority(FacilityWorkType workType)
    {
        return workType switch
        {
            FacilityWorkType.Operate => operate,
            FacilityWorkType.Restock => restock,
            FacilityWorkType.Repair => repair,
            FacilityWorkType.Clean => clean,
            FacilityWorkType.Research => research,
            FacilityWorkType.Guard => guard,
            FacilityWorkType.Rescue => rescue,
            FacilityWorkType.Rest => rest,
            _ => GetBestPriority(workType)
        };
    }

    public void SetPriority(FacilityWorkType workType, WorkPriorityLevel priority)
    {
        switch (workType)
        {
            case FacilityWorkType.Operate:
                operate = priority;
                break;
            case FacilityWorkType.Restock:
                restock = priority;
                break;
            case FacilityWorkType.Repair:
                repair = priority;
                break;
            case FacilityWorkType.Clean:
                clean = priority;
                break;
            case FacilityWorkType.Research:
                research = priority;
                break;
            case FacilityWorkType.Guard:
                guard = priority;
                break;
            case FacilityWorkType.Rescue:
                rescue = priority;
                break;
            case FacilityWorkType.Rest:
                rest = priority;
                break;
        }
    }

    public bool IsEnabled(FacilityWorkType workType)
    {
        return GetPriority(workType) != WorkPriorityLevel.Off;
    }

    public void ApplyPreferredTypes(FacilityWorkType preferredTypes)
    {
        foreach (FacilityWorkType type in WorkTaskCatalog.GetSingleTypes(preferredTypes))
        {
            WorkPriorityLevel current = GetPriority(type);
            if (current == WorkPriorityLevel.Off || current > WorkPriorityLevel.Priority1)
            {
                SetPriority(type, WorkPriorityLevel.Priority1);
            }
        }
    }

    private WorkPriorityLevel GetBestPriority(FacilityWorkType workTypes)
    {
        WorkPriorityLevel best = WorkPriorityLevel.Off;
        foreach (FacilityWorkType type in WorkTaskCatalog.GetSingleTypes(workTypes))
        {
            WorkPriorityLevel current = GetPriority(type);
            if (current == WorkPriorityLevel.Off) continue;

            if (best == WorkPriorityLevel.Off || current < best)
            {
                best = current;
            }
        }

        return best;
    }
}
