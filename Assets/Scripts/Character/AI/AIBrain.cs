using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Linq;

public class AIBrain : CharacterAbility
{
    private const string WaitActionPath = "SO/AI/Action/Wait";
    private const string WorkActionPath = "SO/AI/Action/Work";
    private const string EatActionPath = "SO/AI/Action/Eat";
    private const string RestActionPath = "SO/AI/Action/Rest";
    private const string ShoppingActionPath = "SO/AI/Action/Shopping";
    private const string LookAroundActionPath = "SO/AI/Action/LookAround";
    private const string ExitDungeonActionPath = "SO/AI/Action/ExitDungeon";

    public AIAction[] availableActions;
    [ReadOnly]public AIAction bestAction;
    public bool isBestActionEnd = true;
    public bool isExecuted = false;
    [SerializeField] private float actionFailureCooldown = 1f;
    [SerializeField, Range(0f, 0.5f)] private float actionSwitchScoreMargin = 0.12f;
    [SerializeField, Range(1, 8)] private int debugCandidateLimit = 3;
    private GridPathSearchResult pathSearchCache;
    private readonly Dictionary<AIActionSet, float> actionFailureCooldownUntil = new Dictionary<AIActionSet, float>();
    private readonly List<AIAction> destinationFailedThisDecision = new List<AIAction>();
    private readonly List<AIActionDebugCandidate> lastCandidateScores = new List<AIActionDebugCandidate>();
    private float noActionLogCooldownUntil;
    private AIAction queuedAction;
    private AIActionFailure lastActionFailure = AIActionFailure.None;
    private AIActionSet lastFailedActionSet;
    private string currentActionDebugLabel = "대기";
    public bool IsPathSearchDeferred { get; private set; }
    public AIActionFailure LastActionFailure => lastActionFailure;
    public IReadOnlyList<AIActionDebugCandidate> LastCandidateScores => lastCandidateScores;
    public string CurrentActionDebugLabel => currentActionDebugLabel;
    public int DebugVersion { get; private set; }

    public override void Initializtion(CharacterSO data)
    {
        base.Initializtion(data);
        if (data != null && data.role == CharacterRole.Owner)
        {
            UseOwnerWorkActions();
        }
        else
        {
            NormalizeConfiguredActions();
            EnsureVisitorActions();
        }
        isBestActionEnd = true;
        ClearPathSearchCache();
    }

    public void UseOwnerWorkActions()
    {
        List<AIAction> actions = new List<AIAction>();
        AddRequiredAction<AIWork>(actions, WorkActionPath);
        AddRequiredAction<AIWait>(actions, WaitActionPath);
        availableActions = actions.ToArray();
    }

    private void NormalizeConfiguredActions()
    {
        if (availableActions == null)
        {
            Debug.LogError($"{name}: AI actions are not configured.");
            return;
        }

        AIAction[] configuredActions = availableActions
            .Where((action) => action != null && action.actionset != null)
            .ToArray();
        if (configuredActions.Length != availableActions.Length)
        {
            Debug.LogWarning($"{name}: Removed null AI action entries. Configure the missing actions on the prefab or asset.");
        }

        availableActions = configuredActions;
        if (availableActions.Length == 0)
        {
            Debug.LogError($"{name}: AI actions are empty after validation.");
        }
    }

    private void EnsureVisitorActions()
    {
        if (CharacterWorkRoleUtility.TryGetWork(character, out _))
        {
            return;
        }

        List<AIAction> actions = availableActions != null
            ? availableActions.ToList()
            : new List<AIAction>();
        AddRequiredAction<AIEat>(actions, EatActionPath);
        AddRequiredAction<AIRest>(actions, RestActionPath);
        AddRequiredAction<AIShopping>(actions, ShoppingActionPath);
        AddRequiredAction<AILookAround>(actions, LookAroundActionPath);
        AddRequiredAction<AIExitDungeon>(actions, ExitDungeonActionPath);
        availableActions = actions.ToArray();
    }

