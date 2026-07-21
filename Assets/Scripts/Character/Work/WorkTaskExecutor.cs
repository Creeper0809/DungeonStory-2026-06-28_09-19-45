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
            actor?.AddActivity(CharacterActivityEvent.Work(
                work.AssignedWorkType,
                CharacterActivityOutcomes.Failed,
                "작업 실패: 이동 정보 없음",
                work.assignedShop,
                reasonCode: "missing-movement",
                bubbleEligible: true));
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
            FacilityWorkType workType = work.AssignedWorkType;
            CharacterSkillRuntimeEffects.BeginWork(
                actor,
                assignedTarget,
                workType,
                $"work:{runId}:{assignedTarget.GetInstanceID()}:started");
            WorkDebugLog.LogStarted(actor);
            bool completedImmediately = false;
            bool completedSuccessfully = true;
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
            else if (workType == FacilityWorkType.Craft)
            {
                completedSuccessfully = assignedTarget.HasPendingEquipmentCraftWork();
                if (completedSuccessfully)
                {
                    yield return ExecuteCraftWork();
                }

                completedImmediately = true;
            }
            else if (workType == FacilityWorkType.Clean)
            {
                float cleaningSpeed = CharacterSkillRuntimeEffects.GetCleaningSpeedMultiplier(actor);
                yield return new WaitForSeconds(1f / Mathf.Max(0.1f, cleaningSpeed));
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
                completedSuccessfully = work.LastWorkRunCompleted;
            }
            else
            {
                work.isWorking = false;
                WorkDebugLog.LogEnd(actor, "즉시 작업 완료");
            }

            if (completedSuccessfully)
            {
                actor.Progression?.AddExperience(5);
                CharacterSkillRuntimeEffects.TriggerWorkCompleted(
                    actor,
                    assignedTarget,
                    workType,
                    $"work:{runId}:{assignedTarget.GetInstanceID()}:completed");
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

            CharacterSkillRuntimeEffects.EndWork(actor);
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
            actor?.AddActivity(CharacterActivityEvent.Work(
                work.AssignedWorkType,
                CharacterActivityOutcomes.Failed,
                "작업 실패: 작업 도달 실패",
                assignedTarget,
                reasonCode: "target-unreachable",
                bubbleEligible: true));
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
        BuildableObject restockTarget = work.assignedShop;
        CharacterSkillRuntimeEffects.BeginWork(
            actor,
            restockTarget,
            FacilityWorkType.Restock,
            $"work:{runId}:{(restockTarget != null ? restockTarget.GetInstanceID() : 0)}:restock-started");
        float durationMultiplier = work.GetWorkEnvironmentDurationMultiplier(FacilityWorkType.Restock)
            / Mathf.Max(0.1f, CharacterSkillRuntimeEffects.GetWorkSpeedMultiplier(actor));
        if (restockTarget is not IRestockableFacility restockable)
        {
            actor?.AddActivity(CharacterActivityEvent.Work(
                FacilityWorkType.Restock,
                CharacterActivityOutcomes.Failed,
                "보충 실패: 재고를 받을 수 없는 시설",
                restockTarget,
                reasonCode: "target-not-restockable",
                bubbleEligible: true));
            work.isWorking = false;
            yield break;
        }

        if (!TryCreateRestockHaulPlan(
            actor,
            grid,
            restockTarget,
            restockable,
            out BuildableObject warehouseBuilding,
            out IWarehouseFacility warehouse,
            out SaleItem saleItem,
            out int loadAmount,
            out Queue<GridMoveStep> pathToWarehouse,
            out string failureReason))
        {
            actor?.AddActivity(CharacterActivityEvent.Work(
                FacilityWorkType.Restock,
                CharacterActivityOutcomes.Failed,
                $"보충 실패: {failureReason}",
                restockTarget,
                reasonCode: failureReason,
                bubbleEligible: true));
            work.isWorking = false;
            yield break;
        }

        actor?.AddActivity(CharacterActivityEvent.Work(
            FacilityWorkType.Restock,
            CharacterActivityOutcomes.Progress,
            $"보충 이동: {warehouseBuilding.name} -> {restockTarget.name}",
            restockTarget,
            reasonCode: "moving-to-stock"));
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
            actor?.AddActivity(CharacterActivityEvent.Work(
                FacilityWorkType.Restock,
                CharacterActivityOutcomes.Progress,
                $"보충 적재: {saleItem.itemName} {carriedAmount}/{loadAmount}",
                warehouseBuilding,
                reasonCode: "loading-stock",
                quantity: carriedAmount));
            work.FloatingIconFeedbackService.Show(actor, saleItem.itemSprite, FloatingIconFeedbackDefaults.DefaultMaxWorldSize);
            yield return new WaitForSeconds(RestockPickupWaitSeconds * durationMultiplier);
            if (ShouldAbortWorkRun(runId, actor))
            {
                ReturnCarriedStock(warehouse, saleItem, carriedAmount);
                AbortWorkRun(runId, actor, currentAction);
                yield break;
            }
        }

        if (carriedAmount <= 0)
        {
            actor?.AddActivity(CharacterActivityEvent.Work(
                FacilityWorkType.Restock,
                CharacterActivityOutcomes.Failed,
                "보충 실패: 창고 재고 부족",
                warehouseBuilding,
                reasonCode: "warehouse-stock-shortage",
                bubbleEligible: true));
            work.isWorking = false;
            yield break;
        }

        if (!TryGetPathToBuilding(grid, actor, restockTarget, out Queue<GridMoveStep> pathToShop))
        {
            ReturnCarriedStock(warehouse, saleItem, carriedAmount);
            actor?.AddActivity(CharacterActivityEvent.Work(
                FacilityWorkType.Restock,
                CharacterActivityOutcomes.Blocked,
                "보충 실패: 상점 경로 없음",
                restockTarget,
                reasonCode: "shop-path-missing",
                bubbleEligible: true));
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

        int restocked = restockable.ReceiveRestock(
            saleItem,
            carriedAmount,
            carriedAmount,
            out string resultMessage);
        int leftover = carriedAmount - restocked;
        if (leftover > 0)
        {
            ReturnCarriedStock(warehouse, saleItem, leftover);
        }

        actor?.AddActivity(CharacterActivityEvent.Work(
            FacilityWorkType.Restock,
            restocked > 0 ? CharacterActivityOutcomes.Completed : CharacterActivityOutcomes.Failed,
            restocked > 0
                ? $"보충 완료: {restockTarget.name} {resultMessage}"
                : $"보충 실패: {resultMessage}",
            restockTarget,
            reasonCode: resultMessage,
            quantity: restocked,
            bubbleEligible: restocked <= 0));
        if (restocked > 0)
        {
            CharacterSkillRuntimeEffects.TriggerWorkCompleted(
                actor,
                restockTarget,
                FacilityWorkType.Restock,
                $"work:{runId}:{restockTarget.GetInstanceID()}:restock-completed");
        }

        yield return new WaitForSeconds(0.5f * durationMultiplier);
        work.isWorking = false;
        WorkDebugLog.LogEnd(actor, "보충 완료");
    }

    private bool TryCreateRestockHaulPlan(
        CharacterActor actor,
        Grid grid,
        BuildableObject restockTarget,
        IRestockableFacility restockable,
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

        if (actor == null || grid == null || restockTarget == null || restockable == null)
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

        if (!restockable.TryFindRestockSource(
            reachableWarehouses,
            restockable.MissingStock,
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
        BuildableObject restockTarget = work.assignedShop;
        float durationMultiplier = work.GetWorkEnvironmentDurationMultiplier(FacilityWorkType.Restock);
        if (restockTarget is not IRestockableFacility restockable)
        {
            actor?.AddActivity(CharacterActivityEvent.Work(
                FacilityWorkType.Restock,
                CharacterActivityOutcomes.Failed,
                "보충 실패: 재고를 받을 수 없는 시설",
                restockTarget,
                reasonCode: "target-not-restockable",
                bubbleEligible: true));
            yield break;
        }

        IEnumerable<IWarehouseFacility> warehouses = targetSelector.FindReachableWarehouses();
        int amount = restockable.RestockFrom(
            warehouses,
            restockable.MissingStock,
            out string resultMessage);
        actor?.AddActivity(CharacterActivityEvent.Work(
            FacilityWorkType.Restock,
            amount > 0 ? CharacterActivityOutcomes.Completed : CharacterActivityOutcomes.Failed,
            amount > 0
                ? $"보충 완료: {restockTarget.name} {resultMessage}"
                : $"보충 실패: {resultMessage}",
            restockTarget,
            reasonCode: resultMessage,
            quantity: amount,
            bubbleEligible: amount <= 0));

        yield return new WaitForSeconds(0.5f * durationMultiplier);
    }

    public IEnumerator ExecuteRepairWork()
    {
        CharacterActor actor = work.WorkerActor;
        if (work.assignedShop == null)
        {
            actor?.AddActivity(CharacterActivityEvent.Work(
                FacilityWorkType.Repair,
                CharacterActivityOutcomes.Failed,
                "수리 실패: 대상 없음",
                reasonCode: "missing-target",
                bubbleEligible: true));
            yield break;
        }

        float workSpeed = actor != null
            ? Mathf.Max(0.1f, actor.GetWorkSpeedMultiplier(FacilityWorkType.Repair))
            : 1f;
        workSpeed *= CharacterSkillRuntimeEffects.GetRepairSpeedMultiplier(actor);
        yield return new WaitForSeconds(0.8f / workSpeed);
        work.assignedShop.SetDamaged(false);
        actor?.AddActivity(CharacterActivityEvent.Work(
            FacilityWorkType.Repair,
            CharacterActivityOutcomes.Completed,
            $"수리 완료: {work.assignedShop.name}",
            work.assignedShop));
    }

    public IEnumerator ExecuteResearchWork()
    {
        yield return ExecuteResearchWork(null);
    }

    private IEnumerator ExecuteCraftWork()
    {
        float durationMultiplier = work.GetWorkEnvironmentDurationMultiplier(FacilityWorkType.Craft);
        yield return new WaitForSeconds(1f * durationMultiplier);
    }

    private IEnumerator ExecuteResearchWork(Action<bool> onCompleted)
    {
        CharacterActor actor = work.WorkerActor;
        IBlueprintResearchWorkService researchWorkService = work.BlueprintResearchWorkService;

        const float workSeconds = 1f;
        float durationMultiplier = work.GetWorkEnvironmentDurationMultiplier(FacilityWorkType.Research);
        BlueprintResearchWorkResult result = researchWorkService.ApplyResearchWork(
            actor,
            work.assignedShop,
            workSeconds);
        if (!result.Success)
        {
            onCompleted?.Invoke(false);
            actor?.AddActivity(CharacterActivityEvent.Work(
                FacilityWorkType.Research,
                CharacterActivityOutcomes.Failed,
                $"연구 실패: {result.Message}",
                work.assignedShop,
                reasonCode: result.Message,
                bubbleEligible: true));
            yield return new WaitForSeconds(0.2f);
            yield break;
        }

        string blueprintName = result.Blueprint != null ? result.Blueprint.DisplayName : "설계도";
        actor?.AddActivity(CharacterActivityEvent.Work(
            FacilityWorkType.Research,
            result.Completed ? CharacterActivityOutcomes.Completed : CharacterActivityOutcomes.Progress,
            result.Completed
                ? $"연구 완료: {blueprintName}"
                : $"연구 진행: {blueprintName} {Mathf.RoundToInt(result.ProgressRatio * 100f)}%",
            work.assignedShop,
            reasonCode: result.Completed ? "blueprint-completed" : "research-progress",
            value: result.ProgressRatio));

        onCompleted?.Invoke(true);
        yield return new WaitForSeconds(workSeconds * durationMultiplier);
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
        CharacterSkillRuntimeEffects.EndWork(actor);
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
        CharacterSkillRuntimeEffects.EndWork(actor);
        currentAction?.ReleaseReservation(actor);
        work.isWorking = false;
        if (work.IsActiveWorkRun(runId))
        {
            work.AssignWork(null, FacilityWorkType.None);
        }

        work.ClearActiveWorkRoutine(runId);
    }
}
