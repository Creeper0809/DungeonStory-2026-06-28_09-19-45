using System;
using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/Facility Shop/Blueprint", order = 0)]
public class FacilityBlueprintSO : DataScriptableObject
{
    public string blueprintName;
    [TextArea] public string description;
    public FacilityShopRarity rarity = FacilityShopRarity.Common;
    [Min(0)] public int defaultCost = 120;
    [Min(1f)] public float researchWorkRequired = 20f;
    public int[] unlockBuildingIds = Array.Empty<int>();
    public int[] unlockBasicPurchaseBuildingIds = Array.Empty<int>();
    public string[] unlockRecipeIds = Array.Empty<string>();

    public string DisplayName => string.IsNullOrWhiteSpace(blueprintName) ? name : blueprintName;
}
