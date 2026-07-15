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
