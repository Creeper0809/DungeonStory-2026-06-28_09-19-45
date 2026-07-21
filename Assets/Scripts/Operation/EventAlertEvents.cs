public struct EventAlertRequestedEvent
{
    public EventAlertRequest request;

    public EventAlertRequestedEvent(EventAlertRequest request)
    {
        this.request = request;
    }

    public static void Trigger(EventAlertRequest request)
    {
        EventAlertRequestedEvent e = new EventAlertRequestedEvent();
        e.request = request;
        EventObserver.TriggerEvent(e);
    }
}

public struct EventAlertLoggedEvent
{
    public EventAlertRecordSnapshot record;

    public EventAlertLoggedEvent(EventAlertRecord record)
    {
        this.record = record?.CreateSnapshot();
    }

    public EventAlertLoggedEvent(EventAlertRecordSnapshot record)
    {
        this.record = record;
    }

    public static void Trigger(EventAlertRecord record)
    {
        EventAlertLoggedEvent e = new EventAlertLoggedEvent(record);
        EventObserver.TriggerEvent(e);
    }
}
