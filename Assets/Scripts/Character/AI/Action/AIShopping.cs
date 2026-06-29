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
    public override IReadOnlyList<BuildableObject> GetDestinationCandidates(
        Character character,
        GridPathSearchResult searchResult)
    {
        AbilityShopping shopping = character != null ? character.GetAbility<AbilityShopping>() : null;
        if (shopping == null)
        {
            return Array.Empty<BuildableObject>();
        }

        IEnumerable<BuildableObject> reachableBuildings = searchResult != null
            ? searchResult.GetAllVisitableBuilding()
            : character.GetReachableBuilding();

        return reachableBuildings
            .Where((building) => building != null && !shopping.visitedBuilding.Contains(building))
            .ToList();
    }

    public override BuildableObject SelectDestination(
        Character character,
        IReadOnlyList<BuildableObject> candidates)
    {
        if (character == null || candidates == null || candidates.Count == 0)
        {
            return null;
        }

        if (character.data == null || character.data.favoriteStore == null || character.data.favoriteStore.Length == 0)
        {
            return candidates.FirstOrDefault();
        }

        int wantId = character.data.favoriteStore[Random.Range(0, character.data.favoriteStore.Length)].id;
        return candidates.FirstOrDefault((building) => building != null && building.id == wantId)
            ?? candidates.FirstOrDefault();
    }
}
