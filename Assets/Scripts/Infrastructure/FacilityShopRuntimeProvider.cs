using System;
using System.Collections.Generic;
using System.Linq;

public interface IDailyFacilityShopRuntimeProvider
{
    bool TryGetRuntime(out DailyFacilityShopRuntime runtime);
}

public interface IFacilityShopCatalog
{
    IReadOnlyCollection<BuildingSO> Buildings { get; }
    IReadOnlyCollection<FacilityBlueprintSO> Blueprints { get; }
    BuildingSO FindBuildingById(int buildingId);
}

public interface IFacilityShopUnlockStateService
{
    FacilityShopUnlockState GetUnlockState();
}

public sealed class DailyFacilityShopRuntimeProvider :
    CachedSceneRuntimeProvider<DailyFacilityShopRuntime>,
    IDailyFacilityShopRuntimeProvider
{
    public DailyFacilityShopRuntimeProvider(IDungeonSceneComponentQuery sceneQuery)
        : base(sceneQuery)
    {
    }

    public bool TryGetRuntime(out DailyFacilityShopRuntime runtime)
    {
        return TryGetRuntimeComponent(out runtime);
    }
}

public sealed class FacilityShopUnlockStateService : IFacilityShopUnlockStateService
{
    private readonly IDailyFacilityShopRuntimeProvider runtimeProvider;

    public FacilityShopUnlockStateService(IDailyFacilityShopRuntimeProvider runtimeProvider)
    {
        this.runtimeProvider = runtimeProvider
            ?? throw new ArgumentNullException(nameof(runtimeProvider));
    }

    public FacilityShopUnlockState GetUnlockState()
    {
        if (!runtimeProvider.TryGetRuntime(out DailyFacilityShopRuntime runtime))
        {
            throw new InvalidOperationException($"{nameof(FacilityShopUnlockStateService)} requires a loaded {nameof(DailyFacilityShopRuntime)}.");
        }

        return runtime.UnlockState;
    }
}

public sealed class DataCatalogFacilityShopCatalog : IFacilityShopCatalog
{
    private readonly IDataCatalog catalog;

    public DataCatalogFacilityShopCatalog(IDataCatalog catalog)
    {
        this.catalog = catalog
            ?? throw new ArgumentNullException(nameof(catalog));
    }

    public IReadOnlyCollection<BuildingSO> Buildings => catalog
        .GetData<BuildingSO>()
        .Values
        .Where((building) => building != null)
        .ToArray();

    public IReadOnlyCollection<FacilityBlueprintSO> Blueprints => catalog
        .GetData<FacilityBlueprintSO>()
        .Values
        .Where((blueprint) => blueprint != null)
        .ToArray();

    public BuildingSO FindBuildingById(int buildingId)
    {
        if (buildingId < 0)
        {
            return null;
        }

        return catalog.GetData<BuildingSO>().TryGetValue(buildingId, out BuildingSO building)
            ? building
            : null;
    }
}
