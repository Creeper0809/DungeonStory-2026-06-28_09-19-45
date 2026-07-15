using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public enum FacilityShopOfferType
{
    Building,
    Blueprint
}

public enum FacilityShopRarity
{
    Common,
    Rare,
    Special
}

[Serializable]
public class FacilityShopOfferSnapshot
{
    public FacilityShopOfferType type;
    public FacilityShopRarity rarity;
    public string displayName;
    public int cost;
    public int star;
    public bool basicPurchase;

    public string ToSummaryText()
    {
        string typeName = type == FacilityShopOfferType.Blueprint ? "설계도" : "시설";
        string rarityText = rarity == FacilityShopRarity.Common ? string.Empty : $" / {rarity}";
        string basicText = basicPurchase ? " / 기본 구매" : string.Empty;
        string starText = star > 0 ? $" / {star}성" : string.Empty;
        return $"{typeName}: {displayName}{starText} / 비용 {cost}{rarityText}{basicText}";
    }
}

public class FacilityShopOffer
{
    public FacilityShopOffer(
        BuildingSO building,
        int cost,
        FacilityShopRarity rarity,
        bool basicPurchase,
        bool randomOffer)
    {
        Building = building;
        Cost = Mathf.Max(0, cost);
        Rarity = rarity;
        IsBasicPurchase = basicPurchase;
        IsRandomOffer = randomOffer;
        Type = FacilityShopOfferType.Building;
    }

    public FacilityShopOffer(
        FacilityBlueprintSO blueprint,
        int cost,
        FacilityShopRarity rarity,
        bool randomOffer)
    {
        Blueprint = blueprint;
        Cost = Mathf.Max(0, cost);
        Rarity = rarity;
        IsRandomOffer = randomOffer;
        Type = FacilityShopOfferType.Blueprint;
    }

    public FacilityShopOfferType Type { get; }
    public BuildingSO Building { get; }
    public FacilityBlueprintSO Blueprint { get; }
    public int Cost { get; }
    public FacilityShopRarity Rarity { get; }
    public bool IsBasicPurchase { get; }
    public bool IsRandomOffer { get; }
    public bool IsValid => Type == FacilityShopOfferType.Building ? Building != null : Blueprint != null;
    public int Star => Type == FacilityShopOfferType.Building ? FacilityShopService.GetBuildingStar(Building) : 0;
    public string DisplayName => Type == FacilityShopOfferType.Building
        ? FacilityShopService.GetBuildingName(Building)
        : Blueprint != null ? Blueprint.DisplayName : "설계도";

    public FacilityShopOfferSnapshot ToSnapshot()
    {
        return new FacilityShopOfferSnapshot
        {
            type = Type,
            rarity = Rarity,
            displayName = DisplayName,
            cost = Cost,
            star = Star,
            basicPurchase = IsBasicPurchase
        };
    }
}

public readonly struct FacilityShopPurchaseResult
{
    public readonly bool success;
    public readonly FacilityShopOfferSnapshot offer;
    public readonly FacilityShopOfferType offerType;
    public readonly int dataId;
    public readonly BuildingSO building;
    public readonly FacilityBlueprintSO blueprint;
    public readonly int cost;
    public readonly string message;

    public FacilityShopPurchaseResult(bool success, FacilityShopOffer offer, int cost, string message)
    {
        this.success = success;
        this.offer = offer != null ? offer.ToSnapshot() : null;
        offerType = offer != null ? offer.Type : FacilityShopOfferType.Building;
        building = offer != null ? offer.Building : null;
        blueprint = offer != null ? offer.Blueprint : null;
        dataId = building != null
            ? building.id
            : blueprint != null
                ? blueprint.id
                : -1;
        this.cost = Mathf.Max(0, cost);
        this.message = message ?? string.Empty;
    }
}

public struct FacilityShopRefreshedEvent
{
    public int day;
    public IReadOnlyList<FacilityShopOffer> offers;
    public IReadOnlyList<FacilityShopOffer> basicPurchaseOffers;