    private static void AddRequiredAction<T>(List<AIAction> actions, string resourcePath) where T : AIActionSet
    {
        if (actions.Any((action) => action.actionset is T)) return;

        T actionSet = Resources.Load<T>(resourcePath);
        if (actionSet == null)
        {
            Debug.LogError($"Required AI action asset is missing: Resources/{resourcePath}");
            return;
        }

        actions.Add(new AIAction { actionset = actionSet });
    }
    public bool DecideAction()
    {
        ReleaseFinishedActionReservation();

        if (character != null && character.CanRunAi)
        {
            EnsureVisitorActions();
        }

        if (character == null
            || !character.CanRunAi
            || GridSystemManager.Instance.grid == null
            || availableActions == null
            || availableActions.Length == 0)
        {
            bestAction = null;
            isBestActionEnd = true;
            isExecuted = false;
            return false;
        }

        GetPathSearch(character);
        if (IsPathSearchDeferred)
        {
            bestAction = null;
            isBestActionEnd = true;
            isExecuted = false;
            return false;
        }

        if (TryUseQueuedAction())
        {
            return true;
        }

        return DecideActionByScoreThenDestination();

#if false
        float highestScore = float.MinValue;
        AIAction tempBestAction = null;
        isBestActionEnd = false;
        isExecuted = false;

        foreach (var action in availableActions)
        {
            if (action == null) continue;

            if (!CanUseAction(action, out string failureReason))
            {
                if (!string.IsNullOrWhiteSpace(failureReason)
                    && failureReason != "점수 없음"
                    && failureReason != "쿨다운"
                    && failureReason != "시작 조건 불만족")
                {
                    RecordActionFailure(action.actionset, failureReason);
                }
                continue;
            }

            if (action.score > highestScore)
            {
                highestScore = action.score;
                tempBestAction = action;
            }
        }
        bestAction = tempBestAction;
        if (bestAction == null)
        {
            isBestActionEnd = true;
            RecordNoActionFailure();
            return false;
        }

        return true;
#endif
    }

    private bool DecideActionByScoreThenDestination()
    {
        isBestActionEnd = false;
        isExecuted = false;
        destinationFailedThisDecision.Clear();
        lastCandidateScores.Clear();
        lastActionFailure = AIActionFailure.None;
        lastFailedActionSet = null;

        foreach (AIAction action in availableActions)
        {
            if (action == null) continue;

            if (!CanConsiderAction(action, out AIActionFailure failure))
            {
                action.score = 0f;
                RememberCandidateFailure(action, failure);
            }

            RecordCandidateDebug(action, failure);
        }

        while (TryFindHighestScoredAction(out AIAction candidate))
        {
            if (candidate.SetDestinationWithFailure(character, out AIActionFailure failure))
            {
                bestAction = candidate;
                currentActionDebugLabel = GetActionLabel(candidate.actionset);
                destinationFailedThisDecision.Clear();
                MarkDebugDirty();
                return true;
            }

            destinationFailedThisDecision.Add(candidate);
            RecordActionFailure(candidate.actionset, failure);
        }

        destinationFailedThisDecision.Clear();
        bestAction = null;
        isBestActionEnd = true;
        RecordNoActionFailure();
        return false;
    }

    private bool TryFindHighestScoredAction(out AIAction bestCandidate)
    {
        bestCandidate = null;
        float highestScore = float.MinValue;
        foreach (AIAction action in availableActions)
        {
            if (action == null
                || action.score <= 0f
                || destinationFailedThisDecision.Contains(action))
            {
                continue;
            }

            float selectionScore = GetSelectionScore(action);
            if (selectionScore > highestScore)
            {
                highestScore = selectionScore;
                bestCandidate = action;
            }
        }

        return bestCandidate != null;
    }

    private float GetSelectionScore(AIAction action)
    {
        if (action == null)
        {
            return 0f;
        }

        float selectionScore = action.score;
        if (bestAction != null
            && bestAction.actionset != null
            && action.actionset == bestAction.actionset)
        {
            selectionScore += actionSwitchScoreMargin;
        }

        return selectionScore;
    }

