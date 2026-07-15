using System;
using UnityEngine;

[Serializable]
public class InvasionIntruderSettings
{
    [Min(0.1f)] public float secondsToFullFocus = 30f;
    [Min(0.1f)] public float repathIntervalSeconds = 1.5f;
    [Min(0f)] public float facilityDamageIntervalSeconds = 5f;
    [Min(0f)] public float finalCombatDamage = 45f;
    [Min(0f)] public float finalCombatWindupSeconds = 0.7f;
}

public enum InvasionIntruderState
{
    None,
    Entering,
    Searching,
    MovingToOwner,
    DamagingFacility,
    FinalCombat,
    Finished
}
