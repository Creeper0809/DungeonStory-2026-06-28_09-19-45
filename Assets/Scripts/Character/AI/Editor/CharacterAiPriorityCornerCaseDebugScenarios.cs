using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class CharacterAiPriorityCornerCaseDebugScenarios
{
    [MenuItem("DungeonStory/Debug/AI/Run Priority Corner Case Scenarios")]
    public static void RunFromMenu()
    {
        bool success = RunAll(true);
        if (!success)
        {
            Debug.LogError("Character AI priority corner case scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        List<string> errors = new List<string>();

        RunScenario("AI action score edge cases", VerifyActionScoreEdgeCases, errors);
        RunScenario("AI selects next action after failed high-score destination", VerifyBrainSelectsNextActionAfterDestinationFailure, errors);
        RunScenario("AI tie keeps action order", VerifyBrainTieKeepsActionOrder, errors);
        RunScenario("Off priority excludes urgent automatic work", VerifyOffPriorityExcludesUrgentAutomaticWork, errors);
        RunScenario("Direct command bypasses Off through assignment", VerifyDirectCommandBypassesOffThroughAssignment, errors);
        RunScenario("Requested work type does not substitute", VerifyRequestedWorkTypeDoesNotSubstitute, errors);
        RunScenario("Invalid priority target clears and resumes automatic work", VerifyInvalidPriorityTargetClearsAndResumesAutomaticWork, errors);
        RunScenario("Nearest equivalent work target wins", VerifyNearestEquivalentWorkTargetWins, errors);
        RunScenario("Priority level beats lower urgent work", VerifyPriorityLevelBeatsLowerUrgentWork, errors);
        RunScenario("Combined priority profile edges", VerifyCombinedPriorityProfileEdges, errors);
        RunScenario("AI personality modifies action score", VerifyPersonalityModifierAffectsActionScore, errors);
        RunScenario("Occupied work target is classified", VerifyOccupiedWorkTargetFailureClassification, errors);
        RunScenario("Work and wait scores prefer real work", VerifyWorkAndWaitScoresPreferRealWork, errors);
        RunScenario("No work target uses explicit wait", VerifyNoWorkTargetUsesExplicitWait, errors);

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
            Debug.Log("Character AI priority corner case scenarios passed.");
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

    private static bool VerifyActionScoreEdgeCases()
    {
        TestActionSet actionSet = ScriptableObject.CreateInstance<TestActionSet>();
        FixedScoreConsideration one = ScriptableObject.CreateInstance<FixedScoreConsideration>();
        FixedScoreConsideration zero = ScriptableObject.CreateInstance<FixedScoreConsideration>();
        FixedScoreConsideration overMax = ScriptableObject.CreateInstance<FixedScoreConsideration>();
        FixedScoreConsideration half = ScriptableObject.CreateInstance<FixedScoreConsideration>();

        try
        {
            AIAction missingSet = new AIAction();
            bool nullActionSetScoresZero = NearlyEqual(missingSet.CalculateScore(null), 0f);

            SetConsiderations(actionSet);
            bool emptyConsiderationsScoreOne = NearlyEqual(new AIAction { actionset = actionSet }.CalculateScore(null), 1f);

            SetConsiderations(actionSet, one, null);
            one.FixedScore = 1f;
            bool nullConsiderationScoresZero = NearlyEqual(new AIAction { actionset = actionSet }.CalculateScore(null), 0f);

            zero.FixedScore = 0f;
            SetConsiderations(actionSet, one, zero);
            bool zeroConsiderationScoresZero = NearlyEqual(new AIAction { actionset = actionSet }.CalculateScore(null), 0f);

            overMax.FixedScore = 5f;
            half.FixedScore = 0.5f;
            SetConsiderations(actionSet, overMax, half);
            float clampedScore = new AIAction { actionset = actionSet }.CalculateScore(null);
            bool overMaxIsClamped = NearlyEqual(clampedScore, ExpectedWeightedScore(1f, 0.5f));

            AIAction propertyClamp = new AIAction();
            propertyClamp.score = 2f;
            bool clampsHigh = NearlyEqual(propertyClamp.score, 1f);
            propertyClamp.score = -1f;
            bool clampsLow = NearlyEqual(propertyClamp.score, 0f);

            return nullActionSetScoresZero
                && emptyConsiderationsScoreOne
                && nullConsiderationScoresZero
                && zeroConsiderationScoresZero
                && overMaxIsClamped
                && clampsHigh
                && clampsLow;
        }
        finally
        {
            Object.DestroyImmediate(actionSet);
            Object.DestroyImmediate(one);
            Object.DestroyImmediate(zero);
            Object.DestroyImmediate(overMax);
            Object.DestroyImmediate(half);
        }
    }

    private static bool VerifyBrainSelectsNextActionAfterDestinationFailure()
    {
        using PriorityScenarioWorld world = new PriorityScenarioWorld();
        Character character = world.CreateOwner("Owner_Slime", Vector2Int.zero);
        TestActionSet failingHighScore = CreateAction("Failing high score", 1f, requiresDestination: true, resolvesDestination: false);
        TestActionSet nextAction = CreateAction("Next action", 0.25f, requiresDestination: false, resolvesDestination: true);

        try
        {
            character.ai.availableActions = new[]
            {
                new AIAction { actionset = failingHighScore },
                new AIAction { actionset = nextAction }
            };

            bool decided = character.ai.DecideAction();
            return decided
                && character.ai.bestAction != null
                && character.ai.bestAction.actionset == nextAction
                && character.ai.bestAction.planKind == AIActionPlanKind.NoDestination;
        }
        finally
        {
            Object.DestroyImmediate(failingHighScore);
            Object.DestroyImmediate(nextAction);
        }
    }

    private static bool VerifyBrainTieKeepsActionOrder()
    {
        using PriorityScenarioWorld world = new PriorityScenarioWorld();
        Character character = world.CreateOwner("Owner_Slime", Vector2Int.zero);
        TestActionSet first = CreateAction("Tie first", 0.5f, requiresDestination: false, resolvesDestination: true);
        TestActionSet second = CreateAction("Tie second", 0.5f, requiresDestination: false, resolvesDestination: true);

        try
        {
            character.ai.availableActions = new[]
            {
                new AIAction { actionset = first },
                new AIAction { actionset = second }
            };

            bool decided = character.ai.DecideAction();
            return decided
                && character.ai.bestAction != null
                && character.ai.bestAction.actionset == first;
        }
        finally
        {
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
        }
    }

    private static bool VerifyOffPriorityExcludesUrgentAutomaticWork()
    {
        using PriorityScenarioWorld world = new PriorityScenarioWorld();
        BuildableObject damaged = world.Place("P1_RestRoom", new Vector2Int(2, 0));
        damaged.SetDamaged(true);
        Character owner = world.CreateOwner("Owner_Slime", Vector2Int.zero);
        AbilityWork work = owner.GetAbility<AbilityWork>();
        SetOnly(work);

        GridPathSearchResult search = world.Grid.SearchPath(Vector2Int.zero);
        bool assigned = work.TryAssignShop(search);
        return !assigned
            && work.assignedShop == null
            && Mathf.Approximately(work.GetWorkUtilityScore(FacilityWorkType.Repair, search), 0f);
    }

    private static bool VerifyDirectCommandBypassesOffThroughAssignment()
    {
        using PriorityScenarioWorld world = new PriorityScenarioWorld();
        BuildableObject damaged = world.Place("P1_RestRoom", new Vector2Int(2, 0));
        damaged.SetDamaged(true);
        Character owner = world.CreateOwner("Owner_Slime", Vector2Int.zero);
        AbilityWork work = owner.GetAbility<AbilityWork>();
        SetOnly(work);

        GridPathSearchResult search = world.Grid.SearchPath(Vector2Int.zero);
        bool prioritySet = work.TrySetPriorityWorkTarget(damaged, FacilityWorkType.Repair, search, out _);
        bool assigned = work.TryAssignWork(FacilityWorkType.Repair, search);
        return prioritySet
            && assigned
            && work.assignedShop == damaged
            && work.AssignedWorkType == FacilityWorkType.Repair;
    }

    private static bool VerifyRequestedWorkTypeDoesNotSubstitute()
    {
        using PriorityScenarioWorld world = new PriorityScenarioWorld();
        BuildableObject restRoom = world.Place("P1_RestRoom", new Vector2Int(2, 0));
        Character owner = world.CreateOwner("Owner_Slime", Vector2Int.zero);
        AbilityWork work = owner.GetAbility<AbilityWork>();
        SetOnly(work, FacilityWorkType.Operate, FacilityWorkType.Repair);

        GridPathSearchResult search = world.Grid.SearchPath(Vector2Int.zero);
        bool assigned = work.TryAssignWork(FacilityWorkType.Repair, search);
        return restRoom != null
            && !assigned
            && work.assignedShop == null;
    }

    private static bool VerifyInvalidPriorityTargetClearsAndResumesAutomaticWork()
    {
        using PriorityScenarioWorld world = new PriorityScenarioWorld();
        BuildableObject priorityTarget = world.Place("P1_RestRoom", new Vector2Int(2, 0));
        BuildableObject alternateTarget = world.Place("P1_RestRoom", new Vector2Int(7, 0));
        priorityTarget.SetDamaged(true);
        alternateTarget.SetDamaged(true);

        Character owner = world.CreateOwner("Owner_Slime", Vector2Int.zero);
        AbilityWork work = owner.GetAbility<AbilityWork>();
        SetOnly(work, FacilityWorkType.Repair);

        GridPathSearchResult search = world.Grid.SearchPath(Vector2Int.zero);
        bool prioritySet = work.TrySetPriorityWorkTarget(priorityTarget, FacilityWorkType.Repair, search, out _);
        priorityTarget.SetDamaged(false);
        bool assigned = work.TryAssignShop(search);

        return prioritySet
            && assigned
            && work.PriorityWorkTarget == null
            && work.assignedShop == alternateTarget
            && work.AssignedWorkType == FacilityWorkType.Repair;
    }

    private static bool VerifyNearestEquivalentWorkTargetWins()
    {
        using PriorityScenarioWorld world = new PriorityScenarioWorld();
        BuildableObject near = world.Place("P1_RestRoom", new Vector2Int(2, 0));
        BuildableObject far = world.Place("P1_RestRoom", new Vector2Int(9, 0));
        near.SetDamaged(true);
        far.SetDamaged(true);

        Character owner = world.CreateOwner("Owner_Slime", Vector2Int.zero);
        AbilityWork work = owner.GetAbility<AbilityWork>();
        SetOnly(work, FacilityWorkType.Repair);

        bool assigned = work.TryAssignShop(world.Grid.SearchPath(Vector2Int.zero));
        return assigned
            && work.assignedShop == near
            && work.assignedShop != far;
    }

    private static bool VerifyPriorityLevelBeatsLowerUrgentWork()
    {
        using PriorityScenarioWorld world = new PriorityScenarioWorld();
        BuildableObject damagedRepair = world.Place("P1_RestRoom", new Vector2Int(2, 0));
        BuildableObject restockShop = world.Place("P1_LowFoodShop", new Vector2Int(9, 0));
        BuildableObject warehouse = world.Place("P1_Warehouse", new Vector2Int(14, 0));
        damagedRepair.SetDamaged(true);
        ClearShopStock(restockShop);

        Character owner = world.CreateOwner("Owner_Slime", Vector2Int.zero);
        AbilityWork work = owner.GetAbility<AbilityWork>();
        SetAllOff(work);
        work.SetWorkPriority(FacilityWorkType.Restock, WorkPriorityLevel.Priority1);
        work.SetWorkPriority(FacilityWorkType.Repair, WorkPriorityLevel.Priority2);

        bool assigned = work.TryAssignShop(world.Grid.SearchPath(Vector2Int.zero));
        return warehouse != null
            && assigned
            && work.assignedShop == restockShop
            && work.AssignedWorkType == FacilityWorkType.Restock;
    }

    private static bool VerifyCombinedPriorityProfileEdges()
    {
        WorkPriorityProfile priorities = WorkPriorityProfile.CreateDefault();
        priorities.SetPriority(FacilityWorkType.Operate, WorkPriorityLevel.Priority3);
        priorities.SetPriority(FacilityWorkType.Repair, WorkPriorityLevel.Priority1);
        priorities.SetPriority(FacilityWorkType.Guard, WorkPriorityLevel.Off);

        bool bestCombinedPriority = priorities.GetPriority(FacilityWorkType.Operate | FacilityWorkType.Repair)
            == WorkPriorityLevel.Priority1;
        bool noneIsOff = priorities.GetPriority(FacilityWorkType.None) == WorkPriorityLevel.Off;

        priorities.ApplyPreferredTypes(FacilityWorkType.Guard | FacilityWorkType.Operate);
        bool preferredRevivesOff = priorities.GetPriority(FacilityWorkType.Guard) == WorkPriorityLevel.Priority1;
        bool preferredUpgradesLow = priorities.GetPriority(FacilityWorkType.Operate) == WorkPriorityLevel.Priority1;

        return bestCombinedPriority
            && noneIsOff
            && preferredRevivesOff
            && preferredUpgradesLow;
    }

    private static bool VerifyPersonalityModifierAffectsActionScore()
    {
        GameObject obj = new GameObject("Personality Score Character");
        CharacterSO data = ScriptableObject.CreateInstance<CharacterSO>();
        AIWait waitAction = ScriptableObject.CreateInstance<AIWait>();
        try
        {
            Character character = obj.AddComponent<Character>();
            data.aiPersonality.patience = 0.5f;
            character.data = data;

            float score = new AIAction { actionset = waitAction }.CalculateScore(character);
            return NearlyEqual(score, 0.5f);
        }
        finally
        {
            Object.DestroyImmediate(waitAction);
            Object.DestroyImmediate(data);
            Object.DestroyImmediate(obj);
        }
    }

    private static bool VerifyOccupiedWorkTargetFailureClassification()
    {
        using PriorityScenarioWorld world = new PriorityScenarioWorld();
        BuildableObject shop = world.Place("P1_LowFoodShop", new Vector2Int(2, 0));
        Character first = world.CreateOwner("Owner_Slime", Vector2Int.zero);
        Character second = world.CreateOwner("Owner_Slime", new Vector2Int(5, 0));
        if (shop is not IWorkableFacility workable)
        {
            return false;
        }

        IEnumerator allocation = workable.AllocateWorker(first);
        allocation?.MoveNext();

        AbilityWork secondWork = second.GetAbility<AbilityWork>();
        GridPathSearchResult search = world.Grid.SearchPath(second.GetNowXY());
        bool found = secondWork.TryGetBestWorkCandidate(FacilityWorkType.None, search, out _);
        bool rejected = secondWork.TryGetLastRejectedWorkCandidate(out WorkTargetCandidate candidate);
        return !found
            && rejected
            && candidate.Building == shop
            && candidate.FailureKind == AIActionFailureKind.DestinationOccupied
            && AIActionFailure.ClassifyKind(candidate.FailureReason) == AIActionFailureKind.DestinationOccupied;
    }

    private static bool VerifyWorkAndWaitScoresPreferRealWork()
    {
        using PriorityScenarioWorld world = new PriorityScenarioWorld();
        BuildableObject shop = world.Place("P1_LowFoodShop", new Vector2Int(2, 0));
        Character owner = world.CreateOwner("Owner_Slime", Vector2Int.zero);
        AIWork workAction = ScriptableObject.CreateInstance<AIWork>();
        AIWait waitAction = ScriptableObject.CreateInstance<AIWait>();
        try
        {
            owner.ai.availableActions = new[]
            {
                new AIAction { actionset = workAction },
                new AIAction { actionset = waitAction }
            };

            bool decided = owner.ai.DecideAction();
            return shop != null
                && decided
                && owner.ai.bestAction != null
                && owner.ai.bestAction.actionset == workAction
                && owner.ai.bestAction.score > owner.ai.availableActions[1].score;
        }
        finally
        {
            Object.DestroyImmediate(workAction);
            Object.DestroyImmediate(waitAction);
        }
    }

    private static bool VerifyNoWorkTargetUsesExplicitWait()
    {
        using PriorityScenarioWorld world = new PriorityScenarioWorld();
        Character owner = world.CreateOwner("Owner_Slime", Vector2Int.zero);
        AIWork workAction = ScriptableObject.CreateInstance<AIWork>();
        AIWait waitAction = ScriptableObject.CreateInstance<AIWait>();
        try
        {
            owner.ai.availableActions = new[]
            {
                new AIAction { actionset = workAction },
                new AIAction { actionset = waitAction }
            };

            bool decided = owner.ai.DecideAction();
            return decided
                && owner.ai.bestAction != null
                && owner.ai.bestAction.actionset == waitAction
                && Mathf.Approximately(owner.ai.availableActions[0].score, 0f)
                && owner.ai.availableActions[1].score > 0f;
        }
        finally
        {
            Object.DestroyImmediate(workAction);
            Object.DestroyImmediate(waitAction);
        }
    }

    private static TestActionSet CreateAction(
        string actionName,
        float score,
        bool requiresDestination,
        bool resolvesDestination)
    {
        TestActionSet action = ScriptableObject.CreateInstance<TestActionSet>();
        action.actionName = actionName;
        action.RequireDestination = requiresDestination;
        action.ResolveDestination = resolvesDestination;

        FixedScoreConsideration consideration = ScriptableObject.CreateInstance<FixedScoreConsideration>();
        consideration.FixedScore = score;
        action.OwnedConsideration = consideration;
        SetConsiderations(action, consideration);
        return action;
    }

    private static void SetOnly(AbilityWork work, params FacilityWorkType[] enabledTypes)
    {
        SetAllOff(work);
        foreach (FacilityWorkType type in enabledTypes)
        {
            work.SetWorkPriority(type, WorkPriorityLevel.Priority1);
        }
    }

    private static void SetAllOff(AbilityWork work)
    {
        foreach (FacilityWorkType type in WorkTaskCatalog.TaskTypes)
        {
            work.SetWorkPriority(type, WorkPriorityLevel.Off);
        }
    }

    private static void ClearShopStock(BuildableObject building)
    {
        FieldInfo field = typeof(Shop).GetField("stocks", BindingFlags.Instance | BindingFlags.NonPublic);
        field?.SetValue(building, new List<RemainStock>());
        FacilityCandidateCache.MarkDynamicStateDirty();
    }

    private static void SetConsiderations(AIActionSet actionSet, params Consideration[] considerations)
    {
        FieldInfo field = typeof(AIActionSet).GetField(
            "<considerations>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field?.SetValue(actionSet, considerations);
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

    private static bool IsAdjacentWalkPath(Queue<GridMoveStep> path)
    {
        if (path == null || path.Count == 0)
        {
            return false;
        }

        bool first = true;
        Vector2Int expectedFrom = default;
        foreach (GridMoveStep step in path)
        {
            if (step == null || step.MoveType != GridMoveType.Walk)
            {
                return false;
            }

            if (!first && step.From != expectedFrom)
            {
                return false;
            }

            int distance = Mathf.Abs(step.From.x - step.To.x) + Mathf.Abs(step.From.y - step.To.y);
            if (distance != 1)
            {
                return false;
            }

            expectedFrom = step.To;
            first = false;
        }

        return true;
    }

    private sealed class FixedScoreConsideration : Consideration
    {
        public float FixedScore { get; set; }

        public override float ScoreConsideration(Character character)
        {
            return FixedScore;
        }
    }

    private sealed class TestActionSet : AIActionSet
    {
        public bool RequireDestination { get; set; }
        public bool ResolveDestination { get; set; }
        public FixedScoreConsideration OwnedConsideration { get; set; }

        public override bool RequiresDestination => RequireDestination;

        public override void Execute(Character character)
        {
        }

        public override bool TryResolveDestination(
            Character character,
            GridPathSearchResult searchResult,
            out BuildableObject destination,
            out string failureReason)
        {
            destination = null;
            failureReason = string.Empty;
            if (!RequireDestination)
            {
                return true;
            }

            if (!ResolveDestination)
            {
                failureReason = "forced destination failure";
                return false;
            }

            failureReason = "test destination not configured";
            return false;
        }

        private void OnDestroy()
        {
            if (OwnedConsideration != null)
            {
                Object.DestroyImmediate(OwnedConsideration);
            }
        }
    }

    private sealed class PriorityScenarioWorld : IDisposable
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

        public PriorityScenarioWorld(int width = 16)
        {
            previousGridSystem = GridSystemInstanceField?.GetValue(null) as GridSystemManager;
            previousScheduler = CharacterAiSchedulerInstanceField?.GetValue(null) as CharacterAiScheduler;
            CharacterAiSchedulerInstanceField?.SetValue(null, null);
            Grid = new Grid(width, 1);
            for (int x = 0; x < Grid.width; x++)
            {
                Grid.RegisterOccupant(
                    new TestHallwayOccupant(),
                    GridLayer.Hallway,
                    new List<Vector2Int> { new Vector2Int(x, 0) },
                    false);
            }

            GameObject gridSystemObject = new GameObject("Character AI Priority Corner GridSystemManager");
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

        public Character CreateOwner(string ownerAssetName, Vector2Int position)
        {
            CharacterSO data = AssetDatabase.LoadAssetAtPath<CharacterSO>(
                $"Assets/Resources/SO/Character/Owners/{ownerAssetName}.asset");
            if (data == null)
            {
                throw new InvalidOperationException($"{ownerAssetName} asset not found.");
            }

            GameObject obj = new GameObject(ownerAssetName);
            objects.Add(obj);
            obj.transform.position = Grid.GetWorldPos(position);
            obj.AddComponent<SpriteRenderer>();
            obj.AddComponent<Character>();
            obj.AddComponent<AbilityMove>();
            obj.AddComponent<AbilityShopping>();
            obj.AddComponent<AbilityWork>();
            obj.AddComponent<AIBrain>();

            Character character = obj.GetComponent<Character>();
            CharacterAwakeMethod?.Invoke(character, null);
            character.RefreshAbilityCache();
            character.Initialization(data);
            character.SetLifecycleState(Character.LifecycleState.Active);
            return character;
        }

        public void Dispose()
        {
            GridSystemInstanceField?.SetValue(null, previousGridSystem);
            CharacterAiSchedulerInstanceField?.SetValue(null, previousScheduler);
            FacilityCandidateCache.Clear();

            foreach (GameObject obj in objects.Where((obj) => obj != null))
            {
                Object.DestroyImmediate(obj);
            }
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
