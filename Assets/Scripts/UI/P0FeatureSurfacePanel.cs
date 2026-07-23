using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public interface IP0FeatureSurfacePanelFactory
{
    P0FeatureSurfacePanel Ensure(GameObject panelObject, TabId tabId);
}

public sealed class P0FeatureSurfacePanelFactory : IP0FeatureSurfacePanelFactory
{
    private readonly IObjectResolver objectResolver;

    public P0FeatureSurfacePanelFactory(IObjectResolver objectResolver)
    {
        this.objectResolver = objectResolver
            ?? throw new ArgumentNullException(nameof(objectResolver));
    }

    public P0FeatureSurfacePanel Ensure(GameObject panelObject, TabId tabId)
    {
        if (panelObject == null)
        {
            throw new ArgumentNullException(nameof(panelObject));
        }

        P0FeatureSurfacePanel panel = panelObject.GetComponent<P0FeatureSurfacePanel>();
        if (panel == null)
        {
            panel = panelObject.AddComponent<P0FeatureSurfacePanel>();
        }

        panel.SetTabId(tabId);
        objectResolver.Inject(panel);
        return panel;
    }
}

public sealed partial class P0FeatureSurfacePanel : MonoBehaviour
{
    private const float SectionSpacing = 10f;
    private const float CardHeight = 86f;
    private const float CompactCardHeight = 66f;
    private const float EventAlertSafeRightInset = EventAlertLayout.AlertButtonWidth + 32f;
    private const int MaxVisibleCardsPerSection = 8;

    private readonly HashSet<string> completedUiActions = new HashSet<string>();
    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    private TabId? tabId;
    private RectTransform contentRoot;
    private TMP_Text feedbackText;
    private string feedbackMessage = "작업 대기";
    private bool layoutReady;

    private IDailyFacilityShopRuntimeProvider dailyShopProvider;
    private IBlueprintResearchRuntimeProvider researchProvider;
    private IRegularCustomerRuntimeProvider regularCustomerProvider;
    private IMetaProgressionRuntimeProvider metaProvider;
    private IBuildingManagementSummaryService buildingSummaryService;
    private IDungeonSceneComponentQuery sceneQuery;
    private IGameDataProvider gameDataProvider;
    private IRunVariableRuntimeReader runVariableReader;
    private IFacilityShopCatalog facilityShopCatalog;
    private IDefenseStatusRuntimeService defenseStatusRuntimeService;
    private IDefenseResponsePolicyRuntime defenseResponsePolicyRuntime;
    private IDefenseEngagementRuntime defenseEngagementRuntime;
    private IInvasionOwnerEvacuationService ownerEvacuationService;
    private ICombatEquipmentMaintenanceRuntime equipmentMaintenanceRuntime;
    private IStaffWorkforceQueryService staffWorkforceQuery;
    private IRoomLayoutCache roomLayoutCache;
    private IRoomEnvironmentEvaluator roomEnvironmentEvaluator;
    private IRoomInspectionService roomInspectionService;
    private IExteriorZoneQuery exteriorZoneQuery;
    private ISurvivalFoodRuntime survivalFoodRuntime;
    private IWildlifeEcosystemRuntime wildlifeEcosystemRuntime;
    private ITmpKoreanFontService fontService;
    private IFeatureSurfaceTabPresenterRegistry presenterRegistry;
    private IMainCameraProvider mainCameraProvider;

    [Inject]
    public void Construct(
        IDailyFacilityShopRuntimeProvider dailyShopProvider,
        IBlueprintResearchRuntimeProvider researchProvider,
        IRegularCustomerRuntimeProvider regularCustomerProvider,
        IMetaProgressionRuntimeProvider metaProvider,
        IBuildingManagementSummaryService buildingSummaryService,
        IDungeonSceneComponentQuery sceneQuery,
        IGameDataProvider gameDataProvider,
        IRunVariableRuntimeReader runVariableReader,
        IFacilityShopCatalog facilityShopCatalog,
        IDefenseStatusRuntimeService defenseStatusRuntimeService,
        IDefenseResponsePolicyRuntime defenseResponsePolicyRuntime,
        IDefenseEngagementRuntime defenseEngagementRuntime,
        IInvasionOwnerEvacuationService ownerEvacuationService,
        ICombatEquipmentMaintenanceRuntime equipmentMaintenanceRuntime,
        IStaffWorkforceQueryService staffWorkforceQuery,
        IRoomLayoutCache roomLayoutCache,
        IRoomEnvironmentEvaluator roomEnvironmentEvaluator,
        IRoomInspectionService roomInspectionService,
        IExteriorZoneQuery exteriorZoneQuery,
        ISurvivalFoodRuntime survivalFoodRuntime,
        IWildlifeEcosystemRuntime wildlifeEcosystemRuntime,
        ITmpKoreanFontService fontService,
        IFeatureSurfaceTabPresenterRegistry presenterRegistry,
        IMainCameraProvider mainCameraProvider)
    {
        this.dailyShopProvider = dailyShopProvider
            ?? throw new ArgumentNullException(nameof(dailyShopProvider));
        this.researchProvider = researchProvider
            ?? throw new ArgumentNullException(nameof(researchProvider));
        this.regularCustomerProvider = regularCustomerProvider
            ?? throw new ArgumentNullException(nameof(regularCustomerProvider));
        this.metaProvider = metaProvider
            ?? throw new ArgumentNullException(nameof(metaProvider));
        this.buildingSummaryService = buildingSummaryService
            ?? throw new ArgumentNullException(nameof(buildingSummaryService));
        this.sceneQuery = sceneQuery
            ?? throw new ArgumentNullException(nameof(sceneQuery));
        this.gameDataProvider = gameDataProvider
            ?? throw new ArgumentNullException(nameof(gameDataProvider));
        this.runVariableReader = runVariableReader
            ?? throw new ArgumentNullException(nameof(runVariableReader));
        this.facilityShopCatalog = facilityShopCatalog
            ?? throw new ArgumentNullException(nameof(facilityShopCatalog));
        this.defenseStatusRuntimeService = defenseStatusRuntimeService
            ?? throw new ArgumentNullException(nameof(defenseStatusRuntimeService));
        this.defenseResponsePolicyRuntime = defenseResponsePolicyRuntime
            ?? throw new ArgumentNullException(nameof(defenseResponsePolicyRuntime));
        this.defenseEngagementRuntime = defenseEngagementRuntime
            ?? throw new ArgumentNullException(nameof(defenseEngagementRuntime));
        this.ownerEvacuationService = ownerEvacuationService
            ?? throw new ArgumentNullException(nameof(ownerEvacuationService));
        this.equipmentMaintenanceRuntime = equipmentMaintenanceRuntime
            ?? throw new ArgumentNullException(nameof(equipmentMaintenanceRuntime));
        this.staffWorkforceQuery = staffWorkforceQuery
            ?? throw new ArgumentNullException(nameof(staffWorkforceQuery));
        this.roomLayoutCache = roomLayoutCache
            ?? throw new ArgumentNullException(nameof(roomLayoutCache));
        this.roomEnvironmentEvaluator = roomEnvironmentEvaluator
            ?? throw new ArgumentNullException(nameof(roomEnvironmentEvaluator));
        this.roomInspectionService = roomInspectionService
            ?? throw new ArgumentNullException(nameof(roomInspectionService));
        this.exteriorZoneQuery = exteriorZoneQuery
            ?? throw new ArgumentNullException(nameof(exteriorZoneQuery));
        this.survivalFoodRuntime = survivalFoodRuntime
            ?? throw new ArgumentNullException(nameof(survivalFoodRuntime));
        this.wildlifeEcosystemRuntime = wildlifeEcosystemRuntime
            ?? throw new ArgumentNullException(nameof(wildlifeEcosystemRuntime));
        this.fontService = fontService
            ?? throw new ArgumentNullException(nameof(fontService));
        this.presenterRegistry = presenterRegistry
            ?? throw new ArgumentNullException(nameof(presenterRegistry));
        this.mainCameraProvider = mainCameraProvider
            ?? throw new ArgumentNullException(nameof(mainCameraProvider));
    }

    public void SetTabId(TabId id)
    {
        tabId = id;
    }

    private void Awake()
    {
        UITab tab = GetComponent<UITab>();
        UITabIdentity identity = GetComponent<UITabIdentity>();
        if (!tabId.HasValue && identity != null)
        {
            tabId = identity.Id;
        }
        else if (!tabId.HasValue && tab != null && UITabCatalog.TryFromLegacyId(tab.id, out TabId legacyId))
        {
            tabId = legacyId;
        }
    }

    private void Start()
    {
        Refresh();
    }

    private void OnEnable()
    {
        if (layoutReady)
        {
            Refresh();
        }
    }

    public void Refresh()
    {
        EnsureLayout();
        Rebuild();
    }

