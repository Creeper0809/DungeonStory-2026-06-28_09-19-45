using System;
using UnityEngine;

public abstract class CachedSceneRuntimeProvider<T> where T : Component
{
    private readonly IDungeonSceneComponentQuery sceneQuery;
    private T runtime;

    protected CachedSceneRuntimeProvider(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery
            ?? throw new ArgumentNullException(nameof(sceneQuery));
    }

    protected bool TryGetRuntimeComponent(out T resolvedRuntime)
    {
        if (runtime == null)
        {
            runtime = sceneQuery.First<T>(includeInactive: true);
        }

        resolvedRuntime = runtime;
        return resolvedRuntime != null;
    }

    protected T GetRequiredRuntimeComponent(string ownerName)
    {
        if (TryGetRuntimeComponent(out T resolvedRuntime))
        {
            return resolvedRuntime;
        }

        throw new InvalidOperationException($"{ownerName} requires a loaded {typeof(T).Name}.");
    }
}

