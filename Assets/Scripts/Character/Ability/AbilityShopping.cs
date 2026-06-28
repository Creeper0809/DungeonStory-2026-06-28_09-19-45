using DamageNumbersPro;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
public class AbilityShopping : CharacterAbility
{
    private int holdingMoney;
    public int visitCount { get; private set; }
    public List<BuildableObject> visitedBuilding { get; private set; }
    public override void Initializtion(CharacterSO data)
    {
        visitedBuilding = new List<BuildableObject>();
        visitCount = data.GetFrequencyVisit();
        holdingMoney = data.GetHoldingMoney();
    }
    public Stock DetermineBuyingItem(List<Stock> stocks)
    {
        if(character.characterType == CharacterType.NPC)
        {
            return stocks.OrderBy((_) => Guid.NewGuid()).First();
        }
        List<Stock> filter = stocks.Where((stock) => stock.cost <= holdingMoney).ToList();
        if (!filter.Any()) return new Stock(-1,0);
        return filter.OrderBy((_) => Guid.NewGuid()).First();
    }
    public void StartSopping()
    {
        StartCoroutine(Shopping());
    }
    private IEnumerator Shopping()
    {
        yield return move.MoveByPath(character.ai.bestAction.path);
        if(grid.GetGridCell(grid.GetXY(transform.position)).GetBuildingInlayer() == character.ai.bestAction.destination && character.ai.bestAction.destination is IInteractable shop)
        {
            yield return shop.Interact(character);
            visitedBuilding.Add(character.ai.bestAction.destination);
        }
        character.ai.isBestActionEnd = true;
    }
    public bool IsThereVisitableBuilding()
    {
        Vector2Int startPos = grid.GetXY(transform.position);
        startPos = grid.IsValidGridPos(startPos) ? startPos : Vector2Int.zero;
        return grid.GetAllVisitableBuilding(startPos)
                          .Where((x) => !visitedBuilding.Contains(x))
                          .Any();
    }
    public BuildableObject FindShop()
    {
        Vector2Int startPos = grid.GetXY(transform.position);
        startPos = grid.IsValidGridPos(startPos) ? startPos : Vector2Int.zero;
        int wantId = character.data.favoriteStore[Random.Range(0, character.data.favoriteStore.Length)].id;
        List<BuildableObject> reachableBulding = grid.GetAllVisitableBuilding(startPos)
                                                           .Where((x) => !visitedBuilding.Contains(x))
                                                           .OrderBy((x) => Guid.NewGuid())
                                                           .ToList();
        if (!reachableBulding.Any())
        {
            return null;
        }
        BuildableObject selectedBuilding = reachableBulding.Any((building) => building.id == wantId) 
                                                              ? reachableBulding.Find((building) => building.id == wantId)
                                                              : reachableBulding.First();
        return selectedBuilding;
    }
    public int GetShoppingCount()
    {
        return UnityEngine.Random.Range(1, 4);
    }
    public IEnumerator BuyItem(RemainStock item)
    {
        DamageNumber number = GameManager.Instance.numbers[NumberCondition.ONBUYINGITEM].Spawn(gameObject.transform.position + Vector3.up);
        SaleItem iteminfo = DataManager.Instance.GetData<SaleItem>()[item.id];
        number.GetComponent<SpriteRenderer>().sprite = iteminfo.itemSprite;
        //사는 애니메이션
        yield return new WaitForSeconds(0.5f);
        if(character.characterType != CharacterType.NPC)
        {
            holdingMoney -= item.cost;
        }
        foreach(var events in item.onbuy)
        {
            events.Onbuy(character);
        }
    }
}
