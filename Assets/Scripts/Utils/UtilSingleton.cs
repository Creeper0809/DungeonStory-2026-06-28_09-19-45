using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 싱글톤 패턴을 자동적으로 만들어주는 클래스
/// </summary>
/// <typeparam name="T">형태가 될 클래스</typeparam>
public class UtilSingleton<T> : SerializedMonoBehaviour where T : Component
{
    protected static T _instance;
    public static bool HasInstance => _instance != null;
    public static T TryGetInstance() => HasInstance ? _instance : null;
    public static T Current => _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                
                if (_instance == null)
                {
                    GameObject obj = GameObject.Find(typeof(T).Name);
                    if(obj != null)
                    {
                        obj = new GameObject();
                        obj.name = typeof(T).Name + "_AutoCreated";
                    }
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }
    protected virtual void Awake()
    {
        InitializeSingleton();
    }

    protected virtual void InitializeSingleton()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        _instance = this as T;
    }
}
