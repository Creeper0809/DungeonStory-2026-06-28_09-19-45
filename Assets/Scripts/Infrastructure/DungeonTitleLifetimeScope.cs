using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

public sealed class DungeonTitleLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        Scene titleScene = gameObject.scene;
        DungeonSceneComponentQuery sceneQuery = new DungeonSceneComponentQuery(titleScene);
        builder.RegisterInstance(sceneQuery)
            .AsSelf()
            .As<IDungeonSceneComponentQuery>();
        builder.Register<UnityResourcesAssetLoader>(Lifetime.Singleton)
            .As<IResourcesAssetLoader>();
        builder.Register<ResourceTmpKoreanFontProvider>(Lifetime.Singleton)
            .As<ITmpKoreanFontProvider>();
        builder.Register<TmpKoreanFontService>(Lifetime.Singleton)
            .As<ITmpKoreanFontService>();
        builder.Register<DungeonTitleCanvasProvider>(Lifetime.Singleton)
            .As<IDungeonUiCanvasProvider>();
        builder.Register<DungeonSaveSlotCatalog>(Lifetime.Singleton)
            .As<IDungeonSaveSlotCatalog>();
        builder.Register<DungeonSceneNavigator>(Lifetime.Singleton)
            .As<IDungeonSceneNavigator>();
        builder.RegisterEntryPoint<DungeonUserSettingsService>(Lifetime.Singleton)
            .As<IDungeonUserSettingsService>();
        builder.RegisterEntryPoint<DungeonAudioController>(Lifetime.Singleton)
            .As<IDungeonAudioService>();
        builder.RegisterEntryPoint<DungeonSettingsUiController>(Lifetime.Singleton)
            .As<IDungeonSettingsUi>();
        builder.RegisterEntryPoint<DungeonTitleUiController>(Lifetime.Singleton);
    }
}
