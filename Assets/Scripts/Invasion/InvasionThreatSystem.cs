using System;
using System.Collections.Generic;
using System.Linq;
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
        if (settings == null)
        {
            return 0f;
        }

        float raw = settings.baseRisePerSecond
            + (factors.dungeonValue * settings.dungeonValueRiseWeight)
            + (factors.reputation * settings.reputationRiseWeight)
            + (factors.time * settings.timeRiseWeight)
            + (factors.risk * settings.riskRiseWeight);

        float runMultiplier = RunVariableRuntime.Instance != null
            ? RunVariableRuntime.Instance.GetThreatRiseMultiplier()
            : 1f;
        return Mathf.Max(0f, raw * settings.GetDifficultyMultiplier() * Mathf.Max(0.05f, runMultiplier));
    }

    public static InvasionThreatFactors SampleWorldFactors(float secondsSinceLastInvasion)
    {
        BuildableObject[] buildings = UnityEngine.Object.FindObjectsByType<BuildableObject>(FindObjectsSortMode.None);
        Character[] characters = UnityEngine.Object.FindObjectsByType<Character>(FindObjectsSortMode.None);

        float dungeonValue = CalculateDungeonValue(buildings);
        float reputation = CalculateReputation(characters);
        float time = Mathf.Clamp(secondsSinceLastInvasion / 180f, 0f, 10f);
        float risk = CalculateRisk(buildings);
        return new InvasionThreatFactors(dungeonValue, reputation, time, risk);
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

    private static float CalculateDungeonValue(IEnumerable<BuildableObject> buildings)
    {
        if (buildings == null)
        {
            return 0f;
        }

        float value = 0f;
        foreach (BuildableObject building in buildings)
        {
            if (building == null || building.isDestroy)
            {
                continue;
            }

            value += 1f;
            if (building.BuildingData != null)
            {
                value += Mathf.Max(0, building.BuildingData.maintenance) / 100f;
                if (building.BuildingData.Facility != null && building.BuildingData.Facility.roles != FacilityRole.None)
                {
                    value += 0.5f;
                }
            }
        }

        return value;
    }

    private static float CalculateReputation(IEnumerable<Character> characters)
    {
        if (characters == null)
        {
            return 0f;
        }

        int customers = 0;
        float mood = 0f;
        foreach (Character character in characters)
        {
            if (character == null || character.characterType != CharacterType.Customer)
            {
                continue;
            }

            customers++;
            if (character.stats != null && character.stats.TryGetValue(Character.Condition.MOOD, out float sample))
            {
                mood += Mathf.Clamp(sample, 0f, 100f) / 100f;
            }
        }

        if (customers == 0)
        {
            return 0f;
        }

        return customers + (mood / Mathf.Max(1, customers));
    }

    private static float CalculateRisk(IEnumerable<BuildableObject> buildings)
    {
        if (buildings == null)
        {
            return 0f;
        }

        float risk = 0f;
        foreach (BuildableObject building in buildings)
        {
            if (building == null || building.isDestroy)
            {
                continue;
            }

            if (building.IsDamaged)
            {
                risk += 1.5f;
            }

            if (building is Shop shop && shop.Facility != null && shop.CurrentStock <= shop.Facility.restockRequestThreshold)
            {
                risk += 0.5f;
            }
        }

        return risk;
    }
}

