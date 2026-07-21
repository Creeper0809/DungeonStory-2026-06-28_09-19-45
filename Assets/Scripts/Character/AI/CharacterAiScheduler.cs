using System;
using System.Collections.Generic;
using System.Diagnostics;
using BehaviorDesigner.Runtime;
using Unity.Profiling;
using UnityEngine;
using VContainer;
#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class CharacterAiScheduler : MonoBehaviour
{
    private static readonly ProfilerMarker ProcessAiBudgetMarker =
        new ProfilerMarker("CharacterAiScheduler.ProcessAiBudget");

    [SerializeField] private bool driveCharacterUpdates = true;
    [SerializeField] private bool driveBehaviorDesignerTrees = true;
    [SerializeField] private bool registerExistingSceneCharacters = true;
    [SerializeField] private ExternalBehaviorTree characterAiExternalBehavior;
    [SerializeField] private bool limitPathSearches = true;
    [SerializeField] private bool limitFeedbackToVisibleCharacters = true;
    [SerializeField] private bool adaptBudgetsToFrameCost = true;
    [SerializeField, Min(1)] private int maxDecisionsPerFrame = 16;
    [SerializeField, Min(1)] private int maxPathSearchesPerFrame = 8;
    [SerializeField, Min(1)] private int minDecisionsPerFrame = 4;
    [SerializeField, Min(1)] private int minPathSearchesPerFrame = 2;
    [SerializeField, Min(0.1f)] private float targetAiMilliseconds = 4f;
    [SerializeField, Min(0.01f)] private float ownerDecisionInterval = 0.2f;
    [SerializeField, Min(0.01f)] private float visibleDecisionInterval = 0.35f;
    [SerializeField, Min(0.01f)] private float offscreenDecisionInterval = 1.5f;
    [SerializeField, Min(0.01f)] private float retryDelay = 0.05f;
    [SerializeField, Min(0f)] private float viewportMargin = 0.15f;
    [SerializeField, Range(1, 8)] private int offscreenMovementFrameStride = 3;

    private readonly List<CharacterActor> actors = new List<CharacterActor>();
    private readonly Dictionary<CharacterActor, float> nextDecisionTime = new Dictionary<CharacterActor, float>();
    private readonly HashSet<CharacterActor> missingBehaviorTreeLogged = new HashSet<CharacterActor>();
    private readonly HashSet<CharacterActor> missingExternalBehaviorLogged = new HashSet<CharacterActor>();
    private int cursor;
    private int pathBudgetFrame = -1;
    private int pathSearchesThisFrame;
    private int currentDecisionBudget;
    private int currentPathSearchBudget;
    private int adaptiveCooldownFrames;
    private float manualTime;
    private IDungeonSceneComponentQuery sceneQuery;
    private IMainCameraProvider mainCameraProvider;
    private ICharacterBehaviorTreeRuntimeConfigurator behaviorTreeConfigurator;

    public int RegisteredCharacterCount => actors.Count;
    public int LastProcessedDecisionCount { get; private set; }
    public int LastBehaviorTreeTickCount { get; private set; }
    public int LastPathSearchCount { get; private set; }
    public double LastProcessingMilliseconds { get; private set; }
    public ExternalBehaviorTree CharacterAiExternalBehavior => characterAiExternalBehavior;
    public bool IsDrivingAi => enabled && driveCharacterUpdates;
    public int CurrentDecisionBudget => Mathf.Max(1, currentDecisionBudget);
    public int CurrentPathSearchBudget => Mathf.Max(1, currentPathSearchBudget);
    public bool IsPathBudgetActiveForDebug => enabled
        && driveCharacterUpdates
        && limitPathSearches;

    private void Awake()
    {
        ConfigureBehaviorManagerForManualTick();
    }

    private void OnEnable()
    {
        ConfigureBehaviorManagerForManualTick();
        if (registerExistingSceneCharacters)
        {
            RegisterExistingCharactersIfInjected();
        }
    }

    [Inject]
    public void Construct(
        IDungeonSceneComponentQuery sceneQuery,
        IMainCameraProvider mainCameraProvider,
        ICharacterBehaviorTreeRuntimeConfigurator behaviorTreeConfigurator)
    {
        this.sceneQuery = sceneQuery
            ?? throw new ArgumentNullException(nameof(sceneQuery));
        this.mainCameraProvider = mainCameraProvider
            ?? throw new ArgumentNullException(nameof(mainCameraProvider));
        this.behaviorTreeConfigurator = behaviorTreeConfigurator
            ?? throw new ArgumentNullException(nameof(behaviorTreeConfigurator));

        if (isActiveAndEnabled && registerExistingSceneCharacters)
        {
            RegisterExistingCharacters();
        }
    }

    private void Start()
    {
        if (registerExistingSceneCharacters)
        {
            RegisterExistingCharacters();
        }
    }

    private void Update()
    {
        ProcessAiBudget(Time.time);
    }

    public void RegisterActor(CharacterActor actor)
    {
        if (actor == null)
        {
            return;
        }

        RegisterInternal(actor);
    }

    public void UnregisterActor(CharacterActor actor)
    {
        if (actor == null)
        {
            return;
        }

        UnregisterInternal(actor);
    }

    public void RequestImmediateDecisionFor(CharacterActor actor)
    {
        if (actor == null)
        {
            return;
        }

        RegisterInternal(actor);
        nextDecisionTime[actor] = 0f;
    }

    public bool TryConsumePathSearchBudget()
    {
        if (!enabled || !driveCharacterUpdates || !limitPathSearches)
        {
            return true;
        }

        return TryConsumePathSearchBudgetInternal();
    }

    public bool ShouldShowCharacterFeedbackFor(CharacterActor actor)
    {
        if (!enabled || !limitFeedbackToVisibleCharacters)
        {
            return true;
        }

        return IsHighDetailCharacter(actor);
    }

    public int GetMovementFrameStrideFor(CharacterActor actor)
    {
        if (!enabled
            || !driveCharacterUpdates
            || offscreenMovementFrameStride <= 1
            || IsHighDetailCharacter(actor))
        {
            return 1;
        }

        return offscreenMovementFrameStride;
    }

    public void RunManualTick(float deltaTime)
    {
        manualTime += Mathf.Max(0f, deltaTime);
        ProcessAiBudget(manualTime);
    }

    public void ClearRegistrationsForDebug()
    {
        ResetAdaptiveBudgets();
        manualTime = Time.time;
        actors.Clear();
        nextDecisionTime.Clear();
        missingBehaviorTreeLogged.Clear();
        missingExternalBehaviorLogged.Clear();
        cursor = 0;
    }

    public void ResetPathSearchBudgetForDebugInstance()
    {
        pathBudgetFrame = -1;
        pathSearchesThisFrame = 0;
        LastPathSearchCount = 0;
        EnsureAdaptiveBudgetsInitialized();
    }

    private void ProcessAiBudget(float now)
    {
        using (ProcessAiBudgetMarker.Auto())
        {
            long startTimestamp = Stopwatch.GetTimestamp();
            try
            {
                BeginPathBudgetWindow();
                LastProcessedDecisionCount = 0;
                LastBehaviorTreeTickCount = 0;

                if (actors.Count == 0)
                {
                    LastPathSearchCount = 0;
                    return;
                }

                int inspected = 0;
                int initialCount = actors.Count;
                while (actors.Count > 0
                    && inspected < initialCount
                    && LastProcessedDecisionCount < GetDecisionBudgetForFrame())
                {
                    if (cursor >= actors.Count)
                    {
                        cursor = 0;
                    }

                    CharacterActor actor = actors[cursor];
                    if (actor == null)
                    {
                        RemoveAt(cursor);
                        initialCount--;
                        continue;
                    }

                    cursor++;
                    inspected++;

                    BehaviorTree behaviorTree = ConfigureCharacterBehaviorTree(actor);
                    bool needsInitialTreeTick = behaviorTree != null
                        && behaviorTree.ExternalBehavior != null
                        && behaviorTree.DungeonStoryTickCount == 0;
                    bool hasSelectedActionWaitingToStart = HasSelectedActionWaitingToStart(actor);

                    if (!actor.IsAiDecisionPending
                        && !needsInitialTreeTick
                        && !hasSelectedActionWaitingToStart)
                    {
                        continue;
                    }

                    if (!needsInitialTreeTick
                        && !hasSelectedActionWaitingToStart
                        && nextDecisionTime.TryGetValue(actor, out float dueTime)
                        && now < dueTime)
                    {
                        continue;
                    }

                    bool decided = TryRunScheduledDecision(actor);
                    nextDecisionTime[actor] = now + (decided ? GetDecisionInterval(actor) : retryDelay);
                    LastProcessedDecisionCount++;
                }

#if UNITY_EDITOR
                RefreshBehaviorDesignerVisualsForEditor();
#endif
                LastPathSearchCount = pathSearchesThisFrame;
            }
            finally
            {
                LastProcessingMilliseconds =
                    (Stopwatch.GetTimestamp() - startTimestamp) * 1000.0 / Stopwatch.Frequency;
                UpdateAdaptiveBudgets();
            }
        }
    }

#if UNITY_EDITOR
    private void RefreshBehaviorDesignerVisualsForEditor()
    {
        if (!driveBehaviorDesignerTrees || !Application.isPlaying)
        {
            return;
        }

        GameObject selectedObject = Selection.activeGameObject;
        CharacterActor selectedActor = selectedObject != null
            ? selectedObject.GetComponent<CharacterActor>() ?? selectedObject.GetComponentInParent<CharacterActor>()
            : null;
        if (selectedActor == null || !actors.Contains(selectedActor))
        {
            return;
        }

        BehaviorTree behaviorTree = ConfigureCharacterBehaviorTree(selectedActor);
        behaviorTree?.DungeonStoryRefreshVisualStatus(selectedActor);
    }
#endif

    private void RegisterExistingCharacters()
    {
        foreach (CharacterActor actor in RequireSceneQuery().All<CharacterActor>())
        {
            RegisterInternal(actor);
        }
    }

    private void RegisterExistingCharactersIfInjected()
    {
        if (sceneQuery != null)
        {
            RegisterExistingCharacters();
        }
    }

    private IDungeonSceneComponentQuery RequireSceneQuery()
    {
        if (sceneQuery == null)
        {
            throw new InvalidOperationException($"{nameof(CharacterAiScheduler)} requires {nameof(IDungeonSceneComponentQuery)} injection.");
        }

        return sceneQuery;
    }

    private void RegisterInternal(CharacterActor actor)
    {
        if (actor == null || actors.Contains(actor))
        {
            return;
        }

        EnsureAdaptiveBudgetsInitialized();
        ConfigureCharacterBehaviorTree(actor);
        actors.Add(actor);
        nextDecisionTime[actor] = 0f;
    }

    private bool TryRunScheduledDecision(CharacterActor actor)
    {
        if (actor == null)
        {
            return false;
        }

        if (!driveBehaviorDesignerTrees)
        {
            return TryRunFallbackDecision(actor);
        }

        BehaviorTree behaviorTree = ConfigureCharacterBehaviorTree(actor);
        if (behaviorTree == null)
        {
            return TryRunFallbackDecision(actor);
        }

        if (behaviorTree.ExternalBehavior == null)
        {
            return TryRunFallbackDecision(actor);
        }

        LastBehaviorTreeTickCount++;
        return behaviorTree.DungeonStoryManualTick(actor);
    }

    private static bool TryRunFallbackDecision(CharacterActor actor)
    {
        AIBrain brain = actor != null ? actor.ai ?? actor.GetComponent<AIBrain>() : null;
        if (brain == null)
        {
            return false;
        }

        if (HasSelectedActionWaitingToStart(actor))
        {
            return actor.TryExecuteSelectedAiAction();
        }

        bool decided = brain.DecideAction();
        if (!decided)
        {
            return false;
        }

        return HasSelectedActionWaitingToStart(actor)
            ? actor.TryExecuteSelectedAiAction()
            : true;
    }

    private static bool HasSelectedActionWaitingToStart(CharacterActor actor)
    {
        AIBrain brain = actor != null ? actor.Brain : null;
        AIAction selectedAction = brain != null ? brain.bestAction : null;
        return actor != null
            && actor.CanRunAi
            && brain != null
            && selectedAction != null
            && selectedAction.actionset != null
            && !selectedAction.HasStarted
            && !brain.isBestActionEnd;
    }

    private static void ConfigureBehaviorManagerForManualTick()
    {
        if (BehaviorManager.instance == null)
        {
            Behavior.CreateBehaviorManager();
        }

        if (BehaviorManager.instance != null)
        {
            // DungeonStory patch: scheduler owns character BT cadence.
            BehaviorManager.instance.UpdateInterval = UpdateIntervalType.Manual;
        }
    }

    private BehaviorTree ConfigureCharacterBehaviorTree(CharacterActor actor)
    {
        return RequireBehaviorTreeConfigurator().Configure(actor, characterAiExternalBehavior);
    }

    private void UnregisterInternal(CharacterActor actor)
    {
        int index = actors.IndexOf(actor);
        if (index < 0)
        {
            nextDecisionTime.Remove(actor);
            return;
        }

        RemoveAt(index);
    }

    private void RemoveAt(int index)
    {
        if (index < 0 || index >= actors.Count)
        {
            return;
        }

        CharacterActor actor = actors[index];
        nextDecisionTime.Remove(actor);
        actors.RemoveAt(index);
        if (cursor > index)
        {
            cursor--;
        }
    }

    private float GetDecisionInterval(CharacterActor actor)
    {
        if (actor != null && actor.IsOwner)
        {
            return ownerDecisionInterval;
        }

        return IsHighDetailCharacter(actor)
            ? visibleDecisionInterval
            : offscreenDecisionInterval;
    }

    private bool IsHighDetailCharacter(CharacterActor actor)
    {
        if (actor == null)
        {
            return false;
        }

        if (actor.IsOwner)
        {
            return true;
        }

        Camera camera = RequireMainCameraProvider().Camera;
        if (camera == null)
        {
            return true;
        }

        Vector3 viewport = camera.WorldToViewportPoint(actor.transform.position);
        return viewport.z >= 0f
            && viewport.x >= -viewportMargin
            && viewport.x <= 1f + viewportMargin
            && viewport.y >= -viewportMargin
            && viewport.y <= 1f + viewportMargin;
    }

    private bool TryConsumePathSearchBudgetInternal()
    {
        ResetPathBudgetIfNeeded();
        if (pathSearchesThisFrame >= GetPathSearchBudgetForFrame())
        {
            return false;
        }

        pathSearchesThisFrame++;
        return true;
    }

    private void BeginPathBudgetWindow()
    {
        EnsureAdaptiveBudgetsInitialized();
        pathBudgetFrame = Time.frameCount;
        pathSearchesThisFrame = 0;
        LastPathSearchCount = 0;
    }

    private void ResetPathBudgetIfNeeded()
    {
        if (pathBudgetFrame == Time.frameCount)
        {
            return;
        }

        pathBudgetFrame = Time.frameCount;
        pathSearchesThisFrame = 0;
    }

    private int GetDecisionBudgetForFrame()
    {
        EnsureAdaptiveBudgetsInitialized();
        return Mathf.Clamp(currentDecisionBudget, Mathf.Max(1, minDecisionsPerFrame), Mathf.Max(1, maxDecisionsPerFrame));
    }

    private int GetPathSearchBudgetForFrame()
    {
        EnsureAdaptiveBudgetsInitialized();
        return Mathf.Clamp(currentPathSearchBudget, Mathf.Max(1, minPathSearchesPerFrame), Mathf.Max(1, maxPathSearchesPerFrame));
    }

    private void EnsureAdaptiveBudgetsInitialized()
    {
        if (currentDecisionBudget <= 0 || currentPathSearchBudget <= 0)
        {
            ResetAdaptiveBudgets();
        }
    }

    private void ResetAdaptiveBudgets()
    {
        currentDecisionBudget = Mathf.Max(1, maxDecisionsPerFrame);
        currentPathSearchBudget = Mathf.Max(1, maxPathSearchesPerFrame);
    }

    private void UpdateAdaptiveBudgets()
    {
        if (!adaptBudgetsToFrameCost || actors.Count == 0)
        {
            return;
        }

        if (adaptiveCooldownFrames > 0)
        {
            adaptiveCooldownFrames--;
            return;
        }

        int minDecisionBudget = Mathf.Clamp(minDecisionsPerFrame, 1, Mathf.Max(1, maxDecisionsPerFrame));
        int minPathBudget = Mathf.Clamp(minPathSearchesPerFrame, 1, Mathf.Max(1, maxPathSearchesPerFrame));
        bool overBudget = LastProcessingMilliseconds > targetAiMilliseconds * 1.25;
        bool underBudget = LastProcessingMilliseconds < targetAiMilliseconds * 0.5
            && (LastProcessedDecisionCount >= currentDecisionBudget || LastPathSearchCount >= currentPathSearchBudget);

        if (overBudget)
        {
            currentDecisionBudget = Mathf.Max(minDecisionBudget, currentDecisionBudget - 1);
            currentPathSearchBudget = Mathf.Max(minPathBudget, currentPathSearchBudget - 1);
            adaptiveCooldownFrames = 30;
            return;
        }

        if (underBudget)
        {
            currentDecisionBudget = Mathf.Min(Mathf.Max(1, maxDecisionsPerFrame), currentDecisionBudget + 1);
            currentPathSearchBudget = Mathf.Min(Mathf.Max(1, maxPathSearchesPerFrame), currentPathSearchBudget + 1);
            adaptiveCooldownFrames = 30;
        }
    }

    private IMainCameraProvider RequireMainCameraProvider()
    {
        return mainCameraProvider
            ?? throw new InvalidOperationException($"{nameof(CharacterAiScheduler)} requires {nameof(IMainCameraProvider)} injection.");
    }

    private ICharacterBehaviorTreeRuntimeConfigurator RequireBehaviorTreeConfigurator()
    {
        return behaviorTreeConfigurator
            ?? throw new InvalidOperationException($"{nameof(CharacterAiScheduler)} requires {nameof(ICharacterBehaviorTreeRuntimeConfigurator)} injection.");
    }
}
