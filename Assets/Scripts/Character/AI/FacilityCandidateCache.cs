using System;
using System.Collections.Generic;

public static class FacilityCandidateCache
{
    private sealed class GridFacilityCache
    {
        public int Version = -1;
        public int StateVersion = -1;
        public readonly Dictionary<FacilityRole, List<BuildableObject>> CandidatesByRole =
            new Dictionary<FacilityRole, List<BuildableObject>>();
    }

    private static readonly Dictionary<Grid, GridFacilityCache> CacheByGrid =
        new Dictionary<Grid, GridFacilityCache>();
    private static int facilityStateVersion;

    public static IReadOnlyList<BuildableObject> GetCandidates(Grid grid, FacilityRole role)
    {
        if (grid == null || role == FacilityRole.None)
        {
            return Array.Empty<BuildableObject>();
        }

        GridFacilityCache cache = GetCache(grid);
        if (IsSingleRole(role))
        {
            return GetSingleRoleCandidates(grid, cache, role);
        }

        List<BuildableObject> merged = new List<BuildableObject>();
        foreach (FacilityRole singleRole in GetSingleRoles(role))
        {
            foreach (BuildableObject building in GetSingleRoleCandidates(grid, cache, singleRole))
            {
                if (building != null && !merged.Contains(building))
                {
                    merged.Add(building);
                }
            }
        }

        return merged;
    }

    public static void MarkDynamicStateDirty()
    {
        unchecked
        {
            facilityStateVersion++;
        }
    }

    public static void Clear()
    {
        CacheByGrid.Clear();
        MarkDynamicStateDirty();
    }

    private static GridFacilityCache GetCache(Grid grid)
    {
        if (!CacheByGrid.TryGetValue(grid, out GridFacilityCache cache))
        {
            cache = new GridFacilityCache();
            CacheByGrid[grid] = cache;
        }

        if (cache.Version != grid.version || cache.StateVersion != facilityStateVersion)
        {
            cache.Version = grid.version;
            cache.StateVersion = facilityStateVersion;
            cache.CandidatesByRole.Clear();
        }

        return cache;
    }

    private static List<BuildableObject> GetSingleRoleCandidates(
        Grid grid,
        GridFacilityCache cache,
        FacilityRole role)
    {
        if (!cache.CandidatesByRole.TryGetValue(role, out List<BuildableObject> candidates))
        {
            candidates = new List<BuildableObject>();
            foreach (IGridOccupant occupant in grid.FindAllOccupants(null))
            {
                if (occupant is BuildableObject building
                    && !building.isDestroy
                    && building.SupportsFacilityRole(role))
                {
                    candidates.Add(building);
                }
            }

            cache.CandidatesByRole[role] = candidates;
        }

        return candidates;
    }

    private static bool IsSingleRole(FacilityRole role)
    {
        int value = (int)role;
        return value != 0 && (value & (value - 1)) == 0;
    }

    private static IEnumerable<FacilityRole> GetSingleRoles(FacilityRole roles)
    {
        foreach (FacilityRole role in Enum.GetValues(typeof(FacilityRole)))
        {
            if (role != FacilityRole.None && (roles & role) != 0)
            {
                yield return role;
            }
        }
    }
}
