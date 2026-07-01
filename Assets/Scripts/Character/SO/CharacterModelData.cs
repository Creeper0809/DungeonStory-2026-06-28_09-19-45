using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CharacterStatType
{
    Attack,
    Sales,
    Research,
    MoveSpeed,
    Strength,
    Toughness,
    Dexterity,
    Cleaning,
    Endurance
}

[Serializable]
public class CharacterStatBlock
{
    public int attack;
    public int sales;
    public int research;
    public int moveSpeed;
    public int strength;
    public int toughness;
    public int dexterity;
    public int cleaning;
    public int endurance;

    public bool HasAnyValue => attack != 0
        || sales != 0
        || research != 0
        || moveSpeed != 0
        || strength != 0
        || toughness != 0
        || dexterity != 0
        || cleaning != 0
        || endurance != 0;

    public static CharacterStatBlock CreateDefault(int value = 5)
    {
        return new CharacterStatBlock
        {
            attack = value,
            sales = value,
            research = value,
            moveSpeed = value,
            strength = value,
            toughness = value,
            dexterity = value,
            cleaning = value,
            endurance = value
        };
    }

    public int Get(CharacterStatType type)
    {
        return type switch
        {
            CharacterStatType.Attack => attack,
            CharacterStatType.Sales => sales,
            CharacterStatType.Research => research,
            CharacterStatType.MoveSpeed => moveSpeed,
            CharacterStatType.Strength => strength,
            CharacterStatType.Toughness => toughness,
            CharacterStatType.Dexterity => dexterity,
            CharacterStatType.Cleaning => cleaning,
            CharacterStatType.Endurance => endurance,
            _ => 0
        };
    }

    public void Add(CharacterStatBlock other)
    {
        if (other == null) return;

        attack += other.attack;
        sales += other.sales;
        research += other.research;
        moveSpeed += other.moveSpeed;
        strength += other.strength;
        toughness += other.toughness;
        dexterity += other.dexterity;
        cleaning += other.cleaning;
        endurance += other.endurance;
    }
}

[Serializable]
public class CharacterModelModifiers
{
    [Min(0f)] public float consumptionMultiplier = 1f;
    [Min(0f)] public float spendingMultiplier = 1f;
    [Min(0f)] public float waitPatienceMultiplier = 1f;
    [Min(0f)] public float crowdSensitivityMultiplier = 1f;
    [Min(0f)] public float accidentChanceMultiplier = 1f;
    [Min(0f)] public float workSpeedMultiplier = 1f;
    [Min(0f)] public float researchSpeedMultiplier = 1f;
    [Min(0f)] public float combatPowerMultiplier = 1f;
    [Min(0f)] public float moveSpeedMultiplier = 1f;
    [Min(0f)] public float stayDurationMultiplier = 1f;
    public FacilityRole preferredFacilityRoles;
    public FacilityRole dislikedFacilityRoles;
    public FacilityWorkType preferredWorkTypes;
    public FacilityWorkType dislikedWorkTypes;

    public void Multiply(CharacterModelModifiers other)
    {
        if (other == null) return;

        consumptionMultiplier *= Mathf.Max(0f, other.consumptionMultiplier);
        spendingMultiplier *= Mathf.Max(0f, other.spendingMultiplier);
        waitPatienceMultiplier *= Mathf.Max(0f, other.waitPatienceMultiplier);
        crowdSensitivityMultiplier *= Mathf.Max(0f, other.crowdSensitivityMultiplier);
        accidentChanceMultiplier *= Mathf.Max(0f, other.accidentChanceMultiplier);
        workSpeedMultiplier *= Mathf.Max(0f, other.workSpeedMultiplier);
        researchSpeedMultiplier *= Mathf.Max(0f, other.researchSpeedMultiplier);
        combatPowerMultiplier *= Mathf.Max(0f, other.combatPowerMultiplier);
        moveSpeedMultiplier *= Mathf.Max(0f, other.moveSpeedMultiplier);
        stayDurationMultiplier *= Mathf.Max(0f, other.stayDurationMultiplier);
        preferredFacilityRoles |= other.preferredFacilityRoles;
        dislikedFacilityRoles |= other.dislikedFacilityRoles;
        preferredWorkTypes |= other.preferredWorkTypes;
        dislikedWorkTypes |= other.dislikedWorkTypes;
    }
}

