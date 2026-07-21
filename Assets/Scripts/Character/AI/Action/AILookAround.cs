using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "DungeonStory/AI/Action/LookAround", order = 0)]
public class AILookAround : AIActionSet
{
    private static readonly CharacterAiActionDescriptor ActionDescriptor = new CharacterAiActionDescriptor(
        CharacterAiBranch.LookAround,
        "둘러보기",
        CharacterAiActionTags.Curiosity);

    public override CharacterAiActionDescriptor Descriptor => ActionDescriptor;
    private const float FallbackScore = 0.05f;

    [SerializeField] private float minWaitDuration = 0.5f;
    [SerializeField] private float maxWaitDuration = 1.2f;

    public override bool RequiresDestination => false;

    public override bool CanStart(CharacterActor actor)
    {
        return CanUseVisitLookAround(actor);
    }

    public override float AdjustScore(CharacterActor actor, float baseScore)
    {
        return Mathf.Clamp01(Mathf.Min(baseScore, FallbackScore));
    }

    public override void Execute(CharacterActor actor)
    {
        if (actor == null) return;

        if (CanUseVisitLookAround(actor)
            && actor.TryGetAbility(out AbilityShopping shopping))
        {
            shopping.RegisterLookAround();
        }

        float waitDuration = Random.Range(minWaitDuration, maxWaitDuration);
        actor.TryGetAbility(out AbilityMove move);
        if (move != null)
        {
            if (move.StartIdleWander(waitDuration, 1, 6))
            {
                return;
            }

            move.StartWait(waitDuration);
            return;
        }

        if (actor.Brain != null)
        {
            actor.Brain.isBestActionEnd = true;
        }
    }

    public override IReadOnlyList<BuildableObject> GetDestinationCandidates(
        CharacterActor actor,
        GridPathSearchResult searchResult)
    {
        if (actor == null || !CanUseVisitLookAround(actor))
        {
            return new List<BuildableObject>();
        }

        List<BuildableObject> reachableBuildings = searchResult != null
            ? searchResult.GetAllReachableBuilding()
            : actor.GetReachableBuilding();

        Vector2Int currentPos = actor.GetNowXY();
        return reachableBuildings
            .Where((building) => building != null
                && !building.isDestroy
                && building.IsGridMovement
                && (building.buildPoses == null || !building.buildPoses.Contains(currentPos)))
            .OrderBy((_) => Random.value)
            .ToList();
    }

    public override BuildableObject SelectDestination(
        CharacterActor actor,
        IReadOnlyList<BuildableObject> candidates)
    {
        if (candidates == null || candidates.Count == 0)
        {
            return null;
        }

        return candidates.FirstOrDefault();
    }

    public static bool CanUseVisitLookAround(CharacterActor actor)
    {
        if (actor == null
            || !actor.TryGetAbility(out AbilityShopping shopping)
            || !shopping.CanLookAround())
        {
            return false;
        }

        return !CharacterWorkRoleUtility.TryGetWork(actor, out AbilityWork work)
            || work.IsOffDuty;
    }
}
