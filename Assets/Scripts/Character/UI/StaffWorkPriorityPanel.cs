using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public partial class StaffWorkPriorityPanel : MonoBehaviour, UtilEventListener<InfoFeedEvent>
{
    private const float CharacterColumnWidth = 180f;
    private const float WorkColumnWidth = 98f;
    private const float StatusColumnWidth = 270f;
    private const float RowHeight = 78f;
    private const float HeaderHeight = 56f;
    private const float PanelPadding = 16f;
    private static readonly IDungeonSceneComponentQuery FallbackSceneQuery =
        new DungeonSceneComponentQuery();
    private static readonly IStaffWorkPriorityPanelUiFactory FallbackUiFactory =
        new StaffWorkPriorityPanelUiFactory(new StaffWorkPriorityPanelNoopFontService());

    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform rowRoot;
    [SerializeField] private Button rowButtonPrefab;
    [SerializeField] private TMP_Text selectedCharacterText;
    [SerializeField] private bool hideWhenSelectedCharacterCannotWork;

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private CharacterActor selectedCharacter;
    private RectTransform contentRoot;
    private RectTransform tableRoot;
    private TMP_Text titleText;
    private int lastWorkerHash;
    private float nextAutoRefreshAt;
    private IStaffWorkPriorityPanelModelBuilder modelBuilder;
    private IStaffWorkPriorityPanelUiFactory uiFactory;
    private IDungeonSceneComponentQuery sceneQuery;
    public int VisibleWorkerCount { get; private set; }
    public int VisibleCellCount { get; private set; }

    [Inject]
    public void ConstructStaffWorkPriorityPanel(
        IStaffWorkPriorityPanelModelBuilder modelBuilder,
        IStaffWorkPriorityPanelUiFactory uiFactory,
        IDungeonSceneComponentQuery sceneQuery)
    {
        this.modelBuilder = modelBuilder
            ?? throw new ArgumentNullException(nameof(modelBuilder));
        this.uiFactory = uiFactory
            ?? throw new ArgumentNullException(nameof(uiFactory));
        this.sceneQuery = sceneQuery
            ?? throw new ArgumentNullException(nameof(sceneQuery));
    }

    private void Awake()
    {
        panelRoot ??= gameObject;
        EnsureFallbackServices();
    }

    private void Start()
    {
        RequireUiFactory().ApplyFonts(transform);
        Refresh();
    }

    private void Update()
    {
        if (!isActiveAndEnabled || Time.unscaledTime < nextAutoRefreshAt)
        {
            return;
        }

        nextAutoRefreshAt = Time.unscaledTime + 0.5f;
        int workerHash = CalculateWorkerHash();
        if (workerHash != lastWorkerHash)
        {
            Refresh();
        }
    }

    public void OnTriggerEvent(InfoFeedEvent eventType)
    {
        CharacterActor actor = eventType.infoable as CharacterActor;
        if (actor != null && actor.TryGetAbility(out AbilityWork _))
        {
            selectedCharacter = actor;
            Refresh();
            return;
        }

        if (hideWhenSelectedCharacterCannotWork)
        {
            selectedCharacter = null;
            Refresh();
        }
    }

    public void Refresh()
    {
        EnsureLayout();
        BuildTable();
    }

    private void EnsureLayout()
    {
        if (contentRoot != null && tableRoot != null)
        {
            return;
        }

        RectTransform host = ResolveHost();
        ClearHost(host);

        GameObject titleObject = RequireUiFactory().CreateUiObject("Title", host);
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(0f, 44f);

        titleText = RequireUiFactory().AddText(titleObject);
        titleText.text = "직원 작업 우선순위";
        titleText.color = DungeonUiTheme.TextPrimary;
        titleText.fontSize = 28f;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Left;

        BuildModeBar(host);

        GameObject scrollObject = RequireUiFactory().CreateUiObject("PriorityScrollView", host);
        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = Vector2.zero;
        scrollRectTransform.anchorMax = Vector2.one;
        scrollRectTransform.offsetMin = Vector2.zero;
        scrollRectTransform.offsetMax = new Vector2(0f, -106f);

        RequireUiFactory().AddImage(scrollObject, DungeonUiTheme.SurfaceMuted);

        ScrollRect scrollRect = RequireUiFactory().AddScrollRect(scrollObject);

        GameObject viewportObject = RequireUiFactory().CreateUiObject("Viewport", scrollRectTransform);
        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(PanelPadding, PanelPadding);
        viewportRect.offsetMax = new Vector2(-PanelPadding, -PanelPadding);

        RequireUiFactory().AddImage(viewportObject, new Color(1f, 1f, 1f, 0.01f));
        RequireUiFactory().AddMask(viewportObject, false);

        GameObject contentObject = RequireUiFactory().CreateUiObject("Content", viewportRect);
        contentRoot = contentObject.GetComponent<RectTransform>();
        contentRoot.anchorMin = new Vector2(0f, 1f);
        contentRoot.anchorMax = new Vector2(0f, 1f);
        contentRoot.pivot = new Vector2(0f, 1f);
        contentRoot.anchoredPosition = Vector2.zero;

        RequireUiFactory().AddVerticalLayoutGroup(contentObject);
        RequireUiFactory().AddContentSizeFitter(contentObject);

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRoot;
        tableRoot = contentRoot;
    }

    private RectTransform ResolveHost()
    {
        Transform body = transform.Find("Body");
        if (body != null && body is RectTransform bodyRect)
        {
            TMP_Text bodyText = body.GetComponent<TMP_Text>();
            if (bodyText != null)
            {
                bodyText.text = string.Empty;
                bodyText.enabled = false;
            }

            return bodyRect;
        }

        if (rowRoot != null && rowRoot is RectTransform rowRect)
        {
            return rowRect;
        }

        RectTransform rect = transform as RectTransform;
        if (rect != null)
        {
            return rect;
        }

        return RequireUiFactory().EnsureRectTransform(gameObject);
    }

    private void ClearHost(RectTransform host)
    {
        if (host == null)
        {
            return;
        }

        for (int i = host.childCount - 1; i >= 0; i--)
        {
            Transform child = host.GetChild(i);
            if (child == rowButtonPrefab?.transform
                || child == selectedCharacterText?.transform)
            {
                child.gameObject.SetActive(false);
                continue;
            }

            RequireUiFactory().Release(child.gameObject);
        }
    }

    private void BuildTable()
    {
        if (tableRoot == null)
        {
            return;
        }

        ClearSpawnedObjects();

        FacilityWorkType[] workTypes = WorkTaskCatalog.TaskTypes;
        IReadOnlyList<StaffWorkPriorityRowModel> workers = RequireModelBuilder().BuildRows();
        VisibleWorkerCount = workers.Count;
        VisibleCellCount = workers.Count * workTypes.Length;
        lastWorkerHash = RequireModelBuilder().CalculateWorkerHash(workers);

        if (panelMode == StaffPanelMode.Management)
        {
            BuildStaffManagement(workers);
            return;
        }

        if (titleText != null)
        {
            string selectedName = selectedCharacter != null
                ? RequireModelBuilder().GetDisplayName(selectedCharacter)
                : string.Empty;
            titleText.text = string.IsNullOrEmpty(selectedName)
                ? $"직원 작업 우선순위 ({workers.Count})"
                : $"직원 작업 우선순위 ({workers.Count}) - {selectedName}";
        }

        float tableWidth = CharacterColumnWidth + StatusColumnWidth + (WorkColumnWidth * workTypes.Length);
        float tableHeight = HeaderHeight + (RowHeight * Mathf.Max(1, workers.Count));
        contentRoot.sizeDelta = new Vector2(tableWidth, tableHeight);

        if (workers.Count == 0)
        {
            GameObject emptyRow = CreateRow("Empty", tableWidth, HeaderHeight);
            CreateLabelCell(emptyRow.transform, "직원 없음", tableWidth, HeaderHeight, TextAlignmentOptions.Center, true);
            return;
        }

        BuildHeader(workTypes, tableWidth);
        foreach (StaffWorkPriorityRowModel worker in workers)
        {
            BuildWorkerRow(worker, workTypes, tableWidth);
        }
    }

    private void BuildHeader(IReadOnlyList<FacilityWorkType> workTypes, float tableWidth)
    {
        GameObject row = CreateRow("Header", tableWidth, HeaderHeight);
        CreateLabelCell(row.transform, "캐릭터", CharacterColumnWidth, HeaderHeight, TextAlignmentOptions.Center, true);

        foreach (FacilityWorkType workType in workTypes)
        {
            TMP_Text label = CreateLabelCell(
                row.transform,
                GetWorkTypeLabel(workType),
                WorkColumnWidth,
                HeaderHeight,
                TextAlignmentOptions.Center,
                true);
            label.enableAutoSizing = true;
            label.fontSizeMin = 14f;
            label.fontSizeMax = 20f;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.color = DungeonUiTheme.TextPrimary;
        }

        CreateLabelCell(row.transform, "상태", StatusColumnWidth, HeaderHeight, TextAlignmentOptions.Center, true);
    }

    private void BuildWorkerRow(StaffWorkPriorityRowModel worker, IReadOnlyList<FacilityWorkType> workTypes, float tableWidth)
    {
        GameObject row = CreateRow(worker.Character.name, tableWidth, RowHeight);
        TMP_Text nameLabel = CreateLabelCell(
            row.transform,
            worker.Name,
            CharacterColumnWidth,
            RowHeight,
            TextAlignmentOptions.Left,
            false);
        nameLabel.enableAutoSizing = true;
        nameLabel.fontSizeMin = 13f;
        nameLabel.fontSizeMax = 19f;
        nameLabel.fontStyle = worker.Character == selectedCharacter ? FontStyles.Bold : FontStyles.Normal;
        nameLabel.color = worker.Character == selectedCharacter
            ? DungeonUiTheme.Warning
            : DungeonUiTheme.TextPrimary;

        foreach (FacilityWorkType workType in workTypes)
        {
            CreatePriorityCell(row.transform, worker, workType);
        }

        TMP_Text statusLabel = CreateLabelCell(
            row.transform,
            GetWorkerStatus(worker),
            StatusColumnWidth,
            RowHeight,
            TextAlignmentOptions.Left,
            false);
        statusLabel.enableAutoSizing = true;
        statusLabel.fontSizeMin = 10f;
        statusLabel.fontSizeMax = 14f;
        statusLabel.textWrappingMode = TextWrappingModes.Normal;
    }

    private GameObject CreateRow(string name, float width, float height)
    {
        GameObject row = RequireUiFactory().CreateUiObject(name, tableRoot);
        spawnedObjects.Add(row);

        RectTransform rect = row.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);

        RequireUiFactory().AddHorizontalLayoutGroup(row);
        RequireUiFactory().AddLayoutElement(row, width, height);
        return row;
    }

    private TMP_Text CreateLabelCell(
        Transform parent,
        string text,
        float width,
        float height,
        TextAlignmentOptions alignment,
        bool header)
    {
        GameObject cell = CreateCellObject("Label", parent, width, height);
        RequireUiFactory().AddImage(
            cell,
            header ? DungeonUiTheme.SurfaceRaised : DungeonUiTheme.Surface);

        TMP_Text label = AddCellText(cell.transform, text, alignment, header);
        label.fontSize = header ? 16f : 18f;
        label.color = DungeonUiTheme.TextPrimary;
        label.margin = alignment == TextAlignmentOptions.Left
            ? new Vector4(8f, 0f, 4f, 0f)
            : Vector4.zero;

        return label;
    }

    private void CreatePriorityCell(Transform parent, StaffWorkPriorityRowModel worker, FacilityWorkType workType)
    {
        WorkPriorityLevel priority = worker.Work.WorkPriorities.GetPriority(workType);
        GameObject cell = CreateCellObject($"Cell_{worker.Character.GetInstanceID()}_{workType}", parent, WorkColumnWidth, RowHeight);
        Image image = RequireUiFactory().AddImage(cell, GetPriorityColor(priority, worker.Character == selectedCharacter));

        Button button = RequireUiFactory().AddButton(cell, image);
        FacilityWorkType capturedType = workType;
        AbilityWork capturedWork = worker.Work;
        button.onClick.AddListener(() =>
        {
            WorkPriorityLevel next = capturedWork.WorkPriorities.GetPriority(capturedType).Next();
            capturedWork.SetWorkPriority(capturedType, next);
            Refresh();
        });

        TMP_Text label = AddCellText(cell.transform, GetPriorityLabel(priority), TextAlignmentOptions.Center, true);
        label.enableAutoSizing = false;
        label.fontSize = 30f;
        label.fontStyle = FontStyles.Bold;
        label.color = priority == WorkPriorityLevel.Off
            ? DungeonUiTheme.TextSecondary
            : Color.white;

        RequireUiFactory().AddShadow(label.gameObject, new Color(0f, 0f, 0f, 0.9f), new Vector2(1.2f, -1.2f));

    }

    private GameObject CreateCellObject(string name, Transform parent, float width, float height)
    {
        GameObject cell = RequireUiFactory().CreateUiObject(name, parent);
        RectTransform rect = cell.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);

        RequireUiFactory().AddLayoutElement(cell, width, height);
        return cell;
    }

    private TMP_Text AddCellText(Transform parent, string text, TextAlignmentOptions alignment, bool allowAutoSize)
    {
        GameObject textObject = RequireUiFactory().CreateUiObject("Text", parent);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(3f, 2f);
        rect.offsetMax = new Vector2(-3f, -2f);

        TMP_Text label = RequireUiFactory().AddText(textObject);
        label.text = text;
        label.alignment = alignment;
        label.textWrappingMode = allowAutoSize ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.enableAutoSizing = allowAutoSize;
        label.fontSizeMin = 14f;
        label.fontSizeMax = 32f;
        label.raycastTarget = false;
        return label;
    }

    private int CalculateWorkerHash()
    {
        return RequireModelBuilder().CalculateWorkerHash();
    }

    private static string GetWorkTypeLabel(FacilityWorkType workType)
    {
        return workType switch
        {
            FacilityWorkType.Operate => "운영",
            FacilityWorkType.Restock => "보충",
            FacilityWorkType.Repair => "수리",
            FacilityWorkType.Clean => "청소",
            FacilityWorkType.Research => "연구",
            FacilityWorkType.Guard => "경비",
            FacilityWorkType.Rescue => "구조",
            FacilityWorkType.Rest => "휴식",
            _ => workType.ToString()
        };
    }

    private static string GetPriorityLabel(WorkPriorityLevel priority)
    {
        return priority switch
        {
            WorkPriorityLevel.Priority1 => "1",
            WorkPriorityLevel.Priority2 => "2",
            WorkPriorityLevel.Priority3 => "3",
            _ => "X"
        };
    }

    private static Color GetPriorityColor(WorkPriorityLevel priority, bool selected)
    {
        Color baseColor = priority switch
        {
            WorkPriorityLevel.Priority1 => DungeonUiTheme.Good,
            WorkPriorityLevel.Priority2 => DungeonUiTheme.Warning,
            WorkPriorityLevel.Priority3 => DungeonUiTheme.SurfaceRaised,
            _ => DungeonUiTheme.SurfaceMuted
        };

        return selected
            ? Color.Lerp(baseColor, DungeonUiTheme.TextPrimary, 0.2f)
            : baseColor;
    }

    private static string GetWorkerStatus(StaffWorkPriorityRowModel worker)
    {
        string status;
        if (worker.Character.Lifecycle != null
            && worker.Character.Lifecycle.CurrentState == CharacterLifecycleState.OnExpedition)
        {
            status = "원정";
        }
        else if (worker.Work.IsOffDuty)
        {
            status = "비번";
        }
        else
        {
            status = worker.Work.isWorking ? "작업중" : "대기";
        }

        string aiSummary = worker.Character.Brain != null
            ? worker.Character.Brain.GetDebugSummary(2)
            : string.Empty;
        if (string.IsNullOrWhiteSpace(aiSummary))
        {
            return status;
        }

        return $"{status}\n{aiSummary}";
    }

    private void ClearSpawnedObjects()
    {
        if (tableRoot != null)
        {
            for (int i = tableRoot.childCount - 1; i >= 0; i--)
            {
                RequireUiFactory().Release(tableRoot.GetChild(i).gameObject);
            }

            spawnedObjects.Clear();
            return;
        }

        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                RequireUiFactory().Release(obj);
            }
        }

        spawnedObjects.Clear();
    }

    private void OnEnable()
    {
        this.EventStartListening<InfoFeedEvent>();
        nextAutoRefreshAt = 0f;
    }

    private void OnDisable()
    {
        this.EventStopListening<InfoFeedEvent>();
    }

    private IStaffWorkPriorityPanelModelBuilder RequireModelBuilder()
    {
        EnsureFallbackServices();
        return modelBuilder;
    }

    private IStaffWorkPriorityPanelUiFactory RequireUiFactory()
    {
        EnsureFallbackServices();
        return uiFactory;
    }

    private void EnsureFallbackServices()
    {
        sceneQuery ??= FallbackSceneQuery;
        modelBuilder ??= new StaffWorkPriorityPanelModelBuilder(
            new StaffWorkforceRuntimeQueryService(sceneQuery));
        uiFactory ??= FallbackUiFactory;
    }

    private sealed class StaffWorkPriorityPanelNoopFontService : ITmpKoreanFontService
    {
        public TMP_FontAsset Resolve() => null;
        public void Apply(TMP_Text text) { }
        public void ApplyToChildren(Transform root, bool includeInactive = true) { }
    }
}