public class InvasionThreatRuntime : MonoBehaviour,
    UtilEventListener<InvasionStartedEvent>,
    UtilEventListener<InvasionResolvedEvent>,
    UtilEventListener<OperatingDayStartedEvent>
{
    [SerializeField] private InvasionThreatSettings settings = new InvasionThreatSettings();

    private float currentThreat;
    private float secondsSinceLastInvasion;
    private float safetyRemaining;
    private float candidateDelayRemaining = -1f;
    private float warningCooldownRemaining;
    private bool warningRaisedThisCycle;
    private bool candidateRaisedThisCycle;
    private float residualRisk;
    private InvasionThreatFactors lastFactors;

    public float CurrentThreat => currentThreat;
    public float SafetyRemaining => safetyRemaining;
    public bool IsCandidatePending => candidateDelayRemaining >= 0f;
    public InvasionThreatStage CurrentStage => ResolveStage();
    public InvasionThreatSnapshot LatestSnapshot => BuildSnapshot();
    public InvasionThreatSettings Settings => settings;
    public static InvasionThreatRuntime Instance => FindFirstObjectByType<InvasionThreatRuntime>();

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    public void Tick(float deltaTime)
    {
        float safeDelta = Mathf.Max(0f, deltaTime);
        if (safeDelta <= 0f)
        {
            return;
        }

        if (warningCooldownRemaining > 0f)
        {
            warningCooldownRemaining = Mathf.Max(0f, warningCooldownRemaining - safeDelta);
        }

        if (safetyRemaining > 0f)
        {
            safetyRemaining = Mathf.Max(0f, safetyRemaining - safeDelta);
            secondsSinceLastInvasion += safeDelta;
            return;
        }

        secondsSinceLastInvasion += safeDelta;
        lastFactors = InvasionThreatCalculator.SampleWorldFactors(secondsSinceLastInvasion);
        if (residualRisk > 0f)
        {
            lastFactors = new InvasionThreatFactors(
                lastFactors.dungeonValue,
                lastFactors.reputation,
                lastFactors.time,
                lastFactors.risk + residualRisk);
        }

        currentThreat += InvasionThreatCalculator.CalculateRisePerSecond(settings, lastFactors) * safeDelta;
        currentThreat = Mathf.Max(0f, currentThreat);

        TryRaiseWarning();
        TickCandidateDelay(safeDelta);
    }

    public void AddThreat(float amount)
    {
        currentThreat = Mathf.Max(0f, currentThreat + amount);
        lastFactors = InvasionThreatCalculator.SampleWorldFactors(secondsSinceLastInvasion);
        TryRaiseWarning();
        TickCandidateDelay(0f);
    }

    public void OnTriggerEvent(InvasionStartedEvent eventType)
    {
        ResetAfterInvasion();
    }

    public void OnTriggerEvent(InvasionResolvedEvent eventType)
    {
        residualRisk = Mathf.Max(0f, eventType.residualRisk);
        if (!eventType.defended)
        {
            residualRisk += 2f;
        }
    }

    public void OnTriggerEvent(OperatingDayStartedEvent eventType)
    {
        if (eventType.day <= 1)
        {
            return;
        }

        AddThreat(Mathf.Min(6f, eventType.day * 0.5f));
    }

    private void OnEnable()
    {
        this.EventStartListening<InvasionStartedEvent>();
        this.EventStartListening<InvasionResolvedEvent>();
        this.EventStartListening<OperatingDayStartedEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<InvasionStartedEvent>();
        this.EventStopListening<InvasionResolvedEvent>();
        this.EventStopListening<OperatingDayStartedEvent>();
    }

    private void TryRaiseWarning()
    {
        float thresholdMultiplier = RunVariableRuntime.Instance != null
            ? RunVariableRuntime.Instance.GetWarningThresholdMultiplier()
            : 1f;
        if (MetaProgressionRuntime.Instance != null)
        {
            thresholdMultiplier *= MetaProgressionRuntime.Instance.GetInvasionWarningThresholdMultiplier();
        }
        float warningThreshold = settings != null
            ? settings.warningThreshold * Mathf.Max(0.05f, thresholdMultiplier)
            : 0f;

        if (settings == null
            || currentThreat < warningThreshold
            || warningRaisedThisCycle
            || warningCooldownRemaining > 0f)
        {
            return;
        }

        warningRaisedThisCycle = true;
        warningCooldownRemaining = settings.warningCooldownSeconds;
        InvasionThreatSnapshot snapshot = BuildSnapshot();
        InvasionThreatWarningEvent.Trigger(snapshot);
        EventAlertService.Raise(
            "침입 경고",
            InvasionThreatCalculator.BuildWarningDetail(snapshot),
            EventAlertImportance.Medium,
            "침입");
    }

    private void TickCandidateDelay(float deltaTime)
    {
        if (settings == null || candidateRaisedThisCycle)
        {
            return;
        }

        float thresholdMultiplier = RunVariableRuntime.Instance != null
            ? RunVariableRuntime.Instance.GetWarningThresholdMultiplier()
            : 1f;
        if (MetaProgressionRuntime.Instance != null)
        {
            thresholdMultiplier *= MetaProgressionRuntime.Instance.GetInvasionWarningThresholdMultiplier();
        }
        float candidateThreshold = settings.candidateThreshold * Mathf.Max(0.05f, thresholdMultiplier);
        if (currentThreat < candidateThreshold)
        {
            return;
        }

        if (candidateDelayRemaining < 0f)
        {
            candidateDelayRemaining = settings.GetCandidateDelay();
            return;
        }

        candidateDelayRemaining = Mathf.Max(0f, candidateDelayRemaining - Mathf.Max(0f, deltaTime));
        if (candidateDelayRemaining > 0f)
        {
            return;
        }

        candidateRaisedThisCycle = true;
        InvasionThreatSnapshot snapshot = BuildSnapshot();
        InvasionCandidateEvent.Trigger(snapshot);
        EventAlertService.Raise(
            "침입 임박",
            InvasionThreatCalculator.BuildCandidateDetail(snapshot),
            EventAlertImportance.High,
            "침입");
    }

    private void ResetAfterInvasion()
    {
        currentThreat = 0f;
        secondsSinceLastInvasion = 0f;
        safetyRemaining = settings != null ? Mathf.Max(0f, settings.safetyDurationSeconds) : 0f;
        candidateDelayRemaining = -1f;
        warningCooldownRemaining = 0f;
        warningRaisedThisCycle = false;
        candidateRaisedThisCycle = false;
        lastFactors = default;
    }

    private InvasionThreatSnapshot BuildSnapshot()
    {
        return new InvasionThreatSnapshot(
            currentThreat,
            ResolveStage(),
            lastFactors,
            candidateDelayRemaining,
            safetyRemaining);
    }

    private InvasionThreatStage ResolveStage()
    {
        if (safetyRemaining > 0f)
        {
            return InvasionThreatStage.Safety;
        }

        if (candidateDelayRemaining >= 0f || candidateRaisedThisCycle)
        {
            return InvasionThreatStage.Candidate;
        }

        float thresholdMultiplier = RunVariableRuntime.Instance != null
            ? RunVariableRuntime.Instance.GetWarningThresholdMultiplier()
            : 1f;
        if (MetaProgressionRuntime.Instance != null)
        {
            thresholdMultiplier *= MetaProgressionRuntime.Instance.GetInvasionWarningThresholdMultiplier();
        }
        float warningThreshold = settings != null
            ? settings.warningThreshold * Mathf.Max(0.05f, thresholdMultiplier)
            : 0f;
        if (settings != null && currentThreat >= warningThreshold)
        {
            return InvasionThreatStage.Warning;
        }

        return InvasionThreatStage.Peaceful;
    }
}
