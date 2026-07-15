using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

[DisallowMultipleComponent]
public class CharacterLog : SerializedMonoBehaviour
{
    [SerializeField]
    [ReadOnly]
    private List<string> log;

    private Dictionary<string, int> logTagCounts;
    [NonSerialized] private List<int> logEntryIds;
    [NonSerialized] private HashSet<int> pendingDisplayEntryIds;
    [NonSerialized] private List<string> visibleEntries;
    [NonSerialized] private bool visibleEntriesDirty = true;
    [NonSerialized] private int nextEntryId;
    private string lastLogTag;
    private ICharacterLogNarrativeService narrativeService;

    public IReadOnlyList<string> Entries
    {
        get
        {
            EnsureLog();
            RebuildVisibleEntriesIfNeeded();
            return visibleEntries;
        }
    }
    public event Action<CharacterLogEntry> OnLogAdded;
    public event Action OnLogDisplayChanged;

    [Inject]
    public void ConstructCharacterLog(ICharacterLogNarrativeService narrativeService)
    {
        this.narrativeService = narrativeService
            ?? throw new ArgumentNullException(nameof(narrativeService));
    }

    private void Awake()
    {
        EnsureLog();
    }

    public void Bind()
    {
        EnsureLog();
    }

    public void AddLog(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        EnsureLog();
        string tag = CharacterLogUtility.ToCauseTag(message);
        int count = lastLogTag == tag && logTagCounts.TryGetValue(tag, out int currentCount)
            ? currentCount + 1
            : 1;
        logTagCounts[tag] = count;

        string line = count > 1 ? $"{tag} x{count}" : tag;
        int entryId;
        if (lastLogTag == tag && log.Count > 0)
        {
            log[log.Count - 1] = line;
            entryId = logEntryIds[logEntryIds.Count - 1];
        }
        else
        {
            log.Add(line);
            entryId = ++nextEntryId;
            logEntryIds.Add(entryId);
            lastLogTag = tag;
        }

        visibleEntriesDirty = true;

        const int maxLogCount = 80;
        if (log.Count > maxLogCount)
        {
            int removeCount = log.Count - maxLogCount;
            log.RemoveRange(0, removeCount);
            logEntryIds.RemoveRange(0, removeCount);
        }

        CharacterLogEntry entry = new CharacterLogEntry(entryId, tag, line, count, message);
        bool wasPending = pendingDisplayEntryIds.Contains(entryId);
        bool deferDisplay = narrativeService != null
            && narrativeService.ShouldDeferDisplay(entry);
        SetDisplayPending(entryId, deferDisplay || wasPending);
        OnLogAdded?.Invoke(entry);

        if (deferDisplay)
        {
            if (!narrativeService.RequestNarrative(this, entry))
            {
                narrativeService.TryApplyFallback(this, entry);
            }
        }
        else if (wasPending)
        {
            narrativeService?.TryApplyFallback(this, entry);
        }
    }

    public bool TryUpdateDisplayLine(int entryId, string expectedLine, string displayLine)
    {
        if (entryId <= 0 || string.IsNullOrWhiteSpace(displayLine))
        {
            return false;
        }

        EnsureLog();
        int index = logEntryIds.IndexOf(entryId);
        if (index < 0
            || index >= log.Count
            || !string.Equals(log[index], expectedLine, StringComparison.Ordinal))
        {
            return false;
        }

        string normalized = displayLine.Trim();
        if (string.Equals(log[index], normalized, StringComparison.Ordinal))
        {
            return false;
        }

        log[index] = normalized;
        SetDisplayPending(entryId, false);
        visibleEntriesDirty = true;
        OnLogDisplayChanged?.Invoke();
        return true;
    }

    public bool TryGetVisibleDisplayLine(int entryId, out string displayLine)
    {
        displayLine = string.Empty;
        if (entryId <= 0)
        {
            return false;
        }

        EnsureLog();
        int index = logEntryIds.IndexOf(entryId);
        if (index < 0
            || index >= log.Count
            || pendingDisplayEntryIds.Contains(entryId))
        {
            return false;
        }

        displayLine = log[index];
        return !string.IsNullOrWhiteSpace(displayLine);
    }

