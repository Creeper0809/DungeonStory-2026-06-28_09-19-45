using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum RoomEnvironmentStatus
{
    Usable,
    OpenBoundary,
    MissingDoor,
    SelfContained
}

public sealed class RoomRoleContribution
{
    public RoomRoleContribution(FacilityRole role, IReadOnlyList<BuildableObject> fixtures)
    {
        Role = role;
        Fixtures = fixtures ?? Array.Empty<BuildableObject>();
    }

    public FacilityRole Role { get; }
    public IReadOnlyList<BuildableObject> Fixtures { get; }
    public int Count => Fixtures.Count;
}

public sealed class RoomEnvironmentSnapshot
{
    public RoomEnvironmentSnapshot(
        Grid grid,
        RoomInstance room,
        RoomEnvironmentStatus status,
        IReadOnlyList<BuildableObject> fixtures,
        IReadOnlyList<RoomRoleContribution> roleContributions,
        FacilityRole primaryRole,
        bool usesMixedColor,
        int occupiedCells,
        float luxury,
        float hygiene,
        int cleanServiceStreak,
        int damagedFixtures,
        float spaciousness,
        float beauty,
        float cleanliness,
        float impressiveness)
    {
        Grid = grid;
        Room = room;
        Status = status;
        Fixtures = fixtures ?? Array.Empty<BuildableObject>();
        RoleContributions = roleContributions ?? Array.Empty<RoomRoleContribution>();
        PrimaryRole = primaryRole;
        UsesMixedColor = usesMixedColor;
        OccupiedCells = Mathf.Max(0, occupiedCells);
        Luxury = luxury;
        Hygiene = hygiene;
        CleanServiceStreak = Mathf.Max(0, cleanServiceStreak);
        DamagedFixtures = Mathf.Max(0, damagedFixtures);
        Spaciousness = Mathf.Clamp(spaciousness, 0f, 100f);
        Beauty = Mathf.Clamp(beauty, 0f, 100f);
        Cleanliness = Mathf.Clamp(cleanliness, 0f, 100f);
        Impressiveness = Mathf.Clamp(impressiveness, 0f, 100f);
    }

    public Grid Grid { get; }
    public RoomInstance Room { get; }
    public RoomEnvironmentStatus Status { get; }
    public IReadOnlyList<BuildableObject> Fixtures { get; }
    public IReadOnlyList<RoomRoleContribution> RoleContributions { get; }
    public FacilityRole PrimaryRole { get; }
    public bool UsesMixedColor { get; }
    public int OccupiedCells { get; }
    public float Luxury { get; }
    public float Hygiene { get; }
    public int CleanServiceStreak { get; }
    public int DamagedFixtures { get; }
    public float Spaciousness { get; }
    public float Beauty { get; }
    public float Cleanliness { get; }
    public float Impressiveness { get; }
    public int Area => Room?.Cells.Count ?? 0;
    public int FreeCells => Mathf.Max(0, Area - OccupiedCells);
    public int DoorCount => Room?.Doors.Count ?? 0;
    public int WallCount => Room?.SolidBoundaryCount ?? 0;
    public FacilityRole Roles => Room?.Roles ?? FacilityRole.None;
    public bool IsEnvironmentActive => Status == RoomEnvironmentStatus.Usable;
}

public interface IRoomEnvironmentEvaluator
{
    RoomEnvironmentSnapshot Evaluate(Grid grid, RoomInstance room);
}

public interface IRoomEnvironmentQuery
{
    bool TryGetSnapshot(BuildableObject facility, out RoomEnvironmentSnapshot snapshot);
    float GetWorkEnvironmentScore(BuildableObject facility);
    float GetWorkDurationMultiplier(BuildableObject facility, FacilityWorkType workType);
    float GetFacilityPreferenceScore(BuildableObject facility);
}

public sealed class RoomEnvironmentQuery : IRoomEnvironmentQuery
{
    private sealed class CachedSnapshot
    {
        public int GridVersion = -1;
        public int FacilityStateVersion = -1;
        public RoomEnvironmentSnapshot Snapshot;
    }

    private const int MaxCachedFacilities = 512;

    private readonly IRoomLayoutCache roomLayoutCache;
    private readonly IRoomEnvironmentEvaluator evaluator;
    private readonly IFacilityCandidateCache facilityCandidateCache;
    private readonly Dictionary<BuildableObject, CachedSnapshot> cacheByFacility =
        new Dictionary<BuildableObject, CachedSnapshot>();

