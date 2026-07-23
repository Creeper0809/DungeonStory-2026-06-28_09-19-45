using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
[CreateAssetMenu(menuName = "DungeonStory/Character/SO", order = 0)]
[DrawWithUnity]
public class CharacterSO : ScriptableObject
{
    public CharacterType characterType;
    public CharacterRole role;
    public int id;
    public string characterName;
    public string speciesTag;
    public CharacterSpeciesSO species;
    public CharacterStatBlock baseStats = CharacterStatBlock.CreateDefault();
    public CharacterTraitSO[] traits = Array.Empty<CharacterTraitSO>();
    public WorkPriorityProfile defaultWorkPriorities = WorkPriorityProfile.CreateDefault();
    public CharacterAiPersonality aiPersonality = new CharacterAiPersonality();
    [TextArea] public string ownerSummary;
    public FacilityWorkType ownerPreferredWorkTypes;
    [SerializeField] private CharacterSkillInstance[] ownerFixedSkills = Array.Empty<CharacterSkillInstance>();

    public Sprite characterSprite;

    public BuildingSO[] favoriteStore;

    [SerializeField] private int frequencyVisitMax;
    [SerializeField] private int frequencyVisitMin;

    [SerializeField] private int maxHoldingMoney;
    [SerializeField] private int minHoldingMoney;

    [SerializeField] private CharacterSpeedType speedType;
    [SerializeField] private CharacterRespawnSpeedType respawnSpeedType;

    public string SpeciesTag => species != null && !string.IsNullOrWhiteSpace(species.speciesTag)
        ? species.speciesTag
        : speciesTag;
    public bool IsOwnerCandidate => role == CharacterRole.Owner;
    public IReadOnlyList<CharacterSkillInstance> OwnerFixedSkills =>
        ownerFixedSkills ?? Array.Empty<CharacterSkillInstance>();
    public float moveSpeed
    {
        get
        {
            int rawSpeed = (int)speedType;
            if (rawSpeed <= 0)
            {
                rawSpeed = (int)CharacterSpeedType.Normal;
            }

            return rawSpeed / 3.5f;
        }
    }

    public CharacterRuntimeProfile CreateRuntimeProfile()
    {
        return CharacterRuntimeProfile.From(this);
    }

    public TimeOfDay leavingTime
    {
        get
        {
            if(characterType == CharacterType.NPC)
            {
                return TimeOfDay.Morning;
            }
            else
            {
                return TimeOfDay.Evening;
            }
        }
    }
    public TimeOfDay respawnTime
    {
        get
        {
            if(characterType == CharacterType.NPC)
            {
                return TimeOfDay.Night;
            }
            else
            {
                return TimeOfDay.Noon;
            }
        }
    }

    public float respawnSpeed {
        get
        {
            int temp = (int)respawnSpeedType;
            return Random.Range(temp, temp * 1.5f);
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //                                                                                                                              //
    //                                                                    GetSet                                                    //
    //                                                                                                                              //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public int GetFrequencyVisit()
    {
        int min = Mathf.Max(1, frequencyVisitMin);
        int max = Mathf.Max(min, frequencyVisitMax);
        return Random.Range(min, max + 1);
    }
    public int GetHoldingMoney()
    {
        return Random.Range(minHoldingMoney, maxHoldingMoney);
    }

    public int GetHoldingMoney(CharacterRuntimeProfile profile)
    {
        int baseMoney = GetHoldingMoney();
        float multiplier = profile != null ? profile.GetSpendingMultiplier() : 1f;
        return Mathf.Max(0, Mathf.RoundToInt(baseMoney * multiplier));
    }
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //                                                                                                                              //
    //                                                                    ForEditor                                                 //
    //                                                                                                                              //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private enum CharacterSpeedType
    {
        VerySlow = 2,
        Slow = 3,
        Normal = 4,
        Fast = 5,
        VeryFast = 6
    }
    private enum CharacterRespawnSpeedType
    {
        VerySlow = 23,
        Slow = 18,
        Normal = 13,
        Fast = 8,
        VeryFast = 3
    }
}
public enum CharacterType
{
    NPC,
    Customer,
    Intruder
}

public enum CharacterRole
{
    Regular,
    Owner
}
