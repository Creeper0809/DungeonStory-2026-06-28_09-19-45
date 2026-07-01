using System.Collections.Generic;
using UnityEngine;

public static class FacilityCandidateScorer
{
    private static readonly FacilityRole[] ScoredRoles =
    {
        FacilityRole.Meal,
        FacilityRole.Purchase,
        FacilityRole.Rest,
        FacilityRole.Training,
        FacilityRole.Research,
        FacilityRole.Mana,
        FacilityRole.Logistics
    };

    public static List<BuildableObject> GetCandidates(
        Character character,
        GridPathSearchResult searchResult,
        FacilityRole role)
    {
        List<BuildableObject> result = new List<BuildableObject>();
        IEnumerable<BuildableObject> source;
        if (searchResult != null)
        {
            source = FacilityCandidateCache.GetCandidates(searchResult.sourceGrid, role);
        }
        else if (character != null)
        {
            source = character.GetReachableBuilding();
        }
        else
        {
            source = System.Array.Empty<BuildableObject>();
        }

        foreach (BuildableObject building in source)
        {
            if (searchResult != null && !searchResult.ContainsVisitableOccupant(building))
            {
                continue;
            }

            if (IsCandidate(character, building, role, out _))
            {
                result.Add(building);
            }
        }

        return result;
    }

    public static BuildableObject SelectBest(
        Character character,
        IReadOnlyList<BuildableObject> candidates,
        FacilityRole role,
        GridPathSearchResult searchResult)
    {
        if (candidates == null || candidates.Count == 0)
        {
            return null;
        }

        BuildableObject bestBuilding = null;
        float bestScore = float.MinValue;
        int bestId = int.MaxValue;
        foreach (BuildableObject building in candidates)
        {
            if (building == null)
            {
                continue;
            }

            float score = ScoreCandidate(character, building, role, searchResult);
            if (bestBuilding == null
                || score > bestScore
                || (Mathf.Approximately(score, bestScore) && building.id < bestId))
            {
                bestBuilding = building;
                bestScore = score;
                bestId = building.id;
            }
        }

        return bestBuilding;
    }

    public static bool HasCandidate(
        Character character,
        GridPathSearchResult searchResult,
        FacilityRole role)
    {
        if (role == FacilityRole.None)
        {
            return false;
        }

        foreach (BuildableObject building in GetCandidateSource(character, searchResult, role))
        {
            if (IsReachableCandidate(character, searchResult, building, role))
            {
                return true;
            }
        }

        return false;
    }

    public static bool TrySelectBest(
        Character character,
        GridPathSearchResult searchResult,
        FacilityRole role,
        out BuildableObject bestBuilding)
    {
        bestBuilding = null;
        if (role == FacilityRole.None)
        {
            return false;
        }

        float bestScore = float.MinValue;
        int bestId = int.MaxValue;
        foreach (BuildableObject building in GetCandidateSource(character, searchResult, role))
        {
            if (!IsReachableCandidate(character, searchResult, building, role))
            {
                continue;
            }

            float score = ScoreCandidate(character, building, role, searchResult);
            if (bestBuilding == null
                || score > bestScore
                || (Mathf.Approximately(score, bestScore) && building.id < bestId))
            {
                bestBuilding = building;
                bestScore = score;
                bestId = building.id;
            }
        }

        return bestBuilding != null;
    }

    public static bool IsCandidate(
        Character character,
        BuildableObject building,
        FacilityRole role,
        out string rejectReason)
    {
        rejectReason = string.Empty;
        if (building == null)
        {
            rejectReason = "시설 없음";
            return false;
        }

        if (!building.SupportsFacilityRole(role))
        {
            rejectReason = "역할 불일치";
            return false;
        }

        if (!building.CanVisit(character, out rejectReason))
        {
            return false;
        }

        if (building is Shop shop
            && character != null
            && character.TryGetAbility(out AbilityShopping shopping)
            && !shopping.CanBuyFrom(shop, out rejectReason))
        {
            return false;
        }

        return true;
    }

    private static IEnumerable<BuildableObject> GetCandidateSource(
        Character character,
        GridPathSearchResult searchResult,
        FacilityRole role)
    {
        if (searchResult != null)
        {
            return FacilityCandidateCache.GetCandidates(searchResult.sourceGrid, role);
        }

        if (character != null)
        {
            return character.GetReachableBuilding();
        }

        return System.Array.Empty<BuildableObject>();
    }

    private static bool IsReachableCandidate(
        Character character,
        GridPathSearchResult searchResult,
        BuildableObject building,
        FacilityRole role)
    {
        if (searchResult != null && !searchResult.ContainsVisitableOccupant(building))
        {
            return false;
        }

        return IsCandidate(character, building, role, out _);
    }

    public static float ScoreCandidate(
        Character character,
        BuildableObject building,
        FacilityRole role,
        GridPathSearchResult searchResult)
    {
        if (!IsCandidate(character, building, role, out _))
        {
            return 0f;
        }

        FacilityRole matchedRole = GetBestMatchedRole(character, building, role);
        float desireScore = GetNeedScore(character, matchedRole);
        float preferenceScore = GetPreferenceScore(character, building, matchedRole);
        float stockScore = GetStockScore(building);
        float affordabilityScore = GetAffordabilityScore(character, building);
        float crowdScore = GetCrowdScore(character, building);
        float distanceScore = GetDistanceScore(building, searchResult);
        float noveltyScore = GetNoveltyScore(character, building);

        float score =
            desireScore * 0.35f
            + preferenceScore * 0.2f
            + stockScore * 0.15f
            + affordabilityScore * 0.1f
            + crowdScore * 0.1f
            + distanceScore * 0.07f
            + noveltyScore * 0.03f;

        return Mathf.Clamp01(score);
    }

