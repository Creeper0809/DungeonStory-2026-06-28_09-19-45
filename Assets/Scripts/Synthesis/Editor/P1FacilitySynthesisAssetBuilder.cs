using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class P1FacilitySynthesisAssetBuilder
{
    private const string BuildingFolder = "Assets/Resources/SO/Building/P1";
    private const string RecipeFolder = "Assets/Resources/SO/Synthesis/P1";
    private const string EffectFolder = "Assets/Resources/SO/Defense/Effects/P1/Synthesis";
    private const string StockFolder = "Assets/Resources/SO/Stock/P1";

    [MenuItem("DungeonStory/Debug/Synthesis/Ensure P1 Synthesis Assets")]
    public static void EnsureP1SynthesisAssetsFromMenu()
    {
        EnsureP1SynthesisAssets();
    }

    public static void EnsureP1SynthesisAssets()
    {
        AssetDatabase.Refresh();
        P1DefenseFacilityAssetBuilder.EnsureP1DefenseAssets();

        EnsureFolder(RecipeFolder);
        EnsureFolder(EffectFolder);
        EnsureFolder(StockFolder);

        foreach (SynthesisBuildingSpec spec in CreateBuildingSpecs())
        {
            EnsureBuildingAsset(spec);
        }

        foreach (RecipeSpec spec in CreateRecipeSpecs())
        {
            EnsureRecipeAsset(spec);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsureBuildingAsset(SynthesisBuildingSpec spec)
    {
        string assetPath = $"{BuildingFolder}/{spec.assetName}.asset";
        BuildingSO building = AssetDatabase.LoadAssetAtPath<BuildingSO>(assetPath);
        if (building == null)
        {
            building = ScriptableObject.CreateInstance<BuildingSO>();
            AssetDatabase.CreateAsset(building, assetPath);
        }

        EnsureSpriteImport(spec.spritePath);

        Sprite sprite = LoadSprite(spec.spritePath);
        building.id = spec.id;
        building.objectName = spec.displayName;
        building.sprite = sprite;
        building.icon = sprite;
        building.width = spec.width;
        building.height = 1;
        building.layer = GridLayer.Building;
        building.category = spec.category;
        building.horizontalDraggable = false;
        building.verticalDraggable = false;
        building.type = spec.componentType;
        building.tiles = null;
        building.movementAnchorOffset = Vector2.zero;
        building.maintenance = spec.maintenance;
        building.facility = spec.facility;
        building.defense = spec.defense;
        building.unlocked = false;
        EditorUtility.SetDirty(building);

        if (spec.componentType == typeof(Shop))
        {
            EnsureStockInfo(spec);
        }
    }

    private static void EnsureRecipeAsset(RecipeSpec spec)
    {
        string assetPath = $"{RecipeFolder}/{spec.assetName}.asset";
        FacilitySynthesisRecipeSO recipe = AssetDatabase.LoadAssetAtPath<FacilitySynthesisRecipeSO>(assetPath);
        if (recipe == null)
        {
            recipe = ScriptableObject.CreateInstance<FacilitySynthesisRecipeSO>();
            AssetDatabase.CreateAsset(recipe, assetPath);
        }

        recipe.id = spec.id;
        recipe.recipeId = spec.recipeId;
        recipe.displayName = spec.displayName;
        recipe.description = spec.description;
        recipe.resultBuilding = LoadBuilding(spec.resultAssetName);
        recipe.materialBuildings = spec.materialAssetNames
            .Select(LoadBuilding)
            .Where((building) => building != null)
            .ToArray();
        recipe.publicByDefault = spec.publicByDefault;
        recipe.requiredResearchRecipeId = spec.requiredResearchRecipeId;
        recipe.levelInheritanceRatio = spec.levelInheritanceRatio;
        EditorUtility.SetDirty(recipe);
    }

    private static SynthesisBuildingSpec[] CreateBuildingSpecs()
    {
        return new[]
        {
            new SynthesisBuildingSpec(
                "P1_BattleDining",
                50,
                "2성 전투 식당",
                "Assets/Images/Placeholders/Facilities/facility_battle_dining.png",
                4,
                BuildingCategory.Shop,
                typeof(Shop),
                3,
                new FacilityData
                {
                    roles = FacilityRole.Meal | FacilityRole.Training,
                    capacity = 3,
                    useDuration = 1.5f,
                    internalStockMax = 22,
                    restockRequestThreshold = 5,
                    requiredWorkers = 1,
                    supportedWorkTypes = FacilityWorkType.Operate | FacilityWorkType.Restock | FacilityWorkType.Repair | FacilityWorkType.Guard,
                    preferredSpeciesTags = new[] { "Orc" },
                    dislikedSpeciesTags = Array.Empty<string>(),
                    disabledWhenDamaged = true,
                    requiresStock = true,
                    requiresRoomRole = true
                },
                EmptyDefense()),
            new SynthesisBuildingSpec(
                "P1_PremiumMeatRestaurant",
                51,
                "2성 고급 고기 식당",
                "Assets/Images/Placeholders/Facilities/facility_premium_meat_restaurant.png",
                4,
                BuildingCategory.Shop,
                typeof(Shop),
                4,
                new FacilityData
                {
                    roles = FacilityRole.Meal | FacilityRole.Rest,
                    capacity = 3,
                    useDuration = 1.9f,
                    internalStockMax = 18,
                    restockRequestThreshold = 4,
                    requiredWorkers = 1,
                    supportedWorkTypes = FacilityWorkType.Operate | FacilityWorkType.Restock | FacilityWorkType.Repair,
                    preferredSpeciesTags = new[] { "Orc", "Vampire" },
                    dislikedSpeciesTags = Array.Empty<string>(),
                    disabledWhenDamaged = true,
                    requiresStock = true,
                    requiresRoomRole = true
                },
                EmptyDefense()),
            new SynthesisBuildingSpec(
                "P1_VenomSpikeTrap",
                52,
                "2성 맹독 가시 함정",
                "Assets/Images/Placeholders/Defense/defense_venom_spike.png",
                3,
                BuildingCategory.Special,
                typeof(DefenseFacility),
                2,
                DefenseFacilityData(FacilityWorkType.Repair, 0),
                DefenseData(
                    DefenseAttackConcept.Poison,
                    DefenseTriggerTiming.OnEnter | DefenseTriggerTiming.Periodic,
                    DefenseTargetRule.EnteringIntruder,
                    1f,
                    1f,
                    2,
                    "2성 맹독 가시 함정",
                    "P1_VenomSpikeTrap",
                    new[]
                    {
                        Effect(DefenseEffectKind.Damage, 18f, 0f, 1, "피해"),
                        Effect(DefenseEffectKind.Corrosion, 0.35f, 10f, 1, "부식")
                    })),
            new SynthesisBuildingSpec(
                "P1_AlarmCoil",
                53,
                "2성 경보 코일",
                "Assets/Images/Placeholders/Defense/defense_alarm_coil.png",
                3,
                BuildingCategory.Special,
                typeof(DefenseFacility),
                3,
                DefenseFacilityData(FacilityWorkType.Repair | FacilityWorkType.Guard, 1),
                DefenseData(
                    DefenseAttackConcept.Lightning,
                    DefenseTriggerTiming.OnEnter | DefenseTriggerTiming.Cooldown | DefenseTriggerTiming.GuardResponse,
                    DefenseTargetRule.GuardTarget,
                    2f,
                    0f,
                    2,
                    "2성 경보 코일",
                    "P1_AlarmCoil",
                    new[]
                    {
                        Effect(DefenseEffectKind.Damage, 12f, 0f, 1, "피해"),
                        Effect(DefenseEffectKind.Charge, 14f, 10f, 1, "축전"),
                        Effect(DefenseEffectKind.GuardAttack, 8f, 0f, 1, "경비 교전")
                    })),
            new SynthesisBuildingSpec(
                "P1_Barracks",
                54,
                "2성 병영",
                "Assets/Images/Placeholders/Defense/defense_barracks.png",
                4,
                BuildingCategory.Special,
                typeof(DefenseFacility),
                3,
                new FacilityData
                {
                    roles = FacilityRole.Training,
                    capacity = 2,
                    useDuration = 2f,
                    internalStockMax = 0,
                    restockRequestThreshold = 0,
                    requiredWorkers = 2,
                    supportedWorkTypes = FacilityWorkType.Repair | FacilityWorkType.Guard,
                    preferredSpeciesTags = new[] { "Orc" },
                    dislikedSpeciesTags = Array.Empty<string>(),
                    disabledWhenDamaged = true,
                    requiresStock = false,
                    requiresRoomRole = true
                },
                DefenseData(
                    DefenseAttackConcept.Guard,
                    DefenseTriggerTiming.OnEnter | DefenseTriggerTiming.GuardResponse,
                    DefenseTargetRule.GuardTarget,
                    1.5f,
                    0f,
                    2,
                    "2성 병영",
                    "P1_Barracks",
                    new[] { Effect(DefenseEffectKind.GuardAttack, 16f, 0f, 1, "경비 교전") })),
            new SynthesisBuildingSpec(
                "P1_BattlefieldDining",
                55,
                "3성 전장의 식당",
                "Assets/Images/Placeholders/Facilities/facility_battlefield_dining.png",
                4,
                BuildingCategory.Shop,
                typeof(Shop),
                6,
                new FacilityData
                {
                    roles = FacilityRole.Meal | FacilityRole.Training,
                    capacity = 4,
                    useDuration = 1.4f,
                    internalStockMax = 30,
                    restockRequestThreshold = 7,
                    requiredWorkers = 2,
                    supportedWorkTypes = FacilityWorkType.Operate | FacilityWorkType.Restock | FacilityWorkType.Repair | FacilityWorkType.Guard,
                    preferredSpeciesTags = new[] { "Orc" },
                    dislikedSpeciesTags = Array.Empty<string>(),
                    disabledWhenDamaged = true,
                    requiresStock = true,
                    requiresRoomRole = true
                },
                EmptyDefense()),
            new SynthesisBuildingSpec(
                "P1_NobleDining",
                56,
                "3성 귀족의 식당",
                "Assets/Images/Placeholders/Facilities/facility_noble_dining.png",
                4,
                BuildingCategory.Shop,
                typeof(Shop),
                7,
                new FacilityData
                {
                    roles = FacilityRole.Meal | FacilityRole.Rest | FacilityRole.Mana,
                    capacity = 3,
                    useDuration = 2.2f,
                    internalStockMax = 24,
                    restockRequestThreshold = 5,
                    requiredWorkers = 2,
                    supportedWorkTypes = FacilityWorkType.Operate | FacilityWorkType.Restock | FacilityWorkType.Repair | FacilityWorkType.Research,
                    preferredSpeciesTags = new[] { "Vampire" },
                    dislikedSpeciesTags = Array.Empty<string>(),
                    disabledWhenDamaged = true,
                    requiresStock = true,
                    requiresRoomRole = true
                },
                EmptyDefense()),
            new SynthesisBuildingSpec(
                "P1_CorrosionFreezer",
                57,
                "3성 부식 냉각 함정",
                "Assets/Images/Placeholders/Defense/defense_corrosion_freezer.png",
                3,
                BuildingCategory.Special,
                typeof(DefenseFacility),
                4,
                DefenseFacilityData(FacilityWorkType.Repair, 0),
                DefenseData(
                    DefenseAttackConcept.Ice,
                    DefenseTriggerTiming.OnEnter | DefenseTriggerTiming.Periodic,
                    DefenseTargetRule.AllIntrudersInRoom,
                    1.2f,
                    1.2f,
                    3,
                    "3성 부식 냉각 함정",
                    "P1_CorrosionFreezer",
                    new[]
                    {
                        Effect(DefenseEffectKind.Damage, 12f, 0f, 1, "피해"),
                        Effect(DefenseEffectKind.Corrosion, 0.45f, 12f, 1, "부식"),
                        Effect(DefenseEffectKind.Slow, 0.75f, 5f, 1, "감속")
                    })),
            new SynthesisBuildingSpec(
                "P1_StormFireTrap",
                58,
                "3성 폭뢰 분사구",
                "Assets/Images/Placeholders/Defense/defense_storm_fire.png",
                3,
                BuildingCategory.Special,
                typeof(DefenseFacility),
                5,
                DefenseFacilityData(FacilityWorkType.Repair | FacilityWorkType.Guard, 1),
                DefenseData(
                    DefenseAttackConcept.Lightning,
                    DefenseTriggerTiming.OnEnter | DefenseTriggerTiming.Cooldown | DefenseTriggerTiming.GuardResponse,
                    DefenseTargetRule.AllIntrudersInRoom,
                    2.2f,
                    0f,
                    3,
                    "3성 폭뢰 분사구",
                    "P1_StormFireTrap",
                    new[]
                    {
                        Effect(DefenseEffectKind.Damage, 18f, 0f, 1, "피해"),
                        Effect(DefenseEffectKind.Burn, 3f, 6f, 1, "연소"),
                        Effect(DefenseEffectKind.Charge, 18f, 10f, 1, "축전"),
                        Effect(DefenseEffectKind.GuardAttack, 10f, 0f, 1, "경비 교전")
                    })),
            new SynthesisBuildingSpec(
                "P1_WarBarracks",
                59,
                "3성 전투 병영",
                "Assets/Images/Placeholders/Defense/defense_war_barracks.png",
                4,
                BuildingCategory.Special,
                typeof(DefenseFacility),
                5,
                new FacilityData
                {
                    roles = FacilityRole.Training,
                    capacity = 2,
                    useDuration = 1.8f,
                    internalStockMax = 0,
                    restockRequestThreshold = 0,
                    requiredWorkers = 2,
                    supportedWorkTypes = FacilityWorkType.Repair | FacilityWorkType.Guard | FacilityWorkType.Operate,
                    preferredSpeciesTags = new[] { "Orc" },
                    dislikedSpeciesTags = Array.Empty<string>(),
                    disabledWhenDamaged = true,
                    requiresStock = false,
                    requiresRoomRole = true
                },
                DefenseData(
                    DefenseAttackConcept.Guard,
                    DefenseTriggerTiming.OnEnter | DefenseTriggerTiming.Cooldown | DefenseTriggerTiming.GuardResponse,
                    DefenseTargetRule.GuardTarget,
                    1.2f,
                    0f,
                    3,
                    "3성 전투 병영",
                    "P1_WarBarracks",
                    new[] { Effect(DefenseEffectKind.GuardAttack, 24f, 0f, 1, "경비 교전") }))
        };
    }

    private static RecipeSpec[] CreateRecipeSpecs()
    {
        return new[]
        {
            new RecipeSpec(
                7001,
                "RS_BattleDining",
                "recipe_battle_dining_1",
                "전투 식당",
                "고기 식당에 훈련 기능을 섞어 오크 친화 전투 식당으로 만든다.",
                "P1_BattleDining",
                new[] { "P1_MeatRestaurant", "P1_TrainingRoom" },
                true,
                string.Empty,
                0.75f),
            new RecipeSpec(
                7002,
                "RS_PremiumMeatRestaurant",
                "recipe_premium_meat_1",
                "고급 고기 식당",
                "고기 식당에 휴식 기능을 섞어 체류 가치가 높은 식당으로 만든다.",
                "P1_PremiumMeatRestaurant",
                new[] { "P1_MeatRestaurant", "P1_RestRoom" },
                true,
                string.Empty,
                0.75f),
            new RecipeSpec(
                7003,
                "RS_VenomSpikeTrap",
                "recipe_venom_spike_1",
                "맹독 가시 함정",
                "가시 피해와 부식 효과를 함께 가진 함정으로 만든다.",
                "P1_VenomSpikeTrap",
                new[] { "P1_SpikeTrap", "P1_PoisonPool" },
                true,
                string.Empty,
                0.75f),
            new RecipeSpec(
                7004,
                "RS_Barracks",
                "recipe_barracks_1",
                "병영",
                "경비실과 훈련장을 합쳐 더 강한 경비 대응 시설로 만든다.",
                "P1_Barracks",
                new[] { "P1_GuardRoom", "P1_TrainingRoom" },
                true,
                string.Empty,
                0.75f),
            new RecipeSpec(
                7091,
                "RS_AlarmCoil",
                "recipe_alarm_coil_2",
                "경보 코일",
                "번개 장치와 경비실을 연결한 특수 방어 조합식.",
                "P1_AlarmCoil",
                new[] { "P1_LightningPillar", "P1_GuardRoom" },
                false,
                "recipe_trap_chain_2",
                0.8f),
            new RecipeSpec(
                7010,
                "RS_BattlefieldDining",
                "recipe_battlefield_dining_2",
                "전장의 식당",
                "전투 식당과 병영을 연결해 식사와 경비 대응을 함께 강화한다.",
                "P1_BattlefieldDining",
                new[] { "P1_BattleDining", "P1_Barracks" },
                true,
                string.Empty,
                0.85f),
            new RecipeSpec(
                7011,
                "RS_NobleDining",
                "recipe_noble_dining_2",
                "귀족의 식당",
                "고급 고기 식당에 마력 저장소를 연결해 고가 체류형 식당으로 만든다.",
                "P1_NobleDining",
                new[] { "P1_PremiumMeatRestaurant", "P1_ManaStorage" },
                true,
                string.Empty,
                0.85f),
            new RecipeSpec(
                7012,
                "RS_CorrosionFreezer",
                "recipe_corrosion_freezer_2",
                "부식 냉각 함정",
                "맹독 가시 함정에 냉기 분사구를 합쳐 부식과 감속을 동시에 건다.",
                "P1_CorrosionFreezer",
                new[] { "P1_VenomSpikeTrap", "P1_IceVent" },
                true,
                string.Empty,
                0.85f),
            new RecipeSpec(
                7013,
                "RS_WarBarracks",
                "recipe_war_barracks_2",
                "전투 병영",
                "병영과 무기점을 연결해 경비 대응 피해를 강화한다.",
                "P1_WarBarracks",
                new[] { "P1_Barracks", "P1_WeaponShop" },
                true,
                string.Empty,
                0.85f),
            new RecipeSpec(
                7092,
                "RS_StormFireTrap",
                "recipe_storm_fire_3",
                "폭뢰 분사구",
                "경보 코일과 화염 분사구를 연결한 특수 고화력 방어 조합식.",
                "P1_StormFireTrap",
                new[] { "P1_AlarmCoil", "P1_FireVent" },
                false,
                "recipe_trap_chain_3",
                0.8f)
        };
    }

    private static FacilityData DefenseFacilityData(FacilityWorkType workTypes, int requiredWorkers)
    {
        return new FacilityData
        {
            roles = FacilityRole.None,
            capacity = 0,
            useDuration = 0f,
            internalStockMax = 0,
            restockRequestThreshold = 0,
            requiredWorkers = requiredWorkers,
            supportedWorkTypes = workTypes,
            preferredSpeciesTags = Array.Empty<string>(),
            dislikedSpeciesTags = Array.Empty<string>(),
            disabledWhenDamaged = true,
            requiresStock = false
        };
    }

    private static DefenseFacilityData DefenseData(
        DefenseAttackConcept concept,
        DefenseTriggerTiming trigger,
        DefenseTargetRule target,
        float cooldown,
        float period,
        int star,
        string combatLogText,
        string effectAssetPrefix,
        DefenseEffectData[] effects)
    {
        return new DefenseFacilityData
        {
            enabled = true,
            concept = concept,
            triggerTimings = trigger,
            targetRule = target,
            cooldownSeconds = cooldown,
            periodicIntervalSeconds = period,
            range = 0,
            star = star,
            combatLogText = combatLogText,
            effectAssets = EnsureEffectAssets(effectAssetPrefix, effects),
            effects = effects
        };
    }

    private static DefenseFacilityData EmptyDefense()
    {
        return new DefenseFacilityData
        {
            enabled = false,
            concept = DefenseAttackConcept.None,
            triggerTimings = DefenseTriggerTiming.None,
            targetRule = DefenseTargetRule.EnteringIntruder,
            cooldownSeconds = 0f,
            periodicIntervalSeconds = 0f,
            range = 0,
            star = 1,
            combatLogText = string.Empty,
            effectAssets = Array.Empty<DefenseEffectSO>(),
            effects = Array.Empty<DefenseEffectData>()
        };
    }

    private static DefenseEffectSO[] EnsureEffectAssets(string prefix, DefenseEffectData[] effects)
    {
        if (effects == null || effects.Length == 0)
        {
            return Array.Empty<DefenseEffectSO>();
        }

        DefenseEffectSO[] result = new DefenseEffectSO[effects.Length];
        for (int i = 0; i < effects.Length; i++)
        {
            DefenseEffectData effect = effects[i];
            string assetPath = $"{EffectFolder}/{prefix}_{i + 1}_{effect.kind}.asset";
            DefenseEffectSO effectAsset = AssetDatabase.LoadAssetAtPath<DefenseEffectSO>(assetPath);
            if (effectAsset == null)
            {
                effectAsset = ScriptableObject.CreateInstance<DefenseEffectSO>();
                AssetDatabase.CreateAsset(effectAsset, assetPath);
            }

            effectAsset.Kind = effect.kind;
            effectAsset.Amount = effect.amount;
            effectAsset.Duration = effect.duration;
            effectAsset.Stacks = effect.stacks;
            effectAsset.LogTag = effect.logTag;
            EditorUtility.SetDirty(effectAsset);
            result[i] = effectAsset;
        }

        return result;
    }

    private static DefenseEffectData Effect(DefenseEffectKind kind, float amount, float duration, int stacks, string logTag)
    {
        return new DefenseEffectData
        {
            kind = kind,
            amount = amount,
            duration = duration,
            stacks = stacks,
            logTag = logTag
        };
    }

    private static BuildingSO LoadBuilding(string assetName)
    {
        if (string.IsNullOrWhiteSpace(assetName))
        {
            return null;
        }

        return AssetDatabase.LoadAssetAtPath<BuildingSO>($"{BuildingFolder}/{assetName}.asset");
    }

    private static Sprite LoadSprite(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void EnsureSpriteImport(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 16f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static void EnsureStockInfo(SynthesisBuildingSpec spec)
    {
        string assetPath = $"{StockFolder}/{spec.assetName}Stock.asset";
        StockInfo stockInfo = AssetDatabase.LoadAssetAtPath<StockInfo>(assetPath);
        if (stockInfo == null)
        {
            stockInfo = ScriptableObject.CreateInstance<StockInfo>();
            AssetDatabase.CreateAsset(stockInfo, assetPath);
        }

        SaleItem food = AssetDatabase.LoadAssetAtPath<SaleItem>("Assets/Resources/SO/Stock/Item/햄버거.asset");
        stockInfo.id = spec.id;
        stockInfo.shopId = spec.id;
        stockInfo.type = Shop.Type.Food;
        stockInfo.multifly = 1.1f;
        stockInfo.stocks = food != null
            ? new List<Tuple<SaleItem, int>> { Tuple.Create(food, Mathf.Max(1, spec.facility.internalStockMax)) }
            : new List<Tuple<SaleItem, int>>();
        EditorUtility.SetDirty(stockInfo);
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

    private readonly struct SynthesisBuildingSpec
    {
        public SynthesisBuildingSpec(
            string assetName,
            int id,
            string displayName,
            string spritePath,
            int width,
            BuildingCategory category,
            Type componentType,
            int maintenance,
            FacilityData facility,
            DefenseFacilityData defense)
        {
            this.assetName = assetName;
            this.id = id;
            this.displayName = displayName;
            this.spritePath = spritePath;
            this.width = width;
            this.category = category;
            this.componentType = componentType;
            this.maintenance = maintenance;
            this.facility = facility;
            this.defense = defense;
        }

        public readonly string assetName;
        public readonly int id;
        public readonly string displayName;
        public readonly string spritePath;
        public readonly int width;
        public readonly BuildingCategory category;
        public readonly Type componentType;
        public readonly int maintenance;
        public readonly FacilityData facility;
        public readonly DefenseFacilityData defense;
    }

    private readonly struct RecipeSpec
    {
        public RecipeSpec(
            int id,
            string assetName,
            string recipeId,
            string displayName,
            string description,
            string resultAssetName,
            string[] materialAssetNames,
            bool publicByDefault,
            string requiredResearchRecipeId,
            float levelInheritanceRatio)
        {
            this.id = id;
            this.assetName = assetName;
            this.recipeId = recipeId;
            this.displayName = displayName;
            this.description = description;
            this.resultAssetName = resultAssetName;
            this.materialAssetNames = materialAssetNames ?? Array.Empty<string>();
            this.publicByDefault = publicByDefault;
            this.requiredResearchRecipeId = requiredResearchRecipeId ?? string.Empty;
            this.levelInheritanceRatio = Mathf.Clamp01(levelInheritanceRatio);
        }

        public readonly int id;
        public readonly string assetName;
        public readonly string recipeId;
        public readonly string displayName;
        public readonly string description;
        public readonly string resultAssetName;
        public readonly string[] materialAssetNames;
        public readonly bool publicByDefault;
        public readonly string requiredResearchRecipeId;
        public readonly float levelInheritanceRatio;
    }
}
