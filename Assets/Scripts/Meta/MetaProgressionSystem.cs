using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum MetaProgressionBranch
{
    OperationKnowledge,
    DesignPreservation,
    OwnerSurvival
}

public enum MetaUpgradeId
{
    None,
    StartingFacilityCandidatePlusOne,
    StartingOwnerTraitCandidatePlusOne,
    BasicPurchaseListExpansion,
    SpecialRecipeRecordSlot,
    OwnerSurvivalBonus,
    InvasionWarningAccuracy
}

public sealed class MetaUpgradeDefinition
{
    public MetaUpgradeId id;
    public MetaProgressionBranch branch;
    public string title;
    public string detail;
    public int cost;
    public int maxLevel = 1;
}

public static class MetaProgressionCatalog
{
    private static readonly Dictionary<MetaUpgradeId, MetaUpgradeDefinition> definitions = BuildDefinitions();

    public static IReadOnlyCollection<MetaUpgradeDefinition> All => definitions.Values;

    public static MetaUpgradeDefinition Get(MetaUpgradeId id)
    {
        return definitions.TryGetValue(id, out MetaUpgradeDefinition definition) ? definition : null;
    }

    private static Dictionary<MetaUpgradeId, MetaUpgradeDefinition> BuildDefinitions()
    {
        MetaUpgradeDefinition[] list =
        {
            new MetaUpgradeDefinition
            {
                id = MetaUpgradeId.StartingFacilityCandidatePlusOne,
                branch = MetaProgressionBranch.OperationKnowledge,
                title = "시작 시설 후보 +1",
                detail = "런 시작 시설 후보가 1개 늘어납니다.",
                cost = 80,
                maxLevel = 2
            },
            new MetaUpgradeDefinition
            {
                id = MetaUpgradeId.StartingOwnerTraitCandidatePlusOne,
                branch = MetaProgressionBranch.OwnerSurvival,
                title = "시작 사장 특성 후보 +1",
                detail = "사장 특성 선택 후보를 늘리는 메타 보상입니다.",
                cost = 90,
                maxLevel = 1
            },
            new MetaUpgradeDefinition
            {
                id = MetaUpgradeId.BasicPurchaseListExpansion,
                branch = MetaProgressionBranch.OperationKnowledge,
                title = "1일차 기본 구매 목록 확장",
                detail = "일부 1성 시설이 기본 구매 후보에 미리 들어옵니다.",
                cost = 100,
                maxLevel = 3
            },
            new MetaUpgradeDefinition
            {
                id = MetaUpgradeId.SpecialRecipeRecordSlot,
                branch = MetaProgressionBranch.DesignPreservation,
                title = "특수 조합식 기록 보존",
                detail = "런에서 해금한 특수 조합식 일부를 다음 런에 기억합니다.",
                cost = 120,
                maxLevel = 3
            },
            new MetaUpgradeDefinition
            {
                id = MetaUpgradeId.OwnerSurvivalBonus,
                branch = MetaProgressionBranch.OwnerSurvival,
                title = "사장 생존력 보너스 증가",
                detail = "사장 최대 체력이 조금 오릅니다.",
                cost = 110,
                maxLevel = 3
            },
            new MetaUpgradeDefinition
            {
                id = MetaUpgradeId.InvasionWarningAccuracy,
                branch = MetaProgressionBranch.OwnerSurvival,
                title = "침입 전 경고 정확도 증가",
                detail = "침입 경고를 조금 더 일찍 받을 수 있습니다.",
                cost = 90,
                maxLevel = 2
            }
        };

        return list.ToDictionary((definition) => definition.id, (definition) => definition);
    }
}

public sealed class MetaProgressionState
{
    private readonly Dictionary<MetaUpgradeId, int> upgradeLevels = new Dictionary<MetaUpgradeId, int>();
    private readonly HashSet<string> preservedRecipeIds = new HashSet<string>();

    public int LifetimeEarnedCurrency { get; private set; }
    public int SpentCurrency { get; private set; }
    public int AvailableCurrency => Mathf.Max(0, LifetimeEarnedCurrency - SpentCurrency);
    public IReadOnlyDictionary<MetaUpgradeId, int> UpgradeLevels => upgradeLevels;
    public IReadOnlyCollection<string> PreservedRecipeIds => preservedRecipeIds;

    public void AddCurrency(int amount)
    {
        LifetimeEarnedCurrency += Mathf.Max(0, amount);
    }

    public int GetUpgradeLevel(MetaUpgradeId id)
    {
        return upgradeLevels.TryGetValue(id, out int level) ? level : 0;
    }

