using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class OffenseRewardGrantResult
{
    public OffenseRewardCategory category;
    public string label;
    public int requestedAmount;
    public int grantedAmount;
    public bool success;
    public string detail;

    public string ToSummaryText()
    {
        string rewardName = string.IsNullOrWhiteSpace(label) ? category.ToString() : label;
        if (success)
        {
            string amountText = grantedAmount > 0 ? $" x{grantedAmount}" : string.Empty;
            string detailText = string.IsNullOrWhiteSpace(detail) ? string.Empty : $" - {detail}";
            return $"{rewardName}{amountText}{detailText}";
        }

        string reason = string.IsNullOrWhiteSpace(detail) ? "지급 실패" : detail;
        return $"{rewardName} 지급 실패 - {reason}";
    }
}

public sealed class OffenseRewardState
{
    private readonly Dictionary<StockCategory, int> stockGrantedByCategory = new Dictionary<StockCategory, int>();
    private readonly HashSet<int> rareFacilityBuildingIds = new HashSet<int>();
    private readonly HashSet<int> acquiredBlueprintIds = new HashSet<int>();

    public int MoneyEarned { get; private set; }
    public int HumanFactionWeakening { get; private set; }
    public int RivalFactionWeakening { get; private set; }
    public int RecruitCandidateCount { get; private set; }
    public int PrisonerCount { get; private set; }
    public int SpecialMonsterCount { get; private set; }
    public IReadOnlyDictionary<StockCategory, int> StockGrantedByCategory => stockGrantedByCategory;
    public IReadOnlyCollection<int> RareFacilityBuildingIds => rareFacilityBuildingIds;
    public IReadOnlyCollection<int> AcquiredBlueprintIds => acquiredBlueprintIds;

    public void Reset()
    {
        MoneyEarned = 0;
        HumanFactionWeakening = 0;
        RivalFactionWeakening = 0;
        RecruitCandidateCount = 0;
        PrisonerCount = 0;
        SpecialMonsterCount = 0;
        stockGrantedByCategory.Clear();
        rareFacilityBuildingIds.Clear();
        acquiredBlueprintIds.Clear();
    }

    public void RecordMoney(int amount)
    {
        MoneyEarned += Mathf.Max(0, amount);
    }

    public void RecordStock(StockCategory category, int amount)
    {
        int safeAmount = Mathf.Max(0, amount);
        if (safeAmount <= 0) return;

        stockGrantedByCategory[category] = stockGrantedByCategory.TryGetValue(category, out int current)
            ? current + safeAmount
            : safeAmount;
    }

    public bool RecordRareFacility(BuildingSO building)
    {
        return building != null && rareFacilityBuildingIds.Add(building.id);
    }

    public bool RecordBlueprint(FacilityBlueprintSO blueprint)
    {
        return blueprint != null && acquiredBlueprintIds.Add(blueprint.id);
    }

    public void RecordFactionWeakening(bool humanFaction, int amount)
    {
        int safeAmount = Mathf.Max(0, amount);
        if (humanFaction)
        {
            HumanFactionWeakening += safeAmount;
        }
        else
        {
            RivalFactionWeakening += safeAmount;
        }
    }

    public void RecordRecruitCandidates(int amount)
    {
        RecruitCandidateCount += Mathf.Max(0, amount);
    }

    public void RecordPrisoners(int amount)
    {
        PrisonerCount += Mathf.Max(0, amount);
    }

    public void RecordSpecialMonsters(int amount)
    {
        SpecialMonsterCount += Mathf.Max(0, amount);
    }
}

public sealed class OffenseRewardContext
{
    public GameData gameData;
    public IEnumerable<IWarehouseFacility> warehouses = Enumerable.Empty<IWarehouseFacility>();
    public FacilityShopUnlockState shopUnlockState;
    public BlueprintResearchState researchState;
    public BlueprintResearchRuntime researchRuntime;
    public OffenseRewardState rewardState;
    public OffenseTargetDefinition target;
}

public struct OffenseRewardGrantedEvent
{
    public OffenseExpeditionResult expeditionResult;
    public IReadOnlyList<OffenseRewardGrantResult> grantResults;

    public OffenseRewardGrantedEvent(
        OffenseExpeditionResult expeditionResult,
        IReadOnlyList<OffenseRewardGrantResult> grantResults)
    {
        this.expeditionResult = expeditionResult;
        this.grantResults = grantResults ?? Array.Empty<OffenseRewardGrantResult>();
    }

    private static OffenseRewardGrantedEvent e;

    public static void Trigger(
        OffenseExpeditionResult expeditionResult,
        IReadOnlyList<OffenseRewardGrantResult> grantResults)
    {
        e.expeditionResult = expeditionResult;
        e.grantResults = grantResults ?? Array.Empty<OffenseRewardGrantResult>();
        EventObserver.TriggerEvent(e);
    }
}

