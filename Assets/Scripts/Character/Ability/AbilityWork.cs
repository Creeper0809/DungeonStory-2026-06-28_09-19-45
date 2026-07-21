using System;
using System.Collections;
using UnityEngine;
using VContainer;

public class AbilityWork : CharacterAbility
{
    private static readonly IFloatingIconFeedbackService FallbackFloatingIconFeedbackService =
        new AbilityWorkNoopFloatingIconFeedbackService();

    public enum DutyState
    {
        OnDuty,
        OffDuty
    }

    public BuildableObject assignedShop;
    public bool isWorking;

    [SerializeField] private WorkPriorityProfile workPriorities = WorkPriorityProfile.CreateDefault();
    [SerializeField] private float restProtectionSleepThreshold = 1f;
    [SerializeField] private float restProtectionResumeSleepThreshold = 35f;
    [SerializeField, Min(0f)] private float minimumRestProtectionSeconds = 6f;
    [SerializeField] private float restRecoveryOnWait = 15f;
    [SerializeField] private float offDutySleepThreshold = 25f;
    [SerializeField] private float returnToWorkSleepThreshold = 55f;
    [SerializeField] private float offDutyMoodThreshold = 25f;
    [SerializeField] private float returnToWorkMoodThreshold = 45f;
    [SerializeField] private float minimumOffDutySeconds = 8f;
    [SerializeField] private float sleepDrainPerWorkTick = 3f;
    [SerializeField] private float moodDrainPerWorkTick = 1f;
    [SerializeField] private float suppressBaseDamage = 18f;
    [SerializeField] private float suppressAttackInterval = 0.55f;
    [SerializeField, Min(0.5f)] private float routineOperateShiftSeconds = 45f;
    [SerializeField, Min(0f)] private float routineOperateCooldownSeconds = 4f;

    private AbilitySchedule schedule;
    private FacilityWorkType assignedWorkType = FacilityWorkType.None;
    private WorkTargetSelector targetSelector;
    private WorkTaskExecutor taskExecutor;
    private WorkDutyController dutyController;
    private WorkCommandHandler commandHandler;
    private IBlueprintResearchWorkService blueprintResearchWorkService;
    private IStaffDiscontentRuntimeService staffDiscontentRuntimeService;
    private IFloatingIconFeedbackService floatingIconFeedbackService;
    private IWorkGridResolver workGridResolver;
    private IFacilityCandidateCache facilityCandidateCache;
    private IRoomEnvironmentQuery roomEnvironmentQuery;
    private bool isScheduleBound;
    private float routineOperateCooldownUntil;
    private Coroutine activeWorkRoutine;
    private Coroutine activeWorkCheckRoutine;
    private int activeWorkRunId;

    public BuildableObject PriorityWorkTarget => CommandHandler.PriorityWorkTarget;
    public CharacterActor PrioritySuppressActor => CommandHandler.PrioritySuppressActor;
    public bool HasPrioritySuppressTarget => CommandHandler.HasPrioritySuppressTarget;
    public FacilityWorkType PriorityWorkType => CommandHandler.PriorityWorkType;
    public WorkPriorityProfile WorkPriorities => workPriorities ??= WorkPriorityProfile.CreateDefault();
    public FacilityWorkType AssignedWorkType => assignedWorkType;
    public float RestRecoveryOnWait => restRecoveryOnWait;
    public DutyState CurrentDutyState => DutyController.CurrentState;
    public bool IsOffDuty => DutyController.IsOffDuty;
    public WorkTargetCandidate LastRejectedWorkCandidate => TargetSelector.LastRejectedCandidate;

    public CharacterActor WorkerActor => actor;
    public AbilityMove WorkerMove => move;
    public Grid CachedGrid => grid;
    internal IBlueprintResearchWorkService BlueprintResearchWorkService =>
        RuntimeDependency.Require(blueprintResearchWorkService, this);
    internal IStaffDiscontentRuntimeService StaffDiscontentRuntimeService =>
        RuntimeDependency.Require(staffDiscontentRuntimeService, this);
    internal IFloatingIconFeedbackService FloatingIconFeedbackService => floatingIconFeedbackService
        ?? FallbackFloatingIconFeedbackService;
    internal IWorkGridResolver WorkGridResolver => RuntimeDependency.Require(workGridResolver, this);
    internal IFacilityCandidateCache FacilityCandidateCacheService =>
        RuntimeDependency.Require(facilityCandidateCache, this);
    internal IRoomEnvironmentQuery RoomEnvironmentQuery => roomEnvironmentQuery;

