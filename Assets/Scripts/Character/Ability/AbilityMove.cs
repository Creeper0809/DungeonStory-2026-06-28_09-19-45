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
    protected override void Awake()
    {
        base.Awake();
        spawner = GameObject.FindGameObjectWithTag("CharacterSpawner").GetComponent<CharacterSpawner>();
    }
    public override void Initializtion(CharacterSO data)
    {
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
        BuildableObject nextBuilding;
        while (path.Any() && !(nextBuilding = path.Dequeue()).isDestroy)
        {
            yield return Move2Building(nextBuilding);
            if (!nextBuilding.isDestroy && nextBuilding is Stair interactable)
            {
                yield return interactable.Interact(character);
            }
        }
    }
    public IEnumerator Move2Building(BuildableObject building)
    {
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
    public void StartExitDungeon()
    {
        StartCoroutine(ExitDungeon());
    }
    private IEnumerator ExitDungeon()
    {
        Vector2Int startPos = grid.GetXY(transform.position);
        startPos = grid.IsValidGridPos(startPos) ? startPos : Vector2Int.zero;
        Func<Vector2Int, bool> condition = (pos) => grid.GetGridCell(pos).GetBuildingInlayer()?.id == 1;
        int counter = 0;
        while (true)
        {
            Queue<BuildableObject> path = grid.SmoothingPath(grid.GetGridPath(startPos, condition));
            BuildableObject nextBuilding;
            while (path.Any() && (nextBuilding = path.Dequeue()) != null)
            {
                yield return Move2Building(nextBuilding);
                if (nextBuilding is IInteractable interactable)
                {
                    yield return interactable.Interact(character);
                }
            }
            if(grid.GetXY(character.transform.position).y == 0)
            {
                break;
            }
            yield return new WaitForSeconds(1f);
            counter++;
            if(counter == 5)
            {
                break;
            }
        }
        yield return Move2Building(spawner);
        yield return spawner.Interact(character);
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
