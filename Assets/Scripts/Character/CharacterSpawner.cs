using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class CharacterSpawner : BuildableObject,IInteractable
{
    public CharacterSO[] characters;
    public GameObject characterPrefab;
    private float timer;
    private Dictionary<int, CharacterRespawnData> respawnDict;
    public IObjectPool<GameObject> characterPool;
    private WaitForSeconds spawnDelay;
    void Start()
    {
        centerPos = new Vector2Int(-10, 0);
        characters = characters.OrderBy((x) => x.id).ToArray();
        characterPool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 5, 15);
        respawnDict = new Dictionary<int, CharacterRespawnData>();
        spawnDelay = new WaitForSeconds(0.3f);
        StartCoroutine(StartSpawn());
    }
    public IEnumerator StartSpawn()
    {
        while (true)
        {
            foreach (var item in characters)
            {
                if (TrySpawnCharacter(item.id))
                {
                    yield return spawnDelay;
                    break;
                }
            }
            yield return null;
        }
    }
    void Update()
    {
        timer += Time.deltaTime;
        foreach(var item in respawnDict)
        {
            if (item.Value.CheckResapwn(timer))
            {
                respawnDict.Remove(item.Key);
                break;
            }
        }
    }
    public bool TrySpawnCharacter(int id)
    {
        if (respawnDict.ContainsKey(id)) return false;
        GameObject spawnedCharacterGameobject = characterPool.Get();
        spawnedCharacterGameobject.transform.position = this.transform.position;
        Character spawnedCharacter = spawnedCharacterGameobject.GetComponent<Character>();
        spawnedCharacter.Initialization(characters[id]);
        respawnDict.Add(id, new CharacterRespawnData(id, characters[id].respawnSpeed));
        return true;
    }
    public void Respawned(CharacterRespawnData data)
    {
        respawnDict.Remove(data.id);    
    }
    private GameObject CreatePooledItem()
    {
        return Instantiate(characterPrefab);
    }
    private void OnTakeFromPool(GameObject poolGo)
    {
        poolGo.SetActive(true);
    }
    private void OnReturnedToPool(GameObject poolGo)
    {
        poolGo.SetActive(false);
    }
    private void OnDestroyPoolObject(GameObject poolGo)
    {
        Destroy(poolGo);
    }
    public IEnumerator Interact(Character character)
    {
        respawnDict[character.data.id].StartCheckRespawn(timer);
        characterPool.Release(character.gameObject);
        yield return null;
    }
}
public class CharacterRespawnData
{
    public int id;
    public float lastDisabledTime;
    public float respawnTime;
    public bool isDiabled;
    public CharacterRespawnData(int id, float respawnTime)
    {
        this.respawnTime = respawnTime;
        this.id = id;
        isDiabled = false;
    }
    public void StartCheckRespawn(float lastDisabledTime)
    {
        isDiabled = true;
        this.lastDisabledTime = lastDisabledTime;
    }
    public bool CheckResapwn(float time)
    {
        if (!isDiabled) return false;
        if ((time - lastDisabledTime) < respawnTime) return false;
        return true;
    }
}