    public GridPathSearchResult GetPathSearch(Character character)
    {
        IsPathSearchDeferred = false;
        if (character == null || GridSystemManager.Instance.grid == null) return null;

        Grid grid = GridSystemManager.Instance.grid;
        Vector2Int start = character.GetNowXY();
        if (pathSearchCache == null ||
            pathSearchCache.sourceGrid != grid ||
            pathSearchCache.start != start ||
            pathSearchCache.gridVersion != grid.version)
        {
            if (!CharacterAiScheduler.TryConsumePathSearchBudget())
            {
                IsPathSearchDeferred = true;
                return null;
            }

            pathSearchCache = grid.SearchPath(start);
        }
        return pathSearchCache;
    }

    public void ClearPathSearchCache()
    {
        pathSearchCache = null;
    }

    public void RequestImmediateReplan(bool clearFailures = false)
    {
        bestAction?.ReleaseReservation(character);
        queuedAction?.ReleaseReservation(character);
        bestAction = null;
        queuedAction = null;
        destinationFailedThisDecision.Clear();
        ClearPathSearchCache();

        isExecuted = false;
        isBestActionEnd = character == null || character.CanRunAi;
        IsPathSearchDeferred = false;

        if (clearFailures)
        {
            actionFailureCooldownUntil.Clear();
            lastActionFailure = AIActionFailure.None;
            lastFailedActionSet = null;
            noActionLogCooldownUntil = 0f;
        }

        MarkDebugDirty();
        CharacterAiScheduler.RequestImmediateDecision(character);
    }

    public void NotifyActionStarted()
    {
        if (bestAction == null || bestAction.actionset == null)
        {
            return;
        }

        bestAction.MarkStarted(Time.time);
        currentActionDebugLabel = GetActionLabel(bestAction.actionset);
        MarkDebugDirty();
    }

    public bool ShouldStopCurrentAction(out string stopReason)
    {
        stopReason = string.Empty;
        if (character == null || bestAction == null || bestAction.actionset == null)
        {
            return false;
        }

        AIActionSet runningActionSet = bestAction.actionset;
        if (!runningActionSet.IsContinuous || !bestAction.HasStarted)
        {
            return false;
        }

        if (!runningActionSet.CanContinue(character, bestAction, out stopReason))
        {
            if (string.IsNullOrWhiteSpace(stopReason))
            {
                stopReason = "행동 유지 조건 불만족";
            }
            return true;
        }

        if (bestAction.RunningSeconds < runningActionSet.MinimumDuration)
        {
            return false;
        }

        if (runningActionSet.CanInterrupt(character, bestAction, out stopReason))
        {
            if (string.IsNullOrWhiteSpace(stopReason))
            {
                stopReason = "중단 조건 발생";
            }
            return true;
        }

        if (TryFindInterruptAction(bestAction, out AIAction interruptAction, out stopReason))
        {
            queuedAction = interruptAction;
            return true;
        }

        return false;
    }

    private bool TryUseQueuedAction()
    {
        if (queuedAction == null)
        {
            return false;
        }

        AIAction action = queuedAction;
        queuedAction = null;
        action.ReleaseReservation(character);

        if (!CanUseAction(action, out AIActionFailure _))
        {
            return false;
        }

        bestAction = action;
        currentActionDebugLabel = GetActionLabel(action.actionset);
        isBestActionEnd = false;
        isExecuted = false;
        MarkDebugDirty();
        return true;
    }

    private bool TryFindInterruptAction(
        AIAction runningAction,
        out AIAction interruptAction,
        out string interruptReason)
    {
        interruptAction = null;
        interruptReason = string.Empty;
        if (runningAction == null
            || runningAction.actionset == null
            || availableActions == null)
        {
            return false;
        }

        int runningPriority = runningAction.actionset.InterruptPriority;
        float bestScore = float.MinValue;
        int bestPriority = int.MinValue;

        foreach (AIAction action in availableActions)
        {
            if (action == null
                || action == runningAction
                || action.actionset == null
                || action.actionset.InterruptPriority <= runningPriority)
            {
                continue;
            }

            if (!CanUseAction(action, out AIActionFailure _))
            {
                action.ReleaseReservation(character);
                continue;
            }

            int priority = action.actionset.InterruptPriority;
            float score = GetSelectionScore(action);
            if (priority > bestPriority || (priority == bestPriority && score > bestScore))
            {
                if (interruptAction != null && interruptAction != action)
                {
                    interruptAction.ReleaseReservation(character);
                }

                bestPriority = priority;
                bestScore = score;
                interruptAction = action;
            }
            else
            {
                action.ReleaseReservation(character);
            }
        }

        if (interruptAction == null)
        {
            return false;
        }

        interruptReason = $"상위 행동: {GetActionLabel(interruptAction.actionset)}";
        return true;
    }

