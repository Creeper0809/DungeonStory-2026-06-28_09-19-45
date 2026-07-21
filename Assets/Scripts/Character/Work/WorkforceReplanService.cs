using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IWorkforceReplanService
{
    void RequestIdleWorkersToReplan(bool clearFailures = true);
    void RequestOneWorkerToReplanFor(FacilityWorkType workType, bool clearFailures = true);
}

public sealed class DungeonWorkforceReplanService : IWorkforceReplanService
{
    private readonly IDungeonSceneComponentQuery sceneQuery;

    public DungeonWorkforceReplanService(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery
            ?? throw new ArgumentNullException(nameof(sceneQuery));
    }

    public void RequestIdleWorkersToReplan(bool clearFailures = true)
    {
        if (!Application.isPlaying)
        {
            return;
        }

        IReadOnlyList<AbilityWork> works = sceneQuery.All<AbilityWork>();
        foreach (AbilityWork work in works)
        {
            if (work == null)
            {
                continue;
            }

            AIBrain brain = work.WorkerActor?.Brain;
            if (work.isWorking)
            {
                brain?.InvalidateQueuedActionForNextDecision();
                continue;
            }

            brain?.RequestImmediateReplan(clearFailures);
        }
    }

    public void RequestOneWorkerToReplanFor(
        FacilityWorkType workType,
        bool clearFailures = true)
    {
        if (!Application.isPlaying || workType == FacilityWorkType.None)
        {
            return;
        }

        List<(AbilityWork work, WorkTargetCandidate candidate)> eligible =
            new List<(AbilityWork work, WorkTargetCandidate candidate)>();
        foreach (AbilityWork work in sceneQuery.All<AbilityWork>())
        {
            CharacterActor actor = work != null ? work.WorkerActor : null;
            AIBrain brain = actor != null ? actor.Brain : null;
            if (work == null
                || actor == null
                || brain == null
                || work.IsOffDuty
                || work.AssignedWorkType == workType
                || !work.WorkPriorities.IsEnabled(workType))
            {
                continue;
            }

            GridPathSearchResult search = brain.GetPathSearch(actor);
            if (!work.CanStartWorkAction(workType, search)
                || !work.TryGetBestWorkCandidate(workType, search, out WorkTargetCandidate candidate))
            {
                continue;
            }

            WorkPriorityLevel requestedPriority = work.WorkPriorities.GetPriority(workType);
            WorkPriorityLevel currentPriority = work.WorkPriorities.GetPriority(work.AssignedWorkType);
            bool canReplaceCurrent = !work.isWorking
                || currentPriority == WorkPriorityLevel.Off
                || requestedPriority <= currentPriority;
            if (canReplaceCurrent)
            {
                eligible.Add((work, candidate));
            }
        }

        (AbilityWork work, WorkTargetCandidate candidate) selected = eligible
            .OrderBy((entry) => entry.work.isWorking)
            .ThenByDescending((entry) => entry.candidate.Score)
            .FirstOrDefault();
        if (selected.work == null)
        {
            return;
        }

        AIBrain selectedBrain = selected.work.WorkerActor.Brain;
        if (selected.work.isWorking)
        {
            selectedBrain.StopCurrentActionForReplan(
                $"{WorkTaskCatalog.GetDisplayName(workType)} 작업 시작");
        }

        selectedBrain.RequestImmediateReplan(clearFailures);
    }
}
