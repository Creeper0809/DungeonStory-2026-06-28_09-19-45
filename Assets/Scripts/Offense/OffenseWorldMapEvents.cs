using System;
using System.Collections.Generic;
using System.Linq;

public readonly struct OffenseWorldMapStateSnapshot
{
    public OffenseWorldMapStateSnapshot(IOffenseWorldMapStateView state)
    {
        ReconLevel = state != null ? state.ReconLevel : 0;
        SelectedTargetId = state != null ? state.SelectedTargetId : string.Empty;
        KnownTargetIds = state != null
            ? EventPayloadSnapshot.Copy(state.KnownTargetIds.OrderBy(id => id, StringComparer.Ordinal).ToArray())
            : Array.Empty<string>();
        CompletedTargetIds = state != null
            ? EventPayloadSnapshot.Copy(state.CompletedTargetIds.OrderBy(id => id, StringComparer.Ordinal).ToArray())
            : Array.Empty<string>();
        RevealedTruthTargetId = state?.RevealedTruthTargetId ?? string.Empty;
    }

    public int ReconLevel { get; }
    public string SelectedTargetId { get; }
    public IReadOnlyList<string> KnownTargetIds { get; }
    public IReadOnlyList<string> CompletedTargetIds { get; }
    public string RevealedTruthTargetId { get; }
    public bool TruthRevealed => !string.IsNullOrWhiteSpace(RevealedTruthTargetId);
}

public readonly struct OffenseWorldMapChangedEvent
{
    public readonly OffenseWorldMapStateSnapshot state;
    public readonly IReadOnlyList<OffenseTargetSnapshot> visibleTargets;

    public OffenseWorldMapChangedEvent(
        OffenseWorldMapState state,
        IReadOnlyList<OffenseTargetSnapshot> visibleTargets)
    {
        this.state = new OffenseWorldMapStateSnapshot(state);
        OffenseTargetSnapshot[] targetSnapshots = visibleTargets?
            .Where(target => target != null)
            .Select(target => target.Copy())
            .ToArray()
            ?? Array.Empty<OffenseTargetSnapshot>();
        this.visibleTargets = EventPayloadSnapshot.Copy(targetSnapshots);
    }

    public static void Trigger(
        OffenseWorldMapState state,
        IReadOnlyList<OffenseTargetSnapshot> visibleTargets)
    {
        EventObserver.TriggerEvent(new OffenseWorldMapChangedEvent(state, visibleTargets));
    }
}

public readonly struct OffenseTargetSelectedEvent
{
    public OffenseTargetSnapshot target { get; }

    public OffenseTargetSelectedEvent(OffenseTargetSnapshot target)
    {
        this.target = target;
    }

    public static void Trigger(OffenseTargetSnapshot target)
    {
        EventObserver.TriggerEvent(new OffenseTargetSelectedEvent(target));
    }
}

public readonly struct OffenseReconUpgradedEvent
{
    public int reconLevel { get; }
    public float scanRange { get; }
    public int newlyRevealedCount { get; }

    public OffenseReconUpgradedEvent(int reconLevel, float scanRange, int newlyRevealedCount)
    {
        this.reconLevel = reconLevel;
        this.scanRange = scanRange;
        this.newlyRevealedCount = newlyRevealedCount;
    }

    public static void Trigger(int reconLevel, float scanRange, int newlyRevealedCount)
    {
        EventObserver.TriggerEvent(new OffenseReconUpgradedEvent(
            reconLevel,
            scanRange,
            newlyRevealedCount));
    }
}

public readonly struct OffenseCampaignProgressedEvent
{
    public OffenseCampaignProgressedEvent(
        OffenseTargetSnapshot target,
        int completedTargetCount,
        int totalTargetCount)
    {
        this.target = target;
        this.completedTargetCount = Math.Max(0, completedTargetCount);
        this.totalTargetCount = Math.Max(0, totalTargetCount);
    }

    public OffenseTargetSnapshot target { get; }
    public int completedTargetCount { get; }
    public int totalTargetCount { get; }

    public static void Trigger(
        OffenseTargetSnapshot target,
        int completedTargetCount,
        int totalTargetCount)
    {
        EventObserver.TriggerEvent(new OffenseCampaignProgressedEvent(
            target,
            completedTargetCount,
            totalTargetCount));
    }
}

public readonly struct OffenseTruthRevealedEvent
{
    public OffenseTruthRevealedEvent(
        string targetId,
        string title,
        string truthText)
    {
        this.targetId = targetId ?? string.Empty;
        this.title = string.IsNullOrWhiteSpace(title)
            ? OffenseWorldMapService.TruthTitle
            : title;
        this.truthText = string.IsNullOrWhiteSpace(truthText)
            ? OffenseWorldMapService.TruthRevealText
            : truthText;
    }

    public string targetId { get; }
    public string title { get; }
    public string truthText { get; }

    public static void Trigger(string targetId, string title, string truthText)
    {
        EventObserver.TriggerEvent(new OffenseTruthRevealedEvent(targetId, title, truthText));
    }
}
