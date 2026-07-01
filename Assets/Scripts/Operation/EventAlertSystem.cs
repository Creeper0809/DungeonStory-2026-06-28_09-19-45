using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum EventAlertImportance
{
    Low,
    Medium,
    High
}

public class EventAlertChoice
{
    public string Label { get; }
    public string Description { get; }
    public Action Callback { get; }

    public EventAlertChoice(string label, string description = "", Action callback = null)
    {
        Label = string.IsNullOrWhiteSpace(label) ? "Choice" : label;
        Description = description ?? string.Empty;
        Callback = callback;
    }
}

public class EventAlertRequest
{
    public string Title { get; }
    public string Detail { get; }
    public EventAlertImportance Importance { get; }
    public string Category { get; }
    public IReadOnlyList<EventAlertChoice> Choices { get; }

    public EventAlertRequest(
        string title,
        string detail,
        EventAlertImportance importance,
        string category = "",
        IEnumerable<EventAlertChoice> choices = null)
    {
        Title = string.IsNullOrWhiteSpace(title) ? "Event" : title;
        Detail = detail ?? string.Empty;
        Importance = importance;
        Category = category ?? string.Empty;
        Choices = NormalizeChoices(choices);
    }

    private static IReadOnlyList<EventAlertChoice> NormalizeChoices(IEnumerable<EventAlertChoice> choices)
    {
        return choices?
            .Where((choice) => choice != null)
            .Take(3)
            .ToList()
            ?? new List<EventAlertChoice>();
    }
}

public class EventAlertRecord
{
    public int Id { get; }
    public string Title { get; }
    public string Detail { get; }
    public EventAlertImportance Importance { get; }
    public string Category { get; }
    public int Count { get; private set; }
    public IReadOnlyList<EventAlertChoice> Choices { get; }

    public EventAlertRecord(int id, EventAlertRequest request)
    {
        Id = id;
        Title = request.Title;
        Detail = request.Detail;
        Importance = request.Importance;
        Category = request.Category;
        Choices = request.Choices;
        Count = 1;
    }

    public void Increment()
    {
        Count++;
    }

    public string ButtonText => Count > 1 ? $"{Title} x{Count}" : Title;

    public string ToDetailText()
    {
        List<string> lines = new List<string>
        {
            Title,
            $"중요도: {GetImportanceName(Importance)}",
        };

        if (!string.IsNullOrWhiteSpace(Category))
        {
            lines.Add($"분류: {Category}");
        }

        if (Count > 1)
        {
            lines.Add($"반복: {Count}");
        }

        if (!string.IsNullOrWhiteSpace(Detail))
        {
            lines.Add(string.Empty);
            lines.Add(Detail);
        }

        if (Choices.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("선택지:");
            foreach (EventAlertChoice choice in Choices)
            {
                lines.Add(string.IsNullOrWhiteSpace(choice.Description)
                    ? $"- {choice.Label}"
                    : $"- {choice.Label}: {choice.Description}");
            }
        }

        return string.Join("\n", lines);
    }

    private static string GetImportanceName(EventAlertImportance importance)
    {
        return importance switch
        {
            EventAlertImportance.Low => "낮음",
            EventAlertImportance.Medium => "중간",
            EventAlertImportance.High => "높음",
            _ => importance.ToString()
        };
    }
}

public struct EventAlertRequestedEvent
{
    public EventAlertRequest request;

    public EventAlertRequestedEvent(EventAlertRequest request)
    {
        this.request = request;
    }

    private static EventAlertRequestedEvent e;

    public static void Trigger(EventAlertRequest request)
    {
        e.request = request;
        EventObserver.TriggerEvent(e);
    }
}

public struct EventAlertLoggedEvent
{
    public EventAlertRecord record;

    public EventAlertLoggedEvent(EventAlertRecord record)
    {
        this.record = record;
    }

    private static EventAlertLoggedEvent e;

    public static void Trigger(EventAlertRecord record)
    {
        e.record = record;
        EventObserver.TriggerEvent(e);
    }
}