    public FacilityShopRefreshedEvent(
        int day,
        IReadOnlyList<FacilityShopOffer> offers,
        IReadOnlyList<FacilityShopOffer> basicPurchaseOffers)
    {
        this.day = Mathf.Max(1, day);
        this.offers = offers ?? Array.Empty<FacilityShopOffer>();
        this.basicPurchaseOffers = basicPurchaseOffers ?? Array.Empty<FacilityShopOffer>();
    }

    private static FacilityShopRefreshedEvent e;

    public static void Trigger(
        int day,
        IReadOnlyList<FacilityShopOffer> offers,
        IReadOnlyList<FacilityShopOffer> basicPurchaseOffers)
    {
        e.day = Mathf.Max(1, day);
        e.offers = offers ?? Array.Empty<FacilityShopOffer>();
        e.basicPurchaseOffers = basicPurchaseOffers ?? Array.Empty<FacilityShopOffer>();
        EventObserver.TriggerEvent(e);
    }
}

public struct FacilityShopPurchasedEvent
{
    public FacilityShopPurchaseResult result;

    public FacilityShopPurchasedEvent(FacilityShopPurchaseResult result)
    {
        this.result = result;
    }

    private static FacilityShopPurchasedEvent e;

    public static void Trigger(FacilityShopPurchaseResult result)
    {
        e.result = result;
        EventObserver.TriggerEvent(e);
    }
}

public class FacilityShopUnlockState
{
    private readonly HashSet<int> basicPurchaseBuildingIds = new HashSet<int>();
    private readonly HashSet<int> acquiredBlueprintIds = new HashSet<int>();

    public IReadOnlyCollection<int> BasicPurchaseBuildingIds => basicPurchaseBuildingIds;
    public IReadOnlyCollection<int> AcquiredBlueprintIds => acquiredBlueprintIds;

    public bool UnlockBasicPurchase(BuildingSO building)
    {
        if (building == null || !FacilityShopService.CanEnterBasicPurchase(building))
        {
            return false;
        }

        return basicPurchaseBuildingIds.Add(building.id);
    }

    public bool UnlockBasicPurchaseById(int buildingId)
    {
        if (buildingId < 0)
        {
            return false;
        }

        return basicPurchaseBuildingIds.Add(buildingId);
    }

    public bool IsBasicPurchaseUnlocked(BuildingSO building)
    {
        return building != null && basicPurchaseBuildingIds.Contains(building.id);
    }

    public bool MarkBlueprintAcquired(FacilityBlueprintSO blueprint)
    {
        if (blueprint == null)
        {
            return false;
        }

        return acquiredBlueprintIds.Add(blueprint.id);
    }

    public bool IsBlueprintAcquired(FacilityBlueprintSO blueprint)
    {
        return blueprint != null && acquiredBlueprintIds.Contains(blueprint.id);
    }
}

public static class FacilityShopService
{
    private const int RandomBuildingSlots = 3;
    private const int GuaranteedBlueprintSlots = 1;
    private const float RareOfferChance = 0.35f;

    public static IReadOnlyList<FacilityShopOffer> CreateDailyOffers(
        int day,
        IFacilityShopCatalog catalog,
        IRunVariableRuntimeReader runVariableReader)
    {
        if (catalog == null)
        {
            throw new ArgumentNullException(nameof(catalog));
        }

        if (runVariableReader == null)
        {
            throw new ArgumentNullException(nameof(runVariableReader));
        }

        return CreateDailyOffers(
            day,
            catalog.Buildings,
            catalog.Blueprints,
            runVariableReader.GetInitialShopSeed(),
            runVariableReader.GetFacilityShopCostMultiplier,
            runVariableReader.GetBlueprintCostMultiplier);
    }