    internal float GetWorkEnvironmentDurationMultiplier(FacilityWorkType workType)
    {
        return roomEnvironmentQuery?.GetWorkDurationMultiplier(assignedShop, workType) ?? 1f;
    }

    internal float RestProtectionSleepThreshold => restProtectionSleepThreshold;
    internal float RestProtectionResumeSleepThreshold => Mathf.Max(
        restProtectionSleepThreshold,
        restProtectionResumeSleepThreshold);
    internal float MinimumRestProtectionSeconds => minimumRestProtectionSeconds;
    internal float OffDutySleepThreshold => offDutySleepThreshold;
    internal float ReturnToWorkSleepThreshold => returnToWorkSleepThreshold;
    internal float OffDutyMoodThreshold => offDutyMoodThreshold;
    internal float ReturnToWorkMoodThreshold => returnToWorkMoodThreshold;
    internal float MinimumOffDutySeconds => minimumOffDutySeconds;
    internal float SleepDrainPerWorkTick => sleepDrainPerWorkTick;
    internal float MoodDrainPerWorkTick => moodDrainPerWorkTick;
    internal float SuppressBaseDamage => suppressBaseDamage;
    internal float SuppressAttackInterval => suppressAttackInterval;
    internal float RoutineOperateShiftSeconds => routineOperateShiftSeconds;
    internal bool LastWorkRunCompleted => DutyController.LastWorkRunCompleted;

    private WorkTargetSelector TargetSelector
    {
        get
        {
            EnsureWorkModules();
            return targetSelector;
        }
    }

    private WorkTaskExecutor TaskExecutor
    {
        get
        {
            EnsureWorkModules();
            return taskExecutor;
        }
    }

    private WorkDutyController DutyController
    {
        get
        {
            EnsureWorkModules();
            return dutyController;
        }
    }

