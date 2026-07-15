using System;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterMoodFactorKind
{
    Need,
    Interaction
}

public sealed class CharacterMoodFactorSnapshot
{
    public CharacterMoodFactorSnapshot(
        string id,
        string label,
        float value,
        CharacterMoodFactorKind kind,
        float remainingSeconds)
    {
        Id = id ?? string.Empty;
        Label = label ?? string.Empty;
        Value = value;
        Kind = kind;
        RemainingSeconds = Mathf.Max(0f, remainingSeconds);
    }

    public string Id { get; }
    public string Label { get; }
    public float Value { get; }
    public CharacterMoodFactorKind Kind { get; }
    public float RemainingSeconds { get; }
}

public sealed class CharacterMoodSnapshot
{
    public CharacterMoodSnapshot(
        float value,
        float baseValue,
        IReadOnlyList<CharacterMoodFactorSnapshot> factors)
    {
        Value = Mathf.Clamp(value, 0f, 100f);
        BaseValue = Mathf.Clamp(baseValue, 0f, 100f);
        Factors = factors ?? Array.Empty<CharacterMoodFactorSnapshot>();
    }

    public float Value { get; }
    public float BaseValue { get; }
    public IReadOnlyList<CharacterMoodFactorSnapshot> Factors { get; }
    public float TotalOffset => Value - BaseValue;
}

[Serializable]
public sealed class CharacterMoodMemory
{
    [SerializeField] private string id;
    [SerializeField] private string label;
    [SerializeField] private float valuePerStack;
    [SerializeField] private int stacks = 1;
    [SerializeField] private int maxStacks = 1;
    [SerializeField] private float expiresAt;

    public string Id => id;
    public float ExpiresAt => expiresAt;

    public CharacterMoodMemory(
        string factorId,
        string factorLabel,
        float value,
        float durationSeconds,
        int stackLimit,
        float now)
    {
        id = factorId;
        Apply(factorLabel, value, durationSeconds, stackLimit, now, resetStacks: true);
    }

    public void Apply(
        string factorLabel,
        float value,
        float durationSeconds,
        int stackLimit,
        float now,
        bool resetStacks = false)
    {
        int nextLimit = Mathf.Clamp(stackLimit, 1, 8);
        bool signChanged = !Mathf.Approximately(valuePerStack, 0f)
            && Mathf.Sign(valuePerStack) != Mathf.Sign(value);
        if (resetStacks || signChanged)
        {
            stacks = 1;
        }
        else if (nextLimit > 1)
        {
            stacks = Mathf.Min(stacks + 1, nextLimit);
        }
        else
        {
            stacks = 1;
        }

        label = factorLabel;
        valuePerStack = Mathf.Clamp(value, -25f, 25f);
        maxStacks = nextLimit;
        stacks = Mathf.Clamp(stacks, 1, maxStacks);
        expiresAt = now + Mathf.Max(0.25f, durationSeconds);
    }

    public bool IsExpired(float now)
    {
        return now >= expiresAt;
    }

    public CharacterMoodFactorSnapshot CreateSnapshot(float now)
    {
        float totalValue = Mathf.Clamp(valuePerStack * stacks, -25f, 25f);
        string displayLabel = stacks > 1 ? $"{label} x{stacks}" : label;
        return new CharacterMoodFactorSnapshot(
            id,
            displayLabel,
            totalValue,
            CharacterMoodFactorKind.Interaction,
            expiresAt - now);
    }
}

public static class CharacterMoodRules
{
    public const float DefaultBaseMood = 55f;

    public static List<CharacterMoodFactorSnapshot> BuildNeedFactors(
        IReadOnlyDictionary<CharacterCondition, float> stats)
    {
        List<CharacterMoodFactorSnapshot> factors = new List<CharacterMoodFactorSnapshot>();
        AddNeedFactor(factors, stats, CharacterCondition.HUNGER,
            "need:hunger", "굶주림", -18f, "허기짐", -9f, "배가 든든함", 4f);
        AddNeedFactor(factors, stats, CharacterCondition.SLEEP,
            "need:sleep", "완전히 지침", -15f, "피곤함", -8f, "푹 쉼", 4f);
        AddNeedFactor(factors, stats, CharacterCondition.FUN,
            "need:fun", "몹시 지루함", -12f, "재미 부족", -6f, "즐거움 충족", 4f);
        AddNeedFactor(factors, stats, CharacterCondition.EXCRETION,
            "need:excretion", "용변이 매우 급함", -12f, "화장실이 필요함", -6f, "개운함", 2f);
        AddNeedFactor(factors, stats, CharacterCondition.HYGIENE,
            "need:hygiene", "매우 지저분함", -10f, "씻고 싶음", -5f, "깨끗함", 3f);
        return factors;
    }

    public static string GetMoodLabel(float mood)
    {
        if (mood < 15f) return "절망적";
        if (mood < 30f) return "불쾌함";
        if (mood < 45f) return "가라앉음";
        if (mood < 60f) return "평온함";
        if (mood < 80f) return "만족함";
        return "아주 좋음";
    }

    private static void AddNeedFactor(
        ICollection<CharacterMoodFactorSnapshot> factors,
        IReadOnlyDictionary<CharacterCondition, float> stats,
        CharacterCondition condition,
        string id,
        string criticalLabel,
        float criticalValue,
        string lowLabel,
        float lowValue,
        string highLabel,
        float highValue)
    {
        float value = stats != null && stats.TryGetValue(condition, out float current)
            ? Mathf.Clamp(current, 0f, 100f)
            : 50f;
        if (value <= 15f)
        {
            factors.Add(new CharacterMoodFactorSnapshot(
                id, criticalLabel, criticalValue, CharacterMoodFactorKind.Need, 0f));
        }
        else if (value <= 35f)
        {
            factors.Add(new CharacterMoodFactorSnapshot(
                id, lowLabel, lowValue, CharacterMoodFactorKind.Need, 0f));
        }
        else if (value >= 85f)
        {
            factors.Add(new CharacterMoodFactorSnapshot(
                id, highLabel, highValue, CharacterMoodFactorKind.Need, 0f));
        }
    }
}