    public static IReadOnlyList<FacilityShopOffer> CreateDailyOffers(
        int day,
        IEnumerable<BuildingSO> buildings,
        IEnumerable<FacilityBlueprintSO> blueprints,
        int runShopSeed,
        Func<BuildingSO, float> buildingCostMultiplier,
        Func<FacilityBlueprintSO, float> blueprintCostMultiplier)
    {
        if (buildingCostMultiplier == null)
        {
            throw new ArgumentNullException(nameof(buildingCostMultiplier));
        }

        if (blueprintCostMultiplier == null)
        {
            throw new ArgumentNullException(nameof(blueprintCostMultiplier));
        }

        int safeDay = Mathf.Max(1, day);
        System.Random random = new System.Random(7919 + (safeDay * 104729) + runShopSeed);
        List<FacilityShopOffer> offers = new List<FacilityShopOffer>();

        List<BuildingSO> buildingPool = buildings?
            .Where(IsDailyShopBuildingCandidate)
            .OrderBy((building) => random.NextDouble())
            .ToList()
            ?? new List<BuildingSO>();

        foreach (BuildingSO building in buildingPool.Take(RandomBuildingSlots))
        {
            offers.Add(CreateBuildingOffer(building, false, true, buildingCostMultiplier));
        }

        List<FacilityBlueprintSO> commonBlueprints = blueprints?
            .Where((blueprint) => blueprint != null && blueprint.rarity == FacilityShopRarity.Common)
            .OrderBy((blueprint) => random.NextDouble())
            .ToList()
            ?? new List<FacilityBlueprintSO>();

        foreach (FacilityBlueprintSO blueprint in commonBlueprints.Take(GuaranteedBlueprintSlots))
        {
            offers.Add(CreateBlueprintOffer(blueprint, true, blueprintCostMultiplier));
        }

        List<FacilityBlueprintSO> rareBlueprints = blueprints?
            .Where((blueprint) => blueprint != null && blueprint.rarity != FacilityShopRarity.Common)
            .OrderBy((blueprint) => random.NextDouble())
            .ToList()
            ?? new List<FacilityBlueprintSO>();

        if (rareBlueprints.Count > 0 && random.NextDouble() <= RareOfferChance)
        {
            offers.Add(CreateBlueprintOffer(rareBlueprints[0], true, blueprintCostMultiplier));
        }

        return offers.Where((offer) => offer != null && offer.IsValid).ToList();
    }

    public static IReadOnlyList<FacilityShopOffer> CreateBasicPurchaseOffers(
        IFacilityShopCatalog catalog,
        FacilityShopUnlockState unlockState,
        IMetaProgressionRuntimeReader metaProgressionReader,
        IRunVariableRuntimeReader runVariableReader)
    {
        if (catalog == null)
        {
            throw new ArgumentNullException(nameof(catalog));
        }

        if (metaProgressionReader == null)
        {
            throw new ArgumentNullException(nameof(metaProgressionReader));
        }

        if (runVariableReader == null)
        {
            throw new ArgumentNullException(nameof(runVariableReader));
        }

        return CreateBasicPurchaseOffers(
            catalog.Buildings,
            unlockState,
            metaProgressionReader.GetExpandedBasicPurchaseBuildingIds(catalog.Buildings),
            runVariableReader.GetFacilityShopCostMultiplier);
    }

    public static IReadOnlyList<FacilityShopOffer> CreateBasicPurchaseOffers(
        IEnumerable<BuildingSO> buildings,
        FacilityShopUnlockState unlockState,
        IEnumerable<int> expandedBasicPurchaseBuildingIds,
        Func<BuildingSO, float> buildingCostMultiplier)
    {
        if (buildingCostMultiplier == null)
        {
            throw new ArgumentNullException(nameof(buildingCostMultiplier));
        }

        if (unlockState == null)
        {
            return Array.Empty<FacilityShopOffer>();
        }

        List<BuildingSO> buildingList = buildings?
            .Where((building) => building != null)
            .ToList()
            ?? new List<BuildingSO>();
        HashSet<int> metaBasicPurchaseIds = expandedBasicPurchaseBuildingIds?.ToHashSet()
            ?? throw new ArgumentNullException(nameof(expandedBasicPurchaseBuildingIds));

        return buildingList
            .Where((building) => (unlockState.IsBasicPurchaseUnlocked(building) || metaBasicPurchaseIds.Contains(building.id))
                && CanEnterBasicPurchase(building))
            .OrderBy((building) => building.id)
            .Select((building) => CreateBuildingOffer(building, true, false, buildingCostMultiplier))
            .Where((offer) => offer != null && offer.IsValid)
            .ToList()
            ?? new List<FacilityShopOffer>();
    }