    public RoomEnvironmentQuery(
        IRoomLayoutCache roomLayoutCache,
        IRoomEnvironmentEvaluator evaluator,
        IFacilityCandidateCache facilityCandidateCache)
    {
        this.roomLayoutCache = roomLayoutCache
            ?? throw new ArgumentNullException(nameof(roomLayoutCache));
        this.evaluator = evaluator
            ?? throw new ArgumentNullException(nameof(evaluator));
        this.facilityCandidateCache = facilityCandidateCache
            ?? throw new ArgumentNullException(nameof(facilityCandidateCache));
    }

    public bool TryGetSnapshot(BuildableObject facility, out RoomEnvironmentSnapshot snapshot)
    {
        snapshot = null;
        if (facility == null
            || facility.isDestroy
            || facility.Grid == null
            || !roomLayoutCache.TryGetRoom(facility, out RoomInstance room)
            || room == null
            || room.IsSelfContained)
        {
            return false;
        }

        if (!cacheByFacility.TryGetValue(facility, out CachedSnapshot cache))
        {
            if (cacheByFacility.Count >= MaxCachedFacilities)
            {
                cacheByFacility.Clear();
            }

            cache = new CachedSnapshot();
            cacheByFacility[facility] = cache;
        }

        int gridVersion = facility.Grid.version;
        int stateVersion = facilityCandidateCache.DynamicStateVersion;
        if (cache.Snapshot == null
            || cache.GridVersion != gridVersion
            || cache.FacilityStateVersion != stateVersion)
        {
            cache.GridVersion = gridVersion;
            cache.FacilityStateVersion = stateVersion;
            cache.Snapshot = evaluator.Evaluate(facility.Grid, room);
        }

        snapshot = cache.Snapshot;
        return snapshot != null;
    }

    public float GetWorkEnvironmentScore(BuildableObject facility)
    {
        return TryGetSnapshot(facility, out RoomEnvironmentSnapshot snapshot)
            && snapshot.IsEnvironmentActive
                ? Mathf.Clamp(snapshot.Impressiveness * 0.6f + snapshot.Cleanliness * 0.4f, 0f, 100f)
                : 50f;
    }

    public float GetWorkDurationMultiplier(BuildableObject facility, FacilityWorkType workType)
    {
        if (!UsesWorkEnvironment(workType)
            || !TryGetSnapshot(facility, out RoomEnvironmentSnapshot snapshot)
            || !snapshot.IsEnvironmentActive)
        {
            return 1f;
        }

        float score = Mathf.Clamp(snapshot.Impressiveness * 0.6f + snapshot.Cleanliness * 0.4f, 0f, 100f);
        float speedMultiplier = Mathf.Clamp(0.85f + score * 0.003f, 0.85f, 1.15f);
        return 1f / speedMultiplier;
    }

    public float GetFacilityPreferenceScore(BuildableObject facility)
    {
        if (!TryGetSnapshot(facility, out RoomEnvironmentSnapshot snapshot))
        {
            return 8f;
        }

        if (!snapshot.IsEnvironmentActive)
        {
            return facility != null && facility.BuildingData != null && facility.BuildingData.RequiresRoomRole()
                ? -15f
                : 0f;
        }

        float score = Mathf.Clamp(snapshot.Impressiveness * 0.6f + snapshot.Cleanliness * 0.4f, 0f, 100f);
        return Mathf.Lerp(8f, 28f, score / 100f);
    }

    private static bool UsesWorkEnvironment(FacilityWorkType workType)
    {
        return workType == FacilityWorkType.Operate
            || workType == FacilityWorkType.Research
            || workType == FacilityWorkType.Restock
            || workType == FacilityWorkType.Guard
            || workType == FacilityWorkType.Craft;
    }
}

public sealed class RoomEnvironmentEvaluator : IRoomEnvironmentEvaluator
{
    private readonly IRoomEnvironmentSettingsProvider settingsProvider;
    private readonly IFacilityEvolutionRecordProvider recordProvider;

    public RoomEnvironmentEvaluator(
        IRoomEnvironmentSettingsProvider settingsProvider,
        IFacilityEvolutionRecordProvider recordProvider)
    {
        this.settingsProvider = settingsProvider
            ?? throw new ArgumentNullException(nameof(settingsProvider));
        this.recordProvider = recordProvider
            ?? throw new ArgumentNullException(nameof(recordProvider));
    }

