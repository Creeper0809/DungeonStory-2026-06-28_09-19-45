using System;
using System.Collections.Generic;
using System.Linq;
using DamageNumbersPro;
using UnityEditor;
using UnityEngine;

internal static class CharacterAiEditorTestDependencies
{
    private static readonly ICharacterAiSchedulingService Scheduling =
        new ImmediateSchedulingService();
    private static readonly IFacilityCandidateCache FacilityCandidates =
        new FacilityCandidateCacheStore();
    private static readonly IRoomFacilityPolicy RoomPolicy =
        new RoomFacilityPolicyService(RoomRegistry.EditorCache);
    private static readonly IStaffDiscontentRuntimeService StaffDiscontent =
        new NoopStaffDiscontentService();
    private static readonly IMetaProgressionRuntimeReader MetaProgression =
        new DefaultMetaProgressionReader();
    private static readonly IFloatingIconFeedbackService FloatingIcons =
        new NoopFloatingIconFeedbackService();
    private static readonly IBlueprintResearchWorkService BlueprintResearch =
        new SceneBlueprintResearchWorkService();
    private static readonly IWorldInfoClickSelector WorldInfo =
        new NoopWorldInfoClickSelector();
    private static readonly IShopStockCatalog ShopStock =
        new AssetDatabaseShopStockCatalog();
    private static readonly IGridSystemProvider GridSystem =
        new EditorGridSystemProvider();
    private static GameData gameData;

    public static void Inject(GameObject actorObject)
    {
        if (actorObject == null)
        {
            return;
        }

        foreach (CharacterAbility ability in actorObject.GetComponents<CharacterAbility>())
        {
            ability.ConstructCharacterAbility(GridSystem);
        }

        actorObject.GetComponent<CharacterStats>()?.ConstructCharacterStats(
            StaffDiscontent,
            new NoopOwnerRunLifecycleService(),
            MetaProgression);

        actorObject.GetComponent<CustomerPersonaRuntime>()?.ConstructCustomerPersonaRuntime(
            new MissingLocalLlmRuntimeProvider());

        actorObject.GetComponent<AbilityWork>()?.ConstructAbilityWork(
            BlueprintResearch,
            StaffDiscontent,
            FloatingIcons,
            new ActiveWorkGridResolver(),
            FacilityCandidates);

        actorObject.GetComponent<AbilityShopping>()?.ConstructAbilityShopping(
            ShopStock,
            FloatingIcons);

        actorObject.GetComponent<AIBrain>()?.ConstructAIBrain(
            new ResourceCharacterAiActionAssetCatalog(new UnityResourcesAssetLoader()),
            Scheduling,
            new NeutralSocialReputationBiasService(),
            FacilityCandidates,
            new SceneFacilityLookup(),
            new CharacterAiJobGiverCatalog(),
            new CharacterAiDecisionPipeline(),
            RoomPolicy);
    }

    public static void Inject(BuildableObject building)
    {
        building?.ConstructBuildableObject(
            BlueprintResearch,
            WorldInfo,
            FacilityCandidates,
            RoomPolicy);
    }

    public static void InjectShop(Shop shop)
    {
        shop?.ConstructShop(
            new FixedGameDataProvider(GetGameData()),
            ShopStock,
            new NoopFloatingNumberFeedbackService(),
            new NoopWorkforceReplanService());
    }

    private static GameData GetGameData()
    {
        if (gameData != null)
        {
            return gameData;
        }

        gameData = ScriptableObject.CreateInstance<GameData>();
        gameData.hideFlags = HideFlags.HideAndDontSave;
        gameData.gameSpeed = new Data<int>();
        gameData.holdingMoney = new Data<int>();
        gameData.day = new Data<int>();
        gameData.curTime = new Data<float>();
        gameData.hour = new Data<int>();
        gameData.timeOfDay = new Data<TimeOfDay>();
        gameData.gameSpeed.Initialize(1);
        gameData.holdingMoney.Initialize(100000);
        gameData.day.Initialize(7);
        gameData.curTime.Initialize(0f);
        gameData.hour.Initialize(0);
        gameData.timeOfDay.Initialize(TimeOfDay.Morning);
        return gameData;
    }

