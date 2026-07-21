using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface IDungeonGameSaveService
{
    DungeonGameSaveData Capture();
    string ToJson(DungeonGameSaveData saveData, bool prettyPrint = false);
    DungeonGameSaveData FromJson(string json);
    bool TryRestore(DungeonGameSaveData saveData, out DungeonGameRestoreReport report);
}

public interface IDungeonGameSaveSlotService
{
    string Save(string slotId, bool prettyPrint = false);
    bool TryLoad(string slotId, out DungeonGameRestoreReport report);
    bool HasSave(string slotId);
    IReadOnlyList<DungeonSaveSlotInfo> GetSlots();
    bool Delete(string slotId);
}

public interface IDungeonSaveSlotCatalog
{
    bool HasSave(string slotId);
    IReadOnlyList<DungeonSaveSlotInfo> GetSlots();
    bool Delete(string slotId);
    string GetPath(string slotId);
}

[Serializable]
public sealed class DungeonGameSaveData
{
    public const int CurrentVersion = 5;

    public int version = CurrentVersion;
    public string savedAtUtc = string.Empty;
    public string sceneName = string.Empty;
    public ModularFacilityWorldSaveData world = new ModularFacilityWorldSaveData();
    public DungeonCharacterWorldSaveData characters = new DungeonCharacterWorldSaveData();
    public DungeonResearchSaveData research = new DungeonResearchSaveData();
    public DungeonFacilityShopSaveData facilityShop = new DungeonFacilityShopSaveData();
    public DungeonRunVariableSaveData runVariables = new DungeonRunVariableSaveData();
    public DungeonMetaProgressionSaveData metaProgression = new DungeonMetaProgressionSaveData();
    public DungeonRegularCustomerSaveData regularCustomers = new DungeonRegularCustomerSaveData();
    public DungeonStaffDiscontentSaveData staffDiscontent = new DungeonStaffDiscontentSaveData();
    public DungeonCodexSaveData codex = new DungeonCodexSaveData();
    public DungeonOperatingDaySettlementSaveData settlement = new DungeonOperatingDaySettlementSaveData();
    public DungeonEventAlertSaveData alerts = new DungeonEventAlertSaveData();
    public DungeonPhysicalItemSaveData physicalItems = new DungeonPhysicalItemSaveData();
    public ExpeditionEquipmentSaveData expeditionEquipment = new ExpeditionEquipmentSaveData();
    public DungeonOffenseSaveData offense = new DungeonOffenseSaveData();
    public DungeonInvasionSaveData invasion = new DungeonInvasionSaveData();
    public DungeonRunFlowSaveData runFlow = new DungeonRunFlowSaveData();
}

[Serializable]
public sealed class DungeonCharacterWorldSaveData
{
    public List<DungeonCharacterSaveData> actors = new List<DungeonCharacterSaveData>();
    public List<WorldCharacterProfile> populationProfiles = new List<WorldCharacterProfile>();
    public GlobalFacilityReputationSnapshot globalFacilityReputation =
        new GlobalFacilityReputationSnapshot();
}

[Serializable]
public sealed class DungeonCharacterSaveData
{
    public string persistentId = string.Empty;
    public int dataId = -1;
    public bool isOwner;
    public string displayName = string.Empty;
    public CharacterType characterType;
    public CharacterRole role;
    public int gridX;
    public int gridY;
    public CharacterLifecycleState lifecycleState = CharacterLifecycleState.Active;
    public float currentHealth;
    public float injurySeverity;
    public float baseMood = CharacterMoodRules.DefaultBaseMood;
    public List<DungeonCharacterConditionSaveData> conditions = new List<DungeonCharacterConditionSaveData>();
    public List<DungeonCharacterMoodFactorSaveData> moodFactors = new List<DungeonCharacterMoodFactorSaveData>();
    public List<DungeonCharacterWorkPrioritySaveData> workPriorities = new List<DungeonCharacterWorkPrioritySaveData>();
    public AbilityWork.DutyState dutyState = AbilityWork.DutyState.OnDuty;
    public int visitCount;
    public int lookAroundCount;
    public int holdingMoney;
    public List<string> recentLogEntries = new List<string>();
    public int level = 1;
    public int currentExperience;
    public List<string> learnedSkillIds = new List<string>();
    public List<string> equippedSkillIds = new List<string>();
    public CharacterGrowthState growth = new CharacterGrowthState();
    public CharacterNarrativeLedger narrative = new CharacterNarrativeLedger();
    public CharacterSocialMemorySnapshot socialMemory = new CharacterSocialMemorySnapshot();
    public CharacterExpeditionRecoveryState expeditionRecovery = new CharacterExpeditionRecoveryState();
    public CharacterCarryInventorySaveData carryInventory = new CharacterCarryInventorySaveData();
}

[Serializable]
public sealed class DungeonCharacterConditionSaveData
{
    public CharacterCondition condition;
    public float value;
}

[Serializable]
public sealed class DungeonCharacterMoodFactorSaveData
{
    public string id = string.Empty;
    public string label = string.Empty;
    public float value;
    public float remainingSeconds;
}

[Serializable]
public sealed class DungeonCharacterWorkPrioritySaveData
{
    public string workTypeId = string.Empty;
    public WorkPriorityLevel priority;
}

[Serializable]
public sealed class DungeonResearchSaveData
{
    public List<DungeonResearchTaskSaveData> tasks = new List<DungeonResearchTaskSaveData>();
    public List<int> completedBlueprintIds = new List<int>();
    public List<int> unlockedBuildingIds = new List<int>();
    public List<string> unlockedRecipeIds = new List<string>();
}

[Serializable]
public sealed class DungeonResearchTaskSaveData
{
    public int blueprintId = -1;
    public float progress;
}

[Serializable]
public sealed class DungeonFacilityShopSaveData
{
    public int currentOfferDay = 1;
    public List<int> basicPurchaseBuildingIds = new List<int>();
    public List<int> acquiredBlueprintIds = new List<int>();
    public List<int> unlockedBuildingIds = new List<int>();
}

[Serializable]
public sealed class DungeonRunVariableSaveData
{
    public int runSeed;
    public int currentDay = 1;
    public List<int> randomDrawMaxima = new List<int>();
    public bool hasStartVariables;
    public DungeonRunStartSaveData startVariables = new DungeonRunStartSaveData();
    public List<DungeonActiveRunVariableSaveData> activeOperationVariables =
        new List<DungeonActiveRunVariableSaveData>();
    public string invasionVariableId = string.Empty;
}

