using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/Building/StockInfo", order = 0)]
public class StockInfo : DataScriptableObject
{
    public Shop.Type type;
    public int shopId;
    public List<Tuple<SaleItem,int>> stocks;
    public float multifly;
}

[Serializable]
public class WarehouseInventory
{
    private readonly Dictionary<StockCategory, int> stockByCategory = new Dictionary<StockCategory, int>();
    [SerializeField] private int maxCapacity;

    public int TotalStock => stockByCategory.Values.Sum();
    public int MaxCapacity => maxCapacity > 0 ? maxCapacity : int.MaxValue;
    public int RemainingCapacity => Mathf.Max(0, MaxCapacity - TotalStock);
    public bool HasCapacityLimit => maxCapacity > 0;

    public WarehouseInventory()
    {
    }

    public WarehouseInventory(int maxCapacity)
    {
        this.maxCapacity = Mathf.Max(0, maxCapacity);
    }

    public static WarehouseInventory CreateSeeded(int totalStock)
    {
        int capacity = Mathf.Max(0, totalStock);
        WarehouseInventory inventory = new WarehouseInventory(capacity);
        int food = Mathf.RoundToInt(capacity * 0.4f);
        int general = Mathf.RoundToInt(capacity * 0.25f);
        int weapon = Mathf.RoundToInt(capacity * 0.25f);
        int mana = Mathf.Max(0, capacity - food - general - weapon);

        inventory.AddStock(StockCategory.Food, food);
        inventory.AddStock(StockCategory.General, general);
        inventory.AddStock(StockCategory.Weapon, weapon);
        inventory.AddStock(StockCategory.Mana, mana);
        return inventory;
    }

    public int GetStock(StockCategory category)
    {
        return stockByCategory.TryGetValue(category, out int amount)
            ? amount
            : 0;
    }

    public bool HasStock(StockCategory category)
    {
        return GetStock(category) > 0;
    }

    public bool CanStore(int amount)
    {
        return RemainingCapacity >= Mathf.Max(0, amount);
    }

    public int AddStock(StockCategory category, int amount)
    {
        int safeAmount = Mathf.Max(0, amount);
        if (safeAmount == 0) return 0;

        stockByCategory[category] = GetStock(category) + safeAmount;
        return safeAmount;
    }

    public int Deposit(StockCategory category, int amount)
    {
        int safeAmount = Mathf.Max(0, amount);
        int deposited = Mathf.Min(safeAmount, RemainingCapacity);
        if (deposited <= 0) return 0;

        AddStock(category, deposited);
        return deposited;
    }

    public int Withdraw(StockCategory category, int amount)
    {
        int safeAmount = Mathf.Max(0, amount);
        if (safeAmount == 0) return 0;

        int current = GetStock(category);
        int withdrawn = Mathf.Min(current, safeAmount);
        stockByCategory[category] = current - withdrawn;
        return withdrawn;
    }
}

[Serializable]
public struct StockDeliveryOffer
{
    public StockCategory category;
    public int amount;
    public int cost;
    public string sourceLabel;

    public StockDeliveryOffer(StockCategory category, int amount, int cost, string sourceLabel)
    {
        this.category = category;
        this.amount = Mathf.Max(0, amount);
        this.cost = Mathf.Max(0, cost);
        this.sourceLabel = sourceLabel;
    }

    public bool IsValid => amount > 0 && cost >= 0;
}

[Serializable]
public struct StockProductionRule
{
    public StockCategory category;
    public int amount;
    public string sourceLabel;

    public StockProductionRule(StockCategory category, int amount, string sourceLabel)
    {
        this.category = category;
        this.amount = Mathf.Max(0, amount);
        this.sourceLabel = sourceLabel;
    }
}

[Serializable]
public struct StockSupplyResult
{
    public bool success;
    public StockCategory category;
    public int requestedAmount;
    public int deliveredAmount;
    public int cost;
    public string sourceLabel;
    public string reason;

    public StockSupplyResult(
        bool success,
        StockCategory category,
        int requestedAmount,
        int deliveredAmount,
        int cost,
        string sourceLabel,
        string reason)
    {
        this.success = success;
        this.category = category;
        this.requestedAmount = Mathf.Max(0, requestedAmount);
        this.deliveredAmount = Mathf.Max(0, deliveredAmount);
        this.cost = Mathf.Max(0, cost);
        this.sourceLabel = sourceLabel;
        this.reason = reason;
    }

    public string ToSummaryText()
    {
        string label = string.IsNullOrWhiteSpace(sourceLabel) ? "재고 수급" : sourceLabel;
        if (success)
        {
            string costText = cost > 0 ? $" / 비용 {cost}" : string.Empty;
            return $"{label}: {category} {deliveredAmount}개 입고{costText}";
        }

        return $"{label}: {category} {requestedAmount}개 입고 실패 - {reason}";
    }
}

public struct StockSupplyEvent
{
    public StockSupplyResult result;

    public StockSupplyEvent(StockSupplyResult result)
    {
        this.result = result;
    }

    private static StockSupplyEvent e;

    public static void Trigger(StockSupplyResult result)
    {
        e.result = result;
        EventObserver.TriggerEvent(e);
    }
}

public static class StockSupplyService
{
    public static IReadOnlyList<StockDeliveryOffer> CreateDailyDeliveryOffers(int day)
    {
        int safeDay = Mathf.Max(1, day);
        int smallGrowth = Mathf.Min(12, safeDay / 3);

        return new List<StockDeliveryOffer>
        {
            CreateOffer(StockCategory.Food, 20 + smallGrowth, 4, "운영일 납품"),
            CreateOffer(StockCategory.General, 14 + smallGrowth, 6, "운영일 납품"),
            CreateOffer(StockCategory.Weapon, 8 + Mathf.Max(0, smallGrowth / 2), 10, "운영일 납품"),
            CreateOffer(StockCategory.Mana, 10 + Mathf.Max(0, smallGrowth / 2), 9, "운영일 납품")
        };
    }

