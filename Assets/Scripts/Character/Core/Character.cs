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

[DrawWithUnity]
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
        OnExpedition,
        Despawned
    }

    public CharacterSO data;

    [SerializeField] private Transform visualRoot;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool autoAlignVisualToFeet = true;
    private CharacterAbility[] characterAbilities;
    [SerializeField]
    [ReadOnly]
    private List<string> log;
    private Dictionary<string, int> logTagCounts;
    private string lastLogTag;
    public GameObject noExit;
    public AIBrain ai;
    public Dictionary<Condition, float> stats;
    public CharacterRuntimeProfile profile { get; private set; }

    private bool isAbilityCache = false;
    [SerializeField]
    [ReadOnly]
    private float maxHealth = 100f;
    [SerializeField]
    [ReadOnly]
    private float currentHealth = 100f;
    [SerializeField]
    [ReadOnly]
    [Range(0f, 1f)]
    private float injurySeverity;
    [SerializeField]
    [ReadOnly]
    private LifecycleState lifecycleState = LifecycleState.None;

    public Action<Dictionary<Condition, float>> OnStatChange;
    public event Action<CharacterLogEntry> OnLogAdded;
    public event Action<Character, string> OnDied;

    public State state;

    private Facing facing;
    public CharacterType characterType;
    public CharacterRole Role { get; private set; } = CharacterRole.Regular;

    public LifecycleState CurrentLifecycleState => lifecycleState;
    public bool CanRunAi => data != null && lifecycleState == LifecycleState.Active;
    public bool IsOnExpedition => lifecycleState == LifecycleState.OnExpedition;
    public bool IsOwner => Role == CharacterRole.Owner;
    public bool CanLeaveByDissatisfaction => !IsOwner;
    public bool CanRebel => !IsOwner;
    public bool IsDead => currentHealth <= 0f || lifecycleState == LifecycleState.Despawned;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float InjurySeverity => injurySeverity;
    public IReadOnlyList<string> Log => log;
    public string SpeciesTag => profile != null ? profile.SpeciesTag : data != null ? data.SpeciesTag : string.Empty;
    public Transform VisualRoot => visualRoot;
    public SpriteRenderer VisualRenderer => spriteRenderer;

    private void Awake()
    {
        EnsureRuntimeState();
        CacheAbility();
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
        if (CharacterAiScheduler.IsDrivingAi) return;

        TryRunAiDecision();
    }

    public bool IsAiDecisionPending => CanRunAi && ai != null && ai.isBestActionEnd;

    public bool TryRunAiDecision()
    {
        if (!IsAiDecisionPending)
        {
            return false;
        }

        if (!ai.DecideAction() || ai.bestAction == null || ai.bestAction.actionset == null)
        {
            return false;
        }

        string actionName = ai.bestAction.actionset.actionName;
        ai.NotifyActionStarted();
        ai.bestAction.actionset.Execute(this);
        AddLog($"{gameObject.name}이 {actionName} 행동을 시작하였습니다");
        return true;
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
        EnsureVisualReferences();
        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.sortingLayerName = layer;
        }
    }
    public void CacheAbility()
    {
        if (isAbilityCache) return;
        RefreshAbilityCache();
    }

    public void RefreshAbilityCache()
    {
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
        EnsureRuntimeState();
        this.data = data;
        profile = data != null ? data.CreateRuntimeProfile() : null;
        if (data != null)
        {
            SetCharacterSprite(data.characterSprite);
        }
        characterType = data != null ? data.characterType : CharacterType.Customer;
        Role = data != null ? data.role : CharacterRole.Regular;
        RecalculateVitals(resetCurrentHealth: true);
        CacheAbility();
        foreach (CharacterAbility ability in characterAbilities)
        {
            ability.Initializtion(data);
        }
    }

    private void EnsureRuntimeState()
    {
        EnsureVisualReferences();

        if (ai == null)
        {
            ai = GetComponent<AIBrain>();
        }

        log ??= new List<string>();
        logTagCounts ??= new Dictionary<string, int>();
        EnsureStats();
        EnsureFeedbackBubble();
    }

    private void EnsureVisualReferences()
    {
        if (visualRoot == null)
        {
            visualRoot = transform.Find("Visual");
        }

        SpriteRenderer rootRenderer = GetComponent<SpriteRenderer>();
        if ((visualRoot == null || visualRoot == transform) && rootRenderer != null)
        {
            visualRoot = CreateVisualRoot();
        }

        if (visualRoot != null)
        {
            SpriteRenderer visualRenderer = visualRoot.GetComponent<SpriteRenderer>();
            if (visualRenderer == null && rootRenderer != null)
            {
                visualRenderer = visualRoot.gameObject.AddComponent<SpriteRenderer>();
                CopySpriteRenderer(rootRenderer, visualRenderer);
            }

            if (rootRenderer != null && rootRenderer != visualRenderer)
            {
                if (visualRenderer != null && visualRenderer.sprite == null)
                {
                    CopySpriteRenderer(rootRenderer, visualRenderer);
                }

                RemoveRootSpriteRenderer(rootRenderer);
            }

            spriteRenderer = visualRenderer;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }

        if (visualRoot == null && spriteRenderer != null)
        {
            visualRoot = spriteRenderer.transform;
        }

        ApplyVisualFootAnchor();
    }

    private Transform CreateVisualRoot()
    {
        GameObject visualObject = new GameObject("Visual");
        Transform visual = visualObject.transform;
        visual.SetParent(transform, false);
        visual.localPosition = Vector3.zero;
        visual.localRotation = Quaternion.identity;
        visual.localScale = Vector3.one;
        return visual;
    }

    private static void CopySpriteRenderer(SpriteRenderer source, SpriteRenderer target)
    {
        if (source == null || target == null)
        {
            return;
        }

        target.sprite = source.sprite;
        target.color = source.color;
        target.sharedMaterials = source.sharedMaterials;
        target.sortingLayerID = source.sortingLayerID;
        target.sortingOrder = source.sortingOrder;
        target.maskInteraction = source.maskInteraction;
        target.flipX = source.flipX;
        target.flipY = source.flipY;
        target.drawMode = SpriteDrawMode.Simple;
        target.size = source.sprite != null ? (Vector2)source.sprite.bounds.size : Vector2.one;
    }

    private static void RemoveRootSpriteRenderer(SpriteRenderer rootRenderer)
    {
        if (rootRenderer == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(rootRenderer);
            return;
        }

        DestroyImmediate(rootRenderer);
    }

    private void SetCharacterSprite(Sprite sprite)
    {
        EnsureVisualReferences();
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.sprite = sprite;
        ApplyVisualFootAnchor();
    }

    public void ApplyVisualFootAnchor()
    {
        if (!autoAlignVisualToFeet
            || visualRoot == null
            || visualRoot == transform
            || spriteRenderer == null
            || spriteRenderer.sprite == null)
        {
            return;
        }

        Vector3 localPosition = visualRoot.localPosition;
        localPosition.y = -spriteRenderer.sprite.bounds.min.y;
        visualRoot.localPosition = localPosition;
    }

    public float GetVisualTopLocalY()
    {
        EnsureVisualReferences();
        if (visualRoot == null || spriteRenderer == null || spriteRenderer.sprite == null)
        {
            return 1f;
        }

        float visualY = visualRoot == transform ? 0f : visualRoot.localPosition.y;
        return visualY + spriteRenderer.sprite.bounds.max.y;
    }

    private void EnsureStats()
    {
        stats ??= new Dictionary<Condition, float>();
        EnsureStat(Condition.SLEEP, 5f);
        EnsureStat(Condition.HUNGER, 100f);
        EnsureStat(Condition.FUN, 5f);
        EnsureStat(Condition.MOOD, 5f);
    }

    private void EnsureStat(Condition condition, float defaultValue)
    {
        if (!stats.ContainsKey(condition))
        {
            stats[condition] = defaultValue;
        }
    }

    public int GetCharacterStat(CharacterStatType statType)
    {
        return profile != null ? profile.GetStat(statType) : 5;
    }

    public float GetMoveSpeed()
    {
        float baseSpeed = data != null ? data.moveSpeed : 1f;
        return baseSpeed
            * (profile != null ? profile.GetMoveSpeedMultiplier() : 1f)
            * GetFatigueEfficiencyMultiplier()
            * GetInjuryEfficiencyMultiplier();
    }

    public float GetConsumptionMultiplier()
    {
        return profile != null ? profile.GetConsumptionMultiplier() : 1f;
    }

    public float GetStayDurationMultiplier()
    {
        return profile != null ? profile.GetStayDurationMultiplier() : 1f;
    }

    public float GetCrowdSensitivityMultiplier()
    {
        return profile != null ? profile.GetCrowdSensitivityMultiplier() : 1f;
    }

    public float GetWorkSpeedMultiplier(FacilityWorkType workTypes)
    {
        float discontentMultiplier = StaffDiscontentRuntime.Instance != null
            ? StaffDiscontentRuntime.Instance.GetWorkEfficiencyMultiplier(this)
            : 1f;
        return (profile != null ? profile.GetWorkSpeedMultiplier(workTypes) : 1f)
            * GetFatigueEfficiencyMultiplier()
            * GetInjuryEfficiencyMultiplier()
            * discontentMultiplier;
    }

    public float GetWorkPreferenceScore(FacilityWorkType workTypes)
    {
        return profile != null ? profile.GetWorkPreferenceScore(workTypes) : 0.5f;
    }

    public float GetFacilityPreferenceScore(FacilityRole roles)
    {
        return profile != null ? profile.GetFacilityPreferenceScore(roles) : 0.5f;
    }

    public float GetAccidentChanceMultiplier()
    {
        return profile != null ? profile.GetAccidentChanceMultiplier() : 1f;
    }

    public CharacterSpeciesIncidentType GetIncidentType()
    {
        return profile != null ? profile.GetIncidentType() : CharacterSpeciesIncidentType.None;
    }

    public float GetCombatPowerMultiplier()
    {
        return (profile != null ? profile.GetCombatPowerMultiplier() : 1f)
            * GetInjuryEfficiencyMultiplier();
    }

    public float GetFatigueEfficiencyMultiplier()
    {
        if (stats == null || !stats.TryGetValue(Condition.SLEEP, out float sleep))
        {
            return 1f;
        }

        return Mathf.Lerp(0.65f, 1f, Mathf.Clamp01(sleep / 100f));
    }

    public float GetInjuryEfficiencyMultiplier()
    {
        return Mathf.Lerp(1f, 0.45f, Mathf.Clamp01(injurySeverity));
    }

    public void ApplyDamage(float amount, string reason = "")
    {
        if (amount <= 0f || IsDead) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        injurySeverity = Mathf.Clamp01(1f - (currentHealth / Mathf.Max(1f, maxHealth)));
        AddLog(string.IsNullOrWhiteSpace(reason)
            ? $"피해 {amount:0.#}"
            : $"피해 {amount:0.#}: {reason}");

        if (currentHealth <= 0f)
        {
            Die(reason);
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || IsDead) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        injurySeverity = Mathf.Clamp01(1f - (currentHealth / Mathf.Max(1f, maxHealth)));
        AddLog($"회복 {amount:0.#}");
    }

    public void SetInjurySeverity(float value)
    {
        injurySeverity = Mathf.Clamp01(value);
        currentHealth = Mathf.Clamp(maxHealth * (1f - injurySeverity), 1f, maxHealth);
        AddLog($"부상도 변경: {Mathf.RoundToInt(injurySeverity * 100f)}%");
    }

    public void Die(string reason = "")
    {
        if (lifecycleState == LifecycleState.Despawned) return;

        SetRenderersVisible(true);
        currentHealth = 0f;
        injurySeverity = 1f;
        AddLog(string.IsNullOrWhiteSpace(reason) ? "사망" : $"사망: {reason}");
        SetLifecycleState(LifecycleState.Despawned);
        OnDied?.Invoke(this, reason);
        CharacterDeathEvent.Trigger(this, reason);

        if (IsOwner)
        {
            OwnerRunManager.Instance.HandleOwnerDeath(this, reason);
        }
    }

    public string GetSpeciesShortDescription()
    {
        return profile != null ? profile.GetShortDescription() : string.Empty;
    }

    public void SetAiPaused(bool value)
    {
        SetLifecycleState(value ? LifecycleState.EnteringDungeon : LifecycleState.Active);
    }

    public bool IsAiPaused()
    {
        return !CanRunAi;
    }

    public bool BeginExpedition()
    {
        if (IsDead || IsOwner || IsOnExpedition)
        {
            return false;
        }

        if (TryGetAbility(out AbilityWork work))
        {
            work.PrepareForExpedition();
        }

        AddLog("원정 출발");
        SetLifecycleState(LifecycleState.OnExpedition);
        SetRenderersVisible(false);
        return true;
    }

    public void EndExpedition(bool alive = true)
    {
        if (alive && !IsDead)
        {
            SetRenderersVisible(true);
            SetLifecycleState(LifecycleState.Active);
            AddLog("원정 복귀");
            return;
        }

        SetRenderersVisible(true);
        Die("원정 중 사망");
    }

    public void SetLifecycleState(LifecycleState nextState)
    {
        lifecycleState = nextState;
        if (ai == null) return;

        if (nextState == LifecycleState.Active)
        {
            ai.RequestImmediateReplan(clearFailures: true);
            return;
        }

        ai.bestAction = null;
        ai.isExecuted = false;
        ai.isBestActionEnd = false;
        ai.ClearPathSearchCache();
    }

    public void AddLog(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        log ??= new List<string>();
        logTagCounts ??= new Dictionary<string, int>();
        string tag = CharacterLogUtility.ToCauseTag(message);
        int count = logTagCounts.TryGetValue(tag, out int currentCount)
            ? currentCount + 1
            : 1;
        logTagCounts[tag] = count;

        string line = count > 1 ? $"{tag} x{count}" : tag;
        if (lastLogTag == tag && log.Count > 0)
        {
            log[log.Count - 1] = line;
        }
        else
        {
            log.Add(line);
            lastLogTag = tag;
        }

        const int maxLogCount = 80;
        if (log.Count > maxLogCount)
        {
            log.RemoveRange(0, log.Count - maxLogCount);
        }

        OnLogAdded?.Invoke(new CharacterLogEntry(tag, line, count, message));
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
                || lifecycleState == LifecycleState.OnExpedition
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

    private void OnEnable()
    {
        CharacterAiScheduler.Register(this);
    }

    private void OnDisable()
    {
        CharacterAiScheduler.Unregister(this);
    }

    private void RecalculateVitals(bool resetCurrentHealth)
    {
        int toughness = GetCharacterStat(CharacterStatType.Toughness);
        int endurance = GetCharacterStat(CharacterStatType.Endurance);
        maxHealth = 60f + (toughness * 8f) + (endurance * 4f);
        if (IsOwner && MetaProgressionRuntime.Instance != null)
        {
            maxHealth *= MetaProgressionRuntime.Instance.GetOwnerMaxHealthMultiplier();
        }

        if (resetCurrentHealth || currentHealth <= 0f)
        {
            currentHealth = maxHealth;
            injurySeverity = 0f;
        }
        else
        {
            currentHealth = Mathf.Clamp(currentHealth, 1f, maxHealth);
            injurySeverity = Mathf.Clamp01(1f - (currentHealth / Mathf.Max(1f, maxHealth)));
        }
    }
    public void DoFade(float alpha,float duration)
    {
        EnsureVisualReferences();
        if (spriteRenderer != null)
        {
            spriteRenderer.DOFade(alpha,duration);
        }
    }
    public void Flip(Facing facing)
    {
        this.facing = facing;
        EnsureVisualReferences();
        Vector3 rootScale = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(rootScale.x), rootScale.y, rootScale.z);

        if (visualRoot != null && visualRoot != transform)
        {
            Vector3 visualScale = visualRoot.localScale;
            visualRoot.localScale = new Vector3(Mathf.Abs(visualScale.x), visualScale.y, visualScale.z);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = facing == Facing.RIGHT;
        }
    }
    private void OnMouseDown()
    {
        if (GridSystemManager.Instance.Mode != GridMode.None) return;
        InfoFeedEvent.Trigger(this);
    }

    private void EnsureFeedbackBubble()
    {
        if (GetComponent<CharacterFeedbackBubble>() == null)
        {
            gameObject.AddComponent<CharacterFeedbackBubble>();
        }
    }

    private void SetRenderersVisible(bool value)
    {
        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.enabled = value;
        }
    }

    public InfoFeedEvent.Type GetInfoType()
    {
        return InfoFeedEvent.Type.CHARACTER;
    }
}

