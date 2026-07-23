using System;
using System.Collections.Generic;
using UnityEngine;

public static class GridPathSearchBroker
{
    private readonly struct PathKey : IEquatable<PathKey>
    {
        private readonly int gridHash;
        private readonly int gridVersion;
        private readonly Vector2Int start;

        public PathKey(Grid grid, Vector2Int start)
        {
            gridHash = grid != null ? grid.GetHashCode() : 0;
            gridVersion = grid != null ? grid.version : -1;
            this.start = start;
        }

        public bool Equals(PathKey other)
        {
            return gridHash == other.gridHash
                && gridVersion == other.gridVersion
                && start == other.start;
        }

        public override bool Equals(object obj)
        {
            return obj is PathKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = gridHash;
                hash = (hash * 397) ^ gridVersion;
                hash = (hash * 397) ^ start.GetHashCode();
                return hash;
            }
        }
    }

    private static readonly Dictionary<PathKey, GridPathSearchResult> frameCache =
        new Dictionary<PathKey, GridPathSearchResult>();

    private static int cacheFrame = -1;

    public static int SearchesThisFrame { get; private set; }
    public static int CacheHitsThisFrame { get; private set; }
    public static int BudgetDeferralsThisFrame { get; private set; }

    public static bool TryGetSearch(
        Grid grid,
        Vector2Int start,
        Func<bool> consumeBudget,
        out GridPathSearchResult result)
    {
        BeginFrameIfNeeded();
        result = null;
        if (grid == null)
        {
            return false;
        }

        PathKey key = new PathKey(grid, start);
        if (frameCache.TryGetValue(key, out result)
            && result != null
            && result.sourceGrid == grid
            && result.start == start
            && result.gridVersion == grid.version)
        {
            CacheHitsThisFrame++;
            return true;
        }

        if (consumeBudget != null && !consumeBudget())
        {
            BudgetDeferralsThisFrame++;
            return false;
        }

        result = grid.SearchPath(start);
        frameCache[key] = result;
        SearchesThisFrame++;
        return true;
    }

    public static Queue<GridMoveStep> GetMovePath(
        Grid grid,
        Vector2Int start,
        Func<Vector2Int, bool> terminateEndCondition,
        Func<bool> consumeBudget)
    {
        return TryGetSearch(grid, start, consumeBudget, out GridPathSearchResult search)
            ? search.GetMovePath(terminateEndCondition)
            : null;
    }

    public static void Clear()
    {
        frameCache.Clear();
        cacheFrame = -1;
        SearchesThisFrame = 0;
        CacheHitsThisFrame = 0;
        BudgetDeferralsThisFrame = 0;
    }

    private static void BeginFrameIfNeeded()
    {
        int frame = Time.frameCount;
        if (cacheFrame == frame)
        {
            return;
        }

        cacheFrame = frame;
        frameCache.Clear();
        SearchesThisFrame = 0;
        CacheHitsThisFrame = 0;
        BudgetDeferralsThisFrame = 0;
    }
}
