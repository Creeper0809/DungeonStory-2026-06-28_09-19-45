using System;
using System.Collections.Generic;
using UnityEngine;

public enum DeprivationKind
{
    Hunger = 0,
    Thirst = 1,
    Bladder = 2,
    Contamination = 3,
    Exhaustion = 4,
    MentalInstability = 5
}

public enum CharacterBreakdownKind
{
    None = 0,
    DesperateRelief = 1,
    DesperateDrink = 2,
    DesperateEat = 3,
    Collapse = 4,
    ViolentImpulse = 5
}

public enum WorldFilthType
{
    Waste = 0,
    Blood = 1,
    Rot = 2,
    Stain = 3
}

public enum WorldWaterQuality
{
    Clean = 0,
    Unsafe = 1,
    Foul = 2
}

public enum GridCellTerrainType
{
    Dry = 0,
    ShallowWater = 1,
    DeepWater = 2
}

[Serializable]
public sealed class DeprivationBurdenSaveData
{
    public DeprivationKind kind;
    public float burden;
    public float maximumHeldSeconds;
    public float nextBreakdownCheckAt;
    public float nextDamageAt;
}

[Serializable]
public sealed class CharacterBreakdownState
{
    public bool active;
    public CharacterBreakdownKind kind;
    public DeprivationKind cause;
    public string targetId = string.Empty;
    public int targetGridX;
    public int targetGridY;
    public float startedAt;
    public float suppressionResistance;
    public string lastReplanReason = string.Empty;
}

[Serializable]
public sealed class CharacterDeprivationState
{
    public string persistentId = string.Empty;
    public List<DeprivationBurdenSaveData> burdens = new List<DeprivationBurdenSaveData>();
    public CharacterBreakdownState breakdown = new CharacterBreakdownState();
    public List<string> tabooMemories = new List<string>();
    public float infectionBurden;
    public float lastUpdatedAt;
}

[Serializable]
public sealed class WorldFilthSaveData
{
    public string filthId = string.Empty;
    public WorldFilthType type;
    public float amount;
    public int gridX;
    public int gridY;
    public string sourceCharacterId = string.Empty;
    public float infectionRisk;
    public bool wallStain;
}

[Serializable]
public sealed class WorldWaterSourceSaveData
{
    public string sourceId = string.Empty;
    public int gridX;
    public int gridY;
    public GridCellTerrainType terrainType = GridCellTerrainType.ShallowWater;
    public WorldWaterQuality quality = WorldWaterQuality.Unsafe;
    public float capacity = 12f;
    public float remaining = 12f;
    public float regenerationPerSecond = 0.02f;
}

[Serializable]
public sealed class DungeonDarkSurvivalSaveData
{
    public const int CurrentVersion = 1;

    public int version = CurrentVersion;
    public int nextFilthSequence = 1;
    public int nextWaterSequence = 1;
    public List<CharacterDeprivationState> characters = new List<CharacterDeprivationState>();
    public List<WorldFilthSaveData> filth = new List<WorldFilthSaveData>();
    public List<WorldWaterSourceSaveData> waterSources = new List<WorldWaterSourceSaveData>();
}

public readonly struct CharacterDeprivationSnapshot
{
    public CharacterDeprivationSnapshot(
        IReadOnlyDictionary<DeprivationKind, float> burdens,
        CharacterBreakdownState breakdown,
        float infectionBurden,
        IReadOnlyList<string> tabooMemories)
    {
        Burdens = burdens;
        Breakdown = breakdown;
        InfectionBurden = Mathf.Clamp(infectionBurden, 0f, 100f);
        TabooMemories = tabooMemories ?? Array.Empty<string>();
    }

    public IReadOnlyDictionary<DeprivationKind, float> Burdens { get; }
    public CharacterBreakdownState Breakdown { get; }
    public float InfectionBurden { get; }
    public IReadOnlyList<string> TabooMemories { get; }
    public float HighestBurden
    {
        get
        {
            float highest = 0f;
            if (Burdens != null)
            {
                foreach (float burden in Burdens.Values)
                {
                    highest = Mathf.Max(highest, burden);
                }
            }

            return highest;
        }
    }
}