    private WorkCommandHandler CommandHandler
    {
        get
        {
            EnsureWorkModules();
            return commandHandler;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        EnsureWorkModules();
        TryBindScheduleEvents();
    }

    [Inject]
    public void ConstructAbilityWork(
        IBlueprintResearchWorkService blueprintResearchWorkService,
        IStaffDiscontentRuntimeService staffDiscontentRuntimeService,
        IFloatingIconFeedbackService floatingIconFeedbackService,
        IWorkGridResolver workGridResolver,
        IFacilityCandidateCache facilityCandidateCache,
        IRoomEnvironmentQuery roomEnvironmentQuery)
    {
        this.blueprintResearchWorkService = blueprintResearchWorkService
            ?? throw new ArgumentNullException(nameof(blueprintResearchWorkService));
        this.staffDiscontentRuntimeService = staffDiscontentRuntimeService
            ?? throw new ArgumentNullException(nameof(staffDiscontentRuntimeService));
        this.floatingIconFeedbackService = floatingIconFeedbackService
            ?? throw new ArgumentNullException(nameof(floatingIconFeedbackService));
        this.workGridResolver = workGridResolver
            ?? throw new ArgumentNullException(nameof(workGridResolver));
        this.facilityCandidateCache = facilityCandidateCache
            ?? throw new ArgumentNullException(nameof(facilityCandidateCache));
        this.roomEnvironmentQuery = roomEnvironmentQuery;
    }

    public override void Initializtion(CharacterSO data)
    {
        base.Initializtion(data);
        EnsureWorkModules();
        TryBindScheduleEvents();
        workPriorities = data != null && data.defaultWorkPriorities != null
            ? data.defaultWorkPriorities.Clone()
            : WorkPriorityProfile.CreateDefault();
        if (data != null && data.role == CharacterRole.Owner)
        {
            workPriorities.ApplyPreferredTypes(data.ownerPreferredWorkTypes);
        }

        DutyController.InitializeWorkerCondition(data);
        TryAssignShop();
    }

    public void EnsureWorkReferences()
    {
        CacheCommonReferences();
    }

    public bool TryAssignShop(GridPathSearchResult searchResult = null)
    {
        return TryAssignWork(FacilityWorkType.None, searchResult);
    }

    public bool TryAssignWork(FacilityWorkType requestedWorkType, GridPathSearchResult searchResult = null)
    {
        return TargetSelector.TryAssignWork(searchResult, requestedWorkType);
    }

    public bool TryGetBestWorkCandidate(
        FacilityWorkType requestedWorkType,
        GridPathSearchResult searchResult,
        out WorkTargetCandidate candidate)
    {
        bool found = TargetSelector.TryGetBestCandidate(requestedWorkType, searchResult, out candidate);
        if (!found && !isWorking)
        {
            AssignWork(null, FacilityWorkType.None);
        }

        return found;
    }

    public float GetWorkUtilityScore(FacilityWorkType requestedWorkType, GridPathSearchResult searchResult)
    {
        return TargetSelector.GetUtilityScore(requestedWorkType, searchResult);
    }

    public bool TryGetLastRejectedWorkCandidate(out WorkTargetCandidate candidate)
    {
        candidate = TargetSelector.LastRejectedCandidate;
        return candidate.Building != null
            && !candidate.IsValid
            && candidate.FailureKind != AIActionFailureKind.None;
    }

    public void StartWorking(
        FacilityWorkType requestedWorkType = FacilityWorkType.None,
        BuildableObject preferredTarget = null)
    {
        WorkerMove?.CancelActiveMovement();

        if (CanExecuteSuppressCommand(requestedWorkType))
        {
            StartCoroutine(CommandHandler.SuppressPriorityTarget());
            return;
        }

        if (isWorking || activeWorkRoutine != null || activeWorkCheckRoutine != null)
        {
            StopAssignedWork(null);
        }

        bool assigned = preferredTarget != null
            ? TryAssignWorkTarget(preferredTarget, requestedWorkType)
            : TryAssignWork(requestedWorkType);
        if (!assigned || actor == null || actor.Brain == null)
        {
            if (actor != null && actor.Brain != null)
            {
                actor.AddActivity(CharacterActivityEvent.Work(
                    requestedWorkType,
                    CharacterActivityOutcomes.Failed,
                    "작업 실패: 작업장 없음",
                    preferredTarget,
                    reasonCode: "no-workplace",
                    bubbleEligible: true));
                actor.Brain.isBestActionEnd = true;
            }
            return;
        }

        int runId = BeginWorkRun();
        activeWorkRoutine = StartCoroutine(TaskExecutor.Work(runId));
    }

    public bool TryAssignWorkTarget(
        BuildableObject target,
        FacilityWorkType requestedWorkType,
        GridPathSearchResult searchResult = null)
    {
        bool forced = requestedWorkType != FacilityWorkType.None;
        if (TargetSelector.TryEvaluateWorkTarget(target, searchResult, requestedWorkType, forced, out WorkTargetCandidate candidate))
        {
            AssignWork(target, candidate.WorkType);
            return true;
        }

        return false;
    }

    public bool TrySetPriorityWorkTarget(BuildableObject building, out string errorMessage)
    {
        return CommandHandler.TrySetPriorityWorkTarget(building, out errorMessage);
    }

    public bool TrySetPriorityWorkTarget(
        BuildableObject building,
        FacilityWorkType preferredWorkType,
        GridPathSearchResult searchResult,
        out string errorMessage)
    {
        return CommandHandler.TrySetPriorityWorkTarget(building, preferredWorkType, searchResult, out errorMessage);
    }

    public bool TrySetPrioritySuppressTarget(
        CharacterActor target,
        GridPathSearchResult searchResult,
        out string errorMessage)
    {
        return CommandHandler.TrySetPrioritySuppressTarget(target, searchResult, out errorMessage);
    }

    public bool TryGetPrioritySuppressDestination(GridPathSearchResult searchResult, out BuildableObject destination)
    {
        return CommandHandler.TryGetPrioritySuppressDestination(searchResult, out destination);
    }

    public void ClearPriorityWorkTarget()
    {
        CommandHandler.ClearPriorityWorkTarget();
    }

    public void SetWorkPriority(FacilityWorkType workType, WorkPriorityLevel priority)
    {
        SetWorkPriority(workType, priority, null);
    }

    public void SetWorkPriority(
        FacilityWorkType workType,
        WorkPriorityLevel priority,
        GridPathSearchResult searchResult)
    {
        workPriorities ??= WorkPriorityProfile.CreateDefault();
        WorkPriorityLevel previousPriority = workPriorities.GetPriority(workType);
        workPriorities.SetPriority(workType, priority);

        bool currentWorkDisabled = assignedWorkType != FacilityWorkType.None
            && !workPriorities.IsEnabled(assignedWorkType);
        if (currentWorkDisabled)
        {
            StopAssignedWork($"{WorkTaskCatalog.GetDisplayName(assignedWorkType)} 우선순위 꺼짐");
        }
        else if (!isWorking)
        {
            AssignWork(null, FacilityWorkType.None);
            MarkFacilityDynamicStateDirty();
            actor?.Brain?.RequestImmediateReplan(clearFailures: true);
        }
        else
        {
            MarkFacilityDynamicStateDirty();
            AIBrain brain = actor?.Brain;
            brain?.InvalidateQueuedActionForNextDecision();
            if (IsPriorityRaised(previousPriority, priority)
                && ShouldReplanForRaisedPriority(workType, brain, searchResult))
            {
                string reason = $"{WorkTaskCatalog.GetDisplayName(workType)} 우선순위 상향";
                if (!brain.StopCurrentActionForReplan(reason))
                {
                    StopAssignedWork(reason, false);
                }

                brain.RequestImmediateReplan(clearFailures: true);
            }
        }

        actor?.AddActivity(CharacterActivityEvent.Work(
            workType,
            CharacterActivityOutcomes.Changed,
            $"{WorkTaskCatalog.GetDisplayName(workType)} 우선순위: {priority.ToDisplayText()}",
            reasonCode: $"priority:{(int)priority}",
            value: (int)priority));
    }

    private bool ShouldReplanForRaisedPriority(
        FacilityWorkType workType,
        AIBrain brain,
        GridPathSearchResult searchResult)
    {
        if (brain == null
            || actor == null
            || workType == FacilityWorkType.None
            || workType == assignedWorkType)
        {
            return false;
        }

        WorkPriorityLevel requestedPriority = workPriorities.GetPriority(workType);
        WorkPriorityLevel currentPriority = workPriorities.GetPriority(assignedWorkType);
        if (requestedPriority == WorkPriorityLevel.Off
            || (currentPriority != WorkPriorityLevel.Off && requestedPriority > currentPriority))
        {
            return false;
        }

        searchResult ??= brain.GetPathSearch(actor);
        if (!CanStartWorkAction(workType, searchResult)
            || !TryGetBestWorkCandidate(workType, searchResult, out WorkTargetCandidate requestedCandidate)
            || !TryGetBestWorkCandidate(FacilityWorkType.None, searchResult, out WorkTargetCandidate bestCandidate))
        {
            return false;
        }

        return bestCandidate.WorkType == workType
            && bestCandidate.Building == requestedCandidate.Building;
    }

    private static bool IsPriorityRaised(
        WorkPriorityLevel previousPriority,
        WorkPriorityLevel currentPriority)
    {
        return currentPriority != WorkPriorityLevel.Off
            && (previousPriority == WorkPriorityLevel.Off || currentPriority < previousPriority);
    }

    public bool ShouldUseRestProtection()
    {
        return DutyController.ShouldUseRestProtection();
    }

    public bool CanStartWorkAction()
    {
        return DutyController.CanStartWorkAction();
    }

    public bool CanStartWorkAction(FacilityWorkType requestedWorkType, GridPathSearchResult searchResult)
    {
        return CanStartWorkAction()
            || TargetSelector.HasUrgentAvailableWork(searchResult, requestedWorkType);
    }

    public bool CanContinueCurrentWork(out string stopReason)
    {
        return DutyController.CanContinueAssignedWork(out stopReason);
    }

    public bool ShouldInterruptCurrentWork(out string interruptReason)
    {
        return DutyController.ShouldInterruptCurrentWork(out interruptReason);
    }

    public bool ShouldThrottleRoutineWork(FacilityWorkType workType)
    {
        return workType == FacilityWorkType.Operate
            && Time.time < routineOperateCooldownUntil
            && !HasUrgentPriorityTarget()
            && PriorityWorkTarget == null
            && !HasPrioritySuppressTarget;
    }

    public void BeginRoutineWorkCooldown(FacilityWorkType workType)
    {
        if (workType != FacilityWorkType.Operate || routineOperateCooldownSeconds <= 0f)
        {
            return;
        }

        routineOperateCooldownUntil = Time.time + routineOperateCooldownSeconds;
        if (!isWorking)
        {
            actor?.Brain?.RequestImmediateReplan(clearFailures: true);
        }
    }

    public bool ShouldTakeOffDuty()
    {
        return DutyController.ShouldTakeOffDuty();
    }

    public bool ShouldReturnToWork()
    {
        return DutyController.ShouldReturnToWork();
    }

    public void BeginOffDuty(string reason)
    {
        DutyController.BeginOffDuty(reason);
    }

    public void PrepareForExpedition()
    {
        DutyController.PrepareForExpedition();
    }

    public void SetDutyState(DutyState nextState)
    {
        DutyController.SetDutyState(nextState);
    }

    public void RecoverOffDuty(
        float sleep,
        float mood,
        float fun = 0f,
        float hunger = 0f,
        float excretion = 0f,
        float hygiene = 0f)
    {
        DutyController.RecoverOffDuty(sleep, mood, fun, hunger, excretion, hygiene);
    }

    public void ApplyWorkFatigueTick()
    {
        DutyController.ApplyWorkFatigueTick();
    }

    public IEnumerator CheckActionWork()
    {
        return DutyController.CheckActionWork(activeWorkRunId);
    }

    public void CheckSchedule(Schedule schedule)
    {
        if (isWorking && schedule != Schedule.WORK)
        {
            StopAssignedWork("스케줄 변경");
        }
    }

    internal void AssignWork(BuildableObject building, FacilityWorkType workType)
    {
        assignedShop = building;
        assignedWorkType = workType;
    }

    internal void ReleaseAssignedWorkTarget()
    {
        StopAssignedWork(null);
    }

    internal void StopAssignedWork(string reason)
    {
        StopAssignedWork(reason, true);
    }

    internal void StopAssignedWorkFromAi(string reason)
    {
        StopAssignedWork(reason, false);
    }

    private void StopAssignedWork(string reason, bool requestImmediateReplan)
    {
        FacilityWorkType stoppedWorkType = assignedWorkType;
        BuildableObject stoppedTarget = assignedShop;
        InvalidateActiveWorkRun();
        WorkerMove?.CancelActiveMovement();

        if (assignedShop is IWorkableFacility facility)
        {
            facility.DeallocateWorker(actor);
        }

        AIAction currentAction = actor != null && actor.Brain != null
            ? actor.Brain.bestAction
            : null;
        currentAction?.ReleaseReservation(actor);

        AssignWork(null, FacilityWorkType.None);
        isWorking = false;
        MarkFacilityDynamicStateDirty();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            actor?.AddActivity(CharacterActivityEvent.Work(
                stoppedWorkType,
                CharacterActivityOutcomes.Cancelled,
                $"작업 종료: {reason}",
                stoppedTarget,
                reasonCode: reason));
        }

        if (actor != null && actor.Brain != null)
        {
            actor.Brain.bestAction = null;
            actor.Brain.isBestActionEnd = true;
            if (requestImmediateReplan)
            {
                actor.Brain.RequestImmediateReplan(clearFailures: true);
            }
        }
    }

