using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class DefenseFacilityDebugScenarios
{
    private static readonly string[] DefenseAssetNames =
    {
        "P1_SpikeTrap",
        "P1_PoisonPool",
        "P1_FireVent",
        "P1_LightningPillar",
        "P1_IceVent",
        "P1_GuardRoom"
    };

    [MenuItem("DungeonStory/Debug/Defense/Run P1 Defense Facility Scenarios")]
    public static void RunFromMenu()
    {
        bool success = RunAll(true);
        if (!success)
        {
            Debug.LogError("P1 defense facility scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        P1DefenseFacilityAssetBuilder.EnsureP1DefenseAssets();

        List<string> errors = new List<string>();
        RunScenario("방어 시설 에셋", VerifyDefenseAssets, errors);
        RunScenario("SO Effect 적용", VerifyEffectAssetsDriveDamage, errors);
        RunScenario("진입 발동 피해와 이벤트", VerifyTriggerDamageAndEvent, errors);
        RunScenario("파손 비활성화와 수리 복구", VerifyDamagedDisableAndRepair, errors);
        RunScenario("독 부식 피해 보정", VerifyPoisonCorrosion, errors);
        RunScenario("화염 연소 지속 피해", VerifyFireBurn, errors);
        RunScenario("번개 축전 방전", VerifyLightningCharge, errors);
        RunScenario("냉기 감속 지연", VerifyIceSlow, errors);
        RunScenario("경비실 경비 작업과 교전", VerifyGuardRoom, errors);

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
            Debug.Log("P1 defense facility scenarios passed.");
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

    private static bool VerifyDefenseAssets()
    {
        BuildingSO[] assets = DefenseAssetNames.Select(LoadDefense).ToArray();
        return assets.All((asset) => asset != null
            && asset.type == typeof(DefenseFacility)
            && asset.layer == GridLayer.Building
            && asset.category == BuildingCategory.Special
            && asset.Facility != null
            && asset.Facility.disabledWhenDamaged
            && asset.Facility.SupportsWork(FacilityWorkType.Repair)
            && asset.Defense != null
            && asset.Defense.IsDefenseFacility
            && asset.Defense.star == 1
            && asset.Defense.effectAssets != null
            && asset.Defense.effectAssets.Length > 0
            && asset.Defense.effectAssets.All((effect) => effect != null)
            && asset.sprite != null)
            && LoadDefense("P1_GuardRoom").Facility.SupportsWork(FacilityWorkType.Guard);
    }

    private static bool VerifyEffectAssetsDriveDamage()
    {
        using DefenseScenarioWorld world = new DefenseScenarioWorld();
        BuildingSO source = LoadDefense("P1_SpikeTrap");
        BuildingSO clone = Object.Instantiate(source);
        world.TrackScriptableObject(clone);
        clone.defense = new DefenseFacilityData
        {
            enabled = source.Defense.enabled,
            concept = source.Defense.concept,
            triggerTimings = source.Defense.triggerTimings,
            targetRule = source.Defense.targetRule,
            cooldownSeconds = source.Defense.cooldownSeconds,
            periodicIntervalSeconds = source.Defense.periodicIntervalSeconds,
            range = source.Defense.range,
            star = source.Defense.star,
            combatLogText = source.Defense.combatLogText,
            effectAssets = source.Defense.effectAssets,
            effects = Array.Empty<DefenseEffectData>()
        };

        world.PlaceDefense(clone, new Vector2Int(2, 0));
        Character intruder = world.CreateIntruder(new Vector2Int(1, 0));
        float before = intruder.CurrentHealth;
        List<DefenseActivationReport> reports = DefenseFacilityResolver.TriggerAt(
            world.Grid,
            intruder,
            new Vector2Int(1, 0),
            DefenseTriggerTiming.OnEnter);

        return reports.Count == 1
            && reports[0].TotalDamage > 0f
            && intruder.CurrentHealth < before;
    }

    private static bool VerifyTriggerDamageAndEvent()
    {
        using DefenseScenarioWorld world = new DefenseScenarioWorld();
        DefenseFacility spike = world.PlaceDefense("P1_SpikeTrap", new Vector2Int(2, 0));
        Character intruder = world.CreateIntruder(new Vector2Int(1, 0));
        CountingDefenseTriggerListener listener = new CountingDefenseTriggerListener();

        List<DefenseActivationReport> reports = DefenseFacilityResolver.TriggerAt(
            world.Grid,
            intruder,
            new Vector2Int(1, 0),
            DefenseTriggerTiming.OnEnter);

        bool valid = reports.Count == 1
            && reports[0].Facility == spike
            && reports[0].TotalDamage > 0f
            && intruder.CurrentHealth < intruder.MaxHealth
            && listener.Count == 1;

        listener.Dispose();
        return valid;
    }

    private static bool VerifyDamagedDisableAndRepair()
    {
        using DefenseScenarioWorld world = new DefenseScenarioWorld();
        DefenseFacility spike = world.PlaceDefense("P1_SpikeTrap", new Vector2Int(2, 0));
        Character intruder = world.CreateIntruder(new Vector2Int(1, 0));
        Character worker = world.CreateWorker(new Vector2Int(0, 0));

        spike.SetDamaged(true);
        bool disabled = DefenseFacilityResolver.TriggerAt(
            world.Grid,
            intruder,
            new Vector2Int(1, 0),
            DefenseTriggerTiming.OnEnter).Count == 0;

        bool repairCandidate = worker.TryGetAbility(out AbilityWork work)
            && work.TrySetPriorityWorkTarget(spike, FacilityWorkType.Repair, world.Grid.SearchPath(worker.GetNowXY()), out _)
            && work.AssignedWorkType == FacilityWorkType.Repair;
        bool repaired = ExecuteRepairForTest(work, spike) && !spike.IsDamaged;

        return disabled && repairCandidate && repaired;
    }

    private static bool VerifyPoisonCorrosion()
    {
        using DefenseScenarioWorld world = new DefenseScenarioWorld();
        world.PlaceDefense("P1_PoisonPool", new Vector2Int(2, 0));
        world.PlaceDefense("P1_SpikeTrap", new Vector2Int(4, 0));
        Character intruder = world.CreateIntruder(new Vector2Int(1, 0));

        float beforePoison = intruder.CurrentHealth;
        DefenseFacilityResolver.TriggerAt(world.Grid, intruder, new Vector2Int(1, 0), DefenseTriggerTiming.OnEnter);
        float poisonDamage = beforePoison - intruder.CurrentHealth;
        float beforeSpike = intruder.CurrentHealth;
        DefenseFacilityResolver.TriggerAt(world.Grid, intruder, new Vector2Int(3, 0), DefenseTriggerTiming.OnEnter);
        float spikeDamageAfterCorrosion = beforeSpike - intruder.CurrentHealth;

        return poisonDamage > 0f && spikeDamageAfterCorrosion > 14f;
    }

    private static bool VerifyFireBurn()
    {
        using DefenseScenarioWorld world = new DefenseScenarioWorld();
        world.PlaceDefense("P1_FireVent", new Vector2Int(2, 0));
        Character intruder = world.CreateIntruder(new Vector2Int(1, 0));

        DefenseFacilityResolver.TriggerAt(world.Grid, intruder, new Vector2Int(1, 0), DefenseTriggerTiming.OnEnter);
        float beforeTick = intruder.CurrentHealth;
        float tickDamage = DefenseEffectResolver.TickStatuses(intruder, 2f);

        return tickDamage > 0f && intruder.CurrentHealth < beforeTick;
    }

    private static bool VerifyLightningCharge()
    {
        using DefenseScenarioWorld world = new DefenseScenarioWorld();
        world.PlaceDefense("P1_LightningPillar", new Vector2Int(1, 0));
        world.PlaceDefense("P1_LightningPillar", new Vector2Int(3, 0));
        world.PlaceDefense("P1_LightningPillar", new Vector2Int(5, 0));
        Character intruder = world.CreateIntruder(new Vector2Int(0, 0));

        float before = intruder.CurrentHealth;
        DefenseFacilityResolver.TriggerAt(world.Grid, intruder, new Vector2Int(0, 0), DefenseTriggerTiming.OnEnter);
        DefenseFacilityResolver.TriggerAt(world.Grid, intruder, new Vector2Int(2, 0), DefenseTriggerTiming.OnEnter);
        DefenseFacilityResolver.TriggerAt(world.Grid, intruder, new Vector2Int(4, 0), DefenseTriggerTiming.OnEnter);
        float totalDamage = before - intruder.CurrentHealth;

        return totalDamage > 24f;
    }

    private static bool VerifyIceSlow()
    {
        using DefenseScenarioWorld world = new DefenseScenarioWorld();
        world.PlaceDefense("P1_IceVent", new Vector2Int(2, 0));
        Character intruder = world.CreateIntruder(new Vector2Int(1, 0));

        List<DefenseActivationReport> reports = DefenseFacilityResolver.TriggerAt(
            world.Grid,
            intruder,
            new Vector2Int(1, 0),
            DefenseTriggerTiming.OnEnter);

        return reports.Count == 1
            && reports[0].TotalDamage > 0f
            && reports[0].MovementDelaySeconds > 0f;
    }

    private static bool VerifyGuardRoom()
    {
        using DefenseScenarioWorld world = new DefenseScenarioWorld();
        DefenseFacility guardRoom = world.PlaceDefense("P1_GuardRoom", new Vector2Int(2, 0));
        Character intruder = world.CreateIntruder(new Vector2Int(1, 0));

        List<DefenseActivationReport> reports = DefenseFacilityResolver.TriggerAt(
            world.Grid,
            intruder,
            new Vector2Int(1, 0),
            DefenseTriggerTiming.OnEnter);

        return guardRoom.Facility.SupportsWork(FacilityWorkType.Guard)
            && guardRoom.Facility.requiredWorkers == 1
            && reports.Count == 1
            && reports[0].TotalDamage > 0f
            && reports[0].EffectTags.Contains("경비 교전");
    }

    private static BuildingSO LoadDefense(string assetName)
    {
        return AssetDatabase.LoadAssetAtPath<BuildingSO>(
            $"Assets/Resources/SO/Building/P1/{assetName}.asset");
    }

    private static bool ExecuteRepairForTest(AbilityWork work, BuildableObject target)
    {
        if (work == null || target == null)
        {
            return false;
        }

        typeof(AbilityWork)
            .GetField("assignedWorkType", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(work, FacilityWorkType.Repair);
        work.assignedShop = target;

        MethodInfo method = typeof(AbilityWork).GetMethod("ExecuteRepairWork", BindingFlags.Instance | BindingFlags.NonPublic);
        if (method?.Invoke(work, null) is not IEnumerator routine)
        {
            return false;
        }

        routine.MoveNext();
        routine.MoveNext();
        return !target.IsDamaged;
    }

    private sealed class DefenseScenarioWorld : IDisposable
    {
        private static readonly FieldInfo GridSystemInstanceField =
            typeof(GridSystemManager).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly FieldInfo GridField =
            typeof(GridSystemManager).GetField("<grid>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo CharacterAwakeMethod =
            typeof(Character).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly GridSystemManager previousGridSystem;
        private readonly List<GameObject> objects = new List<GameObject>();
        private readonly List<ScriptableObject> scriptableObjects = new List<ScriptableObject>();

        public DefenseScenarioWorld()
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

            GameObject gridSystemObject = new GameObject("Defense Scenario GridSystemManager");
            objects.Add(gridSystemObject);
            GridSystemManager manager = gridSystemObject.AddComponent<GridSystemManager>();
            GridField?.SetValue(manager, Grid);
            GridSystemInstanceField?.SetValue(null, manager);
        }

        public Grid Grid { get; }

        public DefenseFacility PlaceDefense(string assetName, Vector2Int position)
        {
            BuildingSO buildingData = LoadDefense(assetName);
            return PlaceDefense(buildingData, position);
        }

        public DefenseFacility PlaceDefense(BuildingSO buildingData, Vector2Int position)
        {
            GridBuildingFactory factory = new GridBuildingFactory();
            BuildableObject building = factory.Create(Grid, buildingData, position);
            if (building is not DefenseFacility defense)
            {
                throw new InvalidOperationException($"{buildingData?.name ?? "Defense asset"} did not create DefenseFacility.");
            }

            objects.Add(defense.gameObject);
            defense.SetGrid(Grid);
            defense.Initialization(buildingData, position);
            bool registered = Grid.RegisterOccupant(
                defense,
                buildingData.Placement.Layer,
                buildingData.GetGridPosList(position),
                buildingData.Placement.IsMovement);
            if (!registered)
            {
                throw new InvalidOperationException($"{buildingData.name} could not be registered.");
            }

            return defense;
        }

        public void TrackScriptableObject(ScriptableObject scriptableObject)
        {
            if (scriptableObject != null && !scriptableObjects.Contains(scriptableObject))
            {
                scriptableObjects.Add(scriptableObject);
            }
        }

        public Character CreateIntruder(Vector2Int position)
        {
            CharacterSO data = AssetDatabase.LoadAssetAtPath<CharacterSO>(
                "Assets/Resources/SO/Character/Intruders/Intruder_Breakthrough.asset");
            GameObject obj = CreateCharacterObject("Defense Scenario Intruder");
            Character character = obj.GetComponent<Character>();
            InitializeCharacter(character, data, position);
            return character;
        }

        public Character CreateWorker(Vector2Int position)
        {
            CharacterSO data = ScriptableObject.CreateInstance<CharacterSO>();
            scriptableObjects.Add(data);
            data.characterType = CharacterType.NPC;
            data.characterName = "Defense Repair Worker";
            data.speciesTag = "Orc";
            GameObject obj = CreateCharacterObject("Defense Scenario Worker");
            obj.AddComponent<AbilityWork>();
            Character character = obj.GetComponent<Character>();
            InitializeCharacter(character, data, position);
            character.RefreshAbilityCache();
            return character;
        }

        public void Dispose()
        {
            GridSystemInstanceField?.SetValue(null, previousGridSystem);
            foreach (GameObject obj in objects.Where((obj) => obj != null))
            {
                Object.DestroyImmediate(obj);
            }

            foreach (ScriptableObject obj in scriptableObjects.Where((obj) => obj != null))
            {
                Object.DestroyImmediate(obj);
            }
        }

        private GameObject CreateCharacterObject(string name)
        {
            GameObject obj = new GameObject(name);
            objects.Add(obj);
            obj.AddComponent<SpriteRenderer>();
            obj.AddComponent<AbilityMove>();
            obj.AddComponent<Character>();
            return obj;
        }

        private void InitializeCharacter(Character character, CharacterSO data, Vector2Int position)
        {
            CharacterAwakeMethod?.Invoke(character, null);
            character.RefreshAbilityCache();
            character.Initialization(data);
            character.SetLifecycleState(Character.LifecycleState.Active);
            character.transform.position = Grid.GetWorldPos(position);
        }
    }

    private sealed class TestHallwayOccupant : IGridOccupant
    {
        public int GridId => 0;
        public bool IsGridDestroyed => false;
        public bool IsGridVisitable => false;
        public bool IsGridMovement => true;
    }

    private sealed class CountingDefenseTriggerListener : UtilEventListener<DefenseFacilityTriggeredEvent>, IDisposable
    {
        public int Count { get; private set; }

        public CountingDefenseTriggerListener()
        {
            this.EventStartListening<DefenseFacilityTriggeredEvent>();
        }

        public void OnTriggerEvent(DefenseFacilityTriggeredEvent eventType)
        {
            Count++;
        }

        public void Dispose()
        {
            this.EventStopListening<DefenseFacilityTriggeredEvent>();
        }
    }
}
