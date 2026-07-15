using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

[DrawWithUnity]
[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterIdentity))]
[RequireComponent(typeof(CharacterAbilityCache))]
[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(CharacterVisual))]
[RequireComponent(typeof(CharacterLifecycle))]
[RequireComponent(typeof(CharacterLog))]
[RequireComponent(typeof(CharacterBlackboard))]
[RequireComponent(typeof(CustomerPersonaRuntime))]
[RequireComponent(typeof(CharacterDialogueRuntime))]
[RequireComponent(typeof(CharacterSocialMemory))]
[RequireComponent(typeof(BehaviorTree))]
public class CharacterActor : SerializedMonoBehaviour, IInfoable
{
    public GameObject noExit;

    private AIBrain brain;
    [SerializeField]
    [ReadOnly]
    private CharacterIdentity identity;
    [SerializeField]
    [ReadOnly]
    private CharacterAbilityCache abilityCache;
    [SerializeField]
    [ReadOnly]
    private CharacterStats characterStats;
    [SerializeField]
    [ReadOnly]
    private CharacterVisual visual;
    [SerializeField]
    [ReadOnly]
    private CharacterLifecycle lifecycle;
    [SerializeField]
    [ReadOnly]
    private CharacterLog characterLog;
    [SerializeField]
    [ReadOnly]
    private CharacterBlackboard blackboard;
    [SerializeField]
    [ReadOnly]
    private CustomerPersonaRuntime personaRuntime;
    [SerializeField]
    [ReadOnly]
    private CharacterDialogueRuntime dialogueRuntime;
    [SerializeField]
    [ReadOnly]
    private CharacterSocialMemory socialMemory;
    [SerializeField]
    [ReadOnly]
    private BehaviorTree behaviorTree;
    private IGridSystemProvider gridSystemProvider;
    private ICharacterAiSchedulingService aiSchedulingService;
    private IWorldInfoClickSelector worldInfoClickSelector;
    private ICharacterSocialMemoryFactory socialMemoryFactory;
    private ICharacterFeedbackBubbleFactory feedbackBubbleFactory;
    private bool registeredWithAiScheduler;

    public CharacterDecisionState State { get; private set; }
    public CharacterDecisionState state
    {
        get => State;
        set => State = value;
    }
    public AIBrain Brain => brain;
    public AIBrain ai => brain;
    public CharacterIdentity Identity => identity;
    public CharacterStats Stats => characterStats;
    public CharacterAbilityCache AbilityCache => abilityCache;
    public CharacterLifecycle Lifecycle => lifecycle;
    public CharacterLog LogComponent => characterLog;
    public CharacterBlackboard Blackboard => blackboard;
    public CustomerPersonaRuntime PersonaRuntime => personaRuntime;
    public CharacterDialogueRuntime DialogueRuntime => dialogueRuntime;
    public CharacterSocialMemory SocialMemory => socialMemory;
    public BehaviorTree BehaviorTree => behaviorTree;
    public CharacterRuntimeProfile profile => identity != null ? identity.Profile : null;
    public CharacterRole Role => identity != null ? identity.Role : CharacterRole.Regular;
    public bool IsOwner => identity != null && identity.IsOwner;
    public bool CanLeaveByDissatisfaction => !IsOwner;
    public bool CanRebel => !IsOwner;
    public bool IsDead => CurrentLifecycleState == CharacterLifecycleState.Despawned
        || (characterStats != null && characterStats.IsDead);
    public bool IsOnExpedition => lifecycle != null
        && lifecycle.CurrentState == CharacterLifecycleState.OnExpedition;
    public CharacterLifecycleState CurrentLifecycleState => lifecycle != null
        ? lifecycle.CurrentState
        : CharacterLifecycleState.None;
    public bool CanRunAi => identity != null
        && identity.Data != null
        && lifecycle != null
        && lifecycle.CurrentState == CharacterLifecycleState.Active;
    public bool IsAiDecisionPending => CanRunAi && brain != null && brain.isBestActionEnd;
    public float InjurySeverity => characterStats != null ? characterStats.InjurySeverity : 0f;
    public CharacterMoodSnapshot Mood => characterStats != null
        ? characterStats.GetMoodSnapshot()
        : new CharacterMoodSnapshot(
            CharacterMoodRules.DefaultBaseMood,
            CharacterMoodRules.DefaultBaseMood,
            Array.Empty<CharacterMoodFactorSnapshot>());
    public IReadOnlyList<string> Log
    {
        get
        {
            EnsureRuntimeState();
            return characterLog != null ? characterLog.Entries : Array.Empty<string>();
        }
    }
    public string SpeciesTag => identity != null ? identity.SpeciesTag : string.Empty;
    public Transform VisualRoot => visual != null ? visual.VisualRoot : null;
    public SpriteRenderer VisualRenderer => visual != null ? visual.VisualRenderer : null;
    public event Action<CharacterActor, string> OnDied;

