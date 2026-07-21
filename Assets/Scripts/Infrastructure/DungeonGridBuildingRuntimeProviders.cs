using System;
using UnityEngine;

public interface IDungeonGridBuildingControllerProvider
{
    DungeonStoryGridBuildingController Controller { get; }
}

public interface IWorldPointerPositionProvider
{
    Vector3 MouseWorldPosition { get; }
}

public interface IMainCameraProvider
{
    Camera Camera { get; }
}

public interface IGridTextureProvider
{
    GridTexture Texture { get; }
}

public sealed class DungeonGridBuildingControllerProvider : IDungeonGridBuildingControllerProvider
{
    private readonly IDungeonSceneComponentQuery sceneQuery;
    private DungeonStoryGridBuildingController controller;

    public DungeonGridBuildingControllerProvider(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery ?? throw new ArgumentNullException(nameof(sceneQuery));
    }

    public DungeonStoryGridBuildingController Controller
    {
        get
        {
            if (controller == null)
            {
                controller = sceneQuery.First<DungeonStoryGridBuildingController>(includeInactive: true);
            }

            return controller != null
                ? controller
                : throw new InvalidOperationException($"{nameof(IDungeonGridBuildingControllerProvider)} requires a loaded {nameof(DungeonStoryGridBuildingController)}.");
        }
    }
}

public sealed class GridTextureProvider : IGridTextureProvider
{
    private readonly IDungeonSceneComponentQuery sceneQuery;
    private GridTexture texture;

    public GridTextureProvider(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery ?? throw new ArgumentNullException(nameof(sceneQuery));
    }

    public GridTexture Texture
    {
        get
        {
            if (texture == null)
            {
                texture = sceneQuery.First<GridTexture>(includeInactive: true);
            }

            return texture != null
                ? texture
                : throw new InvalidOperationException($"{nameof(IGridTextureProvider)} requires a loaded {nameof(GridTexture)}.");
        }
    }
}

public sealed class SceneCameraWorldPointerPositionProvider : IWorldPointerPositionProvider
{
    private readonly IMainCameraProvider cameraProvider;
    private readonly IPlayerInputReader inputReader;

    public SceneCameraWorldPointerPositionProvider(
        IMainCameraProvider cameraProvider,
        IPlayerInputReader inputReader)
    {
        this.cameraProvider = cameraProvider ?? throw new ArgumentNullException(nameof(cameraProvider));
        this.inputReader = inputReader ?? throw new ArgumentNullException(nameof(inputReader));
    }

    public Vector3 MouseWorldPosition
    {
        get
        {
            Camera camera = cameraProvider.Camera;
            Vector3 mousePosition = inputReader.MousePosition;
            return camera.ScreenToWorldPoint(new Vector3(
                mousePosition.x,
                mousePosition.y,
                -camera.transform.position.z));
        }
    }
}

public sealed class SceneMainCameraProvider : IMainCameraProvider
{
    private readonly IDungeonSceneComponentQuery sceneQuery;
    private Camera camera;

    public SceneMainCameraProvider(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery ?? throw new ArgumentNullException(nameof(sceneQuery));
    }

    public Camera Camera
    {
        get
        {
            if (camera == null)
            {
                camera = sceneQuery.First<Camera>(includeInactive: true);
            }

            return camera != null
                ? camera
                : throw new InvalidOperationException($"{nameof(IMainCameraProvider)} requires a loaded {nameof(Camera)}.");
        }
    }
}
