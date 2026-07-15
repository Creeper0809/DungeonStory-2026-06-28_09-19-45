using System.Collections.Generic;
using System.Linq;
using System;

public sealed class OffenseRewardDebugContext
{
    public GameData gameData;
    public IEnumerable<IWarehouseFacility> warehouses;
    public FacilityShopUnlockState shopUnlockState;
    public BlueprintResearchState researchState;

    public void Clear()
    {
        gameData = null;
        warehouses = null;
        shopUnlockState = null;
        researchState = null;
    }
}

public interface IOffenseRewardContextBuilder
{
    OffenseRewardContext Create(
        OffenseTargetDefinition target,
        OffenseRewardState state,
        OffenseRewardDebugContext debugContext);
}

public sealed class OffenseRewardContextBuilder : IOffenseRewardContextBuilder
{
    private readonly IDungeonSceneComponentQuery sceneQuery;

    public OffenseRewardContextBuilder(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery
            ?? throw new ArgumentNullException(nameof(sceneQuery));
    }

    public OffenseRewardContext Create(
        OffenseTargetDefinition target,
        OffenseRewardState state,
        OffenseRewardDebugContext debugContext)
    {
        BlueprintResearchRuntime researchRuntime = sceneQuery.First<BlueprintResearchRuntime>(includeInactive: true);
        DailyFacilityShopRuntime shopRuntime = sceneQuery.First<DailyFacilityShopRuntime>(includeInactive: true);
        GameManager gameManager = sceneQuery.First<GameManager>(includeInactive: true);
        GameData gameData = debugContext?.gameData != null
            ? debugContext.gameData
            : gameManager != null
                ? gameManager.gameData
                : null;
        FacilityShopUnlockState shopUnlockState = debugContext?.shopUnlockState != null
            ? debugContext.shopUnlockState
            : shopRuntime != null
                ? shopRuntime.UnlockState
                : researchRuntime != null
                    ? researchRuntime.ShopUnlockState
                    : null;

        return new OffenseRewardContext
        {
            gameData = gameData,
            warehouses = debugContext?.warehouses ?? ResolveWarehouses(),
            shopUnlockState = shopUnlockState,
            researchState = debugContext?.researchState ?? researchRuntime?.State,
            researchRuntime = debugContext?.researchState == null ? researchRuntime : null,
            rewardState = state,
            target = target
        };
    }

    private IEnumerable<IWarehouseFacility> ResolveWarehouses()
    {
        return sceneQuery.All<BuildableObject>()
            .OfType<IWarehouseFacility>()
            .Where((warehouse) => warehouse != null && warehouse.HasWarehouseInventory);
    }
}
