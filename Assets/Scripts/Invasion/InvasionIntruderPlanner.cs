using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class InvasionIntruderPlanner
{
    public static float CalculateFocus(float elapsedSeconds, float secondsToFullFocus)
    {
        return Mathf.Clamp01(elapsedSeconds / Mathf.Max(0.1f, secondsToFullFocus));
    }

    public static Queue<GridMoveStep> GetNextPath(
        Grid grid,
        Vector2Int start,
        Vector2Int ownerPosition,
        float focus,
        out bool directPath)
    {
        directPath = false;
        if (grid == null || !grid.IsValidGridPos(start) || !grid.IsValidGridPos(ownerPosition))
        {
            return new Queue<GridMoveStep>();
        }

        if (start == ownerPosition)
        {
            directPath = true;
            return new Queue<GridMoveStep>();
        }

        if (focus >= 0.95f)
        {
            directPath = true;
            return grid.GetMovePath(start, (pos) => pos == ownerPosition);
        }

        GridPathSearchResult searchResult = grid.SearchPath(start);
        Vector2Int exploreTarget = SelectExploreTarget(grid, searchResult, ownerPosition, focus);
        if (exploreTarget == start)
        {
            directPath = true;
            return grid.GetMovePath(start, (pos) => pos == ownerPosition);
        }

        return searchResult.GetMovePath((pos) => pos == exploreTarget);
    }

    public static Vector2Int SelectExploreTarget(
        Grid grid,
        GridPathSearchResult searchResult,
        Vector2Int ownerPosition,
        float focus)
    {
        if (grid == null || searchResult == null)
        {
            return Vector2Int.zero;
        }

        List<Vector2Int> candidates = searchResult.GetReachablePositions()
            .Where((pos) => pos != searchResult.start && grid.IsWalkable(pos))
            .ToList();

        if (candidates.Count == 0)
        {
            return searchResult.start;
        }

        if (focus <= 0.01f && candidates.Count > 1)
        {
            candidates.Remove(ownerPosition);
        }

        if (candidates.Count == 0)
        {
            return searchResult.start;
        }

        int maxDistance = Mathf.Max(1, candidates.Max((pos) => Manhattan(pos, ownerPosition)));
        float clampedFocus = Mathf.Clamp01(focus);

        return candidates
            .OrderByDescending((pos) =>
            {
                float closeness = 1f - ((float)Manhattan(pos, ownerPosition) / maxDistance);
                float explorationNoise = Random.value;
                return Mathf.Lerp(explorationNoise, closeness, clampedFocus);
            })
            .First();
    }

    public static bool IsAtOwner(Grid grid, CharacterActor intruder, CharacterActor owner)
    {
        if (grid == null || intruder == null || owner == null)
        {
            return false;
        }

        return grid.GetXY(intruder.transform.position) == grid.GetXY(owner.transform.position);
    }

    private static int Manhattan(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
