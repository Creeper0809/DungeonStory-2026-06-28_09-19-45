using System;
using System.Collections.Generic;
using System.Linq;

public interface IOffenseWorldMapRuntimeProvider
{
    bool TryGetRuntime(out OffenseWorldMapRuntime runtime);
}

public interface IOffenseRewardRuntimeProvider
{
    bool TryGetRuntime(out OffenseRewardRuntime runtime);
}

public interface IOffenseExpeditionRuntimeProvider
{
    bool TryGetRuntime(out OffenseExpeditionRuntime runtime);
}

public interface IOffenseExpeditionMemberQuery
{
    IReadOnlyList<CharacterActor> GetAvailableMemberActors();
}

public interface IOffenseRewardCatalog
{
    IReadOnlyCollection<BuildingSO> Buildings { get; }
    IReadOnlyCollection<FacilityBlueprintSO> Blueprints { get; }
}

public interface IOffenseRewardSelector
{
    BuildingSO SelectRareFacility(
        OffenseRewardContext context,
        IReadOnlyCollection<int> additionallyExcludedBuildingIds);
    FacilityBlueprintSO SelectBlueprint(
        OffenseBlueprintRewardSpec rewardSpec,
        OffenseRewardContext context);
}

public interface IOffenseRewardGrantHandler
{
    string RewardTypeId { get; }
    OffenseRewardGrantResult Grant(
        OffenseRewardPreview reward,
        OffenseRewardContext context,
        IOffenseRewardSelector selector);
}

public interface IOffenseRewardGrantService
{
    IReadOnlyList<OffenseRewardGrantResult> GrantRewards(
        IEnumerable<OffenseRewardPreview> rewards,
        OffenseRewardContext context);
}

public interface IOffensePanelService
{
    OffenseWorldMapPanel ShowWorldMap(OffenseWorldMapRuntime runtime);
    OffenseExpeditionPanel ShowExpedition(OffenseExpeditionRuntime runtime);
}

public sealed class OffenseWorldMapRuntimeProvider :
    CachedSceneRuntimeProvider<OffenseWorldMapRuntime>,
    IOffenseWorldMapRuntimeProvider
{
    public OffenseWorldMapRuntimeProvider(IDungeonSceneComponentQuery sceneQuery)
        : base(sceneQuery)
    {
    }

    public bool TryGetRuntime(out OffenseWorldMapRuntime runtime)
    {
        return TryGetRuntimeComponent(out runtime);
    }
}

public sealed class OffenseRewardRuntimeProvider :
    CachedSceneRuntimeProvider<OffenseRewardRuntime>,
    IOffenseRewardRuntimeProvider
{
    public OffenseRewardRuntimeProvider(IDungeonSceneComponentQuery sceneQuery)
        : base(sceneQuery)
    {
    }

    public bool TryGetRuntime(out OffenseRewardRuntime runtime)
    {
        return TryGetRuntimeComponent(out runtime);
    }
}

public sealed class OffenseExpeditionRuntimeProvider :
    CachedSceneRuntimeProvider<OffenseExpeditionRuntime>,
    IOffenseExpeditionRuntimeProvider
{
    public OffenseExpeditionRuntimeProvider(IDungeonSceneComponentQuery sceneQuery)
        : base(sceneQuery)
    {
    }

    public bool TryGetRuntime(out OffenseExpeditionRuntime runtime)
    {
        return TryGetRuntimeComponent(out runtime);
    }
}

public sealed class OffenseExpeditionMemberQuery : IOffenseExpeditionMemberQuery
{
    private readonly IDungeonSceneComponentQuery sceneQuery;

    public OffenseExpeditionMemberQuery(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery
            ?? throw new ArgumentNullException(nameof(sceneQuery));
    }

    public IReadOnlyList<CharacterActor> GetAvailableMemberActors()
    {
        return OffenseExpeditionService
            .GetDistinctMembers(sceneQuery.All<CharacterActor>())
            .Where((actor) => OffenseExpeditionService.CanJoinExpedition(actor, out _))
            .OrderByDescending(OffenseExpeditionService.CalculateMemberPower)
            .ToList();
    }
}

public sealed class DataCatalogOffenseRewardCatalog : IOffenseRewardCatalog
{
    private readonly IDataCatalog catalog;

    public DataCatalogOffenseRewardCatalog(IDataCatalog catalog)
    {
        this.catalog = catalog
            ?? throw new ArgumentNullException(nameof(catalog));
    }

    public IReadOnlyCollection<BuildingSO> Buildings => catalog
        .GetData<BuildingSO>()
        .Values
        .Where((building) => building != null)
        .ToArray();

    public IReadOnlyCollection<FacilityBlueprintSO> Blueprints => catalog
        .GetData<FacilityBlueprintSO>()
        .Values
        .Where((blueprint) => blueprint != null)
        .ToArray();
}

public sealed class OffensePanelService : IOffensePanelService
{
    private readonly IDungeonSceneComponentQuery sceneQuery;
    private readonly IOffenseWorldMapRuntimeProvider worldMapProvider;
    private readonly IOffensePanelFactory panelFactory;
    private readonly IOffensePanelButtonFactory buttonFactory;

    public OffensePanelService(
        IDungeonSceneComponentQuery sceneQuery,
        IOffenseWorldMapRuntimeProvider worldMapProvider,
        IOffensePanelFactory panelFactory,
        IOffensePanelButtonFactory buttonFactory)
    {
        this.sceneQuery = sceneQuery
            ?? throw new ArgumentNullException(nameof(sceneQuery));
        this.worldMapProvider = worldMapProvider
            ?? throw new ArgumentNullException(nameof(worldMapProvider));
        this.panelFactory = panelFactory
            ?? throw new ArgumentNullException(nameof(panelFactory));
        this.buttonFactory = buttonFactory
            ?? throw new ArgumentNullException(nameof(buttonFactory));
    }

    public OffenseWorldMapPanel ShowWorldMap(OffenseWorldMapRuntime runtime)
    {
        if (runtime == null)
        {
            throw new ArgumentNullException(nameof(runtime));
        }

        sceneQuery.First<OffenseExpeditionPanel>(includeInactive: true)?.Hide();
        OffenseWorldMapPanel panel = sceneQuery.First<OffenseWorldMapPanel>(includeInactive: true)
            ?? panelFactory.CreateWorldMapPanel();
        panel.Bind(runtime, buttonFactory);
        return panel;
    }

    public OffenseExpeditionPanel ShowExpedition(OffenseExpeditionRuntime runtime)
    {
        if (runtime == null)
        {
            throw new ArgumentNullException(nameof(runtime));
        }

        worldMapProvider.TryGetRuntime(out OffenseWorldMapRuntime worldMap);
        sceneQuery.First<OffenseWorldMapPanel>(includeInactive: true)?.Hide();
        OffenseExpeditionPanel panel = sceneQuery.First<OffenseExpeditionPanel>(includeInactive: true)
            ?? panelFactory.CreateExpeditionPanel();
        panel.Bind(runtime, worldMap, buttonFactory);
        return panel;
    }
}