    private void EnsureLayout()
    {
        if (layoutReady && contentRoot != null)
        {
            return;
        }

        RectTransform host = ResolveBodyHost();
        ClearChildren(host);

        GameObject scrollObject = CreateUiObject("P0SurfaceScroll", host);
        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = Vector2.zero;
        scrollRectTransform.anchorMax = Vector2.one;
        scrollRectTransform.offsetMin = Vector2.zero;
        scrollRectTransform.offsetMax = Vector2.zero;

        Image scrollBackground = scrollObject.AddComponent<Image>();
        scrollBackground.color = DungeonUiTheme.SurfaceMuted;

        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 18f;

        GameObject viewportObject = CreateUiObject("Viewport", scrollRectTransform);
        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(12f, 12f);
        viewportRect.offsetMax = new Vector2(-EventAlertSafeRightInset, -12f);

        Image viewportImage = viewportObject.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
        Mask mask = viewportObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject contentObject = CreateUiObject("Content", viewportRect);
        contentRoot = contentObject.GetComponent<RectTransform>();
        contentRoot.anchorMin = new Vector2(0f, 1f);
        contentRoot.anchorMax = new Vector2(1f, 1f);
        contentRoot.pivot = new Vector2(0.5f, 1f);
        contentRoot.anchoredPosition = Vector2.zero;
        contentRoot.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup vertical = contentObject.AddComponent<VerticalLayoutGroup>();
        vertical.spacing = SectionSpacing;
        vertical.padding = new RectOffset(0, 0, 0, 14);
        vertical.childControlWidth = true;
        vertical.childControlHeight = false;
        vertical.childForceExpandWidth = true;
        vertical.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRoot;
        layoutReady = true;
    }

    private RectTransform ResolveBodyHost()
    {
        Transform body = transform.Find("Body");
        RectTransform host = body as RectTransform;
        TMP_Text placeholder = body != null ? body.GetComponent<TMP_Text>() : null;
        if (placeholder != null)
        {
            placeholder.text = string.Empty;
            placeholder.enabled = false;
        }

        if (host != null)
        {
            return host;
        }

        return GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
    }

    private void Rebuild()
    {
        ClearSpawnedObjects();
        feedbackText = null;

        AddFeedback();

        if (!tabId.HasValue)
        {
            AddEmptyState("기능 패널의 안정 ID가 설정되지 않았습니다.");
            return;
        }

        if (presenterRegistry == null)
        {
            throw new InvalidOperationException(
                $"{nameof(P0FeatureSurfacePanel)} requires {nameof(IFeatureSurfaceTabPresenterRegistry)} injection.");
        }

        if (!presenterRegistry.TryGet(tabId.Value, out IFeatureSurfaceTabPresenter presenter))
        {
            AddEmptyState($"기능 presenter가 등록되지 않은 탭입니다. id={tabId.Value}");
            return;
        }

        presenter.Present(this);
    }

    internal void BuildFacilityShop()
    {
        if (!dailyShopProvider.TryGetRuntime(out DailyFacilityShopRuntime runtime))
        {
            AddEmptyState("시설 상점 런타임이 현재 씬에 없습니다.");
            return;
        }

        GameData gameData = ResolveGameData();
        IReadOnlyList<FacilityShopOffer> dailyOffers = runtime.CurrentDailyOffers;
        IReadOnlyList<FacilityShopOffer> basicOffers = runtime.CurrentBasicPurchaseOffers;

        AddSection(
            "일일 시설 상점",
            $"Day {runtime.CurrentOfferDay} 상품 {dailyOffers.Count}개 / 보유 자금 {FormatMoney(gameData)}");
        if (dailyOffers.Count == 0)
        {
            AddLabel("오늘 구매 가능한 상품이 없습니다.", 20f, 34f);
        }

        IEnumerable<(FacilityShopOffer offer, int index)> orderedDailyOffers = dailyOffers
            .Select((offer, index) => (offer, index))
            .OrderBy(entry => string.Equals(
                entry.offer?.OfferTypeId,
                FacilityShopOfferTypeIds.Blueprint,
                StringComparison.Ordinal) ? 0 : 1)
            .ThenBy(entry => entry.index)
            .Take(MaxVisibleCardsPerSection);
        foreach ((FacilityShopOffer offer, int offerIndex) in orderedDailyOffers)
        {
            int index = offerIndex;
            string actionName = $"P0Action_ShopDaily_{index}";
            CreateOfferCard(
                actionName,
                offer,
                completedUiActions.Contains(actionName) ? "구매됨" : "구매",
                () =>
                {
                    if (completedUiActions.Contains(actionName))
                    {
                        SetFeedback($"{offer.DisplayName}은 이미 이 UI에서 구매했습니다.");
                        return;
                    }

                    GameData resolvedGameData = ResolveGameData();
                    int beforeMoney = GetHoldingMoney(resolvedGameData);
                    bool success = runtime.TryPurchaseDailyOffer(index, resolvedGameData, out FacilityShopPurchaseResult result);
                    int afterMoney = GetHoldingMoney(resolvedGameData);
                    if (success)
                    {
                        completedUiActions.Add(actionName);
                    }

                    SetFeedback(
                        $"{(success ? "구매 성공" : "구매 실패")}: {result.message} / 자금 {beforeMoney} -> {afterMoney}");
                });
        }

        AddSection("기본 구매", $"해금된 기본 구매 후보 {basicOffers.Count}개");
        if (basicOffers.Count == 0)
        {
            AddLabel("기본 구매 후보가 아직 없습니다. 설계도 연구나 계승 강화로 해금됩니다.", 19f, 44f);
        }

        for (int i = 0; i < Mathf.Min(basicOffers.Count, MaxVisibleCardsPerSection); i++)
        {
            int index = i;
            FacilityShopOffer offer = basicOffers[i];
            string actionName = $"P0Action_ShopBasic_{index}";
            CreateOfferCard(
                actionName,
                offer,
                completedUiActions.Contains(actionName) ? "구매됨" : "구매",
                () =>
                {
                    if (completedUiActions.Contains(actionName))
                    {
                        SetFeedback($"{offer.DisplayName}은 이미 이 UI에서 구매했습니다.");
                        return;
                    }

                    GameData resolvedGameData = ResolveGameData();
                    int beforeMoney = GetHoldingMoney(resolvedGameData);
                    bool success = runtime.TryPurchaseBasicOffer(index, resolvedGameData, out FacilityShopPurchaseResult result);
                    int afterMoney = GetHoldingMoney(resolvedGameData);
                    if (success)
                    {
                        completedUiActions.Add(actionName);
                    }

                    SetFeedback(
                        $"{(success ? "기본 구매 성공" : "기본 구매 실패")}: {result.message} / 자금 {beforeMoney} -> {afterMoney}");
                });
        }

        BuildShopOperationsDetail();
    }

    internal void BuildResearch()
    {
        if (!researchProvider.TryGetRuntime(out BlueprintResearchRuntime runtime))
        {
            AddEmptyState("설계도 연구 런타임이 현재 씬에 없습니다.");
            return;
        }

        BlueprintResearchState state = runtime.State;
        BlueprintResearchTask activeTask = null;
        bool hasActive = state.TryGetActiveTask(out activeTask);
        string activeText = hasActive
            ? $"{GetBlueprintName(activeTask.Blueprint)} {activeTask.ProgressRatio:P0} ({activeTask.Progress:0.#}/{activeTask.RequiredWork:0.#})"
            : "진행 중 연구 없음";

        AddSection(
            "연구 진행",
            $"대기/완료 포함 작업 {state.Tasks.Count}개 / 완료 {state.CompletedBlueprintIds.Count}개");
        AddLabel(activeText, 20f, 38f);

        bool hasResearchFacility = TryFindResearchFacility(out BuildableObject researchFacility);
        List<AbilityWork> researchWorkers = sceneQuery.All<AbilityWork>()
            .Where((work) => work != null
                && work.WorkerActor != null
                && !work.WorkerActor.IsDead
                && work.WorkPriorities.IsEnabled(FacilityWorkType.Research))
            .ToList();
        int assignedResearchers = researchWorkers.Count((work) =>
            work.AssignedWorkType == FacilityWorkType.Research && work.assignedShop == researchFacility);
        string researchBlocker = !hasActive
            ? "활성 연구 없음"
            : !hasResearchFacility
                ? "연구 시설 필요"
                : researchWorkers.Count == 0
                    ? "연구 허용 직원 필요"
                    : assignedResearchers > 0
                        ? "연구 진행 중"
                        : researchWorkers.Any((work) =>
                            work.WorkPriorities.GetPriority(FacilityWorkType.Research) == WorkPriorityLevel.Priority1)
                            ? "연구 최우선 대기"
                            : "다른 우선 업무 대기";
        CreateStatusCard(
            "P0State_ResearchWorkSource",
            "연구 작업 상태",
            $"{researchBlocker} / 시설 {(hasResearchFacility ? GetBuildingName(researchFacility) : "없음")} / 연구 허용 {researchWorkers.Count}명 / 현재 배정 {assignedResearchers}명",
            CompactCardHeight);

        if (hasActive)
        {
            FacilityBlueprintSO activeBlueprint = activeTask.Blueprint;
            string cancelActionName = $"P0Action_ResearchCancel_{GetBlueprintId(activeBlueprint)}";
            CreateButtonRow(
                cancelActionName,
                "활성 연구 취소",
                $"{GetBlueprintName(activeBlueprint)} 연구를 큐에서 제거합니다.",
                () =>
                {
                    bool cancelled = runtime.TryCancelBlueprint(activeBlueprint, out string message);
                    SetFeedback($"{(cancelled ? "연구 취소" : "취소 실패")}: {message}");
                });
        }

        AddSection("설계도 목록", "상점에서 구매한 설계도는 여기서 연구 큐에 올릴 수 있습니다.");

        FacilityShopUnlockState unlockState = runtime.ShopUnlockState;
        IReadOnlyCollection<FacilityBlueprintSO> blueprints = facilityShopCatalog.Blueprints
            .Where((blueprint) => blueprint != null)
            .OrderBy((blueprint) => blueprint.id)
            .Take(MaxVisibleCardsPerSection)
            .ToArray();

        foreach (FacilityBlueprintSO blueprint in blueprints)
        {
            bool acquired = unlockState.IsBlueprintAcquired(blueprint);
            bool queued = IsQueued(state, blueprint);
            bool completed = state.IsCompleted(blueprint);
            string status = completed
                ? "연구 완료"
                : queued
                    ? "연구 큐 등록"
                    : acquired
                        ? "연구 가능"
                        : "상점 구매 필요";
            string actionName = $"P0Action_ResearchStart_{blueprint.id}";
            string rewardPreview = FormatBlueprintRewardPreview(blueprint);
            CreateDataCard(
                actionName,
                GetBlueprintName(blueprint),
                $"{status} / 구매 {blueprint.defaultCost}G / 연구 {blueprint.researchWorkRequired:0.#}\n{rewardPreview}",
                acquired && !queued && !completed ? "연구 시작" : "상태 확인",
                () =>
                {
                    if (!acquired)
                    {
                        SetFeedback($"{GetBlueprintName(blueprint)} 연구 잠김: 상점에서 설계도를 먼저 구매해야 합니다.");
                        return;
                    }

                    if (queued)
                    {
                        SetFeedback($"{GetBlueprintName(blueprint)}은 이미 연구 큐에 있습니다.");
                        return;
                    }

                    if (completed)
                    {
                        SetFeedback($"{GetBlueprintName(blueprint)}은 이미 연구 완료되었습니다.");
                        return;
                    }

                    bool started = runtime.EnqueueBlueprint(blueprint);
                    SetFeedback($"{(started ? "연구 시작" : "연구 시작 실패")}: {GetBlueprintName(blueprint)}");
                },
                108f);
        }
    }

