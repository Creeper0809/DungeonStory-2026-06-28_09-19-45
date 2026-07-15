using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class WorkTaskExecutor
{
    private const float RestockPickupWaitSeconds = 0.35f;

    private readonly AbilityWork work;
    private readonly WorkTargetSelector targetSelector;

    public WorkTaskExecutor(AbilityWork work, WorkTargetSelector targetSelector)
    {
        this.work = work;
        this.targetSelector = targetSelector;
    }

    public IEnumerator Work(int runId)
    {
        CharacterActor actor = work.WorkerActor;
        AIAction currentAction = actor != null && actor.Brain != null
            ? actor.Brain.bestAction
            : null;

        work.EnsureWorkReferences();
        AbilityMove move = work.WorkerMove;
        Grid grid = work.WorkGridResolver.ResolveActiveGrid(work, null);
        if (move == null || grid == null)
        {
            WorkDebugLog.LogEnd(actor, "이동 정보 없음");
            actor?.AddLog("작업 실패: 이동 정보 없음");
            work.isWorking = false;
            EndAiAction(actor, currentAction);
            work.ClearActiveWorkRoutine(runId);
            yield break;
        }

        work.isWorking = true;
        if (work.AssignedWorkType == FacilityWorkType.Restock)
        {
            yield return ExecuteRestockHaulWork(runId, currentAction, move, grid);
            FinishWorkRun(actor, currentAction);
            work.ClearActiveWorkRoutine(runId);
            yield break;
        }

        yield return move.MoveByCurrentBestActionPath();
        if (ShouldAbortWorkRun(runId, actor) || !work.isWorking)
        {
            AbortWorkRun(runId, actor, currentAction);
            yield break;
        }

        BuildableObject assignedTarget = work.assignedShop;
        if (HasReachedAssignedWorkTarget(actor, grid)
            && assignedTarget is IWorkableFacility facility)
        {
            yield return facility.AllocateWorker(actor);
            if (ShouldAbortWorkRun(runId, actor)
                || !work.isWorking
                || work.assignedShop != assignedTarget)
            {
                facility.DeallocateWorker(actor);
                AbortWorkRun(runId, actor, currentAction);
                yield break;
            }

            currentAction?.ReleaseReservation(actor);
            WorkDebugLog.LogStarted(actor);
            bool completedImmediately = false;
            bool completedSuccessfully = true;
            FacilityWorkType workType = work.AssignedWorkType;
            if (workType == FacilityWorkType.Repair)
            {
                yield return ExecuteRepairWork();
                if (ShouldAbortWorkRun(runId, actor))
                {
                    facility.DeallocateWorker(actor);
                    AbortWorkRun(runId, actor, currentAction);
                    yield break;
                }

                completedImmediately = true;
            }
            else if (workType == FacilityWorkType.Research)
            {
                yield return ExecuteResearchWork((success) => completedSuccessfully = success);
                if (ShouldAbortWorkRun(runId, actor))
                {
                    facility.DeallocateWorker(actor);
                    AbortWorkRun(runId, actor, currentAction);
                    yield break;
                }

                completedImmediately = true;
            }

            if (!completedImmediately)
            {
                work.StartCheckActionWork(runId);
                yield return new WaitUntil(() => !work.IsActiveWorkRun(runId) || !work.isWorking);
                if (!work.IsActiveWorkRun(runId))
                {
                    AbortWorkRun(runId, actor, currentAction);
                    yield break;
                }

                work.ClearActiveWorkCheckRoutine(runId);
            }
            else
            {
                work.isWorking = false;
                WorkDebugLog.LogEnd(actor, "즉시 작업 완료");
            }

            if (completedSuccessfully)
            {
                ModularFacilityRuntimeEffects.ApplyWorkCompleted(
                    actor,
                    assignedTarget,
                    workType);
                RoomEnvironmentExperienceEvent.Trigger(
                    actor,
                    assignedTarget,
                    RoomExperienceActivity.Work,
                    workType);
            }

            bool wasPriorityTarget = work.assignedShop == work.PriorityWorkTarget;
            facility.DeallocateWorker(actor);
            currentAction?.ReleaseReservation(actor);
            work.AssignWork(null, FacilityWorkType.None);
            if (wasPriorityTarget)
            {
                work.ClearPriorityWorkTarget();
            }
        }
        else
        {
            work.isWorking = false;
            WorkDebugLog.LogEnd(actor, "작업 도달 실패");
            actor?.AddLog("작업 실패: 작업 도달 실패");
            currentAction?.ReleaseReservation(actor);
        }

        EndAiAction(actor, currentAction);
        work.ClearActiveWorkRoutine(runId);
    }

    private bool HasReachedAssignedWorkTarget(CharacterActor actor, Grid grid)
    {
        if (actor == null || grid == null || work.assignedShop == null)
        {
            return false;
        }

        GridCell currentCell = grid.GetGridCell(grid.GetXY(work.transform.position));
        return currentCell != null && currentCell.ContainsOccupant(work.assignedShop);
    }

    private IEnumerator ExecuteRestockHaulWork(
        int runId,
        AIAction currentAction,
        AbilityMove move,
        Grid grid)
    {
        CharacterActor actor = work.WorkerActor;
        if (work.assignedShop is not Shop shop)
        {
            actor?.AddLog("보충 실패: 대상이 상점이 아님");
            work.isWorking = false;
            yield break;
        }

        if (!TryCreateRestockHaulPlan(
            actor,
            grid,
            shop,
            out BuildableObject warehouseBuilding,
            out IWarehouseFacility warehouse,
            out SaleItem saleItem,
            out int loadAmount,
            out Queue<GridMoveStep> pathToWarehouse,
            out string failureReason))
        {
            actor?.AddLog($"보충 실패: {failureReason}");
            work.isWorking = false;
            yield break;
        }

        actor?.AddLog($"보충 이동: {warehouseBuilding.name} -> {shop.name}");
        yield return move.MoveByPath(pathToWarehouse, currentAction);
        if (ShouldAbortWorkRun(runId, actor))
        {
            AbortWorkRun(runId, actor, currentAction);
            yield break;
        }

        int carriedAmount = 0;
        for (int i = 0; i < loadAmount; i++)
        {
            Vector3 pickupPosition = GetWarehousePickupWorldPosition(grid, warehouseBuilding, i, loadAmount);
            yield return move.Move2PosBySpeed(pickupPosition, 0.8f, currentAction);
            if (ShouldAbortWorkRun(runId, actor))
            {
                ReturnCarriedStock(warehouse, saleItem, carriedAmount);
                AbortWorkRun(runId, actor, currentAction);
                yield break;
            }

            int withdrawn = warehouse.Inventory.Withdraw(saleItem.category, 1);
            if (withdrawn <= 0)
            {
                break;
            }

            carriedAmount += withdrawn;
            actor?.AddLog($"보충 적재: {saleItem.itemName} {carriedAmount}/{loadAmount}");
            work.FloatingIconFeedbackService.Show(actor, saleItem.itemSprite, FloatingIconFeedbackDefaults.DefaultMaxWorldSize);
            yield return new WaitForSeconds(RestockPickupWaitSeconds);
            if (ShouldAbortWorkRun(runId, actor))
            {
                ReturnCarriedStock(warehouse, saleItem, carriedAmount);
                AbortWorkRun(runId, actor, currentAction);
                yield break;
            }
        }

        if (carriedAmount <= 0)
        {
            actor?.AddLog("보충 실패: 창고 재고 부족");
            work.isWorking = false;
            yield break;
        }

        if (!TryGetPathToBuilding(grid, actor, shop, out Queue<GridMoveStep> pathToShop))
        {
            ReturnCarriedStock(warehouse, saleItem, carriedAmount);
            actor?.AddLog("보충 실패: 상점 경로 없음");
            work.isWorking = false;
            yield break;
        }

        yield return move.MoveByPath(pathToShop, currentAction);
        if (ShouldAbortWorkRun(runId, actor))
        {
            ReturnCarriedStock(warehouse, saleItem, carriedAmount);
            AbortWorkRun(runId, actor, currentAction);
            yield break;
        }

        int restocked = shop.ReceiveRestock(saleItem, carriedAmount, carriedAmount, out string resultMessage);
        int leftover = carriedAmount - restocked;
        if (leftover > 0)
        {
            ReturnCarriedStock(warehouse, saleItem, leftover);
        }

        actor?.AddLog(restocked > 0
            ? $"보충 완료: {shop.name} {resultMessage}"
            : $"보충 실패: {resultMessage}");

        yield return new WaitForSeconds(0.5f);
        work.isWorking = false;
        WorkDebugLog.LogEnd(actor, "보충 완료");
    }

    private bool TryCreateRestockHaulPlan(
        CharacterActor actor,
        Grid grid,
        Shop shop,
        out BuildableObject warehouseBuilding,
        out IWarehouseFacility warehouse,
        out SaleItem saleItem,
        out int loadAmount,
        out Queue<GridMoveStep> pathToWarehouse,
        out string failureReason)
    {
        warehouseBuilding = null;
        warehouse = null;
        saleItem = null;
        loadAmount = 0;
        pathToWarehouse = null;
        failureReason = string.Empty;

        if (actor == null || grid == null || shop == null)
        {
            failureReason = "보충 경로 정보 없음";
            return false;
        }

        Vector2Int startPos = work.WorkGridResolver.GetGridPosition(grid, actor);
        GridPathSearchResult searchResult = grid.SearchPath(startPos);
        List<IWarehouseFacility> reachableWarehouses = searchResult
            .GetAllReachableBuilding()
            .OfType<IWarehouseFacility>()
            .Where((candidate) => candidate.HasWarehouseInventory && candidate.Inventory != null)
            .ToList();

        if (!shop.TryFindRestockSource(
            reachableWarehouses,
            shop.MissingStock,
            out warehouse,
            out saleItem,
            out loadAmount,
            out failureReason))
        {
            return false;
        }

        warehouseBuilding = warehouse as BuildableObject;
        if (warehouseBuilding == null)
        {
            failureReason = "창고 건물 정보 없음";
            return false;
        }

        pathToWarehouse = searchResult.GetMovePathTo(warehouseBuilding);
        if (pathToWarehouse == null)
        {
            failureReason = "창고 경로 없음";
            return false;
        }

        return true;
    }

    private static Vector3 GetWarehousePickupWorldPosition(
        Grid grid,
        BuildableObject warehouseBuilding,
        int pickupIndex,
        int pickupCount)
    {
        if (grid == null
            || warehouseBuilding == null
            || warehouseBuilding.buildPoses == null
            || warehouseBuilding.buildPoses.Count == 0)
        {
            return warehouseBuilding != null ? warehouseBuilding.transform.position : Vector3.zero;
        }

        int minX = warehouseBuilding.buildPoses.Min((pos) => pos.x);
        int maxX = warehouseBuilding.buildPoses.Max((pos) => pos.x);
        int slotCount = Mathf.Clamp(pickupCount, 1, Mathf.Max(1, maxX - minX + 1));
        int slot = pickupIndex % slotCount;
        if ((pickupIndex / slotCount) % 2 == 1)
        {
            slot = slotCount - 1 - slot;
        }

        Vector2 minWorld = grid.GetWorldPos(new Vector2Int(minX, warehouseBuilding.centerPos.y));
        Vector2 maxWorld = grid.GetWorldPos(new Vector2Int(maxX, warehouseBuilding.centerPos.y));
        float minWorldX = Mathf.Min(minWorld.x, maxWorld.x) + 0.15f;
        float maxWorldX = Mathf.Max(minWorld.x, maxWorld.x) - 0.15f;
        float t = slotCount <= 1 ? 0.5f : (slot + 0.5f) / slotCount;
        float x = minWorldX <= maxWorldX
            ? Mathf.Lerp(minWorldX, maxWorldX, t)
            : (minWorld.x + maxWorld.x) * 0.5f;

        return new Vector3(x, minWorld.y, warehouseBuilding.transform.position.z);
    }

    private bool TryGetPathToBuilding(
        Grid grid,
        CharacterActor actor,
        BuildableObject target,
        out Queue<GridMoveStep> path)
    {
        path = null;
        if (grid == null || actor == null || target == null)
        {
            return false;
        }

        Vector2Int startPos = work.WorkGridResolver.GetGridPosition(grid, actor);
        path = grid.SearchPath(startPos).GetMovePathTo(target);
        return path != null;
    }

    private void ReturnCarriedStock(
        IWarehouseFacility warehouse,
        SaleItem saleItem,
        int amount)
    {
        if (warehouse == null
            || !warehouse.HasWarehouseInventory
            || warehouse.Inventory == null
            || saleItem == null
            || amount <= 0)
        {
            return;
        }

        warehouse.Inventory.Deposit(saleItem.category, amount);
        work.MarkFacilityDynamicStateDirty();
    }

    public IEnumerator ExecuteRestockWork()
    {
        CharacterActor actor = work.WorkerActor;
        if (work.assignedShop is not Shop shop)
        {
            actor?.AddLog("보충 실패: 대상이 상점이 아님");
            yield break;
        }

        IEnumerable<IWarehouseFacility> warehouses = targetSelector.FindReachableWarehouses();
        int amount = shop.RestockFrom(warehouses, shop.MissingStock, out string resultMessage);
        actor?.AddLog(amount > 0
            ? $"보충 완료: {work.assignedShop.name} {resultMessage}"
            : $"보충 실패: {resultMessage}");

        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator ExecuteRepairWork()
    {
        CharacterActor actor = work.WorkerActor;
        if (work.assignedShop == null)
        {
            actor?.AddLog("수리 실패: 대상 없음");
            yield break;
        }

        float workSpeed = actor != null
            ? Mathf.Max(0.1f, actor.GetWorkSpeedMultiplier(FacilityWorkType.Repair))
            : 1f;
        yield return new WaitForSeconds(0.8f / workSpeed);
        work.assignedShop.SetDamaged(false);
        actor?.AddLog($"수리 완료: {work.assignedShop.name}");
    }

    public IEnumerator ExecuteResearchWork()
    {
        yield return ExecuteResearchWork(null);
    }

    private IEnumerator ExecuteResearchWork(Action<bool> onCompleted)
    {
        CharacterActor actor = work.WorkerActor;
        IBlueprintResearchWorkService researchWorkService = work.BlueprintResearchWorkService;

        const float workSeconds = 1f;
        BlueprintResearchWorkResult result = researchWorkService.ApplyResearchWork(
            actor,
            work.assignedShop,
            workSeconds);
        if (!result.Success)
        {
            onCompleted?.Invoke(false);
            actor?.AddLog($"연구 실패: {result.Message}");
            yield return new WaitForSeconds(0.2f);
            yield break;
        }

        string blueprintName = result.Blueprint != null ? result.Blueprint.DisplayName : "설계도";
        actor?.AddLog(result.Completed
            ? $"연구 완료: {blueprintName}"
            : $"연구 진행: {blueprintName} {Mathf.RoundToInt(result.ProgressRatio * 100f)}%");

        onCompleted?.Invoke(true);
        yield return new WaitForSeconds(workSeconds);
    }

    private static void EndAiAction(CharacterActor actor, AIAction currentAction)
    {
        currentAction?.ReleaseReservation(actor);
        if (actor != null && actor.Brain != null)
        {
            actor.Brain.isBestActionEnd = true;
        }
    }

    private void FinishWorkRun(CharacterActor actor, AIAction currentAction)
    {
        bool wasPriorityTarget = work.assignedShop == work.PriorityWorkTarget;
        currentAction?.ReleaseReservation(actor);
        work.AssignWork(null, FacilityWorkType.None);
        if (wasPriorityTarget)
        {
            work.ClearPriorityWorkTarget();
        }

        if (actor != null && actor.Brain != null)
        {
            actor.Brain.isBestActionEnd = true;
        }
    }

    private bool ShouldAbortWorkRun(int runId, CharacterActor actor)
    {
        return !work.IsActiveWorkRun(runId)
            || actor == null
            || actor.Brain == null
            || actor.Brain.isBestActionEnd;
    }

    private void AbortWorkRun(int runId, CharacterActor actor, AIAction currentAction)
    {
        currentAction?.ReleaseReservation(actor);
        work.isWorking = false;
        if (work.IsActiveWorkRun(runId))
        {
            work.AssignWork(null, FacilityWorkType.None);
        }

        work.ClearActiveWorkRoutine(runId);
    }
}
