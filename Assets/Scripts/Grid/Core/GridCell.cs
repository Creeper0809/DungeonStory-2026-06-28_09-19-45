using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridCell
{
    private readonly Dictionary<GridLayer, IGridOccupant> occupants;
    private List<GridTraversalLink> traversalLinks;
    private bool isBuildable;

    public Vector2Int Position { get; }
    public IReadOnlyList<GridTraversalLink> TraversalLinks => traversalLinks;
    public IReadOnlyList<Vector2Int> ConnectedFloor => traversalLinks.Select((link) => link.To).ToList();

    public GridCell(Vector2Int pos)
    {
        occupants = new Dictionary<GridLayer, IGridOccupant>();
        traversalLinks = new List<GridTraversalLink>();
        isBuildable = true;
        Position = pos;
    }
    public IGridOccupant GetOccupant(GridLayer layer = GridLayer.Building)
    {
        if(!HasOccupantInLayer(layer)) return null;
        return occupants[layer];
    }
    public IGridOccupant GetTopOccupant()
    {
        if (occupants.Count == 0) return null;
        var orderedBuildings = occupants.OrderByDescending(kvp => (int)kvp.Key).ToList();
        return orderedBuildings.First().Value;
    }
    public void ConnectFloor(IEnumerable<Vector2Int> poses)
    {
        traversalLinks = poses?
            .Where((pos) => pos != Position)
            .Select((pos) => new GridTraversalLink(pos, GetTopOccupant(), GridMoveType.Instant))
            .ToList() ?? new List<GridTraversalLink>();
    }
    public void SetTraversalLinks(IEnumerable<GridTraversalLink> links)
    {
        traversalLinks = links?.ToList() ?? new List<GridTraversalLink>();
    }
    public void RemoveOccupantByLayer(GridLayer layer)
    {
        if (!HasOccupantInLayer(layer)) return;
        occupants.Remove(layer);
    }
    public List<IGridOccupant> GetAllOccupants()
    {
        List<IGridOccupant> result = new List<IGridOccupant>();
        foreach (var building in occupants.Values)
        {
            result.Add(building);
        }
        return result;
    }
    public bool CanOccupy(GridLayer layer = GridLayer.Building)
    {
        return !HasOccupantInLayer(layer) && isBuildable;
    }
    public bool HasOccupantInLayer(GridLayer layer = GridLayer.Building)
    {
        return occupants.ContainsKey(layer);
    }
    public bool HasOccupant()
    {
        return occupants.Count > 0;
    }
    public bool TrySetOccupant(GridLayer layer, IGridOccupant occupant)
    {
        if (occupant == null || !CanOccupy(layer)) return false;

        occupants.Add(layer, occupant);
        return true;
    }

    public void SetOccupant(GridLayer layer,IGridOccupant occupant)
    {
        TrySetOccupant(layer, occupant);
    }

}
