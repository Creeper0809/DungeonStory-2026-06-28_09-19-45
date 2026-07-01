using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FacilitySynthesisPanel :
    MonoBehaviour,
    UtilEventListener<FacilitySynthesisSelectionChangedEvent>,
    UtilEventListener<BlueprintResearchCompletedEvent>,
    UtilEventListener<FacilitySynthesisCompletedEvent>
{
    [SerializeField] private FacilitySynthesisRuntime runtime;
    [SerializeField] private TMP_Text summaryText;

    public string LastRenderedText { get; private set; } = string.Empty;

    public static FacilitySynthesisPanel CreateDefaultPanel(FacilitySynthesisRuntime runtime)
    {
        GameObject canvasObject = new GameObject("FacilitySynthesisCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject panelObject = new GameObject("FacilitySynthesisPanel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);
        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.anchoredPosition = new Vector2(-24f, 0f);
        rect.sizeDelta = new Vector2(420f, 520f);
        panelObject.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.1f, 0.88f);

        GameObject textObject = new GameObject("Summary", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panelObject.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = new Vector2(18f, 18f);
        textRect.offsetMax = new Vector2(-18f, -18f);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        TMPKoreanFont.Apply(text);
        text.fontSize = 22f;
        text.color = Color.white;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.alignment = TextAlignmentOptions.TopLeft;

        FacilitySynthesisPanel panel = panelObject.AddComponent<FacilitySynthesisPanel>();
        panel.Bind(runtime);
        panel.summaryText = text;
        panel.Refresh();
        return panel;
    }

    public void Bind(FacilitySynthesisRuntime nextRuntime)
    {
        runtime = nextRuntime;
        Refresh();
    }

    public void Refresh()
    {
        FacilitySynthesisRuntime activeRuntime = runtime != null ? runtime : FacilitySynthesisRuntime.Instance;
        if (activeRuntime == null)
        {
            LastRenderedText = "시설 합성\n런타임 없음";
            ApplyText();
            return;
        }

        List<string> lines = new List<string>
        {
            "시설 합성",
            string.Empty,
            "선택 재료:"
        };

        IReadOnlyList<BuildableObject> selected = activeRuntime.SelectedMaterials;
        if (selected == null || selected.Count == 0)
        {
            lines.Add("- 없음");
        }
        else
        {
            lines.AddRange(selected
                .Where((building) => building != null)
                .Select((building) => $"- {FacilityShopService.GetBuildingName(building.BuildingData)} Lv.{building.FacilityLevel}"));
        }

        lines.Add(string.Empty);
        lines.Add("조합식:");
        IReadOnlyList<FacilitySynthesisRecipeSO> recipes = activeRuntime.VisibleRecipes;
        if (recipes == null || recipes.Count == 0)
        {
            lines.Add("- 없음");
        }
        else
        {
            lines.AddRange(recipes.Select((recipe) =>
            {
                FacilitySynthesisRecipeSnapshot snapshot = FacilitySynthesisService.ToSnapshot(recipe, activeRuntime.ResearchState);
                return snapshot != null ? $"- {snapshot.ToSummaryText()}" : "- 조합식 오류";
            }));
        }

        LastRenderedText = string.Join("\n", lines);
        ApplyText();
    }

    public void OnTriggerEvent(FacilitySynthesisSelectionChangedEvent eventType)
    {
        Refresh();
    }

    public void OnTriggerEvent(BlueprintResearchCompletedEvent eventType)
    {
        Refresh();
    }

    public void OnTriggerEvent(FacilitySynthesisCompletedEvent eventType)
    {
        Refresh();
    }

    private void ApplyText()
    {
        if (summaryText != null)
        {
            summaryText.text = LastRenderedText;
        }
    }

    private void OnEnable()
    {
        this.EventStartListening<FacilitySynthesisSelectionChangedEvent>();
        this.EventStartListening<BlueprintResearchCompletedEvent>();
        this.EventStartListening<FacilitySynthesisCompletedEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<FacilitySynthesisSelectionChangedEvent>();
        this.EventStopListening<BlueprintResearchCompletedEvent>();
        this.EventStopListening<FacilitySynthesisCompletedEvent>();
    }
}
