using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum RunVariableCategory
{
    Start,
    Operation,
    Invasion
}

public enum RunVariableId
{
    None,
    SlimeCrowdVisit,
    OrcFeast,
    VampireNightVisit,
    FoodDeliveryDelay,
    GeneralGoodsDiscount,
    ManaStockSurplus,
    VisitingMerchant,
    DefenseFacilityDiscount,
    BlueprintRumor,
    ScoutTraces,
    Ambush,
    ArmedIntruder,
    LootPriority,
    TiredIntruder
}

public sealed class RunStartVariableSnapshot
{
    public int seed;
    public string ownerSpeciesTag;
    public InvasionThreatDifficulty difficulty;
    public int[] startingFacilityCandidateIds = Array.Empty<int>();
    public string[] startingGuestSpeciesCandidates = Array.Empty<string>();
    public int[] startingBlueprintCandidateIds = Array.Empty<int>();
    public int initialShopSeed;
    public string initialDungeonLayoutId;
    public float threatRiseMultiplier = 1f;

    public string ToSummaryText()
    {
        return string.Join("\n", new[]
        {
            $"사장 종족: {TextOrDefault(ownerSpeciesTag, "미정")}",
            $"시작 시설 후보: {FormatIds(startingFacilityCandidateIds)}",
            $"시작 손님층 후보: {FormatStrings(startingGuestSpeciesCandidates)}",
            $"시작 설계도 후보: {FormatIds(startingBlueprintCandidateIds)}",
            $"초기 상점 시드: {initialShopSeed}",
            $"초기 구조: {TextOrDefault(initialDungeonLayoutId, "기본")}",
            $"난이도: {difficulty} / 위협 계수 {threatRiseMultiplier:0.##}"
        });
    }

    private static string FormatIds(IEnumerable<int> values)
    {
        string text = values != null ? string.Join(", ", values) : string.Empty;
        return string.IsNullOrWhiteSpace(text) ? "없음" : text;
    }

    private static string FormatStrings(IEnumerable<string> values)
    {
        string text = values != null
            ? string.Join(", ", values.Where((value) => !string.IsNullOrWhiteSpace(value)))
            : string.Empty;
        return string.IsNullOrWhiteSpace(text) ? "없음" : text;
    }

    private static string TextOrDefault(string value, string defaultValue)
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }
}

public sealed class RunVariableDefinition
{
    public RunVariableId id;
    public RunVariableCategory category;
    public string title;
    public string detail;
    public EventAlertImportance importance;
    public int activeDays = 1;
    public string guestSpeciesTag;
    public float guestDemandMultiplier = 1f;
    public StockCategory stockCategory;
    public bool hasStockCostModifier;
    public float stockCostMultiplier = 1f;
    public float shopCostMultiplier = 1f;
    public float defenseShopCostMultiplier = 1f;
    public float blueprintCostMultiplier = 1f;
    public float threatRiseMultiplier = 1f;
    public float warningThresholdMultiplier = 1f;
    public float secondsToFullFocusMultiplier = 1f;
    public float repathIntervalMultiplier = 1f;
    public float facilityDamageIntervalMultiplier = 1f;
    public float finalCombatDamageMultiplier = 1f;

    public string ToDetailText()
    {
        return string.IsNullOrWhiteSpace(detail) ? title : detail;
    }
}

public sealed class ActiveRunVariable
{
    public ActiveRunVariable(RunVariableDefinition definition, int startDay)
    {
        Definition = definition;
        StartDay = Mathf.Max(1, startDay);
        RemainingDays = Mathf.Max(1, definition != null ? definition.activeDays : 1);
    }

    public RunVariableDefinition Definition { get; }
    public int StartDay { get; }
    public int RemainingDays { get; private set; }
    public bool IsExpired => RemainingDays <= 0;

    public void AdvanceDay()
    {
        RemainingDays = Mathf.Max(0, RemainingDays - 1);
    }
}

public sealed class RunVariableState
{
    private readonly List<ActiveRunVariable> activeOperationVariables = new List<ActiveRunVariable>();

    public RunStartVariableSnapshot StartVariables { get; private set; }
    public IReadOnlyList<ActiveRunVariable> ActiveOperationVariables => activeOperationVariables;
    public RunVariableDefinition CurrentInvasionVariable { get; private set; }
    public bool HasStarted => StartVariables != null;

