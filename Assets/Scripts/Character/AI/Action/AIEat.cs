using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/AI/Action/Eat", order = 0)]
public class AIEat : AIActionSet
{
    public override void Execute(Character character)
    {
        character.GetAbility<AbilityShopping>().StartSopping();
    }
    public override BuildableObject GetDestination(Character character)
    {
        Func<BuildableObject, bool> condition = (building) =>
        {
            if (building is Shop shop)
            {
                if (shop.type == Shop.Type.Food)
                {
                    return true;
                }
            }
            return false;
        };
        List<BuildableObject> reachableBuilding = character.GetReachableBuilding()
                                                           .Where(condition)
                                                           .ToList();
        if (!reachableBuilding.Any())
        {
            return null;
        }
        return reachableBuilding.First();
    }
}
