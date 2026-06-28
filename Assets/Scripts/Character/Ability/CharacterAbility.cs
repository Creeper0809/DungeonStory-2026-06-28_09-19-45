using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAbility : SerializedMonoBehaviour
{
    protected Character character;
    protected Grid grid;
    protected AbilityMove move;
    protected virtual void Awake()
    {
        character = GetComponent<Character>();
    }
    private void Start()
    {
        move = character.GetAbility<AbilityMove>();
        grid = GridSystemManager.Instance.grid;
    }
    public virtual void Initializtion(CharacterSO data)
    {
        
    }
}
