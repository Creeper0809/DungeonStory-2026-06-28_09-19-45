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
        Character character = work.WorkerCharacter;
        if (character == null
            || character.stats == null
            || data == null)
        {
            return;
        }

        EnsureStatAtLeast(Character.Condition.SLEEP, 85f);
        EnsureStatAtLeast(Character.Condition.MOOD, 75f);
        EnsureStatAtLeast(Character.Condition.FUN, 70f);
        EnsureStatAtLeast(Character.Condition.HUNGER, 80f);
    }

    public bool ShouldUseRestProtection()
    {
        WorkPriorityProfile priorities = work.WorkPriorities;
        if (priorities == null || !priorities.IsEnabled(FacilityWorkType.Rest))
        {
            restProtectionActive = false;
            return false;
        }

        Character character = work.WorkerCharacter;
        if (character == null || character.stats == null)
        {
            restProtectionActive = false;
            return false;
        }

        if (!character.stats.TryGetValue(Character.Condition.SLEEP, out float sleep))
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
        Character character = work.WorkerCharacter;
        if (character != null && character.IsOnExpedition)
        {
            return false;
        }

        if (work.HasPrioritySuppressTarget)
        {
            if (IsOffDuty)
            {
                SetDutyState(AbilityWork.DutyState.OnDuty);
                character?.AddLog("비번 중 제압 명령 대응");
            }

            return true;
        }

        if (work.HasUrgentPriorityTarget())
        {
            if (IsOffDuty)
            {
                SetDutyState(AbilityWork.DutyState.OnDuty);
                character?.AddLog("비번 중 우선 작업 대응");
            }

            return true;
        }

        if (StaffDiscontentRuntime.Instance != null
            && StaffDiscontentRuntime.Instance.ShouldBlockWork(character, out string discontentReason))
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
                character?.AddLog("근무 복귀");
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
        Character character = work.WorkerCharacter;
        if (character == null
            || character.IsOwner
            || character.stats == null)
        {
            return false;
        }

        float sleep = GetStat(Character.Condition.SLEEP, 100f);
        float mood = GetStat(Character.Condition.MOOD, 100f);
        return sleep <= work.OffDutySleepThreshold
            || mood <= work.OffDutyMoodThreshold;
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

        float sleep = GetStat(Character.Condition.SLEEP, 0f);
        float mood = GetStat(Character.Condition.MOOD, 0f);
        return sleep >= work.ReturnToWorkSleepThreshold
            && mood >= work.ReturnToWorkMoodThreshold;
    }

    public void BeginOffDuty(string reason)
    {
        Character character = work.WorkerCharacter;
        if (character == null || character.IsOwner)
        {
            return;
        }

        bool wasOffDuty = IsOffDuty;
        work.ReleaseAssignedWorkTarget();
        SetDutyState(AbilityWork.DutyState.OffDuty);
        if (!wasOffDuty)
        {
            character.AddLog(string.IsNullOrWhiteSpace(reason)
                ? "비번 시작"
                : $"비번 시작: {reason}");
        }

        if (!wasOffDuty && character.TryGetAbility(out AbilityShopping shopping))
        {
            shopping.BeginOffDutyVisitCycle();
        }
    }

    public void PrepareForExpedition()
    {
        work.ReleaseAssignedWorkTarget();
        work.ClearPriorityWorkTarget();
        SetDutyState(AbilityWork.DutyState.OnDuty);
        work.WorkerCharacter?.AddLog("원정 준비: 작업 해제");
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
        FacilityCandidateCache.MarkDynamicStateDirty();
        if (!work.isWorking)
        {
            work.WorkerCharacter?.ai?.RequestImmediateReplan(
                clearFailures: nextState == AbilityWork.DutyState.OnDuty);
        }
    }

    public void RecoverOffDuty(float sleep, float mood, float fun = 0f, float hunger = 0f)
    {
        Character character = work.WorkerCharacter;
        if (character == null) return;

        if (sleep != 0f) character.ChangesStat(Character.Condition.SLEEP, sleep);
        if (mood != 0f) character.ChangesStat(Character.Condition.MOOD, mood);
        if (fun != 0f) character.ChangesStat(Character.Condition.FUN, fun);
        if (hunger != 0f) character.ChangesStat(Character.Condition.HUNGER, hunger);
    }

    public void ApplyWorkFatigueTick()
    {
        Character character = work.WorkerCharacter;
        if (character == null || character.stats == null) return;

        character.ChangesStat(Character.Condition.SLEEP, -work.SleepDrainPerWorkTick);
        character.ChangesStat(Character.Condition.MOOD, -work.MoodDrainPerWorkTick);
    }

    public IEnumerator CheckActionWork(int runId)
    {
        Character character = work.WorkerCharacter;
        string endReason = string.Empty;
        float startedAt = Time.time;
        while (work.CanContinueWorkRun(runId) && character != null && character.ai != null)
        {
            WorkDebugLog.LogProgress(character);
            ApplyWorkFatigueTick();
            if (!CanContinueAssignedWork(out string stopReason))
            {
                endReason = stopReason;
                WorkDebugLog.LogEnd(character, stopReason);
                character.AddLog($"작업 종료: {stopReason}");
                break;
            }

            if (ShouldInterruptCurrentWork(out string interruptReason))
            {
                endReason = interruptReason;
                WorkDebugLog.LogEnd(character, interruptReason);
                character.AddLog($"작업 종료: {interruptReason}");
                break;
            }

            if (ShouldEndRoutineWorkShift(startedAt, out string routineShiftReason))
            {
                endReason = routineShiftReason;
                WorkDebugLog.LogEnd(character, routineShiftReason);
                character.AddLog($"근무 교대: {routineShiftReason}");
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
            WorkDebugLog.LogEnd(character, externalStopReason);
            character?.AddLog($"작업 종료: {externalStopReason}");
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

    private void EnsureStatAtLeast(Character.Condition condition, float value)
    {
        Character character = work.WorkerCharacter;
        if (character == null || character.stats == null) return;

        if (!character.stats.TryGetValue(condition, out float current) || current < value)
        {
            character.stats[condition] = value;
        }
    }

    private float GetStat(Character.Condition condition, float defaultValue)
    {
        Character character = work.WorkerCharacter;
        if (character == null || character.stats == null)
        {
            return defaultValue;
        }

        return character.stats.TryGetValue(condition, out float value)
            ? value
            : defaultValue;
    }
}
