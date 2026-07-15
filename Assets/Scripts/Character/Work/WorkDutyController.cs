using System.Collections;
using UnityEngine;

public sealed class WorkDutyController
{
    private readonly AbilityWork work;
    private AbilityWork.DutyState dutyState = AbilityWork.DutyState.OnDuty;
    private float offDutyStartedAt = float.NegativeInfinity;
    private bool restProtectionActive;
    private float restProtectionStartedAt = float.NegativeInfinity;

    public WorkDutyController(AbilityWork work)
    {
        this.work = work;
    }

    public AbilityWork.DutyState CurrentState => dutyState;
    public bool IsOffDuty => dutyState == AbilityWork.DutyState.OffDuty;

    public void InitializeWorkerCondition(CharacterSO data)
    {
        if (GetWorkerStats() == null || data == null)
        {
            return;
        }

        EnsureStatAtLeast(CharacterCondition.SLEEP, 85f);
        EnsureStatAtLeast(CharacterCondition.MOOD, 75f);
        EnsureStatAtLeast(CharacterCondition.FUN, 70f);
        EnsureStatAtLeast(CharacterCondition.HUNGER, 80f);
        EnsureStatAtLeast(CharacterCondition.EXCRETION, 85f);
        EnsureStatAtLeast(CharacterCondition.HYGIENE, 80f);
    }

    public bool ShouldUseRestProtection()
    {
        WorkPriorityProfile priorities = work.WorkPriorities;
        if (priorities == null || !priorities.IsEnabled(FacilityWorkType.Rest))
        {
            restProtectionActive = false;
            return false;
        }

        CharacterStats stats = GetWorkerStats();
        if (stats == null)
        {
            restProtectionActive = false;
            return false;
        }

        if (!stats.Stats.TryGetValue(CharacterCondition.SLEEP, out float sleep))
        {
            restProtectionActive = false;
            return false;
        }

        if (sleep <= work.RestProtectionSleepThreshold)
        {
            if (!restProtectionActive)
            {
                restProtectionStartedAt = Time.time;
            }

            restProtectionActive = true;
        }

        if (!restProtectionActive)
        {
            return false;
        }

        bool sleptEnough = sleep >= work.RestProtectionResumeSleepThreshold;
        bool waitedEnough = Time.time - restProtectionStartedAt >= work.MinimumRestProtectionSeconds;
        if (sleptEnough && waitedEnough)
        {
            restProtectionActive = false;
            restProtectionStartedAt = float.NegativeInfinity;
            return false;
        }

        return true;
    }

    public bool CanStartWorkAction()
    {
        CharacterActor actor = work.WorkerActor;
        if (actor != null && actor.IsOnExpedition)
        {
            return false;
        }

        if (work.HasPrioritySuppressTarget)
        {
            if (IsOffDuty)
            {
                SetDutyState(AbilityWork.DutyState.OnDuty);
                actor?.AddLog("비번 중 제압 명령 대응");
            }

            return true;
        }

        if (work.HasUrgentPriorityTarget())
        {
            if (IsOffDuty)
            {
                SetDutyState(AbilityWork.DutyState.OnDuty);
                actor?.AddLog("비번 중 우선 작업 대응");
            }

            return true;
        }

        if (work.StaffDiscontentRuntimeService.ShouldBlockWork(actor, out string discontentReason))
        {
            BeginOffDuty(string.IsNullOrWhiteSpace(discontentReason)
                ? "직원 불만"
                : discontentReason);
            return false;
        }

        if (IsOffDuty)
        {
            if (ShouldReturnToWork())
            {
                SetDutyState(AbilityWork.DutyState.OnDuty);
                actor?.AddLog("근무 복귀");
                return true;
            }

            return false;
        }

        if (ShouldTakeOffDuty())
        {
            BeginOffDuty("컨디션 저하");
            return false;
        }

        if (ShouldUseRestProtection())
        {
            return false;
        }

        return true;
    }

    public bool ShouldTakeOffDuty()
    {
        CharacterActor actor = work.WorkerActor;
        if (actor == null || actor.IsOwner || GetWorkerStats() == null)
        {
            return false;
        }

        float sleep = GetStat(CharacterCondition.SLEEP, 100f);
        float mood = GetStat(CharacterCondition.MOOD, 100f);
        float excretion = GetStat(CharacterCondition.EXCRETION, 100f);
        float hygiene = GetStat(CharacterCondition.HYGIENE, 100f);
        return sleep <= work.OffDutySleepThreshold
            || mood <= work.OffDutyMoodThreshold
            || excretion <= 25f
            || hygiene <= 20f;
    }