[Serializable]
public sealed class DungeonRunStartSaveData
{
    public int seed;
    public string ownerSpeciesTag = string.Empty;
    public string ownerDoctrineId = string.Empty;
    public InvasionThreatDifficulty difficulty;
    public DungeonDifficulty runDifficulty = DungeonDifficulty.Normal;
    public List<int> startingFacilityCandidateIds = new List<int>();
    public List<string> startingGuestSpeciesCandidates = new List<string>();
    public List<int> startingBlueprintCandidateIds = new List<int>();
    public int initialShopSeed;
    public string initialDungeonLayoutId = string.Empty;
    public float threatRiseMultiplier = 1f;
}

[Serializable]
public sealed class DungeonActiveRunVariableSaveData
{
    public string definitionId = string.Empty;
    public int startDay = 1;
    public int remainingDays = 1;
}

[Serializable]
public sealed class DungeonMetaProgressionSaveData
{
    public int lifetimeEarnedCurrency;
    public int spentCurrency;
    public int completedRunCount;
    public List<DungeonStringIntSaveEntry> upgradeLevels = new List<DungeonStringIntSaveEntry>();
    public List<string> preservedRecipeIds = new List<string>();
    public DungeonMetaRunProgressSaveData runProgress = new DungeonMetaRunProgressSaveData();
    public bool ended;
    public bool hasLatestResult;
    public DungeonRunResultSaveData latestResult = new DungeonRunResultSaveData();
}

[Serializable]
public sealed class DungeonStringIntSaveEntry
{
    public string key = string.Empty;
    public int value;
}

[Serializable]
public sealed class DungeonMetaRunProgressSaveData
{
    public float elapsedSeconds;
    public int currentDay = 1;
    public int settlementCount;
    public int defendedInvasionCount;
    public InvasionThreatStage maxThreatStage = InvasionThreatStage.Peaceful;
    public float finalInvasionThreat;
    public int offenseSuccessCount;
    public List<int> discoveredFacilityIds = new List<int>();
    public List<string> unlockedRecipeIds = new List<string>();
}

[Serializable]
public sealed class DungeonRunResultSaveData
{
    public string ownerName = string.Empty;
    public string endReason = string.Empty;
    public float survivalSeconds;
    public int survivedOperatingDays;
    public int settlementCount;
    public int defendedInvasionCount;
    public InvasionThreatStage maxThreatStage = InvasionThreatStage.Peaceful;
    public float finalInvasionThreat;
    public int firstDiscoveredFacilityCount;
    public int firstUnlockedRecipeCount;
    public int offenseSuccessCount;
    public float difficultyMultiplier = 1f;
    public DungeonDifficulty difficulty = DungeonDifficulty.Normal;
    public int legacyCurrency;
    public DungeonRunOutcome outcome = DungeonRunOutcome.Defeat;
}

[Serializable]
public sealed class DungeonRunFlowSaveData
{
    public DungeonRunPhase phase = DungeonRunPhase.Preparation;
    public DungeonRunOutcome outcome = DungeonRunOutcome.None;
    public int currentDay = 1;
    public bool bossArmed;
    public bool bossActive;
    public bool finalInvasionDefended;
    public int bossCycle;
}

[Serializable]
public sealed class DungeonRegularCustomerSaveData
{
    public List<DungeonRegularCustomerRecordSaveData> records =
        new List<DungeonRegularCustomerRecordSaveData>();
}

[Serializable]
public sealed class DungeonRegularCustomerRecordSaveData
{
    public string customerId = string.Empty;
    public string displayName = string.Empty;
    public string speciesTag = string.Empty;
    public int sourceDataId = -1;
    public int visitCount;
    public float averageSatisfaction;
    public bool isRegular;
    public bool isRecruitCandidate;
    public bool isRecruited;
    public RecruitCapability recruitCapabilities;
}

[Serializable]
public sealed class DungeonStaffDiscontentSaveData
{
    public List<DungeonStaffDiscontentRecordSaveData> records =
        new List<DungeonStaffDiscontentRecordSaveData>();
}

[Serializable]
public sealed class DungeonStaffDiscontentRecordSaveData
{
    public string staffId = string.Empty;
    public string displayName = string.Empty;
    public StaffDiscontentStage stage = StaffDiscontentStage.Stable;
    public StaffDiscontentOutcome outcome = StaffDiscontentOutcome.None;
    public float mood = 100f;
    public int lowMoodDays;
    public bool permanentLoss;
    public bool departed;
    public bool localRebellion;
    public bool ownerThreat;
    public bool isolated;
    public bool suppressed;
}

[Serializable]
public sealed class DungeonCodexSaveData
{
    public List<DungeonCodexEntrySaveData> entries = new List<DungeonCodexEntrySaveData>();
}

[Serializable]
public sealed class DungeonCodexEntrySaveData
{
    public CodexEntryCategory category;
    public string entryId = string.Empty;
    public string title = string.Empty;
    public List<DungeonCodexLineSaveData> lines = new List<DungeonCodexLineSaveData>();
}

[Serializable]
public sealed class DungeonCodexLineSaveData
{
    public string text = string.Empty;
    public CodexInfoSource source;
}

public sealed class DungeonGameRestoreReport
{
    private readonly List<string> warnings = new List<string>();
    private readonly List<string> errors = new List<string>();

    public IReadOnlyList<string> Warnings => warnings;
    public IReadOnlyList<string> Errors => errors;
    public bool Success => errors.Count == 0;
    public int RestoredBuildingCount { get; internal set; }
    public int RestoredCharacterCount { get; internal set; }
    public int RestoredExpeditionCount { get; internal set; }
    public int RestoredIntruderCount { get; internal set; }

    public void AddWarning(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            warnings.Add(message);
        }
    }

    public void AddError(string message)
    {
        errors.Add(string.IsNullOrWhiteSpace(message) ? "Unknown restore error." : message);
    }
}

public sealed class DungeonSaveSlotInfo
{
    public string SlotId { get; internal set; }
    public string Path { get; internal set; }
    public string SavedAtUtc { get; internal set; }
    public string SceneName { get; internal set; }
    public int Day { get; internal set; }
    public int Money { get; internal set; }
    public bool IsValid { get; internal set; }
}