    internal bool IsActiveWorkRun(int runId)
    {
        return runId == activeWorkRunId;
    }

    internal bool CanContinueWorkRun(int runId)
    {
        return isWorking && IsActiveWorkRun(runId);
    }

    internal Coroutine StartCheckActionWork(int runId)
    {
        if (!IsActiveWorkRun(runId))
        {
            return null;
        }

        StopActiveWorkCheckRoutine();
        activeWorkCheckRoutine = StartCoroutine(DutyController.CheckActionWork(runId));
        return activeWorkCheckRoutine;
    }

    internal void ClearActiveWorkRoutine(int runId)
    {
        if (IsActiveWorkRun(runId))
        {
            activeWorkRoutine = null;
        }
    }

    internal void ClearActiveWorkCheckRoutine(int runId)
    {
        if (IsActiveWorkRun(runId))
        {
            activeWorkCheckRoutine = null;
        }
    }

    internal bool HasUrgentPriorityTarget()
    {
        return CommandHandler.HasUrgentPriorityTarget();
    }

    internal void MarkFacilityDynamicStateDirty()
    {
        FacilityCandidateCacheService.MarkDynamicStateDirty();
    }

    private bool CanExecuteSuppressCommand(FacilityWorkType requestedWorkType)
    {
        return HasPrioritySuppressTarget
            && (requestedWorkType == FacilityWorkType.None || requestedWorkType == FacilityWorkType.Guard);
    }

