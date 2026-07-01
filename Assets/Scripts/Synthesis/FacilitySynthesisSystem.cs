using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class FacilitySynthesisRecipeSnapshot
{
    public string recipeId;
    public string displayName;
    public string resultName;
    public string[] materialNames = Array.Empty<string>();
    public bool special;
    public bool visible;

    public string ToSummaryText()
    {
        string materials = materialNames != null && materialNames.Length > 0
            ? string.Join(" + ", materialNames)
            : "재료 없음";
        string specialText = special ? " / 특수" : string.Empty;
        return $"{materials} -> {resultName}{specialText}";
    }
}

public readonly struct FacilitySynthesisResult
{
    public FacilitySynthesisResult(
        bool success,
        FacilitySynthesisRecipeSO recipe,
        BuildableObject resultBuilding,
        int inheritedLevel,
        string message)
    {
        Success = success;
        Recipe = recipe;
        ResultBuilding = resultBuilding;
        InheritedLevel = Mathf.Max(1, inheritedLevel);
        Message = message ?? string.Empty;
    }

    public bool Success { get; }
    public FacilitySynthesisRecipeSO Recipe { get; }
    public BuildableObject ResultBuilding { get; }
    public int InheritedLevel { get; }
    public string Message { get; }
}

public struct FacilitySynthesisCompletedEvent
{
    public FacilitySynthesisResult result;

    public FacilitySynthesisCompletedEvent(FacilitySynthesisResult result)
    {
        this.result = result;
    }

    private static FacilitySynthesisCompletedEvent e;

    public static void Trigger(FacilitySynthesisResult result)
    {
        e.result = result;
        EventObserver.TriggerEvent(e);
    }
}

public struct FacilitySynthesisSelectionChangedEvent
{
    public IReadOnlyList<BuildableObject> selectedMaterials;

    public FacilitySynthesisSelectionChangedEvent(IReadOnlyList<BuildableObject> selectedMaterials)
    {
        this.selectedMaterials = selectedMaterials ?? Array.Empty<BuildableObject>();
    }

    private static FacilitySynthesisSelectionChangedEvent e;

    public static void Trigger(IReadOnlyList<BuildableObject> selectedMaterials)
    {
        e.selectedMaterials = selectedMaterials ?? Array.Empty<BuildableObject>();
        EventObserver.TriggerEvent(e);
    }
}

public static class FacilitySynthesisService
{
    public static IReadOnlyList<FacilitySynthesisRecipeSO> LoadAllRecipes()
    {
        return Resources.LoadAll<FacilitySynthesisRecipeSO>("SO/Synthesis")
            .Where((recipe) => recipe != null && recipe.HasValidData)
            .OrderBy((recipe) => recipe.id)
            .ToList();
    }

    public static bool IsRecipeVisible(FacilitySynthesisRecipeSO recipe, BlueprintResearchState researchState)
    {
        if (recipe == null || !recipe.HasValidData)
        {
            return false;
        }

        if (recipe.publicByDefault && string.IsNullOrWhiteSpace(recipe.requiredResearchRecipeId))
        {
            return true;
        }

        if (MetaProgressionRuntime.Instance != null
            && (MetaProgressionRuntime.Instance.IsRecipePreserved(recipe.requiredResearchRecipeId)
                || MetaProgressionRuntime.Instance.IsRecipePreserved(recipe.recipeId)))
        {
            return true;
        }

        return researchState != null
            && !string.IsNullOrWhiteSpace(recipe.requiredResearchRecipeId)
            && researchState.UnlockedRecipeIds.Contains(recipe.requiredResearchRecipeId);
    }

    public static IReadOnlyList<FacilitySynthesisRecipeSO> GetVisibleRecipes(BlueprintResearchState researchState)
    {
        return LoadAllRecipes()
            .Where((recipe) => IsRecipeVisible(recipe, researchState))
            .ToList();
    }

    public static FacilitySynthesisRecipeSnapshot ToSnapshot(
        FacilitySynthesisRecipeSO recipe,
        BlueprintResearchState researchState)
    {
        if (recipe == null)
        {
            return null;
        }

        return new FacilitySynthesisRecipeSnapshot
        {
            recipeId = recipe.recipeId,
            displayName = recipe.DisplayName,
            resultName = FacilityShopService.GetBuildingName(recipe.resultBuilding),
            materialNames = recipe.materialBuildings?
                .Where((building) => building != null)
                .Select(FacilityShopService.GetBuildingName)
                .ToArray()
                ?? Array.Empty<string>(),
            special = recipe.IsSpecial,
            visible = IsRecipeVisible(recipe, researchState)
        };
    }

