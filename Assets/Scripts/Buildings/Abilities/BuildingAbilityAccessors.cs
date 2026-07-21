using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BuildingAbilityAccessors
{
    public static int GetConstructionCost(this BuildingSO building)
    {
        BuildingEconomyAbility ability = building != null
            ? building.GetAbility<BuildingEconomyAbility>()
            : null;
        if (ability != null)
        {
            return Mathf.Max(0, ability.constructionCost);
        }

        return 0;
    }

    public static int GetMaintenanceCost(this BuildingSO building)
    {
        BuildingEconomyAbility ability = building != null
            ? building.GetAbility<BuildingEconomyAbility>()
            : null;
        return ability != null ? Mathf.Max(0, ability.maintenance) : 0;
    }

    public static int GetUnlockPhase(this BuildingSO building)
    {
        BuildingEconomyAbility ability = building != null
            ? building.GetAbility<BuildingEconomyAbility>()
            : null;
        if (ability != null)
        {
            return Mathf.Clamp(ability.unlockPhase, 1, 3);
        }

        return 1;
    }

    public static float GetDemolitionRefundRate(this BuildingSO building)
    {
        BuildingEconomyAbility ability = building != null
            ? building.GetAbility<BuildingEconomyAbility>()
            : null;
        if (ability != null)
        {
            return Mathf.Clamp01(ability.demolitionRefundRate);
        }

        return 0.5f;
    }

    public static FacilityNeedRecoveryData GetNeedRecovery(this BuildingSO building)
    {
        BuildingNeedRecoveryAbility ability = building != null
            ? building.GetAbility<BuildingNeedRecoveryAbility>()
            : null;
        if (ability != null && ability.HasEffect)
        {
            return ability.recovery;
        }

        return default;
    }

    public static int GetInternalStockCapacity(this BuildingSO building)
    {
        BuildingInternalStockAbility ability = building?.GetAbility<BuildingInternalStockAbility>();
        return ability != null ? Mathf.Max(0, ability.capacity) : 0;
    }

    public static int GetRestockRequestThreshold(this BuildingSO building)
    {
        BuildingInternalStockAbility ability = building?.GetAbility<BuildingInternalStockAbility>();
        int capacity = building.GetInternalStockCapacity();
        int threshold = ability?.restockRequestThreshold ?? 0;
        return Mathf.Clamp(threshold, 0, capacity);
    }

    public static bool RequiresStockForUse(this BuildingSO building)
    {
        return building?.GetAbility<BuildingRequiresStockAbility>() != null;
    }

    public static bool RequiresStaffedService(this BuildingSO building)
    {
        return building?.GetAbility<BuildingStaffedServiceAbility>() != null;
    }

    public static bool RequiresRoomRole(this BuildingSO building)
    {
        return building?.GetAbility<BuildingRoomRequirementAbility>() != null;
    }

    public static bool IsSelfContainedRoom(this BuildingSO building)
    {
        return building?.GetAbility<BuildingSelfContainedRoomAbility>() != null;
    }

    public static bool IsPreferredSpecies(this BuildingSO building, string speciesTag)
    {
        return building?.GetAbility<BuildingSpeciesAffinityAbility>()?.IsPreferred(speciesTag) == true;
    }

    public static bool IsDislikedSpecies(this BuildingSO building, string speciesTag)
    {
        return building?.GetAbility<BuildingSpeciesAffinityAbility>()?.IsDisliked(speciesTag) == true;
    }

    public static IEnumerable<string> GetPreferredSpeciesTags(this BuildingSO building)
    {
        return building?.GetAbility<BuildingSpeciesAffinityAbility>()?.preferredTags?
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            ?? Enumerable.Empty<string>();
    }

    public static IEnumerable<string> GetDislikedSpeciesTags(this BuildingSO building)
    {
        return building?.GetAbility<BuildingSpeciesAffinityAbility>()?.dislikedTags?
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            ?? Enumerable.Empty<string>();
    }

    public static int GetStorageCapacity(this BuildingSO building)
    {
        BuildingStorageAbility ability = building != null
            ? building.GetAbility<BuildingStorageAbility>()
            : null;
        if (ability != null)
        {
            return Mathf.Max(0, ability.capacity);
        }

        return 0;
    }

    public static StockCategory GetStorageCategory(this BuildingSO building)
    {
        BuildingStorageAbility ability = building != null
            ? building.GetAbility<BuildingStorageAbility>()
            : null;
        return ability != null ? ability.category : StockCategory.General;
    }

    public static bool StoresAllCategories(this BuildingSO building)
    {
        BuildingStorageAbility ability = building != null
            ? building.GetAbility<BuildingStorageAbility>()
            : null;
        if (ability != null)
        {
            return ability.allCategories;
        }

        return false;
    }

    public static int GetSeatCapacity(this BuildingSO building)
    {
        BuildingSeatingAbility ability = building != null
            ? building.GetAbility<BuildingSeatingAbility>()
            : null;
        return ability != null ? Mathf.Max(0, ability.capacity) : 0;
    }

    public static int GetTableCapacity(this BuildingSO building)
    {
        BuildingTableAbility ability = building != null
            ? building.GetAbility<BuildingTableAbility>()
            : null;
        return ability != null ? Mathf.Max(0, ability.capacity) : 0;
    }

    public static int GetServiceCapacity(this BuildingSO building)
    {
        BuildingServiceAbility ability = building != null
            ? building.GetAbility<BuildingServiceAbility>()
            : null;
        return ability != null ? Mathf.Max(0, ability.capacity) : 0;
    }

    public static BuildingProductionAbility GetProductionAbility(this BuildingSO building)
    {
        BuildingProductionAbility ability = building != null
            ? building.GetAbility<BuildingProductionAbility>()
            : null;
        return ability != null && ability.IsValid ? ability : null;
    }

    public static int GetWorkOutputAmount(this BuildingSO building)
    {
        BuildingProductionAbility ability = building.GetProductionAbility();
        return ability != null ? Mathf.Max(0, ability.amount) : 0;
    }

    public static string GetFacilityCode(this BuildingSO building)
    {
        return building?.GetAbility<BuildingFacilityPartAbility>()?.code ?? string.Empty;
    }

    public static bool IsModularFacility(this BuildingSO building)
    {
        return !string.IsNullOrWhiteSpace(building.GetFacilityCode());
    }

    public static IEnumerable<TCapability> GetAbilityCapabilities<TCapability>(this BuildingSO building)
    {
        return building?.Abilities?.OfType<TCapability>() ?? Enumerable.Empty<TCapability>();
    }

    public static IEnumerable<StockCategory> GetStockCategorySignals(this BuildingSO building)
    {
        return building.GetAbilityCapabilities<IBuildingStockCategorySignal>()
            .SelectMany(capability => capability.GetStockCategorySignals() ?? Enumerable.Empty<StockCategory>());
    }

    public static IEnumerable<string> GetSemanticTags(this BuildingSO building)
    {
        if (building == null)
        {
            yield break;
        }

        HashSet<string> emitted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        FacilityRole roles = building.Facility?.roles ?? FacilityRole.None;
        foreach (FacilityRoleDefinition definition in FacilityRoleCatalog.Enumerate(roles))
        {
            if (!string.IsNullOrWhiteSpace(definition.SemanticTag)
                && emitted.Add(definition.SemanticTag))
            {
                yield return definition.SemanticTag;
            }
        }

        string[] explicitTags = building.GetAbility<BuildingSemanticTagsAbility>()?.tags;
        if (explicitTags == null)
        {
            yield break;
        }

        for (int index = 0; index < explicitTags.Length; index++)
        {
            string tag = explicitTags[index]?.Trim();
            if (!string.IsNullOrWhiteSpace(tag) && emitted.Add(tag))
            {
                yield return tag;
            }
        }
    }

    public static bool HasSemanticTag(this BuildingSO building, string tag)
    {
        if (building == null || string.IsNullOrWhiteSpace(tag))
        {
            return false;
        }

        string normalized = tag.Trim();
        return building.GetSemanticTags().Any(candidate =>
            string.Equals(candidate, normalized, StringComparison.OrdinalIgnoreCase));
    }

    public static string GetPrimarySemanticTag(this BuildingSO building)
    {
        return building.GetSemanticTags().FirstOrDefault() ?? string.Empty;
    }

    public static bool HasSemanticTag(this BuildableObject building, string tag)
    {
        return building?.BuildingData.HasSemanticTag(tag) == true;
    }

    public static int GetConstructionCost(this BuildableObject building)
    {
        return building?.BuildingData.GetConstructionCost() ?? 0;
    }

    public static int GetStorageCapacity(this BuildableObject building)
    {
        return building?.BuildingData.GetStorageCapacity() ?? 0;
    }

    public static int GetInternalStockCapacity(this BuildableObject building)
    {
        return building?.BuildingData.GetInternalStockCapacity() ?? 0;
    }

    public static int GetRestockRequestThreshold(this BuildableObject building)
    {
        return building?.BuildingData.GetRestockRequestThreshold() ?? 0;
    }

    public static StockCategory GetStorageCategory(this BuildableObject building)
    {
        return building?.BuildingData.GetStorageCategory() ?? StockCategory.General;
    }

    public static bool StoresAllCategories(this BuildableObject building)
    {
        return building?.BuildingData.StoresAllCategories() == true;
    }

    public static int GetSeatCapacity(this BuildableObject building)
    {
        return building?.BuildingData.GetSeatCapacity() ?? 0;
    }

    public static int GetTableCapacity(this BuildableObject building)
    {
        return building?.BuildingData.GetTableCapacity() ?? 0;
    }

    public static int GetServiceCapacity(this BuildableObject building)
    {
        return building?.BuildingData.GetServiceCapacity() ?? 0;
    }
}
