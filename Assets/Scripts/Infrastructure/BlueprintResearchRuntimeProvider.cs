using System;

public interface IBlueprintResearchRuntimeProvider
{
    bool TryGetRuntime(out BlueprintResearchRuntime runtime);
}

public interface IBlueprintResearchWorkService
{
    bool HasResearchWorkFor(BuildableObject facility);
    BlueprintResearchWorkResult ApplyResearchWork(
        CharacterActor researcher,
        BuildableObject researchFacility,
        float seconds);
}

public interface IBlueprintResearchStateService
{
    BlueprintResearchState GetState();
}

public sealed class BlueprintResearchRuntimeProvider :
    CachedSceneRuntimeProvider<BlueprintResearchRuntime>,
    IBlueprintResearchRuntimeProvider
{
    public BlueprintResearchRuntimeProvider(IDungeonSceneComponentQuery sceneQuery)
        : base(sceneQuery)
    {
    }

    public bool TryGetRuntime(out BlueprintResearchRuntime runtime)
    {
        return TryGetRuntimeComponent(out runtime);
    }
}

public sealed class BlueprintResearchWorkService : IBlueprintResearchWorkService
{
    private readonly IBlueprintResearchRuntimeProvider runtimeProvider;

    public BlueprintResearchWorkService(IBlueprintResearchRuntimeProvider runtimeProvider)
    {
        this.runtimeProvider = runtimeProvider
            ?? throw new ArgumentNullException(nameof(runtimeProvider));
    }

    public bool HasResearchWorkFor(BuildableObject facility)
    {
        return runtimeProvider.TryGetRuntime(out BlueprintResearchRuntime runtime)
            && runtime.HasActiveResearch
            && facility != null
            && facility.SupportsWork(FacilityWorkType.Research);
    }

    public BlueprintResearchWorkResult ApplyResearchWork(
        CharacterActor researcher,
        BuildableObject researchFacility,
        float seconds)
    {
        if (!runtimeProvider.TryGetRuntime(out BlueprintResearchRuntime runtime))
        {
            return new BlueprintResearchWorkResult(
                false,
                null,
                0f,
                0f,
                1f,
                false,
                "Research runtime is not available.");
        }

        return runtime.ApplyResearchWork(researcher, researchFacility, seconds);
    }
}

public sealed class BlueprintResearchStateService : IBlueprintResearchStateService
{
    private readonly IBlueprintResearchRuntimeProvider runtimeProvider;

    public BlueprintResearchStateService(IBlueprintResearchRuntimeProvider runtimeProvider)
    {
        this.runtimeProvider = runtimeProvider
            ?? throw new ArgumentNullException(nameof(runtimeProvider));
    }

    public BlueprintResearchState GetState()
    {
        if (!runtimeProvider.TryGetRuntime(out BlueprintResearchRuntime runtime))
        {
            throw new InvalidOperationException($"{nameof(BlueprintResearchStateService)} requires a loaded {nameof(BlueprintResearchRuntime)}.");
        }

        return runtime.State;
    }
}
