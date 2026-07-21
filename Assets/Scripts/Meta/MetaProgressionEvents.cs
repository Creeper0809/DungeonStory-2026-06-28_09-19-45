public readonly struct RunResultReadyEvent
{
    public RunResultSnapshot result { get; }

    public RunResultReadyEvent(RunResultSnapshot result)
    {
        this.result = result;
    }

    public static void Trigger(RunResultSnapshot result)
    {
        EventObserver.TriggerEvent(new RunResultReadyEvent(result));
    }
}