    [Inject]
    public void ConstructCharacterActor(
        IGridSystemProvider gridSystemProvider,
        ICharacterAiSchedulingService aiSchedulingService,
        IWorldInfoClickSelector worldInfoClickSelector,
        ICharacterSocialMemoryFactory socialMemoryFactory,
        ICharacterFeedbackBubbleFactory feedbackBubbleFactory)
    {
        this.gridSystemProvider = gridSystemProvider
            ?? throw new ArgumentNullException(nameof(gridSystemProvider));
        this.aiSchedulingService = aiSchedulingService
            ?? throw new ArgumentNullException(nameof(aiSchedulingService));
        this.worldInfoClickSelector = worldInfoClickSelector
            ?? throw new ArgumentNullException(nameof(worldInfoClickSelector));
        this.socialMemoryFactory = socialMemoryFactory
            ?? throw new ArgumentNullException(nameof(socialMemoryFactory));
        this.feedbackBubbleFactory = feedbackBubbleFactory
            ?? throw new ArgumentNullException(nameof(feedbackBubbleFactory));
        EnsureSocialMemory();
        EnsureFeedbackBubbleIfInjected();
        RegisterWithAiSchedulerIfReady();
    }

    public event Action<Dictionary<CharacterCondition, float>> OnStatChange
    {
        add
        {
            EnsureRuntimeState();
            if (characterStats != null)
            {
                characterStats.OnStatChange += value;
            }
        }
        remove
        {
            if (characterStats != null)
            {
                characterStats.OnStatChange -= value;
            }
        }
    }
    public event Action<CharacterLogEntry> OnLogAdded
    {
        add
        {
            EnsureRuntimeState();
            if (characterLog != null)
            {
                characterLog.OnLogAdded += value;
            }
        }
        remove
        {
            if (characterLog != null)
            {
                characterLog.OnLogAdded -= value;
            }
        }
    }

    public Dictionary<CharacterCondition, float> stats
    {
        get
        {
            EnsureRuntimeState();
            return characterStats != null ? characterStats.Stats : null;
        }
        set
        {
            EnsureRuntimeState();
            if (characterStats != null)
            {
                characterStats.Stats = value;
            }
        }
    }

    public CharacterSO data
    {
        get
        {
            EnsureRuntimeState();
            return identity != null ? identity.Data : null;
        }
        set
        {
            EnsureRuntimeState();
            identity?.SetData(value);
        }
    }

    public CharacterType characterType
    {
        get
        {
            EnsureRuntimeState();
            return identity != null ? identity.CharacterType : CharacterType.Customer;
        }
        set
        {
            EnsureRuntimeState();
            identity?.SetCharacterType(value);
        }
    }

    public static CharacterActor From(Component component)
    {
        return component != null ? component.GetComponent<CharacterActor>() : null;
    }

    private void Awake()
    {
        EnsureRuntimeState();
        abilityCache?.CacheAbility();
    }

    private void Start()
    {
        EnsureRuntimeState();
        RegisterWithAiSchedulerRequired();
        State = CharacterDecisionState.DECIDE;
        if (identity != null && identity.Data != null)
        {
            Initialize(identity.Data);
            if (lifecycle != null && lifecycle.CurrentState == CharacterLifecycleState.None)
            {
                lifecycle.SetLifecycleState(CharacterLifecycleState.Active);
            }

            StartCoroutine(lifecycle != null ? lifecycle.SnapToWalkableGridWhenReady() : EmptyRoutine());
        }

        StartCoroutine(characterStats != null ? characterStats.ChangeStatByTick() : EmptyRoutine());
    }

    private void Update()
    {
        visual?.RecoverExpiredTraversalVisibility();
    }

    private void OnEnable()
    {
        RegisterWithAiSchedulerIfReady();
    }

