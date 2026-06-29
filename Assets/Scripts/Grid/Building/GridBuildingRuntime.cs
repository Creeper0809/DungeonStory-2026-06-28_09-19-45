using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class InitialBuildInfo
{
    public Vector2Int Position;
    public BuildingSO Building;
}

public class GridBuildingPlacementService
{
    private Grid grid;
    private readonly BuildingSO hallwayBuilding;
    private readonly Func<int, BuildingSO> findBuildingData;
    private readonly GridBuildingFactory buildingFactory;
    private readonly BuildingPlacementValidator placementValidator;

    public GridBuildingPlacementService(Grid grid, BuildingSO hallwayBuilding)
        : this(grid, hallwayBuilding, null)
    {
    }

    public GridBuildingPlacementService(Grid grid, BuildingSO hallwayBuilding, Func<int, BuildingSO> findBuildingData)
        : this(grid, hallwayBuilding, findBuildingData, new GridBuildingFactory(), new BuildingPlacementValidator())
    {
    }

    public GridBuildingPlacementService(
        Grid grid,
        BuildingSO hallwayBuilding,
        Func<int, BuildingSO> findBuildingData,
        GridBuildingFactory buildingFactory,
        BuildingPlacementValidator placementValidator)
    {
        this.grid = grid;
        this.hallwayBuilding = hallwayBuilding;
        this.findBuildingData = findBuildingData;
        this.buildingFactory = buildingFactory ?? new GridBuildingFactory();
        this.placementValidator = placementValidator ?? new BuildingPlacementValidator();
    }

    public void SetGrid(Grid grid)
    {
        this.grid = grid;
    }

    public bool TryPlaceBuilding(BuildingSO buildingData, Vector2Int position, out string errorMessage)
    {
        if (!CanPlaceBuilding(buildingData, position, out errorMessage))
        {
            return false;
        }

        if (!PlaceBuildingWithoutValidation(buildingData, position, out errorMessage))
        {
            return false;
        }

        EnsureHallwayUnderBuilding(buildingData, position);
        placementValidator.ApplyBuildSuccess(buildingData);
        return true;
    }

    public bool CanPlaceBuilding(BuildingSO buildingData, Vector2Int position)
    {
        return CanPlaceBuilding(buildingData, position, out _);
    }

    public bool CanPlaceBuilding(BuildingSO buildingData, Vector2Int position, out string errorMessage)
    {
        return placementValidator.CanBuild(grid, buildingData, position, out errorMessage);
    }

    public bool TryDestroyBuilding(BuildableObject building, out BuildingSO buildingData, out string errorMessage)
    {
        buildingData = null;

        if (grid == null)
        {
            errorMessage = "그리드가 초기화되지 않았습니다";
            return false;
        }

        if (building == null)
        {
            errorMessage = "삭제할 건물이 없습니다";
            return false;
        }

        buildingData = findBuildingData?.Invoke(building.id);
        if (buildingData == null)
        {
            errorMessage = "건물 데이터를 찾을 수 없습니다";
            return false;
        }

        if (!placementValidator.CanDestroy(grid, buildingData, building, out errorMessage))
        {
            return false;
        }

        grid.RemoveOccupant(buildingData.Placement.Layer, building.buildPoses, buildingData.Placement.IsMovement);
        buildingFactory.DeleteVisual(buildingData, building.centerPos);
        building.DestroySelf();
        return true;
    }

    public void PlaceInitialBuildings(IEnumerable<InitialBuildInfo> initialPlacement)
    {
        if (initialPlacement == null) return;

        foreach (InitialBuildInfo item in initialPlacement)
        {
            if (item == null) continue;

            PlaceBuildingWithoutValidation(item.Building, item.Position, out _);
        }
    }

    private void EnsureHallwayUnderBuilding(BuildingSO buildingData, Vector2Int position)
    {
        if (buildingData.Placement.Layer == GridLayer.Hallway || hallwayBuilding == null) return;

        foreach (Vector2Int gridPos in buildingData.GetGridPosList(position))
        {
            GridCell cell = grid.GetGridCell(gridPos);
            if (cell == null || cell.HasBuildingInLayer(GridLayer.Hallway)) continue;

            PlaceBuildingWithoutValidation(hallwayBuilding, gridPos, out _);
        }
    }

