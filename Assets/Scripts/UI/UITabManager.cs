using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITabManager : MonoBehaviour
{
    public List<UITab> tabList;
    public Transform buttonPanel;

    [SerializeField] private bool autoBindTopButtons = true;
    [SerializeField] private bool autoCreateMissingTabs = true;

    private const float BottomTabBarHeight = 60f;
    private const float GeneratedPanelBottomOffset = 64f;
    private const float GeneratedPanelHeight = 420f;

    private readonly Dictionary<int, TMP_Text> generatedTabBodies = new Dictionary<int, TMP_Text>();
    private bool isConfigured;

    private static readonly KeyValuePair<int, string>[] TopTabs =
    {
        new KeyValuePair<int, string>(0, "건축"),
        new KeyValuePair<int, string>(1, "건물"),
        new KeyValuePair<int, string>(2, "직원"),
        new KeyValuePair<int, string>(3, "상점"),
        new KeyValuePair<int, string>(4, "창고"),
        new KeyValuePair<int, string>(5, "운영"),
        new KeyValuePair<int, string>(6, "방어"),
        new KeyValuePair<int, string>(7, "원정"),
        new KeyValuePair<int, string>(8, "연구"),
        new KeyValuePair<int, string>(9, "도감")
    };

    private static readonly Dictionary<string, int> TopTabIds = new Dictionary<string, int>
    {
        { "건축", 0 },
        { "건물", 1 },
        { "건물관리", 1 },
        { "직원", 2 },
        { "직원관리", 2 },
        { "상점", 3 },
        { "창고", 4 },
        { "운영", 5 },
        { "침공/방어", 6 },
        { "침공방어", 6 },
        { "방어", 6 },
        { "원정", 7 },
        { "연구/제작", 8 },
        { "연구제작", 8 },
        { "연구", 8 },
        { "제작", 8 },
        { "도감/기록", 9 },
        { "도감기록", 9 },
        { "도감", 9 },
        { "기록", 9 }
    };

    private void Start()
    {
        ConfigureTopTabs();
    }

    public void ToggleSelectButton(int category)
    {
        ConfigureTopTabs();
        EnsureTopButtons();
        EnsureSpecializedTabContent(category);
        RefreshGeneratedTab(category);

        UITab target = GetAllTabs().FirstOrDefault((tab) => tab.id == category);
        bool shouldOpen = target != null && !target.gameObject.activeSelf;

        CloseAllTabsImmediate();

        if (shouldOpen)
        {
            OpenTabImmediate(target);
            EnsureSpecializedTabContent(category);
        }

        if (target == null)
        {
            Debug.LogWarning($"UITabManager.ToggleSelectButton() : id {category}에 해당하는 탭 패널이 없습니다.");
        }
    }
    public void ResgisterTab(UITab tab)
    {
        if (tabList.Contains(tab)) return;
        tabList.Add(tab);
    }
    public void UnRegisterTab(UITab tab)
    {
        if (!tabList.Contains(tab)) return;
        tabList.Remove(tab);
    }

    private void ConfigureTopTabs()
    {
        if (isConfigured) return;
        isConfigured = true;

        tabList ??= new List<UITab>();
        RegisterExistingTabs();
        if (autoCreateMissingTabs)
        {
            foreach (KeyValuePair<int, string> tab in TopTabs)
            {
                if (tab.Key == 0) continue;

                EnsureGeneratedTab(tab.Key, tab.Value);
            }
        }

        EnsureSpecializedTabContent();

        if (autoBindTopButtons)
        {
            EnsureTopButtons();
            BindTopButtons();
        }

        foreach (KeyValuePair<int, TMP_Text> pair in generatedTabBodies)
        {
            RefreshGeneratedTab(pair.Key);
        }

        CloseAllTabsForInitialState();
    }

    private void RegisterExistingTabs()
    {
        foreach (UITab tab in GetComponentsInChildren<UITab>(true))
        {
            if (tab != null && !tabList.Contains(tab))
            {
                tabList.Add(tab);
            }
        }

        tabList = tabList
            .Where((tab) => tab != null)
            .Distinct()
            .ToList();
    }

    private void EnsureSpecializedTabContent()
    {
        foreach (UITab tab in GetAllTabs())
        {
            EnsureSpecializedTabContent(tab);
        }
    }

    private void EnsureSpecializedTabContent(int id)
    {
        if (id != 2)
        {
            return;
        }

        foreach (UITab tab in GetAllTabs().Where((tab) => tab.id == id))
        {
            EnsureSpecializedTabContent(tab);
        }
    }

    private static void EnsureSpecializedTabContent(UITab tab)
    {
        if (tab == null || tab.id != 2)
        {
            return;
        }

        StaffWorkPriorityPanel panel = EnsureStaffPriorityPanel(tab.gameObject);
        if (panel != null)
        {
            panel.Refresh();
        }
    }

    private static StaffWorkPriorityPanel EnsureStaffPriorityPanel(GameObject panelObject)
    {
        if (panelObject == null)
        {
            return null;
        }

        StaffWorkPriorityPanel panel = panelObject.GetComponent<StaffWorkPriorityPanel>();
        if (panel == null)
        {
            panel = panelObject.AddComponent<StaffWorkPriorityPanel>();
        }

        Transform body = panelObject.transform.Find("Body");
        TMP_Text bodyText = body != null ? body.GetComponent<TMP_Text>() : null;
        if (bodyText != null)
        {
            bodyText.text = string.Empty;
            bodyText.enabled = false;
        }

        return panel;
    }

    private IEnumerable<UITab> GetAllTabs()
    {
        RegisterExistingTabs();
        return tabList
            .Concat(GetComponentsInChildren<UITab>(true))
            .Where((tab) => tab != null)
            .Distinct();
    }

    private void CloseAllTabsImmediate()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.CloseAllPopup();
        }

        foreach (UITab tab in GetAllTabs())
        {
            if (tab.gameObject.activeSelf)
            {
                tab.OnClose();
            }
        }
    }

    private void CloseAllTabsForInitialState()
    {
        foreach (UITab tab in GetAllTabs())
        {
            tab.gameObject.SetActive(false);
        }
    }

    private static string GetPanelTitle(int id, string defaultTitle)
    {
        return id switch
        {
            1 => "건물 관리",
            2 => "직원 관리",
            6 => "침공/방어",
            8 => "연구/제작",
            9 => "도감/기록",
            _ => defaultTitle
        };
    }

    private static void OpenTabImmediate(UITab tab)
    {
        if (tab == null) return;

        UIManager.Instance.OpenPopup(tab);
        tab.gameObject.SetActive(true);
    }

    private void BindTopButtons()
    {
        Transform root = ResolveButtonPanel();
        if (root == null)
        {
            Debug.LogWarning("UITabManager : TabButtons 오브젝트를 찾지 못해 상단 탭 자동 연결을 건너뜁니다.");
            return;
        }

        foreach (Button button in root.GetComponentsInChildren<Button>(true))
        {
            string label = GetButtonLabel(button);
            if (!TryGetTopTabId(label, out int tabId))
            {
                continue;
            }

            if (button.onClick.GetPersistentEventCount() > 0)
            {
                continue;
            }

            button.onClick.AddListener(() => ToggleSelectButton(tabId));
        }
    }

    private Transform ResolveButtonPanel()
    {
        if (buttonPanel != null)
        {
            return buttonPanel;
        }

        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "TabButtons")
            {
                buttonPanel = child;
                return buttonPanel;
            }
        }

        return null;
    }

    private static string GetButtonLabel(Button button)
    {
        TMP_Text label = button != null ? button.GetComponentInChildren<TMP_Text>(true) : null;
        return label != null ? label.text : string.Empty;
    }

    private static bool TryGetTopTabId(string label, out int tabId)
    {
        string normalized = NormalizeTopTabLabel(label);
        return TopTabIds.TryGetValue(normalized, out tabId);
    }

    private static string NormalizeTopTabLabel(string label)
    {
        return string.IsNullOrWhiteSpace(label)
            ? string.Empty
            : new string(label.Where((character) => !char.IsWhiteSpace(character)).ToArray());
    }

    private void EnsureTopButtons()
    {
        Transform root = ResolveButtonPanel();
        if (root == null)
        {
            return;
        }

        MoveRowButtonsBackToRoot(root, "TopTabPrimaryRow");
        MoveRowButtonsBackToRoot(root, "TopTabSecondaryRow");

        Button template = root.GetComponentsInChildren<Button>(true)
            .FirstOrDefault((button) => TryGetTopTabId(GetButtonLabel(button), out _));

        foreach (KeyValuePair<int, string> tab in TopTabs)
        {
            Button existing = FindTopButtonForId(root, tab.Key);
            if (existing != null)
            {
                existing.gameObject.SetActive(true);
                SetButtonLabel(existing, tab.Value);
                continue;
            }

            if (template == null)
            {
                Debug.LogError("UITabManager requires at least one top tab button template.");
                return;
            }

            Button button = Instantiate(template, root);

            button.name = $"TopTabButton_{tab.Key}_{tab.Value.Replace("/", string.Empty)}";
            button.onClick = new Button.ButtonClickedEvent();
            SetButtonLabel(button, tab.Value);
        }

        NormalizeTopButtons(root);
        ArrangeTopButtonsInSingleRow(root);

        foreach (Button button in root.GetComponentsInChildren<Button>(true))
        {
            if (!button.gameObject.activeSelf) continue;

            PolishTopButton(button);
        }
    }

    private static void ArrangeTopButtonsInSingleRow(Transform root)
    {
        if (root is RectTransform rootRect)
        {
            rootRect.sizeDelta = new Vector2(rootRect.sizeDelta.x, BottomTabBarHeight);
            rootRect.anchoredPosition = new Vector2(rootRect.anchoredPosition.x, BottomTabBarHeight * 0.5f);
        }

        MoveRowButtonsBackToRoot(root, "TopTabPrimaryRow");
        MoveRowButtonsBackToRoot(root, "TopTabSecondaryRow");

        VerticalLayoutGroup oldVertical = root.GetComponent<VerticalLayoutGroup>();
        if (oldVertical != null)
        {
            oldVertical.enabled = false;
        }

        HorizontalLayoutGroup horizontal = root.GetComponent<HorizontalLayoutGroup>();
        if (horizontal == null)
        {
            horizontal = root.gameObject.AddComponent<HorizontalLayoutGroup>();
        }

        horizontal.enabled = true;
        horizontal.padding = new RectOffset(0, 0, 0, 0);
        horizontal.spacing = 1f;
        horizontal.childAlignment = TextAnchor.MiddleCenter;
        horizontal.childControlWidth = true;
        horizontal.childControlHeight = true;
        horizontal.childForceExpandWidth = true;
        horizontal.childForceExpandHeight = true;

        OrderTopButtons(root);
    }

    private static void MoveRowButtonsBackToRoot(Transform root, string rowName)
    {
        Transform row = root.Find(rowName);
        if (row == null) return;

        while (row.childCount > 0)
        {
            row.GetChild(0).SetParent(root, false);
        }

        row.gameObject.SetActive(false);
    }

    private static void OrderTopButtons(Transform root)
    {
        Button[] buttons = root.GetComponentsInChildren<Button>(true)
            .Where((button) => button.transform.parent == root && button.gameObject.activeSelf)
            .ToArray();

        foreach (Button button in buttons)
        {
            if (!TryGetTopTabId(GetButtonLabel(button), out int tabId))
            {
                continue;
            }

            button.transform.SetSiblingIndex(tabId);
        }
    }

    private static Button FindTopButtonForId(Transform root, int id)
    {
        foreach (Button button in root.GetComponentsInChildren<Button>(true))
        {
            if (TryGetTopTabId(GetButtonLabel(button), out int tabId) && tabId == id)
            {
                return button;
            }
        }

        return null;
    }

    private static void NormalizeTopButtons(Transform root)
    {
        Button[] buttons = root.GetComponentsInChildren<Button>(true)
            .Where((button) => button.transform.parent == root)
            .ToArray();

        foreach (KeyValuePair<int, string> tab in TopTabs)
        {
            Button[] matches = buttons
                .Where((button) => TryGetTopTabId(GetButtonLabel(button), out int tabId) && tabId == tab.Key)
                .ToArray();

            if (matches.Length == 0)
            {
                continue;
            }

            Button keep = matches.FirstOrDefault((button) => NormalizeTopTabLabel(GetButtonLabel(button)) == NormalizeTopTabLabel(tab.Value))
                ?? matches[0];
            keep.gameObject.SetActive(true);
            SetButtonLabel(keep, tab.Value);
            keep.transform.SetSiblingIndex(tab.Key);

            foreach (Button duplicate in matches)
            {
                if (duplicate == keep) continue;

                duplicate.gameObject.SetActive(false);
            }
        }
    }

    private static void SetButtonLabel(Button button, string title)
    {
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label == null) return;

        TMPKoreanFont.Apply(label);
        label.text = title;
    }

    private static void PolishTopButton(Button button)
    {
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label == null) return;

        TMPKoreanFont.Apply(label);
        label.enableAutoSizing = true;
        label.fontSizeMin = 10f;
        label.fontSizeMax = 24f;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
    }

    private void EnsureGeneratedTab(int id, string title)
    {
        if (tabList.Any((tab) => tab != null && tab.id == id))
        {
            return;
        }

        string panelTitle = GetPanelTitle(id, title);
        GameObject panelObject = new GameObject($"{panelTitle}Tab", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(transform, false);

        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, GeneratedPanelBottomOffset);
        rect.sizeDelta = new Vector2(0f, GeneratedPanelHeight);

        Image background = panelObject.GetComponent<Image>();
        background.color = new Color(0.06f, 0.07f, 0.08f, 0.92f);

        GameObject titleObject = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObject.transform.SetParent(panelObject.transform, false);
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -20f);
        titleRect.sizeDelta = new Vector2(-48f, 42f);

        TMP_Text titleText = titleObject.GetComponent<TMP_Text>();
        TMPKoreanFont.Apply(titleText);
        titleText.text = panelTitle;
        titleText.fontSize = 30f;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Left;

        GameObject bodyObject = new GameObject("Body", typeof(RectTransform), typeof(TextMeshProUGUI));
        bodyObject.transform.SetParent(panelObject.transform, false);
        RectTransform bodyRect = bodyObject.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 0f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.offsetMin = new Vector2(24f, 24f);
        bodyRect.offsetMax = new Vector2(-24f, -76f);

        TMP_Text bodyText = bodyObject.GetComponent<TMP_Text>();
        TMPKoreanFont.Apply(bodyText);
        bodyText.fontSize = 22f;
        bodyText.color = new Color(0.9f, 0.94f, 0.96f, 1f);
        bodyText.alignment = TextAlignmentOptions.TopLeft;
        bodyText.textWrappingMode = TextWrappingModes.Normal;

        if (id == 2)
        {
            EnsureStaffPriorityPanel(panelObject);
        }

        UITab tab = panelObject.AddComponent<UITab>();
        tab.id = id;
        panelObject.SetActive(false);

        tabList.Add(tab);
        generatedTabBodies[id] = bodyText;
    }

    private void RefreshGeneratedTab(int id)
    {
        if (id == 2)
        {
            EnsureSpecializedTabContent(id);
            return;
        }

        if (!generatedTabBodies.TryGetValue(id, out TMP_Text body) || body == null)
        {
            return;
        }

        body.text = id switch
        {
            1 => BuildBuildingManagementText(),
            2 => BuildStaffManagementText(),
            3 => BuildShopText(),
            4 => BuildWarehouseText(),
            5 => BuildOperationText(),
            6 => BuildInvasionDefenseText(),
            7 => BuildOffenseText(),
            8 => BuildResearchCraftingText(),
            9 => BuildCodexRecordText(),
            _ => string.Empty
        };
    }

    private static string BuildBuildingManagementText()
    {
        BuildableObject[] buildings = UnityEngine.Object.FindObjectsByType<BuildableObject>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        int damaged = buildings.Count((building) => building != null && building.IsDamaged);
        int visitorFacilities = buildings.Count((building) => building != null && building.Facility != null && building.Facility.IsVisitorFacility);
        int workableFacilities = buildings.Count((building) => building != null && building.Facility != null && building.Facility.supportedWorkTypes != FacilityWorkType.None);

        return string.Join("\n", new[]
        {
            $"총 건물: {buildings.Length}",
            $"방문 가능 시설: {visitorFacilities}",
            $"직원 작업 가능 시설: {workableFacilities}",
            $"수리 필요: {damaged}",
            string.Empty,
            "다음 UI 연결 후보",
            "- 선택 건물 상세: 레벨, 내구/손상, 수용 인원, 담당 직원",
            "- 일괄 수리/업그레이드/철거",
            "- 보충 필요 시설과 연구 가능 시설 필터"
        });
    }

    private static string BuildStaffManagementText()
    {
        Character[] characters = UnityEngine.Object.FindObjectsByType<Character>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        int staffCount = 0;
        int offDutyCount = 0;
        int workingCount = 0;
        int expeditionCount = 0;

        foreach (Character character in characters)
        {
            if (character == null || !character.TryGetAbility(out AbilityWork work)) continue;

            staffCount++;
            if (work.IsOffDuty) offDutyCount++;
            if (work.isWorking) workingCount++;
            if (character.IsOnExpedition) expeditionCount++;
        }

        return string.Join("\n", new[]
        {
            $"직원 수: {staffCount}",
            $"근무 중: {workingCount}",
            $"비번/휴식 보호: {offDutyCount}",
            $"원정 중: {expeditionCount}",
            string.Empty,
            "다음 UI 연결 후보",
            "- 직원별 업무 우선순위",
            "- 불만도/피로도/기분 위험 목록",
            "- 특정 시설 우선 배치와 침입자 제압 명령"
        });
    }

    private static string BuildShopText()
    {
        Shop[] shops = UnityEngine.Object.FindObjectsByType<Shop>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        int stocked = shops.Count((shop) => shop != null && shop.HasAvailableStock);
        int empty = shops.Length - stocked;

        return string.Join("\n", new[]
        {
            $"상점 수: {shops.Length}",
            $"판매 가능: {stocked}",
            $"품절/보충 필요: {empty}",
            string.Empty,
            "다음 UI 연결 후보",
            "- 오늘의 시설 상점 상품",
            "- 판매 재고/가격/품절 위험",
            "- 창고에서 수동 보충 요청",
            "- 구매한 설계도와 연구 잠금 상태"
        });
    }

    private static string BuildWarehouseText()
    {
        IWarehouseFacility[] warehouses = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None)
            .OfType<IWarehouseFacility>()
            .Where((warehouse) => warehouse.HasWarehouseInventory && warehouse.Inventory != null)
            .ToArray();

        int totalStock = warehouses.Sum((warehouse) => warehouse.Inventory.TotalStock);
        int totalCapacity = warehouses.Any((warehouse) => warehouse.Inventory.HasCapacityLimit)
            ? warehouses.Sum((warehouse) => warehouse.Inventory.HasCapacityLimit ? warehouse.Inventory.MaxCapacity : 0)
            : 0;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"창고 시설: {warehouses.Length}");
        builder.AppendLine(totalCapacity > 0 ? $"총 재고: {totalStock} / {totalCapacity}" : $"총 재고: {totalStock}");
        builder.AppendLine($"식재료: {warehouses.Sum((warehouse) => warehouse.Inventory.GetStock(StockCategory.Food))}");
        builder.AppendLine($"잡화: {warehouses.Sum((warehouse) => warehouse.Inventory.GetStock(StockCategory.General))}");
        builder.AppendLine($"무기: {warehouses.Sum((warehouse) => warehouse.Inventory.GetStock(StockCategory.Weapon))}");
        builder.AppendLine($"마력: {warehouses.Sum((warehouse) => warehouse.Inventory.GetStock(StockCategory.Mana))}");
        builder.AppendLine();
        builder.AppendLine("창고 탭에 들어가야 할 핵심");
        builder.AppendLine("- 카테고리별 보유량과 남은 용량");
        builder.AppendLine("- 상점 보충 예약/수동 보충");
        builder.AppendLine("- 원정 보상 입고와 시설 생산 입고 로그");
        builder.AppendLine("- 품절 위험 품목과 하루 예상 소모량");
        builder.AppendLine("- 창고별 우선순위: 식재료/무기/마력 전용화");
        return builder.ToString();
    }

    private static string BuildOperationText()
    {
        UIManager uiManager = UIManager.TryGetInstance();
        GameData gameData = uiManager != null ? uiManager.gameData : null;
        OperatingDaySettlementRuntime settlement = Object.FindFirstObjectByType<OperatingDaySettlementRuntime>();
        EventAlertRuntime alerts = Object.FindFirstObjectByType<EventAlertRuntime>();
        RunVariableRuntime runVariables = RunVariableRuntime.Instance;

        StringBuilder builder = new StringBuilder();
        if (gameData != null)
        {
            builder.AppendLine($"날짜: Day {gameData.day.Value} / {gameData.hour.Value}:00");
            builder.AppendLine($"보유 자금: {gameData.holdingMoney.Value}");
            builder.AppendLine($"게임 속도: x{gameData.gameSpeed.Value}");
        }
        else
        {
            builder.AppendLine("날짜/자금 데이터: 연결 필요");
        }

        builder.AppendLine($"이벤트 알림: {(alerts != null ? alerts.EventLog.Count : 0)}");
        builder.AppendLine($"최근 정산 보고서: {(settlement != null && settlement.LatestReport != null ? $"Day {settlement.LatestReport.day}" : "없음")}");
        builder.AppendLine($"런 변수: {(runVariables != null ? "활성" : "없음")}");
        builder.AppendLine();
        builder.AppendLine("운영 탭에 들어갈 핵심");
        builder.AppendLine("- 일일 정산, 매출, 유지비, 순이익");
        builder.AppendLine("- 방문자 수, 평균 만족도, 단골 변화");
        builder.AppendLine("- 당일 사건/알림 로그와 선택지");
        builder.AppendLine("- 다음 운영일 진행 버튼");
        return builder.ToString();
    }

    private static string BuildInvasionDefenseText()
    {
        InvasionThreatRuntime threat = InvasionThreatRuntime.Instance;
        InvasionDirectorRuntime director = Object.FindFirstObjectByType<InvasionDirectorRuntime>();
        InvasionCombatReportRuntime reportRuntime = Object.FindFirstObjectByType<InvasionCombatReportRuntime>();
        BuildableObject[] buildings = Object.FindObjectsByType<BuildableObject>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        int defenseFacilities = buildings.Count((building) => building != null && building.BuildingData != null && building.BuildingData.Defense.IsDefenseFacility);
        int damagedFacilities = buildings.Count((building) => building != null && building.IsDamaged);
        int activeIntruders = director != null ? director.ActiveIntruders.Count : 0;

        StringBuilder builder = new StringBuilder();
        if (threat != null)
        {
            builder.AppendLine($"위협도: {threat.CurrentThreat:0.#}");
            builder.AppendLine($"단계: {threat.CurrentStage}");
            builder.AppendLine($"안전 시간: {threat.SafetyRemaining:0.#}초");
            builder.AppendLine($"침공 후보 대기: {(threat.IsCandidatePending ? "예" : "아니오")}");
        }
        else
        {
            builder.AppendLine("위협도 런타임: 없음");
        }

        builder.AppendLine($"활성 침입자: {activeIntruders}");
        builder.AppendLine($"방어 시설: {defenseFacilities}");
        builder.AppendLine($"손상 시설: {damagedFacilities}");
        builder.AppendLine($"최근 전투 보고서: {(reportRuntime != null && reportRuntime.CurrentReport != null ? "있음" : "없음")}");
        builder.AppendLine();
        builder.AppendLine("침공/방어 탭에 들어갈 핵심");
        builder.AppendLine("- 위협도 게이지와 침공 예고");
        builder.AppendLine("- 활성 침입자 위치/상태");
        builder.AppendLine("- 방어 시설 발동 로그와 파손 목록");
        builder.AppendLine("- 전투 결과 리포트");
        return builder.ToString();
    }

    private static string BuildOffenseText()
    {
        OffenseWorldMapRuntime worldMap = OffenseWorldMapRuntime.Instance;
        OffenseExpeditionRuntime expedition = OffenseExpeditionRuntime.Instance;
        OffenseRewardRuntime rewards = OffenseRewardRuntime.Instance;

        StringBuilder builder = new StringBuilder();
        if (worldMap != null)
        {
            builder.AppendLine($"정찰 레벨: {worldMap.State.ReconLevel}");
            builder.AppendLine($"정찰 범위: {worldMap.CurrentScanRange:0.#}");
            builder.AppendLine($"발견 목표: {worldMap.VisibleTargets.Count}");
            builder.AppendLine($"선택 목표: {(string.IsNullOrWhiteSpace(worldMap.State.SelectedTargetId) ? "없음" : worldMap.State.SelectedTargetId)}");
        }
        else
        {
            builder.AppendLine("월드맵 런타임: 없음");
        }

        builder.AppendLine($"진행 중 원정: {(expedition != null ? expedition.ActiveExpeditions.Count : 0)}");
        builder.AppendLine($"누적 원정 자금: {(rewards != null ? rewards.State.MoneyEarned : 0)}");
        builder.AppendLine($"포로/후보: {(rewards != null ? $"{rewards.State.PrisonerCount}/{rewards.State.RecruitCandidateCount}" : "0/0")}");
        builder.AppendLine();
        builder.AppendLine("원정 탭에 들어갈 핵심");
        builder.AppendLine("- 월드맵 목표 선택");
        builder.AppendLine("- 파티 편성, 요구 전투력, 성공 확률");
        builder.AppendLine("- 진행 중 원정 타이머");
        builder.AppendLine("- 보상 수령과 창고 입고 내역");
        return builder.ToString();
    }

    private static string BuildResearchCraftingText()
    {
        BlueprintResearchRuntime research = BlueprintResearchRuntime.Instance;
        FacilitySynthesisRuntime synthesis = FacilitySynthesisRuntime.Instance;

        BlueprintResearchTask activeTask = null;
        if (research != null)
        {
            research.State.TryGetActiveTask(out activeTask);
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"연구 작업 수: {(research != null ? research.State.Tasks.Count : 0)}");
        builder.AppendLine($"완료 설계도: {(research != null ? research.State.CompletedBlueprintIds.Count : 0)}");
        builder.AppendLine(activeTask != null
            ? $"진행 중 연구: {activeTask.Blueprint.DisplayName} {activeTask.ProgressRatio:P0}"
            : "진행 중 연구: 없음");
        builder.AppendLine($"선택 합성 재료: {(synthesis != null ? synthesis.SelectedMaterials.Count : 0)}");
        builder.AppendLine($"보이는 합성식: {(synthesis != null ? synthesis.VisibleRecipes.Count : 0)}");
        builder.AppendLine();
        builder.AppendLine("연구/제작 탭에 들어갈 핵심");
        builder.AppendLine("- 설계도 연구 큐와 진행률");
        builder.AppendLine("- 연구 완료 보상/해금 시설");
        builder.AppendLine("- 시설 합성 재료 선택");
        builder.AppendLine("- 결과 시설 미리보기와 합성 확정");
        return builder.ToString();
    }

    private static string BuildCodexRecordText()
    {
        CodexRuntime codex = CodexRuntime.Instance;
        OperatingDaySettlementRuntime settlement = Object.FindFirstObjectByType<OperatingDaySettlementRuntime>();
        EventAlertRuntime alerts = Object.FindFirstObjectByType<EventAlertRuntime>();

        int monsterEntries = codex != null ? codex.GetEntries(CodexEntryCategory.Monster).Count : 0;
        int invasionEntries = codex != null ? codex.GetEntries(CodexEntryCategory.Invasion).Count : 0;
        int facilityEntries = codex != null ? codex.GetEntries(CodexEntryCategory.Facility).Count : 0;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"몬스터 도감: {monsterEntries}");
        builder.AppendLine($"침공 도감: {invasionEntries}");
        builder.AppendLine($"시설 도감: {facilityEntries}");
        builder.AppendLine($"이벤트 기록: {(alerts != null ? alerts.EventLog.Count : 0)}");
        builder.AppendLine($"최근 운영 보고서: {(settlement != null && settlement.LatestReport != null ? $"Day {settlement.LatestReport.day}" : "없음")}");
        builder.AppendLine();
        builder.AppendLine("도감/기록 탭에 들어갈 핵심");
        builder.AppendLine("- 몬스터/침입자/시설 도감");
        builder.AppendLine("- 발견 조건과 새 정보 표시");
        builder.AppendLine("- 운영일 보고서 히스토리");
        builder.AppendLine("- 침공 전투 리포트 아카이브");
        return builder.ToString();
    }
}