    private void EnsureLog()
    {
        log ??= new List<string>();
        logTagCounts ??= new Dictionary<string, int>();
        logEntryIds ??= new List<int>();
        pendingDisplayEntryIds ??= new HashSet<int>();
        visibleEntries ??= new List<string>();

        if (logEntryIds.Count > log.Count)
        {
            logEntryIds.RemoveRange(log.Count, logEntryIds.Count - log.Count);
        }

        while (logEntryIds.Count < log.Count)
        {
            logEntryIds.Add(++nextEntryId);
        }

        for (int i = 0; i < logEntryIds.Count; i++)
        {
            nextEntryId = Math.Max(nextEntryId, logEntryIds[i]);
        }

        pendingDisplayEntryIds.RemoveWhere(entryId => !logEntryIds.Contains(entryId));
    }

    private void SetDisplayPending(int entryId, bool pending)
    {
        bool changed = pending
            ? pendingDisplayEntryIds.Add(entryId)
            : pendingDisplayEntryIds.Remove(entryId);
        if (changed)
        {
            visibleEntriesDirty = true;
        }
    }

    private void RebuildVisibleEntriesIfNeeded()
    {
        if (!visibleEntriesDirty)
        {
            return;
        }

        visibleEntries.Clear();
        for (int i = 0; i < log.Count; i++)
        {
            if (!pendingDisplayEntryIds.Contains(logEntryIds[i]))
            {
                visibleEntries.Add(log[i]);
            }
        }

        visibleEntriesDirty = false;
    }
}

public readonly struct CharacterLogEntry
{
    public int EntryId { get; }
    public string Tag { get; }
    public string DisplayLine { get; }
    public int Count { get; }
    public string OriginalMessage { get; }

    public CharacterLogEntry(string tag, string displayLine, int count, string originalMessage)
        : this(-1, tag, displayLine, count, originalMessage)
    {
    }

    public CharacterLogEntry(int entryId, string tag, string displayLine, int count, string originalMessage)
    {
        EntryId = entryId;
        Tag = tag;
        DisplayLine = displayLine;
        Count = count;
        OriginalMessage = originalMessage;
    }
}

public static class CharacterLogUtility
{
    public static string ToCauseTag(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "기록 없음";
        }

        string normalized = message.Trim();
        if (ContainsAny(normalized, "작업 시작", "작업 중", "작업 진행", "작업 종료"))
        {
            return normalized;
        }

        if (ContainsAny(normalized, "재고 없음", "재고 부족", "창고 재고 부족", "보충 실패"))
        {
            return "재고 부족";
        }

        if (ContainsAny(normalized, "길 막힘", "목적지 없음", "도달 실패", "이동 정보 없음", "경로"))
        {
            return "길 막힘";
        }

        if (ContainsAny(normalized, "수용 인원", "혼잡"))
        {
            return "혼잡함";
        }

        if (ContainsAny(normalized, "돈 부족", "자금 부족", "가격"))
        {
            return "돈 부족";
        }

        if (ContainsAny(normalized, "시설 파손", "파손"))
        {
            return "시설 파손";
        }

        if (ContainsAny(normalized, "피로", "비번", "휴식"))
        {
            return "피로";
        }

        if (ContainsAny(normalized, "분노", "사고", "사망"))
        {
            return "위험";
        }

        if (ContainsAny(normalized, "완료", "회복", "근무 복귀"))
        {
            return "만족";
        }

        string extracted = ExtractReasonAfterSeparator(normalized);
        return string.IsNullOrWhiteSpace(extracted) ? normalized : extracted;
    }

    private static bool ContainsAny(string value, params string[] patterns)
    {
        return patterns.Any((pattern) => value.Contains(pattern, StringComparison.Ordinal));
    }

    private static string ExtractReasonAfterSeparator(string value)
    {
        int colonIndex = value.LastIndexOf(':');
        if (colonIndex >= 0 && colonIndex + 1 < value.Length)
        {
            return value[(colonIndex + 1)..].Trim();
        }

        int dashIndex = value.LastIndexOf(" - ", StringComparison.Ordinal);
        if (dashIndex >= 0 && dashIndex + 3 < value.Length)
        {
            return value[(dashIndex + 3)..].Trim();
        }

        return value;
    }
}
