using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class CharacterSpawner : BuildableObject,IInteractable
{
    public CharacterSO[] characters;
    public GameObject characterPrefab;
    [SerializeField] private Transform outsideSpawnPoint;
    [SerializeField] private Transform entryDoorPoint;
    [SerializeField] private Vector2Int entryGridPosition = new Vector2Int(4, 0);

    private float timer;
    private Dictionary<int, CharacterRespawnData> respawnDict = new Dictionary<int, CharacterRespawnData>();
    private Dictionary<int, CharacterSO> charactersById = new Dictionary<int, CharacterSO>();
    public IObjectPool<GameObject> characterPool;
    private WaitForSeconds spawnDelay = new WaitForSeconds(0.3f);
    private bool spawnRoutineStarted;

    private void Awake()
    {
        EnsureRuntimeState();
    }

    public override void Start()
    {
        base.Start();
        centerPos = GetEntryGridPosition();
        EnsureRuntimeState();

        if (!spawnRoutineStarted)
        {
            spawnRoutineStarted = true;
            StartCoroutine(StartSpawn());
        }
    }

    private void EnsureRuntimeState()
    {
        if (characters == null)
        {
            characters = new CharacterSO[0];
        }

        characters = characters.Where((x) => x != null).OrderBy((x) => x.id).ToArray();
        charactersById = characters.GroupBy((x) => x.id).ToDictionary((x) => x.Key, (x) => x.First());
        respawnDict ??= new Dictionary<int, CharacterRespawnData>();
        spawnDelay ??= new WaitForSeconds(0.3f);

        if (characterPool == null && characterPrefab != null)
        {
            characterPool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 5, 15);
        }
    }

    public IEnumerator StartSpawn()
    {
        while (true)
        {
            EnsureRuntimeState();

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
        if (respawnDict == null) return;

        timer += Time.deltaTime;
        int? respawnedId = null;
        foreach(var item in respawnDict)
        {
            if (item.Value.CheckResapwn(timer))
            {
                respawnedId = item.Key;
                break;
            }
        }
        if (respawnedId.HasValue)
        {
            respawnDict.Remove(respawnedId.Value);
        }
    }
    public bool TrySpawnCharacter(int id)
    {
        EnsureRuntimeState();
        if (characterPool == null)
        {
            Debug.LogWarning("캐릭터 프리팹이 없어 캐릭터를 스폰할 수 없습니다.");
            return false;
        }

        if (respawnDict.ContainsKey(id)) return false;

        if (!charactersById.TryGetValue(id, out CharacterSO characterData))
        {
            Debug.LogWarning($"스폰할 캐릭터 데이터를 찾지 못했습니다. id: {id}");
            return false;
        }

        if (!TryGetEntryGridPosition(out Vector2Int resolvedEntryGridPosition))
        {
            return false;
        }

        GameObject spawnedCharacterGameobject = characterPool.Get();
        spawnedCharacterGameobject.transform.position = GetOutsideSpawnWorldPosition();
        Character spawnedCharacter = spawnedCharacterGameobject.GetComponent<Character>();
        if (spawnedCharacter == null)
        {
            Debug.LogWarning("캐릭터 프리팹에 Character 컴포넌트가 없습니다.");
            characterPool.Release(spawnedCharacterGameobject);
            return false;
        }

        spawnedCharacter.SetLifecycleState(Character.LifecycleState.SpawningOutside);
        spawnedCharacter.Initialization(characterData);
        if (spawnedCharacter.TryGetAbility(out AbilityMove move))
        {
            move.StartEnterDungeon(GetEntryDoorWorldPosition(), resolvedEntryGridPosition);
        }
        else
        {
            Grid grid = GridSystemManager.Instance != null ? GridSystemManager.Instance.grid : null;
            if (grid != null)
            {
                spawnedCharacter.transform.position = grid.GetWorldPos(resolvedEntryGridPosition);
            }

            spawnedCharacter.SetLifecycleState(Character.LifecycleState.Active);
        }

        respawnDict.Add(id, new CharacterRespawnData(id, characterData.respawnSpeed));
        return true;
    }

    public Vector3 GetOutsideSpawnWorldPosition()
    {
        if (outsideSpawnPoint != null)
        {
            return outsideSpawnPoint.position;
        }

        return transform.position;
    }

    public Vector3 GetEntryDoorWorldPosition()
    {
        if (entryDoorPoint != null)
        {
            return entryDoorPoint.position;
        }

        Grid grid = GridSystemManager.Instance != null ? GridSystemManager.Instance.grid : null;
        if (grid == null)
        {
            return transform.position;
        }

        return grid.GetWorldPos(GetEntryGridPosition());
    }

    public Vector2Int GetEntryGridPosition()
    {
        return TryGetEntryGridPosition(out Vector2Int resolvedEntryGridPosition)
            ? resolvedEntryGridPosition
            : entryGridPosition;
    }

    public bool TryGetEntryGridPosition(out Vector2Int resolvedEntryGridPosition)
    {
        Grid grid = GridSystemManager.Instance != null ? GridSystemManager.Instance.grid : null;
        if (grid == null)
        {
            resolvedEntryGridPosition = entryGridPosition;
            return false;
        }

        if (grid.IsValidGridPos(entryGridPosition) && grid.IsWalkable(entryGridPosition))
        {
            resolvedEntryGridPosition = entryGridPosition;
            return true;
        }

        Vector3 desiredWorldPosition = entryDoorPoint != null ? entryDoorPoint.position : transform.position;
        Vector2Int desiredGridPosition = grid.GetXY(desiredWorldPosition);
        return grid.TryFindNearestWalkablePosition(desiredGridPosition, out resolvedEntryGridPosition);
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
        Character character = poolGo.GetComponent<Character>();
        if (character != null)
        {
            character.SetLifecycleState(Character.LifecycleState.Despawned);
        }

        poolGo.SetActive(false);
    }
    private void OnDestroyPoolObject(GameObject poolGo)
    {
        Destroy(poolGo);
    }
    public IEnumerator Interact(Character character)
    {
        if (character == null || character.data == null) yield break;

        if (respawnDict.TryGetValue(character.data.id, out CharacterRespawnData respawnData))
        {
            respawnData.StartCheckRespawn(timer);
        }
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
