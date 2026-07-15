using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class P1FacilityShopAssetBuilder
{
    private const string BlueprintFolder = "Assets/Resources/SO/Blueprint/P1";

    private static readonly BlueprintSpec[] BlueprintSpecs =
    {
        new BlueprintSpec(
            6101,
            "BP_CommercialBasics",
            "상업 기초 설계도",
            "저가 음식점, 고기 식당, 잡화점을 기본 구매 후보로 여는 설계도.",
            FacilityShopRarity.Common,
            120,
            18f,
            new[] { "P1_LowFoodShop", "P1_MeatRestaurant", "P1_GeneralStore" }),
        new BlueprintSpec(
            6102,
            "BP_DefenseBasics",
            "방어 기초 설계도",
            "가시 함정과 경비실을 기본 구매 후보로 여는 설계도.",
            FacilityShopRarity.Common,
            140,
            22f,
            new[] { "P1_SpikeTrap", "P1_GuardRoom" }),
        new BlueprintSpec(
            6103,
            "BP_SupportBasics",
            "지원 기초 설계도",
            "창고, 훈련장, 연구실을 기본 구매 후보로 여는 설계도.",
            FacilityShopRarity.Common,
            130,
            20f,
            new[] { "P1_Warehouse", "P1_RestRoom", "P1_Toilet", "P1_Washroom", "P1_TrainingRoom", "P1_ResearchLab" }),
        new BlueprintSpec(
            6104,
            "BP_ArcaneBasics",
            "마력 기초 설계도",
            "마력 저장소를 기본 구매 후보로 여는 설계도.",
            FacilityShopRarity.Common,
            130,
            24f,
            new[] { "P1_ManaStorage" }),
        new BlueprintSpec(
            6191,
            "BP_BattleDining",
            "전장의 식당 설계도",
            "전투 식당과 병영을 잇는 조합식 힌트.",
            FacilityShopRarity.Rare,
            260,
            45f,
            Array.Empty<string>(),
            new[] { "recipe_battlefield_dining_2" }),
        new BlueprintSpec(
            6192,
            "BP_TrapChain",
            "연쇄 함정 설계도",
            "번개와 냉기 방어 시설을 잇는 특수 조합식 힌트.",
            FacilityShopRarity.Rare,
            280,
            50f,
            Array.Empty<string>(),
            new[] { "recipe_trap_chain_2" }),
        new BlueprintSpec(
            6193,
            "BP_StormFireTrap",
            "폭뢰 분사구 설계도",
            "화염과 축전 방어 시설을 잇는 특수 조합식 힌트.",
            FacilityShopRarity.Rare,
            340,
            60f,
            Array.Empty<string>(),
            new[] { "recipe_trap_chain_3" })
    };

    [MenuItem("DungeonStory/Debug/Facility Shop/Ensure P1 Facility Shop Assets")]
    public static void EnsureP1FacilityShopAssets()
    {
        EnsureFolder(BlueprintFolder);

        foreach (BlueprintSpec spec in BlueprintSpecs)
        {
            FacilityBlueprintSO blueprint = LoadOrCreateBlueprint(spec);
            blueprint.id = spec.Id;
            blueprint.blueprintName = spec.DisplayName;
            blueprint.description = spec.Description;
            blueprint.rarity = spec.Rarity;
            blueprint.defaultCost = spec.Cost;
            blueprint.researchWorkRequired = spec.ResearchWorkRequired;
            blueprint.unlockBasicPurchaseBuildingIds = ResolveBuildingIds(spec.BasicPurchaseBuildingAssetNames);
            blueprint.unlockBuildingIds = Array.Empty<int>();
            blueprint.unlockRecipeIds = spec.UnlockRecipeIds ?? Array.Empty<string>();
            EditorUtility.SetDirty(blueprint);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static FacilityBlueprintSO LoadOrCreateBlueprint(BlueprintSpec spec)
    {
        string path = $"{BlueprintFolder}/{spec.AssetName}.asset";
        FacilityBlueprintSO blueprint = AssetDatabase.LoadAssetAtPath<FacilityBlueprintSO>(path);
        if (blueprint != null)
        {
            return blueprint;
        }

        blueprint = ScriptableObject.CreateInstance<FacilityBlueprintSO>();
        AssetDatabase.CreateAsset(blueprint, path);
        return blueprint;
    }

    private static int[] ResolveBuildingIds(IEnumerable<string> assetNames)
    {
        return assetNames?
            .Select(LoadBuilding)
            .Where((building) => building != null)
            .Select((building) => building.id)
            .Distinct()
            .ToArray()
            ?? Array.Empty<int>();
    }

    private static BuildingSO LoadBuilding(string assetName)
    {
        if (string.IsNullOrWhiteSpace(assetName))
        {
            return null;
        }

        return AssetDatabase.LoadAssetAtPath<BuildingSO>($"Assets/Resources/SO/Building/P1/{assetName}.asset");
    }

    private static void EnsureFolder(string folder)
    {
        string[] parts = folder.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }

    private readonly struct BlueprintSpec
    {
        public readonly int Id;
        public readonly string AssetName;
        public readonly string DisplayName;
        public readonly string Description;
        public readonly FacilityShopRarity Rarity;
        public readonly int Cost;
        public readonly float ResearchWorkRequired;
        public readonly string[] BasicPurchaseBuildingAssetNames;
        public readonly string[] UnlockRecipeIds;

        public BlueprintSpec(
            int id,
            string assetName,
            string displayName,
            string description,
            FacilityShopRarity rarity,
            int cost,
            float researchWorkRequired,
            string[] basicPurchaseBuildingAssetNames,
            string[] unlockRecipeIds = null)
        {
            Id = id;
            AssetName = assetName;
            DisplayName = displayName;
            Description = description;
            Rarity = rarity;
            Cost = Mathf.Max(0, cost);
            ResearchWorkRequired = Mathf.Max(1f, researchWorkRequired);
            BasicPurchaseBuildingAssetNames = basicPurchaseBuildingAssetNames ?? Array.Empty<string>();
            UnlockRecipeIds = unlockRecipeIds ?? Array.Empty<string>();
        }
    }
}
