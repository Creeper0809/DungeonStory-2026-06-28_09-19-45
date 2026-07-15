using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class OffenseExpeditionMemberSnapshot
{
    public string name;
    public string speciesTag;
    public float power;
    public bool survived;
    public float damageTaken;

    public string ToSummaryText()
    {
        string state = survived ? "복귀" : "사망";
        string species = string.IsNullOrWhiteSpace(speciesTag) ? "미상" : speciesTag;
        return $"{name} / {species} / 전투력 {power:0.#} / 피해 {damageTaken:0.#} / {state}";
    }
}

public sealed class OffenseExpeditionResult
{
    public string expeditionId;
    public string targetId;
    public string targetTitle;
    public bool success;
    public float totalPower;
    public float requiredPower;
    public float danger;
    public float elapsedSeconds;
    public OffenseExpeditionMemberSnapshot[] members = Array.Empty<OffenseExpeditionMemberSnapshot>();
    public string[] rewardSummaries = Array.Empty<string>();
    public OffenseRewardGrantResult[] grantedRewards = Array.Empty<OffenseRewardGrantResult>();

    public string ToDetailText()
    {
        List<string> lines = new List<string>
        {
            success ? "원정 성공" : "원정 실패",
            $"대상: {targetTitle}",
            $"전투력: {totalPower:0.#} / 권장 {requiredPower:0.#}",
            $"위험도: {danger:0.#}",
            $"소요 시간: {elapsedSeconds:0.#}초"
        };

        if (members != null && members.Length > 0)
        {
            lines.Add(string.Empty);
            lines.Add("원정대:");
            foreach (OffenseExpeditionMemberSnapshot member in members)
            {
                if (member != null)
                {
                    lines.Add($"- {member.ToSummaryText()}");
                }
            }
        }

        if (success && grantedRewards != null && grantedRewards.Length > 0)
        {
            lines.Add(string.Empty);
            lines.Add("지급 결과:");
            foreach (OffenseRewardGrantResult reward in grantedRewards)
            {
                if (reward != null)
                {
                    lines.Add($"- {reward.ToSummaryText()}");
                }
            }
        }
        else if (success && rewardSummaries != null && rewardSummaries.Length > 0)
        {
            lines.Add(string.Empty);
            lines.Add("획득 보상:");
            foreach (string reward in rewardSummaries)
            {
                if (!string.IsNullOrWhiteSpace(reward))
                {
                    lines.Add($"- {reward}");
                }
            }
        }

        return string.Join("\n", lines);
    }
}

public sealed class OffenseExpeditionRun
{
    private readonly List<CharacterActor> members;

    public OffenseExpeditionRun(
        string expeditionId,
        OffenseTargetDefinition target,
        IEnumerable<CharacterActor> members,
        float totalPower)
    {
        ExpeditionId = string.IsNullOrWhiteSpace(expeditionId)
            ? Guid.NewGuid().ToString("N")
            : expeditionId;
        Target = target;
        this.members = members?.Where((member) => member != null).Distinct().ToList()
            ?? new List<CharacterActor>();
        TotalPower = Mathf.Max(0f, totalPower);
        RemainingSeconds = target != null ? Mathf.Max(1f, target.durationSeconds) : 1f;
        TotalDurationSeconds = RemainingSeconds;
    }

    public string ExpeditionId { get; }
    public OffenseTargetDefinition Target { get; }
    public IReadOnlyList<CharacterActor> MemberActors => members;
    public float TotalPower { get; }
    public float RemainingSeconds { get; private set; }
    public float TotalDurationSeconds { get; }
    public bool IsComplete => RemainingSeconds <= 0f;

    public void Tick(float deltaTime)
    {
        RemainingSeconds = Mathf.Max(0f, RemainingSeconds - Mathf.Max(0f, deltaTime));
    }
}
