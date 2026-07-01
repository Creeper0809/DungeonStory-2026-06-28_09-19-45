using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/AI/Action/Rest", order = 0)]
public class AIRest : AIActionSet
{
    public override bool CanStart(Character character)
    {
        if (character == null) return false;

        if (!character.TryGetAbility(out AbilityShopping _))
        {
            return false;
        }

        if (CharacterWorkRoleUtility.TryGetWork(character, out AbilityWork work))
        {
            return work.IsOffDuty;
        }

        return true;
    }

    public override void Execute(Character character)
    {
        character.GetAbility<AbilityShopping>().StartSopping();
    }

    public override IReadOnlyList<BuildableObject> GetDestinationCandidates(
        Character character,
        GridPathSearchResult searchResult)
    {
        if (character == null)
        {
            return Array.Empty<BuildableObject>();
        }

        return FacilityCandidateScorer.GetCandidates(
            character,
            searchResult,
            FacilityRole.Rest);
    }

    public override BuildableObject SelectDestination(
        Character character,
        IReadOnlyList<BuildableObject> candidates)
    {
        GridPathSearchResult searchResult = character.ai != null ? character.ai.GetPathSearch(character) : null;
        return FacilityCandidateScorer.SelectBest(
            character,
            candidates,
            FacilityRole.Rest,
            searchResult);
    }

    public override bool TryResolveDestination(
        Character character,
        GridPathSearchResult searchResult,
        out BuildableObject destination,
        out string failureReason)
    {
        if (FacilityCandidateScorer.TrySelectBest(
            character,
            searchResult,
            FacilityRole.Rest,
            out destination))
        {
            failureReason = string.Empty;
            return true;
        }

        failureReason = "목적지 없음";
        return false;
    }

    public override bool TryReserveDestination(
        Character character,
        BuildableObject destination,
        out AIActionFailure failure)
    {
        failure = AIActionFailure.None;
        if (destination == null)
        {
            return true;
        }

        if (destination.TryReserveVisit(character, out string failureReason))
        {
            return true;
        }

        failure = AIActionFailure.FromReason(
            failureReason,
            AIActionFailureKind.DestinationOccupied,
            destination);
        return false;
    }

    public override void RefreshDestinationReservation(Character character, BuildableObject destination)
    {
        destination?.RefreshVisitReservation(character);
    }

    public override void ReleaseDestinationReservation(Character character, BuildableObject destination)
    {
        destination?.ReleaseVisitReservation(character);
    }
}
