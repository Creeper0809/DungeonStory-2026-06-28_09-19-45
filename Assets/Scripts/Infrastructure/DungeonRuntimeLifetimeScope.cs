using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

public sealed class DungeonRuntimeLifetimeScope : LifetimeScope
{
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void Configure(IContainerBuilder builder)
    {
        Scene scopeScene = gameObject.scene;
        DungeonSceneComponentQuery sceneQuery = new DungeonSceneComponentQuery(scopeScene);
        builder.RegisterInstance(sceneQuery)
            .AsSelf()
            .As<IDungeonSceneComponentQuery>();
        builder.RegisterEntryPoint<SceneBuildableLeakValidator>(Lifetime.Singleton);
        builder.Register<UnityResourcesAssetLoader>(Lifetime.Singleton)
            .As<IResourcesAssetLoader>();
        builder.Register<ResourceDataScriptableObjectSource>(Lifetime.Singleton)
            .As<IDataScriptableObjectSource>();
        builder.Register<DataManager>(Lifetime.Singleton);
        builder.Register<DataManagerCatalog>(Lifetime.Singleton)
            .As<IDataCatalog>();
        builder.Register<BuildingDefinitionLookup>(Lifetime.Singleton)
            .As<IBuildingDefinitionLookup>();
        builder.Register<BuildingSummaryFormatter>(Lifetime.Singleton)
            .As<IBuildingSummaryFormatter>();
        builder.Register<GameManagerGameDataProvider>(Lifetime.Singleton)
            .As<IGameDataProvider>();
        builder.Register<GameManagerFloatingNumberFeedbackService>(Lifetime.Singleton)
            .As<IFloatingNumberFeedbackService>();
        builder.Register<UnityPlayerInputReader>(Lifetime.Singleton)
            .As<IPlayerInputReader>();
        builder.Register<EventSystemUiPointerBlocker>(Lifetime.Singleton)
            .As<IUiPointerBlocker>();
        builder.Register<PhysicsWorldPointerRaycaster>(Lifetime.Singleton)
            .As<IWorldPointerRaycaster>();
        builder.Register<ResourceTmpKoreanFontProvider>(Lifetime.Singleton)
            .As<ITmpKoreanFontProvider>();
        builder.Register<TmpKoreanFontService>(Lifetime.Singleton)
            .As<ITmpKoreanFontService>();
        builder.Register<ShopStockCatalog>(Lifetime.Singleton)
            .As<IShopStockCatalog>();
        builder.Register<GridSystemProvider>(Lifetime.Singleton)
            .As<IGridSystemProvider>();
        builder.Register<DungeonBackdropReferenceProvider>(Lifetime.Singleton)
            .As<IDungeonBackdropReferenceProvider>();
        builder.Register<DungeonBackdropSpriteTilingFactory>(Lifetime.Singleton)
            .As<IDungeonBackdropSpriteTilingFactory>();
        builder.Register<WorldInfoClickSelectionService>(Lifetime.Singleton)
            .As<IWorldInfoClickSelector>();
        builder.RegisterEntryPoint<WorldInfoClickInputController>(Lifetime.Singleton);
        builder.Register<DungeonGridBuildingControllerProvider>(Lifetime.Singleton)
            .As<IDungeonGridBuildingControllerProvider>();
        builder.Register<SceneMainCameraProvider>(Lifetime.Singleton)
            .As<IMainCameraProvider>();
        builder.Register<SceneCameraWorldPointerPositionProvider>(Lifetime.Singleton)
            .As<IWorldPointerPositionProvider>();
        builder.Register<GridTextureProvider>(Lifetime.Singleton)
            .As<IGridTextureProvider>();
        builder.Register<GridBuildingObjectFactory>(Lifetime.Singleton)
            .As<IGridBuildingObjectFactory>();
        builder.Register<ModularFacilityWorldSaveService>(Lifetime.Singleton)
            .As<IModularFacilityWorldSaveService>();
        builder.Register<GridGhostObjectResolver>(Lifetime.Singleton)
            .As<IGridGhostObjectResolver>();
        builder.Register<GridConstructButtonFactory>(Lifetime.Singleton)
            .As<IGridConstructButtonFactory>();
        builder.Register<FacilityEvolutionRuntimeProvider>(Lifetime.Singleton)
            .As<IFacilityEvolutionRuntimeProvider>();
        builder.Register<FacilitySynthesisRuntimeProvider>(Lifetime.Singleton)
            .As<IFacilitySynthesisRuntimeProvider>();
        builder.Register<DataCatalogFacilitySynthesisRecipeCatalog>(Lifetime.Singleton)
            .As<IFacilitySynthesisRecipeCatalog>();
        builder.Register<FacilitySynthesisRecipeQuery>(Lifetime.Singleton)
            .As<IFacilitySynthesisRecipeQuery>();
        builder.Register<DataCatalogFacilityEvolutionRecipeProvider>(Lifetime.Singleton)
            .As<IFacilityEvolutionRecipeProvider>();
        builder.Register<FacilityEvolutionRecipeQuery>(Lifetime.Singleton)
            .As<IFacilityEvolutionRecipeQuery>();
        builder.Register<DataCatalogFacilityEvolutionRecordTokenDefinitionProvider>(Lifetime.Singleton)
            .As<IFacilityEvolutionRecordTokenDefinitionProvider>();
        builder.Register<DefaultFacilityEvolutionRecordTokenConsumer>(Lifetime.Singleton)
            .As<IFacilityEvolutionRecordTokenConsumer>();
        builder.Register<GridFacilityEvolutionBuildingReplacerFactory>(Lifetime.Singleton)
            .As<IFacilityEvolutionBuildingReplacerFactory>();
        builder.Register<FacilityEvolutionRecordComponentFactory>(Lifetime.Singleton)
            .As<IFacilityEvolutionRecordComponentFactory>();
        builder.Register<FacilityEvolutionRecordComponentService>(Lifetime.Singleton)
            .As<IFacilityEvolutionRecordComponentService>()
            .As<IFacilityEvolutionRecordProvider>();
        builder.Register<FacilityEvolutionRecordEventRecorder>(Lifetime.Singleton)
            .As<IFacilityEvolutionRecordEventRecorder>();
        builder.Register<FacilityEvolutionStateComponentFactory>(Lifetime.Singleton)
            .As<IFacilityEvolutionStateComponentFactory>();
        builder.Register<DataCatalogCodexReferenceCatalog>(Lifetime.Singleton)
            .As<ICodexReferenceCatalog>();
        builder.Register<CodexReferenceImporter>(Lifetime.Singleton)
            .As<ICodexReferenceImporter>();
        builder.Register<CodexRuntimeProvider>(Lifetime.Singleton)
            .As<ICodexRuntimeProvider>();
        builder.Register<InvasionThreatRuntimeProvider>(Lifetime.Singleton)
            .As<IInvasionThreatRuntimeProvider>();
        builder.Register<InvasionThreatWorldSampler>(Lifetime.Singleton)
            .As<IInvasionThreatWorldSampler>();
        builder.Register<ResourceInvasionIntruderDataProvider>(Lifetime.Singleton)
            .As<IInvasionIntruderDataProvider>();
        builder.Register<InvasionIntruderContext>(Lifetime.Singleton)
            .As<IInvasionIntruderContext>();
        builder.Register<InvasionIntruderRuntimeFactory>(Lifetime.Singleton)
            .As<IInvasionIntruderFactory>();
        builder.Register<DefenseStatusRuntimeFactory>(Lifetime.Singleton)
            .As<IDefenseStatusRuntimeFactory>();
        builder.Register<DefenseStatusRuntimeService>(Lifetime.Singleton)
            .As<IDefenseStatusRuntimeService>();
        builder.Register<RunVariableRuntimeProvider>(Lifetime.Singleton)
            .As<IRunVariableRuntimeProvider>();
        builder.Register<RunVariableRuntimeReader>(Lifetime.Singleton)
            .As<IRunVariableRuntimeReader>();
        builder.Register<ResourceRunCharacterCatalog>(Lifetime.Singleton)
            .As<IRunCharacterCatalog>();
        builder.Register<ResourceOwnerCandidateCatalog>(Lifetime.Singleton)
            .As<IOwnerCandidateCatalog>();
        builder.Register<RunStartVariableCatalog>(Lifetime.Singleton)
            .As<IRunStartVariableCatalog>();
        builder.Register<RunStartVariableSelector>(Lifetime.Singleton)
            .As<IRunStartVariableSelector>();
        builder.Register<CharacterVisualRootFactory>(Lifetime.Singleton)
            .As<ICharacterVisualRootFactory>();
        builder.Register<OwnerRunDataProvider>(Lifetime.Singleton)
            .As<IOwnerRunDataProvider>()
            .As<IOwnerRunManagerProvider>();
        builder.Register<OwnerCharacterFactory>(Lifetime.Singleton)
            .As<IOwnerCharacterFactory>();
        builder.Register<OwnerSelectionOptionButtonFactory>(Lifetime.Singleton)
            .As<IOwnerSelectionOptionButtonFactory>();
        builder.Register<OwnerRunLifecycleService>(Lifetime.Singleton)
            .As<IOwnerRunLifecycleService>();
        builder.Register<CharacterSpawnerProvider>(Lifetime.Singleton)
            .As<ICharacterSpawnerProvider>();
        builder.Register<CharacterSpawnObjectFactory>(Lifetime.Singleton)
            .As<ICharacterSpawnObjectFactory>();
        builder.Register<StaffDiscontentRuntimeProvider>(Lifetime.Singleton)
            .As<IStaffDiscontentRuntimeProvider>();
        builder.Register<StaffDiscontentRuntimeService>(Lifetime.Singleton)
            .As<IStaffDiscontentRuntimeService>();
        builder.Register<DungeonWorkforceReplanService>(Lifetime.Singleton)
            .As<IWorkforceReplanService>();
        builder.Register<LocalLlmRuntimeProvider>(Lifetime.Singleton)
            .As<ILocalLlmRuntimeProvider>();
        builder.Register<CharacterLogNarrativeService>(Lifetime.Singleton)
            .AsSelf()
            .As<ICharacterLogNarrativeService>();
        builder.Register<AiDirectorContextSceneQuery>(Lifetime.Singleton)
            .As<IAiDirectorContextSceneQuery>();
        builder.Register<CharacterAiFacilityLookup>(Lifetime.Singleton)
            .As<ICharacterAiFacilityLookup>();
        builder.Register<FacilityCandidateCacheStore>(Lifetime.Singleton)
            .As<IFacilityCandidateCache>();
        builder.Register<RoomLayoutCache>(Lifetime.Singleton)
            .As<IRoomLayoutCache>();
        builder.Register<ResourceRoomEnvironmentSettingsProvider>(Lifetime.Singleton)
            .As<IRoomEnvironmentSettingsProvider>();
        builder.Register<RoomEnvironmentEvaluator>(Lifetime.Singleton)
            .As<IRoomEnvironmentEvaluator>();
        builder.RegisterEntryPoint<RoomEnvironmentExperienceService>(Lifetime.Singleton);
        builder.RegisterEntryPoint<RoomInspectionRuntime>(Lifetime.Singleton);
        builder.Register<RoomFacilityPolicyService>(Lifetime.Singleton)
            .As<IRoomFacilityPolicy>();
        builder.Register<CharacterBehaviorTreeRuntimeConfigurator>(Lifetime.Singleton)
            .As<ICharacterBehaviorTreeRuntimeConfigurator>();
        builder.Register<CharacterAiSchedulingService>(Lifetime.Singleton)
            .As<ICharacterAiSchedulingService>();
        builder.Register<CharacterSocialMemoryFactory>(Lifetime.Singleton)
            .As<ICharacterSocialMemoryFactory>();
        builder.Register<CharacterFeedbackBubbleFactory>(Lifetime.Singleton)
            .As<ICharacterFeedbackBubbleFactory>();
        builder.Register<CharacterFeedbackBubbleViewFactory>(Lifetime.Singleton)
            .As<ICharacterFeedbackBubbleViewFactory>();
        builder.Register<CharacterDialogueBubbleFactory>(Lifetime.Singleton)
            .As<ICharacterDialogueBubbleFactory>();
        builder.Register<CharacterAiJobGiverCatalog>(Lifetime.Singleton)
            .As<ICharacterAiJobGiverCatalog>();
        builder.Register<CharacterAiDecisionPipeline>(Lifetime.Singleton)
            .As<ICharacterAiDecisionPipeline>();
        builder.Register<ResourceCharacterAiActionAssetCatalog>(Lifetime.Singleton)
            .As<ICharacterAiActionAssetCatalog>();
        builder.Register<SocialReputationRuntimeProvider>(Lifetime.Singleton)
            .As<ISocialReputationRuntimeProvider>();
        builder.Register<SocialReputationBiasService>(Lifetime.Singleton)
            .As<ISocialReputationBiasService>();
        builder.Register<RegularCustomerRuntimeProvider>(Lifetime.Singleton)
            .As<IRegularCustomerRuntimeProvider>();
        builder.Register<DailyFacilityShopRuntimeProvider>(Lifetime.Singleton)
            .As<IDailyFacilityShopRuntimeProvider>();
        builder.Register<DataCatalogFacilityShopCatalog>(Lifetime.Singleton)
            .As<IFacilityShopCatalog>();
        builder.Register<FacilityShopUnlockStateService>(Lifetime.Singleton)
            .As<IFacilityShopUnlockStateService>();
        builder.Register<BlueprintResearchRuntimeProvider>(Lifetime.Singleton)
            .As<IBlueprintResearchRuntimeProvider>();
        builder.Register<BlueprintResearchWorkService>(Lifetime.Singleton)
            .As<IBlueprintResearchWorkService>();
        builder.Register<BlueprintResearchStateService>(Lifetime.Singleton)
            .As<IBlueprintResearchStateService>();
        builder.Register<MetaProgressionRuntimeProvider>(Lifetime.Singleton)
            .As<IMetaProgressionRuntimeProvider>();
        builder.Register<MetaProgressionRuntimeReader>(Lifetime.Singleton)
            .As<IMetaProgressionRuntimeReader>();
        builder.Register<OffenseWorldMapRuntimeProvider>(Lifetime.Singleton)
            .As<IOffenseWorldMapRuntimeProvider>();
        builder.Register<OffenseRewardRuntimeProvider>(Lifetime.Singleton)
            .As<IOffenseRewardRuntimeProvider>();
        builder.Register<OffenseExpeditionMemberQuery>(Lifetime.Singleton)
            .As<IOffenseExpeditionMemberQuery>();
        builder.Register<DataCatalogOffenseRewardCatalog>(Lifetime.Singleton)
            .As<IOffenseRewardCatalog>();
        builder.Register<OffenseRewardSelector>(Lifetime.Singleton)
            .As<IOffenseRewardSelector>();
        builder.Register<OffenseRewardGrantService>(Lifetime.Singleton)
            .As<IOffenseRewardGrantService>();
        builder.Register<OffensePanelFactory>(Lifetime.Singleton)
            .As<IOffensePanelFactory>();
        builder.Register<OffensePanelButtonFactory>(Lifetime.Singleton)
            .As<IOffensePanelButtonFactory>();
        builder.Register<OffensePanelService>(Lifetime.Singleton)
            .As<IOffensePanelService>();
        builder.Register<OffenseRewardContextBuilder>(Lifetime.Singleton)
            .As<IOffenseRewardContextBuilder>();
        builder.Register<MetaRunResultBuilder>(Lifetime.Singleton)
            .As<IMetaRunResultBuilder>();
        builder.Register<RunResultPanelFactory>(Lifetime.Singleton)
            .As<IRunResultPanelFactory>();
        builder.Register<RunResultPanelService>(Lifetime.Singleton)
            .As<IRunResultPanelService>();
        builder.Register<CodexPanelFactory>(Lifetime.Singleton)
            .As<ICodexPanelFactory>();
        builder.Register<FacilitySynthesisPanelFactory>(Lifetime.Singleton)
            .As<IFacilitySynthesisPanelFactory>();
        builder.Register<FacilityEvolutionPanelFactory>(Lifetime.Singleton)
            .As<IFacilityEvolutionPanelFactory>();
        builder.Register<StaffWorkPriorityPanelUiFactory>(Lifetime.Singleton)
            .As<IStaffWorkPriorityPanelUiFactory>();
        builder.Register<StaffWorkPriorityPanelModelBuilder>(Lifetime.Singleton)
            .As<IStaffWorkPriorityPanelModelBuilder>();
        builder.Register<StaffWorkPriorityPanelFactory>(Lifetime.Singleton)
            .As<IStaffWorkPriorityPanelFactory>();
        builder.Register<P0FeatureSurfacePanelFactory>(Lifetime.Singleton)
            .As<IP0FeatureSurfacePanelFactory>();
        builder.Register<UITabGeneratedPanelFactory>(Lifetime.Singleton)
            .As<IUITabGeneratedPanelFactory>();
        builder.Register<UITabTopButtonFactory>(Lifetime.Singleton)
            .As<IUITabTopButtonFactory>();
        builder.Register<EventAlertButtonFactory>(Lifetime.Singleton)
            .As<IEventAlertButtonFactory>();
        builder.Register<EventAlertViewUiFactory>(Lifetime.Singleton)
            .As<IEventAlertViewUiFactory>();
        builder.Register<EventAlertViewPresenterFactory>(Lifetime.Singleton)
            .As<IEventAlertViewPresenterFactory>();
        builder.Register<EventAlertCanvasProvider>(Lifetime.Singleton)
            .As<IEventAlertCanvasProvider>();
        builder.Register<NoticeFeedItemFactory>(Lifetime.Singleton)
            .As<INoticeFeedItemFactory>();
        builder.Register<NoticeFeedPresenter>(Lifetime.Singleton)
            .As<INoticeFeedPresenter>();
        builder.Register<CharacterSummaryRuntimeLogFactory>(Lifetime.Singleton)
            .As<ICharacterSummaryRuntimeLogFactory>();
        builder.RegisterInstance(CaptureSceneRuntimeReferences(sceneQuery));
        builder.Register<StaffWorkforceRuntimeQueryService>(Lifetime.Singleton)
            .As<IStaffWorkforceQueryService>();
        builder.Register<WorkGridResolver>(Lifetime.Singleton)
            .As<IWorkGridResolver>();
        builder.Register<BuildingManagementSummaryService>(Lifetime.Singleton)
            .As<IBuildingManagementSummaryService>();
        builder.Register<InvasionDefenseSummaryService>(Lifetime.Singleton)
            .As<IInvasionDefenseSummaryService>();
        builder.Register<OffenseTabSummaryService>(Lifetime.Singleton)
            .As<IOffenseTabSummaryService>();
        builder.Register<OperationTabSummaryService>(Lifetime.Singleton)
            .As<IOperationTabSummaryService>();
        builder.Register<ResearchCraftingSummaryService>(Lifetime.Singleton)
            .As<IResearchCraftingSummaryService>();
        builder.Register<CodexRecordSummaryService>(Lifetime.Singleton)
            .As<ICodexRecordSummaryService>();
        builder.Register<UITabContentTextProvider>(Lifetime.Singleton)
            .As<IUITabContentTextProvider>();
        builder.Register<UiPopupService>(Lifetime.Singleton)
            .As<IUiPopupService>();
        builder.Register<GameManagerFloatingIconFeedbackService>(Lifetime.Singleton)
            .As<IFloatingIconFeedbackService>();

        builder.RegisterBuildCallback(InjectSceneComponents);
    }

