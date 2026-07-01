using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/AI/Action/Work", order = 0)]
public class AIWork : AIActionSet
{
    [SerializeField] private FacilityWorkType workType = FacilityWorkType.None;
    [SerializeField] private float minimumDuration = 1f;
    [SerializeField] private int workInterruptPriority = 50;

    public FacilityWorkType WorkType => workType;
    public override bool IsContinuous => true;
    public override float MinimumDuration => minimumDuration;
    public override int InterruptPriority => workInterruptPriority;

    public override float AdjustScore(Character character, float baseScore)
    {
        if (character == null || !character.TryGetAbility(out AbilityWork work))
        {
            return 0f;
        }

        GridPathSearchResult searchResult = character.ai != null
            ? character.ai.GetPathSearch(character)
            : null;
        float utilityScore = work.GetWorkUtilityScore(workType, searchResult);
        if (utilityScore <= 0f)
        {
            return 0f;
        }

        float availableWorkWeight = Mathf.Lerp(0.6f, 1f, utilityScore);
        return Mathf.Clamp01(baseScore * availableWorkWeight);
    }

    public override bool CanStart(Character character)
    {
        if (character == null || !character.TryGetAbility(out AbilityWork work))
        {
            return false;
        }

        GridPathSearchResult searchResult = character.ai != null
            ? character.ai.GetPathSearch(character)
            : null;
        return work.CanStartWorkAction(workType, searchResult);
    }

    public override bool CanContinue(Character character, AIAction runningAction, out string stopReason)
    {
        stopReason = string.Empty;
        return character != null
            && character.TryGetAbility(out AbilityWork work)
            && work.CanContinueCurrentWork(out stopReason);
    }

    public override bool CanInterrupt(Character character, AIAction runningAction, out string interruptReason)
    {
        interruptReason = string.Empty;
        return character != null
            && character.TryGetAbility(out AbilityWork work)
            && work.ShouldInterruptCurrentWork(out interruptReason);
    }

    public override void Execute(Character character)
    {
        if (character != null && character.TryGetAbility(out AbilityWork work))
        {
            BuildableObject selectedDestination = character.ai != null
                ? character.ai.bestAction?.destination
                : null;
            work.StartWorking(workType, selectedDestination);
            return;
        }

        if (character != null && character.ai != null)
        {
            character.ai.isBestActionEnd = true;
        }
    }

    public override bool TryReserveDestination(
        Character character,
        BuildableObject destination,
        out AIActionFailure failure)
    {
        failure = AIActionFailure.None;
        if (destination == null)
        {
            return true;
        }

        if (destination.TryReserveWorker(character, out string failureReason))
        {
            return true;
        }

        failure = AIActionFailure.FromReason(
            failureReason,
            AIActionFailureKind.DestinationOccupied,
            destination);
        return false;
    }

    public override void RefreshDestinationReservation(Character character, BuildableObject destination)
    {
        destination?.RefreshWorkerReservation(character);
    }

    public override void ReleaseDestinationReservation(Character character, BuildableObject destination)
    {
        destination?.ReleaseWorkerReservation(character);
    }

    public override IReadOnlyList<BuildableObject> GetDestinationCandidates(
        Character character,
        GridPathSearchResult searchResult)
    {
        if (character == null || !character.TryGetAbility(out AbilityWork work))
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
