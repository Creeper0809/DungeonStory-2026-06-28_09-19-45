using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public interface ICharacterSummaryRuntimeLogFactory
{
    void Ensure(CharacterSummeryInfo owner, GameObject uiRoot);
    void ApplyFonts(Transform root);
}

public sealed class CharacterSummaryRuntimeLogFactory : ICharacterSummaryRuntimeLogFactory
{
    private const string RuntimeViewName = "CharacterSummaryGeneratedView";

    private readonly ITmpKoreanFontService tmpKoreanFontService;

    [Inject]
    public CharacterSummaryRuntimeLogFactory(ITmpKoreanFontService tmpKoreanFontService)
    {
        this.tmpKoreanFontService = tmpKoreanFontService
            ?? throw new ArgumentNullException(nameof(tmpKoreanFontService));
    }

    public void Ensure(CharacterSummeryInfo owner, GameObject uiRoot)
    {
        if (owner == null)
        {
            throw new ArgumentNullException(nameof(owner));
        }

        if (uiRoot == null)
        {
            throw new ArgumentNullException(nameof(uiRoot));
        }

        ConfigurePanelBounds(uiRoot);

        Transform generated = uiRoot.transform.Find(RuntimeViewName);
        if (generated != null
            && (generated.Find("TabBar/GrowthTab") == null
                || generated.Find("Content/GrowthContent/GrowthList") == null
                || generated.Find("Content/StatusContent/CarrySummaryText") == null))
        {
            UnityEngine.Object.DestroyImmediate(generated.gameObject);
            generated = null;
        }

        if (generated == null)
        {
            DisableLegacyChildren(uiRoot.transform);
            generated = CreateView(owner, uiRoot.transform);
        }

        Bind(owner, generated);
        ApplyFonts(uiRoot.transform);
    }

    public void ApplyFonts(Transform root)
    {
        if (root == null)
        {
            throw new ArgumentNullException(nameof(root));
        }

        tmpKoreanFontService.ApplyToChildren(root);
    }

    private Transform CreateView(CharacterSummeryInfo owner, Transform parent)
    {
        RectTransform view = CreateRect(RuntimeViewName, parent);
        SetStretch(view, Vector2.zero, Vector2.zero);

        RectTransform header = CreateRect("Header", view);
        header.anchorMin = new Vector2(0f, 1f);
        header.anchorMax = new Vector2(1f, 1f);
        header.pivot = new Vector2(0.5f, 1f);
        header.anchoredPosition = Vector2.zero;
        header.sizeDelta = new Vector2(0f, 76f);
        header.gameObject.AddComponent<Image>().color = DungeonUiTheme.SurfaceRaised;

        TMP_Text nameText = CreateText("CharacterName", header, 28f, FontStyles.Bold);
        nameText.alignment = TextAlignmentOptions.BottomLeft;
        nameText.color = DungeonUiTheme.TextPrimary;
        nameText.enableAutoSizing = true;
        nameText.fontSizeMin = 18f;
        nameText.fontSizeMax = 28f;
        nameText.textWrappingMode = TextWrappingModes.NoWrap;
        nameText.overflowMode = TextOverflowModes.Truncate;
        SetStretch(nameText.rectTransform, new Vector2(18f, 32f), new Vector2(-92f, -8f));

        TMP_Text profileText = CreateText("CharacterProfile", header, 15f, FontStyles.Normal);
        profileText.alignment = TextAlignmentOptions.TopLeft;
        profileText.color = DungeonUiTheme.TextSecondary;
        profileText.textWrappingMode = TextWrappingModes.NoWrap;
        profileText.overflowMode = TextOverflowModes.Truncate;
        SetStretch(profileText.rectTransform, new Vector2(18f, 8f), new Vector2(-92f, -44f));

        Button closeButton = CreateButton("CloseButton", header, "닫기");
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-12f, -12f);
        closeRect.sizeDelta = new Vector2(68f, 36f);
        closeButton.onClick.AddListener(owner.OnClose);

