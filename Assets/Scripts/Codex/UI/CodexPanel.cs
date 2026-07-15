using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using VContainer;

public class CodexPanel : MonoBehaviour, UtilEventListener<CodexUpdatedEvent>
{
    [SerializeField] private CodexRuntime runtime;
    [SerializeField] private TMP_Text summaryText;
    private ICodexRuntimeProvider runtimeProvider;

    public string LastRenderedText { get; private set; } = string.Empty;

    [Inject]
    public void Construct(ICodexRuntimeProvider runtimeProvider)
    {
        this.runtimeProvider = runtimeProvider
            ?? throw new System.ArgumentNullException(nameof(runtimeProvider));
    }

    public void Bind(CodexRuntime nextRuntime)
    {
        runtime = nextRuntime;
        Refresh();
    }

    internal void BindGeneratedView(TMP_Text summaryText)
    {
        this.summaryText = summaryText
            ?? throw new System.ArgumentNullException(nameof(summaryText));
        ApplyText();
    }

    public void Refresh()
    {
        CodexRuntime activeRuntime = ResolveRuntime();
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

    private CodexRuntime ResolveRuntime()
    {
        if (runtime != null) return runtime;

        return (runtimeProvider
                ?? throw new System.InvalidOperationException($"{nameof(CodexPanel)} requires {nameof(ICodexRuntimeProvider)} injection or an explicit runtime binding."))
            .Runtime;
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
