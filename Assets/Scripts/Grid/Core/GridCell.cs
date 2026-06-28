using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridCell
{
    public Dictionary<GridLayer, BuildableObject> cell { get; private set;}
    public bool isbuildable;
    public Vector2Int pos;
    public List<Vector2Int> connectedFloor;
    public GridCell(Vector2Int pos)
    {
        cell = new Dictionary<GridLayer, BuildableObject>();
        connectedFloor = new List<Vector2Int>();
        isbuildable = true;
        this.pos = pos;
    }
    public BuildableObject GetBuildingInlayer(GridLayer layer = GridLayer.Building)
    {
        if(!HasBuildingInLayer()) return null;
        return cell[layer];
    }
    public BuildableObject GetBuilding()
    {
        if (cell == null || cell.Count == 0) return null;
        var orderedBuildings = cell.OrderByDescending(kvp => (int)kvp.Key).ToList();
        return orderedBuildings.First().Value;
    }
    public void ConnectFloor(List<Vector2Int> poses)
    {
        connectedFloor = poses;
    }
    public void DestroyBuildByLayer(GridLayer layer)
    {
        if (!HasBuildingInLayer(layer)) return;
        cell.Remove(layer);
    }
    public List<BuildableObject> GetAllBuilding()
    {
        List<BuildableObject> result = new List<BuildableObject>();
        foreach (var building in cell.Values)
        {
            result.Add(building);
        }
        return result;
    }
    public bool CanBuild(GridLayer layer = GridLayer.Building)
    {
        return !HasBuildingInLayer(layer) && isbuildable;
    }
    public bool HasBuildingInLayer(GridLayer layer = GridLayer.Building)
    {
        if(cell.ContainsKey(layer))
        {
            return true;
        }
        return false;
    }
    public bool HasBuilding()
    {
        return cell.Count > 0;
    }
    public void Build(GridLayer layer,BuildableObject buildableObject)
    {
        cell.Add(layer, buildableObject);
    }
}
