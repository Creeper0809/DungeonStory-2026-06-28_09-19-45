using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "DungeonStory/AI/Action/Shopping", order = 0)]
public class AIShopping : AIActionSet
{
    public override void Execute(Character character)
    {
        character.GetAbility<AbilityShopping>().StartSopping();
    }
    public override BuildableObject GetDestination(Character character)
    {
        AbilityShopping shopping = character.GetAbility<AbilityShopping>();
        int wantId = character.data.favoriteStore[Random.Range(0, character.data.favoriteStore.Length)].id;
        List<BuildableObject> reachableBulding = character.GetReachableBuilding()
                                                                .Where((x) => !shopping.visitedBuilding.Contains(x))
                                                                .ToList();
        if (!reachableBulding.Any())
        {
            return null;
        }
        BuildableObject selectedBuilding = reachableBulding.Any((building) => building.id == wantId)
                                                              ? reachableBulding.First((building) => building.id == wantId)
                                                              : reachableBulding.First();
        return selectedBuilding;
    }
}
