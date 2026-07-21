using System;

public interface IFacilityEvolutionRuntimeProvider
{
    FacilityEvolutionRuntime Runtime { get; }
}

public interface IFacilitySynthesisRuntimeProvider
{
    FacilitySynthesisRuntime Runtime { get; }
}

public interface ICodexRuntimeProvider
{
    CodexRuntime Runtime { get; }
    bool TryGetRuntime(out CodexRuntime runtime);
}

public sealed class FacilityEvolutionRuntimeProvider :
    CachedSceneRuntimeProvider<FacilityEvolutionRuntime>,
    IFacilityEvolutionRuntimeProvider
{
    public FacilityEvolutionRuntimeProvider(IDungeonSceneComponentQuery sceneQuery)
        : base(sceneQuery)
    {
    }

    public FacilityEvolutionRuntime Runtime
    {
        get
        {
            return GetRequiredRuntimeComponent(nameof(IFacilityEvolutionRuntimeProvider));
        }
    }
}

public sealed class FacilitySynthesisRuntimeProvider :
    CachedSceneRuntimeProvider<FacilitySynthesisRuntime>,
    IFacilitySynthesisRuntimeProvider
{
    public FacilitySynthesisRuntimeProvider(IDungeonSceneComponentQuery sceneQuery)
        : base(sceneQuery)
    {
    }

    public FacilitySynthesisRuntime Runtime
    {
        get
        {
            return GetRequiredRuntimeComponent(nameof(IFacilitySynthesisRuntimeProvider));
        }
    }
}

public sealed class CodexRuntimeProvider :
    CachedSceneRuntimeProvider<CodexRuntime>,
    ICodexRuntimeProvider
{
    public CodexRuntimeProvider(IDungeonSceneComponentQuery sceneQuery)
        : base(sceneQuery)
    {
    }

    public CodexRuntime Runtime
    {
        get
        {
            return GetRequiredRuntimeComponent(nameof(ICodexRuntimeProvider));
        }
    }

    public bool TryGetRuntime(out CodexRuntime runtime)
    {
        return TryGetRuntimeComponent(out runtime);
    }
}
