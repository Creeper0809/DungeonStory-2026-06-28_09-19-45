using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class RunVariableRuntime :
    MonoBehaviour,
    UtilEventListener<OperatingDayStartedEvent>,
    UtilEventListener<OperatingDayEndedEvent>,
    UtilEventListener<InvasionCandidateEvent>,
    UtilEventListener<InvasionResolvedEvent>
{
    [SerializeField] private int runSeed;
    [SerializeField] private bool raiseAlerts = true;

    private readonly RunVariableState state = new RunVariableState();
    private System.Random random;
    private int currentDay = 1;
    private IOwnerRunDataProvider ownerRunDataProvider;
    private IInvasionThreatRuntimeProvider invasionThreatRuntimeProvider;
    private IRunStartVariableSelector runStartVariableSelector;

    public RunVariableState State => state;

    [Inject]
    public void Construct(
        IOwnerRunDataProvider ownerRunDataProvider,
        IInvasionThreatRuntimeProvider invasionThreatRuntimeProvider,
        IRunStartVariableSelector runStartVariableSelector)
    {
        this.ownerRunDataProvider = ownerRunDataProvider
            ?? throw new ArgumentNullException(nameof(ownerRunDataProvider));
        this.invasionThreatRuntimeProvider = invasionThreatRuntimeProvider
            ?? throw new ArgumentNullException(nameof(invasionThreatRuntimeProvider));
        this.runStartVariableSelector = runStartVariableSelector
            ?? throw new ArgumentNullException(nameof(runStartVariableSelector));
    }

    private void Awake()
    {
        if (runSeed == 0)
        {
            runSeed = Environment.TickCount;
        }

        random = new System.Random(runSeed);
    }

    public void StartRun(int seed, CharacterSO ownerData = null, InvasionThreatDifficulty difficulty = InvasionThreatDifficulty.Normal)
    {
        runSeed = seed != 0 ? seed : Environment.TickCount;
        random = new System.Random(runSeed);
        RunStartVariableSnapshot snapshot = ResolveRunStartVariableSelector().Create(runSeed, ownerData, difficulty);
        state.SetStartVariables(snapshot);
        RunStartVariablesSelectedEvent.Trigger(snapshot);

        if (raiseAlerts)
        {
            EventAlertService.Raise(
                "런 시작 변수",
                snapshot.ToSummaryText(),
                EventAlertImportance.Low,
                "런 변수");
        }
    }

    public void OnTriggerEvent(OperatingDayStartedEvent eventType)
    {
        currentDay = Mathf.Max(1, eventType.day);
        EnsureRunStarted();

        if (currentDay > 1)
        {
            RollOperationVariable(currentDay);
        }
    }

    public void OnTriggerEvent(OperatingDayEndedEvent eventType)
    {
        IReadOnlyList<ActiveRunVariable> expired = state.AdvanceOperationVariables();
        foreach (ActiveRunVariable active in expired)
        {
            RunVariableExpiredEvent.Trigger(active.Definition);
        }
    }

    public void OnTriggerEvent(InvasionCandidateEvent eventType)
    {
        EnsureRunStarted();
        SelectRandomInvasionVariable();
    }

    public void OnTriggerEvent(InvasionResolvedEvent eventType)
    {
        state.ClearInvasionVariable();
    }

    public ActiveRunVariable ActivateOperationVariable(RunVariableId id, int day = -1, bool alert = true)
    {
        RunVariableDefinition definition = RunVariableCatalog.Get(id);
        ActiveRunVariable active = state.ActivateOperationVariable(definition, day > 0 ? day : currentDay);
        if (active == null)
        {
            return null;
        }

        RunVariableActivatedEvent.Trigger(active);
        if (alert && raiseAlerts)
        {
            EventAlertService.Raise(
                active.Definition.title,
                active.Definition.ToDetailText(),
                active.Definition.importance,
                "운영 변수");
        }

        return active;
    }

    public RunVariableDefinition SelectInvasionVariable(RunVariableId id, bool alert = true)
    {
        RunVariableDefinition definition = RunVariableCatalog.Get(id);
        if (definition == null || definition.category != RunVariableCategory.Invasion)
        {
            return null;
        }

        state.SetInvasionVariable(definition);
        InvasionVariableSelectedEvent.Trigger(definition);
        if (alert && raiseAlerts)
        {
            EventAlertService.Raise(
                definition.title,
                definition.ToDetailText(),
                definition.importance,
                "침입 변수");
        }

        return definition;
    }

    public float GetGuestDemandMultiplier(string speciesTag)
    {
        return RunVariableEffects.GetGuestDemandMultiplier(state, speciesTag);
    }

    public float GetStockCostMultiplier(StockCategory category)
    {
        return RunVariableEffects.GetStockCostMultiplier(state, category);
    }

    public float GetFacilityShopCostMultiplier(BuildingSO building)
    {
        return RunVariableEffects.GetFacilityShopCostMultiplier(state, building);
    }

    public float GetBlueprintCostMultiplier(FacilityBlueprintSO blueprint)
    {
        return RunVariableEffects.GetBlueprintCostMultiplier(state, blueprint);
    }

    public float GetThreatRiseMultiplier()
    {
        return RunVariableEffects.GetThreatRiseMultiplier(state);
    }

    public float GetWarningThresholdMultiplier()
    {
        return RunVariableEffects.GetWarningThresholdMultiplier(state);
    }

    public InvasionIntruderSettings ApplyInvasionSettings(InvasionIntruderSettings source)
    {
        return RunVariableEffects.ApplyInvasionSettings(state, source);
    }

    private void RollOperationVariable(int day)
    {
        IReadOnlyList<RunVariableDefinition> definitions = RunVariableCatalog.GetByCategory(RunVariableCategory.Operation);
        if (definitions.Count == 0)
        {
            return;
        }

        RunVariableDefinition selected = definitions[random.Next(definitions.Count)];
        ActivateOperationVariable(selected.id, day);
    }

    private void SelectRandomInvasionVariable()
    {
        EnsureRandom();

        IReadOnlyList<RunVariableDefinition> definitions = RunVariableCatalog.GetByCategory(RunVariableCategory.Invasion);
        if (definitions.Count == 0)
        {
            return;
        }

        SelectInvasionVariable(definitions[random.Next(definitions.Count)].id);
    }

    private void EnsureRunStarted()
    {
        EnsureRandom();

        if (state.HasStarted)
        {
            return;
        }

        StartRun(runSeed, ResolveSelectedOwnerData(), ResolveDifficulty());
    }

    private void EnsureRandom()
    {
        if (random == null)
        {
            random = new System.Random(runSeed != 0 ? runSeed : Environment.TickCount);
        }
    }

    private InvasionThreatDifficulty ResolveDifficulty()
    {
        InvasionThreatRuntime threatRuntime = ResolveInvasionThreatRuntimeProvider()
            .TryGetRuntime(out InvasionThreatRuntime resolvedRuntime)
            ? resolvedRuntime
            : null;
        return threatRuntime != null && threatRuntime.Settings != null
            ? threatRuntime.Settings.difficulty
            : InvasionThreatDifficulty.Normal;
    }

    private CharacterSO ResolveSelectedOwnerData()
    {
        return ResolveOwnerRunDataProvider().SelectedOwnerData;
    }

    private IOwnerRunDataProvider ResolveOwnerRunDataProvider()
    {
        return ownerRunDataProvider
            ?? throw new InvalidOperationException($"{nameof(RunVariableRuntime)} requires {nameof(IOwnerRunDataProvider)} injection.");
    }

    private IInvasionThreatRuntimeProvider ResolveInvasionThreatRuntimeProvider()
    {
        return invasionThreatRuntimeProvider
            ?? throw new InvalidOperationException($"{nameof(RunVariableRuntime)} requires {nameof(IInvasionThreatRuntimeProvider)} injection.");
    }

    private IRunStartVariableSelector ResolveRunStartVariableSelector()
    {
        return runStartVariableSelector
            ?? throw new InvalidOperationException($"{nameof(RunVariableRuntime)} requires {nameof(IRunStartVariableSelector)} injection.");
    }

    private void OnEnable()
    {
        this.EventStartListening<OperatingDayStartedEvent>();
        this.EventStartListening<OperatingDayEndedEvent>();
        this.EventStartListening<InvasionCandidateEvent>();
        this.EventStartListening<InvasionResolvedEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<OperatingDayStartedEvent>();
        this.EventStopListening<OperatingDayEndedEvent>();
        this.EventStopListening<InvasionCandidateEvent>();
        this.EventStopListening<InvasionResolvedEvent>();
    }

}