public static class OffenseRewardService
{
    public static IReadOnlyList<OffenseRewardGrantResult> GrantRewards(
        IEnumerable<OffenseRewardPreview> rewards,
        OffenseRewardContext context)
    {
        List<OffenseRewardGrantResult> results = new List<OffenseRewardGrantResult>();
        if (rewards == null) return results;

        OffenseRewardContext safeContext = context ?? new OffenseRewardContext();
        foreach (OffenseRewardPreview reward in rewards.Where((reward) => reward != null))
        {
            results.Add(GrantReward(reward, safeContext));
        }

        return results;
    }

    private static OffenseRewardGrantResult GrantReward(
        OffenseRewardPreview reward,
        OffenseRewardContext context)
    {
        return reward.category switch
        {
            OffenseRewardCategory.Money => GrantMoney(reward, context),
            OffenseRewardCategory.Stock => GrantStock(reward, context),
            OffenseRewardCategory.RareFacility => GrantRareFacility(reward, context),
            OffenseRewardCategory.Blueprint => GrantBlueprint(reward, context),
            OffenseRewardCategory.FactionWeakening => GrantFactionWeakening(reward, context),
            OffenseRewardCategory.RecruitCandidate => GrantRecruitCandidate(reward, context),
            OffenseRewardCategory.Prisoner => GrantPrisoner(reward, context),
            _ => Fail(reward, "알 수 없는 보상 종류")
        };
    }

    private static OffenseRewardGrantResult GrantMoney(
        OffenseRewardPreview reward,
        OffenseRewardContext context)
    {
        int amount = Mathf.Max(0, reward.amount);
        if (amount <= 0)
        {
            return Fail(reward, "보상 금액이 없습니다");
        }

        if (context.gameData == null || context.gameData.holdingMoney == null)
        {
            return Fail(reward, "자금 데이터가 없습니다");
        }

        context.gameData.holdingMoney.Value += amount;
        context.rewardState?.RecordMoney(amount);
        return Success(reward, amount, "자금 입금");
    }

    private static OffenseRewardGrantResult GrantStock(
        OffenseRewardPreview reward,
        OffenseRewardContext context)
    {
        int amount = Mathf.Max(0, reward.amount);
        if (amount <= 0)
        {
            return Fail(reward, "보상 수량이 없습니다");
        }

        StockCategory category = ResolveStockCategory(reward);
        bool success = StockSupplyService.GrantReward(
            context.warehouses,
            category,
            amount,
            string.IsNullOrWhiteSpace(reward.label) ? "오펜스 보상" : reward.label,
            out StockSupplyResult result);

        if (!success || !result.success)
        {
            return Fail(reward, string.IsNullOrWhiteSpace(result.reason) ? "재고 입고 실패" : result.reason);
        }

        context.rewardState?.RecordStock(category, result.deliveredAmount);
        return Success(reward, result.deliveredAmount, $"{category} 입고");
    }

    private static OffenseRewardGrantResult GrantRareFacility(
        OffenseRewardPreview reward,
        OffenseRewardContext context)
    {
        int count = Mathf.Max(1, reward.amount);
        List<string> grantedNames = new List<string>();
        HashSet<int> grantedBuildingIds = new HashSet<int>();
        for (int i = 0; i < count; i++)
        {
            BuildingSO building = SelectRareFacility(context, grantedBuildingIds);
            if (building == null)
            {
                break;
            }

            building.unlocked = true;
            if (FacilityShopService.CanEnterBasicPurchase(building))
            {
                context.shopUnlockState?.UnlockBasicPurchase(building);
            }

            context.rewardState?.RecordRareFacility(building);
            grantedBuildingIds.Add(building.id);
            grantedNames.Add(FacilityShopService.GetBuildingName(building));
        }

        return grantedNames.Count > 0
            ? Success(reward, grantedNames.Count, string.Join(", ", grantedNames))
            : Fail(reward, "해금 가능한 희귀 시설이 없습니다");
    }

    private static OffenseRewardGrantResult GrantBlueprint(
        OffenseRewardPreview reward,
        OffenseRewardContext context)
    {
        int count = Mathf.Max(1, reward.amount);
        List<string> grantedNames = new List<string>();
        for (int i = 0; i < count; i++)
        {
            FacilityBlueprintSO blueprint = SelectBlueprint(reward, context);
            if (blueprint == null)
            {
                break;
            }

            context.shopUnlockState?.MarkBlueprintAcquired(blueprint);
            bool queued = context.researchRuntime != null
                ? context.researchRuntime.EnqueueBlueprint(blueprint)
                : context.researchState != null && context.researchState.EnqueueBlueprint(blueprint);
            context.rewardState?.RecordBlueprint(blueprint);
            grantedNames.Add(queued ? $"{blueprint.DisplayName} 연구 대기" : $"{blueprint.DisplayName} 획득");
        }

        return grantedNames.Count > 0
            ? Success(reward, grantedNames.Count, string.Join(", ", grantedNames))
            : Fail(reward, "획득 가능한 설계도가 없습니다");
    }