    private void OnDisable()
    {
        visual?.RestoreTraversalVisibility();
        UnregisterFromAiScheduler();
    }

    private void OnMouseDown()
    {
        RequireWorldInfoClickSelector().TryHandleWorldInfoClick();
    }

    public void Initialize(CharacterSO data)
    {
        EnsureRuntimeState();
        if (!HasRuntimeComponents)
        {
            return;
        }

        identity.SetData(data);
        if (identity.Data != null)
        {
            visual.SetCharacterSprite(identity.Data.characterSprite);
        }

        characterStats.RecalculateVitals(resetCurrentHealth: true);
        abilityCache.CacheAbility();
        foreach (CharacterAbility ability in abilityCache.Abilities)
        {
            ability.Initializtion(data);
        }

        personaRuntime.RequestPersonaIfNeeded(logIfMissingQueue: false);
    }

    public bool TryExecuteSelectedAiAction()
    {
        EnsureRuntimeState();
        if (brain == null || brain.bestAction == null || brain.bestAction.actionset == null)
        {
            return false;
        }

        AIAction selectedAction = brain.bestAction;
        string actionName = !string.IsNullOrWhiteSpace(selectedAction.actionset.actionName)
            ? selectedAction.actionset.actionName
            : selectedAction.actionset.GetType().Name;
        brain.NotifyActionStarted();
        blackboard?.Commit(selectedAction, actionName);
        selectedAction.actionset.Execute(this);
        return true;
    }

    public List<BuildableObject> GetReachableBuilding()
    {
        EnsureRuntimeState();
        if (!TryGetGrid(out Grid grid))
        {
            return new List<BuildableObject>();
        }

        GridPathSearchResult searchResult = brain != null
            ? brain.GetPathSearch(this)
            : null;
        if (searchResult != null)
        {
            return searchResult.GetAllVisitableBuilding();
        }

        Vector2Int pos = lifecycle != null ? lifecycle.GetNowXY() : Vector2Int.zero;
        return grid.GetAllVisitableBuilding(pos).ToList();
    }

    private bool TryGetGrid(out Grid grid)
    {
        if (gridSystemProvider == null)
        {
            grid = null;
            return false;
        }

        return gridSystemProvider.TryGetGrid(out grid);
    }

    public void EnsureRuntimeState()
    {
        if (brain == null)
        {
            brain = GetComponent<AIBrain>();
        }

        identity = GetComponent<CharacterIdentity>();
        abilityCache = GetComponent<CharacterAbilityCache>();
        characterStats = GetComponent<CharacterStats>();
        visual = GetComponent<CharacterVisual>();
        lifecycle = GetComponent<CharacterLifecycle>();
        characterLog = GetComponent<CharacterLog>();
        blackboard = GetComponent<CharacterBlackboard>();
        personaRuntime = GetComponent<CustomerPersonaRuntime>();
        dialogueRuntime = GetComponent<CharacterDialogueRuntime>();
        EnsureSocialMemory();

        behaviorTree = GetComponent<BehaviorTree>();
        if (behaviorTree != null)
        {
            behaviorTree.StartWhenEnabled = false;
        }

        if (!HasRuntimeComponents)
        {
            Debug.LogError($"{name}: CharacterActor component split is incomplete. Fix the prefab components.", this);
            return;
        }

        identity.Bind(this);
        characterStats.Bind(this);
        lifecycle.Bind(this);
        blackboard.Bind(this);
        personaRuntime.Bind(this);
        socialMemory.Bind(this);

        visual.Bind();
        characterLog.Bind();
        EnsureFeedbackBubbleIfInjected();
    }

    public InfoFeedEvent.Type GetInfoType()
    {
        return InfoFeedEvent.Type.CHARACTER;
    }

    public T GetAbility<T>() where T : CharacterAbility
    {
        EnsureRuntimeState();
        return abilityCache != null ? abilityCache.GetAbility<T>() : null;
    }

    public bool TryGetAbility<T>(out T result) where T : CharacterAbility
    {
        EnsureRuntimeState();
        if (abilityCache != null)
        {
            return abilityCache.TryGetAbility(out result);
        }

        result = null;
        return false;
    }

    public Vector2Int GetNowXY()
    {
        EnsureRuntimeState();
        return lifecycle != null ? lifecycle.GetNowXY() : Vector2Int.zero;
    }

