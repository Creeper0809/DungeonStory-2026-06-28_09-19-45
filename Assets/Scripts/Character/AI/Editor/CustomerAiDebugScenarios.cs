using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class CustomerAiDebugScenarios
{
    [MenuItem("DungeonStory/Debug/AI/Run P1 Customer AI Scenarios")]
    public static void RunFromMenu()
    {
        bool success = RunAll(true);
        if (!success)
        {
            Debug.LogError("P1 customer AI scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        List<string> errors = new List<string>();
        RunScenario("Action score compensation", VerifyActionScoreCompensation, errors);
        RunScenario("?ㅽ뻾 媛?ν븳 紐⑹쟻吏 ?녿뒗 ?됰룞? ?꾨낫 ?쒖쇅", VerifyUnavailableDestinationActionsReportNoAction, errors);
        RunScenario("怨좎젏 ?뺢뎄 ?됰룞 紐⑹쟻吏 ?놁쓬 substitute", VerifyUnavailableHighScoreNeedSelectsReachableAction, errors);

        RunScenario("??븷蹂??꾨낫 ?섏쭛", VerifyRoleCandidateFiltering, errors);
        RunScenario("?뺢뎄 湲곕컲 ?됰룞 ?먯닔", VerifyNeedScoresDriveActionPriority, errors);
        RunScenario("醫낆” ?좏샇媛 嫄곕━蹂대떎 ?곗꽑", VerifySpeciesPreferenceCanBeatDistance, errors);
        RunScenario("諭?뚯씠???λ? ?쒖꽕 ?좏깮", VerifyVampireSelectsManaOrResearch, errors);
        RunScenario("?꾨낫 ?쒓굅 議곌굔", VerifyUnavailableFacilitiesAreExcluded, errors);
        RunScenario("돈 부족 상점만 남으면 방문 종료", VerifyUnaffordableShopEndsVisitCycle, errors);
        RunScenario("방문객 필수 AI 행동 자동 보강", VerifyVisitorActionsAreCompletedFromPartialPrefab, errors);
        RunScenario("?щ갑臾??쇱옟 ?먯닔 蹂댁젙", VerifyNoveltyAndCrowdScores, errors);

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
            Debug.Log("P1 customer AI scenarios passed.");
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

    private static bool VerifyRoleCandidateFiltering()
    {
        using CustomerAiScenarioWorld world = new CustomerAiScenarioWorld();
        BuildableObject lowFood = world.Place("P1_LowFoodShop", new Vector2Int(3, 0));
        BuildableObject meat = world.Place("P1_MeatRestaurant", new Vector2Int(9, 0));
        BuildableObject general = world.Place("P1_GeneralStore", new Vector2Int(15, 0));
        BuildableObject weapon = world.Place("P1_WeaponShop", new Vector2Int(21, 0));
        BuildableObject rest = world.Place("P1_RestRoom", new Vector2Int(27, 0));
        BuildableObject lab = world.Place("P1_ResearchLab", new Vector2Int(33, 0));
        Character slime = world.CreateCustomer("Slime", Vector2Int.zero, 20f, 70f, 20f, 70f);
        GridPathSearchResult searchResult = world.Grid.SearchPath(Vector2Int.zero);

        List<BuildableObject> mealCandidates = FacilityCandidateScorer.GetCandidates(slime, searchResult, FacilityRole.Meal);
        List<BuildableObject> interestCandidates = FacilityCandidateScorer.GetCandidates(slime, searchResult, AIShopping.CustomerInterestRoles);
        List<BuildableObject> restCandidates = FacilityCandidateScorer.GetCandidates(slime, searchResult, FacilityRole.Rest);

        return mealCandidates.Contains(lowFood)
            && mealCandidates.Contains(meat)
            && !mealCandidates.Contains(general)
            && !mealCandidates.Contains(rest)
            && interestCandidates.Contains(general)
            && interestCandidates.Contains(weapon)
            && interestCandidates.Contains(lab)
            && !interestCandidates.Contains(lowFood)
            && restCandidates.SequenceEqual(new[] { rest });
    }

    private static bool VerifyActionScoreCompensation()
    {
        AIWait actionSet = ScriptableObject.CreateInstance<AIWait>();
        FixedScoreConsideration highA = ScriptableObject.CreateInstance<FixedScoreConsideration>();
        FixedScoreConsideration highB = ScriptableObject.CreateInstance<FixedScoreConsideration>();
        FixedScoreConsideration overMax = ScriptableObject.CreateInstance<FixedScoreConsideration>();
        FixedScoreConsideration half = ScriptableObject.CreateInstance<FixedScoreConsideration>();
        FixedScoreConsideration midA = ScriptableObject.CreateInstance<FixedScoreConsideration>();
        FixedScoreConsideration midB = ScriptableObject.CreateInstance<FixedScoreConsideration>();
        FixedScoreConsideration midC = ScriptableObject.CreateInstance<FixedScoreConsideration>();

        try
        {
            highA.FixedScore = 0.9f;
            highB.FixedScore = 0.9f;
            SetConsiderations(actionSet, highA, highB);
            float highScore = new AIAction { actionset = actionSet }.CalculateScore(null);
            bool highScoreUsesWeightedScore =
                NearlyEqual(highScore, ExpectedWeightedScore(0.9f, 0.9f))
                && highScore < 0.99f;

            overMax.FixedScore = 2f;
            half.FixedScore = 0.5f;
            SetConsiderations(actionSet, overMax, half);
            float clampedScore = new AIAction { actionset = actionSet }.CalculateScore(null);
            bool clampsConsiderationScores =
                NearlyEqual(clampedScore, ExpectedWeightedScore(1f, 0.5f));

            midA.FixedScore = 0.5f;
            midB.FixedScore = 0.5f;
            midC.FixedScore = 0.5f;
            SetConsiderations(actionSet, midA, midB, midC);
            float threeScore = new AIAction { actionset = actionSet }.CalculateScore(null);
            bool countDoesNotInflateScore =
                NearlyEqual(threeScore, ExpectedWeightedScore(0.5f, 0.5f, 0.5f));

            return highScoreUsesWeightedScore
                && clampsConsiderationScores
                && countDoesNotInflateScore;
        }
        finally
        {
            Object.DestroyImmediate(actionSet);
            Object.DestroyImmediate(highA);
            Object.DestroyImmediate(highB);
            Object.DestroyImmediate(overMax);
            Object.DestroyImmediate(half);
            Object.DestroyImmediate(midA);
            Object.DestroyImmediate(midB);
            Object.DestroyImmediate(midC);
        }
    }

    private static bool VerifyNeedScoresDriveActionPriority()
    {
        using CustomerAiScenarioWorld world = new CustomerAiScenarioWorld();
        world.Place("P1_LowFoodShop", new Vector2Int(4, 0));
        world.Place("P1_GeneralStore", new Vector2Int(10, 0));
        world.Place("P1_RestRoom", new Vector2Int(16, 0));
        Character customer = world.CreateCustomer("Slime", Vector2Int.zero, 10f, 90f, 90f, 90f);

        ConsiderationFacilityNeed mealNeed = CreateNeed(FacilityRole.Meal);
        ConsiderationFacilityNeed interestNeed = CreateNeed(AIShopping.CustomerInterestRoles);
        ConsiderationFacilityNeed restNeed = CreateNeed(FacilityRole.Rest);

        bool hungryPrefersMeal = mealNeed.ScoreConsideration(customer) > interestNeed.ScoreConsideration(customer)
            && mealNeed.ScoreConsideration(customer) > restNeed.ScoreConsideration(customer);

        SetStats(customer, 90f, 90f, 10f, 20f);
        bool boredPrefersInterest = interestNeed.ScoreConsideration(customer) > mealNeed.ScoreConsideration(customer)
            && interestNeed.ScoreConsideration(customer) > restNeed.ScoreConsideration(customer);

        SetStats(customer, 90f, 10f, 90f, 20f);
        bool tiredPrefersRest = restNeed.ScoreConsideration(customer) > mealNeed.ScoreConsideration(customer)
            && restNeed.ScoreConsideration(customer) > interestNeed.ScoreConsideration(customer);

        Object.DestroyImmediate(mealNeed);
        Object.DestroyImmediate(interestNeed);
        Object.DestroyImmediate(restNeed);
        return hungryPrefersMeal && boredPrefersInterest && tiredPrefersRest;
    }

    private static bool VerifySpeciesPreferenceCanBeatDistance()
    {
        using CustomerAiScenarioWorld world = new CustomerAiScenarioWorld();
        BuildableObject general = world.Place("P1_GeneralStore", new Vector2Int(3, 0));
        BuildableObject weapon = world.Place("P1_WeaponShop", new Vector2Int(18, 0));
        GridPathSearchResult searchResult = world.Grid.SearchPath(Vector2Int.zero);

        Character orc = world.CreateCustomer("Orc", Vector2Int.zero, 90f, 90f, 10f, 30f);
        BuildableObject orcBest = SelectBest(orc, searchResult, AIShopping.CustomerInterestRoles);

        Character slime = world.CreateCustomer("Slime", Vector2Int.zero, 90f, 90f, 10f, 30f);
        BuildableObject slimeBest = SelectBest(slime, searchResult, AIShopping.CustomerInterestRoles);

        return orcBest == weapon && slimeBest == general;
    }

    private static bool VerifyVampireSelectsManaOrResearch()
    {
        using CustomerAiScenarioWorld world = new CustomerAiScenarioWorld();
        BuildableObject general = world.Place("P1_GeneralStore", new Vector2Int(3, 0));
        BuildableObject lab = world.Place("P1_ResearchLab", new Vector2Int(16, 0));
        BuildableObject mana = world.Place("P1_ManaStorage", new Vector2Int(24, 0));
        GridPathSearchResult searchResult = world.Grid.SearchPath(Vector2Int.zero);

        Character vampire = world.CreateCustomer("Vampire", Vector2Int.zero, 90f, 90f, 10f, 10f);
        BuildableObject best = SelectBest(vampire, searchResult, AIShopping.CustomerInterestRoles);

        return best != general && (best == lab || best == mana);
    }

    private static bool VerifyUnavailableFacilitiesAreExcluded()
    {
        using CustomerAiScenarioWorld world = new CustomerAiScenarioWorld();
        BuildableObject lowFood = world.Place("P1_LowFoodShop", new Vector2Int(4, 0));
        BuildableObject general = world.Place("P1_GeneralStore", new Vector2Int(10, 0));
        BuildableObject rest = world.Place("P1_RestRoom", new Vector2Int(16, 0));
        Character customer = world.CreateCustomer("Slime", Vector2Int.zero, 10f, 20f, 10f, 20f);
        GridPathSearchResult searchResult = world.Grid.SearchPath(Vector2Int.zero);

        ClearShopStock(lowFood);
        rest.SetDamaged(true);
        SetHoldingMoney(customer, 0);

        List<BuildableObject> mealCandidates = FacilityCandidateScorer.GetCandidates(customer, searchResult, FacilityRole.Meal);
        List<BuildableObject> restCandidates = FacilityCandidateScorer.GetCandidates(customer, searchResult, FacilityRole.Rest);
        List<BuildableObject> interestCandidates = FacilityCandidateScorer.GetCandidates(customer, searchResult, AIShopping.CustomerInterestRoles);

        bool stockRejected = !mealCandidates.Contains(lowFood)
            && !lowFood.CanVisit(customer, out string stockReason)
            && stockReason == "재고 없음";
        bool damageRejected = !restCandidates.Contains(rest)
            && !rest.CanVisit(customer, out string damageReason)
            && damageReason == "시설 파손";
        bool moneyRejected = !interestCandidates.Contains(general)
            && customer.GetAbility<AbilityShopping>().CanBuyFrom((Shop)general, out string moneyReason) == false
            && moneyReason == "소지금 부족";

        return stockRejected && damageRejected && moneyRejected;
    }

    private static bool VerifyUnaffordableShopEndsVisitCycle()
    {
        using CustomerAiScenarioWorld world = new CustomerAiScenarioWorld();
        BuildableObject general = world.Place("P1_GeneralStore", new Vector2Int(4, 0));
        Character customer = world.CreateCustomer("Slime", Vector2Int.zero, 90f, 90f, 10f, 20f);
        AbilityShopping shopping = customer.GetAbility<AbilityShopping>();
        AILookAround lookAround = ScriptableObject.CreateInstance<AILookAround>();
        SetHoldingMoney(customer, 0);

        try
        {
            return general.CanVisit(customer, out _)
                && !shopping.CanBuyFrom((Shop)general, out _)
                && !shopping.IsThereVisitableBuilding()
                && shopping.ShouldEndVisitCycle()
                && shopping.CanLookAround()
                && lookAround.CanStartWithContext(customer, world.Grid.SearchPath(Vector2Int.zero), out _);
        }
        finally
        {
            Object.DestroyImmediate(lookAround);
        }
    }

    private static bool VerifyVisitorActionsAreCompletedFromPartialPrefab()
    {
        using CustomerAiScenarioWorld world = new CustomerAiScenarioWorld();
        Character customer = world.CreateCustomer("Slime", Vector2Int.zero, 90f, 90f, 10f, 20f);
        customer.ai.availableActions = new[]
        {
            new AIAction { actionset = Resources.Load<AIActionSet>("SO/AI/Action/Eat") },
            new AIAction { actionset = Resources.Load<AIActionSet>("SO/AI/Action/Shopping") },
            new AIAction { actionset = Resources.Load<AIActionSet>("SO/AI/Action/Rest") },
        };

        customer.ai.Initializtion(customer.data);
        Type[] actionTypes = customer.ai.availableActions
            .Where((action) => action != null && action.actionset != null)
            .Select((action) => action.actionset.GetType())
            .ToArray();

        return actionTypes.Contains(typeof(AIEat))
            && actionTypes.Contains(typeof(AIRest))
            && actionTypes.Contains(typeof(AIShopping))
            && actionTypes.Contains(typeof(AILookAround))
            && actionTypes.Contains(typeof(AIExitDungeon));
    }

    private static bool VerifyUnavailableDestinationActionsReportNoAction()
    {
        using CustomerAiScenarioWorld world = new CustomerAiScenarioWorld();
        Character customer = world.CreateCustomer("Slime", Vector2Int.zero, 5f, 5f, 5f, 5f);
        List<ScriptableObject> owned = new List<ScriptableObject>();

        try
        {
            AIEat eat = CreateScoredAction<AIEat>(owned, 1f);
            AIRest rest = CreateScoredAction<AIRest>(owned, 0.9f);
            AIShopping shopping = CreateScoredAction<AIShopping>(owned, 0.8f);
            customer.ai.availableActions = new[]
            {
                new AIAction { actionset = eat },
                new AIAction { actionset = rest },
                new AIAction { actionset = shopping }
            };

            GridPathSearchResult searchResult = customer.ai.GetPathSearch(customer);
            bool eatUnavailable = !eat.CanStartWithContext(customer, searchResult, out _);
            bool restUnavailable = !rest.CanStartWithContext(customer, searchResult, out _);
            bool shoppingUnavailable = !shopping.CanStartWithContext(customer, searchResult, out _);
            bool decided = customer.ai.DecideAction();

            return eatUnavailable
                && restUnavailable
                && shoppingUnavailable
                && decided
                && customer.ai.bestAction != null
                && customer.ai.bestAction.actionset is AILookAround
                && customer.ai.bestAction.planKind == AIActionPlanKind.NoDestination;
        }
        finally
        {
            DestroyScriptableObjects(owned);
        }
    }

    private static bool VerifyUnavailableHighScoreNeedSelectsReachableAction()
    {
        using CustomerAiScenarioWorld world = new CustomerAiScenarioWorld();
        BuildableObject restRoom = world.Place("P1_RestRoom", new Vector2Int(6, 0));
        Character customer = world.CreateCustomer("Slime", Vector2Int.zero, 5f, 5f, 90f, 90f);
        List<ScriptableObject> owned = new List<ScriptableObject>();

        try
        {
            AIEat eat = CreateScoredAction<AIEat>(owned, 1f);
            AIRest rest = CreateScoredAction<AIRest>(owned, 0.6f);
            AIWait wait = CreateScoredAction<AIWait>(owned, 0.1f);
            customer.ai.availableActions = new[]
            {
                new AIAction { actionset = eat },
                new AIAction { actionset = rest },
                new AIAction { actionset = wait }
            };

            bool decided = customer.ai.DecideAction();
            return decided
                && customer.ai.bestAction != null
                && customer.ai.bestAction.actionset == rest
                && customer.ai.bestAction.destination == restRoom
                && customer.ai.bestAction.planKind == AIActionPlanKind.MovePath;
        }
        finally
        {
            DestroyScriptableObjects(owned);
        }
    }

    private static bool VerifyNoveltyAndCrowdScores()
    {
        using CustomerAiScenarioWorld world = new CustomerAiScenarioWorld();
        BuildableObject general = world.Place("P1_GeneralStore", new Vector2Int(4, 0));
        BuildableObject rest = world.Place("P1_RestRoom", new Vector2Int(10, 0));
        Character customer = world.CreateCustomer("Slime", Vector2Int.zero, 90f, 90f, 10f, 10f);
        GridPathSearchResult searchResult = world.Grid.SearchPath(Vector2Int.zero);

        float freshScore = FacilityCandidateScorer.ScoreCandidate(
            customer,
            general,
            AIShopping.CustomerInterestRoles,
            searchResult);
        customer.GetAbility<AbilityShopping>().RegisterVisit(general);
        float revisitedScore = FacilityCandidateScorer.ScoreCandidate(
            customer,
            general,
            AIShopping.CustomerInterestRoles,
            searchResult);

        float emptyRestScore = FacilityCandidateScorer.ScoreCandidate(customer, rest, FacilityRole.Rest, searchResult);
        rest.TryBeginUse(customer, out _);
        rest.TryBeginUse(customer, out _);
        float crowdedRestScore = FacilityCandidateScorer.ScoreCandidate(customer, rest, FacilityRole.Rest, searchResult);

        return revisitedScore < freshScore && crowdedRestScore < emptyRestScore;
    }

    private static BuildableObject SelectBest(
        Character character,
        GridPathSearchResult searchResult,
        FacilityRole role)
    {
        List<BuildableObject> candidates = FacilityCandidateScorer.GetCandidates(character, searchResult, role);
        return FacilityCandidateScorer.SelectBest(character, candidates, role, searchResult);
    }

    private static ConsiderationFacilityNeed CreateNeed(FacilityRole role)
    {
        ConsiderationFacilityNeed need = ScriptableObject.CreateInstance<ConsiderationFacilityNeed>();
        need.Role = role;
        return need;
    }

    private static void SetStats(Character character, float hunger, float sleep, float fun, float mood)
    {
        character.stats[Character.Condition.HUNGER] = hunger;
        character.stats[Character.Condition.SLEEP] = sleep;
        character.stats[Character.Condition.FUN] = fun;
        character.stats[Character.Condition.MOOD] = mood;
    }

    private static void SetHoldingMoney(Character character, int money)
    {
        AbilityShopping shopping = character.GetAbility<AbilityShopping>();
        FieldInfo field = typeof(AbilityShopping).GetField("holdingMoney", BindingFlags.Instance | BindingFlags.NonPublic);
        field?.SetValue(shopping, money);
    }

    private static void ClearShopStock(BuildableObject building)
    {
        FieldInfo field = typeof(Shop).GetField("stocks", BindingFlags.Instance | BindingFlags.NonPublic);
        field?.SetValue(building, new List<RemainStock>());
    }

    private static void SetConsiderations(AIActionSet actionSet, params Consideration[] considerations)
    {
        FieldInfo field = typeof(AIActionSet).GetField(
            "<considerations>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field?.SetValue(actionSet, considerations);
    }

    private static T CreateScoredAction<T>(List<ScriptableObject> owned, float score)
        where T : AIActionSet
    {
        T action = ScriptableObject.CreateInstance<T>();
        FixedScoreConsideration consideration = ScriptableObject.CreateInstance<FixedScoreConsideration>();
        consideration.FixedScore = score;
        SetConsiderations(action, consideration);
        owned.Add(action);
        owned.Add(consideration);
        return action;
    }

    private static void DestroyScriptableObjects(List<ScriptableObject> objects)
    {
        foreach (ScriptableObject obj in objects)
        {
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }
    }

    private static float ExpectedWeightedScore(params float[] scores)
    {
        if (scores == null || scores.Length == 0)
        {
            return 1f;
        }

        float totalScore = 0f;
        foreach (float score in scores)
        {
            float clampedScore = Mathf.Clamp01(score);
            if (clampedScore <= 0f)
            {
                return 0f;
            }

            totalScore += clampedScore;
        }

        return Mathf.Clamp01(totalScore / scores.Length);
    }

    private static bool NearlyEqual(float a, float b)
    {
        return Mathf.Abs(a - b) <= 0.0001f;
    }

    private sealed class FixedScoreConsideration : Consideration
    {
        public float FixedScore { get; set; }

        public override float ScoreConsideration(Character character)
        {
            return FixedScore;
        }
    }

    private sealed class CustomerAiScenarioWorld : IDisposable
    {
        private static readonly FieldInfo GridSystemInstanceField =
            typeof(GridSystemManager).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly FieldInfo GridField =
            typeof(GridSystemManager).GetField("<grid>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo CharacterAwakeMethod =
            typeof(Character).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo CharacterAiSchedulerInstanceField =
            typeof(CharacterAiScheduler).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);

        private readonly GridSystemManager previousGridSystem;
        private readonly CharacterAiScheduler previousScheduler;
        private readonly List<GameObject> objects = new List<GameObject>();
        private readonly List<ScriptableObject> scriptableObjects = new List<ScriptableObject>();
        private readonly GameObject gridSystemObject;

        public CustomerAiScenarioWorld()
        {
            previousGridSystem = GridSystemInstanceField?.GetValue(null) as GridSystemManager;
            previousScheduler = CharacterAiSchedulerInstanceField?.GetValue(null) as CharacterAiScheduler;
            CharacterAiSchedulerInstanceField?.SetValue(null, null);
            Grid = new Grid(40, 1);
            for (int x = 0; x < Grid.width; x++)
            {
                Grid.RegisterOccupant(
                    new TestHallwayOccupant(),
                    GridLayer.Hallway,
                    new List<Vector2Int> { new Vector2Int(x, 0) },
                    false);
            }

            gridSystemObject = new GameObject("Customer AI Scenario GridSystemManager");
            objects.Add(gridSystemObject);
            GridSystemManager manager = gridSystemObject.AddComponent<GridSystemManager>();
            GridField?.SetValue(manager, Grid);
            GridSystemInstanceField?.SetValue(null, manager);
        }

        public Grid Grid { get; }

        public BuildableObject Place(string assetName, Vector2Int position)
        {
            BuildingSO buildingData = AssetDatabase.LoadAssetAtPath<BuildingSO>(
                $"Assets/Resources/SO/Building/P1/{assetName}.asset");
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

        public Character CreateCustomer(
            string speciesTag,
            Vector2Int position,
            float hunger,
            float sleep,
            float fun,
            float mood)
        {
            GameObject obj = new GameObject($"Customer AI Test {speciesTag}");
            objects.Add(obj);
            obj.AddComponent<SpriteRenderer>();
            obj.AddComponent<AbilityMove>();
            obj.AddComponent<AbilityShopping>();
            AIBrain brain = obj.AddComponent<AIBrain>();
            brain.availableActions = AiDebugScenarioActionFactory.CreateCustomerActions();
            Character character = obj.AddComponent<Character>();
            CharacterAwakeMethod?.Invoke(character, null);

            CharacterSO data = ScriptableObject.CreateInstance<CharacterSO>();
            scriptableObjects.Add(data);
            SetPrivateField(data, "frequencyVisitMin", 3);
            SetPrivateField(data, "frequencyVisitMax", 3);
            SetPrivateField(data, "minHoldingMoney", 500);
            SetPrivateField(data, "maxHoldingMoney", 600);
            data.characterType = CharacterType.Customer;
            data.characterName = speciesTag;
            data.speciesTag = speciesTag;

            obj.transform.position = Grid.GetWorldPos(position);
            character.Initialization(data);
            character.SetLifecycleState(Character.LifecycleState.Active);
            SetStats(character, hunger, sleep, fun, mood);
            return character;
        }

        public void Dispose()
        {
            GridSystemInstanceField?.SetValue(null, previousGridSystem);
            CharacterAiSchedulerInstanceField?.SetValue(null, previousScheduler);
            foreach (GameObject obj in objects.Where((obj) => obj != null))
            {
                Object.DestroyImmediate(obj);
            }

            foreach (ScriptableObject obj in scriptableObjects.Where((obj) => obj != null))
            {
                Object.DestroyImmediate(obj);
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(target, value);
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
