public struct EventAlertRequestedEvent
{
    public EventAlertRequest request;

    public EventAlertRequestedEvent(EventAlertRequest request)
    {
        this.request = request;
    }

    private static EventAlertRequestedEvent e;

    public static void Trigger(EventAlertRequest request)
    {
        e.request = request;
        EventObserver.TriggerEvent(e);
    }
}

public struct EventAlertLoggedEvent
{
    public EventAlertRecord record;

    public EventAlertLoggedEvent(EventAlertRecord record)
    {
        this.record = record;
    }

    private static EventAlertLoggedEvent e;

    public static void Trigger(EventAlertRecord record)
    {
        e.record = record;
        EventObserver.TriggerEvent(e);
    }
}
