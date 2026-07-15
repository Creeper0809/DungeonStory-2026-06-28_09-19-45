using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class BuildableObject : MonoBehaviour, IGridOccupant, IGridMovementOccupant
{
    private const float DefaultAiReservationSeconds = 12f;
    private static readonly IFacilityCandidateCache FallbackFacilityCandidateCache =
        new FacilityCandidateCacheStore();
    private static readonly IRoomFacilityPolicy FallbackRoomFacilityPolicy =
        new RoomFacilityPolicyService(new RoomLayoutCache());
    private static readonly IBlueprintResearchWorkService FallbackBlueprintResearchWorkService =
        new BuildableObjectNoopBlueprintResearchWorkService();
    private static readonly IWorldInfoClickSelector FallbackWorldInfoClickSelector =
        new BuildableObjectNoopWorldInfoClickSelector();

    public int id { get; private set; }
    public Vector2Int centerPos { get; protected set; }
    public List<Vector2Int> buildPoses { get; private set; }
    public BuildingSO BuildingData { get; private set; }

    protected Grid grid;
    public BuildingCategory category { get; private set; }

    public event Action OnBuildingDestroyed;
    public event Action<BuildableObject> OnBuildingClicked;
    public bool isDestroy;
    [SerializeField] private bool isDamaged;
    [SerializeField] private int facilityLevel = 1;
    [SerializeField] private FacilityOperationalState operationalState = new FacilityOperationalState();
    private int currentUserCount;
    private readonly Dictionary<CharacterActor, float> visitReservations = new Dictionary<CharacterActor, float>();
    private CharacterActor workerReservation;
    private float workerReservationUntil;
    private IBlueprintResearchWorkService blueprintResearchWorkService;
    private IWorldInfoClickSelector worldInfoClickSelector;
    private IFacilityCandidateCache facilityCandidateCache;
    private IRoomFacilityPolicy roomFacilityPolicy;

    public int GridId => id;
    public bool IsGridDestroyed => isDestroy;
    public bool IsGridVisitable => isVisitable();
    public bool IsGridMovement => category == BuildingCategory.Movement;
    public virtual GridMoveType GridMoveType => IsGridMovement ? GridMoveType.Instant : GridMoveType.Walk;
    public Grid Grid => grid;
    public FacilityData Facility => BuildingData != null ? BuildingData.Facility : null;
    public bool IsDamaged => isDamaged;
    public int FacilityLevel => facilityLevel;
    public int CurrentUserCount => currentUserCount;
    public FacilityOperationalData Operational => BuildingData != null ? BuildingData.Operational : null;
    public FacilityOperationalState OperationalState => operationalState ??= new FacilityOperationalState();
    public int EffectiveCapacity => ResolveRoomFacilityPolicy().GetEffectiveCapacity(this);

    public int ActiveVisitReservationCount
    {
        get
        {
            PruneExpiredVisitReservations();
            return visitReservations.Count;
        }
    }

    public CharacterActor WorkerReservation
    {
        get
        {
            PruneExpiredWorkerReservation();
            return workerReservation;
        }
    }

    public virtual void Start()
    {
    }

    [Inject]
    public void ConstructBuildableObject(
        IBlueprintResearchWorkService blueprintResearchWorkService,
        IWorldInfoClickSelector worldInfoClickSelector,
        IFacilityCandidateCache facilityCandidateCache,
        IRoomFacilityPolicy roomFacilityPolicy)
    {
        this.blueprintResearchWorkService = blueprintResearchWorkService
            ?? throw new ArgumentNullException(nameof(blueprintResearchWorkService));
        this.worldInfoClickSelector = worldInfoClickSelector
            ?? throw new ArgumentNullException(nameof(worldInfoClickSelector));
        this.facilityCandidateCache = facilityCandidateCache
            ?? throw new ArgumentNullException(nameof(facilityCandidateCache));
        this.roomFacilityPolicy = roomFacilityPolicy
            ?? throw new ArgumentNullException(nameof(roomFacilityPolicy));
    }

    public virtual void SetGrid(Grid grid)
    {
        this.grid = grid;
    }

    public virtual void Initialization(BuildingSO buildingSO, Vector2Int buildPos)
    {
        BuildingData = buildingSO;
        GridBuildingPlacement placement = buildingSO.Placement;
        id = buildingSO.id;
        isDestroy = false;
        isDamaged = false;
        facilityLevel = 1;
        currentUserCount = 0;
        visitReservations.Clear();
        workerReservation = null;
        workerReservationUntil = 0f;
        operationalState ??= new FacilityOperationalState();
        operationalState.CopyFrom(null);
        centerPos = buildPos;
        category = placement.Category;
        buildPoses = placement.GetGridPosList(buildPos);
        ModularFacilityRuntimeEffects.ConfigureVisual(this);
    }

    public virtual Vector3 GetMovementWorldPosition(Vector2Int gridPosition)
    {
        if (grid == null)
        {
            return transform.position;
        }

        Vector3 anchor = grid.GetWorldPos(gridPosition);
        if (BuildingData != null)
        {
            anchor += (Vector3)BuildingData.movementAnchorOffset;
        }

        return anchor;
    }

    public bool TryGetFacilityOccupiedWorldPosition(Vector3 fromWorld, out Vector3 worldPosition)
    {
        worldPosition = fromWorld;
        if (grid == null || buildPoses == null || buildPoses.Count == 0)
        {
            return false;
        }

        if (TryFindNearestFacilityCell(fromWorld, requireRegisteredOccupant: true, out Vector2Int facilityCell)
            || TryFindNearestFacilityCell(fromWorld, requireRegisteredOccupant: false, out facilityCell))
        {
            worldPosition = grid.GetWorldPos(facilityCell);
            return true;
        }

        return false;
    }

    public Vector3 GetFacilityAnchorWorldPosition(FacilityAnchorKind kind, Vector3 fromWorld)
    {
        return TryGetFacilityAnchorWorldPosition(kind, fromWorld, out Vector3 worldPosition)
            ? worldPosition
            : transform.position;
    }

    public bool TryGetFacilityAnchorWorldPosition(
        FacilityAnchorKind kind,
        Vector3 fromWorld,
        out Vector3 worldPosition)
    {
        worldPosition = transform.position;
        if (grid == null || buildPoses == null || buildPoses.Count == 0)
        {
            return false;
        }

        if (TryGetConfiguredFacilityAnchorWorldPosition(kind, out worldPosition))
        {
            return true;
        }

        switch (kind)
        {
            case FacilityAnchorKind.Work:
                return TryGetHorizontalFootprintAnchorWorldPosition(0.85f, out worldPosition);
            case FacilityAnchorKind.Checkout:
                return TryGetHorizontalFootprintAnchorWorldPosition(0.75f, out worldPosition);
            case FacilityAnchorKind.Use:
            case FacilityAnchorKind.Exit:
            default:
                return TryGetFacilityOccupiedWorldPosition(fromWorld, out worldPosition)
                    || TryGetHorizontalFootprintAnchorWorldPosition(0.5f, out worldPosition);
        }
    }

    public void DestroySelf()
    {
        OnBuildingDestroyed?.Invoke();
        isDestroy = true;
        MarkFacilityDynamicStateDirty();
        if (Application.isPlaying)
        {
            Destroy(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    public void SetDamaged(bool value)
    {
        if (isDamaged == value)
        {
            return;
        }

        isDamaged = value;
        MarkFacilityDynamicStateDirty();
    }

    public void SetFacilityLevel(int value)
    {
        int nextLevel = Mathf.Max(1, value);
        if (facilityLevel == nextLevel)
        {
            return;
        }

        facilityLevel = nextLevel;
        MarkFacilityDynamicStateDirty();
    }

    public bool SupportsFacilityRole(FacilityRole role)
    {
        return Facility != null && Facility.SupportsRole(role);
    }

    public bool SupportsWork(FacilityWorkType workType)
    {
        return Facility != null && Facility.SupportsWork(workType);
    }

    public bool CanVisit(CharacterActor visitor, out string failureReason)
    {
        PruneExpiredVisitReservations();
        failureReason = string.Empty;
        if (isDestroy)
        {
            failureReason = "시설 파괴됨";
            return false;
        }

        FacilityData facilityData = Facility;
        if (facilityData == null || !facilityData.IsVisitorFacility)
        {
            failureReason = "방문용 시설 아님";
            return false;
        }

        if (!ResolveRoomFacilityPolicy().IsFacilityRoleAvailable(this, facilityData.roles, out failureReason))
        {
            return false;
        }

        if (isDamaged && facilityData.disabledWhenDamaged)
        {
            failureReason = "시설 파손";
            return false;
        }

        int effectiveCapacity = EffectiveCapacity;
        if (effectiveCapacity > 0
            && currentUserCount + GetActiveVisitReservationCountExcept(visitor) >= effectiveCapacity)
        {
            failureReason = "수용 인원 초과";
            return false;
        }

        if (facilityData.requiresStock
            && this is IStockedFacility stockedFacility
            && !stockedFacility.HasAvailableStock)
        {
            failureReason = "재고 없음";
            return false;
        }

        return true;
    }

    public bool TryBeginUse(CharacterActor visitor, out string failureReason)
    {
        if (!CanVisit(visitor, out failureReason))
        {
            return false;
        }

        ReleaseVisitReservation(visitor);
        currentUserCount++;
        OperationalState.completedUses++;
        OperationalState.cleanliness = Mathf.Clamp(OperationalState.cleanliness - 1.5f, 0f, 100f);
        MarkFacilityDynamicStateDirty();
        FacilityVisitEvent.Trigger(visitor, this);
        return true;
    }

    public void EndUse(CharacterActor visitor)
    {
        if (currentUserCount > 0)
        {
            currentUserCount--;
            MarkFacilityDynamicStateDirty();
        }
    }

    public bool TryReserveVisit(CharacterActor visitor, out string failureReason, float seconds = DefaultAiReservationSeconds)
    {
        failureReason = string.Empty;
        if (visitor == null)
        {
            failureReason = "방문 예약 대상 없음";
            return false;
        }

        if (!CanVisit(visitor, out failureReason))
        {
            return false;
        }

        visitReservations[visitor] = Time.time + Mathf.Max(0.1f, seconds);
        MarkFacilityDynamicStateDirty();
        return true;
    }

    public void RefreshVisitReservation(CharacterActor visitor, float seconds = DefaultAiReservationSeconds)
    {
        if (visitor == null || !visitReservations.ContainsKey(visitor))
        {
            return;
        }

        visitReservations[visitor] = Time.time + Mathf.Max(0.1f, seconds);
    }

    public void ReleaseVisitReservation(CharacterActor visitor)
    {
        if (visitor == null)
        {
            return;
        }

        if (visitReservations.Remove(visitor))
        {
            MarkFacilityDynamicStateDirty();
        }
    }

    public bool TryReserveWorker(CharacterActor worker, out string failureReason, float seconds = DefaultAiReservationSeconds)
    {
        failureReason = string.Empty;
        if (worker == null)
        {
            failureReason = "작업 예약 대상 없음";
            return false;
        }

        PruneExpiredWorkerReservation();
        if (HasWorkerReservationForOther(worker))
        {
            failureReason = "이미 작업 예약됨";
            return false;
        }

        workerReservation = worker;
        workerReservationUntil = Time.time + Mathf.Max(0.1f, seconds);
        MarkFacilityDynamicStateDirty();
        return true;
    }

    public void RefreshWorkerReservation(CharacterActor worker, float seconds = DefaultAiReservationSeconds)
    {
        PruneExpiredWorkerReservation();
        if (worker == null || workerReservation != worker)
        {
            return;
        }

        workerReservationUntil = Time.time + Mathf.Max(0.1f, seconds);
    }

    public bool HasWorkerReservationForOther(CharacterActor worker)
    {
        PruneExpiredWorkerReservation();
        return workerReservation != null && workerReservation != worker;
    }

    public void ReleaseWorkerReservation(CharacterActor worker)
    {
        if (worker == null || workerReservation != worker)
        {
            return;
        }

        workerReservation = null;
        workerReservationUntil = 0f;
        MarkFacilityDynamicStateDirty();
    }

    public bool CanAssignWork(FacilityWorkType workType, out string failureReason)
    {
        PruneExpiredWorkerReservation();
        failureReason = string.Empty;
        if (isDestroy)
        {
            failureReason = "시설 파괴됨";
            return false;
        }

        FacilityData facilityData = Facility;
        if (facilityData == null || !facilityData.SupportsWork(workType))
        {
            failureReason = "지원하지 않는 작업";
            return false;
        }

        if (workType == FacilityWorkType.Repair && !isDamaged)
        {
            failureReason = "수리할 필요가 없음";
            return false;
        }

        if (isDamaged && facilityData.disabledWhenDamaged && workType != FacilityWorkType.Repair)
        {
            failureReason = "시설 파손";
            return false;
        }

        return true;
    }
    private int GetActiveVisitReservationCountExcept(CharacterActor visitor)
    {
        PruneExpiredVisitReservations();
        int count = 0;
        foreach (CharacterActor reservedVisitor in visitReservations.Keys)
        {
            if (reservedVisitor != null && reservedVisitor != visitor)
            {
                count++;
            }
        }

        return count;
    }

    private void PruneExpiredVisitReservations()
    {
        if (visitReservations.Count == 0)
        {
            return;
        }

        bool changed = false;
        List<CharacterActor> expired = null;
        foreach (KeyValuePair<CharacterActor, float> pair in visitReservations)
        {
            if (pair.Key != null && Time.time < pair.Value)
            {
                continue;
            }

            expired ??= new List<CharacterActor>();
            expired.Add(pair.Key);
        }

        if (expired != null)
        {
            foreach (CharacterActor visitor in expired)
            {
                visitReservations.Remove(visitor);
                changed = true;
            }
        }

        if (changed)
        {
            MarkFacilityDynamicStateDirty();
        }
    }

    private void PruneExpiredWorkerReservation()
    {
        if (workerReservation == null)
        {
            return;
        }

        if (Time.time < workerReservationUntil)
        {
            return;
        }

        workerReservation = null;
        workerReservationUntil = 0f;
        MarkFacilityDynamicStateDirty();
    }

    private bool TryFindNearestFacilityCell(
        Vector3 fromWorld,
        bool requireRegisteredOccupant,
        out Vector2Int result)
    {
        result = default;
        if (grid == null || buildPoses == null || buildPoses.Count == 0)
        {
            return false;
        }

        int floor = centerPos.y;
        Vector2Int origin = grid.GetXY(fromWorld);
        origin.y = floor;
        bool found = false;
        Vector2Int best = default;
        int bestDistance = int.MaxValue;
        foreach (Vector2Int position in buildPoses)
        {
            if (position.y != floor || !grid.IsValidGridPos(position))
            {
                continue;
            }

            GridCell cell = grid.GetGridCell(position);
            if (requireRegisteredOccupant
                && (cell == null || !cell.ContainsOccupant(this)))
            {
                continue;
            }

            int distance = Mathf.Abs(position.x - origin.x);
            if (found && distance >= bestDistance)
            {
                continue;
            }

            found = true;
            best = position;
            bestDistance = distance;
        }

        result = best;
        return found;
    }

    private bool TryGetConfiguredFacilityAnchorWorldPosition(
        FacilityAnchorKind kind,
        out Vector3 worldPosition)
    {
        worldPosition = transform.position;
        if (BuildingData == null
            || BuildingData.FacilityAnchors == null
            || !BuildingData.FacilityAnchors.TryGetOffset(kind, out Vector2 offset))
        {
            return false;
        }

        worldPosition = grid.GetWorldPos(new Vector2(centerPos.x + offset.x, centerPos.y + offset.y));
        return true;
    }

    private bool TryGetHorizontalFootprintAnchorWorldPosition(
        float normalizedX,
        out Vector3 worldPosition)
    {
        worldPosition = transform.position;
        if (grid == null || buildPoses == null || buildPoses.Count == 0)
        {
            return false;
        }

        int minX = int.MaxValue;
        int maxX = int.MinValue;
        foreach (Vector2Int position in buildPoses)
        {
            if (position.y != centerPos.y)
            {
                continue;
            }

            minX = Mathf.Min(minX, position.x);
            maxX = Mathf.Max(maxX, position.x);
        }

        if (minX == int.MaxValue || maxX == int.MinValue)
        {
            return false;
        }

        float clamped = Mathf.Clamp01(normalizedX);
        float x = Mathf.Lerp(minX, maxX, clamped);
        worldPosition = grid.GetWorldPos(new Vector2(x, centerPos.y));
        return true;
    }

    public virtual float GetWorkUrgency(FacilityWorkType workType)
    {
        float urgency = 0f;
        FacilityData facilityData = Facility;
        if (facilityData == null)
        {
            return urgency;
        }

        if (isDamaged && workType == FacilityWorkType.Repair)
        {
            urgency += 80f;
        }

        if (workType == FacilityWorkType.Restock
            && facilityData.internalStockMax > 0
            && this is IStockedFacility stockedFacility)
        {
            float stockRatio = Mathf.Clamp01((float)stockedFacility.CurrentStock / facilityData.internalStockMax);
            urgency += Mathf.Lerp(70f, 0f, stockRatio);
            if (stockedFacility.CurrentStock <= facilityData.restockRequestThreshold)
            {
                urgency += 20f;
            }
        }

        if (workType == FacilityWorkType.Research && ResolveBlueprintResearchWorkService().HasResearchWorkFor(this))
        {
            urgency += 45f;
        }

        return urgency;
    }

    public virtual bool isVisitable()
    {
        return CanVisit((CharacterActor)null, out _);
    }

    private void OnMouseDown()
    {
        RequireWorldInfoClickSelector().TryHandleWorldInfoClick();
    }

    public void TriggerWorldInfoClick()
    {
        OnBuildingClicked?.Invoke(this);
    }

    protected void MarkFacilityDynamicStateDirty()
    {
        ResolveFacilityCandidateCache().MarkDynamicStateDirty();
    }

    public FacilityRoomOperationalProfile GetRoomOperationalProfile()
    {
        return ResolveRoomFacilityPolicy().GetOperationalProfile(this);
    }

    public void RestoreOperationalState(FacilityOperationalState state)
    {
        operationalState ??= new FacilityOperationalState();
        operationalState.CopyFrom(state);
        MarkFacilityDynamicStateDirty();
    }

    private IFacilityCandidateCache ResolveFacilityCandidateCache()
    {
        return facilityCandidateCache ?? FallbackFacilityCandidateCache;
    }

    private IRoomFacilityPolicy ResolveRoomFacilityPolicy()
    {
        return roomFacilityPolicy ?? FallbackRoomFacilityPolicy;
    }

    private IBlueprintResearchWorkService ResolveBlueprintResearchWorkService()
    {
        return blueprintResearchWorkService ?? FallbackBlueprintResearchWorkService;
    }

    private IWorldInfoClickSelector RequireWorldInfoClickSelector()
    {
        return worldInfoClickSelector ?? FallbackWorldInfoClickSelector;
    }

    private sealed class BuildableObjectNoopBlueprintResearchWorkService : IBlueprintResearchWorkService
    {
        public bool HasResearchWorkFor(BuildableObject facility) => false;

        public BlueprintResearchWorkResult ApplyResearchWork(
            CharacterActor researcher,
            BuildableObject researchFacility,
            float seconds)
        {
            return new BlueprintResearchWorkResult(
                false,
                null,
                0f,
                0f,
                1f,
                false,
                "Blueprint research runtime is not available.");
        }
    }

    private sealed class BuildableObjectNoopWorldInfoClickSelector : IWorldInfoClickSelector
    {
        public bool TryHandleWorldInfoClick() => false;

        public bool TryTriggerCharacterUnderPointer() => false;

        public bool TryGetPreferredCharacterUnderPointer(out CharacterActor actor)
        {
            actor = null;
            return false;
        }

        public bool TryGetPreferredCharacterAtScreenPosition(
            Vector3 screenPosition,
            Camera camera,
            out CharacterActor actor)
        {
            actor = null;
            return false;
        }

        public bool TryGetPreferredCharacter(Collider2D[] hits, out CharacterActor actor)
        {
            actor = null;
            return false;
        }
    }
}
