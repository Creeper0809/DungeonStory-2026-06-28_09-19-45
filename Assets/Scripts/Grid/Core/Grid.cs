using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Grid
{
    public int width { get; private set; }
    public int height { get; private set; }
    public Vector3 originPos { get; private set; }

    private GridCell[,] gridArray;
    public Grid(int gridWidth, int gridHeight,Vector3 originPos)
    {
        this.width = gridWidth;
        this.height = gridHeight;
        this.originPos = originPos;
        gridArray = new GridCell[gridHeight, gridWidth];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);  
                gridArray[y, x] = new GridCell(pos);
            }
        }
    }
    public Grid(int gridWidth, int gridHeight, Vector3 originPos,List<InitialBuildInfo> initialPlacement)
        :this(gridWidth, gridHeight, originPos)
    {
        foreach(InitialBuildInfo item in initialPlacement)
        {
            PlaceBuilding(item.Position, item.Building);
        }
    }
    public HashSet<Vector2Int> CheckWallsAndBuildable()
    {
        HashSet<Vector2Int> changedWallPoint = new HashSet<Vector2Int>();
        Vector2Int[] dir = { Vector2Int.left, Vector2Int.right };
        foreach(GridCell cell in gridArray)
        {
            if (cell.HasBuilding()) continue;
            foreach (var a in dir)
            {
                Vector2Int nextPos = a + cell.pos;
                if (IsValidGridPos(nextPos) && GetGridCell(nextPos).HasBuilding())
                {
                    Vector2Int floor = new Vector2Int(cell.pos.x, cell.pos.y * 3);
                    for(int y = 0; y < 3; y++)
                    {
                        changedWallPoint.Add(floor +(Vector2Int.up * y));
                    }
                    break;
                }
            }
        }
        return changedWallPoint;
    }

    public Vector2Int GetXY(Vector3 worldPosition)
    {
        int x = -Mathf.FloorToInt((worldPosition - originPos).x);
        int y = Mathf.FloorToInt((worldPosition - originPos).y) / 3;
        return new Vector2Int(x, y);
    }
    public Vector3 GetWorldPos(Vector2 gridXY)
    {
        return new Vector3(originPos.x - gridXY.x, originPos.y + (gridXY.y * 3)) + new Vector3(0.5f , 0) ;
    }
    public bool IsValidGridPos(Vector2Int gridPos)
    {
        int x = gridPos.x;
        int y = gridPos.y;
        if(x >= 0 && y >= 0 && x < width && y < height) return true;
        return false;
    }
    public GridCell GetGridCell(Vector2Int pos)
    {
        if (IsValidGridPos(pos)) return gridArray[pos.y, pos.x];
        return default(GridCell);
    }
    public Grid TryExpandGrid(int x, int y)
    {
        Grid newGrid = new Grid(width + x, height + y, originPos);
        for (int j = 0; j < height ; j++)
        {
            for(int i = 0; i < width; i++)
            {
                newGrid.gridArray[j, i] = GetGridCell(new Vector2Int(i,j));
            }
        }
        return newGrid;
    }
    public void PlaceBuilding(Vector2Int designedPos, BuildingSO buildingData)
    {
        BuildableObject buildableObject = buildingData.InstatiateObject(this,designedPos);
        buildableObject.Initialization(buildingData, designedPos);
        List<Vector2Int> buildPoses = buildingData.GetGridPosList(designedPos);
        foreach (Vector2Int tempPos in buildPoses)
        {
            GetGridCell(tempPos).Build(buildingData.layer,buildableObject);
            if(buildingData.category == BuildingCategory.Movement)
            {
                GetGridCell(tempPos).ConnectFloor(buildPoses);
            }
        }
    }
    public void DestroyBuilding(List<Vector2Int> buildPoses, BuildingSO buildingData)
    {
        foreach (Vector2Int tempPos in buildPoses)
        {
            GetGridCell(tempPos).DestroyBuildByLayer(buildingData.layer);
            if (buildingData.category == BuildingCategory.Movement)
            {
                GetGridCell(tempPos).ConnectFloor(new List<Vector2Int>());
            }
        }
    }
    public Queue<BuildableObject> GetGridPath(Vector2Int start,Func<Vector2Int,bool> terminateEndCondition)
    {
        bool[,] visited = new bool[height, width];
        Vector2Int[] dir = { Vector2Int.left, Vector2Int.right };
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Queue<BuildableObject> path = new Queue<BuildableObject>();
        Dictionary<Vector2Int, Vector2Int> parent = new Dictionary<Vector2Int, Vector2Int>();
        List<Vector2Int> nextPositions = new List<Vector2Int>();

        queue.Enqueue(start);
        visited[start.y, start.x] = true;

        while (queue.Count > 0)
        {
            Vector2Int pos = queue.Dequeue();

            nextPositions.Clear();

            if(terminateEndCondition(pos))
            {
                Vector2Int current = pos;
                while (current != start && parent.ContainsKey(current))
                {
                    Vector2Int after = parent[current];
                    BuildableObject building = GetGridCell(current).GetBuildingInlayer();
                    if (building && !path.Contains(building))
                    {
                        if (building.category != BuildingCategory.Movement || current.y != after.y)
                        {
                            path.Enqueue(building);
                        }
                    }
                    current = after;
                }
                path = new Queue<BuildableObject>(path.Reverse());
                break;
            }

            GetGridCell(pos).connectedFloor.ForEach((nextpos) => nextPositions.Add(nextpos));
            dir.ForEach((nextPos) => nextPositions.Add(new Vector2Int(nextPos.x + pos.x, nextPos.y + pos.y)));

            foreach (Vector2Int nextPos in nextPositions)
            {
                if (IsValidGridPos(nextPos)&& GetGridCell(nextPos).HasBuilding() && !visited[nextPos.y, nextPos.x])
                {
                    queue.Enqueue(nextPos);
                    visited[nextPos.y, nextPos.x] = true;
                    parent[nextPos] = pos;
                }
            }
        }
        return path;
    }
    public List<BuildableObject> GetAllVisitableBuilding(Vector2Int start)
    {
        bool[,] visited = new bool[height, width];
        Vector2Int[] dir = { Vector2Int.left, Vector2Int.right };
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        List<Vector2Int> nextPositions = new List<Vector2Int>();
        List<BuildableObject> result = new List<BuildableObject>();

        queue.Enqueue(start);
        visited[start.y, start.x] = true;

        while (queue.Count > 0)
        {
            Vector2Int pos = queue.Dequeue();

            BuildableObject building = GetGridCell(pos).GetBuildingInlayer();
            if (building && building.isVisitable() && !result.Contains(building))
                result.Add(building);

            GetGridCell(pos).connectedFloor.ForEach((nextpos) => nextPositions.Add(nextpos));
            dir.ForEach((nextPos) => nextPositions.Add(new Vector2Int(nextPos.x + pos.x, nextPos.y + pos.y)));

            foreach (Vector2Int nextPos in nextPositions)
            {
                if (IsValidGridPos(nextPos) && GetGridCell(nextPos).HasBuilding() && !visited[nextPos.y, nextPos.x])
                {
                    queue.Enqueue(nextPos);
                    visited[nextPos.y, nextPos.x] = true;
                }
            }
        }
        return result;
    }
    public Queue<BuildableObject> SmoothingPath(Queue<BuildableObject> gridPath)
    {
        Queue<BuildableObject> result = new Queue<BuildableObject>();
        if(!gridPath.Any()) return result;

        while (gridPath.Count > 1)
        {
            BuildableObject building = gridPath.Dequeue();
            if (building.category == BuildingCategory.Movement)
            {
                result.Enqueue(building);
            }
        }
        result.Enqueue(gridPath.Dequeue());
        return result;
    }
    public bool IsConnectedWithGate(List<Vector2Int> end)
    {
        return GetGridPath(Vector2Int.zero, (pos) => end.Contains(pos)).Any();
    }
    public bool IsConneted(Vector2Int start,int id)
    {
        return GetGridPath(start, (pos) => GetGridCell(pos).GetBuildingInlayer()?.id == id).Any();
    }
    public List<BuildableObject> FindAllBuilding(int id)
    {
        List<BuildableObject> result = new List<BuildableObject>();
        for(int i = 0; i < height; i++)
        {
            for(int j = 0; j < width; j ++)
            {
                if (gridArray[i, j].HasBuildingInLayer())
                {
                    BuildableObject building = gridArray[i, j].GetBuildingInlayer();
                    if(!result.Contains(building) && building.id == id)
                    result.Add(building);
                }
            }
        }
        return result;
    }
    public int CountBuilding(BuildingSO buildingSO)
    {
        return FindAllBuilding(buildingSO.id).Count;
    }
}

