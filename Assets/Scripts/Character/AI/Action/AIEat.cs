using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/AI/Action/Eat", order = 0)]
public class AIEat : AIActionSet
{
    public override bool CanStart(CharacterActor actor)
    {
        return CanUseVisitorAction(actor);
    }

    public override void Execute(CharacterActor actor)
    {
        actor?.GetAbility<AbilityShopping>()?.StartSopping();
    }

    public override IReadOnlyList<BuildableObject> GetDestinationCandidates(
        CharacterActor actor,
        GridPathSearchResult searchResult)
    {
        if (actor == null) return Array.Empty<BuildableObject>();

        return FacilityCandidateScorer.GetCandidates(
            actor,
            searchResult,
            FacilityRole.Meal);
    }

    public override BuildableObject SelectDestination(
        CharacterActor actor,
        IReadOnlyList<BuildableObject> candidates)
    {
        GridPathSearchResult searchResult = actor != null && actor.Brain != null
            ? actor.Brain.GetPathSearch(actor)
            : null;
        return FacilityCandidateScorer.SelectBest(
            actor,
            candidates,
            FacilityRole.Meal,
            searchResult,
            FacilityScoringContext.RequireFromActor(actor));
    }

    public override bool TryResolveDestination(
        CharacterActor actor,
        GridPathSearchResult searchResult,
        out BuildableObject destination,
        out string failureReason)
    {
        if (FacilityCandidateScorer.TrySelectBest(
            actor,
            searchResult,
            FacilityRole.Meal,
            FacilityScoringContext.RequireFromActor(actor),
            out destination))
        {
            failureReason = string.Empty;
            return true;
        }

        failureReason = "목적지 없음";
        return false;
    }

    public override bool TryReserveDestination(
        CharacterActor actor,
        BuildableObject destination,
        out AIActionFailure failure)
    {
        failure = AIActionFailure.None;
        if (destination == null)
        {
            return true;
        }

        if (destination.TryReserveVisit(actor, out string failureReason))
        {
            return true;
        }

        failure = AIActionFailure.FromReason(
            failureReason,
            AIActionFailureKind.DestinationOccupied,
            destination);
        return false;
    }

    public override void RefreshDestinationReservation(CharacterActor actor, BuildableObject destination)
    {
        destination?.RefreshVisitReservation(actor);
    }

    public override void ReleaseDestinationReservation(CharacterActor actor, BuildableObject destination)
    {
        destination?.ReleaseVisitReservation(actor);
    }

    private static bool CanUseVisitorAction(CharacterActor actor)
    {
        if (actor == null) return false;

        if (!actor.TryGetAbility(out AbilityShopping _))
        {
            return false;
        }

        if (CharacterWorkRoleUtility.TryGetWork(actor, out AbilityWork work))
        {
            return work.IsOffDuty;
        }

        return true;
    }
}
