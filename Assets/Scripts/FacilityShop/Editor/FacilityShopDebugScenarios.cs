using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class FacilityShopDebugScenarios
{
    [MenuItem("DungeonStory/Debug/Facility Shop/Run P1 Facility Shop Scenarios")]
    public static void RunFromMenu()
    {
        bool success = RunAll(true);
        if (!success)
        {
            Debug.LogError("P1 facility shop scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        P1FacilityShopAssetBuilder.EnsureP1FacilityShopAssets();

        List<string> errors = new List<string>();
        RunScenario("일일 상품 시설/설계도 포함", VerifyDailyOffersContainBuildingAndBlueprint, errors);
        RunScenario("희귀 상품 랜덤 등장", VerifyRareOffersAppearRandomly, errors);
        RunScenario("기본 구매 해금", VerifyBasicPurchaseUnlocksLowStarsOnly, errors);
        RunScenario("시설 구매", VerifyBuildingPurchaseUsesMoneyAndUnlocksBuilding, errors);
        RunScenario("설계도 구매", VerifyBlueprintPurchaseUsesMoneyAndRecordsBlueprint, errors);
        RunScenario("운영일 후 상점 갱신", VerifyRuntimeRefreshesAfterOperatingDay, errors);
        RunScenario("정산 보고서 시설 상점 항목", VerifySettlementReportIncludesFacilityShop, errors);

        if (errors.Count > 0)
        {
            foreach (string error in errors)
            {
                Debug.LogError(error);
            }

            return false;
        }

        if (logSuccess)
        {
            Debug.Log("P1 facility shop scenarios passed.");
        }

        return true;
    }

    private static void RunScenario(string name, Func<bool> scenario, List<string> errors)
    {
        try
        {
            if (scenario()) return;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        errors.Add(name);
    }

    private static bool VerifyDailyOffersContainBuildingAndBlueprint()
    {
        IReadOnlyList<FacilityShopOffer> offers = FacilityShopService.CreateDailyOffers(1);

        return offers.Count >= 4
            && offers.Any((offer) => offer.Type == FacilityShopOfferType.Building && offer.Building != null)
            && offers.Any((offer) => offer.Type == FacilityShopOfferType.Blueprint && offer.Blueprint != null)
            && offers.All((offer) => offer.IsValid && offer.Cost > 0)
            && offers.All((offer) => offer.Type != FacilityShopOfferType.Building || offer.Star <= 2);
    }

    private static bool VerifyRareOffersAppearRandomly()
    {
        bool sawRare = false;
        bool sawNoRare = false;
        for (int day = 1; day <= 40; day++)
        {
            IReadOnlyList<FacilityShopOffer> offers = FacilityShopService.CreateDailyOffers(day);
            bool hasRare = offers.Any((offer) => offer.Rarity != FacilityShopRarity.Common);
            sawRare |= hasRare;
            sawNoRare |= !hasRare;
        }

        return sawRare && sawNoRare;
    }

    private static bool VerifyBasicPurchaseUnlocksLowStarsOnly()
    {
        BuildingSO oneStar = LoadBuilding("P1_SpikeTrap");
        BuildingSO twoStar = CreateSyntheticDefenseBuilding(9202, "2성 테스트 방어 시설", 2);
        BuildingSO threeStar = CreateSyntheticDefenseBuilding(9203, "3성 테스트 방어 시설", 3);
        FacilityShopUnlockState state = new FacilityShopUnlockState();

        bool oneUnlocked = state.UnlockBasicPurchase(oneStar);
        bool twoUnlocked = state.UnlockBasicPurchase(twoStar);
        bool threeRejected = !state.UnlockBasicPurchase(threeStar);
        state.UnlockBasicPurchaseById(threeStar.id);

        IReadOnlyList<FacilityShopOffer> offers = FacilityShopService.CreateBasicPurchaseOffers(
            new[] { oneStar, twoStar, threeStar },
            state);

        bool valid = oneUnlocked
            && twoUnlocked
            && threeRejected
            && offers.Any((offer) => offer.Building == oneStar && offer.IsBasicPurchase)
            && offers.Any((offer) => offer.Building != null && offer.Building.id == 9202 && offer.IsBasicPurchase)
            && offers.All((offer) => offer.Star <= 2)
            && offers.All((offer) => offer.Building == null || offer.Building.id != 9203);

        Object.DestroyImmediate(twoStar);
        Object.DestroyImmediate(threeStar);
        return valid;
    }

    private static bool VerifyBuildingPurchaseUsesMoneyAndUnlocksBuilding()
    {
        BuildingSO source = LoadBuilding("P1_GuardRoom");
        BuildingSO building = Object.Instantiate(source);
        building.id = 9301;
        building.unlocked = false;
        GameData gameData = CreateGameData(500);
        FacilityShopUnlockState state = new FacilityShopUnlockState();
        FacilityShopOffer offer = new FacilityShopOffer(
            building,
            120,
            FacilityShopRarity.Common,
            false,
            true);

        bool success = FacilityShopService.TryPurchaseOffer(gameData, offer, state, out FacilityShopPurchaseResult result);

        bool valid = success
            && result.success
            && building.unlocked
            && gameData.holdingMoney.Value == 380
            && result.message.Contains("구매 완료");

        Object.DestroyImmediate(building);
        Object.DestroyImmediate(gameData);
        return valid;
    }

    private static bool VerifyBlueprintPurchaseUsesMoneyAndRecordsBlueprint()
    {
        FacilityBlueprintSO blueprint = LoadBlueprint("BP_CommercialBasics");
        GameData gameData = CreateGameData(500);
        FacilityShopUnlockState state = new FacilityShopUnlockState();
        FacilityShopOffer offer = new FacilityShopOffer(
            blueprint,
            100,
            blueprint.rarity,
            true);
        CountingEventAlertRequestListener alerts = new CountingEventAlertRequestListener();

        bool success = FacilityShopService.TryPurchaseOffer(gameData, offer, state, out FacilityShopPurchaseResult result);
        bool valid = success
            && result.success
            && gameData.holdingMoney.Value == 400
            && state.IsBlueprintAcquired(blueprint)
            && alerts.Requests.Any((request) => request.Title == "설계도 획득");

        alerts.Dispose();
        Object.DestroyImmediate(gameData);
        return valid;
    }

    private static bool VerifyRuntimeRefreshesAfterOperatingDay()
    {
        GameObject runtimeObject = new GameObject("DailyFacilityShopRuntime_Test");
        DailyFacilityShopRuntime runtime = runtimeObject.AddComponent<DailyFacilityShopRuntime>();
        CountingFacilityShopRefreshedListener listener = new CountingFacilityShopRefreshedListener();

        runtime.OnTriggerEvent(new OperatingDayEndedEvent(2));
        bool valid = runtime.CurrentOfferDay == 3
            && runtime.CurrentDailyOffers.Count > 0
            && listener.Count == 1
            && listener.LastDay == 3;

        listener.Dispose();
        Object.DestroyImmediate(runtimeObject);
        return valid;
    }

    private static bool VerifySettlementReportIncludesFacilityShop()
    {
        GameObject settlementObject = new GameObject("Settlement_FacilityShop_Test");
        OperatingDaySettlementRuntime settlement = settlementObject.AddComponent<OperatingDaySettlementRuntime>();

        settlement.OnTriggerEvent(new OperatingDayEndedEvent(1));
        OperatingDayReport report = settlement.LatestReport;
        bool valid = report != null
            && report.refreshedFacilityShopOffers.Count > 0
            && report.refreshedFacilityShopOffers.Any((offer) => offer.type == FacilityShopOfferType.Building)
            && report.refreshedFacilityShopOffers.Any((offer) => offer.type == FacilityShopOfferType.Blueprint)
            && report.ToDetailText().Contains("시설 상점 갱신");

        Object.DestroyImmediate(settlementObject);
        return valid;
    }

    private static BuildingSO LoadBuilding(string assetName)
    {
        return AssetDatabase.LoadAssetAtPath<BuildingSO>($"Assets/Resources/SO/Building/P1/{assetName}.asset");
    }

    private static FacilityBlueprintSO LoadBlueprint(string assetName)
    {
        return AssetDatabase.LoadAssetAtPath<FacilityBlueprintSO>($"Assets/Resources/SO/Blueprint/P1/{assetName}.asset");
    }

    private static BuildingSO CreateSyntheticDefenseBuilding(int id, string objectName, int star)
    {
        BuildingSO building = ScriptableObject.CreateInstance<BuildingSO>();
        building.id = id;
        building.objectName = objectName;
        building.width = 1;
        building.height = 1;
        building.layer = GridLayer.Building;
        building.category = BuildingCategory.Special;
        building.type = typeof(DefenseFacility);
        building.defense = new DefenseFacilityData
        {
            enabled = true,
            concept = DefenseAttackConcept.Physical,
            triggerTimings = DefenseTriggerTiming.OnEnter,
            targetRule = DefenseTargetRule.EnteringIntruder,
            star = star
        };
        return building;
    }

    private static GameData CreateGameData(int holdingMoney)
    {
        GameData gameData = ScriptableObject.CreateInstance<GameData>();
        gameData.holdingMoney = new Data<int>();
        gameData.holdingMoney.Initialize(holdingMoney);
        return gameData;
    }

    private sealed class CountingFacilityShopRefreshedListener : UtilEventListener<FacilityShopRefreshedEvent>, IDisposable
    {
        public int Count { get; private set; }
        public int LastDay { get; private set; }

        public CountingFacilityShopRefreshedListener()
        {
            this.EventStartListening<FacilityShopRefreshedEvent>();
        }

        public void OnTriggerEvent(FacilityShopRefreshedEvent eventType)
        {
            Count++;
            LastDay = eventType.day;
        }

        public void Dispose()
        {
            this.EventStopListening<FacilityShopRefreshedEvent>();
        }
    }

    private sealed class CountingEventAlertRequestListener : UtilEventListener<EventAlertRequestedEvent>, IDisposable
    {
        private readonly List<EventAlertRequest> requests = new List<EventAlertRequest>();

        public IReadOnlyList<EventAlertRequest> Requests => requests;
        public EventAlertRequest LastRequest { get; private set; }

        public CountingEventAlertRequestListener()
        {
            this.EventStartListening<EventAlertRequestedEvent>();
        }

        public void OnTriggerEvent(EventAlertRequestedEvent eventType)
        {
            LastRequest = eventType.request;
            requests.Add(eventType.request);
        }

        public void Dispose()
        {
            this.EventStopListening<EventAlertRequestedEvent>();
        }
    }
}