    private static OffenseRewardGrantResult GrantFactionWeakening(
        OffenseRewardPreview reward,
        OffenseRewardContext context)
    {
        int amount = Mathf.Max(1, reward.amount);
        bool humanFaction = IsHumanFactionWeakening(reward, context.target);
        context.rewardState?.RecordFactionWeakening(humanFaction, amount);
        return Success(reward, amount, humanFaction ? "인간 세력 약화" : "경쟁 세력 약화");
    }

    private static OffenseRewardGrantResult GrantRecruitCandidate(
        OffenseRewardPreview reward,
        OffenseRewardContext context)
    {
        int amount = Mathf.Max(1, reward.amount);
        context.rewardState?.RecordRecruitCandidates(amount);
        return Success(reward, amount, "영입 후보 등록");
    }

    private static OffenseRewardGrantResult GrantPrisoner(
        OffenseRewardPreview reward,
        OffenseRewardContext context)
    {
        int amount = Mathf.Max(1, reward.amount);
        if (ContainsAny(reward.label, "특수", "몬스터"))
        {
            context.rewardState?.RecordSpecialMonsters(amount);
            return Success(reward, amount, "특수 몬스터 후보 등록");
        }

        context.rewardState?.RecordPrisoners(amount);
        return Success(reward, amount, "포로 등록");
    }

    private static StockCategory ResolveStockCategory(OffenseRewardPreview reward)
    {
        string label = reward.label ?? string.Empty;
        if (ContainsAny(label, "식재료", "음식", "Food"))
        {
            return StockCategory.Food;
        }

        if (ContainsAny(label, "무기", "Weapon"))
        {
            return StockCategory.Weapon;
        }

        if (ContainsAny(label, "마력", "Mana"))
        {
            return StockCategory.Mana;
        }

        return StockCategory.General;
    }

    private static BuildingSO SelectRareFacility(
        OffenseRewardContext context,
        IReadOnlyCollection<int> additionallyExcludedBuildingIds)
    {
        HashSet<int> alreadyGranted = context.rewardState != null
            ? context.rewardState.RareFacilityBuildingIds.ToHashSet()
            : new HashSet<int>();
        if (additionallyExcludedBuildingIds != null)
        {
            foreach (int buildingId in additionallyExcludedBuildingIds)
            {
                alreadyGranted.Add(buildingId);
            }
        }

        return Resources.LoadAll<BuildingSO>("SO/Building")
            .Where((building) => building != null
                && !building.IsGridMovement
                && !building.IsWall
                && FacilityShopService.GetBuildingStar(building) >= 2
                && !alreadyGranted.Contains(building.id))
            .OrderBy((building) => FacilityShopService.GetBuildingStar(building))
            .ThenBy((building) => building.id)
            .FirstOrDefault();
    }

    private static FacilityBlueprintSO SelectBlueprint(
        OffenseRewardPreview reward,
        OffenseRewardContext context)
    {
        IEnumerable<FacilityBlueprintSO> blueprints = Resources.LoadAll<FacilityBlueprintSO>("SO/Blueprint")
            .Where((blueprint) => blueprint != null);

        if (ContainsAny(reward.label, "방어"))
        {
            blueprints = blueprints.Where((blueprint) => ContainsAny(blueprint.DisplayName, "방어", "함정")
                || ContainsAny(blueprint.description, "방어", "함정"));
        }
        else if (ContainsAny(reward.label, "특수", "희귀"))
        {
            blueprints = blueprints.Where((blueprint) => blueprint.rarity != FacilityShopRarity.Common);
        }

        HashSet<int> acquired = context.rewardState != null
            ? context.rewardState.AcquiredBlueprintIds.ToHashSet()
            : new HashSet<int>();

        return blueprints
            .Where((blueprint) => !acquired.Contains(blueprint.id))
            .Where((blueprint) => context.shopUnlockState == null || !context.shopUnlockState.IsBlueprintAcquired(blueprint))
            .Where((blueprint) => context.researchState == null || !context.researchState.IsCompleted(blueprint))
            .OrderByDescending((blueprint) => blueprint.rarity)
            .ThenBy((blueprint) => blueprint.id)
            .FirstOrDefault();
    }

