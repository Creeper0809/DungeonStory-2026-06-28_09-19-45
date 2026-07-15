public struct OffenseExpeditionStartedEvent
{
    public OffenseExpeditionRun expedition;

    public OffenseExpeditionStartedEvent(OffenseExpeditionRun expedition)
    {
        this.expedition = expedition;
    }

    private static OffenseExpeditionStartedEvent e;

    public static void Trigger(OffenseExpeditionRun expedition)
    {
        e.expedition = expedition;
        EventObserver.TriggerEvent(e);
    }
}

public struct OffenseExpeditionCompletedEvent
{
    public OffenseExpeditionResult result;

    public OffenseExpeditionCompletedEvent(OffenseExpeditionResult result)
    {
        this.result = result;
    }

    private static OffenseExpeditionCompletedEvent e;

    public static void Trigger(OffenseExpeditionResult result)
    {
        e.result = result;
        EventObserver.TriggerEvent(e);
    }
}
