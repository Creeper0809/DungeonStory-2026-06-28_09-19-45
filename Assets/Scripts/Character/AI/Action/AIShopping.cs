using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/AI/Action/Shopping", order = 0)]
public class AIShopping : AIActionSet
{
    public const FacilityRole CustomerInterestRoles =
        FacilityRole.Purchase
        | FacilityRole.Training
        | FacilityRole.Research
        | FacilityRole.Mana;

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
        AbilityShopping shopping = character != null ? character.GetAbility<AbilityShopping>() : null;
        if (shopping == null)
        {
            return Array.Empty<BuildableObject>();
        }

        return FacilityCandidateScorer.GetCandidates(
            character,
            searchResult,
            CustomerInterestRoles);
    }

    public override BuildableObject SelectDestination(
        Character character,
        IReadOnlyList<BuildableObject> candidates)
    {
        if (character == null || candidates == null || candidates.Count == 0)
        {
            return null;
        }

        GridPathSearchResult searchResult = character.ai != null ? character.ai.GetPathSearch(character) : null;
        return FacilityCandidateScorer.SelectBest(
            character,
            candidates,
            CustomerInterestRoles,
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
            CustomerInterestRoles,
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