    private string FormatBlueprintRewardPreview(FacilityBlueprintSO blueprint)
    {
        if (blueprint == null)
        {
            return "해금 보상 없음";
        }

        Dictionary<int, BuildingSO> buildings = facilityShopCatalog.Buildings
            .Where(building => building != null)
            .GroupBy(building => building.id)
            .ToDictionary(group => group.Key, group => group.First());
        List<string> rewards = new List<string>();
        foreach (BlueprintUnlock unlock in blueprint.Unlocks)
        {
            switch (unlock)
            {
                case BlueprintBuildingUnlock buildingUnlock
                    when buildings.TryGetValue(buildingUnlock.buildingId, out BuildingSO directBuilding):
                    rewards.Add(FacilityShopService.GetBuildingName(directBuilding));
                    break;
                case BlueprintBasicPurchaseUnlock purchaseUnlock
                    when buildings.TryGetValue(purchaseUnlock.buildingId, out BuildingSO purchaseBuilding):
                    rewards.Add($"{FacilityShopService.GetBuildingName(purchaseBuilding)} 구매");
                    break;
                case BlueprintRecipeUnlock recipeUnlock
                    when !string.IsNullOrWhiteSpace(recipeUnlock.recipeId):
                    rewards.Add($"조합식 {recipeUnlock.recipeId}");
                    break;
            }
        }

        string[] distinct = rewards.Distinct(StringComparer.Ordinal).ToArray();
        if (distinct.Length == 0)
        {
            return string.IsNullOrWhiteSpace(blueprint.description)
                ? "해금 보상 없음"
                : blueprint.description;
        }

        const int visibleRewardCount = 4;
        string summary = string.Join(", ", distinct.Take(visibleRewardCount));
        if (distinct.Length > visibleRewardCount)
        {
            summary += $" 외 {distinct.Length - visibleRewardCount}개";
        }

        return $"해금: {summary}";
    }

    internal void BuildWarehouse()
    {
        WarehouseManagementSummary summary = buildingSummaryService.CaptureWarehouses();
        List<IWarehouseFacility> warehouses = FindWarehouses();
        List<BuildableObject> restockableFacilities = FindRestockableFacilities();

        AddSection(
            "창고 재고",
            summary.HasCapacityLimit
                ? $"창고 {summary.WarehouseCount}개 / 총 재고 {summary.TotalStock}/{summary.TotalCapacity}"
                : $"창고 {summary.WarehouseCount}개 / 총 재고 {summary.TotalStock}");
        AddLabel(
            FormatStockAmounts(summary.GetStock, useShortNames: false),
            20f,
            36f);
        AddLabel(
            BuildPhysicalStockStateText(summary.TotalStock),
            18f,
            32f);

        foreach (IWarehouseFacility warehouse in warehouses.Take(MaxVisibleCardsPerSection))
        {
            WarehouseInventory inventory = warehouse.Inventory;
            CreateDataCard(
                "P0State_Warehouse_" + GetUnityObjectId(warehouse),
                GetWarehouseName(warehouse),
                $"총 {inventory.TotalStock}/{(inventory.HasCapacityLimit ? inventory.MaxCapacity.ToString() : "무제한")} / {FormatStockAmounts(inventory.GetStock, useShortNames: true)}",
                "상태",
                () => SetFeedback($"{GetWarehouseName(warehouse)} 재고를 확인했습니다."),
                CompactCardHeight);
        }

        AddSection(
            "상점 수동 보충",
            $"상점 {restockableFacilities.Count}개 / 보충 필요 {restockableFacilities.Count((building) => ((IRestockableFacility)building).NeedsRestock)}개");
        foreach (BuildableObject building in restockableFacilities
                     .Where((candidate) => ((IRestockableFacility)candidate).NeedsRestock)
                     .Take(MaxVisibleCardsPerSection))
        {
            BuildableObject capturedBuilding = building;
            IRestockableFacility capturedFacility = (IRestockableFacility)capturedBuilding;
            string actionName = "P0Action_WarehouseRestock_" + capturedBuilding.GetInstanceID();
            CreateDataCard(
                actionName,
                GetBuildingName(capturedBuilding),
                $"재고 {capturedFacility.CurrentStock}/{capturedFacility.MaxStock} / 부족 {capturedFacility.MissingStock}",
                "보충",
                () =>
                {
                    int beforeShop = capturedFacility.CurrentStock;
                    int beforeWarehouse = warehouses.Sum((warehouse) => warehouse.Inventory.TotalStock);
                    int moved = capturedFacility.RestockFrom(
                        warehouses,
                        Mathf.Min(5, capturedFacility.MissingStock),
                        out string message);
                    int afterShop = capturedFacility.CurrentStock;
                    int afterWarehouse = warehouses.Sum((warehouse) => warehouse.Inventory.TotalStock);
                    SetFeedback(
                        $"보충 {(moved > 0 ? "성공" : "실패")}: {GetBuildingName(capturedBuilding)} {message} / 상점 {beforeShop}->{afterShop}, 창고 {beforeWarehouse}->{afterWarehouse}");
                },
                CompactCardHeight);
        }

        AddSection("일일 납품", "자금을 지불하고 창고에 재고를 입고합니다.");
        int day = GetCurrentDay();
        IReadOnlyList<StockDeliveryOffer> offers = StockSupplyService.CreateDailyDeliveryOffers(day, runVariableReader);
        for (int i = 0; i < Mathf.Min(offers.Count, MaxVisibleCardsPerSection); i++)
        {
            int index = i;
            StockDeliveryOffer offer = offers[i];
            string actionName = $"P0Action_WarehouseDelivery_{index}";
            CreateDataCard(
                actionName,
                $"{GetStockCategoryName(offer.category)} {offer.amount}개",
                $"{offer.sourceLabel} / 비용 {offer.cost} / 현재 자금 {FormatMoney(ResolveGameData())}",
                "납품 구매",
                () =>
                {
                    GameData gameData = ResolveGameData();
                    int beforeMoney = GetHoldingMoney(gameData);
                    int beforeStock = warehouses.Sum((warehouse) => warehouse.Inventory.TotalStock);
                    bool success = StockSupplyService.TryPurchaseDelivery(gameData, warehouses, offer, out StockSupplyResult result);
                    int afterMoney = GetHoldingMoney(gameData);
                    int afterStock = warehouses.Sum((warehouse) => warehouse.Inventory.TotalStock);
                    SetFeedback(
                        $"납품 {(success ? "성공" : "실패")}: {result.ToSummaryText()} / 자금 {beforeMoney}->{afterMoney}, 창고 {beforeStock}->{afterStock}");
                },
                CompactCardHeight);
        }
    }

