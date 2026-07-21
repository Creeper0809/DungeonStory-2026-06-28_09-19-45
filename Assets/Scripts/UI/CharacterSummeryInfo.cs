using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class CharacterSummeryInfo : UIPopUp,
    UtilEventListener<InfoFeedEvent>,
    UtilEventListener<CharacterGrowthTabRequestedEvent>
{
    public GameObject UI;
    public TMP_Text ObjectName;
    public TMP_Text logText;
    public Slider mood;
    public Slider fun;
    public Slider hunger;
    public Slider sleep;
    public Slider excretion;
    public Slider hygiene;

    private CharacterActor actor;
    private CharacterStats characterStats;
    private CharacterLog characterLog;
    private CharacterProgression progression;
    private TMP_Text profileText;
    private Slider health;
    private Slider experience;
    private TMP_Text progressionSummaryText;
    private Button[] skillButtons = Array.Empty<Button>();
    private TMP_Text moodSummaryText;
    private TMP_Text moodFactorsText;
    private TMP_Text carrySummaryText;
    private GameObject statusTabContent;
    private GameObject growthTabContent;
    private GameObject moodTabContent;
    private GameObject recordsTabContent;
    private Button statusTabButton;
    private Button growthTabButton;
    private Button moodTabButton;
    private Button recordsTabButton;
    private float nextVitalsRefreshAt;
    private int pendingCandidateConfirmation = -1;
    private int pendingCandidateUnlockLevel = -1;
    private IUiPopupService popupService;
    private ICharacterSummaryRuntimeLogFactory runtimeLogFactory;
    private IExpeditionEquipmentRuntime equipmentRuntime;

    [Inject]
    public void Construct(
        IUiPopupService popupService,
        ICharacterSummaryRuntimeLogFactory runtimeLogFactory)
    {
        this.popupService = popupService
            ?? throw new ArgumentNullException(nameof(popupService));
        this.runtimeLogFactory = runtimeLogFactory
            ?? throw new ArgumentNullException(nameof(runtimeLogFactory));
    }

    private void Start()
    {
        GameObject uiRoot = RequireUiRoot();
        ICharacterSummaryRuntimeLogFactory viewFactory = ResolveRuntimeLogFactory();
        viewFactory.Ensure(this, uiRoot);
        viewFactory.ApplyFonts(uiRoot.transform);
        uiRoot.SetActive(false);
    }

    private void Update()
    {
        if (actor == null || UI == null || !UI.activeInHierarchy || Time.unscaledTime < nextVitalsRefreshAt)
        {
            return;
        }

        nextVitalsRefreshAt = Time.unscaledTime + 0.25f;
        RefreshProfileAndVitals();
        RefreshCarrySummary();
        RefreshProgression();
        RefreshMoodDetails();
    }

    public void OnTriggerEvent(InfoFeedEvent eventType)
    {
        if (eventType.Target is not CharacterActor nextActor || nextActor == null)
        {
            return;
        }

        ResolvePopupService().CloseAll();
        UnbindCharacter();

        actor = nextActor;
        GameObject targetObject = actor.gameObject;
        characterStats = targetObject.GetComponent<CharacterStats>();
        characterLog = targetObject.GetComponent<CharacterLog>();
        progression = actor.Progression;

        GameObject uiRoot = RequireUiRoot();
        ICharacterSummaryRuntimeLogFactory viewFactory = ResolveRuntimeLogFactory();
        viewFactory.Ensure(this, uiRoot);
        viewFactory.ApplyFonts(uiRoot.transform);
        uiRoot.SetActive(true);
        ResolvePopupService().Open(this);
        ObjectName.text = GetDisplayName(targetObject);
        RefreshProfileAndVitals();
        RefreshCarrySummary();
        RefreshProgression();

        if (characterStats != null)
        {
            OnStatChange(characterStats.StatSnapshot);
            characterStats.OnStatChange += OnStatChange;
        }

        if (characterLog != null)
        {
            characterLog.OnLogAdded += OnLogAdded;
            characterLog.OnLogDisplayChanged += RefreshLogText;
        }

        if (progression != null)
        {
            progression.Changed += RefreshProgression;
        }

        RefreshLogText();
    }

    public override void OnClose()
    {
        RequireUiRoot().SetActive(false);
        UnbindCharacter();
    }

    public void OnStatChange(IReadOnlyDictionary<CharacterCondition, float> stats)
    {
        SetSlider(fun, stats, CharacterCondition.FUN);
        SetSlider(hunger, stats, CharacterCondition.HUNGER);
        SetSlider(sleep, stats, CharacterCondition.SLEEP);
        SetSlider(excretion, stats, CharacterCondition.EXCRETION);
        SetSlider(hygiene, stats, CharacterCondition.HYGIENE);
        RefreshMoodDetails();
    }

    private static void SetSlider(
        Slider slider,
        IReadOnlyDictionary<CharacterCondition, float> stats,
        CharacterCondition condition)
    {
        if (slider == null || stats == null)
        {
            return;
        }

        float rawValue = stats.TryGetValue(condition, out float value) ? value : 0f;
        SetMeter(slider, rawValue / 100f, $"{Mathf.RoundToInt(rawValue)}");
    }

    public void BindGeneratedView(
        TMP_Text nameText,
        TMP_Text generatedProfileText,
        Slider healthSlider,
        Slider moodSlider,
        Slider funSlider,
        Slider hungerSlider,
        Slider sleepSlider,
        Slider excretionSlider,
        Slider hygieneSlider,
        TMP_Text generatedMoodSummaryText,
        TMP_Text generatedMoodFactorsText,
        TMP_Text generatedCarrySummaryText,
        TMP_Text generatedLogText)
    {
        ObjectName = nameText;
        profileText = generatedProfileText;
        health = healthSlider;
        mood = moodSlider;
        fun = funSlider;
        hunger = hungerSlider;
        sleep = sleepSlider;
        excretion = excretionSlider;
        hygiene = hygieneSlider;
        moodSummaryText = generatedMoodSummaryText;
        moodFactorsText = generatedMoodFactorsText;
        carrySummaryText = generatedCarrySummaryText;
        logText = generatedLogText;
    }

    public void BindGeneratedTabs(
        GameObject generatedStatusTabContent,
        GameObject generatedGrowthTabContent,
        GameObject generatedMoodTabContent,
        GameObject generatedRecordsTabContent,
        Button generatedStatusTabButton,
        Button generatedGrowthTabButton,
        Button generatedMoodTabButton,
        Button generatedRecordsTabButton)
    {
        statusTabContent = generatedStatusTabContent;
        growthTabContent = generatedGrowthTabContent;
        moodTabContent = generatedMoodTabContent;
        recordsTabContent = generatedRecordsTabContent;
        statusTabButton = generatedStatusTabButton;
        growthTabButton = generatedGrowthTabButton;
        moodTabButton = generatedMoodTabButton;
        recordsTabButton = generatedRecordsTabButton;
        ShowStatusTab();
    }

    public void BindGeneratedGrowth(
        Slider generatedExperience,
        TMP_Text generatedSummary,
        Button[] generatedSkillButtons)
    {
        experience = generatedExperience;
        progressionSummaryText = generatedSummary;
        skillButtons = generatedSkillButtons ?? Array.Empty<Button>();
        RefreshProgression();
    }

    public void ShowStatusTab()
    {
        SetActiveTab(CharacterSummaryTab.Status);
    }

    public void ShowMoodTab()
    {
        SetActiveTab(CharacterSummaryTab.Mood);
        RefreshMoodDetails();
    }

    public void ShowGrowthTab()
    {
        SetActiveTab(CharacterSummaryTab.Growth);
        RefreshProgression();
    }

    public void ToggleSkillAt(int index)
    {
        if (actor == null || progression == null)
        {
            return;
        }

        if (index < 7 || index >= 10)
        {
            return;
        }

        CharacterSkillDraft draft = GetCurrentActiveDraft();
        int candidateIndex = index - 7;
        if (draft == null || candidateIndex >= draft.candidates.Count)
        {
            return;
        }

        bool confirmed = pendingCandidateUnlockLevel == draft.unlockLevel
            && pendingCandidateConfirmation == candidateIndex;
        if (!confirmed)
        {
            pendingCandidateUnlockLevel = draft.unlockLevel;
            pendingCandidateConfirmation = candidateIndex;
            NoticeFeedEvent.Trigger(
                $"{draft.candidates[candidateIndex].displayName}: 다시 누르면 영구 확정",
                NoticeFeedEvent.Grade.WARNING);
        }
        else
        {
            progression.TryChooseActiveSkill(
                draft.unlockLevel,
                candidateIndex,
                confirmed: true,
                out string message);
            NoticeFeedEvent.Trigger(message, NoticeFeedEvent.Grade.NONE);
            pendingCandidateUnlockLevel = -1;
            pendingCandidateConfirmation = -1;
        }

        RefreshProgression();
    }

    public void ShowRecordsTab()
    {
        SetActiveTab(CharacterSummaryTab.Records);
        RefreshLogText();
    }

    private void SetActiveTab(CharacterSummaryTab tab)
    {
        if (statusTabContent != null)
        {
            statusTabContent.SetActive(tab == CharacterSummaryTab.Status);
        }

        if (growthTabContent != null)
        {
            growthTabContent.SetActive(tab == CharacterSummaryTab.Growth);
        }

        if (moodTabContent != null)
        {
            moodTabContent.SetActive(tab == CharacterSummaryTab.Mood);
        }

        if (recordsTabContent != null)
        {
            recordsTabContent.SetActive(tab == CharacterSummaryTab.Records);
        }

        DungeonUiTheme.StyleButton(statusTabButton, selected: tab == CharacterSummaryTab.Status);
        DungeonUiTheme.StyleButton(growthTabButton, selected: tab == CharacterSummaryTab.Growth);
        DungeonUiTheme.StyleButton(moodTabButton, selected: tab == CharacterSummaryTab.Mood);
        DungeonUiTheme.StyleButton(recordsTabButton, selected: tab == CharacterSummaryTab.Records);
    }

    public void RefreshProgression()
    {
        if (progression == null || actor == null)
        {
            SetMeter(experience, 0f, "--");
            if (progressionSummaryText != null)
            {
                progressionSummaryText.text = "성장 정보가 없습니다.";
            }
            return;
        }

        string experienceText = progression.Level >= CharacterProgression.MaxLevel
            ? "MAX"
            : $"{progression.CurrentExperience}/{progression.ExperienceToNextLevel}";
        SetMeter(experience, progression.ExperienceRatio, experienceText);
        if (progressionSummaryText != null)
        {
            string traits = string.Join(", ", progression.ResolveSelectedTraits()
                .Where(trait => trait != null)
                .Select(trait => trait.traitName));
            progressionSummaryText.text = BuildProgressionSummary(traits);
        }

        for (int i = 0; i < skillButtons.Length; i++)
        {
            Button button = skillButtons[i];
            if (button == null)
            {
                continue;
            }

            TMP_Text label = button.transform.Find("Label")?.GetComponent<TMP_Text>();
            button.gameObject.SetActive(true);
            button.interactable = false;
            bool selected = false;
            string text;
            if (i == 0)
            {
                CharacterCombatAbilityDefinition species =
                    CharacterCombatAbilityCatalog.GetSpeciesAbilities(actor).FirstOrDefault();
                text = species != null
                    ? $"종족기  {species.DisplayName}  ·  {species.Description}"
                    : "종족기  없음";
            }
            else if (i <= 3)
            {
                int slot = i - 1;
                CharacterSkillInstance skill = slot < progression.ActiveSkills.Count
                    ? progression.ActiveSkills[slot]
                    : null;
                int unlockLevel = new[] { 1, 5, 30 }[slot];
                text = skill != null
                    ? $"액티브 {slot + 1}  [{CharacterSkillDisplay.Rarity(skill.rarity)}] {skill.displayName}  ·  {skill.description}"
                    : $"액티브 {slot + 1}  Lv.{unlockLevel} 선택 대기";
            }
            else if (i <= 5)
            {
                int slot = i - 4;
                CharacterSkillInstance skill = slot < progression.PassiveSkills.Count
                    ? progression.PassiveSkills[slot]
                    : null;
                text = skill != null
                    ? $"패시브 {slot + 1}  [{CharacterSkillDisplay.Rarity(skill.rarity)}] {skill.displayName}  ·  {skill.description}"
                    : $"패시브 {slot + 1}  {(slot == 0 ? "정체성 기술 대기" : "Lv.25 서사 조건")}";
            }
            else if (i == 6)
            {
                CharacterSkillInstance skill = progression.Ultimate;
                text = skill != null
                    ? $"궁극기  [{skill.ultimateDomain}] {skill.displayName}  ·  {skill.description}"
                    : "궁극기  Lv.50 서사 완성 시 획득";
            }
            else
            {
                CharacterSkillDraft draft = GetCurrentActiveDraft();
                int candidateIndex = i - 7;
                CharacterSkillInstance candidate = draft != null && candidateIndex < draft.candidates.Count
                    ? draft.candidates[candidateIndex]
                    : null;
                bool visible = candidate != null;
                button.gameObject.SetActive(visible);
                if (!visible)
                {
                    continue;
                }

                selected = pendingCandidateUnlockLevel == draft.unlockLevel
                    && pendingCandidateConfirmation == candidateIndex;
                text = $"후보 {candidateIndex + 1}  [{CharacterSkillDisplay.Rarity(candidate.rarity)}] {candidate.displayName}  ·  {candidate.description}";
                button.interactable = true;
            }

            if (label != null)
            {
                label.text = text;
            }
            DungeonUiTheme.StyleButton(button, selected);
        }
    }

    private CharacterSkillDraft GetCurrentActiveDraft()
    {
        return progression?.Drafts
            .Where(draft => draft != null
                && draft.kind == CharacterSkillKind.Active
                && draft.isReady
                && !draft.permanentlyChosen)
            .OrderBy(draft => draft.unlockLevel)
            .FirstOrDefault();
    }

    private string BuildProgressionSummary(string traits)
    {
        StringBuilder builder = new StringBuilder(768);
        builder.AppendLine(
            $"Lv.{progression.Level}  ·  잠재력 {CharacterSkillDisplay.Potential(progression.PotentialGrade)}  ·  성장 +{progression.GrowthState.allocatedGrowthPoints}");
        builder.AppendLine($"특성  {(string.IsNullOrWhiteSpace(traits) ? "없음" : traits)}");
        builder.AppendLine("능력치  기본 | 종족·특성 | 레벨 | 장비 | 조건부 | 최종");

        ExpeditionEquipmentStatBlock equipment = GetCurrentEquipmentBonuses();
        foreach (CharacterStatDefinition definition in CharacterStatCatalog.All
                     .Where(item => item.LegacyType.HasValue))
        {
            CharacterStatType statType = definition.LegacyType.Value;
            CharacterStatBreakdown breakdown = progression.GetStatBreakdown(statType);
            int equipmentBonus = GetEquipmentBonus(equipment, statType);
            int finalValue = Mathf.Max(0, breakdown.FinalValue + equipmentBonus);
            builder.AppendLine(
                $"{definition.DisplayName}  {breakdown.BaseValue} | {FormatSigned(breakdown.SpeciesTraitValue)} | {FormatSigned(breakdown.LevelGrowthValue)} | {FormatSigned(equipmentBonus)} | {FormatSigned(breakdown.ConditionalPassiveValue)} | {finalValue}");
        }

        if (equipment != null && equipment.maxHealth != 0)
        {
            builder.AppendLine($"장비 체력 {FormatSigned(equipment.maxHealth)} · 장비 보정은 출정 전투에만 적용");
        }
        else
        {
            builder.AppendLine("장비 보정은 출정 전투에만 적용");
        }

        IReadOnlyList<CharacterGrowthAllocationRecord> records =
            progression.GrowthState.allocationRecords;
        if (records == null || records.Count == 0)
        {
            builder.Append("최근 성장  아직 레벨 성장 기록 없음");
        }
        else
        {
            builder.Append("최근 성장  ");
            builder.Append(string.Join(" / ", records
                .Where(record => record != null)
                .OrderByDescending(record => record.level)
                .Take(4)
                .Select(FormatGrowthAllocationRecord)));
        }

        return builder.ToString();
    }

    private string FormatGrowthAllocationRecord(CharacterGrowthAllocationRecord record)
    {
        string statName = CharacterStatCatalog.TryGet(record.statType, out CharacterStatDefinition definition)
            ? definition.DisplayName
            : record.statType.ToString();
        string reason = string.IsNullOrWhiteSpace(record.reason)
            ? "성장 기록"
            : record.reason;
        return $"Lv.{record.level} {statName}+1({reason})";
    }

    private ExpeditionEquipmentStatBlock GetCurrentEquipmentBonuses()
    {
        string characterId = actor?.Identity?.PersistentId;
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return ExpeditionEquipmentStatBlock.Empty;
        }

        IExpeditionEquipmentRuntime runtime = ResolveEquipmentRuntime();
        return runtime?.GetCombatBonuses(characterId) ?? ExpeditionEquipmentStatBlock.Empty;
    }

    private IExpeditionEquipmentRuntime ResolveEquipmentRuntime()
    {
        if (equipmentRuntime != null)
        {
            return equipmentRuntime;
        }

        DungeonRuntimeLifetimeScope scope =
            FindFirstObjectByType<DungeonRuntimeLifetimeScope>(FindObjectsInactive.Include);
        if (scope == null || scope.Container == null)
        {
            return null;
        }

        try
        {
            equipmentRuntime = scope.Container.Resolve<IExpeditionEquipmentRuntime>();
        }
        catch
        {
            equipmentRuntime = null;
        }

        return equipmentRuntime;
    }

    private static int GetEquipmentBonus(
        ExpeditionEquipmentStatBlock equipment,
        CharacterStatType statType)
    {
        if (equipment == null)
        {
            return 0;
        }

        return statType switch
        {
            CharacterStatType.Attack => equipment.attack,
            CharacterStatType.Strength => equipment.strength,
            CharacterStatType.Toughness => equipment.toughness,
            CharacterStatType.Dexterity => equipment.dexterity,
            CharacterStatType.MoveSpeed => equipment.moveSpeed,
            _ => 0
        };
    }

    private static string FormatSigned(int value)
    {
        return value > 0 ? "+" + value : value.ToString();
    }

    public void OnTriggerEvent(CharacterGrowthTabRequestedEvent eventType)
    {
        if (eventType.Actor == null)
        {
            return;
        }

        if (actor != eventType.Actor)
        {
            OnTriggerEvent(new InfoFeedEvent(eventType.Actor));
        }

        ShowGrowthTab();
    }

    public void OnLogAdded(CharacterLogEntry entry)
    {
        RefreshLogText();
    }

    public void RefreshLogText()
    {
        if (logText == null)
        {
            return;
        }

        logText.text = FormatLogText(characterLog, 40);
    }

    public void RefreshMoodDetails()
    {
        if (characterStats == null)
        {
            SetMeter(mood, 0f, "--");
            if (moodSummaryText != null) moodSummaryText.text = "기분 정보가 없습니다.";
            if (moodFactorsText != null) moodFactorsText.text = "적용 중인 요인이 없습니다.";
            return;
        }

        CharacterMoodSnapshot snapshot = characterStats.GetMoodSnapshot();
        SetMeter(mood, snapshot.Value / 100f, $"{Mathf.RoundToInt(snapshot.Value)}");
        if (moodSummaryText != null)
        {
            string offset = snapshot.TotalOffset >= 0f
                ? $"+{Mathf.RoundToInt(snapshot.TotalOffset)}"
                : $"{Mathf.RoundToInt(snapshot.TotalOffset)}";
            moodSummaryText.text = $"{CharacterMoodRules.GetMoodLabel(snapshot.Value)} · 기준 {Mathf.RoundToInt(snapshot.BaseValue)} · 보정 {offset}";
        }

        if (moodFactorsText != null)
        {
            moodFactorsText.text = FormatMoodFactors(snapshot);
        }
    }

    public static string FormatMoodFactors(CharacterMoodSnapshot snapshot)
    {
        if (snapshot == null || snapshot.Factors == null || snapshot.Factors.Count == 0)
        {
            return "현재 기분을 바꾸는 요인이 없습니다.";
        }

        StringBuilder builder = new StringBuilder();
        AppendMoodFactorGroup(builder, snapshot.Factors, CharacterMoodFactorKind.Need, "욕구 영향");
        AppendMoodFactorGroup(builder, snapshot.Factors, CharacterMoodFactorKind.Interaction, "최근 경험");
        return builder.ToString().TrimEnd();
    }

    private static void AppendMoodFactorGroup(
        StringBuilder builder,
        IReadOnlyList<CharacterMoodFactorSnapshot> factors,
        CharacterMoodFactorKind kind,
        string heading)
    {
        bool hasGroup = false;
        for (int i = 0; i < factors.Count; i++)
        {
            if (factors[i] != null && factors[i].Kind == kind)
            {
                hasGroup = true;
                break;
            }
        }

        if (!hasGroup)
        {
            return;
        }

        if (builder.Length > 0)
        {
            builder.AppendLine();
        }

        builder.AppendLine(heading);
        for (int i = 0; i < factors.Count; i++)
        {
            CharacterMoodFactorSnapshot factor = factors[i];
            if (factor == null || factor.Kind != kind)
            {
                continue;
            }

            string signedValue = factor.Value >= 0f
                ? $"+{Mathf.RoundToInt(factor.Value)}"
                : $"{Mathf.RoundToInt(factor.Value)}";
            if (kind == CharacterMoodFactorKind.Interaction)
            {
                builder.AppendLine($"• {factor.Label}  {signedValue}  · {FormatRemainingTime(factor.RemainingSeconds)}");
            }
            else
            {
                builder.AppendLine($"• {factor.Label}  {signedValue}");
            }
        }
    }

    private static string FormatRemainingTime(float remainingSeconds)
    {
        int seconds = Mathf.Max(1, Mathf.CeilToInt(remainingSeconds));
        if (seconds < 60)
        {
            return $"{seconds}초";
        }

        int minutes = seconds / 60;
        int remainder = seconds % 60;
        return remainder > 0 ? $"{minutes}분 {remainder}초" : $"{minutes}분";
    }

    private void RefreshProfileAndVitals()
    {
        if (actor == null) return;

        if (profileText != null)
        {
            string species = !string.IsNullOrWhiteSpace(actor.SpeciesTag) ? actor.SpeciesTag : "종족 미상";
            int actorLevel = actor.Progression != null ? actor.Progression.Level : 1;
            profileText.text = $"Lv.{actorLevel} · {species} · {FormatRole(actor.Role)} · {FormatLifecycle(actor.CurrentLifecycleState)}";
        }

        if (characterStats == null)
        {
            SetMeter(health, 0f, "--");
            RefreshCarrySummary();
            return;
        }

        float maximum = Mathf.Max(1f, characterStats.MaxHealth);
        float current = Mathf.Clamp(characterStats.CurrentHealth, 0f, maximum);
        int injuryPercent = Mathf.RoundToInt(characterStats.InjurySeverity * 100f);
        SetMeter(
            health,
            current / maximum,
            injuryPercent > 0
                ? $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(maximum)} · 부상 {injuryPercent}%"
                : $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(maximum)}");
    }

    private void RefreshCarrySummary()
    {
        if (carrySummaryText == null)
        {
            return;
        }

        CharacterCarryInventory inventory = actor != null
            ? actor.GetComponent<CharacterCarryInventory>()
            : null;
        if (inventory == null)
        {
            carrySummaryText.text = "소지품 없음\n운반 한도 정보 없음";
            carrySummaryText.color = DungeonUiTheme.TextSecondary;
            return;
        }

        IDungeonItemCatalogProvider catalogProvider = WorldItemStackRuntime.Active?.CatalogProvider;
        float currentWeight = inventory.GetCurrentWeight(catalogProvider);
        float baseLimit = inventory.GetBaseCarryLimit();
        float maxAllowed = inventory.GetMaxAllowedWeight();
        float speedMultiplier = inventory.GetMoveSpeedMultiplier(catalogProvider);
        bool overloaded = currentWeight > baseLimit + 0.01f;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"소지 무게 {FormatWeight(currentWeight)} / 기본 {FormatWeight(baseLimit)} / 최대 {FormatWeight(maxAllowed)}");
        builder.AppendLine(overloaded
            ? $"과적 중 · 이동 속도 {Mathf.RoundToInt(speedMultiplier * 100f)}%"
            : "과적 없음 · 이동 속도 100%");

        IReadOnlyList<CharacterCarriedItemSaveData> items = inventory.Items;
        List<string> entries = items == null
            ? new List<string>()
            : items
                .Where(item => item != null && item.quantity > 0)
                .GroupBy(item => item.itemId ?? string.Empty)
                .Select(group =>
                {
                    DungeonItemDefinition definition = catalogProvider != null
                        ? catalogProvider.GetDefinition(group.Key)
                        : new ResourceDungeonItemCatalogProvider().GetDefinition(group.Key);
                    return $"{definition.DisplayName} x{group.Sum(item => item.quantity)}";
                })
                .Take(4)
                .ToList();
        builder.Append(entries.Count > 0 ? string.Join(" · ", entries) : "소지 아이템 없음");

        carrySummaryText.text = builder.ToString();
        carrySummaryText.color = overloaded ? DungeonUiTheme.Warning : DungeonUiTheme.TextSecondary;
    }

    private static string FormatWeight(float weight)
    {
        return $"{Mathf.Max(0f, weight):0.#}kg";
    }

    private static void SetMeter(Slider slider, float normalizedValue, string valueText)
    {
        if (slider == null) return;

        float clamped = Mathf.Clamp01(normalizedValue);
        slider.value = clamped;
        Image fill = slider.fillRect != null ? slider.fillRect.GetComponent<Image>() : null;
        if (fill != null)
        {
            fill.color = DungeonUiTheme.GetMeterColor(clamped);
        }

        Transform row = slider.transform.parent;
        TMP_Text value = row != null ? row.Find("Value")?.GetComponent<TMP_Text>() : null;
        if (value != null)
        {
            value.text = valueText;
            value.color = clamped < 0.25f
                ? DungeonUiTheme.Danger
                : clamped < 0.5f
                    ? DungeonUiTheme.Warning
                    : DungeonUiTheme.TextSecondary;
        }
    }

    private static string FormatRole(CharacterRole role)
    {
        return role == CharacterRole.Owner ? "사장" : "일반";
    }

    private static string FormatLifecycle(CharacterLifecycleState state)
    {
        return state switch
        {
            CharacterLifecycleState.SpawningOutside => "입장 준비",
            CharacterLifecycleState.EnteringDungeon => "입장 중",
            CharacterLifecycleState.Active => "활동 중",
            CharacterLifecycleState.ExitingDungeon => "퇴장 중",
            CharacterLifecycleState.OnExpedition => "원정 중",
            CharacterLifecycleState.Despawned => "이탈",
            _ => "대기"
        };
    }

    public static string FormatLogText(CharacterActor character, int maxLines = 8)
    {
        return FormatLogText(character != null ? character.LogComponent : null, maxLines);
    }

    public static string FormatLogText(CharacterLog characterLog, int maxLines = 8)
    {
        IReadOnlyList<string> entries = characterLog != null ? characterLog.Entries : null;
        if (entries == null || entries.Count == 0)
        {
            return "아직 기록이 없습니다.";
        }

        int start = Mathf.Max(0, entries.Count - Mathf.Max(1, maxLines));
        List<string> rows = new List<string>();
        for (int i = entries.Count - 1; i >= start; i--)
        {
            rows.Add($"• {entries[i]}");
        }

        return string.Join("\n\n", rows);
    }

    public void OnEnable()
    {
        this.EventStartListening<InfoFeedEvent>();
        this.EventStartListening<CharacterGrowthTabRequestedEvent>();
    }

    private void OnDisable()
    {
        UnbindCharacter();
        this.EventStopListening<InfoFeedEvent>();
        this.EventStopListening<CharacterGrowthTabRequestedEvent>();
    }

    private void UnbindCharacter()
    {
        if (actor == null && characterStats == null && characterLog == null)
        {
            return;
        }

        if (characterStats != null)
        {
            characterStats.OnStatChange -= OnStatChange;
        }

        if (characterLog != null)
        {
            characterLog.OnLogAdded -= OnLogAdded;
            characterLog.OnLogDisplayChanged -= RefreshLogText;
        }

        if (progression != null)
        {
            progression.Changed -= RefreshProgression;
        }

        actor = null;
        characterStats = null;
        characterLog = null;
        progression = null;
        pendingCandidateConfirmation = -1;
        pendingCandidateUnlockLevel = -1;
        nextVitalsRefreshAt = 0f;
    }

    private static string GetDisplayName(GameObject targetObject)
    {
        CharacterIdentity identity = targetObject != null ? targetObject.GetComponent<CharacterIdentity>() : null;
        if (!string.IsNullOrWhiteSpace(identity != null ? identity.DisplayName : null))
        {
            return identity.DisplayName;
        }

        return targetObject != null ? targetObject.name : string.Empty;
    }

    private IUiPopupService ResolvePopupService()
    {
        if (popupService == null)
        {
            throw new InvalidOperationException(
                $"{nameof(CharacterSummeryInfo)} requires VContainer injection of {nameof(IUiPopupService)}.");
        }

        return popupService;
    }

    private ICharacterSummaryRuntimeLogFactory ResolveRuntimeLogFactory()
    {
        if (runtimeLogFactory == null)
        {
            throw new InvalidOperationException(
                $"{nameof(CharacterSummeryInfo)} requires VContainer injection of {nameof(ICharacterSummaryRuntimeLogFactory)}.");
        }

        return runtimeLogFactory;
    }

    private GameObject RequireUiRoot()
    {
        if (UI == null)
        {
            throw new InvalidOperationException($"{nameof(CharacterSummeryInfo)} requires a UI root reference.");
        }

        return UI;
    }

    private enum CharacterSummaryTab
    {
        Status,
        Growth,
        Mood,
        Records
    }
}
