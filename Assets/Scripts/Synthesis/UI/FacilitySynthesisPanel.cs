using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using VContainer;

public class FacilitySynthesisPanel :
    MonoBehaviour,
    UtilEventListener<FacilitySynthesisSelectionChangedEvent>,
    UtilEventListener<BlueprintResearchCompletedEvent>,
    UtilEventListener<FacilitySynthesisCompletedEvent>
{
    [SerializeField] private FacilitySynthesisRuntime runtime;
    [SerializeField] private TMP_Text summaryText;
    private IFacilitySynthesisRuntimeProvider runtimeProvider;

    public string LastRenderedText { get; private set; } = string.Empty;

    [Inject]
    public void Construct(IFacilitySynthesisRuntimeProvider runtimeProvider)
    {
        this.runtimeProvider = runtimeProvider
            ?? throw new System.ArgumentNullException(nameof(runtimeProvider));
    }

    public void Bind(FacilitySynthesisRuntime nextRuntime)
    {
        runtime = nextRuntime;
        Refresh();
    }

    internal void BindGeneratedView(TMP_Text summaryText)
    {
        this.summaryText = summaryText
            ?? throw new System.ArgumentNullException(nameof(summaryText));
        ApplyText();
    }

    public void Refresh()
    {
        FacilitySynthesisRuntime activeRuntime = ResolveRuntime();
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
                FacilitySynthesisRecipeSnapshot snapshot = activeRuntime.ToSnapshot(recipe);
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

    private FacilitySynthesisRuntime ResolveRuntime()
    {
        if (runtime != null) return runtime;

        return (runtimeProvider
                ?? throw new System.InvalidOperationException($"{nameof(FacilitySynthesisPanel)} requires {nameof(IFacilitySynthesisRuntimeProvider)} injection or an explicit runtime binding."))
            .Runtime;
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