        RectTransform tabBar = CreateRect("TabBar", view);
        tabBar.anchorMin = new Vector2(0f, 1f);
        tabBar.anchorMax = new Vector2(1f, 1f);
        tabBar.pivot = new Vector2(0.5f, 1f);
        tabBar.anchoredPosition = new Vector2(0f, -82f);
        tabBar.sizeDelta = new Vector2(0f, 42f);
        HorizontalLayoutGroup tabs = tabBar.gameObject.AddComponent<HorizontalLayoutGroup>();
        tabs.padding = new RectOffset(14, 14, 0, 0);
        tabs.spacing = 6f;
        tabs.childAlignment = TextAnchor.MiddleLeft;
        tabs.childControlWidth = true;
        tabs.childControlHeight = true;
        tabs.childForceExpandWidth = true;
        tabs.childForceExpandHeight = true;

        Button statusTabButton = CreateButton("StatusTab", tabBar, "상태");
        statusTabButton.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        Button growthTabButton = CreateButton("GrowthTab", tabBar, "성장");
        growthTabButton.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        Button moodTabButton = CreateButton("MoodTab", tabBar, "기분");
        moodTabButton.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        Button recordsTabButton = CreateButton("RecordsTab", tabBar, "기록");
        recordsTabButton.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        statusTabButton.onClick.AddListener(owner.ShowStatusTab);
        growthTabButton.onClick.AddListener(owner.ShowGrowthTab);
        moodTabButton.onClick.AddListener(owner.ShowMoodTab);
        recordsTabButton.onClick.AddListener(owner.ShowRecordsTab);

        RectTransform content = CreateRect("Content", view);
        SetStretch(content, new Vector2(14f, 14f), new Vector2(-14f, -132f));

        RectTransform statusContent = CreateRect("StatusContent", content);
        SetStretch(statusContent, Vector2.zero, Vector2.zero);
        VerticalLayoutGroup vertical = statusContent.gameObject.AddComponent<VerticalLayoutGroup>();
        vertical.spacing = 6f;
        vertical.padding = new RectOffset(0, 0, 0, 0);
        vertical.childAlignment = TextAnchor.UpperLeft;
        vertical.childControlWidth = true;
        vertical.childControlHeight = true;
        vertical.childForceExpandWidth = true;
        vertical.childForceExpandHeight = false;

        CreateSectionLabel(statusContent, "상태");
        Slider health = CreateMeterRow(statusContent, "Health", "체력", 46f);
        CreateSectionLabel(statusContent, "욕구");
        Slider hunger = CreateMeterRow(statusContent, "Hunger", "포만감", 40f);
        Slider fun = CreateMeterRow(statusContent, "Fun", "재미", 40f);
        Slider sleep = CreateMeterRow(statusContent, "Sleep", "휴식", 40f);
        Slider excretion = CreateMeterRow(statusContent, "Excretion", "배변", 40f);
        Slider hygiene = CreateMeterRow(statusContent, "Hygiene", "위생", 40f);

        TMP_Text carrySummary = CreateText("CarrySummaryText", statusContent, 14f, FontStyles.Normal);
        carrySummary.text = "소지 아이템 없음";
        carrySummary.color = DungeonUiTheme.TextSecondary;
        carrySummary.alignment = TextAlignmentOptions.TopLeft;
        carrySummary.textWrappingMode = TextWrappingModes.Normal;
        carrySummary.overflowMode = TextOverflowModes.Ellipsis;
        carrySummary.margin = new Vector4(8f, 4f, 8f, 4f);
        LayoutElement carrySummaryLayout = carrySummary.gameObject.AddComponent<LayoutElement>();
        carrySummaryLayout.minHeight = 72f;
        carrySummaryLayout.preferredHeight = 86f;

        RectTransform growthContent = CreateRect("GrowthContent", content);
        SetStretch(growthContent, Vector2.zero, Vector2.zero);
        growthContent.gameObject.AddComponent<Image>().color = DungeonUiTheme.Panel;
        growthContent.gameObject.AddComponent<RectMask2D>();
        ScrollRect growthScroll = growthContent.gameObject.AddComponent<ScrollRect>();
        growthScroll.viewport = growthContent;
        growthScroll.horizontal = false;
        growthScroll.vertical = true;
        growthScroll.movementType = ScrollRect.MovementType.Clamped;
        growthScroll.scrollSensitivity = 28f;