public sealed class CharacterRuntimeProfile
{
    private const int DefaultStatValue = 5;

    private readonly CharacterStatBlock finalStats;
    private readonly CharacterModelModifiers finalModifiers;
    private readonly List<CharacterTraitSO> traits;

    private CharacterRuntimeProfile(
        CharacterSO source,
        CharacterSpeciesSO species,
        IEnumerable<CharacterTraitSO> traits)
    {
        Source = source;
        Species = species;
        this.traits = traits?.Where((trait) => trait != null).ToList() ?? new List<CharacterTraitSO>();
        finalStats = BuildFinalStats(source, species, this.traits);
        finalModifiers = BuildFinalModifiers(species, this.traits);
    }

    public CharacterSO Source { get; }
    public CharacterSpeciesSO Species { get; }
    public IReadOnlyList<CharacterTraitSO> Traits => traits;
    public string SpeciesTag => !string.IsNullOrWhiteSpace(Species?.speciesTag)
        ? Species.speciesTag
        : Source != null
            ? Source.speciesTag
            : string.Empty;

    public static CharacterRuntimeProfile From(CharacterSO source)
    {
        return new CharacterRuntimeProfile(source, source != null ? source.species : null, source?.traits);
    }

    public int GetStat(CharacterStatType type)
    {
        return Mathf.Max(0, finalStats.Get(type));
    }

    public float GetMoveSpeedMultiplier()
    {
        return ClampStatMultiplier(CharacterStatType.MoveSpeed, 0.08f, 0.5f, 1.8f)
            * finalModifiers.moveSpeedMultiplier;
    }

    public float GetSpendingMultiplier()
    {
        return ClampStatMultiplier(CharacterStatType.Sales, 0.05f, 0.5f, 2f)
            * finalModifiers.spendingMultiplier;
    }

    public float GetConsumptionMultiplier()
    {
        return Mathf.Max(0f, finalModifiers.consumptionMultiplier);
    }

    public float GetStayDurationMultiplier()
    {
        float speciesStay = Species != null ? Mathf.Max(0f, Species.stayDurationMultiplier) : 1f;
        return speciesStay * Mathf.Max(0f, finalModifiers.stayDurationMultiplier);
    }

    public float GetCrowdSensitivityMultiplier()
    {
        return Mathf.Max(0f, finalModifiers.crowdSensitivityMultiplier);
    }

    public float GetAccidentChanceMultiplier()
    {
        float enduranceMultiplier = Mathf.Clamp(1f - ((GetStat(CharacterStatType.Endurance) - DefaultStatValue) * 0.03f), 0.5f, 1.5f);
        float toughnessMultiplier = Mathf.Clamp(1f - ((GetStat(CharacterStatType.Toughness) - DefaultStatValue) * 0.02f), 0.6f, 1.4f);
        return Mathf.Max(0f, finalModifiers.accidentChanceMultiplier * enduranceMultiplier * toughnessMultiplier);
    }

    public CharacterSpeciesIncidentType GetIncidentType()
    {
        return Species != null ? Species.incidentType : CharacterSpeciesIncidentType.None;
    }

    public string GetIncidentName()
    {
        return Species != null ? Species.incidentName : string.Empty;
    }

    public string GetIncidentDescription()
    {
        return Species != null ? Species.incidentDescription : string.Empty;
    }

    public string GetShortDescription()
    {
        return Species != null ? Species.shortDescription : string.Empty;
    }

    public float GetCombatPowerMultiplier()
    {
        return ClampStatMultiplier(CharacterStatType.Attack, 0.07f, 0.5f, 2f)
            * finalModifiers.combatPowerMultiplier;
    }