public sealed class DungeonSaveSlotCatalog : IDungeonSaveSlotCatalog
{
    private readonly string saveDirectory;

    [VContainer.Inject]
    public DungeonSaveSlotCatalog()
        : this(Path.Combine(Application.persistentDataPath, "Saves"))
    {
    }

    internal DungeonSaveSlotCatalog(string saveDirectory)
    {
        this.saveDirectory = string.IsNullOrWhiteSpace(saveDirectory)
            ? throw new ArgumentException("Save directory is required.", nameof(saveDirectory))
            : saveDirectory;
    }

    public bool HasSave(string slotId)
    {
        return File.Exists(GetPath(slotId));
    }

    public IReadOnlyList<DungeonSaveSlotInfo> GetSlots()
    {
        if (!Directory.Exists(saveDirectory))
        {
            return Array.Empty<DungeonSaveSlotInfo>();
        }

        List<DungeonSaveSlotInfo> slots = new List<DungeonSaveSlotInfo>();
        foreach (string path in Directory.GetFiles(saveDirectory, "*.json").OrderBy(path => path, StringComparer.Ordinal))
        {
            DungeonSaveSlotInfo info = new DungeonSaveSlotInfo
            {
                SlotId = Path.GetFileNameWithoutExtension(path),
                Path = path
            };

            try
            {
                DungeonSaveSlotHeaderData data = JsonUtility.FromJson<DungeonSaveSlotHeaderData>(File.ReadAllText(path));
                info.SavedAtUtc = data?.savedAtUtc ?? string.Empty;
                info.SceneName = data?.sceneName ?? string.Empty;
                info.Day = data?.world?.gameData?.day ?? 1;
                info.Money = data?.world?.gameData?.holdingMoney ?? 0;
                info.IsValid = data != null && data.version == DungeonGameSaveData.CurrentVersion;
            }
            catch
            {
                info.IsValid = false;
            }

            slots.Add(info);
        }

        return slots;
    }

    public bool Delete(string slotId)
    {
        string path = GetPath(slotId);
        if (!File.Exists(path))
        {
            return false;
        }

        File.Delete(path);
        return true;
    }

    public string GetPath(string slotId)
    {
        return Path.Combine(saveDirectory, NormalizeSlotId(slotId) + ".json");
    }

    private static string NormalizeSlotId(string slotId)
    {
        string normalized = (slotId ?? string.Empty).Trim();
        if (normalized.Length == 0
            || normalized.Any(character => !char.IsLetterOrDigit(character) && character != '-' && character != '_'))
        {
            throw new ArgumentException("Save slot ids may only contain letters, numbers, '-' and '_'.", nameof(slotId));
        }

        return normalized;
    }

    [Serializable]
    private sealed class DungeonSaveSlotHeaderData
    {
        public int version;
        public string savedAtUtc;
        public string sceneName;
        public DungeonSaveSlotWorldHeaderData world;
    }

    [Serializable]
    private sealed class DungeonSaveSlotWorldHeaderData
    {
        public DungeonSaveSlotGameDataHeaderData gameData;
    }

    [Serializable]
    private sealed class DungeonSaveSlotGameDataHeaderData
    {
        public int day = 1;
        public int holdingMoney;
    }
}

public sealed class DungeonGameSaveService : IDungeonGameSaveService
{
    private readonly IModularFacilityWorldSaveService worldSaveService;
    private readonly ICharacterWorldSaveService characterWorldSaveService;
    private readonly IGridSystemProvider gridSystemProvider;
    private readonly IGameDataProvider gameDataProvider;
    private readonly IBlueprintResearchRuntimeProvider researchRuntimeProvider;
    private readonly IDailyFacilityShopRuntimeProvider shopRuntimeProvider;
    private readonly IRunVariableRuntimeProvider runVariableRuntimeProvider;
    private readonly IMetaProgressionRuntimeProvider metaRuntimeProvider;
    private readonly IRegularCustomerRuntimeProvider regularCustomerRuntimeProvider;
    private readonly IStaffDiscontentRuntimeProvider staffDiscontentRuntimeProvider;
    private readonly IExpeditionEquipmentRuntime expeditionEquipmentRuntime;
    private readonly ICodexRuntimeProvider codexRuntimeProvider;
    private readonly IOperatingDaySettlementSaveService settlementSaveService;
    private readonly IEventAlertSaveService alertSaveService;
    private readonly IOffenseSaveService offenseSaveService;
    private readonly IInvasionSaveService invasionSaveService;
    private readonly IWorldItemStackRuntime itemStackRuntime;
    private readonly IFacilityShopCatalog facilityCatalog;
    private readonly IRunCharacterCatalog characterCatalog;
    private readonly IDungeonRunFlowRuntime runFlowRuntime;

