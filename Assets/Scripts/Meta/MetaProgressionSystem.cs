using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public class MetaProgressionRuntime :
    MonoBehaviour,
    UtilEventListener<OperatingDayStartedEvent>,
    UtilEventListener<OperatingDayReportEvent>,
    UtilEventListener<InvasionThreatWarningEvent>,
    UtilEventListener<InvasionCandidateEvent>,
    UtilEventListener<InvasionStartedEvent>,
    UtilEventListener<InvasionResolvedEvent>,
    UtilEventListener<FacilityVisitEvent>,
    UtilEventListener<BlueprintResearchCompletedEvent>,
    UtilEventListener<FacilitySynthesisCompletedEvent>,
    UtilEventListener<OwnerRunEndedEvent>
{
    [SerializeField] private bool showRunResultPanel = true;

    private readonly MetaProgressionState state = new MetaProgressionState();
    private readonly MetaRunProgressTracker runProgress = new MetaRunProgressTracker();

    private bool ended;
    private IMetaRunResultBuilder runResultBuilder;
    private IRunResultPanelService runResultPanelService;

    public MetaProgressionState State => state;
    public RunResultSnapshot LatestResult { get; private set; }
    public MetaRunProgressTracker RunProgress => runProgress;
    public bool HasEnded => ended;

    [Inject]
    public void Construct(
        IMetaRunResultBuilder runResultBuilder,
        IRunResultPanelService runResultPanelService)
    {
        this.runResultBuilder = runResultBuilder
            ?? throw new ArgumentNullException(nameof(runResultBuilder));
        this.runResultPanelService = runResultPanelService
            ?? throw new ArgumentNullException(nameof(runResultPanelService));
    }

    public void SetShowRunResultPanel(bool value)
    {
        showRunResultPanel = value;
    }

    private void Awake()
    {
        StartNewRun();
    }

    public void StartNewRun()
    {
        runProgress.StartNewRun(Time.time);
        ended = false;
        LatestResult = null;
    }

    public void RestoreRunState(bool hasEnded, RunResultSnapshot latestResult)
    {
        ended = hasEnded;
        LatestResult = latestResult;
    }

    public bool TryPurchaseUpgrade(string id, out string message)
    {
        bool success = state.TryPurchaseUpgrade(id, out message);
        if (success)
        {
            MetaUpgradePurchasedEvent.Trigger(id);
            EventAlertService.Raise("계승 강화", message, EventAlertImportance.Medium, "계승");
        }

        return success;
    }

    public int GetStartingFacilityCandidateBonus()
    {
        return MetaProgressionEffects.GetIntegerBonus(
            state,
            MetaUpgradeEffectIds.StartingFacilityCandidates);
    }

    public int GetStartingOwnerTraitCandidateBonus()
    {
        return MetaProgressionEffects.GetIntegerBonus(
            state,
            MetaUpgradeEffectIds.StartingOwnerTraitCandidates);
    }

    public float GetOwnerMaxHealthMultiplier()
    {
        return MetaProgressionEffects.GetMultiplier(state, MetaUpgradeEffectIds.OwnerMaxHealth);
    }

    public float GetInvasionWarningThresholdMultiplier()
    {
        return MetaProgressionEffects.GetMultiplier(
            state,
            MetaUpgradeEffectIds.InvasionWarningThreshold);
    }

    public float GetCommerceStockCostMultiplier(StockCategory category)
    {
        if (category != StockCategory.Food && category != StockCategory.General)
        {
            return 1f;
        }

        return MetaProgressionEffects.GetMultiplier(state, MetaUpgradeEffectIds.CommerceStockCost);
    }

    public float GetFortressFacilityCostMultiplier(BuildingSO building)
    {
        return building?.Defense != null && building.Defense.IsDefenseFacility
            ? MetaProgressionEffects.GetMultiplier(state, MetaUpgradeEffectIds.FortressDefenseFacilityCost)
            : 1f;
    }

    public float GetArcaneResearchWorkMultiplier()
    {
        return MetaProgressionEffects.GetMultiplier(state, MetaUpgradeEffectIds.ArcaneResearchWork);
    }

    public bool IsRecipePreserved(string recipeId)
    {
        return !string.IsNullOrWhiteSpace(recipeId)
            && state.PreservedRecipeIds.Contains(recipeId);
    }

    public IReadOnlyCollection<int> GetExpandedBasicPurchaseBuildingIds(IEnumerable<BuildingSO> buildings)
    {
        int count = MetaProgressionEffects.GetIntegerBonus(
            state,
            MetaUpgradeEffectIds.BasicPurchaseEntries);
        if (count <= 0)
        {
            return Array.Empty<int>();
        }

        return buildings?
            .Where((building) => building != null
                && !building.IsGridMovement
                && !building.IsWall
                && FacilityShopService.GetBuildingStar(building) <= 1)
            .OrderBy((building) => building.id)
            .Take(count)
            .Select((building) => building.id)
            .ToArray()
            ?? Array.Empty<int>();
    }

    public void RecordOffenseSuccess()
    {
        runProgress.RecordOffenseSuccess();
    }

    public void OnTriggerEvent(OperatingDayStartedEvent eventType)
    {
        if (eventType.day <= 1 && ended)
        {
            StartNewRun();
        }

        runProgress.RecordOperatingDayStarted(eventType.day);
    }

    public void OnTriggerEvent(OperatingDayReportEvent eventType)
    {
        runProgress.RecordOperatingDayReport(eventType.report);
    }

    public void OnTriggerEvent(InvasionThreatWarningEvent eventType)
    {
        runProgress.RecordThreat(eventType.snapshot);
    }

    public void OnTriggerEvent(InvasionCandidateEvent eventType)
    {
        runProgress.RecordThreat(eventType.snapshot);
    }

    public void OnTriggerEvent(InvasionStartedEvent eventType)
    {
        runProgress.RecordInvasionStarted(eventType.snapshot);
    }

    public void OnTriggerEvent(InvasionResolvedEvent eventType)
    {
        runProgress.RecordInvasionResolved(eventType.defended);
    }

    public void OnTriggerEvent(FacilityVisitEvent eventType)
    {
        runProgress.RecordFacilityVisit(eventType.facility);
    }

    public void OnTriggerEvent(BlueprintResearchCompletedEvent eventType)
    {
        runProgress.RecordBlueprintResearchCompleted(eventType.unlockResult);
    }

    public void OnTriggerEvent(FacilitySynthesisCompletedEvent eventType)
    {
        runProgress.RecordFacilitySynthesisCompleted(eventType.result);
    }

    public void OnTriggerEvent(OwnerRunEndedEvent eventType)
    {
        EndRun(eventType.OwnerActor, eventType.Reason, eventType.Outcome);
    }

    public RunResultSnapshot EndRun(CharacterActor owner, string reason)
    {
        return EndRun(owner, reason, DungeonRunOutcome.Defeat);
    }

    public RunResultSnapshot EndRun(
        CharacterActor owner,
        string reason,
        DungeonRunOutcome outcome)
    {
        if (ended && LatestResult != null)
        {
            return LatestResult;
        }

        ended = true;
        RunResultSnapshot result = ResolveRunResultBuilder().Build(
            runProgress.CreateResultContext(owner, reason, outcome));
        result = result.WithLegacyCurrency(MetaProgressionCalculator.CalculateLegacyCurrency(result));
        state.AddCurrency(result.legacyCurrency);
        state.RecordRunCompleted();
        PreserveRunRecipes();
        LatestResult = result;

        RunResultReadyEvent.Trigger(result);
        EventAlertService.Raise("런 결과 정산", result.ToDetailText(), EventAlertImportance.High, "계승");
        if (showRunResultPanel)
        {
            ResolveRunResultPanelService().Show(result);
        }

        return result;
    }

    private IMetaRunResultBuilder ResolveRunResultBuilder()
    {
        return runResultBuilder
            ?? throw new InvalidOperationException($"{nameof(MetaProgressionRuntime)} requires {nameof(IMetaRunResultBuilder)} injection.");
    }

    private IRunResultPanelService ResolveRunResultPanelService()
    {
        return runResultPanelService
            ?? throw new InvalidOperationException($"{nameof(MetaProgressionRuntime)} requires {nameof(IRunResultPanelService)} injection.");
    }

    private void PreserveRunRecipes()
    {
        int slots = MetaProgressionEffects.GetIntegerBonus(
            state,
            MetaUpgradeEffectIds.PreservedRecipeSlots);
        state.PreserveRecipes(runProgress.UnlockedRecipeIds.OrderBy((id) => id), slots);
    }

    private void OnEnable()
    {
        this.EventStartListening<OperatingDayStartedEvent>();
        this.EventStartListening<OperatingDayReportEvent>();
        this.EventStartListening<InvasionThreatWarningEvent>();
        this.EventStartListening<InvasionCandidateEvent>();
        this.EventStartListening<InvasionStartedEvent>();
        this.EventStartListening<InvasionResolvedEvent>();
        this.EventStartListening<FacilityVisitEvent>();
        this.EventStartListening<BlueprintResearchCompletedEvent>();
        this.EventStartListening<FacilitySynthesisCompletedEvent>();
        this.EventStartListening<OwnerRunEndedEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<OperatingDayStartedEvent>();
        this.EventStopListening<OperatingDayReportEvent>();
        this.EventStopListening<InvasionThreatWarningEvent>();
        this.EventStopListening<InvasionCandidateEvent>();
        this.EventStopListening<InvasionStartedEvent>();
        this.EventStopListening<InvasionResolvedEvent>();
        this.EventStopListening<FacilityVisitEvent>();
        this.EventStopListening<BlueprintResearchCompletedEvent>();
        this.EventStopListening<FacilitySynthesisCompletedEvent>();
        this.EventStopListening<OwnerRunEndedEvent>();
    }

}
