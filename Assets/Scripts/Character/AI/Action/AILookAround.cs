using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "DungeonStory/AI/Action/LookAround", order = 0)]
public class AILookAround : AIActionSet
{
    [SerializeField] private float minWaitDuration = 0.5f;
    [SerializeField] private float maxWaitDuration = 1.2f;

    public override bool RequiresDestination => false;

    public override bool CanStart(Character character)
    {
        return CanUseVisitLookAround(character);
    }

    public override void Execute(Character character)
    {
        if (character == null) return;

        if (CanUseVisitLookAround(character)
            && character.TryGetAbility(out AbilityShopping shopping))
        {
            shopping.RegisterLookAround();
        }
        float waitDuration = Random.Range(minWaitDuration, maxWaitDuration);
        character.TryGetAbility(out AbilityMove move);
        if (move != null)
        {
            if (move.StartIdleWander(waitDuration, 1, 6))
            {
                return;
            }

            move.StartWait(waitDuration);
            return;
        }

        if (character.ai != null)
        {
            character.ai.isBestActionEnd = true;
        }
    }

    public override IReadOnlyList<BuildableObject> GetDestinationCandidates(
        Character character,
        GridPathSearchResult searchResult)
    {
        if (character == null)
        {
            return new List<BuildableObject>();
        }

        if (!CanUseVisitLookAround(character))
        {
            return new List<BuildableObject>();
        }

        List<BuildableObject> reachableBuildings = searchResult != null
            ? searchResult.GetAllReachableBuilding()
            : character.GetReachableBuilding();

        Vector2Int currentPos = character.GetNowXY();
        return reachableBuildings
            .Where((building) => building != null
                && !building.isDestroy
                && building.IsGridMovement
                && (building.buildPoses == null || !building.buildPoses.Contains(currentPos)))
            .OrderBy((_) => Random.value)
            .ToList();
    }

    public override BuildableObject SelectDestination(
        Character character,
        IReadOnlyList<BuildableObject> candidates)
    {
        if (candidates == null || candidates.Count == 0)
        {
            return null;
        }

        return candidates.FirstOrDefault();
    }

    public static bool CanUseVisitLookAround(Character character)
    {
        if (character == null
            || !character.TryGetAbility(out AbilityShopping shopping)
            || !shopping.CanLookAround())
        {
            return false;
        }

        return !CharacterWorkRoleUtility.TryGetWork(character, out AbilityWork work)
            || work.IsOffDuty;
    }
}
