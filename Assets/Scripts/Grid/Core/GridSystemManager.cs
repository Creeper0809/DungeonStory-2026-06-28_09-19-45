using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.PlayerSettings;
using static UnityEditor.ShaderData;

public enum GridLayer
{
    Hallway = 0,
    Building = 1,
}
public enum GridMode
{
    X,
    None,
    Build,
    Destory
}
public class GridSystemManager : UtilSingleton<GridSystemManager>
{
    public Grid grid { get; private set; }
    private DataManager dataManager;
    public Data<BuildingSO> selectedBuilding;
    public Data<GridMode> gridMode;

    [Min(1)][Tooltip("그리드 행 개수")]
    public int defaultGridWidth;
    [Min(1)][Tooltip("그리드 열 개수")]
    public int defaultGridHeight;
    public Vector3 gridOriginPos;
    [ReadOnly] public Vector2Int firstSelectedPos { get; private set; }
    public Data<Vector2Int> lastSelectedPos;
    public List<Vector2Int> totalSelectedPos { get; private set; }
    public bool isDragging { get; private set; }
    public event OnGridExpandDelegate OnGridExpand;
    public delegate void OnGridExpandDelegate();

    public List<InitialBuildInfo> initialPlacement;

    public event OnGridObjectChangeDelegate onGridObjectChange;
    public delegate void OnGridObjectChangeDelegate();

