using System.Collections.Generic;
using System.Linq;

public static class MetaProgressionCatalog
{
    private static readonly Dictionary<MetaUpgradeId, MetaUpgradeDefinition> definitions = BuildDefinitions();

    public static IReadOnlyCollection<MetaUpgradeDefinition> All => definitions.Values;

    public static MetaUpgradeDefinition Get(MetaUpgradeId id)
    {
        return definitions.TryGetValue(id, out MetaUpgradeDefinition definition) ? definition : null;
    }

    private static Dictionary<MetaUpgradeId, MetaUpgradeDefinition> BuildDefinitions()
    {
        MetaUpgradeDefinition[] list =
        {
            new MetaUpgradeDefinition
            {
                id = MetaUpgradeId.StartingFacilityCandidatePlusOne,
                branch = MetaProgressionBranch.OperationKnowledge,
                title = "시작 시설 후보 +1",
                detail = "런 시작 시설 후보가 1개 늘어납니다.",
                cost = 80,
                maxLevel = 2
            },
            new MetaUpgradeDefinition
            {
                id = MetaUpgradeId.StartingOwnerTraitCandidatePlusOne,
                branch = MetaProgressionBranch.OwnerSurvival,
                title = "시작 사장 특성 후보 +1",
                detail = "사장 특성 선택 후보를 늘리는 메타 보상입니다.",
                cost = 90,
                maxLevel = 1
            },
            new MetaUpgradeDefinition
            {
                id = MetaUpgradeId.BasicPurchaseListExpansion,
                branch = MetaProgressionBranch.OperationKnowledge,
                title = "1일차 기본 구매 목록 확장",
                detail = "일부 1성 시설이 기본 구매 후보에 미리 들어옵니다.",
                cost = 100,
                maxLevel = 3
            },
            new MetaUpgradeDefinition
            {
                id = MetaUpgradeId.SpecialRecipeRecordSlot,
                branch = MetaProgressionBranch.DesignPreservation,
                title = "특수 조합식 기록 보존",
                detail = "런에서 해금한 특수 조합식 일부를 다음 런에 기억합니다.",
                cost = 120,
                maxLevel = 3
            },
            new MetaUpgradeDefinition
            {
                id = MetaUpgradeId.OwnerSurvivalBonus,
                branch = MetaProgressionBranch.OwnerSurvival,
                title = "사장 생존력 보너스 증가",
                detail = "사장 최대 체력이 조금 오릅니다.",
                cost = 110,
                maxLevel = 3
            },
            new MetaUpgradeDefinition
            {
                id = MetaUpgradeId.InvasionWarningAccuracy,
                branch = MetaProgressionBranch.OwnerSurvival,
                title = "침입 전 경고 정확도 증가",
                detail = "침입 경고를 조금 더 일찍 받을 수 있습니다.",
                cost = 90,
                maxLevel = 2
            }
        };

        return list.ToDictionary((definition) => definition.id, (definition) => definition);
    }
}
