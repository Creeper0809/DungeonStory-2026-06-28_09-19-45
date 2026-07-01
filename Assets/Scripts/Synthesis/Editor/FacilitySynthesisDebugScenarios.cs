using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class FacilitySynthesisDebugScenarios
{
    [MenuItem("DungeonStory/Debug/Synthesis/Run P1 Facility Synthesis Scenarios")]
    public static void RunFromMenu()
    {
        bool success = RunAll(true);
        if (!success)
        {
            Debug.LogError("P1 facility synthesis scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        P1FacilitySynthesisAssetBuilder.EnsureP1SynthesisAssets();

        List<string> errors = new List<string>();
        RunScenario("합성 에셋 생성", VerifySynthesisAssets, errors);
        RunScenario("대표 합성 트리 에셋", VerifyRepresentativeTreeAssets, errors);
        RunScenario("공개/특수 조합식 가시성", VerifyRecipeVisibility, errors);
        RunScenario("배치 시설 합성", VerifyPlacedFacilitiesAreConsumedAndReplaced, errors);
        RunScenario("3성 식당 합성 트리", VerifyThreeStarRestaurantSynthesis, errors);
        RunScenario("3성 함정/경비 합성 트리", VerifyThreeStarDefenseAndGuardTrees, errors);
        RunScenario("레벨 계승", VerifyLevelInheritance, errors);
        RunScenario("파손 재료 거부", VerifyDamagedMaterialRejected, errors);
        RunScenario("특수 조합식 연구 해금", VerifySpecialRecipeRequiresResearch, errors);
        RunScenario("3성 특수 조합식 연구 해금", VerifyThreeStarSpecialRecipeRequiresResearch, errors);
        RunScenario("합성 UI 렌더", VerifySynthesisPanelRendering, errors);

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
            Debug.Log("P1 facility synthesis scenarios passed.");
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

    private static bool VerifySynthesisAssets()
    {
        IReadOnlyList<FacilitySynthesisRecipeSO> recipes = FacilitySynthesisService.LoadAllRecipes();
        BuildingSO battleDining = LoadBuilding("P1_BattleDining");
        BuildingSO premium = LoadBuilding("P1_PremiumMeatRestaurant");
        BuildingSO venom = LoadBuilding("P1_VenomSpikeTrap");
        BuildingSO alarm = LoadBuilding("P1_AlarmCoil");

        return recipes.Count >= 10
            && battleDining != null
            && battleDining.Facility.SupportsRole(FacilityRole.Training)
            && premium != null
            && premium.Facility.SupportsRole(FacilityRole.Rest)
            && venom != null
            && venom.Defense.IsDefenseFacility
            && venom.Defense.star == 2
            && alarm != null
            && alarm.Defense.concept == DefenseAttackConcept.Lightning;
    }

    private static bool VerifyRepresentativeTreeAssets()
    {
        BuildingSO battlefieldDining = LoadBuilding("P1_BattlefieldDining");
        BuildingSO nobleDining = LoadBuilding("P1_NobleDining");
        BuildingSO corrosionFreezer = LoadBuilding("P1_CorrosionFreezer");
        BuildingSO stormFireTrap = LoadBuilding("P1_StormFireTrap");
        BuildingSO warBarracks = LoadBuilding("P1_WarBarracks");
        IReadOnlyList<FacilitySynthesisRecipeSO> recipes = FacilitySynthesisService.LoadAllRecipes();

        return battlefieldDining != null
            && battlefieldDining.Facility.SupportsRole(FacilityRole.Meal)
            && battlefieldDining.Facility.SupportsRole(FacilityRole.Training)
            && nobleDining != null
            && nobleDining.Facility.SupportsRole(FacilityRole.Mana)
            && corrosionFreezer != null
            && corrosionFreezer.Defense.IsDefenseFacility
            && corrosionFreezer.Defense.star == 3
            && corrosionFreezer.Defense.effects.Any((effect) => effect.kind == DefenseEffectKind.Slow)
            && stormFireTrap != null
            && stormFireTrap.Defense.IsDefenseFacility
            && stormFireTrap.Defense.star == 3
            && stormFireTrap.Defense.effects.Any((effect) => effect.kind == DefenseEffectKind.Charge)
            && warBarracks != null
            && warBarracks.Defense.IsDefenseFacility
            && warBarracks.Defense.concept == DefenseAttackConcept.Guard
            && recipes.Any((recipe) => recipe.recipeId == "recipe_battlefield_dining_2")
            && recipes.Any((recipe) => recipe.recipeId == "recipe_noble_dining_2")
            && recipes.Any((recipe) => recipe.recipeId == "recipe_corrosion_freezer_2")
            && recipes.Any((recipe) => recipe.recipeId == "recipe_war_barracks_2")
            && recipes.Any((recipe) => recipe.recipeId == "recipe_storm_fire_3");
    }

    private static bool VerifyRecipeVisibility()
    {
        IReadOnlyList<FacilitySynthesisRecipeSO> publicRecipes = FacilitySynthesisService.GetVisibleRecipes(null);
        FacilitySynthesisRecipeSO alarm = LoadRecipe("RS_AlarmCoil");
        BlueprintResearchState researchState = new BlueprintResearchState();

        bool hiddenBeforeResearch = !FacilitySynthesisService.IsRecipeVisible(alarm, researchState);
        researchState.UnlockRecipe("recipe_trap_chain_2");
        bool visibleAfterResearch = FacilitySynthesisService.IsRecipeVisible(alarm, researchState);

        return publicRecipes.Any((recipe) => recipe.recipeId == "recipe_battle_dining_1")
            && publicRecipes.Any((recipe) => recipe.recipeId == "recipe_premium_meat_1")
            && publicRecipes.Any((recipe) => recipe.recipeId == "recipe_battlefield_dining_2")
            && publicRecipes.Any((recipe) => recipe.recipeId == "recipe_noble_dining_2")
            && publicRecipes.Any((recipe) => recipe.recipeId == "recipe_venom_spike_1")
            && publicRecipes.Any((recipe) => recipe.recipeId == "recipe_corrosion_freezer_2")
            && publicRecipes.Any((recipe) => recipe.recipeId == "recipe_war_barracks_2")
            && publicRecipes.All((recipe) => recipe.recipeId != "recipe_alarm_coil_2")
            && hiddenBeforeResearch
            && visibleAfterResearch;
    }

    private static bool VerifyPlacedFacilitiesAreConsumedAndReplaced()
    {
        using SynthesisScenarioWorld world = new SynthesisScenarioWorld();
        FacilitySynthesisRuntime runtime = world.CreateRuntime();
        BuildableObject meat = world.Place("P1_MeatRestaurant", new Vector2Int(3, 0));
        BuildableObject training = world.Place("P1_TrainingRoom", new Vector2Int(8, 0));
        FacilitySynthesisRecipeSO recipe = LoadRecipe("RS_BattleDining");

        bool success = runtime.TrySynthesize(
            recipe,
            new[] { meat, training },
            out FacilitySynthesisResult result);

        BuildableObject resultBuilding = world.Grid.GetGridCell(new Vector2Int(3, 0)).GetBuilding();
        return success
            && result.Success
            && resultBuilding == result.ResultBuilding
            && resultBuilding.id == LoadBuilding("P1_BattleDining").id
            && meat.isDestroy
            && training.isDestroy
            && resultBuilding.Facility.SupportsRole(FacilityRole.Training);
    }

    private static bool VerifyThreeStarRestaurantSynthesis()
    {
        using SynthesisScenarioWorld world = new SynthesisScenarioWorld();
        FacilitySynthesisRuntime runtime = world.CreateRuntime();
        BuildableObject battleDining = world.Place("P1_BattleDining", new Vector2Int(3, 0));
        BuildableObject barracks = world.Place("P1_Barracks", new Vector2Int(9, 0));
        BuildableObject premium = world.Place("P1_PremiumMeatRestaurant", new Vector2Int(15, 0));
        BuildableObject manaStorage = world.Place("P1_ManaStorage", new Vector2Int(21, 0));

        bool battleBranch = runtime.TrySynthesize(
            LoadRecipe("RS_BattlefieldDining"),
            new[] { battleDining, barracks },
            out FacilitySynthesisResult battleResult);
        bool nobleBranch = runtime.TrySynthesize(
            LoadRecipe("RS_NobleDining"),
            new[] { premium, manaStorage },
            out FacilitySynthesisResult nobleResult);

        return battleBranch
            && nobleBranch
            && battleResult.ResultBuilding != null
            && battleResult.ResultBuilding.id == LoadBuilding("P1_BattlefieldDining").id
            && battleResult.ResultBuilding.Facility.SupportsRole(FacilityRole.Training)
            && nobleResult.ResultBuilding != null
            && nobleResult.ResultBuilding.id == LoadBuilding("P1_NobleDining").id
            && nobleResult.ResultBuilding.Facility.SupportsRole(FacilityRole.Mana);
    }

    private static bool VerifyThreeStarDefenseAndGuardTrees()
    {
        using SynthesisScenarioWorld world = new SynthesisScenarioWorld();
        FacilitySynthesisRuntime runtime = world.CreateRuntime();
        BuildableObject venom = world.Place("P1_VenomSpikeTrap", new Vector2Int(3, 0));
        BuildableObject ice = world.Place("P1_IceVent", new Vector2Int(8, 0));
        BuildableObject barracks = world.Place("P1_Barracks", new Vector2Int(13, 0));
        BuildableObject weaponShop = world.Place("P1_WeaponShop", new Vector2Int(19, 0));

        bool trapBranch = runtime.TrySynthesize(
            LoadRecipe("RS_CorrosionFreezer"),
            new[] { venom, ice },
            out FacilitySynthesisResult trapResult);
        bool guardBranch = runtime.TrySynthesize(
            LoadRecipe("RS_WarBarracks"),
            new[] { barracks, weaponShop },
            out FacilitySynthesisResult guardResult);

        return trapBranch
            && guardBranch
            && trapResult.ResultBuilding != null
            && trapResult.ResultBuilding.id == LoadBuilding("P1_CorrosionFreezer").id
            && trapResult.ResultBuilding.BuildingData.Defense.star == 3
            && trapResult.ResultBuilding.BuildingData.Defense.effects.Any((effect) => effect.kind == DefenseEffectKind.Corrosion)
            && trapResult.ResultBuilding.BuildingData.Defense.effects.Any((effect) => effect.kind == DefenseEffectKind.Slow)
            && guardResult.ResultBuilding != null
            && guardResult.ResultBuilding.id == LoadBuilding("P1_WarBarracks").id
            && guardResult.ResultBuilding.BuildingData.Defense.star == 3
            && guardResult.ResultBuilding.BuildingData.Defense.concept == DefenseAttackConcept.Guard;
    }

    private static bool VerifyLevelInheritance()
    {
        using SynthesisScenarioWorld world = new SynthesisScenarioWorld();
        FacilitySynthesisRuntime runtime = world.CreateRuntime();
        BuildableObject meat = world.Place("P1_MeatRestaurant", new Vector2Int(3, 0));
        BuildableObject rest = world.Place("P1_RestRoom", new Vector2Int(8, 0));
        meat.SetFacilityLevel(4);
        rest.SetFacilityLevel(2);

        bool success = runtime.TrySynthesize(
            LoadRecipe("RS_PremiumMeatRestaurant"),
            new[] { meat, rest },
            out FacilitySynthesisResult result);

        return success
            && result.InheritedLevel == 2
            && result.ResultBuilding != null
            && result.ResultBuilding.FacilityLevel == 2;
    }

    private static bool VerifyDamagedMaterialRejected()
    {
        using SynthesisScenarioWorld world = new SynthesisScenarioWorld();
        FacilitySynthesisRuntime runtime = world.CreateRuntime();
        BuildableObject spike = world.Place("P1_SpikeTrap", new Vector2Int(2, 0));
        BuildableObject poison = world.Place("P1_PoisonPool", new Vector2Int(6, 0));
        poison.SetDamaged(true);

        bool rejected = !runtime.TrySynthesize(
            LoadRecipe("RS_VenomSpikeTrap"),
            new[] { spike, poison },
            out FacilitySynthesisResult result);

        return rejected
            && result.Message.Contains("파손")
            && !spike.isDestroy
            && !poison.isDestroy;
    }

    private static bool VerifySpecialRecipeRequiresResearch()
    {
        using SynthesisScenarioWorld world = new SynthesisScenarioWorld();
        FacilitySynthesisRuntime runtime = world.CreateRuntime();
        BlueprintResearchRuntime researchRuntime = world.CreateResearchRuntime();
        BuildableObject lightning = world.Place("P1_LightningPillar", new Vector2Int(2, 0));
        BuildableObject guard = world.Place("P1_GuardRoom", new Vector2Int(6, 0));
        FacilitySynthesisRecipeSO recipe = LoadRecipe("RS_AlarmCoil");

        bool rejectedBeforeResearch = !runtime.TrySynthesize(
            recipe,
            new[] { lightning, guard },
            out FacilitySynthesisResult beforeResult);

        researchRuntime.State.UnlockRecipe("recipe_trap_chain_2");
        bool acceptedAfterResearch = runtime.TrySynthesize(
            recipe,
            new[] { lightning, guard },
            out FacilitySynthesisResult afterResult);

        return rejectedBeforeResearch
            && beforeResult.Message.Contains("해금")
            && acceptedAfterResearch
            && afterResult.ResultBuilding != null
            && afterResult.ResultBuilding.id == LoadBuilding("P1_AlarmCoil").id;
    }

    private static bool VerifyThreeStarSpecialRecipeRequiresResearch()
    {
        using SynthesisScenarioWorld world = new SynthesisScenarioWorld();
        FacilitySynthesisRuntime runtime = world.CreateRuntime();
        BlueprintResearchRuntime researchRuntime = world.CreateResearchRuntime();
        BuildableObject alarm = world.Place("P1_AlarmCoil", new Vector2Int(3, 0));
        BuildableObject fire = world.Place("P1_FireVent", new Vector2Int(8, 0));
        FacilitySynthesisRecipeSO recipe = LoadRecipe("RS_StormFireTrap");

        bool rejectedBeforeResearch = !runtime.TrySynthesize(
            recipe,
            new[] { alarm, fire },
            out FacilitySynthesisResult beforeResult);

        researchRuntime.State.UnlockRecipe("recipe_trap_chain_3");
        bool acceptedAfterResearch = runtime.TrySynthesize(
            recipe,
            new[] { alarm, fire },
            out FacilitySynthesisResult afterResult);

        return rejectedBeforeResearch
            && beforeResult.Message.Contains("해금")
            && acceptedAfterResearch
            && afterResult.ResultBuilding != null
            && afterResult.ResultBuilding.id == LoadBuilding("P1_StormFireTrap").id
            && afterResult.ResultBuilding.BuildingData.Defense.effects.Any((effect) => effect.kind == DefenseEffectKind.Burn)
            && afterResult.ResultBuilding.BuildingData.Defense.effects.Any((effect) => effect.kind == DefenseEffectKind.Charge);
    }

    private static bool VerifySynthesisPanelRendering()
    {
        using SynthesisScenarioWorld world = new SynthesisScenarioWorld();
        FacilitySynthesisRuntime runtime = world.CreateRuntime();
        BuildableObject meat = world.Place("P1_MeatRestaurant", new Vector2Int(3, 0));
        BuildableObject training = world.Place("P1_TrainingRoom", new Vector2Int(8, 0));
        runtime.ToggleMaterialSelection(meat);
        runtime.ToggleMaterialSelection(training);

        FacilitySynthesisPanel panel = FacilitySynthesisPanel.CreateDefaultPanel(runtime);
        world.TrackObject(panel.transform.root.gameObject);

        return panel.LastRenderedText.Contains("시설 합성")
            && panel.LastRenderedText.Contains("고기 식당")
            && panel.LastRenderedText.Contains("전투 식당");
    }

    private static BuildingSO LoadBuilding(string assetName)
    {
        return AssetDatabase.LoadAssetAtPath<BuildingSO>($"Assets/Resources/SO/Building/P1/{assetName}.asset");
    }

    private static FacilitySynthesisRecipeSO LoadRecipe(string assetName)
    {
        return AssetDatabase.LoadAssetAtPath<FacilitySynthesisRecipeSO>($"Assets/Resources/SO/Synthesis/P1/{assetName}.asset");
    }

    private sealed class SynthesisScenarioWorld : IDisposable
    {
        private static readonly FieldInfo GridSystemInstanceField =
            typeof(GridSystemManager).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly FieldInfo GridField =
            typeof(GridSystemManager).GetField("<grid>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly GridSystemManager previousGridSystem;
        private readonly List<GameObject> objects = new List<GameObject>();

        public SynthesisScenarioWorld()
        {
            previousGridSystem = GridSystemInstanceField?.GetValue(null) as GridSystemManager;
            Grid = new Grid(24, 1);
            for (int x = 0; x < Grid.width; x++)
            {
                Grid.RegisterOccupant(
                    new TestHallwayOccupant(),
                    GridLayer.Hallway,
                    new List<Vector2Int> { new Vector2Int(x, 0) },
                    false);
            }

            GameObject gridObject = new GameObject("Synthesis Scenario GridSystemManager");
            objects.Add(gridObject);
            GridSystemManager manager = gridObject.AddComponent<GridSystemManager>();
            GridField?.SetValue(manager, Grid);
            GridSystemInstanceField?.SetValue(null, manager);
        }

        public Grid Grid { get; }

        public FacilitySynthesisRuntime CreateRuntime()
        {
            GameObject obj = new GameObject("FacilitySynthesisRuntime_Test");
            objects.Add(obj);
            return obj.AddComponent<FacilitySynthesisRuntime>();
        }

        public BlueprintResearchRuntime CreateResearchRuntime()
        {
            GameObject obj = new GameObject("BlueprintResearchRuntime_Synthesis_Test");
            objects.Add(obj);
            return obj.AddComponent<BlueprintResearchRuntime>();
        }

        public BuildableObject Place(string assetName, Vector2Int position)
        {
            BuildingSO buildingData = LoadBuilding(assetName);
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

        public void TrackObject(GameObject obj)
        {
            if (obj != null && !objects.Contains(obj))
            {
                objects.Add(obj);
            }
        }

        public void Dispose()
        {
            foreach (GameObject obj in objects.Where((obj) => obj != null))
            {
                Object.DestroyImmediate(obj);
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
