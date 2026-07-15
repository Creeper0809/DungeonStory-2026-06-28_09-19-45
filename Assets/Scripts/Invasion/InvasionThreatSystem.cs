using System;
using System.Collections.Generic;
using UnityEngine;

public enum InvasionThreatDifficulty
{
    Easy,
    Normal,
    Hard
}
public enum InvasionThreatStage
{
    Peaceful,
    Warning,
    Candidate,
    Safety
}

[Serializable]
public class InvasionThreatSettings
{
    [Header("Thresholds")]
    [Range(1f, 100f)] public float warningThreshold = 70f;
    [Range(1f, 200f)] public float candidateThreshold = 100f;
    [Min(0f)] public float warningCooldownSeconds = 45f;
    [Min(0f)] public float safetyDurationSeconds = 30f;
    [Min(0f)] public float minCandidateDelaySeconds = 5f;
    [Min(0f)] public float maxCandidateDelaySeconds = 12f;

    [Header("Base Rise")]
    [Min(0f)] public float baseRisePerSecond = 0.025f;
    [Min(0f)] public float dungeonValueRiseWeight = 0.012f;
    [Min(0f)] public float reputationRiseWeight = 0.018f;
    [Min(0f)] public float timeRiseWeight = 0.01f;
    [Min(0f)] public float riskRiseWeight = 0.04f;

    [Header("Difficulty")]
    public InvasionThreatDifficulty difficulty = InvasionThreatDifficulty.Normal;
    [Min(0f)] public float easyMultiplier = 0.65f;
    [Min(0f)] public float normalMultiplier = 1f;
    [Min(0f)] public float hardMultiplier = 1.45f;

    public float GetDifficultyMultiplier()
    {
        return difficulty switch
        {
            InvasionThreatDifficulty.Easy => easyMultiplier,
            InvasionThreatDifficulty.Hard => hardMultiplier,
            _ => normalMultiplier
        };
    }

    public float GetCandidateDelay()
    {
        float min = Mathf.Max(0f, minCandidateDelaySeconds);
        float max = Mathf.Max(min, maxCandidateDelaySeconds);
        return UnityEngine.Random.Range(min, max);
    }
}

public readonly struct InvasionThreatFactors
{
    public readonly float dungeonValue;
    public readonly float reputation;
    public readonly float time;
    public readonly float risk;

    public InvasionThreatFactors(float dungeonValue, float reputation, float time, float risk)
    {
        this.dungeonValue = Mathf.Max(0f, dungeonValue);
        this.reputation = Mathf.Max(0f, reputation);
        this.time = Mathf.Max(0f, time);
        this.risk = Mathf.Max(0f, risk);
    }
}

public readonly struct InvasionThreatSnapshot
{
    public readonly float threat;
    public readonly InvasionThreatStage stage;
    public readonly InvasionThreatFactors factors;
    public readonly float pendingDelayRemaining;
    public readonly float safetyRemaining;

    public InvasionThreatSnapshot(
        float threat,
        InvasionThreatStage stage,
        InvasionThreatFactors factors,
        float pendingDelayRemaining,
        float safetyRemaining)
    {
        this.threat = threat;
        this.stage = stage;
        this.factors = factors;
        this.pendingDelayRemaining = Mathf.Max(0f, pendingDelayRemaining);
        this.safetyRemaining = Mathf.Max(0f, safetyRemaining);
    }
}

public struct InvasionThreatWarningEvent
{
    public InvasionThreatSnapshot snapshot;

    public InvasionThreatWarningEvent(InvasionThreatSnapshot snapshot)
    {
        this.snapshot = snapshot;
    }

    private static InvasionThreatWarningEvent e;

    public static void Trigger(InvasionThreatSnapshot snapshot)
    {
        e.snapshot = snapshot;
        EventObserver.TriggerEvent(e);
    }
}

public struct InvasionCandidateEvent
{
    public InvasionThreatSnapshot snapshot;

    public InvasionCandidateEvent(InvasionThreatSnapshot snapshot)
    {
        this.snapshot = snapshot;
    }

    private static InvasionCandidateEvent e;

    public static void Trigger(InvasionThreatSnapshot snapshot)
    {
        e.snapshot = snapshot;
        EventObserver.TriggerEvent(e);
    }
}

public struct InvasionStartedEvent
{
    public InvasionThreatSnapshot snapshot;

    public InvasionStartedEvent(InvasionThreatSnapshot snapshot)
    {
        this.snapshot = snapshot;
    }

    private static InvasionStartedEvent e;

    public static void Trigger(InvasionThreatSnapshot snapshot)
    {
        e.snapshot = snapshot;
        EventObserver.TriggerEvent(e);
    }
}

public struct InvasionResolvedEvent
{
    public bool defended;
    public float residualRisk;

    public InvasionResolvedEvent(bool defended, float residualRisk)
    {
        this.defended = defended;
        this.residualRisk = Mathf.Max(0f, residualRisk);
    }

    private static InvasionResolvedEvent e;

    public static void Trigger(bool defended, float residualRisk = 0f)
    {
        e.defended = defended;
        e.residualRisk = Mathf.Max(0f, residualRisk);
        EventObserver.TriggerEvent(e);
    }
}

public static class InvasionThreatCalculator
{
    public static float CalculateRisePerSecond(InvasionThreatSettings settings, InvasionThreatFactors factors)
    {
        return CalculateRisePerSecond(settings, factors, 1f);
    }

    public static float CalculateRisePerSecond(InvasionThreatSettings settings, InvasionThreatFactors factors, float runMultiplier)
    {
        if (settings == null)
        {
            return 0f;
        }

        float raw = settings.baseRisePerSecond
            + (factors.dungeonValue * settings.dungeonValueRiseWeight)
            + (factors.reputation * settings.reputationRiseWeight)
            + (factors.time * settings.timeRiseWeight)
            + (factors.risk * settings.riskRiseWeight);

        return Mathf.Max(0f, raw * settings.GetDifficultyMultiplier() * Mathf.Max(0.05f, runMultiplier));
    }

    public static string BuildWarningDetail(InvasionThreatSnapshot snapshot)
    {
        List<string> reasons = new List<string>();
        InvasionThreatFactors factors = snapshot.factors;

        if (factors.dungeonValue >= 3f)
        {
            reasons.Add("던전 가치 상승");
        }

        if (factors.reputation >= 2f)
        {
            reasons.Add("소문 증가");
        }

        if (factors.time >= 1f)
        {
            reasons.Add("마지막 침입 이후 시간 경과");
        }

        if (factors.risk >= 1f)
        {
            reasons.Add("취약한 운영 흔적");
        }

        string reasonText = reasons.Count > 0
            ? string.Join(", ", reasons)
            : "주변 정찰 활동 증가";

        return $"모험가들의 소문이 늘고 있습니다.\n징후: {reasonText}";
    }

    public static string BuildCandidateDetail(InvasionThreatSnapshot snapshot)
    {
        return "수상한 정찰대가 던전 근처에서 목격되었습니다.\n침입이 임박한 것 같습니다.";
    }
}