    private bool PlaceBuildingWithoutValidation(BuildingSO buildingData, Vector2Int position, out string errorMessage)
    {
        BuildableObject buildableObject = buildingFactory.Create(grid, buildingData, position);
        if (buildableObject == null)
        {
            errorMessage = buildingData != null
                ? $"{buildingData.objectName} 생성에 실패했습니다"
                : "건물 생성에 실패했습니다";
            return false;
        }

        buildableObject.SetGrid(grid);
        buildableObject.Initialization(buildingData, position);
        bool registered = grid.RegisterOccupant(
            buildableObject,
            buildingData.Placement.Layer,
            buildingData.GetGridPosList(position),
            buildingData.Placement.IsMovement);
        if (!registered)
        {
            buildingFactory.DeleteVisual(buildingData, position);
            buildableObject.DestroySelf();
            errorMessage = $"{buildingData.objectName} 그리드 등록에 실패했습니다";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}

public interface IGridBuildingVisual
{
    void DrawBuilding(BuildingSO buildingData, Vector2Int position);
    void DeleteBuilding(BuildingSO buildingData, Vector2Int position);
}

public class GridBuildingFactory
{
    private readonly Action<BuildableObject> onBuildingCreated;
    private readonly IGridBuildingVisual buildingVisual;

    public GridBuildingFactory(Action<BuildableObject> onBuildingCreated = null)
        : this(null, onBuildingCreated)
    {
    }

    public GridBuildingFactory(IGridBuildingVisual buildingVisual, Action<BuildableObject> onBuildingCreated = null)
    {
        this.buildingVisual = buildingVisual;
        this.onBuildingCreated = onBuildingCreated;
    }

    public BuildableObject Create(Grid grid, BuildingSO buildingData, Vector2Int selectPos)
    {
        if (grid == null || buildingData == null) return null;

        GridBuildingPlacement placement = buildingData.Placement;
        Vector3 evenOffset = new Vector2(0.5f, 0);
        Vector2 instantiatePos = placement.HasEvenWidth
            ? grid.GetWorldPos(selectPos) + evenOffset
            : grid.GetWorldPos(selectPos);

        GameObject placedObject = new GameObject();
        placedObject.name = buildingData.objectName;
        placedObject.transform.position = instantiatePos;

        if (!placement.IsWall)
        {
            BoxCollider2D collider2D = placedObject.AddComponent<BoxCollider2D>();
            collider2D.size = new Vector2(placement.Width, placement.Height * 2.9f);
            collider2D.offset = new Vector2(0, 1.5f);

            placedObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        }

        buildingVisual?.DrawBuilding(buildingData, selectPos);

        if (placedObject.AddComponent(buildingData.type) is BuildableObject buildableObject)
        {
            onBuildingCreated?.Invoke(buildableObject);
            return buildableObject;
        }

        Debug.Log("건물 데이터에 원치 않은 컴퍼넌트가 연결되었습니다");
        UnityEngine.Object.Destroy(placedObject);
        return null;
    }

    public void DeleteVisual(BuildingSO buildingData, Vector2Int selectPos)
    {
        if (buildingData == null) return;

        buildingVisual?.DeleteBuilding(buildingData, selectPos);
    }
}

public class BuildingPlacementValidator
{
    private readonly GridPlacementValidator gridPlacementValidator;

    public BuildingPlacementValidator()
        : this(new GridPlacementValidator())
    {
    }

    public BuildingPlacementValidator(GridPlacementValidator gridPlacementValidator)
    {
        this.gridPlacementValidator = gridPlacementValidator ?? new GridPlacementValidator();
    }

    public bool CanBuild(Grid grid, BuildingSO buildingData, Vector2Int buildPos, out string errorMessage)
    {
        if (grid == null)
        {
            errorMessage = "그리드가 초기화되지 않았습니다";
            return false;
        }

        if (buildingData == null)
        {
            errorMessage = "설치할 건물이 선택되지 않았습니다";
            return false;
        }

        List<Vector2Int> totalBuildPos = buildingData.GetGridPosList(buildPos);
        if (!gridPlacementValidator.AreInsideHorizontalBounds(grid, totalBuildPos, 1))
        {
            errorMessage = "설치할 수 없는 위치입니다";
            return false;
        }

        if (!gridPlacementValidator.CanOccupy(grid, buildingData.Placement.Layer, totalBuildPos))
        {
            errorMessage = "이미 설치 된 건물이 존재합니다";
            return false;
        }

        if (!gridPlacementValidator.HasSupportBelow(grid, totalBuildPos))
        {
            errorMessage = "바닥이 없습니다.";
            return false;
        }

        foreach (IBuildingCondition condition in buildingData.BuildConditions)
        {
            if (condition != null && !condition.IsSatisfy(grid, totalBuildPos, out errorMessage))
            {
                return false;
            }
        }

        errorMessage = string.Empty;
        return true;
    }

    public bool CanDestroy(Grid grid, BuildingSO buildingData, BuildableObject building, out string errorMessage)
    {
        if (grid == null)
        {
            errorMessage = "그리드가 초기화되지 않았습니다";
            return false;
        }

        if (buildingData == null || building == null)
        {
            errorMessage = "삭제할 건물이 없습니다";
            return false;
        }

        List<Vector2Int> buildedPos = buildingData.GetGridPosList(building.centerPos);
        if (!gridPlacementValidator.CanRemoveOccupantWithoutUnsupportedAbove(grid, buildedPos, building))
        {
            errorMessage = "윗층에 건물이 존재합니다";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    public void ApplyBuildSuccess(BuildingSO buildingData)
    {
        if (buildingData == null) return;

        foreach (IBuildingCondition condition in buildingData.BuildConditions)
        {
            condition?.OnBuild();
        }
    }
}

public static class GridBuildingExtensions
{
    public static bool CanBuild(this GridCell cell, GridLayer layer = GridLayer.Building)
    {
        return cell != null && cell.CanOccupy(layer);
    }

    public static bool HasBuildingInLayer(this GridCell cell, GridLayer layer = GridLayer.Building)
    {
        return cell != null && cell.HasOccupantInLayer(layer);
    }

    public static bool HasBuilding(this GridCell cell)
    {
        return cell != null && cell.HasOccupant();
    }

    public static BuildableObject GetBuildingInlayer(this GridCell cell, GridLayer layer = GridLayer.Building)
    {
        return cell?.GetOccupant(layer) as BuildableObject;
    }

    public static BuildableObject GetBuilding(this GridCell cell)
    {
        return cell?.GetTopOccupant() as BuildableObject;
    }

    public static List<BuildableObject> GetAllBuilding(this GridCell cell)
    {
        if (cell == null) return new List<BuildableObject>();

        return cell.GetAllOccupants()
                   .OfType<BuildableObject>()
                   .ToList();
    }

    public static List<BuildableObject> GetAllVisitableBuilding(this GridPathSearchResult searchResult)
    {
        if (searchResult == null) return new List<BuildableObject>();

        return searchResult.GetAllVisitableOccupants()
                           .OfType<BuildableObject>()
                           .ToList();
    }

    public static List<BuildableObject> GetAllReachableBuilding(this GridPathSearchResult searchResult)
    {
        if (searchResult == null) return new List<BuildableObject>();

        return searchResult.GetAllReachableOccupants()
                           .OfType<BuildableObject>()
                           .ToList();
    }

    public static Queue<BuildableObject> GetPathTo(this GridPathSearchResult searchResult, BuildableObject destination)
    {
        if (searchResult == null || destination == null) return new Queue<BuildableObject>();

        return ToBuildingQueue(searchResult.GetOccupantPathTo(destination));
    }

    public static Queue<BuildableObject> GetPath(this GridPathSearchResult searchResult, Func<Vector2Int, bool> terminateEndCondition)
    {
        if (searchResult == null || terminateEndCondition == null) return new Queue<BuildableObject>();

        return ToBuildingQueue(searchResult.GetOccupantPath(terminateEndCondition));
    }

    public static Queue<BuildableObject> GetGridPath(this Grid grid, Vector2Int start, Func<Vector2Int, bool> terminateEndCondition)
    {
        if (grid == null || terminateEndCondition == null) return new Queue<BuildableObject>();

        return ToBuildingQueue(grid.GetOccupantPath(start, terminateEndCondition));
    }

    public static List<BuildableObject> GetAllVisitableBuilding(this Grid grid, Vector2Int start)
    {
        if (grid == null) return new List<BuildableObject>();

        return grid.GetAllVisitableOccupants(start)
                   .OfType<BuildableObject>()
                   .ToList();
    }

    public static List<BuildableObject> GetAllReachableBuilding(this Grid grid, Vector2Int start)
    {
        if (grid == null) return new List<BuildableObject>();

        return grid.GetAllReachableOccupants(start)
                   .OfType<BuildableObject>()
                   .ToList();
    }

    public static Queue<BuildableObject> SmoothingPath(this Grid grid, Queue<BuildableObject> gridPath)
    {
        Queue<BuildableObject> result = new Queue<BuildableObject>();
        if (gridPath == null || !gridPath.Any()) return result;

        while (gridPath.Count > 1)
        {
            BuildableObject building = gridPath.Dequeue();
            if (building != null && building.IsGridMovement)
            {
                result.Enqueue(building);
            }
        }

        result.Enqueue(gridPath.Dequeue());
        return result;
    }

    public static bool IsConneted(this Grid grid, Vector2Int start, int id)
    {
        return grid.IsConnected(start, id);
    }

    public static bool IsConnected(this Grid grid, Vector2Int start, int id)
    {
        if (grid == null) return false;

        return grid.GetOccupantPath(start, (pos) =>
        {
            GridCell cell = grid.GetGridCell(pos);
            return cell != null && cell.GetAllOccupants().Any((occupant) => occupant.GridId == id);
        }).Any();
    }

    public static List<BuildableObject> FindAllBuilding(this Grid grid, int id)
    {
        if (grid == null) return new List<BuildableObject>();

        return grid.FindAllOccupants((occupant) => occupant.GridId == id)
                   .OfType<BuildableObject>()
                   .ToList();
    }

    public static int CountBuilding(this Grid grid, BuildingSO buildingSO)
    {
        if (grid == null || buildingSO == null) return 0;

        return grid.FindAllOccupants((occupant) => occupant.GridId == buildingSO.id).Count;
    }

    private static Queue<BuildableObject> ToBuildingQueue(Queue<IGridOccupant> occupants)
    {
        Queue<BuildableObject> result = new Queue<BuildableObject>();
        if (occupants == null) return result;

        foreach (IGridOccupant occupant in occupants)
        {
            if (occupant is BuildableObject building)
            {
                result.Enqueue(building);
            }
        }

        return result;
    }
}
