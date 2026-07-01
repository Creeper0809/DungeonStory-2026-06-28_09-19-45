using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class WorkCommandHandler
{
    private readonly AbilityWork work;
    private readonly WorkTargetSelector targetSelector;
    private BuildableObject priorityWorkTarget;
    private Character prioritySuppressTarget;
    private Grid prioritySuppressGrid;
    private FacilityWorkType priorityWorkType = FacilityWorkType.None;

    public WorkCommandHandler(AbilityWork work, WorkTargetSelector targetSelector)
    {
        this.work = work;
        this.targetSelector = targetSelector;
    }

    public BuildableObject PriorityWorkTarget => priorityWorkTarget;
    public Character PrioritySuppressTarget => prioritySuppressTarget;
    public bool HasPrioritySuppressTarget => prioritySuppressTarget != null && !prioritySuppressTarget.IsDead;
    public FacilityWorkType PriorityWorkType => priorityWorkType;

    public bool TrySetPriorityWorkTarget(BuildableObject building, out string errorMessage)
    {
        return TrySetPriorityWorkTarget(building, FacilityWorkType.None, null, out errorMessage);
    }

    public bool TrySetPriorityWorkTarget(
        BuildableObject building,
        FacilityWorkType preferredWorkType,
        GridPathSearchResult searchResult,
        out string errorMessage)
    {
        if (building == null)
        {
            errorMessage = "우선 지정할 시설이 없습니다";
            return false;
        }

        bool forced = preferredWorkType != FacilityWorkType.None;
        if (!targetSelector.TryEvaluateWorkTarget(building, searchResult, preferredWorkType, forced, out WorkTargetCandidate candidate))
        {
            errorMessage = candidate.FailureReason;
            return false;
        }

        priorityWorkTarget = building;
        prioritySuppressTarget = null;
        prioritySuppressGrid = null;
        priorityWorkType = candidate.WorkType;
        work.AssignWork(building, candidate.WorkType);
        work.WorkerCharacter?.ai?.RequestImmediateReplan(clearFailures: true);
        work.WorkerCharacter?.AddLog($"우선 작업 지정: {building.name} - {WorkTaskCatalog.GetDisplayName(candidate.WorkType)}");
        errorMessage = string.Empty;
        return true;
    }

    public bool TrySetPrioritySuppressTarget(
        Character target,
        GridPathSearchResult searchResult,
        out string errorMessage)
    {
        if (!WorkCommandResolver.TryResolveSuppressCommand(work.WorkerCharacter, target, out errorMessage))
        {
            return false;
        }

        if (!CanReachSuppressTarget(target, searchResult, out errorMessage))
        {
            return false;
        }

        prioritySuppressTarget = target;
        prioritySuppressGrid = WorkGridUtility.ResolveActiveGrid(work, searchResult);
        priorityWorkTarget = null;
        priorityWorkType = FacilityWorkType.Guard;
        work.AssignWork(null, FacilityWorkType.Guard);
        work.SetDutyState(AbilityWork.DutyState.OnDuty);
        work.WorkerCharacter?.ai?.RequestImmediateReplan(clearFailures: true);
        work.WorkerCharacter?.AddLog($"우선 제압 지정: {target.name}");
        errorMessage = string.Empty;
        return true;
    }

    public bool TryGetPrioritySuppressDestination(GridPathSearchResult searchResult, out BuildableObject destination)
    {
        destination = null;
        if (!HasPrioritySuppressTarget)
        {
            return false;
        }

        Grid activeGrid = WorkGridUtility.ResolveActiveGrid(work, searchResult, prioritySuppressGrid);
        if (activeGrid == null)
        {
            return false;
        }

        GridCell cell = activeGrid.GetGridCell(WorkGridUtility.GetGridPosition(activeGrid, prioritySuppressTarget));
        if (cell == null)
        {
            return false;
        }

        destination = cell.GetAllBuilding()
            .Where((building) => building != null && !building.isDestroy)
            .OrderByDescending((building) => building.IsGridMovement)
            .FirstOrDefault();
        return destination != null;
    }

    public void ClearPriorityWorkTarget()
    {
        priorityWorkTarget = null;
        prioritySuppressTarget = null;
        prioritySuppressGrid = null;
        priorityWorkType = FacilityWorkType.None;
    }

    public bool HasUrgentPriorityTarget()
    {
        if (priorityWorkTarget == null)
        {
            return false;
        }

        bool forced = priorityWorkType != FacilityWorkType.None;
        return targetSelector.TryEvaluateWorkTarget(priorityWorkTarget, null, priorityWorkType, forced, out WorkTargetCandidate candidate)
            && candidate.UrgencyScore >= 60f;
    }

    public IEnumerator SuppressPriorityTarget()
    {
        Character character = work.WorkerCharacter;
        if (character == null || character.ai == null)
        {
            yield break;
        }

        work.EnsureWorkReferences();
        AbilityMove move = work.WorkerMove;
        Character target = prioritySuppressTarget;
        if (!WorkCommandResolver.TryResolveSuppressCommand(character, target, out string errorMessage))
        {
            character.AddLog($"제압 실패: {errorMessage}");
            ClearPriorityWorkTarget();
            character.ai.isBestActionEnd = true;
            yield break;
        }

        Grid activeGrid = WorkGridUtility.ResolveActiveGrid(work, null, prioritySuppressGrid);
        if (move == null || activeGrid == null)
        {
            character.AddLog("제압 실패: 이동 정보 없음");
            ClearPriorityWorkTarget();
            character.ai.isBestActionEnd = true;
            yield break;
        }

        character.AddLog($"제압 시작: {target.name}");
        while (target != null && !target.IsDead && character != null && !character.IsDead)
        {
            Vector2Int targetPos = WorkGridUtility.GetGridPosition(activeGrid, target);
            Vector2Int actorPos = WorkGridUtility.GetGridPosition(activeGrid, character);
            if (actorPos != targetPos)
            {
                Queue<GridMoveStep> path = activeGrid.GetMovePath(actorPos, (pos) => pos == targetPos);
                if (path == null || path.Count == 0)
                {
                    character.AddLog("제압 실패: 도달할 수 없는 대상");
                    break;
                }

                yield return move.MoveByPath(path);
                continue;
            }

            float damage = Mathf.Max(1f, work.SuppressBaseDamage * character.GetCombatPowerMultiplier());
            target.ApplyDamage(damage, $"제압: {character.name}");
            character.AddLog(target.IsDead
                ? $"제압 완료: {target.name}"
                : $"제압 공격: {target.name}");

            if (target.IsDead)
            {
                if (target.TryGetComponent(out InvasionIntruderRuntime intruderRuntime))
                {
                    intruderRuntime.ResolveSuppressedBy(character);
                }
                else if (StaffDiscontentRuntime.Instance != null)
                {
                    StaffDiscontentRuntime.Instance.ResolveSuppressedRebel(target, character);
                }
                break;
            }

            yield return new WaitForSeconds(Mathf.Max(0.1f, work.SuppressAttackInterval));
        }

        ClearPriorityWorkTarget();
        character.ai.isBestActionEnd = true;
    }

    private bool CanReachSuppressTarget(
        Character target,
        GridPathSearchResult searchResult,
        out string errorMessage)
    {
        errorMessage = string.Empty;
        if (target == null)
        {
            errorMessage = "제압할 대상이 없습니다";
            return false;
        }

        Grid activeGrid = WorkGridUtility.ResolveActiveGrid(work, searchResult);
        if (activeGrid == null)
        {
            errorMessage = "그리드가 초기화되지 않았습니다";
            return false;
        }

        Vector2Int targetPos = WorkGridUtility.GetGridPosition(activeGrid, target);
        if (!activeGrid.IsValidGridPos(targetPos))
        {
            errorMessage = "제압 대상 위치가 유효하지 않습니다";
            return false;
        }

        Character character = work.WorkerCharacter;
        if (character != null && WorkGridUtility.GetGridPosition(activeGrid, character) == targetPos)
        {
            return true;
        }

        Vector2Int actorPos = character != null ? WorkGridUtility.GetGridPosition(activeGrid, character) : Vector2Int.zero;
        Queue<GridMoveStep> path = searchResult != null
            ? searchResult.GetMovePath((pos) => pos == targetPos)
            : activeGrid.GetMovePath(actorPos, (pos) => pos == targetPos);
        if (path == null || path.Count == 0)
        {
            errorMessage = "도달할 수 없는 대상입니다";
            return false;
        }

        return true;
    }
}
