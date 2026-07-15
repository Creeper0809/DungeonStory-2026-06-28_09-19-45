using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

[Serializable]
public class FacilityRevenueSummary
{
    public string facilityName;
    public int revenue;
}

[Serializable]
public class SpeciesVisitSummary
{
    public string speciesTag;
    public int visitCount;
}

[Serializable]
public class StockConsumptionSummary
{
    public StockCategory category;
    public int amount;
}

[Serializable]
public class WarehouseStockSummary
{
    public string warehouseName;
    public int totalStock;
    public int maxCapacity;
    public int food;
    public int general;
    public int weapon;
    public int mana;
}

[Serializable]
public class StaffWorkSummary
{
    public int staffCount;
    public int workingCount;
    public int offDutyCount;
    public float averageSleep;
    public float averageMood;
}

[Serializable]
public class OperatingDayReport
{
    public int day;
    public int totalRevenue;
    public int totalVisits;
    public float averageSatisfaction;
    public int repairCost;
    public int restockFailureCount;
    public List<FacilityRevenueSummary> facilityRevenues = new List<FacilityRevenueSummary>();
    public List<SpeciesVisitSummary> speciesVisits = new List<SpeciesVisitSummary>();
    public List<string> incidents = new List<string>();
    public List<string> damagedFacilities = new List<string>();
    public List<string> stockShortageFacilities = new List<string>();
    public List<string> staffComplaintEvents = new List<string>();
    public List<string> eventLog = new List<string>();
    public List<string> unlockedCodexInfo = new List<string>();
    public StaffWorkSummary staffSummary = new StaffWorkSummary();
    public List<WarehouseStockSummary> warehouseStocks = new List<WarehouseStockSummary>();
    public List<StockConsumptionSummary> stockConsumed = new List<StockConsumptionSummary>();
    public List<StockSupplyResult> stockSupplyResults = new List<StockSupplyResult>();
    public List<StockDeliveryOffer> refreshedDailyShopOffers = new List<StockDeliveryOffer>();
    public List<FacilityShopOfferSnapshot> refreshedFacilityShopOffers = new List<FacilityShopOfferSnapshot>();

    public string ToDetailText()
    {
        List<string> lines = new List<string>
        {
            $"Day {day} 운영 정산",
            string.Empty,
            $"총 매출: {totalRevenue}",
            $"방문 손님 수: {totalVisits}",
            $"평균 만족도: {averageSatisfaction:0.#}",
            $"수리 비용 예상: {repairCost}",
            $"보충 실패 횟수: {restockFailureCount}",
            string.Empty,
            FormatList("시설별 매출", facilityRevenues.Select((item) => $"{item.facilityName}: {item.revenue}")),
            FormatList("종족별 방문", speciesVisits.Select((item) => $"{TextOrDefault(item.speciesTag, "Unknown")}: {item.visitCount}")),
            FormatList("소비된 재고", stockConsumed.Select((item) => $"{GetStockCategoryName(item.category)}: {item.amount}")),
            FormatList("창고 재고", warehouseStocks.Select((item) => $"{item.warehouseName}: {item.totalStock}/{item.maxCapacity} (식자재 {item.food}, 잡화 {item.general}, 무기 {item.weapon}, 마력 {item.mana})")),
            FormatList("재고 부족 시설", stockShortageFacilities),
            FormatList("파손 시설", damagedFacilities),
            FormatList("발생한 사고", incidents),
            FormatList("이벤트 로그", eventLog),
            FormatList("직원 불만 사건", staffComplaintEvents),
            $"직원 요약: 총 {staffSummary.staffCount}, 근무 {staffSummary.workingCount}, 비번 {staffSummary.offDutyCount}, 평균 피로 {staffSummary.averageSleep:0.#}, 평균 기분 {staffSummary.averageMood:0.#}",
            FormatList("재고 수급 결과", stockSupplyResults.Select((result) => result.ToSummaryText())),
            FormatList("상점 판매 목록 갱신", refreshedDailyShopOffers.Select((offer) => $"{GetStockCategoryName(offer.category)} {offer.amount}개 / 비용 {offer.cost}")),
            FormatList("시설 상점 갱신", refreshedFacilityShopOffers.Select((offer) => offer.ToSummaryText())),
            FormatList("신규 도감 정보", unlockedCodexInfo)
        };

        return string.Join("\n", lines.Where((line) => line != null));
    }