    public RoomEnvironmentSnapshot Evaluate(Grid grid, RoomInstance room)
    {
        if (grid == null)
        {
            throw new ArgumentNullException(nameof(grid));
        }

        if (room == null)
        {
            throw new ArgumentNullException(nameof(room));
        }

        RoomEnvironmentSettingsSO settings = settingsProvider.Settings;
        List<BuildableObject> fixtures = CollectInteriorFixtures(grid, room);
        HashSet<Vector2Int> occupiedCells = CollectOccupiedCells(room, fixtures);
        List<RoomRoleContribution> contributions = BuildRoleContributions(room);
        ResolvePrimaryRole(contributions, out FacilityRole primaryRole, out bool usesMixedColor);

        float luxury = 0f;
        float hygiene = 0f;
        int cleanStreak = 0;
        int damagedFixtures = 0;
        float operationalCleanlinessTotal = 0f;
        foreach (BuildableObject fixture in fixtures)
        {
            luxury += GetEvolutionScore(fixture, FacilityEvolutionTerms.Luxury);
            hygiene += GetEvolutionScore(fixture, FacilityEvolutionTerms.Hygiene);
            if (fixture.SupportsFacilityRole(FacilityRole.Hygiene))
            {
                hygiene += settings.HygieneFacilityContribution;
            }

            FacilityEvolutionRecord record = recordProvider.GetRecord(fixture);
            cleanStreak += record != null
                ? record.GetToken(FacilityEvolutionTerms.CleanServiceStreak)
                : 0;
            if (fixture.IsDamaged)
            {
                damagedFixtures++;
            }

            operationalCleanlinessTotal += fixture.FacilityState.cleanliness;
        }

        int area = Mathf.Max(1, room.Cells.Count);
        float occupiedRatio = Mathf.Clamp01((float)occupiedCells.Count / area);
        float freeCellRatio = 1f - occupiedRatio;
        float damagedRatio = fixtures.Count > 0
            ? (float)damagedFixtures / fixtures.Count
            : 0f;
        float operationalCleanliness = fixtures.Count > 0
            ? operationalCleanlinessTotal / fixtures.Count
            : 100f;
        float normalizedArea = Mathf.InverseLerp(
            settings.SpaciousAreaMinimum,
            settings.SpaciousAreaMaximum,
            area);
        float spaciousness = 100f * (
            normalizedArea * settings.SpaciousAreaWeight
            + freeCellRatio * settings.SpaciousFreeCellWeight);
        float beauty = settings.BeautyBaseline
            + luxury * settings.LuxuryMultiplier
            - damagedRatio * settings.BeautyDamagePenalty
            - Mathf.Max(0f, occupiedRatio - settings.BeautyCrowdingThreshold)
                * settings.BeautyCrowdingPenalty;
        float cleanliness = settings.CleanlinessBaseline
            + Mathf.Min(settings.HygieneContributionMaximum, hygiene)
            + Mathf.Min(
                settings.CleanStreakContributionMaximum,
                cleanStreak * settings.CleanStreakContribution)
            - damagedRatio * settings.CleanlinessDamagePenalty
            - Mathf.Max(0f, occupiedRatio - settings.CleanlinessCrowdingThreshold)
                * settings.CleanlinessCrowdingPenalty
            + (operationalCleanliness - 50f) * 0.2f;
        float impressiveness = beauty * settings.ImpressivenessBeautyWeight
            + spaciousness * settings.ImpressivenessSpaciousnessWeight
            + cleanliness * settings.ImpressivenessCleanlinessWeight
            + room.GetQualityScore() * 100f * settings.ImpressivenessQualityWeight;

        return new RoomEnvironmentSnapshot(
            grid,
            room,
            GetStatus(room),
            fixtures,
            contributions,
            primaryRole,
            usesMixedColor,
            occupiedCells.Count,
            luxury,
            hygiene,
            cleanStreak,
            damagedFixtures,
            Mathf.Clamp(spaciousness, 0f, 100f),
            Mathf.Clamp(beauty, 0f, 100f),
            Mathf.Clamp(cleanliness, 0f, 100f),
            Mathf.Clamp(impressiveness, 0f, 100f));
    }

    private static List<BuildableObject> CollectInteriorFixtures(Grid grid, RoomInstance room)
    {
        HashSet<Vector2Int> roomCells = new HashSet<Vector2Int>(room.Cells);
        return grid.FindAllOccupants(null)
            .OfType<BuildableObject>()
            .Where((fixture) => fixture != null
                && !fixture.isDestroy
                && fixture.BuildingData != null
                && IsInteriorFixtureLayer(fixture.BuildingData.Placement.Layer)
                && !RoomDetector.IsDoor(fixture)
                && !RoomDetector.IsWall(fixture)
                && GetBuildPositions(fixture).Any(roomCells.Contains))
            .Distinct()
            .ToList();
    }

    private static bool IsInteriorFixtureLayer(GridLayer layer)
    {
        return layer == GridLayer.Building
            || layer == GridLayer.WallFixture
            || layer == GridLayer.CeilingFixture
            || layer == GridLayer.FloorOverlay;
    }

