public struct RunResultReadyEvent
{
    public RunResultSnapshot result;

    public RunResultReadyEvent(RunResultSnapshot result)
    {
        this.result = result;
    }

    private static RunResultReadyEvent e;

    public static void Trigger(RunResultSnapshot result)
    {
        e.result = result;
        EventObserver.TriggerEvent(e);
    }
}
