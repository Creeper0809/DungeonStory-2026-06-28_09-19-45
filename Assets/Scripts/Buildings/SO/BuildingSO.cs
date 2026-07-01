using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
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

[Flags]
public enum FacilityRole
{
    None = 0,
    Meal = 1 << 0,
    Purchase = 1 << 1,
    Rest = 1 << 2,
    Training = 1 << 3,
    Research = 1 << 4,
    Mana = 1 << 5,
    Logistics = 1 << 6
}

[Flags]
public enum FacilityWorkType
{
    None = 0,
    Operate = 1 << 0,
    Restock = 1 << 1,
    Repair = 1 << 2,
    Clean = 1 << 3,
    Research = 1 << 4,
    Guard = 1 << 5,
    Rescue = 1 << 6,
    Rest = 1 << 7
}

[Serializable]
public class FacilityData
{
    public FacilityRole roles;
    [Min(0)] public int capacity = 1;
    [Min(0f)] public float useDuration = 1f;
    [Min(0)] public int internalStockMax;
    [Min(0)] public int restockRequestThreshold;
    [Min(0)] public int requiredWorkers;
    public FacilityWorkType supportedWorkTypes;
    public string[] preferredSpeciesTags = Array.Empty<string>();
    public string[] dislikedSpeciesTags = Array.Empty<string>();
    public bool disabledWhenDamaged = true;
    public bool requiresStock;

    public bool IsVisitorFacility => roles != FacilityRole.None && capacity > 0;

    public bool SupportsRole(FacilityRole role)
    {
        return role != FacilityRole.None && (roles & role) != 0;
    }

    public bool SupportsWork(FacilityWorkType workType)
    {
        return workType != FacilityWorkType.None && (supportedWorkTypes & workType) != 0;
    }
}

public readonly struct GridBuildingPlacement
{
    public int Width { get; }
    public int Height { get; }
    public GridLayer Layer { get; }
    public BuildingCategory Category { get; }
    public bool HorizontalDraggable { get; }
    public bool VerticalDraggable { get; }

    public bool IsMovement => Category == BuildingCategory.Movement;
    public bool IsWall => Category == BuildingCategory.Wall;
    public bool IsDraggable => HorizontalDraggable || VerticalDraggable;
    public bool HasEvenWidth => Width % 2 == 0;

    public GridBuildingPlacement(
        int width,
        int height,
        GridLayer layer,
        BuildingCategory category,
        bool horizontalDraggable,
        bool verticalDraggable)
    {
        Width = Mathf.Max(1, width);
        Height = Mathf.Max(1, height);
        Layer = layer;
        Category = category;
        HorizontalDraggable = horizontalDraggable;
        VerticalDraggable = verticalDraggable;
    }

    public List<Vector2Int> GetGridPosList(Vector2Int center)
    {
        List<Vector2Int> posList = new List<Vector2Int>();
        int startX = center.x - (Width / 2);

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                posList.Add(new Vector2Int(startX + i, center.y + j));
            }
        }

        return posList;
    }
}

[CreateAssetMenu(menuName = "Grid/Building/SO", order = 0)]
public class BuildingSO : DataScriptableObject
{
    [Header("Presentation")]
    public string objectName;
    public Sprite sprite;
    public Sprite icon;

    [Header("Grid Placement")]
    public int width;
    public int height;
    public GridLayer layer;
    public BuildingCategory category;
    public bool horizontalDraggable;
    public bool verticalDraggable;
    public Type type;
    public Dictionary<GridTexture.TilemapLayer, Tile> tiles;
    [Tooltip("이동 시설이 캐릭터를 통과시킬 때 기준점에 더하는 월드 좌표 오프셋")]
    public Vector2 movementAnchorOffset;
    [Min(0f)]
    public float movementTravelTime = 2f;

    [Header("Game Data")]
    public int maintenance;
    public FacilityData facility = new FacilityData();
    public DefenseFacilityData defense = new DefenseFacilityData();
    [SerializeField] private List<IBuildingCondition> OnBuildCondition;
    public bool unlocked;

    public GridBuildingPlacement Placement => new GridBuildingPlacement(
        width,
        height,
        layer,
        category,
        horizontalDraggable,
        verticalDraggable);

    public bool IsGridMovement => Placement.IsMovement;
    public bool IsWall => Placement.IsWall;
    public bool IsEvenWidth => Placement.HasEvenWidth;
    public FacilityData Facility => facility ??= new FacilityData();
    public DefenseFacilityData Defense => defense ??= new DefenseFacilityData();

    public IReadOnlyList<IBuildingCondition> BuildConditions => OnBuildCondition != null
        ? (IReadOnlyList<IBuildingCondition>)OnBuildCondition
        : Array.Empty<IBuildingCondition>();

    public List<Vector2Int> GetGridPosList(Vector2Int center)
    {
        return Placement.GetGridPosList(center);
    }

    public bool GetDraggable()
    {
        return Placement.IsDraggable;
    }
}