    public static bool MatchesMaterials(FacilitySynthesisRecipeSO recipe, IReadOnlyList<BuildableObject> materials)
    {
        if (recipe == null || !recipe.HasValidData || materials == null)
        {
            return false;
        }

        List<int> required = recipe.MaterialBuildingIds.OrderBy((id) => id).ToList();
        List<int> provided = materials
            .Where((building) => building != null)
            .Select((building) => building.id)
            .OrderBy((id) => id)
            .ToList();

        return required.SequenceEqual(provided);
    }

    public static int CalculateInheritedLevel(
        FacilitySynthesisRecipeSO recipe,
        IReadOnlyList<BuildableObject> materials)
    {
        if (materials == null || materials.Count == 0)
        {
            return 1;
        }

        float averageLevel = (float)materials.Average((building) => building != null ? building.FacilityLevel : 1);
        float inheritanceRatio = recipe != null ? Mathf.Clamp01(recipe.levelInheritanceRatio) : 0.75f;
        return Mathf.Max(1, Mathf.RoundToInt(averageLevel * inheritanceRatio));
    }
}

public class FacilitySynthesisRuntime : MonoBehaviour
{
    private readonly List<BuildableObject> selectedMaterials = new List<BuildableObject>();
    private GridBuildingFactory buildingFactory;

    public static FacilitySynthesisRuntime Instance => FindFirstObjectByType<FacilitySynthesisRuntime>();
    public IReadOnlyList<BuildableObject> SelectedMaterials => selectedMaterials;

    public BlueprintResearchState ResearchState
    {
        get
        {
            BlueprintResearchRuntime researchRuntime = BlueprintResearchRuntime.Instance;
            return researchRuntime != null ? researchRuntime.State : null;
        }
    }

    public IReadOnlyList<FacilitySynthesisRecipeSO> VisibleRecipes => FacilitySynthesisService.GetVisibleRecipes(ResearchState);

    private GridBuildingFactory BuildingFactory => buildingFactory ??= new GridBuildingFactory(GridTexture.Instance);

    public void ToggleMaterialSelection(BuildableObject building)
    {
        if (building == null || building.isDestroy)
        {
            return;
        }

        if (selectedMaterials.Contains(building))
        {
            selectedMaterials.Remove(building);
        }
        else
        {
            selectedMaterials.Add(building);
        }

        FacilitySynthesisSelectionChangedEvent.Trigger(selectedMaterials);
    }

    public void ClearSelection()
    {
        selectedMaterials.Clear();
        FacilitySynthesisSelectionChangedEvent.Trigger(selectedMaterials);
    }

    public bool TrySynthesizeSelected(FacilitySynthesisRecipeSO recipe, out FacilitySynthesisResult result)
    {
        bool success = TrySynthesize(recipe, selectedMaterials, out result);
        if (success)
        {
            ClearSelection();
        }

        return success;
    }

    public bool TrySynthesizeSelected(string recipeId, out FacilitySynthesisResult result)
    {
        FacilitySynthesisRecipeSO recipe = VisibleRecipes.FirstOrDefault((candidate) => candidate.recipeId == recipeId);
        return TrySynthesizeSelected(recipe, out result);
    }

