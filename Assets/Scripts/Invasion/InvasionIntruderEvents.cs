public struct InvasionSpawnedEvent
{
    public CharacterActor intruderActor;
    public InvasionThreatSnapshot threatSnapshot;

    public InvasionSpawnedEvent(CharacterActor intruder, InvasionThreatSnapshot threatSnapshot)
    {
        intruderActor = intruder;
        this.threatSnapshot = threatSnapshot;
    }

    private static InvasionSpawnedEvent e;

    public static void Trigger(CharacterActor intruder, InvasionThreatSnapshot threatSnapshot)
    {
        e.intruderActor = intruder;
        e.threatSnapshot = threatSnapshot;
        EventObserver.TriggerEvent(e);
    }
}

public struct InvasionFacilityDamagedEvent
{
    public CharacterActor intruderActor;
    public BuildableObject facility;

    public InvasionFacilityDamagedEvent(CharacterActor intruder, BuildableObject facility)
    {
        intruderActor = intruder;
        this.facility = facility;
    }

    private static InvasionFacilityDamagedEvent e;

    public static void Trigger(CharacterActor intruder, BuildableObject facility)
    {
        e.intruderActor = intruder;
        e.facility = facility;
        EventObserver.TriggerEvent(e);
    }
}

public struct InvasionFinalCombatStartedEvent
{
    public CharacterActor intruderActor;
    public CharacterActor ownerActor;

    public InvasionFinalCombatStartedEvent(CharacterActor intruder, CharacterActor owner)
    {
        intruderActor = intruder;
        ownerActor = owner;
    }

    private static InvasionFinalCombatStartedEvent e;

    public static void Trigger(CharacterActor intruder, CharacterActor owner)
    {
        e.intruderActor = intruder;
        e.ownerActor = owner;
        EventObserver.TriggerEvent(e);
    }
}
