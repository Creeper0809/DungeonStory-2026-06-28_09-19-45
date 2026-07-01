using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaffWorkPriorityPanel : MonoBehaviour, UtilEventListener<InfoFeedEvent>
{
    private const float CharacterColumnWidth = 180f;
    private const float WorkColumnWidth = 98f;
    private const float StatusColumnWidth = 270f;
    private const float RowHeight = 78f;
    private const float HeaderHeight = 56f;
    private const float PanelPadding = 16f;

    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform rowRoot;
    [SerializeField] private Button rowButtonPrefab;
    [SerializeField] private TMP_Text selectedCharacterText;
    [SerializeField] private bool hideWhenSelectedCharacterCannotWork;

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private Character selectedCharacter;
    private RectTransform contentRoot;
    private RectTransform tableRoot;
    private TMP_Text titleText;
    private int lastWorkerHash;
    private float nextAutoRefreshAt;
    private static Font cachedCellFont;

    public int VisibleWorkerCount { get; private set; }
    public int VisibleCellCount { get; private set; }

    private void Awake()
    {
        panelRoot ??= gameObject;
    }

    private void Start()
    {
        TMPKoreanFont.ApplyToChildren(transform);
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
        if (eventType.infoable is Character character
            && character.TryGetAbility(out AbilityWork _))
        {
            selectedCharacter = character;
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

        GameObject titleObject = CreateUIObject("Title", host);
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(0f, 44f);

        titleText = titleObject.AddComponent<TextMeshProUGUI>();
        TMPKoreanFont.Apply(titleText);
        titleText.text = "직원 작업 우선순위";
        titleText.color = new Color(0.95f, 0.96f, 0.92f, 1f);
        titleText.fontSize = 28f;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Left;

        GameObject scrollObject = CreateUIObject("PriorityScrollView", host);
        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = Vector2.zero;
        scrollRectTransform.anchorMax = Vector2.one;
        scrollRectTransform.offsetMin = Vector2.zero;
        scrollRectTransform.offsetMax = new Vector2(0f, -54f);

        Image scrollBackground = scrollObject.AddComponent<Image>();
        scrollBackground.color = new Color(0.045f, 0.05f, 0.055f, 0.78f);

        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = true;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 18f;

        GameObject viewportObject = CreateUIObject("Viewport", scrollRectTransform);
        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(PanelPadding, PanelPadding);
        viewportRect.offsetMax = new Vector2(-PanelPadding, -PanelPadding);

        Image viewportImage = viewportObject.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
        Mask mask = viewportObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject contentObject = CreateUIObject("Content", viewportRect);
        contentRoot = contentObject.GetComponent<RectTransform>();
        contentRoot.anchorMin = new Vector2(0f, 1f);
        contentRoot.anchorMax = new Vector2(0f, 1f);
        contentRoot.pivot = new Vector2(0f, 1f);
        contentRoot.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup vertical = contentObject.AddComponent<VerticalLayoutGroup>();
        vertical.spacing = 1f;
        vertical.childControlWidth = false;
        vertical.childControlHeight = false;
        vertical.childForceExpandWidth = false;
        vertical.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

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

        return gameObject.AddComponent<RectTransform>();
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

            DestroyUiObject(child.gameObject);
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
        List<WorkerRow> workers = FindWorkers();
        VisibleWorkerCount = workers.Count;
        VisibleCellCount = workers.Count * workTypes.Length;
        lastWorkerHash = CalculateWorkerHash(workers);

        if (titleText != null)
        {
            string selectedName = selectedCharacter != null
                ? GetCharacterDisplayName(selectedCharacter)
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
        foreach (WorkerRow worker in workers)
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
            label.color = new Color(0.96f, 0.95f, 0.86f, 1f);
        }

        CreateLabelCell(row.transform, "상태", StatusColumnWidth, HeaderHeight, TextAlignmentOptions.Center, true);
    }

    private void BuildWorkerRow(WorkerRow worker, IReadOnlyList<FacilityWorkType> workTypes, float tableWidth)
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
            ? new Color(1f, 0.92f, 0.55f, 1f)
            : new Color(0.92f, 0.93f, 0.9f, 1f);

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
        GameObject row = CreateUIObject(name, tableRoot);
        spawnedObjects.Add(row);

        RectTransform rect = row.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);

        HorizontalLayoutGroup horizontal = row.AddComponent<HorizontalLayoutGroup>();
        horizontal.spacing = 1f;
        horizontal.childControlWidth = false;
        horizontal.childControlHeight = false;
        horizontal.childForceExpandWidth = false;
        horizontal.childForceExpandHeight = false;

        LayoutElement layout = row.AddComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.preferredHeight = height;
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
        Image image = cell.AddComponent<Image>();
        image.color = header
            ? new Color(0.12f, 0.13f, 0.13f, 1f)
            : new Color(0.075f, 0.08f, 0.083f, 0.96f);

        TMP_Text label = AddCellText(cell.transform, text, alignment, header);
        label.fontSize = header ? 16f : 18f;
        label.color = new Color(0.92f, 0.93f, 0.9f, 1f);
        label.margin = alignment == TextAlignmentOptions.Left
            ? new Vector4(8f, 0f, 4f, 0f)
            : Vector4.zero;

        bool multiline = !string.IsNullOrEmpty(text) && text.Contains("\n");
        AddVisibleCellText(
            cell.transform,
            text,
            alignment,
            multiline ? 11 : header ? 16 : 18,
            label.color,
            header ? UnityEngine.FontStyle.Bold : UnityEngine.FontStyle.Normal,
            true,
            alignment == TextAlignmentOptions.Left ? 8f : 0f);
        return label;
    }

    private void CreatePriorityCell(Transform parent, WorkerRow worker, FacilityWorkType workType)
    {
        WorkPriorityLevel priority = worker.Work.WorkPriorities.GetPriority(workType);
        GameObject cell = CreateCellObject($"Cell_{worker.Character.GetInstanceID()}_{workType}", parent, WorkColumnWidth, RowHeight);
        Image image = cell.AddComponent<Image>();
        image.color = GetPriorityColor(priority, worker.Character == selectedCharacter);

        Button button = cell.AddComponent<Button>();
        button.targetGraphic = image;
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
            ? new Color(0.95f, 0.9f, 0.84f, 1f)
            : Color.white;

        Shadow shadow = label.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
        shadow.effectDistance = new Vector2(1.2f, -1.2f);

        Text visibleText = AddVisibleCellText(
            cell.transform,
            GetPriorityLabel(priority),
            TextAlignmentOptions.Center,
            30,
            label.color,
            UnityEngine.FontStyle.Bold,
            false,
            0f);
        Shadow visibleShadow = visibleText.gameObject.AddComponent<Shadow>();
        visibleShadow.effectColor = new Color(0f, 0f, 0f, 0.92f);
        visibleShadow.effectDistance = new Vector2(1.2f, -1.2f);
    }

    private static GameObject CreateCellObject(string name, Transform parent, float width, float height)
    {
        GameObject cell = CreateUIObject(name, parent);
        RectTransform rect = cell.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);

        LayoutElement layout = cell.AddComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.preferredHeight = height;
        layout.minWidth = width;
        layout.minHeight = height;
        return cell;
    }

    private static TMP_Text AddCellText(Transform parent, string text, TextAlignmentOptions alignment, bool allowAutoSize)
    {
        GameObject textObject = CreateUIObject("Text", parent);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(3f, 2f);
        rect.offsetMax = new Vector2(-3f, -2f);

        TMP_Text label = textObject.AddComponent<TextMeshProUGUI>();
        TMPKoreanFont.Apply(label);
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

    private static Text AddVisibleCellText(
        Transform parent,
        string text,
        TextAlignmentOptions alignment,
        int fontSize,
        Color color,
        UnityEngine.FontStyle fontStyle,
        bool bestFit,
        float leftPadding)
    {
        GameObject textObject = CreateUIObject("VisibleText", parent);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(6f + leftPadding, 4f);
        rect.offsetMax = new Vector2(-6f, -4f);

        Text label = textObject.AddComponent<Text>();
        label.font = ResolveCellFont();
        label.text = text;
        label.color = color;
        label.alignment = ToTextAnchor(alignment);
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.raycastTarget = false;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.resizeTextForBestFit = bestFit;
        label.resizeTextMinSize = Mathf.Min(10, fontSize);
        label.resizeTextMaxSize = fontSize;
        return label;
    }

    private static Font ResolveCellFont()
    {
        if (cachedCellFont != null)
        {
            return cachedCellFont;
        }

        cachedCellFont = Font.CreateDynamicFontFromOSFont(
            new[] { "Malgun Gothic", "맑은 고딕", "Arial", "Segoe UI" },
            18);
        if (cachedCellFont == null)
        {
            cachedCellFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        return cachedCellFont;
    }

    private static TextAnchor ToTextAnchor(TextAlignmentOptions alignment)
    {
        if ((alignment & TextAlignmentOptions.Left) != 0)
        {
            return TextAnchor.MiddleLeft;
        }

        if ((alignment & TextAlignmentOptions.Right) != 0)
        {
            return TextAnchor.MiddleRight;
        }

        return TextAnchor.MiddleCenter;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private List<WorkerRow> FindWorkers()
    {
        return Object.FindObjectsByType<Character>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .Where((character) => character != null
                && !character.IsDead
                && character.TryGetAbility(out AbilityWork _))
            .OrderByDescending((character) => character.IsOwner)
            .ThenBy((character) => GetCharacterDisplayName(character))
            .Select((character) => new WorkerRow(character, character.GetAbility<AbilityWork>()))
            .Where((worker) => worker.Work != null)
            .ToList();
    }

    private int CalculateWorkerHash()
    {
        return CalculateWorkerHash(FindWorkers());
    }

    private static int CalculateWorkerHash(IReadOnlyList<WorkerRow> workers)
    {
        unchecked
        {
            int hash = 17;
            foreach (WorkerRow worker in workers)
            {
                hash = (hash * 31) + worker.Character.GetInstanceID();
                hash = (hash * 31) + (int)worker.Work.CurrentDutyState;
                hash = (hash * 31) + (worker.Work.isWorking ? 1 : 0);
                hash = (hash * 31) + (worker.Character.IsOnExpedition ? 1 : 0);
                hash = (hash * 31) + (worker.Character.ai != null ? worker.Character.ai.GetDebugHash() : 0);

                foreach (FacilityWorkType type in WorkTaskCatalog.TaskTypes)
                {
                    hash = (hash * 31) + (int)worker.Work.WorkPriorities.GetPriority(type);
                }
            }

            return hash;
        }
    }

    private static string GetCharacterDisplayName(Character character)
    {
        if (character == null) return string.Empty;

        if (character.data != null && !string.IsNullOrWhiteSpace(character.data.characterName))
        {
            return character.data.characterName;
        }

        return character.name;
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
            WorkPriorityLevel.Priority1 => new Color(0.18f, 0.5f, 0.28f, 1f),
            WorkPriorityLevel.Priority2 => new Color(0.58f, 0.43f, 0.14f, 1f),
            WorkPriorityLevel.Priority3 => new Color(0.24f, 0.34f, 0.45f, 1f),
            _ => new Color(0.13f, 0.14f, 0.15f, 1f)
        };

        return selected
            ? Color.Lerp(baseColor, new Color(1f, 0.88f, 0.38f, 1f), 0.28f)
            : baseColor;
    }

    private static string GetWorkerStatus(WorkerRow worker)
    {
        string status;
        if (worker.Character.IsOnExpedition)
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

        string aiSummary = worker.Character.ai != null
            ? worker.Character.ai.GetDebugSummary(2)
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
                DestroyUiObject(tableRoot.GetChild(i).gameObject);
            }

            spawnedObjects.Clear();
            return;
        }

        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                DestroyUiObject(obj);
            }
        }

        spawnedObjects.Clear();
    }

    private static void DestroyUiObject(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

        obj.SetActive(false);

        if (Application.isPlaying)
        {
            Destroy(obj);
        }
        else
        {
            DestroyImmediate(obj);
        }
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

    private readonly struct WorkerRow
    {
        public WorkerRow(Character character, AbilityWork work)
        {
            Character = character;
            Work = work;
            Name = GetCharacterDisplayName(character);
        }

        public Character Character { get; }
        public AbilityWork Work { get; }
        public string Name { get; }
    }
}