    public static bool TryPurchaseOffer(
        GameData gameData,
        FacilityShopOffer offer,
        FacilityShopUnlockState unlockState,
        out FacilityShopPurchaseResult result)
    {
        if (offer == null || !offer.IsValid)
        {
            result = new FacilityShopPurchaseResult(false, offer, 0, "상품 정보가 올바르지 않습니다");
            FacilityShopPurchasedEvent.Trigger(result);
            return false;
        }

        if (gameData == null || gameData.holdingMoney == null)
        {
            result = new FacilityShopPurchaseResult(false, offer, offer.Cost, "게임 자금 데이터가 없습니다");
            FacilityShopPurchasedEvent.Trigger(result);
            return false;
        }

        if (gameData.holdingMoney.Value < offer.Cost)
        {
            result = new FacilityShopPurchaseResult(false, offer, offer.Cost, "자금 부족");
            FacilityShopPurchasedEvent.Trigger(result);
            return false;
        }

        gameData.holdingMoney.Value -= offer.Cost;
        string message = ApplyPurchase(offer, unlockState);
        result = new FacilityShopPurchaseResult(true, offer, offer.Cost, message);
        FacilityShopPurchasedEvent.Trigger(result);
        return true;
    }

    public static bool CanEnterBasicPurchase(BuildingSO building)
    {
        return building != null && GetBuildingStar(building) <= 2;
    }

    public static int GetBuildingStar(BuildingSO building)
    {
        if (building == null)
        {
            return 0;
        }

        if (building.Defense != null && building.Defense.IsDefenseFacility)
        {
            return Mathf.Max(1, building.Defense.star);
        }

        string objectName = GetBuildingName(building);
        if (!string.IsNullOrWhiteSpace(objectName) && objectName.Length >= 2 && char.IsDigit(objectName[0]) && objectName[1] == '성')
        {
            return Mathf.Max(1, objectName[0] - '0');
        }

        return 1;
    }

    public static string GetBuildingName(BuildingSO building)
    {
        if (building == null)
        {
            return "시설";
        }

        return string.IsNullOrWhiteSpace(building.objectName) ? building.name : building.objectName;
    }

    public static BuildingSO FindBuildingById(IFacilityShopCatalog catalog, int buildingId)
    {
        if (catalog == null)
        {
            throw new ArgumentNullException(nameof(catalog));
        }

        if (buildingId < 0)
        {
            return null;
        }

        return catalog.FindBuildingById(buildingId);
    }

    private static FacilityShopOffer CreateBuildingOffer(
        BuildingSO building,
        bool basicPurchase,
        bool randomOffer,
        Func<BuildingSO, float> buildingCostMultiplier)
    {
        if (building == null)
        {
            return null;
        }

        FacilityShopRarity rarity = ResolveBuildingRarity(building);
        int cost = CalculateBuildingCost(building, basicPurchase, rarity, buildingCostMultiplier);
        return new FacilityShopOffer(building, cost, rarity, basicPurchase, randomOffer);
    }

    private static FacilityShopOffer CreateBlueprintOffer(
        FacilityBlueprintSO blueprint,
        bool randomOffer,
        Func<FacilityBlueprintSO, float> blueprintCostMultiplier)
    {
        if (blueprint == null)
        {
            return null;
        }

        return new FacilityShopOffer(
            blueprint,
            Mathf.Max(0, Mathf.RoundToInt(blueprint.defaultCost * GetBlueprintCostMultiplier(blueprint, blueprintCostMultiplier))),
            blueprint.rarity,
            randomOffer);
    }

    private static bool IsDailyShopBuildingCandidate(BuildingSO building)
    {
        return building != null
            && !building.IsGridMovement
            && !building.IsWall
            && GetBuildingStar(building) <= 2;
    }

    private static FacilityShopRarity ResolveBuildingRarity(BuildingSO building)
    {
        int star = GetBuildingStar(building);
        if (star >= 2)
        {
            return FacilityShopRarity.Rare;
        }

        return FacilityShopRarity.Common;
    }