    public static float GetNeedScore(Character character, FacilityRole role)
    {
        if (HasMultipleRoles(role))
        {
            float highestNeed = 0f;
            foreach (FacilityRole scoredRole in ScoredRoles)
            {
                if ((role & scoredRole) == 0) continue;

                highestNeed = Mathf.Max(highestNeed, GetNeedScore(character, scoredRole));
            }

            return highestNeed;
        }

        if (character == null || character.stats == null)
        {
            return 0.5f;
        }

        return role switch
        {
            FacilityRole.Meal => GetLowStatNeed(character, Character.Condition.HUNGER),
            FacilityRole.Purchase => Mathf.Max(
                GetLowStatNeed(character, Character.Condition.FUN),
                GetLowStatNeed(character, Character.Condition.MOOD) * 0.6f),
            FacilityRole.Rest => Mathf.Max(
                GetLowStatNeed(character, Character.Condition.SLEEP),
                GetLowStatNeed(character, Character.Condition.MOOD) * 0.4f),
            FacilityRole.Training => GetLowStatNeed(character, Character.Condition.FUN),
            FacilityRole.Research => GetLowStatNeed(character, Character.Condition.FUN),
            FacilityRole.Mana => GetLowStatNeed(character, Character.Condition.MOOD),
            _ => 0.5f
        };
    }

    private static FacilityRole GetBestMatchedRole(
        Character character,
        BuildableObject building,
        FacilityRole requestedRoles)
    {
        if (building == null || requestedRoles == FacilityRole.None)
        {
            return requestedRoles;
        }

        FacilityRole bestRole = FacilityRole.None;
        float highestNeed = float.MinValue;
        foreach (FacilityRole role in ScoredRoles)
        {
            if ((requestedRoles & role) == 0 || !building.SupportsFacilityRole(role))
            {
                continue;
            }

            float need = GetNeedScore(character, role);
            if (need > highestNeed)
            {
                highestNeed = need;
                bestRole = role;
            }
        }

        return bestRole != FacilityRole.None ? bestRole : requestedRoles;
    }

    private static bool HasMultipleRoles(FacilityRole role)
    {
        int value = (int)role;
        return value != 0 && (value & (value - 1)) != 0;
    }

    private static float GetLowStatNeed(Character character, Character.Condition condition)
    {
        if (character.stats == null || !character.stats.TryGetValue(condition, out float value))
        {
            return 0.5f;
        }

        return Mathf.Clamp01(1f - (value / 100f));
    }

    private static float GetPreferenceScore(
        Character character,
        BuildableObject building,
        FacilityRole matchedRole)
    {
        float speciesTagPreferenceScore = GetSpeciesTagPreferenceScore(character, building);
        float modelPreferenceScore = GetCharacterModelPreferenceScore(character, building, matchedRole);
        return Mathf.Clamp01((speciesTagPreferenceScore + modelPreferenceScore) * 0.5f);
    }

    private static float GetSpeciesTagPreferenceScore(Character character, BuildableObject building)
    {
        string speciesTag = character != null ? character.SpeciesTag : string.Empty;
        if (string.IsNullOrWhiteSpace(speciesTag) || building.Facility == null)
        {
            return 0.5f;
        }

        if (building.Facility.dislikedSpeciesTags != null
            && System.Array.IndexOf(building.Facility.dislikedSpeciesTags, speciesTag) >= 0)
        {
            return 0.1f;
        }

        if (building.Facility.preferredSpeciesTags != null
            && System.Array.IndexOf(building.Facility.preferredSpeciesTags, speciesTag) >= 0)
        {
            return 1f;
        }

        return 0.5f;
    }

    private static float GetCharacterModelPreferenceScore(
        Character character,
        BuildableObject building,
        FacilityRole matchedRole)
    {
        if (character == null || building == null || building.Facility == null)
        {
            return 0.5f;
        }

        FacilityRole roles = matchedRole != FacilityRole.None
            ? matchedRole
            : building.Facility.roles;
        return character.GetFacilityPreferenceScore(roles);
    }

    private static float GetStockScore(BuildableObject building)
    {
        if (building.Facility == null || !building.Facility.requiresStock)
        {
            return 1f;
        }

        if (building is not IStockedFacility stockedFacility)
        {
            return 0f;
        }

        int max = Mathf.Max(1, building.Facility.internalStockMax);
        return Mathf.Clamp01((float)stockedFacility.CurrentStock / max);
    }

    private static float GetAffordabilityScore(Character character, BuildableObject building)
    {
        if (building is not Shop shop)
        {
            return 1f;
        }

        if (character == null || !character.TryGetAbility(out AbilityShopping shopping))
        {
            return 1f;
        }

        return shopping.GetAffordabilityScore(shop);
    }

    private static float GetCrowdScore(Character character, BuildableObject building)
    {
        if (building.Facility == null || building.Facility.capacity <= 0)
        {
            return 1f;
        }

        float sensitivity = character != null ? character.GetCrowdSensitivityMultiplier() : 1f;
        return Mathf.Clamp01(1f - (((float)building.CurrentUserCount / building.Facility.capacity) * sensitivity));
    }

    private static float GetDistanceScore(BuildableObject building, GridPathSearchResult searchResult)
    {
        if (building == null || searchResult == null)
        {
            return 0.5f;
        }

        int distance = searchResult.GetMoveDistanceTo(building);
        if (distance == int.MaxValue)
        {
            return 0f;
        }

        return 1f / (1f + distance);
    }

    private static float GetNoveltyScore(Character character, BuildableObject building)
    {
        if (character == null || !character.TryGetAbility(out AbilityShopping shopping))
        {
            return 1f;
        }

        return shopping.visitedBuilding.Contains(building) ? 0.2f : 1f;
    }
}
