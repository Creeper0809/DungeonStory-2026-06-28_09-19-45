using System;

public interface IRegularCustomerRuntimeProvider
{
    bool TryGetRuntime(out RegularCustomerRuntime runtime);
}

public sealed class RegularCustomerRuntimeProvider :
    CachedSceneRuntimeProvider<RegularCustomerRuntime>,
    IRegularCustomerRuntimeProvider
{
    public RegularCustomerRuntimeProvider(IDungeonSceneComponentQuery sceneQuery)
        : base(sceneQuery)
    {
    }

    public bool TryGetRuntime(out RegularCustomerRuntime resolvedRuntime)
    {
        return TryGetRuntimeComponent(out resolvedRuntime);
    }
}
