using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataScriptableObject : SerializedScriptableObject
{
    [field: SerializeField] public int id { get; private set; }
}
