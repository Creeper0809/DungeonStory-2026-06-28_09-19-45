using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class WorkTaskExecutor
{
    private readonly AbilityWork work;
    private readonly WorkTargetSelector targetSelector;

    public WorkTaskExecutor(AbilityWork work, WorkTargetSelector targetSelector)
    {
        this.work = work;
        this.targetSelector = targetSelector;
    }

    public IEnumerator Work(int runId)
    {
        Character character = work.WorkerCharacter;
        AIAction currentAction = character != null && character.ai != null
            ? character.ai.bestAction
            : null;
        work.EnsureWorkReferences();
        AbilityMove move = work.WorkerMove;
        Grid grid = WorkGridUtility.ResolveActiveGrid(work, null);
        if (move == null || grid == null)
        {
            WorkDebugLog.LogEnd(character, "이동 정보 없음");
            character?.AddLog("작업 실패: 이동 정보 없음");
            work.isWorking = false;
            EndAiAction(character, currentAction);
            work.ClearActiveWorkRoutine(runId);
            yield break;
        }

        work.isWorking = true;
        yield return move.MoveByCurrentBestActionPath();
        if (ShouldAbortWorkRun(runId, character) || !work.isWorking)
        {
            AbortWorkRun(runId, character, currentAction);
            yield break;
        }

        BuildableObject assignedTarget = work.assignedShop;
        if (HasReachedAssignedWorkTarget(character, grid)
            && assignedTarget is IWorkableFacility facility)
        {
            yield return facility.AllocateWorker(character);
            if (ShouldAbortWorkRun(runId, character)
                || !work.isWorking
                || work.assignedShop != assignedTarget)
            {
                facility.DeallocateWorker(character);
                AbortWorkRun(runId, character, currentAction);
                yield break;
            }

            currentAction?.ReleaseReservation(character);
            WorkDebugLog.LogProgress(character);
            bool completedImmediately = false;
            FacilityWorkType workType = work.AssignedWorkType;
            if (workType == FacilityWorkType.Restock)
            {
                yield return ExecuteRestockWork();
                if (ShouldAbortWorkRun(runId, character))
                {
                    facility.DeallocateWorker(character);
                    AbortWorkRun(runId, character, currentAction);
                    yield break;
                }

                completedImmediately = true;
            }
            else if (workType == FacilityWorkType.Repair)
            {
                yield return ExecuteRepairWork();
                if (ShouldAbortWorkRun(runId, character))
                {
                    facility.DeallocateWorker(character);
                    AbortWorkRun(runId, character, currentAction);
                    yield break;
                }

                completedImmediately = true;
            }
            else if (workType == FacilityWorkType.Research)
            {
                yield return ExecuteResearchWork();
                if (ShouldAbortWorkRun(runId, character))
                {
                    facility.DeallocateWorker(character);
                    AbortWorkRun(runId, character, currentAction);
                    yield break;
                }

                completedImmediately = true;
            }

            if (!completedImmediately)
            {
                character?.AddLog($"작업 시작: {WorkTaskCatalog.GetDisplayName(work.AssignedWorkType)}");
                work.StartCheckActionWork(runId);
                yield return new WaitUntil(() => !work.IsActiveWorkRun(runId) || work.isWorking == false);
                if (!work.IsActiveWorkRun(runId))
                {
                    AbortWorkRun(runId, character, currentAction);
                    yield break;
                }

                work.ClearActiveWorkCheckRoutine(runId);
            }
            else
            {
                work.isWorking = false;
                WorkDebugLog.LogEnd(character, "즉시 작업 완료");
            }

            bool wasPriorityTarget = work.assignedShop == work.PriorityWorkTarget;
            facility.DeallocateWorker(character);
            currentAction?.ReleaseReservation(character);
            work.AssignWork(null, FacilityWorkType.None);
            if (wasPriorityTarget)
            {
                work.ClearPriorityWorkTarget();
            }
        }
        else
        {
            work.isWorking = false;
            WorkDebugLog.LogEnd(character, "작업장 도달 실패");
            character?.AddLog("작업 실패: 작업장 도달 실패");
            currentAction?.ReleaseReservation(character);
        }

        if (character != null && character.ai != null)
        {
            character.ai.isBestActionEnd = true;
        }

        work.ClearActiveWorkRoutine(runId);
    }

    private bool HasReachedAssignedWorkTarget(Character character, Grid grid)
    {
        if (character == null || grid == null || work.assignedShop == null)
        {
            return false;
        }

        GridCell currentCell = grid.GetGridCell(grid.GetXY(work.transform.position));
        return currentCell != null && currentCell.ContainsOccupant(work.assignedShop);
    }

    public IEnumerator ExecuteRestockWork()
    {
        Character character = work.WorkerCharacter;
        if (work.assignedShop is not Shop shop)
        {
            character?.AddLog("보충 실패: 대상이 상점이 아님");
            yield break;
        }

        IEnumerable<IWarehouseFacility> warehouses = targetSelector.FindReachableWarehouses();
        int amount = shop.RestockFrom(warehouses, shop.MissingStock, out string resultMessage);
        character?.AddLog(amount > 0
            ? $"보충 완료: {work.assignedShop.name} {resultMessage}"
            : $"보충 실패: {resultMessage}");

        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator ExecuteRepairWork()
    {
        Character character = work.WorkerCharacter;
        if (work.assignedShop == null)
        {
            character?.AddLog("수리 실패: 대상 없음");
            yield break;
        }

        float workSpeed = character != null
            ? Mathf.Max(0.1f, character.GetWorkSpeedMultiplier(FacilityWorkType.Repair))
            : 1f;
        yield return new WaitForSeconds(0.8f / workSpeed);
        work.assignedShop.SetDamaged(false);
        character?.AddLog($"수리 완료: {work.assignedShop.name}");
    }

    public IEnumerator ExecuteResearchWork()
    {
        Character character = work.WorkerCharacter;
        BlueprintResearchRuntime researchRuntime = BlueprintResearchRuntime.Instance;
        if (researchRuntime == null)
        {
            character?.AddLog("연구 실패: 연구 시스템 없음");
            yield return new WaitForSeconds(0.2f);
            yield break;
        }

        const float workSeconds = 1f;
        BlueprintResearchWorkResult result = researchRuntime.ApplyResearchWork(
            character,
            work.assignedShop,
            workSeconds);
        if (!result.Success)
        {
            character?.AddLog($"연구 실패: {result.Message}");
            yield return new WaitForSeconds(0.2f);
            yield break;
        }

        string blueprintName = result.Blueprint != null ? result.Blueprint.DisplayName : "설계도";
        character?.AddLog(result.Completed
            ? $"연구 완료: {blueprintName}"
            : $"연구 진행: {blueprintName} {Mathf.RoundToInt(result.ProgressRatio * 100f)}%");

        yield return new WaitForSeconds(workSeconds);
    }

    private static void EndAiAction(Character character, AIAction currentAction)
    {
        currentAction?.ReleaseReservation(character);
        if (character != null && character.ai != null)
        {
            character.ai.isBestActionEnd = true;
        }
    }

    private bool ShouldAbortWorkRun(int runId, Character character)
    {
        return !work.IsActiveWorkRun(runId)
            || character == null
            || character.ai == null
            || character.ai.isBestActionEnd;
    }

    private void AbortWorkRun(int runId, Character character, AIAction currentAction)
    {
        currentAction?.ReleaseReservation(character);
        work.isWorking = false;
        if (work.IsActiveWorkRun(runId))
        {
            work.AssignWork(null, FacilityWorkType.None);
        }

        work.ClearActiveWorkRoutine(runId);
    }
}
