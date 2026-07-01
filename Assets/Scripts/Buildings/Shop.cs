using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shop : BuildableObject, IInteractable, IStockedFacility, IWorkableFacility
{
    public enum Type
    {
        X,
        Food,
        Item
    }
    private List<RemainStock> stocks = new List<RemainStock>();
    private Character worker;
    public Type type { get; private set; }
    private StockInfo baseStock;
    private GameData gameData;
    public int CurrentStock => GetStockCount();
    public bool HasAvailableStock => CurrentStock > 0;
    public int MaxInternalStock => Facility != null && Facility.internalStockMax > 0
        ? Facility.internalStockMax
        : GetConfiguredStockCapacity();
    public int MissingStock => Mathf.Max(0, MaxInternalStock - CurrentStock);
    public bool NeedsRestock => MissingStock > 0;

    public override void Initialization(BuildingSO buildingSO, Vector2Int buildPos)
    {
        base.Initialization(buildingSO, buildPos);
        gameData = GameManager.Current != null ? GameManager.Current.gameData : null;

        Dictionary<int, StockInfo> stockInfos = DataManager.Instance.GetData<StockInfo>();
        baseStock = stockInfos?.Values.FirstOrDefault((x) => x.shopId == id);
        if (baseStock == null)
        {
            Debug.LogWarning($"{name} 상점 재고 데이터를 찾지 못했습니다. shopId: {id}");
            return;
        }

        type = baseStock.type;
        FillStock();
    }
    public IEnumerator Interact(Character character)
    {
        if (!TryBeginUse(character, out string failureReason))
        {
            character?.AddLog($"{objectNameOrDefault()} 이용 실패: {failureReason}");
            yield break;
        }

        AbilityShopping shopable = character.GetAbility<AbilityShopping>();
        AbilityMove moveable = character.GetAbility<AbilityMove>();
        if (shopable == null || moveable == null)
        {
            EndUse(character);
            yield break;
        }

        int howmany = shopable.GetShoppingCount();
        int usedMoney = 0;
        int purchaseCount = 0;
        bool createsRevenue = CreatesRevenueFor(character);
        for(int i = 0;i< howmany;i++)
        {
            Stock buyItem = shopable.DetermineBuyingItem(GetStock());
            yield return moveable.Move2PosBySpeed(GetRandomBuyPos(), 0.7f);
            if (buyItem.id == -1) continue;
            RemainStock remainStock = stocks.FirstOrDefault((stock) => stock.id == buyItem.id);
            if (remainStock == null || remainStock.stock <= 0) continue;

            yield return shopable.BuyItem(remainStock);
            purchaseCount++;
            FacilityStockConsumedEvent.Trigger(character, this, GetStockCategory(remainStock.id), 1);
            if (createsRevenue)
            {
                usedMoney += buyItem.cost;
            }
            remainStock.stock--;
            FacilityCandidateCache.MarkDynamicStateDirty();
        }
        if (purchaseCount == 0)
        {
            character?.AddLog($"{objectNameOrDefault()} 이용 실패: 구매 가능한 상품 없음");
            EndUse(character);
            yield break;
        }
        int endX = buildPoses.Max((pos) => pos.x) - 1;
        Vector2 endPos = grid.GetWorldPos(new Vector2Int(endX, centerPos.y));
        yield return moveable.Move2PosBySpeed(endPos);

        GameManager gameManager = GameManager.Current;
        if (createsRevenue
            && usedMoney > 0
            && gameManager != null
            && gameManager.numbers != null
            && gameManager.numbers.TryGetValue(NumberCondition.ONEARNMONEY, out var earnMoneyNumber))
        {
            earnMoneyNumber.Spawn(endPos + Vector2.up, usedMoney);
        }

        if (gameData == null && gameManager != null)
        {
            gameData = gameManager.gameData;
        }

        if (createsRevenue && usedMoney > 0 && gameData != null)
        {
            gameData.holdingMoney.Value += usedMoney;
        }

        if (createsRevenue && usedMoney > 0)
        {
            FacilityRevenueEvent.Trigger(character, this, usedMoney);
        }

        if (!createsRevenue)
        {
            character?.AddLog($"{objectNameOrDefault()} 직원 이용: 매출 제외");
        }

        yield return new WaitForSeconds(0.5f);
        EndUse(character);
    }

    public static bool CreatesRevenueFor(Character character)
    {
        return character == null || !IsInternalStaffUse(character);
    }

    public static bool IsInternalStaffUse(Character character)
    {
        return CharacterWorkRoleUtility.TryGetWork(character, out _);
    }

    public List<Stock> GetStock()
    {
        List<Stock> result = new List<Stock>();
        PruneInvalidWorker();
        float multifly = worker == null ? 1.0f : 1.2f;
        foreach (RemainStock stock in stocks)
        {
            if (stock.stock <= 0) continue;
            result.Add(new Stock(stock.id, Mathf.FloorToInt(stock.cost * multifly)));
        }
        return result;
    }
    public int GetStockCount()
    {
        int count = 0;
        foreach (RemainStock stock in stocks)
        {
            if (stock != null)
            {
                count += stock.stock;
            }
        }

        return count;
    }

    public int RestockFrom(IEnumerable<IWarehouseFacility> warehouses, int maxAmount, out string resultMessage)
    {
        resultMessage = string.Empty;
        if (baseStock == null || baseStock.stocks == null || baseStock.stocks.Count == 0)
        {
            resultMessage = "보충할 상품 데이터가 없습니다";
            return 0;
        }

        int targetAmount = Mathf.Min(Mathf.Max(0, maxAmount), MissingStock);
        if (targetAmount <= 0)
        {
            resultMessage = "재고가 이미 가득 찼습니다";
            return 0;
        }

        int restocked = 0;
        foreach (var stockTuple in baseStock.stocks)
        {
            if (stockTuple == null || stockTuple.Item1 == null) continue;

            SaleItem saleItem = stockTuple.Item1;
            while (restocked < targetAmount)
            {
                IWarehouseFacility warehouse = warehouses?
                    .FirstOrDefault((candidate) => candidate != null
                        && candidate.HasWarehouseInventory
                        && candidate.Inventory.HasStock(saleItem.category));
                if (warehouse == null)
                {
                    break;
                }

                int withdrawn = warehouse.Inventory.Withdraw(saleItem.category, 1);
                if (withdrawn <= 0)
                {
                    break;
                }

                AddRemainStock(saleItem, withdrawn);
                restocked += withdrawn;
            }

            if (restocked >= targetAmount)
            {
                break;
            }
        }

        resultMessage = restocked > 0
            ? $"{restocked}개 보충"
            : "창고 재고 부족";
        FacilityRestockEvent.Trigger(this, targetAmount, restocked, resultMessage);
        if (restocked > 0)
        {
            FacilityCandidateCache.MarkDynamicStateDirty();
        }

        return restocked;
    }

    public bool HasRestockSupply(IEnumerable<IWarehouseFacility> warehouses, out string failureReason)
    {
        failureReason = string.Empty;
        if (!NeedsRestock)
        {
            failureReason = "재고가 이미 충분함";
            return false;
        }

        if (baseStock == null || baseStock.stocks == null || baseStock.stocks.Count == 0)
        {
            failureReason = "보충할 상품 데이터가 없습니다";
            return false;
        }

        foreach (var stockTuple in baseStock.stocks)
        {
            if (stockTuple == null || stockTuple.Item1 == null)
            {
                continue;
            }

            StockCategory category = stockTuple.Item1.category;
            if (warehouses != null
                && warehouses.Any((warehouse) => warehouse != null
                    && warehouse.HasWarehouseInventory
                    && warehouse.Inventory != null
                    && warehouse.Inventory.HasStock(category)))
            {
                return true;
            }
        }

        failureReason = "창고 재고 부족";
        return false;
    }

    public Vector2 GetRandomBuyPos()
    {
        Vector2 startPos = grid.GetWorldPos(centerPos + new Vector2(0.5f, 0));
        Vector2 endPos = grid.GetWorldPos(centerPos + new Vector2Int(-1, 0));
        return new Vector2(UnityEngine.Random.Range(startPos.x, endPos.x), startPos.y);
    }
    public override bool isVisitable()
    {
        return CanVisit(null, out _);
    }

    public bool CanAssignWorker(Character character, out string failureReason)
    {
        PruneInvalidWorker();
        bool hasAssignableWork = false;
        failureReason = "지원하지 않는 작업";
        foreach (FacilityWorkType workType in WorkTaskCatalog.GetSingleTypes(Facility != null ? Facility.supportedWorkTypes : FacilityWorkType.Operate))
        {
            if (CanAssignWork(workType, out failureReason))
            {
                hasAssignableWork = true;
                break;
            }
        }

        if (!hasAssignableWork)
        {
            return false;
        }

        if (worker != null && worker != character)
        {
            failureReason = "이미 근무자 있음";
            return false;
        }

        if (HasWorkerReservationForOther(character))
        {
            failureReason = "이미 작업 예약됨";
            return false;
        }

        return true;
    }

    public IEnumerator AllocateWorker(Character character)
    {
        PruneInvalidWorker();
        if (worker != null && worker != character)
        {
            yield break;
        }
        worker = character;
        ReleaseWorkerReservation(character);
        float endX = buildPoses.Max((pos) => pos.x) - 0.2f;
        Vector2 endPos = grid.GetWorldPos(new Vector2(endX, centerPos.y));
        yield return character.GetAbility<AbilityMove>().Move2PosBySpeed(endPos);
        character.ChangeLayer("DungeonMiddleObject");
        character.transform.position = endPos + new Vector2(0, 0.15f);
        character.Flip(Character.Facing.RIGHT);
    }
    public void DeallocateWorker(Character character)
    {
        PruneInvalidWorker();
        if (worker != character) return;

        worker = null;
        character.transform.position = character.transform.position - new Vector3(0, 0.15f);
        if (TryGetNearestWalkableWorldPosition(character.transform.position, out Vector3 exitPosition))
        {
            character.transform.position = exitPosition;
        }
        character.ChangeLayer("Default");
    }

    private void PruneInvalidWorker()
    {
        if (worker == null)
        {
            return;
        }

        try
        {
            if (worker.gameObject == null
                || !worker.gameObject.scene.IsValid()
                || !worker.gameObject.activeInHierarchy)
            {
                worker = null;
            }
        }
        catch (MissingReferenceException)
        {
            worker = null;
        }
    }

    private void FillStock()
    {
        stocks.Clear();
        if (baseStock == null || baseStock.stocks == null) return;

        foreach (var stockTuple in baseStock.stocks)
        {
            if (stockTuple == null || stockTuple.Item1 == null) continue;

            var(stock, count) = stockTuple;
            stocks.Add(CreateRemainStock(stock, count));
        }

        FacilityCandidateCache.MarkDynamicStateDirty();
    }

    private void AddRemainStock(SaleItem saleItem, int amount)
    {
        if (saleItem == null || amount <= 0) return;

        RemainStock remainStock = stocks.FirstOrDefault((stock) => stock.id == saleItem.id);
        if (remainStock == null)
        {
            stocks.Add(CreateRemainStock(saleItem, amount));
            FacilityCandidateCache.MarkDynamicStateDirty();
            return;
        }

        remainStock.stock += amount;
        FacilityCandidateCache.MarkDynamicStateDirty();
    }

    private RemainStock CreateRemainStock(SaleItem saleItem, int count)
    {
        return new RemainStock(
            saleItem.id,
            saleItem.itemName,
            Mathf.FloorToInt(saleItem.cost * (baseStock != null ? baseStock.multifly : 1f)),
            count,
            saleItem.buyevent);
    }

    private StockCategory GetStockCategory(int saleItemId)
    {
        Dictionary<int, SaleItem> saleItems = DataManager.Instance != null
            ? DataManager.Instance.GetData<SaleItem>()
            : null;

        return saleItems != null && saleItems.TryGetValue(saleItemId, out SaleItem item)
            ? item.category
            : StockCategory.General;
    }

    private int GetConfiguredStockCapacity()
    {
        if (baseStock == null || baseStock.stocks == null)
        {
            return 0;
        }

        int capacity = 0;
        foreach (var stockTuple in baseStock.stocks)
        {
            if (stockTuple != null)
            {
                capacity += Mathf.Max(0, stockTuple.Item2);
            }
        }

        return capacity;
    }

    private string objectNameOrDefault()
    {
        return BuildingData != null && !string.IsNullOrWhiteSpace(BuildingData.objectName)
            ? BuildingData.objectName
            : name;
    }
}
public class RemainStock
{
    public int id;
    public string itemName;
    public int cost;
    public int stock;
    public OnBuyItemSO[] onbuy;
    public RemainStock(int id,string itemName, int cost, int stock, OnBuyItemSO[] onbuy)
    {
        this.id = id;
        this.itemName = itemName;
        this.cost = cost;
        this.stock = stock;
        this.onbuy = onbuy;
    }
}
public struct Stock
{
    public int id;
    public int cost;

    public Stock(int id, int cost) : this()
    {
        this.id = id;
        this.cost = cost;
    }
}
