using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public enum OffenseExpeditionPhase
{
    ChoosingRoute,
    ResolvingNode,
    InBattle,
    Completed,
    Retreated,
    Defeated
}

public enum OffenseRouteNodeKind
{
    Entrance,
    Battle,
    Event,
    Camp,
    Cache,
    Boss
}

public enum OffenseSupplyType
{
    Rations,
    Medicine,
    Tools,
    ManaLantern
}

public enum OffenseFormationSlot
{
    Front,
    Middle,
    Rear
}

[Flags]
public enum OffenseFormationMask
{
    None = 0,
    Front = 1 << 0,
    Middle = 1 << 1,
    Rear = 1 << 2,
    Any = Front | Middle | Rear
}

public static class OffenseFormationUtility
{
    public static OffenseFormationMask ToMask(OffenseFormationSlot slot)
    {
        return slot switch
        {
            OffenseFormationSlot.Front => OffenseFormationMask.Front,
            OffenseFormationSlot.Middle => OffenseFormationMask.Middle,
            _ => OffenseFormationMask.Rear
        };
    }

    public static string GetDisplayName(OffenseFormationSlot slot)
    {
        return slot switch
        {
            OffenseFormationSlot.Front => "전열",
            OffenseFormationSlot.Middle => "중열",
            _ => "후열"
        };
    }
}

public sealed class OffenseRouteNode
{
    private readonly IReadOnlyList<string> nextNodeIds;

    public OffenseRouteNode(
        string id,
        int depth,
        int lane,
        OffenseRouteNodeKind kind,
        string title,
        string description,
        float dangerMultiplier,
        IEnumerable<string> nextNodeIds)
    {
        Id = id ?? string.Empty;
        Depth = Mathf.Max(0, depth);
        Lane = Mathf.Max(0, lane);
        Kind = kind;
        Title = title ?? string.Empty;
        Description = description ?? string.Empty;
        DangerMultiplier = Mathf.Max(0.1f, dangerMultiplier);
        this.nextNodeIds = (nextNodeIds ?? Array.Empty<string>())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    public string Id { get; }
    public int Depth { get; }
    public int Lane { get; }
    public OffenseRouteNodeKind Kind { get; }
    public string Title { get; }
    public string Description { get; }
    public float DangerMultiplier { get; }
    public IReadOnlyList<string> NextNodeIds => nextNodeIds;
    public bool StartsBattle => Kind is OffenseRouteNodeKind.Battle or OffenseRouteNodeKind.Boss;
    public bool IsBoss => Kind == OffenseRouteNodeKind.Boss;
}

public sealed class OffenseRouteGraph
{
    private readonly Dictionary<string, OffenseRouteNode> nodeById;
    private readonly IReadOnlyList<OffenseRouteNode> nodes;

    public OffenseRouteGraph(IEnumerable<OffenseRouteNode> nodes, string entranceNodeId)
    {
        OffenseRouteNode[] safeNodes = (nodes ?? Array.Empty<OffenseRouteNode>())
            .Where(node => node != null && !string.IsNullOrWhiteSpace(node.Id))
            .OrderBy(node => node.Depth)
            .ThenBy(node => node.Lane)
            .ToArray();
        this.nodes = safeNodes;
        nodeById = safeNodes.ToDictionary(node => node.Id, StringComparer.Ordinal);
        EntranceNodeId = entranceNodeId ?? string.Empty;
        if (!nodeById.ContainsKey(EntranceNodeId))
        {
            throw new ArgumentException("The route entrance node is missing.", nameof(entranceNodeId));
        }
    }

    public string EntranceNodeId { get; }
    public IReadOnlyList<OffenseRouteNode> Nodes => nodes;

    public bool TryGetNode(string nodeId, out OffenseRouteNode node)
    {
        return nodeById.TryGetValue(nodeId ?? string.Empty, out node);
    }

