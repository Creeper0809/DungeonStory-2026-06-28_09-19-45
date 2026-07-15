using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public interface IRunResultPanelFactory
{
    RunResultPanel CreateDefaultPanel();
}

public sealed class RunResultPanelFactory : IRunResultPanelFactory
{
    private readonly ITmpKoreanFontService tmpKoreanFontService;
    private readonly IObjectResolver objectResolver;

    public RunResultPanelFactory(
        ITmpKoreanFontService tmpKoreanFontService,
        IObjectResolver objectResolver)
    {
        this.tmpKoreanFontService = tmpKoreanFontService
            ?? throw new System.ArgumentNullException(nameof(tmpKoreanFontService));
        this.objectResolver = objectResolver
            ?? throw new System.ArgumentNullException(nameof(objectResolver));
    }

    public RunResultPanel CreateDefaultPanel()
    {
        GameObject canvasObject = new GameObject("RunResultCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject panelObject = new GameObject("RunResultPanel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);
        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.28f, 0.14f);
        rect.anchorMax = new Vector2(0.72f, 0.86f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = panelObject.GetComponent<Image>();
        image.color = DungeonUiTheme.Panel;

        GameObject textObject = new GameObject("RunResultText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panelObject.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = new Vector2(28f, 28f);
        textRect.offsetMax = new Vector2(-28f, -28f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        tmpKoreanFontService.Apply(text);
        text.fontSize = 24f;
        text.color = DungeonUiTheme.TextPrimary;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.textWrappingMode = TextWrappingModes.Normal;

        RunResultPanel panel = panelObject.AddComponent<RunResultPanel>();
        objectResolver.Inject(panel);
        return panel;
    }
}