    private static void InjectSceneComponents(IObjectResolver resolver)
    {
        IDungeonSceneComponentQuery sceneQuery = resolver.Resolve<IDungeonSceneComponentQuery>();

        foreach (UIManager uiManager in sceneQuery.All<UIManager>(includeInactive: true))
        {
            resolver.Inject(uiManager);
        }

        foreach (BuildingSummaryInfo summaryInfo in sceneQuery.All<BuildingSummaryInfo>(includeInactive: true))
        {
            resolver.Inject(summaryInfo);
        }

        foreach (UIBuildingInfo buildingInfo in sceneQuery.All<UIBuildingInfo>(includeInactive: true))
        {
            resolver.Inject(buildingInfo);
        }

        foreach (CharacterSummeryInfo summaryInfo in sceneQuery.All<CharacterSummeryInfo>(includeInactive: true))
        {
            resolver.Inject(summaryInfo);
        }

        foreach (BuildableObject building in sceneQuery.All<BuildableObject>(includeInactive: true))
        {
            resolver.Inject(building);
        }

        foreach (OwnerSelectionPanel ownerSelectionPanel in sceneQuery.All<OwnerSelectionPanel>(includeInactive: true))
        {
            resolver.Inject(ownerSelectionPanel);
        }

        foreach (OwnerCommandController ownerCommandController in sceneQuery.All<OwnerCommandController>(includeInactive: true))
        {
            resolver.Inject(ownerCommandController);
        }

        foreach (CharacterLog characterLog in sceneQuery.All<CharacterLog>(includeInactive: true))
        {
            resolver.Inject(characterLog);
        }

        foreach (CharacterActor actor in sceneQuery.All<CharacterActor>(includeInactive: true))
        {
            resolver.Inject(actor);
        }

        foreach (CharacterAbility ability in sceneQuery.All<CharacterAbility>(includeInactive: true))
        {
            resolver.Inject(ability);
        }

        foreach (CharacterLifecycle lifecycle in sceneQuery.All<CharacterLifecycle>(includeInactive: true))
        {
            resolver.Inject(lifecycle);
        }

        foreach (CharacterStats stats in sceneQuery.All<CharacterStats>(includeInactive: true))
        {
            resolver.Inject(stats);
        }

        foreach (CharacterFeedbackBubble feedbackBubble in sceneQuery.All<CharacterFeedbackBubble>(includeInactive: true))
        {
            resolver.Inject(feedbackBubble);
        }

        foreach (CharacterAiScheduler scheduler in sceneQuery.All<CharacterAiScheduler>(includeInactive: true))
        {
            resolver.Inject(scheduler);
        }

        foreach (StaffDiscontentRuntime runtime in sceneQuery.All<StaffDiscontentRuntime>(includeInactive: true))
        {
            resolver.Inject(runtime);
        }

        foreach (CustomerPersonaRuntime personaRuntime in sceneQuery.All<CustomerPersonaRuntime>(includeInactive: true))
        {
            resolver.Inject(personaRuntime);
        }

        foreach (CharacterDialogueRuntime dialogueRuntime in sceneQuery.All<CharacterDialogueRuntime>(includeInactive: true))
        {
            resolver.Inject(dialogueRuntime);
        }

        foreach (OwnerRunManager ownerRunManager in sceneQuery.All<OwnerRunManager>(includeInactive: true))
        {
            resolver.Inject(ownerRunManager);
        }

        foreach (UITab tab in sceneQuery.All<UITab>(includeInactive: true))
        {
            resolver.Inject(tab);
        }

        foreach (UITabManager manager in sceneQuery.All<UITabManager>(includeInactive: true))
        {
            resolver.Inject(manager);
        }

        foreach (NoticeFeed noticeFeed in sceneQuery.All<NoticeFeed>(includeInactive: true))
        {
            resolver.Inject(noticeFeed);
        }

        foreach (StaffWorkPriorityPanel panel in sceneQuery.All<StaffWorkPriorityPanel>(includeInactive: true))
        {
            resolver.Inject(panel);
        }

        foreach (P0FeatureSurfacePanel panel in sceneQuery.All<P0FeatureSurfacePanel>(includeInactive: true))
        {
            resolver.Inject(panel);
        }

        foreach (CameraManager cameraManager in sceneQuery.All<CameraManager>(includeInactive: true))
        {
            resolver.Inject(cameraManager);
        }

        foreach (DungeonSceneBackdropFitter backdropFitter in sceneQuery.All<DungeonSceneBackdropFitter>(includeInactive: true))
        {
            resolver.Inject(backdropFitter);
        }

        foreach (DungeonStoryGridBuildingController controller in sceneQuery.All<DungeonStoryGridBuildingController>(includeInactive: true))
        {
            resolver.Inject(controller);
        }

        foreach (GridUIManager gridUiManager in sceneQuery.All<GridUIManager>(includeInactive: true))
        {
            resolver.Inject(gridUiManager);
        }

        foreach (DungeonStoryGridGhostPresenter ghostPresenter in sceneQuery.All<DungeonStoryGridGhostPresenter>(includeInactive: true))
        {
            resolver.Inject(ghostPresenter);
        }

        foreach (CharacterSpawner spawner in sceneQuery.All<CharacterSpawner>(includeInactive: true))
        {
            resolver.Inject(spawner);
        }

        foreach (UIBuildingSelectButton selectButton in sceneQuery.All<UIBuildingSelectButton>(includeInactive: true))
        {
            resolver.Inject(selectButton);
        }

        foreach (FacilityEvolutionPanel panel in sceneQuery.All<FacilityEvolutionPanel>(includeInactive: true))
        {
            resolver.Inject(panel);
        }

        foreach (FacilityEvolutionRuntime runtime in sceneQuery.All<FacilityEvolutionRuntime>(includeInactive: true))
        {
            resolver.Inject(runtime);
        }

        foreach (FacilityEvolutionRecordRuntime runtime in sceneQuery.All<FacilityEvolutionRecordRuntime>(includeInactive: true))
        {
            resolver.Inject(runtime);
        }

        foreach (FacilitySynthesisPanel panel in sceneQuery.All<FacilitySynthesisPanel>(includeInactive: true))
        {
            resolver.Inject(panel);
        }

        foreach (FacilitySynthesisRuntime runtime in sceneQuery.All<FacilitySynthesisRuntime>(includeInactive: true))
        {
            resolver.Inject(runtime);
        }

        foreach (CodexPanel panel in sceneQuery.All<CodexPanel>(includeInactive: true))
        {
            resolver.Inject(panel);
        }

        foreach (CodexRuntime runtime in sceneQuery.All<CodexRuntime>(includeInactive: true))
        {
            resolver.Inject(runtime);
        }

        foreach (OperatingDaySettlementRuntime runtime in sceneQuery.All<OperatingDaySettlementRuntime>(includeInactive: true))
        {
            resolver.Inject(runtime);
        }

        foreach (BlueprintResearchRuntime runtime in sceneQuery.All<BlueprintResearchRuntime>(includeInactive: true))
        {
            resolver.Inject(runtime);
        }

        foreach (MetaProgressionRuntime runtime in sceneQuery.All<MetaProgressionRuntime>(includeInactive: true))
        {
            resolver.Inject(runtime);
        }

        foreach (RunVariableRuntime runtime in sceneQuery.All<RunVariableRuntime>(includeInactive: true))
        {
            resolver.Inject(runtime);
        }

        foreach (DailyFacilityShopRuntime runtime in sceneQuery.All<DailyFacilityShopRuntime>(includeInactive: true))
        {
            resolver.Inject(runtime);
        }

        foreach (AiDirectorRuntime runtime in sceneQuery.All<AiDirectorRuntime>(includeInactive: true))
        {
            resolver.Inject(runtime);
        }

        foreach (SocialReputationRuntime runtime in sceneQuery.All<SocialReputationRuntime>(includeInactive: true))
        {
            resolver.Inject(runtime);
        }

        foreach (InvasionThreatRuntime runtime in sceneQuery.All<InvasionThreatRuntime>(includeInactive: true))
        {
            resolver.Inject(runtime);
        }

        foreach (EventAlertRuntime runtime in sceneQuery.All<EventAlertRuntime>(includeInactive: true))
        {
            resolver.Inject(runtime);
        }

        foreach (InvasionDirectorRuntime runtime in sceneQuery.All<InvasionDirectorRuntime>(includeInactive: true))
        {
            resolver.Inject(runtime);
        }

        foreach (OffenseWorldMapRuntime runtime in sceneQuery.All<OffenseWorldMapRuntime>(includeInactive: true))
        {
            resolver.Inject(runtime);
        }

        foreach (OffenseExpeditionRuntime runtime in sceneQuery.All<OffenseExpeditionRuntime>(includeInactive: true))
        {
            resolver.Inject(runtime);
        }

        foreach (OffenseRewardRuntime runtime in sceneQuery.All<OffenseRewardRuntime>(includeInactive: true))
        {
            resolver.Inject(runtime);
        }
    }

    private static DungeonSceneRuntimeReferences CaptureSceneRuntimeReferences(IDungeonSceneComponentQuery sceneQuery)
    {
        return new DungeonSceneRuntimeReferences(
            sceneQuery.First<UIManager>(includeInactive: true),
            sceneQuery.First<OperatingDaySettlementRuntime>(includeInactive: true),
            sceneQuery.First<EventAlertRuntime>(includeInactive: true),
            sceneQuery.First<RunVariableRuntime>(includeInactive: true),
            sceneQuery.First<Canvas>(includeInactive: true));
    }
}