    private static string FormatList(string title, IEnumerable<string> rows)
    {
        List<string> validRows = rows?
            .Where((row) => !string.IsNullOrWhiteSpace(row))
            .ToList()
            ?? new List<string>();

        if (validRows.Count == 0)
        {
            return $"{title}: 없음";
        }

        return $"{title}:\n- {string.Join("\n- ", validRows)}";
    }

    private static string TextOrDefault(string value, string defaultValue)
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    private static string GetStockCategoryName(StockCategory category)
    {
        return category switch
        {
            StockCategory.Food => "식자재",
            StockCategory.General => "잡화",
            StockCategory.Weapon => "무기",
            StockCategory.Mana => "마력",
            _ => category.ToString()
        };
    }
}

public struct OperatingDayStartedEvent
{
    public int day;

    public OperatingDayStartedEvent(int day)
    {
        this.day = day;
    }

    private static OperatingDayStartedEvent e;

    public static void Trigger(int day)
    {
        e.day = day;
        EventObserver.TriggerEvent(e);
    }
}

public struct OperatingDayEndedEvent
{
    public int day;

    public OperatingDayEndedEvent(int day)
    {
        this.day = day;
    }

    private static OperatingDayEndedEvent e;

    public static void Trigger(int day)
    {
        e.day = day;
        EventObserver.TriggerEvent(e);
    }
}

public struct OperatingDayReportEvent
{
    public OperatingDayReport report;

    public OperatingDayReportEvent(OperatingDayReport report)
    {
        this.report = report;
    }

    private static OperatingDayReportEvent e;

    public static void Trigger(OperatingDayReport report)
    {
        e.report = report;
        EventObserver.TriggerEvent(e);
    }
}

public struct FacilityVisitEvent
{
    public CharacterActor visitorActor;
    public BuildableObject facility;

    public FacilityVisitEvent(CharacterActor visitor, BuildableObject facility)
    {
        visitorActor = visitor;
        this.facility = facility;
    }

    private static FacilityVisitEvent e;

    public static void Trigger(CharacterActor visitor, BuildableObject facility)
    {
        e.visitorActor = visitor;
        e.facility = facility;
        EventObserver.TriggerEvent(e);
    }
}

public struct FacilityRevenueEvent
{
    public CharacterActor customerActor;
    public BuildableObject facility;
    public int revenue;

    public FacilityRevenueEvent(CharacterActor customer, BuildableObject facility, int revenue)
    {
        customerActor = customer;
        this.facility = facility;
        this.revenue = revenue;
    }

    private static FacilityRevenueEvent e;

    public static void Trigger(CharacterActor customer, BuildableObject facility, int revenue)
    {
        e.customerActor = customer;
        e.facility = facility;
        e.revenue = revenue;
        EventObserver.TriggerEvent(e);
    }
}

public struct FacilityStockConsumedEvent
{
    public CharacterActor consumerActor;
    public BuildableObject facility;
    public StockCategory category;
    public int amount;

    public FacilityStockConsumedEvent(CharacterActor consumer, BuildableObject facility, StockCategory category, int amount)
    {
        consumerActor = consumer;
        this.facility = facility;
        this.category = category;
        this.amount = amount;
    }

    private static FacilityStockConsumedEvent e;

    public static void Trigger(CharacterActor consumer, BuildableObject facility, StockCategory category, int amount)
    {
        e.consumerActor = consumer;
        e.facility = facility;
        e.category = category;
        e.amount = amount;
        EventObserver.TriggerEvent(e);
    }
}

