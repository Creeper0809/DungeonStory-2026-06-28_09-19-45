using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Linq;

public class AIBrain : CharacterAbility
{
    private const string LookAroundActionPath = "SO/AI/Action/LookAround";
    private const string ExitDungeonActionPath = "SO/AI/Action/ExitDungeon";
    private const string WaitActionPath = "SO/AI/Action/Wait";

    public AIAction[] availableActions;
    [ReadOnly]public AIAction bestAction;
    public bool isBestActionEnd = true;
    public bool isExecuted = false;
    [SerializeField] private float actionFailureCooldown = 1f;
    private GridPathSearchResult pathSearchCache;
    private readonly Dictionary<AIActionSet, float> actionFailureCooldownUntil = new Dictionary<AIActionSet, float>();
    private float noActionLogCooldownUntil;

    public override void Initializtion(CharacterSO data)
    {
        base.Initializtion(data);
        EnsureFallbackActions();
        isBestActionEnd = true;
        ClearPathSearchCache();
    }

    private void EnsureFallbackActions()
    {
        List<AIAction> actions = availableActions != null
            ? availableActions.Where((action) => action != null && action.actionset != null).ToList()
            : new List<AIAction>();

        bool hasVisitAction = actions.Any((action) =>
            action.actionset is AIShopping
            || action.actionset is AIEat
            || action.actionset is AILookAround
            || action.actionset is AIExitDungeon);
        bool hasWorkAction = actions.Any((action) => action.actionset is AIWork);

        if (hasWorkAction)
        {
            AddFallbackAction<AIWait>(actions, WaitActionPath);
        }
        else if (hasVisitAction)
        {
            AddFallbackAction<AILookAround>(actions, LookAroundActionPath);
            AddFallbackAction<AIExitDungeon>(actions, ExitDungeonActionPath);
            AddFallbackAction<AIWait>(actions, WaitActionPath);
        }

        availableActions = actions.ToArray();
    }

    private static void AddFallbackAction<T>(List<AIAction> actions, string resourcePath) where T : AIActionSet
    {
        if (actions.Any((action) => action.actionset is T)) return;

        T actionSet = Resources.Load<T>(resourcePath);
        if (actionSet == null)
        {
            actionSet = ScriptableObject.CreateInstance<T>();
        }

        actions.Add(new AIAction { actionset = actionSet });
    }
    public bool DecideAction()
    {
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

        float highestScore = float.MinValue;
        AIAction tempBestAction = null;
        isBestActionEnd = false;
        isExecuted = false;

        foreach (var action in availableActions)
        {
            if (action == null) continue;

            float actionScore = action.CalculateScore(character);
            if (actionScore <= 0f) continue;
            if (IsActionCoolingDown(action.actionset)) continue;
            if (!action.actionset.CanStart(character)) continue;

            if (!action.SetDestination(character, out string failureReason))
            {
                RecordActionFailure(action.actionset, failureReason);
                continue;
            }

            if (actionScore > highestScore)
            {
                highestScore = actionScore;
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
    }

    public GridPathSearchResult GetPathSearch(Character character)
    {
        if (character == null || GridSystemManager.Instance.grid == null) return null;

        Grid grid = GridSystemManager.Instance.grid;
        Vector2Int start = character.GetNowXY();
        if (pathSearchCache == null ||
            pathSearchCache.sourceGrid != grid ||
            pathSearchCache.start != start ||
            pathSearchCache.gridVersion != grid.version)
        {
            pathSearchCache = grid.SearchPath(start);
        }
        return pathSearchCache;
    }

    public void ClearPathSearchCache()
    {
        pathSearchCache = null;
    }

    private bool IsActionCoolingDown(AIActionSet actionSet)
    {
        return actionSet != null
            && actionFailureCooldownUntil.TryGetValue(actionSet, out float until)
            && Time.time < until;
    }

    private void RecordActionFailure(AIActionSet actionSet, string reason)
    {
        if (actionSet == null) return;

        actionFailureCooldownUntil[actionSet] = Time.time + Mathf.Max(0.1f, actionFailureCooldown);
        character?.AddLog($"AI 실패: {GetActionLabel(actionSet)} - {reason}");
    }

    private void RecordNoActionFailure()
    {
        if (Time.time < noActionLogCooldownUntil) return;

        noActionLogCooldownUntil = Time.time + Mathf.Max(0.1f, actionFailureCooldown);
        character?.AddLog("AI 대기: 실행 가능한 행동 없음");
    }

    private static string GetActionLabel(AIActionSet actionSet)
    {
        if (actionSet == null) return "알 수 없음";
        return !string.IsNullOrWhiteSpace(actionSet.actionName)
            ? actionSet.actionName
            : actionSet.GetType().Name;
    }
}
public enum AIActionPlanKind
{
    None,
    NoDestination,
    DestinationOnly,
    MovePath
}

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
    public Queue<BuildableObject> path;
    public Queue<GridMoveStep> pathSteps;
    public AIActionPlanKind planKind;
    public float CalculateScore(Character character)
    {
        if (actionset == null)
        {
            this.score = 0f;
            return this.score;
        }

        if (actionset.considerations == null || actionset.considerations.Length == 0)
        {
            this.score = 1f;
            return this.score;
        }

        float totalScore = 1f;
        foreach (var consideration in actionset.considerations)
        {
            if (consideration == null)
            {
                this.score = 0f;
                return this.score;
            }

            totalScore *= consideration.ScoreConsideration(character);
            if (totalScore == 0f)
            {
                this.score = 0;
                return this.score;
            }
        }
        float modFactor = 1 - (1 - actionset.considerations.Length);
        float makeupValue = (1 - totalScore) * modFactor;
        this.score = totalScore + (makeupValue * totalScore);
        return this.score;
    }
    public bool SetDestination(Character character, out string failureReason)
    {
        destination = null;
        path = new Queue<BuildableObject>();
        pathSteps = new Queue<GridMoveStep>();
        planKind = AIActionPlanKind.None;
        failureReason = string.Empty;
        if (character == null || actionset == null || GridSystemManager.Instance.grid == null)
        {
            failureReason = "AI 또는 그리드 없음";
            return false;
        }

        Grid grid = GridSystemManager.Instance.grid;
        GridPathSearchResult searchResult = character.ai != null ? character.ai.GetPathSearch(character) : null;
        if (!actionset.TryResolveDestination(character, searchResult, out destination, out failureReason))
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
            path = grid.SmoothingPath(searchResult.GetPathTo(destination));
            return ResolvePathPlan(character, destination, out failureReason);
        }

        Func<Vector2Int, bool> condition = (pos) => grid.GetGridCell(pos)?.GetBuildingInlayer() == destination;
        pathSteps = grid.GetMovePath(character.GetNowXY(), condition);
        path = grid.SmoothingPath(grid.GetGridPath(character.GetNowXY(), condition));
        return ResolvePathPlan(character, destination, out failureReason);
    }

    private bool ResolvePathPlan(Character character, BuildableObject destination, out string failureReason)
    {
        failureReason = string.Empty;
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

        failureReason = "경로 없음";
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
