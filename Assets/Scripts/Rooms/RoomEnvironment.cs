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
    public RoomRoleContribution(RoomRole role, IReadOnlyList<BuildableObject> fixtures)
    {
        Role = role;
        Fixtures = fixtures ?? Array.Empty<BuildableObject>();
    }

    public RoomRole Role { get; }
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
        RoomRole primaryRole,
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
    public RoomRole PrimaryRole { get; }
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
    public RoomRole Roles => Room?.Roles ?? RoomRole.None;
    public bool IsEnvironmentActive => Status == RoomEnvironmentStatus.Usable;
}

public interface IRoomEnvironmentEvaluator
{
    RoomEnvironmentSnapshot Evaluate(Grid grid, RoomInstance room);
}

public sealed class RoomEnvironmentEvaluator : IRoomEnvironmentEvaluator
{
    private static readonly RoomRole[] OrderedRoles =
    {
        RoomRole.Dining,
        RoomRole.Shop,
        RoomRole.Rest,
        RoomRole.Training,
        RoomRole.Research,
        RoomRole.Mana,
        RoomRole.Storage,
        RoomRole.Toilet,
        RoomRole.Hygiene,
        RoomRole.Administration,
        RoomRole.Security
    };

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
        ResolvePrimaryRole(contributions, out RoomRole primaryRole, out bool usesMixedColor);

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

            operationalCleanlinessTotal += fixture.OperationalState.cleanliness;
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
        foreach (RoomRole role in OrderedRoles)
        {
            if ((room.Roles & role) == 0)
            {
                continue;
            }

            FacilityRole facilityRole = RoomRoleUtility.ToFacilityRoles(role);
            List<BuildableObject> fixtures = room.Furniture
                .Where((fixture) => fixture != null
                    && !fixture.isDestroy
                    && fixture.SupportsFacilityRole(facilityRole))
                .Distinct()
                .ToList();
            result.Add(new RoomRoleContribution(role, fixtures));
        }

        return result;
    }

    private static void ResolvePrimaryRole(
        IReadOnlyList<RoomRoleContribution> contributions,
        out RoomRole primaryRole,
        out bool usesMixedColor)
    {
        primaryRole = RoomRole.None;
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
    private static readonly RoomRole[] OrderedRoles =
    {
        RoomRole.Dining,
        RoomRole.Shop,
        RoomRole.Rest,
        RoomRole.Training,
        RoomRole.Research,
        RoomRole.Mana,
        RoomRole.Storage,
        RoomRole.Toilet,
        RoomRole.Hygiene,
        RoomRole.Administration,
        RoomRole.Security
    };

    public static string GetRoomName(RoomRole roles)
    {
        if (roles == RoomRole.None)
        {
            return "미지정 방";
        }

        List<string> labels = OrderedRoles
            .Where((role) => (roles & role) != 0)
            .Select(GetRoleLabel)
            .ToList();
        return labels.Count == 1
            ? GetSingleRoomName(OrderedRoles.First((role) => (roles & role) != 0))
            : string.Join(" + ", labels);
    }

    public static string GetRoleLabel(RoomRole role)
    {
        return role switch
        {
            RoomRole.Dining => "식사",
            RoomRole.Shop => "상점",
            RoomRole.Rest => "휴식",
            RoomRole.Training => "훈련",
            RoomRole.Research => "연구",
            RoomRole.Mana => "마나",
            RoomRole.Storage => "창고",
            RoomRole.Toilet => "화장실",
            RoomRole.Hygiene => "위생",
            RoomRole.Administration => "집무",
            RoomRole.Security => "경비",
            _ => "미지정"
        };
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

    private static string GetSingleRoomName(RoomRole role)
    {
        return role switch
        {
            RoomRole.Dining => "식당",
            RoomRole.Shop => "상점",
            RoomRole.Rest => "휴게실",
            RoomRole.Training => "훈련실",
            RoomRole.Research => "연구실",
            RoomRole.Mana => "마나실",
            RoomRole.Storage => "창고",
            RoomRole.Toilet => "화장실",
            RoomRole.Hygiene => "세면실",
            RoomRole.Administration => "사장실",
            RoomRole.Security => "경비실",
            _ => "미지정 방"
        };
    }
}