    public DungeonGameSaveService(
        IModularFacilityWorldSaveService worldSaveService,
        ICharacterWorldSaveService characterWorldSaveService,
        IGridSystemProvider gridSystemProvider,
        IGameDataProvider gameDataProvider,
        IBlueprintResearchRuntimeProvider researchRuntimeProvider,
        IDailyFacilityShopRuntimeProvider shopRuntimeProvider,
        IRunVariableRuntimeProvider runVariableRuntimeProvider,
        IMetaProgressionRuntimeProvider metaRuntimeProvider,
        IRegularCustomerRuntimeProvider regularCustomerRuntimeProvider,
        IStaffDiscontentRuntimeProvider staffDiscontentRuntimeProvider,
        IExpeditionEquipmentRuntime expeditionEquipmentRuntime,
        ICodexRuntimeProvider codexRuntimeProvider,
        IOperatingDaySettlementSaveService settlementSaveService,
        IEventAlertSaveService alertSaveService,
        IOffenseSaveService offenseSaveService,
        IInvasionSaveService invasionSaveService,
        IWorldItemStackRuntime itemStackRuntime,
        IFacilityShopCatalog facilityCatalog,
        IRunCharacterCatalog characterCatalog,
        IDungeonRunFlowRuntime runFlowRuntime)
    {
        this.worldSaveService = worldSaveService ?? throw new ArgumentNullException(nameof(worldSaveService));
        this.characterWorldSaveService = characterWorldSaveService ?? throw new ArgumentNullException(nameof(characterWorldSaveService));
        this.gridSystemProvider = gridSystemProvider ?? throw new ArgumentNullException(nameof(gridSystemProvider));
        this.gameDataProvider = gameDataProvider ?? throw new ArgumentNullException(nameof(gameDataProvider));
        this.researchRuntimeProvider = researchRuntimeProvider ?? throw new ArgumentNullException(nameof(researchRuntimeProvider));
        this.shopRuntimeProvider = shopRuntimeProvider ?? throw new ArgumentNullException(nameof(shopRuntimeProvider));
        this.runVariableRuntimeProvider = runVariableRuntimeProvider ?? throw new ArgumentNullException(nameof(runVariableRuntimeProvider));
        this.metaRuntimeProvider = metaRuntimeProvider ?? throw new ArgumentNullException(nameof(metaRuntimeProvider));
        this.regularCustomerRuntimeProvider = regularCustomerRuntimeProvider ?? throw new ArgumentNullException(nameof(regularCustomerRuntimeProvider));
        this.staffDiscontentRuntimeProvider = staffDiscontentRuntimeProvider ?? throw new ArgumentNullException(nameof(staffDiscontentRuntimeProvider));
        this.expeditionEquipmentRuntime = expeditionEquipmentRuntime ?? throw new ArgumentNullException(nameof(expeditionEquipmentRuntime));
        this.codexRuntimeProvider = codexRuntimeProvider ?? throw new ArgumentNullException(nameof(codexRuntimeProvider));
        this.settlementSaveService = settlementSaveService ?? throw new ArgumentNullException(nameof(settlementSaveService));
        this.alertSaveService = alertSaveService ?? throw new ArgumentNullException(nameof(alertSaveService));
        this.offenseSaveService = offenseSaveService ?? throw new ArgumentNullException(nameof(offenseSaveService));
        this.invasionSaveService = invasionSaveService ?? throw new ArgumentNullException(nameof(invasionSaveService));
        this.itemStackRuntime = itemStackRuntime ?? throw new ArgumentNullException(nameof(itemStackRuntime));
        this.facilityCatalog = facilityCatalog ?? throw new ArgumentNullException(nameof(facilityCatalog));
        this.characterCatalog = characterCatalog ?? throw new ArgumentNullException(nameof(characterCatalog));
        this.runFlowRuntime = runFlowRuntime ?? throw new ArgumentNullException(nameof(runFlowRuntime));
    }

    public DungeonGameSaveData Capture()
    {
        if (!gridSystemProvider.TryGetGrid(out Grid grid))
        {
            throw new InvalidOperationException("Cannot save before the dungeon grid is initialized.");
        }

        if (!gameDataProvider.TryGetGameData(out GameData gameData))
        {
            throw new InvalidOperationException("Cannot save without active GameData.");
        }

        DungeonGameSaveData save = new DungeonGameSaveData
        {
            version = DungeonGameSaveData.CurrentVersion,
            savedAtUtc = DateTime.UtcNow.ToString("O"),
            sceneName = SceneManager.GetActiveScene().name,
            world = worldSaveService.CreateSnapshot(grid, gameData),
            characters = characterWorldSaveService.Capture(grid)
        };

        CaptureResearch(save.research);
        CaptureFacilityShop(save.facilityShop);
        CaptureRunVariables(save.runVariables);
        CaptureMetaProgression(save.metaProgression);
        CaptureRegularCustomers(save.regularCustomers);
        CaptureStaffDiscontent(save.staffDiscontent);
        save.expeditionEquipment = expeditionEquipmentRuntime.Capture();
        CaptureCodex(save.codex);
        save.settlement = settlementSaveService.Capture();
        save.alerts = alertSaveService.Capture();
        save.physicalItems = itemStackRuntime.Capture();
        save.offense = offenseSaveService.Capture();
        save.invasion = invasionSaveService.Capture();
        save.runFlow = new DungeonRunFlowSaveData
        {
            phase = runFlowRuntime.Phase,
            outcome = runFlowRuntime.Outcome,
            currentDay = runFlowRuntime.CurrentDay,
            bossArmed = runFlowRuntime.IsBossArmed,
            bossActive = runFlowRuntime.IsBossActive,
            finalInvasionDefended = runFlowRuntime.IsFinalInvasionDefended,
            bossCycle = runFlowRuntime.BossCycle
        };
        return save;
    }

    public string ToJson(DungeonGameSaveData saveData, bool prettyPrint = false)
    {
        return JsonUtility.ToJson(saveData ?? new DungeonGameSaveData(), prettyPrint);
    }

    public DungeonGameSaveData FromJson(string json)
    {
        return string.IsNullOrWhiteSpace(json)
            ? new DungeonGameSaveData()
            : JsonUtility.FromJson<DungeonGameSaveData>(json) ?? new DungeonGameSaveData();
    }

    public bool TryRestore(DungeonGameSaveData saveData, out DungeonGameRestoreReport report)
    {
        report = new DungeonGameRestoreReport();
        if (saveData == null)
        {
            report.AddError("Save data is null.");
            return false;
        }

        if (saveData.version != DungeonGameSaveData.CurrentVersion)
        {
            report.AddError(
                $"저장 버전 {saveData.version}은 새 성장 시스템과 호환되지 않습니다. 새 게임을 시작해 주세요.");
            return false;
        }

        if (!gridSystemProvider.TryGetGrid(out Grid grid)
            || !gameDataProvider.TryGetGameData(out GameData gameData))
        {
            report.AddError("The active scene has no initialized grid or GameData.");
            return false;
        }

        try
        {
            RestoreMetaProgression(saveData.metaProgression, report);
            RestoreRunVariables(saveData.runVariables, report);
            characterWorldSaveService.PrepareForWorldRestore();

            if (!worldSaveService.TryRestoreSnapshot(
                grid,
                gameData,
                saveData.world ?? new ModularFacilityWorldSaveData(),
                out ModularFacilityWorldRestoreReport worldReport))
            {
                foreach (string error in worldReport.errors)
                {
                    report.AddError(error);
                }

                foreach (string warning in worldReport.warnings)
                {
                    report.AddWarning(warning);
                }

                return false;
            }

            report.RestoredBuildingCount = worldReport.restoredCount;
            foreach (string warning in worldReport.warnings)
            {
                report.AddWarning(warning);
            }

            report.RestoredCharacterCount = characterWorldSaveService.Restore(
                grid,
                saveData.characters ?? new DungeonCharacterWorldSaveData(),
                report);
            itemStackRuntime.Restore(saveData.physicalItems ?? new DungeonPhysicalItemSaveData());
            offenseSaveService.Restore(saveData.offense, report);
            invasionSaveService.Restore(saveData.invasion, report);
            RestoreRunFlow(saveData.runFlow);
            RestoreResearch(saveData.research, report);
            RestoreFacilityShop(saveData.facilityShop, report);
            RestoreRegularCustomers(saveData.regularCustomers, report);
            RestoreStaffDiscontent(saveData.staffDiscontent, report);
            expeditionEquipmentRuntime.Restore(saveData.expeditionEquipment);
            RestoreCodex(saveData.codex, report);
            settlementSaveService.Restore(saveData.settlement, report);
            alertSaveService.Restore(saveData.alerts, report);
        }
        catch (Exception exception)
        {
            report.AddError(exception.Message);
        }

        return report.Success;
    }

