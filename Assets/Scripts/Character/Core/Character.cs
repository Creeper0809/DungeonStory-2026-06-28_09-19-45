using BehaviorDesigner.Runtime;
using DamageNumbersPro;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.TextCore.Text;
using Random = UnityEngine.Random;

public class Character : SerializedMonoBehaviour, IInfoable
{
    public enum Facing
    {
        RIGHT,
        LEFT
    }
    public enum Condition
    {
        HUNGER,
        SLEEP,
        FUN,
        MOOD
    }
    public enum State
    {
        DECIDE,
        MOVE,
        EXECUTE
    }

    public enum LifecycleState
    {
        None,
        SpawningOutside,
        EnteringDungeon,
        Active,
        ExitingDungeon,
        Despawned
    }

    public CharacterSO data;

    private SpriteRenderer spriteRenderer;
    private CharacterAbility[] characterAbilities;
    [SerializeField]
    [ReadOnly]
    private List<string> log;
    public GameObject noExit;
    public AIBrain ai;
    public Dictionary<Condition, float> stats;

    private bool isAbilityCache = false;
    [SerializeField]
    [ReadOnly]
    private LifecycleState lifecycleState = LifecycleState.None;

    public Action<Dictionary<Condition, float>> OnStatChange;

    public State state;

    private Facing facing;
    public CharacterType characterType;

    public LifecycleState CurrentLifecycleState => lifecycleState;
    public bool CanRunAi => data != null && lifecycleState == LifecycleState.Active;
    public IReadOnlyList<string> Log => log;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ai = GetComponent<AIBrain>();
        CacheAbility();
        log = new List<string>();
        stats = new Dictionary<Condition, float>()
        {
             { Condition.SLEEP,     5f},
             { Condition.HUNGER,    100f},
             { Condition.FUN,       5f},
             { Condition.MOOD,      5f},
        };
    }
    void Start()
    {
        state = State.DECIDE;
        if(data != null)
        {
            Initialization(data);
            if (lifecycleState == LifecycleState.None)
            {
                SetLifecycleState(LifecycleState.Active);
            }
            StartCoroutine(SnapToWalkableGridWhenReady());
        }
        StartCoroutine(ChangeStatByTick());
    }
    public IEnumerator ChangeStatByTick()
    {
        while (true)
        {
            ChangesStat(Condition.HUNGER, -5);
            yield return new WaitForSeconds(5f);
        }
    }

    public void ChangesStat(Condition condition, float value)
    {
        stats[condition] = Mathf.Clamp(stats[condition] + value,0,100);
        OnStatChange?.Invoke(stats);
    }
    void Update()
    {
        if (!CanRunAi)
        {
            return;
        }

        if(ai != null && ai.isBestActionEnd)
        {
            if (!ai.DecideAction() || ai.bestAction == null || ai.bestAction.actionset == null)
            {
                return;
            }

            string actionName = ai.bestAction.actionset.actionName;
            ai.bestAction.actionset.Execute(this);
            AddLog($"{gameObject.name}이 {actionName} 행동을 시작하였습니다");
        }
    }

    public List<BuildableObject> GetReachableBuilding()
    {
        Grid grid = GridSystemManager.Instance.grid;
        if (grid == null) return new List<BuildableObject>();

        GridPathSearchResult searchResult = ai != null ? ai.GetPathSearch(this) : null;
        if (searchResult != null)
        {
            return searchResult.GetAllVisitableBuilding();
        }

        Vector2Int pos = GetNowXY();
        return grid.GetAllVisitableBuilding(pos).ToList();
    }
    public void ChangeLayer(string layer)
    {
        spriteRenderer.sortingLayerName = layer;
    }
    public void CacheAbility()
    {
        if (isAbilityCache) return;
        characterAbilities = GetComponents<CharacterAbility>();
        isAbilityCache = true;
    }
    public T GetAbility<T>() where T : CharacterAbility
    {
        CacheAbility();
        foreach (CharacterAbility ability in characterAbilities)
        {
            if(ability is T characterAbility)
            {
                return characterAbility;
            }
        }
        Debug.Log($"{gameObject.name} : 찾는 어빌리티가 없음 {typeof(T)}");
        return null;
    }

    public bool TryGetAbility<T>(out T result) where T : CharacterAbility
    {
        CacheAbility();
        foreach (CharacterAbility ability in characterAbilities)
        {
            if (ability is T characterAbility)
            {
                result = characterAbility;
                return true;
            }
        }

        result = null;
        return false;
    }

    public void Initialization(CharacterSO data)
    {
        this.data = data;
        spriteRenderer.sprite = data.characterSprite;
        characterType = data.characterType;
        CacheAbility();
        foreach (CharacterAbility ability in characterAbilities)
        {
            ability.Initializtion(data);
        }
    }

    public void SetAiPaused(bool value)
    {
        SetLifecycleState(value ? LifecycleState.EnteringDungeon : LifecycleState.Active);
    }

    public bool IsAiPaused()
    {
        return !CanRunAi;
    }

    public void SetLifecycleState(LifecycleState nextState)
    {
        lifecycleState = nextState;
        if (ai == null) return;

        ai.bestAction = null;
        ai.isExecuted = false;
        ai.isBestActionEnd = nextState == LifecycleState.Active;
        ai.ClearPathSearchCache();
    }

    public void AddLog(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        log ??= new List<string>();
        log.Add(message);
        const int maxLogCount = 80;
        if (log.Count > maxLogCount)
        {
            log.RemoveRange(0, log.Count - maxLogCount);
        }
    }

    public Vector2Int GetNowXY()
    {
        Vector2Int startPos = GridSystemManager.Instance.grid.GetXY(transform.position);
        startPos = GridSystemManager.Instance.grid.IsValidGridPos(startPos) ? startPos : Vector2Int.zero;
        return startPos;
    }

    private IEnumerator SnapToWalkableGridWhenReady()
    {
        for (int i = 0; i < 30; i++)
        {
            if (lifecycleState == LifecycleState.SpawningOutside
                || lifecycleState == LifecycleState.EnteringDungeon
                || lifecycleState == LifecycleState.ExitingDungeon
                || lifecycleState == LifecycleState.Despawned)
            {
                yield break;
            }

            Grid grid = GridSystemManager.Instance != null ? GridSystemManager.Instance.grid : null;
            if (grid != null)
            {
                Vector2Int currentPos = grid.GetXY(transform.position);
                if (grid.IsValidGridPos(currentPos) && grid.IsWalkable(currentPos))
                {
                    yield break;
                }

                if (grid.TryFindNearestWalkablePosition(currentPos, out Vector2Int walkablePosition))
                {
                    transform.position = grid.GetWorldPos(walkablePosition);
                    ai?.ClearPathSearchCache();
                    yield break;
                }
            }

            yield return null;
        }
    }
    public void DoFade(float alpha,float duration)
    {
        spriteRenderer.DOFade(alpha,duration);
    }
    public void Flip(Facing facing)
    {
        if (facing == Facing.RIGHT)
        {
            transform.localScale = new Vector3(-1,1,1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }
    private void OnMouseDown()
    {
        if (GridSystemManager.Instance.Mode != GridMode.None) return;
        InfoFeedEvent.Trigger(this);
    }

    public InfoFeedEvent.Type GetInfoType()
    {
        return InfoFeedEvent.Type.CHARACTER;
    }
}