public struct CharacterDeathEvent
{
    public Character Character;
    public string Reason;

    public CharacterDeathEvent(Character character, string reason)
    {
        Character = character;
        Reason = reason;
    }

    private static CharacterDeathEvent e;

    public static void Trigger(Character character, string reason)
    {
        e.Character = character;
        e.Reason = reason;
        EventObserver.TriggerEvent(e);
    }
}

public readonly struct CharacterLogEntry
{
    public string Tag { get; }
    public string DisplayLine { get; }
    public int Count { get; }
    public string OriginalMessage { get; }

    public CharacterLogEntry(string tag, string displayLine, int count, string originalMessage)
    {
        Tag = tag;
        DisplayLine = displayLine;
        Count = count;
        OriginalMessage = originalMessage;
    }
}

public static class CharacterLogUtility
{
    public static string ToCauseTag(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "기록 없음";
        }

        string normalized = message.Trim();
        if (ContainsAny(normalized, "작업 진행", "작업 종료"))
        {
            return normalized;
        }
        if (ContainsAny(normalized, "재고 없음", "재고 부족", "창고 재고 부족", "보충 실패"))
        {
            return "재고 부족";
        }

        if (ContainsAny(normalized, "길 막힘", "목적지 없음", "도달 실패", "이동 정보 없음", "경로"))
        {
            return "길 막힘";
        }

