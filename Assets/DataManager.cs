using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager
{
    private readonly Dictionary<Type, Dictionary<int, DataScriptableObject>> data;

    public DataManager(IDataScriptableObjectSource source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        data = new Dictionary<Type, Dictionary<int, DataScriptableObject>>();
        BuildAllData(source.LoadAll());
    }

    private void BuildAllData(IReadOnlyCollection<DataScriptableObject> allScriptableObjects)
    {
        foreach (DataScriptableObject scriptableObject in allScriptableObjects)
        {
            if (scriptableObject == null)
            {
                continue;
            }

            Type type = scriptableObject.GetType();
            if (!data.ContainsKey(type))
            {
                data[type] = new Dictionary<int, DataScriptableObject>();
            }

            if (data[type].ContainsKey(scriptableObject.id))
            {
                Debug.LogWarning(
                    $"{type} id {scriptableObject.id} is duplicated. " +
                    $"Keeping {data[type][scriptableObject.id].name} and ignoring {scriptableObject.name}.");
                continue;
            }

            data[type].Add(scriptableObject.id, scriptableObject);
            Debug.Log($"Loaded {type} data: {scriptableObject.name}");
        }
    }

    public Dictionary<int, T> GetData<T>() where T : DataScriptableObject
    {
        if (data.TryGetValue(typeof(T), out Dictionary<int, DataScriptableObject> typeData))
        {
            Dictionary<int, T> result = new Dictionary<int, T>();
            foreach (KeyValuePair<int, DataScriptableObject> item in typeData)
            {
                result.Add(item.Key, (T)item.Value);
            }

            return result;
        }

        Debug.Log($"No data found for {typeof(T).Name}.");
        return null;
    }
}