public static class EventAlertService
{
    public static void Raise(
        string title,
        string detail,
        EventAlertImportance importance,
        string category = "",
        IEnumerable<EventAlertChoice> choices = null)
    {
        EventAlertRequestedEvent.Trigger(new EventAlertRequest(title, detail, importance, category, choices));
    }

    public static void RaiseInvasionResult(string detail, EventAlertImportance importance = EventAlertImportance.High)
    {
        Raise("침입 결과", detail, importance, "침입");
    }

    public static void RaiseStaffComplaint(string detail, EventAlertImportance importance = EventAlertImportance.Medium)
    {
        Raise("직원 불만", detail, importance, "직원");
    }

    public static void RaiseBlueprintAcquired(string detail, EventAlertImportance importance = EventAlertImportance.Medium)
    {
        Raise("설계도 획득", detail, importance, "설계도");
    }
}

public class EventAlertRuntime : MonoBehaviour, UtilEventListener<EventAlertRequestedEvent>
{
    private const float AlertListWidth = 154f;
    private const float AlertListMaxHeight = 500f;
    private const int AlertListMinVisibleRows = 4;
    private const float AlertListRightOffset = 16f;
    private const float AlertListBottomOffset = 78f;
    private const float AlertListTopReserved = 116f;
    private const float AlertListVerticalPadding = 6f;
    private const float AlertButtonWidth = 146f;
    private const float AlertButtonHeight = 38f;
    private const float AlertButtonSpacing = 5f;

    [SerializeField] private Transform buttonRoot;
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private TMP_Text detailText;

    private readonly List<EventAlertRecord> eventLog = new List<EventAlertRecord>();
    private readonly Dictionary<int, Button> buttonsById = new Dictionary<int, Button>();
    private readonly List<Button> choiceButtons = new List<Button>();
    private GameObject runtimeRoot;
    private RectTransform buttonViewportRect;
    private RectTransform buttonContentRect;
    private ScrollRect buttonScrollRect;
    private int nextId = 1;
    private EventAlertRecord selectedRecord;

    public IReadOnlyList<EventAlertRecord> EventLog => eventLog;
    public bool IsDetailVisible => detailPanel != null && detailPanel.activeSelf;
    public EventAlertRecord SelectedRecord => selectedRecord;

    private void Awake()
    {
        EnsureRuntimeUI();
    }

    public void OnTriggerEvent(EventAlertRequestedEvent eventType)
    {
        if (eventType.request == null)
        {
            return;
        }

        EventAlertRecord record = FindMergeTarget(eventType.request);
        if (record == null)
        {
            record = new EventAlertRecord(nextId++, eventType.request);
            eventLog.Add(record);
            CreateButton(record);
        }
        else
        {
            record.Increment();
            UpdateButton(record);
        }

        EventAlertLoggedEvent.Trigger(record);
    }

    public void Open(EventAlertRecord record)
    {
        EnsureRuntimeUI();
        if (record == null || detailPanel == null || detailText == null)
        {
            return;
        }

        selectedRecord = record;
        detailPanel.SetActive(true);
        detailText.text = record.ToDetailText();
        RebuildChoices(record);
    }

    public void CloseDetail()
    {
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
    }

    public bool ExecuteChoice(int index)
    {
        if (selectedRecord == null || index < 0 || index >= selectedRecord.Choices.Count)
        {
            return false;
        }

        selectedRecord.Choices[index].Callback?.Invoke();
        CloseDetail();
        return true;
    }

    private void OnEnable()
    {
        EnsureRuntimeUI();
        this.EventStartListening<EventAlertRequestedEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<EventAlertRequestedEvent>();
    }

    private void OnDestroy()
    {
        if (runtimeRoot == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(runtimeRoot);
        }
        else
        {
            DestroyImmediate(runtimeRoot);
        }
    }

    private EventAlertRecord FindMergeTarget(EventAlertRequest request)
    {
        if (request.Choices.Count > 0)
        {
            return null;
        }

        return eventLog.LastOrDefault((record) =>
            record.Title == request.Title
            && record.Category == request.Category
            && record.Importance == request.Importance
            && record.Detail == request.Detail);
    }

