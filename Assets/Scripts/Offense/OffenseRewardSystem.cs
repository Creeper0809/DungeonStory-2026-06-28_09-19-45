using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public sealed class OffenseRewardGrantService : IOffenseRewardGrantService
{
    private readonly IOffenseRewardSelector selector;

    public OffenseRewardGrantService(IOffenseRewardSelector selector)
    {
        this.selector = selector
            ?? throw new ArgumentNullException(nameof(selector));
    }

    public IReadOnlyList<OffenseRewardGrantResult> GrantRewards(
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

    private OffenseRewardGrantResult GrantReward(
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

    private OffenseRewardGrantResult GrantStock(
        OffenseRewardPreview reward,
        OffenseRewardContext context)
    {
        int amount = Mathf.Max(0, reward.amount);
        if (amount <= 0)
        {
            return Fail(reward, "보상 수량이 없습니다");
        }

        StockCategory category = selector.ResolveStockCategory(reward);
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

    private OffenseRewardGrantResult GrantRareFacility(
        OffenseRewardPreview reward,
        OffenseRewardContext context)
    {
        int count = Mathf.Max(1, reward.amount);
        List<string> grantedNames = new List<string>();
        HashSet<int> grantedBuildingIds = new HashSet<int>();
        for (int i = 0; i < count; i++)
        {
            BuildingSO building = selector.SelectRareFacility(context, grantedBuildingIds);
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

    private OffenseRewardGrantResult GrantBlueprint(
        OffenseRewardPreview reward,
        OffenseRewardContext context)
    {
        int count = Mathf.Max(1, reward.amount);
        List<string> grantedNames = new List<string>();
        for (int i = 0; i < count; i++)
        {
            FacilityBlueprintSO blueprint = selector.SelectBlueprint(reward, context);
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

    private OffenseRewardGrantResult GrantFactionWeakening(
        OffenseRewardPreview reward,
        OffenseRewardContext context)
    {
        int amount = Mathf.Max(1, reward.amount);
        bool humanFaction = selector.IsHumanFactionWeakening(reward, context.target);
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

    private OffenseRewardGrantResult GrantPrisoner(
        OffenseRewardPreview reward,
        OffenseRewardContext context)
    {
        int amount = Mathf.Max(1, reward.amount);
        if (selector.ContainsAny(reward.label, "특수", "몬스터"))
        {
            context.rewardState?.RecordSpecialMonsters(amount);
            return Success(reward, amount, "특수 몬스터 후보 등록");
        }

        context.rewardState?.RecordPrisoners(amount);
        return Success(reward, amount, "포로 등록");
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
    private readonly OffenseRewardState state = new OffenseRewardState();
    private readonly OffenseRewardDebugContext debugContext = new OffenseRewardDebugContext();
    private IOffenseRewardContextBuilder contextBuilder;
    private IOffenseRewardGrantService grantService;

    public OffenseRewardState State => state;

    [Inject]
    public void Construct(
        IOffenseRewardContextBuilder contextBuilder,
        IOffenseRewardGrantService grantService)
    {
        this.contextBuilder = contextBuilder
            ?? throw new ArgumentNullException(nameof(contextBuilder));
        this.grantService = grantService
            ?? throw new ArgumentNullException(nameof(grantService));
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
        IReadOnlyList<OffenseRewardGrantResult> grantResults = ResolveGrantService().GrantRewards(
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
        debugContext.gameData = gameData;
        debugContext.warehouses = warehouses?.Where((warehouse) => warehouse != null).ToList();
        debugContext.shopUnlockState = shopUnlockState;
        debugContext.researchState = researchState;
    }

    public void ClearDebugContext()
    {
        debugContext.Clear();
    }

    public void ResetState()
    {
        state.Reset();
    }

    private OffenseRewardContext CreateContext(OffenseTargetDefinition target)
    {
        return ResolveContextBuilder().Create(target, state, debugContext);
    }

    private IOffenseRewardContextBuilder ResolveContextBuilder()
    {
        return contextBuilder
            ?? throw new InvalidOperationException($"{nameof(OffenseRewardRuntime)} requires {nameof(IOffenseRewardContextBuilder)} injection.");
    }

    private IOffenseRewardGrantService ResolveGrantService()
    {
        return grantService
            ?? throw new InvalidOperationException($"{nameof(OffenseRewardRuntime)} requires {nameof(IOffenseRewardGrantService)} injection.");
    }
}
