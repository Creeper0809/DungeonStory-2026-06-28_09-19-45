using System;
using System.Collections.Generic;
using UnityEngine;

public interface IWorkforceReplanService
{
    void RequestIdleWorkersToReplan(bool clearFailures = true);
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
            if (work == null || work.isWorking)
            {
                continue;
            }

            work.WorkerActor?.Brain?.RequestImmediateReplan(clearFailures);
        }
    }
}
