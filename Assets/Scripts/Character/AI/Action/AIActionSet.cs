using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AIActionSet : SerializedScriptableObject
{
    public string actionName;
    [field: SerializeField]
    public Consideration[] considerations { get; private set; }
    public virtual bool RequiresDestination => true;
    public virtual bool CanStart(Character character)
    {
        return true;
    }

    public abstract void Execute(Character character);

    public virtual IReadOnlyList<BuildableObject> GetDestinationCandidates(
        Character character,
        GridPathSearchResult searchResult)
    {
        BuildableObject legacyDestination = GetDestination(character);
        return legacyDestination != null
            ? new List<BuildableObject> { legacyDestination }
            : Array.Empty<BuildableObject>();
    }

    public virtual BuildableObject SelectDestination(
        Character character,
        IReadOnlyList<BuildableObject> candidates)
    {
        return candidates != null
            ? candidates.FirstOrDefault((building) => building != null && !building.isDestroy)
            : null;
    }

    public virtual bool TryResolveDestination(
        Character character,
        GridPathSearchResult searchResult,
        out BuildableObject destination,
        out string failureReason)
    {
        destination = null;
        failureReason = string.Empty;

        if (!RequiresDestination)
        {
            return true;
        }

        IReadOnlyList<BuildableObject> candidates = GetDestinationCandidates(character, searchResult);
        if (candidates == null || candidates.Count == 0)
        {
            failureReason = "목적지 없음";
            return false;
        }

        destination = SelectDestination(character, candidates);
        if (destination == null)
        {
            failureReason = "목적지 선택 실패";
            return false;
        }

        return true;
    }

    public virtual BuildableObject GetDestination(Character character)
    {
        return null;
    }
}
