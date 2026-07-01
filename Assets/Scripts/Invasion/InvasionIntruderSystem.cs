using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class InvasionIntruderSettings
{
    [Min(0.1f)] public float secondsToFullFocus = 30f;
    [Min(0.1f)] public float repathIntervalSeconds = 1.5f;
    [Min(0f)] public float facilityDamageIntervalSeconds = 5f;
    [Min(0f)] public float finalCombatDamage = 45f;
    [Min(0f)] public float finalCombatWindupSeconds = 0.7f;
}

public enum InvasionIntruderState
{
    None,
    Entering,
    Searching,
    MovingToOwner,
    DamagingFacility,
    FinalCombat,
    Finished
}

public struct InvasionSpawnedEvent
{
    public Character intruder;
    public InvasionThreatSnapshot threatSnapshot;

    public InvasionSpawnedEvent(Character intruder, InvasionThreatSnapshot threatSnapshot)
    {
        this.intruder = intruder;
        this.threatSnapshot = threatSnapshot;
    }

    private static InvasionSpawnedEvent e;

    public static void Trigger(Character intruder, InvasionThreatSnapshot threatSnapshot)
    {
        e.intruder = intruder;
        e.threatSnapshot = threatSnapshot;
        EventObserver.TriggerEvent(e);
    }
}

public struct InvasionFacilityDamagedEvent
{
    public Character intruder;
    public BuildableObject facility;

    public InvasionFacilityDamagedEvent(Character intruder, BuildableObject facility)
    {
        this.intruder = intruder;
        this.facility = facility;
    }

    private static InvasionFacilityDamagedEvent e;

    public static void Trigger(Character intruder, BuildableObject facility)
    {
        e.intruder = intruder;
        e.facility = facility;
        EventObserver.TriggerEvent(e);
    }
}

public struct InvasionFinalCombatStartedEvent
{
    public Character intruder;
    public Character owner;

    public InvasionFinalCombatStartedEvent(Character intruder, Character owner)
    {
        this.intruder = intruder;
        this.owner = owner;
    }

    private static InvasionFinalCombatStartedEvent e;

    public static void Trigger(Character intruder, Character owner)
    {
        e.intruder = intruder;
        e.owner = owner;
        EventObserver.TriggerEvent(e);
    }
}

public static class InvasionIntruderPlanner
{
    public static float CalculateFocus(float elapsedSeconds, float secondsToFullFocus)
    {
        return Mathf.Clamp01(elapsedSeconds / Mathf.Max(0.1f, secondsToFullFocus));
    }

    public static Queue<GridMoveStep> GetNextPath(
        Grid grid,
        Vector2Int start,
        Vector2Int ownerPosition,
        float focus,
        out bool directPath)
    {
        directPath = false;
        if (grid == null || !grid.IsValidGridPos(start) || !grid.IsValidGridPos(ownerPosition))
        {
            return new Queue<GridMoveStep>();
        }

        if (start == ownerPosition)
        {
            directPath = true;
            return new Queue<GridMoveStep>();
        }

        if (focus >= 0.95f)
        {
            directPath = true;
            return grid.GetMovePath(start, (pos) => pos == ownerPosition);
        }

        GridPathSearchResult searchResult = grid.SearchPath(start);
        Vector2Int exploreTarget = SelectExploreTarget(grid, searchResult, ownerPosition, focus);
        if (exploreTarget == start)
        {
            directPath = true;
            return grid.GetMovePath(start, (pos) => pos == ownerPosition);
        }

        return searchResult.GetMovePath((pos) => pos == exploreTarget);
    }

    public static Vector2Int SelectExploreTarget(
        Grid grid,
        GridPathSearchResult searchResult,
        Vector2Int ownerPosition,
        float focus)
    {
        if (grid == null || searchResult == null)
        {
            return Vector2Int.zero;
        }

        List<Vector2Int> candidates = searchResult.GetReachablePositions()
            .Where((pos) => pos != searchResult.start && grid.IsWalkable(pos))
            .ToList();

        if (candidates.Count == 0)
        {
            return searchResult.start;
        }

        if (focus <= 0.01f && candidates.Count > 1)
        {
            candidates.Remove(ownerPosition);
        }

        if (candidates.Count == 0)
        {
            return searchResult.start;
        }

        int maxDistance = Mathf.Max(1, candidates.Max((pos) => Manhattan(pos, ownerPosition)));
        float clampedFocus = Mathf.Clamp01(focus);

        return candidates
            .OrderByDescending((pos) =>
            {
                float closeness = 1f - ((float)Manhattan(pos, ownerPosition) / maxDistance);
                float explorationNoise = UnityEngine.Random.value;
                return Mathf.Lerp(explorationNoise, closeness, clampedFocus);
            })
            .First();
    }

