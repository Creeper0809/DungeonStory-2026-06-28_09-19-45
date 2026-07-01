using System;
using UnityEngine;

public enum AIActionFailureKind
{
    None,
    NoAction,
    Cooldown,
    PathSearchDeferred,
    CannotStart,
    NoScore,
    NoDestination,
    DestinationSelectionFailed,
    DestinationOccupied,
    NoPath,
    NoGrid,
    NoWork,
    OffDuty,
    Unsupported,
    Destroyed,
    Unknown
}

[Serializable]
public struct AIActionFailure
{
    public AIActionFailure(AIActionFailureKind kind, string reason, BuildableObject target = null)
    {
        Kind = kind;
        Reason = reason ?? string.Empty;
        Target = target;
    }

    public AIActionFailureKind Kind { get; }
    public string Reason { get; }
    public BuildableObject Target { get; }
    public bool HasFailure => Kind != AIActionFailureKind.None;

    public static AIActionFailure None => new AIActionFailure(AIActionFailureKind.None, string.Empty);

    public static AIActionFailure Create(
        AIActionFailureKind kind,
        string reason = null,
        BuildableObject target = null)
    {
        return new AIActionFailure(kind, reason ?? GetDefaultReason(kind), target);
    }

    public static AIActionFailure FromReason(
        string reason,
        AIActionFailureKind defaultKind = AIActionFailureKind.Unknown,
        BuildableObject target = null)
    {
        return new AIActionFailure(ClassifyKind(reason, defaultKind), reason ?? string.Empty, target);
    }

    public static AIActionFailureKind ClassifyKind(
        string reason,
        AIActionFailureKind defaultKind = AIActionFailureKind.Unknown)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return defaultKind;
        }

        string normalized = reason.Trim();
        if (ContainsAny(normalized, "cooldown", "쿨다운"))
        {
            return AIActionFailureKind.Cooldown;
        }
        if (ContainsAny(normalized, "path search deferred", "경로 탐색 지연"))
        {
            return AIActionFailureKind.PathSearchDeferred;
        }
        if (ContainsAny(normalized, "no action", "행동 없음"))
        {
            return AIActionFailureKind.NoAction;
        }
        if (ContainsAny(normalized, "no score", "점수 없음"))
        {
            return AIActionFailureKind.NoScore;
        }
        if (ContainsAny(normalized, "AI 또는 그리드 없음", "그리드 없음"))
        {
            return AIActionFailureKind.NoGrid;
        }
        if (ContainsAny(normalized, "이미 근무자", "수용 인원", "혼잡", "occupied", "capacity"))
        {
            return AIActionFailureKind.DestinationOccupied;
        }
        if (ContainsAny(normalized, "경로 없음", "도달할 수 없는", "길 막힘", "no path"))
        {
            return AIActionFailureKind.NoPath;
        }
        if (ContainsAny(normalized, "목적지 선택 실패", "destination selection"))
        {
            return AIActionFailureKind.DestinationSelectionFailed;
        }
        if (ContainsAny(normalized, "목적지 없음", "no destination"))
        {
            return AIActionFailureKind.NoDestination;
        }
        if (ContainsAny(normalized, "작업 보류: 비번", "비번"))
        {
            return AIActionFailureKind.OffDuty;
        }
        if (ContainsAny(normalized, "지원하지", "지원하는 작업이 없습니다", "작업 가능한 시설이 아닙니다"))
        {
            return AIActionFailureKind.Unsupported;
        }
        if (ContainsAny(normalized, "시설 파괴", "destroy"))
        {
            return AIActionFailureKind.Destroyed;
        }
        if (ContainsAny(normalized, "작업", "근무"))
        {
            return AIActionFailureKind.NoWork;
        }
        if (ContainsAny(normalized, "cannot start", "시작 조건"))
        {
            return AIActionFailureKind.CannotStart;
        }

        return defaultKind;
    }

    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Reason) ? GetDefaultReason(Kind) : Reason;
    }

    private static bool ContainsAny(string value, params string[] patterns)
    {
        foreach (string pattern in patterns)
        {
            if (value.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static string GetDefaultReason(AIActionFailureKind kind)
    {
        return kind switch
        {
            AIActionFailureKind.None => string.Empty,
            AIActionFailureKind.NoAction => "행동 없음",
            AIActionFailureKind.Cooldown => "쿨다운",
            AIActionFailureKind.PathSearchDeferred => "경로 탐색 지연",
            AIActionFailureKind.CannotStart => "시작 조건 불만족",
            AIActionFailureKind.NoScore => "점수 없음",
            AIActionFailureKind.NoDestination => "목적지 없음",
            AIActionFailureKind.DestinationSelectionFailed => "목적지 선택 실패",
            AIActionFailureKind.DestinationOccupied => "목적지 선점됨",
            AIActionFailureKind.NoPath => "경로 없음",
            AIActionFailureKind.NoGrid => "AI 또는 그리드 없음",
            AIActionFailureKind.NoWork => "작업 없음",
            AIActionFailureKind.OffDuty => "비번",
            AIActionFailureKind.Unsupported => "지원하지 않음",
            AIActionFailureKind.Destroyed => "시설 파괴됨",
            _ => "알 수 없음"
        };
    }
}

public readonly struct AIActionDebugCandidate
{
    public AIActionDebugCandidate(
        string actionLabel,
        float score,
        AIActionFailure failure,
        BuildableObject destination)
    {
        ActionLabel = actionLabel ?? string.Empty;
        Score = Mathf.Clamp01(score);
        Failure = failure;
        Destination = destination;
    }

    public string ActionLabel { get; }
    public float Score { get; }
    public AIActionFailure Failure { get; }
    public BuildableObject Destination { get; }
}
