using System;
using UnityEngine;

[Flags]
public enum FacilityFunction
{
    None = 0,
    Support = 1 << 0,
    MealProduction = 1 << 1,
    MeatProduction = 1 << 2,
    MealService = 1 << 3,
    RetailGeneral = 1 << 4,
    RetailWeapon = 1 << 5,
    WeaponCrafting = 1 << 6,
    Rest = 1 << 7,
    Administration = 1 << 8,
    Research = 1 << 9,
    Alchemy = 1 << 10,
    ManaStorage = 1 << 11,
    ManaRitual = 1 << 12,
    MeleeTraining = 1 << 13,
    RangedTraining = 1 << 14,
    StrengthTraining = 1 << 15,
    GuardPost = 1 << 16,
    Alarm = 1 << 17,
    Logistics = 1 << 18,
    Toilet = 1 << 19,
    Hygiene = 1 << 20,
    Cleaning = 1 << 21,
    Lighting = 1 << 22,
    Decoration = 1 << 23,
    Seating = 1 << 24,
    Table = 1 << 25,
    Storage = 1 << 26,
    Signage = 1 << 27
}

[Serializable]
public struct FacilityNeedRecoveryData
{
    public float sleep;
    public float mood;
    public float fun;
    public float hunger;
    public float excretion;
    public float hygiene;

    public bool HasEffect => !Mathf.Approximately(sleep, 0f)
        || !Mathf.Approximately(mood, 0f)
        || !Mathf.Approximately(fun, 0f)
        || !Mathf.Approximately(hunger, 0f)
        || !Mathf.Approximately(excretion, 0f)
        || !Mathf.Approximately(hygiene, 0f);
}

[Serializable]
public sealed class FacilityOperationalData
{
    public string code;
    public FacilityFunction functions;
    public FacilityNeedRecoveryData recovery;
    public StockCategory storageCategory = StockCategory.General;
    [Min(0)] public int storageCapacity;
    [Min(0)] public int seatCapacity;
    [Min(0)] public int tableCapacity;
    [Min(0)] public int serviceCapacity;
    [Min(0)] public int workOutputAmount;
    [Min(0)] public int constructionCost;
    [Range(1, 3)] public int unlockPhase = 1;
    [Range(0f, 1f)] public float demolitionRefundRate = 0.5f;
    [Min(0f)] public float lightIntensity;
    [Min(0f)] public float lightRadius;

    public bool IsModular => !string.IsNullOrWhiteSpace(code);
    public bool HasFunction(FacilityFunction function)
    {
        return function != FacilityFunction.None && (functions & function) != 0;
    }
}

[Serializable]
public sealed class FacilityOperationalState
{
    [Min(0)] public int completedUses;
    [Min(0)] public int completedWorkCycles;
    [Min(0)] public int producedStock;
    [Min(0)] public int alarmCharges;
    [Range(0f, 100f)] public float cleanliness = 100f;

    public FacilityOperationalState Clone()
    {
        return new FacilityOperationalState
        {
            completedUses = completedUses,
            completedWorkCycles = completedWorkCycles,
            producedStock = producedStock,
            alarmCharges = alarmCharges,
            cleanliness = cleanliness
        };
    }

    public void CopyFrom(FacilityOperationalState source)
    {
        if (source == null)
        {
            completedUses = 0;
            completedWorkCycles = 0;
            producedStock = 0;
            alarmCharges = 0;
            cleanliness = 100f;
            return;
        }

        completedUses = Mathf.Max(0, source.completedUses);
        completedWorkCycles = Mathf.Max(0, source.completedWorkCycles);
        producedStock = Mathf.Max(0, source.producedStock);
        alarmCharges = Mathf.Max(0, source.alarmCharges);
        cleanliness = Mathf.Clamp(source.cleanliness, 0f, 100f);
    }
}

public static class FacilityProgression
{
    public static int GetCurrentPhase(GameData gameData)
    {
        int day = gameData != null && gameData.day != null
            ? Mathf.Max(1, gameData.day.Value)
            : 1;
        return Mathf.Clamp(1 + ((day - 1) / 3), 1, 3);
    }

    public static bool IsUnlocked(BuildingSO building, GameData gameData)
    {
        if (building == null || !building.unlocked)
        {
            return false;
        }

        FacilityOperationalData data = building.Operational;
        return !data.IsModular || GetCurrentPhase(gameData) >= Mathf.Clamp(data.unlockPhase, 1, 3);
    }

    public static int GetRefund(BuildingSO building)
    {
        if (building == null)
        {
            return 0;
        }

        FacilityOperationalData data = building.Operational;
        return Mathf.Max(0, Mathf.FloorToInt(data.constructionCost * Mathf.Clamp01(data.demolitionRefundRate)));
    }
}
