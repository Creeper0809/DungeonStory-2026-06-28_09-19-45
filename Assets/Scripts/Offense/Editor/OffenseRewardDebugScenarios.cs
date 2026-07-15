using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class OffenseRewardDebugScenarios
{
    [MenuItem("DungeonStory/Debug/Offense/Run P3 Reward Scenarios")]
    public static void RunFromMenu()
    {
        bool success = RunAll(true);
        if (!success)
        {
            Debug.LogError("P3 offense reward scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        List<string> errors = new List<string>();

        RunScenario("보상 서비스 돈/재고/상태 지급", VerifyMoneyStockAndStateRewards, errors);
        RunScenario("희귀 시설과 설계도 지급", VerifyRareFacilityAndBlueprintRewards, errors);
        RunScenario("원정 완료 시 보상 지급 연결", VerifyExpeditionCompletionGrantsRewards, errors);

        if (errors.Count > 0)
        {
            foreach (string error in errors)
            {
                Debug.LogError(error);
            }

            return false;
        }

        if (logSuccess)
        {
            Debug.Log("P3 offense reward scenarios passed.");
        }

        return true;
    }

    private static void RunScenario(string name, Func<bool> scenario, List<string> errors)
    {
        try
        {
            if (scenario()) return;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        errors.Add(name);
    }

    private static bool VerifyMoneyStockAndStateRewards()
    {
        using ScenarioContext context = new ScenarioContext(100);
        IReadOnlyList<OffenseRewardGrantResult> results = CreateGrantService().GrantRewards(
            new[]
            {
                Reward(OffenseRewardCategory.Money, "약탈금", 80),
                Reward(OffenseRewardCategory.Stock, "식재료", 40),
                Reward(OffenseRewardCategory.FactionWeakening, "인간 세력 약화", 2),
                Reward(OffenseRewardCategory.RecruitCandidate, "직원 후보", 1),
                Reward(OffenseRewardCategory.Prisoner, "포로", 1),
                Reward(OffenseRewardCategory.Prisoner, "특수 몬스터", 1)
            },
            context.CreateRewardContext());

        return results.Count == 6
            && results.All((result) => result.success)
            && context.GameData.holdingMoney.Value == 180
            && context.Warehouse.Inventory.GetStock(StockCategory.Food) == 40
            && context.RewardState.MoneyEarned == 80
            && context.RewardState.HumanFactionWeakening == 2
            && context.RewardState.RecruitCandidateCount == 1
            && context.RewardState.PrisonerCount == 1
            && context.RewardState.SpecialMonsterCount == 1;
    }

    private static bool VerifyRareFacilityAndBlueprintRewards()
    {
        using ScenarioContext context = new ScenarioContext(0);
        IReadOnlyList<OffenseRewardGrantResult> results = CreateGrantService().GrantRewards(
            new[]
            {
                Reward(OffenseRewardCategory.RareFacility, "희귀 시설", 1),
                Reward(OffenseRewardCategory.Blueprint, "특수 설계도", 1)
            },
            context.CreateRewardContext());

        return results.Count == 2
            && results.All((result) => result.success)
            && context.RewardState.RareFacilityBuildingIds.Count == 1
            && context.RewardState.AcquiredBlueprintIds.Count == 1
            && context.ResearchState.Tasks.Count == 1;
    }

    private static bool VerifyExpeditionCompletionGrantsRewards()
    {
        using ExpeditionRewardScenario scenario = new ExpeditionRewardScenario();
        CharacterActor worker = scenario.CreateCharacter("RewardWorker", CharacterType.NPC, CharacterRole.Regular, 12);

        bool started = scenario.Expedition.Runtime.TryStartExpedition(
            "food_farm",
            new[] { CharacterActor.From(worker) },
            out OffenseExpeditionRun expedition,
            out _);
        bool completed = scenario.Expedition.Runtime.CompleteExpeditionForDebug(
            expedition?.ExpeditionId,
            true,
            out OffenseExpeditionResult result);

        return started
            && completed
            && result != null
            && result.success
            && result.grantedRewards.Length == 2
            && result.grantedRewards.All((reward) => reward.success)
            && scenario.Context.GameData.holdingMoney.Value == 80
            && scenario.Context.Warehouse.Inventory.GetStock(StockCategory.Food) == 40
            && scenario.Reward.Runtime.State.MoneyEarned == 80;
    }

    private static OffenseRewardPreview Reward(OffenseRewardCategory category, string label, int amount)
    {
        return new OffenseRewardPreview
        {
            category = category,
            label = label,
            amount = amount
        };
    }

    private static IOffenseRewardGrantService CreateGrantService()
    {
        return new OffenseRewardGrantService(
            new OffenseRewardSelector(new EditorOffenseRewardCatalog()));
    }

    private static GameData CreateGameData(int holdingMoney)
    {
        GameData gameData = ScriptableObject.CreateInstance<GameData>();
        gameData.holdingMoney = new Data<int>();
        gameData.holdingMoney.Initialize(holdingMoney);
        return gameData;
    }

    private sealed class ScenarioContext : IDisposable
    {
        public ScenarioContext(int holdingMoney)
        {
            GameData = CreateGameData(holdingMoney);
            Warehouse = new TestWarehouse(500);
            ShopUnlockState = new FacilityShopUnlockState();
            ResearchState = new BlueprintResearchState();
            RewardState = new OffenseRewardState();
        }

        public GameData GameData { get; }
        public TestWarehouse Warehouse { get; }
        public FacilityShopUnlockState ShopUnlockState { get; }
        public BlueprintResearchState ResearchState { get; }
        public OffenseRewardState RewardState { get; }

        public OffenseRewardContext CreateRewardContext(OffenseTargetDefinition target = null)
        {
            return new OffenseRewardContext
            {
                gameData = GameData,
                warehouses = new[] { Warehouse },
                shopUnlockState = ShopUnlockState,
                researchState = ResearchState,
                rewardState = RewardState,
                target = target
            };
        }

        public void Dispose()
        {
            if (GameData != null)
            {
                Object.DestroyImmediate(GameData);
            }
        }
    }

    private sealed class TestWarehouse : IWarehouseFacility
    {
        public TestWarehouse(int capacity)
        {
            Inventory = new WarehouseInventory(capacity);
        }

        public WarehouseInventory Inventory { get; }
        public bool HasWarehouseInventory => true;
    }

    private sealed class EditorOffenseRewardCatalog : IOffenseRewardCatalog
    {
        private IReadOnlyCollection<BuildingSO> buildings;
        private IReadOnlyCollection<FacilityBlueprintSO> blueprints;

        public IReadOnlyCollection<BuildingSO> Buildings => buildings ??= LoadAssets<BuildingSO>();
        public IReadOnlyCollection<FacilityBlueprintSO> Blueprints => blueprints ??= LoadAssets<FacilityBlueprintSO>();

        private static IReadOnlyCollection<T> LoadAssets<T>()
            where T : Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where((asset) => asset != null)
                .ToArray();
        }
    }

    private sealed class ExpeditionRewardScenario : IDisposable
    {
        private readonly List<Object> objects = new List<Object>();

        public ScenarioContext Context { get; }
        public WorldMapFixture WorldMap { get; }
        public ExpeditionFixture Expedition { get; }
        public RewardFixture Reward { get; }

        public ExpeditionRewardScenario()
        {
            Context = new ScenarioContext(0);
            WorldMap = new WorldMapFixture();
            Reward = new RewardFixture(Context);
            Expedition = new ExpeditionFixture();
        }

        public CharacterActor CreateCharacter(
            string name,
            CharacterType type,
            CharacterRole role,
            int statValue)
        {
            CharacterSO data = ScriptableObject.CreateInstance<CharacterSO>();
            data.id = 9800 + objects.Count;
            data.characterName = name;
            data.characterType = type;
            data.role = role;
            data.speciesTag = "Orc";
            data.baseStats = CharacterStatBlock.CreateDefault(statValue);
            data.defaultWorkPriorities = WorkPriorityProfile.CreateDefault();

            GameObject obj = new GameObject(name);
            objects.Add(obj);
            objects.Add(data);
            obj.AddComponent<SpriteRenderer>();
            CharacterActor character = obj.AddComponent<CharacterActor>();
            obj.AddComponent<AbilityMove>();
            obj.AddComponent<AbilityWork>();
            AIBrain brain = obj.AddComponent<AIBrain>();
            brain.availableActions = type == CharacterType.Customer
                ? AiDebugScenarioActionFactory.CreateCustomerActions()
                : AiDebugScenarioActionFactory.CreateStaffActions();
            character.RefreshAbilityCache();
            character.Initialization(data);
            character.SetLifecycleState(CharacterLifecycleState.Active);
            character.stats[CharacterCondition.SLEEP] = 100f;
            character.stats[CharacterCondition.MOOD] = 100f;
            return character;
        }

        public void Dispose()
        {
            Expedition.Dispose();
            Reward.Dispose();
            WorldMap.Dispose();
            Context.Dispose();
            foreach (Object obj in objects.Where((item) => item != null))
            {
                Object.DestroyImmediate(obj);
            }
        }
    }

    private sealed class WorldMapFixture : IDisposable
    {
        private readonly GameObject obj;

        public OffenseWorldMapRuntime Runtime { get; }

        public WorldMapFixture()
        {
            obj = new GameObject("Offense Reward World Map Fixture");
            Runtime = obj.AddComponent<OffenseWorldMapRuntime>();
            Runtime.StartWorldMap();
        }

        public void Dispose()
        {
            Object.DestroyImmediate(obj);
        }
    }

    private sealed class ExpeditionFixture : IDisposable
    {
        private readonly GameObject obj;

        public OffenseExpeditionRuntime Runtime { get; }

        public ExpeditionFixture()
        {
            obj = new GameObject("Offense Reward Expedition Fixture");
            Runtime = obj.AddComponent<OffenseExpeditionRuntime>();
        }

        public void Dispose()
        {
            Object.DestroyImmediate(obj);
        }
    }

    private sealed class RewardFixture : IDisposable
    {
        private readonly GameObject obj;

        public OffenseRewardRuntime Runtime { get; }

        public RewardFixture(ScenarioContext context)
        {
            obj = new GameObject("Offense Reward Runtime Fixture");
            Runtime = obj.AddComponent<OffenseRewardRuntime>();
            Runtime.SetDebugContext(
                context.GameData,
                new[] { context.Warehouse },
                context.ShopUnlockState,
                context.ResearchState);
        }

        public void Dispose()
        {
            Runtime.ClearDebugContext();
            Object.DestroyImmediate(obj);
        }
    }
}
