using System;
using System.Collections.Generic;
using System.Linq;

public interface IFeatureSurfaceTabPresenter
{
    TabId Id { get; }
    void Present(P0FeatureSurfacePanel surface);
}

public interface IFeatureSurfaceTabPresenterRegistry
{
    bool TryGet(TabId id, out IFeatureSurfaceTabPresenter presenter);
}

public sealed class FeatureSurfaceTabPresenterRegistry : IFeatureSurfaceTabPresenterRegistry
{
    private readonly IReadOnlyDictionary<TabId, IFeatureSurfaceTabPresenter> presenters;

    public FeatureSurfaceTabPresenterRegistry(IEnumerable<IFeatureSurfaceTabPresenter> presenters)
    {
        IFeatureSurfaceTabPresenter[] registered = presenters?.ToArray()
            ?? throw new ArgumentNullException(nameof(presenters));
        IGrouping<TabId, IFeatureSurfaceTabPresenter> duplicate = registered
            .GroupBy((presenter) => presenter.Id)
            .FirstOrDefault((group) => group.Count() > 1);
        if (duplicate != null)
        {
            throw new InvalidOperationException($"Multiple feature surface presenters are registered for {duplicate.Key}.");
        }

        this.presenters = registered.ToDictionary((presenter) => presenter.Id);
        TabId[] missing = UITabCatalog.All
            .Where((definition) => definition.SurfaceKind == UITabSurfaceKind.Feature)
            .Select((definition) => definition.Id)
            .Where((id) => !this.presenters.ContainsKey(id))
            .ToArray();
        if (missing.Length > 0)
        {
            throw new InvalidOperationException(
                $"Feature surface presenters are missing for: {string.Join(", ", missing)}.");
        }
    }

    public bool TryGet(TabId id, out IFeatureSurfaceTabPresenter presenter)
    {
        return presenters.TryGetValue(id, out presenter);
    }
}

public sealed class BuildingFeatureSurfacePresenter : IFeatureSurfaceTabPresenter
{
    public TabId Id => TabId.Buildings;
    public void Present(P0FeatureSurfacePanel surface) => surface.BuildFacilitiesManagement();
}

public sealed class ShopFeatureSurfacePresenter : IFeatureSurfaceTabPresenter
{
    public TabId Id => TabId.Shop;
    public void Present(P0FeatureSurfacePanel surface) => surface.BuildFacilityShop();
}

public sealed class WarehouseFeatureSurfacePresenter : IFeatureSurfaceTabPresenter
{
    public TabId Id => TabId.Warehouse;
    public void Present(P0FeatureSurfacePanel surface) => surface.BuildWarehouse();
}

public sealed class OperationsFeatureSurfacePresenter : IFeatureSurfaceTabPresenter
{
    public TabId Id => TabId.Operations;
    public void Present(P0FeatureSurfacePanel surface) => surface.BuildOperationHub();
}

public sealed class DefenseFeatureSurfacePresenter : IFeatureSurfaceTabPresenter
{
    public TabId Id => TabId.Defense;
    public void Present(P0FeatureSurfacePanel surface) => surface.BuildDefenseOperations();
}

public sealed class ExpeditionFeatureSurfacePresenter : IFeatureSurfaceTabPresenter
{
    public TabId Id => TabId.Expedition;
    public void Present(P0FeatureSurfacePanel surface) => surface.BuildOffenseOperations();
}

public sealed class ResearchFeatureSurfacePresenter : IFeatureSurfaceTabPresenter
{
    public TabId Id => TabId.Research;
    public void Present(P0FeatureSurfacePanel surface) => surface.BuildResearch();
}

public sealed class CodexFeatureSurfacePresenter : IFeatureSurfaceTabPresenter
{
    public TabId Id => TabId.Codex;
    public void Present(P0FeatureSurfacePanel surface) => surface.BuildCodexAndHistory();
}
