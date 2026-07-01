using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DrawWithUnity]
public abstract class AIActionSet : SerializedScriptableObject
{
    public string actionName;
    [SerializeField] private int defaultInterruptPriority;

    [field: SerializeField]
    public Consideration[] considerations { get; private set; }
    public virtual bool RequiresDestination => true;
    public virtual bool IsContinuous => false;
    public virtual float MinimumDuration => 0f;
    public virtual int InterruptPriority => defaultInterruptPriority;

    public virtual bool CanStart(Character character)
    {
        return true;
    }

    public virtual float AdjustScore(Character character, float baseScore)
    {
        return Mathf.Clamp01(baseScore);
    }

    public virtual bool CanStartWithContext(
        Character character,
        GridPathSearchResult searchResult,
        out string failureReason)
    {
        failureReason = string.Empty;
        if (!CanStart(character))
        {
            failureReason = "cannot start";
            return false;
        }

        if (!RequiresDestination)
        {
            return true;
        }

        if (!TryResolveDestination(character, searchResult, out BuildableObject destination, out failureReason))
        {
            if (string.IsNullOrWhiteSpace(failureReason))
            {
                failureReason = "no destination";
            }

            return false;
        }

        if (destination == null)
        {
            failureReason = "no destination";
            return false;
        }

        return true;
    }

    public virtual bool CanStartWithFailure(
        Character character,
        GridPathSearchResult searchResult,
        out AIActionFailure failure)
    {
        if (CanStartWithContext(character, searchResult, out string failureReason))
        {
            failure = AIActionFailure.None;
            return true;
        }

        failure = AIActionFailure.FromReason(
            failureReason,
            AIActionFailureKind.CannotStart);
        return false;
    }

    public virtual bool CanContinue(Character character, AIAction runningAction, out string stopReason)
    {
        stopReason = string.Empty;
        return true;
    }

    public virtual bool CanInterrupt(Character character, AIAction runningAction, out string interruptReason)
    {
        interruptReason = string.Empty;
        return false;
    }

    public abstract void Execute(Character character);

    public virtual IReadOnlyList<BuildableObject> GetDestinationCandidates(
        Character character,
        GridPathSearchResult searchResult)
    {
        BuildableObject legacyDestination = GetDestination(character);
        return legacyDestination != null
            ? new[] { legacyDestination }
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

    public virtual bool TryResolveDestinationWithFailure(
        Character character,
        GridPathSearchResult searchResult,
        out BuildableObject destination,
        out AIActionFailure failure)
    {
        if (TryResolveDestination(character, searchResult, out destination, out string failureReason))
        {
            failure = AIActionFailure.None;
            return true;
        }

        failure = AIActionFailure.FromReason(
            failureReason,
            AIActionFailureKind.NoDestination,
            destination);
        return false;
    }

    public virtual bool TryReserveDestination(
        Character character,
        BuildableObject destination,
        out AIActionFailure failure)
    {
        failure = AIActionFailure.None;
        return true;
    }

    public virtual void RefreshDestinationReservation(
        Character character,
        BuildableObject destination)
    {
    }

    public virtual void ReleaseDestinationReservation(
        Character character,
        BuildableObject destination)
    {
    }

    public virtual BuildableObject GetDestination(Character character)
    {
        return null;
    }
}
