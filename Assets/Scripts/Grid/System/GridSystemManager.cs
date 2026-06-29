using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GridMode
{
    X,
    None,
    Build,
    Destory
}

public class GridSystemManager : MonoBehaviour
{
    private static GridSystemManager instance;

    public static GridSystemManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GridSystemManager>();
            }

            if (instance != null)
            {
                instance.EnsureGridInitialized();
            }

            return instance;
        }
    }

    public Grid grid { get; private set; }
    public GridMode Mode { get; private set; } = GridMode.None;

    [Min(1)] [Tooltip("그리드 행 개수")]
    public int defaultGridWidth;
    [Min(1)] [Tooltip("그리드 열 개수")]
    public int defaultGridHeight;
    public Vector3 gridOriginPos;

    public Vector2Int firstSelectedPos { get; private set; }
    public Vector2Int lastSelectedPos { get; private set; }
    public List<Vector2Int> totalSelectedPos { get; private set; } = new List<Vector2Int>();
    public bool isDragging { get; private set; }
    public int xCount { get; private set; }
    public int yCount { get; private set; }

    public event Action OnGridExpand;
    public event Action OnGridObjectChanged;
    public event Action<GridMode> OnGridModeChanged;
    public event Action<Vector2Int> OnLastSelectedPosChanged;

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"Multiple {nameof(GridSystemManager)} instances found. Using the newest instance.");
        }

        instance = this;
        EnsureGridInitialized();
    }

    protected virtual void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
        }

        EnsureGridInitialized();
    }

    public void EnsureGridInitialized()
    {
        if (grid != null) return;

        grid = new Grid(defaultGridWidth, defaultGridHeight, gridOriginPos);
    }

    protected virtual void Start()
    {
        SetGridMode(GridMode.None);
        NotifyGridObjectChanged();
    }

    public void GridExpand(int x,int y)
    {
        Grid newGrid = grid.TryExpandGrid(x,y);
        if (newGrid == null) return;

        grid = newGrid;
        OnGridExpand?.Invoke();
    }

    public void SetGridMode(GridMode gridMode)
    {
        if (Mode == gridMode) return;

        Mode = gridMode;
        if (Mode == GridMode.None)
        {
            CancelDragSelection();
        }

        OnGridModeChanged?.Invoke(Mode);
    }

    public void SetGridModeBuild()
    {
        if (Mode == GridMode.Build) return;

        SetGridMode(GridMode.Build);
    }

    public void SetGridModeNone()
    {
        SetGridMode(GridMode.None);
    }

    public void ToggleBuildMode()
    {
        SetGridMode(Mode == GridMode.Build ? GridMode.None : GridMode.Build);
    }

    public bool TryBeginDragSelection(Vector2Int start, bool horizontalDraggable, bool verticalDraggable)
    {
        if (grid == null || !grid.IsValidGridPos(start)) return false;

        isDragging = true;
        firstSelectedPos = start;
        UpdateDragSelection(start, horizontalDraggable, verticalDraggable);
        return true;
    }

    public void UpdateDragSelection(Vector2Int pos, bool horizontalDraggable, bool verticalDraggable)
    {
        if (!isDragging && totalSelectedPos.Count == 0) return;

        lastSelectedPos = pos;
        OnLastSelectedPosChanged?.Invoke(lastSelectedPos);
        RecalculateSelectedPositions(pos, horizontalDraggable, verticalDraggable);
    }

    public List<Vector2Int> CompleteDragSelection()
    {
        isDragging = false;
        return new List<Vector2Int>(totalSelectedPos);
    }

    public void CancelDragSelection()
    {
        isDragging = false;
        totalSelectedPos.Clear();
        xCount = 0;
        yCount = 0;
    }

    public Vector3 GetWorldPosSnapped(Vector3 worldPosition)
    {
        if (grid == null) return worldPosition;

        Vector2Int pos = grid.GetXY(worldPosition);
        if (!grid.IsValidGridPos(pos)) return worldPosition;

        return grid.GetWorldPos(pos);
    }

    public void NotifyGridObjectChanged()
    {
        OnGridObjectChanged?.Invoke();
    }

    private void RecalculateSelectedPositions(Vector2Int pos, bool horizontalDraggable, bool verticalDraggable)
    {
        totalSelectedPos = new List<Vector2Int>();
        int startX = Mathf.Min(firstSelectedPos.x, pos.x);
        int endX = Mathf.Max(firstSelectedPos.x, pos.x);
        int startY = Mathf.Min(firstSelectedPos.y, pos.y);
        int endY = Mathf.Max(firstSelectedPos.y, pos.y);

        if (verticalDraggable && horizontalDraggable)
        {
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    totalSelectedPos.Add(new Vector2Int(x, y));
                }
            }
        }
        else if (verticalDraggable)
        {
            for (int y = startY; y <= endY; y++)
            {
                totalSelectedPos.Add(new Vector2Int(firstSelectedPos.x, y));
            }
        }
        else if (horizontalDraggable)
        {
            for (int x = startX; x <= endX; x++)
            {
                totalSelectedPos.Add(new Vector2Int(x, firstSelectedPos.y));
            }
        }
        else
        {
            totalSelectedPos.Add(firstSelectedPos);
        }

        totalSelectedPos = totalSelectedPos.Where(gridPos => grid.IsValidGridPos(gridPos)).ToList();
        xCount = totalSelectedPos.GroupBy(gridPos => gridPos.x).Count();
        yCount = totalSelectedPos.GroupBy(gridPos => gridPos.y).Count();
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
