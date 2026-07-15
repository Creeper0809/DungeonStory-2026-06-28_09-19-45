using System;
using UnityEngine;

[Serializable]
public class CharacterAiPersonality
{
    [Range(0.25f, 2f)] public float diligence = 1f;
    [Range(0.25f, 2f)] public float curiosity = 1f;
    [Range(0.25f, 2f)] public float selfCare = 1f;
    [Range(0.25f, 2f)] public float patience = 1f;
    [Range(0.25f, 2f)] public float shoppingInterest = 1f;

    public float GetActionMultiplier(AIActionSet actionSet)
    {
        if (actionSet is AIWork)
        {
            return ClampMultiplier(diligence);
        }
        if (actionSet is AILookAround)
        {
            return ClampMultiplier(curiosity);
        }
        if (actionSet is AIEat
            || actionSet is AIRest
            || actionSet is AIFacilityRoleAction)
        {
            return ClampMultiplier(selfCare);
        }
        if (actionSet is AIShopping)
        {
            return ClampMultiplier(shoppingInterest);
        }
        if (actionSet is AIWait)
        {
            return ClampMultiplier(patience);
        }

        return 1f;
    }

    private static float ClampMultiplier(float value)
    {
        return Mathf.Clamp(value, 0.25f, 2f);
    }
}

public static class CharacterAiPersonalityUtility
{
    public static float GetActionScoreMultiplier(CharacterActor actor, AIActionSet actionSet)
    {
        CharacterIdentity identity = actor != null ? actor.Identity : null;
        CharacterAiPersonality personality = identity != null && identity.Data != null
            ? identity.Data.aiPersonality
            : null;
        float baseMultiplier = personality != null ? personality.GetActionMultiplier(actionSet) : 1f;
        float runtimeMultiplier = actor != null && actor.PersonaRuntime != null
            ? actor.PersonaRuntime.GetActionMultiplier(actionSet)
            : 1f;
        float configuredMultiplier = Mathf.Clamp(baseMultiplier * runtimeMultiplier, 0.1f, 4f);
        return CharacterMoodImpulseUtility.GetGoodMoodAdherenceMultiplier(
            actor,
            actionSet,
            configuredMultiplier);
    }
}
