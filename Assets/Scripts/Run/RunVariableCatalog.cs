using System.Collections.Generic;
using System.Linq;

public static class RunVariableCatalog
{
    private static readonly Dictionary<RunVariableId, RunVariableDefinition> definitions = BuildDefinitions();

    public static IReadOnlyCollection<RunVariableDefinition> All => definitions.Values;

    public static RunVariableDefinition Get(RunVariableId id)
    {
        return definitions.TryGetValue(id, out RunVariableDefinition definition) ? definition : null;
    }

    public static IReadOnlyList<RunVariableDefinition> GetByCategory(RunVariableCategory category)
    {
        return definitions.Values
            .Where((definition) => definition.category == category)
            .ToList();
    }

    private static Dictionary<RunVariableId, RunVariableDefinition> BuildDefinitions()
    {
        RunVariableDefinition[] list =
        {
            new RunVariableDefinition
            {
                id = RunVariableId.SlimeCrowdVisit,
                category = RunVariableCategory.Operation,
                title = "슬라임 단체 방문",
                detail = "슬라임 손님 수요가 일시적으로 증가합니다.",
                importance = EventAlertImportance.Low,
                guestSpeciesTag = "Slime",
                guestDemandMultiplier = 1.7f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.OrcFeast,
                category = RunVariableCategory.Operation,
                title = "오크 회식",
                detail = "오크 손님 수요가 증가하고 고소비 시설 우선순위가 올라갑니다.",
                importance = EventAlertImportance.Low,
                guestSpeciesTag = "Orc",
                guestDemandMultiplier = 1.6f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.VampireNightVisit,
                category = RunVariableCategory.Operation,
                title = "뱀파이어 야간 방문",
                detail = "뱀파이어 손님 수요가 증가합니다.",
                importance = EventAlertImportance.Low,
                guestSpeciesTag = "Vampire",
                guestDemandMultiplier = 1.55f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.FoodDeliveryDelay,
                category = RunVariableCategory.Operation,
                title = "식자재 배송 지연",
                detail = "식자재 상품 가격이 올라갑니다.",
                importance = EventAlertImportance.Medium,
                stockCategory = StockCategory.Food,
                hasStockCostModifier = true,
                stockCostMultiplier = 1.5f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.GeneralGoodsDiscount,
                category = RunVariableCategory.Operation,
                title = "잡화 할인",
                detail = "잡화 상품 가격이 내려갑니다.",
                importance = EventAlertImportance.Low,
                stockCategory = StockCategory.General,
                hasStockCostModifier = true,
                stockCostMultiplier = 0.75f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.ManaStockSurplus,
                category = RunVariableCategory.Operation,
                title = "마력 재고 과잉",
                detail = "마력 상품 가격이 크게 내려갑니다.",
                importance = EventAlertImportance.Low,
                stockCategory = StockCategory.Mana,
                hasStockCostModifier = true,
                stockCostMultiplier = 0.65f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.VisitingMerchant,
                category = RunVariableCategory.Operation,
                title = "방문 상인",
                detail = "시설과 설계도 가격이 조금 내려갑니다.",
                importance = EventAlertImportance.Medium,
                shopCostMultiplier = 0.85f,
                blueprintCostMultiplier = 0.9f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.DefenseFacilityDiscount,
                category = RunVariableCategory.Operation,
                title = "방어 시설 할인",
                detail = "방어 시설 가격이 내려갑니다.",
                importance = EventAlertImportance.Low,
                defenseShopCostMultiplier = 0.7f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.BlueprintRumor,
                category = RunVariableCategory.Operation,
                title = "설계도 소문",
                detail = "설계도 가격이 내려가고 연구 목표를 뽑기 쉬워집니다.",
                importance = EventAlertImportance.Medium,
                blueprintCostMultiplier = 0.8f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.ScoutTraces,
                category = RunVariableCategory.Invasion,
                title = "정찰 흔적",
                detail = "침입자가 사장 위치를 더 빨리 좁혀 옵니다.",
                importance = EventAlertImportance.Medium,
                secondsToFullFocusMultiplier = 0.8f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.Ambush,
                category = RunVariableCategory.Invasion,
                title = "급습",
                detail = "침입자의 경로 재탐색이 빨라집니다.",
                importance = EventAlertImportance.High,
                repathIntervalMultiplier = 0.75f,
                finalCombatDamageMultiplier = 0.9f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.ArmedIntruder,
                category = RunVariableCategory.Invasion,
                title = "무장한 침입자",
                detail = "최종 교전 피해가 증가합니다.",
                importance = EventAlertImportance.High,
                finalCombatDamageMultiplier = 1.35f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.LootPriority,
                category = RunVariableCategory.Invasion,
                title = "약탈 우선",
                detail = "침입자가 주요 시설을 더 자주 훼손합니다.",
                importance = EventAlertImportance.Medium,
                facilityDamageIntervalMultiplier = 0.55f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.TiredIntruder,
                category = RunVariableCategory.Invasion,
                title = "지친 침입자",
                detail = "최종 교전 피해가 낮아지지만 경로 탐색은 조금 느려집니다.",
                importance = EventAlertImportance.Low,
                secondsToFullFocusMultiplier = 1.15f,
                finalCombatDamageMultiplier = 0.7f
            }
        };

        return list.ToDictionary((definition) => definition.id, (definition) => definition);
    }
}
