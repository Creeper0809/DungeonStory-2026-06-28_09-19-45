using System;
using System.Collections.Generic;

public struct OffenseWorldMapChangedEvent
{
    public OffenseWorldMapState state;
    public IReadOnlyList<OffenseTargetSnapshot> visibleTargets;

    public OffenseWorldMapChangedEvent(
        OffenseWorldMapState state,
        IReadOnlyList<OffenseTargetSnapshot> visibleTargets)
    {
        this.state = state;
        this.visibleTargets = visibleTargets ?? Array.Empty<OffenseTargetSnapshot>();
    }

    private static OffenseWorldMapChangedEvent e;

    public static void Trigger(
        OffenseWorldMapState state,
        IReadOnlyList<OffenseTargetSnapshot> visibleTargets)
    {
        e.state = state;
        e.visibleTargets = visibleTargets ?? Array.Empty<OffenseTargetSnapshot>();
        EventObserver.TriggerEvent(e);
    }
}

public struct OffenseTargetSelectedEvent
{
    public OffenseTargetSnapshot target;

    public OffenseTargetSelectedEvent(OffenseTargetSnapshot target)
    {
        this.target = target;
    }

    private static OffenseTargetSelectedEvent e;

    public static void Trigger(OffenseTargetSnapshot target)
    {
        e.target = target;
        EventObserver.TriggerEvent(e);
    }
}

public struct OffenseReconUpgradedEvent
{
    public int reconLevel;
    public float scanRange;
    public int newlyRevealedCount;

    public OffenseReconUpgradedEvent(int reconLevel, float scanRange, int newlyRevealedCount)
    {
        this.reconLevel = reconLevel;
        this.scanRange = scanRange;
        this.newlyRevealedCount = newlyRevealedCount;
    }

    private static OffenseReconUpgradedEvent e;

    public static void Trigger(int reconLevel, float scanRange, int newlyRevealedCount)
    {
        e.reconLevel = reconLevel;
        e.scanRange = scanRange;
        e.newlyRevealedCount = newlyRevealedCount;
        EventObserver.TriggerEvent(e);
    }
}
