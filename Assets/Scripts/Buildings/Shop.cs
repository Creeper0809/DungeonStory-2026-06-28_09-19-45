using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shop : BuildableObject, IInteractable
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
        AbilityShopping shopable = character.GetAbility<AbilityShopping>();
        AbilityMove moveable = character.GetAbility<AbilityMove>();
        if (shopable == null || moveable == null) yield break;

        int howmany = shopable.GetShoppingCount();
        int usedMoney = 0;
        for(int i = 0;i< howmany;i++)
        {
            Stock buyItem = shopable.DetermineBuyingItem(GetStock());
            yield return moveable.Move2PosBySpeed(GetRandomBuyPos(), 0.7f);
            if (buyItem.id == -1) continue;
            RemainStock remainStock = stocks.FirstOrDefault((stock) => stock.id == buyItem.id);
            if (remainStock == null || remainStock.stock <= 0) continue;

            yield return shopable.BuyItem(remainStock);
            usedMoney += buyItem.cost;
            remainStock.stock--;
        }
        if (usedMoney == 0)
        {
            yield break;
        }
        int endX = buildPoses.Max((pos) => pos.x) - 1;
        Vector2 endPos = grid.GetWorldPos(new Vector2Int(endX, centerPos.y));
        yield return moveable.Move2PosBySpeed(endPos);

        GameManager gameManager = GameManager.Current;
        if (gameManager != null
            && gameManager.numbers != null
            && gameManager.numbers.TryGetValue(NumberCondition.ONEARNMONEY, out var earnMoneyNumber))
        {
            earnMoneyNumber.Spawn(endPos + Vector2.up, usedMoney);
        }

        if (gameData == null && gameManager != null)
        {
            gameData = gameManager.gameData;
        }

        if (gameData != null)
        {
            gameData.holdingMoney.Value += usedMoney;
        }

        yield return new WaitForSeconds(0.5f);
    }
    public List<Stock> GetStock()
    {
        List<Stock> result = new List<Stock>();
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
        return stocks.Sum(stock => stock.stock);
    }
    public Vector2 GetRandomBuyPos()
    {
        Vector2 startPos = grid.GetWorldPos(centerPos + new Vector2(0.5f, 0));
        Vector2 endPos = grid.GetWorldPos(centerPos + new Vector2Int(-1, 0));
        return new Vector2(UnityEngine.Random.Range(startPos.x, endPos.x), startPos.y);
    }
    public override bool isVisitable()
    {
        return true;
    }
    public IEnumerator AllocateWorker(Character character)
    {
        if (worker != null && worker != character)
        {
            yield break;
        }
        worker = character;
        float endX = buildPoses.Max((pos) => pos.x) - 0.2f;
        Vector2 endPos = grid.GetWorldPos(new Vector2(endX, centerPos.y));
        yield return character.GetAbility<AbilityMove>().Move2PosBySpeed(endPos);
        character.ChangeLayer("DungeonMiddleObject");
        character.transform.position = endPos + new Vector2(0, 0.15f);
        character.Flip(Character.Facing.RIGHT);
    }
    public void DeallocateWorker(Character character)
    {
        if (worker != character) return;

        worker = null;
        character.transform.position = character.transform.position - new Vector3(0, 0.15f);
        character.ChangeLayer("Default");
    }
    private void FillStock()
    {
        stocks.Clear();
        if (baseStock == null || baseStock.stocks == null) return;

        foreach (var stockTuple in baseStock.stocks)
        {
            if (stockTuple == null || stockTuple.Item1 == null) continue;

            var(stock, count) = stockTuple;
            stocks.Add(new RemainStock(stock.id,stock.itemName,Mathf.FloorToInt(stock.cost * baseStock.multifly),count,stock.buyevent));
        }
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