    private static int CalculateBuildingCost(
        BuildingSO building,
        bool basicPurchase,
        FacilityShopRarity rarity,
        Func<BuildingSO, float> buildingCostMultiplier)
    {
        int star = Mathf.Max(1, GetBuildingStar(building));
        int categoryWeight = building.category switch
        {
            BuildingCategory.Shop => 90,
            BuildingCategory.Production => 110,
            BuildingCategory.Crafting => 120,
            BuildingCategory.Resource => 130,
            BuildingCategory.Special => 140,
            _ => 100
        };

        int rarityWeight = rarity switch
        {
            FacilityShopRarity.Rare => 80,
            FacilityShopRarity.Special => 160,
            _ => 0
        };
        int basicDiscount = basicPurchase ? 20 : 0;
        int baseCost = Mathf.Max(25, (star * categoryWeight) + rarityWeight - basicDiscount);
        return Mathf.Max(1, Mathf.RoundToInt(baseCost * GetBuildingCostMultiplier(building, buildingCostMultiplier)));
    }

    private static float GetBuildingCostMultiplier(BuildingSO building, Func<BuildingSO, float> buildingCostMultiplier)
    {
        return Mathf.Max(0.05f, buildingCostMultiplier(building));
    }

    private static float GetBlueprintCostMultiplier(
        FacilityBlueprintSO blueprint,
        Func<FacilityBlueprintSO, float> blueprintCostMultiplier)
    {
        return Mathf.Max(0.05f, blueprintCostMultiplier(blueprint));
    }

    private static string ApplyPurchase(FacilityShopOffer offer, FacilityShopUnlockState unlockState)
    {
        if (offer.Type == FacilityShopOfferType.Building)
        {
            offer.Building.unlocked = true;
            if (offer.IsBasicPurchase)
            {
                unlockState?.UnlockBasicPurchase(offer.Building);
            }

            return $"{offer.DisplayName} 구매 완료";
        }

        unlockState?.MarkBlueprintAcquired(offer.Blueprint);
        EventAlertService.RaiseBlueprintAcquired($"{offer.DisplayName} 획득");
        return $"{offer.DisplayName} 설계도 획득";
    }

}

public class DailyFacilityShopRuntime : MonoBehaviour, UtilEventListener<OperatingDayEndedEvent>
{
    [SerializeField] private bool raiseAlertOnRefresh = true;

    private readonly List<FacilityShopOffer> currentDailyOffers = new List<FacilityShopOffer>();
    private readonly FacilityShopUnlockState unlockState = new FacilityShopUnlockState();
    private int currentOfferDay = 1;
    private IFacilityShopCatalog facilityShopCatalog;
    private IRunVariableRuntimeReader runVariableReader;
    private IMetaProgressionRuntimeReader metaProgressionReader;

    public IReadOnlyList<FacilityShopOffer> CurrentDailyOffers => currentDailyOffers;
    public IReadOnlyList<FacilityShopOffer> CurrentBasicPurchaseOffers =>
        FacilityShopService.CreateBasicPurchaseOffers(
            ResolveFacilityShopCatalog(),
            unlockState,
            ResolveMetaProgressionReader(),
            ResolveRunVariableReader());
    public FacilityShopUnlockState UnlockState => unlockState;
    public int CurrentOfferDay => currentOfferDay;

    [Inject]
    public void ConstructDailyFacilityShopRuntime(
        IFacilityShopCatalog facilityShopCatalog,
        IRunVariableRuntimeReader runVariableReader,
        IMetaProgressionRuntimeReader metaProgressionReader)
    {
        this.facilityShopCatalog = facilityShopCatalog
            ?? throw new ArgumentNullException(nameof(facilityShopCatalog));
        this.runVariableReader = runVariableReader
            ?? throw new ArgumentNullException(nameof(runVariableReader));
        this.metaProgressionReader = metaProgressionReader
            ?? throw new ArgumentNullException(nameof(metaProgressionReader));
    }

    private void Start()
    {
        if (currentDailyOffers.Count == 0)
        {
            Refresh(1, false);
        }
    }

