public sealed class ActiveRunVariableSnapshot
{
    public ActiveRunVariableSnapshot(ActiveRunVariable source)
    {
        Definition = source?.Definition;
        StartDay = source?.StartDay ?? 0;
        RemainingDays = source?.RemainingDays ?? 0;
        IsExpired = source == null || source.IsExpired;
    }

    public RunVariableDefinition Definition { get; }
    public int StartDay { get; }
    public int RemainingDays { get; }
    public bool IsExpired { get; }
}

public readonly struct RunStartVariablesSelectedEvent
{
    public RunStartVariablesSelectedEvent(RunStartVariableSnapshot snapshot)
    {
        this.snapshot = snapshot;
    }

    public RunStartVariableSnapshot snapshot { get; }

    public static void Trigger(RunStartVariableSnapshot snapshot)
    {
        EventObserver.TriggerEvent(new RunStartVariablesSelectedEvent(snapshot));
    }
}

public readonly struct RunVariableActivatedEvent
{
    public RunVariableActivatedEvent(ActiveRunVariable activeVariable)
    {
        this.activeVariable = new ActiveRunVariableSnapshot(activeVariable);
    }

    public ActiveRunVariableSnapshot activeVariable { get; }

    public static void Trigger(ActiveRunVariable activeVariable)
    {
        EventObserver.TriggerEvent(new RunVariableActivatedEvent(activeVariable));
    }
}

public readonly struct RunVariableExpiredEvent
{
    public RunVariableExpiredEvent(RunVariableDefinition definition)
    {
        this.definition = definition;
    }

    public RunVariableDefinition definition { get; }

    public static void Trigger(RunVariableDefinition definition)
    {
        EventObserver.TriggerEvent(new RunVariableExpiredEvent(definition));
    }
}

public readonly struct InvasionVariableSelectedEvent
{
    public InvasionVariableSelectedEvent(RunVariableDefinition definition)
    {
        this.definition = definition;
    }

    public RunVariableDefinition definition { get; }

    public static void Trigger(RunVariableDefinition definition)
    {
        EventObserver.TriggerEvent(new InvasionVariableSelectedEvent(definition));
    }
}
