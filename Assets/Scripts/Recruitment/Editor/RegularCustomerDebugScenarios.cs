using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class RegularCustomerDebugScenarios
{
    [MenuItem("DungeonStory/Debug/Recruitment/Run P2 Regular Customer Scenarios")]
    public static void RunFromMenu()
    {
        bool success = RunAll(true);
        if (!success)
        {
            Debug.LogError("P2 regular customer scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        List<string> errors = new List<string>();

        RunScenario("방문 횟수와 평균 만족도 기록", VerifyVisitCountAndAverageSatisfaction, errors);
        RunScenario("단골 기준 달성", VerifyRegularThreshold, errors);
        RunScenario("영입 후보 기준 달성", VerifyRecruitCandidateThreshold, errors);
        RunScenario("영입 결과와 소비 고객 제외", VerifyRecruitmentConsumesCustomer, errors);
        RunScenario("낮은 만족도는 단골 제외", VerifyLowSatisfactionDoesNotBecomeRegular, errors);
        RunScenario("방문 이벤트 런타임 연결", VerifyRuntimeEvents, errors);

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
            Debug.Log("P2 regular customer scenarios passed.");
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

    private static bool VerifyVisitCountAndAverageSatisfaction()
    {
        RegularCustomerState state = new RegularCustomerState();
        RegularCustomerRules rules = CreateTestRules();
        Character customer = CreateCustomer(101, "Slime Regular", "Slime", 80f);

        state.RecordVisit(customer, rules);
        customer.stats[Character.Condition.MOOD] = 60f;
        RegularCustomerVisitResult result = state.RecordVisit(customer, rules);

        bool valid = result.Success
            && result.Record.VisitCount == 2
            && Mathf.Approximately(result.Record.AverageSatisfaction, 70f)
            && result.Record.Status == RegularCustomerStatus.Visitor;

        DestroyCustomer(customer);
        return valid;
    }

    private static bool VerifyRegularThreshold()
    {
        RegularCustomerState state = new RegularCustomerState();
        RegularCustomerRules rules = CreateTestRules();
        Character customer = CreateCustomer(102, "Orc Regular", "Orc", 70f);

        state.RecordVisit(customer, rules);
        state.RecordVisit(customer, rules);
        RegularCustomerVisitResult result = state.RecordVisit(customer, rules);

        bool valid = result.Success
            && result.BecameRegular
            && result.Record.IsRegular
            && result.Record.Status == RegularCustomerStatus.Regular;

        DestroyCustomer(customer);
        return valid;
    }

    private static bool VerifyRecruitCandidateThreshold()
    {
        RegularCustomerState state = new RegularCustomerState();
        RegularCustomerRules rules = CreateTestRules();
        Character customer = CreateCustomer(103, "Vampire Candidate", "Vampire", 85f);

        state.RecordVisit(customer, rules);
        state.RecordVisit(customer, rules);
        state.RecordVisit(customer, rules);
        RegularCustomerVisitResult result = state.RecordVisit(customer, rules);

        bool valid = result.Success
            && result.BecameRecruitCandidate
            && result.Record.IsRecruitCandidate
            && result.Record.Status == RegularCustomerStatus.RecruitCandidate;

        DestroyCustomer(customer);
        return valid;
    }

    private static bool VerifyRecruitmentConsumesCustomer()
    {
        RegularCustomerState state = new RegularCustomerState();
        RegularCustomerRules rules = CreateTestRules();
        Character customer = CreateCustomer(104, "Goblin Recruit", "Goblin", 90f);

        for (int i = 0; i < 4; i++)
        {
            state.RecordVisit(customer, rules);
        }

        bool recruited = state.TryRecruit(customer.data.id, out RegularCustomerRecruitResult result);
        bool canSpawnAgain = RegularCustomerService.CanSpawnAsCustomer(customer.data, state);
        bool valid = recruited
            && result.Success
            && result.ResultType == CharacterType.NPC
            && (result.Capabilities & RecruitCapability.Staff) != 0
            && (result.Capabilities & RecruitCapability.Defense) != 0
            && (result.Capabilities & RecruitCapability.Expedition) != 0
            && state.RecruitedCharacters.Count == 1
            && !canSpawnAgain;

        DestroyCustomer(customer);
        return valid;
    }

    private static bool VerifyLowSatisfactionDoesNotBecomeRegular()
    {
        RegularCustomerState state = new RegularCustomerState();
        RegularCustomerRules rules = CreateTestRules();
        Character customer = CreateCustomer(105, "Low Mood Customer", "Slime", 30f);

        state.RecordVisit(customer, rules);
        state.RecordVisit(customer, rules);
        RegularCustomerVisitResult result = state.RecordVisit(customer, rules);

        bool valid = result.Success
            && !result.Record.IsRegular
            && !result.Record.IsRecruitCandidate
            && result.Record.Status == RegularCustomerStatus.Visitor;

        DestroyCustomer(customer);
        return valid;
    }

    private static bool VerifyRuntimeEvents()
    {
        GameObject runtimeObject = new GameObject("RegularCustomerRuntime_Test");
        RegularCustomerRuntime runtime = runtimeObject.AddComponent<RegularCustomerRuntime>();
        Character customer = CreateCustomer(106, "Runtime Candidate", "Orc", 85f);
        BuildableObject facility = CreateFacility();

        using CountingRegularListener regularListener = new CountingRegularListener();
        using CountingCandidateListener candidateListener = new CountingCandidateListener();

        for (int i = 0; i < 4; i++)
        {
            runtime.OnTriggerEvent(new FacilityVisitEvent(customer, facility));
        }

        bool recruitSuccess = runtime.TryRecruit(customer.data.id, out RegularCustomerRecruitResult recruitResult);
        bool valid = regularListener.Count == 1
            && candidateListener.Count == 1
            && recruitSuccess
            && recruitResult.Success
            && runtime.State.IsRecruited(customer.data.id);

        Object.DestroyImmediate(facility.BuildingData);
        Object.DestroyImmediate(facility.gameObject);
        DestroyCustomer(customer);
        Object.DestroyImmediate(runtimeObject);
        return valid;
    }

    private static RegularCustomerRules CreateTestRules()
    {
        return new RegularCustomerRules
        {
            regularVisitThreshold = 3,
            regularAverageSatisfactionThreshold = 65f,
            recruitCandidateVisitThreshold = 4,
            recruitCandidateAverageSatisfactionThreshold = 75f,
            defaultRecruitCapabilities = RecruitCapability.All
        };
    }

    private static Character CreateCustomer(int id, string name, string speciesTag, float mood)
    {
        GameObject obj = new GameObject(name);
        obj.AddComponent<SpriteRenderer>();
        Character character = obj.AddComponent<Character>();

        CharacterSO data = ScriptableObject.CreateInstance<CharacterSO>();
        data.id = id;
        data.characterType = CharacterType.Customer;
        data.role = CharacterRole.Regular;
        data.characterName = name;
        data.speciesTag = speciesTag;

        character.data = data;
        character.characterType = CharacterType.Customer;
        character.stats ??= new Dictionary<Character.Condition, float>();
        character.stats[Character.Condition.HUNGER] = 100f;
        character.stats[Character.Condition.SLEEP] = 100f;
        character.stats[Character.Condition.FUN] = 100f;
        character.stats[Character.Condition.MOOD] = mood;
        return character;
    }

    private static BuildableObject CreateFacility()
    {
        GameObject obj = new GameObject("Regular Customer Facility");
        BuildableObject facility = obj.AddComponent<BuildableObject>();
        BuildingSO data = ScriptableObject.CreateInstance<BuildingSO>();
        data.id = 9106;
        data.objectName = "Regular Customer Facility";
        data.width = 1;
        data.height = 1;
        data.category = BuildingCategory.Shop;
        data.facility = new FacilityData
        {
            roles = FacilityRole.Meal,
            capacity = 1,
            supportedWorkTypes = FacilityWorkType.Operate
        };
        facility.Initialization(data, Vector2Int.zero);
        return facility;
    }

    private static void DestroyCustomer(Character character)
    {
        if (character == null) return;

        if (character.data != null)
        {
            Object.DestroyImmediate(character.data);
        }

        Object.DestroyImmediate(character.gameObject);
    }

    private sealed class CountingRegularListener : UtilEventListener<RegularCustomerBecameRegularEvent>, IDisposable
    {
        public int Count { get; private set; }

        public CountingRegularListener()
        {
            this.EventStartListening<RegularCustomerBecameRegularEvent>();
        }

        public void OnTriggerEvent(RegularCustomerBecameRegularEvent eventType)
        {
            Count++;
        }

        public void Dispose()
        {
            this.EventStopListening<RegularCustomerBecameRegularEvent>();
        }
    }

    private sealed class CountingCandidateListener : UtilEventListener<RecruitCandidateDiscoveredEvent>, IDisposable
    {
        public int Count { get; private set; }

        public CountingCandidateListener()
        {
            this.EventStartListening<RecruitCandidateDiscoveredEvent>();
        }

        public void OnTriggerEvent(RecruitCandidateDiscoveredEvent eventType)
        {
            Count++;
        }

        public void Dispose()
        {
            this.EventStopListening<RecruitCandidateDiscoveredEvent>();
        }
    }
}
