using UnityEngine;

public readonly struct FacilityCrimeRiskContext
{
    public readonly FacilityData facility;
    public readonly CharacterActor actor;
    public readonly bool hasServingWorker;
    public readonly bool hasWaitingCheckout;
    public readonly int currentUserCount;
    public readonly int cartItemCount;
    public readonly int cartValue;
    public readonly int currentStock;
    public readonly bool isDamaged;

    public FacilityCrimeRiskContext(
        FacilityData facility,
        CharacterActor actor,
        bool hasServingWorker,
        bool hasWaitingCheckout,
        int currentUserCount,
        int cartItemCount,
        int cartValue,
        int currentStock,
        bool isDamaged)
    {
        this.facility = facility;
        this.actor = actor;
        this.hasServingWorker = hasServingWorker;
        this.hasWaitingCheckout = hasWaitingCheckout;
        this.currentUserCount = Mathf.Max(0, currentUserCount);
        this.cartItemCount = Mathf.Max(0, cartItemCount);
        this.cartValue = Mathf.Max(0, cartValue);
        this.currentStock = Mathf.Max(0, currentStock);
        this.isDamaged = isDamaged;
    }
}

public static class FacilityCrimeRiskUtility
{
    public static float CalculateShopliftingChance(FacilityCrimeRiskContext context)
    {
        FacilityData facility = context.facility;
        if (facility == null || context.cartItemCount <= 0 || context.currentStock <= 0)
        {
            return 0f;
        }

        float pressure = Mathf.Max(0f, facility.baseCrimePressure);
        pressure += GetSupervisionPressure(context);
        pressure += GetNeedPressure(context);
        pressure += GetCrowdPressure(context);
        pressure += GetCartValuePressure(context);
        pressure += GetFacilityStatePressure(context);

        pressure *= GetActorIncidentMultiplier(context.actor);
        return Mathf.Clamp01(pressure);
    }

    public static float CalculateOperationalRisk(FacilityCrimeRiskContext context)
    {
        FacilityData facility = context.facility;
        if (facility == null || context.currentStock <= 0)
        {
            return 0f;
        }

        FacilityCrimeRiskContext baselineContext = new FacilityCrimeRiskContext(
            facility,
            context.actor,
            context.hasServingWorker,
            context.hasWaitingCheckout,
            context.currentUserCount,
            cartItemCount: 1,
            cartValue: context.cartValue,
            context.currentStock,
            context.isDamaged);

        float chance = CalculateShopliftingChance(baselineContext);
        float risk = chance * 10f;
        if (context.hasWaitingCheckout && !context.hasServingWorker)
        {
            risk += facility.crowdCrimeRiskWeight;
        }

        return Mathf.Max(0f, risk);
    }

    private static float GetSupervisionPressure(FacilityCrimeRiskContext context)
    {
        FacilityData facility = context.facility;
        if (context.hasServingWorker)
        {
            return -Mathf.Max(0f, facility.staffedSupervisionReduction);
        }

        return Mathf.Max(0f, facility.unstaffedSupervisionRisk);
    }

    private static float GetNeedPressure(FacilityCrimeRiskContext context)
    {
        CharacterStats stats = context.actor != null ? context.actor.Stats : null;
        if (stats == null)
        {
            return 0f;
        }

        float mood = GetStat(stats, CharacterCondition.MOOD, 50f);
        float hunger = GetStat(stats, CharacterCondition.HUNGER, 50f);
        float fun = GetStat(stats, CharacterCondition.FUN, 50f);
        float sleep = GetStat(stats, CharacterCondition.SLEEP, 50f);
        float excretion = GetStat(stats, CharacterCondition.EXCRETION, 50f);
        float hygiene = GetStat(stats, CharacterCondition.HYGIENE, 50f);

        float lowMoodPressure = context.facility.lowMoodCrimeRiskWeight
            * LowStatPressure(mood, comfortable: 65f, critical: 15f);
        float unmetNeedPressure = context.facility.unmetNeedCrimeRiskWeight
            * Mathf.Max(
                LowStatPressure(hunger, comfortable: 45f, critical: 5f),
                LowStatPressure(fun, comfortable: 40f, critical: 0f) * 0.6f,
                LowStatPressure(sleep, comfortable: 35f, critical: 0f) * 0.4f,
                LowStatPressure(excretion, comfortable: 55f, critical: 10f) * 0.9f,
                LowStatPressure(hygiene, comfortable: 45f, critical: 5f) * 0.55f);
        return lowMoodPressure + unmetNeedPressure;
    }

    private static float GetCrowdPressure(FacilityCrimeRiskContext context)
    {
        FacilityData facility = context.facility;
        if (facility.capacity <= 0)
        {
            return 0f;
        }

        float crowdRatio = Mathf.Clamp01((float)context.currentUserCount / Mathf.Max(1, facility.capacity));
        float waitPressure = context.hasWaitingCheckout && !context.hasServingWorker ? 0.5f : 0f;
        return facility.crowdCrimeRiskWeight * Mathf.Clamp01(crowdRatio + waitPressure);
    }

    private static float GetCartValuePressure(FacilityCrimeRiskContext context)
    {
        if (context.cartValue <= 0)
        {
            return 0f;
        }

        float valuePressure = Mathf.Clamp01(context.cartValue / 500f);
        float countPressure = Mathf.Clamp01((context.cartItemCount - 1f) / 3f) * 0.5f;
        return context.facility.highValueCrimeRiskWeight * Mathf.Clamp01(valuePressure + countPressure);
    }

    private static float GetFacilityStatePressure(FacilityCrimeRiskContext context)
    {
        float pressure = 0f;
        if (context.isDamaged)
        {
            pressure += context.facility.damagedFacilityCrimeRiskWeight;
        }

        if (context.currentStock <= context.facility.restockRequestThreshold)
        {
            pressure += context.facility.highValueCrimeRiskWeight * 0.25f;
        }

        return pressure;
    }

    private static float GetActorIncidentMultiplier(CharacterActor actor)
    {
        CharacterSpeciesIncidentType incidentType = actor != null
            ? actor.GetIncidentType()
            : CharacterSpeciesIncidentType.None;
        return incidentType switch
        {
            CharacterSpeciesIncidentType.OrcRampage => 1.2f,
            CharacterSpeciesIncidentType.VampireFear => 1.1f,
            CharacterSpeciesIncidentType.SlimeContamination => 1.05f,
            _ => 1f
        };
    }

    private static float GetStat(CharacterStats stats, CharacterCondition condition, float defaultValue)
    {
        return stats.Stats.TryGetValue(condition, out float value) ? Mathf.Clamp(value, 0f, 100f) : defaultValue;
    }

    private static float LowStatPressure(float value, float comfortable, float critical)
    {
        float range = Mathf.Max(1f, comfortable - critical);
        return Mathf.Clamp01((comfortable - value) / range);
    }

    public static bool ShouldTriggerCrime(float chance, float roll)
    {
        if (chance <= 0f)
        {
            return false;
        }

        return Mathf.Clamp01(roll) < Mathf.Clamp01(chance);
    }
}
