using BehaviorDesigner.Runtime;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "DungeonStory/Character/SO", order = 0)]
public class CharacterSO : ScriptableObject
{
    public CharacterType characterType;
    public int id;
    public string characterName;

    public Sprite characterSprite;

    public BuildingSO[] favoriteStore;

    [SerializeField] private int frequencyVisitMax;
    [SerializeField] private int frequencyVisitMin;

    [SerializeField] private int maxHoldingMoney;
    [SerializeField] private int minHoldingMoney;

    [SerializeField] private CharacterSpeedType speedType;
    [SerializeField] private CharacterRespawnSpeedType respawnSpeedType;

    public float moveSpeed => (int)speedType / 3.5f;

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

    public ExternalBehavior basicBehaviorPatterns;
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
