using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class CharacterSummeryInfo : UIPopUp, UtilEventListener<InfoFeedEvent>
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
    private TMP_Text profileText;
    private Slider health;
    private TMP_Text moodSummaryText;
    private TMP_Text moodFactorsText;
    private GameObject statusTabContent;
    private GameObject moodTabContent;
    private GameObject recordsTabContent;
    private Button statusTabButton;
    private Button moodTabButton;
    private Button recordsTabButton;
    private float nextVitalsRefreshAt;
    private IUiPopupService popupService;
    private ICharacterSummaryRuntimeLogFactory runtimeLogFactory;

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
        RefreshMoodDetails();
    }

    public void OnTriggerEvent(InfoFeedEvent eventType)
    {
        if (eventType.infoable == null || eventType.infoable.GetInfoType() != InfoFeedEvent.Type.CHARACTER) return;

        CharacterActor nextActor = eventType.infoable as CharacterActor;
        if (nextActor == null)
        {
            return;
        }

        ResolvePopupService().CloseAll();
        UnbindCharacter();

        actor = nextActor;
        GameObject targetObject = actor.gameObject;
        characterStats = targetObject.GetComponent<CharacterStats>();
        characterLog = targetObject.GetComponent<CharacterLog>();

        GameObject uiRoot = RequireUiRoot();
        ICharacterSummaryRuntimeLogFactory viewFactory = ResolveRuntimeLogFactory();
        viewFactory.Ensure(this, uiRoot);
        viewFactory.ApplyFonts(uiRoot.transform);
        uiRoot.SetActive(true);
        ResolvePopupService().Open(this);
        ObjectName.text = GetDisplayName(targetObject);
        RefreshProfileAndVitals();

        if (characterStats != null)
        {
            OnStatChange(characterStats.Stats);
            characterStats.OnStatChange += OnStatChange;
        }

        if (characterLog != null)
        {
            characterLog.OnLogAdded += OnLogAdded;
            characterLog.OnLogDisplayChanged += RefreshLogText;
        }

        RefreshLogText();
    }

    public override void OnClose()
    {
        RequireUiRoot().SetActive(false);
        UnbindCharacter();
    }

    public void OnStatChange(Dictionary<CharacterCondition, float> stats)
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
        Dictionary<CharacterCondition, float> stats,
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
        logText = generatedLogText;
    }

    public void BindGeneratedTabs(
        GameObject generatedStatusTabContent,
        GameObject generatedMoodTabContent,
        GameObject generatedRecordsTabContent,
        Button generatedStatusTabButton,
        Button generatedMoodTabButton,
        Button generatedRecordsTabButton)
    {
        statusTabContent = generatedStatusTabContent;
        moodTabContent = generatedMoodTabContent;
        recordsTabContent = generatedRecordsTabContent;
        statusTabButton = generatedStatusTabButton;
        moodTabButton = generatedMoodTabButton;
        recordsTabButton = generatedRecordsTabButton;
        ShowStatusTab();
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

    public void ShowRecordsTab()
    {
        SetActiveTab(CharacterSummaryTab.Records);
        RefreshLogText();
    }

    private void SetActiveTab(CharacterSummaryTab tab)
    {
        statusTabContent?.SetActive(tab == CharacterSummaryTab.Status);
        moodTabContent?.SetActive(tab == CharacterSummaryTab.Mood);
        recordsTabContent?.SetActive(tab == CharacterSummaryTab.Records);
        DungeonUiTheme.StyleButton(statusTabButton, selected: tab == CharacterSummaryTab.Status);
        DungeonUiTheme.StyleButton(moodTabButton, selected: tab == CharacterSummaryTab.Mood);
        DungeonUiTheme.StyleButton(recordsTabButton, selected: tab == CharacterSummaryTab.Records);
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
            profileText.text = $"{species} · {FormatRole(actor.Role)} · {FormatLifecycle(actor.CurrentLifecycleState)}";
        }

        if (characterStats == null)
        {
            SetMeter(health, 0f, "--");
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
    }

    private void OnDisable()
    {
        UnbindCharacter();
        this.EventStopListening<InfoFeedEvent>();
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

        actor = null;
        characterStats = null;
        characterLog = null;
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
        Mood,
        Records
    }
}