    internal void BuildOperationHub()
    {
        GameData gameData = ResolveGameData();
        OperatingDaySettlementRuntime settlement = sceneQuery.First<OperatingDaySettlementRuntime>(includeInactive: true);
        OperatingDayReport report = settlement != null ? settlement.LatestReport : null;

        BuildRecruitmentSection();
        BuildEquipmentMaintenanceSection();

        AddSection(
            "운영 정산",
            $"Day {GetCurrentDay()} / {GetCurrentHour()}:00 / 자금 {FormatMoney(gameData)}");
        if (settlement == null)
        {
            AddLabel("정산 정보를 불러오지 못했습니다.", 19f, 44f);
        }
        else
        {
            OperatingCostForecast forecast = settlement.CurrentOperatingCostForecast;
            string paymentState = forecast.CanPayInFull
                ? $"납부 후 {forecast.AvailableMoney - forecast.TotalDue}"
                : $"부족 {forecast.ExpectedShortfall}";
            AddLabel(
                $"오늘 예상: 유지비 {forecast.MaintenanceCost} + 급여 {forecast.PayrollCost} + 미납 {forecast.OutstandingDebt} = {forecast.TotalDue} / {paymentState}",
                19f,
                52f);
            CreateButtonRow(
                "P0Action_OperationEmergencyFunding",
                settlement.CanTakeEmergencyFunding ? "긴급 융자" : "융자 사용됨",
                settlement.CanTakeEmergencyFunding
                    ? "이번 런에서 한 번만 자금을 확보할 수 있습니다. 받은 금액보다 큰 미납금이 다음 정산에 추가됩니다."
                    : $"이번 런의 긴급 융자를 이미 사용했습니다. 현재 미납금 {settlement.OutstandingDebt}.",
                () =>
                {
                    bool success = settlement.TryTakeEmergencyFunding(out string message);
                    SetFeedback($"{(success ? "융자 실행" : "융자 불가")}: {message}");
                });
        }

        if (report != null)
        {
            int shortageCount = report.stockShortageFacilities?.Count ?? 0;
            int incidentCount = report.incidents?.Count ?? 0;
            AddLabel(
                $"최근 정산: Day {report.day} / 매출 {report.totalRevenue} / 운영비 {report.paidOperatingCost}/{report.totalOperatingCost} / 미납 {report.unpaidOperatingCost} / 재고 부족 {shortageCount} / 사건 {incidentCount}",
                19f,
                52f);
        }

        BuildSurvivalEcosystemSection();
        BuildExteriorActivitySection();
        BuildMetaUpgradeSection();
        BuildRunVariableSection();
        BuildEconomyDetailSection();
    }

    private void BuildSurvivalSection()
    {
        SurvivalFoodOverview overview = survivalFoodRuntime != null
            ? survivalFoodRuntime.GetOverview()
            : new SurvivalFoodOverview(0, 0, 0, 0, 0, 0);
        int wildlifeCount = WildlifeRuntime.Active != null
            ? WildlifeRuntime.Active.Wildlife.Count(actor => actor != null && actor.IsAlive)
            : 0;
        int designatedCount = WildlifeRuntime.Active != null
            ? WildlifeRuntime.Active.Wildlife.Count(actor => actor != null && actor.IsAlive && actor.HuntDesignated)
            : 0;

        AddSection(
            "생존",
            $"오늘 필요 식량 {overview.TodayRequired} / 창고 {overview.StoredFood} / 바닥 {overview.LooseFood}");
        CreateStatusCard(
            "P0Survival_Food",
            overview.ShortageDays < 2 ? "식량 부족 위험" : "식량 상태",
            $"사체 {overview.CarcassCount} / 도축 예상 {overview.ButcherPendingFood} / 예상 버팀 {FormatShortageDays(overview.ShortageDays)}",
            CompactCardHeight);
        CreateStatusCard(
            "P0Survival_Wildlife",
            "야생",
            $"외부 동물 {wildlifeCount} / 사냥 지정 {designatedCount} / 동물 클릭으로 사냥 지정",
            CompactCardHeight);
        CreateStatusCard(
            "P0Survival_WaterFuel",
            overview.WaterShortageDays < 2 ? "물 부족 위험" : "물·연료",
            $"물 창고/바닥 {overview.StoredWater}/{overview.LooseWater} · 연료 {overview.StoredFuel} · 약품 {overview.StoredMedicine} · 물 버팀 {FormatShortageDays(overview.WaterShortageDays)}",
            CompactCardHeight);
        CreateStatusCard(
            "P0Survival_Environment",
            "날씨·위생",
            $"{FormatSurvivalWeather(overview.Weather)} {overview.OutdoorTemperature:0.#}도 · 위생 위험 {overview.SanitationRisk:0} · 질병 위험 {overview.DiseaseRisk:0} · 야간 외부 위험 {overview.ExteriorNightDanger:0}",
            CompactCardHeight);
        BuildWildlifeListRows();
    }

    private void BuildWildlifeListRows()
    {
        List<WildlifeActor> animals = WildlifeRuntime.Active != null
            ? WildlifeRuntime.Active.Wildlife
                .Where(actor => actor != null && actor.IsAlive)
                .OrderByDescending(actor => actor.HuntDesignated)
                .ThenByDescending(actor => actor.IsDangerous)
                .ThenBy(actor => actor.DisplayName, StringComparer.Ordinal)
                .Take(MaxVisibleCardsPerSection)
                .ToList()
            : new List<WildlifeActor>();
        List<WorldItemStackSnapshot> carcasses = WorldItemStackRuntime.Active != null
            ? WorldItemStackRuntime.Active.GetAllStacks()
                .Where(stack => stack != null
                    && stack.Quantity > 0
                    && (WildlifeItemDefinitions.TryGetSpeciesIdFromCarcass(stack.ItemId, out _)
                        || string.Equals(stack.ItemId, WildlifeItemDefinitions.RotItemId, StringComparison.Ordinal)))
                .OrderBy(stack => string.Equals(stack.ItemId, WildlifeItemDefinitions.RotItemId, StringComparison.Ordinal) ? 0 : 1)
                .ThenBy(stack => stack.DisplayName, StringComparer.Ordinal)
                .Take(MaxVisibleCardsPerSection)
                .ToList()
            : new List<WorldItemStackSnapshot>();

        AddSection(
            "야생 목록",
            $"동물 {animals.Count} / 사체·부패물 {carcasses.Count} / 행을 열면 위치와 정보가 함께 잡힙니다.");
        if (animals.Count == 0 && carcasses.Count == 0)
        {
            CreateStatusCard(
                "P0Wildlife_Empty",
                "외부가 조용함",
                "현재 보이는 야생동물이나 처리 대기 중인 사체가 없습니다.",
                CompactCardHeight);
            return;
        }

        foreach (WildlifeActor animal in animals)
        {
            WildlifeActor captured = animal;
            string status = captured.HuntDesignated
                ? captured.PriorityHunt ? "우선 사냥" : "사냥 지정"
                : FormatWildlifeState(captured.State);
            CreateDataCard(
                "P0Wildlife_Animal_" + SanitizeActionName(captured.WildlifeId),
                $"{captured.DisplayName} · {status}",
                $"체력 {captured.CurrentHealth}/{captured.MaxHealth} / 위험 {(captured.IsDangerous ? "높음" : "낮음")} / 위치 ({captured.GridPosition.x},{captured.GridPosition.y})",
                "보기",
                () =>
                {
                    FocusCameraAtGrid(captured.GridPosition);
                    InfoFeedEvent.Trigger(captured);
                    SetFeedback($"{captured.DisplayName} 위치를 확인했습니다.");
                },
                CompactCardHeight);
        }

        foreach (WorldItemStackSnapshot stack in carcasses)
        {
            WorldItemStackSnapshot captured = stack;
            bool rot = string.Equals(captured.ItemId, WildlifeItemDefinitions.RotItemId, StringComparison.Ordinal);
            CreateDataCard(
                "P0Wildlife_Carcass_" + SanitizeActionName(captured.StackId),
                rot ? "부패물" : captured.DisplayName,
                $"{captured.Quantity}개 / {captured.TotalWeight:0.#}kg / {FormatItemState(captured.State)} / 위치 ({captured.Position.x},{captured.Position.y})",
                "더미",
                () =>
                {
                    FocusCameraAtGrid(captured.Position);
                    InfoFeedEvent.Trigger(new ItemPileInfoTarget(captured.Position));
                    SetFeedback($"{captured.DisplayName} 더미를 열었습니다.");
                },
                CompactCardHeight);
        }
    }

