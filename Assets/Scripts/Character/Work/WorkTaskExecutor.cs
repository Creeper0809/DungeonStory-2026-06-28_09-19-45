using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class WorkTaskExecutor
{
    private const float RestockPickupWaitSeconds = 0.35f;
    private const float MinimumNaturalExteriorWorkSeconds = 3.2f;

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
            bool completionEffectsAlreadyApplied = false;
            if (WorkOrderRuntime.Active != null
                && WorkOrderRuntime.Active.TryGetOrderFor(assignedTarget, workType, out _))
            {
                yield return ExecuteWorkOrderRoutine(
                    runId,
                    actor,
                    assignedTarget,
                    workType,
                    (success, appliedEffects) =>
                    {
                        completedSuccessfully = success;
                        completionEffectsAlreadyApplied = appliedEffects;
                    });
                if (ShouldAbortWorkRun(runId, actor))
                {
                    facility.DeallocateWorker(actor);
                    AbortWorkRun(runId, actor, currentAction);
                    yield break;
                }

                completedImmediately = true;
            }
            else if (TryGetExteriorWorkSeconds(assignedTarget, actor, workType, out float exteriorWorkSeconds))
            {
                yield return ExecuteWorkAmountLoop(
                    runId,
                    actor,
                    assignedTarget,
                    workType,
                    exteriorWorkSeconds,
                    WorkTaskCatalog.GetDisplayName(workType));
                if (ShouldAbortWorkRun(runId, actor))
                {
                    facility.DeallocateWorker(actor);
                    AbortWorkRun(runId, actor, currentAction);
                    yield break;
                }

                completedImmediately = true;
            }
            else if (workType == FacilityWorkType.Repair)
            {
                yield return ExecuteRepairWork(runId);
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
                yield return ExecuteResearchWork(runId, (success) => completedSuccessfully = success);
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
                    yield return ExecuteCraftWork(runId, (success, appliedEffects) =>
                    {
                        completedSuccessfully = success;
                        completionEffectsAlreadyApplied = appliedEffects;
                    });
                }

                completedImmediately = true;
            }
            else if (workType == FacilityWorkType.Butcher)
            {
                completedSuccessfully = WildlifeRuntime.Active != null
                    && WildlifeRuntime.Active.HasButcherWorkAvailable(assignedTarget);
                if (completedSuccessfully)
                {
                    yield return ExecuteButcherWork(runId);
                }

                completedImmediately = true;
            }
            else if (SurvivalFacilityUtility.IsSurvivalWork(workType))
            {
                completedSuccessfully = SurvivalFoodRuntime.Active != null
                    && SurvivalFoodRuntime.Active.HasSurvivalWorkAvailable(assignedTarget, workType);
                if (completedSuccessfully)
                {
                    yield return ExecuteSurvivalWork(runId, workType);
                }

                completedImmediately = true;
            }
            else if (workType == FacilityWorkType.Clean)
            {
                yield return ExecuteCleanWork(runId);
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
                WorkDebugLog.LogEnd(actor, "작업량 완료");
            }

            if (completedSuccessfully)
            {
                actor.Progression?.AddExperience(5);
                CharacterSkillRuntimeEffects.TriggerWorkCompleted(
                    actor,
                    assignedTarget,
                    workType,
                    $"work:{runId}:{assignedTarget.GetInstanceID()}:completed");
                if (!completionEffectsAlreadyApplied)
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
            }

            actor?.AiMemory?.RecordWork(
                workType,
                assignedTarget,
                completedSuccessfully,
                $"{WorkTaskCatalog.GetDisplayName(workType)} {(completedSuccessfully ? "완료" : "실패")}: {assignedTarget.name}");
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
            actor?.AiMemory?.RecordWork(
                work.AssignedWorkType,
                assignedTarget,
                false,
                $"작업 도달 실패: {(assignedTarget != null ? assignedTarget.name : "대상 없음")}");
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

        actor?.AiMemory?.RecordWork(
            FacilityWorkType.Restock,
            restockTarget,
            restocked > 0,
            restocked > 0
                ? $"보충 완료: {restockTarget.name}"
                : $"보충 실패: {restockTarget.name}");

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
        if (!GridPathSearchBroker.TryGetSearch(grid, startPos, () => true, out GridPathSearchResult searchResult))
        {
            failureReason = "창고 경로 계산 대기";
            return false;
        }

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
        if (!GridPathSearchBroker.TryGetSearch(grid, startPos, () => true, out GridPathSearchResult searchResult))
        {
            return false;
        }

        path = searchResult.GetMovePathTo(target);
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
        actor?.AiMemory?.RecordWork(
            FacilityWorkType.Restock,
            restockTarget,
            amount > 0,
            amount > 0
                ? $"보충 완료: {restockTarget.name}"
                : $"보충 실패: {restockTarget.name}");

        yield return new WaitForSeconds(0.5f * durationMultiplier);
    }

    public IEnumerator ExecuteRepairWork()
    {
        yield return ExecuteRepairWork(0);
    }

    private IEnumerator ExecuteRepairWork(int runId)
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

        if (EquipmentMaintenancePolicyRuntime.Active?.HasRepairWorkFor(work.assignedShop) == true)
        {
            yield return ExecuteEquipmentRepairWork(runId, actor, work.assignedShop);
            yield break;
        }

        float requiredWork = Mathf.Max(0.1f, work.assignedShop.BuildingData.GetRequiredWork(FacilityWorkType.Repair));
        float repairMultiplier = CharacterSkillRuntimeEffects.GetRepairSpeedMultiplier(actor);
        yield return ExecuteWorkAmountLoop(
            runId,
            actor,
            work.assignedShop,
            FacilityWorkType.Repair,
            requiredWork,
            "수리",
            repairMultiplier);
        if (!CanContinueTimedWork(runId, actor) || !work.isWorking)
        {
            yield break;
        }

        work.assignedShop.SetDamaged(false);
        actor?.AddActivity(CharacterActivityEvent.Work(
            FacilityWorkType.Repair,
            CharacterActivityOutcomes.Completed,
            $"수리 완료: {work.assignedShop.name}",
            work.assignedShop));
    }

    private IEnumerator ExecuteEquipmentRepairWork(
        int runId,
        CharacterActor actor,
        BuildableObject building)
    {
        EquipmentMaintenancePolicyRuntime maintenance =
            EquipmentMaintenancePolicyRuntime.Active;
        if (maintenance == null)
        {
            yield break;
        }

        float durationMultiplier =
            work.GetWorkEnvironmentDurationMultiplier(FacilityWorkType.Repair);
        float lastReportAt = -10f;
        while (CanContinueTimedWork(runId, actor)
            && work.isWorking
            && maintenance.HasRepairWorkFor(building))
        {
            float deltaWork = WorkAmountUtility.CalculateWorkPerSecond(
                    actor,
                    building,
                    FacilityWorkType.Repair,
                    durationMultiplier)
                * Time.deltaTime;
            if (!maintenance.TryApplyRepairWork(
                    actor,
                    building,
                    deltaWork,
                    out bool completed,
                    out string message))
            {
                actor?.AddActivity(CharacterActivityEvent.Work(
                    FacilityWorkType.Repair,
                    CharacterActivityOutcomes.Blocked,
                    $"장비 수리 중단: {message}",
                    building,
                    reasonCode: "equipment-repair-blocked",
                    bubbleEligible: true));
                yield break;
            }

            if (Time.time - lastReportAt >= 0.75f)
            {
                lastReportAt = Time.time;
                actor?.Brain?.SetActionPhase(message, building);
            }

            if (completed)
            {
                actor?.AddActivity(CharacterActivityEvent.Work(
                    FacilityWorkType.Repair,
                    CharacterActivityOutcomes.Completed,
                    message,
                    building,
                    reasonCode: "equipment-repair-completed",
                    bubbleEligible: true));
                yield break;
            }

            yield return null;
        }
    }

    public IEnumerator ExecuteResearchWork()
    {
        yield return ExecuteResearchWork(0, null);
    }

    private IEnumerator ExecuteCraftWork()
    {
        yield return ExecuteCraftWork(0, null);
    }

    private IEnumerator ExecuteCraftWork(int runId, Action<bool, bool> onCompleted)
    {
        float requiredWork = GetEquipmentCraftWorkRequired(work.assignedShop);
        yield return ExecuteWorkAmountLoop(
            runId,
            work.WorkerActor,
            work.assignedShop,
            FacilityWorkType.Craft,
            requiredWork,
            "제작");
        if (!CanContinueTimedWork(runId, work.WorkerActor) || !work.isWorking)
        {
            onCompleted?.Invoke(false, false);
            yield break;
        }

        int applied = ModularFacilityRuntimeEffects.ApplyWorkCompleted(
            work.WorkerActor,
            work.assignedShop,
            FacilityWorkType.Craft);
        onCompleted?.Invoke(applied > 0, true);
    }

    private IEnumerator ExecuteButcherWork()
    {
        yield return ExecuteButcherWork(0);
    }

    private IEnumerator ExecuteButcherWork(int runId)
    {
        float requiredWork = GetButcherWorkRequired(work.assignedShop);
        yield return ExecuteWorkAmountLoop(
            runId,
            work.WorkerActor,
            work.assignedShop,
            FacilityWorkType.Butcher,
            requiredWork,
            "도축");
    }

    private IEnumerator ExecuteSurvivalWork(FacilityWorkType workType)
    {
        yield return ExecuteSurvivalWork(0, workType);
    }

    private IEnumerator ExecuteSurvivalWork(int runId, FacilityWorkType workType)
    {
        float requiredWork = GetSurvivalWorkRequired(work.assignedShop, workType);
        yield return ExecuteWorkAmountLoop(
            runId,
            work.WorkerActor,
            work.assignedShop,
            workType,
            requiredWork,
            WorkTaskCatalog.GetDisplayName(workType));
    }

    private IEnumerator ExecuteCleanWork(int runId)
    {
        WorldFilthWorkTarget filthTarget = work.assignedShop as WorldFilthWorkTarget;
        float requiredWork = filthTarget != null
            ? Mathf.Max(0.1f, filthTarget.RequiredCleaningWork)
            : Mathf.Max(0.1f, work.assignedShop.BuildingData.GetRequiredWork(FacilityWorkType.Clean));
        float cleaningMultiplier = CharacterSkillRuntimeEffects.GetCleaningSpeedMultiplier(work.WorkerActor);
        yield return ExecuteWorkAmountLoop(
            runId,
            work.WorkerActor,
            work.assignedShop,
            FacilityWorkType.Clean,
            requiredWork,
            "청소",
            cleaningMultiplier);
        filthTarget?.CompleteCleaning(requiredWork);
    }

    private IEnumerator ExecuteWorkOrderRoutine(
        int runId,
        CharacterActor actor,
        BuildableObject target,
        FacilityWorkType workType,
        Action<bool, bool> onCompleted)
    {
        bool completed = false;
        bool appliedCompletionEffects = false;
        if (target == null || WorkOrderRuntime.Active == null)
        {
            onCompleted?.Invoke(false, false);
            yield break;
        }

        string label = WorkTaskCatalog.GetDisplayName(workType);
        float durationMultiplier = work.GetWorkEnvironmentDurationMultiplier(workType);
        float lastReportTime = -10f;
        while (CanContinueTimedWork(runId, actor)
            && work.isWorking
            && WorkOrderRuntime.Active.TryGetOrderFor(target, workType, out WorkOrderProgressState order)
            && order.Status != WorkOrderStatus.Completed
            && order.Status != WorkOrderStatus.Cancelled)
        {
            if (order.Status == WorkOrderStatus.WaitingForMaterials)
            {
                actor?.AddActivity(CharacterActivityEvent.Work(
                    workType,
                    CharacterActivityOutcomes.Blocked,
                    $"{label} 대기: 재료가 아직 도착하지 않음",
                    target,
                    reasonCode: "waiting-for-materials",
                    value: order.ProgressRatio));
                yield return new WaitForSeconds(0.35f);
                if (!WorkOrderRuntime.Active.RefreshMaterialsReady(target as ConstructionSite))
                {
                    continue;
                }
            }

            float deltaWork = WorkAmountUtility.CalculateWorkPerSecond(
                    actor,
                    target,
                    workType,
                    durationMultiplier)
                * Time.deltaTime;
            if (!WorkOrderRuntime.Active.ApplyWork(
                    actor,
                    target,
                    workType,
                    deltaWork,
                    out completed,
                    out appliedCompletionEffects,
                    out string message))
            {
                actor?.AddActivity(CharacterActivityEvent.Work(
                    workType,
                    CharacterActivityOutcomes.Blocked,
                    $"{label} 중단: {message}",
                    target,
                    reasonCode: "work-order-blocked",
                    bubbleEligible: true));
                onCompleted?.Invoke(false, false);
                yield break;
            }

            if (Time.time - lastReportTime >= 0.75f
                && WorkOrderRuntime.Active.TryGetOrderFor(target, workType, out order))
            {
                lastReportTime = Time.time;
                actor?.Brain?.SetActionPhase($"{label} {Mathf.RoundToInt(order.ProgressRatio * 100f)}%", target);
                actor?.AddActivity(CharacterActivityEvent.Work(
                    workType,
                    CharacterActivityOutcomes.Progress,
                    $"{label} 진행 {Mathf.RoundToInt(order.ProgressRatio * 100f)}%",
                    target,
                    reasonCode: "work-progress",
                    value: order.ProgressRatio));
            }

            if (completed)
            {
                actor?.AddActivity(CharacterActivityEvent.Work(
                    workType,
                    CharacterActivityOutcomes.Completed,
                    $"{label} 완료",
                    target,
                    reasonCode: "work-order-completed",
                    value: 1f));
                onCompleted?.Invoke(true, appliedCompletionEffects);
                yield break;
            }

            yield return null;
        }

        onCompleted?.Invoke(false, appliedCompletionEffects);
    }

    private IEnumerator ExecuteWorkAmountLoop(
        int runId,
        CharacterActor actor,
        BuildableObject target,
        FacilityWorkType workType,
        float requiredWork,
        string label,
        float extraMultiplier = 1f)
    {
        requiredWork = Mathf.Max(0.1f, requiredWork);
        label = string.IsNullOrWhiteSpace(label) ? WorkTaskCatalog.GetDisplayName(workType) : label;
        if (DungeonDebugRuntimeRules.IsEnabled(DungeonDebugCheat.InstantWork))
        {
            actor?.Brain?.SetActionPhase($"{label} 100%", target);
            yield return null;
            yield break;
        }

        float completedWork = 0f;
        float durationMultiplier = work.GetWorkEnvironmentDurationMultiplier(workType);
        float lastReportTime = -10f;
        while (completedWork + 0.001f < requiredWork
            && CanContinueTimedWork(runId, actor)
            && work.isWorking)
        {
            float tickDeltaTime = Time.deltaTime > 0f ? Time.deltaTime : 1f / 60f;
            float deltaWork = WorkAmountUtility.CalculateWorkPerSecond(
                    actor,
                    target,
                    workType,
                    durationMultiplier)
                * Mathf.Max(0.05f, extraMultiplier)
                * tickDeltaTime;
            completedWork = Mathf.Min(requiredWork, completedWork + deltaWork);
            if (Time.time - lastReportTime >= 0.75f)
            {
                lastReportTime = Time.time;
                float ratio = Mathf.Clamp01(completedWork / requiredWork);
                actor?.Brain?.SetActionPhase($"{label} {Mathf.RoundToInt(ratio * 100f)}%", target);
                actor?.AddActivity(CharacterActivityEvent.Work(
                    workType,
                    CharacterActivityOutcomes.Progress,
                    $"{label} 진행 {Mathf.RoundToInt(ratio * 100f)}%",
                    target,
                    reasonCode: "work-progress",
                    value: ratio));
            }

            yield return null;
        }
    }

    private IEnumerator ExecuteResearchWork(int runId, Action<bool> onCompleted)
    {
        CharacterActor actor = work.WorkerActor;
        IBlueprintResearchWorkService researchWorkService = work.BlueprintResearchWorkService;

        const float researchSeconds = 1f;
        float requiredWork = GetResearchWorkRequired(work.assignedShop);
        yield return ExecuteWorkAmountLoop(
            runId,
            actor,
            work.assignedShop,
            FacilityWorkType.Research,
            requiredWork,
            "연구");
        if (!CanContinueTimedWork(runId, actor) || !work.isWorking)
        {
            onCompleted?.Invoke(false);
            yield break;
        }

        BlueprintResearchWorkResult result = researchWorkService.ApplyResearchWork(
            null,
            work.assignedShop,
            researchSeconds);
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

    private bool CanContinueTimedWork(int runId)
    {
        return runId <= 0 || work.IsActiveWorkRun(runId);
    }

    private bool CanContinueTimedWork(int runId, CharacterActor actor)
    {
        if (!CanContinueTimedWork(runId))
        {
            return false;
        }

        if (runId <= 0)
        {
            return true;
        }

        return actor != null
            && actor.Brain != null
            && !actor.Brain.isBestActionEnd;
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

    private static bool TryGetExteriorWorkSeconds(
        BuildableObject target,
        CharacterActor actor,
        FacilityWorkType workType,
        out float seconds)
    {
        seconds = 0f;
        if (target?.BuildingData == null)
        {
            return false;
        }

        foreach (IBuildingExteriorWorkRuntimeAbility ability in target.BuildingData.Abilities
                     .OfType<IBuildingExteriorWorkRuntimeAbility>())
        {
            if (!ability.SupportsExteriorWork(workType)
                || !ability.IsExteriorWorkAvailable(actor, target, workType))
            {
                continue;
            }

            seconds = Mathf.Max(
                MinimumNaturalExteriorWorkSeconds,
                ability.GetExteriorWorkSeconds(actor, target, workType));
            return true;
        }

        return false;
    }

    private static float GetSurvivalWorkSeconds(BuildableObject target, FacilityWorkType workType)
    {
        if (target?.BuildingData == null)
        {
            return 1f;
        }

        return workType switch
        {
            FacilityWorkType.DrawWater => Mathf.Max(
                0.1f,
                target.BuildingData.GetAbility<BuildingWaterSourceAbility>()?.workSeconds ?? 1f),
            FacilityWorkType.Cook => Mathf.Max(
                0.1f,
                target.BuildingData.GetAbility<BuildingCookingAbility>()?.workSeconds ?? 1f),
            FacilityWorkType.Treat => Mathf.Max(
                0.1f,
                target.BuildingData.GetAbility<BuildingMedicalAbility>()?.workSeconds ?? 1f),
            FacilityWorkType.Refuel => Mathf.Max(
                0.1f,
                target.BuildingData.GetAbility<BuildingFuelConsumerAbility>()?.workSeconds ?? 1f),
            _ => 1f
        };
    }

    private static float GetSurvivalWorkRequired(BuildableObject target, FacilityWorkType workType)
    {
        return Mathf.Max(0.1f, GetSurvivalWorkSeconds(target, workType));
    }

    private static float GetEquipmentCraftWorkRequired(BuildableObject target)
    {
        if (target?.BuildingData == null)
        {
            return 1f;
        }

        return Mathf.Max(
            0.1f,
            target.BuildingData.GetAbility<BuildingEquipmentCraftingAbility>()?.workSecondsPerCycle ?? 1f);
    }

    private static float GetButcherWorkRequired(BuildableObject target)
    {
        if (target?.BuildingData == null)
        {
            return 1f;
        }

        return Mathf.Max(
            0.1f,
            target.BuildingData.GetAbility<BuildingButcherAbility>()?.workSeconds ?? 1f);
    }

    private static float GetResearchWorkRequired(BuildableObject target)
    {
        if (target?.BuildingData == null)
        {
            return 1f;
        }

        return Mathf.Max(0.1f, target.BuildingData.GetRequiredWork(FacilityWorkType.Research));
    }
}