    private sealed class ImmediateSchedulingService : ICharacterAiSchedulingService
    {
        public bool IsDrivingAi => false;
        public void Register(CharacterActor actor) { }
        public void Unregister(CharacterActor actor) { }
        public void RequestImmediateDecision(CharacterActor actor) { }
        public bool TryConsumePathSearchBudget() => true;
        public bool ShouldShowCharacterFeedback(CharacterActor actor) => false;
        public int GetMovementFrameStride(CharacterActor actor) => 1;
        public void ResetPathSearchBudgetForDebug() { }
    }

    private sealed class EditorGridSystemProvider : IGridSystemProvider
    {
        public GridSystemManager Manager
        {
            get
            {
                if (!TryGetManager(out GridSystemManager manager))
                {
                    throw new InvalidOperationException($"{nameof(GridSystemManager)} not found for editor AI fixture.");
                }

                return manager;
            }
        }

        public Grid Grid
        {
            get
            {
                if (!TryGetGrid(out Grid grid))
                {
                    throw new InvalidOperationException($"{nameof(GridSystemManager)} has no grid for editor AI fixture.");
                }

                return grid;
            }
        }

        public bool TryGetManager(out GridSystemManager manager)
        {
            manager = UnityEngine.Object.FindFirstObjectByType<GridSystemManager>(FindObjectsInactive.Include);
            if (manager == null)
            {
                return false;
            }

            manager.EnsureGridInitialized();
            return true;
        }

        public bool TryGetGrid(out Grid grid)
        {
            if (!TryGetManager(out GridSystemManager manager) || manager.grid == null)
            {
                grid = null;
                return false;
            }

            grid = manager.grid;
            return true;
        }
    }

    private sealed class NeutralSocialReputationBiasService : ISocialReputationBiasService
    {
        public float GetFacilityUtilityBias(CharacterActor actor, BuildableObject building) => 0f;
    }

    private sealed class MissingLocalLlmRuntimeProvider : ILocalLlmRuntimeProvider
    {
        public bool TryGetRuntime(out ILocalLlmRuntime runtime)
        {
            runtime = null;
            return false;
        }

        public ILocalLlmRuntime GetRequiredRuntime()
        {
            throw new InvalidOperationException("Editor AI fixture has no Local LLM runtime.");
        }
    }

    private sealed class SceneFacilityLookup : ICharacterAiFacilityLookup
    {
        public BuildableObject FindFacility(int id, string tag)
        {
            return UnityEngine.Object.FindObjectsByType<BuildableObject>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .FirstOrDefault(building => CharacterAiDecisionPipeline.MatchesFacility(building, id, tag));
        }
    }

    private sealed class ActiveWorkGridResolver : IWorkGridResolver
    {
        public Grid ResolveActiveGrid(
            AbilityWork work,
            GridPathSearchResult searchResult,
            Grid priorityGrid = null)
        {
            return priorityGrid
                ?? searchResult?.sourceGrid
                ?? UnityEngine.Object.FindFirstObjectByType<GridSystemManager>()?.grid;
        }

        public Vector2Int GetGridPosition(Grid activeGrid, CharacterActor actor)
        {
            return activeGrid != null && actor != null
                ? activeGrid.GetXY(actor.transform.position)
                : Vector2Int.zero;
        }
    }

    private sealed class NoopStaffDiscontentService : IStaffDiscontentRuntimeService
    {
        public float GetWorkEfficiencyMultiplier(CharacterActor staff) => 1f;

        public bool ShouldBlockWork(CharacterActor staff, out string reason)
        {
            reason = string.Empty;
            return false;
        }

        public bool IsRebellionTarget(CharacterActor target) => false;
        public bool ResolveSuppressedRebel(CharacterActor rebel, CharacterActor defender) => false;
    }

    private sealed class NoopOwnerRunLifecycleService : IOwnerRunLifecycleService
    {
        public void HandleOwnerDeath(CharacterActor owner, string reason) { }
    }

    private sealed class DefaultMetaProgressionReader : IMetaProgressionRuntimeReader
    {
        public int GetStartingFacilityCandidateBonus() => 0;
        public int GetStartingOwnerTraitCandidateBonus() => 0;
        public float GetOwnerMaxHealthMultiplier() => 1f;
        public float GetInvasionWarningThresholdMultiplier() => 1f;
        public bool IsRecipePreserved(string recipeId) => false;