    private void BuildSurvivalEcosystemSection()
    {
        SurvivalFoodOverview overview = survivalFoodRuntime != null
            ? survivalFoodRuntime.GetOverview()
            : new SurvivalFoodOverview(0, 0, 0, 0, 0, 0);
        IReadOnlyList<WildlifeActor> wildlife = WildlifeRuntime.Active != null
            ? WildlifeRuntime.Active.Wildlife
            : Array.Empty<WildlifeActor>();
        int wildlifeCount = wildlife.Count(actor => actor != null && actor.IsAlive);
        int designatedCount = wildlife.Count(actor => actor != null && actor.IsAlive && actor.HuntDesignated);
        WildlifeEcosystemOverview ecosystem = wildlifeEcosystemRuntime != null
            ? wildlifeEcosystemRuntime.GetOverview(wildlife)
            : new WildlifeEcosystemOverview(0, 0, 0, 0f, 0f, 0f, 0f, 0, wildlifeCount, 0f);

        AddSection(
            "생존",
            $"오늘 필요 식량 {overview.TodayRequired} / 창고 {overview.StoredFood} / 바닥 {overview.LooseFood}");
        CreateStatusCard(
            "P0Survival_Food_V10",
            overview.ShortageDays < 2 ? "식량 부족 위험" : "식량 상태",
            $"사체 {overview.CarcassCount} / 도축 예상 {overview.ButcherPendingFood} / 예상 버팀 {FormatShortageDaysReadable(overview.ShortageDays)}",
            CompactCardHeight);
        CreateStatusCard(
            "P0Survival_Wildlife_V10",
            "야생 생태",
            $"동물 {wildlifeCount}/{ecosystem.DesiredWildlifeCount} / 사냥 지정 {designatedCount} / 먹이 {FormatUnitPercent(ecosystem.FoodAbundance01)} / 물 {FormatUnitPercent(ecosystem.WaterAbundance01)} / 포식자 위험 {FormatUnitPercent(ecosystem.PredatorDanger01)}",
            CompactCardHeight);
        CreateStatusCard(
            "P0Survival_Habitat_V10",
            "서식지",
            $"패치 {ecosystem.PatchCount} / 풀·덤불 {ecosystem.GrassPatchCount} / 물 {ecosystem.WaterPatchCount} / 보충 대기 {FormatSecondsReadable(ecosystem.RespawnRemainingSeconds)}",
            CompactCardHeight);
        CreateStatusCard(
            "P0Survival_WaterFuel_V10",
            overview.WaterShortageDays < 2 ? "물 부족 위험" : "물·연료",
            $"물 창고/바닥 {overview.StoredWater}/{overview.LooseWater} · 연료 {overview.StoredFuel} · 약품 {overview.StoredMedicine} · 물 버팀 {FormatShortageDaysReadable(overview.WaterShortageDays)}",
            CompactCardHeight);
        CreateStatusCard(
            "P0Survival_Environment_V10",
            "날씨·위생",
            $"{FormatSurvivalWeatherReadable(overview.Weather)} {overview.OutdoorTemperature:0.#}도 · 위생 위험 {overview.SanitationRisk:0} · 질병 위험 {overview.DiseaseRisk:0} · 야간 외부 위험 {overview.ExteriorNightDanger:0}",
            CompactCardHeight);

        BuildWildlifeEcosystemListRows(wildlife);
    }

    private void BuildWildlifeEcosystemListRows(IReadOnlyList<WildlifeActor> wildlife)
    {
        List<WildlifeActor> animals = (wildlife ?? Array.Empty<WildlifeActor>())
            .Where(actor => actor != null && actor.IsAlive)
            .OrderByDescending(actor => actor.HuntDesignated)
            .ThenByDescending(actor => actor.IsDangerous)
            .ThenByDescending(actor => Mathf.Max(actor.Hunger, actor.Thirst))
            .ThenBy(actor => actor.DisplayName, StringComparer.Ordinal)
            .Take(MaxVisibleCardsPerSection)
            .ToList();
        List<WorldItemStackSnapshot> carcasses = WorldItemStackRuntime.Active != null
            ? WorldItemStackRuntime.Active.GetAllStacks()
                .Where(stack => stack != null
                    && stack.Quantity > 0
                    && (WildlifeItemDefinitions.TryGetSpeciesIdFromCarcass(stack.ItemId, out _)
                        || string.Equals(stack.ItemId, WildlifeItemDefinitions.RotItemId, StringComparison.Ordinal)))
                .OrderBy(stack => string.Equals(stack.ItemId, WildlifeItemDefinitions.RotItemId, StringComparison.Ordinal) ? 0 : 1)
                .ThenBy(stack => stack.DisplayName, StringComparer.Ordinal)
                .Take(MaxVisibleCardsPerSection)
                .ToList()
            : new List<WorldItemStackSnapshot>();

        AddSection(
            "야생 목록",
            $"동물 {animals.Count} / 사체·부패물 {carcasses.Count} / 행을 누르면 위치와 정보를 엽니다.");
        if (animals.Count == 0 && carcasses.Count == 0)
        {
            CreateStatusCard(
                "P0Wildlife_Empty_V10",
                "외부가 조용함",
                "현재 보이는 야생동물이나 처리 대기 중인 사체가 없습니다.",
                CompactCardHeight);
            return;
        }

        foreach (WildlifeActor animal in animals)
        {
            WildlifeActor captured = animal;
            string status = captured.HuntDesignated
                ? captured.PriorityHunt ? "우선 사냥" : "사냥 지정"
                : FormatWildlifeStateReadable(captured.State);
            string intent = FormatWildlifeIntentReadable(captured);
            CreateDataCard(
                "P0Wildlife_Animal_V10_" + SanitizeActionName(captured.WildlifeId),
                $"{captured.DisplayName} · {status}",
                $"체력 {captured.CurrentHealth}/{captured.MaxHealth} / 위험 {(captured.IsDangerous ? "높음" : "낮음")} / 허기 {FormatUnitPercent(captured.Hunger)} / 갈증 {FormatUnitPercent(captured.Thirst)} / 위치 ({captured.GridPosition.x},{captured.GridPosition.y})\n{intent}",
                "보기",
                () =>
                {
                    FocusCameraAtGrid(captured.GridPosition);
                    InfoFeedEvent.Trigger(captured);
                    SetFeedback($"{captured.DisplayName} 위치를 확인했습니다.");
                },
                CompactCardHeight);
        }

        foreach (WorldItemStackSnapshot stack in carcasses)
        {
            WorldItemStackSnapshot captured = stack;
            bool rot = string.Equals(captured.ItemId, WildlifeItemDefinitions.RotItemId, StringComparison.Ordinal);
            CreateDataCard(
                "P0Wildlife_Carcass_V10_" + SanitizeActionName(captured.StackId),
                rot ? "부패물" : captured.DisplayName,
                $"{captured.Quantity}개 / {captured.TotalWeight:0.#}kg / {FormatItemStateReadable(captured.State)} / 위치 ({captured.Position.x},{captured.Position.y})",
                "더미",
                () =>
                {
                    FocusCameraAtGrid(captured.Position);
                    InfoFeedEvent.Trigger(new ItemPileInfoTarget(captured.Position));
                    SetFeedback($"{captured.DisplayName} 더미를 열었습니다.");
                },
                CompactCardHeight);
        }
    }

    private void BuildExteriorActivitySection()
    {
        if (exteriorZoneQuery == null)
        {
            AddSection("입구/외부", "외부 활동 상태를 불러오지 못했습니다.");
            return;
        }

        ExteriorActivityOverviewSnapshot overview = exteriorZoneQuery.GetOverview();
        IReadOnlyList<ExteriorZoneMarker> zones = exteriorZoneQuery.Zones ?? Array.Empty<ExteriorZoneMarker>();
        int waitingVisitors = zones.Sum((zone) => zone != null ? zone.WaitingVisitorCount : 0);
        int activeIncidents = zones.Count((zone) => zone != null && zone.HasActiveIncident);
        int dropLooseStacks = CountWorldStacksAtZones(zones, ExteriorZoneType.DropZone, WorldItemStackState.Loose);
        int packedStacks = CountWorldStacksAtZones(zones, ExteriorZoneType.DropZone, WorldItemStackState.ExpeditionPacked);

        AddSection(
            "입구/외부",
            $"구역 {overview.ZoneCount} / 하차장 {overview.DropZoneCount} / 사건 {activeIncidents} / 대기 {waitingVisitors}");
        AddLabel(
            $"청결 {overview.AverageCleanliness:0} / 손상 {overview.AverageDamage:0} / 응대 {overview.AverageReceptionReadiness:0} / 순찰 {overview.AveragePatrolReadiness:0}",
            18f,
            40f);

        CreateStatusCard(
            "P0Exterior_Logistics",
            "하차장 물류",
            $"바닥 스택 {dropLooseStacks} / 원정 포장 {packedStacks} / {DescribeExteriorZone(ExteriorZoneType.DropZone)}",
            CompactCardHeight);
        CreateStatusCard(
            "P0Exterior_Reception",
            "입구 응대",
            $"{DescribeExteriorZone(ExteriorZoneType.ReceptionPoint)} / 대기 방문객 {waitingVisitors}",
            CompactCardHeight);
        CreateStatusCard(
            "P0Exterior_Patrol",
            "외부 순찰",
            $"{DescribeExteriorZone(ExteriorZoneType.GuardPost)} / {DescribeExteriorZone(ExteriorZoneType.PatrolPoint)}",
            CompactCardHeight);
        CreateStatusCard(
            "P0Exterior_Incident",
            activeIncidents > 0 ? "진행 중 사건" : "외부 사건",
            activeIncidents > 0 ? DescribeActiveExteriorIncidents(zones) : "진행 중인 외부 사건 없음",
            CompactCardHeight);
    }

    private int CountWorldStacksAtZones(
        IReadOnlyList<ExteriorZoneMarker> zones,
        ExteriorZoneType zoneType,
        WorldItemStackState state)
    {
        if (WorldItemStackRuntime.Active == null || zones == null)
        {
            return 0;
        }

        HashSet<Vector2Int> positions = zones
            .Where((zone) => zone != null && zone.ZoneType == zoneType)
            .Select((zone) => zone.GridPosition)
            .ToHashSet();
        if (positions.Count == 0)
        {
            return 0;
        }

        return WorldItemStackRuntime.Active.GetAllStacks()
            .Count((stack) => stack != null
                && stack.State == state
                && positions.Contains(stack.Position));
    }

