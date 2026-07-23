using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

public sealed class DungeonPreparationCanvasProvider : IDungeonUiCanvasProvider
{
    private Canvas canvas;

    public Canvas GetOrCreateCanvas()
    {
        if (canvas != null)
        {
            return canvas;
        }

        EnsureEventSystem();
        GameObject canvasObject = new GameObject(
            "PreparationCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static void EnsureEventSystem()
    {
        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }
}

public sealed class DungeonPreparationLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        Scene preparationScene = gameObject.scene;
        DungeonSceneComponentQuery sceneQuery = new DungeonSceneComponentQuery(preparationScene);
        builder.RegisterInstance(sceneQuery)
            .AsSelf()
            .As<IDungeonSceneComponentQuery>();
        builder.Register<UnityResourcesAssetLoader>(Lifetime.Singleton)
            .As<IResourcesAssetLoader>();
        builder.Register<ResourceTmpKoreanFontProvider>(Lifetime.Singleton)
            .As<ITmpKoreanFontProvider>();
        builder.Register<TmpKoreanFontService>(Lifetime.Singleton)
            .As<ITmpKoreanFontService>();
        builder.Register<DungeonPreparationCanvasProvider>(Lifetime.Singleton)
            .As<IDungeonUiCanvasProvider>();
        builder.Register<DungeonSceneNavigator>(Lifetime.Singleton)
            .As<IDungeonSceneNavigator>();
        builder.Register<ResourceRunCharacterCatalog>(Lifetime.Singleton)
            .As<IRunCharacterCatalog>();
        builder.Register<ResourceOwnerCandidateCatalog>(Lifetime.Singleton)
            .As<IOwnerCandidateCatalog>();
        builder.Register<OwnerRunDataProvider>(Lifetime.Singleton)
            .As<IOwnerRunDataProvider>()
            .As<IOwnerRunManagerProvider>();
        builder.Register<CharacterSpawnerProvider>(Lifetime.Singleton)
            .As<ICharacterSpawnerProvider>();
        builder.Register<GridSystemProvider>(Lifetime.Singleton)
            .As<IGridSystemProvider>();
        builder.Register<CharacterSpawnObjectFactory>(Lifetime.Singleton)
            .As<ICharacterSpawnObjectFactory>();
        builder.Register<RunVariableRuntimeProvider>(Lifetime.Singleton)
            .As<IRunVariableRuntimeProvider>();
        builder.Register<LocalLlmRuntimeProvider>(Lifetime.Singleton)
            .As<ILocalLlmRuntimeProvider>();
        builder.Register<ResourceCharacterSkillSystemSettingsProvider>(Lifetime.Singleton)
            .As<ICharacterSkillSystemSettingsProvider>();
        builder.RegisterComponentOnNewGameObject<LocalLlmRequestQueue>(
                Lifetime.Singleton,
                nameof(LocalLlmRequestQueue))
            .UnderTransform(transform);
        builder.RegisterEntryPoint<CharacterSkillGenerationService>(Lifetime.Singleton)
            .As<ICharacterSkillGenerationService>();
        builder.Register<StartPartyPreparationService>(Lifetime.Singleton)
            .As<IStartPartyPreparationService>();
        builder.RegisterEntryPoint<StartPartyPreparationUiController>(Lifetime.Singleton);

        builder.RegisterBuildCallback(resolver =>
        {
            InjectSceneHierarchy(resolver, preparationScene);
            resolver.Resolve<LocalLlmRequestQueue>();
        });
    }

    private static void InjectSceneHierarchy(IObjectResolver resolver, Scene scene)
    {
        if (!scene.IsValid())
        {
            return;
        }

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            resolver.InjectGameObject(root);
        }
    }
}