    public void AddLog(string message)
    {
        EnsureRuntimeState();
        characterLog?.AddLog(message);
    }

    public float GetMoveSpeed()
    {
        EnsureRuntimeState();
        return characterStats != null ? characterStats.GetMoveSpeed() : identity != null && identity.Data != null ? identity.Data.moveSpeed : 1f;
    }

    public float GetConsumptionMultiplier()
    {
        EnsureRuntimeState();
        return characterStats != null ? characterStats.GetConsumptionMultiplier() : 1f;
    }

    public float GetStayDurationMultiplier()
    {
        EnsureRuntimeState();
        return characterStats != null ? characterStats.GetStayDurationMultiplier() : 1f;
    }

    public float GetCrowdSensitivityMultiplier()
    {
        EnsureRuntimeState();
        return characterStats != null ? characterStats.GetCrowdSensitivityMultiplier() : 1f;
    }

    public float GetWorkSpeedMultiplier(FacilityWorkType workTypes)
    {
        EnsureRuntimeState();
        return characterStats != null ? characterStats.GetWorkSpeedMultiplier(workTypes) : 1f;
    }

    public float GetWorkPreferenceScore(FacilityWorkType workTypes)
    {
        EnsureRuntimeState();
        return characterStats != null ? characterStats.GetWorkPreferenceScore(workTypes) : 0.5f;
    }

    public float GetFacilityPreferenceScore(FacilityRole roles)
    {
        EnsureRuntimeState();
        return characterStats != null ? characterStats.GetFacilityPreferenceScore(roles) : 0.5f;
    }

    public float GetAccidentChanceMultiplier()
    {
        EnsureRuntimeState();
        return characterStats != null ? characterStats.GetAccidentChanceMultiplier() : 1f;
    }

    public CharacterSpeciesIncidentType GetIncidentType()
    {
        EnsureRuntimeState();
        return characterStats != null ? characterStats.GetIncidentType() : CharacterSpeciesIncidentType.None;
    }

    public float GetCombatPowerMultiplier()
    {
        EnsureRuntimeState();
        return characterStats != null ? characterStats.GetCombatPowerMultiplier() : 1f;
    }

    public float GetFatigueEfficiencyMultiplier()
    {
        EnsureRuntimeState();
        return characterStats != null ? characterStats.GetFatigueEfficiencyMultiplier() : 1f;
    }

    public float GetInjuryEfficiencyMultiplier()
    {
        EnsureRuntimeState();
        return characterStats != null ? characterStats.GetInjuryEfficiencyMultiplier() : 1f;
    }

    public float MaxHealth
    {
        get
        {
            EnsureRuntimeState();
            return characterStats != null ? characterStats.MaxHealth : 100f;
        }
    }

    public float CurrentHealth
    {
        get
        {
            EnsureRuntimeState();
            return characterStats != null ? characterStats.CurrentHealth : 100f;
        }
    }

    public void Initialization(CharacterSO data)
    {
        Initialize(data);
    }

    public void CacheAbility()
    {
        EnsureRuntimeState();
        abilityCache?.CacheAbility();
    }

    public void RefreshAbilityCache()
    {
        EnsureRuntimeState();
        abilityCache?.RefreshAbilityCache();
    }

    public IEnumerator ChangeStatByTick()
    {
        EnsureRuntimeState();
        return characterStats != null ? characterStats.ChangeStatByTick() : EmptyRoutine();
    }

    public void ChangesStat(CharacterCondition condition, float value)
    {
        EnsureRuntimeState();
        characterStats?.ChangesStat(condition, value);
    }

    public void ApplyMoodFactor(
        string id,
        string label,
        float value,
        float durationSeconds = 180f,
        int maxStacks = 1)
    {
        EnsureRuntimeState();
        characterStats?.ApplyMoodFactor(id, label, value, durationSeconds, maxStacks);
    }

    public int GetCharacterStat(CharacterStatType statType)
    {
        EnsureRuntimeState();
        return characterStats != null ? characterStats.GetCharacterStat(statType) : 5;
    }

    public void ApplyDamage(float amount, string reason = "")
    {
        EnsureRuntimeState();
        characterStats?.ApplyDamage(amount, reason);
    }

    public void Heal(float amount)
    {
        EnsureRuntimeState();
        characterStats?.Heal(amount);
    }

