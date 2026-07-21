using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class UIBuildingInfo : SerializedMonoBehaviour, UtilEventListener<InfoFeedEvent>
{
    private BuildableObject selectedBuilding;
    private CanvasGroup canvasGroup;
    private Image buildingImage;
    private RectTransform buildingImageSize;
    private IBuildingDefinitionLookup buildingDefinitionLookup;
    private IBuildingSummaryFormatter summaryFormatter;
    private IUiPopupService popupService;
    private bool initialized;
    private readonly List<GameObject> craftActionObjects = new List<GameObject>();
    private string craftStatusMessage = string.Empty;

    public GameObject buildingImageObject;

    public List<UIConfig<TMP_Text>> simpleInfoText;
    public TMP_Text nameText;

    public GameObject textPrefab;
    public GameObject simpleInfoPanel;

    private bool hidden = true;

    [Inject]
    public void ConstructUIBuildingInfo(
        IBuildingDefinitionLookup buildingDefinitionLookup,
        IBuildingSummaryFormatter summaryFormatter,
        IUiPopupService popupService)
    {
        this.buildingDefinitionLookup = buildingDefinitionLookup
            ?? throw new ArgumentNullException(nameof(buildingDefinitionLookup));
        this.summaryFormatter = summaryFormatter
            ?? throw new ArgumentNullException(nameof(summaryFormatter));
        this.popupService = popupService
            ?? throw new ArgumentNullException(nameof(popupService));
    }

    private void Awake()
    {
        EnsureInitialized();
    }

    private void Start()
    {
        EnsureInitialized();
        if (hidden)
        {
            SetHiddenImmediate();
        }
    }

    private void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (buildingImageObject != null)
        {
            buildingImage = buildingImageObject.GetComponent<Image>();
            buildingImageSize = buildingImageObject.GetComponent<RectTransform>();
        }

        initialized = true;
    }

    public void DisplayBuildingInfo(BuildableObject building)
    {
        EnsureInitialized();

        if (building == null)
        {
            throw new ArgumentNullException(nameof(building));
        }

        if (building != selectedBuilding && !hidden) return;
        selectedBuilding = building;
        BuildingSO buildingData = ResolveBuildingLookup().GetBuilding(building.id) ?? building.BuildingData;
        if (buildingData == null)
        {
            return;
        }

        ResolvePopupService().CloseAll();
        OpenDispaly();
        if (buildingImageObject != null)
        {
            Vector2 size = new Vector2(Mathf.Max(40f, (buildingData.width / 3f) * 160f), 160f);
            if (buildingImageSize != null)
            {
                buildingImageSize.sizeDelta = size;
            }
            if (buildingImage != null)
            {
                ApplyBuildingPreview(buildingData.icon);
            }
        }
        if (nameText != null)
        {
            nameText.text = buildingData.objectName;
        }

        BuildingSummaryPresentation presentation = ResolveSummaryFormatter().Format(building);
        IReadOnlyList<string> details = presentation.DetailLines;
        List<UIConfig<TMP_Text>> detailViews = simpleInfoText ?? new List<UIConfig<TMP_Text>>();
        for (int index = 0; index < detailViews.Count; index++)
        {
            UIConfig<TMP_Text> ui = detailViews[index];
            if (ui?.uiObject == null) continue;

            bool visible = index < details.Count;
            ui.uiObject.gameObject.SetActive(visible);
            if (visible)
            {
                ui.uiObject.text = details[index];
                ui.uiObject.color = DungeonUiTheme.TextPrimary;
                ui.uiObject.fontSize = 24f;
                ui.uiObject.enableAutoSizing = true;
                ui.uiObject.fontSizeMin = 14f;
                ui.uiObject.fontSizeMax = 24f;
            }
        }

        RenderCraftActions(buildingData, building);
    }

    private void ApplyBuildingPreview(Sprite sprite)
    {
        buildingImage.sprite = sprite;
        buildingImage.color = Color.white;
        buildingImage.material = null;
        buildingImage.type = Image.Type.Simple;
        buildingImage.preserveAspect = true;
        buildingImage.raycastTarget = false;
    }

    public void OpenDispaly()
    {
        EnsureInitialized();
        hidden = false;
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        ResolvePopupService().BlockTouch();
        canvasGroup.DOKill();
        canvasGroup.DOFade(1.0f, 0.1f).SetUpdate(true);
    }
    public void CloseDispaly()
    {
        EnsureInitialized();
        hidden = true;
        selectedBuilding = null;
        ClearCraftActions();

        if (!gameObject.activeInHierarchy)
        {
            SetHiddenImmediate();
            ResolvePopupService().ReleaseTouch();
            return;
        }

        canvasGroup.DOKill();
        canvasGroup.DOFade(0f, 0.1f).SetUpdate(true).OnComplete(() =>
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
            ResolvePopupService().ReleaseTouch();
        });
    }

    public void OnTriggerEvent(InfoFeedEvent eventType)
    {
        if (eventType.Target is not CharacterActor actor || actor == null)
        {
            return;
        }

        CloseDisplayImmediate();
    }

    private void OnEnable()
    {
        this.EventStartListening<InfoFeedEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<InfoFeedEvent>();
    }

    private void CloseDisplayImmediate()
    {
        EnsureInitialized();
        hidden = true;
        selectedBuilding = null;
        ClearCraftActions();
        SetHiddenImmediate();
        ResolvePopupService().ReleaseTouch();
        gameObject.SetActive(false);
    }

    private void RenderCraftActions(BuildingSO buildingData, BuildableObject building)
    {
        ClearCraftActions();
        BuildingEquipmentCraftingAbility crafting = buildingData
            ?.GetAbility<BuildingEquipmentCraftingAbility>();
        if (crafting == null
            || simpleInfoPanel == null
            || !building.TryGetExpeditionEquipmentRuntime(out IExpeditionEquipmentRuntime runtime))
        {
            craftStatusMessage = string.Empty;
            return;
        }

        HashSet<string> craftableIds = new HashSet<string>(
            crafting.CraftableEquipmentIds.Where(id => !string.IsNullOrWhiteSpace(id)),
            StringComparer.Ordinal);
        foreach (ExpeditionEquipmentDefinition definition in runtime.Definitions
            .Where(definition => definition != null && craftableIds.Contains(definition.id))
            .OrderBy(definition => definition.slot)
            .ThenBy(definition => definition.displayName, StringComparer.Ordinal))
        {
            ExpeditionEquipmentDefinition captured = definition;
            GameObject buttonObject = CreateCraftButton(
                simpleInfoPanel.transform,
                $"제작 {definition.displayName}",
                () =>
                {
                    craftStatusMessage = runtime.TryQueueCraft(captured.id, building, out string message)
                        ? $"{captured.displayName} 제작 예약"
                        : FormatCraftMessage(message);
                    DisplayBuildingInfo(building);
                });
            buttonObject.name = $"BuildingCraft_{SanitizeObjectName(definition.id)}";
            craftActionObjects.Add(buttonObject);
        }

        if (!string.IsNullOrWhiteSpace(craftStatusMessage))
        {
            GameObject statusObject = CreateCraftStatus(simpleInfoPanel.transform, craftStatusMessage);
            craftActionObjects.Add(statusObject);
        }
    }

    private GameObject CreateCraftButton(Transform parent, string label, Action callback)
    {
        GameObject buttonObject = new GameObject("BuildingCraftButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.GetComponent<Image>();
        image.color = DungeonUiTheme.Accent;

        LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
        layout.preferredWidth = 180f;
        layout.preferredHeight = 46f;

        Button button = buttonObject.GetComponent<Button>();
        DungeonUiTheme.StyleButton(button, selected: true);
        button.onClick.AddListener(() => callback?.Invoke());

        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 4f);
        textRect.offsetMax = new Vector2(-8f, -4f);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.text = label;
        text.color = DungeonUiTheme.TextPrimary;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = 13f;
        text.fontSizeMax = 20f;
        text.textWrappingMode = TextWrappingModes.Normal;
        if (nameText != null && nameText.font != null)
        {
            text.font = nameText.font;
        }

        return buttonObject;
    }

    private GameObject CreateCraftStatus(Transform parent, string message)
    {
        GameObject statusObject = new GameObject("BuildingCraftStatus", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        statusObject.transform.SetParent(parent, false);
        LayoutElement layout = statusObject.GetComponent<LayoutElement>();
        layout.preferredWidth = 360f;
        layout.preferredHeight = 46f;

        TMP_Text text = statusObject.GetComponent<TMP_Text>();
        text.text = message;
        text.color = DungeonUiTheme.TextSecondary;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = 12f;
        text.fontSizeMax = 18f;
        text.textWrappingMode = TextWrappingModes.Normal;
        if (nameText != null && nameText.font != null)
        {
            text.font = nameText.font;
        }

        return statusObject;
    }

    private void ClearCraftActions()
    {
        foreach (GameObject item in craftActionObjects)
        {
            if (item == null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(item);
            }
            else
            {
                DestroyImmediate(item);
            }
        }

        craftActionObjects.Clear();
    }

    private static string FormatCraftMessage(string message)
    {
        return message switch
        {
            "craft-cost-not-available" => "재료 부족",
            "craft-cost-withdraw-failed" => "재료 출고 실패",
            "equipment-not-found" => "장비 정의 없음",
            _ => string.IsNullOrWhiteSpace(message) ? "제작 실패" : message
        };
    }

    private static string SanitizeObjectName(string value)
    {
        string normalized = string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim();
        char[] chars = normalized
            .Select(character => char.IsLetterOrDigit(character) ? character : '_')
            .ToArray();
        return new string(chars);
    }

    private void SetHiddenImmediate()
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.DOKill();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private IBuildingDefinitionLookup ResolveBuildingLookup()
    {
        return buildingDefinitionLookup
            ?? throw new InvalidOperationException($"{nameof(UIBuildingInfo)} requires {nameof(IBuildingDefinitionLookup)} injection.");
    }

    private IUiPopupService ResolvePopupService()
    {
        return popupService
            ?? throw new InvalidOperationException($"{nameof(UIBuildingInfo)} requires {nameof(IUiPopupService)} injection.");
    }

    private IBuildingSummaryFormatter ResolveSummaryFormatter()
    {
        return summaryFormatter
            ?? throw new InvalidOperationException($"{nameof(UIBuildingInfo)} requires {nameof(IBuildingSummaryFormatter)} injection.");
    }
}
[Serializable]
public class UIConfig<T>
{
    public string name;
    public T uiObject;
}