    private void CreateButton(EventAlertRecord record)
    {
        EnsureRuntimeUI();
        if (buttonRoot == null) return;

        GameObject buttonObject = new GameObject($"EventAlertButton_{record.Id}", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(buttonRoot, false);
        buttonObject.transform.SetAsFirstSibling();

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(AlertButtonWidth, AlertButtonHeight);

        Image image = buttonObject.GetComponent<Image>();
        image.color = GetImportanceColor(record.Importance);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => Open(record));

        TMP_Text label = CreateText(buttonObject.transform, "Label", record.ButtonText, 15, TextAlignmentOptions.Center);
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8f, 0f);
        labelRect.offsetMax = new Vector2(-8f, 0f);

        buttonsById[record.Id] = button;
        LayoutButtons();
    }

    private void UpdateButton(EventAlertRecord record)
    {
        if (!buttonsById.TryGetValue(record.Id, out Button button) || button == null)
        {
            return;
        }

        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = record.ButtonText;
        }
    }

    private void LayoutButtons()
    {
        if (buttonRoot == null) return;

        List<RectTransform> rows = new List<RectTransform>();
        foreach (Transform child in buttonRoot)
        {
            if (child is RectTransform rect)
            {
                rows.Add(rect);
            }
        }

        float contentHeight = Mathf.Max(
            GetButtonViewportHeight(),
            GetContentHeightForRows(rows.Count));
        if (buttonContentRect == null)
        {
            buttonContentRect = buttonRoot as RectTransform;
        }
        if (buttonContentRect != null)
        {
            buttonContentRect.sizeDelta = new Vector2(AlertButtonWidth, contentHeight);
        }

        for (int i = 0; i < rows.Count; i++)
        {
            rows[i].anchorMin = new Vector2(0f, 0f);
            rows[i].anchorMax = new Vector2(0f, 0f);
            rows[i].pivot = new Vector2(0f, 0f);
            rows[i].sizeDelta = new Vector2(AlertButtonWidth, AlertButtonHeight);
            rows[i].anchoredPosition = new Vector2(0f, AlertListVerticalPadding + (i * (AlertButtonHeight + AlertButtonSpacing)));
        }

        if (buttonScrollRect != null)
        {
            buttonScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void RebuildChoices(EventAlertRecord record)
    {
        foreach (Button button in choiceButtons)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }

        choiceButtons.Clear();
        if (record.Choices.Count == 0 || detailPanel == null)
        {
            return;
        }

        for (int i = 0; i < record.Choices.Count; i++)
        {
            EventAlertChoice choice = record.Choices[i];
            GameObject buttonObject = new GameObject($"EventChoice_{i + 1}", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(detailPanel.transform, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(18f + (i * 138f), 18f);
            rect.sizeDelta = new Vector2(128f, 34f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.18f, 0.2f, 0.24f, 0.96f);

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            int choiceIndex = i;
            button.onClick.AddListener(() => ExecuteChoice(choiceIndex));

            TMP_Text label = CreateText(buttonObject.transform, "Label", choice.Label, 14, TextAlignmentOptions.Center);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            choiceButtons.Add(button);
        }
    }

    private void EnsureRuntimeUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("RuntimeUICanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        if (runtimeRoot == null)
        {
            runtimeRoot = new GameObject("EventAlertRuntimeUI", typeof(RectTransform));
            runtimeRoot.transform.SetParent(canvas.transform, false);
            RectTransform rootRect = runtimeRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
        }
        runtimeRoot.transform.SetAsFirstSibling();

        BindExistingButtonRootReferences();
        if (!IsButtonRootReady())
        {
            CreateButtonRoot(canvas, buttonRoot);
        }
        else
        {
            ConfigureButtonViewport(canvas);
        }

        if (detailPanel == null)
        {
            detailPanel = CreateDetailPanel(runtimeRoot.transform);
            detailPanel.SetActive(false);
        }
    }

    private void CreateButtonRoot(Canvas canvas, Transform previousButtonRoot)
    {
        List<Transform> existingButtons = new List<Transform>();
        if (previousButtonRoot != null)
        {
            foreach (Transform child in previousButtonRoot)
            {
                existingButtons.Add(child);
            }
        }

        GameObject rootObject = new GameObject("EventAlertButtonRoot", typeof(RectTransform), typeof(RectMask2D), typeof(ScrollRect));
        rootObject.transform.SetParent(runtimeRoot.transform, false);
        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(1f, 0f);
        rootRect.anchorMax = new Vector2(1f, 0f);
        rootRect.pivot = new Vector2(1f, 0f);

        GameObject contentObject = new GameObject("Content", typeof(RectTransform));
        contentObject.transform.SetParent(rootObject.transform, false);
        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 0f);
        contentRect.anchorMax = new Vector2(0f, 0f);
        contentRect.pivot = new Vector2(0f, 0f);
        contentRect.anchoredPosition = Vector2.zero;

        ScrollRect scrollRect = rootObject.GetComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = false;
        scrollRect.scrollSensitivity = AlertButtonHeight + AlertButtonSpacing;
        scrollRect.viewport = rootRect;
        scrollRect.content = contentRect;

        buttonViewportRect = rootRect;
        buttonContentRect = contentRect;
        buttonScrollRect = scrollRect;
        buttonRoot = contentObject.transform;

        foreach (Transform existingButton in existingButtons)
        {
            existingButton.SetParent(buttonRoot, false);
        }

        ConfigureButtonViewport(canvas);
        LayoutButtons();
    }

    private void BindExistingButtonRootReferences()
    {
        if (buttonRoot == null || buttonRoot.name != "Content") return;

        buttonContentRect = buttonRoot as RectTransform;
        buttonViewportRect = buttonRoot.parent as RectTransform;
        buttonScrollRect = buttonViewportRect != null
            ? buttonViewportRect.GetComponent<ScrollRect>()
            : null;
    }

    private bool IsButtonRootReady()
    {
        return buttonRoot != null
            && buttonRoot.name == "Content"
            && buttonContentRect != null
            && buttonViewportRect != null
            && buttonScrollRect != null;
    }

    private void ConfigureButtonViewport(Canvas canvas)
    {
        if (buttonViewportRect == null) return;

        buttonViewportRect.anchorMin = new Vector2(1f, 0f);
        buttonViewportRect.anchorMax = new Vector2(1f, 0f);
        buttonViewportRect.pivot = new Vector2(1f, 0f);
        buttonViewportRect.anchoredPosition = new Vector2(-AlertListRightOffset, AlertListBottomOffset);
        buttonViewportRect.sizeDelta = new Vector2(AlertListWidth, GetAlertListHeight(canvas));

        if (buttonContentRect != null)
        {
            buttonContentRect.anchorMin = new Vector2(0f, 0f);
            buttonContentRect.anchorMax = new Vector2(0f, 0f);
            buttonContentRect.pivot = new Vector2(0f, 0f);
            buttonContentRect.anchoredPosition = Vector2.zero;
            buttonContentRect.sizeDelta = new Vector2(AlertButtonWidth, buttonViewportRect.sizeDelta.y);
        }
    }

    private static float GetAlertListHeight(Canvas canvas)
    {
        RectTransform canvasRect = canvas != null ? canvas.transform as RectTransform : null;
        if (canvasRect == null || canvasRect.rect.height <= 0f)
        {
            return GetContentHeightForRows(GetMaxVisibleRows());
        }

        float availableHeight = canvasRect.rect.height - AlertListTopReserved - AlertListBottomOffset;
        int visibleRows = GetVisibleRowsForHeight(availableHeight);
        return GetContentHeightForRows(visibleRows);
    }

    private float GetButtonViewportHeight()
    {
        if (buttonViewportRect != null && buttonViewportRect.rect.height > 0f)
        {
            return buttonViewportRect.rect.height;
        }

        return GetContentHeightForRows(GetMaxVisibleRows());
    }

    private static int GetVisibleRowsForHeight(float availableHeight)
    {
        int maxRows = GetMaxVisibleRows();
        if (availableHeight <= AlertListVerticalPadding * 2f + AlertButtonHeight)
        {
            return 1;
        }

        int rowsBySpace = Mathf.FloorToInt(
            (availableHeight - (AlertListVerticalPadding * 2f) + AlertButtonSpacing)
            / (AlertButtonHeight + AlertButtonSpacing));
        int minRows = Mathf.Min(AlertListMinVisibleRows, maxRows);
        return Mathf.Clamp(rowsBySpace, minRows, maxRows);
    }

    private static int GetMaxVisibleRows()
    {
        return Mathf.Max(1, Mathf.FloorToInt(
            (AlertListMaxHeight - (AlertListVerticalPadding * 2f) + AlertButtonSpacing)
            / (AlertButtonHeight + AlertButtonSpacing)));
    }

    private static float GetContentHeightForRows(int rowCount)
    {
        int rows = Mathf.Max(1, rowCount);
        return (AlertListVerticalPadding * 2f)
            + (rows * AlertButtonHeight)
            + ((rows - 1) * AlertButtonSpacing);
    }

    private GameObject CreateDetailPanel(Transform parent)
    {
        GameObject panelObject = new GameObject("EventAlertDetailPanel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(parent, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 0.5f);
        panelRect.anchorMax = new Vector2(1f, 0.5f);
        panelRect.pivot = new Vector2(1f, 0.5f);
        panelRect.anchoredPosition = new Vector2(-24f, 0f);
        panelRect.sizeDelta = new Vector2(480f, 560f);

        Image image = panelObject.GetComponent<Image>();
        image.color = new Color(0.05f, 0.05f, 0.06f, 0.95f);

        GameObject closeObject = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        closeObject.transform.SetParent(panelObject.transform, false);
        RectTransform closeRect = closeObject.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-14f, -14f);
        closeRect.sizeDelta = new Vector2(34f, 30f);
        closeObject.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.18f, 1f);
        Button closeButton = closeObject.GetComponent<Button>();
        closeButton.onClick.AddListener(CloseDetail);
        TMP_Text closeLabel = CreateText(closeObject.transform, "Label", "X", 16, TextAlignmentOptions.Center);
        RectTransform closeLabelRect = closeLabel.GetComponent<RectTransform>();
        closeLabelRect.anchorMin = Vector2.zero;
        closeLabelRect.anchorMax = Vector2.one;
        closeLabelRect.offsetMin = Vector2.zero;
        closeLabelRect.offsetMax = Vector2.zero;

        detailText = CreateText(panelObject.transform, "DetailText", string.Empty, 16, TextAlignmentOptions.TopLeft);
        RectTransform textRect = detailText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(18f, 64f);
        textRect.offsetMax = new Vector2(-18f, -54f);
        detailText.textWrappingMode = TextWrappingModes.Normal;
        detailText.overflowMode = TextOverflowModes.Overflow;

        return panelObject;
    }

    private TMP_Text CreateText(Transform parent, string name, string text, int fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TMP_Text tmp = textObject.GetComponent<TMP_Text>();
        TMPKoreanFont.Apply(tmp);
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = alignment;
        return tmp;
    }

    private static Color GetImportanceColor(EventAlertImportance importance)
    {
        return importance switch
        {
            EventAlertImportance.Low => new Color(0.12f, 0.15f, 0.18f, 0.94f),
            EventAlertImportance.Medium => new Color(0.22f, 0.18f, 0.08f, 0.96f),
            EventAlertImportance.High => new Color(0.28f, 0.08f, 0.08f, 0.98f),
            _ => Color.black
        };
    }
}

public class OperatingDayReportAlertBridge : MonoBehaviour, UtilEventListener<OperatingDayReportEvent>
{
    public void OnTriggerEvent(OperatingDayReportEvent eventType)
    {
        OperatingDayReport report = eventType.report;
        if (report == null)
        {
            return;
        }

        EventAlertService.Raise(
            $"Day {report.day} 정산",
            report.ToDetailText(),
            EventAlertImportance.Medium,
            "정산");

        if (report.staffComplaintEvents.Count > 0)
        {
            EventAlertService.RaiseStaffComplaint(
                string.Join("\n", report.staffComplaintEvents),
                report.staffComplaintEvents.Count >= 3 ? EventAlertImportance.High : EventAlertImportance.Medium);
        }
    }

    private void OnEnable()
    {
        this.EventStartListening<OperatingDayReportEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<OperatingDayReportEvent>();
    }
}
