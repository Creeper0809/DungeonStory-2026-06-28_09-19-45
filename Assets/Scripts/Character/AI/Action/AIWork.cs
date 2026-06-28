using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/AI/Action/Work", order = 0)]
public class AIWork : AIActionSet
{
    public override void Execute(Character character)
    {
        character.GetAbility<AbilityWork>().StartWorking();
    }
    public override BuildableObject GetDestination(Character character)
    {
        return character.GetAbility<AbilityWork>().assignedShop;
    }
}
