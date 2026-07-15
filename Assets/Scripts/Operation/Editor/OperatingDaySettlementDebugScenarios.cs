using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class OperatingDaySettlementDebugScenarios
{
    [MenuItem("DungeonStory/Debug/Operation/Run P1 Operating Day Scenarios")]
    public static void RunFromMenu()
    {
        bool success = RunAll(true);
        if (!success)
        {
            Debug.LogError("P1 operating day scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        List<string> errors = new List<string>();

        RunScenario("운영일 정산 수집", VerifySettlementCollectsRuntimeEvents, errors);
        RunScenario("정산 상세 텍스트", VerifyReportDetailText, errors);

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
            Debug.Log("P1 operating day scenarios passed.");
        }

        return true;
    }

    private static void RunScenario(string name, System.Func<bool> scenario, List<string> errors)
    {
        if (scenario()) return;

        errors.Add(name);
    }

    private static bool VerifySettlementCollectsRuntimeEvents()
    {
        GameObject runtimeObject = new GameObject("OperatingDaySettlementRuntime_Test");
        OperatingDaySettlementRuntime runtime = runtimeObject.AddComponent<OperatingDaySettlementRuntime>();

        CharacterActor customer = CreateCharacter("Customer_Test", CharacterType.Customer, "slime", 64f, 80f, false);
        CharacterActor staff = CreateCharacter("Staff_Test", CharacterType.NPC, "orc", 20f, 20f, true);
        BuildableObject shop = CreateBuilding("Food Shop", false, 10);
        Facility warehouse = CreateWarehouse("Warehouse", 24);

        runtime.OnTriggerEvent(new OperatingDayStartedEvent(1));
        runtime.OnTriggerEvent(new FacilityVisitEvent(CharacterActor.From(customer), shop));
        runtime.OnTriggerEvent(new FacilityRevenueEvent(CharacterActor.From(customer), shop, 120));
        runtime.OnTriggerEvent(new FacilityStockConsumedEvent(CharacterActor.From(customer), shop, StockCategory.Food, 2));
        runtime.OnTriggerEvent(new FacilityCrimeEvent(CharacterActor.From(customer), shop, FacilityCrimeKind.Shoplifting, "Shoplifting test", 30));
        runtime.OnTriggerEvent(new FacilityRestockEvent(shop, 5, 0, "창고 재고 부족"));
        runtime.OnTriggerEvent(new StockSupplyEvent(new StockSupplyResult(true, StockCategory.Food, 5, 5, 20, "테스트 납품", string.Empty)));
        runtime.OnTriggerEvent(new OperatingDayEndedEvent(1));

        OperatingDayReport report = runtime.LatestReport;
        bool valid = report != null
            && report.day == 1
            && report.totalVisits == 1
            && report.totalRevenue == 120
            && report.averageSatisfaction == 64f
            && report.restockFailureCount == 1
            && report.facilityRevenues.Count == 1
            && report.facilityRevenues[0].revenue == 120
            && report.speciesVisits.Count == 1
            && report.stockConsumed.Count == 1
            && report.stockConsumed[0].amount == 2
            && report.incidents.Count == 1
            && report.incidents[0] == "Shoplifting test"
            && report.stockSupplyResults.Count == 1
            && report.warehouseStocks.Count >= 1
            && report.staffSummary.staffCount >= 1
            && report.staffComplaintEvents.Count >= 1
            && report.refreshedDailyShopOffers.Count == 4;

        Object.DestroyImmediate(customer.data);
        Object.DestroyImmediate(staff.data);
        Object.DestroyImmediate(shop.BuildingData);
        Object.DestroyImmediate(warehouse.BuildingData);
        Object.DestroyImmediate(customer.gameObject);
        Object.DestroyImmediate(staff.gameObject);
        Object.DestroyImmediate(shop.gameObject);
        Object.DestroyImmediate(warehouse.gameObject);
        Object.DestroyImmediate(runtimeObject);
        return valid;
    }

    private static bool VerifyReportDetailText()
    {
        OperatingDayReport report = new OperatingDayReport
        {
            day = 2,
            totalRevenue = 50,
            totalVisits = 3,
            averageSatisfaction = 70f,
            eventLog = new List<string> { "설계도 획득" }
        };

        string detail = report.ToDetailText();
        return detail.Contains("Day 2")
            && detail.Contains("총 매출: 50")
            && detail.Contains("이벤트 로그")
            && detail.Contains("설계도 획득");
    }

    private static CharacterActor CreateCharacter(
        string name,
        CharacterType type,
        string speciesTag,
        float mood,
        float sleep,
        bool withWork)
    {
        GameObject obj = new GameObject(name);
        CharacterActor character = obj.AddComponent<CharacterActor>();
        if (withWork)
        {
            obj.AddComponent<AbilityWork>();
            character.RefreshAbilityCache();
        }

        CharacterSO data = ScriptableObject.CreateInstance<CharacterSO>();
        data.characterType = type;
        data.speciesTag = speciesTag;
        character.data = data;
        character.characterType = type;
        character.stats ??= new Dictionary<CharacterCondition, float>();
        EnsureStat(character, CharacterCondition.HUNGER, 100f);
        EnsureStat(character, CharacterCondition.FUN, 50f);
        character.stats[CharacterCondition.MOOD] = mood;
        character.stats[CharacterCondition.SLEEP] = sleep;
        return character;
    }

    private static void EnsureStat(CharacterActor character, CharacterCondition condition, float value)
    {
        if (!character.stats.ContainsKey(condition))
        {
            character.stats[condition] = value;
        }
    }

    private static BuildableObject CreateBuilding(string objectName, bool damaged, int maintenance)
    {
        GameObject obj = new GameObject(objectName);
        BuildableObject building = obj.AddComponent<BuildableObject>();
        BuildingSO data = ScriptableObject.CreateInstance<BuildingSO>();
        data.objectName = objectName;
        data.maintenance = maintenance;
        data.width = 1;
        data.height = 1;
        data.category = BuildingCategory.Shop;
        data.facility = new FacilityData
        {
            roles = FacilityRole.Meal,
            capacity = 1,
            supportedWorkTypes = FacilityWorkType.Operate
        };
        building.Initialization(data, Vector2Int.zero);
        building.SetDamaged(damaged);
        return building;
    }

    private static Facility CreateWarehouse(string objectName, int capacity)
    {
        GameObject obj = new GameObject(objectName);
        Facility warehouse = obj.AddComponent<Facility>();
        BuildingSO data = ScriptableObject.CreateInstance<BuildingSO>();
        data.objectName = objectName;
        data.width = 1;
        data.height = 1;
        data.category = BuildingCategory.Resource;
        data.facility = new FacilityData
        {
            roles = FacilityRole.Logistics,
            capacity = 1,
            internalStockMax = capacity,
            supportedWorkTypes = FacilityWorkType.Restock
        };
        warehouse.Initialization(data, Vector2Int.zero);
        return warehouse;
    }
}
