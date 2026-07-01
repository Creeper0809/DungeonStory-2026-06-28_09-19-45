using DamageNumbersPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
public class AbilityShopping : CharacterAbility
{
    private const int DefaultMaxLookAroundCount = 1;
    private const float DefaultPurchaseFeedbackIconMaxWorldSize = 0.45f;

    private int holdingMoney;
    [SerializeField, Min(0.05f)]
    private float purchaseFeedbackIconMaxWorldSize = DefaultPurchaseFeedbackIconMaxWorldSize;

    public int visitCount { get; private set; }
    public int lookAroundCount { get; private set; }
    public List<BuildableObject> visitedBuilding { get; private set; }
    public int HoldingMoney => holdingMoney;
    public override void Initializtion(CharacterSO data)
    {
        base.Initializtion(data);
        visitedBuilding = new List<BuildableObject>();
        visitCount = data != null ? data.GetFrequencyVisit() : 1;
        lookAroundCount = 0;
        holdingMoney = data != null
            ? data.GetHoldingMoney(character != null ? character.profile : null)
            : 0;
    }
    public Stock DetermineBuyingItem(List<Stock> stocks)
    {
        if (stocks == null || stocks.Count == 0)
        {
            return new Stock(-1,0);
        }

        if (Shop.IsInternalStaffUse(character))
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
        return character != null
            && (Shop.IsInternalStaffUse(character) || stock.cost <= holdingMoney);
    }

    public bool CanBuyFrom(Shop shop, out string failureReason)
    {
        failureReason = string.Empty;
        if (shop == null)
        {
            failureReason = "상점 없음";
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

        if (Shop.IsInternalStaffUse(character))
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
            action.ReleaseReservation(character);
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
            action.ReleaseReservation(character);
            RegisterVisit(action.destination);
        }
        else
        {
            character?.AddLog("쇼핑 실패: 목적지 도달 실패");
            action.ReleaseReservation(character);
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

    public void BeginOffDutyVisitCycle()
    {
        visitCount = Mathf.Max(visitCount, 1);
        lookAroundCount = DefaultMaxLookAroundCount;
        visitedBuilding?.Clear();
    }

    public bool IsOffDutyStaffVisitor()
    {
        return character != null
            && Shop.IsInternalStaffUse(character)
            && character.TryGetAbility(out AbilityWork work)
            && work.IsOffDuty;
    }

    public bool CanLookAround()
    {
        return character != null
            && ShouldEndVisitCycle()
            && lookAroundCount < DefaultMaxLookAroundCount;
    }

    public bool ShouldExitDungeon()
    {
        return character != null
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

        if (character == null)
        {
            return false;
        }

        foreach (BuildableObject building in character.GetReachableBuilding())
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
        if (character == null)
        {
            return null;
        }

        BuildableObject randomCandidate = null;
        BuildableObject favorite = null;
        int candidateCount = 0;
        int favoriteCount = 0;
        int wantId = character.data != null
            && character.data.favoriteStore != null
            && character.data.favoriteStore.Length > 0
                ? character.data.favoriteStore[Random.Range(0, character.data.favoriteStore.Length)].id
                : -1;

        foreach (BuildableObject building in character.GetReachableBuilding())
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
        if (building == null
            || building.isDestroy
            || visitedBuilding.Contains(building)
            || !building.CanVisit(character, out _))
        {
            return false;
        }

        return building is not Shop shop || CanBuyFrom(shop, out _);
    }

    public int GetShoppingCount()
    {
        int baseCount = UnityEngine.Random.Range(1, 4);
        float multiplier = character != null ? character.GetConsumptionMultiplier() : 1f;
        return Mathf.Max(1, Mathf.RoundToInt(baseCount * multiplier));
    }
    public IEnumerator BuyItem(RemainStock item)
    {
        if (GameManager.Instance != null
            && GameManager.Instance.numbers != null
            && GameManager.Instance.numbers.TryGetValue(NumberCondition.ONBUYINGITEM, out var buyingNumber)
            && DataManager.Instance != null
            && DataManager.Instance.GetData<SaleItem>() != null
            && DataManager.Instance.GetData<SaleItem>().TryGetValue(item.id, out SaleItem iteminfo))
        {
            DamageNumber number = buyingNumber.Spawn(gameObject.transform.position + Vector3.up);
            SpriteRenderer iconRenderer = number.GetComponent<SpriteRenderer>();
            if (iconRenderer != null)
            {
                iconRenderer.sprite = iteminfo.itemSprite;
                FitPurchaseFeedbackIcon(iconRenderer);
            }
        }

        yield return new WaitForSeconds(0.5f);
        if (!Shop.IsInternalStaffUse(character))
        {
            holdingMoney -= item.cost;
        }
        foreach(var events in item.onbuy)
        {
            events.Onbuy(character);
        }
    }

    private void FitPurchaseFeedbackIcon(SpriteRenderer iconRenderer)
    {
        if (iconRenderer == null || iconRenderer.sprite == null)
        {
            return;
        }

        Vector2 spriteSize = iconRenderer.sprite.bounds.size;
        float longestSide = Mathf.Max(spriteSize.x, spriteSize.y);
        if (longestSide <= Mathf.Epsilon)
        {
            return;
        }

        float maxWorldSize = purchaseFeedbackIconMaxWorldSize > 0f
            ? purchaseFeedbackIconMaxWorldSize
            : DefaultPurchaseFeedbackIconMaxWorldSize;
        float fittedScale = Mathf.Min(1f, maxWorldSize / longestSide);
        iconRenderer.drawMode = SpriteDrawMode.Simple;
        iconRenderer.transform.localScale = new Vector3(fittedScale, fittedScale, iconRenderer.transform.localScale.z);
    }
}