    public IReadOnlyList<OffenseRouteNode> GetNextNodes(string currentNodeId)
    {
        if (!TryGetNode(currentNodeId, out OffenseRouteNode current))
        {
            return Array.Empty<OffenseRouteNode>();
        }

        return current.NextNodeIds
            .Select(id => nodeById.TryGetValue(id, out OffenseRouteNode node) ? node : null)
            .Where(node => node != null)
            .OrderBy(node => node.Lane)
            .ToArray();
    }
}

public static class OffenseRouteGenerator
{
    public static OffenseRouteGraph Create(OffenseTargetDefinition target)
    {
        string targetId = !string.IsNullOrWhiteSpace(target?.id) ? target.id : "unknown";
        int stage = Mathf.Max(1, target?.campaignOrder ?? 1);
        string entrance = $"{targetId}:entrance";
        string firstBattle = $"{targetId}:approach-battle";
        string firstEvent = $"{targetId}:approach-event";
        string camp = $"{targetId}:camp";
        string cache = $"{targetId}:cache";
        string elite = $"{targetId}:elite-battle";
        string lateEvent = $"{targetId}:deep-event";
        string boss = $"{targetId}:boss";

        return new OffenseRouteGraph(new[]
        {
            Node(entrance, 0, 0, OffenseRouteNodeKind.Entrance,
                "원정 입구", "보급을 점검하고 첫 경로를 고릅니다.", 0f,
                firstBattle, firstEvent),
            Node(firstBattle, 1, 0, OffenseRouteNodeKind.Battle,
                "경계 병력", "정면 경계를 돌파하는 빠르고 위험한 길입니다.", 0.75f,
                camp, cache),
            Node(firstEvent, 1, 1, OffenseRouteNodeKind.Event,
                "수상한 흔적", "도구를 쓰면 위험을 줄이고 정보를 얻을 수 있습니다.", 0.55f,
                camp, cache),
            Node(camp, 2, 0, OffenseRouteNodeKind.Camp,
                "숨 돌릴 곳", "식량을 사용해 체력과 스트레스를 회복할 수 있습니다.", 0.35f,
                elite, lateEvent),
            Node(cache, 2, 1, OffenseRouteNodeKind.Cache,
                "버려진 보급고", "운반 가능한 전리품이 있지만 더 깊은 길로 이어집니다.", 0.6f,
                elite, lateEvent),
            Node(elite, 3, 0, OffenseRouteNodeKind.Battle,
                "정예 수비대", "강한 적을 쓰러뜨리면 추가 전리품을 확보합니다.", 0.95f + stage * 0.03f,
                boss),
            Node(lateEvent, 3, 1, OffenseRouteNodeKind.Event,
                "봉인된 통로", "도구로 봉인을 해제하거나 위험을 감수해야 합니다.", 0.8f,
                boss),
            Node(boss, 4, 0, OffenseRouteNodeKind.Boss,
                target?.title ?? "지역 지휘관", "이 지역의 지휘관을 쓰러뜨려 원정을 끝냅니다.", 1f,
                Array.Empty<string>())
        }, entrance);
    }

    private static OffenseRouteNode Node(
        string id,
        int depth,
        int lane,
        OffenseRouteNodeKind kind,
        string title,
        string description,
        float danger,
        params string[] next)
    {
        return new OffenseRouteNode(id, depth, lane, kind, title, description, danger, next);
    }
}

public static class OffenseSupplyCatalog
{
    public static StockCategory GetStockCategory(OffenseSupplyType type)
    {
        return type switch
        {
            OffenseSupplyType.Rations => StockCategory.Food,
            OffenseSupplyType.Medicine => StockCategory.General,
            OffenseSupplyType.Tools => StockCategory.Weapon,
            _ => StockCategory.Mana
        };
    }

    public static string GetDisplayName(OffenseSupplyType type)
    {
        return type switch
        {
            OffenseSupplyType.Rations => "식량",
            OffenseSupplyType.Medicine => "치료약",
            OffenseSupplyType.Tools => "원정 도구",
            _ => "마력등"
        };
    }
}

public sealed class OffenseSupplyLoadout
{
    private readonly Dictionary<OffenseSupplyType, int> amounts;
    private readonly IReadOnlyDictionary<OffenseSupplyType, int> view;

