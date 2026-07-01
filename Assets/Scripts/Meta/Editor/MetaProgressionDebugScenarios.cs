using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class MetaProgressionDebugScenarios
{
    [MenuItem("DungeonStory/Debug/Meta/Run P2 Meta Progression Scenarios")]
    public static void RunFromMenu()
    {
        bool success = RunAll(true);
        if (!success)
        {
            Debug.LogError("P2 meta progression scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        List<string> errors = new List<string>();

        RunScenario("사장 사망 런 결과 정산", VerifyOwnerDeathCreatesRunResult, errors);
        RunScenario("생존 보상 우선 계승 화폐", VerifySurvivalRewardDominatesDiscoveryOnly, errors);
        RunScenario("운영 지식 강화 효과", VerifyOperationKnowledgeUpgrades, errors);
        RunScenario("설계 보존 강화 효과", VerifyRecipePreservation, errors);
        RunScenario("사장 생존 강화 효과", VerifyOwnerSurvivalUpgrades, errors);

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
            Debug.Log("P2 meta progression scenarios passed.");
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

    private static bool VerifyOwnerDeathCreatesRunResult()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        using CountingEventAlertRequestListener alerts = new CountingEventAlertRequestListener();
        using CountingRunResultReadyListener results = new CountingRunResultReadyListener();

        scenario.Runtime.OnTriggerEvent(new OperatingDayStartedEvent(4));
        scenario.Runtime.OnTriggerEvent(new OperatingDayReportEvent(new OperatingDayReport { day = 1 }));
        scenario.Runtime.OnTriggerEvent(new OperatingDayReportEvent(new OperatingDayReport { day = 2 }));
        scenario.Runtime.OnTriggerEvent(new InvasionStartedEvent(new InvasionThreatSnapshot(
            120f,
            InvasionThreatStage.Candidate,
            new InvasionThreatFactors(2f, 2f, 2f, 1f),
            0f,
            0f)));
        scenario.Runtime.OnTriggerEvent(new InvasionResolvedEvent(true, 1f));
        scenario.Runtime.OnTriggerEvent(new FacilityVisitEvent(null, CreateFacility(9001, "발견 시설")));

        Character owner = CreateOwner();
        RunResultSnapshot result = scenario.Runtime.EndRun(owner, "테스트 사망");

        bool valid = result != null
            && result.legacyCurrency > 0
            && result.defendedInvasionCount == 1
            && result.maxThreatStage == InvasionThreatStage.Candidate
            && result.firstDiscoveredFacilityCount == 1
            && scenario.Runtime.State.LifetimeEarnedCurrency == result.legacyCurrency
            && results.Count == 1
            && alerts.Requests.Any((request) => request.Title == "런 결과 정산")
            && result.ToDetailText().Contains("계승되지 않음");

        Object.DestroyImmediate(owner.gameObject);
        return valid;
    }

    private static bool VerifySurvivalRewardDominatesDiscoveryOnly()
    {
        RunResultSnapshot shortDiscovery = new RunResultSnapshot
        {
            survivalSeconds = 30f,
            survivedOperatingDays = 1,
            settlementCount = 0,
            firstDiscoveredFacilityCount = 15,
            firstUnlockedRecipeCount = 3,
            difficultyMultiplier = 1f
        };
        RunResultSnapshot longSurvival = new RunResultSnapshot
        {
            survivalSeconds = 180f * 8f,
            survivedOperatingDays = 8,
            settlementCount = 7,
            firstDiscoveredFacilityCount = 1,
            firstUnlockedRecipeCount = 0,
            difficultyMultiplier = 1f
        };

        return MetaProgressionCalculator.CalculateLegacyCurrency(longSurvival)
            > MetaProgressionCalculator.CalculateLegacyCurrency(shortDiscovery);
    }

    private static bool VerifyOperationKnowledgeUpgrades()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        scenario.Runtime.State.AddCurrency(500);

        bool purchasedFacility = scenario.Runtime.TryPurchaseUpgrade(
            MetaUpgradeId.StartingFacilityCandidatePlusOne,
            out _);
        bool purchasedBasic = scenario.Runtime.TryPurchaseUpgrade(
            MetaUpgradeId.BasicPurchaseListExpansion,
            out _);

        BuildingSO first = CreateBuilding(9101, "1성 테스트 시설 A", false);
        BuildingSO second = CreateBuilding(9102, "1성 테스트 시설 B", false);
        IReadOnlyList<FacilityShopOffer> offers = FacilityShopService.CreateBasicPurchaseOffers(
            new[] { second, first },
            new FacilityShopUnlockState());

        bool valid = purchasedFacility
            && purchasedBasic
            && scenario.Runtime.GetStartingFacilityCandidateBonus() == 1
            && offers.Any((offer) => offer.Building == first || offer.Building == second);

        Object.DestroyImmediate(first);
        Object.DestroyImmediate(second);
        return valid;
    }

    private static bool VerifyRecipePreservation()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        scenario.Runtime.State.AddCurrency(500);
        scenario.Runtime.TryPurchaseUpgrade(MetaUpgradeId.SpecialRecipeRecordSlot, out _);

        FacilityBlueprintSO blueprint = ScriptableObject.CreateInstance<FacilityBlueprintSO>();
        blueprint.id = 9201;
        blueprint.blueprintName = "보존 테스트 설계도";
        BlueprintResearchUnlockResult unlock = new BlueprintResearchUnlockResult(
            blueprint,
            Array.Empty<string>(),
            Array.Empty<string>(),
            new[] { "recipe_preserve_test" });
        scenario.Runtime.OnTriggerEvent(new BlueprintResearchCompletedEvent(blueprint, unlock));
        Character owner = CreateOwner();
        scenario.Runtime.EndRun(owner, "테스트 사망");

        bool valid = scenario.Runtime.IsRecipePreserved("recipe_preserve_test");

        Object.DestroyImmediate(owner.gameObject);
        Object.DestroyImmediate(blueprint);
        return valid;
    }

    private static bool VerifyOwnerSurvivalUpgrades()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        scenario.Runtime.State.AddCurrency(500);
        bool healthPurchased = scenario.Runtime.TryPurchaseUpgrade(MetaUpgradeId.OwnerSurvivalBonus, out _);
        bool traitPurchased = scenario.Runtime.TryPurchaseUpgrade(MetaUpgradeId.StartingOwnerTraitCandidatePlusOne, out _);
        bool warningPurchased = scenario.Runtime.TryPurchaseUpgrade(MetaUpgradeId.InvasionWarningAccuracy, out _);

        Character owner = CreateOwner();
        bool valid = healthPurchased
            && traitPurchased
            && warningPurchased
            && scenario.Runtime.GetOwnerMaxHealthMultiplier() > 1f
            && scenario.Runtime.GetStartingOwnerTraitCandidateBonus() == 1
            && scenario.Runtime.GetInvasionWarningThresholdMultiplier() < 1f
            && owner.MaxHealth > 100f;

        Object.DestroyImmediate(owner.gameObject);
        return valid;
    }

    private static Character CreateOwner()
    {
        CharacterSO data = ScriptableObject.CreateInstance<CharacterSO>();
        data.id = 9301;
        data.characterName = "테스트 사장";
        data.characterType = CharacterType.NPC;
        data.role = CharacterRole.Owner;
        data.speciesTag = "Orc";

        GameObject obj = new GameObject("Meta Test Owner");
        obj.AddComponent<SpriteRenderer>();
        Character character = obj.AddComponent<Character>();
        obj.AddComponent<AbilityMove>();
        obj.AddComponent<AbilityWork>();
        obj.AddComponent<AIBrain>();
        character.RefreshAbilityCache();
        character.Initialization(data);
        character.SetLifecycleState(Character.LifecycleState.Active);
        return character;
    }

    private static BuildableObject CreateFacility(int id, string name)
    {
        BuildingSO building = CreateBuilding(id, name, false);
        GameObject obj = new GameObject(name);
        BuildableObject facility = obj.AddComponent<BuildableObject>();
        facility.Initialization(building, Vector2Int.zero);
        return facility;
    }

    private static BuildingSO CreateBuilding(int id, string name, bool defense)
    {
        BuildingSO building = ScriptableObject.CreateInstance<BuildingSO>();
        building.id = id;
        building.objectName = name;
        building.width = 1;
        building.height = 1;
        building.layer = GridLayer.Building;
        building.category = defense ? BuildingCategory.Special : BuildingCategory.Shop;
        if (defense)
        {
            building.Defense.enabled = true;
            building.Defense.concept = DefenseAttackConcept.Physical;
            building.Defense.star = 1;
        }

        return building;
    }

    private sealed class ScenarioRuntime : IDisposable
    {
        private readonly GameObject runtimeObject;

        public MetaProgressionRuntime Runtime { get; }

        public ScenarioRuntime()
        {
            runtimeObject = new GameObject("Meta Progression Scenario Runtime");
            Runtime = runtimeObject.AddComponent<MetaProgressionRuntime>();
            Runtime.SetShowRunResultPanel(false);
            Runtime.StartNewRun();
        }

        public void Dispose()
        {
            foreach (BuildableObject facility in Object.FindObjectsByType<BuildableObject>(FindObjectsSortMode.None)
                         .Where((building) => building != null && building.name.Contains("발견 시설")))
            {
                BuildingSO buildingData = facility.BuildingData;
                Object.DestroyImmediate(facility.gameObject);
                if (buildingData != null)
                {
                    Object.DestroyImmediate(buildingData);
                }
            }

            Object.DestroyImmediate(runtimeObject);
        }
    }

    private sealed class CountingRunResultReadyListener :
        UtilEventListener<RunResultReadyEvent>,
        IDisposable
    {
        public int Count { get; private set; }

        public CountingRunResultReadyListener()
        {
            this.EventStartListening<RunResultReadyEvent>();
        }

        public void OnTriggerEvent(RunResultReadyEvent eventType)
        {
            if (eventType.result != null)
            {
                Count++;
            }
        }

        public void Dispose()
        {
            this.EventStopListening<RunResultReadyEvent>();
        }
    }

    private sealed class CountingEventAlertRequestListener :
        UtilEventListener<EventAlertRequestedEvent>,
        IDisposable
    {
        private readonly List<EventAlertRequest> requests = new List<EventAlertRequest>();

        public IReadOnlyList<EventAlertRequest> Requests => requests;

        public CountingEventAlertRequestListener()
        {
            this.EventStartListening<EventAlertRequestedEvent>();
        }

        public void OnTriggerEvent(EventAlertRequestedEvent eventType)
        {
            if (eventType.request != null)
            {
                requests.Add(eventType.request);
            }
        }

        public void Dispose()
        {
            this.EventStopListening<EventAlertRequestedEvent>();
        }
    }
}
