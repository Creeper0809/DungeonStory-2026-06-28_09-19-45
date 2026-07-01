using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DrawWithUnity]
public class CharacterAbility : SerializedMonoBehaviour
{
    protected Character character;
    protected Grid grid;
    protected AbilityMove move;
    protected virtual void Awake()
    {
        character = GetComponent<Character>();
    }
    protected virtual void Start()
    {
        CacheCommonReferences();
    }
    protected void CacheCommonReferences()
    {
        if (character == null)
        {
            character = GetComponent<Character>();
        }
        if (character == null) return;

        move = character.GetAbility<AbilityMove>();
        grid = GridSystemManager.Instance.grid;
    }
    public virtual void Initializtion(CharacterSO data)
    {
        CacheCommonReferences();
    }
}