        if (ContainsAny(normalized, "수용 인원", "혼잡"))
        {
            return "혼잡함";
        }

        if (ContainsAny(normalized, "돈 부족", "자금 부족", "가격"))
        {
            return "돈 부족";
        }

        if (ContainsAny(normalized, "시설 파손", "파손"))
        {
            return "시설 파손";
        }

        if (ContainsAny(normalized, "피로", "비번", "휴식"))
        {
            return "피로";
        }

        if (ContainsAny(normalized, "분노", "사고", "사망"))
        {
            return "위험";
        }

        if (ContainsAny(normalized, "완료", "회복", "근무 복귀"))
        {
            return "만족";
        }

        if (ContainsAny(normalized, "작업 시작", "행동을 시작"))
        {
            return "행동 시작";
        }

        string extracted = ExtractReasonAfterSeparator(normalized);
        return string.IsNullOrWhiteSpace(extracted) ? normalized : extracted;
    }

    private static bool ContainsAny(string value, params string[] patterns)
    {
        return patterns.Any((pattern) => value.Contains(pattern, StringComparison.Ordinal));
    }

    private static string ExtractReasonAfterSeparator(string value)
    {
        int colonIndex = value.LastIndexOf(':');
        if (colonIndex >= 0 && colonIndex + 1 < value.Length)
        {
            return value[(colonIndex + 1)..].Trim();
        }

        int dashIndex = value.LastIndexOf(" - ", StringComparison.Ordinal);
        if (dashIndex >= 0 && dashIndex + 3 < value.Length)
        {
            return value[(dashIndex + 3)..].Trim();
        }

        return value;
    }
}
