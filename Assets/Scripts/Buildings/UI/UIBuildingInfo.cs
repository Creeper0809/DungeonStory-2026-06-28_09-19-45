using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
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
        BuildingSO buildingData = ResolveBuildingLookup().GetBuilding(building.id);
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

        canvasGroup.DOKill();
        canvasGroup.DOFade(1.0f, 0.1f).SetUpdate(true).OnComplete(() =>
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            ResolvePopupService().BlockTouch();
        });
    }
    public void CloseDispaly()
    {
        EnsureInitialized();
        hidden = true;
        selectedBuilding = null;

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
        if (eventType.infoable == null
            || eventType.infoable.GetInfoType() != InfoFeedEvent.Type.CHARACTER)
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
        SetHiddenImmediate();
        ResolvePopupService().ReleaseTouch();
        gameObject.SetActive(false);
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
