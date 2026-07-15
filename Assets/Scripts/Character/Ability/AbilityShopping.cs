using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;
public class AbilityShopping : CharacterAbility
{
    private const int DefaultMaxLookAroundCount = 1;

    private int holdingMoney;
    [SerializeField, Min(0.05f)]
    private float purchaseFeedbackIconMaxWorldSize = FloatingIconFeedbackDefaults.DefaultMaxWorldSize;
    private IShopStockCatalog shopStockCatalog;
    private IFloatingIconFeedbackService floatingIconFeedbackService;

    public int visitCount { get; private set; }
    public int lookAroundCount { get; private set; }
    public List<BuildableObject> visitedBuilding { get; private set; }
    public int HoldingMoney => holdingMoney;

    [Inject]
    public void ConstructAbilityShopping(
        IShopStockCatalog shopStockCatalog,
        IFloatingIconFeedbackService floatingIconFeedbackService)
    {
        this.shopStockCatalog = shopStockCatalog
            ?? throw new ArgumentNullException(nameof(shopStockCatalog));
        this.floatingIconFeedbackService = floatingIconFeedbackService
            ?? throw new ArgumentNullException(nameof(floatingIconFeedbackService));
    }

    public override void Initializtion(CharacterSO data)
    {
        base.Initializtion(data);
        visitedBuilding = new List<BuildableObject>();
        visitCount = data != null ? data.GetFrequencyVisit() : 1;
        lookAroundCount = 0;
        holdingMoney = data != null
            ? data.GetHoldingMoney(identity != null ? identity.Profile : null)
            : 0;
    }
    public Stock DetermineBuyingItem(List<Stock> stocks)
    {
        if (stocks == null || stocks.Count == 0)
        {
            return new Stock(-1,0);
        }

        if (Shop.IsInternalStaffUse(actor))
        {
            return stocks[Random.Range(0, stocks.Count)];
        }

        int affordableCount = 0;
        Stock selected = default;
        foreach (Stock stock in stocks)
        {
            if (stock.cost > holdingMoney)
            {
                continue;
            }

            affordableCount++;
            if (Random.Range(0, affordableCount) == 0)
            {
                selected = stock;
            }
        }

        return affordableCount > 0 ? selected : new Stock(-1, 0);
    }

    public bool CanPay(Stock stock)
    {
        return actor != null
            && (Shop.IsInternalStaffUse(actor) || stock.cost <= holdingMoney);
    }

    public bool CanBuyFrom(Shop shop, out string failureReason)
    {
        failureReason = string.Empty;
        if (shop == null)
        {
            failureReason = "상점 없음";
            return false;
        }

        if (CharacterVisitPolicy.IsStaffPurchaseShop(actor, shop))
        {
            failureReason = "직원은 구매 상점을 이용하지 않음";
            return false;
        }

        List<Stock> stocks = shop.GetStock();
        if (stocks == null || stocks.Count == 0)
        {
            failureReason = "재고 없음";
            return false;
        }

        bool canPayAny = false;
        foreach (Stock stock in stocks)
        {
            if (CanPay(stock))
            {
                canPayAny = true;
                break;
            }
        }

        if (!canPayAny)
        {
            failureReason = "소지금 부족";
            return false;
        }

        return true;
    }

    public float GetAffordabilityScore(Shop shop)
    {
        if (shop == null) return 1f;

        List<Stock> stocks = shop.GetStock();
        if (stocks == null || stocks.Count == 0)
        {
            return 0f;
        }

        if (Shop.IsInternalStaffUse(actor))
        {
            return 1f;
        }

        int affordableCount = 0;
        foreach (Stock stock in stocks)
        {
            if (CanPay(stock))
            {
                affordableCount++;
            }
        }

        return Mathf.Clamp01((float)affordableCount / stocks.Count);
    }

