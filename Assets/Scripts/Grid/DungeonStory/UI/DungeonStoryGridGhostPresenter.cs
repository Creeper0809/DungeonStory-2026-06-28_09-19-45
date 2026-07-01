using System.Collections.Generic;
using UnityEngine;

public class DungeonStoryGridGhostPresenter : GridPlacementGhostPresenter
{
    private GridSystemManager gridSystemManager;
    private DungeonStoryGridBuildingController buildingController;

    protected override Grid ActiveGrid => gridSystemManager?.grid;
    protected override bool HasGhostSelection => buildingController?.SelectedBuilding != null
        && gridSystemManager != null
        && gridSystemManager.Mode == GridMode.Build;
    protected override Sprite GhostSprite => buildingController.SelectedBuilding.sprite;
    protected override int GhostWidth => buildingController.SelectedBuilding.width;
    protected override float GridOriginZ => gridSystemManager != null ? gridSystemManager.gridOriginPos.z : 0f;
    protected override bool IsDraggingSelection => gridSystemManager != null && gridSystemManager.isDragging;
    protected override IReadOnlyList<Vector2Int> SelectedGridPositions => gridSystemManager?.totalSelectedPos;
    protected override Vector3 MouseWorldPosition => GameManager.Instance.GetMouseWorldPos();
    protected override Vector3 MouseWorldPositionSnapped => buildingController != null
        ? buildingController.GetMouseWorldPosSnapped()
        : MouseWorldPosition;

    protected override void Awake()
    {
        gridSystemManager = GridSystemManager.Instance;
        buildingController = DungeonStoryGridBuildingController.Instance;
        base.Awake();
    }

    protected override bool CanPlaceAt(Vector2Int gridPosition)
    {
        return buildingController != null && buildingController.IsBuildableAt(gridPosition);
    }

    private void OnGridModeChanged(GridMode gridMode)
    {
        if (gridMode == GridMode.None)
        {
            HideGhost();
            return;
        }

        RefreshGhostSelection();
    }

    public void OnSelectedChanged(BuildingSO buildingSO)
    {
        RefreshGhostSelection();
    }

    private void OnEnable()
    {
        if (gridSystemManager == null)
        {
            gridSystemManager = GridSystemManager.Instance;
        }
        if (buildingController == null)
        {
            buildingController = DungeonStoryGridBuildingController.Instance;
        }
        if (gridSystemManager == null || buildingController == null) return;

        buildingController.OnSelectedBuildingChanged += OnSelectedChanged;
        gridSystemManager.OnGridModeChanged += OnGridModeChanged;
    }

    private void OnDisable()
    {
        if (gridSystemManager == null || buildingController == null) return;

        buildingController.OnSelectedBuildingChanged -= OnSelectedChanged;
        gridSystemManager.OnGridModeChanged -= OnGridModeChanged;
    }
}
