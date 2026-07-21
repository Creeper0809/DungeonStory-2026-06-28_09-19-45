using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public class BuildableObject : MonoBehaviour, IGridOccupant, IGridMovementOccupant
{
    private const float DefaultAiReservationSeconds = 12f;
    private const float CleaningWorkThreshold = 75f;
    private readonly List<Vector2Int> mutableBuildPoses = new List<Vector2Int>();
    private IReadOnlyList<Vector2Int> buildPosesView;
    public int id { get; private set; }
    public Vector2Int centerPos { get; protected set; }
    public IReadOnlyList<Vector2Int> buildPoses =>
        buildPosesView ??= ReadOnlyView.List(mutableBuildPoses);
    public BuildingSO BuildingData { get; private set; }

    protected Grid grid;
    public BuildingCategory category { get; private set; }

    public event Action OnBuildingDestroyed;
    public event Action<BuildableObject> OnBuildingClicked;
    public bool isDestroy;
    [SerializeField] private bool isDamaged;
    [SerializeField] private int facilityLevel = 1;
    [SerializeField] private FacilityRuntimeState facilityState = new FacilityRuntimeState();
    private readonly List<IBuildingStateModule> runtimeStateModules = new List<IBuildingStateModule>();
    private int currentUserCount;
    private readonly Dictionary<CharacterActor, float> visitReservations = new Dictionary<CharacterActor, float>();
    private CharacterActor workerReservation;
    private float workerReservationUntil;
    private IBlueprintResearchWorkService blueprintResearchWorkService;
    private IWorldInfoClickSelector worldInfoClickSelector;
    private IFacilityCandidateCache facilityCandidateCache;
    private IRoomFacilityPolicy roomFacilityPolicy;
    private IExpeditionEquipmentRuntime expeditionEquipmentRuntime;

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
    public FacilityRuntimeState FacilityState => facilityState ??= new FacilityRuntimeState();
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
        IRoomFacilityPolicy roomFacilityPolicy,
        IExpeditionEquipmentRuntime expeditionEquipmentRuntime = null)
    {
        this.blueprintResearchWorkService = blueprintResearchWorkService
            ?? throw new ArgumentNullException(nameof(blueprintResearchWorkService));
        this.worldInfoClickSelector = worldInfoClickSelector
            ?? throw new ArgumentNullException(nameof(worldInfoClickSelector));
        this.facilityCandidateCache = facilityCandidateCache
            ?? throw new ArgumentNullException(nameof(facilityCandidateCache));
        this.roomFacilityPolicy = roomFacilityPolicy
            ?? throw new ArgumentNullException(nameof(roomFacilityPolicy));
        this.expeditionEquipmentRuntime = expeditionEquipmentRuntime;
    }

    public virtual void SetGrid(Grid grid)
    {
        this.grid = grid;
    }

    public virtual void Initialization(BuildingSO buildingSO, Vector2Int buildPos)
    {
        if (buildingSO == null)
        {
            throw new ArgumentNullException(nameof(buildingSO));
        }

        buildingSO.ValidateAbilitiesOrThrow();
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
        facilityState ??= new FacilityRuntimeState();
        facilityState.CopyFrom(null);
        runtimeStateModules.Clear();
        RegisterStateModule(new FacilityRuntimeStateModule(this));
        foreach (BuildingAbility ability in buildingSO.Abilities)
        {
            if (ability is IBuildingRuntimeStateAbility stateAbility)
            {
                RegisterStateModule(stateAbility.CreateStateModule(this));
            }
        }
        centerPos = buildPos;
        category = placement.Category;
        mutableBuildPoses.Clear();
        mutableBuildPoses.AddRange(placement.GetGridPosList(buildPos));
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

    public bool ContainsGridPosition(Vector2Int gridPosition)
    {
        return buildPoses != null && buildPoses.Contains(gridPosition);
    }

    public Vector3 GetFacilityAnchorWorldPosition(string purposeId, Vector3 fromWorld)
    {
        return TryGetFacilityAnchorWorldPosition(purposeId, fromWorld, out Vector3 worldPosition)
            ? worldPosition
            : transform.position;
    }

    public bool TryGetFacilityAnchorWorldPosition(
        string purposeId,
        Vector3 fromWorld,
        out Vector3 worldPosition)
    {
        worldPosition = transform.position;
        if (grid == null || buildPoses == null || buildPoses.Count == 0)
        {
            return false;
        }

        if (TryGetConfiguredFacilityAnchorWorldPosition(purposeId, fromWorld, out worldPosition))
        {
            return true;
        }

        if (FacilityAnchorPurposeCatalog.TryGet(purposeId, out FacilityAnchorPurposeDefinition definition))
        {
            return definition.FallbackResolver(this, fromWorld, out worldPosition);
        }

        return TryGetFacilityOccupiedWorldPosition(fromWorld, out worldPosition)
            || TryGetHorizontalFootprintAnchorWorldPosition(0.5f, out worldPosition);
    }

    public void DestroySelf()
    {
        OnBuildingDestroyed?.Invoke();
        isDestroy = true;
        DetachFromGridIfStillRegistered();
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

    private void DetachFromGridIfStillRegistered()
    {
        if (grid == null || BuildingData == null || buildPoses == null || buildPoses.Count == 0)
        {
            return;
        }

        GridBuildingPlacement placement = BuildingData.Placement;
        grid.RemoveOccupant(this, placement.Layer, buildPoses, placement.IsMovement);
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

        if (BuildingData.RequiresStockForUse()
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
        FacilityState.completedUses++;
        FacilityState.cleanliness = Mathf.Clamp(FacilityState.cleanliness - 1.5f, 0f, 100f);
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

    public bool TryReserveWorker(
        CharacterActor worker,
        out FacilityAssignmentStatus status,
        float seconds = DefaultAiReservationSeconds)
    {
        if (worker == null)
        {
            status = FacilityAssignmentStatus.Rejected(
                FacilityAssignmentFailureKind.MissingWorker,
                "작업 예약 대상 없음");
            return false;
        }

        PruneExpiredWorkerReservation();
        if (HasWorkerReservationForOther(worker))
        {
            status = FacilityAssignmentStatus.Rejected(
                FacilityAssignmentFailureKind.Reserved,
                "이미 작업 예약됨");
            return false;
        }

        workerReservation = worker;
        workerReservationUntil = Time.time + Mathf.Max(0.1f, seconds);
        MarkFacilityDynamicStateDirty();
        status = FacilityAssignmentStatus.Allowed();
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
        FacilityAssignmentStatus status = GetWorkAssignmentStatus(workType);
        failureReason = status.Reason;
        return status.IsAllowed;
    }

    public FacilityAssignmentStatus GetWorkAssignmentStatus(FacilityWorkType workType)
    {
        PruneExpiredWorkerReservation();
        if (isDestroy)
        {
            return FacilityAssignmentStatus.Rejected(
                FacilityAssignmentFailureKind.Destroyed,
                "시설 파괴됨");
        }

        FacilityData facilityData = Facility;
        if (facilityData == null || !facilityData.SupportsWork(workType))
        {
            return FacilityAssignmentStatus.Rejected(
                FacilityAssignmentFailureKind.UnsupportedWork,
                "지원하지 않는 작업");
        }

        if (workType == FacilityWorkType.Repair && !isDamaged)
        {
            return FacilityAssignmentStatus.Rejected(
                FacilityAssignmentFailureKind.WorkNotNeeded,
                "수리할 필요가 없음");
        }

        if (workType == FacilityWorkType.Clean
            && FacilityState.cleanliness >= CleaningWorkThreshold)
        {
            return FacilityAssignmentStatus.Rejected(
                FacilityAssignmentFailureKind.WorkNotNeeded,
                "청소할 필요가 없음");
        }

        if (isDamaged && facilityData.disabledWhenDamaged && workType != FacilityWorkType.Repair)
        {
            return FacilityAssignmentStatus.Rejected(
                FacilityAssignmentFailureKind.Damaged,
                "시설 파손");
        }

        return FacilityAssignmentStatus.Allowed();
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
        string purposeId,
        Vector3 fromWorld,
        out Vector3 worldPosition)
    {
        worldPosition = transform.position;
        if (BuildingData == null
            || BuildingData.FacilityAnchors == null
            || string.IsNullOrWhiteSpace(purposeId))
        {
            return false;
        }

        bool found = false;
        float bestDistance = float.MaxValue;
        foreach (FacilityAnchorSlot slot in BuildingData.FacilityAnchors.Enumerate(purposeId))
        {
            Vector3 candidate = grid.GetWorldPos(new Vector2(
                centerPos.x + slot.offset.x,
                centerPos.y + slot.offset.y));
            float distance = (candidate - fromWorld).sqrMagnitude;
            if (found && distance >= bestDistance)
            {
                continue;
            }

            found = true;
            bestDistance = distance;
            worldPosition = candidate;
        }

        return found;
    }

    public bool TryGetHorizontalFootprintAnchorWorldPosition(
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

        int internalStockCapacity = BuildingData.GetInternalStockCapacity();
        if (workType == FacilityWorkType.Restock
            && internalStockCapacity > 0
            && this is IStockedFacility stockedFacility)
        {
            float stockRatio = Mathf.Clamp01((float)stockedFacility.CurrentStock / internalStockCapacity);
            urgency += Mathf.Lerp(70f, 0f, stockRatio);
            if (stockedFacility.CurrentStock <= BuildingData.GetRestockRequestThreshold())
            {
                urgency += 20f;
            }
        }

        if (workType == FacilityWorkType.Research && ResolveBlueprintResearchWorkService().HasResearchWorkFor(this))
        {
            urgency += 45f;
        }

        if (workType == FacilityWorkType.Craft && HasPendingEquipmentCraftWork())
        {
            urgency += 55f;
        }

        if (workType == FacilityWorkType.Clean && FacilityState.cleanliness < CleaningWorkThreshold)
        {
            urgency += Mathf.Lerp(15f, 70f, 1f - (FacilityState.cleanliness / CleaningWorkThreshold));
        }

        return urgency;
    }

    public virtual bool isVisitable()
    {
        return CanVisit((CharacterActor)null, out _);
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

    public void RestoreFacilityState(FacilityRuntimeState state)
    {
        facilityState ??= new FacilityRuntimeState();
        facilityState.CopyFrom(state);
        MarkFacilityDynamicStateDirty();
    }

    public void RestoreLegacyFacilityStateV1(LegacyFacilityOperationalStateV1 state)
    {
        state ??= new LegacyFacilityOperationalStateV1();
        RestoreFacilityState(new FacilityRuntimeState
        {
            completedUses = state.completedUses,
            completedWorkCycles = state.completedWorkCycles,
            cleanliness = state.cleanliness
        });

        BuildingProductionStateModule production = runtimeStateModules
            .OfType<BuildingProductionStateModule>()
            .FirstOrDefault();
        production?.SetProducedStock(state.producedStock);

        BuildingSecurityStateModule security = runtimeStateModules
            .OfType<BuildingSecurityStateModule>()
            .FirstOrDefault();
        security?.SetAlarmCharges(state.alarmCharges);
    }

    public void RecordCompletedWorkCycle()
    {
        FacilityState.completedWorkCycles++;
        MarkFacilityDynamicStateDirty();
    }

    public void SetCleanliness(float value)
    {
        FacilityState.cleanliness = Mathf.Clamp(value, 0f, 100f);
        MarkFacilityDynamicStateDirty();
    }

    public IReadOnlyList<IBuildingStateModule> GetStateModules()
    {
        List<IBuildingStateModule> modules = new List<IBuildingStateModule>(runtimeStateModules);
        MonoBehaviour[] components = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour component in components)
        {
            if (component is IBuildingStateModule module && !modules.Contains(module))
            {
                modules.Add(module);
            }
        }

        return modules;
    }

    protected void RegisterStateModule(IBuildingStateModule module)
    {
        if (module == null)
        {
            throw new ArgumentNullException(nameof(module));
        }

        string moduleId = module.ModuleId?.Trim();
        if (string.IsNullOrWhiteSpace(moduleId))
        {
            throw new InvalidOperationException(
                $"{GetType().Name} '{name}' cannot register a state module without an ID.");
        }

        if (module.CurrentVersion <= 0)
        {
            throw new InvalidOperationException(
                $"{GetType().Name} '{name}' state module '{moduleId}' has invalid version {module.CurrentVersion}.");
        }

        if (runtimeStateModules.Any(candidate => candidate != null
                && string.Equals(candidate.ModuleId?.Trim(), moduleId, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                $"{GetType().Name} '{name}' already registered state module '{moduleId}'.");
        }

        runtimeStateModules.Add(module);
    }

    public bool TryGetStateModule<TModule>(string moduleId, out TModule module)
        where TModule : class, IBuildingStateModule
    {
        foreach (IBuildingStateModule candidate in runtimeStateModules)
        {
            if (candidate is TModule typed
                && string.Equals(candidate.ModuleId, moduleId, StringComparison.Ordinal))
            {
                module = typed;
                return true;
            }
        }

        module = null;
        return false;
    }

    public TModule RequireStateModule<TModule>(string moduleId)
        where TModule : class, IBuildingStateModule
    {
        if (TryGetStateModule(moduleId, out TModule module))
        {
            return module;
        }

        throw new InvalidOperationException(
            $"{GetType().Name} '{name}' is missing runtime state module '{moduleId}'.");
    }

    private IFacilityCandidateCache ResolveFacilityCandidateCache()
    {
        return RuntimeDependency.Require(facilityCandidateCache, this);
    }

    private IRoomFacilityPolicy ResolveRoomFacilityPolicy()
    {
        return RuntimeDependency.Require(roomFacilityPolicy, this);
    }

    private IBlueprintResearchWorkService ResolveBlueprintResearchWorkService()
    {
        return RuntimeDependency.Require(blueprintResearchWorkService, this);
    }

    public bool TryGetExpeditionEquipmentRuntime(out IExpeditionEquipmentRuntime runtime)
    {
        runtime = expeditionEquipmentRuntime;
        return runtime != null;
    }

    public bool HasPendingEquipmentCraftWork()
    {
        BuildingEquipmentCraftingAbility crafting = BuildingData?.GetAbility<BuildingEquipmentCraftingAbility>();
        return crafting != null
            && expeditionEquipmentRuntime != null
            && expeditionEquipmentRuntime.HasPendingCraftWork(crafting.CraftableEquipmentIds);
    }

    private IWorldInfoClickSelector RequireWorldInfoClickSelector()
    {
        return RuntimeDependency.Require(worldInfoClickSelector, this);
    }

}
