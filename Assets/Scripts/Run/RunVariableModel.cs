using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum RunVariableCategory
{
    Start,
    Operation,
    Invasion
}

public enum RunVariableId
{
    None,
    SlimeCrowdVisit,
    OrcFeast,
    VampireNightVisit,
    FoodDeliveryDelay,
    GeneralGoodsDiscount,
    ManaStockSurplus,
    VisitingMerchant,
    DefenseFacilityDiscount,
    BlueprintRumor,
    ScoutTraces,
    Ambush,
    ArmedIntruder,
    LootPriority,
    TiredIntruder
}

public sealed class RunStartVariableSnapshot
{
    public int seed;
    public string ownerSpeciesTag;
    public InvasionThreatDifficulty difficulty;
    public int[] startingFacilityCandidateIds = Array.Empty<int>();
    public string[] startingGuestSpeciesCandidates = Array.Empty<string>();
    public int[] startingBlueprintCandidateIds = Array.Empty<int>();
    public int initialShopSeed;
    public string initialDungeonLayoutId;
    public float threatRiseMultiplier = 1f;

    public string ToSummaryText()
    {
        return string.Join("\n", new[]
        {
            $"사장 종족: {TextOrDefault(ownerSpeciesTag, "미정")}",
            $"시작 시설 후보: {FormatIds(startingFacilityCandidateIds)}",
            $"시작 손님층 후보: {FormatStrings(startingGuestSpeciesCandidates)}",
            $"시작 설계도 후보: {FormatIds(startingBlueprintCandidateIds)}",
            $"초기 상점 시드: {initialShopSeed}",
            $"초기 구조: {TextOrDefault(initialDungeonLayoutId, "기본")}",
            $"난이도: {difficulty} / 위협 계수 {threatRiseMultiplier:0.##}"
        });
    }

    private static string FormatIds(IEnumerable<int> values)
    {
        string text = values != null ? string.Join(", ", values) : string.Empty;
        return string.IsNullOrWhiteSpace(text) ? "없음" : text;
    }

    private static string FormatStrings(IEnumerable<string> values)
    {
        string text = values != null
            ? string.Join(", ", values.Where((value) => !string.IsNullOrWhiteSpace(value)))
            : string.Empty;
        return string.IsNullOrWhiteSpace(text) ? "없음" : text;
    }

    private static string TextOrDefault(string value, string defaultValue)
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }
}

public sealed class RunVariableDefinition
{
    public RunVariableId id;
    public RunVariableCategory category;
    public string title;
    public string detail;
    public EventAlertImportance importance;
    public int activeDays = 1;
    public string guestSpeciesTag;
    public float guestDemandMultiplier = 1f;
    public StockCategory stockCategory;
    public bool hasStockCostModifier;
    public float stockCostMultiplier = 1f;
    public float shopCostMultiplier = 1f;
    public float defenseShopCostMultiplier = 1f;
    public float blueprintCostMultiplier = 1f;
    public float threatRiseMultiplier = 1f;
    public float warningThresholdMultiplier = 1f;
    public float secondsToFullFocusMultiplier = 1f;
    public float repathIntervalMultiplier = 1f;
    public float facilityDamageIntervalMultiplier = 1f;
    public float finalCombatDamageMultiplier = 1f;

    public string ToDetailText()
    {
        return string.IsNullOrWhiteSpace(detail) ? title : detail;
    }
}

public sealed class ActiveRunVariable
{
    public ActiveRunVariable(RunVariableDefinition definition, int startDay)
    {
        Definition = definition;
        StartDay = Mathf.Max(1, startDay);
        RemainingDays = Mathf.Max(1, definition != null ? definition.activeDays : 1);
    }

    public RunVariableDefinition Definition { get; }
    public int StartDay { get; }
    public int RemainingDays { get; private set; }
    public bool IsExpired => RemainingDays <= 0;

    public void AdvanceDay()
    {
        RemainingDays = Mathf.Max(0, RemainingDays - 1);
    }
}

public sealed class RunVariableState
{
    private readonly List<ActiveRunVariable> activeOperationVariables = new List<ActiveRunVariable>();

    public RunStartVariableSnapshot StartVariables { get; private set; }
    public IReadOnlyList<ActiveRunVariable> ActiveOperationVariables => activeOperationVariables;
    public RunVariableDefinition CurrentInvasionVariable { get; private set; }
    public bool HasStarted => StartVariables != null;

    public void SetStartVariables(RunStartVariableSnapshot snapshot)
    {
        StartVariables = snapshot;
    }

    public ActiveRunVariable ActivateOperationVariable(RunVariableDefinition definition, int day)
    {
        if (definition == null || definition.category != RunVariableCategory.Operation)
        {
            return null;
        }

        activeOperationVariables.RemoveAll((active) => active == null || active.Definition == null || active.Definition.id == definition.id);
        ActiveRunVariable instance = new ActiveRunVariable(definition, day);
        activeOperationVariables.Add(instance);
        return instance;
    }

    public IReadOnlyList<ActiveRunVariable> AdvanceOperationVariables()
    {
        List<ActiveRunVariable> expired = new List<ActiveRunVariable>();
        foreach (ActiveRunVariable active in activeOperationVariables)
        {
            active.AdvanceDay();
            if (active.IsExpired)
            {
                expired.Add(active);
            }
        }

        activeOperationVariables.RemoveAll((active) => active == null || active.IsExpired);
        return expired;
    }

    public void SetInvasionVariable(RunVariableDefinition definition)
    {
        CurrentInvasionVariable = definition != null && definition.category == RunVariableCategory.Invasion
            ? definition
            : null;
    }

    public void ClearInvasionVariable()
    {
        CurrentInvasionVariable = null;
    }
}