public enum FacilityCrimeKind
{
    Shoplifting
}

public struct FacilityCrimeEvent
{
    public CharacterActor actor;
    public BuildableObject facility;
    public FacilityCrimeKind kind;
    public string detail;
    public int lossValue;

    public FacilityCrimeEvent(
        CharacterActor actor,
        BuildableObject facility,
        FacilityCrimeKind kind,
        string detail,
        int lossValue)
    {
        this.actor = actor;
        this.facility = facility;
        this.kind = kind;
        this.detail = detail ?? string.Empty;
        this.lossValue = Mathf.Max(0, lossValue);
    }

    private static FacilityCrimeEvent e;

    public static void Trigger(
        CharacterActor actor,
        BuildableObject facility,
        FacilityCrimeKind kind,
        string detail,
        int lossValue)
    {
        e.actor = actor;
        e.facility = facility;
        e.kind = kind;
        e.detail = detail ?? string.Empty;
        e.lossValue = Mathf.Max(0, lossValue);
        EventObserver.TriggerEvent(e);
    }
}

public struct FacilityRestockEvent
{
    public BuildableObject facility;
    public int requestedAmount;
    public int restockedAmount;
    public string message;

    public FacilityRestockEvent(BuildableObject facility, int requestedAmount, int restockedAmount, string message)
    {
        this.facility = facility;
        this.requestedAmount = requestedAmount;
        this.restockedAmount = restockedAmount;
        this.message = message;
    }

    private static FacilityRestockEvent e;

    public static void Trigger(BuildableObject facility, int requestedAmount, int restockedAmount, string message)
    {
        e.facility = facility;
        e.requestedAmount = requestedAmount;
        e.restockedAmount = restockedAmount;
        e.message = message;
        EventObserver.TriggerEvent(e);
    }
}

