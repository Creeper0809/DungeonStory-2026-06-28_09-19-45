public interface IInvasionThreatRuntimeProvider
{
    bool TryGetRuntime(out InvasionThreatRuntime runtime);
}

public interface IInvasionDirectorRuntimeProvider
{
    bool TryGetRuntime(out InvasionDirectorRuntime runtime);
}

public sealed class InvasionThreatRuntimeProvider :
    CachedSceneRuntimeProvider<InvasionThreatRuntime>,
    IInvasionThreatRuntimeProvider
{
    public InvasionThreatRuntimeProvider(IDungeonSceneComponentQuery sceneQuery)
        : base(sceneQuery)
    {
    }

    public bool TryGetRuntime(out InvasionThreatRuntime resolvedRuntime)
    {
        return TryGetRuntimeComponent(out resolvedRuntime);
    }
}


public sealed class InvasionDirectorRuntimeProvider :
    CachedSceneRuntimeProvider<InvasionDirectorRuntime>,
    IInvasionDirectorRuntimeProvider
{
    public InvasionDirectorRuntimeProvider(IDungeonSceneComponentQuery sceneQuery)
        : base(sceneQuery)
    {
    }

    public bool TryGetRuntime(out InvasionDirectorRuntime resolvedRuntime)
    {
        return TryGetRuntimeComponent(out resolvedRuntime);
    }
}
