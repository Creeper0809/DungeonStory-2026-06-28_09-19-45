public interface IInvasionThreatRuntimeProvider
{
    bool TryGetRuntime(out InvasionThreatRuntime runtime);
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