    private static HashSet<Vector2Int> CollectOccupiedCells(
        RoomInstance room,
        IReadOnlyList<BuildableObject> fixtures)
    {
        HashSet<Vector2Int> roomCells = new HashSet<Vector2Int>(room.Cells);
        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
        foreach (BuildableObject fixture in fixtures)
        {
            foreach (Vector2Int position in GetBuildPositions(fixture))
            {
                if (roomCells.Contains(position))
                {
                    occupied.Add(position);
                }
            }
        }

        return occupied;
    }

    private static IEnumerable<Vector2Int> GetBuildPositions(BuildableObject fixture)
    {
        return fixture.buildPoses != null && fixture.buildPoses.Count > 0
            ? fixture.buildPoses
            : new[] { fixture.centerPos };
    }

    private static List<RoomRoleContribution> BuildRoleContributions(RoomInstance room)
    {
        List<RoomRoleContribution> result = new List<RoomRoleContribution>();
        foreach (FacilityRoleDefinition definition in FacilityRoleCatalog.Enumerate(room.Roles))
        {
            List<BuildableObject> fixtures = room.Furniture
                .Where((fixture) => fixture != null
                    && !fixture.isDestroy
                    && fixture.SupportsFacilityRole(definition.Role))
                .Distinct()
                .ToList();
            result.Add(new RoomRoleContribution(definition.Role, fixtures));
        }

        return result;
    }

    private static void ResolvePrimaryRole(
        IReadOnlyList<RoomRoleContribution> contributions,
        out FacilityRole primaryRole,
        out bool usesMixedColor)
    {
        primaryRole = FacilityRole.None;
        usesMixedColor = false;
        if (contributions == null || contributions.Count == 0)
        {
            return;
        }

        int highestCount = contributions.Max((item) => item.Count);
        List<RoomRoleContribution> leaders = contributions
            .Where((item) => item.Count == highestCount)
            .ToList();
        if (leaders.Count == 1)
        {
            primaryRole = leaders[0].Role;
            return;
        }

        usesMixedColor = true;
    }

    private static float GetEvolutionScore(BuildableObject fixture, string key)
    {
        FacilityEvolutionContributionData contribution = fixture?.BuildingData?.Evolution;
        if (contribution == null
            || !contribution.contributesToRoomProfile
            || contribution.scores == null)
        {
            return 0f;
        }

        float result = 0f;
        foreach (FacilityEvolutionValue score in contribution.scores)
        {
            if (string.Equals(score.key, key, StringComparison.Ordinal))
            {
                result += score.value;
            }
        }

        return result;
    }

    private static RoomEnvironmentStatus GetStatus(RoomInstance room)
    {
        if (room.IsSelfContained) return RoomEnvironmentStatus.SelfContained;
        if (!room.IsClosed) return RoomEnvironmentStatus.OpenBoundary;
        return room.HasDoor
            ? RoomEnvironmentStatus.Usable
            : RoomEnvironmentStatus.MissingDoor;
    }
}

public static class RoomEnvironmentPresentation
{
    public static string GetRoomName(FacilityRole roles)
    {
        if (roles == FacilityRole.None)
        {
            return "미지정 방";
        }

        List<FacilityRoleDefinition> definitions = FacilityRoleCatalog
            .Enumerate(roles)
            .ToList();
        if (definitions.Count == 0)
        {
            return "미지정 방";
        }

        return definitions.Count == 1
            ? definitions[0].RoomName
            : string.Join(" + ", definitions.Select((definition) => definition.RoomLabel));
    }

    public static string GetRoleLabel(FacilityRole role)
    {
        return FacilityRoleCatalog.TryGet(role, out FacilityRoleDefinition definition)
            ? definition.RoomLabel
            : "미지정";
    }

    public static string GetStatusLabel(RoomEnvironmentStatus status)
    {
        return status switch
        {
            RoomEnvironmentStatus.Usable => "사용 가능한 방",
            RoomEnvironmentStatus.OpenBoundary => "열린 경계 · 환경 효과 비활성",
            RoomEnvironmentStatus.MissingDoor => "출입문 없음 · 환경 효과 비활성",
            RoomEnvironmentStatus.SelfContained => "독립 시설 · 방 판정 제외",
            _ => "방 상태 확인 불가"
        };
    }

    public static string GetGrade(float value)
    {
        if (value < 20f) return "끔찍함";
        if (value < 40f) return "나쁨";
        if (value < 60f) return "평범함";
        if (value < 80f) return "좋음";
        return "훌륭함";
    }

    public static string GetFixtureName(BuildableObject fixture)
    {
        if (fixture == null) return "알 수 없는 시설";
        return fixture.BuildingData != null && !string.IsNullOrWhiteSpace(fixture.BuildingData.objectName)
            ? fixture.BuildingData.objectName
            : fixture.name;
    }

}
