using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodexPanel : MonoBehaviour, UtilEventListener<CodexUpdatedEvent>
{
    [SerializeField] private CodexRuntime runtime;
    [SerializeField] private TMP_Text summaryText;

    public string LastRenderedText { get; private set; } = string.Empty;

    public static CodexPanel CreateDefaultPanel(CodexRuntime runtime)
    {
        GameObject canvasObject = new GameObject("CodexCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject panelObject = new GameObject("CodexPanel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);
        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.anchoredPosition = new Vector2(-24f, 0f);
        rect.sizeDelta = new Vector2(520f, 680f);
        panelObject.GetComponent<Image>().color = new Color(0.07f, 0.08f, 0.1f, 0.9f);

        GameObject textObject = new GameObject("Summary", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panelObject.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20f, 20f);
        textRect.offsetMax = new Vector2(-20f, -20f);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        TMPKoreanFont.Apply(text);
        text.fontSize = 20f;
        text.color = Color.white;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.alignment = TextAlignmentOptions.TopLeft;

        CodexPanel panel = panelObject.AddComponent<CodexPanel>();
        panel.Bind(runtime);
        panel.summaryText = text;
        panel.Refresh();
        return panel;
    }

    public void Bind(CodexRuntime nextRuntime)
    {
        runtime = nextRuntime;
        Refresh();
    }

    public void Refresh()
    {
        CodexRuntime activeRuntime = runtime != null ? runtime : CodexRuntime.Instance;
        if (activeRuntime == null)
        {
            LastRenderedText = "도감\n런타임 없음";
            ApplyText();
            return;
        }

        List<string> lines = new List<string> { "도감" };
        AppendCategory(lines, "몬스터 도감", activeRuntime.GetEntries(CodexEntryCategory.Monster), 4);
        AppendCategory(lines, "침략 도감", activeRuntime.GetEntries(CodexEntryCategory.Invasion), 4);
        AppendCategory(lines, "시설 도감", activeRuntime.GetEntries(CodexEntryCategory.Facility), 8);

        LastRenderedText = string.Join("\n", lines);
        ApplyText();
    }

    public void OnTriggerEvent(CodexUpdatedEvent eventType)
    {
        Refresh();
    }

    private static void AppendCategory(
        List<string> lines,
        string title,
        IReadOnlyList<CodexEntrySnapshot> entries,
        int maxEntries)
    {
        lines.Add(string.Empty);
        lines.Add(title);
        if (entries == null || entries.Count == 0)
        {
            lines.Add("- 없음");
            return;
        }

        foreach (CodexEntrySnapshot entry in entries.Take(maxEntries))
        {
            lines.Add($"[{entry.title}]");
            if (entry.lines == null || entry.lines.Length == 0)
            {
                lines.Add("- 정보 없음");
                continue;
            }

            foreach (CodexInfoLine line in GetSummaryLines(entry.lines, 4))
            {
                lines.Add($"- {line.Text}");
            }
        }

        if (entries.Count > maxEntries)
        {
            lines.Add($"... {entries.Count - maxEntries}개 더 있음");
        }
    }

    private static IEnumerable<CodexInfoLine> GetSummaryLines(CodexInfoLine[] lines, int maxLines)
    {
        return lines
            .OrderBy((line) => line.Source == CodexInfoSource.System ? 1 : 0)
            .Take(maxLines);
    }

    private void ApplyText()
    {
        if (summaryText != null)
        {
            summaryText.text = LastRenderedText;
        }
    }

    private void OnEnable()
    {
        this.EventStartListening<CodexUpdatedEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<CodexUpdatedEvent>();
    }
}
