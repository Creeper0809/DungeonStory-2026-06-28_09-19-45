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
using static UnityEditor.PlayerSettings;
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

    public Action<Dictionary<Condition, float>> OnStatChange;

    public State state;

    private Facing facing;
    public CharacterType characterType;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ai = GetComponent<AIBrain>();
        CacheAbility();
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
        if(ai.isBestActionEnd)
        {
            ai.DecideAction();
            ai.bestAction.SetDestination(this);
            ai.bestAction.actionset.Execute(this);
            log.Add($"{gameObject.name}이 {ai.bestAction.actionset.actionName} 행동을 시작하였습니다");
        }
    }
    public List<BuildableObject> GetReachableBuilding()
    {
        Vector2Int pos = GetNowXY();
        return GridSystemManager.Instance.grid.GetAllVisitableBuilding(pos).ToList();
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
    public Vector2Int GetNowXY()
    {
        Vector2Int startPos = GridSystemManager.Instance.grid.GetXY(transform.position);
        startPos = GridSystemManager.Instance.grid.IsValidGridPos(startPos) ? startPos : Vector2Int.zero;
        return startPos;
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
        if (GridSystemManager.Instance.gridMode.Value != GridMode.None) return;
        InfoFeedEvent.Trigger(this);
    }

    public InfoFeedEvent.Type GetInfoType()
    {
        return InfoFeedEvent.Type.CHARACTER;
    }
}