    private void OnDisable()
    {
        UnbindScheduleEvents();
    }

    private void OnEnable()
    {
        TryBindScheduleEvents();
    }

    private void TryBindScheduleEvents()
    {
        CacheLocalReferences();

        if (abilityCache == null
            || !abilityCache.TryGetAbility(out AbilitySchedule nextSchedule)
            || nextSchedule == null
            || nextSchedule.nowSheduleData == null)
        {
            return;
        }

        if (schedule == nextSchedule && isScheduleBound)
        {
            return;
        }

        UnbindScheduleEvents();
        schedule = nextSchedule;
        schedule.nowSheduleData.OnValueChange += CheckSchedule;
        isScheduleBound = true;
    }

    private void UnbindScheduleEvents()
    {
        if (schedule != null && schedule.nowSheduleData != null && isScheduleBound)
        {
            schedule.nowSheduleData.OnValueChange -= CheckSchedule;
        }

        isScheduleBound = false;
    }

    private void EnsureWorkModules()
    {
        if (targetSelector != null
            && taskExecutor != null
            && dutyController != null
            && commandHandler != null)
        {
            return;
        }

        targetSelector ??= new WorkTargetSelector(this);
        taskExecutor ??= new WorkTaskExecutor(this, targetSelector);
        dutyController ??= new WorkDutyController(this);
        commandHandler ??= new WorkCommandHandler(this, targetSelector);
    }

