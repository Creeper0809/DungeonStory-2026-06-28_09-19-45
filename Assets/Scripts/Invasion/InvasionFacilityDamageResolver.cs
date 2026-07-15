using System.Linq;
using UnityEngine;

public static class InvasionFacilityDamageResolver
{
    public static bool TryFindDamageTarget(Grid grid, Vector2Int current, out BuildableObject target)
    {
        target = null;
        if (grid == null)
        {
            return false;
        }

        Vector2Int[] positions =
        {
            current,
            current + Vector2Int.left,
            current + Vector2Int.right
        };

        foreach (Vector2Int position in positions)
        {
            GridCell cell = grid.GetGridCell(position);
            if (cell == null)
            {
                continue;
            }

            target = cell.GetAllOccupants()
                .OfType<BuildableObject>()
                .FirstOrDefault(IsDamageableFacility);
            if (target != null)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsDamageableFacility(BuildableObject building)
    {
        return building != null
            && !building.isDestroy
            && !building.IsDamaged
            && !building.IsGridMovement
            && building.Facility != null;
    }
}
