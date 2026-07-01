using System.Collections;
using UnityEngine;

public class AbilityWork : CharacterAbility
{
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
    private bool isScheduleBound;
    private float routineOperateCooldownUntil;
    private Coroutine activeWorkRoutine;
    private Coroutine activeWorkCheckRoutine;
    private int activeWorkRunId;

    public BuildableObject PriorityWorkTarget => CommandHandler.PriorityWorkTarget;
    public Character PrioritySuppressTarget => CommandHandler.PrioritySuppressTarget;
    public bool HasPrioritySuppressTarget => CommandHandler.HasPrioritySuppressTarget;
    public FacilityWorkType PriorityWorkType => CommandHandler.PriorityWorkType;
    public WorkPriorityProfile WorkPriorities => workPriorities ??= WorkPriorityProfile.CreateDefault();
    public FacilityWorkType AssignedWorkType => assignedWorkType;
    public float RestRecoveryOnWait => restRecoveryOnWait;
    public DutyState CurrentDutyState => DutyController.CurrentState;
    public bool IsOffDuty => DutyController.IsOffDuty;
    public WorkTargetCandidate LastRejectedWorkCandidate => TargetSelector.LastRejectedCandidate;

    public Character WorkerCharacter => character;
    public AbilityMove WorkerMove => move;
    public Grid CachedGrid => grid;

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
        if (!assigned || character == null || character.ai == null)
        {
            if (character != null && character.ai != null)
            {
                character.AddLog("작업 실패: 작업장 없음");
                character.ai.isBestActionEnd = true;
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
        Character target,
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
        workPriorities ??= WorkPriorityProfile.CreateDefault();
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
            FacilityCandidateCache.MarkDynamicStateDirty();
            character?.ai?.RequestImmediateReplan(clearFailures: true);
        }
        else
        {
            FacilityCandidateCache.MarkDynamicStateDirty();
        }

        character?.AddLog($"{WorkTaskCatalog.GetDisplayName(workType)} 우선순위: {priority.ToDisplayText()}");
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
            character?.ai?.RequestImmediateReplan(clearFailures: true);
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

    public void RecoverOffDuty(float sleep, float mood, float fun = 0f, float hunger = 0f)
    {
        DutyController.RecoverOffDuty(sleep, mood, fun, hunger);
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
        InvalidateActiveWorkRun();

        if (assignedShop is IWorkableFacility facility)
        {
            facility.DeallocateWorker(character);
        }

        AIAction currentAction = character != null && character.ai != null
            ? character.ai.bestAction
            : null;
        currentAction?.ReleaseReservation(character);

        AssignWork(null, FacilityWorkType.None);
        isWorking = false;
        FacilityCandidateCache.MarkDynamicStateDirty();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            character?.AddLog($"작업 종료: {reason}");
        }

        if (character != null && character.ai != null)
        {
            character.ai.bestAction = null;
            character.ai.isBestActionEnd = true;
            character.ai.RequestImmediateReplan(clearFailures: true);
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
        if (character == null)
        {
            character = GetComponent<Character>();
        }
        if (character == null)
        {
            return;
        }

        if (!character.TryGetAbility(out AbilitySchedule nextSchedule)
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
}