        public IReadOnlyCollection<int> GetExpandedBasicPurchaseBuildingIds(
            IEnumerable<BuildingSO> buildings)
        {
            return Array.Empty<int>();
        }
    }

    private sealed class NoopFloatingIconFeedbackService : IFloatingIconFeedbackService
    {
        public bool Show(Component target, Sprite sprite, float maxWorldSize) => false;
    }

    private sealed class SceneBlueprintResearchWorkService : IBlueprintResearchWorkService
    {
        public bool HasResearchWorkFor(BuildableObject facility)
        {
            return TryGetRuntime(preferActive: true, out BlueprintResearchRuntime runtime)
                && runtime.HasActiveResearch
                && facility != null
                && facility.SupportsWork(FacilityWorkType.Research);
        }

        public BlueprintResearchWorkResult ApplyResearchWork(
            CharacterActor researcher,
            BuildableObject researchFacility,
            float seconds)
        {
            if (!TryGetRuntime(preferActive: true, out BlueprintResearchRuntime runtime))
            {
                return new BlueprintResearchWorkResult(
                    false,
                    null,
                    0f,
                    0f,
                    1f,
                    false,
                    "Editor test fixture has no blueprint research runtime.");
            }

            return runtime.ApplyResearchWork(researcher, researchFacility, seconds);
        }

        private static bool TryGetRuntime(bool preferActive, out BlueprintResearchRuntime runtime)
        {
            BlueprintResearchRuntime[] runtimes = UnityEngine.Object.FindObjectsByType<BlueprintResearchRuntime>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            runtime = preferActive
                ? runtimes.FirstOrDefault((candidate) => candidate != null && candidate.HasActiveResearch)
                    ?? runtimes.FirstOrDefault((candidate) => candidate != null)
                : runtimes.FirstOrDefault((candidate) => candidate != null);
            return runtime != null;
        }
    }

    private sealed class NoopWorldInfoClickSelector : IWorldInfoClickSelector
    {
        public bool TryHandleWorldInfoClick() => false;
        public bool TryTriggerCharacterUnderPointer() => false;

        public bool TryGetPreferredCharacterUnderPointer(out CharacterActor actor)
        {
            actor = null;
            return false;
        }

        public bool TryGetPreferredCharacterAtScreenPosition(
            Vector3 screenPosition,
            Camera camera,
            out CharacterActor actor)
        {
            actor = null;
            return false;
        }

        public bool TryGetPreferredCharacter(Collider2D[] hits, out CharacterActor actor)
        {
            actor = null;
            return false;
        }
    }

    private sealed class AssetDatabaseShopStockCatalog : IShopStockCatalog
    {
        public bool TryGetStockInfoForShop(int shopId, out StockInfo stockInfo)
        {
            stockInfo = AssetDatabase.FindAssets("t:StockInfo", new[] { "Assets/Resources/SO/Stock" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<StockInfo>)
                .FirstOrDefault(candidate => candidate != null && candidate.shopId == shopId);
            return stockInfo != null;
        }

        public bool TryGetSaleItem(int saleItemId, out SaleItem saleItem)
        {
            saleItem = LoadSaleItems().FirstOrDefault(candidate => candidate.id == saleItemId);
            return saleItem != null;
        }

        public StockCategory GetStockCategory(int saleItemId)
        {
            return TryGetSaleItem(saleItemId, out SaleItem saleItem)
                ? saleItem.category
                : StockCategory.General;
        }

        private static IEnumerable<SaleItem> LoadSaleItems()
        {
            return AssetDatabase.FindAssets("t:SaleItem", new[] { "Assets/Resources/SO/Stock" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<SaleItem>)
                .Where(item => item != null);
        }
    }

    private sealed class FixedGameDataProvider : IGameDataProvider
    {
        private readonly GameData value;

        public FixedGameDataProvider(GameData value)
        {
            this.value = value;
        }

        public bool TryGetGameData(out GameData resolvedGameData)
        {
            resolvedGameData = value;
            return resolvedGameData != null;
        }
    }

    private sealed class NoopFloatingNumberFeedbackService : IFloatingNumberFeedbackService
    {
        public bool TryShow(NumberCondition condition, Vector3 worldPosition, float value) => false;
    }

    private sealed class NoopWorkforceReplanService : IWorkforceReplanService
    {
        public void RequestIdleWorkersToReplan(bool clearFailures = true) { }
    }
}
