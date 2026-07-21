using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/AI/Action/Work", order = 0)]
public class AIWork : AIActionSet
{
    private static readonly CharacterAiActionDescriptor ActionDescriptor = new CharacterAiActionDescriptor(
        CharacterAiBranch.Work,
        "작업",
        CharacterAiActionTags.Work);

    public override CharacterAiActionDescriptor Descriptor => ActionDescriptor;
    [SerializeField] private FacilityWorkType workType = FacilityWorkType.None;
    [SerializeField] private float minimumDuration = 1f;
    [SerializeField] private int workInterruptPriority = 50;

    public FacilityWorkType WorkType => workType;
    public override bool IsContinuous => true;
    public override float MinimumDuration => minimumDuration;
    public override int InterruptPriority => workInterruptPriority;

    public override float AdjustScore(CharacterActor actor, float baseScore)
    {
        if (actor == null || !actor.TryGetAbility(out AbilityWork work))
        {
            return 0f;
        }

        GridPathSearchResult searchResult = actor.Brain != null
            ? actor.Brain.GetPathSearch(actor)
            : null;
        float utilityScore = work.GetWorkUtilityScore(workType, searchResult);
        if (utilityScore <= 0f)
        {
            return 0f;
        }

        float availableWorkWeight = Mathf.Lerp(0.6f, 1f, utilityScore);
        return Mathf.Clamp01(baseScore * availableWorkWeight);
    }

    public override bool CanStart(CharacterActor actor)
    {
        if (actor == null || !actor.TryGetAbility(out AbilityWork work))
        {
            return false;
        }

        GridPathSearchResult searchResult = actor.Brain != null
            ? actor.Brain.GetPathSearch(actor)
            : null;
        return work.CanStartWorkAction(workType, searchResult);
    }

    public override bool CanContinue(CharacterActor actor, AIAction runningAction, out string stopReason)
    {
        stopReason = string.Empty;
        return actor != null
            && actor.TryGetAbility(out AbilityWork work)
            && work.CanContinueCurrentWork(out stopReason);
    }

    public override bool CanInterrupt(CharacterActor actor, AIAction runningAction, out string interruptReason)
    {
        interruptReason = string.Empty;
        return actor != null
            && actor.TryGetAbility(out AbilityWork work)
            && work.ShouldInterruptCurrentWork(out interruptReason);
    }

    public override void Execute(CharacterActor actor)
    {
        if (actor != null && actor.TryGetAbility(out AbilityWork work))
        {
            BuildableObject selectedDestination = actor.Brain != null
                ? actor.Brain.bestAction?.destination
                : null;
            work.StartWorking(workType, selectedDestination);
            return;
        }

        if (actor != null && actor.Brain != null)
        {
            actor.Brain.isBestActionEnd = true;
        }
    }

    public override void OnStop(CharacterActor actor, AIAction runningAction, string reason)
    {
        if (actor != null && actor.TryGetAbility(out AbilityWork work))
        {
            work.StopAssignedWorkFromAi(reason);
        }
    }

    public override bool TryReserveDestination(
        CharacterActor actor,
        BuildableObject destination,
        out AIActionFailure failure)
    {
        failure = AIActionFailure.None;
        if (destination == null)
        {
            return true;
        }

        if (destination.TryReserveWorker(actor, out FacilityAssignmentStatus status))
        {
            return true;
        }

        failure = AIActionFailure.Create(
            status.FailureKind.ToAiActionFailureKind(),
            status.Reason,
            destination);
        return false;
    }

    public override void RefreshDestinationReservation(CharacterActor actor, BuildableObject destination)
    {
        destination?.RefreshWorkerReservation(actor);
    }

    public override void ReleaseDestinationReservation(CharacterActor actor, BuildableObject destination)
    {
        destination?.ReleaseWorkerReservation(actor);
    }

    public override IReadOnlyList<BuildableObject> GetDestinationCandidates(
        CharacterActor actor,
        GridPathSearchResult searchResult)
    {
        if (actor == null || !actor.TryGetAbility(out AbilityWork work))
        {
            return new List<BuildableObject>();
        }

        if (CanHandleSuppressCommand()
            && work.TryGetPrioritySuppressDestination(searchResult, out BuildableObject suppressDestination))
        {
            return new List<BuildableObject> { suppressDestination };
        }

        return work.TryGetBestWorkCandidate(workType, searchResult, out WorkTargetCandidate candidate)
            ? new List<BuildableObject> { candidate.Building }
            : new List<BuildableObject>();
    }

    private bool CanHandleSuppressCommand()
    {
        return workType == FacilityWorkType.None || workType == FacilityWorkType.Guard;
    }
}
