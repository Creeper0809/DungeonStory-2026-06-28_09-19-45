using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[Serializable]
public class Data<T>
{
    public Action<T> OnValueChange;
    [SerializeField] private T v;
    public T Value
    {
        get => v;
        set
        {
            if (v != null && v.Equals(value)) return;
            v = value;
            OnValueChange?.Invoke(Value);
        }
    }
    public void Initialize(T t)
    {
        v = t;
        OnValueChange?.Invoke(Value);
    }
}
public class DataList<T>
{
    public Action<List<T>> OnDataAdded;
    public Action<List<T>> OnDataDeleted;
    [SerializeField]
    private List<T> v = new List<T>();

    public List<T> Value
    {
        get => v;
    }

    public void Add(T item)
    {
        v.Add(item);
        OnDataAdded?.Invoke(v);
    }
    public void Remove(T item)
    {
        v.Remove(item);
        OnDataDeleted?.Invoke(v);
    }
}