    public bool ShouldReturnToWork()
    {
        if (!IsOffDuty)
        {
            return false;
        }

        if (Time.time - offDutyStartedAt < work.MinimumOffDutySeconds)
        {
            return false;
        }

        float sleep = GetStat(CharacterCondition.SLEEP, 0f);
        float mood = GetStat(CharacterCondition.MOOD, 0f);
        float excretion = GetStat(CharacterCondition.EXCRETION, 0f);
        float hygiene = GetStat(CharacterCondition.HYGIENE, 0f);
        return sleep >= work.ReturnToWorkSleepThreshold
            && mood >= work.ReturnToWorkMoodThreshold
            && excretion >= 55f
            && hygiene >= 45f;
    }

    public void BeginOffDuty(string reason)
    {
        CharacterActor actor = work.WorkerActor;
        if (actor == null || actor.IsOwner)
        {
            return;
        }

        bool wasOffDuty = IsOffDuty;
        work.ReleaseAssignedWorkTarget();
        SetDutyState(AbilityWork.DutyState.OffDuty);
        if (!wasOffDuty)
        {
            actor.AddLog(string.IsNullOrWhiteSpace(reason)
                ? "비번 시작"
                : $"비번 시작: {reason}");
        }

        if (!wasOffDuty && actor.TryGetAbility(out AbilityShopping shopping))
        {
            shopping.BeginOffDutyVisitCycle();
        }
    }

    public void PrepareForExpedition()
    {
        work.ReleaseAssignedWorkTarget();
        work.ClearPriorityWorkTarget();
        SetDutyState(AbilityWork.DutyState.OnDuty);
        work.WorkerActor?.AddLog("원정 준비: 작업 해제");
    }

    public void SetDutyState(AbilityWork.DutyState nextState)
    {
        if (dutyState == nextState)
        {
            return;
        }

        if (dutyState != AbilityWork.DutyState.OffDuty
            && nextState == AbilityWork.DutyState.OffDuty)
        {
            offDutyStartedAt = Time.time;
        }

        dutyState = nextState;
        work.MarkFacilityDynamicStateDirty();
        if (!work.isWorking)
        {
            work.WorkerActor?.Brain?.RequestImmediateReplan(
                clearFailures: nextState == AbilityWork.DutyState.OnDuty);
        }
    }

    public void RecoverOffDuty(
        float sleep,
        float mood,
        float fun = 0f,
        float hunger = 0f,
        float excretion = 0f,
        float hygiene = 0f)
    {
        CharacterStats stats = GetWorkerStats();
        if (stats == null) return;

        if (sleep != 0f) stats.ChangesStat(CharacterCondition.SLEEP, sleep);
        if (mood != 0f)
        {
            stats.ApplyMoodFactor(
                "rest:off-duty",
                mood > 0f ? "잠깐 숨을 돌림" : "제대로 쉬지 못함",
                mood,
                90f,
                3);
        }
        if (fun != 0f) stats.ChangesStat(CharacterCondition.FUN, fun);
        if (hunger != 0f) stats.ChangesStat(CharacterCondition.HUNGER, hunger);
        if (excretion != 0f) stats.ChangesStat(CharacterCondition.EXCRETION, excretion);
        if (hygiene != 0f) stats.ChangesStat(CharacterCondition.HYGIENE, hygiene);
    }

    public void ApplyWorkFatigueTick()
    {
        CharacterStats stats = GetWorkerStats();
        if (stats == null) return;

        stats.ChangesStat(CharacterCondition.SLEEP, -work.SleepDrainPerWorkTick);
        stats.ApplyMoodFactor(
            "work:fatigue",
            "계속된 작업",
            -work.MoodDrainPerWorkTick,
            90f,
            8);
        stats.ChangesStat(CharacterCondition.EXCRETION, -0.35f);
        stats.ChangesStat(CharacterCondition.HYGIENE, -0.2f);
    }