    public void OnTriggerEvent(OperatingDayEndedEvent eventType)
    {
        Refresh(Mathf.Max(1, eventType.day + 1), raiseAlertOnRefresh);
    }

    public void Refresh(int day, bool raiseAlert)
    {
        currentOfferDay = Mathf.Max(1, day);
        currentDailyOffers.Clear();
        currentDailyOffers.AddRange(FacilityShopService.CreateDailyOffers(
            currentOfferDay,
            ResolveFacilityShopCatalog(),
            ResolveRunVariableReader()));

        IReadOnlyList<FacilityShopOffer> basicPurchaseOffers = CurrentBasicPurchaseOffers;
        FacilityShopRefreshedEvent.Trigger(currentOfferDay, currentDailyOffers, basicPurchaseOffers);

        if (raiseAlert)
        {
            EventAlertService.Raise(
                "시설 상점 갱신",
                FormatOfferList(currentDailyOffers, basicPurchaseOffers),
                EventAlertImportance.Medium,
                "상점");
        }
    }

    public bool TryPurchaseDailyOffer(int index, GameData gameData, out FacilityShopPurchaseResult result)
    {
        if (index < 0 || index >= currentDailyOffers.Count)
        {
            result = new FacilityShopPurchaseResult(false, null, 0, "선택한 상품이 없습니다");
            FacilityShopPurchasedEvent.Trigger(result);
            return false;
        }

        return FacilityShopService.TryPurchaseOffer(gameData, currentDailyOffers[index], unlockState, out result);
    }

    public bool TryPurchaseBasicOffer(int index, GameData gameData, out FacilityShopPurchaseResult result)
    {
        IReadOnlyList<FacilityShopOffer> offers = CurrentBasicPurchaseOffers;
        if (index < 0 || index >= offers.Count)
        {
            result = new FacilityShopPurchaseResult(false, null, 0, "선택한 기본 구매 상품이 없습니다");
            FacilityShopPurchasedEvent.Trigger(result);
            return false;
        }

        return FacilityShopService.TryPurchaseOffer(gameData, offers[index], unlockState, out result);
    }

    private void OnEnable()
    {
        this.EventStartListening<OperatingDayEndedEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<OperatingDayEndedEvent>();
    }

    private static string FormatOfferList(
        IEnumerable<FacilityShopOffer> dailyOffers,
        IEnumerable<FacilityShopOffer> basicPurchaseOffers)
    {
        List<string> lines = new List<string> { "일일 상품:" };
        List<string> dailyRows = dailyOffers?
            .Where((offer) => offer != null && offer.IsValid)
            .Select((offer) => $"- {offer.ToSnapshot().ToSummaryText()}")
            .ToList()
            ?? new List<string>();
        if (dailyRows.Count > 0)
        {
            lines.AddRange(dailyRows);
        }
        else
        {
            lines.Add("- 없음");
        }

        List<string> basicRows = basicPurchaseOffers?
            .Where((offer) => offer != null && offer.IsValid)
            .Select((offer) => $"- {offer.ToSnapshot().ToSummaryText()}")
            .ToList()
            ?? new List<string>();
        lines.Add(string.Empty);
        lines.Add("기본 구매:");
        if (basicRows.Count > 0)
        {
            lines.AddRange(basicRows);
        }
        else
        {
            lines.Add("- 없음");
        }
        return string.Join("\n", lines);
    }

    private IFacilityShopCatalog ResolveFacilityShopCatalog()
    {
        return facilityShopCatalog
            ?? throw new InvalidOperationException($"{nameof(DailyFacilityShopRuntime)} requires {nameof(IFacilityShopCatalog)} injection.");
    }

    private IRunVariableRuntimeReader ResolveRunVariableReader()
    {
        return runVariableReader
            ?? throw new InvalidOperationException($"{nameof(DailyFacilityShopRuntime)} requires {nameof(IRunVariableRuntimeReader)} injection.");
    }

    private IMetaProgressionRuntimeReader ResolveMetaProgressionReader()
    {
        return metaProgressionReader
            ?? throw new InvalidOperationException($"{nameof(DailyFacilityShopRuntime)} requires {nameof(IMetaProgressionRuntimeReader)} injection.");
    }
}
