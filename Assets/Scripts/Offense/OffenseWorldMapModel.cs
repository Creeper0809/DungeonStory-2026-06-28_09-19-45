using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum OffenseTargetKind
{
    RivalDungeon,
    HumanOutpost,
    ResourceSite,
    SpecialEvent
}

public enum OffenseRewardCategory
{
    Money,
    Stock,
    RareFacility,
    Blueprint,
    FactionWeakening,
    RecruitCandidate,
    Prisoner
}

[Serializable]
public class OffenseRewardPreview
{
    public OffenseRewardCategory category;
    public string label;
    public int amount;

    public string ToSummaryText()
    {
        string name = string.IsNullOrWhiteSpace(label) ? category.ToString() : label;
        return amount > 0 ? $"{name} x{amount}" : name;
    }
}

[Serializable]
public class OffenseTargetDefinition
{
    public string id;
    public string title;
    [TextArea] public string description;
    public OffenseTargetKind kind;
    [Min(0f)] public float distance;
    [Min(0f)] public float danger;
    [Min(1f)] public float durationSeconds = 90f;
    [Min(1)] public int requiredMembers = 1;
    [Min(0f)] public float requiredPower;
    public OffenseRewardPreview[] rewards = Array.Empty<OffenseRewardPreview>();

    public bool IsValid => !string.IsNullOrWhiteSpace(id)
        && !string.IsNullOrWhiteSpace(title)
        && distance >= 0f
        && requiredMembers > 0;

    public OffenseTargetSnapshot ToSnapshot(bool preciseIntel)
    {
        return new OffenseTargetSnapshot
        {
            id = id,
            title = title,
            description = description,
            kind = kind,
            distance = distance,
            danger = preciseIntel ? danger : Mathf.Max(1f, Mathf.Round(danger / 5f) * 5f),
            durationSeconds = durationSeconds,
            requiredMembers = requiredMembers,
            requiredPower = preciseIntel ? requiredPower : Mathf.Max(0f, Mathf.Round(requiredPower / 5f) * 5f),
            rewards = rewards?
                .Where((reward) => reward != null)
                .Select((reward) => reward.ToSummaryText())
                .ToArray()
                ?? Array.Empty<string>()
        };
    }
}

[Serializable]
public class OffenseTargetSnapshot
{
    public string id;
    public string title;
    public string description;
    public OffenseTargetKind kind;
    public float distance;
    public float danger;
    public float durationSeconds;
    public int requiredMembers;
    public float requiredPower;
    public string[] rewards = Array.Empty<string>();

    public string ToDetailText()
    {
        List<string> lines = new List<string>
        {
            string.IsNullOrWhiteSpace(title) ? "원정 대상" : title,
            $"종류: {GetKindName(kind)}",
            $"거리: {distance:0.#}",
            $"위험도: {danger:0.#}",
            $"소요 시간: {FormatDuration(durationSeconds)}",
            $"필요 인력: {requiredMembers}",
            $"권장 전투력: {requiredPower:0.#}"
        };

        if (!string.IsNullOrWhiteSpace(description))
        {
            lines.Add(string.Empty);
            lines.Add(description);
        }

        if (rewards != null && rewards.Length > 0)
        {
            lines.Add(string.Empty);
            lines.Add("예상 보상:");
            foreach (string reward in rewards)
            {
                if (!string.IsNullOrWhiteSpace(reward))
                {
                    lines.Add($"- {reward}");
                }
            }
        }

        return string.Join("\n", lines);
    }

    private static string FormatDuration(float seconds)
    {
        int safeSeconds = Mathf.Max(1, Mathf.RoundToInt(seconds));
        int minutes = safeSeconds / 60;
        int remainSeconds = safeSeconds % 60;
        return minutes > 0 ? $"{minutes}분 {remainSeconds:00}초" : $"{remainSeconds}초";
    }

    private static string GetKindName(OffenseTargetKind kind)
    {
        return kind switch
        {
            OffenseTargetKind.HumanOutpost => "인간 거점",
            OffenseTargetKind.ResourceSite => "자원지",
            OffenseTargetKind.SpecialEvent => "특수 지점",
            OffenseTargetKind.RivalDungeon => "경쟁 던전",
            _ => kind.ToString()
        };
    }
}

public sealed class OffenseWorldMapState
{
    private readonly HashSet<string> knownTargetIds = new HashSet<string>();

    public int ReconLevel { get; private set; }
    public string SelectedTargetId { get; private set; }
    public IReadOnlyCollection<string> KnownTargetIds => knownTargetIds;

    public void Reset(int reconLevel = 0)
    {
        ReconLevel = Mathf.Max(0, reconLevel);
        SelectedTargetId = string.Empty;
        knownTargetIds.Clear();
    }

    public bool KnowTarget(string targetId)
    {
        return !string.IsNullOrWhiteSpace(targetId) && knownTargetIds.Contains(targetId);
    }

    public bool AddKnownTarget(string targetId)
    {
        return !string.IsNullOrWhiteSpace(targetId) && knownTargetIds.Add(targetId);
    }

    public void SetSelectedTarget(string targetId)
    {
        SelectedTargetId = targetId ?? string.Empty;
    }

    public bool TryUpgradeRecon(int maxLevel)
    {
        int safeMax = Mathf.Max(0, maxLevel);
        if (ReconLevel >= safeMax)
        {
            return false;
        }

        ReconLevel++;
        return true;
    }
}
