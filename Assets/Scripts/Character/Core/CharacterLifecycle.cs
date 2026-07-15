using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

[DisallowMultipleComponent]
public class CharacterLifecycle : SerializedMonoBehaviour
{
    [SerializeField]
    [ReadOnly]
    private CharacterActor actor;
    [SerializeField]
    [ReadOnly]
    private CharacterIdentity identity;
    [SerializeField]
    [ReadOnly]
    private CharacterAbilityCache abilityCache;
    [SerializeField]
    [ReadOnly]
    private CharacterVisual visual;
    [SerializeField]
    [ReadOnly]
    private CharacterStats stats;
    [SerializeField]
    [ReadOnly]
    private CharacterLog log;
    [SerializeField]
    [ReadOnly]
    private CharacterLifecycleState lifecycleState = CharacterLifecycleState.None;
    private IGridSystemProvider gridSystemProvider;
    private GridSystemManager subscribedGridSystem;

    public CharacterLifecycleState CurrentState => lifecycleState;

    [Inject]
    public void ConstructCharacterLifecycle(IGridSystemProvider gridSystemProvider)
    {
        this.gridSystemProvider = gridSystemProvider
            ?? throw new System.ArgumentNullException(nameof(gridSystemProvider));
        TrySubscribeToGridChanges();
    }

    private void Awake()
    {
        Bind(GetComponent<CharacterActor>());
    }

    private void Start()
    {
        TrySubscribeToGridChanges();
    }

    private void OnEnable()
    {
        TrySubscribeToGridChanges();
    }

    private void OnDisable()
    {
        UnsubscribeFromGridChanges();
    }

    private void OnDestroy()
    {
        UnsubscribeFromGridChanges();
    }

    public void Bind(CharacterActor owner)
    {
        actor = owner;
        identity = GetComponent<CharacterIdentity>();
        abilityCache = GetComponent<CharacterAbilityCache>();
        visual = GetComponent<CharacterVisual>();
        stats = GetComponent<CharacterStats>();
        log = GetComponent<CharacterLog>();
    }

    public void SetAiPaused(bool value)
    {
        SetLifecycleState(value ? CharacterLifecycleState.EnteringDungeon : CharacterLifecycleState.Active);
    }

    public bool BeginExpedition()
    {
        if (actor == null
            || (stats != null && stats.IsDead)
            || (identity != null && identity.IsOwner)
            || lifecycleState == CharacterLifecycleState.OnExpedition)
        {
            return false;
        }

        if (abilityCache != null && abilityCache.TryGetAbility(out AbilityWork work))
        {
            work.PrepareForExpedition();
        }

        log?.AddLog("원정 출발");
        SetLifecycleState(CharacterLifecycleState.OnExpedition);
        visual?.SetRenderersVisible(false);
        return true;
    }

    public void EndExpedition(bool alive = true)
    {
        if (actor == null)
        {
            return;
        }

        if (alive && (stats == null || !stats.IsDead))
        {
            visual?.SetRenderersVisible(true);
            SetLifecycleState(CharacterLifecycleState.Active);
            log?.AddLog("원정 복귀");
            return;
        }

        visual?.SetRenderersVisible(true);
        stats?.Die("원정 중 사망");
    }

    public void SetLifecycleState(CharacterLifecycleState nextState)
    {
        lifecycleState = nextState;

        if (actor == null)
        {
            return;
        }

        if (nextState == CharacterLifecycleState.Active)
        {
            visual?.EnsureVisibleForActiveLifecycle();
            if (actor.Brain == null) return;

            actor.Brain.RequestImmediateReplan(clearFailures: true);
            return;
        }

        if (actor.Brain == null) return;

        actor.Brain.bestAction = null;
        actor.Brain.isExecuted = false;
        actor.Brain.isBestActionEnd = false;
        actor.Brain.ClearPathSearchCache();
    }

    public Vector2Int GetNowXY()
    {
        if (!TryGetGrid(out Grid grid))
        {
            return Vector2Int.zero;
        }

        Vector2Int startPos = grid.GetXY(transform.position);
        startPos = grid.IsValidGridPos(startPos) ? startPos : Vector2Int.zero;
        return startPos;
    }

    public IEnumerator SnapToWalkableGridWhenReady()
    {
        for (int i = 0; i < 30; i++)
        {
            if (lifecycleState == CharacterLifecycleState.SpawningOutside
                || lifecycleState == CharacterLifecycleState.EnteringDungeon
                || lifecycleState == CharacterLifecycleState.ExitingDungeon
                || lifecycleState == CharacterLifecycleState.OnExpedition
                || lifecycleState == CharacterLifecycleState.Despawned)
            {
                yield break;
            }

            if (TryGetGrid(out Grid grid))
            {
                Vector2Int currentPos = grid.GetXY(transform.position);
                if (grid.IsValidGridPos(currentPos) && grid.IsWalkable(currentPos))
                {
                    yield break;
                }

                if (grid.TryFindNearestWalkablePositionOnSameFloor(currentPos, out Vector2Int walkablePosition))
                {
                    transform.position = grid.GetWorldPos(walkablePosition);
                    actor?.Brain?.ClearPathSearchCache();
                    yield break;
                }
            }

            yield return null;
        }
    }

    private void TrySubscribeToGridChanges()
    {
        if (subscribedGridSystem != null
            || gridSystemProvider == null
            || !gridSystemProvider.TryGetManager(out GridSystemManager manager))
        {
            return;
        }

        subscribedGridSystem = manager;
        subscribedGridSystem.OnGridObjectChanged += HandleGridObjectChanged;
    }

    private void UnsubscribeFromGridChanges()
    {
        if (subscribedGridSystem == null)
        {
            return;
        }

        subscribedGridSystem.OnGridObjectChanged -= HandleGridObjectChanged;
        subscribedGridSystem = null;
    }

    private void HandleGridObjectChanged()
    {
        if (lifecycleState != CharacterLifecycleState.Active || !TryGetGrid(out Grid grid))
        {
            return;
        }

        Vector2Int currentPos = grid.GetXY(transform.position);
        if (!grid.IsValidGridPos(currentPos) || !grid.IsMovementBlockedByWall(currentPos))
        {
            return;
        }

        actor?.GetAbility<AbilityMove>()?.CancelActiveMovement();
        if (grid.TryFindNearestWalkablePositionOnSameFloor(currentPos, out Vector2Int walkablePosition))
        {
            transform.position = grid.GetWorldPos(walkablePosition);
        }

        actor?.Brain?.RequestImmediateReplan(clearFailures: true);
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
}
