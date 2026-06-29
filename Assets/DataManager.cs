using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager
{
    private static DataManager instance;
    public static DataManager Instance { 
        get
        {
            if(instance == null)
            {
                instance = new DataManager();
            }
            return instance;
        }
    }
    private Dictionary<Type, Dictionary<int, DataScriptableObject>> data;
    private DataManager()
    {
        data = new Dictionary<Type, Dictionary<int, DataScriptableObject>>();
        BuildAllData();
    }
    private void BuildAllData()
    {
        DataScriptableObject[] allScriptableObjects = Resources.LoadAll<DataScriptableObject>("SO");
        foreach (var scriptableObject in allScriptableObjects)
        {
            Type type = scriptableObject.GetType();
            if (!data.ContainsKey(type))
            {
                data[type] = new Dictionary<int, DataScriptableObject>();
            }

            if (data[type].ContainsKey(scriptableObject.id))
            {
                Debug.LogWarning(
                    $"{type} id {scriptableObject.id} 중복 데이터가 있습니다. " +
                    $"{data[type][scriptableObject.id].name}를 유지하고 {scriptableObject.name}는 무시합니다.");
                continue;
            }

            data[type].Add(scriptableObject.id, scriptableObject);
            Debug.Log($"{type}에 {scriptableObject.name} 데이터가 추가되었습니다");
        }
    }
    public Dictionary<int, T> GetData<T>() where T : DataScriptableObject
    {
        if (data.TryGetValue(typeof(T), out var typeData))
        {
            Dictionary<int, T> result = new Dictionary<int, T>();
            foreach (var item in typeData)
            {
                result.Add(item.Key, (T)item.Value);
            }
            return result;
        }
        Debug.Log("데이터 타입 찾지 못함");
        return null;
    }
}