    public void SetInjurySeverity(float value)
    {
        EnsureRuntimeState();
        characterStats?.SetInjurySeverity(value);
    }

    public void Die(string reason = "")
    {
        EnsureRuntimeState();
        characterStats?.Die(reason);
    }

    public void InitializeStats(bool resetCurrentHealth)
    {
        EnsureRuntimeState();
        characterStats?.RecalculateVitals(resetCurrentHealth);
    }

    public void SetLifecycleState(CharacterLifecycleState nextState)
    {
        EnsureRuntimeState();
        lifecycle?.SetLifecycleState(nextState);
    }

    public bool BeginExpedition()
    {
        EnsureRuntimeState();
        return lifecycle != null && lifecycle.BeginExpedition();
    }

    public void EndExpedition(bool alive = true)
    {
        EnsureRuntimeState();
        lifecycle?.EndExpedition(alive);
    }

    public void ChangeLayer(string layer)
    {
        EnsureRuntimeState();
        visual?.ChangeLayer(layer);
    }

    public void ApplyVisualFootAnchor()
    {
        EnsureRuntimeState();
        visual?.ApplyVisualFootAnchor();
    }

    public float GetVisualTopLocalY()
    {
        EnsureRuntimeState();
        return visual != null ? visual.GetVisualTopLocalY() : 1f;
    }

    public void DoFade(float alpha, float duration)
    {
        EnsureRuntimeState();
        visual?.DoFade(alpha, duration);
    }

    public void Flip(CharacterFacing facing)
    {
        EnsureRuntimeState();
        visual?.Flip(facing);
    }

    public void HideForTraversal(float failSafeSeconds)
    {
        EnsureRuntimeState();
        visual?.HideForTraversal(failSafeSeconds);
    }

    public void RestoreTraversalVisibility()
    {
        EnsureRuntimeState();
        visual?.RestoreTraversalVisibility();
    }

    public void SetAiPaused(bool value)
    {
        EnsureRuntimeState();
        lifecycle?.SetAiPaused(value);
    }

    public bool IsAiPaused()
    {
        return !CanRunAi;
    }

    public string GetSpeciesShortDescription()
    {
        EnsureRuntimeState();
        return identity != null ? identity.GetSpeciesShortDescription() : string.Empty;
    }

    internal void RaiseDied(string reason)
    {
        OnDied?.Invoke(this, reason);
    }

    private bool HasRuntimeComponents => identity != null
        && abilityCache != null
        && characterStats != null
        && visual != null
        && lifecycle != null
        && characterLog != null
        && blackboard != null
        && personaRuntime != null
        && dialogueRuntime != null
        && socialMemory != null;

    private void EnsureFeedbackBubbleIfInjected()
    {
        if (feedbackBubbleFactory == null)
        {
            return;
        }

        feedbackBubbleFactory.GetOrAdd(this);
    }

    private void EnsureSocialMemory()
    {
        socialMemory = socialMemoryFactory != null
            ? socialMemoryFactory.GetOrAdd(this)
            : GetComponent<CharacterSocialMemory>();
    }

    private ICharacterAiSchedulingService RequireAiSchedulingService()
    {
        return aiSchedulingService
            ?? throw new InvalidOperationException($"{nameof(CharacterActor)} requires {nameof(ICharacterAiSchedulingService)} injection.");
    }

    private IWorldInfoClickSelector RequireWorldInfoClickSelector()
    {
        return worldInfoClickSelector
            ?? throw new InvalidOperationException($"{nameof(CharacterActor)} requires {nameof(IWorldInfoClickSelector)} injection.");
    }

    private void RegisterWithAiSchedulerRequired()
    {
        RequireAiSchedulingService();
        RegisterWithAiSchedulerIfReady();
    }

    private void RegisterWithAiSchedulerIfReady()
    {
        if (!isActiveAndEnabled || registeredWithAiScheduler || aiSchedulingService == null)
        {
            return;
        }

        aiSchedulingService.Register(this);
        registeredWithAiScheduler = true;
    }

    private void UnregisterFromAiScheduler()
    {
        if (!registeredWithAiScheduler)
        {
            return;
        }

        aiSchedulingService?.Unregister(this);
        registeredWithAiScheduler = false;
    }

    private static IEnumerator EmptyRoutine()
    {
        yield break;
    }
}
