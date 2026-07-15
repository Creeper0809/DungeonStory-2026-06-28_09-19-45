using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class UITabManager : MonoBehaviour
{
    public List<UITab> tabList;
    public Transform buttonPanel;

    [SerializeField] private bool autoBindTopButtons = true;
    [SerializeField] private bool autoCreateMissingTabs = true;

    private const float BottomTabBarHeight = 60f;
    private const int HudCanvasSortingOrder = 300;

    private readonly Dictionary<int, TMP_Text> generatedTabBodies = new Dictionary<int, TMP_Text>();
    private IUITabContentTextProvider contentTextProvider;
    private IUiPopupService popupService;
    private ITmpKoreanFontService tmpKoreanFontService;
    private IUITabGeneratedPanelFactory generatedPanelFactory;
    private IStaffWorkPriorityPanelFactory staffWorkPriorityPanelFactory;
    private IP0FeatureSurfacePanelFactory p0FeatureSurfacePanelFactory;
    private IUITabTopButtonFactory topButtonFactory;
    private IDungeonGridBuildingControllerProvider buildingControllerProvider;
    private DungeonUiThemeRuntime themeRuntime;
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

    [Inject]
    public void Construct(
        IUITabContentTextProvider contentTextProvider,
        IUiPopupService popupService,
        ITmpKoreanFontService tmpKoreanFontService,
        IUITabGeneratedPanelFactory generatedPanelFactory,
        IStaffWorkPriorityPanelFactory staffWorkPriorityPanelFactory,
        IP0FeatureSurfacePanelFactory p0FeatureSurfacePanelFactory,
        IUITabTopButtonFactory topButtonFactory,
        IDungeonGridBuildingControllerProvider buildingControllerProvider)
    {
        this.contentTextProvider = contentTextProvider
            ?? throw new ArgumentNullException(nameof(contentTextProvider));
        this.popupService = popupService
            ?? throw new ArgumentNullException(nameof(popupService));
        this.tmpKoreanFontService = tmpKoreanFontService
            ?? throw new ArgumentNullException(nameof(tmpKoreanFontService));
        this.generatedPanelFactory = generatedPanelFactory
            ?? throw new ArgumentNullException(nameof(generatedPanelFactory));
        this.staffWorkPriorityPanelFactory = staffWorkPriorityPanelFactory
            ?? throw new ArgumentNullException(nameof(staffWorkPriorityPanelFactory));
        this.p0FeatureSurfacePanelFactory = p0FeatureSurfacePanelFactory
            ?? throw new ArgumentNullException(nameof(p0FeatureSurfacePanelFactory));
        this.topButtonFactory = topButtonFactory
            ?? throw new ArgumentNullException(nameof(topButtonFactory));
        this.buildingControllerProvider = buildingControllerProvider
            ?? throw new ArgumentNullException(nameof(buildingControllerProvider));
    }

    public void ToggleSelectButton(int category)
    {
        CancelActiveGridPlacement();
        ConfigureTopTabs();
        EnsureTopButtons();
        EnsureSpecializedTabContent(category);
        RefreshGeneratedTab(category);

        UITab target = GetTopLevelTabs().FirstOrDefault((tab) => tab.id == category);
        bool shouldOpen = target != null && !target.gameObject.activeSelf;

        CloseAllTabsImmediate();

        if (shouldOpen)
        {
            OpenTabImmediate(target);
            EnsureSpecializedTabContent(category);
        }

        ResolveThemeRuntime()?.SetActiveTab(shouldOpen ? category : null);

        if (target == null)
        {
            Debug.LogWarning($"UITabManager.ToggleSelectButton() : id {category}에 해당하는 탭 패널이 없습니다.");
        }
    }

    private void CancelActiveGridPlacement()
    {
        DungeonStoryGridBuildingController controller = buildingControllerProvider?.Controller;
        if (controller != null && controller.GridSystem.Mode != GridMode.None)
        {
            controller.SetGridModeNone();
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
        EnsureHudCanvasRendersAboveWorld();

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
        ResolveThemeRuntime()?.ApplyNow();
    }

    private void EnsureHudCanvasRendersAboveWorld()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            return;
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = Mathf.Max(canvas.sortingOrder, HudCanvasSortingOrder);
        themeRuntime = DungeonUiThemeRuntime.Ensure(canvas, tmpKoreanFontService);
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
        foreach (UITab tab in GetTopLevelTabs())
        {
            EnsureSpecializedTabContent(tab);
        }
    }

    private void EnsureSpecializedTabContent(int id)
    {
        if (id != 2 && !IsFeatureSurfaceTab(id))
        {
            return;
        }

        foreach (UITab tab in GetTopLevelTabs().Where((tab) => tab.id == id))
        {
            EnsureSpecializedTabContent(tab);
        }
    }

    private void EnsureSpecializedTabContent(UITab tab)
    {
        if (tab == null)
        {
            return;
        }

        if (tab.id == 2)
        {
            StaffWorkPriorityPanel panel = RequireStaffWorkPriorityPanelFactory().Ensure(tab.gameObject);
            if (panel != null)
            {
                panel.Refresh();
            }

            return;
        }

        if (IsFeatureSurfaceTab(tab.id))
        {
            P0FeatureSurfacePanel panel = RequireP0FeatureSurfacePanelFactory().Ensure(tab.gameObject, tab.id);
            if (panel != null)
            {
                panel.Refresh();
            }
        }
    }

    private IEnumerable<UITab> GetAllTabs()
    {
        RegisterExistingTabs();
        return tabList
            .Concat(GetComponentsInChildren<UITab>(true))
            .Where((tab) => tab != null)
            .Distinct();
    }

    private IEnumerable<UITab> GetTopLevelTabs()
    {
        return GetAllTabs().Where((tab) => tab.transform.parent == transform);
    }

    private void CloseAllTabsImmediate()
    {
        ResolvePopupService().CloseAll();

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

        ResolveThemeRuntime()?.SetActiveTab(null);
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

    private void OpenTabImmediate(UITab tab)
    {
        if (tab == null) return;

        ResolvePopupService().Open(tab);
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

            RequireTopButtonFactory().CreateButton(template, root, tab.Key, tab.Value);
        }

        NormalizeTopButtons(root);
        ArrangeTopButtonsInSingleRow(root);

        foreach (Button button in root.GetComponentsInChildren<Button>(true))
        {
            if (!button.gameObject.activeSelf) continue;

            PolishTopButton(button);
        }
    }

    private void ArrangeTopButtonsInSingleRow(Transform root)
    {
        RequireTopButtonFactory().EnsureSingleRowLayout(
            root,
            BottomTabBarHeight,
            "TopTabPrimaryRow",
            "TopTabSecondaryRow");
        OrderTopButtons(root);
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

    private void NormalizeTopButtons(Transform root)
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

    private void SetButtonLabel(Button button, string title)
    {
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label == null) return;

        RequireTmpKoreanFontService().Apply(label);
        label.text = title;
    }

    private void PolishTopButton(Button button)
    {
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label == null) return;

        RequireTmpKoreanFontService().Apply(label);
        label.enableAutoSizing = true;
        label.fontSizeMin = 10f;
        label.fontSizeMax = 24f;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
    }

    private void EnsureGeneratedTab(int id, string title)
    {
        if (GetTopLevelTabs().Any((tab) => tab.id == id))
        {
            return;
        }

        string panelTitle = GetPanelTitle(id, title);
        GeneratedUITabPanel generatedPanel = RequireGeneratedPanelFactory()
            .Create(transform, id, panelTitle);
        tabList.Add(generatedPanel.Tab);
        generatedTabBodies[id] = generatedPanel.BodyText;
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

        body.text = BuildTabContent(id);
    }

    private string BuildTabContent(int id)
    {
        if (contentTextProvider == null)
        {
            throw new InvalidOperationException(
                $"{nameof(UITabManager)} requires VContainer injection of {nameof(IUITabContentTextProvider)}.");
        }

        return contentTextProvider.Build(id);
    }

    private static bool IsFeatureSurfaceTab(int id)
    {
        return id == 1
            || id == 3
            || id == 4
            || id == 5
            || id == 6
            || id == 7
            || id == 8
            || id == 9;
    }

    private IUiPopupService ResolvePopupService()
    {
        if (popupService == null)
        {
            throw new InvalidOperationException(
                $"{nameof(UITabManager)} requires VContainer injection of {nameof(IUiPopupService)}.");
        }

        return popupService;
    }

    private ITmpKoreanFontService RequireTmpKoreanFontService()
    {
        return tmpKoreanFontService
            ?? throw new InvalidOperationException(
                $"{nameof(UITabManager)} requires VContainer injection of {nameof(ITmpKoreanFontService)}.");
    }

    private IUITabGeneratedPanelFactory RequireGeneratedPanelFactory()
    {
        return generatedPanelFactory
            ?? throw new InvalidOperationException(
                $"{nameof(UITabManager)} requires VContainer injection of {nameof(IUITabGeneratedPanelFactory)}.");
    }

    private IStaffWorkPriorityPanelFactory RequireStaffWorkPriorityPanelFactory()
    {
        return staffWorkPriorityPanelFactory
            ?? throw new InvalidOperationException(
                $"{nameof(UITabManager)} requires VContainer injection of {nameof(IStaffWorkPriorityPanelFactory)}.");
    }

    private IP0FeatureSurfacePanelFactory RequireP0FeatureSurfacePanelFactory()
    {
        return p0FeatureSurfacePanelFactory
            ?? throw new InvalidOperationException(
                $"{nameof(UITabManager)} requires VContainer injection of {nameof(IP0FeatureSurfacePanelFactory)}.");
    }

    private IUITabTopButtonFactory RequireTopButtonFactory()
    {
        return topButtonFactory
            ?? throw new InvalidOperationException(
                $"{nameof(UITabManager)} requires VContainer injection of {nameof(IUITabTopButtonFactory)}.");
    }

    private DungeonUiThemeRuntime ResolveThemeRuntime()
    {
        if (themeRuntime != null)
        {
            return themeRuntime;
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            return null;
        }

        themeRuntime = DungeonUiThemeRuntime.Ensure(canvas, tmpKoreanFontService);
        return themeRuntime;
    }
}