    private static void MigrateV1ToV2(
        DungeonGameSaveData saveData,
        DungeonGameRestoreReport report)
    {
        saveData.offense ??= new DungeonOffenseSaveData();
        saveData.runFlow ??= new DungeonRunFlowSaveData();
        saveData.runVariables ??= new DungeonRunVariableSaveData();
        saveData.runVariables.startVariables ??= new DungeonRunStartSaveData();
        saveData.runVariables.startVariables.runDifficulty = DungeonDifficultyRules.FromLegacy(
            saveData.runVariables.startVariables.difficulty);
        if (saveData.metaProgression?.latestResult != null)
        {
            saveData.metaProgression.latestResult.difficulty = DifficultyFromMultiplier(
                saveData.metaProgression.latestResult.difficultyMultiplier);
        }
        saveData.version = DungeonGameSaveData.CurrentVersion;
        report.AddWarning(
            "Save V1 was migrated to V2. An active timed expedition will resume as a first-turn battle.");
    }

    private void CaptureResearch(DungeonResearchSaveData destination)
    {
        if (!researchRuntimeProvider.TryGetRuntime(out BlueprintResearchRuntime runtime))
        {
            return;
        }

        destination.tasks = runtime.State.Tasks
            .Where(task => task?.Blueprint != null)
            .Select(task => new DungeonResearchTaskSaveData
            {
                blueprintId = task.Blueprint.id,
                progress = task.Progress
            })
            .ToList();
        destination.completedBlueprintIds = runtime.State.CompletedBlueprintIds.OrderBy(id => id).ToList();
        destination.unlockedBuildingIds = runtime.State.UnlockedBuildingIds.OrderBy(id => id).ToList();
        destination.unlockedRecipeIds = runtime.State.UnlockedRecipeIds.OrderBy(id => id, StringComparer.Ordinal).ToList();
    }

    private void CaptureFacilityShop(DungeonFacilityShopSaveData destination)
    {
        destination.unlockedBuildingIds = facilityCatalog.Buildings
            .Where(building => building != null && building.unlocked)
            .Select(building => building.id)
            .OrderBy(id => id)
            .ToList();

        if (!shopRuntimeProvider.TryGetRuntime(out DailyFacilityShopRuntime runtime))
        {
            return;
        }

        destination.currentOfferDay = runtime.CurrentOfferDay;
        destination.basicPurchaseBuildingIds = runtime.UnlockState.BasicPurchaseBuildingIds.OrderBy(id => id).ToList();
        destination.acquiredBlueprintIds = runtime.UnlockState.AcquiredBlueprintIds.OrderBy(id => id).ToList();
    }

    private void CaptureRunVariables(DungeonRunVariableSaveData destination)
    {
        if (!runVariableRuntimeProvider.TryGetRuntime(out RunVariableRuntime runtime))
        {
            return;
        }

        destination.runSeed = runtime.RunSeed;
        destination.currentDay = runtime.CurrentDay;
        destination.randomDrawMaxima = runtime.RandomDrawMaxima.ToList();
        RunStartVariableSnapshot start = runtime.State.StartVariables;
        destination.hasStartVariables = start != null;
        if (start != null)
        {
            destination.startVariables = new DungeonRunStartSaveData
            {
                seed = start.seed,
                ownerSpeciesTag = start.ownerSpeciesTag,
                ownerDoctrineId = start.ownerDoctrineId,
                difficulty = start.difficulty,
                runDifficulty = start.runDifficulty,
                startingFacilityCandidateIds = start.startingFacilityCandidateIds.ToList(),
                startingGuestSpeciesCandidates = start.startingGuestSpeciesCandidates.ToList(),
                startingBlueprintCandidateIds = start.startingBlueprintCandidateIds.ToList(),
                initialShopSeed = start.initialShopSeed,
                initialDungeonLayoutId = start.initialDungeonLayoutId,
                threatRiseMultiplier = start.threatRiseMultiplier
            };
        }

        destination.activeOperationVariables = runtime.State.ActiveOperationVariables
            .Where(active => active?.Definition != null)
            .Select(active => new DungeonActiveRunVariableSaveData
            {
                definitionId = active.Definition.id,
                startDay = active.StartDay,
                remainingDays = active.RemainingDays
            })
            .ToList();
        destination.invasionVariableId = runtime.State.CurrentInvasionVariable?.id ?? string.Empty;
    }