    private int BeginWorkRun()
    {
        StopActiveWorkRoutines();
        activeWorkRunId++;
        return activeWorkRunId;
    }

    private void InvalidateActiveWorkRun()
    {
        activeWorkRunId++;
        StopActiveWorkRoutines();
    }

    private void StopActiveWorkRoutines()
    {
        StopActiveWorkRoutine();
        StopActiveWorkCheckRoutine();
    }

    private void StopActiveWorkRoutine()
    {
        if (activeWorkRoutine == null)
        {
            return;
        }

        StopCoroutine(activeWorkRoutine);
        activeWorkRoutine = null;
    }

    private void StopActiveWorkCheckRoutine()
    {
        if (activeWorkCheckRoutine == null)
        {
            return;
        }

        StopCoroutine(activeWorkCheckRoutine);
        activeWorkCheckRoutine = null;
    }

    private IEnumerator ExecuteRestockWork()
    {
        return TaskExecutor.ExecuteRestockWork();
    }

    private IEnumerator ExecuteRepairWork()
    {
        return TaskExecutor.ExecuteRepairWork();
    }

    private IEnumerator ExecuteResearchWork()
    {
        return TaskExecutor.ExecuteResearchWork();
    }

    private IEnumerator SuppressPriorityTarget()
    {
        return CommandHandler.SuppressPriorityTarget();
    }

    private bool TryEvaluateWorkTarget(
        BuildableObject building,
        GridPathSearchResult searchResult,
        FacilityWorkType forcedWorkType,
        bool ignorePriority,
        out WorkTargetCandidate bestCandidate)
    {
        return TargetSelector.TryEvaluateWorkTarget(
            building,
            searchResult,
            forcedWorkType,
            ignorePriority,
            out bestCandidate);
    }

    private sealed class AbilityWorkNoopFloatingIconFeedbackService : IFloatingIconFeedbackService
    {
        public bool Show(Component target, Sprite sprite, float maxWorldSize) => false;
    }

}