    public OffenseSupplyLoadout(IReadOnlyDictionary<OffenseSupplyType, int> initial = null)
    {
        amounts = Enum.GetValues(typeof(OffenseSupplyType))
            .Cast<OffenseSupplyType>()
            .ToDictionary(type => type, _ => 0);
        if (initial != null)
        {
            foreach (KeyValuePair<OffenseSupplyType, int> pair in initial)
            {
                amounts[pair.Key] = Mathf.Max(0, pair.Value);
            }
        }

        view = new ReadOnlyDictionary<OffenseSupplyType, int>(amounts);
    }

    public IReadOnlyDictionary<OffenseSupplyType, int> Amounts => view;
    public int TotalCount => amounts.Values.Sum();
    public int Get(OffenseSupplyType type) => amounts.TryGetValue(type, out int value) ? value : 0;

    public void Add(OffenseSupplyType type, int amount)
    {
        if (amount > 0) amounts[type] = Get(type) + amount;
    }

    public bool TryConsume(OffenseSupplyType type, int amount)
    {
        int safeAmount = Mathf.Max(0, amount);
        if (safeAmount == 0) return true;
        if (Get(type) < safeAmount) return false;
        amounts[type] -= safeAmount;
        return true;
    }
}

public sealed class OffenseExpeditionPreparation
{
    public OffenseExpeditionPreparation(
        int supplyCapacity = 7,
        float startingLight = 45f,
        float campHealRatio = 0.12f,
        float campStressRecovery = 12f,
        float medicineHealRatio = 0.25f,
        int scouting = 0,
        IEnumerable<string> sourceSummaries = null)
    {
        SupplyCapacity = Mathf.Max(0, supplyCapacity);
        StartingLight = Mathf.Clamp(startingLight, 0f, 100f);
        CampHealRatio = Mathf.Clamp01(campHealRatio);
        CampStressRecovery = Mathf.Max(0f, campStressRecovery);
        MedicineHealRatio = Mathf.Clamp01(medicineHealRatio);
        Scouting = Mathf.Max(0, scouting);
        SourceSummaries = EventPayloadSnapshot.Copy<string>(
            (sourceSummaries ?? Array.Empty<string>()).ToArray());
    }

    public int SupplyCapacity { get; }
    public float StartingLight { get; }
    public float CampHealRatio { get; }
    public float CampStressRecovery { get; }
    public float MedicineHealRatio { get; }
    public int Scouting { get; }
    public IReadOnlyList<string> SourceSummaries { get; }
}

public sealed class OffenseExpeditionMemberState
{
    public OffenseExpeditionMemberState(
        CharacterActor actor,
        OffenseFormationSlot formation,
        float stress = 0f)
    {
        Actor = actor;
        Formation = formation;
        Stress = Mathf.Clamp(stress, 0f, 100f);
    }

    public CharacterActor Actor { get; }
    public OffenseFormationSlot Formation { get; internal set; }
    public float Stress { get; private set; }
    public float TotalDamageTaken { get; private set; }
    public bool IsAlive => Actor != null && !Actor.IsDead;

    public void AddStress(float amount)
    {
        Stress = Mathf.Clamp(Stress + Mathf.Max(0f, amount), 0f, 100f);
    }

    public void RecoverStress(float amount)
    {
        Stress = Mathf.Clamp(Stress - Mathf.Max(0f, amount), 0f, 100f);
    }

    public void RecordDamage(float amount)
    {
        TotalDamageTaken += Mathf.Max(0f, amount);
    }

    public void Restore(
        OffenseFormationSlot formation,
        float stress,
        float totalDamageTaken)
    {
        Formation = formation;
        Stress = Mathf.Clamp(stress, 0f, 100f);
        TotalDamageTaken = Mathf.Max(0f, totalDamageTaken);
    }
}

public sealed class OffenseExpeditionNodeResult
{
    public OffenseExpeditionNodeResult(string message, bool usedSupply, bool gainedLoot)
    {
        Message = message ?? string.Empty;
        UsedSupply = usedSupply;
        GainedLoot = gainedLoot;
    }

    public string Message { get; }
    public bool UsedSupply { get; }
    public bool GainedLoot { get; }
}