    private bool CanConsiderAction(AIAction action, out string failureReason)
    {
        bool result = CanConsiderAction(action, out AIActionFailure failure);
        failureReason = failure.ToString();
        return result;
    }

    private bool CanConsiderAction(AIAction action, out AIActionFailure failure)
    {
        failure = AIActionFailure.None;
        if (action == null || action.actionset == null)
        {
            failure = AIActionFailure.Create(AIActionFailureKind.NoAction);
            return false;
        }

        if (IsActionCoolingDown(action.actionset))
        {
            failure = AIActionFailure.Create(AIActionFailureKind.Cooldown);
            return false;
        }

        GridPathSearchResult searchResult = character != null && character.ai != null
            ? character.ai.GetPathSearch(character)
            : null;
        if (character != null && character.ai != null && character.ai.IsPathSearchDeferred)
        {
            failure = AIActionFailure.Create(AIActionFailureKind.PathSearchDeferred);
            return false;
        }

        if (!action.actionset.CanStartWithFailure(character, searchResult, out failure))
        {
            if (!failure.HasFailure)
            {
                failure = AIActionFailure.Create(AIActionFailureKind.CannotStart);
            }

            failure = RefineActionFailure(action, failure);
            return false;
        }

        float actionScore = action.CalculateScore(character);
        if (actionScore <= 0f)
        {
            failure = AIActionFailure.Create(AIActionFailureKind.NoScore);
            return false;
        }

        return true;
    }

    private bool CanUseAction(AIAction action, out string failureReason)
    {
        bool result = CanUseAction(action, out AIActionFailure failure);
        failureReason = failure.ToString();
        return result;
    }

    private bool CanUseAction(AIAction action, out AIActionFailure failure)
    {
        failure = AIActionFailure.None;
        if (!CanConsiderAction(action, out failure))
        {
            return false;
        }

        if (action.SetDestinationWithFailure(character, out failure))
        {
            return true;
        }

        failure = RefineActionFailure(action, failure);
        return false;
    }

#if false
        if (action == null || action.actionset == null)
        {
            failureReason = "행동 없음";
            return false;
        }

        float actionScore = action.CalculateScore(character);
        if (actionScore <= 0f)
        {
            failureReason = "점수 없음";
            return false;
        }

        if (IsActionCoolingDown(action.actionset))
        {
            failureReason = "쿨다운";
            return false;
        }

        if (!action.actionset.CanStart(character))
        {
            failureReason = "시작 조건 불만족";
            return false;
        }

        return action.SetDestination(character, out failureReason);
    }