    private static string FormatShortageDaysReadable(int shortageDays)
    {
        if (shortageDays == int.MaxValue)
        {
            return "-";
        }

        return shortageDays <= 0 ? "오늘 부족" : $"{shortageDays}일";
    }

    private static string FormatSecondsReadable(float seconds)
    {
        if (seconds <= 0.1f)
        {
            return "가능";
        }

        if (seconds < 60f)
        {
            return $"{seconds:0}초";
        }

        return $"{seconds / 60f:0.#}분";
    }

    private static string FormatUnitPercent(float value01)
    {
        return $"{Mathf.Clamp01(value01) * 100f:0}%";
    }

    private static string FormatSurvivalWeatherReadable(SurvivalWeatherType weather)
    {
        return weather switch
        {
            SurvivalWeatherType.Rain => "비",
            SurvivalWeatherType.Fog => "안개",
            SurvivalWeatherType.HeatWave => "폭염",
            SurvivalWeatherType.ColdSnap => "한파",
            SurvivalWeatherType.Storm => "폭우",
            _ => "맑음"
        };
    }

    private static string FormatWildlifeStateReadable(WildlifeState state)
    {
        return state switch
        {
            WildlifeState.Grazing => "먹이 활동",
            WildlifeState.Fleeing => "도망 중",
            WildlifeState.Hunted => "쫓기는 중",
            WildlifeState.Retaliating => "반격 중",
            WildlifeState.PredatorStalking => "추적 중",
            WildlifeState.Dead => "죽음",
            WildlifeState.Leaving => "떠나는 중",
            _ => "배회 중"
        };
    }

    private static string FormatWildlifeIntentReadable(WildlifeActor actor)
    {
        if (actor == null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(actor.IntentReason))
        {
            return actor.IntentReason;
        }

        return actor.Intent switch
        {
            WildlifeIntent.Forage => "먹이를 찾는 중",
            WildlifeIntent.Drink => "물가로 이동",
            WildlifeIntent.Rest => "은신처에서 쉬는 중",
            WildlifeIntent.ReturnToTerritory => "영역으로 돌아가는 중",
            WildlifeIntent.HuntPrey => "먹잇감을 추적",
            WildlifeIntent.Flee => "위협을 피해 도망",
            WildlifeIntent.LeaveMap => "지역을 떠나는 중",
            _ => "영역 안을 배회"
        };
    }

    private static string FormatItemStateReadable(WorldItemStackState state)
    {
        return state switch
        {
            WorldItemStackState.Stored => "창고",
            WorldItemStackState.FacilityBuffer => "시설 버퍼",
            WorldItemStackState.Carried => "운반 중",
            WorldItemStackState.ExpeditionPacked => "원정 포장",
            _ => "바닥"
        };
    }

    private static string FormatShortageDays(int shortageDays)
    {
        if (shortageDays == int.MaxValue)
        {
            return "-";
        }

        return shortageDays <= 0 ? "오늘 부족" : $"{shortageDays}일";
    }

    private static string FormatSurvivalWeather(SurvivalWeatherType weather)
    {
        return weather switch
        {
            SurvivalWeatherType.Rain => "비",
            SurvivalWeatherType.Fog => "안개",
            SurvivalWeatherType.HeatWave => "폭염",
            SurvivalWeatherType.ColdSnap => "한파",
            SurvivalWeatherType.Storm => "폭우",
            _ => "맑음"
        };
    }

    private static string FormatWildlifeState(WildlifeState state)
    {
        return state switch
        {
            WildlifeState.Grazing => "먹이 찾는 중",
            WildlifeState.Fleeing => "도망 중",
            WildlifeState.Hunted => "쫓기는 중",
            WildlifeState.Retaliating => "반격 중",
            WildlifeState.PredatorStalking => "기회를 보는 중",
            WildlifeState.Dead => "사망",
            WildlifeState.Leaving => "떠나는 중",
            _ => "배회 중"
        };
    }

    private static string FormatItemState(WorldItemStackState state)
    {
        return state switch
        {
            WorldItemStackState.Stored => "창고",
            WorldItemStackState.FacilityBuffer => "시설 버퍼",
            WorldItemStackState.Carried => "운반 중",
            WorldItemStackState.ExpeditionPacked => "원정 포장",
            _ => "바닥"
        };
    }

    private static string SanitizeActionName(string raw)
    {
        string value = raw ?? string.Empty;
        return new string(value.Select(character =>
                char.IsLetterOrDigit(character) ? character : '_')
            .ToArray());
    }

    private void FocusCameraAtGrid(Vector2Int position)
    {
        Camera camera = mainCameraProvider != null ? mainCameraProvider.Camera : null;
        GridSystemManager gridManager = sceneQuery?.First<GridSystemManager>(includeInactive: true);
        if (camera == null || gridManager == null || gridManager.grid == null)
        {
            return;
        }

        Vector3 world = gridManager.grid.GetWorldPos(position);
        camera.transform.position = new Vector3(world.x, world.y, -10f);
        camera.GetComponent<CameraManager>()?.ClampToCurrentBounds();
    }

    private string DescribeExteriorZone(ExteriorZoneType zoneType)
    {
        ExteriorZoneMarker zone = exteriorZoneQuery != null
            ? exteriorZoneQuery.GetZones(zoneType).FirstOrDefault()
            : null;
        if (zone == null)
        {
            return $"{FormatExteriorZoneType(zoneType)} 없음";
        }

        return $"{FormatExteriorZoneType(zoneType)} ({zone.GridPosition.x},{zone.GridPosition.y}) 청결 {zone.Cleanliness:0} 손상 {zone.Damage:0}";
    }

    private static string DescribeActiveExteriorIncidents(IReadOnlyList<ExteriorZoneMarker> zones)
    {
        string[] incidents = zones
            .Where((zone) => zone != null && zone.HasActiveIncident)
            .Select((zone) =>
            {
                string text = !string.IsNullOrWhiteSpace(zone.ActiveIncidentText)
                    ? zone.ActiveIncidentText
                    : FormatExteriorIncidentKind(zone.ActiveIncidentKind);
                return $"{FormatExteriorZoneType(zone.ZoneType)}: {text}";
            })
            .Take(3)
            .ToArray();
        return incidents.Length > 0 ? string.Join(" / ", incidents) : "진행 중인 외부 사건 없음";
    }

    private static string FormatExteriorZoneType(ExteriorZoneType zoneType)
    {
        return zoneType switch
        {
            ExteriorZoneType.Entrance => "입구",
            ExteriorZoneType.DropZone => "하차장",
            ExteriorZoneType.ReceptionPoint => "응대 지점",
            ExteriorZoneType.GuardPost => "경비 초소",
            ExteriorZoneType.PatrolPoint => "순찰 지점",
            ExteriorZoneType.OutdoorRestSpot => "외부 휴식",
            ExteriorZoneType.ExpeditionStaging => "출정 집결",
            ExteriorZoneType.IncidentPoint => "사건 지점",
            _ => zoneType.ToString()
        };
    }

    private static string FormatExteriorIncidentKind(ExteriorIncidentKind kind)
    {
        return kind switch
        {
            ExteriorIncidentKind.MerchantCart => "상인 마차",
            ExteriorIncidentKind.Informant => "정보상",
            ExteriorIncidentKind.Thief => "도둑",
            ExteriorIncidentKind.InjuredReturnee => "부상 귀환자",
            _ => "사건"
        };
    }

    private void BuildRecruitmentSection()
    {
        if (!regularCustomerProvider.TryGetRuntime(out RegularCustomerRuntime runtime))
        {
            AddSection("단골/영입", "단골 런타임이 현재 씬에 없습니다.");
            return;
        }

        IReadOnlyList<RegularCustomerRecord> records = runtime.State.Records
            .OrderByDescending((record) => record.Status == RegularCustomerStatus.RecruitCandidate && !record.IsRecruited)
            .ThenBy((record) => record.IsRecruited)
            .ThenByDescending((record) => record.AverageSatisfaction)
            .ThenByDescending((record) => record.VisitCount)
            .ThenBy((record) => record.CustomerId, StringComparer.Ordinal)
            .ToArray();
        AddSection(
            "단골/영입",
            $"기록 {records.Count}명 / 후보 {records.Count((record) => record.Status == RegularCustomerStatus.RecruitCandidate)}명 / 영입 완료 {runtime.State.RecruitedCharacters.Count}명");

        if (records.Count == 0)
        {
            AddLabel("아직 단골 기록이 없습니다. 손님이 시설을 이용하면 방문 기록이 쌓입니다.", 19f, 44f);
            return;
        }

        foreach (RegularCustomerRecord record in records.Take(MaxVisibleCardsPerSection))
        {
            RegularCustomerRecord capturedRecord = record;
            string actionName = $"P0Action_Recruit_{capturedRecord.CustomerId}";
            bool canRecruit = capturedRecord.Status == RegularCustomerStatus.RecruitCandidate;
            string buttonText = capturedRecord.IsRecruited ? "영입됨" : canRecruit ? "영입" : "상태 확인";
            CreateDataCard(
                actionName,
                capturedRecord.DisplayName,
                $"{capturedRecord.SpeciesTag} / 방문 {capturedRecord.VisitCount}회 / 만족도 {capturedRecord.AverageSatisfaction:0.#} / {capturedRecord.Status} / 역할 {RegularCustomerService.FormatCapabilities(capturedRecord.RecruitCapabilities)}",
                buttonText,
                () =>
                {
                    bool success = runtime.TryRecruit(capturedRecord.CustomerId, out RegularCustomerRecruitResult result);
                    SetFeedback(success
                        ? $"영입 성공: {result.Record.DisplayName} / 역할 {RegularCustomerService.FormatCapabilities(result.Capabilities)}"
                        : $"영입 불가: {result.Message}");
                },
                CompactCardHeight);
        }
    }

