using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class RunVariableDebugScenarios
{
    [MenuItem("DungeonStory/Debug/Run/Run P2 Run Variable Scenarios")]
    public static void RunFromMenu()
    {
        bool success = RunAll(true);
        if (!success)
        {
            Debug.LogError("P2 run variable scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        List<string> errors = new List<string>();

        RunScenario("시작 변수 선택", VerifyStartVariables, errors);
        RunScenario("운영 이벤트 9종 배율", VerifyOperationVariables, errors);
        RunScenario("운영 이벤트 만료", VerifyOperationVariableExpiration, errors);
        RunScenario("상점/재고 비용 연결", VerifyCostIntegrations, errors);
        RunScenario("침입 변수 5종 설정 연결", VerifyInvasionVariables, errors);
        RunScenario("이벤트 알림 발행", VerifyEventAlerts, errors);

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
            Debug.Log("P2 run variable scenarios passed.");
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

    private static bool VerifyStartVariables()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        CharacterSO owner = CreateCharacter(8101, CharacterType.NPC, "Slime");

        scenario.Runtime.StartRun(12345, owner, InvasionThreatDifficulty.Hard);
        RunStartVariableSnapshot snapshot = scenario.Runtime.State.StartVariables;
        bool valid = snapshot != null
            && snapshot.seed == 12345
            && snapshot.ownerSpeciesTag == "Slime"
            && snapshot.difficulty == InvasionThreatDifficulty.Hard
            && snapshot.threatRiseMultiplier > 1f
            && snapshot.initialShopSeed != 0
            && snapshot.initialDungeonLayoutId == "wet-front";

        Object.DestroyImmediate(owner);
        return valid;
    }

    private static bool VerifyOperationVariables()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        foreach (RunVariableDefinition definition in RunVariableCatalog.GetByCategory(RunVariableCategory.Operation))
        {
            scenario.Runtime.ActivateOperationVariable(definition.id, 1, false);
        }

        BuildingSO generalBuilding = CreateBuilding(8201, "일반 시설", false);
        BuildingSO defenseBuilding = CreateBuilding(8202, "방어 시설", true);
        FacilityBlueprintSO blueprint = CreateBlueprint(8301, "테스트 설계도", 100);

        bool guestDemand = scenario.Runtime.GetGuestDemandMultiplier("Slime") > 1f
            && scenario.Runtime.GetGuestDemandMultiplier("Orc") > 1f
            && scenario.Runtime.GetGuestDemandMultiplier("Vampire") > 1f;
        bool stockCosts = scenario.Runtime.GetStockCostMultiplier(StockCategory.Food) > 1f
            && scenario.Runtime.GetStockCostMultiplier(StockCategory.General) < 1f
            && scenario.Runtime.GetStockCostMultiplier(StockCategory.Mana) < 1f;
        bool shopCosts = scenario.Runtime.GetFacilityShopCostMultiplier(generalBuilding) < 1f
            && scenario.Runtime.GetFacilityShopCostMultiplier(defenseBuilding) < 0.75f;
        bool blueprintCosts = scenario.Runtime.GetBlueprintCostMultiplier(blueprint) < 1f;

        Object.DestroyImmediate(generalBuilding);
        Object.DestroyImmediate(defenseBuilding);
        Object.DestroyImmediate(blueprint);
        return guestDemand && stockCosts && shopCosts && blueprintCosts;
    }

    private static bool VerifyOperationVariableExpiration()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        scenario.Runtime.ActivateOperationVariable(RunVariableId.SlimeCrowdVisit, 1, false);
        bool active = scenario.Runtime.GetGuestDemandMultiplier("Slime") > 1f;

        scenario.Runtime.OnTriggerEvent(new OperatingDayEndedEvent(1));
        bool expired = Math.Abs(scenario.Runtime.GetGuestDemandMultiplier("Slime") - 1f) < 0.001f
            && scenario.Runtime.State.ActiveOperationVariables.Count == 0;

        return active && expired;
    }

    private static bool VerifyCostIntegrations()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        BuildingSO generalBuilding = CreateBuilding(8401, "일반 시설", false);
        FacilityBlueprintSO blueprint = CreateBlueprint(8402, "가격 테스트 설계도", 200);

        int baseFoodCost = StockSupplyService.CreateDailyDeliveryOffers(1)
            .First((offer) => offer.category == StockCategory.Food)
            .cost;
        int baseBuildingCost = FacilityShopService.CreateDailyOffers(1, new[] { generalBuilding }, Array.Empty<FacilityBlueprintSO>())
            .First((offer) => offer.Type == FacilityShopOfferType.Building)
            .Cost;
        int baseBlueprintCost = FacilityShopService.CreateDailyOffers(1, Array.Empty<BuildingSO>(), new[] { blueprint })
            .First((offer) => offer.Type == FacilityShopOfferType.Blueprint)
            .Cost;

        scenario.Runtime.ActivateOperationVariable(RunVariableId.FoodDeliveryDelay, 1, false);
        scenario.Runtime.ActivateOperationVariable(RunVariableId.VisitingMerchant, 1, false);
        scenario.Runtime.ActivateOperationVariable(RunVariableId.BlueprintRumor, 1, false);

        int eventFoodCost = StockSupplyService.CreateDailyDeliveryOffers(1)
            .First((offer) => offer.category == StockCategory.Food)
            .cost;
        int eventBuildingCost = FacilityShopService.CreateDailyOffers(1, new[] { generalBuilding }, Array.Empty<FacilityBlueprintSO>())
            .First((offer) => offer.Type == FacilityShopOfferType.Building)
            .Cost;
        int eventBlueprintCost = FacilityShopService.CreateDailyOffers(1, Array.Empty<BuildingSO>(), new[] { blueprint })
            .First((offer) => offer.Type == FacilityShopOfferType.Blueprint)
            .Cost;

        Object.DestroyImmediate(generalBuilding);
        Object.DestroyImmediate(blueprint);
        return eventFoodCost > baseFoodCost
            && eventBuildingCost < baseBuildingCost
            && eventBlueprintCost < baseBlueprintCost;
    }

    private static bool VerifyInvasionVariables()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        InvasionIntruderSettings source = new InvasionIntruderSettings
        {
            secondsToFullFocus = 30f,
            repathIntervalSeconds = 2f,
            facilityDamageIntervalSeconds = 10f,
            finalCombatDamage = 100f,
            finalCombatWindupSeconds = 0.7f
        };

        bool scout = scenario.Runtime.SelectInvasionVariable(RunVariableId.ScoutTraces, false) != null
            && scenario.Runtime.ApplyInvasionSettings(source).secondsToFullFocus < source.secondsToFullFocus;
        bool ambush = scenario.Runtime.SelectInvasionVariable(RunVariableId.Ambush, false) != null
            && scenario.Runtime.ApplyInvasionSettings(source).repathIntervalSeconds < source.repathIntervalSeconds;
        bool armed = scenario.Runtime.SelectInvasionVariable(RunVariableId.ArmedIntruder, false) != null
            && scenario.Runtime.ApplyInvasionSettings(source).finalCombatDamage > source.finalCombatDamage;
        bool loot = scenario.Runtime.SelectInvasionVariable(RunVariableId.LootPriority, false) != null
            && scenario.Runtime.ApplyInvasionSettings(source).facilityDamageIntervalSeconds < source.facilityDamageIntervalSeconds;
        bool tired = scenario.Runtime.SelectInvasionVariable(RunVariableId.TiredIntruder, false) != null
            && scenario.Runtime.ApplyInvasionSettings(source).finalCombatDamage < source.finalCombatDamage;

        scenario.Runtime.OnTriggerEvent(new InvasionResolvedEvent(true, 0f));
        bool cleared = scenario.Runtime.State.CurrentInvasionVariable == null;

        return scout && ambush && armed && loot && tired && cleared;
    }

    private static bool VerifyEventAlerts()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        using CountingEventAlertRequestListener alerts = new CountingEventAlertRequestListener();

        scenario.Runtime.ActivateOperationVariable(RunVariableId.OrcFeast, 1);
        scenario.Runtime.SelectInvasionVariable(RunVariableId.ArmedIntruder);

        return alerts.Requests.Any((request) => request.Title == "오크 회식" && request.Category == "운영 변수")
            && alerts.Requests.Any((request) => request.Title == "무장한 침입자" && request.Category == "침입 변수");
    }

    private static BuildingSO CreateBuilding(int id, string name, bool defense)
    {
        BuildingSO building = ScriptableObject.CreateInstance<BuildingSO>();
        building.id = id;
        building.objectName = name;
        building.category = defense ? BuildingCategory.Special : BuildingCategory.Shop;
        building.width = 1;
        building.height = 1;
        building.layer = GridLayer.Building;
        if (defense)
        {
            building.Defense.enabled = true;
            building.Defense.star = 1;
            building.Defense.concept = DefenseAttackConcept.Physical;
        }

        return building;
    }

    private static FacilityBlueprintSO CreateBlueprint(int id, string displayName, int defaultCost)
    {
        FacilityBlueprintSO blueprint = ScriptableObject.CreateInstance<FacilityBlueprintSO>();
        blueprint.id = id;
        blueprint.blueprintName = displayName;
        blueprint.defaultCost = defaultCost;
        blueprint.rarity = FacilityShopRarity.Common;
        return blueprint;
    }

    private static CharacterSO CreateCharacter(int id, CharacterType type, string speciesTag)
    {
        CharacterSO character = ScriptableObject.CreateInstance<CharacterSO>();
        character.id = id;
        character.characterType = type;
        character.speciesTag = speciesTag;
        return character;
    }

    private sealed class ScenarioRuntime : IDisposable
    {
        private readonly GameObject runtimeObject;

        public RunVariableRuntime Runtime { get; }

        public ScenarioRuntime()
        {
            runtimeObject = new GameObject("Run Variable Scenario Runtime");
            Runtime = runtimeObject.AddComponent<RunVariableRuntime>();
            Runtime.StartRun(999, null, InvasionThreatDifficulty.Normal);
        }

        public void Dispose()
        {
            Object.DestroyImmediate(runtimeObject);
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
