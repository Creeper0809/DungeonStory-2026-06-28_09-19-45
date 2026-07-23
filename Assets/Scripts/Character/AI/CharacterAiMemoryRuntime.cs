using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public sealed class CharacterAiMemoryEntry
{
    public string label = string.Empty;
    public CharacterAiIntentionType intention = CharacterAiIntentionType.None;
    public CharacterAiBranch branch = CharacterAiBranch.None;
    public int facilityId = -1;
    public int targetGridId = -1;
    public FacilityWorkType workType = FacilityWorkType.None;
    public AIActionFailureKind failureKind = AIActionFailureKind.None;
    public int gridX;
    public int gridY;
    public float movementDistance;
    public float sentiment;
    public float time;
}

[DisallowMultipleComponent]
[DrawWithUnity]
public sealed class CharacterAiMemoryRuntime : MonoBehaviour
{
    private const int MaxEntries = 24;
    private const float RecentWindowSeconds = 60f;

    [SerializeField, ReadOnly] private CharacterActor actor;
    [SerializeField, ReadOnly] private List<CharacterAiMemoryEntry> recentEntries =
        new List<CharacterAiMemoryEntry>();
    private IReadOnlyList<CharacterAiMemoryEntry> recentEntriesView;

    public IReadOnlyList<CharacterAiMemoryEntry> RecentEntries =>
        recentEntriesView ??= ReadOnlyView.List(recentEntries);

    private void Awake()
    {
        Bind(GetComponent<CharacterActor>());
    }

    public void Bind(CharacterActor owner)
    {
        actor = owner;
        recentEntries ??= new List<CharacterAiMemoryEntry>();
        recentEntriesView ??= ReadOnlyView.List(recentEntries);
        Prune();
    }

    public void RecordDecision(
        CharacterAiBranch branch,
        CharacterAiIntentionType intention,
        string label,
        float sentiment = 0f)
    {
        AddEntry(new CharacterAiMemoryEntry
        {
            branch = branch,
            intention = intention,
            label = string.IsNullOrWhiteSpace(label)
                ? CharacterAiUtilityText.GetBranchLabel(branch)
                : label,
            sentiment = Mathf.Clamp(sentiment, -1f, 1f),
            time = Time.time
        });
    }

    public void RecordFacility(
        BuildableObject building,
        CharacterAiBranch branch,
        string label,
        float sentiment)
    {
        AddEntry(new CharacterAiMemoryEntry
        {
            branch = branch,
            intention = CharacterAiUtilityText.GetIntention(branch),
            facilityId = building != null ? building.id : -1,
            targetGridId = building != null ? building.GridId : -1,
            gridX = TryGetBuildingGridPosition(building, out Vector2Int position) ? position.x : 0,
            gridY = TryGetBuildingGridPosition(building, out position) ? position.y : 0,
            label = label ?? string.Empty,
            sentiment = Mathf.Clamp(sentiment, -1f, 1f),
            time = Time.time
        });
    }

    public void RecordWork(
        FacilityWorkType workType,
        BuildableObject building,
        bool success,
        string detail = "")
    {
        AddEntry(new CharacterAiMemoryEntry
        {
            branch = CharacterAiBranch.Work,
            intention = workType == FacilityWorkType.Haul
                ? CharacterAiIntentionType.Logistics
                : workType == FacilityWorkType.Hunt
                    ? CharacterAiIntentionType.Hunt
                    : CharacterAiIntentionType.Work,
            facilityId = building != null ? building.id : -1,
            targetGridId = building != null ? building.GridId : -1,
            workType = workType,
            gridX = TryGetBuildingGridPosition(building, out Vector2Int position) ? position.x : 0,
            gridY = TryGetBuildingGridPosition(building, out position) ? position.y : 0,
            label = string.IsNullOrWhiteSpace(detail)
                ? $"{WorkTaskCatalog.GetDisplayName(workType)} {(success ? "완료" : "실패")}"
                : detail,
            sentiment = success ? 0.2f : -0.3f,
            time = Time.time
        });
    }

    public void RecordFailure(
        AIActionFailureKind failureKind,
        string label,
        Vector2Int position)
    {
        AddEntry(new CharacterAiMemoryEntry
        {
            branch = CharacterAiBranch.InterruptCheck,
            intention = CharacterAiIntentionType.None,
            failureKind = failureKind,
            gridX = position.x,
            gridY = position.y,
            label = string.IsNullOrWhiteSpace(label)
                ? "행동 실패"
                : label,
            sentiment = -0.55f,
            time = Time.time
        });
    }

    public void RecordMovement(
        Vector2Int position,
        float distance,
        bool success,
        string label = "")
    {
        AddEntry(new CharacterAiMemoryEntry
        {
            branch = CharacterAiBranch.ContinueCurrent,
            intention = CharacterAiIntentionType.None,
            gridX = position.x,
            gridY = position.y,
            movementDistance = Mathf.Max(0f, distance),
            label = string.IsNullOrWhiteSpace(label)
                ? (success ? "이동 완료" : "이동 막힘")
                : label,
            sentiment = success ? 0.02f : -0.35f,
            time = Time.time
        });
    }

