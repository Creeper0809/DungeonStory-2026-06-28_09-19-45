using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class DungeonStoryGridGhostPresenter : GridPlacementGhostPresenter
{
    private IGridSystemProvider gridSystemProvider;
    private IDungeonGridBuildingControllerProvider buildingControllerProvider;
    private IWorldPointerPositionProvider worldPointerPositionProvider;
    private GridSystemManager gridSystemManager;
    private DungeonStoryGridBuildingController buildingController;
    private bool subscribed;

    protected override Grid ActiveGrid => RequireGridSystem().grid;
    protected override bool HasGhostSelection => RequireBuildingController().SelectedBuilding != null
        && RequireGridSystem().Mode == GridMode.Build;
    protected override Sprite GhostSprite
    {
        get
        {
            BuildingSO building = RequireBuildingController().SelectedBuilding;
            return building.sprite != null ? building.sprite : building.icon;
        }
    }
    protected override int GhostWidth => RequireBuildingController().SelectedBuilding.width;
    protected override int GhostHeight => RequireBuildingController().SelectedBuilding.height;
    protected override bool GhostUsesFullFootprint
    {
        get
        {
            BuildingSO building = RequireBuildingController().SelectedBuilding;
            return building.IsStructuralWall
                || (building.tiles != null && building.tiles.Count > 0);
        }
    }
    protected override Vector2 GhostVisualOffset => RequireBuildingController().SelectedBuilding.IsInteriorDoor
        ? new Vector2(0f, InteriorDoorVisualLayout.GroundingOffsetY)
        : Vector2.zero;
    protected override Vector2? GhostVisualSizeOverride => RequireBuildingController().SelectedBuilding.IsInteriorDoor
        ? new Vector2(InteriorDoorVisualLayout.VisualWidth, InteriorDoorVisualLayout.VisualHeight)
        : null;
    protected override float GridOriginZ => RequireGridSystem().gridOriginPos.z;
    protected override bool IsDraggingSelection => RequireGridSystem().isDragging;
    protected override IReadOnlyList<Vector2Int> SelectedGridPositions => RequireGridSystem().totalSelectedPos;
    protected override Vector3 MouseWorldPosition => RequireWorldPointerPositionProvider().MouseWorldPosition;
    protected override Vector3 MouseWorldPositionSnapped => RequireBuildingController().GetMouseWorldPosSnapped();

    [Inject]
    public void Construct(
        IGridSystemProvider gridSystemProvider,
        IDungeonGridBuildingControllerProvider buildingControllerProvider,
        IWorldPointerPositionProvider worldPointerPositionProvider,
        IGridGhostObjectResolver ghostObjectResolver)
    {
        this.gridSystemProvider = gridSystemProvider
            ?? throw new ArgumentNullException(nameof(gridSystemProvider));
        this.buildingControllerProvider = buildingControllerProvider
            ?? throw new ArgumentNullException(nameof(buildingControllerProvider));
        this.worldPointerPositionProvider = worldPointerPositionProvider
            ?? throw new ArgumentNullException(nameof(worldPointerPositionProvider));
        ConstructGridPlacementGhostPresenter(ghostObjectResolver);
        SubscribeToRuntimeEventsIfInjected();
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override bool CanPlaceAt(Vector2Int gridPosition)
    {
        return RequireBuildingController().IsBuildableAt(gridPosition);
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
        SubscribeToRuntimeEventsIfInjected();
    }

    private void Start()
    {
        SubscribeToRuntimeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromRuntimeEvents();
    }

    private void SubscribeToRuntimeEventsIfInjected()
    {
        if (gridSystemProvider != null && buildingControllerProvider != null && isActiveAndEnabled)
        {
            SubscribeToRuntimeEvents();
        }
    }

    private void SubscribeToRuntimeEvents()
    {
        GridSystemManager manager = RequireGridSystemProvider().Manager;
        DungeonStoryGridBuildingController controller = RequireBuildingControllerProvider().Controller;
        if (subscribed && gridSystemManager == manager && buildingController == controller)
        {
            return;
        }

        UnsubscribeFromRuntimeEvents();
        gridSystemManager = manager;
        buildingController = controller;
        buildingController.OnSelectedBuildingChanged += OnSelectedChanged;
        gridSystemManager.OnGridModeChanged += OnGridModeChanged;
        subscribed = true;
    }

    private void UnsubscribeFromRuntimeEvents()
    {
        if (!subscribed)
        {
            return;
        }

        buildingController.OnSelectedBuildingChanged -= OnSelectedChanged;
        gridSystemManager.OnGridModeChanged -= OnGridModeChanged;
        subscribed = false;
        buildingController = null;
        gridSystemManager = null;
    }

    private IGridSystemProvider RequireGridSystemProvider()
    {
        return gridSystemProvider
            ?? throw new InvalidOperationException($"{nameof(DungeonStoryGridGhostPresenter)} requires {nameof(IGridSystemProvider)} injection.");
    }

    private GridSystemManager RequireGridSystem()
    {
        return gridSystemManager ?? RequireGridSystemProvider().Manager;
    }

    private IDungeonGridBuildingControllerProvider RequireBuildingControllerProvider()
    {
        return buildingControllerProvider
            ?? throw new InvalidOperationException($"{nameof(DungeonStoryGridGhostPresenter)} requires {nameof(IDungeonGridBuildingControllerProvider)} injection.");
    }

    private DungeonStoryGridBuildingController RequireBuildingController()
    {
        return buildingController ?? RequireBuildingControllerProvider().Controller;
    }

    private IWorldPointerPositionProvider RequireWorldPointerPositionProvider()
    {
        return worldPointerPositionProvider
            ?? throw new InvalidOperationException($"{nameof(DungeonStoryGridGhostPresenter)} requires {nameof(IWorldPointerPositionProvider)} injection.");
    }
}
