using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class WorkTargetSelector
{
    private readonly AbilityWork work;
    public WorkTargetCandidate LastRejectedCandidate { get; private set; }

    public WorkTargetSelector(AbilityWork work)
    {
        this.work = work;
    }

    public bool TryAssignWork(
        GridPathSearchResult searchResult = null,
        FacilityWorkType requestedWorkType = FacilityWorkType.None)
    {
        if (work.HasPrioritySuppressTarget && CanUseSuppressFor(requestedWorkType))
        {
            work.AssignWork(null, FacilityWorkType.Guard);
            return true;
        }

        bool canStartWork = work.CanStartWorkAction();
        if (!canStartWork && !HasUrgentAvailableWork(searchResult, requestedWorkType))
        {
            work.AssignWork(null, FacilityWorkType.None);
            work.WorkerActor?.AddLog(work.IsOffDuty ? "작업 보류: 비번" : "작업 보류: 피로/기분 보호");
            return false;
        }

        if (!canStartWork)
        {
            work.SetDutyState(AbilityWork.DutyState.OnDuty);
            work.WorkerActor?.AddLog("비번 중 긴급 작업 합류");
        }

        if (work.PriorityWorkTarget != null)
        {
            bool forced = work.PriorityWorkType != FacilityWorkType.None;
            bool canUsePriorityForRequest = requestedWorkType == FacilityWorkType.None
                || work.PriorityWorkType == requestedWorkType;
            if (canUsePriorityForRequest
                && TryEvaluateWorkTarget(
                    work.PriorityWorkTarget,
                    searchResult,
                    work.PriorityWorkType,
                    forced,
                    out WorkTargetCandidate priorityCandidate))
            {
                work.AssignWork(work.PriorityWorkTarget, priorityCandidate.WorkType);
                return true;
            }

            work.WorkerActor?.AddLog("우선 작업 취소: 대상 사용 불가");
            work.ClearPriorityWorkTarget();
        }

        if (work.assignedShop != null
            && requestedWorkType != FacilityWorkType.None
            && TryEvaluateWorkTarget(
                work.assignedShop,
                searchResult,
                requestedWorkType,
                false,
                out WorkTargetCandidate assignedCandidate))
        {
            work.AssignWork(work.assignedShop, assignedCandidate.WorkType);
            return true;
        }

        if (work.assignedShop != null && CanUseAsWorkTarget(work.assignedShop, requestedWorkType))
        {
            return true;
        }

        TryGetBestCandidate(requestedWorkType, searchResult, out WorkTargetCandidate best);
        work.AssignWork(best.Building, best.WorkType);
        return work.assignedShop != null;
    }

    public bool CanUseAsWorkTarget(BuildableObject building)
    {
        return CanUseAsWorkTarget(building, FacilityWorkType.None);
    }

    public bool CanUseAsWorkTarget(BuildableObject building, FacilityWorkType requestedWorkType)
    {
        return TryEvaluateWorkTarget(building, null, requestedWorkType, false, out _);
    }

    public bool HasUrgentAvailableWork(
        GridPathSearchResult searchResult,
        FacilityWorkType requestedWorkType = FacilityWorkType.None)
    {
        foreach (BuildableObject building in GetReachableBuildings(searchResult))
        {
            if (TryEvaluateWorkTarget(building, searchResult, requestedWorkType, false, out WorkTargetCandidate candidate)
                && candidate.UrgencyScore >= 60f)
            {
                return true;
            }
        }

        return false;
    }

    public bool TryGetBestCandidate(
        FacilityWorkType requestedWorkType,
        GridPathSearchResult searchResult,
        out WorkTargetCandidate best)
    {
        List<WorkTargetCandidate> candidates = GetReachableBuildings(searchResult)
            .Select((building) =>
            {
                TryEvaluateWorkTarget(building, searchResult, requestedWorkType, false, out WorkTargetCandidate candidate);
                return candidate;
            })
            .ToList();

        best = candidates
            .Where((candidate) => candidate.IsValid)
            .OrderByDescending((candidate) => candidate.Score)
            .FirstOrDefault();

        if (best.IsValid)
        {
            LastRejectedCandidate = default;
            return true;
        }

        LastRejectedCandidate = candidates
            .Where((candidate) => !candidate.IsValid)
            .OrderByDescending(GetFailureRelevance)
            .FirstOrDefault();
        return false;
    }

    public float GetUtilityScore(FacilityWorkType requestedWorkType, GridPathSearchResult searchResult)
    {
        if (!TryGetBestCandidate(requestedWorkType, searchResult, out WorkTargetCandidate candidate))
        {
            return 0f;
        }

        return Mathf.Clamp01(candidate.Score / 460f);
    }

    public IEnumerable<BuildableObject> GetReachableBuildings(GridPathSearchResult searchResult)
    {
        if (searchResult != null)
        {
            return searchResult.GetAllReachableBuilding();
        }

        Grid activeGrid = work.WorkGridResolver.ResolveActiveGrid(work, null);
        if (activeGrid == null)
        {
            return Enumerable.Empty<BuildableObject>();
        }

        CharacterActor actor = work.WorkerActor;
        Vector2Int startPos = work.WorkGridResolver.GetGridPosition(activeGrid, actor);
        return activeGrid.GetAllReachableBuilding(startPos);
    }

    public IEnumerable<IWarehouseFacility> FindReachableWarehouses(GridPathSearchResult searchResult = null)
    {
        return GetReachableBuildings(searchResult)
            .OfType<IWarehouseFacility>()
            .Where((warehouse) => warehouse.HasWarehouseInventory);
    }

    public bool TryEvaluateWorkTarget(
        BuildableObject building,
        GridPathSearchResult searchResult,
        FacilityWorkType forcedWorkType,
        bool ignorePriority,
        out WorkTargetCandidate bestCandidate)
    {
        bestCandidate = WorkTargetCandidate.Invalid(
            building,
            "현재 작업할 수 없는 시설입니다",
            AIActionFailureKind.NoWork);
        WorkPriorityProfile priorities = work.WorkPriorities ?? WorkPriorityProfile.CreateDefault();

        if (building == null || building.isDestroy)
        {
            bestCandidate = WorkTargetCandidate.Invalid(
                building,
                "시설이 없습니다",
                building != null && building.isDestroy
                    ? AIActionFailureKind.Destroyed
                    : AIActionFailureKind.NoDestination);
            LastRejectedCandidate = bestCandidate;
            return false;
        }

        if (building is not IWorkableFacility workable)
        {
            bestCandidate = WorkTargetCandidate.Invalid(
                building,
                "작업 가능한 시설이 아닙니다",
                AIActionFailureKind.Unsupported);
            LastRejectedCandidate = bestCandidate;
            return false;
        }

        if (building.Facility == null || building.Facility.supportedWorkTypes == FacilityWorkType.None)
        {
            bestCandidate = WorkTargetCandidate.Invalid(
                building,
                "지원하는 작업이 없습니다",
                AIActionFailureKind.Unsupported);
            LastRejectedCandidate = bestCandidate;
            return false;
        }

        if (!workable.CanAssignWorker(work.WorkerActor, out string failureReason))
        {
            bestCandidate = WorkTargetCandidate.Invalid(
                building,
                failureReason,
                AIActionFailure.ClassifyKind(failureReason, AIActionFailureKind.CannotStart));
            LastRejectedCandidate = bestCandidate;
            return false;
        }

        if (searchResult != null && !searchResult.GetAllReachableBuilding().Contains(building))
        {
            bestCandidate = WorkTargetCandidate.Invalid(
                building,
                "도달할 수 없는 대상입니다",
                AIActionFailureKind.NoPath);
            LastRejectedCandidate = bestCandidate;
            return false;
        }

        AIActionFailure lastWorkTypeFailure = AIActionFailure.None;
        foreach (FacilityWorkType workType in WorkTaskCatalog.GetSingleTypes(building.Facility.supportedWorkTypes))
        {
            if (forcedWorkType != FacilityWorkType.None && workType != forcedWorkType)
            {
                continue;
            }

            if (forcedWorkType == FacilityWorkType.None && work.ShouldThrottleRoutineWork(workType))
            {
                continue;
            }

            if (workType == FacilityWorkType.Restock)
            {
                if (building is not Shop shop)
                {
                    lastWorkTypeFailure = AIActionFailure.FromReason(
                        "보충 대상 상점이 아닙니다",
                        AIActionFailureKind.Unsupported,
                        building);
                    continue;
                }

                if (!shop.NeedsRestock)
                {
                    continue;
                }

                if (!shop.HasRestockSupply(FindReachableWarehouses(searchResult), out failureReason))
                {
                    lastWorkTypeFailure = AIActionFailure.FromReason(
                        failureReason,
                        AIActionFailureKind.NoWork,
                        building);
                    continue;
                }
            }

            if (workType == FacilityWorkType.Research
                && !work.BlueprintResearchWorkService.HasResearchWorkFor(building))
            {
                continue;
            }

            WorkPriorityLevel priority = ignorePriority
                ? WorkPriorityLevel.Priority1
                : priorities.GetPriority(workType);
            if (priority == WorkPriorityLevel.Off)
            {
                continue;
            }

            if (!building.CanAssignWork(workType, out failureReason))
            {
                lastWorkTypeFailure = AIActionFailure.FromReason(
                    failureReason,
                    AIActionFailureKind.NoWork,
                    building);
                continue;
            }

            WorkTargetCandidate candidate = BuildCandidate(building, workType, priority, searchResult);
            if (!bestCandidate.IsValid || candidate.Score > bestCandidate.Score)
            {
                bestCandidate = candidate;
            }
        }

        if (!bestCandidate.IsValid)
        {
            string reason = forcedWorkType != FacilityWorkType.None
                ? $"{WorkTaskCatalog.GetDisplayName(forcedWorkType)} 작업을 수행할 수 없습니다"
                : "켜진 작업 우선순위가 없습니다";
            bestCandidate = WorkTargetCandidate.Invalid(
                building,
                lastWorkTypeFailure.HasFailure ? lastWorkTypeFailure.ToString() : reason,
                lastWorkTypeFailure.HasFailure ? lastWorkTypeFailure.Kind : AIActionFailureKind.NoWork);
            LastRejectedCandidate = bestCandidate;
            return false;
        }

        LastRejectedCandidate = default;
        return true;
    }

    private WorkTargetCandidate BuildCandidate(
        BuildableObject building,
        FacilityWorkType workType,
        WorkPriorityLevel priority,
        GridPathSearchResult searchResult)
    {
        CharacterActor actor = work.WorkerActor;
        float urgency = building.GetWorkUrgency(workType);
        float preferenceScore = actor != null ? actor.GetWorkPreferenceScore(workType) : 0.5f;
        float speedScore = actor != null ? Mathf.Clamp01(actor.GetWorkSpeedMultiplier(workType) / 2f) : 0.5f;
        float distanceScore = GetDistanceScore(building, searchResult);
        float roomContextScore = GetRoomContextScore(building);
        float facilityStateScore = GetFacilityStateScore(building, workType);
        float score = priority.GetBaseScore()
            + urgency
            + (preferenceScore * 35f)
            + (speedScore * 25f)
            + distanceScore
            + roomContextScore
            + facilityStateScore;

        return new WorkTargetCandidate(
            building,
            workType,
            priority,
            score,
            urgency,
            string.Empty);
    }

    private static int GetFailureRelevance(WorkTargetCandidate candidate)
    {
        return candidate.FailureKind switch
        {
            AIActionFailureKind.DestinationOccupied => 50,
            AIActionFailureKind.NoPath => 40,
            AIActionFailureKind.NoWork => 30,
            AIActionFailureKind.OffDuty => 20,
            AIActionFailureKind.Unsupported => 10,
            _ => 0
        };
    }

    private static float GetDistanceScore(BuildableObject building, GridPathSearchResult searchResult)
    {
        if (building == null)
        {
            return 0f;
        }

        Queue<GridMoveStep> path = searchResult != null
            ? searchResult.GetMovePathTo(building)
            : null;

        if (path == null || path.Count == 0)
        {
            return 25f;
        }

        return Mathf.Clamp(25f - path.Count, 0f, 25f);
    }

    private static float GetRoomContextScore(BuildableObject building)
    {
        if (building == null)
        {
            return 0f;
        }

        try
        {
            FacilityRoomOperationalProfile profile = building.GetRoomOperationalProfile();
            if (profile == null || profile.Room == null)
            {
                return 8f;
            }

            if (!profile.IsUsableRoom)
            {
                return building.Facility != null && building.Facility.requiresRoomRole ? -15f : 0f;
            }

            return Mathf.Lerp(8f, 28f, profile.Room.GetQualityScore());
        }
        catch (System.InvalidOperationException)
        {
            return 8f;
        }
    }

    private static float GetFacilityStateScore(BuildableObject building, FacilityWorkType workType)
    {
        if (building == null)
        {
            return 0f;
        }

        float score = Mathf.Lerp(-8f, 12f, Mathf.Clamp01(building.OperationalState.cleanliness / 100f));
        if (building.IsDamaged && workType != FacilityWorkType.Repair)
        {
            score -= 18f;
        }

        return score;
    }

    private static bool CanUseSuppressFor(FacilityWorkType requestedWorkType)
    {
        return requestedWorkType == FacilityWorkType.None || requestedWorkType == FacilityWorkType.Guard;
    }
}
