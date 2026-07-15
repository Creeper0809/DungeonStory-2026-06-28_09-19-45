using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public class Shop : BuildableObject, IInteractable, IStockedFacility, IWorkableFacility
{
    private const float CheckoutWaitPollSeconds = 0.2f;
    private const float SelfServiceCheckoutSeconds = 0.8f;
    private const float StaffedCheckoutSeconds = 0.35f;
    private const float WaitingCheckoutOperateUrgency = 160f;
    private const float WaitingCheckoutOperateUrgencyPerCustomer = 40f;
    private static readonly IGameDataProvider FallbackGameDataProvider = new ShopFallbackGameDataProvider();
    private static readonly IShopStockCatalog FallbackStockCatalog = new ShopResourcesStockCatalog();
    private static readonly IFloatingNumberFeedbackService FallbackFloatingNumberFeedbackService =
        new ShopNoopFloatingNumberFeedbackService();
    private static readonly IWorkforceReplanService FallbackWorkforceReplanService =
        new ShopNoopWorkforceReplanService();

    public enum Type
    {
        X,
        Food,
        Item
    }
    private List<RemainStock> stocks = new List<RemainStock>();
    private CharacterActor worker;
    private int waitingCheckoutCount;
    public Type type { get; private set; }
    private StockInfo baseStock;
    private GameData gameData;
    private IGameDataProvider gameDataProvider;
    private IShopStockCatalog stockCatalog;
    private IFloatingNumberFeedbackService floatingNumberFeedbackService;
    private IWorkforceReplanService workforceReplanService;
    private bool stockInitialized;
    public int CurrentStock => GetStockCount();
    public bool HasAvailableStock => CurrentStock > 0;
    public int WaitingCheckoutCount => waitingCheckoutCount;
    public bool HasWaitingCheckout => waitingCheckoutCount > 0;
    public bool HasServingWorker
    {
        get
        {
            PruneInvalidWorker();
            return worker != null;
        }
    }

    public StockCategory ActiveStockCategory => ResolveActiveStockCategory();
    public int MaxInternalStock => GetMaxInternalStock(ActiveStockCategory);
    public int MissingStock => Mathf.Max(0, MaxInternalStock - CurrentStock);
    public bool NeedsRestock => MissingStock > 0;
    public bool RequiresStaffedCheckout => RequiresServingWorker();
    public bool UsesSelfService => !RequiresStaffedCheckout;
    public float CurrentPriceMultiplier => GetPriceMultiplier();
    public IReadOnlyList<ShopProductSnapshot> ProductSnapshots
    {
        get
        {
            EnsureStockInitialized();
            return stocks
                .Where((stock) => stock != null && IsSaleItemAllowed(stock.id))
                .Select((stock) =>
                {
                    Stock priced = CreatePricedStock(stock);
                    return new ShopProductSnapshot(stock.id, stock.itemName, priced.cost, stock.stock);
                })
                .ToArray();
        }
    }

    public override void Initialization(BuildingSO buildingSO, Vector2Int buildPos)
    {
        base.Initialization(buildingSO, buildPos);
        baseStock = null;
        stockInitialized = false;
        type = Type.X;
        TryResolveGameData(requireProvider: false);
        TryInitializeStock(requireCatalog: false);
    }

    [Inject]
    public void ConstructShop(
        IGameDataProvider gameDataProvider,
        IShopStockCatalog stockCatalog,
        IFloatingNumberFeedbackService floatingNumberFeedbackService,
        IWorkforceReplanService workforceReplanService)
    {
        this.gameDataProvider = gameDataProvider
            ?? throw new ArgumentNullException(nameof(gameDataProvider));
        this.stockCatalog = stockCatalog
            ?? throw new ArgumentNullException(nameof(stockCatalog));
        this.floatingNumberFeedbackService = floatingNumberFeedbackService
            ?? throw new ArgumentNullException(nameof(floatingNumberFeedbackService));
        this.workforceReplanService = workforceReplanService
            ?? throw new ArgumentNullException(nameof(workforceReplanService));

        TryResolveGameData(requireProvider: true);
        TryInitializeStock(requireCatalog: false);
    }

    public IEnumerator Interact(CharacterActor actor)
    {
        EnsureStockInitialized();
        if (!TryBeginUse(actor, out string failureReason))
        {
            actor?.AddLog($"{objectNameOrDefault()} 이용 실패: {failureReason}");
            yield break;
        }

        AbilityShopping shopable = actor != null ? actor.GetAbility<AbilityShopping>() : null;
        AbilityMove moveable = actor != null ? actor.GetAbility<AbilityMove>() : null;
        if (shopable == null || moveable == null)
        {
            EndUse(actor);
            yield break;
        }

        AIAction currentAction = actor != null && actor.Brain != null
            ? actor.Brain.bestAction
            : null;
        int howmany = shopable.GetShoppingCount();
        Dictionary<int, int> selectedCounts = new Dictionary<int, int>();
        List<RemainStock> cart = new List<RemainStock>();
        bool createsRevenue = CreatesRevenueFor(actor);
        for (int i = 0; i < howmany; i++)
        {
            Stock buyItem = shopable.DetermineBuyingItem(GetStock(selectedCounts));
            actor?.Brain?.SetActionPhase("\uC0C1\uD488 \uB458\uB7EC\uBCF4\uAE30", this, $"{i + 1}/{howmany}");
            yield return moveable.Move2PosBySpeed(
                GetFacilityAnchorWorldPosition(FacilityAnchorKind.Use, actor.transform.position),
                0.7f,
                currentAction);
            yield return Linger(actor, 0.1f, currentAction);
            if (buyItem.id == -1) continue;

            RemainStock remainStock = stocks.FirstOrDefault((stock) => stock.id == buyItem.id);
            if (remainStock == null || GetRemainingStockAfterSelection(remainStock, selectedCounts) <= 0) continue;

            cart.Add(remainStock);
            selectedCounts.TryGetValue(remainStock.id, out int selectedCount);
            selectedCounts[remainStock.id] = selectedCount + 1;
        }

        if (cart.Count == 0)
        {
            actor?.AddLog($"{objectNameOrDefault()} 이용 실패: 구매 가능한 상품 없음");
            EndUse(actor);
            yield break;
        }

        Vector2 endPos = GetFacilityAnchorWorldPosition(FacilityAnchorKind.Checkout, actor.transform.position);
        actor?.Brain?.SetActionPhase("\uACC4\uC0B0\uB300 \uC774\uB3D9", this);
        yield return moveable.Move2PosBySpeed(endPos, 1f, currentAction);
        yield return WaitForServingWorker(actor);
        if (!CanServeCustomer(actor, out string serviceFailureReason))
        {
            actor?.AddLog($"{objectNameOrDefault()} 계산 대기 중단: {serviceFailureReason}");
            EndUse(actor);
            yield break;
        }

        yield return RunCheckoutService(actor);
        if (TryResolveCheckoutCrime(actor, cart))
        {
            EndUse(actor);
            yield break;
        }

        int usedMoney = 0;
        int purchaseCount = 0;
        foreach (RemainStock remainStock in cart)
        {
            if (remainStock == null || remainStock.stock <= 0)
            {
                continue;
            }

            Stock pricedStock = CreatePricedStock(remainStock);
            if (!shopable.CanPay(pricedStock))
            {
                continue;
            }

            yield return shopable.BuyItem(remainStock, pricedStock.cost);
            purchaseCount++;
            FacilityStockConsumedEvent.Trigger(actor, this, GetStockCategory(remainStock.id), 1);
            if (createsRevenue)
            {
                usedMoney += pricedStock.cost;
            }
            remainStock.stock--;
            MarkFacilityDynamicStateDirty();
        }

        if (purchaseCount == 0)
        {
            actor?.AddLog($"{objectNameOrDefault()} 이용 실패: 구매 가능한 상품 없음");
            EndUse(actor);
            yield break;
        }

        actor?.ApplyMoodFactor(
            $"shopping:{GetInstanceID()}",
            "마음에 드는 물건을 삼",
            5f,
            120f,
            2);

        if (createsRevenue && usedMoney > 0)
        {
            RequireFloatingNumberFeedbackService().TryShow(NumberCondition.ONEARNMONEY, endPos + Vector2.up, usedMoney);
        }

        TryResolveGameData(requireProvider: true);

        if (createsRevenue && usedMoney > 0 && gameData != null)
        {
            gameData.holdingMoney.Value += usedMoney;
        }

        if (createsRevenue && usedMoney > 0)
        {
            FacilityRevenueEvent.Trigger(actor, this, usedMoney);
        }

        if (!createsRevenue)
        {
            actor?.AddLog($"{objectNameOrDefault()} 직원 이용: 매출 제외");
        }

        RoomEnvironmentExperienceEvent.Trigger(
            actor,
            this,
            RoomExperienceActivity.Shopping);
        yield return new WaitForSeconds(0.5f);
        EndUse(actor);
    }

    public static bool CreatesRevenueFor(CharacterActor actor)
    {
        return actor == null || !IsInternalStaffUse(actor);
    }

    public static bool IsInternalStaffUse(CharacterActor actor)
    {
        return CharacterWorkRoleUtility.TryGetWork(actor, out _);
    }

    public bool CanServeCustomer(CharacterActor actor, out string failureReason)
    {
        failureReason = string.Empty;
        if (!CreatesRevenueFor(actor))
        {
            return true;
        }

        if (!RequiresServingWorker())
        {
            return true;
        }

        if (HasServingWorker)
        {
            return true;
        }

        failureReason = "직원 없음";
        return false;
    }

    public float GetCheckoutCrimeChance(int cartItemCount)
    {
        return GetCheckoutCrimeChance(null, cartItemCount, 0);
    }

    public float GetCheckoutCrimeChance(CharacterActor actor, int cartItemCount, int cartValue)
    {
        return FacilityCrimeRiskUtility.CalculateShopliftingChance(new FacilityCrimeRiskContext(
            Facility,
            actor,
            HasServingWorker,
            HasWaitingCheckout,
            CurrentUserCount,
            cartItemCount,
            cartValue,
            CurrentStock,
            IsDamaged));
    }

    private bool TryResolveCheckoutCrime(CharacterActor actor, IReadOnlyList<RemainStock> cart)
    {
        if (!CreatesRevenueFor(actor))
        {
            return false;
        }

        int cartItemCount = cart != null ? cart.Count : 0;
        int cartValue = GetCartValue(cart);
        float chance = GetCheckoutCrimeChance(actor, cartItemCount, cartValue);
        if (!FacilityCrimeRiskUtility.ShouldTriggerCrime(chance, UnityEngine.Random.value))
        {
            return false;
        }

        RemainStock stolenStock = cart?.FirstOrDefault((stock) => stock != null && stock.stock > 0);
        if (stolenStock == null)
        {
            return false;
        }

        stolenStock.stock--;
        MarkFacilityDynamicStateDirty();
        StockCategory category = GetStockCategory(stolenStock.id);
        FacilityStockConsumedEvent.Trigger(actor, this, category, 1);

        int lossValue = Mathf.Max(0, stolenStock.cost);
        string detail = BuildCrimeDetail(actor, stolenStock, lossValue, chance);
        FacilityCrimeEvent.Trigger(actor, this, FacilityCrimeKind.Shoplifting, detail, lossValue);
        EventAlertService.Raise("Shoplifting", detail, EventAlertImportance.Medium, "Crime");
        actor?.AddLog(detail);
        return true;
    }

    private string BuildCrimeDetail(CharacterActor actor, RemainStock stolenStock, int lossValue, float chance)
    {
        string actorName = actor != null ? actor.name : "Unknown customer";
        string itemName = stolenStock != null && !string.IsNullOrWhiteSpace(stolenStock.itemName)
            ? stolenStock.itemName
            : "item";
        return $"{actorName} stole {itemName} from {objectNameOrDefault()} (loss {lossValue}, chance {chance:0.##}).";
    }

    private static int GetCartValue(IReadOnlyList<RemainStock> cart)
    {
        int total = 0;
        if (cart == null)
        {
            return total;
        }

        foreach (RemainStock stock in cart)
        {
            if (stock != null)
            {
                total += Mathf.Max(0, stock.cost);
            }
        }

        return total;
    }

    private IEnumerator RunCheckoutService(CharacterActor actor)
    {
        if (!CreatesRevenueFor(actor))
        {
            yield break;
        }

        bool selfService = !HasServingWorker;
        if (selfService)
        {
            waitingCheckoutCount++;
            MarkFacilityDynamicStateDirty();
            RequireWorkforceReplanService().RequestIdleWorkersToReplan();
            actor?.AddLog($"{objectNameOrDefault()} self-service checkout");
        }

        WaitForSeconds wait = new WaitForSeconds(selfService ? SelfServiceCheckoutSeconds : StaffedCheckoutSeconds);
        try
        {
            yield return wait;
        }
        finally
        {
            if (selfService)
            {
                waitingCheckoutCount = Mathf.Max(0, waitingCheckoutCount - 1);
                MarkFacilityDynamicStateDirty();
            }
        }
    }

    private IEnumerator WaitForServingWorker(CharacterActor actor)
    {
        if (!ShouldWaitForServingWorker(actor))
        {
            yield break;
        }

        waitingCheckoutCount++;
        MarkFacilityDynamicStateDirty();
        RequireWorkforceReplanService().RequestIdleWorkersToReplan();
        actor?.AddLog($"{objectNameOrDefault()} 계산 대기: 직원 없음");

        WaitForSeconds wait = new WaitForSeconds(CheckoutWaitPollSeconds);
        try
        {
            while (ShouldWaitForServingWorker(actor))
            {
                yield return wait;
            }
        }
        finally
        {
            waitingCheckoutCount = Mathf.Max(0, waitingCheckoutCount - 1);
            MarkFacilityDynamicStateDirty();
        }

        if (HasServingWorker)
        {
            actor?.AddLog($"{objectNameOrDefault()} 계산 시작");
        }
    }

    private bool ShouldWaitForServingWorker(CharacterActor actor)
    {
        return actor != null
            && !isDestroy
            && CreatesRevenueFor(actor)
            && RequiresServingWorker()
            && !HasServingWorker;
    }

    public List<Stock> GetStock()
    {
        return GetStock(null);
    }

    private List<Stock> GetStock(IReadOnlyDictionary<int, int> selectedCounts)
    {
        EnsureStockInitialized();
        List<Stock> result = new List<Stock>();
        PruneInvalidWorker();
        foreach (RemainStock stock in stocks)
        {
            if (stock == null
                || !IsSaleItemAllowed(stock.id)
                || GetRemainingStockAfterSelection(stock, selectedCounts) <= 0) continue;
            result.Add(CreatePricedStock(stock));
        }
        return result;
    }

    private Stock CreatePricedStock(RemainStock stock)
    {
        if (stock == null)
        {
            return new Stock(-1, 0);
        }

        return new Stock(stock.id, Mathf.FloorToInt(stock.cost * GetPriceMultiplier()));
    }

    private float GetPriceMultiplier()
    {
        PruneInvalidWorker();
        return worker == null ? 1.0f : 1.2f;
    }

    private static int GetRemainingStockAfterSelection(
        RemainStock stock,
        IReadOnlyDictionary<int, int> selectedCounts)
    {
        if (stock == null)
        {
            return 0;
        }

        int selected = 0;
        selectedCounts?.TryGetValue(stock.id, out selected);
        return stock.stock - selected;
    }
    public int GetStockCount()
    {
        int count = 0;
        foreach (RemainStock stock in stocks)
        {
            if (stock != null && IsSaleItemAllowed(stock.id))
            {
                count += stock.stock;
            }
        }

        return count;
    }

    public int RestockFrom(IEnumerable<IWarehouseFacility> warehouses, int maxAmount, out string resultMessage)
    {
        EnsureStockInitialized();
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
            if (!IsSaleItemAllowed(saleItem)) continue;
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
            MarkFacilityDynamicStateDirty();
        }

        return restocked;
    }

    public bool TryFindRestockSource(
        IEnumerable<IWarehouseFacility> warehouses,
        int maxAmount,
        out IWarehouseFacility warehouse,
        out SaleItem saleItem,
        out int amount,
        out string failureReason)
    {
        warehouse = null;
        saleItem = null;
        amount = 0;
        failureReason = string.Empty;
        EnsureStockInitialized();

        if (baseStock == null || baseStock.stocks == null || baseStock.stocks.Count == 0)
        {
            failureReason = "보충할 상품 데이터가 없습니다";
            return false;
        }

        int targetAmount = Mathf.Min(Mathf.Max(0, maxAmount), MissingStock);
        if (targetAmount <= 0)
        {
            failureReason = "재고가 이미 충분함";
            return false;
        }

        foreach (var stockTuple in baseStock.stocks)
        {
            if (stockTuple == null || stockTuple.Item1 == null)
            {
                continue;
            }

            SaleItem candidateItem = stockTuple.Item1;
            if (!IsSaleItemAllowed(candidateItem))
            {
                continue;
            }
            IWarehouseFacility candidateWarehouse = warehouses?
                .FirstOrDefault((candidate) => candidate != null
                    && candidate.HasWarehouseInventory
                    && candidate.Inventory != null
                    && candidate.Inventory.HasStock(candidateItem.category));
            if (candidateWarehouse == null)
            {
                continue;
            }

            int available = candidateWarehouse.Inventory.GetStock(candidateItem.category);
            int loadAmount = Mathf.Min(targetAmount, available);
            if (loadAmount <= 0)
            {
                continue;
            }

            warehouse = candidateWarehouse;
            saleItem = candidateItem;
            amount = loadAmount;
            return true;
        }

        failureReason = "창고 재고 부족";
        return false;
    }

    public int ReceiveRestock(SaleItem saleItem, int amount, int requestedAmount, out string resultMessage)
    {
        resultMessage = string.Empty;
        if (saleItem == null)
        {
            resultMessage = "보충할 상품 데이터가 없습니다";
            return 0;
        }

        if (!IsSaleItemAllowed(saleItem))
        {
            resultMessage = "현재 방 구성과 맞지 않는 상품입니다.";
            return 0;
        }

        int restocked = Mathf.Min(Mathf.Max(0, amount), MissingStock);
        if (restocked <= 0)
        {
            resultMessage = "재고가 이미 충분함";
            return 0;
        }

        AddRemainStock(saleItem, restocked);
        resultMessage = $"{restocked}개 보충";
        FacilityRestockEvent.Trigger(this, Mathf.Max(0, requestedAmount), restocked, resultMessage);
        MarkFacilityDynamicStateDirty();
        return restocked;
    }

    public bool HasRestockSupply(IEnumerable<IWarehouseFacility> warehouses, out string failureReason)
    {
        EnsureStockInitialized();
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
            if (!IsSaleItemAllowed(stockTuple.Item1))
            {
                continue;
            }
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

#if UNITY_EDITOR
    public void DebugClearStock()
    {
        stocks.Clear();
        MarkFacilityDynamicStateDirty();
    }
#endif

    public Vector2 GetRandomBuyPos()
    {
        Vector2 firstPos = grid.GetWorldPos(centerPos + new Vector2(0.5f, 0));
        Vector2 secondPos = grid.GetWorldPos(centerPos + new Vector2Int(-1, 0));
        float minX = Mathf.Min(firstPos.x, secondPos.x);
        float maxX = Mathf.Max(firstPos.x, secondPos.x);
        return new Vector2(UnityEngine.Random.Range(minX, maxX), firstPos.y);
    }

    private Vector2 GetCheckoutWorldPosition()
    {
        int endX = buildPoses.Max((pos) => pos.x) - 1;
        return grid.GetWorldPos(new Vector2Int(endX, centerPos.y));
    }

    public override float GetWorkUrgency(FacilityWorkType workType)
    {
        float urgency = base.GetWorkUrgency(workType);
        if (workType == FacilityWorkType.Operate
            && waitingCheckoutCount > 0
            && !HasServingWorker)
        {
            urgency += WaitingCheckoutOperateUrgency
                + waitingCheckoutCount * WaitingCheckoutOperateUrgencyPerCustomer;
        }

        return urgency;
    }

    public override bool isVisitable()
    {
        return CanVisit((CharacterActor)null, out _);
    }

    public bool CanAssignWorker(CharacterActor actor, out string failureReason)
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

        if (worker != null && worker != actor)
        {
            failureReason = "이미 근무자 있음";
            return false;
        }

        if (HasWorkerReservationForOther(actor))
        {
            failureReason = "이미 작업 예약됨";
            return false;
        }

        return true;
    }

    public IEnumerator AllocateWorker(CharacterActor actor)
    {
        PruneInvalidWorker();
        if (worker != null && worker != actor)
        {
            yield break;
        }
        worker = actor;
        MarkFacilityDynamicStateDirty();
        ReleaseWorkerReservation(actor);
        AbilityMove moveable = actor != null ? actor.GetAbility<AbilityMove>() : null;
        if (moveable == null) yield break;

        Vector2 endPos = GetFacilityAnchorWorldPosition(FacilityAnchorKind.Work, actor.transform.position);
        AIAction currentAction = actor != null && actor.Brain != null
            ? actor.Brain.bestAction
            : null;
        actor?.Brain?.SetActionPhase("\uC791\uC5C5\uB300 \uC811\uADFC", this);
        yield return moveable.Move2PosBySpeed(endPos, 1f, currentAction);
        actor.ChangeLayer("DungeonMiddleObject");
        yield return moveable.Move2PosBySpeed(endPos + new Vector2(0, 0.15f), 3f, currentAction);
        actor?.Brain?.SetActionPhase("\uC791\uC5C5 \uC790\uC138", this);
        actor.Flip(CharacterFacing.RIGHT);
    }

    public void DeallocateWorker(CharacterActor actor)
    {
        if (actor == null)
        {
            return;
        }

        PruneInvalidWorker();
        if (worker != actor) return;

        worker = null;
        MarkFacilityDynamicStateDirty();
        actor.Brain?.SetActionPhase("\uC2DC\uC124 \uD1F4\uC7A5", this);
        actor.transform.position = actor.transform.position - new Vector3(0, 0.15f);
        Vector2Int actorGridPosition = grid != null
            ? grid.GetXY(actor.transform.position)
            : centerPos;
        if (!ContainsGridPosition(actorGridPosition)
            && TryGetFacilityOccupiedWorldPosition(actor.transform.position, out Vector3 exitPosition))
        {
            actor.transform.position = exitPosition;
        }
        actor.ChangeLayer("Default");
    }

    private static IEnumerator Linger(CharacterActor actor, float seconds, AIAction expectedAction)
    {
        if (seconds <= 0f)
        {
            yield break;
        }

        float timer = 0f;
        while (timer < seconds)
        {
            if (expectedAction != null
                && (actor == null
                    || actor.Brain == null
                    || actor.Brain.bestAction != expectedAction
                    || actor.Brain.isBestActionEnd))
            {
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }
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
                MarkFacilityDynamicStateDirty();
            }
        }
        catch (MissingReferenceException)
        {
            worker = null;
            MarkFacilityDynamicStateDirty();
        }
    }

    private bool RequiresServingWorker()
    {
        return Facility != null
            && Facility.requiresStaffedService
            && Facility.SupportsWork(FacilityWorkType.Operate);
    }

    private void FillStock()
    {
        stocks.Clear();
        if (baseStock == null || baseStock.stocks == null) return;

        FacilityRoomOperationalProfile profile = GetRoomOperationalProfile();
        Dictionary<StockCategory, int> remainingByCategory = new Dictionary<StockCategory, int>();
        foreach (var stockTuple in baseStock.stocks)
        {
            if (stockTuple == null || stockTuple.Item1 == null) continue;

            var(stock, count) = stockTuple;
            if (!remainingByCategory.TryGetValue(stock.category, out int remainingCapacity))
            {
                remainingCapacity = GetConfiguredInternalStockCapacity()
                    + profile.GetStorageCapacity(stock.category);
            }

            int initialStock = Mathf.Min(Mathf.Max(0, count), remainingCapacity);
            if (initialStock <= 0) continue;

            stocks.Add(CreateRemainStock(stock, initialStock));
            remainingByCategory[stock.category] = remainingCapacity - initialStock;
        }

        MarkFacilityDynamicStateDirty();
    }

    private void AddRemainStock(SaleItem saleItem, int amount)
    {
        if (saleItem == null || amount <= 0) return;

        RemainStock remainStock = stocks.FirstOrDefault((stock) => stock.id == saleItem.id);
        if (remainStock == null)
        {
            stocks.Add(CreateRemainStock(saleItem, amount));
            MarkFacilityDynamicStateDirty();
            return;
        }

        remainStock.stock += amount;
        MarkFacilityDynamicStateDirty();
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
        return RequireStockCatalog().GetStockCategory(saleItemId);
    }

    private bool IsSaleItemAllowed(int saleItemId)
    {
        return ShouldUseBaseStockPassThrough()
            || GetStockCategory(saleItemId) == ActiveStockCategory;
    }

    private bool IsSaleItemAllowed(SaleItem saleItem)
    {
        return saleItem != null
            && (ShouldUseBaseStockPassThrough()
                || saleItem.category == ActiveStockCategory);
    }

    private StockCategory ResolveActiveStockCategory()
    {
        FacilityRoomOperationalProfile profile = GetRoomOperationalProfile();
        if (HasRetailSpecializationSignal(profile))
        {
            return profile.RetailCategory;
        }

        return TryGetSingleBaseStockCategory(out StockCategory fallbackCategory)
            ? fallbackCategory
            : profile.RetailCategory;
    }

    private static bool HasRetailSpecializationSignal(FacilityRoomOperationalProfile profile)
    {
        if (profile?.Parts == null)
        {
            return false;
        }

        foreach (BuildableObject part in profile.Parts)
        {
            FacilityOperationalData data = part?.Operational;
            if (data == null)
            {
                continue;
            }

            if (data.HasFunction(FacilityFunction.MealProduction)
                || data.HasFunction(FacilityFunction.MeatProduction)
                || data.HasFunction(FacilityFunction.MealService)
                || data.HasFunction(FacilityFunction.RetailGeneral)
                || data.HasFunction(FacilityFunction.RetailWeapon)
                || data.HasFunction(FacilityFunction.WeaponCrafting))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryGetSingleBaseStockCategory(out StockCategory category)
    {
        category = StockCategory.General;
        if (baseStock == null || baseStock.stocks == null)
        {
            return false;
        }

        bool found = false;
        foreach (var stockTuple in baseStock.stocks)
        {
            SaleItem saleItem = stockTuple?.Item1;
            if (saleItem == null)
            {
                continue;
            }

            if (!found)
            {
                category = saleItem.category;
                found = true;
                continue;
            }

            if (saleItem.category != category)
            {
                return false;
            }
        }

        return found;
    }

    private bool ShouldUseBaseStockPassThrough()
    {
        FacilityRoomOperationalProfile profile = GetRoomOperationalProfile();
        return !HasRetailSpecializationSignal(profile)
            && !TryGetSingleBaseStockCategory(out _);
    }

    private int GetMaxInternalStock(StockCategory category)
    {
        return GetConfiguredInternalStockCapacity()
            + GetRoomOperationalProfile().GetStorageCapacity(category);
    }

    private int GetConfiguredInternalStockCapacity()
    {
        return Facility != null && Facility.internalStockMax > 0
            ? Facility.internalStockMax
            : GetConfiguredStockCapacity();
    }

    public ShopStockStateSnapshot CreateStockSnapshot()
    {
        EnsureStockInitialized();
        return new ShopStockStateSnapshot
        {
            items = stocks
                .Where((stock) => stock != null)
                .Select((stock) => new ShopStockItemSnapshot
                {
                    saleItemId = stock.id,
                    amount = Mathf.Max(0, stock.stock)
                })
                .ToList()
        };
    }

    public void ApplyStockSnapshot(ShopStockStateSnapshot snapshot)
    {
        EnsureStockInitialized();
        stocks.Clear();
        if (snapshot?.items == null || baseStock?.stocks == null)
        {
            MarkFacilityDynamicStateDirty();
            return;
        }

        foreach (ShopStockItemSnapshot item in snapshot.items)
        {
            SaleItem saleItem = baseStock.stocks
                .Where((tuple) => tuple != null)
                .Select((tuple) => tuple.Item1)
                .FirstOrDefault((candidate) => candidate != null && candidate.id == item.saleItemId);
            if (saleItem != null && item.amount > 0)
            {
                stocks.Add(CreateRemainStock(saleItem, item.amount));
            }
        }

        MarkFacilityDynamicStateDirty();
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

    private bool TryInitializeStock(bool requireCatalog)
    {
        if (stockInitialized)
        {
            return baseStock != null;
        }

        if (BuildingData == null)
        {
            return false;
        }

        if (stockCatalog == null)
        {
            stockCatalog = FallbackStockCatalog;
        }

        stockInitialized = true;
        if (!stockCatalog.TryGetStockInfoForShop(id, out baseStock))
        {
            Debug.LogWarning($"{name} 상점 재고 데이터를 찾지 못했습니다. shopId: {id}");
            return false;
        }

        type = baseStock.type;
        FillStock();
        return true;
    }

    private void EnsureStockInitialized()
    {
        TryInitializeStock(requireCatalog: true);
    }

    private bool TryResolveGameData(bool requireProvider)
    {
        if (gameData != null)
        {
            return true;
        }

        if (gameDataProvider == null)
        {
            gameDataProvider = FallbackGameDataProvider;
        }

        return gameDataProvider.TryGetGameData(out gameData);
    }

    private IShopStockCatalog RequireStockCatalog()
    {
        return stockCatalog ?? FallbackStockCatalog;
    }

    private IFloatingNumberFeedbackService RequireFloatingNumberFeedbackService()
    {
        return floatingNumberFeedbackService ?? FallbackFloatingNumberFeedbackService;
    }

    private IWorkforceReplanService RequireWorkforceReplanService()
    {
        return workforceReplanService ?? FallbackWorkforceReplanService;
    }

    private sealed class ShopFallbackGameDataProvider : IGameDataProvider
    {
        private GameData fallbackGameData;

        public bool TryGetGameData(out GameData resolvedGameData)
        {
            fallbackGameData ??= CreateFallbackGameData();
            resolvedGameData = fallbackGameData;
            return resolvedGameData != null;
        }

        private static GameData CreateFallbackGameData()
        {
            GameData data = ScriptableObject.CreateInstance<GameData>();
            data.hideFlags = HideFlags.HideAndDontSave;
            data.gameSpeed = new Data<int>();
            data.holdingMoney = new Data<int>();
            data.day = new Data<int>();
            data.curTime = new Data<float>();
            data.hour = new Data<int>();
            data.timeOfDay = new Data<TimeOfDay>();
            data.gameSpeed.Initialize(1);
            data.holdingMoney.Initialize(100000);
            data.day.Initialize(1);
            data.curTime.Initialize(0f);
            data.hour.Initialize(0);
            data.timeOfDay.Initialize(TimeOfDay.Morning);
            return data;
        }
    }

    private sealed class ShopResourcesStockCatalog : IShopStockCatalog
    {
        public bool TryGetStockInfoForShop(int shopId, out StockInfo stockInfo)
        {
            stockInfo = Resources.LoadAll<StockInfo>("SO/Stock")
                .FirstOrDefault(candidate => candidate != null && candidate.shopId == shopId);
            return stockInfo != null;
        }

        public bool TryGetSaleItem(int saleItemId, out SaleItem saleItem)
        {
            saleItem = Resources.LoadAll<SaleItem>("SO/Stock")
                .FirstOrDefault(candidate => candidate != null && candidate.id == saleItemId);
            return saleItem != null;
        }

        public StockCategory GetStockCategory(int saleItemId)
        {
            return TryGetSaleItem(saleItemId, out SaleItem saleItem)
                ? saleItem.category
                : StockCategory.General;
        }
    }

    private sealed class ShopNoopFloatingNumberFeedbackService : IFloatingNumberFeedbackService
    {
        public bool TryShow(NumberCondition condition, Vector3 worldPosition, float value) => false;
    }

    private sealed class ShopNoopWorkforceReplanService : IWorkforceReplanService
    {
        public void RequestIdleWorkersToReplan(bool clearFailures = true) { }
    }
}

public readonly struct ShopProductSnapshot
{
    public ShopProductSnapshot(int id, string name, int price, int quantity)
    {
        Id = id;
        Name = name ?? string.Empty;
        Price = Mathf.Max(0, price);
        Quantity = Mathf.Max(0, quantity);
    }

    public int Id { get; }
    public string Name { get; }
    public int Price { get; }
    public int Quantity { get; }
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

[Serializable]
public sealed class ShopStockStateSnapshot
{
    public List<ShopStockItemSnapshot> items = new List<ShopStockItemSnapshot>();
}

[Serializable]
public sealed class ShopStockItemSnapshot
{
    public int saleItemId;
    public int amount;
}