    private static bool IsHumanFactionWeakening(OffenseRewardPreview reward, OffenseTargetDefinition target)
    {
        if (ContainsAny(reward.label, "인간", "Human"))
        {
            return true;
        }

        if (ContainsAny(reward.label, "경쟁", "Rival"))
        {
            return false;
        }

        return target == null || target.kind != OffenseTargetKind.RivalDungeon;
    }

    private static bool ContainsAny(string source, params string[] values)
    {
        if (string.IsNullOrWhiteSpace(source) || values == null)
        {
            return false;
        }

        return values.Any((value) => !string.IsNullOrWhiteSpace(value)
            && source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private static OffenseRewardGrantResult Success(
        OffenseRewardPreview reward,
        int grantedAmount,
        string detail)
    {
        return new OffenseRewardGrantResult
        {
            category = reward.category,
            label = reward.label,
            requestedAmount = Mathf.Max(0, reward.amount),
            grantedAmount = Mathf.Max(0, grantedAmount),
            success = true,
            detail = detail ?? string.Empty
        };
    }

    private static OffenseRewardGrantResult Fail(OffenseRewardPreview reward, string detail)
    {
        return new OffenseRewardGrantResult
        {
            category = reward.category,
            label = reward.label,
            requestedAmount = Mathf.Max(0, reward.amount),
            grantedAmount = 0,
            success = false,
            detail = detail ?? string.Empty
        };
    }
}

public class OffenseRewardRuntime : MonoBehaviour
{
    private static OffenseRewardRuntime instance;
    private readonly OffenseRewardState state = new OffenseRewardState();
    private List<IWarehouseFacility> debugWarehouses;
    private GameData debugGameData;
    private FacilityShopUnlockState debugShopUnlockState;
    private BlueprintResearchState debugResearchState;

    public static OffenseRewardRuntime Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<OffenseRewardRuntime>();
            }

            return instance;
        }
    }

    public OffenseRewardState State => state;

    private void Awake()
    {
        instance = this;
    }

    public IReadOnlyList<OffenseRewardGrantResult> ApplyExpeditionRewards(
        OffenseExpeditionRun expedition,
        OffenseExpeditionResult result)
    {
        if (expedition == null || expedition.Target == null || result == null || !result.success)
        {
            return Array.Empty<OffenseRewardGrantResult>();
        }

        OffenseRewardContext context = CreateContext(expedition.Target);
        IReadOnlyList<OffenseRewardGrantResult> grantResults = OffenseRewardService.GrantRewards(
            expedition.Target.rewards,
            context);
        OffenseRewardGrantedEvent.Trigger(result, grantResults);
        return grantResults;
    }

    public void SetDebugContext(
        GameData gameData,
        IEnumerable<IWarehouseFacility> warehouses,
        FacilityShopUnlockState shopUnlockState,
        BlueprintResearchState researchState)
    {
        debugGameData = gameData;
        debugWarehouses = warehouses?.Where((warehouse) => warehouse != null).ToList();
        debugShopUnlockState = shopUnlockState;
        debugResearchState = researchState;
    }

    public void ClearDebugContext()
    {
        debugGameData = null;
        debugWarehouses = null;
        debugShopUnlockState = null;
        debugResearchState = null;
    }

    public void ResetState()
    {
        state.Reset();
    }

    private OffenseRewardContext CreateContext(OffenseTargetDefinition target)
    {
        BlueprintResearchRuntime researchRuntime = BlueprintResearchRuntime.Instance;
        DailyFacilityShopRuntime shopRuntime = FindFirstObjectByType<DailyFacilityShopRuntime>();
        GameManager gameManager = GameManager.TryGetInstance();
        GameData gameData = debugGameData != null
            ? debugGameData
            : gameManager != null
                ? gameManager.gameData
                : null;
        FacilityShopUnlockState shopUnlockState = debugShopUnlockState != null
            ? debugShopUnlockState
            : shopRuntime != null
                ? shopRuntime.UnlockState
                : researchRuntime != null
                    ? researchRuntime.ShopUnlockState
                    : null;

        return new OffenseRewardContext
        {
            gameData = gameData,
            warehouses = debugWarehouses ?? ResolveWarehouses(),
            shopUnlockState = shopUnlockState,
            researchState = debugResearchState ?? researchRuntime?.State,
            researchRuntime = debugResearchState == null ? researchRuntime : null,
            rewardState = state,
            target = target
        };
    }

    private static IEnumerable<IWarehouseFacility> ResolveWarehouses()
    {
        return FindObjectsByType<BuildableObject>(FindObjectsSortMode.None)
            .OfType<IWarehouseFacility>()
            .Where((warehouse) => warehouse != null && warehouse.HasWarehouseInventory);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