    private void BuildMetaUpgradeSection()
    {
        if (!metaProvider.TryGetRuntime(out MetaProgressionRuntime runtime))
        {
            AddSection("계승 강화", "메타 진행 런타임이 현재 씬에 없습니다.");
            return;
        }

        MetaProgressionState state = runtime.State;
        AddSection(
            "계승 강화",
            $"보유 재화 {state.AvailableCurrency} / 누적 {state.LifetimeEarnedCurrency} / 사용 {state.SpentCurrency}");

        foreach (MetaUpgradeDefinition definition in MetaProgressionCatalog.All
            .OrderBy((item) => item.branch)
            .ThenBy((item) => item.id))
        {
            MetaUpgradeDefinition capturedDefinition = definition;
            int level = state.GetUpgradeLevel(capturedDefinition.id);
            string actionName = $"P0Action_MetaUpgrade_{capturedDefinition.id}";
            string detail = $"{FormatBranch(capturedDefinition.branch)} / Lv.{level}/{capturedDefinition.maxLevel} / 비용 {capturedDefinition.cost}\n{capturedDefinition.detail}";
            CreateDataCard(
                actionName,
                capturedDefinition.title,
                detail,
                level >= capturedDefinition.maxLevel ? "최대" : "구매",
                () =>
                {
                    int beforeCurrency = state.AvailableCurrency;
                    int beforeLevel = state.GetUpgradeLevel(capturedDefinition.id);
                    bool success = runtime.TryPurchaseUpgrade(capturedDefinition.id, out string message);
                    int afterCurrency = state.AvailableCurrency;
                    int afterLevel = state.GetUpgradeLevel(capturedDefinition.id);
                    SetFeedback(
                        $"강화 {(success ? "구매 성공" : "구매 실패")}: {message} / Lv.{beforeLevel}->{afterLevel}, 재화 {beforeCurrency}->{afterCurrency}");
                },
                92f);
        }
    }

    private void CreateOfferCard(string actionName, FacilityShopOffer offer, string buttonText, Action onClick)
    {
        string detail = $"{offer.TypeDisplayName} / {offer.Rarity} / 비용 {offer.Cost} / {offer.Star}성";
        if (offer.IsBasicPurchase)
        {
            detail += " / 기본 구매";
        }

        CreateDataCard(actionName, offer.DisplayName, detail, buttonText, onClick, CardHeight);
    }

    private void CreateDataCard(
        string actionName,
        string title,
        string detail,
        string buttonText,
        Action onClick,
        float height)
    {
        GameObject card = CreateUiObject(actionName + "_Card", contentRoot);
        spawnedObjects.Add(card);
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, height);

        Image image = card.AddComponent<Image>();
        image.color = DungeonUiTheme.Surface;

        HorizontalLayoutGroup horizontal = card.AddComponent<HorizontalLayoutGroup>();
        horizontal.spacing = 10f;
        horizontal.padding = new RectOffset(12, 12, 8, 8);
        horizontal.childControlWidth = true;
        horizontal.childControlHeight = true;
        horizontal.childForceExpandWidth = false;
        horizontal.childForceExpandHeight = true;

        LayoutElement cardLayout = card.AddComponent<LayoutElement>();
        cardLayout.preferredHeight = height;
        cardLayout.minHeight = height;

        GameObject textColumn = CreateUiObject("Text", card.transform);
        VerticalLayoutGroup textVertical = textColumn.AddComponent<VerticalLayoutGroup>();
        textVertical.spacing = 2f;
        textVertical.childControlWidth = true;
        textVertical.childControlHeight = false;
        textVertical.childForceExpandWidth = true;
        textVertical.childForceExpandHeight = false;
        LayoutElement textLayout = textColumn.AddComponent<LayoutElement>();
        textLayout.flexibleWidth = 1f;

        TMP_Text titleText = AddText(textColumn.transform, title, 20f, FontStyles.Bold);
        titleText.color = DungeonUiTheme.TextPrimary;
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 14f;
        titleText.fontSizeMax = 20f;
        titleText.overflowMode = TextOverflowModes.Truncate;

        TMP_Text detailText = AddText(textColumn.transform, detail, 16f, FontStyles.Normal);
        detailText.color = DungeonUiTheme.TextSecondary;
        detailText.enableAutoSizing = true;
        detailText.fontSizeMin = 11f;
        detailText.fontSizeMax = 16f;
        detailText.textWrappingMode = TextWrappingModes.Normal;