    private void CaptureMetaProgression(DungeonMetaProgressionSaveData destination)
    {
        if (!metaRuntimeProvider.TryGetRuntime(out MetaProgressionRuntime runtime))
        {
            return;
        }

        destination.lifetimeEarnedCurrency = runtime.State.LifetimeEarnedCurrency;
        destination.spentCurrency = runtime.State.SpentCurrency;
        destination.completedRunCount = runtime.State.CompletedRunCount;
        destination.upgradeLevels = runtime.State.UpgradeLevels
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => new DungeonStringIntSaveEntry { key = pair.Key, value = pair.Value })
            .ToList();
        destination.preservedRecipeIds = runtime.State.PreservedRecipeIds
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();
        destination.runProgress = new DungeonMetaRunProgressSaveData
        {
            elapsedSeconds = runtime.RunProgress.ElapsedSeconds,
            currentDay = runtime.RunProgress.CurrentDay,
            settlementCount = runtime.RunProgress.SettlementCount,
            defendedInvasionCount = runtime.RunProgress.DefendedInvasionCount,
            maxThreatStage = runtime.RunProgress.MaxThreatStage,
            finalInvasionThreat = runtime.RunProgress.FinalInvasionThreat,
            offenseSuccessCount = runtime.RunProgress.OffenseSuccessCount,
            discoveredFacilityIds = runtime.RunProgress.DiscoveredFacilityIds.OrderBy(id => id).ToList(),
            unlockedRecipeIds = runtime.RunProgress.UnlockedRecipeIds.OrderBy(id => id, StringComparer.Ordinal).ToList()
        };
        destination.ended = runtime.HasEnded;
        destination.hasLatestResult = runtime.LatestResult != null;
        if (runtime.LatestResult != null)
        {
            destination.latestResult = ToSaveData(runtime.LatestResult);
        }
    }

    private void CaptureRegularCustomers(DungeonRegularCustomerSaveData destination)
    {
        if (!regularCustomerRuntimeProvider.TryGetRuntime(out RegularCustomerRuntime runtime))
        {
            return;
        }

        destination.records = runtime.State.Records
            .OrderBy(record => record.CustomerId)
            .Select(record => new DungeonRegularCustomerRecordSaveData
            {
                customerId = record.CustomerId,
                displayName = record.DisplayName,
                speciesTag = record.SpeciesTag,
                sourceDataId = record.SourceData != null ? record.SourceData.id : -1,
                visitCount = record.VisitCount,
                averageSatisfaction = record.AverageSatisfaction,
                isRegular = record.IsRegular,
                isRecruitCandidate = record.IsRecruitCandidate,
                isRecruited = record.IsRecruited,
                recruitCapabilities = record.RecruitCapabilities
            })
            .ToList();
    }

    private void CaptureStaffDiscontent(DungeonStaffDiscontentSaveData destination)
    {
        if (!staffDiscontentRuntimeProvider.TryGetRuntime(out StaffDiscontentRuntime runtime))
        {
            return;
        }

        destination.records = runtime.CaptureSnapshots()
            .OrderBy(snapshot => snapshot.staffId, StringComparer.Ordinal)
            .Select(snapshot => new DungeonStaffDiscontentRecordSaveData
            {
                staffId = snapshot.staffId,
                displayName = snapshot.displayName,
                stage = snapshot.stage,
                outcome = snapshot.outcome,
                mood = snapshot.mood,
                lowMoodDays = snapshot.lowMoodDays,
                permanentLoss = snapshot.permanentLoss,
                departed = snapshot.departed,
                localRebellion = snapshot.localRebellion,
                ownerThreat = snapshot.ownerThreat,
                isolated = snapshot.isolated,
                suppressed = snapshot.suppressed
            })
            .ToList();
    }

    private void CaptureCodex(DungeonCodexSaveData destination)
    {
        if (!codexRuntimeProvider.TryGetRuntime(out CodexRuntime runtime))
        {
            return;
        }

        destination.entries = runtime.State.Entries
            .OrderBy(entry => entry.Category)
            .ThenBy(entry => entry.EntryId, StringComparer.Ordinal)
            .Select(entry => new DungeonCodexEntrySaveData
            {
                category = entry.Category,
                entryId = entry.EntryId,
                title = entry.Title,
                lines = entry.Lines.Select(line => new DungeonCodexLineSaveData
                {
                    text = line.Text,
                    source = line.Source
                }).ToList()
            })
            .ToList();
    }

    private void RestoreResearch(DungeonResearchSaveData source, DungeonGameRestoreReport report)
    {
        if (!researchRuntimeProvider.TryGetRuntime(out BlueprintResearchRuntime runtime))
        {
            report.AddWarning("Research runtime was not present; research state was skipped.");
            return;
        }

        source ??= new DungeonResearchSaveData();
        runtime.State.ClearForRestore();
        Dictionary<int, FacilityBlueprintSO> blueprints = facilityCatalog.Blueprints
            .Where(blueprint => blueprint != null)
            .GroupBy(blueprint => blueprint.id)
            .ToDictionary(group => group.Key, group => group.First());

        foreach (DungeonResearchTaskSaveData task in source.tasks ?? new List<DungeonResearchTaskSaveData>())
        {
            if (task == null || !blueprints.TryGetValue(task.blueprintId, out FacilityBlueprintSO blueprint))
            {
                report.AddWarning($"Research blueprint {task?.blueprintId ?? -1} no longer exists.");
                continue;
            }

            runtime.State.RestoreTask(blueprint, task.progress);
        }

        foreach (int id in source.completedBlueprintIds ?? new List<int>())
        {
            runtime.State.RestoreCompletedBlueprintId(id);
            if (blueprints.TryGetValue(id, out FacilityBlueprintSO completedBlueprint))
            {
                foreach (BlueprintBuildingUnlock unlock in completedBlueprint.Unlocks.OfType<BlueprintBuildingUnlock>())
                {
                    runtime.State.RestoreUnlockedBuildingId(unlock.buildingId);
                }

                foreach (BlueprintRecipeUnlock unlock in completedBlueprint.Unlocks.OfType<BlueprintRecipeUnlock>())
                {
                    runtime.State.UnlockRecipe(unlock.recipeId);
                }
            }
        }

        foreach (int id in source.unlockedBuildingIds ?? new List<int>())
        {
            runtime.State.RestoreUnlockedBuildingId(id);
        }

        foreach (string id in source.unlockedRecipeIds ?? new List<string>())
        {
            runtime.State.UnlockRecipe(id);
        }
    }

    private void RestoreFacilityShop(DungeonFacilityShopSaveData source, DungeonGameRestoreReport report)
    {
        source ??= new DungeonFacilityShopSaveData();
        HashSet<int> unlockedIds = new HashSet<int>(source.unlockedBuildingIds ?? new List<int>());
        foreach (BuildingSO building in facilityCatalog.Buildings.Where(building => building != null))
        {
            if (unlockedIds.Contains(building.id))
            {
                building.unlocked = true;
            }
        }

        if (!shopRuntimeProvider.TryGetRuntime(out DailyFacilityShopRuntime runtime))
        {
            report.AddWarning("Facility shop runtime was not present; shop state was skipped.");
            return;
        }

        runtime.RestoreState(
            source.currentOfferDay,
            source.basicPurchaseBuildingIds,
            source.acquiredBlueprintIds);
    }

    private static void RestoreRunVariables(DungeonRunVariableSaveData source, DungeonGameRestoreReport report,
        RunVariableRuntime runtime)
    {
        source ??= new DungeonRunVariableSaveData();
        RunStartVariableSnapshot start = null;
        if (source.hasStartVariables)
        {
            DungeonRunStartSaveData savedStart = source.startVariables ?? new DungeonRunStartSaveData();
            start = new RunStartVariableSnapshot(
                savedStart.seed,
                savedStart.ownerSpeciesTag,
                savedStart.runDifficulty,
                savedStart.startingFacilityCandidateIds ?? new List<int>(),
                savedStart.startingGuestSpeciesCandidates ?? new List<string>(),
                savedStart.startingBlueprintCandidateIds ?? new List<int>(),
                savedStart.initialShopSeed,
                savedStart.initialDungeonLayoutId,
                savedStart.threatRiseMultiplier,
                !string.IsNullOrWhiteSpace(savedStart.ownerDoctrineId)
                    ? savedStart.ownerDoctrineId
                    : OwnerDoctrineCatalog.ResolveForSpecies(savedStart.ownerSpeciesTag)?.id);
        }

        List<ActiveRunVariable> activeVariables = new List<ActiveRunVariable>();
        foreach (DungeonActiveRunVariableSaveData saved in source.activeOperationVariables
            ?? new List<DungeonActiveRunVariableSaveData>())
        {
            RunVariableDefinition definition = RunVariableCatalog.Get(saved?.definitionId);
            if (definition == null)
            {
                report.AddWarning($"Run variable '{saved?.definitionId}' no longer exists.");
                continue;
            }

            activeVariables.Add(new ActiveRunVariable(definition, saved.startDay, saved.remainingDays));
        }

        RunVariableDefinition invasion = RunVariableCatalog.Get(source.invasionVariableId);
        runtime.RestoreRun(
            source.runSeed,
            source.currentDay,
            start,
            activeVariables,
            invasion,
            source.randomDrawMaxima);
    }

    private void RestoreRunVariables(DungeonRunVariableSaveData source, DungeonGameRestoreReport report)
    {
        if (!runVariableRuntimeProvider.TryGetRuntime(out RunVariableRuntime runtime))
        {
            report.AddWarning("Run variable runtime was not present; run variables were skipped.");
            return;
        }

        RestoreRunVariables(source, report, runtime);
    }

    private void RestoreMetaProgression(DungeonMetaProgressionSaveData source, DungeonGameRestoreReport report)
    {
        if (!metaRuntimeProvider.TryGetRuntime(out MetaProgressionRuntime runtime))
        {
            report.AddWarning("Meta progression runtime was not present; meta state was skipped.");
            return;
        }

        source ??= new DungeonMetaProgressionSaveData();
        runtime.State.Merge(
            source.lifetimeEarnedCurrency,
            source.spentCurrency,
            (source.upgradeLevels ?? new List<DungeonStringIntSaveEntry>())
                .Where(entry => entry != null)
                .Select(entry => new KeyValuePair<string, int>(entry.key, entry.value)),
            source.preservedRecipeIds,
            source.completedRunCount);

        DungeonMetaRunProgressSaveData progress = source.runProgress ?? new DungeonMetaRunProgressSaveData();
        runtime.RunProgress.Restore(
            progress.elapsedSeconds,
            progress.currentDay,
            progress.settlementCount,
            progress.defendedInvasionCount,
            progress.maxThreatStage,
            progress.finalInvasionThreat,
            progress.offenseSuccessCount,
            progress.discoveredFacilityIds,
            progress.unlockedRecipeIds);
        runtime.RestoreRunState(
            source.ended,
            source.hasLatestResult ? ToRuntimeResult(source.latestResult) : null);
    }

    private void RestoreRegularCustomers(DungeonRegularCustomerSaveData source, DungeonGameRestoreReport report)
    {
        if (!regularCustomerRuntimeProvider.TryGetRuntime(out RegularCustomerRuntime runtime))
        {
            report.AddWarning("Regular customer runtime was not present; customer history was skipped.");
            return;
        }

        source ??= new DungeonRegularCustomerSaveData();
        Dictionary<int, CharacterSO> characters = characterCatalog.Characters
            .Where(character => character != null)
            .GroupBy(character => character.id)
            .ToDictionary(group => group.Key, group => group.First());
        List<RegularCustomerRecord> records = new List<RegularCustomerRecord>();
        foreach (DungeonRegularCustomerRecordSaveData saved in source.records
            ?? new List<DungeonRegularCustomerRecordSaveData>())
        {
            if (saved == null || string.IsNullOrWhiteSpace(saved.customerId))
            {
                continue;
            }

            characters.TryGetValue(saved.sourceDataId, out CharacterSO sourceData);
            records.Add(new RegularCustomerRecord(
                saved.customerId,
                saved.displayName,
                saved.speciesTag,
                sourceData,
                saved.visitCount,
                saved.averageSatisfaction,
                saved.isRegular,
                saved.isRecruitCandidate,
                saved.isRecruited,
                saved.recruitCapabilities));
        }

        runtime.State.Restore(records);
    }

    private void RestoreStaffDiscontent(DungeonStaffDiscontentSaveData source, DungeonGameRestoreReport report)
    {
        if (!staffDiscontentRuntimeProvider.TryGetRuntime(out StaffDiscontentRuntime runtime))
        {
            report.AddWarning("Staff discontent runtime was not present; staff discontent was skipped.");
            return;
        }

        source ??= new DungeonStaffDiscontentSaveData();
        HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
        List<StaffDiscontentSnapshot> records = new List<StaffDiscontentSnapshot>();
        foreach (DungeonStaffDiscontentRecordSaveData saved in source.records
            ?? new List<DungeonStaffDiscontentRecordSaveData>())
        {
            if (saved == null || string.IsNullOrWhiteSpace(saved.staffId))
            {
                continue;
            }

            string staffId = saved.staffId.Trim();
            if (!ids.Add(staffId))
            {
                report.AddError($"Duplicate staff discontent ID '{staffId}'.");
                return;
            }

            records.Add(new StaffDiscontentSnapshot(
                staffId,
                saved.displayName,
                saved.stage,
                saved.outcome,
                saved.mood,
                saved.lowMoodDays,
                saved.permanentLoss,
                saved.departed,
                saved.localRebellion,
                saved.ownerThreat,
                saved.isolated,
                saved.suppressed));
        }

        runtime.RestoreSnapshots(records);
    }

    private void RestoreRunFlow(DungeonRunFlowSaveData source)
    {
        source ??= new DungeonRunFlowSaveData();
        runFlowRuntime.RestoreState(
            source.phase,
            source.outcome,
            source.currentDay,
            source.bossArmed,
            source.bossActive,
            source.finalInvasionDefended,
            source.bossCycle);
    }

    private void RestoreCodex(DungeonCodexSaveData source, DungeonGameRestoreReport report)
    {
        if (!codexRuntimeProvider.TryGetRuntime(out CodexRuntime runtime))
        {
            report.AddWarning("Codex runtime was not present; codex state was skipped.");
            return;
        }

        source ??= new DungeonCodexSaveData();
        runtime.State.ClearForRestore();
        foreach (DungeonCodexEntrySaveData entry in source.entries ?? new List<DungeonCodexEntrySaveData>())
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.entryId))
            {
                continue;
            }

            runtime.State.GetOrCreate(entry.category, entry.entryId, entry.title);
            foreach (DungeonCodexLineSaveData line in entry.lines ?? new List<DungeonCodexLineSaveData>())
            {
                if (line != null)
                {
                    runtime.State.AddInfo(entry.category, entry.entryId, entry.title, line.text, line.source);
                }
            }
        }
    }

    private static DungeonRunResultSaveData ToSaveData(RunResultSnapshot result)
    {
        return new DungeonRunResultSaveData
        {
            ownerName = result.ownerName,
            endReason = result.endReason,
            survivalSeconds = result.survivalSeconds,
            survivedOperatingDays = result.survivedOperatingDays,
            settlementCount = result.settlementCount,
            defendedInvasionCount = result.defendedInvasionCount,
            maxThreatStage = result.maxThreatStage,
            finalInvasionThreat = result.finalInvasionThreat,
            firstDiscoveredFacilityCount = result.firstDiscoveredFacilityCount,
            firstUnlockedRecipeCount = result.firstUnlockedRecipeCount,
            offenseSuccessCount = result.offenseSuccessCount,
            difficultyMultiplier = result.difficultyMultiplier,
            difficulty = result.difficulty,
            legacyCurrency = result.legacyCurrency,
            outcome = result.outcome
        };
    }

    private static RunResultSnapshot ToRuntimeResult(DungeonRunResultSaveData result)
    {
        result ??= new DungeonRunResultSaveData();
        return new RunResultSnapshot(
            result.ownerName,
            result.endReason,
            result.survivalSeconds,
            result.survivedOperatingDays,
            result.settlementCount,
            result.defendedInvasionCount,
            result.maxThreatStage,
            result.finalInvasionThreat,
            result.firstDiscoveredFacilityCount,
            result.firstUnlockedRecipeCount,
            result.offenseSuccessCount,
            result.difficultyMultiplier,
            result.legacyCurrency,
            result.outcome,
            result.difficulty);
    }

    private static DungeonDifficulty DifficultyFromMultiplier(float multiplier)
    {
        if (multiplier <= 0.9f) return DungeonDifficulty.Easy;
        if (multiplier >= 1.1f) return DungeonDifficulty.Hard;
        return DungeonDifficulty.Normal;
    }
}

