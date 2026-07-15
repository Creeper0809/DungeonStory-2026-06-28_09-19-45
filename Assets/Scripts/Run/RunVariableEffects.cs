using System;
using System.Linq;
using UnityEngine;

public static class RunVariableEffects
{
    public static float GetGuestDemandMultiplier(RunVariableState state, string speciesTag)
    {
        if (state == null || string.IsNullOrWhiteSpace(speciesTag))
        {
            return 1f;
        }

        return state.ActiveOperationVariables
            .Where((active) => active?.Definition != null
                && !string.IsNullOrWhiteSpace(active.Definition.guestSpeciesTag)
                && string.Equals(active.Definition.guestSpeciesTag, speciesTag, StringComparison.OrdinalIgnoreCase))
            .Aggregate(1f, (value, active) => value * Mathf.Max(0.1f, active.Definition.guestDemandMultiplier));
    }

    public static float GetStockCostMultiplier(RunVariableState state, StockCategory category)
    {
        if (state == null)
        {
            return 1f;
        }

        return state.ActiveOperationVariables
            .Where((active) => active?.Definition != null
                && active.Definition.hasStockCostModifier
                && active.Definition.stockCategory == category)
            .Aggregate(1f, (value, active) => value * Mathf.Max(0.05f, active.Definition.stockCostMultiplier));
    }

    public static float GetFacilityShopCostMultiplier(RunVariableState state, BuildingSO building)
    {
        if (state == null)
        {
            return 1f;
        }

        float multiplier = state.ActiveOperationVariables
            .Where((active) => active?.Definition != null)
            .Aggregate(1f, (value, active) => value * Mathf.Max(0.05f, active.Definition.shopCostMultiplier));

        if (building != null && building.Defense != null && building.Defense.IsDefenseFacility)
        {
            multiplier *= state.ActiveOperationVariables
                .Where((active) => active?.Definition != null)
                .Aggregate(1f, (value, active) => value * Mathf.Max(0.05f, active.Definition.defenseShopCostMultiplier));
        }

        return multiplier;
    }

    public static float GetBlueprintCostMultiplier(RunVariableState state, FacilityBlueprintSO blueprint)
    {
        if (state == null)
        {
            return 1f;
        }

        return state.ActiveOperationVariables
            .Where((active) => active?.Definition != null)
            .Aggregate(1f, (value, active) => value * Mathf.Max(0.05f, active.Definition.blueprintCostMultiplier));
    }

    public static float GetThreatRiseMultiplier(RunVariableState state)
    {
        if (state == null)
        {
            return 1f;
        }

        float startMultiplier = state.StartVariables != null
            ? Mathf.Max(0.05f, state.StartVariables.threatRiseMultiplier)
            : 1f;
        float eventMultiplier = state.ActiveOperationVariables
            .Where((active) => active?.Definition != null)
            .Aggregate(1f, (value, active) => value * Mathf.Max(0.05f, active.Definition.threatRiseMultiplier));
        return startMultiplier * eventMultiplier;
    }

    public static float GetWarningThresholdMultiplier(RunVariableState state)
    {
        if (state == null)
        {
            return 1f;
        }

        return state.ActiveOperationVariables
            .Where((active) => active?.Definition != null)
            .Aggregate(1f, (value, active) => value * Mathf.Max(0.05f, active.Definition.warningThresholdMultiplier));
    }

    public static InvasionIntruderSettings ApplyInvasionSettings(RunVariableState state, InvasionIntruderSettings source)
    {
        InvasionIntruderSettings result = new InvasionIntruderSettings();
        if (source != null)
        {
            result.secondsToFullFocus = source.secondsToFullFocus;
            result.repathIntervalSeconds = source.repathIntervalSeconds;
            result.facilityDamageIntervalSeconds = source.facilityDamageIntervalSeconds;
            result.finalCombatDamage = source.finalCombatDamage;
            result.finalCombatWindupSeconds = source.finalCombatWindupSeconds;
        }

        RunVariableDefinition definition = state?.CurrentInvasionVariable;
        if (definition == null)
        {
            return result;
        }

        result.secondsToFullFocus = Mathf.Max(0.1f, result.secondsToFullFocus * Mathf.Max(0.05f, definition.secondsToFullFocusMultiplier));
        result.repathIntervalSeconds = Mathf.Max(0.1f, result.repathIntervalSeconds * Mathf.Max(0.05f, definition.repathIntervalMultiplier));
        result.facilityDamageIntervalSeconds = Mathf.Max(0f, result.facilityDamageIntervalSeconds * Mathf.Max(0.05f, definition.facilityDamageIntervalMultiplier));
        result.finalCombatDamage = Mathf.Max(0f, result.finalCombatDamage * Mathf.Max(0.05f, definition.finalCombatDamageMultiplier));
        return result;
    }
}
