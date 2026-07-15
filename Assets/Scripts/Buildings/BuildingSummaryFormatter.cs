using System;
using System.Collections.Generic;
using System.Linq;

public readonly struct BuildingSummaryPresentation
{
    public BuildingSummaryPresentation(string objectName, IReadOnlyList<string> detailLines)
    {
        ObjectName = objectName ?? string.Empty;
        DetailLines = detailLines ?? Array.Empty<string>();
        StockText = string.Join("\n", DetailLines);
    }

    public string ObjectName { get; }
    public string StockText { get; }
    public IReadOnlyList<string> DetailLines { get; }
}

public interface IBuildingSummaryFormatter
{
    BuildingSummaryPresentation Format(BuildableObject building);
}

public sealed class BuildingSummaryFormatter : IBuildingSummaryFormatter
{
    private readonly IBuildingDefinitionLookup buildingDefinitionLookup;

    public BuildingSummaryFormatter(IBuildingDefinitionLookup buildingDefinitionLookup)
    {
        this.buildingDefinitionLookup = buildingDefinitionLookup
            ?? throw new ArgumentNullException(nameof(buildingDefinitionLookup));
    }

    public BuildingSummaryPresentation Format(BuildableObject building)
    {
        if (building == null)
        {
            throw new ArgumentNullException(nameof(building));
        }

        BuildingSO data = buildingDefinitionLookup.GetBuilding(building.id) ?? building.BuildingData;
        string objectName = !string.IsNullOrWhiteSpace(data != null ? data.objectName : null)
            ? data.objectName
            : building.name;
        return new BuildingSummaryPresentation(objectName, BuildDetailLines(building));
    }

    private static IReadOnlyList<string> BuildDetailLines(BuildableObject building)
    {
        List<string> lines = new List<string>
        {
            $"상태  {(building.IsDamaged ? "손상" : "정상")}  ·  시설 Lv.{building.FacilityLevel}",
            $"위치  ({building.centerPos.x}, {building.centerPos.y})  ·  {FormatCategory(building.category)}"
        };

        FacilityData facility = building.Facility;
        if (facility != null)
        {
            string capacity = facility.capacity > 0
                ? $"이용 {building.CurrentUserCount}/{facility.capacity}  ·  예약 {building.ActiveVisitReservationCount}"
                : "방문 이용 없음";
            lines.Add(capacity);
            lines.Add($"용도  {FormatRoles(facility.roles)}");
            lines.Add($"업무  {FormatWorkTypes(facility.supportedWorkTypes)}  ·  필요 직원 {facility.requiredWorkers}");
        }

        string stock = FormatStockText(building);
        if (!string.IsNullOrWhiteSpace(stock))
        {
            lines.Add(stock);
        }

        return lines;
    }

    private static string FormatStockText(BuildableObject building)
    {
        if (building is Shop shop)
        {
            return shop.NeedsRestock
                ? $"재고  {shop.GetStockCount()}/{shop.MaxInternalStock}  ·  보충 필요"
                : $"재고  {shop.GetStockCount()}/{shop.MaxInternalStock}";
        }

        if (building is IWarehouseFacility warehouse && warehouse.HasWarehouseInventory)
        {
            return warehouse.Inventory.HasCapacityLimit
                ? $"창고  {warehouse.Inventory.TotalStock}/{warehouse.Inventory.MaxCapacity}"
                : $"창고  {warehouse.Inventory.TotalStock}";
        }

        if (building is IStockedFacility stocked)
        {
            int maximum = building.Facility != null ? building.Facility.internalStockMax : 0;
            return maximum > 0 ? $"재고  {stocked.CurrentStock}/{maximum}" : $"재고  {stocked.CurrentStock}";
        }

        return string.Empty;
    }

    private static string FormatCategory(BuildingCategory category)
    {
        return category switch
        {
            BuildingCategory.Wall => "벽/문",
            BuildingCategory.Shop => "상점",
            BuildingCategory.Special => "특수",
            BuildingCategory.Movement => "이동",
            BuildingCategory.Production => "생산",
            BuildingCategory.Crafting => "제작",
            BuildingCategory.Resource => "자원",
            _ => "시설"
        };
    }

    private static string FormatRoles(FacilityRole roles)
    {
        if (roles == FacilityRole.None) return "없음";

        return string.Join(", ", EnumerateFlags(roles).Select(role => role switch
        {
            FacilityRole.Meal => "식사",
            FacilityRole.Purchase => "구매",
            FacilityRole.Rest => "휴식",
            FacilityRole.Training => "훈련",
            FacilityRole.Research => "연구",
            FacilityRole.Mana => "마력",
            FacilityRole.Logistics => "물류",
            FacilityRole.Toilet => "배변",
            FacilityRole.Hygiene => "위생",
            _ => role.ToString()
        }));
    }

    private static string FormatWorkTypes(FacilityWorkType workTypes)
    {
        if (workTypes == FacilityWorkType.None) return "없음";

        return string.Join(", ", EnumerateFlags(workTypes).Select(workType => workType switch
        {
            FacilityWorkType.Operate => "운영",
            FacilityWorkType.Restock => "보충",
            FacilityWorkType.Repair => "수리",
            FacilityWorkType.Clean => "청소",
            FacilityWorkType.Research => "연구",
            FacilityWorkType.Guard => "경비",
            FacilityWorkType.Rescue => "구조",
            FacilityWorkType.Rest => "휴식",
            _ => workType.ToString()
        }));
    }

    private static IEnumerable<FacilityRole> EnumerateFlags(FacilityRole flags)
    {
        foreach (FacilityRole value in Enum.GetValues(typeof(FacilityRole)))
        {
            if (value != FacilityRole.None && flags.HasFlag(value)) yield return value;
        }
    }

    private static IEnumerable<FacilityWorkType> EnumerateFlags(FacilityWorkType flags)
    {
        foreach (FacilityWorkType value in Enum.GetValues(typeof(FacilityWorkType)))
        {
            if (value != FacilityWorkType.None && flags.HasFlag(value)) yield return value;
        }
    }
}
