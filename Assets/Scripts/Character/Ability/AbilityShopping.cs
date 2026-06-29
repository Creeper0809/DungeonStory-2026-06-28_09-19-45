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
    private const int DefaultMaxLookAroundCount = 1;

    private int holdingMoney;
    public int visitCount { get; private set; }
    public int lookAroundCount { get; private set; }
    public List<BuildableObject> visitedBuilding { get; private set; }
    public override void Initializtion(CharacterSO data)
    {
        base.Initializtion(data);
        visitedBuilding = new List<BuildableObject>();
        visitCount = data.GetFrequencyVisit();
        lookAroundCount = 0;
        holdingMoney = data.GetHoldingMoney();
    }
    public Stock DetermineBuyingItem(List<Stock> stocks)
    {
        if (stocks == null || !stocks.Any())
        {
            return new Stock(-1,0);
        }

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
        AIAction action = character != null && character.ai != null
            ? character.ai.bestAction
            : null;
        if (action == null || action.destination == null)
        {
            character?.AddLog("쇼핑 실패: 목적지 없음");
            if (character != null && character.ai != null)
            {
                character.ai.isBestActionEnd = true;
            }
            yield break;
        }

        if (move == null || grid == null)
        {
            CacheCommonReferences();
        }
        if (move == null || grid == null)
        {
            character?.AddLog("쇼핑 실패: 이동 정보 없음");
            if (character != null && character.ai != null)
            {
                character.ai.isBestActionEnd = true;
            }
            yield break;
        }

        yield return move.MoveByCurrentBestActionPath();
        if(grid.GetGridCell(grid.GetXY(transform.position))?.GetBuildingInlayer() == action.destination && action.destination is IInteractable shop)
        {
            yield return shop.Interact(character);
            RegisterVisit(action.destination);
        }
        else
        {
            character?.AddLog("쇼핑 실패: 목적지 도달 실패");
        }
        if (character != null && character.ai != null)
        {
            character.ai.isBestActionEnd = true;
        }
    }

    public void RegisterVisit(BuildableObject building)
    {
        if (building != null && !visitedBuilding.Contains(building))
        {
            visitedBuilding.Add(building);
        }

        if (visitCount > 0)
        {
            visitCount--;
        }
    }

    public void RegisterLookAround()
    {
        lookAroundCount++;
    }

    public bool CanLookAround()
    {
        return character != null
            && ShouldUseVisitFallback()
            && lookAroundCount < DefaultMaxLookAroundCount;
    }

    public bool ShouldExitDungeon()
    {
        return character != null
            && ShouldUseVisitFallback()
            && lookAroundCount >= DefaultMaxLookAroundCount;
    }

    public bool ShouldUseVisitFallback()
    {
        return visitCount <= 0 || !IsThereVisitableBuilding();
    }

    public bool IsThereVisitableBuilding()
    {
        if (grid == null)
        {
            CacheCommonReferences();
        }

        if (grid == null)
        {
            return false;
        }

        if (visitCount <= 0)
        {
            return false;
        }

        IEnumerable<BuildableObject> reachableBuildings = character != null
            ? character.GetReachableBuilding()
            : Enumerable.Empty<BuildableObject>();

        return reachableBuildings
            .Where((building) => building != null && !building.isDestroy && !visitedBuilding.Contains(building))
            .Any();
    }
    public BuildableObject FindShop()
    {
        IEnumerable<BuildableObject> reachableBuildings = character != null
            ? character.GetReachableBuilding()
            : Enumerable.Empty<BuildableObject>();

        List<BuildableObject> reachableBulding = reachableBuildings
                                                           .Where((x) => !visitedBuilding.Contains(x))
                                                           .OrderBy((x) => Guid.NewGuid())
                                                           .ToList();
        if (!reachableBulding.Any())
        {
            return null;
        }

        if (character.data.favoriteStore == null || character.data.favoriteStore.Length == 0)
        {
            return reachableBulding.First();
        }

        int wantId = character.data.favoriteStore[Random.Range(0, character.data.favoriteStore.Length)].id;
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
