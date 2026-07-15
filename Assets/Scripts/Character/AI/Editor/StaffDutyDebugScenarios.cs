using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class StaffDutyDebugScenarios
{
    [MenuItem("DungeonStory/Debug/Character/Run P1 Staff Duty Scenarios")]
    public static void RunFromMenu()
    {
        bool success = RunAll(true);
        if (!success)
        {
            Debug.LogError("P1 staff duty scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        List<string> errors = new List<string>();

        RunScenario("吏곸썝 珥덇린 而⑤뵒??蹂댁젙", VerifyWorkerInitialCondition, errors);
        RunScenario("洹쇰Т ?쇰줈濡?鍮꾨쾲 ?꾪솚", VerifyWorkFatigueTriggersOffDuty, errors);
        RunScenario("Critical fatigue enters off-duty instead of stuck rest protection", VerifyCriticalFatigueEntersOffDuty, errors);
        RunScenario("鍮꾨쾲 諛⑸Ц ?ъ씠???쒖옉", VerifyOffDutyVisitCycle, errors);
        RunScenario("吏곸썝 AI 鍮꾨쾲 ?됰룞 蹂닿컯", VerifyStaffBrainAddsOffDutyActions, errors);
        RunScenario("?⑤???吏곸썝 援ш꼍 ?쒖쇅", VerifyOnDutyStaffDoesNotUseLookAround, errors);
        RunScenario("?ъ옣 AI 諛⑸Ц ?됰룞 ?쒖쇅", VerifyOwnerBrainDoesNotAddVisitorActions, errors);
        RunScenario("鍮꾨쾲 吏곸썝 留ㅼ텧 ?쒖쇅", VerifyOffDutyStaffDoesNotCreateRevenue, errors);
        RunScenario("湲닿툒 ?묒뾽 鍮꾨쾲 以묐떒", VerifyEmergencyCanInterruptOffDuty, errors);

        RunScenario("?덇린留뚯쑝濡쒕뒗 吏곸썝??鍮꾨쾲 ?꾪솚?섏? ?딆쓬", VerifyHungerDoesNotForceOffDuty, errors);
        RunScenario("鍮꾨쾲 ?湲곕뒗 ?덇린瑜??뚮났?섏? ?딆쓬", VerifyWaitDoesNotRecoverHunger, errors);
        RunScenario("?⑤????湲?吏곸썝 ?섏쟾 諛고쉶 寃쎈줈", VerifyOnDutyWaitCanWander, errors);
        RunScenario("On-duty wait selects dungeon wander", VerifyOnDutyWaitSelectsDungeonWander, errors);
        RunScenario("Idle wander can use valid stairs", VerifyIdleWanderCanUseValidStairs, errors);
        RunScenario("Off-duty wait can wander", VerifyOffDutyWaitCanWander, errors);
        RunScenario("Occupied work wait uses dungeon wander", VerifyOccupiedWorkWaitUsesDungeonWander, errors);
        RunScenario("Customer ???吏곸썝??吏곸썝 AI 洹쒖튃 ?곸슜", VerifyCustomerTypedWorkerUsesStaffRules, errors);
        RunScenario("Expedition return wakes staff work decision", VerifyExpeditionReturnWakesWorkDecision, errors);

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
            Debug.Log("P1 staff duty scenarios passed.");
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

    private static bool VerifyWorkerInitialCondition()
    {
        CharacterActor staff = CreateStaff("Staff Initial Condition", withShopping: true, withWorkAction: true);
        AbilityWork work = staff.GetAbility<AbilityWork>();
        bool canStartWork = work.CanStartWorkAction();

        bool valid = staff.stats[CharacterCondition.SLEEP] >= 80f
            && staff.stats[CharacterCondition.MOOD] >= 70f
            && !work.IsOffDuty
            && canStartWork;

        if (!valid)
        {
            Debug.LogError(
                $"Worker initial condition detail: " +
                $"sleep={staff.stats[CharacterCondition.SLEEP]}, " +
                $"mood={staff.stats[CharacterCondition.MOOD]}, " +
                $"offDuty={work.IsOffDuty}, " +
                $"restProtection={work.ShouldUseRestProtection()}, " +
                $"canStartWork={canStartWork}, " +
                $"actions={(staff.ai != null && staff.ai.availableActions != null ? staff.ai.availableActions.Length : -1)}");
        }

        Object.DestroyImmediate(staff.gameObject);
        return valid;
    }

    private static bool VerifyWorkFatigueTriggersOffDuty()
    {
        CharacterActor staff = CreateStaff("Staff Fatigue", withShopping: true, withWorkAction: true);
        AbilityWork work = staff.GetAbility<AbilityWork>();
        staff.stats[CharacterCondition.SLEEP] = 26f;
        staff.stats[CharacterCondition.MOOD] = 26f;

        work.ApplyWorkFatigueTick();
        bool canWork = work.CanStartWorkAction();
        bool valid = !canWork
            && work.IsOffDuty
            && staff.stats[CharacterCondition.SLEEP] < 26f
            && staff.stats[CharacterCondition.MOOD] < 26f;

        Object.DestroyImmediate(staff.gameObject);
        return valid;
    }

    private static bool VerifyCriticalFatigueEntersOffDuty()
    {
        CharacterActor staff = CreateStaff("Staff Critical Fatigue", withShopping: true, withWorkAction: true);
        AbilityWork work = staff.GetAbility<AbilityWork>();
        staff.stats[CharacterCondition.SLEEP] = 0.5f;
        staff.stats[CharacterCondition.MOOD] = 100f;

        bool canWork = work.CanStartWorkAction();
        bool valid = !canWork
            && work.IsOffDuty
            && staff.ai.isBestActionEnd;

        Object.DestroyImmediate(staff.gameObject);
        return valid;
    }

    private static bool VerifyOffDutyVisitCycle()
    {
        CharacterActor staff = CreateStaff("Staff Off Duty Visit", withShopping: true, withWorkAction: true);
        AbilityWork work = staff.GetAbility<AbilityWork>();
        AbilityShopping shopping = staff.GetAbility<AbilityShopping>();
        BuildableObject visited = CreateDummyBuilding();

        shopping.RegisterVisit(visited);
        work.BeginOffDuty("test");
        bool valid = work.IsOffDuty
            && shopping.visitCount >= 1
            && shopping.lookAroundCount >= 1
            && shopping.visitedBuilding.Count == 0;

        Object.DestroyImmediate(visited.gameObject);
        Object.DestroyImmediate(staff.gameObject);
        return valid;
    }

    private static bool VerifyStaffBrainAddsOffDutyActions()
    {
        CharacterActor staff = CreateStaff("Staff Brain Actions", withShopping: true, withWorkAction: true);
        Type[] actionTypes = staff.ai.availableActions
            .Where((action) => action != null && action.actionset != null)
            .Select((action) => action.actionset.GetType())
            .ToArray();

        bool valid = actionTypes.Contains(typeof(AIWork))
            && actionTypes.Contains(typeof(AIWait))
            && actionTypes.Contains(typeof(AIEat))
            && actionTypes.Contains(typeof(AIRest))
            && actionTypes.Contains(typeof(AIShopping))
            && !actionTypes.Contains(typeof(AIExitDungeon));

        Object.DestroyImmediate(staff.gameObject);
        return valid;
    }

    private static bool VerifyOnDutyStaffDoesNotUseLookAround()
    {
        CharacterActor staff = CreateStaff("Staff LookAround Guard", withShopping: true, withWorkAction: true);
        CharacterActor customer = CreateCustomer();
        CharacterActor staffActor = CharacterActor.From(staff);
        CharacterActor customerActor = CharacterActor.From(customer);
        ConsiderationCanLookAround consideration = ScriptableObject.CreateInstance<ConsiderationCanLookAround>();

        bool valid = !AILookAround.CanUseVisitLookAround(staffActor)
            && Mathf.Approximately(consideration.ScoreConsideration(staffActor), 0f)
            && AILookAround.CanUseVisitLookAround(customerActor)
            && Mathf.Approximately(consideration.ScoreConsideration(customerActor), 1f);

        Object.DestroyImmediate(consideration);
        Object.DestroyImmediate(staff.gameObject);
        Object.DestroyImmediate(customer.gameObject);
        return valid;
    }

    private static bool VerifyOwnerBrainDoesNotAddVisitorActions()
    {
        CharacterActor owner = CreateOwner("Owner_Orc");
        Type[] actionTypes = owner.ai.availableActions
            .Where((action) => action != null && action.actionset != null)
            .Select((action) => action.actionset.GetType())
            .ToArray();

        bool valid = actionTypes.Contains(typeof(AIWork))
            && actionTypes.Contains(typeof(AIWait))
            && !actionTypes.Contains(typeof(AIEat))
            && !actionTypes.Contains(typeof(AIRest))
            && !actionTypes.Contains(typeof(AIShopping))
            && !actionTypes.Contains(typeof(AIExitDungeon));

        Object.DestroyImmediate(owner.gameObject);
        return valid;
    }

    private static bool VerifyOffDutyStaffDoesNotCreateRevenue()
    {
        CharacterActor staff = CreateStaff("Staff No Revenue", withShopping: true, withWorkAction: true);
        CharacterActor customer = CreateCustomer();
        CharacterActor npcVisitor = CreateCustomer();
        npcVisitor.characterType = CharacterType.NPC;

        bool valid = !Shop.CreatesRevenueFor(CharacterActor.From(staff))
            && Shop.CreatesRevenueFor(CharacterActor.From(customer))
            && Shop.CreatesRevenueFor(CharacterActor.From(npcVisitor))
            && Shop.CreatesRevenueFor((CharacterActor)null);

        Object.DestroyImmediate(staff.gameObject);
        Object.DestroyImmediate(customer.gameObject);
        Object.DestroyImmediate(npcVisitor.gameObject);
        return valid;
    }

    private static bool VerifyEmergencyCanInterruptOffDuty()
    {
        using WorkScenarioWorld world = new WorkScenarioWorld();
        BuildableObject damaged = world.Place("P1_RestRoom", new Vector2Int(2, 0));
        damaged.SetDamaged(true);

        CharacterActor staff = CreateStaff("Staff Emergency", withShopping: true, withWorkAction: true);
        AbilityWork work = staff.GetAbility<AbilityWork>();
        staff.stats[CharacterCondition.SLEEP] = 10f;
        staff.stats[CharacterCondition.MOOD] = 10f;
        work.BeginOffDuty("test");

        GridPathSearchResult search = world.Grid.SearchPath(Vector2Int.zero);
        bool prioritySet = work.TrySetPriorityWorkTarget(damaged, FacilityWorkType.Repair, search, out _);
        bool assigned = work.TryAssignShop(search);
        bool valid = prioritySet
            && assigned
            && !work.IsOffDuty
            && work.assignedShop == damaged
            && work.AssignedWorkType == FacilityWorkType.Repair;

        Object.DestroyImmediate(staff.gameObject);
        return valid;
    }

    private static bool VerifyHungerDoesNotForceOffDuty()
    {
        CharacterActor staff = CreateStaff("Staff Hungry Still Works", withShopping: true, withWorkAction: true);
        AbilityWork work = staff.GetAbility<AbilityWork>();

        staff.stats[CharacterCondition.SLEEP] = 100f;
        staff.stats[CharacterCondition.MOOD] = 100f;
        staff.stats[CharacterCondition.HUNGER] = 5f;

        bool valid = work.CanStartWorkAction()
            && !work.IsOffDuty
            && !work.ShouldTakeOffDuty();

        Object.DestroyImmediate(staff.gameObject);
        return valid;
    }

    private static bool VerifyWaitDoesNotRecoverHunger()
    {
        CharacterActor staff = CreateStaff("Staff Wait Hunger", withShopping: true, withWorkAction: true);
        AbilityWork work = staff.GetAbility<AbilityWork>();
        AIWait wait = ScriptableObject.CreateInstance<AIWait>();

        staff.stats[CharacterCondition.SLEEP] = 20f;
        staff.stats[CharacterCondition.MOOD] = 20f;
        staff.stats[CharacterCondition.FUN] = 20f;
        staff.stats[CharacterCondition.HUNGER] = 10f;
        work.BeginOffDuty("test");

        float beforeHunger = staff.stats[CharacterCondition.HUNGER];
        wait.Execute(CharacterActor.From(staff));
        float afterHunger = staff.stats[CharacterCondition.HUNGER];
        bool recoveredOtherStats = staff.stats[CharacterCondition.SLEEP] > 20f
            && staff.stats[CharacterCondition.MOOD] > 20f
            && staff.stats[CharacterCondition.FUN] > 20f;

        Object.DestroyImmediate(wait);
        Object.DestroyImmediate(staff.gameObject);
        return Mathf.Approximately(beforeHunger, afterHunger)
            && recoveredOtherStats;
    }

    private static bool VerifyOnDutyWaitCanWander()
    {
        FieldInfo instanceField = typeof(GridSystemManager)
            .GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
        FieldInfo gridField = typeof(GridSystemManager)
            .GetField("<grid>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        object previousGridSystem = instanceField?.GetValue(null);

        GameObject managerObject = null;
        GameObject staffObject = null;
        CharacterSO data = null;
        try
        {
            managerObject = new GameObject("Idle Wander GridSystem");
            instanceField?.SetValue(null, null);
            GridSystemManager manager = managerObject.AddComponent<GridSystemManager>();

            Grid grid = new Grid(6, 1);
            for (int x = 0; x < grid.width; x++)
            {
                grid.RegisterOccupant(
                    new TestHallwayOccupant(),
                    GridLayer.Hallway,
                    new List<Vector2Int> { new Vector2Int(x, 0) },
                    false);
            }

            gridField?.SetValue(manager, grid);
            instanceField?.SetValue(null, manager);

            data = ScriptableObject.CreateInstance<CharacterSO>();
            data.characterType = CharacterType.NPC;
            data.role = CharacterRole.Regular;
            data.characterName = "Staff Idle Wander";
            data.speciesTag = "Orc";

            staffObject = new GameObject("Staff Idle Wander");
            staffObject.AddComponent<SpriteRenderer>();
            staffObject.AddComponent<CharacterActor>();
            staffObject.AddComponent<AbilityMove>();
            staffObject.transform.position = grid.GetWorldPos(Vector2Int.zero);

            CharacterActor staff = InitializeCharacterObject(staffObject, data);
            AbilityMove move = staff.GetAbility<AbilityMove>();
            bool foundPath = move.TryFindIdleWanderPath(2, 8, out Queue<GridMoveStep> path);
            GridMoveStep lastStep = path != null && path.Count > 0 ? path.Last() : null;
            int distance = lastStep != null
                ? Mathf.Abs(lastStep.To.x) + Mathf.Abs(lastStep.To.y)
                : 0;
            return foundPath
                && path != null
                && path.Count > 0
                && IsAdjacentWalkPath(path)
                && lastStep != null
                && distance >= 2;
        }
        finally
        {
            if (staffObject != null)
            {
                Object.DestroyImmediate(staffObject);
            }
            if (data != null)
            {
                Object.DestroyImmediate(data);
            }
            if (managerObject != null)
            {
                Object.DestroyImmediate(managerObject);
            }
            instanceField?.SetValue(null, previousGridSystem);
        }
    }

    private static bool VerifyOnDutyWaitSelectsDungeonWander()
    {
        FieldInfo instanceField = typeof(GridSystemManager)
            .GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
        FieldInfo gridField = typeof(GridSystemManager)
            .GetField("<grid>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        object previousGridSystem = instanceField?.GetValue(null);

        GameObject managerObject = null;
        GameObject staffObject = null;
        CharacterSO data = null;
        try
        {
            managerObject = new GameObject("On Duty Wait Selection GridSystem");
            instanceField?.SetValue(null, null);
            GridSystemManager manager = managerObject.AddComponent<GridSystemManager>();

            Grid grid = new Grid(6, 1);
            for (int x = 0; x < grid.width; x++)
            {
                grid.RegisterOccupant(
                    new TestHallwayOccupant(),
                    GridLayer.Hallway,
                    new List<Vector2Int> { new Vector2Int(x, 0) },
                    false);
            }

            gridField?.SetValue(manager, grid);
            instanceField?.SetValue(null, manager);

            data = ScriptableObject.CreateInstance<CharacterSO>();
            data.characterType = CharacterType.NPC;
            data.role = CharacterRole.Regular;
            data.characterName = "Staff On Duty Wait Selection";
            data.speciesTag = "Orc";
            data.defaultWorkPriorities = WorkPriorityProfile.CreateDefault();

            staffObject = new GameObject("Staff On Duty Wait Selection");
            staffObject.AddComponent<SpriteRenderer>();
            staffObject.AddComponent<CharacterActor>();
            staffObject.AddComponent<AbilityMove>();
            staffObject.AddComponent<AbilityWork>();
            AIBrain brain = staffObject.AddComponent<AIBrain>();
            brain.availableActions = AiDebugScenarioActionFactory.CreateStaffActions();
            staffObject.transform.position = grid.GetWorldPos(Vector2Int.zero);

            CharacterActor staff = InitializeCharacterObject(staffObject, data);
            string selected = IdleBehaviorRunner.GetSelectedBehaviorTypeNameForDebug(CharacterActor.From(staff), true);
            AbilityMove move = staff.GetAbility<AbilityMove>();
            bool foundPath = move.TryFindIdleWanderPath(2, 8, out Queue<GridMoveStep> path);

            return selected == nameof(StaffWanderIdleBehavior)
                && foundPath
                && path != null
                && path.Count > 0
                && IsAdjacentWalkPath(path);
        }
        finally
        {
            if (staffObject != null)
            {
                Object.DestroyImmediate(staffObject);
            }
            if (data != null)
            {
                Object.DestroyImmediate(data);
            }
            if (managerObject != null)
            {
                Object.DestroyImmediate(managerObject);
            }
            instanceField?.SetValue(null, previousGridSystem);
        }
    }

    private static bool VerifyIdleWanderCanUseValidStairs()
    {
        FieldInfo instanceField = typeof(GridSystemManager)
            .GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
        FieldInfo gridField = typeof(GridSystemManager)
            .GetField("<grid>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        object previousGridSystem = instanceField?.GetValue(null);

        GameObject managerObject = null;
        GameObject staffObject = null;
        CharacterSO data = null;
        try
        {
            managerObject = new GameObject("Idle Wander Stair Link GridSystem");
            instanceField?.SetValue(null, null);
            GridSystemManager manager = managerObject.AddComponent<GridSystemManager>();

            Grid grid = new Grid(3, 2);
            grid.RegisterOccupant(
                new TestHallwayOccupant(),
                GridLayer.Hallway,
                new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(2, 1) },
                false);

            grid.RegisterOccupant(
                new TestStairOccupant(),
                GridLayer.Building,
                new List<Vector2Int> { new Vector2Int(1, 0), new Vector2Int(1, 1) },
                true);

            gridField?.SetValue(manager, grid);
            instanceField?.SetValue(null, manager);

            data = ScriptableObject.CreateInstance<CharacterSO>();
            data.characterType = CharacterType.NPC;
            data.role = CharacterRole.Regular;
            data.characterName = "Staff Idle Wander Stair Link";
            data.speciesTag = "Orc";

            staffObject = new GameObject("Staff Idle Wander Stair Link");
            staffObject.AddComponent<SpriteRenderer>();
            staffObject.AddComponent<CharacterActor>();
            staffObject.AddComponent<AbilityMove>();
            staffObject.transform.position = grid.GetWorldPos(Vector2Int.zero);

            CharacterActor staff = InitializeCharacterObject(staffObject, data);
            AbilityMove move = staff.GetAbility<AbilityMove>();
            bool foundPath = move.TryFindIdleWanderPath(2, 8, out Queue<GridMoveStep> path);
            GridMoveStep lastStep = path != null && path.Count > 0 ? path.Last() : null;

            return foundPath
                && path != null
                && path.Count > 0
                && path.Any((step) => step.MoveType == GridMoveType.Stair)
                && path.All((step) => step.MoveType == GridMoveType.Walk || step.MoveType == GridMoveType.Stair)
                && lastStep != null
                && lastStep.To == new Vector2Int(2, 1);
        }
        finally
        {
            if (staffObject != null)
            {
                Object.DestroyImmediate(staffObject);
            }
            if (data != null)
            {
                Object.DestroyImmediate(data);
            }
            if (managerObject != null)
            {
                Object.DestroyImmediate(managerObject);
            }
            instanceField?.SetValue(null, previousGridSystem);
        }
    }

    private static bool VerifyOffDutyWaitCanWander()
    {
        FieldInfo instanceField = typeof(GridSystemManager)
            .GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
        FieldInfo gridField = typeof(GridSystemManager)
            .GetField("<grid>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        object previousGridSystem = instanceField?.GetValue(null);

        GameObject managerObject = null;
        GameObject staffObject = null;
        CharacterSO data = null;
        AIWait wait = null;
        try
        {
            managerObject = new GameObject("Off Duty Wait Wander GridSystem");
            instanceField?.SetValue(null, null);
            GridSystemManager manager = managerObject.AddComponent<GridSystemManager>();

            Grid grid = new Grid(6, 1);
            for (int x = 0; x < grid.width; x++)
            {
                grid.RegisterOccupant(
                    new TestHallwayOccupant(),
                    GridLayer.Hallway,
                    new List<Vector2Int> { new Vector2Int(x, 0) },
                    false);
            }

            gridField?.SetValue(manager, grid);
            instanceField?.SetValue(null, manager);

            data = ScriptableObject.CreateInstance<CharacterSO>();
            data.characterType = CharacterType.NPC;
            data.role = CharacterRole.Regular;
            data.characterName = "Staff Off Duty Wait Wander";
            data.speciesTag = "Orc";
            data.defaultWorkPriorities = WorkPriorityProfile.CreateDefault();

            staffObject = new GameObject("Staff Off Duty Wait Wander");
            staffObject.AddComponent<SpriteRenderer>();
            staffObject.AddComponent<CharacterActor>();
            staffObject.AddComponent<AbilityMove>();
            staffObject.AddComponent<AbilityWork>();
            AIBrain brain = staffObject.AddComponent<AIBrain>();
            brain.availableActions = AiDebugScenarioActionFactory.CreateStaffActions();
            staffObject.transform.position = grid.GetWorldPos(Vector2Int.zero);

            CharacterActor staff = InitializeCharacterObject(staffObject, data);
            AbilityWork work = staff.GetAbility<AbilityWork>();
            AbilityMove move = staff.GetAbility<AbilityMove>();
            wait = ScriptableObject.CreateInstance<AIWait>();
            work.BeginOffDuty("test");

            bool canStartWait = wait.CanStart(CharacterActor.From(staff));
            bool foundPath = move.TryFindIdleWanderPath(2, 8, out Queue<GridMoveStep> path);
            GridMoveStep lastStep = path != null && path.Count > 0 ? path.Last() : null;
            return work.IsOffDuty
                && canStartWait
                && foundPath
                && IsAdjacentWalkPath(path)
                && lastStep != null
                && grid.IsWalkable(lastStep.To);
        }
        finally
        {
            if (wait != null)
            {
                Object.DestroyImmediate(wait);
            }
            if (staffObject != null)
            {
                Object.DestroyImmediate(staffObject);
            }
            if (data != null)
            {
                Object.DestroyImmediate(data);
            }
            if (managerObject != null)
            {
                Object.DestroyImmediate(managerObject);
            }
            instanceField?.SetValue(null, previousGridSystem);
        }
    }

    private static bool VerifyOccupiedWorkWaitUsesDungeonWander()
    {
        FieldInfo instanceField = typeof(GridSystemManager)
            .GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
        FieldInfo gridField = typeof(GridSystemManager)
            .GetField("<grid>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo lastFailureField = typeof(AIBrain)
            .GetField("lastActionFailure", BindingFlags.Instance | BindingFlags.NonPublic);
        object previousGridSystem = instanceField?.GetValue(null);

        GameObject managerObject = null;
        GameObject staffObject = null;
        BuildableObject occupiedTarget = null;
        CharacterSO data = null;
        try
        {
            managerObject = new GameObject("Occupied Wait Wander GridSystem");
            instanceField?.SetValue(null, null);
            GridSystemManager manager = managerObject.AddComponent<GridSystemManager>();

            Grid grid = new Grid(8, 1);
            for (int x = 0; x < grid.width; x++)
            {
                grid.RegisterOccupant(
                    new TestHallwayOccupant(),
                    GridLayer.Hallway,
                    new List<Vector2Int> { new Vector2Int(x, 0) },
                    false);
            }

            gridField?.SetValue(manager, grid);
            instanceField?.SetValue(null, manager);

            data = ScriptableObject.CreateInstance<CharacterSO>();
            data.characterType = CharacterType.NPC;
            data.role = CharacterRole.Regular;
            data.characterName = "Staff Occupied Wait Wander";
            data.speciesTag = "Orc";
            data.defaultWorkPriorities = WorkPriorityProfile.CreateDefault();

            staffObject = new GameObject("Staff Occupied Wait Wander");
            staffObject.AddComponent<SpriteRenderer>();
            staffObject.AddComponent<CharacterActor>();
            staffObject.AddComponent<AbilityMove>();
            staffObject.AddComponent<AbilityWork>();
            AIBrain brain = staffObject.AddComponent<AIBrain>();
            brain.availableActions = AiDebugScenarioActionFactory.CreateStaffActions();
            staffObject.transform.position = grid.GetWorldPos(Vector2Int.zero);

            CharacterActor staff = InitializeCharacterObject(staffObject, data);
            occupiedTarget = CreateDummyBuilding();
            lastFailureField?.SetValue(
                staff.ai,
                AIActionFailure.Create(
                    AIActionFailureKind.DestinationOccupied,
                    "test occupied",
                    occupiedTarget));

            string selected = IdleBehaviorRunner.GetSelectedBehaviorTypeNameForDebug(CharacterActor.From(staff), true);
            return selected == nameof(StaffWanderIdleBehavior);
        }
        finally
        {
            if (occupiedTarget != null)
            {
                Object.DestroyImmediate(occupiedTarget.gameObject);
            }
            if (staffObject != null)
            {
                Object.DestroyImmediate(staffObject);
            }
            if (data != null)
            {
                Object.DestroyImmediate(data);
            }
            if (managerObject != null)
            {
                Object.DestroyImmediate(managerObject);
            }
            instanceField?.SetValue(null, previousGridSystem);
        }
    }

    private static bool VerifyCustomerTypedWorkerUsesStaffRules()
    {
        CharacterActor staff = CreateStaff(
            "Customer Typed Staff",
            withShopping: true,
            withWorkAction: true,
            characterType: CharacterType.Customer);
        AbilityWork work = staff.GetAbility<AbilityWork>();
        AIWait wait = ScriptableObject.CreateInstance<AIWait>();
        ConsiderationCanLookAround lookAround = ScriptableObject.CreateInstance<ConsiderationCanLookAround>();

        Type[] actionTypes = staff.ai.availableActions
            .Where((action) => action != null && action.actionset != null)
            .Select((action) => action.actionset.GetType())
            .ToArray();

        bool valid = CharacterWorkRoleUtility.IsOnDutyWorker(CharacterActor.From(staff))
            && work.CanStartWorkAction()
            && wait.CanStart(CharacterActor.From(staff))
            && !AILookAround.CanUseVisitLookAround(CharacterActor.From(staff))
            && Mathf.Approximately(lookAround.ScoreConsideration(CharacterActor.From(staff)), 0f)
            && actionTypes.Contains(typeof(AIWork))
            && actionTypes.Contains(typeof(AIWait))
            && actionTypes.Contains(typeof(AIEat))
            && actionTypes.Contains(typeof(AIRest))
            && actionTypes.Contains(typeof(AIShopping));

        Object.DestroyImmediate(lookAround);
        Object.DestroyImmediate(wait);
        Object.DestroyImmediate(staff.gameObject);
        return valid;
    }

    private static bool VerifyExpeditionReturnWakesWorkDecision()
    {
        FieldInfo instanceField = typeof(GridSystemManager)
            .GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
        FieldInfo gridField = typeof(GridSystemManager)
            .GetField("<grid>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo schedulerInstanceField = typeof(CharacterAiScheduler)
            .GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
        object previousGridSystem = instanceField?.GetValue(null);
        object previousScheduler = schedulerInstanceField?.GetValue(null);

        GameObject managerObject = null;
        CharacterActor staff = null;
        try
        {
            using WorkScenarioWorld world = new WorkScenarioWorld();
            managerObject = new GameObject("Expedition Return GridSystem");
            instanceField?.SetValue(null, null);
            GridSystemManager manager = managerObject.AddComponent<GridSystemManager>();
            gridField?.SetValue(manager, world.Grid);
            instanceField?.SetValue(null, manager);

            BuildableObject damaged = world.Place("P1_RestRoom", new Vector2Int(2, 0));
            damaged.SetDamaged(true);

            staff = CreateStaff("Staff Expedition Return", withShopping: true, withWorkAction: true);
            staff.transform.position = world.Grid.GetWorldPos(Vector2Int.zero);

            bool leftDungeon = staff.BeginExpedition()
                && staff.IsOnExpedition
                && !staff.CanRunAi
                && !staff.ai.isBestActionEnd;

            staff.EndExpedition(alive: true);

            bool returnedReady = !staff.IsOnExpedition
                && staff.CanRunAi
                && staff.ai.isBestActionEnd;
            schedulerInstanceField?.SetValue(null, null);
            bool decided = staff.ai.DecideAction();
            bool selectedWork = decided
                && staff.ai.bestAction != null
                && staff.ai.bestAction.actionset is AIWork
                && staff.ai.bestAction.destination == damaged;

            bool valid = leftDungeon && returnedReady && selectedWork;
            if (!valid)
            {
                Debug.LogError(
                    $"Expedition return detail: left={leftDungeon}, returned={returnedReady}, decided={decided}, " +
                    $"best={staff.ai.bestAction?.actionset?.GetType().Name ?? "null"}, " +
                    $"dest={(staff.ai.bestAction?.destination != null ? staff.ai.bestAction.destination.name : "null")}, " +
                    $"target={damaged.name}, failure={staff.ai.LastActionFailure}");
            }

            return valid;
        }
        finally
        {
            if (staff != null)
            {
                Object.DestroyImmediate(staff.gameObject);
            }
            if (managerObject != null)
            {
                Object.DestroyImmediate(managerObject);
            }
            schedulerInstanceField?.SetValue(null, previousScheduler);
            instanceField?.SetValue(null, previousGridSystem);
        }
    }

    private static CharacterActor CreateStaff(
        string name,
        bool withShopping,
        bool withWorkAction,
        CharacterType characterType = CharacterType.NPC)
    {
        CharacterSO data = ScriptableObject.CreateInstance<CharacterSO>();
        data.characterType = characterType;
        data.role = CharacterRole.Regular;
        data.characterName = name;
        data.speciesTag = "Orc";
        data.defaultWorkPriorities = WorkPriorityProfile.CreateDefault();

        GameObject obj = new GameObject(name);
        obj.AddComponent<SpriteRenderer>();
        obj.AddComponent<CharacterActor>();
        obj.AddComponent<AbilityMove>();
        if (withShopping)
        {
            obj.AddComponent<AbilityShopping>();
        }
        obj.AddComponent<AbilityWork>();
        AIBrain brain = obj.AddComponent<AIBrain>();
        if (withWorkAction)
        {
            brain.availableActions = AiDebugScenarioActionFactory.CreateStaffActions();
        }

        CharacterActor character = InitializeCharacterObject(obj, data);

        return character;
    }

    private static CharacterActor CreateOwner(string ownerAssetName)
    {
        CharacterSO data = AssetDatabase.LoadAssetAtPath<CharacterSO>(
            $"Assets/Resources/SO/Character/Owners/{ownerAssetName}.asset");

        GameObject obj = new GameObject(ownerAssetName);
        obj.AddComponent<SpriteRenderer>();
        obj.AddComponent<CharacterActor>();
        obj.AddComponent<AbilityMove>();
        obj.AddComponent<AbilityShopping>();
        obj.AddComponent<AbilityWork>();
        obj.AddComponent<AIBrain>();

        return InitializeCharacterObject(obj, data);
    }

    private static CharacterActor CreateCustomer()
    {
        CharacterSO data = ScriptableObject.CreateInstance<CharacterSO>();
        data.characterType = CharacterType.Customer;
        data.role = CharacterRole.Regular;
        data.characterName = "Customer";
        data.speciesTag = "Slime";

        GameObject obj = new GameObject("Customer");
        obj.AddComponent<SpriteRenderer>();
        obj.AddComponent<CharacterActor>();
        obj.AddComponent<AbilityMove>();
        obj.AddComponent<AbilityShopping>();
        return InitializeCharacterObject(obj, data);
    }

    private static CharacterActor InitializeCharacterObject(GameObject obj, CharacterSO data)
    {
        CharacterAiEditorTestDependencies.Inject(obj);
        CharacterActor character = obj.GetComponent<CharacterActor>();
        typeof(CharacterActor)
            .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(character, null);
        character.RefreshAbilityCache();
        character.Initialization(data);
        character.SetLifecycleState(CharacterLifecycleState.Active);
        return character;
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

    private static BuildableObject CreateDummyBuilding()
    {
        GameObject obj = new GameObject("Dummy Building");
        return obj.AddComponent<BuildableObject>();
    }

    private sealed class WorkScenarioWorld : IDisposable
    {
        private readonly List<GameObject> objects = new List<GameObject>();

        public WorkScenarioWorld()
        {
            Grid = new Grid(8, 1);
            for (int x = 0; x < Grid.width; x++)
            {
                Grid.RegisterOccupant(
                    new TestHallwayOccupant(),
                    GridLayer.Hallway,
                    new List<Vector2Int> { new Vector2Int(x, 0) },
                    false);
            }
        }

        public Grid Grid { get; }

        public BuildableObject Place(string assetName, Vector2Int position)
        {
            BuildingSO buildingData = AssetDatabase.LoadAssetAtPath<BuildingSO>(
                $"Assets/Resources/SO/Building/P1/{assetName}.asset");
            GridBuildingFactory factory = new GridBuildingFactory();
            BuildableObject building = factory.Create(Grid, buildingData, position);
            objects.Add(building.gameObject);
            CharacterAiEditorTestDependencies.Inject(building);
            if (building is Shop shop)
            {
                CharacterAiEditorTestDependencies.InjectShop(shop);
            }
            building.SetGrid(Grid);
            building.Initialization(buildingData, position);
            Grid.RegisterOccupant(
                building,
                buildingData.Placement.Layer,
                buildingData.GetGridPosList(position),
                buildingData.Placement.IsMovement);
            return building;
        }

        public void Dispose()
        {
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

    private sealed class TestStairOccupant : IGridOccupant, IGridMovementOccupant, IGridMovementHandler
    {
        public int GridId => 1;
        public bool IsGridDestroyed => false;
        public bool IsGridVisitable => true;
        public bool IsGridMovement => true;
        public GridMoveType GridMoveType => GridMoveType.Stair;

        public System.Collections.IEnumerator Traverse(CharacterActor actor, GridMoveStep step)
        {
            yield break;
        }
    }
}