        CreateActionButton(card.transform, actionName, buttonText, onClick, 132f, height - 18f);
    }

    private void CreateStatusCard(string stateName, string title, string detail, float height)
    {
        GameObject card = CreateUiObject(stateName, contentRoot);
        spawnedObjects.Add(card);
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, height);

        Image image = card.AddComponent<Image>();
        image.color = DungeonUiTheme.Surface;
        image.raycastTarget = false;

        VerticalLayoutGroup vertical = card.AddComponent<VerticalLayoutGroup>();
        vertical.spacing = 2f;
        vertical.padding = new RectOffset(12, 12, 8, 8);
        vertical.childControlWidth = true;
        vertical.childControlHeight = true;
        vertical.childForceExpandWidth = true;
        vertical.childForceExpandHeight = false;

        LayoutElement cardLayout = card.AddComponent<LayoutElement>();
        cardLayout.preferredHeight = height;
        cardLayout.minHeight = height;

        TMP_Text titleText = AddText(card.transform, title, 20f, FontStyles.Bold);
        titleText.color = DungeonUiTheme.TextPrimary;
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 14f;
        titleText.fontSizeMax = 20f;
        titleText.overflowMode = TextOverflowModes.Truncate;
        titleText.raycastTarget = false;
        LayoutElement titleLayout = titleText.gameObject.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 22f;
        titleLayout.minHeight = 20f;

        TMP_Text detailText = AddText(card.transform, detail, 16f, FontStyles.Normal);
        detailText.color = DungeonUiTheme.TextSecondary;
        detailText.enableAutoSizing = true;
        detailText.fontSizeMin = 11f;
        detailText.fontSizeMax = 16f;
        detailText.textWrappingMode = TextWrappingModes.Normal;
        detailText.raycastTarget = false;
        LayoutElement detailLayout = detailText.gameObject.AddComponent<LayoutElement>();
        detailLayout.preferredHeight = Mathf.Max(20f, height - 40f);
        detailLayout.minHeight = 18f;
    }

    private void CreateButtonRow(string actionName, string buttonText, string detail, Action onClick)
    {
        CreateDataCard(actionName, buttonText, detail, buttonText, onClick, CompactCardHeight);
    }

    private void AddSection(string title, string summary)
    {
        GameObject section = CreateUiObject("Section_" + title, contentRoot);
        spawnedObjects.Add(section);
        RectTransform rect = section.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, 58f);

        Image image = section.AddComponent<Image>();
        image.color = DungeonUiTheme.SurfaceRaised;

        VerticalLayoutGroup vertical = section.AddComponent<VerticalLayoutGroup>();
        vertical.spacing = 1f;
        vertical.padding = new RectOffset(12, 12, 7, 5);
        vertical.childControlWidth = true;
        vertical.childControlHeight = false;

        LayoutElement layout = section.AddComponent<LayoutElement>();
        layout.preferredHeight = 58f;
        layout.minHeight = 58f;

        TMP_Text titleText = AddText(section.transform, title, 21f, FontStyles.Bold);
        titleText.color = DungeonUiTheme.Warning;
        TMP_Text summaryText = AddText(section.transform, summary, 15f, FontStyles.Normal);
        summaryText.color = DungeonUiTheme.TextSecondary;
        summaryText.overflowMode = TextOverflowModes.Truncate;
    }

    private void AddFeedback()
    {
        GameObject feedback = CreateUiObject("P0Feedback", contentRoot);
        spawnedObjects.Add(feedback);
        RectTransform rect = feedback.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, 46f);

        Image image = feedback.AddComponent<Image>();
        image.color = DungeonUiTheme.SurfaceRaised;

        LayoutElement layout = feedback.AddComponent<LayoutElement>();
        layout.preferredHeight = 46f;
        layout.minHeight = 46f;

        feedbackText = AddText(feedback.transform, feedbackMessage, 18f, FontStyles.Bold);
        feedbackText.color = DungeonUiTheme.Warning;
        RectTransform textRect = feedbackText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 3f);
        textRect.offsetMax = new Vector2(-12f, -3f);
        feedbackText.alignment = TextAlignmentOptions.MidlineLeft;
        feedbackText.enableAutoSizing = true;
        feedbackText.fontSizeMin = 11f;
        feedbackText.fontSizeMax = 18f;
        feedbackText.overflowMode = TextOverflowModes.Truncate;
    }

    private void AddLabel(string text, float fontSize, float height)
    {
        GameObject row = CreateUiObject("Label", contentRoot);
        spawnedObjects.Add(row);
        LayoutElement layout = row.AddComponent<LayoutElement>();
        layout.preferredHeight = height;
        layout.minHeight = height;

        TMP_Text label = AddText(row.transform, text, fontSize, FontStyles.Normal);
        label.color = DungeonUiTheme.TextPrimary;
        label.alignment = TextAlignmentOptions.Left;
        label.textWrappingMode = TextWrappingModes.Normal;
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8f, 0f);
        labelRect.offsetMax = new Vector2(-8f, 0f);
    }

    private void AddEmptyState(string message)
    {
        AddSection("연결 상태", "필요한 런타임이 없어서 조작 UI를 만들 수 없습니다.");
        AddLabel(message, 20f, 64f);
    }

    private Button CreateActionButton(
        Transform parent,
        string actionName,
        string label,
        Action onClick,
        float width,
        float height)
    {
        GameObject buttonObject = CreateUiObject(actionName, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);

        Image image = buttonObject.AddComponent<Image>();
        image.color = DungeonUiTheme.Accent;
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        DungeonUiTheme.StyleButton(button, selected: true);
        button.onClick.AddListener(() =>
        {
            onClick?.Invoke();
            Refresh();
        });

        LayoutElement layout = buttonObject.AddComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.minWidth = width;
        layout.preferredHeight = height;
        layout.minHeight = height;

        TMP_Text text = AddText(buttonObject.transform, label, 17f, FontStyles.Bold);
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = 10f;
        text.fontSizeMax = 17f;
        text.textWrappingMode = TextWrappingModes.Normal;
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(6f, 4f);
        textRect.offsetMax = new Vector2(-6f, -4f);
        text.raycastTarget = false;
        return button;
    }

    private TMP_Text AddText(Transform parent, string text, float fontSize, FontStyles style)
    {
        GameObject textObject = CreateUiObject("Text", parent);
        TMP_Text label = textObject.AddComponent<TextMeshProUGUI>();
        fontService?.Apply(label);
        label.text = text ?? string.Empty;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.alignment = TextAlignmentOptions.Left;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.raycastTarget = false;
        return label;
    }

    private GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private void SetFeedback(string message)
    {
        feedbackMessage = string.IsNullOrWhiteSpace(message) ? "결과 없음" : message;
        if (feedbackText != null)
        {
            feedbackText.text = feedbackMessage;
        }
    }

    private GameData ResolveGameData()
    {
        return gameDataProvider.TryGetGameData(out GameData gameData) ? gameData : null;
    }

    private int GetCurrentDay()
    {
        GameData gameData = ResolveGameData();
        return gameData != null && gameData.day != null
            ? Mathf.Max(1, gameData.day.Value)
            : 1;
    }

    private int GetCurrentHour()
    {
        GameData gameData = ResolveGameData();
        return gameData != null && gameData.hour != null ? gameData.hour.Value : 0;
    }

    private static int GetHoldingMoney(GameData gameData)
    {
        return gameData != null && gameData.holdingMoney != null ? gameData.holdingMoney.Value : -1;
    }

    private static string FormatMoney(GameData gameData)
    {
        int money = GetHoldingMoney(gameData);
        return money >= 0 ? money.ToString() : "없음";
    }

    private List<IWarehouseFacility> FindWarehouses()
    {
        return sceneQuery.All<MonoBehaviour>()
            .OfType<IWarehouseFacility>()
            .Where((warehouse) => warehouse != null && warehouse.HasWarehouseInventory && warehouse.Inventory != null)
            .ToList();
    }

    private string BuildPhysicalStockStateText(int availableStock)
    {
        int loose = 0;
        int reserved = 0;
        int facilityBuffer = 0;
        if (WorldItemStackRuntime.Active != null)
        {
            foreach (WorldItemStackSnapshot stack in WorldItemStackRuntime.Active.GetAllStacks())
            {
                if (stack == null || stack.Quantity <= 0)
                {
                    continue;
                }

                bool reservedLike = stack.IsReserved
                    || stack.HasDestinationPosition
                    || stack.State == WorldItemStackState.ExpeditionPacked;
                if (reservedLike)
                {
                    reserved += stack.Quantity;
                    continue;
                }

                if (stack.State == WorldItemStackState.Loose)
                {
                    loose += stack.Quantity;
                }
                else if (stack.State == WorldItemStackState.FacilityBuffer)
                {
                    facilityBuffer += stack.Quantity;
                }
            }
        }

        int carried = sceneQuery.All<CharacterCarryInventory>()
            .Where(inventory => inventory != null)
            .Sum(inventory => inventory.Items.Sum(item => item != null ? Mathf.Max(0, item.quantity) : 0));
        return $"재고 상태  사용 가능 {availableStock} / 예약됨 {reserved} / 바닥 {loose} / 시설 버퍼 {facilityBuffer} / 운반 중 {carried}";
    }

    private List<BuildableObject> FindRetailFacilities()
    {
        return sceneQuery.All<BuildableObject>()
            .Where((building) => building != null
                && !building.isDestroy
                && building is IRetailFacility)
            .OrderByDescending((building) => ((IStockedFacility)building).CurrentStock)
            .ToList();
    }

    private List<BuildableObject> FindRestockableFacilities()
    {
        return sceneQuery.All<BuildableObject>()
            .Where((building) => building != null
                && !building.isDestroy
                && building is IRestockableFacility)
            .OrderByDescending((building) => ((IRestockableFacility)building).MissingStock)
            .ToList();
    }

    private bool TryFindResearchFacility(out BuildableObject researchFacility)
    {
        researchFacility = sceneQuery.All<BuildableObject>()
            .FirstOrDefault((building) => building != null
                && !building.isDestroy
                && building.SupportsWork(FacilityWorkType.Research));
        return researchFacility != null;
    }

    private static bool IsQueued(BlueprintResearchState state, FacilityBlueprintSO blueprint)
    {
        return state != null
            && blueprint != null
            && state.Tasks.Any((task) => task != null
                && task.Blueprint != null
                && task.Blueprint.id == blueprint.id
                && !task.IsCompleted);
    }

    private static string GetBlueprintName(FacilityBlueprintSO blueprint)
    {
        return blueprint != null && !string.IsNullOrWhiteSpace(blueprint.DisplayName)
            ? blueprint.DisplayName
            : "설계도";
    }

    private static int GetBlueprintId(FacilityBlueprintSO blueprint)
    {
        return blueprint != null ? blueprint.id : -1;
    }

    private static string GetWarehouseName(IWarehouseFacility warehouse)
    {
        Component component = warehouse as Component;
        if (component == null)
        {
            return "창고";
        }

        BuildableObject building = component.GetComponent<BuildableObject>();
        return GetBuildingName(building);
    }

    private static string GetBuildingName(BuildableObject building)
    {
        if (building == null)
        {
            return "시설";
        }

        return building.BuildingData != null && !string.IsNullOrWhiteSpace(building.BuildingData.objectName)
            ? building.BuildingData.objectName
            : building.name;
    }

    private static string GetBuildingName(IRetailFacility facility)
    {
        return GetBuildingName(facility as BuildableObject);
    }

    private static int GetUnityObjectId(object value)
    {
        return value is Component component ? component.GetInstanceID() : value != null ? value.GetHashCode() : 0;
    }

    private static string GetStockCategoryName(StockCategory category)
    {
        return StockCategoryCatalog.GetDisplayName(category);
    }

    private static string FormatStockAmounts(
        Func<StockCategory, int> getStock,
        bool useShortNames)
    {
        if (getStock == null)
        {
            return string.Empty;
        }

        return string.Join(
            " / ",
            StockCategoryCatalog.All.Select((definition) =>
                $"{(useShortNames ? definition.ShortName : definition.DisplayName)} {getStock(definition.Category)}"));
    }

    private static string FormatBranch(MetaProgressionBranch branch)
    {
        return branch switch
        {
            MetaProgressionBranch.OperationKnowledge => "운영 지식",
            MetaProgressionBranch.DesignPreservation => "설계 보존",
            MetaProgressionBranch.OwnerSurvival => "사장 생존",
            MetaProgressionBranch.CommerceLogistics => "상업·물류",
            MetaProgressionBranch.FortressDefense => "요새·방어",
            MetaProgressionBranch.ArcaneResearch => "비전·연구",
            _ => branch.ToString()
        };
    }

    private void ClearSpawnedObjects()
    {
        foreach (GameObject spawned in spawnedObjects)
        {
            Release(spawned);
        }

        spawnedObjects.Clear();
    }

    private void ClearChildren(RectTransform host)
    {
        if (host == null)
        {
            return;
        }

        for (int i = host.childCount - 1; i >= 0; i--)
        {
            Release(host.GetChild(i).gameObject);
        }
    }

    private static void Release(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        target.SetActive(false);
        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }
}