    public static bool IsAtOwner(Character intruder, Character owner)
    {
        if (intruder == null || owner == null || GridSystemManager.Instance == null || GridSystemManager.Instance.grid == null)
        {
            return false;
        }

        Grid grid = GridSystemManager.Instance.grid;
        return grid.GetXY(intruder.transform.position) == grid.GetXY(owner.transform.position);
    }

    private static int Manhattan(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}

public class InvasionDirectorRuntime : MonoBehaviour, UtilEventListener<InvasionCandidateEvent>
{
    private const string DefaultIntruderPath = "SO/Character/Intruders/Intruder_Breakthrough";

    [SerializeField] private CharacterSO intruderData;
    [SerializeField] private GameObject intruderPrefab;
    [SerializeField] private InvasionIntruderSettings intruderSettings = new InvasionIntruderSettings();

    private readonly List<InvasionIntruderRuntime> activeIntruders = new List<InvasionIntruderRuntime>();

    public IReadOnlyList<InvasionIntruderRuntime> ActiveIntruders => activeIntruders;

    public void OnTriggerEvent(InvasionCandidateEvent eventType)
    {
        TrySpawnIntruder(eventType.snapshot, out _);
    }

    public bool TrySpawnIntruder(InvasionThreatSnapshot snapshot, out Character intruder)
    {
        intruder = null;
        CharacterSO data = ResolveIntruderData();
        if (data == null)
        {
            EventAlertService.RaiseInvasionResult("침입자 데이터가 없어 침입을 시작하지 못했습니다.", EventAlertImportance.High);
            return false;
        }

        Grid grid = GridSystemManager.Instance != null ? GridSystemManager.Instance.grid : null;
        if (grid == null || !TryResolveEntry(grid, out Vector2Int entryGridPosition, out Vector3 outsidePosition, out Vector3 entryDoorPosition))
        {
            EventAlertService.RaiseInvasionResult("침입자가 들어올 수 있는 입구를 찾지 못했습니다.", EventAlertImportance.High);
            return false;
        }

        GameObject intruderObject = intruderPrefab != null
            ? Instantiate(intruderPrefab)
            : new GameObject("Breakthrough Intruder");
        intruderObject.transform.position = outsidePosition;

        InvasionIntruderRuntime runtime = EnsureIntruderComponents(intruderObject);
        intruder = runtime.Intruder;
        InvasionIntruderSettings effectiveSettings = RunVariableRuntime.Instance != null
            ? RunVariableRuntime.Instance.ApplyInvasionSettings(intruderSettings)
            : intruderSettings;
        runtime.Begin(data, snapshot, effectiveSettings, outsidePosition, entryDoorPosition, entryGridPosition);
        activeIntruders.Add(runtime);
        runtime.OnFinished += OnIntruderFinished;

        InvasionStartedEvent.Trigger(snapshot);
        InvasionSpawnedEvent.Trigger(intruder, snapshot);
        EventAlertService.Raise(
            "침입 시작",
            "모험가가 던전 입구로 접근하고 있습니다.",
            EventAlertImportance.High,
            "침입");
        return true;
    }

    private void OnEnable()
    {
        this.EventStartListening<InvasionCandidateEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<InvasionCandidateEvent>();
    }

    private CharacterSO ResolveIntruderData()
    {
        if (intruderData != null)
        {
            return intruderData;
        }

        intruderData = Resources.Load<CharacterSO>(DefaultIntruderPath);
        return intruderData;
    }

    private static bool TryResolveEntry(
        Grid grid,
        out Vector2Int entryGridPosition,
        out Vector3 outsidePosition,
        out Vector3 entryDoorPosition)
    {
        CharacterSpawner spawner = FindFirstObjectByType<CharacterSpawner>();
        if (spawner != null && spawner.TryGetEntryGridPosition(out entryGridPosition))
        {
            outsidePosition = spawner.GetOutsideSpawnWorldPosition();
            entryDoorPosition = spawner.GetEntryDoorWorldPosition();
            return true;
        }

        if (grid.TryFindNearestWalkablePosition(Vector2Int.zero, out entryGridPosition))
        {
            entryDoorPosition = grid.GetWorldPos(entryGridPosition);
            outsidePosition = entryDoorPosition + new Vector3(2f, 0f, 0f);
            return true;
        }

        outsidePosition = Vector3.zero;
        entryDoorPosition = Vector3.zero;
        return false;
    }

