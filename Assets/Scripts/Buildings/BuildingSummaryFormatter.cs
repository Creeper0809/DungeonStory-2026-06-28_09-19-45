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

        string crafting = FormatEquipmentCraftingText(building);
        if (!string.IsNullOrWhiteSpace(crafting))
        {
            lines.Add(crafting);
        }

        return lines;
    }

    private static string FormatEquipmentCraftingText(BuildableObject building)
    {
        BuildingEquipmentCraftingAbility crafting = building?.BuildingData
            ?.GetAbility<BuildingEquipmentCraftingAbility>();
        if (crafting == null)
        {
            return string.Empty;
        }

        if (!building.TryGetExpeditionEquipmentRuntime(out IExpeditionEquipmentRuntime runtime))
        {
            return "제작  장비 런타임 없음";
        }

        HashSet<string> craftableIds = new HashSet<string>(
            crafting.CraftableEquipmentIds
                .Where(id => !string.IsNullOrWhiteSpace(id)),
            StringComparer.Ordinal);
        string queue = string.Join(", ", runtime.CraftQueue
            .Where(order => order != null
                && craftableIds.Contains(order.equipmentId))
            .Select(order =>
            {
                string name = runtime.TryGetDefinition(order.equipmentId, out ExpeditionEquipmentDefinition definition)
                    ? definition.displayName
                    : order.equipmentId;
                string materialState = order.materialsReady ? string.Empty : " / 재료 이동 중";
                return $"{name} {order.remainingSeconds:0.#}s{materialState}";
            }));
        string craftable = string.Join(", ", runtime.Definitions
            .Where(definition => definition != null
                && craftableIds.Contains(definition.id))
            .Select(definition => definition.displayName));
        return string.IsNullOrWhiteSpace(queue)
            ? $"제작 가능  {craftable}  ·  대기 없음"
            : $"제작 대기  {queue}";
    }

    private static string FormatStockText(BuildableObject building)
    {
        if (building is IRestockableFacility restockable)
        {
            int maximum = building.GetInternalStockCapacity();
            string amount = maximum > 0
                ? $"{restockable.CurrentStock}/{maximum}"
                : restockable.CurrentStock.ToString();
            return restockable.NeedsRestock
                ? $"재고  {amount}  ·  보충 필요"
                : $"재고  {amount}";
        }

        if (building is IWarehouseFacility warehouse && warehouse.HasWarehouseInventory)
        {
            return warehouse.Inventory.HasCapacityLimit
                ? $"창고  {warehouse.Inventory.TotalStock}/{warehouse.Inventory.MaxCapacity}"
                : $"창고  {warehouse.Inventory.TotalStock}";
        }

        if (building is IStockedFacility stocked)
        {
            int maximum = building.GetInternalStockCapacity();
            return maximum > 0 ? $"재고  {stocked.CurrentStock}/{maximum}" : $"재고  {stocked.CurrentStock}";
        }

        return string.Empty;
    }

    private static string FormatCategory(BuildingCategory category)
    {
        return BuildingCategoryCatalog.GetDisplayName(category);
    }

    private static string FormatRoles(FacilityRole roles)
    {
        if (roles == FacilityRole.None) return "없음";

        return string.Join(", ", FacilityRoleCatalog
            .Enumerate(roles)
            .Select((definition) => definition.RoomLabel));
    }

    private static string FormatWorkTypes(FacilityWorkType workTypes)
    {
        if (workTypes == FacilityWorkType.None) return "없음";

        return string.Join(", ", WorkTypeCatalog
            .Enumerate(workTypes)
            .Select((definition) => definition.DisplayName));
    }
}
