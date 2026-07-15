using UnityEngine;

public readonly struct MetaRunResultBuildContext
{
    public MetaRunResultBuildContext(
        CharacterActor owner,
        string reason,
        float runStartTime,
        int currentDay,
        int settlementCount,
        int defendedInvasionCount,
        InvasionThreatStage maxThreatStage,
        float finalInvasionThreat,
        int discoveredFacilityCount,
        int unlockedRecipeCount,
        int offenseSuccessCount)
    {
        Owner = owner;
        Reason = reason;
        RunStartTime = runStartTime;
        CurrentDay = currentDay;
        SettlementCount = settlementCount;
        DefendedInvasionCount = defendedInvasionCount;
        MaxThreatStage = maxThreatStage;
        FinalInvasionThreat = finalInvasionThreat;
        DiscoveredFacilityCount = discoveredFacilityCount;
        UnlockedRecipeCount = unlockedRecipeCount;
        OffenseSuccessCount = offenseSuccessCount;
    }

    public CharacterActor Owner { get; }
    public string Reason { get; }
    public float RunStartTime { get; }
    public int CurrentDay { get; }
    public int SettlementCount { get; }
    public int DefendedInvasionCount { get; }
    public InvasionThreatStage MaxThreatStage { get; }
    public float FinalInvasionThreat { get; }
    public int DiscoveredFacilityCount { get; }
    public int UnlockedRecipeCount { get; }
    public int OffenseSuccessCount { get; }
}

public interface IMetaRunResultBuilder
{
    RunResultSnapshot Build(MetaRunResultBuildContext context);
}

public sealed class MetaRunResultBuilder : IMetaRunResultBuilder
{
    private readonly IInvasionThreatRuntimeProvider threatRuntimeProvider;

    public MetaRunResultBuilder(IInvasionThreatRuntimeProvider threatRuntimeProvider)
    {
        this.threatRuntimeProvider = threatRuntimeProvider
            ?? throw new System.ArgumentNullException(nameof(threatRuntimeProvider));
    }

    public RunResultSnapshot Build(MetaRunResultBuildContext context)
    {
        float difficultyMultiplier = 1f;
        if (threatRuntimeProvider.TryGetRuntime(out InvasionThreatRuntime threatRuntime)
            && threatRuntime.Settings != null)
        {
            difficultyMultiplier = threatRuntime.Settings.GetDifficultyMultiplier();
        }

        CharacterIdentity identity = context.Owner != null ? context.Owner.Identity : null;
        return new RunResultSnapshot
        {
            ownerName = identity != null
                ? identity.DisplayName
                : context.Owner != null ? context.Owner.name : "사장",
            endReason = string.IsNullOrWhiteSpace(context.Reason)
                ? "사장 쓰러짐"
                : context.Reason,
            survivalSeconds = Mathf.Max(0f, Time.time - context.RunStartTime),
            survivedOperatingDays = Mathf.Max(1, context.CurrentDay),
            settlementCount = Mathf.Max(0, context.SettlementCount),
            defendedInvasionCount = Mathf.Max(0, context.DefendedInvasionCount),
            maxThreatStage = context.MaxThreatStage,
            finalInvasionThreat = Mathf.Max(0f, context.FinalInvasionThreat),
            firstDiscoveredFacilityCount = Mathf.Max(0, context.DiscoveredFacilityCount),
            firstUnlockedRecipeCount = Mathf.Max(0, context.UnlockedRecipeCount),
            offenseSuccessCount = Mathf.Max(0, context.OffenseSuccessCount),
            difficultyMultiplier = Mathf.Max(0.1f, difficultyMultiplier)
        };
    }
}

public interface IRunResultPanelService
{
    RunResultPanel Show(RunResultSnapshot result);
}

public sealed class RunResultPanelService : IRunResultPanelService
{
    private readonly IDungeonSceneComponentQuery sceneQuery;
    private readonly IRunResultPanelFactory panelFactory;

    public RunResultPanelService(
        IDungeonSceneComponentQuery sceneQuery,
        IRunResultPanelFactory panelFactory)
    {
        this.sceneQuery = sceneQuery
            ?? throw new System.ArgumentNullException(nameof(sceneQuery));
        this.panelFactory = panelFactory
            ?? throw new System.ArgumentNullException(nameof(panelFactory));
    }

    public RunResultPanel Show(RunResultSnapshot result)
    {
        RunResultPanel panel = sceneQuery.First<RunResultPanel>(includeInactive: true)
            ?? panelFactory.CreateDefaultPanel();
        panel.Render(result);
        return panel;
    }
}