    public void SetStartVariables(RunStartVariableSnapshot snapshot)
    {
        StartVariables = snapshot;
    }

    public ActiveRunVariable ActivateOperationVariable(RunVariableDefinition definition, int day)
    {
        if (definition == null || definition.category != RunVariableCategory.Operation)
        {
            return null;
        }

        activeOperationVariables.RemoveAll((active) => active == null || active.Definition == null || active.Definition.id == definition.id);
        ActiveRunVariable instance = new ActiveRunVariable(definition, day);
        activeOperationVariables.Add(instance);
        return instance;
    }

    public IReadOnlyList<ActiveRunVariable> AdvanceOperationVariables()
    {
        List<ActiveRunVariable> expired = new List<ActiveRunVariable>();
        foreach (ActiveRunVariable active in activeOperationVariables)
        {
            active.AdvanceDay();
            if (active.IsExpired)
            {
                expired.Add(active);
            }
        }

        activeOperationVariables.RemoveAll((active) => active == null || active.IsExpired);
        return expired;
    }

    public void SetInvasionVariable(RunVariableDefinition definition)
    {
        CurrentInvasionVariable = definition != null && definition.category == RunVariableCategory.Invasion
            ? definition
            : null;
    }

    public void ClearInvasionVariable()
    {
        CurrentInvasionVariable = null;
    }
}

public struct RunStartVariablesSelectedEvent
{
    public RunStartVariableSnapshot snapshot;

    public RunStartVariablesSelectedEvent(RunStartVariableSnapshot snapshot)
    {
        this.snapshot = snapshot;
    }

    private static RunStartVariablesSelectedEvent e;

    public static void Trigger(RunStartVariableSnapshot snapshot)
    {
        e.snapshot = snapshot;
        EventObserver.TriggerEvent(e);
    }
}

public struct RunVariableActivatedEvent
{
    public ActiveRunVariable activeVariable;

    public RunVariableActivatedEvent(ActiveRunVariable activeVariable)
    {
        this.activeVariable = activeVariable;
    }

    private static RunVariableActivatedEvent e;

    public static void Trigger(ActiveRunVariable activeVariable)
    {
        e.activeVariable = activeVariable;
        EventObserver.TriggerEvent(e);
    }
}

public struct RunVariableExpiredEvent
{
    public RunVariableDefinition definition;

    public RunVariableExpiredEvent(RunVariableDefinition definition)
    {
        this.definition = definition;
    }

    private static RunVariableExpiredEvent e;

    public static void Trigger(RunVariableDefinition definition)
    {
        e.definition = definition;
        EventObserver.TriggerEvent(e);
    }
}

public struct InvasionVariableSelectedEvent
{
    public RunVariableDefinition definition;

    public InvasionVariableSelectedEvent(RunVariableDefinition definition)
    {
        this.definition = definition;
    }

    private static InvasionVariableSelectedEvent e;

    public static void Trigger(RunVariableDefinition definition)
    {
        e.definition = definition;
        EventObserver.TriggerEvent(e);
    }
}

public static class RunVariableCatalog
{
    private static readonly Dictionary<RunVariableId, RunVariableDefinition> definitions = BuildDefinitions();

    public static IReadOnlyCollection<RunVariableDefinition> All => definitions.Values;

    public static RunVariableDefinition Get(RunVariableId id)
    {
        return definitions.TryGetValue(id, out RunVariableDefinition definition) ? definition : null;
    }

    public static IReadOnlyList<RunVariableDefinition> GetByCategory(RunVariableCategory category)
    {
        return definitions.Values
            .Where((definition) => definition.category == category)
            .ToList();
    }

