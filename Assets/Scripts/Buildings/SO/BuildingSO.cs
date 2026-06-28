using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum BuildingCategory
{
    None = 0,
    Wall = 1,
    Shop = 2,
    Special = 3,
    Movement = 4,
    Production = 5,
    Crafting = 6,
    Resource = 7
}
[CreateAssetMenu(menuName = "DungeonStory/Building/SO", order = 0)]
public class BuildingSO : DataScriptableObject
{
    [field: SerializeField] public string objectName { get; private set; }
    [field: SerializeField] public int width { get; private set; }
    [field: SerializeField] public int height { get; private set; }
    [field: SerializeField] public int maintenance { get; private set; }
    [field: SerializeField] public Sprite sprite { get; private set; }
    [field: SerializeField] public Sprite icon { get; private set; }
    [field: SerializeField] public GridLayer layer { get; private set; }
    [field: SerializeField] public BuildingCategory category { get; private set; }
    [field: SerializeField] public bool horizontalDraggable { get; private set; }
    [field: SerializeField] public bool verticalDraggable { get; private set; }
    [field: SerializeField] private List<IBuildingCondition> OnBuildCondition { get; set; }
    [field: SerializeField] public bool unlocked{ get; set; }
    [field: SerializeField] public Type type { get; private set; }

    public Dictionary<GridTexture.TilemapLayer, Tile> tiles;

    public List<Vector2Int> GetGridPosList(Vector2Int center)
    {
        List<Vector2Int> posList = new List<Vector2Int>();

        int extraWidth = 0;
        int startX = center.x - (width / 2) + extraWidth;

        for (int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                int posX = startX + i;
                posList.Add(new Vector2Int(posX, center.y + j));
            }
        }
        return posList;
    }
    public BuildableObject InstatiateObject(Grid grid, Vector2Int selectPos)
    {
        Vector3 evenOffset = new Vector2(0.5f, 0);
        Vector2 instatiatePos = width % 2 == 0 ? grid.GetWorldPos(selectPos) + evenOffset : grid.GetWorldPos(selectPos);

        GameObject placedObject = new GameObject();
        placedObject.name = objectName;
        placedObject.transform.position = instatiatePos;
        if(category != BuildingCategory.Wall)
        {
            BoxCollider2D collider2D = placedObject.AddComponent<BoxCollider2D>();
            collider2D.size = new Vector2(width, height * 2.9f);
            collider2D.offset = new Vector2(0, 1.5f);

            placedObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        }
       
        GridTexture.Instance.DrawBuilding(tiles, selectPos,width % 2 == 0);

        if(placedObject.AddComponent(type) is BuildableObject buildableObject)
            return buildableObject;
        Debug.Log("건물 데이터에 원치 않은 컴퍼넌트가 연결되었습니다");
        return null;
    }
    public bool IsSatisfyConditionOnBuild(Grid grid, Vector2Int buildPos, out string errorMessage)
    {
        List<Vector2Int> totalBuildPos = GetGridPosList(buildPos);
        foreach (Vector2Int tempPos in totalBuildPos)
        {
            if (!grid.IsValidGridPos(tempPos) || tempPos.x == 0 || tempPos.x == grid.width - 1)
            {
                errorMessage = $"설치할 수 없는 위치입니다";
                return false;
            }
            if (!grid.GetGridCell(tempPos).CanBuild(layer))
            {
                errorMessage = $"이미 설치 된 건물이 존재합니다";
                return false;
            }
        }
        bool canBuild = true;
        foreach (Vector2Int tempPos in totalBuildPos)
        {
            if (tempPos.y != 0 && tempPos.y == totalBuildPos.Min((pos) => pos.y) && !grid.GetGridCell(tempPos + Vector2Int.down).HasBuilding())
            {
                canBuild = false;
                break;
            }
        }
        if (!canBuild)
        {
            errorMessage = $"바닥이 없습니다.";
            return false;
        }
        foreach (IBuildingCondition condition in OnBuildCondition)
        {
            if (!condition.IsSatisfy(grid, totalBuildPos, out errorMessage)) return false;
        }
        errorMessage = string.Empty;
        return true;
    }
    public bool GetDraggable()
    {
        return horizontalDraggable || verticalDraggable;
    }
    public bool IsSatisfyConditionOnBuild(Grid grid, Vector2Int buildPos)
    {
        string errorMessage;
        return IsSatisfyConditionOnBuild(grid, buildPos,out errorMessage);
    }
    public bool IsSatisfyConditionOnDestroy(Grid grid, BuildableObject building, out string errorMessage)
    {
        List<Vector2Int> buildedPos = GetGridPosList(building.centerPos);
        foreach (Vector2Int pos in buildedPos)
        {
            if (pos.y < grid.height - 1
                && !grid.GetGridCell(pos).GetAllBuilding().Where((x) => x != building).Any()
                && grid.GetGridCell(pos + Vector2Int.up).HasBuilding())
            {
                errorMessage = $"윗층에 건물이 존재합니다";
                return false;
            }
        }
        errorMessage = string.Empty;
        return true;
    }
    public void OnBuildSucess()
    {
        OnBuildCondition.ForEach(e => e.OnBuild());
    }
}