        RectTransform growthList = CreateRect("GrowthList", growthContent);
        growthList.anchorMin = new Vector2(0f, 1f);
        growthList.anchorMax = new Vector2(1f, 1f);
        growthList.pivot = new Vector2(0.5f, 1f);
        growthList.anchoredPosition = Vector2.zero;
        growthList.sizeDelta = Vector2.zero;
        VerticalLayoutGroup growthVertical = growthList.gameObject.AddComponent<VerticalLayoutGroup>();
        growthVertical.spacing = 6f;
        growthVertical.childAlignment = TextAnchor.UpperLeft;
        growthVertical.childControlWidth = true;
        growthVertical.childControlHeight = true;
        growthVertical.childForceExpandWidth = true;
        growthVertical.childForceExpandHeight = false;
        ContentSizeFitter growthFitter = growthList.gameObject.AddComponent<ContentSizeFitter>();
        growthFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        growthFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        growthScroll.content = growthList;

        CreateSectionLabel(growthList, "레벨");
        Slider experience = CreateMeterRow(growthList, "Experience", "경험치", 48f);
        TMP_Text progressionSummary = CreateText(
            "ProgressionSummaryText",
            growthList,
            15f,
            FontStyles.Normal);
        progressionSummary.color = DungeonUiTheme.TextSecondary;
        progressionSummary.alignment = TextAlignmentOptions.TopLeft;
        progressionSummary.textWrappingMode = TextWrappingModes.Normal;
        progressionSummary.overflowMode = TextOverflowModes.Overflow;
        progressionSummary.lineSpacing = 5f;
        progressionSummary.margin = new Vector4(6f, 0f, 6f, 0f);
        LayoutElement progressionLayout = progressionSummary.gameObject.AddComponent<LayoutElement>();
        progressionLayout.minHeight = 178f;
        progressionLayout.preferredHeight = 214f;
        CreateSectionLabel(growthList, "기술 슬롯과 후보");

        Button[] skillButtons = new Button[10];
        for (int i = 0; i < skillButtons.Length; i++)
        {
            int capturedIndex = i;
            Button skillButton = CreateButton($"Skill_{i}", growthList, string.Empty);
            LayoutElement skillLayout = skillButton.gameObject.AddComponent<LayoutElement>();
            skillLayout.minHeight = 42f;
            skillLayout.preferredHeight = 42f;
            TMP_Text skillLabel = skillButton.transform.Find("Label")?.GetComponent<TMP_Text>();
            if (skillLabel != null)
            {
                skillLabel.fontSize = 14f;
                skillLabel.alignment = TextAlignmentOptions.MidlineLeft;
                skillLabel.textWrappingMode = TextWrappingModes.Normal;
            }
            skillButton.onClick.AddListener(() => owner.ToggleSkillAt(capturedIndex));
            skillButtons[i] = skillButton;
        }
        growthContent.gameObject.SetActive(false);

        RectTransform moodContent = CreateRect("MoodContent", content);
        SetStretch(moodContent, Vector2.zero, Vector2.zero);
        VerticalLayoutGroup moodVertical = moodContent.gameObject.AddComponent<VerticalLayoutGroup>();
        moodVertical.spacing = 6f;
        moodVertical.childAlignment = TextAnchor.UpperLeft;
        moodVertical.childControlWidth = true;
        moodVertical.childControlHeight = true;
        moodVertical.childForceExpandWidth = true;
        moodVertical.childForceExpandHeight = false;

        CreateSectionLabel(moodContent, "기분");
        Slider mood = CreateMeterRow(moodContent, "MoodOverview", "현재 기분", 48f);
        TMP_Text moodSummaryText = CreateText("MoodSummaryText", moodContent, 15f, FontStyles.Normal);
        moodSummaryText.text = "평온함 · 기준 50 · 보정 +0";
        moodSummaryText.color = DungeonUiTheme.TextSecondary;
        moodSummaryText.alignment = TextAlignmentOptions.MidlineLeft;
        moodSummaryText.margin = new Vector4(6f, 0f, 6f, 0f);
        moodSummaryText.gameObject.AddComponent<LayoutElement>().preferredHeight = 30f;
        CreateSectionLabel(moodContent, "기분 요인");