    private static Dictionary<RunVariableId, RunVariableDefinition> BuildDefinitions()
    {
        RunVariableDefinition[] list =
        {
            new RunVariableDefinition
            {
                id = RunVariableId.SlimeCrowdVisit,
                category = RunVariableCategory.Operation,
                title = "슬라임 단체 방문",
                detail = "슬라임 손님 수요가 일시적으로 증가합니다.",
                importance = EventAlertImportance.Low,
                guestSpeciesTag = "Slime",
                guestDemandMultiplier = 1.7f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.OrcFeast,
                category = RunVariableCategory.Operation,
                title = "오크 회식",
                detail = "오크 손님 수요가 증가하고 고소비 시설 우선순위가 올라갑니다.",
                importance = EventAlertImportance.Low,
                guestSpeciesTag = "Orc",
                guestDemandMultiplier = 1.6f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.VampireNightVisit,
                category = RunVariableCategory.Operation,
                title = "뱀파이어 야간 방문",
                detail = "뱀파이어 손님 수요가 증가합니다.",
                importance = EventAlertImportance.Low,
                guestSpeciesTag = "Vampire",
                guestDemandMultiplier = 1.55f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.FoodDeliveryDelay,
                category = RunVariableCategory.Operation,
                title = "식자재 배송 지연",
                detail = "식자재 상품 가격이 올라갑니다.",
                importance = EventAlertImportance.Medium,
                stockCategory = StockCategory.Food,
                hasStockCostModifier = true,
                stockCostMultiplier = 1.5f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.GeneralGoodsDiscount,
                category = RunVariableCategory.Operation,
                title = "잡화 할인",
                detail = "잡화 상품 가격이 내려갑니다.",
                importance = EventAlertImportance.Low,
                stockCategory = StockCategory.General,
                hasStockCostModifier = true,
                stockCostMultiplier = 0.75f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.ManaStockSurplus,
                category = RunVariableCategory.Operation,
                title = "마력 재고 과잉",
                detail = "마력 상품 가격이 크게 내려갑니다.",
                importance = EventAlertImportance.Low,
                stockCategory = StockCategory.Mana,
                hasStockCostModifier = true,
                stockCostMultiplier = 0.65f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.VisitingMerchant,
                category = RunVariableCategory.Operation,
                title = "방문 상인",
                detail = "시설과 설계도 가격이 조금 내려갑니다.",
                importance = EventAlertImportance.Medium,
                shopCostMultiplier = 0.85f,
                blueprintCostMultiplier = 0.9f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.DefenseFacilityDiscount,
                category = RunVariableCategory.Operation,
                title = "방어 시설 할인",
                detail = "방어 시설 가격이 내려갑니다.",
                importance = EventAlertImportance.Low,
                defenseShopCostMultiplier = 0.7f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.BlueprintRumor,
                category = RunVariableCategory.Operation,
                title = "설계도 소문",
                detail = "설계도 가격이 내려가고 연구 목표를 뽑기 쉬워집니다.",
                importance = EventAlertImportance.Medium,
                blueprintCostMultiplier = 0.8f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.ScoutTraces,
                category = RunVariableCategory.Invasion,
                title = "정찰 흔적",
                detail = "침입자가 사장 위치를 더 빨리 좁혀 옵니다.",
                importance = EventAlertImportance.Medium,
                secondsToFullFocusMultiplier = 0.8f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.Ambush,
                category = RunVariableCategory.Invasion,
                title = "급습",
                detail = "침입자의 경로 재탐색이 빨라집니다.",
                importance = EventAlertImportance.High,
                repathIntervalMultiplier = 0.75f,
                finalCombatDamageMultiplier = 0.9f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.ArmedIntruder,
                category = RunVariableCategory.Invasion,
                title = "무장한 침입자",
                detail = "최종 교전 피해가 증가합니다.",
                importance = EventAlertImportance.High,
                finalCombatDamageMultiplier = 1.35f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.LootPriority,
                category = RunVariableCategory.Invasion,
                title = "약탈 우선",
                detail = "침입자가 주요 시설을 더 자주 훼손합니다.",
                importance = EventAlertImportance.Medium,
                facilityDamageIntervalMultiplier = 0.55f
            },
            new RunVariableDefinition
            {
                id = RunVariableId.TiredIntruder,
                category = RunVariableCategory.Invasion,
                title = "지친 침입자",
                detail = "최종 교전 피해가 낮아지지만 경로 탐색은 조금 느려집니다.",
                importance = EventAlertImportance.Low,
                secondsToFullFocusMultiplier = 1.15f,
                finalCombatDamageMultiplier = 0.7f
            }
        };

        return list.ToDictionary((definition) => definition.id, (definition) => definition);
    }
}

