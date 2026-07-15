using System;

public readonly struct OffenseTabSummary
{
    public OffenseTabSummary(
        bool hasWorldMap,
        int reconLevel,
        float scanRange,
        int visibleTargets,
        string selectedTargetId,
        int activeExpeditions,
        int moneyEarned,
        int prisonerCount,
        int recruitCandidateCount)
    {
        HasWorldMap = hasWorldMap;
        ReconLevel = reconLevel;
        ScanRange = scanRange;
        VisibleTargets = visibleTargets;
        SelectedTargetId = selectedTargetId ?? string.Empty;
        ActiveExpeditions = activeExpeditions;
        MoneyEarned = moneyEarned;
        PrisonerCount = prisonerCount;
        RecruitCandidateCount = recruitCandidateCount;
    }

    public bool HasWorldMap { get; }
    public int ReconLevel { get; }
    public float ScanRange { get; }
    public int VisibleTargets { get; }
    public string SelectedTargetId { get; }
    public int ActiveExpeditions { get; }
    public int MoneyEarned { get; }
    public int PrisonerCount { get; }
    public int RecruitCandidateCount { get; }
    public bool HasSelectedTarget => !string.IsNullOrWhiteSpace(SelectedTargetId);
}

public interface IOffenseTabSummaryService
{
    OffenseTabSummary Capture();
}

public sealed class OffenseTabSummaryService : IOffenseTabSummaryService
{
    private readonly IDungeonSceneComponentQuery sceneQuery;

    public OffenseTabSummaryService(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery ?? throw new ArgumentNullException(nameof(sceneQuery));
    }

    public OffenseTabSummary Capture()
    {
        OffenseWorldMapRuntime worldMap = sceneQuery.First<OffenseWorldMapRuntime>(includeInactive: true);
        OffenseExpeditionRuntime expedition = sceneQuery.First<OffenseExpeditionRuntime>(includeInactive: true);
        OffenseRewardRuntime rewards = sceneQuery.First<OffenseRewardRuntime>(includeInactive: true);

        return new OffenseTabSummary(
            worldMap != null,
            worldMap != null ? worldMap.State.ReconLevel : 0,
            worldMap != null ? worldMap.CurrentScanRange : 0f,
            worldMap != null ? worldMap.VisibleTargets.Count : 0,
            worldMap != null ? worldMap.State.SelectedTargetId : string.Empty,
            expedition != null ? expedition.ActiveExpeditions.Count : 0,
            rewards != null ? rewards.State.MoneyEarned : 0,
            rewards != null ? rewards.State.PrisonerCount : 0,
            rewards != null ? rewards.State.RecruitCandidateCount : 0);
    }
}
