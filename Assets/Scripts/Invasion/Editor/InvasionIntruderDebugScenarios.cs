using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class InvasionIntruderDebugScenarios
{
    [MenuItem("DungeonStory/Debug/Invasion/Run P1 Intruder Scenarios")]
    public static void RunFromMenu()
    {
        bool success = RunAll(true);
        if (!success)
        {
            Debug.LogError("P1 intruder scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        List<string> errors = new List<string>();

        RunScenario("침입자 에셋", VerifyIntruderAsset, errors);
        RunScenario("탐색성과 목표 보정", VerifyExplorationBias, errors);
        RunScenario("시설 파손 보조 목표", VerifyFacilityDamage, errors);
        RunScenario("최종 교전과 런 종료", VerifyFinalCombatEndsRun, errors);

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
            Debug.Log("P1 intruder scenarios passed.");
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

    private static bool VerifyIntruderAsset()
    {
        CharacterSO intruder = LoadIntruder();
        return intruder != null
            && intruder.characterType == CharacterType.Intruder
            && intruder.role == CharacterRole.Regular
            && intruder.id == 2001
            && intruder.characterSprite != null
            && intruder.moveSpeed > 0f;
    }

    private static bool VerifyExplorationBias()
    {
        Grid grid = new Grid(8, 1);
        for (int x = 0; x < grid.width; x++)
        {
            AddHallway(grid, new Vector2Int(x, 0));
        }

        Vector2Int start = new Vector2Int(0, 0);
        Vector2Int ownerPosition = new Vector2Int(7, 0);
        Queue<GridMoveStep> earlyPath = InvasionIntruderPlanner.GetNextPath(
            grid,
            start,
            ownerPosition,
            0f,
            out bool earlyDirect);
        Queue<GridMoveStep> latePath = InvasionIntruderPlanner.GetNextPath(
            grid,
            start,
            ownerPosition,
            1f,
            out bool lateDirect);

        bool earlyExplores = !earlyDirect
            && earlyPath.Count > 0
            && earlyPath.Last().To != ownerPosition;
        bool lateTargetsOwner = lateDirect
            && latePath.Count > 0
            && latePath.Last().To == ownerPosition;

        return earlyExplores && lateTargetsOwner;
    }

    private static bool VerifyFacilityDamage()
    {
        using IntruderScenarioWorld world = new IntruderScenarioWorld(10);
        BuildableObject facility = world.Place("P1_LowFoodShop", new Vector2Int(2, 0));
        Character intruder = world.CreateIntruder(new Vector2Int(1, 0));
        InvasionIntruderRuntime runtime = intruder.gameObject.AddComponent<InvasionIntruderRuntime>();
        SetPrivateField(runtime, "intruder", intruder);

        CountingFacilityDamageListener listener = new CountingFacilityDamageListener();
        bool damaged = runtime.TryDamageNearbyFacility(world.Grid);
        bool valid = damaged && facility.IsDamaged && listener.Count == 1 && listener.LastFacility == facility;

        listener.Dispose();
        return valid;
    }

    private static bool VerifyFinalCombatEndsRun()
    {
        using IntruderScenarioWorld world = new IntruderScenarioWorld(10);
        CharacterSO ownerData = AssetDatabase.LoadAssetAtPath<CharacterSO>(
            "Assets/Resources/SO/Character/Owners/Owner_Orc.asset");
        if (ownerData == null)
        {
            return false;
        }

        GameObject managerObject = new GameObject("Intruder Final Combat OwnerRunManager");
        world.Track(managerObject);
        OwnerRunManager manager = managerObject.AddComponent<OwnerRunManager>();
        manager.SelectOwner(ownerData);

        Character owner = manager.CurrentOwner;
        if (owner == null)
        {
            return false;
        }

        Character intruder = world.CreateIntruder(new Vector2Int(1, 0));
        InvasionIntruderRuntime runtime = intruder.gameObject.AddComponent<InvasionIntruderRuntime>();
        SetPrivateField(runtime, "intruder", intruder);
        SetPrivateField(runtime, "settings", new InvasionIntruderSettings
        {
            finalCombatDamage = owner.MaxHealth + 10f,
            finalCombatWindupSeconds = 0f
        });

        runtime.ApplyFinalCombat(owner);

        return owner.IsDead && manager.IsRunEnded && runtime.State == InvasionIntruderState.FinalCombat;
    }

    private static CharacterSO LoadIntruder()
    {
        return AssetDatabase.LoadAssetAtPath<CharacterSO>(
            "Assets/Resources/SO/Character/Intruders/Intruder_Breakthrough.asset");
    }

    private static void AddHallway(Grid grid, Vector2Int position)
    {
        grid.RegisterOccupant(
            new TestHallwayOccupant(),
            GridLayer.Hallway,
            new List<Vector2Int> { position },
            false);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field?.SetValue(target, value);
    }

    private sealed class IntruderScenarioWorld : IDisposable
    {
        private static readonly FieldInfo GridSystemInstanceField =
            typeof(GridSystemManager).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly FieldInfo GridField =
            typeof(GridSystemManager).GetField("<grid>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo OwnerInstanceField =
            typeof(UtilSingleton<OwnerRunManager>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo CharacterAwakeMethod =
            typeof(Character).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly GridSystemManager previousGridSystem;
        private readonly OwnerRunManager previousOwnerRunManager;
        private readonly List<GameObject> objects = new List<GameObject>();
        private readonly GameObject gridSystemObject;

        public IntruderScenarioWorld(int width)
        {
            previousGridSystem = GridSystemInstanceField?.GetValue(null) as GridSystemManager;
            previousOwnerRunManager = OwnerInstanceField?.GetValue(null) as OwnerRunManager;

            Grid = new Grid(width, 1);
            for (int x = 0; x < Grid.width; x++)
            {
                AddHallway(Grid, new Vector2Int(x, 0));
            }

            gridSystemObject = new GameObject("Intruder Scenario GridSystemManager");
            objects.Add(gridSystemObject);
            GridSystemManager manager = gridSystemObject.AddComponent<GridSystemManager>();
            GridField?.SetValue(manager, Grid);
            GridSystemInstanceField?.SetValue(null, manager);
            OwnerInstanceField?.SetValue(null, null);
        }

        public Grid Grid { get; }

        public void Track(GameObject obj)
        {
            if (obj != null && !objects.Contains(obj))
            {
                objects.Add(obj);
            }
        }

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

        public Character CreateIntruder(Vector2Int position)
        {
            GameObject obj = new GameObject("Intruder Scenario Character");
            objects.Add(obj);
            obj.AddComponent<SpriteRenderer>();
            obj.AddComponent<AbilityMove>();
            Character character = obj.AddComponent<Character>();
            CharacterAwakeMethod?.Invoke(character, null);
            character.RefreshAbilityCache();
            character.Initialization(LoadIntruder());
            character.SetLifecycleState(Character.LifecycleState.Active);
            obj.transform.position = Grid.GetWorldPos(position);
            return character;
        }

        public void Dispose()
        {
            GridSystemInstanceField?.SetValue(null, previousGridSystem);
            OwnerInstanceField?.SetValue(null, previousOwnerRunManager);
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

    private sealed class CountingFacilityDamageListener : UtilEventListener<InvasionFacilityDamagedEvent>, IDisposable
    {
        public int Count { get; private set; }
        public BuildableObject LastFacility { get; private set; }

        public CountingFacilityDamageListener()
        {
            this.EventStartListening<InvasionFacilityDamagedEvent>();
        }

        public void OnTriggerEvent(InvasionFacilityDamagedEvent eventType)
        {
            Count++;
            LastFacility = eventType.facility;
        }

        public void Dispose()
        {
            this.EventStopListening<InvasionFacilityDamagedEvent>();
        }
    }
}