    public bool TrySynthesize(
        FacilitySynthesisRecipeSO recipe,
        IReadOnlyList<BuildableObject> materials,
        out FacilitySynthesisResult result)
    {
        result = new FacilitySynthesisResult(false, recipe, null, 1, "합성할 수 없습니다");

        if (!Validate(recipe, materials, out string errorMessage))
        {
            result = new FacilitySynthesisResult(false, recipe, null, 1, errorMessage);
            return false;
        }

        BuildableObject primary = materials[0];
        Grid grid = primary.Grid;
        Vector2Int resultPosition = primary.centerPos;
        int inheritedLevel = FacilitySynthesisService.CalculateInheritedLevel(recipe, materials);

        foreach (BuildableObject material in materials)
        {
            RemoveMaterialFromGrid(material);
        }

        BuildableObject resultBuilding = BuildingFactory.Create(grid, recipe.resultBuilding, resultPosition);
        if (resultBuilding == null)
        {
            result = new FacilitySynthesisResult(false, recipe, null, inheritedLevel, "결과 시설 생성 실패");
            return false;
        }

        resultBuilding.SetGrid(grid);
        resultBuilding.Initialization(recipe.resultBuilding, resultPosition);
        resultBuilding.SetFacilityLevel(inheritedLevel);
        bool registered = grid.RegisterOccupant(
            resultBuilding,
            recipe.resultBuilding.Placement.Layer,
            recipe.resultBuilding.GetGridPosList(resultPosition),
            recipe.resultBuilding.Placement.IsMovement);
        if (!registered)
        {
            resultBuilding.DestroySelf();
            result = new FacilitySynthesisResult(false, recipe, null, inheritedLevel, "결과 시설 배치 실패");
            return false;
        }

        result = new FacilitySynthesisResult(
            true,
            recipe,
            resultBuilding,
            inheritedLevel,
            $"{recipe.DisplayName} 합성 완료");
        FacilitySynthesisCompletedEvent.Trigger(result);
        EventAlertService.Raise(
            "시설 합성 완료",
            $"{recipe.DisplayName}: {FacilityShopService.GetBuildingName(recipe.resultBuilding)} Lv.{inheritedLevel}",
            EventAlertImportance.Medium,
            "합성");
        return true;
    }

    private bool Validate(
        FacilitySynthesisRecipeSO recipe,
        IReadOnlyList<BuildableObject> materials,
        out string errorMessage)
    {
        if (recipe == null || !recipe.HasValidData)
        {
            errorMessage = "조합식 정보가 올바르지 않습니다";
            return false;
        }

        if (!FacilitySynthesisService.IsRecipeVisible(recipe, ResearchState))
        {
            errorMessage = "아직 해금되지 않은 조합식입니다";
            return false;
        }

        if (materials == null || materials.Count == 0)
        {
            errorMessage = "합성 재료 시설을 선택해야 합니다";
            return false;
        }

        if (materials.Any((building) => building == null || building.isDestroy))
        {
            errorMessage = "사용할 수 없는 재료 시설이 있습니다";
            return false;
        }

        if (materials.Any((building) => building.IsDamaged))
        {
            errorMessage = "파손 시설은 수리 전까지 합성할 수 없습니다";
            return false;
        }

        if (materials.Select((building) => building).Distinct().Count() != materials.Count)
        {
            errorMessage = "같은 시설을 중복 재료로 사용할 수 없습니다";
            return false;
        }

        if (!FacilitySynthesisService.MatchesMaterials(recipe, materials))
        {
            errorMessage = "조합식과 재료 시설이 맞지 않습니다";
            return false;
        }

        Grid grid = materials[0].Grid;
        if (grid == null || materials.Any((building) => building.Grid != grid))
        {
            errorMessage = "같은 그리드의 시설만 합성할 수 있습니다";
            return false;
        }

        if (!CanPlaceResultOverMaterials(grid, recipe, materials))
        {
            errorMessage = "결과 시설을 배치할 공간이 부족합니다";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private static bool CanPlaceResultOverMaterials(
        Grid grid,
        FacilitySynthesisRecipeSO recipe,
        IReadOnlyList<BuildableObject> materials)
    {
        HashSet<IGridOccupant> materialSet = materials.Cast<IGridOccupant>().ToHashSet();
        foreach (Vector2Int pos in recipe.resultBuilding.GetGridPosList(materials[0].centerPos))
        {
            if (!grid.IsValidGridPos(pos))
            {
                return false;
            }

            GridCell cell = grid.GetGridCell(pos);
            IGridOccupant occupant = cell?.GetOccupant(recipe.resultBuilding.Placement.Layer);
            if (occupant != null && !materialSet.Contains(occupant))
            {
                return false;
            }
        }

        return true;
    }

    private void RemoveMaterialFromGrid(BuildableObject material)
    {
        if (material == null || material.BuildingData == null || material.Grid == null)
        {
            return;
        }

        material.Grid.RemoveOccupant(
            material.BuildingData.Placement.Layer,
            material.buildPoses,
            material.BuildingData.Placement.IsMovement);
        BuildingFactory.DeleteVisual(material.BuildingData, material.centerPos);
        material.DestroySelf();
    }
}
