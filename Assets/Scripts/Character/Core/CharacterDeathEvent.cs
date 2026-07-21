public struct CharacterDeathEvent
{
    public CharacterActor Actor;
    public string Reason;

    public CharacterDeathEvent(CharacterActor actor, string reason)
    {
        Actor = actor;
        Reason = reason;
    }

    public static void Trigger(CharacterActor actor, string reason)
    {
        CharacterDeathEvent e = new CharacterDeathEvent();
        e.Actor = actor;
        e.Reason = reason;
        EventObserver.TriggerEvent(e);
    }
}
