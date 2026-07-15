using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public interface IP0FeatureSurfacePanelFactory
{
    P0FeatureSurfacePanel Ensure(GameObject panelObject, int tabId);
}

public sealed class P0FeatureSurfacePanelFactory : IP0FeatureSurfacePanelFactory
{
    private readonly IObjectResolver objectResolver;

    public P0FeatureSurfacePanelFactory(IObjectResolver objectResolver)
    {
        this.objectResolver = objectResolver
            ?? throw new ArgumentNullException(nameof(objectResolver));
    }

    public P0FeatureSurfacePanel Ensure(GameObject panelObject, int tabId)
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
    private const int MaxVisibleCardsPerSection = 8;

    private readonly HashSet<string> completedUiActions = new HashSet<string>();
    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    private int tabId = -1;
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
    private IRoomLayoutCache roomLayoutCache;
    private IRoomEnvironmentEvaluator roomEnvironmentEvaluator;
    private IRoomInspectionService roomInspectionService;
    private ITmpKoreanFontService fontService;

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
        IRoomLayoutCache roomLayoutCache,
        IRoomEnvironmentEvaluator roomEnvironmentEvaluator,
        IRoomInspectionService roomInspectionService,
        ITmpKoreanFontService fontService)
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
        this.roomLayoutCache = roomLayoutCache
            ?? throw new ArgumentNullException(nameof(roomLayoutCache));
        this.roomEnvironmentEvaluator = roomEnvironmentEvaluator
            ?? throw new ArgumentNullException(nameof(roomEnvironmentEvaluator));
        this.roomInspectionService = roomInspectionService
            ?? throw new ArgumentNullException(nameof(roomInspectionService));
        this.fontService = fontService
            ?? throw new ArgumentNullException(nameof(fontService));
    }

    public void SetTabId(int id)
    {
        tabId = id;
    }

    private void Awake()
    {
        UITab tab = GetComponent<UITab>();
        if (tab != null && tabId < 0)
        {
            tabId = tab.id;
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
        viewportRect.offsetMax = new Vector2(-12f, -12f);

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

        switch (tabId)
        {
            case 1:
                BuildFacilitiesManagement();
                break;
            case 3:
                BuildFacilityShop();
                break;
            case 4:
                BuildWarehouse();
                break;
            case 5:
                BuildOperationHub();
                break;
            case 6:
                BuildDefenseOperations();
                break;
            case 7:
                BuildOffenseOperations();
                break;
            case 8:
                BuildResearch();
                break;
            case 9:
                BuildCodexAndHistory();
                break;
            default:
                AddEmptyState($"기능 패널이 없는 탭입니다. id={tabId}");
                break;
        }
    }

    private void BuildFacilityShop()
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

        for (int i = 0; i < Mathf.Min(dailyOffers.Count, MaxVisibleCardsPerSection); i++)
        {
            int index = i;
            FacilityShopOffer offer = dailyOffers[i];
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

    private void BuildResearch()
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

        CreateButtonRow(
            "P0Action_ResearchProgress",
            "연구 작업 +10초",
            "활성 설계도에 연구 작업을 적용합니다.",
            () =>
            {
                if (!TryFindResearchFacility(out BuildableObject researchFacility))
                {
                    SetFeedback("연구 실패: 연구 가능한 시설이 현재 씬에 없습니다.");
                    return;
                }

                BlueprintResearchWorkResult result = runtime.ApplyResearchWork(null, researchFacility, 10f);
                SetFeedback(
                    $"{(result.Success ? "연구 진행" : "연구 실패")}: {result.Message} / {GetBlueprintName(result.Blueprint)} {result.ProgressRatio:P0}");
            });

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
            CreateDataCard(
                actionName,
                GetBlueprintName(blueprint),
                $"{status} / 비용 {blueprint.defaultCost} / 희귀도 {blueprint.rarity}",
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
                CompactCardHeight);
        }
    }

    private void BuildWarehouse()
    {
        WarehouseManagementSummary summary = buildingSummaryService.CaptureWarehouses();
        List<IWarehouseFacility> warehouses = FindWarehouses();
        List<Shop> shops = FindShops();

        AddSection(
            "창고 재고",
            summary.HasCapacityLimit
                ? $"창고 {summary.WarehouseCount}개 / 총 재고 {summary.TotalStock}/{summary.TotalCapacity}"
                : $"창고 {summary.WarehouseCount}개 / 총 재고 {summary.TotalStock}");
        AddLabel(
            $"식재료 {summary.FoodStock} / 잡화 {summary.GeneralStock} / 무기 {summary.WeaponStock} / 마력 {summary.ManaStock}",
            20f,
            36f);

        foreach (IWarehouseFacility warehouse in warehouses.Take(MaxVisibleCardsPerSection))
        {
            WarehouseInventory inventory = warehouse.Inventory;
            CreateDataCard(
                "P0State_Warehouse_" + GetUnityObjectId(warehouse),
                GetWarehouseName(warehouse),
                $"총 {inventory.TotalStock}/{(inventory.HasCapacityLimit ? inventory.MaxCapacity.ToString() : "무제한")} / 식 {inventory.GetStock(StockCategory.Food)} 잡 {inventory.GetStock(StockCategory.General)} 무 {inventory.GetStock(StockCategory.Weapon)} 마 {inventory.GetStock(StockCategory.Mana)}",
                "상태",
                () => SetFeedback($"{GetWarehouseName(warehouse)} 재고를 확인했습니다."),
                CompactCardHeight);
        }

        AddSection("상점 수동 보충", $"상점 {shops.Count}개 / 보충 필요 {shops.Count((shop) => shop != null && shop.NeedsRestock)}개");
        foreach (Shop shop in shops.Where((candidate) => candidate != null && candidate.NeedsRestock).Take(MaxVisibleCardsPerSection))
        {
            Shop capturedShop = shop;
            string actionName = "P0Action_WarehouseRestock_" + capturedShop.GetInstanceID();
            CreateDataCard(
                actionName,
                GetBuildingName(capturedShop),
                $"재고 {capturedShop.CurrentStock}/{capturedShop.MaxInternalStock} / 부족 {capturedShop.MissingStock}",
                "보충",
                () =>
                {
                    int beforeShop = capturedShop.CurrentStock;
                    int beforeWarehouse = warehouses.Sum((warehouse) => warehouse.Inventory.TotalStock);
                    int moved = capturedShop.RestockFrom(warehouses, Mathf.Min(5, capturedShop.MissingStock), out string message);
                    int afterShop = capturedShop.CurrentStock;
                    int afterWarehouse = warehouses.Sum((warehouse) => warehouse.Inventory.TotalStock);
                    SetFeedback(
                        $"보충 {(moved > 0 ? "성공" : "실패")}: {GetBuildingName(capturedShop)} {message} / 상점 {beforeShop}->{afterShop}, 창고 {beforeWarehouse}->{afterWarehouse}");
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

    private void BuildOperationHub()
    {
        GameData gameData = ResolveGameData();
        OperatingDaySettlementRuntime settlement = sceneQuery.First<OperatingDaySettlementRuntime>(includeInactive: true);
        OperatingDayReport report = settlement != null ? settlement.LatestReport : null;

        AddSection(
            "운영 정산",
            $"Day {GetCurrentDay()} / {GetCurrentHour()}:00 / 자금 {FormatMoney(gameData)}");
        CreateButtonRow(
            "P0Action_OperationSettleDay",
            "오늘 정산",
            report != null
                ? $"최근 보고서: Day {report.day}, 매출 {report.totalRevenue}, 방문 {report.totalVisits}"
                : "아직 정산 보고서가 없습니다.",
            () =>
            {
                int beforeDay = GetCurrentDay();
                OperatingDayEndedEvent.Trigger(beforeDay);
                if (gameData != null && gameData.day != null)
                {
                    gameData.day.Value = beforeDay + 1;
                }

                OperatingDayStartedEvent.Trigger(beforeDay + 1);
                OperatingDayReport updated = settlement != null ? settlement.LatestReport : null;
                SetFeedback(updated != null
                    ? $"정산 완료: Day {updated.day} 매출 {updated.totalRevenue}, 방문 {updated.totalVisits}, 보고서 생성"
                    : "정산 이벤트를 보냈지만 보고서 런타임을 찾지 못했습니다.");
            });

        if (report != null)
        {
            AddLabel(
                $"최근 보고서: Day {report.day} / 매출 {report.totalRevenue} / 방문 {report.totalVisits} / 재고 부족 {report.stockShortageFacilities.Count} / 사건 {report.incidents.Count}",
                19f,
                52f);
        }

        BuildRecruitmentSection();
        BuildMetaUpgradeSection();
        BuildRunVariableSection();
        BuildEconomyDetailSection();
    }

    private void BuildRecruitmentSection()
    {
        if (!regularCustomerProvider.TryGetRuntime(out RegularCustomerRuntime runtime))
        {
            AddSection("단골/영입", "단골 런타임이 현재 씬에 없습니다.");
            return;
        }

        IReadOnlyList<RegularCustomerRecord> records = runtime.State.Records
            .OrderByDescending((record) => record.Status)
            .ThenByDescending((record) => record.VisitCount)
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
            .ThenBy((item) => item.id)
            .Take(MaxVisibleCardsPerSection))
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
        string typeText = offer.Type == FacilityShopOfferType.Blueprint ? "설계도" : "시설";
        string detail = $"{typeText} / {offer.Rarity} / 비용 {offer.Cost} / {offer.Star}성";
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
        titleText.overflowMode = TextOverflowModes.Ellipsis;

        TMP_Text detailText = AddText(textColumn.transform, detail, 16f, FontStyles.Normal);
        detailText.color = DungeonUiTheme.TextSecondary;
        detailText.enableAutoSizing = true;
        detailText.fontSizeMin = 11f;
        detailText.fontSizeMax = 16f;
        detailText.textWrappingMode = TextWrappingModes.Normal;

        CreateActionButton(card.transform, actionName, buttonText, onClick, 132f, height - 18f);
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
        summaryText.overflowMode = TextOverflowModes.Ellipsis;
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
        feedbackText.overflowMode = TextOverflowModes.Ellipsis;
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
        fontService.Apply(label);
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

    private List<Shop> FindShops()
    {
        return sceneQuery.All<Shop>()
            .Where((shop) => shop != null && !shop.isDestroy)
            .OrderByDescending((shop) => shop.MissingStock)
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

    private static int GetUnityObjectId(object value)
    {
        return value is Component component ? component.GetInstanceID() : value != null ? value.GetHashCode() : 0;
    }

    private static string GetStockCategoryName(StockCategory category)
    {
        return category switch
        {
            StockCategory.Food => "식재료",
            StockCategory.General => "잡화",
            StockCategory.Weapon => "무기",
            StockCategory.Mana => "마력",
            _ => category.ToString()
        };
    }

    private static string FormatBranch(MetaProgressionBranch branch)
    {
        return branch switch
        {
            MetaProgressionBranch.OperationKnowledge => "운영 지식",
            MetaProgressionBranch.DesignPreservation => "설계 보존",
            MetaProgressionBranch.OwnerSurvival => "사장 생존",
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
