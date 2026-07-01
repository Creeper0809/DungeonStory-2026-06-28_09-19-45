using UnityEngine;

public static class WorkGridUtility
{
    public static Grid ResolveActiveGrid(
        AbilityWork work,
        GridPathSearchResult searchResult,
        Grid priorityGrid = null)
    {
        if (searchResult != null && searchResult.sourceGrid != null)
        {
            return searchResult.sourceGrid;
        }

        if (priorityGrid != null)
        {
            return priorityGrid;
        }

        if (GridSystemManager.Instance != null && GridSystemManager.Instance.grid != null)
        {
            return GridSystemManager.Instance.grid;
        }

        work?.EnsureWorkReferences();
        return work != null ? work.CachedGrid : null;
    }

    public static Vector2Int GetGridPosition(Grid activeGrid, Character actor)
    {
        if (activeGrid == null || actor == null)
        {
            return Vector2Int.zero;
        }

        Vector2Int position = activeGrid.GetXY(actor.transform.position);
        return activeGrid.IsValidGridPos(position) ? position : Vector2Int.zero;
    }
}
