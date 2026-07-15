using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class BlueprintResearchDebugScenarios
{
    [MenuItem("DungeonStory/Debug/Research/Run P1 Blueprint Research Scenarios")]
    public static void RunFromMenu()
    {
        bool success = RunAll(true);
        if (!success)
        {
            Debug.LogError("P1 blueprint research scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        P1FacilityShopAssetBuilder.EnsureP1FacilityShopAssets();

        List<string> errors = new List<string>();
        RunScenario("설계도 연구 데이터", VerifyBlueprintResearchData, errors);
        RunScenario("구매 이벤트 연구 대기열", VerifyBlueprintPurchaseQueuesResearch, errors);
        RunScenario("연구 중단/재개 진행도", VerifyResearchProgressPersistsUntilCompletion, errors);
        RunScenario("연구 완료 기본 구매 해금", VerifyCompletionUnlocksBasicPurchase, errors);
        RunScenario("연구 완료 조합식 해금", VerifyCompletionUnlocksRecipes, errors);
        RunScenario("연구 속도 보정", VerifyResearchSpeedUsesCharacterAndFacilityModifiers, errors);
        RunScenario("연구 작업 후보 조건", VerifyResearchWorkCandidateRequiresQueuedBlueprint, errors);

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
            Debug.Log("P1 blueprint research scenarios passed.");
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

    private static bool VerifyBlueprintResearchData()
    {
        FacilityBlueprintSO commercial = LoadBlueprint("BP_CommercialBasics");
        FacilityBlueprintSO rare = LoadBlueprint("BP_BattleDining");

        return commercial != null
            && commercial.researchWorkRequired > 0f
            && commercial.unlockBasicPurchaseBuildingIds.Length > 0
            && rare != null
            && rare.researchWorkRequired > commercial.researchWorkRequired
            && rare.unlockRecipeIds.Contains("recipe_battlefield_dining_2");
    }

    private static bool VerifyBlueprintPurchaseQueuesResearch()
    {
        FacilityBlueprintSO blueprint = LoadBlueprint("BP_CommercialBasics");
        GameData gameData = CreateGameData(500);
        FacilityShopUnlockState state = new FacilityShopUnlockState();
        FacilityShopOffer offer = new FacilityShopOffer(blueprint, 100, blueprint.rarity, true);
        GameObject runtimeObject = new GameObject("BlueprintResearchRuntime_Purchase_Test");
        BlueprintResearchRuntime runtime = runtimeObject.AddComponent<BlueprintResearchRuntime>();
        runtime.EventStartListening<FacilityShopPurchasedEvent>();

        bool purchased = FacilityShopService.TryPurchaseOffer(gameData, offer, state, out FacilityShopPurchaseResult result);
        bool valid = purchased
            && result.success
            && result.blueprint == blueprint
            && state.IsBlueprintAcquired(blueprint)
            && runtime.State.Tasks.Count == 1
            && runtime.State.Tasks[0].Blueprint == blueprint;

        Object.DestroyImmediate(runtimeObject);
        Object.DestroyImmediate(gameData);
        return valid;
    }

    private static bool VerifyResearchProgressPersistsUntilCompletion()
    {
        using ResearchScenarioWorld world = new ResearchScenarioWorld();
        BlueprintResearchRuntime runtime = world.CreateResearchRuntime();
        FacilityBlueprintSO blueprint = LoadBlueprint("BP_DefenseBasics");
        BuildableObject lab = world.Place("P1_ResearchLab", new Vector2Int(2, 0));
        CharacterActor researcher = world.CreateCharacter("Species_Vampire", "Trait_Researcher");

        runtime.EnqueueBlueprint(blueprint);
        BlueprintResearchWorkResult first = runtime.ApplyResearchWork(CharacterActor.From(researcher), lab, 0.5f);
        BlueprintResearchWorkResult second = runtime.ApplyResearchWork(CharacterActor.From(researcher), lab, 999f);

        return first.Success
            && first.AddedProgress > 0f
            && !first.Completed
            && runtime.State.Tasks[0].Progress > 0f
            && second.Success
            && second.Completed
            && runtime.State.IsCompleted(blueprint);
    }

    private static bool VerifyCompletionUnlocksBasicPurchase()
    {
        using ResearchScenarioWorld world = new ResearchScenarioWorld();
        BlueprintResearchRuntime runtime = world.CreateResearchRuntime();
        FacilityBlueprintSO blueprint = LoadBlueprint("BP_DefenseBasics");
        BuildableObject lab = world.Place("P1_ResearchLab", new Vector2Int(2, 0));
        CharacterActor researcher = world.CreateCharacter("Species_Vampire", "Trait_Researcher");

        runtime.EnqueueBlueprint(blueprint);
        BlueprintResearchWorkResult result = runtime.ApplyResearchWork(CharacterActor.From(researcher), lab, 999f);
        IReadOnlyList<FacilityShopOffer> offers = FacilityShopService.CreateBasicPurchaseOffers(
            new[] { LoadBuilding("P1_SpikeTrap"), LoadBuilding("P1_GuardRoom") },
            runtime.ShopUnlockState,
            Array.Empty<int>(),
            DefaultBuildingCostMultiplier);

        return result.Completed
            && offers.Any((offer) => offer.Building == LoadBuilding("P1_SpikeTrap"))
            && offers.Any((offer) => offer.Building == LoadBuilding("P1_GuardRoom"));
    }

    private static bool VerifyCompletionUnlocksRecipes()
    {
        using ResearchScenarioWorld world = new ResearchScenarioWorld();
        BlueprintResearchRuntime runtime = world.CreateResearchRuntime();
        FacilityBlueprintSO blueprint = LoadBlueprint("BP_BattleDining");
        BuildableObject lab = world.Place("P1_ResearchLab", new Vector2Int(2, 0));
        CharacterActor researcher = world.CreateCharacter("Species_Vampire", "Trait_Researcher");

        runtime.EnqueueBlueprint(blueprint);
        BlueprintResearchWorkResult result = runtime.ApplyResearchWork(CharacterActor.From(researcher), lab, 999f);

        return result.Completed
            && runtime.State.UnlockedRecipeIds.Contains("recipe_battlefield_dining_2");
    }

    private static bool VerifyResearchSpeedUsesCharacterAndFacilityModifiers()
    {
        using ResearchScenarioWorld world = new ResearchScenarioWorld();
        BuildableObject lab = world.Place("P1_ResearchLab", new Vector2Int(2, 0));
        CharacterActor fighter = world.CreateCharacter("Species_Orc", "Trait_Fighter");
        CharacterActor researcher = world.CreateCharacter("Species_Vampire", "Trait_Researcher");

        float fighterWork = BlueprintResearchService.CalculateResearchWork(CharacterActor.From(fighter), lab, 1f);
        float researcherWork = BlueprintResearchService.CalculateResearchWork(CharacterActor.From(researcher), lab, 1f);
        float labMultiplier = BlueprintResearchService.GetFacilityResearchMultiplier(lab);

        return researcherWork > fighterWork
            && labMultiplier > 1f;
    }

    private static bool VerifyResearchWorkCandidateRequiresQueuedBlueprint()
    {
        using ResearchScenarioWorld world = new ResearchScenarioWorld();
        BlueprintResearchRuntime runtime = world.CreateResearchRuntime();
        BuildableObject lab = world.Place("P1_ResearchLab", new Vector2Int(2, 0));
        CharacterActor researcher = world.CreateCharacter("Species_Vampire", "Trait_Researcher");
        AbilityWork work = researcher.GetAbility<AbilityWork>();
        GridPathSearchResult search = world.Grid.SearchPath(Vector2Int.zero);

        bool rejectedWithoutBlueprint = !work.TrySetPriorityWorkTarget(
            lab,
            FacilityWorkType.Research,
            search,
            out _);

        runtime.EnqueueBlueprint(LoadBlueprint("BP_SupportBasics"));

        bool acceptedWithBlueprint = work.TrySetPriorityWorkTarget(
            lab,
            FacilityWorkType.Research,
            search,
            out _);

        return rejectedWithoutBlueprint
            && acceptedWithBlueprint
            && work.PriorityWorkTarget == lab
            && work.PriorityWorkType == FacilityWorkType.Research;
    }

    private static FacilityBlueprintSO LoadBlueprint(string assetName)
    {
        return AssetDatabase.LoadAssetAtPath<FacilityBlueprintSO>($"Assets/Resources/SO/Blueprint/P1/{assetName}.asset");
    }

    private static float DefaultBuildingCostMultiplier(BuildingSO building)
    {
        return 1f;
    }

    private static BuildingSO LoadBuilding(string assetName)
    {
        return AssetDatabase.LoadAssetAtPath<BuildingSO>($"Assets/Resources/SO/Building/P1/{assetName}.asset");
    }

    private static GameData CreateGameData(int holdingMoney)
    {
        GameData gameData = ScriptableObject.CreateInstance<GameData>();
        gameData.holdingMoney = new Data<int>();
        gameData.holdingMoney.Initialize(holdingMoney);
        return gameData;
    }

    private sealed class ResearchScenarioWorld : IDisposable
    {
        private static readonly FieldInfo GridSystemInstanceField =
            typeof(GridSystemManager).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly FieldInfo GridField =
            typeof(GridSystemManager).GetField("<grid>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo CharacterAwakeMethod =
            typeof(CharacterActor).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly GridSystemManager previousGridSystem;
        private readonly List<GameObject> objects = new List<GameObject>();
        private readonly List<ScriptableObject> scriptableObjects = new List<ScriptableObject>();

        public ResearchScenarioWorld()
        {
            previousGridSystem = GridSystemInstanceField?.GetValue(null) as GridSystemManager;
            Grid = new Grid(12, 1);
            for (int x = 0; x < Grid.width; x++)
            {
                Grid.RegisterOccupant(
                    new TestHallwayOccupant(),
                    GridLayer.Hallway,
                    new List<Vector2Int> { new Vector2Int(x, 0) },
                    false);
            }

            GameObject gridObject = new GameObject("Blueprint Research Scenario GridSystemManager");
            objects.Add(gridObject);
            GridSystemManager manager = gridObject.AddComponent<GridSystemManager>();
            GridField?.SetValue(manager, Grid);
            GridSystemInstanceField?.SetValue(null, manager);
        }

        public Grid Grid { get; }

        public BlueprintResearchRuntime CreateResearchRuntime()
        {
            GameObject obj = new GameObject("BlueprintResearchRuntime_Test");
            objects.Add(obj);
            obj.AddComponent<DailyFacilityShopRuntime>();
            BlueprintResearchRuntime runtime = obj.AddComponent<BlueprintResearchRuntime>();
            runtime.EventStartListening<FacilityShopPurchasedEvent>();
            return runtime;
        }

        public BuildableObject Place(string assetName, Vector2Int position)
        {
            BuildingSO buildingData = LoadBuilding(assetName);
            if (buildingData == null)
            {
                throw new InvalidOperationException($"{assetName} asset not found.");
            }

            GridBuildingFactory factory = new GridBuildingFactory();
            BuildableObject building = factory.Create(Grid, buildingData, position);
            if (building == null)
            {
                throw new InvalidOperationException($"{assetName} could not be created.");
            }

            objects.Add(building.gameObject);
            building.SetGrid(Grid);
            building.Initialization(buildingData, position);
            bool registered = Grid.RegisterOccupant(
                building,
                buildingData.Placement.Layer,
                buildingData.GetGridPosList(position),
                buildingData.Placement.IsMovement);
            if (!registered)
            {
                throw new InvalidOperationException($"{assetName} could not be registered.");
            }

            return building;
        }

        public CharacterActor CreateCharacter(string speciesAssetName, string traitAssetName)
        {
            GameObject obj = new GameObject($"Blueprint Research Character {speciesAssetName} {traitAssetName}");
            objects.Add(obj);
            obj.AddComponent<SpriteRenderer>();
            obj.AddComponent<AbilityMove>();
            obj.AddComponent<AbilityWork>();
            AIBrain brain = obj.AddComponent<AIBrain>();
            brain.availableActions = AiDebugScenarioActionFactory.CreateStaffActions();
            CharacterActor character = obj.AddComponent<CharacterActor>();
            CharacterAwakeMethod?.Invoke(character, null);

            CharacterSO data = ScriptableObject.CreateInstance<CharacterSO>();
            scriptableObjects.Add(data);
            data.characterType = CharacterType.NPC;
            data.characterName = "Research Scenario";
            data.species = AssetDatabase.LoadAssetAtPath<CharacterSpeciesSO>(
                $"Assets/Resources/SO/Character/Species/{speciesAssetName}.asset");
            data.speciesTag = data.species != null ? data.species.speciesTag : string.Empty;
            data.traits = new[]
            {
                AssetDatabase.LoadAssetAtPath<CharacterTraitSO>(
                    $"Assets/Resources/SO/Character/Traits/{traitAssetName}.asset")
            }.Where((trait) => trait != null).ToArray();
            data.baseStats = CharacterStatBlock.CreateDefault();
            data.defaultWorkPriorities = WorkPriorityProfile.CreateDefault();
            character.Initialization(data);
            character.stats[CharacterCondition.SLEEP] = 100f;
            character.stats[CharacterCondition.MOOD] = 100f;
            return character;
        }

        public void Dispose()
        {
            foreach (GameObject obj in objects.Where((obj) => obj != null))
            {
                Object.DestroyImmediate(obj);
            }

            foreach (ScriptableObject scriptableObject in scriptableObjects.Where((obj) => obj != null))
            {
                Object.DestroyImmediate(scriptableObject);
            }

            GridSystemInstanceField?.SetValue(null, previousGridSystem);
        }
    }

    private sealed class TestHallwayOccupant : IGridOccupant
    {
        public int GridId => 0;
        public bool IsGridDestroyed => false;
        public bool IsGridVisitable => false;
        public bool IsGridMovement => true;
    }
}
