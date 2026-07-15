using System;

public interface ISocialReputationRuntimeProvider
{
    bool TryGetRuntime(out SocialReputationRuntime runtime);
}

public interface ISocialReputationBiasService
{
    float GetFacilityUtilityBias(CharacterActor actor, BuildableObject building);
}

public sealed class SocialReputationRuntimeProvider :
    CachedSceneRuntimeProvider<SocialReputationRuntime>,
    ISocialReputationRuntimeProvider
{
    public SocialReputationRuntimeProvider(IDungeonSceneComponentQuery sceneQuery)
        : base(sceneQuery)
    {
    }

    public bool TryGetRuntime(out SocialReputationRuntime resolvedRuntime)
    {
        return TryGetRuntimeComponent(out resolvedRuntime);
    }
}

public sealed class SocialReputationBiasService : ISocialReputationBiasService
{
    private readonly ISocialReputationRuntimeProvider provider;

    public SocialReputationBiasService(ISocialReputationRuntimeProvider provider)
    {
        this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public float GetFacilityUtilityBias(CharacterActor actor, BuildableObject building)
    {
        if (actor == null || building == null)
        {
            return 0f;
        }

        if (!provider.TryGetRuntime(out SocialReputationRuntime runtime))
        {
            throw new InvalidOperationException($"{nameof(SocialReputationBiasService)} requires a loaded {nameof(SocialReputationRuntime)}.");
        }

        return runtime.GetFacilityUtilityBias(actor, building);
    }
}