    public bool TryPurchaseUpgrade(MetaUpgradeId id, out string message)
    {
        MetaUpgradeDefinition definition = MetaProgressionCatalog.Get(id);
        if (definition == null)
        {
            message = "강화 정보가 없습니다";
            return false;
        }

        int currentLevel = GetUpgradeLevel(id);
        if (currentLevel >= Mathf.Max(1, definition.maxLevel))
        {
            message = "이미 최대 레벨입니다";
            return false;
        }

        int cost = Mathf.Max(0, definition.cost);
        if (AvailableCurrency < cost)
        {
            message = "계승 재화가 부족합니다";
            return false;
        }

        SpentCurrency += cost;
        upgradeLevels[id] = currentLevel + 1;
        message = $"{definition.title} Lv.{currentLevel + 1}";
        return true;
    }

    public void SetUpgradeLevelForDebug(MetaUpgradeId id, int level)
    {
        MetaUpgradeDefinition definition = MetaProgressionCatalog.Get(id);
        if (definition == null) return;

        upgradeLevels[id] = Mathf.Clamp(level, 0, Mathf.Max(1, definition.maxLevel));
    }

    public void PreserveRecipes(IEnumerable<string> recipeIds, int maxCount)
    {
        if (recipeIds == null || maxCount <= 0)
        {
            return;
        }

        foreach (string recipeId in recipeIds.Where((id) => !string.IsNullOrWhiteSpace(id)).Take(maxCount))
        {
            preservedRecipeIds.Add(recipeId);
        }
    }
}

public sealed class RunResultSnapshot
{
    public string ownerName;
    public string endReason;
    public float survivalSeconds;
    public int survivedOperatingDays;
    public int settlementCount;
    public int defendedInvasionCount;
    public InvasionThreatStage maxThreatStage;
    public float finalInvasionThreat;
    public int firstDiscoveredFacilityCount;
    public int firstUnlockedRecipeCount;
    public int offenseSuccessCount;
    public float difficultyMultiplier = 1f;
    public int legacyCurrency;

    public string ToDetailText()
    {
        return string.Join("\n", new[]
        {
            "런 결과 정산",
            string.Empty,
            $"사장: {TextOrDefault(ownerName, "미상")}",
            $"종료 원인: {TextOrDefault(endReason, "사장 쓰러짐")}",
            $"생존 시간: {FormatTime(survivalSeconds)}",
            $"생존 운영일: {survivedOperatingDays}",
            $"운영일 정산 횟수: {settlementCount}",
            $"막아낸 침입: {defendedInvasionCount}",
            $"최대 위협 단계: {FormatThreatStage(maxThreatStage)}",
            $"최종 침입 강도: {finalInvasionThreat:0.#}",
            $"최초 발견 시설: {firstDiscoveredFacilityCount}",
            $"최초 해금 조합식: {firstUnlockedRecipeCount}",
            $"원정 성과: {offenseSuccessCount}",
            $"난이도 배율: x{difficultyMultiplier:0.##}",
            string.Empty,
            $"획득 계승 재화: {legacyCurrency}",
            string.Empty,
            "계승되지 않음: 현재 런의 돈, 재고, 배치 시설"
        });
    }

    private static string FormatTime(float seconds)
    {
        int safeSeconds = Mathf.Max(0, Mathf.RoundToInt(seconds));
        int minutes = safeSeconds / 60;
        int remainSeconds = safeSeconds % 60;
        return $"{minutes:0}:{remainSeconds:00}";
    }

    private static string TextOrDefault(string value, string defaultValue)
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    private static string FormatThreatStage(InvasionThreatStage stage)
    {
        return stage switch
        {
            InvasionThreatStage.Peaceful => "평온",
            InvasionThreatStage.Warning => "경고",
            InvasionThreatStage.Candidate => "침입 후보",
            InvasionThreatStage.Safety => "안전 시간",
            _ => stage.ToString()
        };
    }
}

public static class MetaProgressionCalculator
{
    public static int CalculateLegacyCurrency(RunResultSnapshot result)
    {
        if (result == null)
        {
            return 0;
        }

        float survivalReward = (result.survivalSeconds / 60f) * 8f
            + Mathf.Max(0, result.survivedOperatingDays) * 35f
            + Mathf.Max(0, result.settlementCount) * 10f;
        float defenseReward = Mathf.Max(0, result.defendedInvasionCount) * 30f;
        float threatReward = GetThreatStageScore(result.maxThreatStage) * 12f
            + Mathf.Max(0f, result.finalInvasionThreat) * 0.25f;
        float discoveryReward = Mathf.Min(60f, Mathf.Max(0, result.firstDiscoveredFacilityCount) * 4f)
            + Mathf.Min(60f, Mathf.Max(0, result.firstUnlockedRecipeCount) * 10f);
        float offenseReward = Mathf.Max(0, result.offenseSuccessCount) * 25f;
        float raw = survivalReward + defenseReward + threatReward + discoveryReward + offenseReward;
        return Mathf.Max(0, Mathf.RoundToInt(raw * Mathf.Max(0.1f, result.difficultyMultiplier)));
    }

