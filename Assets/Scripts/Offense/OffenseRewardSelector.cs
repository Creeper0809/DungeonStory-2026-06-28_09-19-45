using System;
using System.Collections.Generic;
using System.Linq;

public sealed class OffenseRewardSelector : IOffenseRewardSelector
{
    private readonly IOffenseRewardCatalog catalog;

    public OffenseRewardSelector(IOffenseRewardCatalog catalog)
    {
        this.catalog = catalog
            ?? throw new ArgumentNullException(nameof(catalog));
    }

    public StockCategory ResolveStockCategory(OffenseRewardPreview reward)
    {
        string label = reward.label ?? string.Empty;
        if (ContainsAny(label, "식재료", "음식", "Food"))
        {
            return StockCategory.Food;
        }

        if (ContainsAny(label, "무기", "Weapon"))
        {
            return StockCategory.Weapon;
        }

        if (ContainsAny(label, "마력", "Mana"))
        {
            return StockCategory.Mana;
        }

        return StockCategory.General;
    }

    public BuildingSO SelectRareFacility(
        OffenseRewardContext context,
        IReadOnlyCollection<int> additionallyExcludedBuildingIds)
    {
        HashSet<int> alreadyGranted = context.rewardState != null
            ? context.rewardState.RareFacilityBuildingIds.ToHashSet()
            : new HashSet<int>();
        if (additionallyExcludedBuildingIds != null)
        {
            foreach (int buildingId in additionallyExcludedBuildingIds)
            {
                alreadyGranted.Add(buildingId);
            }
        }

        return catalog.Buildings
            .Where((building) => building != null
                && !building.IsGridMovement
                && !building.IsWall
                && FacilityShopService.GetBuildingStar(building) >= 2
                && !alreadyGranted.Contains(building.id))
            .OrderBy((building) => FacilityShopService.GetBuildingStar(building))
            .ThenBy((building) => building.id)
            .FirstOrDefault();
    }

    public FacilityBlueprintSO SelectBlueprint(
        OffenseRewardPreview reward,
        OffenseRewardContext context)
    {
        IEnumerable<FacilityBlueprintSO> blueprints = catalog.Blueprints
            .Where((blueprint) => blueprint != null);

        if (ContainsAny(reward.label, "방어"))
        {
            blueprints = blueprints.Where((blueprint) => ContainsAny(blueprint.DisplayName, "방어", "함정")
                || ContainsAny(blueprint.description, "방어", "함정"));
        }
        else if (ContainsAny(reward.label, "특수", "희귀"))
        {
            blueprints = blueprints.Where((blueprint) => blueprint.rarity != FacilityShopRarity.Common);
        }

        HashSet<int> acquired = context.rewardState != null
            ? context.rewardState.AcquiredBlueprintIds.ToHashSet()
            : new HashSet<int>();

        return blueprints
            .Where((blueprint) => !acquired.Contains(blueprint.id))
            .Where((blueprint) => context.shopUnlockState == null || !context.shopUnlockState.IsBlueprintAcquired(blueprint))
            .Where((blueprint) => context.researchState == null || !context.researchState.IsCompleted(blueprint))
            .OrderByDescending((blueprint) => blueprint.rarity)
            .ThenBy((blueprint) => blueprint.id)
            .FirstOrDefault();
    }

    public bool IsHumanFactionWeakening(OffenseRewardPreview reward, OffenseTargetDefinition target)
    {
        if (ContainsAny(reward.label, "인간", "Human"))
        {
            return true;
        }

        if (ContainsAny(reward.label, "경쟁", "Rival"))
        {
            return false;
        }

        return target == null || target.kind != OffenseTargetKind.RivalDungeon;
    }

    public bool ContainsAny(string source, params string[] values)
    {
        if (string.IsNullOrWhiteSpace(source) || values == null)
        {
            return false;
        }

        return values.Any((value) => !string.IsNullOrWhiteSpace(value)
            && source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);
    }
}
