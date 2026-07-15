public struct CharacterDeathEvent
{
    public CharacterActor Actor;
    public string Reason;

    public CharacterDeathEvent(CharacterActor actor, string reason)
    {
        Actor = actor;
        Reason = reason;
    }

    private static CharacterDeathEvent e;

    public static void Trigger(CharacterActor actor, string reason)
    {
        e.Actor = actor;
        e.Reason = reason;
        EventObserver.TriggerEvent(e);
    }
}