#endif

    private bool IsActionCoolingDown(AIActionSet actionSet)
    {
        return actionSet != null
            && actionFailureCooldownUntil.TryGetValue(actionSet, out float until)
            && Time.time < until;
    }

    private void RecordActionFailure(AIActionSet actionSet, string reason)
    {
        RecordActionFailure(actionSet, AIActionFailure.FromReason(reason));
    }

    private void RecordActionFailure(AIActionSet actionSet, AIActionFailure failure)
    {
        if (actionSet == null) return;

        actionFailureCooldownUntil[actionSet] = Time.time + Mathf.Max(0.1f, actionFailureCooldown);
        lastFailedActionSet = actionSet;
        lastActionFailure = failure.HasFailure ? failure : AIActionFailure.Create(AIActionFailureKind.Unknown);
        character?.AddLog($"AI 실패: {GetActionLabel(actionSet)} - {lastActionFailure}");
        MarkDebugDirty();
    }

    private void RecordNoActionFailure()
    {
        if (Time.time < noActionLogCooldownUntil) return;

        noActionLogCooldownUntil = Time.time + Mathf.Max(0.1f, actionFailureCooldown);
        lastFailedActionSet = null;
        lastActionFailure = AIActionFailure.Create(AIActionFailureKind.NoAction, "실행 가능한 행동 없음");
        currentActionDebugLabel = "대기";
        character?.AddLog("AI 대기: 실행 가능한 행동 없음");
        MarkDebugDirty();
    }

    private static string GetActionLabel(AIActionSet actionSet)
    {
        if (actionSet == null) return "알 수 없음";
        return !string.IsNullOrWhiteSpace(actionSet.actionName)
            ? actionSet.actionName
            : actionSet.GetType().Name;
    }

    private AIActionFailure RefineActionFailure(AIAction action, AIActionFailure failure)
    {
        if (action?.actionset is AIWork
            && character != null
            && character.TryGetAbility(out AbilityWork work)
            && work.TryGetLastRejectedWorkCandidate(out WorkTargetCandidate candidate)
            && candidate.Building != null)
        {
            return AIActionFailure.FromReason(
                candidate.FailureReason,
                candidate.FailureKind != AIActionFailureKind.None
                    ? candidate.FailureKind
                    : failure.Kind,
                candidate.Building);
        }

        return failure;
    }

    private void RecordCandidateDebug(AIAction action, AIActionFailure failure)
    {
        if (action == null || action.actionset == null)
        {
            return;
        }

        lastCandidateScores.Add(new AIActionDebugCandidate(
            GetActionLabel(action.actionset),
            action.score,
            failure,
            action.destination));
        MarkDebugDirty();
    }

    private void RememberCandidateFailure(AIAction action, AIActionFailure failure)
    {
        if (action == null || action.actionset == null || !failure.HasFailure)
        {
            return;
        }

        if (GetFailureDebugPriority(failure.Kind) <= GetFailureDebugPriority(lastActionFailure.Kind))
        {
            return;
        }

        lastFailedActionSet = action.actionset;
        lastActionFailure = failure;
        if (ShouldCooldownCandidateFailure(failure.Kind))
        {
            actionFailureCooldownUntil[action.actionset] = Time.time + Mathf.Max(0.1f, actionFailureCooldown);
        }
    }

    private void ReleaseFinishedActionReservation()
    {
        if (bestAction == null || !isBestActionEnd)
        {
            return;
        }

        bestAction.ReleaseReservation(character);
    }

    private static bool ShouldCooldownCandidateFailure(AIActionFailureKind kind)
    {
        return kind == AIActionFailureKind.DestinationOccupied
            || kind == AIActionFailureKind.NoPath
            || kind == AIActionFailureKind.NoDestination
            || kind == AIActionFailureKind.DestinationSelectionFailed
            || kind == AIActionFailureKind.Unsupported;
    }

    private static int GetFailureDebugPriority(AIActionFailureKind kind)
    {
        return kind switch
        {
            AIActionFailureKind.DestinationOccupied => 80,
            AIActionFailureKind.NoPath => 70,
            AIActionFailureKind.NoDestination => 60,
            AIActionFailureKind.DestinationSelectionFailed => 55,
            AIActionFailureKind.NoWork => 45,
            AIActionFailureKind.OffDuty => 35,
            AIActionFailureKind.Unsupported => 25,
            AIActionFailureKind.CannotStart => 20,
            AIActionFailureKind.PathSearchDeferred => 10,
            AIActionFailureKind.Cooldown => 5,
            AIActionFailureKind.NoScore => 1,
            _ => 0
        };
    }

    public string GetDebugSummary(int candidateCount = 3)
    {
        string actionLabel = bestAction != null && bestAction.actionset != null
            ? GetActionLabel(bestAction.actionset)
            : currentActionDebugLabel;
        string runningLabel = bestAction != null && bestAction.HasStarted
            ? $" ({bestAction.RunningSeconds:0.0}s)"
            : string.Empty;
        string reservationLabel = bestAction != null && bestAction.HasReservation
            ? $"\n예약: {GetDestinationLabel(bestAction.ReservedDestination)}"
            : string.Empty;
        string failureLabel = lastActionFailure.HasFailure
            ? lastActionFailure.ToString()
            : "정상";
        string failedActionLabel = lastFailedActionSet != null
            ? GetActionLabel(lastFailedActionSet)
            : string.Empty;
        string reason = string.IsNullOrWhiteSpace(failedActionLabel)
            ? failureLabel
            : $"{failedActionLabel}: {failureLabel}";

        IEnumerable<AIActionDebugCandidate> candidates = lastCandidateScores
            .OrderByDescending((candidate) => candidate.Score)
            .Take(Mathf.Clamp(candidateCount, 1, debugCandidateLimit));
        string candidateText = string.Join(
            ", ",
            candidates.Select((candidate) =>
                candidate.Failure.HasFailure
                    ? $"{candidate.ActionLabel} {candidate.Score:0.00}({candidate.Failure.Kind})"
                    : $"{candidate.ActionLabel} {candidate.Score:0.00}"));

        return string.IsNullOrWhiteSpace(candidateText)
            ? $"행동: {actionLabel}{runningLabel}{reservationLabel}\n이유: {reason}"
            : $"행동: {actionLabel}{runningLabel}{reservationLabel}\n이유: {reason}\n후보: {candidateText}";
    }

    private static string GetDestinationLabel(BuildableObject destination)
    {
        if (destination == null)
        {
            return "없음";
        }

        if (destination.BuildingData != null
            && !string.IsNullOrWhiteSpace(destination.BuildingData.objectName))
        {
            return destination.BuildingData.objectName;
        }

        return destination.name;
    }

    public int GetDebugHash()
    {
        unchecked
        {
            int hash = DebugVersion;
            hash = (hash * 31) + (bestAction?.actionset != null ? bestAction.actionset.GetInstanceID() : 0);
            hash = (hash * 31) + (int)lastActionFailure.Kind;
            foreach (AIActionDebugCandidate candidate in lastCandidateScores.Take(debugCandidateLimit))
            {
                hash = (hash * 31) + candidate.ActionLabel.GetHashCode();
                hash = (hash * 31) + Mathf.RoundToInt(candidate.Score * 1000f);
                hash = (hash * 31) + (int)candidate.Failure.Kind;
            }

            return hash;
        }
    }

    private void MarkDebugDirty()
    {
        DebugVersion++;
    }
}
public enum AIActionPlanKind
{
    None,
    NoDestination,
    DestinationOnly,
    MovePath
}