    public float GetWorkSpeedMultiplier(FacilityWorkType workTypes)
    {
        float statMultiplier = ClampStatMultiplier(GetBestWorkStat(workTypes), 0.06f, 0.5f, 2f);
        float typeMultiplier = 1f;
        if ((workTypes & finalModifiers.preferredWorkTypes) != 0)
        {
            typeMultiplier *= 1.25f;
        }

        if ((workTypes & finalModifiers.dislikedWorkTypes) != 0)
        {
            typeMultiplier *= 0.75f;
        }

        if ((workTypes & FacilityWorkType.Research) != 0)
        {
            typeMultiplier *= finalModifiers.researchSpeedMultiplier;
        }

        return Mathf.Max(0f, statMultiplier * finalModifiers.workSpeedMultiplier * typeMultiplier);
    }

    public float GetWorkPreferenceScore(FacilityWorkType workTypes)
    {
        if (workTypes == FacilityWorkType.None)
        {
            return 0.5f;
        }

        if ((workTypes & finalModifiers.dislikedWorkTypes) != 0)
        {
            return 0.1f;
        }

        if ((workTypes & finalModifiers.preferredWorkTypes) != 0)
        {
            return 1f;
        }

        return 0.5f;
    }

    public float GetFacilityPreferenceScore(FacilityRole roles)
    {
        if (roles == FacilityRole.None)
        {
            return 0.5f;
        }

        if ((roles & finalModifiers.dislikedFacilityRoles) != 0)
        {
            return 0.1f;
        }

        if ((roles & finalModifiers.preferredFacilityRoles) != 0)
        {
            return 1f;
        }

        return 0.5f;
    }

    public bool HasTrait(string traitName)
    {
        if (string.IsNullOrWhiteSpace(traitName)) return false;

        return traits.Any((trait) => trait != null && trait.traitName == traitName);
    }

    private float ClampStatMultiplier(
        CharacterStatType statType,
        float perPoint,
        float min,
        float max)
    {
        return Mathf.Clamp(1f + ((GetStat(statType) - DefaultStatValue) * perPoint), min, max);
    }

    private static CharacterStatType GetBestWorkStat(FacilityWorkType workTypes)
    {
        if ((workTypes & FacilityWorkType.Research) != 0) return CharacterStatType.Research;
        if ((workTypes & FacilityWorkType.Guard) != 0) return CharacterStatType.Attack;
        if ((workTypes & FacilityWorkType.Clean) != 0) return CharacterStatType.Cleaning;
        if ((workTypes & FacilityWorkType.Restock) != 0) return CharacterStatType.Strength;
        if ((workTypes & FacilityWorkType.Repair) != 0) return CharacterStatType.Dexterity;
        if ((workTypes & FacilityWorkType.Operate) != 0) return CharacterStatType.Sales;
        if ((workTypes & FacilityWorkType.Rescue) != 0) return CharacterStatType.Toughness;
        return CharacterStatType.Endurance;
    }

    private static CharacterStatBlock BuildFinalStats(
        CharacterSO source,
        CharacterSpeciesSO species,
        IEnumerable<CharacterTraitSO> traits)
    {
        CharacterStatBlock result = source != null && source.baseStats != null && source.baseStats.HasAnyValue
            ? CopyStats(source.baseStats)
            : CharacterStatBlock.CreateDefault(DefaultStatValue);

        result.Add(species?.statBonus);
        if (traits != null)
        {
            foreach (CharacterTraitSO trait in traits)
            {
                result.Add(trait?.statBonus);
            }
        }

        return result;
    }

    private static CharacterModelModifiers BuildFinalModifiers(
        CharacterSpeciesSO species,
        IEnumerable<CharacterTraitSO> traits)
    {
        CharacterModelModifiers result = new CharacterModelModifiers();
        result.Multiply(species?.modifiers);
        if (traits != null)
        {
            foreach (CharacterTraitSO trait in traits)
            {
                result.Multiply(trait?.modifiers);
            }
        }

        return result;
    }

    private static CharacterStatBlock CopyStats(CharacterStatBlock source)
    {
        CharacterStatBlock result = new CharacterStatBlock();
        result.Add(source);
        return result;
    }
}