    private static InvasionIntruderRuntime EnsureIntruderComponents(GameObject intruderObject)
    {
        EnsureCharacterVisual(intruderObject);

        if (!intruderObject.TryGetComponent(out Character _))
        {
            intruderObject.AddComponent<Character>();
        }

        if (!intruderObject.TryGetComponent(out AbilityMove _))
        {
            intruderObject.AddComponent<AbilityMove>();
        }

        if (!intruderObject.TryGetComponent(out Collider2D _))
        {
            BoxCollider2D collider = intruderObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.8f, 1.6f);
            collider.offset = new Vector2(0f, 0.8f);
        }

        if (!intruderObject.TryGetComponent(out InvasionIntruderRuntime runtime))
        {
            runtime = intruderObject.AddComponent<InvasionIntruderRuntime>();
        }

        runtime.Intruder.RefreshAbilityCache();
        return runtime;
    }

    private static SpriteRenderer EnsureCharacterVisual(GameObject characterObject)
    {
        Transform visual = characterObject.transform.Find("Visual");
        if (visual == null)
        {
            GameObject visualObject = new GameObject("Visual");
            visual = visualObject.transform;
            visual.SetParent(characterObject.transform, false);
        }

        if (!visual.TryGetComponent(out SpriteRenderer renderer))
        {
            renderer = visual.gameObject.AddComponent<SpriteRenderer>();
        }

        return renderer;
    }

    private void OnIntruderFinished(InvasionIntruderRuntime runtime)
    {
        if (runtime == null)
        {
            return;
        }

        runtime.OnFinished -= OnIntruderFinished;
        activeIntruders.Remove(runtime);
    }
}

public class InvasionIntruderRuntime : MonoBehaviour
{
    private Character intruder;
    private AbilityMove move;
    private InvasionIntruderSettings settings;
    private float elapsed;
    private float nextDamageTime;
    private Coroutine routine;
    private bool resolved;

    public Character Intruder => intruder != null ? intruder : GetComponent<Character>();
    public InvasionIntruderState State { get; private set; }
    public float Focus => settings != null ? InvasionIntruderPlanner.CalculateFocus(elapsed, settings.secondsToFullFocus) : 1f;

    public event Action<InvasionIntruderRuntime> OnFinished;

    private void Awake()
    {
        intruder = GetComponent<Character>();
        move = GetComponent<AbilityMove>();
    }