        RectTransform moodFactorsViewport = CreateRect("MoodFactorsViewport", moodContent);
        moodFactorsViewport.gameObject.AddComponent<Image>().color = DungeonUiTheme.SurfaceMuted;
        moodFactorsViewport.gameObject.AddComponent<RectMask2D>();
        LayoutElement moodFactorsLayout = moodFactorsViewport.gameObject.AddComponent<LayoutElement>();
        moodFactorsLayout.minHeight = 240f;
        moodFactorsLayout.flexibleHeight = 1f;
        ScrollRect moodFactorsScroll = moodFactorsViewport.gameObject.AddComponent<ScrollRect>();
        moodFactorsScroll.viewport = moodFactorsViewport;
        moodFactorsScroll.horizontal = false;
        moodFactorsScroll.vertical = true;
        moodFactorsScroll.movementType = ScrollRect.MovementType.Clamped;
        moodFactorsScroll.scrollSensitivity = 28f;

        TMP_Text moodFactorsText = CreateText("MoodFactorsText", moodFactorsViewport, 16f, FontStyles.Normal);
        moodFactorsText.text = "현재 기분을 바꾸는 요인이 없습니다.";
        moodFactorsText.color = DungeonUiTheme.TextSecondary;
        moodFactorsText.alignment = TextAlignmentOptions.TopLeft;
        moodFactorsText.textWrappingMode = TextWrappingModes.Normal;
        moodFactorsText.overflowMode = TextOverflowModes.Overflow;
        moodFactorsText.lineSpacing = 8f;
        moodFactorsText.margin = new Vector4(14f, 12f, 14f, 12f);
        moodFactorsText.rectTransform.anchorMin = new Vector2(0f, 1f);
        moodFactorsText.rectTransform.anchorMax = new Vector2(1f, 1f);
        moodFactorsText.rectTransform.pivot = new Vector2(0.5f, 1f);
        moodFactorsText.rectTransform.anchoredPosition = Vector2.zero;
        moodFactorsText.rectTransform.sizeDelta = Vector2.zero;
        ContentSizeFitter moodFitter = moodFactorsText.gameObject.AddComponent<ContentSizeFitter>();
        moodFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        moodFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        moodFactorsScroll.content = moodFactorsText.rectTransform;
        moodContent.gameObject.SetActive(false);

        RectTransform recordsContent = CreateRect("RecordsContent", content);
        SetStretch(recordsContent, Vector2.zero, Vector2.zero);
        recordsContent.gameObject.AddComponent<Image>().color = DungeonUiTheme.SurfaceMuted;
        recordsContent.gameObject.AddComponent<RectMask2D>();
        ScrollRect recordsScroll = recordsContent.gameObject.AddComponent<ScrollRect>();
        recordsScroll.viewport = recordsContent;
        recordsScroll.horizontal = false;
        recordsScroll.vertical = true;
        recordsScroll.movementType = ScrollRect.MovementType.Clamped;
        recordsScroll.scrollSensitivity = 28f;

        TMP_Text logText = CreateText("CharacterLogText", recordsContent, 16f, FontStyles.Normal);
        logText.color = DungeonUiTheme.TextSecondary;
        logText.alignment = TextAlignmentOptions.TopLeft;
        logText.textWrappingMode = TextWrappingModes.Normal;
        logText.overflowMode = TextOverflowModes.Overflow;
        logText.lineSpacing = 8f;
        logText.margin = new Vector4(14f, 12f, 14f, 12f);
        logText.rectTransform.anchorMin = new Vector2(0f, 1f);
        logText.rectTransform.anchorMax = new Vector2(1f, 1f);
        logText.rectTransform.pivot = new Vector2(0.5f, 1f);
        logText.rectTransform.anchoredPosition = Vector2.zero;
        logText.rectTransform.sizeDelta = Vector2.zero;
        ContentSizeFitter logFitter = logText.gameObject.AddComponent<ContentSizeFitter>();
        logFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        logFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        recordsScroll.content = logText.rectTransform;
        recordsContent.gameObject.SetActive(false);

