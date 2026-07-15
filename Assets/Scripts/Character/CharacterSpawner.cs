using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

public static class DungeonEntranceGridResolver
{
    public static bool TryResolve(
        Grid grid,
        Vector2Int preferredInsidePosition,
        out Door entrance)
    {
        entrance = null;
        if (grid == null)
        {
            return false;
        }

        entrance = grid.GetCells()
            .Select(cell => cell?.GetBuildingInlayer(GridLayer.Building))
            .OfType<Door>()
            .Where(door => door != null
                && door.GetType() == typeof(Door)
                && !door.isDestroy
                && door.BuildingData != null
                && !door.BuildingData.IsInteriorDoor)
            .Distinct()
            .OrderBy(door => Mathf.Abs(door.centerPos.x - preferredInsidePosition.x)
                + Mathf.Abs(door.centerPos.y - preferredInsidePosition.y))
            .FirstOrDefault();
        return entrance != null;
    }
}

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
    private IRegularCustomerRuntimeProvider regularCustomerRuntimeProvider;
    private IGridSystemProvider gridSystemProvider;
    private IRunVariableRuntimeReader runVariableReader;
    private ICharacterSpawnObjectFactory characterObjectFactory;

    [Inject]
    public void Construct(
        IRegularCustomerRuntimeProvider regularCustomerRuntimeProvider,
        IGridSystemProvider gridSystemProvider,
        IRunVariableRuntimeReader runVariableReader,
        ICharacterSpawnObjectFactory characterObjectFactory)
    {
        this.regularCustomerRuntimeProvider = regularCustomerRuntimeProvider
            ?? throw new ArgumentNullException(nameof(regularCustomerRuntimeProvider));
        this.gridSystemProvider = gridSystemProvider
            ?? throw new ArgumentNullException(nameof(gridSystemProvider));
        this.runVariableReader = runVariableReader
            ?? throw new ArgumentNullException(nameof(runVariableReader));
        this.characterObjectFactory = characterObjectFactory
            ?? throw new ArgumentNullException(nameof(characterObjectFactory));
    }

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

        RegularCustomerState regularCustomerState = GetRegularCustomerState();
        if (!RegularCustomerService.CanSpawnAsCustomer(characterData, regularCustomerState))
        {
            return false;
        }

        if (!TryGetEntryGridPosition(out Vector2Int resolvedEntryGridPosition))
        {
            return false;
        }

        GameObject spawnedCharacterGameobject = characterPool.Get();
        RequireCharacterObjectFactory().Inject(spawnedCharacterGameobject);
        spawnedCharacterGameobject.transform.position = GetOutsideSpawnWorldPosition();
        CharacterActor spawnedCharacter = spawnedCharacterGameobject.GetComponent<CharacterActor>();
        if (spawnedCharacter == null)
        {
            Debug.LogWarning("캐릭터 프리팹에 CharacterActor 컴포넌트가 없습니다.");
            characterPool.Release(spawnedCharacterGameobject);
            return false;
        }

        spawnedCharacter.SetLifecycleState(CharacterLifecycleState.SpawningOutside);
        spawnedCharacter.Initialize(characterData);
        if (spawnedCharacter.TryGetAbility(out AbilityMove move))
        {
            move.StartEnterDungeon(GetEntryDoorWorldPosition(), resolvedEntryGridPosition);
        }
        else
        {
            if (TryGetGrid(out Grid grid))
            {
                spawnedCharacter.transform.position = grid.GetWorldPos(resolvedEntryGridPosition);
            }

            spawnedCharacter.SetLifecycleState(CharacterLifecycleState.Active);
        }

        float demandMultiplier = ResolveRunVariableReader().GetGuestDemandMultiplier(characterData.SpeciesTag);
        float respawnTime = characterData.respawnSpeed / Mathf.Max(0.1f, demandMultiplier);
        respawnDict.Add(id, new CharacterRespawnData(id, respawnTime));
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

        if (!TryGetGrid(out Grid grid))
        {
            return transform.position;
        }

        if (DungeonEntranceGridResolver.TryResolve(
                grid,
                entryGridPosition,
                out Door entrance))
        {
            return grid.GetWorldPos(entrance.centerPos);
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
        if (!TryGetGrid(out Grid grid))
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

    private RegularCustomerState GetRegularCustomerState()
    {
        return ResolveRegularCustomerRuntimeProvider().TryGetRuntime(out RegularCustomerRuntime runtime)
            ? runtime.State
            : null;
    }

    private bool TryGetGrid(out Grid grid)
    {
        return ResolveGridSystemProvider().TryGetGrid(out grid);
    }

    private IRegularCustomerRuntimeProvider ResolveRegularCustomerRuntimeProvider()
    {
        return regularCustomerRuntimeProvider
            ?? throw new InvalidOperationException($"{nameof(CharacterSpawner)} requires {nameof(IRegularCustomerRuntimeProvider)} injection.");
    }

    private IGridSystemProvider ResolveGridSystemProvider()
    {
        return gridSystemProvider
            ?? throw new InvalidOperationException($"{nameof(CharacterSpawner)} requires {nameof(IGridSystemProvider)} injection.");
    }

    private IRunVariableRuntimeReader ResolveRunVariableReader()
    {
        return runVariableReader
            ?? throw new InvalidOperationException($"{nameof(CharacterSpawner)} requires {nameof(IRunVariableRuntimeReader)} injection.");
    }

    private ICharacterSpawnObjectFactory RequireCharacterObjectFactory()
    {
        return characterObjectFactory
            ?? throw new InvalidOperationException($"{nameof(CharacterSpawner)} requires {nameof(ICharacterSpawnObjectFactory)} injection.");
    }

    public void Respawned(CharacterRespawnData data)
    {
        respawnDict.Remove(data.id);
    }
    private GameObject CreatePooledItem()
    {
        return RequireCharacterObjectFactory().Create(characterPrefab);
    }
    private void OnTakeFromPool(GameObject poolGo)
    {
        poolGo.SetActive(true);
    }
    private void OnReturnedToPool(GameObject poolGo)
    {
        CharacterActor actor = poolGo.GetComponent<CharacterActor>();
        if (actor != null)
        {
            actor.SetLifecycleState(CharacterLifecycleState.Despawned);
        }

        poolGo.SetActive(false);
    }
    private void OnDestroyPoolObject(GameObject poolGo)
    {
        RequireCharacterObjectFactory().Destroy(poolGo);
    }
    public IEnumerator Interact(CharacterActor actor)
    {
        CharacterIdentity identity = actor != null ? actor.Identity : null;
        if (identity == null || identity.Data == null) yield break;

        EnsureRuntimeState();
        if (respawnDict.TryGetValue(identity.Data.id, out CharacterRespawnData respawnData))
        {
            respawnData.StartCheckRespawn(timer);
        }

        if (characterPool != null)
        {
            characterPool.Release(actor.gameObject);
        }
        else
        {
            actor.SetLifecycleState(CharacterLifecycleState.Despawned);
            actor.gameObject.SetActive(false);
        }

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