    public void Begin(
        CharacterSO data,
        InvasionThreatSnapshot threatSnapshot,
        InvasionIntruderSettings settings,
        Vector3 outsidePosition,
        Vector3 entryDoorPosition,
        Vector2Int entryGridPosition)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
        }

        this.settings = settings ?? new InvasionIntruderSettings();
        elapsed = 0f;
        nextDamageTime = this.settings.facilityDamageIntervalSeconds;
        resolved = false;
        EnsureRuntimeComponents();

        transform.position = outsidePosition;
        intruder.SetLifecycleState(Character.LifecycleState.SpawningOutside);
        intruder.Initialization(data);
        intruder.SetLifecycleState(Character.LifecycleState.SpawningOutside);
        routine = StartCoroutine(Run(entryDoorPosition, entryGridPosition));
    }

    public Queue<GridMoveStep> CreateNextPath(Grid grid, Vector2Int ownerPosition, out bool direct)
    {
        Vector2Int start = intruder != null ? intruder.GetNowXY() : Vector2Int.zero;
        return InvasionIntruderPlanner.GetNextPath(grid, start, ownerPosition, Focus, out direct);
    }

    public bool TryDamageNearbyFacility(Grid grid)
    {
        if (grid == null || intruder == null)
        {
            return false;
        }

        Vector2Int current = intruder.GetNowXY();
        Vector2Int[] positions =
        {
            current,
            current + Vector2Int.left,
            current + Vector2Int.right
        };

        foreach (Vector2Int position in positions)
        {
            GridCell cell = grid.GetGridCell(position);
            if (cell == null) continue;

            BuildableObject target = cell.GetAllOccupants()
                .OfType<BuildableObject>()
                .FirstOrDefault((building) => building != null
                    && !building.isDestroy
                    && !building.IsDamaged
                    && !building.IsGridMovement
                    && building.Facility != null);
            if (target == null) continue;

            State = InvasionIntruderState.DamagingFacility;
            target.SetDamaged(true);
            intruder.AddLog($"{target.name} 파손");
            InvasionFacilityDamagedEvent.Trigger(intruder, target);
            return true;
        }

        return false;
    }

    public void ApplyFinalCombat(Character owner)
    {
        if (owner == null || intruder == null || settings == null)
        {
            return;
        }

        State = InvasionIntruderState.FinalCombat;
        InvasionFinalCombatStartedEvent.Trigger(intruder, owner);
        owner.ApplyDamage(settings.finalCombatDamage, "침입자 최종 교전");
        resolved = true;
        InvasionResolvedEvent.Trigger(owner.IsDead == false, owner.IsDead ? 5f : 2f);
    }

    public void ResolveSuppressedBy(Character defender)
    {
        if (resolved)
        {
            Finish();
            return;
        }

        resolved = true;
        State = InvasionIntruderState.Finished;
        intruder?.AddLog(defender != null
            ? $"제압됨: {defender.name}"
            : "제압됨");
        InvasionResolvedEvent.Trigger(true, 1f);
        Finish();
    }

    private IEnumerator Run(Vector3 entryDoorPosition, Vector2Int entryGridPosition)
    {
        State = InvasionIntruderState.Entering;
        yield return move.Move2PosBySpeed(entryDoorPosition);

        Grid grid = GridSystemManager.Instance != null ? GridSystemManager.Instance.grid : null;
        if (grid != null && grid.IsValidGridPos(entryGridPosition))
        {
            yield return move.Move2PosBySpeed(grid.GetWorldPos(entryGridPosition));
        }

        intruder.SetLifecycleState(Character.LifecycleState.Active);
        while (State != InvasionIntruderState.Finished && intruder != null && !intruder.IsDead)
        {
            grid = GridSystemManager.Instance != null ? GridSystemManager.Instance.grid : null;
            Character owner = OwnerRunManager.Instance != null ? OwnerRunManager.Instance.CurrentOwner : null;
            if (grid == null || owner == null || owner.IsDead)
            {
                Finish();
                yield break;
            }

            if (InvasionIntruderPlanner.IsAtOwner(intruder, owner))
            {
                yield return FinalCombat(owner);
                yield break;
            }

            elapsed += Mathf.Max(0.01f, settings.repathIntervalSeconds);
            bool direct;
            Queue<GridMoveStep> path = CreateNextPath(grid, owner.GetNowXY(), out direct);
            State = direct ? InvasionIntruderState.MovingToOwner : InvasionIntruderState.Searching;

            if (Time.time >= nextDamageTime)
            {
                TryDamageNearbyFacility(grid);
                nextDamageTime = Time.time + settings.facilityDamageIntervalSeconds;
            }

            if (path.Count == 0)
            {
                TickDefenseStatuses(settings.repathIntervalSeconds);
                yield return new WaitForSeconds(settings.repathIntervalSeconds);
                continue;
            }

            yield return MovePathWithDefense(grid, path);
            if (intruder == null || intruder.IsDead)
            {
                ResolveIntruderDefeated();
                yield break;
            }

            yield return null;
        }

        if (intruder != null && intruder.IsDead && State != InvasionIntruderState.Finished)
        {
            ResolveIntruderDefeated();
        }
    }

    private IEnumerator MovePathWithDefense(Grid grid, Queue<GridMoveStep> path)
    {
        if (path == null)
        {
            yield break;
        }

        while (path.Count > 0 && intruder != null && !intruder.IsDead)
        {
            GridMoveStep step = path.Dequeue();
            if (step == null) continue;

            yield return move.MoveByStep(step);
            List<DefenseActivationReport> reports = DefenseFacilityResolver.TriggerAt(
                grid,
                intruder,
                step.To,
                DefenseTriggerTiming.OnEnter);
            TickDefenseStatuses(settings.repathIntervalSeconds);

            if (intruder.IsDead)
            {
                yield break;
            }

            float delay = reports.Count > 0
                ? reports.Max((report) => report.MovementDelaySeconds)
                : 0f;
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }
        }
    }

    private void TickDefenseStatuses(float deltaSeconds)
    {
        if (intruder == null || intruder.IsDead)
        {
            return;
        }

        DefenseEffectResolver.TickStatuses(intruder, deltaSeconds);
    }

    private IEnumerator FinalCombat(Character owner)
    {
        State = InvasionIntruderState.FinalCombat;
        if (settings.finalCombatWindupSeconds > 0f)
        {
            yield return new WaitForSeconds(settings.finalCombatWindupSeconds);
        }

        ApplyFinalCombat(owner);
        Finish();
    }

    private void ResolveIntruderDefeated()
    {
        if (!resolved)
        {
            resolved = true;
            InvasionResolvedEvent.Trigger(true, 1f);
        }

        Finish();
    }

    private void Finish()
    {
        State = InvasionIntruderState.Finished;
        if (intruder != null)
        {
            intruder.SetLifecycleState(Character.LifecycleState.Despawned);
        }

        OnFinished?.Invoke(this);
        Destroy(gameObject);
    }

    private void EnsureRuntimeComponents()
    {
        intruder = GetComponent<Character>();
        move = GetComponent<AbilityMove>();
        if (intruder == null)
        {
            intruder = gameObject.AddComponent<Character>();
        }

        if (move == null)
        {
            move = gameObject.AddComponent<AbilityMove>();
        }

        intruder.RefreshAbilityCache();
    }
}
