using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public class InvasionDirectorRuntime : MonoBehaviour, UtilEventListener<InvasionCandidateEvent>
{
    [SerializeField] private CharacterSO intruderData;
    [SerializeField] private GameObject intruderPrefab;
    [SerializeField] private InvasionIntruderSettings intruderSettings = new InvasionIntruderSettings();

    private readonly List<InvasionIntruderRuntime> activeIntruders = new List<InvasionIntruderRuntime>();
    private IInvasionIntruderContext invasionContext;
    private IInvasionIntruderDataProvider intruderDataProvider;
    private IInvasionIntruderFactory intruderFactory;
    private IDefenseStatusRuntimeService defenseStatusRuntimeService;

    public IReadOnlyList<InvasionIntruderRuntime> ActiveIntruders => activeIntruders;

    [Inject]
    public void Construct(
        IInvasionIntruderContext invasionContext,
        IInvasionIntruderDataProvider intruderDataProvider,
        IInvasionIntruderFactory intruderFactory,
        IDefenseStatusRuntimeService defenseStatusRuntimeService)
    {
        this.invasionContext = invasionContext
            ?? throw new ArgumentNullException(nameof(invasionContext));
        this.intruderDataProvider = intruderDataProvider
            ?? throw new ArgumentNullException(nameof(intruderDataProvider));
        this.intruderFactory = intruderFactory
            ?? throw new ArgumentNullException(nameof(intruderFactory));
        this.defenseStatusRuntimeService = defenseStatusRuntimeService
            ?? throw new ArgumentNullException(nameof(defenseStatusRuntimeService));
    }

    public void OnTriggerEvent(InvasionCandidateEvent eventType)
    {
        TrySpawnIntruder(eventType.snapshot, out _);
    }

    public bool TrySpawnIntruder(InvasionThreatSnapshot snapshot, out CharacterActor intruder)
    {
        intruder = null;
        CharacterSO data = ResolveIntruderData();
        if (data == null)
        {
            EventAlertService.RaiseInvasionResult("침입자 데이터가 없어 침입을 시작하지 못했습니다.", EventAlertImportance.High);
            return false;
        }

        IInvasionIntruderContext context = ResolveInvasionContext();
        if (!context.TryResolveEntry(out InvasionIntruderEntry entry))
        {
            EventAlertService.RaiseInvasionResult("침입자가 들어올 수 있는 입구를 찾지 못했습니다.", EventAlertImportance.High);
            return false;
        }

        InvasionIntruderRuntime runtime = ResolveIntruderFactory().Create(intruderPrefab, entry.OutsidePosition);
        runtime.Initialize(context, ResolveDefenseStatusRuntimeService());
        intruder = runtime.IntruderActor;
        InvasionIntruderSettings effectiveSettings = context.ApplyRunVariables(intruderSettings);
        runtime.Begin(data, snapshot, effectiveSettings, entry.OutsidePosition, entry.DoorPosition, entry.GridPosition);
        activeIntruders.Add(runtime);
        runtime.OnFinished += OnIntruderFinished;

        InvasionStartedEvent.Trigger(snapshot);
        InvasionSpawnedEvent.Trigger(intruder, snapshot);
        EventAlertService.Raise(
            "침입 시작",
            "침입자가 던전 입구로 접근하고 있습니다.",
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
        intruderData = ResolveIntruderDataProvider().GetRequiredIntruderData(intruderData);
        return intruderData;
    }

    private IInvasionIntruderDataProvider ResolveIntruderDataProvider()
    {
        return intruderDataProvider
            ?? throw new InvalidOperationException($"{nameof(InvasionDirectorRuntime)} requires {nameof(IInvasionIntruderDataProvider)} injection.");
    }

    private IInvasionIntruderContext ResolveInvasionContext()
    {
        return invasionContext
            ?? throw new InvalidOperationException($"{nameof(InvasionDirectorRuntime)} requires {nameof(IInvasionIntruderContext)} injection.");
    }

    private IInvasionIntruderFactory ResolveIntruderFactory()
    {
        return intruderFactory
            ?? throw new InvalidOperationException($"{nameof(InvasionDirectorRuntime)} requires {nameof(IInvasionIntruderFactory)} injection.");
    }

    private IDefenseStatusRuntimeService ResolveDefenseStatusRuntimeService()
    {
        return defenseStatusRuntimeService
            ?? throw new InvalidOperationException($"{nameof(InvasionDirectorRuntime)} requires {nameof(IDefenseStatusRuntimeService)} injection.");
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
    private CharacterActor intruderActor;
    private AbilityMove move;
    private InvasionIntruderSettings settings;
    private float elapsed;
    private float nextDamageTime;
    private Coroutine routine;
    private bool resolved;
    private IInvasionIntruderContext invasionContext;
    private IDefenseStatusRuntimeService defenseStatusRuntimeService;

    public CharacterActor IntruderActor => intruderActor != null ? intruderActor : GetComponent<CharacterActor>();
    public InvasionIntruderState State { get; private set; }
    public float Focus => settings != null ? InvasionIntruderPlanner.CalculateFocus(elapsed, settings.secondsToFullFocus) : 1f;

    public event Action<InvasionIntruderRuntime> OnFinished;

    private void Awake()
    {
        intruderActor = GetComponent<CharacterActor>();
        move = GetComponent<AbilityMove>();
    }

    public void Initialize(
        IInvasionIntruderContext invasionContext,
        IDefenseStatusRuntimeService defenseStatusRuntimeService)
    {
        this.invasionContext = invasionContext
            ?? throw new ArgumentNullException(nameof(invasionContext));
        this.defenseStatusRuntimeService = defenseStatusRuntimeService
            ?? throw new ArgumentNullException(nameof(defenseStatusRuntimeService));
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
        RequireRuntimeComponents();

        transform.position = outsidePosition;
        intruderActor.SetLifecycleState(CharacterLifecycleState.SpawningOutside);
        intruderActor.Initialize(data);
        intruderActor.SetLifecycleState(CharacterLifecycleState.SpawningOutside);
        routine = StartCoroutine(Run(entryDoorPosition, entryGridPosition));
    }

    public Queue<GridMoveStep> CreateNextPath(Grid grid, Vector2Int ownerPosition, out bool direct)
    {
        Vector2Int start = intruderActor != null ? intruderActor.GetNowXY() : Vector2Int.zero;
        return InvasionIntruderPlanner.GetNextPath(grid, start, ownerPosition, Focus, out direct);
    }

    public bool TryDamageNearbyFacility(Grid grid)
    {
        if (grid == null || intruderActor == null)
        {
            return false;
        }

        if (!InvasionFacilityDamageResolver.TryFindDamageTarget(grid, intruderActor.GetNowXY(), out BuildableObject target))
        {
            return false;
        }

        State = InvasionIntruderState.DamagingFacility;
        target.SetDamaged(true);
        intruderActor.AddLog($"{target.name} 손상");
        InvasionFacilityDamagedEvent.Trigger(intruderActor, target);
        return true;
    }

    public void ApplyFinalCombat(CharacterActor owner)
    {
        if (owner == null || intruderActor == null || settings == null)
        {
            return;
        }

        State = InvasionIntruderState.FinalCombat;
        InvasionFinalCombatStartedEvent.Trigger(intruderActor, owner);
        owner.ApplyDamage(settings.finalCombatDamage, "침입자 최종 교전");
        resolved = true;
        InvasionResolvedEvent.Trigger(!owner.IsDead, owner.IsDead ? 5f : 2f);
    }

    public void ResolveSuppressedBy(CharacterActor defender)
    {
        if (resolved)
        {
            Finish();
            return;
        }

        resolved = true;
        State = InvasionIntruderState.Finished;
        intruderActor?.AddLog(defender != null
            ? $"Suppressed by {defender.name}"
            : "Suppressed.");
        InvasionResolvedEvent.Trigger(true, 1f);
        Finish();
    }

    private IEnumerator Run(Vector3 entryDoorPosition, Vector2Int entryGridPosition)
    {
        State = InvasionIntruderState.Entering;
        yield return move.Move2PosBySpeed(entryDoorPosition);

        IInvasionIntruderContext context = ResolveInvasionContext();
        context.TryGetGrid(out Grid grid);
        if (grid != null && grid.IsValidGridPos(entryGridPosition))
        {
            yield return move.Move2PosBySpeed(grid.GetWorldPos(entryGridPosition));
        }

        intruderActor.SetLifecycleState(CharacterLifecycleState.Active);
        while (State != InvasionIntruderState.Finished && intruderActor != null && !intruderActor.IsDead)
        {
            context.TryGetGrid(out grid);
            context.TryGetOwner(out CharacterActor owner);
            if (grid == null || owner == null || owner.IsDead)
            {
                Finish();
                yield break;
            }

            if (InvasionIntruderPlanner.IsAtOwner(grid, intruderActor, owner))
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
            if (intruderActor == null || intruderActor.IsDead)
            {
                ResolveIntruderDefeated();
                yield break;
            }

            yield return null;
        }

        if (intruderActor != null && intruderActor.IsDead && State != InvasionIntruderState.Finished)
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

        while (path.Count > 0 && intruderActor != null && !intruderActor.IsDead)
        {
            GridMoveStep step = path.Dequeue();
            if (step == null) continue;

            yield return move.MoveByStep(step);
            if (move.LastGridMoveWasBlocked)
            {
                yield break;
            }

            List<DefenseActivationReport> reports = DefenseFacilityResolver.TriggerAt(
                grid,
                intruderActor,
                step.To,
                DefenseTriggerTiming.OnEnter,
                ResolveDefenseStatusRuntimeService());
            TickDefenseStatuses(settings.repathIntervalSeconds);

            if (intruderActor.IsDead)
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
        if (intruderActor == null || intruderActor.IsDead)
        {
            return;
        }

        DefenseEffectResolver.TickStatuses(
            intruderActor,
            deltaSeconds,
            ResolveDefenseStatusRuntimeService());
    }

    private IEnumerator FinalCombat(CharacterActor owner)
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
        if (intruderActor != null)
        {
            intruderActor.SetLifecycleState(CharacterLifecycleState.Despawned);
        }

        OnFinished?.Invoke(this);
        Destroy(gameObject);
    }

    private void RequireRuntimeComponents()
    {
        intruderActor = GetComponent<CharacterActor>();
        move = GetComponent<AbilityMove>();
        if (intruderActor == null)
        {
            throw new InvalidOperationException(
                $"{nameof(InvasionIntruderRuntime)} requires {nameof(CharacterActor)} prepared by {nameof(IInvasionIntruderFactory)}.");
        }

        if (move == null)
        {
            throw new InvalidOperationException(
                $"{nameof(InvasionIntruderRuntime)} requires {nameof(AbilityMove)} prepared by {nameof(IInvasionIntruderFactory)}.");
        }

        intruderActor.EnsureRuntimeState();
        intruderActor.AbilityCache?.RefreshAbilityCache();
    }

    private IInvasionIntruderContext ResolveInvasionContext()
    {
        return invasionContext
            ?? throw new InvalidOperationException($"{nameof(InvasionIntruderRuntime)} requires {nameof(IInvasionIntruderContext)} initialization.");
    }

    private IDefenseStatusRuntimeService ResolveDefenseStatusRuntimeService()
    {
        return defenseStatusRuntimeService
            ?? throw new InvalidOperationException($"{nameof(InvasionIntruderRuntime)} requires {nameof(IDefenseStatusRuntimeService)} initialization.");
    }
}