    public static bool TryPurchaseDelivery(
        GameData gameData,
        IEnumerable<IWarehouseFacility> warehouses,
        StockDeliveryOffer offer,
        out StockSupplyResult result)
    {
        if (!offer.IsValid)
        {
            result = Fail(offer.category, offer.amount, offer.cost, offer.sourceLabel, "납품 정보가 올바르지 않습니다");
            StockSupplyEvent.Trigger(result);
            return false;
        }

        if (gameData == null || gameData.holdingMoney == null)
        {
            result = Fail(offer.category, offer.amount, offer.cost, offer.sourceLabel, "자금 데이터가 없습니다");
            StockSupplyEvent.Trigger(result);
            return false;
        }

        if (gameData.holdingMoney.Value < offer.cost)
        {
            result = Fail(offer.category, offer.amount, offer.cost, offer.sourceLabel, "자금 부족");
            StockSupplyEvent.Trigger(result);
            return false;
        }

        if (!CanDepositAll(warehouses, offer.amount))
        {
            result = Fail(offer.category, offer.amount, offer.cost, offer.sourceLabel, "창고 공간 부족");
            StockSupplyEvent.Trigger(result);
            return false;
        }

        gameData.holdingMoney.Value -= offer.cost;
        int delivered = DepositToWarehouses(warehouses, offer.category, offer.amount);
        bool success = delivered == offer.amount;
        result = new StockSupplyResult(
            success,
            offer.category,
            offer.amount,
            delivered,
            offer.cost,
            offer.sourceLabel,
            success ? string.Empty : "입고 중단");
        StockSupplyEvent.Trigger(result);
        return success;
    }

    public static bool GrantReward(
        IEnumerable<IWarehouseFacility> warehouses,
        StockCategory category,
        int amount,
        string sourceLabel,
        out StockSupplyResult result)
    {
        int safeAmount = Mathf.Max(0, amount);
        if (safeAmount <= 0)
        {
            result = Fail(category, safeAmount, 0, sourceLabel, "보상 수량이 없습니다");
            StockSupplyEvent.Trigger(result);
            return false;
        }

        if (!CanDepositAll(warehouses, safeAmount))
        {
            result = Fail(category, safeAmount, 0, sourceLabel, "창고 공간 부족");
            StockSupplyEvent.Trigger(result);
            return false;
        }

        int delivered = DepositToWarehouses(warehouses, category, safeAmount);
        bool success = delivered == safeAmount;
        result = new StockSupplyResult(
            success,
            category,
            safeAmount,
            delivered,
            0,
            sourceLabel,
            success ? string.Empty : "입고 중단");
        StockSupplyEvent.Trigger(result);
        return success;
    }

    public static List<StockSupplyResult> RunInternalProduction(
        IEnumerable<IWarehouseFacility> warehouses,
        IEnumerable<StockProductionRule> productionRules)
    {
        List<StockSupplyResult> results = new List<StockSupplyResult>();
        if (productionRules == null) return results;

        foreach (StockProductionRule rule in productionRules)
        {
            GrantReward(warehouses, rule.category, rule.amount, rule.sourceLabel, out StockSupplyResult result);
            results.Add(result);
        }

        return results;
    }

    public static int GetRemainingCapacity(IEnumerable<IWarehouseFacility> warehouses)
    {
        long capacity = 0;
        foreach (IWarehouseFacility warehouse in GetValidWarehouses(warehouses))
        {
            capacity += warehouse.Inventory.RemainingCapacity;
            if (capacity >= int.MaxValue)
            {
                return int.MaxValue;
            }
        }

        return Mathf.Max(0, (int)capacity);
    }

    private static StockDeliveryOffer CreateOffer(StockCategory category, int amount, int unitCost, string sourceLabel)
    {
        int safeAmount = Mathf.Max(0, amount);
        float costMultiplier = RunVariableRuntime.Instance != null
            ? RunVariableRuntime.Instance.GetStockCostMultiplier(category)
            : 1f;
        int cost = Mathf.RoundToInt(safeAmount * Mathf.Max(0, unitCost) * Mathf.Max(0.05f, costMultiplier));
        return new StockDeliveryOffer(category, safeAmount, cost, sourceLabel);
    }

    private static bool CanDepositAll(IEnumerable<IWarehouseFacility> warehouses, int amount)
    {
        int safeAmount = Mathf.Max(0, amount);
        return safeAmount == 0 || GetRemainingCapacity(warehouses) >= safeAmount;
    }

    private static int DepositToWarehouses(IEnumerable<IWarehouseFacility> warehouses, StockCategory category, int amount)
    {
        int remaining = Mathf.Max(0, amount);
        int delivered = 0;

        foreach (IWarehouseFacility warehouse in GetValidWarehouses(warehouses))
        {
            if (remaining <= 0) break;

            int deposited = warehouse.Inventory.Deposit(category, remaining);
            delivered += deposited;
            remaining -= deposited;
        }

        return delivered;
    }

    private static IEnumerable<IWarehouseFacility> GetValidWarehouses(IEnumerable<IWarehouseFacility> warehouses)
    {
        return warehouses?
            .Where((warehouse) => warehouse != null && warehouse.HasWarehouseInventory && warehouse.Inventory != null)
            ?? Enumerable.Empty<IWarehouseFacility>();
    }

    private static StockSupplyResult Fail(
        StockCategory category,
        int requestedAmount,
        int cost,
        string sourceLabel,
        string reason)
    {
        return new StockSupplyResult(false, category, requestedAmount, 0, cost, sourceLabel, reason);
    }
}
