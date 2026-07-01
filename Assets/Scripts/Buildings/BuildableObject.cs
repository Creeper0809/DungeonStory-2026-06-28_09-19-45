using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildableObject : MonoBehaviour, IGridOccupant, IGridMovementOccupant
{
    private const float DefaultAiReservationSeconds = 12f;

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
    private int currentUserCount;
    private readonly Dictionary<Character, float> visitReservations = new Dictionary<Character, float>();
    private Character workerReservation;
    private float workerReservationUntil;
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
    public int ActiveVisitReservationCount
    {
        get
        {
            PruneExpiredVisitReservations();
            return visitReservations.Count;
        }
    }
    public Character WorkerReservation
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
        centerPos = buildPos;
        category = placement.Category;
        buildPoses = placement.GetGridPosList(buildPos);
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

    public bool TryGetNearestWalkableWorldPosition(Vector3 fromWorld, out Vector3 worldPosition)
    {
        worldPosition = fromWorld;
        if (grid == null)
        {
            return false;
        }

        Vector2Int origin = grid.GetXY(fromWorld);
        if (TryFindNearestWalkableCell(origin, requireEmptyBuildingLayer: true, out Vector2Int emptyCell)
            || TryFindNearestWalkableCell(origin, requireEmptyBuildingLayer: false, out emptyCell))
        {
            worldPosition = grid.GetWorldPos(emptyCell);
            return true;
        }

        return false;
    }

    public void DestroySelf()
    {
        OnBuildingDestroyed?.Invoke();
        isDestroy = true;
        FacilityCandidateCache.MarkDynamicStateDirty();
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
        FacilityCandidateCache.MarkDynamicStateDirty();
    }

    public void SetFacilityLevel(int value)
    {
        int nextLevel = Mathf.Max(1, value);
        if (facilityLevel == nextLevel)
        {
            return;
        }

        facilityLevel = nextLevel;
        FacilityCandidateCache.MarkDynamicStateDirty();
    }

    public bool SupportsFacilityRole(FacilityRole role)
    {
        return Facility != null && Facility.SupportsRole(role);
    }

    public bool SupportsWork(FacilityWorkType workType)
    {
        return Facility != null && Facility.SupportsWork(workType);
    }

    public bool CanVisit(Character visitor, out string failureReason)
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

        if (isDamaged && facilityData.disabledWhenDamaged)
        {
            failureReason = "시설 파손";
            return false;
        }

        if (facilityData.capacity > 0
            && currentUserCount + GetActiveVisitReservationCountExcept(visitor) >= facilityData.capacity)
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

    public bool TryBeginUse(Character visitor, out string failureReason)
    {
        if (!CanVisit(visitor, out failureReason))
        {
            return false;
        }

        ReleaseVisitReservation(visitor);
        currentUserCount++;
        FacilityCandidateCache.MarkDynamicStateDirty();
        FacilityVisitEvent.Trigger(visitor, this);
        return true;
    }

    public void EndUse(Character visitor)
    {
        if (currentUserCount > 0)
        {
            currentUserCount--;
            FacilityCandidateCache.MarkDynamicStateDirty();
        }
    }

    public bool TryReserveVisit(Character visitor, out string failureReason, float seconds = DefaultAiReservationSeconds)
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
        FacilityCandidateCache.MarkDynamicStateDirty();
        return true;
    }

    public void RefreshVisitReservation(Character visitor, float seconds = DefaultAiReservationSeconds)
    {
        if (visitor == null || !visitReservations.ContainsKey(visitor))
        {
            return;
        }

        visitReservations[visitor] = Time.time + Mathf.Max(0.1f, seconds);
    }

    public void ReleaseVisitReservation(Character visitor)
    {
        if (visitor == null)
        {
            return;
        }

        if (visitReservations.Remove(visitor))
        {
            FacilityCandidateCache.MarkDynamicStateDirty();
        }
    }

    public bool TryReserveWorker(Character worker, out string failureReason, float seconds = DefaultAiReservationSeconds)
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
        FacilityCandidateCache.MarkDynamicStateDirty();
        return true;
    }

    public void RefreshWorkerReservation(Character worker, float seconds = DefaultAiReservationSeconds)
    {
        PruneExpiredWorkerReservation();
        if (worker == null || workerReservation != worker)
        {
            return;
        }

        workerReservationUntil = Time.time + Mathf.Max(0.1f, seconds);
    }

    public bool HasWorkerReservationForOther(Character worker)
    {
        PruneExpiredWorkerReservation();
        return workerReservation != null && workerReservation != worker;
    }

    public void ReleaseWorkerReservation(Character worker)
    {
        if (worker == null || workerReservation != worker)
        {
            return;
        }

        workerReservation = null;
        workerReservationUntil = 0f;
        FacilityCandidateCache.MarkDynamicStateDirty();
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

    private int GetActiveVisitReservationCountExcept(Character visitor)
    {
        PruneExpiredVisitReservations();
        int count = 0;
        foreach (Character reservedVisitor in visitReservations.Keys)
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
        List<Character> expired = null;
        foreach (KeyValuePair<Character, float> pair in visitReservations)
        {
            if (pair.Key != null && Time.time < pair.Value)
            {
                continue;
            }

            expired ??= new List<Character>();
            expired.Add(pair.Key);
        }

        if (expired != null)
        {
            foreach (Character visitor in expired)
            {
                visitReservations.Remove(visitor);
                changed = true;
            }
        }

        if (changed)
        {
            FacilityCandidateCache.MarkDynamicStateDirty();
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
        FacilityCandidateCache.MarkDynamicStateDirty();
    }

    private bool TryFindNearestWalkableCell(
        Vector2Int origin,
        bool requireEmptyBuildingLayer,
        out Vector2Int result)
    {
        bool found = false;
        Vector2Int best = default;
        int bestDistance = int.MaxValue;

        foreach (GridCell cell in grid.GetCells())
        {
            if (cell == null || !grid.IsWalkable(cell.Position))
            {
                continue;
            }

            if (requireEmptyBuildingLayer && cell.GetOccupant(GridLayer.Building) != null)
            {
                continue;
            }

            int distance = Mathf.Abs(cell.Position.x - origin.x) + Mathf.Abs(cell.Position.y - origin.y);
            if (found && distance >= bestDistance)
            {
                continue;
            }

            found = true;
            best = cell.Position;
            bestDistance = distance;
        }

        result = best;
        return found;
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

        if (workType == FacilityWorkType.Research && BlueprintResearchRuntime.HasResearchWorkFor(this))
        {
            urgency += 45f;
        }

        return urgency;
    }

    public virtual bool isVisitable()
    {
        return CanVisit(null, out _);
    }

    private void OnMouseDown()
    {
        OnBuildingClicked?.Invoke(this);
    }

}
