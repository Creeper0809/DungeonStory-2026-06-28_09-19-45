using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class GridPlacementGhostPresenter : MonoBehaviour
{
    [SerializeField] private GridGhostObject ghostObject;
    [SerializeField] private int cellWorldHeight = 3;
    [SerializeField] private float followLerpSpeed = 25f;

    protected abstract Grid ActiveGrid { get; }
    protected abstract bool HasGhostSelection { get; }
    protected abstract Sprite GhostSprite { get; }
    protected abstract int GhostWidth { get; }
    protected abstract float GridOriginZ { get; }
    protected abstract bool IsDraggingSelection { get; }
    protected abstract IReadOnlyList<Vector2Int> SelectedGridPositions { get; }
    protected abstract int SelectedColumnCount { get; }
    protected abstract int SelectedRowCount { get; }
    protected abstract Vector3 MouseWorldPosition { get; }
    protected abstract Vector3 MouseWorldPositionSnapped { get; }

    protected virtual int CellWorldHeight => cellWorldHeight;
    protected virtual float FollowLerpSpeed => followLerpSpeed;

    protected abstract bool CanPlaceAt(Vector2Int gridPosition);

    protected virtual void Awake()
    {
        EnsureGhostObject();
        HideGhost();
    }

    protected virtual void Update()
    {
        UpdateGhost();
    }

    protected void RefreshGhostSelection()
    {
        if (!HasGhostSelection)
        {
            HideGhost();
            return;
        }

        EnsureGhostObject();
        ghostObject.SetWorldPosition(MouseWorldPosition);
        ghostObject.Show(GhostSprite);
    }

    protected void HideGhost()
    {
        EnsureGhostObject();
        ghostObject?.Hide();
    }

    private void UpdateGhost()
    {
        EnsureGhostObject();
        Grid grid = ActiveGrid;
        if (ghostObject == null || ghostObject.IsHidden || grid == null) return;

        if (!HasGhostSelection)
        {
            HideGhost();
            return;
        }

        if (!IsDraggingSelection)
        {
            UpdateSingleCellGhost(grid);
            return;
        }

        UpdateDraggedGhost(grid);
    }

    private void UpdateSingleCellGhost(Grid grid)
    {
        Vector2Int targetPos = grid.GetXY(MouseWorldPosition);
        ghostObject.SetBuildable(CanPlaceAt(targetPos));

        Vector3 snappedPosition = MouseWorldPositionSnapped;
        Vector2 offset = GhostWidth % 2 == 0 ? new Vector2(0.5f, 0f) : Vector2.zero;
        Vector2 ghostPosition = (Vector2)snappedPosition + offset;
        ghostObject.SetWorldPosition(new Vector3(ghostPosition.x, ghostPosition.y, GridOriginZ), FollowLerpSpeed);
    }

    private void UpdateDraggedGhost(Grid grid)
    {
        IReadOnlyList<Vector2Int> selectedPositions = SelectedGridPositions;
        if (selectedPositions == null || selectedPositions.Count == 0) return;

        int width = Mathf.Max(1, GhostWidth);
        int xCount = Mathf.Max(1, SelectedColumnCount);
        int yCount = Mathf.Max(1, SelectedRowCount);

        int minY = selectedPositions.Min((pos) => pos.y);
        int minX = selectedPositions.Min((pos) => pos.x) + (xCount / 2);
        Vector3 basePosition = grid.GetWorldPos(new Vector2Int(minX, minY));
        Vector3 offset = (width * xCount) % 2 == 0 ? new Vector3(0.5f, 0f, 0f) : Vector3.zero;

        ghostObject.SetWorldPosition(basePosition + offset);
        ghostObject.SetSize(new Vector2(width * xCount, yCount * CellWorldHeight));
    }

    private void EnsureGhostObject()
    {
        if (ghostObject != null) return;

        ghostObject = GetComponent<GridGhostObject>();
        if (ghostObject == null)
        {
            ghostObject = gameObject.AddComponent<GridGhostObject>();
        }

        ghostObject.Initialize(transform.childCount > 0 ? transform.GetChild(0).gameObject : null);
    }
}
