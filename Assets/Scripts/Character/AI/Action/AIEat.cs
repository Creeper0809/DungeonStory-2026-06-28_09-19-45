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
    public override IReadOnlyList<BuildableObject> GetDestinationCandidates(
        Character character,
        GridPathSearchResult searchResult)
    {
        if (character == null) return Array.Empty<BuildableObject>();

        IEnumerable<BuildableObject> reachableBuildings = searchResult != null
            ? searchResult.GetAllVisitableBuilding()
            : character.GetReachableBuilding();

        return reachableBuildings
            .Where((building) => building is Shop shop && shop.type == Shop.Type.Food)
            .ToList();
    }
}
