using System.Collections.Generic;
using System.Diagnostics;
using Unity.Profiling;
using UnityEngine;

public sealed class CharacterAiScheduler : MonoBehaviour
{
    private static readonly ProfilerMarker ProcessAiBudgetMarker =
        new ProfilerMarker("CharacterAiScheduler.ProcessAiBudget");

    private static CharacterAiScheduler instance;

    [SerializeField] private bool driveCharacterUpdates = true;
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

    private readonly List<Character> characters = new List<Character>();
    private readonly Dictionary<Character, float> nextDecisionTime = new Dictionary<Character, float>();
    private int cursor;
    private int pathBudgetFrame = -1;
    private int pathSearchesThisFrame;
    private int currentDecisionBudget;
    private int currentPathSearchBudget;
    private int adaptiveCooldownFrames;
    private float manualTime;

    public static bool IsDrivingAi => instance != null
        && instance.enabled
        && instance.driveCharacterUpdates;

    public int RegisteredCharacterCount => characters.Count;
    public int LastProcessedDecisionCount { get; private set; }
    public int LastPathSearchCount { get; private set; }
    public double LastProcessingMilliseconds { get; private set; }
    public int CurrentDecisionBudget => Mathf.Max(1, currentDecisionBudget);
    public int CurrentPathSearchBudget => Mathf.Max(1, currentPathSearchBudget);
    public bool IsPathBudgetActiveForDebug => instance == this
        && enabled
        && driveCharacterUpdates
        && limitPathSearches;

    private void Awake()
    {
        instance = this;
        RegisterExistingCharacters();
    }

    private void OnEnable()
    {
        instance = this;
        RegisterExistingCharacters();
    }

    private void OnDisable()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void Update()
    {
        ProcessAiBudget(Time.time);
    }

    public static void Register(Character character)
    {
        if (instance == null || character == null)
        {
            return;
        }

        instance.RegisterInternal(character);
    }

    public static void Unregister(Character character)
    {
        if (instance == null || character == null)
        {
            return;
        }

        instance.UnregisterInternal(character);
    }

    public static void RequestImmediateDecision(Character character)
    {
        if (instance == null || character == null)
        {
            return;
        }

        instance.RegisterInternal(character);
        instance.nextDecisionTime[character] = 0f;
    }

    public static bool TryConsumePathSearchBudget()
    {
        if (instance == null
            || !instance.enabled
            || !instance.driveCharacterUpdates
            || !instance.limitPathSearches)
        {
            return true;
        }

        return instance.TryConsumePathSearchBudgetInternal();
    }

    public static bool ShouldShowCharacterFeedback(Character character)
    {
        if (instance == null
            || !instance.enabled
            || !instance.limitFeedbackToVisibleCharacters)
        {
            return true;
        }

        return instance.IsHighDetailCharacter(character);
    }

    public static int GetMovementFrameStride(Character character)
    {
        if (instance == null
            || !instance.enabled
            || !instance.driveCharacterUpdates
            || instance.offscreenMovementFrameStride <= 1
            || instance.IsHighDetailCharacter(character))
        {
            return 1;
        }

        return instance.offscreenMovementFrameStride;
    }

    public void RunManualTick(float deltaTime)
    {
        manualTime += Mathf.Max(0f, deltaTime);
        ProcessAiBudget(manualTime);
    }

    public void ClearRegistrationsForDebug()
    {
        instance = this;
        ResetAdaptiveBudgets();
        manualTime = Time.time;
        characters.Clear();
        nextDecisionTime.Clear();
        cursor = 0;
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

                if (characters.Count == 0)
                {
                    LastPathSearchCount = 0;
                    return;
                }

                int inspected = 0;
                int initialCount = characters.Count;
                while (characters.Count > 0
                    && inspected < initialCount
                    && LastProcessedDecisionCount < GetDecisionBudgetForFrame())
                {
                    if (cursor >= characters.Count)
                    {
                        cursor = 0;
                    }

                    Character character = characters[cursor];
                    if (character == null)
                    {
                        RemoveAt(cursor);
                        initialCount--;
                        continue;
                    }

                    cursor++;
                    inspected++;

                    if (!character.IsAiDecisionPending)
                    {
                        continue;
                    }

                    if (nextDecisionTime.TryGetValue(character, out float dueTime) && now < dueTime)
                    {
                        continue;
                    }

                    bool decided = character.TryRunAiDecision();
                    nextDecisionTime[character] = now + (decided ? GetDecisionInterval(character) : retryDelay);
                    LastProcessedDecisionCount++;
                }

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

    private void RegisterExistingCharacters()
    {
        foreach (Character character in FindObjectsByType<Character>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None))
        {
            RegisterInternal(character);
        }
    }

    private void RegisterInternal(Character character)
    {
        if (character == null || characters.Contains(character))
        {
            return;
        }

        EnsureAdaptiveBudgetsInitialized();
        characters.Add(character);
        nextDecisionTime[character] = Time.time + Random.Range(0f, visibleDecisionInterval);
    }

    private void UnregisterInternal(Character character)
    {
        int index = characters.IndexOf(character);
        if (index < 0)
        {
            nextDecisionTime.Remove(character);
            return;
        }

        RemoveAt(index);
    }

    private void RemoveAt(int index)
    {
        if (index < 0 || index >= characters.Count)
        {
            return;
        }

        Character character = characters[index];
        nextDecisionTime.Remove(character);
        characters.RemoveAt(index);
        if (cursor > index)
        {
            cursor--;
        }
    }

    private float GetDecisionInterval(Character character)
    {
        if (character != null && character.IsOwner)
        {
            return ownerDecisionInterval;
        }

        return IsHighDetailCharacter(character)
            ? visibleDecisionInterval
            : offscreenDecisionInterval;
    }

    private bool IsHighDetailCharacter(Character character)
    {
        if (character == null)
        {
            return false;
        }

        if (character.IsOwner)
        {
            return true;
        }

        Camera camera = Camera.main;
        if (camera == null)
        {
            return true;
        }

        Vector3 viewport = camera.WorldToViewportPoint(character.transform.position);
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
        if (!adaptBudgetsToFrameCost || characters.Count == 0)
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
}