    private static int GetThreatStageScore(InvasionThreatStage stage)
    {
        return stage switch
        {
            InvasionThreatStage.Warning => 1,
            InvasionThreatStage.Candidate => 2,
            InvasionThreatStage.Safety => 2,
            _ => 0
        };
    }
}

public struct RunResultReadyEvent
{
    public RunResultSnapshot result;

    public RunResultReadyEvent(RunResultSnapshot result)
    {
        this.result = result;
    }

    private static RunResultReadyEvent e;

    public static void Trigger(RunResultSnapshot result)
    {
        e.result = result;
        EventObserver.TriggerEvent(e);
    }
}

public class MetaProgressionRuntime :
    MonoBehaviour,
    UtilEventListener<OperatingDayStartedEvent>,
    UtilEventListener<OperatingDayReportEvent>,
    UtilEventListener<InvasionThreatWarningEvent>,
    UtilEventListener<InvasionCandidateEvent>,
    UtilEventListener<InvasionStartedEvent>,
    UtilEventListener<InvasionResolvedEvent>,
    UtilEventListener<FacilityVisitEvent>,
    UtilEventListener<BlueprintResearchCompletedEvent>,
    UtilEventListener<FacilitySynthesisCompletedEvent>,
    UtilEventListener<OwnerRunEndedEvent>
{
    [SerializeField] private bool showRunResultPanel = true;

    private readonly MetaProgressionState state = new MetaProgressionState();
    private readonly HashSet<int> discoveredFacilityIdsThisRun = new HashSet<int>();
    private readonly HashSet<string> unlockedRecipeIdsThisRun = new HashSet<string>();
    private static MetaProgressionRuntime instance;

    private float runStartTime;
    private int currentDay = 1;
    private int settlementCount;
    private int defendedInvasionCount;
    private InvasionThreatStage maxThreatStage = InvasionThreatStage.Peaceful;
    private float finalInvasionThreat;
    private int offenseSuccessCount;
    private bool ended;

    public static MetaProgressionRuntime Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<MetaProgressionRuntime>();
            }

            return instance;
        }
    }

    public MetaProgressionState State => state;
    public RunResultSnapshot LatestResult { get; private set; }

    public void SetShowRunResultPanel(bool value)
    {
        showRunResultPanel = value;
    }

    private void Awake()
    {
        instance = this;
        StartNewRun();
    }

    public void StartNewRun()
    {
        runStartTime = Time.time;
        currentDay = 1;
        settlementCount = 0;
        defendedInvasionCount = 0;
        maxThreatStage = InvasionThreatStage.Peaceful;
        finalInvasionThreat = 0f;
        offenseSuccessCount = 0;
        ended = false;
        discoveredFacilityIdsThisRun.Clear();
        unlockedRecipeIdsThisRun.Clear();
        LatestResult = null;
    }

    public bool TryPurchaseUpgrade(MetaUpgradeId id, out string message)
    {
        bool success = state.TryPurchaseUpgrade(id, out message);
        if (success)
        {
            EventAlertService.Raise("계승 강화", message, EventAlertImportance.Medium, "계승");
        }

        return success;
    }

    public int GetStartingFacilityCandidateBonus()
    {
        return state.GetUpgradeLevel(MetaUpgradeId.StartingFacilityCandidatePlusOne);
    }

    public int GetStartingOwnerTraitCandidateBonus()
    {
        return state.GetUpgradeLevel(MetaUpgradeId.StartingOwnerTraitCandidatePlusOne);
    }

    public float GetOwnerMaxHealthMultiplier()
    {
        return 1f + (state.GetUpgradeLevel(MetaUpgradeId.OwnerSurvivalBonus) * 0.08f);
    }

    public float GetInvasionWarningThresholdMultiplier()
    {
        return 1f - (state.GetUpgradeLevel(MetaUpgradeId.InvasionWarningAccuracy) * 0.08f);
    }

    public bool IsRecipePreserved(string recipeId)
    {
        return !string.IsNullOrWhiteSpace(recipeId)
            && state.PreservedRecipeIds.Contains(recipeId);
    }

    public IReadOnlyCollection<int> GetExpandedBasicPurchaseBuildingIds(IEnumerable<BuildingSO> buildings)
    {
        int count = state.GetUpgradeLevel(MetaUpgradeId.BasicPurchaseListExpansion);
        if (count <= 0)
        {
            return Array.Empty<int>();
        }

        return buildings?
            .Where((building) => building != null
                && !building.IsGridMovement
                && !building.IsWall
                && FacilityShopService.GetBuildingStar(building) <= 1)
            .OrderBy((building) => building.id)
            .Take(count)
            .Select((building) => building.id)
            .ToArray()
            ?? Array.Empty<int>();
    }

    public void RecordOffenseSuccess()
    {
        offenseSuccessCount++;
    }

    public void OnTriggerEvent(OperatingDayStartedEvent eventType)
    {
        if (eventType.day <= 1 && ended)
        {
            StartNewRun();
        }

        currentDay = Mathf.Max(currentDay, eventType.day);
    }

    public void OnTriggerEvent(OperatingDayReportEvent eventType)
    {
        if (eventType.report != null)
        {
            settlementCount++;
            currentDay = Mathf.Max(currentDay, eventType.report.day);
        }
    }

    public void OnTriggerEvent(InvasionThreatWarningEvent eventType)
    {
        RecordThreat(eventType.snapshot);
    }

    public void OnTriggerEvent(InvasionCandidateEvent eventType)
    {
        RecordThreat(eventType.snapshot);
    }

    public void OnTriggerEvent(InvasionStartedEvent eventType)
    {
        RecordThreat(eventType.snapshot);
        finalInvasionThreat = Mathf.Max(finalInvasionThreat, eventType.snapshot.threat);
    }

    public void OnTriggerEvent(InvasionResolvedEvent eventType)
    {
        if (eventType.defended)
        {
            defendedInvasionCount++;
        }
    }

    public void OnTriggerEvent(FacilityVisitEvent eventType)
    {
        BuildingSO buildingData = eventType.facility != null ? eventType.facility.BuildingData : null;
        if (buildingData != null && buildingData.id >= 0)
        {
            discoveredFacilityIdsThisRun.Add(buildingData.id);
        }
    }

    public void OnTriggerEvent(BlueprintResearchCompletedEvent eventType)
    {
        foreach (string recipeId in eventType.unlockResult.UnlockedRecipes ?? Array.Empty<string>())
        {
            if (!string.IsNullOrWhiteSpace(recipeId))
            {
                unlockedRecipeIdsThisRun.Add(recipeId);
            }
        }
    }

    public void OnTriggerEvent(FacilitySynthesisCompletedEvent eventType)
    {
        string recipeId = eventType.result.Recipe != null ? eventType.result.Recipe.recipeId : string.Empty;
        if (!string.IsNullOrWhiteSpace(recipeId))
        {
            unlockedRecipeIdsThisRun.Add(recipeId);
        }

        BuildingSO resultBuilding = eventType.result.ResultBuilding != null
            ? eventType.result.ResultBuilding.BuildingData
            : eventType.result.Recipe != null
                ? eventType.result.Recipe.resultBuilding
                : null;
        if (resultBuilding != null && resultBuilding.id >= 0)
        {
            discoveredFacilityIdsThisRun.Add(resultBuilding.id);
        }
    }

    public void OnTriggerEvent(OwnerRunEndedEvent eventType)
    {
        EndRun(eventType.Owner, eventType.Reason);
    }

    public RunResultSnapshot EndRun(Character owner, string reason)
    {
        if (ended && LatestResult != null)
        {
            return LatestResult;
        }

        ended = true;
        RunResultSnapshot result = BuildResult(owner, reason);
        result.legacyCurrency = MetaProgressionCalculator.CalculateLegacyCurrency(result);
        state.AddCurrency(result.legacyCurrency);
        PreserveRunRecipes();
        LatestResult = result;

        RunResultReadyEvent.Trigger(result);
        EventAlertService.Raise("런 결과 정산", result.ToDetailText(), EventAlertImportance.High, "계승");
        if (showRunResultPanel)
        {
            RunResultPanel.Show(result);
        }

        return result;
    }

    private RunResultSnapshot BuildResult(Character owner, string reason)
    {
        InvasionThreatRuntime threatRuntime = InvasionThreatRuntime.Instance;
        float difficultyMultiplier = threatRuntime != null && threatRuntime.Settings != null
            ? threatRuntime.Settings.GetDifficultyMultiplier()
            : 1f;

        return new RunResultSnapshot
        {
            ownerName = owner != null && owner.data != null
                ? owner.data.characterName
                : owner != null ? owner.name : "사장",
            endReason = string.IsNullOrWhiteSpace(reason) ? "사장 쓰러짐" : reason,
            survivalSeconds = Mathf.Max(0f, Time.time - runStartTime),
            survivedOperatingDays = Mathf.Max(1, currentDay),
            settlementCount = Mathf.Max(0, settlementCount),
            defendedInvasionCount = Mathf.Max(0, defendedInvasionCount),
            maxThreatStage = maxThreatStage,
            finalInvasionThreat = Mathf.Max(0f, finalInvasionThreat),
            firstDiscoveredFacilityCount = discoveredFacilityIdsThisRun.Count,
            firstUnlockedRecipeCount = unlockedRecipeIdsThisRun.Count,
            offenseSuccessCount = Mathf.Max(0, offenseSuccessCount),
            difficultyMultiplier = Mathf.Max(0.1f, difficultyMultiplier)
        };
    }

    private void PreserveRunRecipes()
    {
        int slots = state.GetUpgradeLevel(MetaUpgradeId.SpecialRecipeRecordSlot);
        state.PreserveRecipes(unlockedRecipeIdsThisRun.OrderBy((id) => id), slots);
    }

    private void RecordThreat(InvasionThreatSnapshot snapshot)
    {
        if (GetThreatStageScore(snapshot.stage) > GetThreatStageScore(maxThreatStage))
        {
            maxThreatStage = snapshot.stage;
        }

        finalInvasionThreat = Mathf.Max(finalInvasionThreat, snapshot.threat);
    }

    private static int GetThreatStageScore(InvasionThreatStage stage)
    {
        return stage switch
        {
            InvasionThreatStage.Warning => 1,
            InvasionThreatStage.Candidate => 2,
            InvasionThreatStage.Safety => 2,
            _ => 0
        };
    }

    private void OnEnable()
    {
        this.EventStartListening<OperatingDayStartedEvent>();
        this.EventStartListening<OperatingDayReportEvent>();
        this.EventStartListening<InvasionThreatWarningEvent>();
        this.EventStartListening<InvasionCandidateEvent>();
        this.EventStartListening<InvasionStartedEvent>();
        this.EventStartListening<InvasionResolvedEvent>();
        this.EventStartListening<FacilityVisitEvent>();
        this.EventStartListening<BlueprintResearchCompletedEvent>();
        this.EventStartListening<FacilitySynthesisCompletedEvent>();
        this.EventStartListening<OwnerRunEndedEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<OperatingDayStartedEvent>();
        this.EventStopListening<OperatingDayReportEvent>();
        this.EventStopListening<InvasionThreatWarningEvent>();
        this.EventStopListening<InvasionCandidateEvent>();
        this.EventStopListening<InvasionStartedEvent>();
        this.EventStopListening<InvasionResolvedEvent>();
        this.EventStopListening<FacilityVisitEvent>();
        this.EventStopListening<BlueprintResearchCompletedEvent>();
        this.EventStopListening<FacilitySynthesisCompletedEvent>();
        this.EventStopListening<OwnerRunEndedEvent>();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}