public readonly struct WorldFilthSnapshot
{
    public WorldFilthSnapshot(
        string filthId,
        WorldFilthType type,
        float amount,
        Vector2Int position,
        string sourceCharacterId,
        float infectionRisk,
        bool wallStain)
    {
        FilthId = filthId ?? string.Empty;
        Type = type;
        Amount = Mathf.Max(0f, amount);
        Position = position;
        SourceCharacterId = sourceCharacterId ?? string.Empty;
        InfectionRisk = Mathf.Clamp01(infectionRisk);
        WallStain = wallStain;
    }

    public string FilthId { get; }
    public WorldFilthType Type { get; }
    public float Amount { get; }
    public Vector2Int Position { get; }
    public string SourceCharacterId { get; }
    public float InfectionRisk { get; }
    public bool WallStain { get; }
    public float RequiredCleaningWork => Mathf.Max(5f, Amount * 12f);
}

public readonly struct WorldWaterSourceSnapshot
{
    public WorldWaterSourceSnapshot(
        string sourceId,
        Vector2Int position,
        GridCellTerrainType terrainType,
        WorldWaterQuality quality,
        float capacity,
        float remaining,
        float regenerationPerSecond)
    {
        SourceId = sourceId ?? string.Empty;
        Position = position;
        TerrainType = terrainType;
        Quality = quality;
        Capacity = Mathf.Max(0f, capacity);
        Remaining = Mathf.Clamp(remaining, 0f, Capacity);
        RegenerationPerSecond = Mathf.Max(0f, regenerationPerSecond);
    }

    public string SourceId { get; }
    public Vector2Int Position { get; }
    public GridCellTerrainType TerrainType { get; }
    public WorldWaterQuality Quality { get; }
    public float Capacity { get; }
    public float Remaining { get; }
    public float RegenerationPerSecond { get; }
    public bool CanDrink => Remaining > 0.05f;
}

public interface ICharacterDeprivationRuntime
{
    bool HasActiveBreakdown(CharacterActor actor);
    bool TryGetSnapshot(CharacterActor actor, out CharacterDeprivationSnapshot snapshot);
    bool TryRunActiveBreakdown(CharacterActor actor, out string status);
    bool TryRunSafeEmergencyRelief(CharacterActor actor, out string status);
    bool IsSuppressible(CharacterActor actor);
    bool ApplySuppression(CharacterActor actor, float amount, out bool ended);
    float GetMoveSpeedMultiplier(CharacterActor actor);
    float GetWorkSpeedMultiplier(CharacterActor actor);
    void AddInfectionBurden(CharacterActor actor, float amount);
    void RecordTaboo(CharacterActor actor, string memory);
    DungeonDarkSurvivalSaveData Capture();
    void Restore(DungeonDarkSurvivalSaveData saveData);
    bool DebugForceBreakdown(CharacterActor actor, CharacterBreakdownKind kind);
    bool DebugClearBreakdown(CharacterActor actor);
}

public interface IWorldFilthQuery
{
    int StateVersion { get; }
    IReadOnlyList<WorldFilthSnapshot> GetAll();
    IReadOnlyList<WorldFilthSnapshot> GetAt(Vector2Int position);
    WorldFilthSnapshot AddFilth(
        WorldFilthType type,
        Vector2Int position,
        float amount,
        string sourceCharacterId,
        float infectionRisk,
        bool wallStain = false);
    bool Clean(string filthId, float workAmount, out float remainingAmount);
    float GetCleanlinessPenalty(Vector2Int position, int radius = 0);
    List<WorldFilthSaveData> CaptureFilth();
    void RestoreFilth(IEnumerable<WorldFilthSaveData> saveData, int nextSequence);
    int NextFilthSequence { get; }
}

public interface IWorldWaterQuery
{
    IReadOnlyList<WorldWaterSourceSnapshot> GetAllSources();
    bool TryGetSource(string sourceId, out WorldWaterSourceSnapshot source);
    bool TryFindDrinkSource(Vector2Int origin, bool allowFoul, out WorldWaterSourceSnapshot source);
    bool TryDrink(string sourceId, float amount, out WorldWaterQuality quality, out float consumed);
    List<WorldWaterSourceSaveData> CaptureWaterSources();
    void RestoreWaterSources(IEnumerable<WorldWaterSourceSaveData> saveData, int nextSequence);
    int NextWaterSequence { get; }
    bool DebugCreateSource(
        Vector2Int position,
        WorldWaterQuality quality,
        float capacity,
        GridCellTerrainType terrainType,
        out string sourceId);
    bool DebugSetSource(string sourceId, WorldWaterQuality quality, float capacity, float remaining);
}