public sealed class DungeonGameSaveSlotService : IDungeonGameSaveSlotService
{
    public const string AutoSaveSlot = "autosave";
    public const string QuickSaveSlot = "quicksave";
    public const string ManualSaveSlot = "manual";

    private readonly IDungeonGameSaveService saveService;
    private readonly IDungeonSaveSlotCatalog slotCatalog;

    [VContainer.Inject]
    public DungeonGameSaveSlotService(
        IDungeonGameSaveService saveService,
        IDungeonSaveSlotCatalog slotCatalog)
    {
        this.saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
        this.slotCatalog = slotCatalog ?? throw new ArgumentNullException(nameof(slotCatalog));
    }

    internal DungeonGameSaveSlotService(IDungeonGameSaveService saveService, string saveDirectory)
        : this(saveService, new DungeonSaveSlotCatalog(saveDirectory))
    {
    }

    public string Save(string slotId, bool prettyPrint = false)
    {
        string path = slotCatalog.GetPath(slotId);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Application.persistentDataPath);
        string temporaryPath = path + ".tmp";
        string backupPath = path + ".bak";
        File.WriteAllText(temporaryPath, saveService.ToJson(saveService.Capture(), prettyPrint));

        if (File.Exists(path))
        {
            File.Replace(temporaryPath, path, backupPath, true);
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
        }
        else
        {
            File.Move(temporaryPath, path);
        }

