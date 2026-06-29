using BehaviorDesigner.Runtime.Tasks.Unity.UnityAnimation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        moveSpeed = data.moveSpeed;
    }

    public void MoveLeft()
    {
        character.Flip(Character.Facing.LEFT);
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
    }
    public void MoveRight()
    {
        character.Flip(Character.Facing.RIGHT);
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
    }
    public IEnumerator MoveByPath(Queue<BuildableObject> path)
    {
        if (path == null) yield break;

        BuildableObject nextBuilding;
        while (path.Any() && !(nextBuilding = path.Dequeue()).isDestroy)
        {
            if (!nextBuilding.isDestroy && nextBuilding is Stair interactable)
            {
                yield return Move2BuildingCenter(nextBuilding);
                yield return interactable.Interact(character);
                continue;
            }

            yield return Move2Building(nextBuilding);
        }
    }

    public IEnumerator MoveByPath(Queue<GridMoveStep> path)
    {
        if (path == null) yield break;

        while (path.Any())
        {
            GridMoveStep step = path.Dequeue();
            if (step == null) continue;

            yield return MoveByStep(step);
        }
    }

    private IEnumerator MoveByStep(GridMoveStep step)
    {
        if (step.MoveType == GridMoveType.Walk)
        {
            yield return Move2GridPosition(step.To);
            yield break;
        }

        yield return Move2GridPosition(step.From);
        if (step.MovementOccupant is BuildableObject building
            && !building.isDestroy
            && building is IGridMovementHandler movementHandler)
        {
            yield return movementHandler.Traverse(character, step);
            yield break;
        }

        yield return Move2GridPosition(step.To);
    }

    public IEnumerator Move2GridPosition(Vector2Int gridPosition)
    {
        if (grid == null) yield break;

        Vector3 endPos = grid.GetWorldPos(gridPosition);
        Vector2Int current = grid.GetXY(character.transform.position);
        if (current.y == gridPosition.y)
        {
            endPos.y = character.transform.position.y;
        }

        yield return Move2PosBySpeed(endPos);
    }

    public IEnumerator Move2Building(BuildableObject building)
    {
        if (building == null) yield break;

        Vector2Int characterPos = grid.GetXY(character.transform.position);
        while (!building.isDestroy && (building != grid.GetGridCell(characterPos)?.GetBuildingInlayer() && building.centerPos.x != characterPos.x))
        {
            characterPos = grid.GetXY(character.transform.position);
            if (building.centerPos.x > characterPos.x)
            {
                MoveLeft();
            }
            else
            {
                MoveRight();
            }
            yield return null;
        }
    }

    private IEnumerator Move2BuildingCenter(BuildableObject building)
    {
        if (building == null) yield break;

        Vector3 target = character.transform.position;
        target.x = building.transform.position.x;
        yield return Move2PosBySpeed(target);
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
            yield return MoveByPath(new Queue<GridMoveStep>(action.pathSteps));
            yield break;
        }

        yield return MoveByPath(action.path);
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
            if (path.Any())
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
    public IEnumerator Move2PosBySpeed(Vector3 endPos, float multifly = 1.0f)
    {
        Vector3 startPos = transform.position;
        if (startPos.x < endPos.x)
        {
            character.Flip(Character.Facing.RIGHT);
        }
        else
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
            transform.position = Vector3.Lerp(startPos, endPos, (timer / duration));
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
    }
}