[Serializable]
public class AIAction
{
    public AIActionSet actionset;
    private float _score;
    public float score
    {
        get { return _score; }
        set
        {
            _score = Mathf.Clamp01(value);
        }
    }
    public BuildableObject destination;
    public Queue<GridMoveStep> pathSteps;
    public AIActionPlanKind planKind;
    public float startedAt = -1f;
    private AIActionSet reservedActionSet;
    private BuildableObject reservedDestination;
    public bool HasStarted => startedAt >= 0f;
    public float RunningSeconds => HasStarted ? Time.time - startedAt : 0f;
    public bool HasReservation => reservedActionSet != null && reservedDestination != null;
    public BuildableObject ReservedDestination => reservedDestination;

    public void MarkStarted(float time)
    {
        startedAt = time;
    }

    public float CalculateScore(Character character)
    {
        if (actionset == null)
        {
            this.score = 0f;
            return this.score;
        }

        if (actionset.considerations == null || actionset.considerations.Length == 0)
        {
            float baseScore = CharacterAiPersonalityUtility.GetActionScoreMultiplier(character, actionset);
            this.score = actionset.AdjustScore(character, baseScore);
            return this.score;
        }

        int considerationCount = actionset.considerations.Length;
        float totalScore = 0f;
        foreach (var consideration in actionset.considerations)
        {
            if (consideration == null)
            {
                this.score = 0f;
                return this.score;
            }

            float considerationScore = Mathf.Clamp01(consideration.ScoreConsideration(character));
            if (considerationScore <= 0f)
            {
                this.score = 0;
                return this.score;
            }

            totalScore += considerationScore;
        }

        float actionScore = totalScore / considerationCount;
        actionScore *= CharacterAiPersonalityUtility.GetActionScoreMultiplier(character, actionset);
        this.score = actionset.AdjustScore(character, actionScore);
        return this.score;
    }