public class RunResultPanel : MonoBehaviour
{
    private TMP_Text detailText;

    public static RunResultPanel Show(RunResultSnapshot result)
    {
        RunResultPanel panel = FindFirstObjectByType<RunResultPanel>();
        if (panel == null)
        {
            panel = CreateDefaultPanel();
        }

        panel.Render(result);
        return panel;
    }

    public void Render(RunResultSnapshot result)
    {
        EnsureView();
        gameObject.SetActive(true);
        if (detailText != null)
        {
            detailText.text = result != null ? result.ToDetailText() : "런 결과 없음";
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void EnsureView()
    {
        if (detailText != null) return;

        detailText = GetComponentInChildren<TMP_Text>(true);
    }

    private static RunResultPanel CreateDefaultPanel()
    {
        GameObject canvasObject = new GameObject("RunResultCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);

        GameObject panelObject = new GameObject("RunResultPanel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);
        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.28f, 0.14f);
        rect.anchorMax = new Vector2(0.72f, 0.86f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = panelObject.GetComponent<Image>();
        image.color = new Color(0.04f, 0.04f, 0.05f, 0.94f);

        GameObject textObject = new GameObject("RunResultText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panelObject.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = new Vector2(28f, 28f);
        textRect.offsetMax = new Vector2(-28f, -28f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        TMPKoreanFont.Apply(text);
        text.fontSize = 24f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.enableWordWrapping = true;

        RunResultPanel panel = panelObject.AddComponent<RunResultPanel>();
        panel.detailText = text;
        return panel;
    }
}