        return path;
    }

    public bool TryLoad(string slotId, out DungeonGameRestoreReport report)
    {
        string path = slotCatalog.GetPath(slotId);
        if (!File.Exists(path))
        {
            report = new DungeonGameRestoreReport();
            report.AddError($"Save slot '{slotId}' does not exist.");
            return false;
        }

        try
        {
            return saveService.TryRestore(saveService.FromJson(File.ReadAllText(path)), out report);
        }
        catch (Exception exception)
        {
            report = new DungeonGameRestoreReport();
            string preservedPath = PreserveUnreadableSave(path);
            report.AddError(string.IsNullOrWhiteSpace(preservedPath)
                ? exception.Message
                : $"{exception.Message} 손상된 저장 사본: {preservedPath}");
            return false;
        }
    }

    public bool HasSave(string slotId)
    {
        return slotCatalog.HasSave(slotId);
    }

    public IReadOnlyList<DungeonSaveSlotInfo> GetSlots()
    {
        return slotCatalog.GetSlots();
    }

    public bool Delete(string slotId)
    {
        return slotCatalog.Delete(slotId);
    }

    private string PreserveUnreadableSave(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return string.Empty;
            }

            string corruptDirectory = Path.Combine(
                Path.GetDirectoryName(path) ?? Application.persistentDataPath,
                "Corrupt");
            Directory.CreateDirectory(corruptDirectory);
            string name = Path.GetFileNameWithoutExtension(path);
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
            string preservedPath = Path.Combine(corruptDirectory, $"{name}-{timestamp}.json");
            File.Copy(path, preservedPath, overwrite: false);
            return preservedPath;
        }
        catch
        {
            return string.Empty;
        }
    }

}
