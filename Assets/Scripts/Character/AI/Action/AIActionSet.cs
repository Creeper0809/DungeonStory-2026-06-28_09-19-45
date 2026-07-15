using Sirenix.OdinInspector;
using System;
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

    public virtual bool CanStart(CharacterActor actor)
    {
        return true;
    }

    public virtual float AdjustScore(CharacterActor actor, float baseScore)
    {
        return Mathf.Clamp01(baseScore);
    }

    public virtual bool CanStartWithContext(
        CharacterActor actor,
        GridPathSearchResult searchResult,
        out string failureReason)
    {
        failureReason = string.Empty;
        if (!CanStart(actor))
        {
            failureReason = "cannot start";
            return false;
        }

        if (!RequiresDestination)
        {
            return true;
        }

        if (!TryResolveDestination(actor, searchResult, out BuildableObject destination, out failureReason))
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
        CharacterActor actor,
        GridPathSearchResult searchResult,
        out AIActionFailure failure)
    {
        if (CanStartWithContext(actor, searchResult, out string failureReason))
        {
            failure = AIActionFailure.None;
            return true;
        }

        failure = AIActionFailure.FromReason(
            failureReason,
            AIActionFailureKind.CannotStart);
        return false;
    }

    public virtual bool CanContinue(CharacterActor actor, AIAction runningAction, out string stopReason)
    {
        stopReason = string.Empty;
        return true;
    }

    public virtual bool CanInterrupt(CharacterActor actor, AIAction runningAction, out string interruptReason)
    {
        interruptReason = string.Empty;
        return false;
    }

    public virtual void Execute(CharacterActor actor)
    {
    }

    public virtual void OnStop(CharacterActor actor, AIAction runningAction, string reason)
    {
    }

    public virtual IReadOnlyList<BuildableObject> GetDestinationCandidates(
        CharacterActor actor,
        GridPathSearchResult searchResult)
    {
        BuildableObject destination = GetDestination(actor);
        return destination != null
            ? new[] { destination }
            : Array.Empty<BuildableObject>();
    }

    public virtual BuildableObject SelectDestination(
        CharacterActor actor,
        IReadOnlyList<BuildableObject> candidates)
    {
        return candidates != null
            ? candidates.FirstOrDefault((building) => building != null && !building.isDestroy)
            : null;
    }

    public virtual bool TryResolveDestination(
        CharacterActor actor,
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

        IReadOnlyList<BuildableObject> candidates = GetDestinationCandidates(actor, searchResult);
        if (candidates == null || candidates.Count == 0)
        {
            failureReason = "목적지 없음";
            return false;
        }

        destination = SelectDestination(actor, candidates);
        if (destination == null)
        {
            failureReason = "목적지 선택 실패";
            return false;
        }

        return true;
    }

    public virtual bool TryResolveDestinationWithFailure(
        CharacterActor actor,
        GridPathSearchResult searchResult,
        out BuildableObject destination,
        out AIActionFailure failure)
    {
        if (TryResolveDestination(actor, searchResult, out destination, out string failureReason))
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
        CharacterActor actor,
        BuildableObject destination,
        out AIActionFailure failure)
    {
        failure = AIActionFailure.None;
        return true;
    }

    public virtual void RefreshDestinationReservation(
        CharacterActor actor,
        BuildableObject destination)
    {
    }

    public virtual void ReleaseDestinationReservation(
        CharacterActor actor,
        BuildableObject destination)
    {
    }

    public virtual BuildableObject GetDestination(CharacterActor actor)
    {
        return null;
    }

}
