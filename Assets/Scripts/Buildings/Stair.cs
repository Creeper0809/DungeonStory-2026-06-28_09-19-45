using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stair : BuildableObject, IInteractable
{
    private WaitForSeconds delay;
    public override void Start()
    {
        base.Start();
        delay = new WaitForSeconds(0.15f);
    }
    public IEnumerator Interact(Character character)
    {
        AbilityMove moveable = character.GetAbility<AbilityMove>();
        Vector3 centerPos = grid.GetWorldPos(this.centerPos);
        yield return moveable.Move2PosBySpeed(new Vector2(centerPos.x, character.transform.position.y),0.7f);
        yield return delay;
        if (centerPos.y == character.transform.position.y)
        {
            character.transform.position = centerPos + (Vector3.up*3);
        }
        else
        {
            character.transform.position = centerPos;
        }
        yield return delay;
    }
}