public class RunVariableRuntime :
    MonoBehaviour,
    UtilEventListener<OperatingDayStartedEvent>,
    UtilEventListener<OperatingDayEndedEvent>,
    UtilEventListener<InvasionCandidateEvent>,
    UtilEventListener<InvasionResolvedEvent>
{
    [SerializeField] private int runSeed;
    [SerializeField] private bool raiseAlerts = true;

    private readonly RunVariableState state = new RunVariableState();
    private System.Random random;
    private int currentDay = 1;
    private static RunVariableRuntime instance;

    public static RunVariableRuntime Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<RunVariableRuntime>();
            }

            return instance;
        }
    }

    public RunVariableState State => state;

    private void Awake()
    {
        instance = this;
        if (runSeed == 0)
        {
            runSeed = Environment.TickCount;
        }

        random = new System.Random(runSeed);
    }

    public void StartRun(int seed, CharacterSO ownerData = null, InvasionThreatDifficulty difficulty = InvasionThreatDifficulty.Normal)
    {
        runSeed = seed != 0 ? seed : Environment.TickCount;
        random = new System.Random(runSeed);
        RunStartVariableSnapshot snapshot = CreateStartVariables(runSeed, ownerData, difficulty);
        state.SetStartVariables(snapshot);
        RunStartVariablesSelectedEvent.Trigger(snapshot);

        if (raiseAlerts)
        {
            EventAlertService.Raise(
                "런 시작 변수",
                snapshot.ToSummaryText(),
                EventAlertImportance.Low,
                "런 변수");
        }
    }

    public void OnTriggerEvent(OperatingDayStartedEvent eventType)
    {
        currentDay = Mathf.Max(1, eventType.day);
        EnsureRunStarted();

        if (currentDay > 1)
        {
            RollOperationVariable(currentDay);
        }
    }

    public void OnTriggerEvent(OperatingDayEndedEvent eventType)
    {
        IReadOnlyList<ActiveRunVariable> expired = state.AdvanceOperationVariables();
        foreach (ActiveRunVariable active in expired)
        {
            RunVariableExpiredEvent.Trigger(active.Definition);
        }
    }

    public void OnTriggerEvent(InvasionCandidateEvent eventType)
    {
        EnsureRunStarted();
        SelectRandomInvasionVariable();
    }

    public void OnTriggerEvent(InvasionResolvedEvent eventType)
    {
        state.ClearInvasionVariable();
    }

    public ActiveRunVariable ActivateOperationVariable(RunVariableId id, int day = -1, bool alert = true)
    {
        RunVariableDefinition definition = RunVariableCatalog.Get(id);
        ActiveRunVariable active = state.ActivateOperationVariable(definition, day > 0 ? day : currentDay);
        if (active == null)
        {
            return null;
        }

        RunVariableActivatedEvent.Trigger(active);
        if (alert && raiseAlerts)
        {
            EventAlertService.Raise(
                active.Definition.title,
                active.Definition.ToDetailText(),
                active.Definition.importance,
                "운영 변수");
        }

        return active;
    }

    public RunVariableDefinition SelectInvasionVariable(RunVariableId id, bool alert = true)
    {
        RunVariableDefinition definition = RunVariableCatalog.Get(id);
        if (definition == null || definition.category != RunVariableCategory.Invasion)
        {
            return null;
        }

        state.SetInvasionVariable(definition);
        InvasionVariableSelectedEvent.Trigger(definition);
        if (alert && raiseAlerts)
        {
            EventAlertService.Raise(
                definition.title,
                definition.ToDetailText(),
                definition.importance,
                "침입 변수");
        }

        return definition;
    }

    public float GetGuestDemandMultiplier(string speciesTag)
    {
        if (string.IsNullOrWhiteSpace(speciesTag))
        {
            return 1f;
        }

        return state.ActiveOperationVariables
            .Where((active) => active?.Definition != null
                && !string.IsNullOrWhiteSpace(active.Definition.guestSpeciesTag)
                && string.Equals(active.Definition.guestSpeciesTag, speciesTag, StringComparison.OrdinalIgnoreCase))
            .Aggregate(1f, (value, active) => value * Mathf.Max(0.1f, active.Definition.guestDemandMultiplier));
    }

    public float GetStockCostMultiplier(StockCategory category)
    {
        return state.ActiveOperationVariables
            .Where((active) => active?.Definition != null
                && active.Definition.hasStockCostModifier
                && active.Definition.stockCategory == category)
            .Aggregate(1f, (value, active) => value * Mathf.Max(0.05f, active.Definition.stockCostMultiplier));
    }

    public float GetFacilityShopCostMultiplier(BuildingSO building)
    {
        float multiplier = state.ActiveOperationVariables
            .Where((active) => active?.Definition != null)
            .Aggregate(1f, (value, active) => value * Mathf.Max(0.05f, active.Definition.shopCostMultiplier));

        if (building != null && building.Defense != null && building.Defense.IsDefenseFacility)
        {
            multiplier *= state.ActiveOperationVariables
                .Where((active) => active?.Definition != null)
                .Aggregate(1f, (value, active) => value * Mathf.Max(0.05f, active.Definition.defenseShopCostMultiplier));
        }

        return multiplier;
    }

    public float GetBlueprintCostMultiplier(FacilityBlueprintSO blueprint)
    {
        return state.ActiveOperationVariables
            .Where((active) => active?.Definition != null)
            .Aggregate(1f, (value, active) => value * Mathf.Max(0.05f, active.Definition.blueprintCostMultiplier));
    }

    public float GetThreatRiseMultiplier()
    {
        float startMultiplier = state.StartVariables != null
            ? Mathf.Max(0.05f, state.StartVariables.threatRiseMultiplier)
            : 1f;
        float eventMultiplier = state.ActiveOperationVariables
            .Where((active) => active?.Definition != null)
            .Aggregate(1f, (value, active) => value * Mathf.Max(0.05f, active.Definition.threatRiseMultiplier));
        return startMultiplier * eventMultiplier;
    }

    public float GetWarningThresholdMultiplier()
    {
        return state.ActiveOperationVariables
            .Where((active) => active?.Definition != null)
            .Aggregate(1f, (value, active) => value * Mathf.Max(0.05f, active.Definition.warningThresholdMultiplier));
    }

    public InvasionIntruderSettings ApplyInvasionSettings(InvasionIntruderSettings source)
    {
        InvasionIntruderSettings result = new InvasionIntruderSettings();
        if (source != null)
        {
            result.secondsToFullFocus = source.secondsToFullFocus;
            result.repathIntervalSeconds = source.repathIntervalSeconds;
            result.facilityDamageIntervalSeconds = source.facilityDamageIntervalSeconds;
            result.finalCombatDamage = source.finalCombatDamage;
            result.finalCombatWindupSeconds = source.finalCombatWindupSeconds;
        }

        RunVariableDefinition definition = state.CurrentInvasionVariable;
        if (definition == null)
        {
            return result;
        }

        result.secondsToFullFocus = Mathf.Max(0.1f, result.secondsToFullFocus * Mathf.Max(0.05f, definition.secondsToFullFocusMultiplier));
        result.repathIntervalSeconds = Mathf.Max(0.1f, result.repathIntervalSeconds * Mathf.Max(0.05f, definition.repathIntervalMultiplier));
        result.facilityDamageIntervalSeconds = Mathf.Max(0f, result.facilityDamageIntervalSeconds * Mathf.Max(0.05f, definition.facilityDamageIntervalMultiplier));
        result.finalCombatDamage = Mathf.Max(0f, result.finalCombatDamage * Mathf.Max(0.05f, definition.finalCombatDamageMultiplier));
        return result;
    }

    private void RollOperationVariable(int day)
    {
        IReadOnlyList<RunVariableDefinition> definitions = RunVariableCatalog.GetByCategory(RunVariableCategory.Operation);
        if (definitions.Count == 0)
        {
            return;
        }

        RunVariableDefinition selected = definitions[random.Next(definitions.Count)];
        ActivateOperationVariable(selected.id, day);
    }

    private void SelectRandomInvasionVariable()
    {
        if (random == null)
        {
            random = new System.Random(runSeed != 0 ? runSeed : Environment.TickCount);
        }

        IReadOnlyList<RunVariableDefinition> definitions = RunVariableCatalog.GetByCategory(RunVariableCategory.Invasion);
        if (definitions.Count == 0)
        {
            return;
        }

        SelectInvasionVariable(definitions[random.Next(definitions.Count)].id);
    }

    private void EnsureRunStarted()
    {
        if (random == null)
        {
            random = new System.Random(runSeed != 0 ? runSeed : Environment.TickCount);
        }

        if (state.HasStarted)
        {
            return;
        }

        CharacterSO owner = OwnerRunManager.Instance != null
            ? OwnerRunManager.Instance.selectedOwnerData.Value
            : null;
        StartRun(runSeed, owner, ResolveDifficulty());
    }

    private RunStartVariableSnapshot CreateStartVariables(
        int seed,
        CharacterSO ownerData,
        InvasionThreatDifficulty difficulty)
    {
        System.Random startRandom = new System.Random(seed);
        int startingFacilityCount = 3 + (MetaProgressionRuntime.Instance != null
            ? MetaProgressionRuntime.Instance.GetStartingFacilityCandidateBonus()
            : 0);
        BuildingSO[] buildings = Resources.LoadAll<BuildingSO>("SO/Building")
            .Where((building) => building != null
                && !building.IsGridMovement
                && !building.IsWall
                && FacilityShopService.GetBuildingStar(building) <= 1)
            .OrderBy((_) => startRandom.NextDouble())
            .Take(Mathf.Max(1, startingFacilityCount))
            .ToArray();
        CharacterSO[] customers = Resources.LoadAll<CharacterSO>("SO/Character")
            .Where((character) => character != null && character.characterType == CharacterType.Customer)
            .OrderBy((_) => startRandom.NextDouble())
            .Take(3)
            .ToArray();
        FacilityBlueprintSO[] blueprints = Resources.LoadAll<FacilityBlueprintSO>("SO/Blueprint")
            .Where((blueprint) => blueprint != null)
            .OrderBy((_) => startRandom.NextDouble())
            .Take(2)
            .ToArray();

        return new RunStartVariableSnapshot
        {
            seed = seed,
            ownerSpeciesTag = !string.IsNullOrWhiteSpace(ownerData?.SpeciesTag) ? ownerData.SpeciesTag : "Unknown",
            difficulty = difficulty,
            startingFacilityCandidateIds = buildings.Select((building) => building.id).DefaultIfEmpty(-1).Where((id) => id >= 0).ToArray(),
            startingGuestSpeciesCandidates = customers.Select((customer) => customer.SpeciesTag).Where((tag) => !string.IsNullOrWhiteSpace(tag)).Distinct().ToArray(),
            startingBlueprintCandidateIds = blueprints.Select((blueprint) => blueprint.id).DefaultIfEmpty(-1).Where((id) => id >= 0).ToArray(),
            initialShopSeed = seed ^ 0x5F3759DF,
            initialDungeonLayoutId = ResolveLayoutId(ownerData, startRandom),
            threatRiseMultiplier = difficulty switch
            {
                InvasionThreatDifficulty.Easy => 0.85f,
                InvasionThreatDifficulty.Hard => 1.2f,
                _ => 1f
            }
        };
    }

    private InvasionThreatDifficulty ResolveDifficulty()
    {
        InvasionThreatRuntime threatRuntime = InvasionThreatRuntime.Instance;
        return threatRuntime != null && threatRuntime.Settings != null
            ? threatRuntime.Settings.difficulty
            : InvasionThreatDifficulty.Normal;
    }

    private static string ResolveLayoutId(CharacterSO ownerData, System.Random startRandom)
    {
        string species = !string.IsNullOrWhiteSpace(ownerData?.SpeciesTag) ? ownerData.SpeciesTag : string.Empty;
        if (species.Equals("Slime", StringComparison.OrdinalIgnoreCase))
        {
            return "wet-front";
        }

        if (species.Equals("Orc", StringComparison.OrdinalIgnoreCase))
        {
            return "training-front";
        }

        if (species.Equals("Vampire", StringComparison.OrdinalIgnoreCase))
        {
            return "research-front";
        }

        string[] defaultLayouts =
        {
            "compact-shop",
            "split-hall",
            "stairs-first"
        };
        return defaultLayouts[startRandom.Next(defaultLayouts.Length)];
    }

    private void OnEnable()
    {
        this.EventStartListening<OperatingDayStartedEvent>();
        this.EventStartListening<OperatingDayEndedEvent>();
        this.EventStartListening<InvasionCandidateEvent>();
        this.EventStartListening<InvasionResolvedEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<OperatingDayStartedEvent>();
        this.EventStopListening<OperatingDayEndedEvent>();
        this.EventStopListening<InvasionCandidateEvent>();
        this.EventStopListening<InvasionResolvedEvent>();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