    public float GetMomentumScore(CharacterAiBranch branch)
    {
        if (branch == CharacterAiBranch.None || recentEntries.Count == 0)
        {
            return 0f;
        }

        Prune();
        CharacterAiIntentionType intention = CharacterAiUtilityText.GetIntention(branch);
        CharacterAiMemoryEntry latest = recentEntries
            .OrderByDescending(entry => entry.time)
            .FirstOrDefault();
        if (latest == null)
        {
            return 0f;
        }

        float age = Time.time - latest.time;
        float recency = Mathf.Clamp01(1f - age / RecentWindowSeconds);
        if (latest.branch == branch || latest.intention == intention)
        {
            return 0.16f * recency;
        }

        return -0.05f * recency;
    }

    public float GetFacilityMemoryScore(BuildableObject building)
    {
        if (building == null)
        {
            return 0.5f;
        }

        Prune();
        List<CharacterAiMemoryEntry> matches = recentEntries
            .Where(entry => entry.facilityId == building.id)
            .ToList();
        if (matches.Count == 0)
        {
            return 0.5f;
        }

        float sentiment = matches.Average(entry => entry.sentiment);
        return Mathf.Clamp01(0.5f + sentiment * 0.5f);
    }

    public float GetRepeatedWorkFatigue(FacilityWorkType workType)
    {
        if (workType == FacilityWorkType.None)
        {
            return 0f;
        }

        Prune();
        int repeated = recentEntries
            .Where(entry => entry.workType == workType)
            .Count(entry => Time.time - entry.time <= RecentWindowSeconds * 2f);
        if (repeated <= 2)
        {
            return 0f;
        }

        return Mathf.Clamp01((repeated - 2) / 5f);
    }

    public float GetRecentTargetWorkFatigue(BuildableObject building, FacilityWorkType workType)
    {
        if (building == null || workType == FacilityWorkType.None)
        {
            return 0f;
        }

        Prune();
        int targetGridId = building.GridId;
        TryGetBuildingGridPosition(building, out Vector2Int targetPosition);
        float now = Time.time;
        float pressure = 0f;
        foreach (CharacterAiMemoryEntry entry in recentEntries)
        {
            if (entry == null || entry.workType != workType)
            {
                continue;
            }

            bool sameTarget = targetGridId >= 0
                ? entry.targetGridId == targetGridId
                : entry.gridX == targetPosition.x && entry.gridY == targetPosition.y;
            if (!sameTarget)
            {
                continue;
            }

            float age = now - entry.time;
            if (age > 36f)
            {
                continue;
            }

            pressure += Mathf.Lerp(0.16f, 0.42f, Mathf.Clamp01(1f - age / 36f));
        }

        return Mathf.Clamp01(pressure);
    }

    public float GetRecentFailurePressure()
    {
        Prune();
        float now = Time.time;
        float pressure = 0f;
        foreach (CharacterAiMemoryEntry entry in recentEntries)
        {
            if (entry == null)
            {
                continue;
            }

            float age = now - entry.time;
            if (age > RecentWindowSeconds * 2f)
            {
                continue;
            }

            bool failed = entry.failureKind != AIActionFailureKind.None
                || entry.sentiment <= -0.3f;
            if (!failed)
            {
                continue;
            }

            pressure += Mathf.Lerp(0.04f, 0.24f, Mathf.Clamp01(1f - age / (RecentWindowSeconds * 2f)));
        }

        return Mathf.Clamp01(pressure);
    }

    public float GetRecentMovementPressure()
    {
        Prune();
        float now = Time.time;
        float movement = 0f;
        foreach (CharacterAiMemoryEntry entry in recentEntries)
        {
            if (entry == null || entry.movementDistance <= 0f)
            {
                continue;
            }

            float age = now - entry.time;
            if (age > RecentWindowSeconds)
            {
                continue;
            }

            movement += entry.movementDistance * Mathf.Clamp01(1f - age / RecentWindowSeconds);
        }

        return Mathf.Clamp01(movement / 22f);
    }

    public string GetRecentMemorySummary(int maxEntries = 6)
    {
        Prune();
        if (recentEntries.Count == 0)
        {
            return "최근 AI 기억 없음";
        }

        return string.Join(
            "\n",
            recentEntries
                .OrderByDescending(entry => entry.time)
                .Take(Mathf.Max(1, maxEntries))
                .Select(entry =>
                {
                    float age = Mathf.Max(0f, Time.time - entry.time);
                    string prefix = CharacterAiUtilityText.GetIntentionLabel(entry.intention);
                    return $"{prefix}: {entry.label} ({age:0}s 전)";
                }));
    }

    private void AddEntry(CharacterAiMemoryEntry entry)
    {
        if (entry == null)
        {
            return;
        }

        recentEntries ??= new List<CharacterAiMemoryEntry>();
        recentEntries.Add(entry);
        while (recentEntries.Count > MaxEntries)
        {
            recentEntries.RemoveAt(0);
        }
    }

    private void Prune()
    {
        if (recentEntries == null || recentEntries.Count == 0)
        {
            return;
        }

        float oldest = Time.time - RecentWindowSeconds * 6f;
        recentEntries.RemoveAll(entry => entry == null || entry.time < oldest);
    }

    private static bool TryGetBuildingGridPosition(BuildableObject building, out Vector2Int position)
    {
        position = Vector2Int.zero;
        if (building == null)
        {
            return false;
        }

        if (building.buildPoses != null && building.buildPoses.Count > 0)
        {
            position = building.buildPoses[0];
            return true;
        }

        position = building.centerPos;
        return true;
    }
}