    public bool SetDestination(Character character, out string failureReason)
    {
        bool result = SetDestinationWithFailure(character, out AIActionFailure failure);
        failureReason = failure.ToString();
        return result;
    }

    public bool SetDestinationWithFailure(Character character, out AIActionFailure failure)
    {
        ReleaseReservation(character);
        destination = null;
        pathSteps = null;
        planKind = AIActionPlanKind.None;
        startedAt = -1f;
        failure = AIActionFailure.None;
        if (character == null || actionset == null || GridSystemManager.Instance.grid == null)
        {
            failure = AIActionFailure.Create(AIActionFailureKind.NoGrid, "AI 또는 그리드 없음");
            return false;
        }

        Grid grid = GridSystemManager.Instance.grid;
        GridPathSearchResult searchResult = character.ai != null ? character.ai.GetPathSearch(character) : null;
        if (!actionset.TryResolveDestinationWithFailure(character, searchResult, out destination, out failure))
        {
            return false;
        }

        if (destination == null)
        {
            planKind = AIActionPlanKind.NoDestination;
            return !actionset.RequiresDestination;
        }

        if (searchResult != null)
        {
            pathSteps = searchResult.GetMovePathTo(destination);
            return ResolvePathPlan(character, destination, out failure)
                && TryReserveResolvedDestination(character, destination, out failure);
        }

        Func<Vector2Int, bool> condition = (pos) => grid.GetGridCell(pos)?.GetBuildingInlayer() == destination;
        pathSteps = grid.GetMovePath(character.GetNowXY(), condition);
        return ResolvePathPlan(character, destination, out failure)
            && TryReserveResolvedDestination(character, destination, out failure);
    }

    public void RefreshReservation(Character character)
    {
        if (!HasReservation)
        {
            return;
        }

        reservedActionSet.RefreshDestinationReservation(character, reservedDestination);
    }

    public void ReleaseReservation(Character character)
    {
        if (!HasReservation)
        {
            return;
        }

        reservedActionSet.ReleaseDestinationReservation(character, reservedDestination);
        reservedActionSet = null;
        reservedDestination = null;
    }

    private bool TryReserveResolvedDestination(
        Character character,
        BuildableObject destination,
        out AIActionFailure failure)
    {
        failure = AIActionFailure.None;
        if (actionset == null || destination == null)
        {
            return true;
        }

        if (!actionset.TryReserveDestination(character, destination, out failure))
        {
            planKind = AIActionPlanKind.None;
            return false;
        }

        reservedActionSet = actionset;
        reservedDestination = destination;
        return true;
    }

    private bool ResolvePathPlan(Character character, BuildableObject destination, out string failureReason)
    {
        bool result = ResolvePathPlan(character, destination, out AIActionFailure failure);
        failureReason = failure.ToString();
        return result;
    }

    private bool ResolvePathPlan(Character character, BuildableObject destination, out AIActionFailure failure)
    {
        failure = AIActionFailure.None;
        if (pathSteps != null && pathSteps.Count > 0)
        {
            planKind = AIActionPlanKind.MovePath;
            return true;
        }

        if (IsCharacterAtDestination(character, destination))
        {
            planKind = AIActionPlanKind.DestinationOnly;
            return true;
        }

        failure = AIActionFailure.Create(AIActionFailureKind.NoPath, "경로 없음", destination);
        planKind = AIActionPlanKind.None;
        return false;
    }

    private static bool IsCharacterAtDestination(Character character, BuildableObject destination)
    {
        if (character == null || destination == null || GridSystemManager.Instance.grid == null)
        {
            return false;
        }

        Grid grid = GridSystemManager.Instance.grid;
        GridCell cell = grid.GetGridCell(character.GetNowXY());
        return cell != null && cell.GetAllOccupants().Contains(destination);
    }
}
