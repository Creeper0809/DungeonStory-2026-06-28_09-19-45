using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class WorkTargetSelector
{
    private const float WorkUtilityScoreDivisor = 460f;

    private readonly AbilityWork work;
    private CharacterActor cachedContextActor;
    private CharacterAiDecisionContext cachedDecisionContext;
    private int cachedDecisionContextFrame = -1;
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
            work.WorkerActor?.AddActivity(CharacterActivityEvent.Create(
                CharacterActivityKinds.Duty,
                CharacterActivityOutcomes.Blocked,
                work.IsOffDuty ? "작업 보류: 비번" : "작업 보류: 피로/기분 보호",
                reasonCode: work.IsOffDuty ? "off-duty" : "wellbeing-protection",
                sentiment: -0.2f,
                bubbleEligible: true));
            return false;
        }

        if (!canStartWork)
        {
            work.SetDutyState(AbilityWork.DutyState.OnDuty);
            work.WorkerActor?.AddActivity(CharacterActivityEvent.Create(
                CharacterActivityKinds.Duty,
                CharacterActivityOutcomes.Responded,
                "비번 중 긴급 작업 합류",
                reasonCode: "urgent-work"));
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

            work.WorkerActor?.AddActivity(CharacterActivityEvent.Work(
                work.PriorityWorkType,
                CharacterActivityOutcomes.Cancelled,
                "우선 작업 취소: 대상 사용 불가",
                work.PriorityWorkTarget,
                reasonCode: "target-unavailable",
                bubbleEligible: true));
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
        best = default;
        WorkTargetCandidate rejected = default;
        float bestScore = float.NegativeInfinity;
        int bestRejectedRelevance = int.MinValue;
        foreach (BuildableObject building in GetReachableBuildings(searchResult))
        {
            TryEvaluateWorkTarget(
                building,
                searchResult,
                requestedWorkType,
                false,
                out WorkTargetCandidate candidate);
            if (candidate.IsValid)
            {
                if (!best.IsValid || candidate.Score > bestScore)
                {
                    best = candidate;
                    bestScore = candidate.Score;
                }

                continue;
            }

            int relevance = GetFailureRelevance(candidate);
            if (relevance > bestRejectedRelevance)
            {
                rejected = candidate;
                bestRejectedRelevance = relevance;
            }
        }

        if (best.IsValid)
        {
            LastRejectedCandidate = default;
            RecordBestWorkBreakdown(best);
            return true;
        }

        LastRejectedCandidate = rejected;
        return false;
    }

    public float GetUtilityScore(FacilityWorkType requestedWorkType, GridPathSearchResult searchResult)
    {
        if (!TryGetBestCandidate(requestedWorkType, searchResult, out WorkTargetCandidate candidate))
        {
            return 0f;
        }

        return Mathf.Clamp01(candidate.Score / WorkUtilityScoreDivisor);
    }

    public IEnumerable<BuildableObject> GetReachableBuildings(GridPathSearchResult searchResult)
    {
        IEnumerable<BuildableObject> buildings;
        if (searchResult != null)
        {
            buildings = searchResult.GetAllReachableBuilding();
        }
        else
        {
            Grid activeGrid = work.WorkGridResolver.ResolveActiveGrid(work, null);
            if (activeGrid == null)
            {
                return Enumerable.Empty<BuildableObject>();
            }

            CharacterActor actor = work.WorkerActor;
            Vector2Int startPos = work.WorkGridResolver.GetGridPosition(activeGrid, actor);
            GridPathSearchBroker.TryGetSearch(activeGrid, startPos, () => true, out searchResult);
            if (searchResult == null)
            {
                return Enumerable.Empty<BuildableObject>();
            }

            buildings = searchResult.GetAllReachableBuilding();
        }

        return MergeExteriorWorkMarkers(buildings, searchResult);
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

        FacilityWorkType supportedTypes = building is ConstructionSite
            ? FacilityWorkType.Construct
            : building.Facility != null
                ? WildlifeButcherFacilityUtility.AddFallbackWorkTypes(building, building.Facility.supportedWorkTypes)
                : FacilityWorkType.None;
        supportedTypes = SurvivalFacilityUtility.AddFallbackWorkTypes(building, supportedTypes);
        supportedTypes = CombatEquipmentMaintenanceFacilityUtility.AddFallbackWorkTypes(
            building,
            supportedTypes);
        if (supportedTypes == FacilityWorkType.None)
        {
            bestCandidate = WorkTargetCandidate.Invalid(
                building,
                "지원하는 작업이 없습니다",
                AIActionFailureKind.Unsupported);
            LastRejectedCandidate = bestCandidate;
            return false;
        }

        FacilityAssignmentStatus workerStatus = workable.GetWorkerAssignmentStatus(work.WorkerActor);
        if (!workerStatus.IsAllowed)
        {
            bestCandidate = WorkTargetCandidate.Invalid(
                building,
                workerStatus.Reason,
                workerStatus.FailureKind.ToAiActionFailureKind());
            LastRejectedCandidate = bestCandidate;
            return false;
        }

        if (building is ConstructionSite safetySite)
        {
            ConstructionSafetyResult safety = safetySite.GetConstructionSafetyState(
                work.WorkerActor,
                forced: ignorePriority);
            if (!safety.IsSafe)
            {
                bestCandidate = WorkTargetCandidate.Invalid(
                    building,
                    safety.Message,
                    safety.Reason == ConstructionSafetyReason.WorkerEscapeBlocked
                        || safety.Reason == ConstructionSafetyReason.EntranceBlocked
                            ? AIActionFailureKind.NoPath
                            : AIActionFailureKind.NoWork);
                LastRejectedCandidate = bestCandidate;
                return false;
            }
        }

        if (searchResult != null && !IsReachableWorkBuilding(building, searchResult))
        {
            bestCandidate = WorkTargetCandidate.Invalid(
                building,
                "도달할 수 없는 대상입니다",
                AIActionFailureKind.NoPath);
            LastRejectedCandidate = bestCandidate;
            return false;
        }

        AIActionFailure lastWorkTypeFailure = AIActionFailure.None;
        foreach (FacilityWorkType workType in WorkTaskCatalog.GetSingleTypes(supportedTypes))
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
                if (building is not IRestockableFacility restockable)
                {
                    lastWorkTypeFailure = AIActionFailure.Create(
                        AIActionFailureKind.Unsupported,
                        "재고를 보충할 수 없는 시설입니다",
                        building);
                    continue;
                }

                if (!restockable.NeedsRestock)
                {
                    continue;
                }

                if (!restockable.HasRestockSupply(
                    FindReachableWarehouses(searchResult),
                    out string supplyFailureReason))
                {
                    lastWorkTypeFailure = AIActionFailure.Create(
                        AIActionFailureKind.NoWork,
                        supplyFailureReason,
                        building);
                    continue;
                }
            }

            if (workType == FacilityWorkType.Research
                && !work.BlueprintResearchWorkService.HasResearchWorkFor(building))
            {
                continue;
            }

            if (workType == FacilityWorkType.Craft && !building.HasPendingEquipmentCraftWork())
            {
                continue;
            }

            if (workType == FacilityWorkType.Repair
                && !building.IsDamaged
                && !(EquipmentMaintenancePolicyRuntime.Active?.HasRepairWorkFor(building) ?? false))
            {
                continue;
            }

            if (workType == FacilityWorkType.Butcher
                && (WildlifeRuntime.Active == null || !WildlifeRuntime.Active.HasButcherWorkAvailable(building)))
            {
                continue;
            }

            if (SurvivalFacilityUtility.IsSurvivalWork(workType)
                && (SurvivalFoodRuntime.Active == null
                    || !SurvivalFoodRuntime.Active.HasSurvivalWorkAvailable(building, workType)))
            {
                continue;
            }

            if (HasExteriorWorkRuntime(building, workType)
                && !IsExteriorWorkAvailable(building, work.WorkerActor, workType))
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

            FacilityAssignmentStatus workStatus = building is ConstructionSite constructionSite
                ? constructionSite.GetConstructionWorkStatus()
                : building.GetWorkAssignmentStatus(workType);
            if (!workStatus.IsAllowed)
            {
                lastWorkTypeFailure = AIActionFailure.Create(
                    workStatus.FailureKind.ToAiActionFailureKind(),
                    workStatus.Reason,
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
        float urgency = building is ConstructionSite constructionSite
            ? constructionSite.GetWorkUrgency(workType)
            : building.GetWorkUrgency(workType);
        if (workType == FacilityWorkType.Repair
            && EquipmentMaintenancePolicyRuntime.Active != null)
        {
            urgency = Mathf.Max(
                urgency,
                EquipmentMaintenancePolicyRuntime.Active.GetRepairUrgency(building));
        }
        urgency += GetExteriorWorkUrgency(building, actor, workType);
        float preferenceScore = actor != null ? actor.GetWorkPreferenceScore(workType) : 0.5f;
        float speedScore = actor != null ? Mathf.Clamp01(actor.GetWorkSpeedMultiplier(workType) / 2f) : 0.5f;
        float distanceScore = GetDistanceScore(building, searchResult);
        float roomContextScore = GetRoomContextScore(building);
        float facilityStateScore = GetFacilityStateScore(building, workType);
        float roleFitScore = GetWorkRoleFitScore(building, workType);
        CharacterAiWorldSignalSnapshot signals = CharacterAiWorldSignalUtility.Capture(
            actor,
            CharacterAiBranch.Work,
            building,
            searchResult);
        float survivalPressure = actor != null
            ? GetDecisionContext(actor).EmergencyScore
            : 0f;
        float fatigueScale = Mathf.Lerp(1f, 0.25f, Mathf.Clamp01(Mathf.Max(urgency / 85f, survivalPressure)));
        float fatiguePenalty = actor != null && actor.AiMemory != null
            ? actor.AiMemory.GetRepeatedWorkFatigue(workType) * 18f * fatigueScale
            : 0f;
        float targetFatiguePenalty = actor != null && actor.AiMemory != null
            ? actor.AiMemory.GetRecentTargetWorkFatigue(building, workType) * 14f * fatigueScale
            : 0f;
        float queueBonus = Mathf.Clamp01(1f - signals.QueuePressure) * 8f;
        float pathConfidenceBonus = signals.PathConfidence * 9f;
        float scheduleBonus = signals.ScheduleScore * 8f;
        float weatherPenalty = signals.WeatherPressure * (IsExteriorWorkType(workType) ? 12f : 4f);
        float failurePenalty = signals.RecentFailurePressure * 10f;
        float movementPenalty = signals.RecentMovementPressure * 8f * fatigueScale;
        float softLockBonus = actor != null
            && actor.Blackboard != null
            && actor.Blackboard.SoftLockIntent != null
            && actor.Blackboard.SoftLockIntent.Matches(CharacterAiBranch.Work, building)
                ? 16f
                : 0f;
        float score = priority.GetBaseScore()
            + urgency
            + (preferenceScore * 35f)
            + (speedScore * 25f)
            + distanceScore
            + roomContextScore
            + facilityStateScore
            + roleFitScore
            + queueBonus
            + pathConfidenceBonus
            + scheduleBonus
            + softLockBonus
            - fatiguePenalty
            - targetFatiguePenalty
            - weatherPenalty
            - failurePenalty
            - movementPenalty;

        CharacterAiUtilityBreakdown breakdown = new CharacterAiUtilityBreakdown(
            GetWorkIntention(workType),
            $"{GetBuildingLabel(building)} {WorkTaskCatalog.GetDisplayName(workType)}");
        breakdown.Add(CharacterAiUtilityFactorKind.Priority, Mathf.Clamp01(priority.GetBaseScore() / 300f), 0.28f, priority.ToDisplayText());
        breakdown.Add(CharacterAiUtilityFactorKind.Need, Mathf.Clamp01(urgency / 100f), 0.22f, "긴급도");
        breakdown.Add(CharacterAiUtilityFactorKind.Personality, preferenceScore, 0.16f, "작업 적성");
        breakdown.Add(
            CharacterAiUtilityFactorKind.Momentum,
            Mathf.Clamp01((speedScore * 25f + softLockBonus) / 41f),
            0.1f,
            "작업 속도/하던 일 유지");
        breakdown.Add(CharacterAiUtilityFactorKind.Distance, Mathf.Clamp01(distanceScore / 25f), 0.08f, "거리");
        breakdown.Add(CharacterAiUtilityFactorKind.Room, Mathf.InverseLerp(-15f, 28f, roomContextScore), 0.08f, "방 환경");
        breakdown.Add(CharacterAiUtilityFactorKind.Reservation, Mathf.InverseLerp(-26f, 12f, facilityStateScore), 0.05f, "시설 상태");
        breakdown.Add(CharacterAiUtilityFactorKind.Queue, Mathf.Clamp01(queueBonus / 8f), 0.05f, "작업 혼잡");
        breakdown.Add(CharacterAiUtilityFactorKind.PathConfidence, signals.PathConfidence, 0.05f, "경로 신뢰");
        breakdown.Add(CharacterAiUtilityFactorKind.Schedule, signals.ScheduleScore, 0.04f, "근무 흐름");
        breakdown.Add(CharacterAiUtilityFactorKind.Weather, Mathf.Clamp01(1f - weatherPenalty / 12f), 0.03f, "외부 부담");
        breakdown.Add(
            CharacterAiUtilityFactorKind.Fatigue,
            Mathf.Clamp01(1f - (fatiguePenalty + targetFatiguePenalty + movementPenalty) / 40f),
            0.04f,
            "반복/이동 피로");
        breakdown.Add(CharacterAiUtilityFactorKind.Memory, Mathf.Clamp01(1f - failurePenalty / 10f), 0.03f, "최근 실패");
        breakdown.SetFinalScore(Mathf.Clamp01(score / WorkUtilityScoreDivisor));

        return new WorkTargetCandidate(
            building,
            workType,
            priority,
            score,
            urgency,
            string.Empty,
            AIActionFailureKind.None,
            breakdown.ToCompactString());
    }

    private void RecordBestWorkBreakdown(WorkTargetCandidate candidate)
    {
        if (!candidate.IsValid || work.WorkerActor == null || work.WorkerActor.Blackboard == null)
        {
            return;
        }

        CharacterAiUtilityBreakdown breakdown = new CharacterAiUtilityBreakdown(
            GetWorkIntention(candidate.WorkType),
            $"{GetBuildingLabel(candidate.Building)} {WorkTaskCatalog.GetDisplayName(candidate.WorkType)}");
        breakdown.Add(CharacterAiUtilityFactorKind.Priority, Mathf.Clamp01(candidate.Priority.GetBaseScore() / 300f), 0.35f, candidate.Priority.ToDisplayText());
        breakdown.Add(CharacterAiUtilityFactorKind.Need, Mathf.Clamp01(candidate.UrgencyScore / 100f), 0.3f, "긴급도");
        breakdown.Add(CharacterAiUtilityFactorKind.Reservation, 1f, 0.2f, "예약 가능");
        breakdown.Add(CharacterAiUtilityFactorKind.Momentum, Mathf.Clamp01(candidate.Score / WorkUtilityScoreDivisor), 0.15f, "종합 점수");
        breakdown.SetFinalScore(Mathf.Clamp01(candidate.Score / WorkUtilityScoreDivisor));
        work.WorkerActor.Blackboard.RecordUtilityBreakdown(breakdown);
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

    private CharacterAiDecisionContext GetDecisionContext(CharacterActor actor)
    {
        int frame = Time.frameCount;
        if (cachedDecisionContextFrame == frame && cachedContextActor == actor)
        {
            return cachedDecisionContext;
        }

        cachedContextActor = actor;
        cachedDecisionContext = CharacterAiDecisionContext.Capture(actor, CharacterAiBranch.Work);
        cachedDecisionContextFrame = frame;
        return cachedDecisionContext;
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

    private float GetRoomContextScore(BuildableObject building)
    {
        if (building == null)
        {
            return 0f;
        }

        if (work.RoomEnvironmentQuery != null)
        {
            return work.RoomEnvironmentQuery.GetFacilityPreferenceScore(building);
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
                return building.BuildingData.RequiresRoomRole() ? -15f : 0f;
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

        float score = Mathf.Lerp(-8f, 12f, Mathf.Clamp01(building.FacilityState.cleanliness / 100f));
        if (building.IsDamaged && workType != FacilityWorkType.Repair)
        {
            score -= 18f;
        }

        return score;
    }

    private static float GetWorkRoleFitScore(BuildableObject building, FacilityWorkType workType)
    {
        FacilityRole roles = building?.Facility?.roles ?? FacilityRole.None;
        if (roles == FacilityRole.None)
        {
            return 0f;
        }

        bool hasTraining = (roles & FacilityRole.Training) != 0;
        bool hasResearch = (roles & FacilityRole.Research) != 0;
        bool hasSecurity = (roles & FacilityRole.Security) != 0;
        bool hasMeal = (roles & FacilityRole.Meal) != 0;
        bool hasLogistics = (roles & FacilityRole.Logistics) != 0;
        bool hasHygiene = (roles & FacilityRole.Hygiene) != 0;

        return workType switch
        {
            FacilityWorkType.Guard when hasSecurity || hasTraining => 45f,
            FacilityWorkType.Guard when hasResearch || hasMeal || hasHygiene => -18f,
            FacilityWorkType.Research when hasResearch => 24f,
            FacilityWorkType.Research when hasTraining || hasSecurity => -18f,
            FacilityWorkType.Craft when hasLogistics => 16f,
            FacilityWorkType.Cook when hasMeal => 18f,
            FacilityWorkType.Butcher when hasMeal => 18f,
            FacilityWorkType.Restock when hasLogistics => 16f,
            FacilityWorkType.Clean when hasHygiene => 12f,
            _ => 0f
        };
    }

    private static CharacterAiIntentionType GetWorkIntention(FacilityWorkType workType)
    {
        return workType switch
        {
            FacilityWorkType.Haul => CharacterAiIntentionType.Logistics,
            FacilityWorkType.Hunt => CharacterAiIntentionType.Hunt,
            FacilityWorkType.Guard => CharacterAiIntentionType.Guard,
            FacilityWorkType.Rest => CharacterAiIntentionType.Recover,
            _ => CharacterAiIntentionType.Work
        };
    }

    private static bool IsExteriorWorkType(FacilityWorkType workType)
    {
        return workType == FacilityWorkType.Haul
            || workType == FacilityWorkType.Hunt
            || workType == FacilityWorkType.Guard
            || workType == FacilityWorkType.Reception
            || workType == FacilityWorkType.Clean
            || workType == FacilityWorkType.Repair;
    }

    private static string GetBuildingLabel(BuildableObject building)
    {
        if (building == null)
        {
            return "시설 없음";
        }

        return building.BuildingData != null && !string.IsNullOrWhiteSpace(building.BuildingData.objectName)
            ? building.BuildingData.objectName
            : building.name;
    }

    private static bool CanUseSuppressFor(FacilityWorkType requestedWorkType)
    {
        return requestedWorkType == FacilityWorkType.None || requestedWorkType == FacilityWorkType.Guard;
    }

    private IEnumerable<BuildableObject> MergeExteriorWorkMarkers(
        IEnumerable<BuildableObject> buildings,
        GridPathSearchResult searchResult)
    {
        HashSet<BuildableObject> seen = new HashSet<BuildableObject>();
        foreach (BuildableObject building in buildings ?? Enumerable.Empty<BuildableObject>())
        {
            if (building != null && seen.Add(building))
            {
                yield return building;
            }
        }

        if (work.ExteriorZoneQuery == null || searchResult == null)
        {
            yield break;
        }

        foreach (ExteriorZoneMarker marker in work.ExteriorZoneQuery.Zones)
        {
            if (marker == null
                || marker.isDestroy
                || !searchResult.ContainsPosition(marker.GridPosition)
                || !seen.Add(marker))
            {
                continue;
            }

            yield return marker;
        }
    }

    private static bool IsReachableWorkBuilding(BuildableObject building, GridPathSearchResult searchResult)
    {
        if (building == null || searchResult == null)
        {
            return false;
        }

        if (searchResult.ContainsVisitableOccupant(building))
        {
            return true;
        }

        if (building.Grid == searchResult.sourceGrid
            && building.buildPoses.Any(searchResult.ContainsPosition))
        {
            return true;
        }

        return building is ExteriorZoneMarker marker
            && searchResult.ContainsPosition(marker.GridPosition);
    }

    private static bool HasExteriorWorkRuntime(BuildableObject building, FacilityWorkType workType)
    {
        return building?.BuildingData != null
            && building.BuildingData.Abilities
                .OfType<IBuildingExteriorWorkRuntimeAbility>()
                .Any(ability => ability.SupportsExteriorWork(workType));
    }

    private static bool IsExteriorWorkAvailable(
        BuildableObject building,
        CharacterActor actor,
        FacilityWorkType workType)
    {
        if (building?.BuildingData == null)
        {
            return false;
        }

        foreach (IBuildingExteriorWorkRuntimeAbility ability in building.BuildingData.Abilities
                     .OfType<IBuildingExteriorWorkRuntimeAbility>())
        {
            if (ability.SupportsExteriorWork(workType)
                && ability.IsExteriorWorkAvailable(actor, building, workType))
            {
                return true;
            }
        }

        return false;
    }

    private static float GetExteriorWorkUrgency(
        BuildableObject building,
        CharacterActor actor,
        FacilityWorkType workType)
    {
        if (building?.BuildingData == null)
        {
            return 0f;
        }

        float urgency = 0f;
        foreach (IBuildingExteriorWorkRuntimeAbility ability in building.BuildingData.Abilities
                     .OfType<IBuildingExteriorWorkRuntimeAbility>())
        {
            if (ability.SupportsExteriorWork(workType)
                && ability.IsExteriorWorkAvailable(actor, building, workType))
            {
                urgency += Mathf.Max(0f, ability.GetExteriorWorkUrgency(actor, building, workType));
            }
        }

        return urgency;
    }
}
