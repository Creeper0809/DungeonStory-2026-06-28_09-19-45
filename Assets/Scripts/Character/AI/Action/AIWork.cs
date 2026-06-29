using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/AI/Action/Work", order = 0)]
public class AIWork : AIActionSet
{
    public override void Execute(Character character)
    {
        if (character != null && character.TryGetAbility(out AbilityWork work))
        {
            work.StartWorking();
            return;
        }

        if (character != null && character.ai != null)
        {
            character.ai.isBestActionEnd = true;
        }
    }
    public override IReadOnlyList<BuildableObject> GetDestinationCandidates(
        Character character,
        GridPathSearchResult searchResult)
    {
        if (character == null || !character.TryGetAbility(out AbilityWork work))
        {
            return new List<BuildableObject>();
        }

        return work.TryAssignShop(searchResult) && work.assignedShop != null
            ? new List<BuildableObject> { work.assignedShop }
            : new List<BuildableObject>();
    }
}
