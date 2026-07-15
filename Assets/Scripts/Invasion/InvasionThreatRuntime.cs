using System;
using UnityEngine;
using VContainer;

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
    private IInvasionThreatWorldSampler worldSampler;
    private IRunVariableRuntimeReader runVariableReader;
    private IMetaProgressionRuntimeReader metaProgressionReader;

    public float CurrentThreat => currentThreat;
    public float SafetyRemaining => safetyRemaining;
    public bool IsCandidatePending => candidateDelayRemaining >= 0f;
    public InvasionThreatStage CurrentStage => ResolveStage();
    public InvasionThreatSnapshot LatestSnapshot => BuildSnapshot();
    public InvasionThreatSettings Settings => settings;

    [Inject]
    public void Construct(
        IInvasionThreatWorldSampler worldSampler,
        IRunVariableRuntimeReader runVariableReader,
        IMetaProgressionRuntimeReader metaProgressionReader)
    {
        this.worldSampler = worldSampler
            ?? throw new ArgumentNullException(nameof(worldSampler));
        this.runVariableReader = runVariableReader
            ?? throw new ArgumentNullException(nameof(runVariableReader));
        this.metaProgressionReader = metaProgressionReader
            ?? throw new ArgumentNullException(nameof(metaProgressionReader));
    }

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
        lastFactors = SampleWorldFactors();
        if (residualRisk > 0f)
        {
            lastFactors = new InvasionThreatFactors(
                lastFactors.dungeonValue,
                lastFactors.reputation,
                lastFactors.time,
                lastFactors.risk + residualRisk);
        }

        currentThreat += InvasionThreatCalculator.CalculateRisePerSecond(
            settings,
            lastFactors,
            RequireRunVariableReader().GetThreatRiseMultiplier()) * safeDelta;
        currentThreat = Mathf.Max(0f, currentThreat);

        TryRaiseWarning();
        TickCandidateDelay(safeDelta);
    }

    public void AddThreat(float amount)
    {
        currentThreat = Mathf.Max(0f, currentThreat + amount);
        lastFactors = SampleWorldFactors();
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
        float thresholdMultiplier = GetWarningThresholdMultiplier();
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

        float thresholdMultiplier = GetWarningThresholdMultiplier();
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

        float thresholdMultiplier = GetWarningThresholdMultiplier();
        float warningThreshold = settings != null
            ? settings.warningThreshold * Mathf.Max(0.05f, thresholdMultiplier)
            : 0f;
        if (settings != null && currentThreat >= warningThreshold)
        {
            return InvasionThreatStage.Warning;
        }

        return InvasionThreatStage.Peaceful;
    }

    private InvasionThreatFactors SampleWorldFactors()
    {
        if (worldSampler == null)
        {
            throw new InvalidOperationException($"{nameof(InvasionThreatRuntime)} requires {nameof(IInvasionThreatWorldSampler)} injection.");
        }

        return worldSampler.Sample(secondsSinceLastInvasion);
    }

    private float GetWarningThresholdMultiplier()
    {
        return RequireRunVariableReader().GetWarningThresholdMultiplier()
            * RequireMetaProgressionReader().GetInvasionWarningThresholdMultiplier();
    }

    private IRunVariableRuntimeReader RequireRunVariableReader()
    {
        if (runVariableReader == null)
        {
            throw new InvalidOperationException($"{nameof(InvasionThreatRuntime)} requires {nameof(IRunVariableRuntimeReader)} injection.");
        }

        return runVariableReader;
    }

    private IMetaProgressionRuntimeReader RequireMetaProgressionReader()
    {
        if (metaProgressionReader == null)
        {
            throw new InvalidOperationException($"{nameof(InvasionThreatRuntime)} requires {nameof(IMetaProgressionRuntimeReader)} injection.");
        }

        return metaProgressionReader;
    }
}
