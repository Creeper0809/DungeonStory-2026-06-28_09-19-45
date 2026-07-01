using BehaviorDesigner.Runtime.Tasks.Unity.UnityAnimation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityMove : CharacterAbility
{
    private float moveSpeed;
    private CharacterSpawner spawner;
    private Coroutine enterDungeonRoutine;

    protected override void Awake()
    {
        base.Awake();
        GameObject spawnerObject = GameObject.FindGameObjectWithTag("CharacterSpawner");
        if (spawnerObject != null)
        {
            spawner = spawnerObject.GetComponent<CharacterSpawner>();
        }
    }

    public override void Initializtion(CharacterSO data)
    {
        base.Initializtion(data);
        moveSpeed = character != null
            ? character.GetMoveSpeed()
            : data != null
                ? data.moveSpeed
                : 1f;
        moveSpeed = Mathf.Max(0.1f, moveSpeed);
    }

    public IEnumerator MoveByPath(Queue<GridMoveStep> path, AIAction expectedAction = null)
    {
        if (path == null) yield break;

        while (path.Count > 0)
        {
            if (IsActionMovementCancelled(expectedAction))
            {
                yield break;
            }

            GridMoveStep step = path.Dequeue();
            if (step == null) continue;

            RefreshCurrentActionReservation();
            yield return MoveByStep(step, expectedAction);
        }
    }

    public IEnumerator MoveByStep(GridMoveStep step, AIAction expectedAction = null)
    {
        if (step.MoveType == GridMoveType.Walk)
        {
            yield return Move2GridPosition(step.To, expectedAction);
            yield break;
        }

        yield return Move2GridPosition(step.From, expectedAction);
        if (IsActionMovementCancelled(expectedAction))
        {
            yield break;
        }

        if (step.MovementOccupant is BuildableObject building
            && !building.isDestroy
            && building is IGridMovementHandler movementHandler)
        {
            yield return movementHandler.Traverse(character, step);
            yield break;
        }

        yield return Move2GridPosition(step.To, expectedAction);
    }

    public IEnumerator Move2GridPosition(Vector2Int gridPosition, AIAction expectedAction = null)
    {
        if (grid == null) yield break;

        RefreshCurrentActionReservation();
        Vector3 endPos = grid.GetWorldPos(gridPosition);
        yield return Move2PosBySpeed(endPos, 1.0f, expectedAction);
    }

    public void StartExitDungeon()
    {
        if (character == null || character.CurrentLifecycleState == Character.LifecycleState.ExitingDungeon)
        {
            return;
        }

        if (enterDungeonRoutine != null)
        {
            StopCoroutine(enterDungeonRoutine);
            enterDungeonRoutine = null;
        }

        character.SetLifecycleState(Character.LifecycleState.ExitingDungeon);
        StartCoroutine(ExitDungeon());
    }

    public void StartEnterDungeon(Vector3 entryDoorWorldPosition, Vector2Int entryGridPosition)
    {
        if (enterDungeonRoutine != null)
        {
            StopCoroutine(enterDungeonRoutine);
        }

        enterDungeonRoutine = StartCoroutine(EnterDungeon(entryDoorWorldPosition, entryGridPosition));
    }

    public void StartMoveByCurrentActionPath(float waitDuration = 0f)
    {
        StartCoroutine(MoveByCurrentActionPath(waitDuration));
    }

    public void StartWait(float duration)
    {
        StartCoroutine(WaitForAiAction(duration));
    }

    public bool StartIdleWander(float waitDuration, int minDistance = 2, int maxDistance = 8)
    {
        if (!TryFindIdleWanderPath(minDistance, maxDistance, out Queue<GridMoveStep> path)
            || path == null
            || path.Count == 0)
        {
            return false;
        }

        StartCoroutine(MoveByPathThenWait(path, waitDuration));
        return true;
    }

    private IEnumerator MoveByCurrentActionPath(float waitDuration)
    {
        yield return MoveByCurrentBestActionPath();

        if (waitDuration > 0f)
        {
            yield return new WaitForSeconds(waitDuration);
        }

        if (character != null && character.ai != null)
        {
            character.ai.isBestActionEnd = true;
        }
    }

    private IEnumerator MoveByPathThenWait(Queue<GridMoveStep> path, float waitDuration)
    {
        yield return MoveByPath(path);

        if (waitDuration > 0f)
        {
            yield return new WaitForSeconds(waitDuration);
        }

        if (character != null && character.ai != null)
        {
            character.ai.isBestActionEnd = true;
        }
    }

    public bool TryFindIdleWanderPath(
        int minDistance,
        int maxDistance,
        out Queue<GridMoveStep> path)
    {
        path = null;
        CacheCommonReferences();
        if (character == null || grid == null)
        {
            return false;
        }

        Vector2Int originalPos = grid.GetXY(character.transform.position);
        GridPathSearchResult searchResult = GetIdleSearchResult(originalPos);
        if (searchResult == null)
        {
            return false;
        }

        int min = Mathf.Max(1, minDistance);
        return searchResult.TryGetMovePathToRandomReachablePosition(
                IsPlainIdleWalkable,
                IsSupportedIdleWanderPath,
                min,
                maxDistance,
                out path)
            || searchResult.TryGetMovePathToRandomReachablePosition(
                IsPlainIdleWalkable,
                IsSupportedIdleWanderPath,
                1,
                0,
                out path);
    }

    private bool IsPlainIdleWalkable(Vector2Int position)
    {
        if (grid == null || !grid.IsWalkable(position))
        {
            return false;
        }

        IGridOccupant buildingOccupant = grid.GetGridCell(position)?.GetOccupant(GridLayer.Building);
        return buildingOccupant == null || !buildingOccupant.IsGridMovement;
    }

    private static bool IsSupportedIdleWanderPath(Queue<GridMoveStep> path)
    {
        if (path == null || path.Count == 0)
        {
            return false;
        }

        foreach (GridMoveStep step in path)
        {
            if (step == null)
            {
                return false;
            }

            if (step.MoveType == GridMoveType.Walk)
            {
                continue;
            }

            if (!IsSupportedVerticalMovementStep(step))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsSupportedVerticalMovementStep(GridMoveStep step)
    {
        return step != null
            && (step.MoveType == GridMoveType.Stair || step.MoveType == GridMoveType.Elevator)
            && step.MovementOccupant is IGridMovementHandler;
    }

    private GridPathSearchResult GetIdleSearchResult(Vector2Int originalPos)
    {
        if (grid == null)
        {
            return null;
        }

        if (!grid.IsValidGridPos(originalPos) || !grid.IsWalkable(originalPos))
        {
            return null;
        }

        return character.ai != null
            ? character.ai.GetPathSearch(character)
            : grid.SearchPath(originalPos);
    }

    private void SnapToGridRowIfWalkable(Vector2Int gridPosition)
    {
        if (grid == null
            || !grid.IsValidGridPos(gridPosition)
            || !grid.IsWalkable(gridPosition))
        {
            return;
        }

        Vector3 position = transform.position;
        position.y = grid.GetWorldPos(gridPosition).y;
        transform.position = position;
    }

    public IEnumerator MoveByCurrentBestActionPath()
    {
        AIAction action = character != null && character.ai != null
            ? character.ai.bestAction
            : null;

        if (action == null)
        {
            yield break;
        }

        if (action.pathSteps != null && action.pathSteps.Count > 0)
        {
            yield return MoveByPath(new Queue<GridMoveStep>(action.pathSteps), action);
        }
    }

    private void RefreshCurrentActionReservation()
    {
        if (character == null || character.ai == null || character.ai.bestAction == null)
        {
            return;
        }

        character.ai.bestAction.RefreshReservation(character);
    }

    private bool IsActionMovementCancelled(AIAction expectedAction)
    {
        return expectedAction != null
            && (character == null
                || character.ai == null
                || character.ai.bestAction != expectedAction
                || character.ai.isBestActionEnd);
    }

    private IEnumerator WaitForAiAction(float duration)
    {
        if (duration > 0f)
        {
            yield return new WaitForSeconds(duration);
        }

        if (character != null && character.ai != null)
        {
            character.ai.isBestActionEnd = true;
        }
    }

    private IEnumerator EnterDungeon(Vector3 entryDoorWorldPosition, Vector2Int entryGridPosition)
    {
        if (character != null)
        {
            character.SetLifecycleState(Character.LifecycleState.EnteringDungeon);
        }

        CacheCommonReferences();

        yield return Move2PosBySpeed(entryDoorWorldPosition);

        if (grid != null && grid.IsValidGridPos(entryGridPosition))
        {
            yield return Move2PosBySpeed(grid.GetWorldPos(entryGridPosition));
        }

        if (character != null)
        {
            character.ChangeLayer("Default");
            character.SetLifecycleState(Character.LifecycleState.Active);
        }

        enterDungeonRoutine = null;
    }

    private IEnumerator ExitDungeon()
    {
        if (grid == null)
        {
            CacheCommonReferences();
        }

        if (grid == null)
        {
            if (character != null)
            {
                character.SetLifecycleState(Character.LifecycleState.Active);
            }
            yield break;
        }

        Func<Vector2Int, bool> condition = (pos) => grid.GetGridCell(pos)?.GetBuildingInlayer()?.id == 1;
        int counter = 0;
        while (counter < 5)
        {
            Vector2Int startPos = grid.GetXY(transform.position);
            startPos = grid.IsValidGridPos(startPos) ? startPos : Vector2Int.zero;
            Queue<GridMoveStep> path = grid.GetMovePath(startPos, condition);
            if (path.Count > 0)
            {
                yield return MoveByPath(path);
            }
            if(grid.GetXY(character.transform.position).y == 0)
            {
                break;
            }
            yield return new WaitForSeconds(1f);
            counter++;
        }

        if (spawner == null)
        {
            GameObject spawnerObject = GameObject.FindGameObjectWithTag("CharacterSpawner");
            if (spawnerObject != null)
            {
                spawner = spawnerObject.GetComponent<CharacterSpawner>();
            }
        }

        if (spawner != null)
        {
            yield return Move2PosBySpeed(spawner.GetEntryDoorWorldPosition());
            yield return Move2PosBySpeed(spawner.GetOutsideSpawnWorldPosition());
            if (character != null)
            {
                character.SetLifecycleState(Character.LifecycleState.Despawned);
            }
            yield return spawner.Interact(character);
        }
        else if (character != null)
        {
            character.SetLifecycleState(Character.LifecycleState.Active);
        }
    }
    public IEnumerator Move2PosByTime(Vector3 endPos, float duration)
    {
        float timer = 0f;
        Vector3 startPos = transform.position;
        while (timer < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, (timer / duration));
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
    }
    public IEnumerator Move2PosBySpeed(Vector3 endPos, float multifly = 1.0f, AIAction expectedAction = null)
    {
        Vector3 startPos = transform.position;
        float deltaX = endPos.x - startPos.x;
        if (Mathf.Abs(deltaX) > 0.01f && deltaX > 0f)
        {
            character.Flip(Character.Facing.RIGHT);
        }
        else if (Mathf.Abs(deltaX) > 0.01f)
        {
            character.Flip(Character.Facing.LEFT);
        }
        float distance = Vector3.Distance(startPos, endPos);
        float totalSpeed = moveSpeed * multifly;
        if (totalSpeed <= 0f)
        {
            yield break;
        }

        float duration = distance / totalSpeed;
        float timer = 0f;

        while (timer < duration)
        {
            if (IsActionMovementCancelled(expectedAction))
            {
                yield break;
            }

            transform.position = Vector3.Lerp(startPos, endPos, (timer / duration));
            timer += Time.deltaTime;
            int frameStride = CharacterAiScheduler.GetMovementFrameStride(character);
            for (int i = 1; i < frameStride && timer < duration; i++)
            {
                yield return null;
                if (IsActionMovementCancelled(expectedAction))
                {
                    yield break;
                }

                timer += Time.deltaTime;
            }
            yield return null;
        }
        transform.position = endPos;
    }
}
