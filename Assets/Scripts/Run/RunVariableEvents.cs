public struct RunStartVariablesSelectedEvent
{
    public RunStartVariableSnapshot snapshot;

    public RunStartVariablesSelectedEvent(RunStartVariableSnapshot snapshot)
    {
        this.snapshot = snapshot;
    }

    private static RunStartVariablesSelectedEvent e;

    public static void Trigger(RunStartVariableSnapshot snapshot)
    {
        e.snapshot = snapshot;
        EventObserver.TriggerEvent(e);
    }
}

public struct RunVariableActivatedEvent
{
    public ActiveRunVariable activeVariable;

    public RunVariableActivatedEvent(ActiveRunVariable activeVariable)
    {
        this.activeVariable = activeVariable;
    }

    private static RunVariableActivatedEvent e;

    public static void Trigger(ActiveRunVariable activeVariable)
    {
        e.activeVariable = activeVariable;
        EventObserver.TriggerEvent(e);
    }
}

public struct RunVariableExpiredEvent
{
    public RunVariableDefinition definition;

    public RunVariableExpiredEvent(RunVariableDefinition definition)
    {
        this.definition = definition;
    }

    private static RunVariableExpiredEvent e;

    public static void Trigger(RunVariableDefinition definition)
    {
        e.definition = definition;
        EventObserver.TriggerEvent(e);
    }
}

public struct InvasionVariableSelectedEvent
{
    public RunVariableDefinition definition;

    public InvasionVariableSelectedEvent(RunVariableDefinition definition)
    {
        this.definition = definition;
    }

    private static InvasionVariableSelectedEvent e;

    public static void Trigger(RunVariableDefinition definition)
    {
        e.definition = definition;
        EventObserver.TriggerEvent(e);
    }
}
