using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DungeonStoryGridBuildingController : MonoBehaviour
{
    private static DungeonStoryGridBuildingController instance;

    public static DungeonStoryGridBuildingController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<DungeonStoryGridBuildingController>();
                if (instance == null && GridSystemManager.Instance != null)
                {
                    instance = GridSystemManager.Instance.gameObject.AddComponent<DungeonStoryGridBuildingController>();
                }
            }

            return instance;
        }
    }

    [SerializeField] private GridSystemManager gridSystem;
    [SerializeField] private InputAction buildingPlaceAction;
    [SerializeField] private InputAction expandGridAction;
    public Data<BuildingSO> selectedBuilding = new Data<BuildingSO>();
    public List<InitialBuildInfo> initialPlacement = new List<InitialBuildInfo>();

    private DataManager dataManager;
    private GridBuildingPlacementService placementService;
    private bool initialized;

    public event Action<BuildingSO> OnSelectedBuildingChanged;

    public GridSystemManager GridSystem => gridSystem != null ? gridSystem : GridSystemManager.Instance;
    public BuildingSO SelectedBuilding => selectedBuilding != null ? selectedBuilding.Value : null;

    private void Awake()
    {
        instance = this;
        if (gridSystem == null)
        {
            gridSystem = GridSystemManager.Instance;
        }
        if (selectedBuilding == null)
        {
            selectedBuilding = new Data<BuildingSO>();
        }

        EnsureInputActions();
    }

    private void Start()
    {
        EnsureInitialized();
        selectedBuilding.Initialize(null);
        if (gridSystem == null) return;

        gridSystem.SetGridMode(GridMode.None);
        gridSystem.NotifyGridObjectChanged();
    }

    private void EnsureInitialized()
    {
        if (initialized) return;

        if (gridSystem == null)
        {
            gridSystem = GridSystemManager.Instance;
        }
        if (gridSystem == null || gridSystem.grid == null) return;

        dataManager = DataManager.Instance;
        IReadOnlyDictionary<int, BuildingSO> buildingDataById = dataManager != null
            ? dataManager.GetData<BuildingSO>()
            : null;
        BuildingSO hallwayBuilding = null;
        buildingDataById?.TryGetValue(0, out hallwayBuilding);
        GridBuildingFactory buildingFactory = new GridBuildingFactory(GridTexture.Instance, ConfigurePlacedBuilding);
        placementService = new GridBuildingPlacementService(
            gridSystem.grid,
            hallwayBuilding,
            FindBuildingDataById,
            buildingFactory,
            new BuildingPlacementValidator());
        placementService.PlaceInitialBuildings(initialPlacement);
        gridSystem.OnGridExpand += OnGridExpand;
        gridSystem.OnGridObjectChanged += DrawGridTextureWalls;
        initialized = true;
    }

    private void Update()
    {
        if (gridSystem == null || gridSystem.grid == null) return;
        if (!gridSystem.isDragging || SelectedBuilding == null) return;

        gridSystem.UpdateDragSelection(
            gridSystem.grid.GetXY(GameManager.Instance.GetMouseWorldPos()),
            SelectedBuilding.horizontalDraggable,
            SelectedBuilding.verticalDraggable);
    }

    public void TriggerPlaceBuilding()
    {
        EnsureInitialized();
        if (gridSystem == null || gridSystem.grid == null || placementService == null) return;
        if (gridSystem.Mode != GridMode.Build || SelectedBuilding == null) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (!SelectedBuilding.GetDraggable())
        {
            Vector2Int pos = gridSystem.grid.GetXY(GameManager.Instance.GetMouseWorldPos());
            PlaceBuilding(new List<Vector2Int> { pos });
            return;
        }

        if (!gridSystem.isDragging)
        {
            Vector2Int firstSelectedPos = gridSystem.grid.GetXY(GameManager.Instance.GetMouseWorldPos());
            if (!gridSystem.TryBeginDragSelection(firstSelectedPos, SelectedBuilding.horizontalDraggable, SelectedBuilding.verticalDraggable))
            {
                NoticeFeedEvent.Trigger("유효하지 않은 위치입니다.", NoticeFeedEvent.Grade.DANGER);
            }
            return;
        }

        PlaceBuilding(gridSystem.CompleteDragSelection());
    }

    public void TriggerDestroyBuildableObject()
    {
        EnsureInitialized();
        if (gridSystem == null || gridSystem.grid == null || placementService == null) return;
        if (gridSystem.Mode != GridMode.Destory) return;

        BuildableObject building = GetBuildingByMousePos();
        if (!building) return;

        if (!placementService.TryDestroyBuilding(building, out BuildingSO buildingData, out string errorMessage))
        {
            NoticeFeedEvent.Trigger(errorMessage, NoticeFeedEvent.Grade.DANGER);
            return;
        }

        NoticeFeedEvent.Trigger($"{buildingData.objectName} 삭제 완료", NoticeFeedEvent.Grade.NONE);
        gridSystem.NotifyGridObjectChanged();
    }

    private void OnEnable()
    {
        EnsureInputActions();

        buildingPlaceAction.performed += OnBuildingPlaceInput;
        expandGridAction.performed += OnExpandGridInput;
        buildingPlaceAction.Enable();
        expandGridAction.Enable();
    }

    private void OnDisable()
    {
        if (buildingPlaceAction == null || expandGridAction == null) return;

        buildingPlaceAction.performed -= OnBuildingPlaceInput;
        expandGridAction.performed -= OnExpandGridInput;
        buildingPlaceAction.Disable();
        expandGridAction.Disable();
    }

    public BuildableObject GetBuildingByMousePos()
    {
        EnsureInitialized();
        if (gridSystem == null || gridSystem.grid == null) return null;

        Vector2Int pos = gridSystem.grid.GetXY(GameManager.Instance.GetMouseWorldPos());
        if (!gridSystem.grid.IsValidGridPos(pos)) return null;

        return gridSystem.grid.GetGridCell(pos).GetBuilding();
    }

    public bool IsBuildable()
    {
        EnsureInitialized();
        if (gridSystem == null || placementService == null) return false;

        return SelectedBuilding != null && placementService.CanPlaceBuilding(SelectedBuilding, gridSystem.firstSelectedPos);
    }

    public bool IsBuildableAt(Vector2Int pos)
    {
        EnsureInitialized();
        if (placementService == null) return false;

        return SelectedBuilding != null && placementService.CanPlaceBuilding(SelectedBuilding, pos);
    }

    public Vector3 GetMouseWorldPosSnapped()
    {
        EnsureInitialized();
        if (gridSystem == null) return GameManager.Instance.GetMouseWorldPos();

        return gridSystem.GetWorldPosSnapped(GameManager.Instance.GetMouseWorldPos());
    }

    public void SelectBuildingById(int id)
    {
        EnsureInitialized();
        if (gridSystem == null || dataManager == null) return;
        if (gridSystem.Mode == GridMode.None) return;

        BuildingSO building = null;
        if (id != -1)
        {
            building = dataManager.GetData<BuildingSO>()[id];
        }

        selectedBuilding.Value = building;
        OnSelectedBuildingChanged?.Invoke(building);
    }

    public void ClearBuildingSO()
    {
        SelectBuildingById(-1);
    }

    public void SetGridModeBuild()
    {
        if (gridSystem == null)
        {
            gridSystem = GridSystemManager.Instance;
        }
        if (gridSystem == null) return;

        gridSystem.SetGridModeBuild();
    }

    public void SetGridModeNone()
    {
        if (gridSystem == null)
        {
            gridSystem = GridSystemManager.Instance;
        }
        if (gridSystem == null) return;

        gridSystem.SetGridModeNone();
        ClearBuildingSO();
    }

    public void SetDestroyMode()
    {
        if (gridSystem == null)
        {
            gridSystem = GridSystemManager.Instance;
        }
        if (gridSystem == null) return;

        gridSystem.SetGridMode(GridMode.Destory);
        ClearBuildingSO();
    }

    private void PlaceBuilding(List<Vector2Int> poses)
    {
        if (placementService == null || SelectedBuilding == null) return;
        int placedCount = 0;
        BuildingSO building = SelectedBuilding;

        foreach (var pos in poses)
        {
            if (!placementService.TryPlaceBuilding(building, pos, out string errorMessage))
            {
                NoticeFeedEvent.Trigger(errorMessage, NoticeFeedEvent.Grade.DANGER);
                continue;
            }

            placedCount++;
            gridSystem.NotifyGridObjectChanged();
        }

        if (placedCount > 0)
        {
            NoticeFeedEvent.Trigger($"{building.objectName}X{placedCount} 설치 완료", NoticeFeedEvent.Grade.NONE);
        }

        ClearBuildingSO();
        gridSystem.SetGridModeNone();
    }

    private void DrawGridTextureWalls()
    {
        if (GridTexture.Instance == null) return;

        GridTexture.Instance.DrawWall(gridSystem.grid);
    }

    private void OnGridExpand()
    {
        if (placementService == null || gridSystem == null) return;

        placementService.SetGrid(gridSystem.grid);
        DrawGridTextureWalls();
    }

    private void OnDestroy()
    {
        if (gridSystem != null)
        {
            gridSystem.OnGridExpand -= OnGridExpand;
            gridSystem.OnGridObjectChanged -= DrawGridTextureWalls;
        }

        if (instance == this)
        {
            instance = null;
        }
    }

    private void EnsureInputActions()
    {
        buildingPlaceAction ??= new InputAction("Building Place", binding: "<Mouse>/leftButton");
        expandGridAction ??= new InputAction("Expand Grid", binding: "<Keyboard>/e");

        if (buildingPlaceAction.bindings.Count == 0)
        {
            buildingPlaceAction.AddBinding("<Mouse>/leftButton");
        }

        if (expandGridAction.bindings.Count == 0)
        {
            expandGridAction.AddBinding("<Keyboard>/e");
        }
    }

    private void OnBuildingPlaceInput(InputAction.CallbackContext context)
    {
        TriggerPlaceBuilding();
        TriggerDestroyBuildableObject();
    }

    private void OnExpandGridInput(InputAction.CallbackContext context)
    {
        if (gridSystem == null)
        {
            gridSystem = GridSystemManager.Instance;
        }
        if (gridSystem == null) return;

        gridSystem.GridExpand(2, 2);
    }

    private BuildingSO FindBuildingDataById(int id)
    {
        if (dataManager == null)
        {
            dataManager = DataManager.Instance;
        }

        IReadOnlyDictionary<int, BuildingSO> buildingDataById = dataManager != null
            ? dataManager.GetData<BuildingSO>()
            : null;
        return buildingDataById != null && buildingDataById.TryGetValue(id, out BuildingSO buildingData)
            ? buildingData
            : null;
    }

    private void ConfigurePlacedBuilding(BuildableObject building)
    {
        if (building == null) return;

        building.OnBuildingClicked -= OnPlacedBuildingClicked;
        building.OnBuildingClicked += OnPlacedBuildingClicked;
    }

    private void OnPlacedBuildingClicked(BuildableObject building)
    {
        if (building == null) return;
        if (gridSystem != null && gridSystem.Mode != GridMode.None) return;

        InfoFeedEvent.Trigger(new BuildingInfoTarget(building));
    }
}
