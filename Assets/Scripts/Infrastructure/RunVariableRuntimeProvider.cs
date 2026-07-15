using System;
using System.Collections.Generic;

public interface IRunVariableRuntimeProvider
{
    bool TryGetRuntime(out RunVariableRuntime runtime);
}

public interface IRunVariableRuntimeReader
{
    int GetInitialShopSeed();
    float GetGuestDemandMultiplier(string speciesTag);
    float GetStockCostMultiplier(StockCategory category);
    float GetFacilityShopCostMultiplier(BuildingSO building);
    float GetBlueprintCostMultiplier(FacilityBlueprintSO blueprint);
    float GetThreatRiseMultiplier();
    float GetWarningThresholdMultiplier();
    InvasionIntruderSettings ApplyInvasionSettings(InvasionIntruderSettings source);
}

public interface IOwnerRunDataProvider
{
    CharacterSO SelectedOwnerData { get; }
}

public interface IOwnerRunManagerProvider
{
    bool TryGetManager(out OwnerRunManager manager);
}

public interface IOwnerRunLifecycleService
{
    void HandleOwnerDeath(CharacterActor owner, string reason);
}

public sealed class RunVariableRuntimeProvider :
    CachedSceneRuntimeProvider<RunVariableRuntime>,
    IRunVariableRuntimeProvider
{
    public RunVariableRuntimeProvider(IDungeonSceneComponentQuery sceneQuery)
        : base(sceneQuery)
    {
    }

    public bool TryGetRuntime(out RunVariableRuntime resolvedRuntime)
    {
        return TryGetRuntimeComponent(out resolvedRuntime);
    }
}

public sealed class RunVariableRuntimeReader : IRunVariableRuntimeReader
{
    private readonly IRunVariableRuntimeProvider provider;

    public RunVariableRuntimeReader(IRunVariableRuntimeProvider provider)
    {
        this.provider = provider
            ?? throw new ArgumentNullException(nameof(provider));
    }

    public int GetInitialShopSeed()
    {
        return provider.TryGetRuntime(out RunVariableRuntime runtime)
            && runtime.State.StartVariables != null
            ? runtime.State.StartVariables.initialShopSeed
            : 0;
    }

    public float GetGuestDemandMultiplier(string speciesTag)
    {
        return provider.TryGetRuntime(out RunVariableRuntime runtime)
            ? runtime.GetGuestDemandMultiplier(speciesTag)
            : 1f;
    }

    public float GetStockCostMultiplier(StockCategory category)
    {
        return provider.TryGetRuntime(out RunVariableRuntime runtime)
            ? runtime.GetStockCostMultiplier(category)
            : 1f;
    }

    public float GetFacilityShopCostMultiplier(BuildingSO building)
    {
        return provider.TryGetRuntime(out RunVariableRuntime runtime)
            ? runtime.GetFacilityShopCostMultiplier(building)
            : 1f;
    }

    public float GetBlueprintCostMultiplier(FacilityBlueprintSO blueprint)
    {
        return provider.TryGetRuntime(out RunVariableRuntime runtime)
            ? runtime.GetBlueprintCostMultiplier(blueprint)
            : 1f;
    }

    public float GetThreatRiseMultiplier()
    {
        return provider.TryGetRuntime(out RunVariableRuntime runtime)
            ? runtime.GetThreatRiseMultiplier()
            : 1f;
    }

    public float GetWarningThresholdMultiplier()
    {
        return provider.TryGetRuntime(out RunVariableRuntime runtime)
            ? runtime.GetWarningThresholdMultiplier()
            : 1f;
    }

    public InvasionIntruderSettings ApplyInvasionSettings(InvasionIntruderSettings source)
    {
        return provider.TryGetRuntime(out RunVariableRuntime runtime)
            ? runtime.ApplyInvasionSettings(source)
            : source;
    }
}

public sealed class OwnerRunDataProvider : IOwnerRunDataProvider, IOwnerRunManagerProvider
{
    private readonly IDungeonSceneComponentQuery sceneQuery;
    private OwnerRunManager ownerRunManager;

    public OwnerRunDataProvider(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery
            ?? throw new ArgumentNullException(nameof(sceneQuery));
    }

    public CharacterSO SelectedOwnerData
    {
        get
        {
            return TryGetManager(out OwnerRunManager manager) && manager.selectedOwnerData != null
                ? manager.selectedOwnerData.Value
                : null;
        }
    }

    public bool TryGetManager(out OwnerRunManager manager)
    {
        ownerRunManager ??= sceneQuery.First<OwnerRunManager>(includeInactive: true);
        manager = ownerRunManager;
        return manager != null;
    }
}

public sealed class OwnerRunLifecycleService : IOwnerRunLifecycleService
{
    private readonly IOwnerRunManagerProvider provider;

    public OwnerRunLifecycleService(IOwnerRunManagerProvider provider)
    {
        this.provider = provider
            ?? throw new ArgumentNullException(nameof(provider));
    }

    public void HandleOwnerDeath(CharacterActor owner, string reason)
    {
        if (!provider.TryGetManager(out OwnerRunManager manager))
        {
            throw new InvalidOperationException($"{nameof(OwnerRunLifecycleService)} requires an active {nameof(OwnerRunManager)}.");
        }

        manager.HandleOwnerDeath(owner, reason);
    }
}
