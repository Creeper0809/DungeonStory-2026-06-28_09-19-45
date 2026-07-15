using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MetaProgressionBranch
{
    OperationKnowledge,
    DesignPreservation,
    OwnerSurvival
}

public enum MetaUpgradeId
{
    None,
    StartingFacilityCandidatePlusOne,
    StartingOwnerTraitCandidatePlusOne,
    BasicPurchaseListExpansion,
    SpecialRecipeRecordSlot,
    OwnerSurvivalBonus,
    InvasionWarningAccuracy
}

public sealed class MetaUpgradeDefinition
{
    public MetaUpgradeId id;
    public MetaProgressionBranch branch;
    public string title;
    public string detail;
    public int cost;
    public int maxLevel = 1;
}

public sealed class MetaProgressionState
{
    private readonly Dictionary<MetaUpgradeId, int> upgradeLevels = new Dictionary<MetaUpgradeId, int>();
    private readonly HashSet<string> preservedRecipeIds = new HashSet<string>();

    public int LifetimeEarnedCurrency { get; private set; }
    public int SpentCurrency { get; private set; }
    public int AvailableCurrency => Mathf.Max(0, LifetimeEarnedCurrency - SpentCurrency);
    public IReadOnlyDictionary<MetaUpgradeId, int> UpgradeLevels => upgradeLevels;
    public IReadOnlyCollection<string> PreservedRecipeIds => preservedRecipeIds;

    public void AddCurrency(int amount)
    {
        LifetimeEarnedCurrency += Mathf.Max(0, amount);
    }

    public int GetUpgradeLevel(MetaUpgradeId id)
    {
        return upgradeLevels.TryGetValue(id, out int level) ? level : 0;
    }

    public bool TryPurchaseUpgrade(MetaUpgradeId id, out string message)
    {
        MetaUpgradeDefinition definition = MetaProgressionCatalog.Get(id);
        if (definition == null)
        {
            message = "강화 정보가 없습니다";
            return false;
        }

        int currentLevel = GetUpgradeLevel(id);
        if (currentLevel >= Mathf.Max(1, definition.maxLevel))
        {
            message = "이미 최대 레벨입니다";
            return false;
        }

        int cost = Mathf.Max(0, definition.cost);
        if (AvailableCurrency < cost)
        {
            message = "계승 재화가 부족합니다";
            return false;
        }

        SpentCurrency += cost;
        upgradeLevels[id] = currentLevel + 1;
        message = $"{definition.title} Lv.{currentLevel + 1}";
        return true;
    }

    public void SetUpgradeLevelForDebug(MetaUpgradeId id, int level)
    {
        MetaUpgradeDefinition definition = MetaProgressionCatalog.Get(id);
        if (definition == null) return;

        upgradeLevels[id] = Mathf.Clamp(level, 0, Mathf.Max(1, definition.maxLevel));
    }

    public void PreserveRecipes(IEnumerable<string> recipeIds, int maxCount)
    {
        if (recipeIds == null || maxCount <= 0)
        {
            return;
        }

        foreach (string recipeId in recipeIds.Where((id) => !string.IsNullOrWhiteSpace(id)).Take(maxCount))
        {
            preservedRecipeIds.Add(recipeId);
        }
    }
}

public sealed class RunResultSnapshot
{
    public string ownerName;
    public string endReason;
    public float survivalSeconds;
    public int survivedOperatingDays;
    public int settlementCount;
    public int defendedInvasionCount;
    public InvasionThreatStage maxThreatStage;
    public float finalInvasionThreat;
    public int firstDiscoveredFacilityCount;
    public int firstUnlockedRecipeCount;
    public int offenseSuccessCount;
    public float difficultyMultiplier = 1f;
    public int legacyCurrency;

    public string ToDetailText()
    {
        return string.Join("\n", new[]
        {
            "런 결과 정산",
            string.Empty,
            $"사장: {TextOrDefault(ownerName, "미상")}",
            $"종료 원인: {TextOrDefault(endReason, "사장 쓰러짐")}",
            $"생존 시간: {FormatTime(survivalSeconds)}",
            $"생존 운영일: {survivedOperatingDays}",
            $"운영일 정산 횟수: {settlementCount}",
            $"막아낸 침입: {defendedInvasionCount}",
            $"최대 위협 단계: {FormatThreatStage(maxThreatStage)}",
            $"최종 침입 강도: {finalInvasionThreat:0.#}",
            $"최초 발견 시설: {firstDiscoveredFacilityCount}",
            $"최초 해금 조합식: {firstUnlockedRecipeCount}",
            $"원정 성과: {offenseSuccessCount}",
            $"난이도 배율: x{difficultyMultiplier:0.##}",
            string.Empty,
            $"획득 계승 재화: {legacyCurrency}",
            string.Empty,
            "계승되지 않음: 현재 런의 돈, 재고, 배치 시설"
        });
    }

    private static string FormatTime(float seconds)
    {
        int safeSeconds = Mathf.Max(0, Mathf.RoundToInt(seconds));
        int minutes = safeSeconds / 60;
        int remainSeconds = safeSeconds % 60;
        return $"{minutes:0}:{remainSeconds:00}";
    }

    private static string TextOrDefault(string value, string defaultValue)
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    private static string FormatThreatStage(InvasionThreatStage stage)
    {
        return stage switch
        {
            InvasionThreatStage.Peaceful => "평온",
            InvasionThreatStage.Warning => "경고",
            InvasionThreatStage.Candidate => "침입 후보",
            InvasionThreatStage.Safety => "안전 시간",
            _ => stage.ToString()
        };
    }
}