    public void StartSopping()
    {
        move?.CancelActiveMovement();
        StartCoroutine(Shopping());
    }
    private IEnumerator Shopping()
    {
        AIAction action = actor != null && actor.Brain != null
            ? actor.Brain.bestAction
            : null;
        if (action == null || action.destination == null)
        {
            actor?.AddLog("쇼핑 실패: 목적지 없음");
            if (actor != null && actor.Brain != null)
            {
                actor.Brain.isBestActionEnd = true;
            }
            yield break;
        }

        if (move == null || grid == null)
        {
            CacheCommonReferences();
        }
        if (move == null || grid == null)
        {
            actor?.AddLog("쇼핑 실패: 이동 정보 없음");
            action.ReleaseReservation(actor);
            if (actor != null && actor.Brain != null)
            {
                actor.Brain.isBestActionEnd = true;
            }
            yield break;
        }

        yield return move.MoveByCurrentBestActionPath();
        GridCell destinationCell = grid.GetGridCell(grid.GetXY(transform.position));
        if (destinationCell != null
            && destinationCell.ContainsOccupant(action.destination)
            && action.destination is IInteractable shop)
        {
            yield return shop.Interact(actor);
            action.ReleaseReservation(actor);
            RegisterVisit(action.destination);
        }
        else
        {
            actor?.AddLog("쇼핑 실패: 목적지 도달 실패");
            action.ReleaseReservation(actor);
        }
        if (actor != null && actor.Brain != null)
        {
            actor.Brain.isBestActionEnd = true;
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

    public void BeginOffDutyVisitCycle()
    {
        visitCount = Mathf.Max(visitCount, 1);
        lookAroundCount = DefaultMaxLookAroundCount;
        visitedBuilding?.Clear();
    }

    public bool IsOffDutyStaffVisitor()
    {
        return actor != null
            && Shop.IsInternalStaffUse(actor)
            && abilityCache != null
            && abilityCache.TryGetAbility(out AbilityWork work)
            && work.IsOffDuty;
    }

    public FacilityRole GetInterestRoles()
    {
        return CharacterVisitPolicy.GetInterestRoles(actor);
    }

    public bool CanLookAround()
    {
        return actor != null
            && ShouldEndVisitCycle()
            && lookAroundCount < DefaultMaxLookAroundCount;
    }

    public bool ShouldExitDungeon()
    {
        return actor != null
            && ShouldEndVisitCycle()
            && lookAroundCount >= DefaultMaxLookAroundCount;
    }

    public bool ShouldEndVisitCycle()
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

        if (actor == null)
        {
            return false;
        }

        foreach (BuildableObject building in actor.GetReachableBuilding())
        {
            if (CanVisitBuilding(building))
            {
                return true;
            }
        }

        return false;
    }
    public BuildableObject FindShop()
    {
        if (actor == null)
        {
            return null;
        }

        BuildableObject randomCandidate = null;
        BuildableObject favorite = null;
        int candidateCount = 0;
        int favoriteCount = 0;
        CharacterSO characterData = identity != null ? identity.Data : null;
        int wantId = characterData != null
            && characterData.favoriteStore != null
            && characterData.favoriteStore.Length > 0
                ? characterData.favoriteStore[Random.Range(0, characterData.favoriteStore.Length)].id
                : -1;

        foreach (BuildableObject building in actor.GetReachableBuilding())
        {
            if (!CanVisitBuilding(building))
            {
                continue;
            }

            candidateCount++;
            if (Random.Range(0, candidateCount) == 0)
            {
                randomCandidate = building;
            }

            if (building.id == wantId)
            {
                favoriteCount++;
                if (Random.Range(0, favoriteCount) == 0)
                {
                    favorite = building;
                }
            }
        }

        return favorite != null ? favorite : randomCandidate;
    }

    private bool CanVisitBuilding(BuildableObject building)
    {
        if (!CharacterVisitPolicy.CanVisitBuilding(
            actor,
            building,
            building != null && visitedBuilding.Contains(building),
            out _))
        {
            return false;
        }

        return building is not Shop shop || CanBuyFrom(shop, out _);
    }

    public int GetShoppingCount()
    {
        int baseCount = UnityEngine.Random.Range(1, 4);
        float multiplier = actor != null && actor.Stats != null ? actor.Stats.GetConsumptionMultiplier() : 1f;
        return Mathf.Max(1, Mathf.RoundToInt(baseCount * multiplier));
    }
    public IEnumerator BuyItem(RemainStock item, int purchaseCost)
    {
        IShopStockCatalog catalog = shopStockCatalog
            ?? throw new InvalidOperationException($"{nameof(AbilityShopping)} requires {nameof(IShopStockCatalog)} injection.");
        if (catalog.TryGetSaleItem(item.id, out SaleItem iteminfo))
        {
            RequireFloatingIconFeedbackService().Show(this, iteminfo.itemSprite, purchaseFeedbackIconMaxWorldSize);
        }

        yield return new WaitForSeconds(0.5f);
        if (!Shop.IsInternalStaffUse(actor))
        {
            holdingMoney -= Mathf.Max(0, purchaseCost);
        }
        foreach(var events in item.onbuy)
        {
            events.Onbuy(actor);
        }
    }

    private IFloatingIconFeedbackService RequireFloatingIconFeedbackService()
    {
        return floatingIconFeedbackService
            ?? throw new InvalidOperationException($"{nameof(AbilityShopping)} requires {nameof(IFloatingIconFeedbackService)} injection.");
    }
}
