using System;

public interface ILocalLlmRuntimeProvider
{
    bool TryGetRuntime(out ILocalLlmRuntime runtime);
    ILocalLlmRuntime GetRequiredRuntime();
}

public sealed class LocalLlmRuntimeProvider :
    CachedSceneRuntimeProvider<LocalLlmRequestQueue>,
    ILocalLlmRuntimeProvider
{
    public LocalLlmRuntimeProvider(IDungeonSceneComponentQuery sceneQuery)
        : base(sceneQuery)
    {
    }

    public bool TryGetRuntime(out ILocalLlmRuntime resolvedRuntime)
    {
        bool found = TryGetRuntimeComponent(out LocalLlmRequestQueue queue);
        resolvedRuntime = queue;
        return found;
    }

    public ILocalLlmRuntime GetRequiredRuntime()
    {
        if (TryGetRuntime(out ILocalLlmRuntime resolvedRuntime))
        {
            return resolvedRuntime;
        }

        throw new InvalidOperationException($"{nameof(LocalLlmRuntimeProvider)} requires a loaded {nameof(LocalLlmRequestQueue)}.");
    }
}
