using System;

public interface IGridSystemProvider
{
    GridSystemManager Manager { get; }
    Grid Grid { get; }
    bool TryGetManager(out GridSystemManager manager);
    bool TryGetGrid(out Grid grid);
}

public sealed class GridSystemProvider : IGridSystemProvider
{
    private readonly IDungeonSceneComponentQuery sceneQuery;
    private GridSystemManager manager;

    public GridSystemProvider(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery ?? throw new ArgumentNullException(nameof(sceneQuery));
    }

    public GridSystemManager Manager
    {
        get
        {
            if (!TryGetManager(out GridSystemManager resolvedManager))
            {
                throw new InvalidOperationException($"{nameof(IGridSystemProvider)} requires a loaded {nameof(GridSystemManager)}.");
            }

            return resolvedManager;
        }
    }

    public Grid Grid
    {
        get
        {
            if (!TryGetGrid(out Grid grid))
            {
                throw new InvalidOperationException($"{nameof(GridSystemManager)} did not initialize its {nameof(Grid)}.");
            }

            return grid;
        }
    }

    public bool TryGetManager(out GridSystemManager resolvedManager)
    {
        if (manager == null)
        {
            manager = sceneQuery.First<GridSystemManager>(includeInactive: true);
        }

        if (manager == null)
        {
            resolvedManager = null;
            return false;
        }

        manager.EnsureGridInitialized();
        resolvedManager = manager;
        return true;
    }

    public bool TryGetGrid(out Grid grid)
    {
        if (!TryGetManager(out GridSystemManager resolvedManager) || resolvedManager.grid == null)
        {
            grid = null;
            return false;
        }

        grid = resolvedManager.grid;
        return true;
    }
}
