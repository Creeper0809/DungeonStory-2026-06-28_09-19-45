using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIActionSet : SerializedScriptableObject
{
    public string actionName;
    [field: SerializeField]
    public Consideration[] considerations { get; private set; }
    public abstract void Execute(Character character);
    public virtual BuildableObject GetDestination(Character character)
    {
        return null;
    }
}
