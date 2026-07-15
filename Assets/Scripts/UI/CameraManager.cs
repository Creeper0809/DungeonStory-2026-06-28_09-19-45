using System;
using UnityEngine;
using VContainer;

public class CameraManager : MonoBehaviour
{
    public float speed = 5.0f;
    public float edgeThreshold = 10.0f;
    public bool enableEdgeScroll = false;
    public Vector2 movementPadding = new Vector2(4f, 1f);
    public Vector2 maxBound;
    public Vector2 minBound;

    private IGridSystemProvider gridSystemProvider;
    private IMainCameraProvider mainCameraProvider;
    private IPlayerInputReader inputReader;
    private GridSystemManager gridSystemManager;
    private GridSystemManager fallbackGridSystemManager;
    private IPlayerInputReader fallbackInputReader;
    private Camera targetCamera;
    private float height;
    private float width;

    [Inject]
    public void Construct(
        IGridSystemProvider gridSystemProvider,
        IMainCameraProvider mainCameraProvider,
        IPlayerInputReader inputReader)
    {
        this.gridSystemProvider = gridSystemProvider ?? throw new ArgumentNullException(nameof(gridSystemProvider));
        this.mainCameraProvider = mainCameraProvider ?? throw new ArgumentNullException(nameof(mainCameraProvider));
        this.inputReader = inputReader ?? throw new ArgumentNullException(nameof(inputReader));
        SubscribeToGridExpansionIfInjected();
    }

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        SubscribeToGridExpansionIfInjected();
    }

    private void Start()
    {
        SubscribeToGridExpansion();
        UpdateViewportSize();
        ClampToCurrentBounds();
    }

    private void OnDisable()
    {
        UnsubscribeFromGridExpansion();
    }

    private void OnDestroy()
    {
        UnsubscribeFromGridExpansion();
    }

    private void Update()
    {
        UpdateViewportSize();

        IPlayerInputReader input = RequireInputReader();
        float moveX = GetKeyboardAxis(
            input.GetKey(KeyCode.A) || input.GetKey(KeyCode.LeftArrow),
            input.GetKey(KeyCode.D) || input.GetKey(KeyCode.RightArrow)) * speed;
        float moveY = GetKeyboardAxis(
            input.GetKey(KeyCode.S) || input.GetKey(KeyCode.DownArrow),
            input.GetKey(KeyCode.W) || input.GetKey(KeyCode.UpArrow)) * speed;
        Vector3 mousePosition = input.MousePosition;

        if (enableEdgeScroll && Mathf.Approximately(moveX, 0f))
        {
            moveX = GetHorizontalEdgeAxis(input, mousePosition) * speed;
        }

        if (enableEdgeScroll && Mathf.Approximately(moveY, 0f))
        {
            moveY = GetVerticalEdgeAxis(input, mousePosition) * speed;
        }

        transform.Translate(new Vector3(moveX * Time.deltaTime, moveY * Time.deltaTime, 0f), Space.World);
        ClampToCurrentBounds();
    }

    public void ClampToCurrentBounds()
    {
        UpdateViewportSize();
        GetGridBounds(out Vector2 lowerBound, out Vector2 upperBound);

        float clampX = ClampAxis(transform.position.x, lowerBound.x, upperBound.x, width);
        float clampY = ClampAxis(transform.position.y, lowerBound.y, upperBound.y, height);
        transform.position = new Vector3(clampX, clampY, -10f);
    }

    public void OnGridExpand()
    {
        ClampToCurrentBounds();
    }

    private void SubscribeToGridExpansionIfInjected()
    {
        if (gridSystemProvider != null && isActiveAndEnabled)
        {
            SubscribeToGridExpansion();
        }
    }

    private void SubscribeToGridExpansion()
    {
        if (!TryResolveGridSystemManager(out GridSystemManager manager))
        {
            return;
        }

        if (gridSystemManager == manager)
        {
            return;
        }

        UnsubscribeFromGridExpansion();
        gridSystemManager = manager;
        gridSystemManager.OnGridExpand += OnGridExpand;
    }

    private void UnsubscribeFromGridExpansion()
    {
        if (gridSystemManager == null)
        {
            return;
        }

        gridSystemManager.OnGridExpand -= OnGridExpand;
        gridSystemManager = null;
    }

    private IGridSystemProvider RequireGridSystemProvider()
    {
        return gridSystemProvider
            ?? throw new InvalidOperationException($"{nameof(CameraManager)} requires {nameof(IGridSystemProvider)} injection.");
    }

    private void UpdateViewportSize()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }

        Camera camera = targetCamera != null ? targetCamera : ResolveMainCamera();
        if (camera == null) return;

        height = camera.orthographicSize;
        width = camera.aspect * height;
    }

    private void GetGridBounds(out Vector2 lowerBound, out Vector2 upperBound)
    {
        Grid grid = RequireGrid();

        Vector3 firstCellCenter = grid.GetWorldPos(Vector2Int.zero);
        Vector3 lastCellCenter = grid.GetWorldPos(new Vector2Int(grid.width - 1, grid.height - 1));
        float minX = Mathf.Min(firstCellCenter.x, lastCellCenter.x) - 0.5f;
        float maxX = Mathf.Max(firstCellCenter.x, lastCellCenter.x) + 0.5f;
        float minY = Mathf.Min(firstCellCenter.y, lastCellCenter.y);
        float maxY = Mathf.Max(firstCellCenter.y, lastCellCenter.y) + 3f;

        lowerBound = new Vector2(minX - movementPadding.x, minY - movementPadding.y);
        upperBound = new Vector2(maxX + movementPadding.x, maxY + movementPadding.y);
    }

    private static float ClampAxis(float value, float min, float max, float halfViewSize)
    {
        float lower = Mathf.Min(min, max);
        float upper = Mathf.Max(min, max);
        if (upper - lower <= halfViewSize * 2f)
        {
            return (lower + upper) * 0.5f;
        }

        return Mathf.Clamp(value, lower + halfViewSize, upper - halfViewSize);
    }

    private static float GetKeyboardAxis(bool negativePressed, bool positivePressed)
    {
        if (negativePressed == positivePressed)
        {
            return 0f;
        }

        return negativePressed ? -1f : 1f;
    }

    private float GetHorizontalEdgeAxis(IPlayerInputReader input, Vector3 mousePosition)
    {
        if (!IsUsableEdgeScrollPointer(input, mousePosition))
        {
            return 0f;
        }

        if (mousePosition.x < edgeThreshold)
        {
            return -1f;
        }

        return mousePosition.x > input.ScreenWidth - edgeThreshold ? 1f : 0f;
    }

    private float GetVerticalEdgeAxis(IPlayerInputReader input, Vector3 mousePosition)
    {
        if (!IsUsableEdgeScrollPointer(input, mousePosition))
        {
            return 0f;
        }

        if (mousePosition.y < edgeThreshold)
        {
            return -1f;
        }

        return mousePosition.y > input.ScreenHeight - edgeThreshold ? 1f : 0f;
    }

    private static bool IsUsableEdgeScrollPointer(IPlayerInputReader input, Vector3 mousePosition)
    {
        return Application.isFocused
            && mousePosition.x > 0f
            && mousePosition.y > 0f
            && mousePosition.x < input.ScreenWidth - 1f
            && mousePosition.y < input.ScreenHeight - 1f;
    }

    private IMainCameraProvider RequireMainCameraProvider()
    {
        return mainCameraProvider
            ?? throw new InvalidOperationException($"{nameof(CameraManager)} requires {nameof(IMainCameraProvider)} injection.");
    }

    private IPlayerInputReader RequireInputReader()
    {
        if (inputReader != null)
        {
            return inputReader;
        }

        fallbackInputReader ??= new UnityPlayerInputReader();
        return fallbackInputReader;
    }

    private Camera ResolveMainCamera()
    {
        if (mainCameraProvider != null)
        {
            Camera injectedCamera = mainCameraProvider.Camera;
            if (injectedCamera != null)
            {
                return injectedCamera;
            }
        }

        return Camera.main ?? FindFirstObjectByType<Camera>(FindObjectsInactive.Include);
    }

    private Grid RequireGrid()
    {
        if (gridSystemProvider != null && gridSystemProvider.TryGetGrid(out Grid injectedGrid))
        {
            return injectedGrid;
        }

        GridSystemManager manager = RequireGridSystemManager();
        if (manager.grid == null)
        {
            throw new InvalidOperationException($"{nameof(GridSystemManager)} did not initialize its {nameof(Grid)}.");
        }

        return manager.grid;
    }

    private GridSystemManager RequireGridSystemManager()
    {
        if (TryResolveGridSystemManager(out GridSystemManager manager))
        {
            return manager;
        }

        throw new InvalidOperationException($"{nameof(CameraManager)} requires a loaded {nameof(GridSystemManager)}.");
    }

    private bool TryResolveGridSystemManager(out GridSystemManager manager)
    {
        if (gridSystemProvider != null && gridSystemProvider.TryGetManager(out manager))
        {
            return true;
        }

        fallbackGridSystemManager ??= FindFirstObjectByType<GridSystemManager>(FindObjectsInactive.Include);
        if (fallbackGridSystemManager == null)
        {
            manager = null;
            return false;
        }

        fallbackGridSystemManager.EnsureGridInitialized();
        manager = fallbackGridSystemManager;
        return true;
    }
}
