public struct InvasionSpawnedEvent
{
    public CharacterActor intruderActor;
    public InvasionThreatSnapshot threatSnapshot;

    public InvasionSpawnedEvent(CharacterActor intruder, InvasionThreatSnapshot threatSnapshot)
    {
        intruderActor = intruder;
        this.threatSnapshot = threatSnapshot;
    }

    public static void Trigger(CharacterActor intruder, InvasionThreatSnapshot threatSnapshot)
    {
        InvasionSpawnedEvent e = new InvasionSpawnedEvent();
        e.intruderActor = intruder;
        e.threatSnapshot = threatSnapshot;
        EventObserver.TriggerEvent(e);
    }
}

public struct InvasionDungeonBreachedEvent
{
    public InvasionIntruderRuntime intruderRuntime;
    public CharacterActor intruderActor;
    public InvasionThreatSnapshot threatSnapshot;

    public InvasionDungeonBreachedEvent(
        InvasionIntruderRuntime intruderRuntime,
        CharacterActor intruderActor,
        InvasionThreatSnapshot threatSnapshot)
    {
        this.intruderRuntime = intruderRuntime;
        this.intruderActor = intruderActor;
        this.threatSnapshot = threatSnapshot;
    }

    public static void Trigger(
        InvasionIntruderRuntime intruderRuntime,
        CharacterActor intruderActor,
        InvasionThreatSnapshot threatSnapshot)
    {
        EventObserver.TriggerEvent(new InvasionDungeonBreachedEvent(
            intruderRuntime,
            intruderActor,
            threatSnapshot));
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

    public static void Trigger(CharacterActor intruder, BuildableObject facility)
    {
        InvasionFacilityDamagedEvent e = new InvasionFacilityDamagedEvent();
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

    public static void Trigger(CharacterActor intruder, CharacterActor owner)
    {
        InvasionFinalCombatStartedEvent e = new InvasionFinalCombatStartedEvent();
        e.intruderActor = intruder;
        e.ownerActor = owner;
        EventObserver.TriggerEvent(e);
    }
}
