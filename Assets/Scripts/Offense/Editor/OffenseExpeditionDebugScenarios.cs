using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class OffenseExpeditionDebugScenarios
{
    [MenuItem("DungeonStory/Debug/Offense/Run P3 Expedition Scenarios")]
    public static void RunFromMenu()
    {
        bool success = RunAll(true);
        if (!success)
        {
            Debug.LogError("P3 offense expedition scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        List<string> errors = new List<string>();

        RunScenario("원정 가능 인력 필터", VerifyAvailableMemberFilter, errors);
        RunScenario("원정 출발 시 던전 운영 제외", VerifyStartExpeditionRemovesMembersFromDungeon, errors);
        RunScenario("필요 인력 부족 실패", VerifyRequiredMemberValidation, errors);
        RunScenario("자동 원정 성공 결과", VerifySuccessfulAutoResolve, errors);
        RunScenario("원정 실패 부상/사망 처리", VerifyFailureCanKillMember, errors);
        RunScenario("원정 편성 패널 생성", VerifyPanelCreation, errors);

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
            Debug.Log("P3 offense expedition scenarios passed.");
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

    private static bool VerifyAvailableMemberFilter()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        CharacterActor worker = scenario.CreateCharacter("Worker", CharacterType.NPC, CharacterRole.Regular, 8);
        CharacterActor owner = scenario.CreateCharacter("Owner", CharacterType.NPC, CharacterRole.Owner, 10);
        CharacterActor customer = scenario.CreateCharacter("Customer", CharacterType.Customer, CharacterRole.Regular, 8);

        IReadOnlyList<CharacterActor> available = scenario.Expedition.Runtime.GetAvailableMemberActors();
        return available.Contains(CharacterActor.From(worker))
            && !available.Contains(CharacterActor.From(owner))
            && !available.Contains(CharacterActor.From(customer));
    }

    private static bool VerifyStartExpeditionRemovesMembersFromDungeon()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        CharacterActor worker = scenario.CreateCharacter("Worker", CharacterType.NPC, CharacterRole.Regular, 10);

        bool started = scenario.Expedition.Runtime.TryStartExpedition(
            "food_farm",
            new[] { CharacterActor.From(worker) },
            out OffenseExpeditionRun expedition,
            out _);

        return started
            && expedition != null
            && worker.IsOnExpedition
            && !worker.CanRunAi
            && worker.VisualRenderer != null
            && !worker.VisualRenderer.enabled
            && !scenario.Expedition.Runtime.GetAvailableMemberActors().Contains(CharacterActor.From(worker));
    }

    private static bool VerifyRequiredMemberValidation()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        scenario.WorldMap.Runtime.TryUpgradeRecon(out _);
        CharacterActor worker = scenario.CreateCharacter("Worker", CharacterType.NPC, CharacterRole.Regular, 10);

        bool started = scenario.Expedition.Runtime.TryStartExpedition(
            "old_armory",
            new[] { CharacterActor.From(worker) },
            out _,
            out string message);

        return !started && message.Contains("필요 인력 부족");
    }

    private static bool VerifySuccessfulAutoResolve()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        using CountingCompletionListener completions = new CountingCompletionListener();
        CharacterActor worker = scenario.CreateCharacter("StrongWorker", CharacterType.NPC, CharacterRole.Regular, 12);

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
            && result.rewardSummaries.Length > 0
            && completions.Count == 1
            && worker.gameObject.activeSelf
            && worker.VisualRenderer != null
            && worker.VisualRenderer.enabled
            && !worker.IsOnExpedition
            && worker.CanRunAi
            && worker.stats[CharacterCondition.SLEEP] < 100f;
    }

    private static bool VerifyFailureCanKillMember()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        scenario.WorldMap.Runtime.SetTargetsForDebug(new[]
        {
            CreateTarget("deadly_test", "위험한 테스트 원정", 1f, 240f, 999f, 1)
        });
        CharacterActor worker = scenario.CreateCharacter("WeakWorker", CharacterType.NPC, CharacterRole.Regular, 1);

        bool started = scenario.Expedition.Runtime.TryStartExpedition(
            "deadly_test",
            new[] { CharacterActor.From(worker) },
            out OffenseExpeditionRun expedition,
            out _);
        bool completed = scenario.Expedition.Runtime.CompleteExpeditionForDebug(
            expedition?.ExpeditionId,
            false,
            out OffenseExpeditionResult result);

        return started
            && completed
            && result != null
            && !result.success
            && result.members.Any((member) => member != null && !member.survived)
            && worker.IsDead;
    }

    private static bool VerifyPanelCreation()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        scenario.WorldMap.Runtime.TrySelectTarget("food_farm", out _, out _);
        scenario.CreateCharacter("Worker", CharacterType.NPC, CharacterRole.Regular, 10);

        OffenseExpeditionPanel panel = scenario.Expedition.Runtime.ShowExpeditionPanel();
        bool valid = panel != null
            && panel.gameObject.activeSelf
            && Object.FindFirstObjectByType<Canvas>() != null;

        if (panel != null)
        {
            Object.DestroyImmediate(panel.transform.root.gameObject);
        }

        return valid;
    }

    private static OffenseTargetDefinition CreateTarget(
        string id,
        string title,
        float distance,
        float danger,
        float requiredPower,
        int requiredMembers)
    {
        return new OffenseTargetDefinition
        {
            id = id,
            title = title,
            description = "디버그 원정 대상",
            kind = OffenseTargetKind.SpecialEvent,
            distance = distance,
            danger = danger,
            durationSeconds = 5f,
            requiredMembers = requiredMembers,
            requiredPower = requiredPower,
            rewards = new[]
            {
                new OffenseRewardPreview
                {
                    category = OffenseRewardCategory.Money,
                    label = "테스트 보상",
                    amount = 1
                }
            }
        };
    }

    private sealed class ScenarioRuntime : IDisposable
    {
        private readonly List<Object> objects = new List<Object>();

        public WorldMapFixture WorldMap { get; }
        public ExpeditionFixture Expedition { get; }

        public ScenarioRuntime()
        {
            WorldMap = new WorldMapFixture();
            Expedition = new ExpeditionFixture();
        }

        public CharacterActor CreateCharacter(
            string name,
            CharacterType type,
            CharacterRole role,
            int statValue)
        {
            CharacterSO data = ScriptableObject.CreateInstance<CharacterSO>();
            data.id = 9600 + objects.Count;
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
            WorldMap.Dispose();
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
            obj = new GameObject("Offense World Map Fixture");
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
            obj = new GameObject("Offense Expedition Fixture");
            Runtime = obj.AddComponent<OffenseExpeditionRuntime>();
        }

        public void Dispose()
        {
            Object.DestroyImmediate(obj);
        }
    }

    private sealed class CountingCompletionListener :
        UtilEventListener<OffenseExpeditionCompletedEvent>,
        IDisposable
    {
        public int Count { get; private set; }

        public CountingCompletionListener()
        {
            this.EventStartListening<OffenseExpeditionCompletedEvent>();
        }

        public void OnTriggerEvent(OffenseExpeditionCompletedEvent eventType)
        {
            if (eventType.result != null)
            {
                Count++;
            }
        }

        public void Dispose()
        {
            this.EventStopListening<OffenseExpeditionCompletedEvent>();
        }
    }
}
