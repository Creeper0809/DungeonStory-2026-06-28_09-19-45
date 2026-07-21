using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using VContainer.Unity;

public sealed class DungeonTitleCanvasProvider : IDungeonUiCanvasProvider
{
    private Canvas canvas;

    public Canvas GetOrCreateCanvas()
    {
        if (canvas != null)
        {
            return canvas;
        }

        EnsureEventSystem();
        GameObject canvasObject = new GameObject(
            "TitleCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static void EnsureEventSystem()
    {
        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }
}

public sealed class DungeonTitleUiController : IStartable, IDisposable
{
    private const float ConfirmationSeconds = 4f;

    private readonly IDungeonSaveSlotCatalog slotCatalog;
    private readonly IDungeonSceneNavigator sceneNavigator;
    private readonly IDungeonUiCanvasProvider canvasProvider;
    private readonly ITmpKoreanFontService fontService;
    private readonly IDungeonSettingsUi settingsUi;

    private readonly Dictionary<string, SlotView> slots = new Dictionary<string, SlotView>();
    private GameObject runtimeRoot;
    private Button continueButton;
    private TMP_Text statusText;
    private GameObject difficultyModal;
    private string confirmationKey = string.Empty;
    private float confirmationExpiresAt;

    public DungeonTitleUiController(
        IDungeonSaveSlotCatalog slotCatalog,
        IDungeonSceneNavigator sceneNavigator,
        IDungeonUiCanvasProvider canvasProvider,
        ITmpKoreanFontService fontService,
        IDungeonSettingsUi settingsUi)
    {
        this.slotCatalog = slotCatalog ?? throw new ArgumentNullException(nameof(slotCatalog));
        this.sceneNavigator = sceneNavigator ?? throw new ArgumentNullException(nameof(sceneNavigator));
        this.canvasProvider = canvasProvider ?? throw new ArgumentNullException(nameof(canvasProvider));
        this.fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
        this.settingsUi = settingsUi ?? throw new ArgumentNullException(nameof(settingsUi));
    }

    public void Start()
    {
        Canvas canvas = canvasProvider.GetOrCreateCanvas();
        CreateTitleScreen(canvas.transform);
        RefreshSlots();
        string transitionMessage = sceneNavigator.ConsumeTitleMessage();
        SetStatus(
            string.IsNullOrWhiteSpace(transitionMessage)
                ? "저장된 던전을 이어가거나 새로운 사장을 선택하세요."
                : transitionMessage,
            !string.IsNullOrWhiteSpace(transitionMessage));
        DungeonUiThemeRuntime.Ensure(canvas, fontService).ApplyNow();
    }

    public void Dispose()
    {
        if (runtimeRoot != null)
        {
            UnityEngine.Object.Destroy(runtimeRoot);
        }
    }

    private void CreateTitleScreen(Transform parent)
    {
        runtimeRoot = new GameObject("DungeonTitleRuntimeUI", typeof(RectTransform));
        runtimeRoot.transform.SetParent(parent, false);
        Stretch(runtimeRoot.GetComponent<RectTransform>());

        Image background = CreateImage(runtimeRoot.transform, "TitleBackground", DungeonUiTheme.SurfaceMuted);
        Stretch(background.rectTransform);
        background.raycastTarget = true;

        Image topBand = CreateImage(runtimeRoot.transform, "TitleTopBand", DungeonUiTheme.Surface);
        SetRect(topBand.rectTransform, new Vector2(0f, 0.9f), Vector2.one, Vector2.zero, Vector2.zero);

        Image accentBand = CreateImage(runtimeRoot.transform, "TitleAccentBand", DungeonUiTheme.Accent);
        SetRect(accentBand.rectTransform, new Vector2(0f, 0.895f), new Vector2(1f, 0.9f), Vector2.zero, Vector2.zero);

        Transform brand = CreatePanel(runtimeRoot.transform, "TitleBrand", new Vector2(0.07f, 0.14f), new Vector2(0.48f, 0.84f), false);
        CreateBranding(brand);

        Transform savePanel = CreatePanel(runtimeRoot.transform, "TitleSavePanel", new Vector2(0.52f, 0.14f), new Vector2(0.93f, 0.84f), true);
        CreateSavePanel(savePanel);
        CreateDifficultyModal(runtimeRoot.transform);

        TMP_Text version = CreateText(runtimeRoot.transform, "VersionText", Application.version, 15f, TextAlignmentOptions.BottomRight);
        SetRect(version.rectTransform, new Vector2(0.76f, 0.035f), new Vector2(0.93f, 0.09f), Vector2.zero, Vector2.zero);
        version.color = DungeonUiTheme.TextSecondary;
    }

    private void CreateBranding(Transform parent)
    {
        GameObject iconObject = new GameObject("BrandIcon", typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(parent, false);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        SetRect(iconRect, new Vector2(0f, 0.77f), new Vector2(0.18f, 1f), Vector2.zero, Vector2.zero);
        Image icon = iconObject.GetComponent<Image>();
        icon.sprite = Resources.Load<Sprite>("Branding/DungeonStoryIcon");
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        TMP_Text title = CreateText(parent, "Title", "DungeonStory", 58f, TextAlignmentOptions.BottomLeft);
        SetRect(title.rectTransform, new Vector2(0.21f, 0.82f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        title.fontStyle = FontStyles.Bold;

        TMP_Text subtitle = CreateText(parent, "Subtitle", "지하에 방을 짓고, 살아 움직이는 던전을 운영하세요.", 22f, TextAlignmentOptions.TopLeft);
        SetRect(subtitle.rectTransform, new Vector2(0.21f, 0.7f), new Vector2(1f, 0.82f), Vector2.zero, Vector2.zero);
        subtitle.color = DungeonUiTheme.TextSecondary;

        continueButton = CreateButton(parent, "ContinueLatestButton", "이어하기", ContinueLatest,
            new Vector2(0f, 0.48f), new Vector2(0.72f, 0.59f), selected: true);
        CreateButton(parent, "StartNewRunButton", "새 게임", StartNewGame,
            new Vector2(0f, 0.34f), new Vector2(0.72f, 0.45f));
        CreateButton(parent, "StartupSettingsButton", "설정", settingsUi.Show,
            new Vector2(0f, 0.2f), new Vector2(0.72f, 0.31f));
        CreateButton(parent, "StartupQuitButton", "종료", QuitGame,
            new Vector2(0f, 0.06f), new Vector2(0.72f, 0.17f), destructive: true);
    }

    private void CreateSavePanel(Transform parent)
    {
        TMP_Text heading = CreateText(parent, "SaveHeading", "저장된 던전", 30f, TextAlignmentOptions.MidlineLeft);
        SetRect(heading.rectTransform, new Vector2(0.055f, 0.88f), new Vector2(0.95f, 0.98f), Vector2.zero, Vector2.zero);
        heading.fontStyle = FontStyles.Bold;

        TMP_Text hint = CreateText(parent, "SaveHint", "최근 저장 기록", 16f, TextAlignmentOptions.MidlineLeft);
        SetRect(hint.rectTransform, new Vector2(0.055f, 0.81f), new Vector2(0.95f, 0.88f), Vector2.zero, Vector2.zero);
        hint.color = DungeonUiTheme.TextSecondary;

        CreateSlot(parent, DungeonGameSaveSlotService.ManualSaveSlot, "수동 저장", 0.59f, 0.79f);
        CreateSlot(parent, DungeonGameSaveSlotService.QuickSaveSlot, "빠른 저장", 0.37f, 0.57f);
        CreateSlot(parent, DungeonGameSaveSlotService.AutoSaveSlot, "자동 저장", 0.15f, 0.35f);

        statusText = CreateText(parent, "TitleStatus", string.Empty, 16f, TextAlignmentOptions.TopLeft);
        SetRect(statusText.rectTransform, new Vector2(0.055f, 0.025f), new Vector2(0.95f, 0.125f), Vector2.zero, Vector2.zero);
        statusText.textWrappingMode = TextWrappingModes.Normal;
    }

    private void CreateSlot(Transform parent, string slotId, string label, float bottom, float top)
    {
        Transform row = CreatePanel(parent, "SaveSlot_" + slotId, new Vector2(0.055f, bottom), new Vector2(0.945f, top), false);
        row.GetComponent<Image>().color = DungeonUiTheme.SurfaceRaised;

        TMP_Text name = CreateText(row, "SlotName", label, 20f, TextAlignmentOptions.TopLeft);
        SetRect(name.rectTransform, new Vector2(0.035f, 0.51f), new Vector2(0.55f, 0.92f), Vector2.zero, Vector2.zero);
        name.fontStyle = FontStyles.Bold;

        TMP_Text metadata = CreateText(row, "Metadata", string.Empty, 15f, TextAlignmentOptions.TopLeft);
        SetRect(metadata.rectTransform, new Vector2(0.035f, 0.08f), new Vector2(0.62f, 0.54f), Vector2.zero, Vector2.zero);
        metadata.color = DungeonUiTheme.TextSecondary;

        Button load = CreateButton(row, "LoadButton_" + slotId, "불러오기", () => LoadSlot(slotId),
            new Vector2(0.64f, 0.2f), new Vector2(0.84f, 0.8f));
        Button delete = CreateButton(row, "DeleteButton_" + slotId, "삭제", () => DeleteSlot(slotId),
            new Vector2(0.86f, 0.2f), new Vector2(0.97f, 0.8f), destructive: true);
        slots.Add(slotId, new SlotView(metadata, load, delete));
    }

    private void ContinueLatest()
    {
        DungeonSaveSlotInfo latest = LatestValidSlot();
        if (latest == null)
        {
            SetStatus("이어갈 수 있는 저장 데이터가 없습니다.", true);
            return;
        }

        BeginLoad(latest.SlotId);
    }

    private void StartNewGame()
    {
        bool hasSave = slots.Keys.Any(slotCatalog.HasSave);
        if (hasSave && !Confirm("new-run", "새 게임은 기존 실행 저장을 지웁니다. 다시 눌러 시작하세요."))
        {
            return;
        }

        if (!ShowDifficultySelection())
        {
            SetStatus("이미 화면을 전환하고 있거나 Gameplay 씬을 찾을 수 없습니다.", true);
        }
    }

    private bool ShowDifficultySelection()
    {
        ClearConfirmation();
        difficultyModal.SetActive(true);
        return true;
    }

    private void BeginNewGame(DungeonDifficulty difficulty)
    {
        difficultyModal.SetActive(false);
        if (!sceneNavigator.StartNewGame(difficulty))
        {
            SetStatus("게임 화면으로 전환할 수 없습니다.", true);
        }
    }

    private void CreateDifficultyModal(Transform parent)
    {
        difficultyModal = new GameObject("DifficultyModal", typeof(RectTransform));
        difficultyModal.transform.SetParent(parent, false);
        Stretch(difficultyModal.GetComponent<RectTransform>());

        Image scrim = CreateImage(difficultyModal.transform, "DifficultyScrim", DungeonUiTheme.ModalScrim);
        Stretch(scrim.rectTransform);
        scrim.raycastTarget = true;

        Transform panel = CreatePanel(
            difficultyModal.transform,
            "DifficultyPanel",
            new Vector2(0.31f, 0.3f),
            new Vector2(0.69f, 0.7f),
            true);
        TMP_Text heading = CreateText(panel, "DifficultyHeading", "난이도 선택", 34f, TextAlignmentOptions.Center);
        SetRect(heading.rectTransform, new Vector2(0.06f, 0.78f), new Vector2(0.94f, 0.94f), Vector2.zero, Vector2.zero);
        heading.fontStyle = FontStyles.Bold;

        TMP_Text easy = CreateText(panel, "EasyInfo", "Easy\n적 체력 · 공격 80%", 17f, TextAlignmentOptions.Center);
        SetRect(easy.rectTransform, new Vector2(0.04f, 0.48f), new Vector2(0.31f, 0.7f), Vector2.zero, Vector2.zero);
        TMP_Text normal = CreateText(panel, "NormalInfo", "Normal\n기본 전투 수치", 17f, TextAlignmentOptions.Center);
        SetRect(normal.rectTransform, new Vector2(0.365f, 0.48f), new Vector2(0.635f, 0.7f), Vector2.zero, Vector2.zero);
        TMP_Text hard = CreateText(panel, "HardInfo", "Hard\n체력 125% · 공격 120%", 17f, TextAlignmentOptions.Center);
        SetRect(hard.rectTransform, new Vector2(0.69f, 0.48f), new Vector2(0.96f, 0.7f), Vector2.zero, Vector2.zero);

        CreateButton(panel, "DifficultyEasyButton", "Easy", () => BeginNewGame(DungeonDifficulty.Easy),
            new Vector2(0.04f, 0.28f), new Vector2(0.31f, 0.46f));
        CreateButton(panel, "DifficultyNormalButton", "Normal", () => BeginNewGame(DungeonDifficulty.Normal),
            new Vector2(0.365f, 0.28f), new Vector2(0.635f, 0.46f), selected: true);
        CreateButton(panel, "DifficultyHardButton", "Hard", () => BeginNewGame(DungeonDifficulty.Hard),
            new Vector2(0.69f, 0.28f), new Vector2(0.96f, 0.46f));
        CreateButton(panel, "DifficultyCancelButton", "취소", () => difficultyModal.SetActive(false),
            new Vector2(0.365f, 0.07f), new Vector2(0.635f, 0.21f));
        difficultyModal.SetActive(false);
    }

    private void LoadSlot(string slotId)
    {
        if (!slotCatalog.HasSave(slotId))
        {
            SetStatus("선택한 저장 데이터가 없습니다.", true);
            RefreshSlots();
            return;
        }

        BeginLoad(slotId);
    }

    private void BeginLoad(string slotId)
    {
        if (!sceneNavigator.LoadGame(slotId))
        {
            SetStatus("이미 화면을 전환하고 있거나 Gameplay 씬을 찾을 수 없습니다.", true);
        }
    }

    private void DeleteSlot(string slotId)
    {
        if (!slotCatalog.HasSave(slotId))
        {
            RefreshSlots();
            return;
        }

        if (!Confirm("delete-" + slotId, "삭제하려면 같은 버튼을 한 번 더 누르세요."))
        {
            return;
        }

        slotCatalog.Delete(slotId);
        ClearConfirmation();
        RefreshSlots();
        SetStatus("저장 데이터를 삭제했습니다.", false);
    }

    private void RefreshSlots()
    {
        Dictionary<string, DungeonSaveSlotInfo> current = slotCatalog.GetSlots()
            .GroupBy(slot => slot.SlotId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        foreach (KeyValuePair<string, SlotView> pair in slots)
        {
            current.TryGetValue(pair.Key, out DungeonSaveSlotInfo info);
            bool valid = info != null && info.IsValid;
            pair.Value.Metadata.text = FormatMetadata(info);
            pair.Value.Load.interactable = valid;
            pair.Value.Delete.interactable = info != null;
        }

        if (continueButton != null)
        {
            continueButton.interactable = LatestValidSlot() != null;
        }
    }

    private DungeonSaveSlotInfo LatestValidSlot()
    {
        return slotCatalog.GetSlots()
            .Where(slot => slot.IsValid)
            .OrderByDescending(slot => ParseSavedAt(slot.SavedAtUtc))
            .FirstOrDefault();
    }

    private static string FormatMetadata(DungeonSaveSlotInfo info)
    {
        if (info == null)
        {
            return "비어 있음";
        }

        if (!info.IsValid)
        {
            return "읽을 수 없는 저장 데이터";
        }

        DateTime timestamp = ParseSavedAt(info.SavedAtUtc);
        string date = timestamp == DateTime.MinValue
            ? "저장 시각 없음"
            : timestamp.ToLocalTime().ToString("M월 d일 HH:mm", CultureInfo.CurrentCulture);
        return $"{date}\n{Mathf.Max(1, info.Day)}일차 · {Mathf.Max(0, info.Money):N0} 골드";
    }

    private static DateTime ParseSavedAt(string value)
    {
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsed)
            ? parsed
            : DateTime.MinValue;
    }

    private bool Confirm(string key, string message)
    {
        if (confirmationKey == key && Time.unscaledTime <= confirmationExpiresAt)
        {
            return true;
        }

        confirmationKey = key;
        confirmationExpiresAt = Time.unscaledTime + ConfirmationSeconds;
        SetStatus(message, true);
        return false;
    }

    private void ClearConfirmation()
    {
        confirmationKey = string.Empty;
        confirmationExpiresAt = 0f;
    }

    private void SetStatus(string message, bool warning)
    {
        if (statusText == null)
        {
            return;
        }

        statusText.text = message ?? string.Empty;
        statusText.color = warning ? DungeonUiTheme.Warning : DungeonUiTheme.TextSecondary;
    }

    private static void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private Button CreateButton(
        Transform parent,
        string name,
        string label,
        UnityEngine.Events.UnityAction action,
        Vector2 anchorMin,
        Vector2 anchorMax,
        bool selected = false,
        bool destructive = false)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        SetRect(buttonObject.GetComponent<RectTransform>(), anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(action);
        TMP_Text text = CreateText(buttonObject.transform, "Label", label, 19f, TextAlignmentOptions.Center);
        Stretch(text.rectTransform);
        DungeonUiTheme.StyleButton(button, selected, destructive);
        return button;
    }

    private TMP_Text CreateText(Transform parent, string name, string value, float size, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.text = value;
        text.fontSize = size;
        text.fontSizeMin = Mathf.Max(12f, size * 0.72f);
        text.fontSizeMax = size;
        text.enableAutoSizing = true;
        text.alignment = alignment;
        text.color = DungeonUiTheme.TextPrimary;
        text.raycastTarget = false;
        fontService.Apply(text);
        return text;
    }

    private static Transform CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, bool raised)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        SetRect(panel.GetComponent<RectTransform>(), anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        panel.GetComponent<Image>().color = raised ? DungeonUiTheme.Panel : DungeonUiTheme.Surface;
        return panel.transform;
    }

    private static Image CreateImage(Transform parent, string name, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static void Stretch(RectTransform rect)
    {
        SetRect(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        rect.localScale = Vector3.one;
    }

    private sealed class SlotView
    {
        public SlotView(TMP_Text metadata, Button load, Button delete)
        {
            Metadata = metadata;
            Load = load;
            Delete = delete;
        }

        public TMP_Text Metadata { get; }
        public Button Load { get; }
        public Button Delete { get; }
    }
}