        owner.BindGeneratedView(
            nameText,
            profileText,
            health,
            mood,
            fun,
            hunger,
            sleep,
            excretion,
            hygiene,
            moodSummaryText,
            moodFactorsText,
            carrySummary,
            logText);
        owner.BindGeneratedGrowth(experience, progressionSummary, skillButtons);
        owner.BindGeneratedTabs(
            statusContent.gameObject,
            growthContent.gameObject,
            moodContent.gameObject,
            recordsContent.gameObject,
            statusTabButton,
            growthTabButton,
            moodTabButton,
            recordsTabButton);
        return view;
    }

    private void Bind(CharacterSummeryInfo owner, Transform generated)
    {
        if (owner == null || generated == null) return;

        owner.BindGeneratedView(
            generated.Find("Header/CharacterName")?.GetComponent<TMP_Text>(),
            generated.Find("Header/CharacterProfile")?.GetComponent<TMP_Text>(),
            FindSlider(generated, "Health"),
            FindSlider(generated, "MoodOverview", "MoodContent"),
            FindSlider(generated, "Fun"),
            FindSlider(generated, "Hunger"),
            FindSlider(generated, "Sleep"),
            FindSlider(generated, "Excretion"),
            FindSlider(generated, "Hygiene"),
            generated.Find("Content/MoodContent/MoodSummaryText")?.GetComponent<TMP_Text>(),
            generated.Find("Content/MoodContent/MoodFactorsViewport/MoodFactorsText")?.GetComponent<TMP_Text>(),
            generated.Find("Content/StatusContent/CarrySummaryText")?.GetComponent<TMP_Text>(),
            generated.Find("Content/RecordsContent/CharacterLogText")?.GetComponent<TMP_Text>());
        Button[] skillButtons = new Button[10];
        for (int i = 0; i < skillButtons.Length; i++)
        {
            skillButtons[i] = generated.Find($"Content/GrowthContent/GrowthList/Skill_{i}")?.GetComponent<Button>();
        }
        owner.BindGeneratedGrowth(
            FindSlider(generated, "Experience", "GrowthContent/GrowthList"),
            generated.Find("Content/GrowthContent/GrowthList/ProgressionSummaryText")?.GetComponent<TMP_Text>(),
            skillButtons);
        owner.BindGeneratedTabs(
            generated.Find("Content/StatusContent")?.gameObject,
            generated.Find("Content/GrowthContent")?.gameObject,
            generated.Find("Content/MoodContent")?.gameObject,
            generated.Find("Content/RecordsContent")?.gameObject,
            generated.Find("TabBar/StatusTab")?.GetComponent<Button>(),
            generated.Find("TabBar/GrowthTab")?.GetComponent<Button>(),
            generated.Find("TabBar/MoodTab")?.GetComponent<Button>(),
            generated.Find("TabBar/RecordsTab")?.GetComponent<Button>());
    }

    private static Slider FindSlider(Transform root, string rowName, string contentName = "StatusContent")
    {
        return root.Find($"Content/{contentName}/{rowName}/Track")?.GetComponent<Slider>();
    }

    private TMP_Text CreateSectionLabel(Transform parent, string text)
    {
        TMP_Text label = CreateText("Section_" + text, parent, 16f, FontStyles.Bold);
        label.text = text;
        label.color = DungeonUiTheme.TextPrimary;
        label.alignment = TextAlignmentOptions.BottomLeft;
        label.margin = new Vector4(2f, 0f, 0f, 0f);
        LayoutElement layout = label.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = 24f;
        layout.preferredHeight = 24f;
        return label;
    }

    private Slider CreateMeterRow(Transform parent, string name, string labelText, float height)
    {
        RectTransform row = CreateRect(name, parent);
        row.gameObject.AddComponent<Image>().color = DungeonUiTheme.Surface;
        LayoutElement rowLayout = row.gameObject.AddComponent<LayoutElement>();
        rowLayout.minHeight = height;
        rowLayout.preferredHeight = height;

        HorizontalLayoutGroup horizontal = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        horizontal.padding = new RectOffset(10, 10, 6, 6);
        horizontal.spacing = 10f;
        horizontal.childAlignment = TextAnchor.MiddleLeft;
        horizontal.childControlWidth = true;
        horizontal.childControlHeight = true;
        horizontal.childForceExpandWidth = false;
        horizontal.childForceExpandHeight = false;

        TMP_Text label = CreateText("Label", row, 16f, FontStyles.Bold);
        label.text = labelText;
        label.color = DungeonUiTheme.TextPrimary;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        LayoutElement labelLayout = label.gameObject.AddComponent<LayoutElement>();
        labelLayout.minWidth = 74f;
        labelLayout.preferredWidth = 74f;

        RectTransform track = CreateRect("Track", row);
        Image trackImage = track.gameObject.AddComponent<Image>();
        trackImage.color = DungeonUiTheme.SurfaceMuted;
        LayoutElement trackLayout = track.gameObject.AddComponent<LayoutElement>();
        trackLayout.minWidth = 120f;
        trackLayout.flexibleWidth = 1f;
        trackLayout.preferredHeight = 16f;

        RectTransform fillArea = CreateRect("FillArea", track);
        SetStretch(fillArea, new Vector2(3f, 3f), new Vector2(-3f, -3f));
        RectTransform fill = CreateRect("Fill", fillArea);
        SetStretch(fill, Vector2.zero, Vector2.zero);
        Image fillImage = fill.gameObject.AddComponent<Image>();
        fillImage.color = DungeonUiTheme.Good;

        Slider slider = track.gameObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.wholeNumbers = false;
        slider.direction = Slider.Direction.LeftToRight;
        slider.fillRect = fill;
        slider.targetGraphic = fillImage;
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;

        TMP_Text value = CreateText("Value", row, 15f, FontStyles.Bold);
        value.text = "100";
        value.color = DungeonUiTheme.TextSecondary;
        value.alignment = TextAlignmentOptions.MidlineRight;
        LayoutElement valueLayout = value.gameObject.AddComponent<LayoutElement>();
        float valueWidth = name == "Health"
            ? 142f
            : name == "Experience"
                ? 92f
                : 48f;
        valueLayout.minWidth = valueWidth;
        valueLayout.preferredWidth = valueWidth;
        return slider;
    }

    private Button CreateButton(string name, Transform parent, string labelText)
    {
        RectTransform rect = CreateRect(name, parent);
        Image image = rect.gameObject.AddComponent<Image>();
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        TMP_Text label = CreateText("Label", rect, 15f, FontStyles.Bold);
        label.text = labelText;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        SetStretch(label.rectTransform, new Vector2(6f, 2f), new Vector2(-6f, -2f));
        DungeonUiTheme.StyleButton(button);
        return button;
    }

    private TMP_Text CreateText(string name, Transform parent, float fontSize, FontStyles style)
    {
        RectTransform rect = CreateRect(name, parent);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        tmpKoreanFontService.Apply(text);
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = DungeonUiTheme.TextPrimary;
        text.characterSpacing = 0f;
        text.raycastTarget = false;
        return text;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject.GetComponent<RectTransform>();
    }

    private static void ConfigurePanelBounds(GameObject uiRoot)
    {
        RectTransform wrapper = uiRoot.transform.parent as RectTransform;
        if (wrapper != null)
        {
            wrapper.anchorMin = Vector2.zero;
            wrapper.anchorMax = Vector2.zero;
            wrapper.pivot = Vector2.zero;
            wrapper.anchoredPosition = new Vector2(24f, 80f);
            wrapper.sizeDelta = new Vector2(460f, 700f);
        }

        RectTransform rootRect = uiRoot.GetComponent<RectTransform>();
        if (rootRect != null)
        {
            SetStretch(rootRect, Vector2.zero, Vector2.zero);
        }

        Image background = uiRoot.GetComponent<Image>();
        if (background == null)
        {
            background = uiRoot.AddComponent<Image>();
        }

        background.color = DungeonUiTheme.Panel;
    }

    private static void DisableLegacyChildren(Transform root)
    {
        foreach (Transform child in root)
        {
            child.gameObject.SetActive(false);
        }
    }

    private static void SetStretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}