    public int xCount { get; private set; }
    public int yCount { get; private set; }
    protected override void Awake()
    {
        base.Awake();
        grid = new Grid(defaultGridWidth, defaultGridHeight, gridOriginPos,initialPlacement);
        dataManager = DataManager.Instance;
    }
    void Start()
    {
        gridMode.Initialize(GridMode.None);
        selectedBuilding.Initialize(null);
        SetGridMode(GridMode.None);
        onGridObjectChange?.Invoke();

        lastSelectedPos.OnValueChange += OnLastSelectedPosChange;
    }
    /// <summary>
    /// 프레임당 1회 실행
    /// </summary>
    private void Update()
    {
        if (isDragging)
        {
            lastSelectedPos.Value = grid.GetXY(GameManager.Instance.GetMouseWorldPos());
        }
    }
    public void OnLastSelectedPosChange(Vector2Int pos)
    {
        totalSelectedPos = new List<Vector2Int>();
        int startX = Mathf.Min(firstSelectedPos.x, pos.x);
        int endX = Mathf.Max(firstSelectedPos.x, pos.x);
        int startY = Mathf.Min(firstSelectedPos.y, pos.y);
        int endY = Mathf.Max(firstSelectedPos.y, pos.y);
        xCount = 0;
        yCount = 0;
        if (selectedBuilding.Value.verticalDraggable && selectedBuilding.Value.horizontalDraggable)
        {
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    totalSelectedPos.Add(new Vector2Int(x, y));
                }
            }
        }
        else if (selectedBuilding.Value.verticalDraggable)
        {
            for (int y = startY; y <= endY; y++)
            {
                totalSelectedPos.Add(new Vector2Int(firstSelectedPos.x, y));
            }
        }
        else if (selectedBuilding.Value.horizontalDraggable)
        {
            for (int x = startX; x <= endX; x++)
            {
                totalSelectedPos.Add(new Vector2Int(x, firstSelectedPos.y));
            }
        }
        totalSelectedPos = totalSelectedPos.Where(pos => grid.IsValidGridPos(pos)).ToList();
        xCount = totalSelectedPos.GroupBy(pos => pos.x).Count();
        yCount = totalSelectedPos.GroupBy(pos => pos.y).Count();
    }
    
    public void TriggerPlaceBuilding()
    {
        if (gridMode.Value != GridMode.Build || selectedBuilding.Value == null || (EventSystem.current.IsPointerOverGameObject())) return;
        if (!selectedBuilding.Value.GetDraggable())
        {
            Vector2Int pos = grid.GetXY(GameManager.Instance.GetMouseWorldPos());
            PlaceBuilding(new List<Vector2Int> { pos });
            return;
        }
        if (!isDragging)
        {
            firstSelectedPos = grid.GetXY(GameManager.Instance.GetMouseWorldPos());
            if (!grid.IsValidGridPos(firstSelectedPos))
            {
                NoticeFeedEvent.Trigger($"유효하지 않은 위치입니다.", NoticeFeedEvent.Grade.DANGER);
                return;
            }
            isDragging = true;
        }
        else
        {
            isDragging = false;
            PlaceBuilding(totalSelectedPos);
        }

    }
    private void PlaceBuilding(List<Vector2Int> poses)
    {
        foreach (var pos in poses)
        {
            if (!selectedBuilding.Value.IsSatisfyConditionOnBuild(grid, pos, out string errorMessage))
            {
                NoticeFeedEvent.Trigger(errorMessage, NoticeFeedEvent.Grade.DANGER);
                continue;
            }
            grid.PlaceBuilding(pos, selectedBuilding.Value);
            if (selectedBuilding.Value.layer != GridLayer.Hallway)
            {
                foreach (var i in selectedBuilding.Value.GetGridPosList(pos))
                {
                    if (grid.GetGridCell(i).HasBuildingInLayer(GridLayer.Hallway)) continue;
                    Vector2Int tempPos = new Vector2Int(i.x, i.y);
                    grid.PlaceBuilding(tempPos, dataManager.GetData<BuildingSO>()[0]);
                }
            }
            selectedBuilding.Value.OnBuildSucess();
            onGridObjectChange?.Invoke();
        }
        NoticeFeedEvent.Trigger($"{selectedBuilding.Value.objectName}X{poses.Count} 설치 완료", NoticeFeedEvent.Grade.NONE);
        ClearBuildingSO();
        SetGridModeNone();
    }
    public void TriggerDestroyBuildableObject()
    {
        if (gridMode.Value != GridMode.Destory) return;
        BuildableObject building = GetBuildingByMousePos();
        if (!building) return;
        BuildingSO buildingData = dataManager.GetData<BuildingSO>()[building.id];
        if (!buildingData.IsSatisfyConditionOnDestroy(grid, building, out string errorMessage))
        {
            NoticeFeedEvent.Trigger(errorMessage, NoticeFeedEvent.Grade.DANGER);
            return;
        }
        grid.DestroyBuilding(building.buildPoses, buildingData);
        NoticeFeedEvent.Trigger($"{buildingData.objectName} 삭제 완료", NoticeFeedEvent.Grade.NONE);
        building.DestroySelf();
        onGridObjectChange?.Invoke();
    }
    public BuildableObject GetBuildingByMousePos()
    {
        Vector2Int pos = grid.GetXY(GameManager.Instance.GetMouseWorldPos());
        if (!grid.IsValidGridPos(pos)) return null;
        return grid.GetGridCell(pos).GetBuilding();
    }
    public bool IsBuildable()
    {
        if (selectedBuilding.Value == null) return false;
        return selectedBuilding.Value.IsSatisfyConditionOnBuild(grid, firstSelectedPos);
    }
    /// <summary>
    /// 가로 세로의 길이만큼 그리드의 크기를 확장한다.
    /// </summary>
    /// <param name="x">가로 길이</param>
    /// <param name="y">세로 길이</param>
    public void GridExpand(int x,int y)
    {
        Grid newGrid = grid.TryExpandGrid(x,y);
        if (newGrid == null) return;
        grid = newGrid;
        OnGridExpand?.Invoke();
    }

    /// <summary>
    /// 그리드 빌딩 모드를 토글한다.
    /// </summary>
    public void ToggleGridModeBuilding()
    {
        if (gridMode.Value != GridMode.Build)
        {
            SetGridMode(GridMode.Build);
        }
        else
        {
            SetGridMode(GridMode.None);
            ClearBuildingSO();
        }
    }
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
    //                                                                                     GET SET                                                                                                              //
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
    
    /// <summary>
    /// 마우스 위치를 스냅핑 해 리턴한다
    /// </summary>
    /// <returns>스냅핑 된 위치</returns>
    public Vector3 GetMouseWorldPosSnapped()
    {
        Vector3 mousePos = GameManager.Instance.GetMouseWorldPos();
        Vector2Int pos = grid.GetXY(mousePos);
        if (!grid.IsValidGridPos(pos)) return mousePos;
        return grid.GetWorldPos(pos);
    }

    public void SelectBuildingById(int id)
    {
        if (gridMode.Value == GridMode.None) return;
        if(id == -1)
        {
            selectedBuilding.Value = null;
            return;
        }
        selectedBuilding.Value = dataManager.GetData<BuildingSO>()[id];
    }

    /// <summary>
    /// 선택된 건물을 초기화 한다.
    /// </summary>
    public void ClearBuildingSO()
    {
        SelectBuildingById(-1);
    }

    public void SetGridMode(GridMode gridMode)
    {
        this.gridMode.Value = gridMode;
    }

    public void SetGridModeBuild()
    {
        if (gridMode.Value == GridMode.Build) return;
        SetGridMode(GridMode.Build);
    }

    public void SetGridModeNone()
    {
        SetGridMode(GridMode.None);
    }
}
[Serializable]
public class InitialBuildInfo
{
    public Vector2Int Position;
    public BuildingSO Building;
}