    public IEnumerator CheckActionWork(int runId)
    {
        CharacterActor actor = work.WorkerActor;
        string endReason = string.Empty;
        float startedAt = Time.time;
        while (work.CanContinueWorkRun(runId) && actor != null && actor.Brain != null)
        {
            ApplyWorkFatigueTick();
            if (!CanContinueAssignedWork(out string stopReason))
            {
                endReason = stopReason;
                WorkDebugLog.LogEnd(actor, stopReason);
                break;
            }

            if (ShouldInterruptCurrentWork(out string interruptReason))
            {
                endReason = interruptReason;
                WorkDebugLog.LogEnd(actor, interruptReason);
                break;
            }

            if (ShouldEndRoutineWorkShift(startedAt, out string routineShiftReason))
            {
                endReason = routineShiftReason;
                WorkDebugLog.LogEnd(actor, $"근무 교대 · {routineShiftReason}");
                actor.AddLog($"근무 교대: {routineShiftReason}");
                work.BeginRoutineWorkCooldown(work.AssignedWorkType);
                break;
            }

            yield return new WaitForSeconds(1f);
        }

        if (!work.IsActiveWorkRun(runId))
        {
            work.ClearActiveWorkCheckRoutine(runId);
            yield break;
        }

        work.isWorking = false;
        if (string.IsNullOrWhiteSpace(endReason))
        {
            const string externalStopReason = "외부 작업 상태 해제";
            WorkDebugLog.LogEnd(actor, externalStopReason);
        }

        work.ClearActiveWorkCheckRoutine(runId);
    }

    public bool ShouldInterruptCurrentWork(out string interruptReason)
    {
        interruptReason = string.Empty;
        if (work.HasPrioritySuppressTarget)
        {
            interruptReason = "우선 제압 명령";
            return true;
        }

        BuildableObject priorityTarget = work.PriorityWorkTarget;
        if (priorityTarget != null && priorityTarget != work.assignedShop)
        {
            interruptReason = "우선 작업 명령";
            return true;
        }

        if (work.HasUrgentPriorityTarget())
        {
            return false;
        }

        if (ShouldUseRestProtection())
        {
            interruptReason = "휴식 필요";
            return true;
        }

        if (ShouldTakeOffDuty())
        {
            BeginOffDuty("근무 피로 누적");
            interruptReason = "근무 피로 누적";
            return true;
        }

        return false;
    }

    private bool ShouldEndRoutineWorkShift(float startedAt, out string reason)
    {
        reason = string.Empty;
        if (work.AssignedWorkType != FacilityWorkType.Operate)
        {
            return false;
        }

        if (work.HasPrioritySuppressTarget || work.HasUrgentPriorityTarget())
        {
            return false;
        }

        if (Time.time - startedAt < work.RoutineOperateShiftSeconds)
        {
            return false;
        }

        reason = "운영 교대";
        return true;
    }

    public bool CanContinueAssignedWork(out string stopReason)
    {
        stopReason = string.Empty;

        if (work.HasPrioritySuppressTarget)
        {
            return true;
        }

        BuildableObject priorityTarget = work.PriorityWorkTarget;
        if (priorityTarget != null && priorityTarget != work.assignedShop)
        {
            return true;
        }

        BuildableObject target = work.assignedShop;
        if (target == null)
        {
            stopReason = "작업장 없음";
            return false;
        }

        if (target.isDestroy)
        {
            stopReason = "작업장 파괴됨";
            return false;
        }

        FacilityWorkType workType = work.AssignedWorkType;
        if (workType == FacilityWorkType.None)
        {
            stopReason = "작업 종류 없음";
            return false;
        }

        if (!work.WorkPriorities.IsEnabled(workType))
        {
            stopReason = $"{WorkTaskCatalog.GetDisplayName(workType)} 우선순위 꺼짐";
            return false;
        }

        if (!target.CanAssignWork(workType, out stopReason))
        {
            return false;
        }

        return true;
    }

    private void EnsureStatAtLeast(CharacterCondition condition, float value)
    {
        CharacterStats stats = GetWorkerStats();
        if (stats == null) return;

        if (!stats.Stats.TryGetValue(condition, out float current) || current < value)
        {
            stats.Stats[condition] = value;
        }
    }

    private float GetStat(CharacterCondition condition, float defaultValue)
    {
        CharacterStats stats = GetWorkerStats();
        if (stats == null)
        {
            return defaultValue;
        }

        return stats.Stats.TryGetValue(condition, out float value)
            ? value
            : defaultValue;
    }

    private CharacterStats GetWorkerStats()
    {
        return work.WorkerActor != null ? work.WorkerActor.Stats : null;
    }
}