public class OperatingDaySettlementRuntime : MonoBehaviour,
    UtilEventListener<OperatingDayStartedEvent>,
    UtilEventListener<OperatingDayEndedEvent>,
    UtilEventListener<FacilityVisitEvent>,
    UtilEventListener<FacilityRevenueEvent>,
    UtilEventListener<FacilityStockConsumedEvent>,
    UtilEventListener<FacilityCrimeEvent>,
    UtilEventListener<FacilityRestockEvent>,
    UtilEventListener<StockSupplyEvent>,
    UtilEventListener<EventAlertLoggedEvent>
{
    private const int MaxReportHistory = 20;

    private readonly Dictionary<string, int> facilityRevenue = new Dictionary<string, int>();
    private readonly Dictionary<string, int> speciesVisits = new Dictionary<string, int>();
    private readonly Dictionary<StockCategory, int> consumedStock = new Dictionary<StockCategory, int>();
    private readonly List<float> visitorMoodSamples = new List<float>();
    private readonly List<StockSupplyResult> stockSupplyResults = new List<StockSupplyResult>();
    private readonly List<string> incidents = new List<string>();
    private readonly List<string> eventLog = new List<string>();
    private int totalRevenue;
    private int totalVisits;
    private int restockFailureCount;
    private int currentDay = 1;
    private OperatingDayReport latestReport;
    private readonly List<OperatingDayReport> reportHistory = new List<OperatingDayReport>();
    private IDungeonSceneComponentQuery sceneQuery;
    private IFacilityShopCatalog facilityShopCatalog;
    private IRunVariableRuntimeReader runVariableReader;

    public OperatingDayReport LatestReport => latestReport;
    public IReadOnlyList<OperatingDayReport> ReportHistory => reportHistory;
    public int CurrentDay => currentDay;
    public int CurrentRevenue => totalRevenue;
    public int CurrentVisits => totalVisits;
    public int CurrentRestockFailureCount => restockFailureCount;
    public int CurrentConsumedStock => consumedStock.Values.Sum();
    public int CurrentIncidentCount => incidents.Count;
    public int CurrentEventCount => eventLog.Count;
    public float CurrentAverageSatisfaction => visitorMoodSamples.Count > 0
        ? visitorMoodSamples.Average()
        : 0f;

    [Inject]
    public void Construct(
        IDungeonSceneComponentQuery sceneQuery,
        IFacilityShopCatalog facilityShopCatalog,
        IRunVariableRuntimeReader runVariableReader)
    {
        this.sceneQuery = sceneQuery
            ?? throw new ArgumentNullException(nameof(sceneQuery));
        this.facilityShopCatalog = facilityShopCatalog
            ?? throw new ArgumentNullException(nameof(facilityShopCatalog));
        this.runVariableReader = runVariableReader
            ?? throw new ArgumentNullException(nameof(runVariableReader));
    }

    public void OnTriggerEvent(OperatingDayStartedEvent eventType)
    {
        currentDay = Mathf.Max(1, eventType.day);
        ResetLedger();
    }

    public void OnTriggerEvent(OperatingDayEndedEvent eventType)
    {
        latestReport = BuildReport(Mathf.Max(1, eventType.day));
        reportHistory.Insert(0, latestReport);
        if (reportHistory.Count > MaxReportHistory)
        {
            reportHistory.RemoveRange(MaxReportHistory, reportHistory.Count - MaxReportHistory);
        }

        OperatingDayReportEvent.Trigger(latestReport);
        ResetLedger();
    }

    public void OnTriggerEvent(FacilityVisitEvent eventType)
    {
        CharacterActor visitor = eventType.visitorActor;
        CharacterIdentity identity = visitor != null ? visitor.Identity : null;
        if (visitor == null || identity == null || identity.CharacterType != CharacterType.Customer)
        {
            return;
        }

        totalVisits++;
        string species = string.IsNullOrWhiteSpace(identity.SpeciesTag) ? "Unknown" : identity.SpeciesTag;
        speciesVisits[species] = speciesVisits.TryGetValue(species, out int count) ? count + 1 : 1;

        CharacterStats stats = visitor.Stats;
        if (stats != null && stats.Stats.TryGetValue(CharacterCondition.MOOD, out float mood))
        {
            visitorMoodSamples.Add(mood);
        }
    }

    public void OnTriggerEvent(FacilityRevenueEvent eventType)
    {
        int revenue = Mathf.Max(0, eventType.revenue);
        if (revenue <= 0) return;

        totalRevenue += revenue;
        string facilityName = GetFacilityName(eventType.facility);
        facilityRevenue[facilityName] = facilityRevenue.TryGetValue(facilityName, out int current)
            ? current + revenue
            : revenue;
    }

    public void OnTriggerEvent(FacilityStockConsumedEvent eventType)
    {
        int amount = Mathf.Max(0, eventType.amount);
        if (amount <= 0) return;

        consumedStock[eventType.category] = consumedStock.TryGetValue(eventType.category, out int current)
            ? current + amount
            : amount;
    }

    public void OnTriggerEvent(FacilityCrimeEvent eventType)
    {
        string detail = string.IsNullOrWhiteSpace(eventType.detail)
            ? $"{eventType.kind}: loss {Mathf.Max(0, eventType.lossValue)}"
            : eventType.detail;
        incidents.Add(detail);
    }

    public void OnTriggerEvent(FacilityRestockEvent eventType)
    {
        if (eventType.requestedAmount > 0 && eventType.restockedAmount <= 0)
        {
            restockFailureCount++;
        }
    }

    public void OnTriggerEvent(StockSupplyEvent eventType)
    {
        stockSupplyResults.Add(eventType.result);
    }

    public void OnTriggerEvent(EventAlertLoggedEvent eventType)
    {
        if (eventType.record == null)
        {
            return;
        }

        eventLog.Add(eventType.record.ButtonText);
    }

    private void OnEnable()
    {
        this.EventStartListening<OperatingDayStartedEvent>();
        this.EventStartListening<OperatingDayEndedEvent>();
        this.EventStartListening<FacilityVisitEvent>();
        this.EventStartListening<FacilityRevenueEvent>();
        this.EventStartListening<FacilityStockConsumedEvent>();
        this.EventStartListening<FacilityCrimeEvent>();
        this.EventStartListening<FacilityRestockEvent>();
        this.EventStartListening<StockSupplyEvent>();
        this.EventStartListening<EventAlertLoggedEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<OperatingDayStartedEvent>();
        this.EventStopListening<OperatingDayEndedEvent>();
        this.EventStopListening<FacilityVisitEvent>();
        this.EventStopListening<FacilityRevenueEvent>();
        this.EventStopListening<FacilityStockConsumedEvent>();
        this.EventStopListening<FacilityCrimeEvent>();
        this.EventStopListening<FacilityRestockEvent>();
        this.EventStopListening<StockSupplyEvent>();
        this.EventStopListening<EventAlertLoggedEvent>();
    }

    private OperatingDayReport BuildReport(int day)
    {
        IReadOnlyList<BuildableObject> buildings = RequireSceneQuery().All<BuildableObject>();
        IReadOnlyList<CharacterActor> characters = RequireSceneQuery().All<CharacterActor>();

        OperatingDayReport report = new OperatingDayReport
        {
            day = day,
            totalRevenue = totalRevenue,
            totalVisits = totalVisits,
            averageSatisfaction = visitorMoodSamples.Count > 0 ? visitorMoodSamples.Average() : 0f,
            restockFailureCount = restockFailureCount,
            facilityRevenues = facilityRevenue
                .Select((pair) => new FacilityRevenueSummary { facilityName = pair.Key, revenue = pair.Value })
                .OrderByDescending((item) => item.revenue)
                .ToList(),
            speciesVisits = speciesVisits
                .Select((pair) => new SpeciesVisitSummary { speciesTag = pair.Key, visitCount = pair.Value })
                .OrderByDescending((item) => item.visitCount)
                .ToList(),
            stockConsumed = consumedStock
                .Select((pair) => new StockConsumptionSummary { category = pair.Key, amount = pair.Value })
                .OrderByDescending((item) => item.amount)
                .ToList(),
            incidents = new List<string>(incidents),
            eventLog = new List<string>(eventLog),
            stockSupplyResults = new List<StockSupplyResult>(stockSupplyResults),
            refreshedDailyShopOffers = StockSupplyService.CreateDailyDeliveryOffers(day + 1, RequireRunVariableReader()).ToList(),
            refreshedFacilityShopOffers = FacilityShopService.CreateDailyOffers(
                    day + 1,
                    RequireFacilityShopCatalog(),
                    RequireRunVariableReader())
                .Select((offer) => offer.ToSnapshot())
                .ToList()
        };

        FillBuildingSnapshot(report, buildings);
        FillStaffSnapshot(report, characters);
        return report;
    }

    private void FillBuildingSnapshot(OperatingDayReport report, IEnumerable<BuildableObject> buildings)
    {
        foreach (BuildableObject building in buildings.Where((building) => building != null && !building.isDestroy))
        {
            if (building.IsDamaged)
            {
                report.damagedFacilities.Add(GetFacilityName(building));
                report.repairCost += Mathf.Max(0, building.BuildingData != null ? building.BuildingData.maintenance : 0);
            }

            if (building is Shop shop
                && shop.Facility != null
                && shop.CurrentStock <= shop.Facility.restockRequestThreshold)
            {
                report.stockShortageFacilities.Add(GetFacilityName(building));
            }

            if (building is IWarehouseFacility warehouse && warehouse.HasWarehouseInventory)
            {
                report.warehouseStocks.Add(new WarehouseStockSummary
                {
                    warehouseName = GetFacilityName(building),
                    totalStock = warehouse.Inventory.TotalStock,
                    maxCapacity = warehouse.Inventory.HasCapacityLimit ? warehouse.Inventory.MaxCapacity : 0,
                    food = warehouse.Inventory.GetStock(StockCategory.Food),
                    general = warehouse.Inventory.GetStock(StockCategory.General),
                    weapon = warehouse.Inventory.GetStock(StockCategory.Weapon),
                    mana = warehouse.Inventory.GetStock(StockCategory.Mana)
                });
            }
        }
    }

    private static void FillStaffSnapshot(OperatingDayReport report, IEnumerable<CharacterActor> characters)
    {
        List<CharacterActor> staff = characters
            .Where(IsStaffCharacter)
            .ToList();

        report.staffSummary.staffCount = staff.Count;
        if (staff.Count == 0)
        {
            return;
        }

        report.staffSummary.workingCount = staff.Count((actor) =>
            CharacterWorkRoleUtility.TryGetWork(actor, out AbilityWork work) && work.isWorking);
        report.staffSummary.offDutyCount = staff.Count((actor) =>
            CharacterWorkRoleUtility.TryGetWork(actor, out AbilityWork work) && work.IsOffDuty);
        report.staffSummary.averageSleep = staff.Average((actor) => GetStat(actor, CharacterCondition.SLEEP));
        report.staffSummary.averageMood = staff.Average((actor) => GetStat(actor, CharacterCondition.MOOD));
        report.staffComplaintEvents = staff
            .Where((actor) => GetStat(actor, CharacterCondition.MOOD) <= 25f)
            .Select((actor) => $"{actor.name}: 기분 낮음")
            .ToList();
    }

    private void ResetLedger()
    {
        facilityRevenue.Clear();
        speciesVisits.Clear();
        consumedStock.Clear();
        visitorMoodSamples.Clear();
        stockSupplyResults.Clear();
        incidents.Clear();
        eventLog.Clear();
        totalRevenue = 0;
        totalVisits = 0;
        restockFailureCount = 0;
    }

    private static string GetFacilityName(BuildableObject facility)
    {
        if (facility == null) return "Unknown";
        if (facility.BuildingData != null && !string.IsNullOrWhiteSpace(facility.BuildingData.objectName))
        {
            return facility.BuildingData.objectName;
        }

        return facility.name;
    }

    private static float GetStat(CharacterActor actor, CharacterCondition condition)
    {
        CharacterStats stats = actor != null ? actor.Stats : null;
        if (stats == null)
        {
            return 0f;
        }

        return stats.Stats.TryGetValue(condition, out float value) ? value : 0f;
    }

    private static bool IsStaffCharacter(CharacterActor actor)
    {
        CharacterIdentity identity = actor != null ? actor.Identity : null;
        return identity != null
            && identity.CharacterType == CharacterType.NPC
            && CharacterWorkRoleUtility.TryGetWork(actor, out _);
    }

    private IDungeonSceneComponentQuery RequireSceneQuery()
    {
        if (sceneQuery == null)
        {
            throw new InvalidOperationException($"{nameof(OperatingDaySettlementRuntime)} requires {nameof(IDungeonSceneComponentQuery)} injection.");
        }

        return sceneQuery;
    }

    private IFacilityShopCatalog RequireFacilityShopCatalog()
    {
        if (facilityShopCatalog == null)
        {
            throw new InvalidOperationException($"{nameof(OperatingDaySettlementRuntime)} requires {nameof(IFacilityShopCatalog)} injection.");
        }

        return facilityShopCatalog;
    }

    private IRunVariableRuntimeReader RequireRunVariableReader()
    {
        if (runVariableReader == null)
        {
            throw new InvalidOperationException($"{nameof(OperatingDaySettlementRuntime)} requires {nameof(IRunVariableRuntimeReader)} injection.");
        }

        return runVariableReader;
    }
}
