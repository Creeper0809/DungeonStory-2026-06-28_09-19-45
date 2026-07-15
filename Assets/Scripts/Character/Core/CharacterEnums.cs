public enum CharacterFacing
{
    RIGHT,
    LEFT
}

public enum CharacterCondition
{
    HUNGER,
    SLEEP,
    FUN,
    MOOD,
    EXCRETION,
    HYGIENE
}

public enum CharacterDecisionState
{
    DECIDE,
    MOVE,
    EXECUTE
}

public enum CharacterLifecycleState
{
    None,
    SpawningOutside,
    EnteringDungeon,
    Active,
    ExitingDungeon,
    OnExpedition,
    Despawned
}
