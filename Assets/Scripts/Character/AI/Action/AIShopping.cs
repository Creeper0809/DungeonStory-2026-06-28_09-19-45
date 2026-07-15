using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/AI/Action/Shopping", order = 0)]
public class AIShopping : AIActionSet
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
        AbilityShopping shopping = actor != null ? actor.GetAbility<AbilityShopping>() : null;
        if (shopping == null)
        {
            return Array.Empty<BuildableObject>();
        }

        return FacilityCandidateScorer.GetCandidates(
            actor,
            searchResult,
            shopping.GetInterestRoles());
    }

    public override BuildableObject SelectDestination(
        CharacterActor actor,
        IReadOnlyList<BuildableObject> candidates)
    {
        if (actor == null || candidates == null || candidates.Count == 0)
        {
            return null;
        }

        AbilityShopping shopping = actor.GetAbility<AbilityShopping>();
        if (shopping == null)
        {
            return null;
        }

        GridPathSearchResult searchResult = actor.Brain != null ? actor.Brain.GetPathSearch(actor) : null;
        return FacilityCandidateScorer.SelectBest(
            actor,
            candidates,
            shopping.GetInterestRoles(),
            searchResult,
            FacilityScoringContext.RequireFromActor(actor));
    }

    public override bool TryResolveDestination(
        CharacterActor actor,
        GridPathSearchResult searchResult,
        out BuildableObject destination,
        out string failureReason)
    {
        AbilityShopping shopping = actor != null ? actor.GetAbility<AbilityShopping>() : null;
        if (shopping == null)
        {
            destination = null;
            failureReason = "쇼핑 능력 없음";
            return false;
        }

        if (FacilityCandidateScorer.TrySelectBest(
            actor,
            searchResult,
            shopping.GetInterestRoles(),
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
