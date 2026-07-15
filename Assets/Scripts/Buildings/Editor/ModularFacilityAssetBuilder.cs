using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ModularFacilityAssetBuilder
{
    public const int FirstBuildingId = 1000;
    private const string SpriteFolder = "Assets/Images/ModularFacilities";
    private const string BuildingFolder = "Assets/Resources/SO/Building/Modular";
    private const string StockFolder = "Assets/Resources/SO/Stock/Modular";
    private const string GeneralStockSource = "Assets/Resources/SO/Stock/P1/P1_GeneralStoreStock.asset";
    private const string SaleItemFolder = "Assets/Resources/SO/Stock/Item";

    private static readonly string[] LegacyRoomAssetPaths =
    {
        "Assets/Resources/SO/Building/LordBedroom.asset",
        "Assets/Resources/SO/Building/HamburgerStore.asset",
        "Assets/Resources/SO/Building/WeaponStore.asset",
        "Assets/Resources/SO/Building/P1/P1_LowFoodShop.asset",
        "Assets/Resources/SO/Building/P1/P1_MeatRestaurant.asset",
        "Assets/Resources/SO/Building/P1/P1_PremiumMeatRestaurant.asset",
        "Assets/Resources/SO/Building/P1/P1_BattleDining.asset",
        "Assets/Resources/SO/Building/P1/P1_BattlefieldDining.asset",
        "Assets/Resources/SO/Building/P1/P1_NobleDining.asset",
        "Assets/Resources/SO/Building/P1/P1_GeneralStore.asset",
        "Assets/Resources/SO/Building/P1/P1_WeaponShop.asset",
        "Assets/Resources/SO/Building/P1/P1_RestRoom.asset",
        "Assets/Resources/SO/Building/P1/P1_TrainingRoom.asset",
        "Assets/Resources/SO/Building/P1/P1_GuardRoom.asset",
        "Assets/Resources/SO/Building/P1/P1_Barracks.asset",
        "Assets/Resources/SO/Building/P1/P1_WarBarracks.asset",
        "Assets/Resources/SO/Building/P1/P1_ResearchLab.asset",
        "Assets/Resources/SO/Building/P1/P1_ManaStorage.asset",
        "Assets/Resources/SO/Building/P1/P1_Warehouse.asset",
        "Assets/Resources/SO/Building/P1/P1_Toilet.asset",
        "Assets/Resources/SO/Building/P1/P1_Washroom.asset"
    };

    [MenuItem("DungeonStory/Content/Build All Modular Facilities")]
    public static void BuildAllFromMenu()
    {
        BuildAll();
    }

    public static void BuildAll()
    {
        FacilityPartSpec[] specs = CreateSpecs();
        if (specs.Length != 73)
        {
            throw new InvalidOperationException($"Modular facility catalog must contain 73 parts, found {specs.Length}.");
        }

        EnsureFolder(SpriteFolder);
        EnsureFolder(BuildingFolder);
        EnsureFolder(StockFolder);

        for (int index = 0; index < specs.Length; index++)
        {
            FacilityPartSpec spec = specs[index];
            int buildingId = FirstBuildingId + index;
            string spritePath = $"{SpriteFolder}/{spec.Code}.png";
            WriteSprite(spec, spritePath);
            ConfigureSpriteImport(spritePath);
            EnsureBuildingAsset(spec, buildingId, spritePath);

        }

        EnsureShopStock(FirstBuildingId + Array.FindIndex(specs, (spec) => spec.Code == "S01"));

        HideLegacyRoomAssets();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Built {specs.Length} modular facility sprites and BuildingSO assets.");
    }

    public static IReadOnlyList<string> GetCatalogCodes()
    {
        return CreateSpecs().Select((spec) => spec.Code).ToArray();
    }

    private static void EnsureBuildingAsset(FacilityPartSpec spec, int buildingId, string spritePath)
    {
        string assetPath = $"{BuildingFolder}/{spec.Code}_{spec.AssetName}.asset";
        BuildingSO building = AssetDatabase.LoadAssetAtPath<BuildingSO>(assetPath);
        if (building == null)
        {
            building = ScriptableObject.CreateInstance<BuildingSO>();
            AssetDatabase.CreateAsset(building, assetPath);
        }

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        building.id = buildingId;
        building.objectName = spec.DisplayName;
        building.sprite = sprite;
        building.icon = sprite;
        building.width = spec.Width;
        building.height = 1;
        building.layer = spec.Layer;
        building.category = spec.Category;
        building.horizontalDraggable = false;
        building.verticalDraggable = false;
        building.type = spec.RuntimeType;
        building.tiles = null;
        building.movementAnchorOffset = Vector2.zero;
        building.movementTravelTime = 2f;
        building.maintenance = spec.Core ? 2 + spec.Phase : spec.Phase;
        building.facility = CreateFacilityData(spec);
        building.defense = new DefenseFacilityData();
        building.evolution = CreateEvolutionData(spec);
        building.operational = CreateOperationalData(spec);
        building.unlocked = true;
        EditorUtility.SetDirty(building);
    }

    private static FacilityData CreateFacilityData(FacilityPartSpec spec)
    {
        return new FacilityData
        {
            roles = spec.Roles,
            capacity = spec.Core ? Mathf.Max(1, spec.Capacity) : 0,
            useDuration = spec.Core ? Mathf.Max(0.5f, spec.UseDuration) : 0f,
            internalStockMax = Mathf.Max(0, spec.InternalStockMax),
            restockRequestThreshold = spec.InternalStockMax > 0
                ? Mathf.Max(1, spec.InternalStockMax / 4)
                : 0,
            requiredWorkers = spec.WorkTypes == FacilityWorkType.None ? 0 : 1,
            supportedWorkTypes = spec.WorkTypes,
            preferredSpeciesTags = Array.Empty<string>(),
            dislikedSpeciesTags = Array.Empty<string>(),
            disabledWhenDamaged = true,
            requiresStock = spec.RuntimeType == typeof(Shop),
            requiresRoomRole = spec.Roles != FacilityRole.None,
            requiresStaffedService = spec.RuntimeType == typeof(Shop),
            selfContainedRoom = false
        };
    }

    private static FacilityEvolutionContributionData CreateEvolutionData(FacilityPartSpec spec)
    {
        string[] traits = spec.Traits ?? Array.Empty<string>();
        return new FacilityEvolutionContributionData
        {
            contributesToRoomProfile = spec.ContributesToRoom,
            tags = traits.Distinct(StringComparer.Ordinal).ToArray(),
            scores = traits
                .Distinct(StringComparer.Ordinal)
                .Select((trait) => new FacilityEvolutionValue(trait, GetTraitScore(trait)))
                .ToArray(),
            metrics = spec.Metrics ?? Array.Empty<FacilityEvolutionValue>()
        };
    }

    private static FacilityOperationalData CreateOperationalData(FacilityPartSpec spec)
    {
        FacilityOperationalData data = new FacilityOperationalData
        {
            code = spec.Code,
            functions = GetFunctions(spec.Code) | (spec.Core ? FacilityFunction.None : FacilityFunction.Support),
            recovery = GetRecovery(spec.Code),
            storageCategory = GetStorageCategory(spec.Code),
            storageCapacity = GetStorageCapacity(spec.Code),
            seatCapacity = GetSeatCapacity(spec.Code),
            tableCapacity = GetTableCapacity(spec.Code),
            serviceCapacity = GetServiceCapacity(spec.Code),
            workOutputAmount = GetWorkOutput(spec.Code),
            constructionCost = 20 + spec.Width * 15 + spec.Phase * 25 + (spec.Core ? 35 : 0),
            unlockPhase = Mathf.Clamp(spec.Phase, 1, 3),
            demolitionRefundRate = 0.5f,
            lightIntensity = GetLightIntensity(spec.Code),
            lightRadius = GetLightRadius(spec.Code)
        };
        return data;
    }

    private static FacilityFunction GetFunctions(string code)
    {
        return code switch
        {
            "D01" => FacilityFunction.MealProduction,
            "D02" => FacilityFunction.MealProduction | FacilityFunction.MeatProduction,
            "D03" => FacilityFunction.MealService,
            "D04" => FacilityFunction.MealService,
            "D05" or "D06" => FacilityFunction.Table,
            "D07" or "D08" or "D09" => FacilityFunction.Seating,
            "D10" => FacilityFunction.Storage,
            "D11" => FacilityFunction.MeatProduction,
            "D12" => FacilityFunction.MealService,
            "S01" or "S02" or "S03" => FacilityFunction.RetailGeneral,
            "S04" => FacilityFunction.RetailGeneral | FacilityFunction.Storage,
            "S05" or "S06" => FacilityFunction.RetailWeapon,
            "S07" => FacilityFunction.RetailWeapon | FacilityFunction.Storage,
            "S08" => FacilityFunction.RetailWeapon | FacilityFunction.WeaponCrafting,
            "R01" or "R02" or "R03" or "R04" => FacilityFunction.Rest,
            "R05" => FacilityFunction.Decoration,
            "R06" or "R09" => FacilityFunction.Storage,
            "R07" => FacilityFunction.Administration,
            "R08" => FacilityFunction.Administration | FacilityFunction.Seating,
            "R10" => FacilityFunction.Research,
            "Q01" => FacilityFunction.Research,
            "Q02" => FacilityFunction.Research | FacilityFunction.Alchemy | FacilityFunction.ManaRitual,
            "Q03" or "Q05" => FacilityFunction.Research | FacilityFunction.Storage,
            "Q04" => FacilityFunction.Alchemy | FacilityFunction.Storage,
            "Q06" => FacilityFunction.Research,
            "M01" or "M02" => FacilityFunction.ManaStorage | FacilityFunction.Storage,
            "M03" => FacilityFunction.ManaRitual,
            "M04" => FacilityFunction.ManaRitual,
            "T01" or "T04" => FacilityFunction.MeleeTraining,
            "T02" => FacilityFunction.RangedTraining,
            "T03" => FacilityFunction.StrengthTraining,
            "G01" or "G03" or "G04" or "G05" or "G06" => FacilityFunction.GuardPost,
            "G02" => FacilityFunction.Alarm,
            "L01" => FacilityFunction.Logistics | FacilityFunction.Storage,
            "L02" or "L03" or "L04" or "L05" or "L06" or "L07" => FacilityFunction.Storage,
            "H01" or "H02" => FacilityFunction.Toilet | FacilityFunction.Cleaning,
            "H03" or "H05" => FacilityFunction.Hygiene | FacilityFunction.Cleaning,
            "H04" => FacilityFunction.Hygiene | FacilityFunction.Rest | FacilityFunction.Cleaning,
            "H06" => FacilityFunction.Cleaning | FacilityFunction.Storage,
            "H07" => FacilityFunction.Cleaning,
            "E01" => FacilityFunction.Lighting,
            "E02" => FacilityFunction.Lighting | FacilityFunction.Rest,
            "E03" or "E07" => FacilityFunction.Lighting | FacilityFunction.Decoration,
            "E04" or "E05" or "E06" or "E08" => FacilityFunction.Decoration,
            "E09" => FacilityFunction.Signage,
            _ => FacilityFunction.Support
        };
    }

    private static FacilityNeedRecoveryData GetRecovery(string code)
    {
        return code switch
        {
            "D01" => Recovery(hunger: 35f, mood: 4f),
            "D02" => Recovery(hunger: 48f, mood: 8f),
            "D04" => Recovery(hunger: 25f, mood: 3f),
            "D12" => Recovery(hunger: 12f, mood: 6f, fun: 8f),
            "R01" => Recovery(sleep: 35f, mood: 4f),
            "R02" => Recovery(sleep: 52f, mood: 10f),
            "R03" => Recovery(sleep: 42f, mood: 3f),
            "R04" => Recovery(sleep: 25f, mood: 6f, fun: 8f),
            "Q01" => Recovery(mood: 4f, fun: 10f),
            "Q02" => Recovery(mood: 7f, fun: 12f),
            "M01" => Recovery(mood: 6f),
            "M02" => Recovery(mood: 10f),
            "M04" => Recovery(mood: 12f, fun: 5f),
            "T01" => Recovery(mood: 4f, fun: 18f),
            "T02" => Recovery(mood: 3f, fun: 16f),
            "T03" => Recovery(mood: 5f, fun: 12f),
            "H01" => Recovery(excretion: 75f, mood: 2f),
            "H03" => Recovery(hygiene: 62f, mood: 3f),
            "H04" => Recovery(sleep: 12f, hygiene: 85f, mood: 10f),
            _ => default
        };
    }

    private static FacilityNeedRecoveryData Recovery(
        float sleep = 0f,
        float mood = 0f,
        float fun = 0f,
        float hunger = 0f,
        float excretion = 0f,
        float hygiene = 0f)
    {
        return new FacilityNeedRecoveryData
        {
            sleep = sleep,
            mood = mood,
            fun = fun,
            hunger = hunger,
            excretion = excretion,
            hygiene = hygiene
        };
    }

    private static int GetStorageCapacity(string code)
    {
        return code switch
        {
            "D10" => 16,
            "S04" => 12,
            "S07" => 16,
            "R06" => 8,
            "R09" => 6,
            "Q03" => 10,
            "Q04" => 12,
            "Q05" => 8,
            "M01" => 18,
            "M02" => 36,
            "L01" => 60,
            "L02" => 16,
            "L03" => 14,
            "L04" => 12,
            "L05" or "L06" or "L07" => 20,
            "H06" => 8,
            _ => 0
        };
    }

    private static StockCategory GetStorageCategory(string code)
    {
        return code switch
        {
            "D10" or "L03" or "L04" or "L05" => StockCategory.Food,
            "S07" or "L06" => StockCategory.Weapon,
            "Q04" or "M01" or "M02" or "L07" => StockCategory.Mana,
            _ => StockCategory.General
        };
    }

    private static int GetSeatCapacity(string code)
    {
        return code switch { "D07" or "D08" or "R08" => 1, "D09" => 2, _ => 0 };
    }

    private static int GetTableCapacity(string code)
    {
        return code switch { "D05" => 2, "D06" => 6, _ => 0 };
    }

    private static int GetServiceCapacity(string code)
    {
        return code switch { "S02" => 1, "S03" => 1, "D03" or "D04" or "D12" => 1, _ => 0 };
    }

    private static int GetWorkOutput(string code)
    {
        return code switch
        {
            "D01" => 4,
            "D02" => 6,
            "S08" => 3,
            "Q02" => 3,
            "M01" => 2,
            "M02" => 4,
            "M04" => 5,
            _ => 0
        };
    }

    private static float GetLightIntensity(string code)
    {
        return code switch { "E01" => 0.75f, "E02" => 0.9f, "E03" => 1.15f, "E07" => 0.5f, _ => 0f };
    }

    private static float GetLightRadius(string code)
    {
        return code switch { "E01" => 2.8f, "E02" => 3.2f, "E03" => 4.2f, "E07" => 2.1f, _ => 0f };
    }

    private static float GetTraitScore(string trait)
    {
        return trait switch
        {
            FacilityEvolutionTerms.Luxury => 3f,
            FacilityEvolutionTerms.Hygiene => 3f,
            FacilityEvolutionTerms.Service => 2.5f,
            FacilityEvolutionTerms.Storage => 2.5f,
            FacilityEvolutionTerms.Security => 2.5f,
            _ => 2f
        };
    }

    private static void EnsureShopStock(int shopId)
    {
        const string assetPath = StockFolder + "/S01_SalesCounterStock.asset";
        StockInfo stock = AssetDatabase.LoadAssetAtPath<StockInfo>(assetPath);
        if (stock == null)
        {
            stock = ScriptableObject.CreateInstance<StockInfo>();
            AssetDatabase.CreateAsset(stock, assetPath);
        }

        StockInfo source = AssetDatabase.LoadAssetAtPath<StockInfo>(GeneralStockSource);
        SaleItem generalItem = EnsureGeneralSaleItem();
        SaleItem foodItem = AssetDatabase.LoadAssetAtPath<SaleItem>(SaleItemFolder + "/햄버거.asset");
        SaleItem swordItem = AssetDatabase.LoadAssetAtPath<SaleItem>(SaleItemFolder + "/도란검.asset");
        SaleItem shieldItem = AssetDatabase.LoadAssetAtPath<SaleItem>(SaleItemFolder + "/도란방패.asset");
        stock.id = 3000;
        stock.shopId = shopId;
        stock.type = source != null ? source.type : Shop.Type.Item;
        stock.stocks = new[]
            {
                new Tuple<SaleItem, int>(generalItem, 12),
                new Tuple<SaleItem, int>(foodItem, 12),
                new Tuple<SaleItem, int>(swordItem, 8),
                new Tuple<SaleItem, int>(shieldItem, 8)
            }
            .Where((tuple) => tuple.Item1 != null)
            .ToList();
        stock.multifly = source != null ? source.multifly : 1f;
        EditorUtility.SetDirty(stock);
    }

    private static SaleItem EnsureGeneralSaleItem()
    {
        const string assetPath = SaleItemFolder + "/모험용품.asset";
        SaleItem item = AssetDatabase.LoadAssetAtPath<SaleItem>(assetPath);
        if (item == null)
        {
            item = ScriptableObject.CreateInstance<SaleItem>();
            AssetDatabase.CreateAsset(item, assetPath);
        }

        item.id = 4000;
        item.itemName = "모험용품";
        item.category = StockCategory.General;
        item.cost = 45;
        item.itemSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + "/S02.png");
        item.buyevent = Array.Empty<OnBuyItemSO>();
        EditorUtility.SetDirty(item);
        return item;
    }

    private static void HideLegacyRoomAssets()
    {
        foreach (string assetPath in LegacyRoomAssetPaths)
        {
            BuildingSO legacy = AssetDatabase.LoadAssetAtPath<BuildingSO>(assetPath);
            if (legacy == null || !legacy.unlocked)
            {
                continue;
            }

            legacy.unlocked = false;
            EditorUtility.SetDirty(legacy);
        }
    }

    private static void WriteSprite(FacilityPartSpec spec, string spritePath)
    {
        int width = Mathf.Max(24, spec.Width * 24);
        const int height = 48;
        PixelCanvas canvas = new PixelCanvas(width, height);
        DrawProp(canvas, spec);

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.name = spec.Code;
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels32(canvas.Pixels);
        texture.Apply(false, false);
        File.WriteAllBytes(spritePath, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
    }

    private static void ConfigureSpriteImport(string spritePath)
    {
        AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 16f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();
    }

    private static FacilityPartSpec[] CreateSpecs()
    {
        return new[]
        {
            Core("D01", "간이 화덕", 1, GridLayer.Building, BuildingCategory.Shop, FacilityRole.Meal, FacilityWorkType.Operate | FacilityWorkType.Clean | FacilityWorkType.Repair, VisualForm.Hearth, Traits("Cooking")),
            Core("D02", "고기 그릴", 2, GridLayer.Building, BuildingCategory.Shop, FacilityRole.Meal, FacilityWorkType.Operate | FacilityWorkType.Clean | FacilityWorkType.Repair, VisualForm.Grill, Traits("Cooking", "Meat")),
            Support("D03", "조리 손질대", 2, GridLayer.Building, BuildingCategory.Shop, VisualForm.Workbench, Traits("Cooking", FacilityEvolutionTerms.Service)),
            Core("D04", "배식 카운터", 2, GridLayer.Building, BuildingCategory.Shop, FacilityRole.Meal, FacilityWorkType.Operate | FacilityWorkType.Clean, VisualForm.Counter, Traits(FacilityEvolutionTerms.Service), Metrics((FacilityEvolutionTerms.CounterCount, 1f))),
            Support("D05", "소형 식탁", 2, GridLayer.Building, BuildingCategory.Shop, VisualForm.Table, Traits(FacilityEvolutionTerms.Dining), Metrics((FacilityEvolutionTerms.TableCount, 1f))),
            Support("D06", "대형 연회 식탁", 3, GridLayer.Building, BuildingCategory.Shop, VisualForm.Table, Traits(FacilityEvolutionTerms.Dining, FacilityEvolutionTerms.Luxury), Metrics((FacilityEvolutionTerms.LargeTableCount, 1f)), phase: 2),
            Support("D07", "목제 의자", 1, GridLayer.Building, BuildingCategory.Shop, VisualForm.Chair, Traits(FacilityEvolutionTerms.Dining), Metrics((FacilityEvolutionTerms.SeatCount, 1f))),
            Support("D08", "푹신한 의자", 1, GridLayer.Building, BuildingCategory.Shop, VisualForm.Chair, Traits(FacilityEvolutionTerms.Dining, FacilityEvolutionTerms.Luxury), Metrics((FacilityEvolutionTerms.SeatCount, 1f), (FacilityEvolutionTerms.PrivateSeatCount, 1f)), phase: 2),
            Support("D09", "긴 벤치", 2, GridLayer.Building, BuildingCategory.Shop, VisualForm.Bench, Traits(FacilityEvolutionTerms.Dining, FacilityEvolutionTerms.Crowd), Metrics((FacilityEvolutionTerms.SeatCount, 2f))),
            Support("D10", "식재료 선반", 1, GridLayer.Building, BuildingCategory.Shop, VisualForm.Shelf, Traits(FacilityEvolutionTerms.Storage, "Cooking")),
            Support("D11", "고기 걸이", 1, GridLayer.WallFixture, BuildingCategory.Shop, VisualForm.WallRack, Traits("Meat", FacilityEvolutionTerms.Brutal), phase: 2),
            Core("D12", "술 음료장", 1, GridLayer.Building, BuildingCategory.Shop, FacilityRole.Meal, FacilityWorkType.Operate | FacilityWorkType.Clean | FacilityWorkType.Repair, VisualForm.Cabinet, Traits(FacilityEvolutionTerms.Service, FacilityEvolutionTerms.Luxury), phase: 2),

            Core("S01", "판매 카운터", 2, GridLayer.Building, BuildingCategory.Shop, FacilityRole.Purchase, FacilityWorkType.Operate | FacilityWorkType.Restock | FacilityWorkType.Repair, VisualForm.Counter, Traits(FacilityEvolutionTerms.Service), Metrics((FacilityEvolutionTerms.CounterCount, 1f)), runtimeType: typeof(Shop), internalStockMax: 24),
            Support("S02", "잡화 진열 선반", 2, GridLayer.Building, BuildingCategory.Shop, VisualForm.Shelf, Traits("Shop", FacilityEvolutionTerms.Service)),
            Support("S03", "잠금 진열장", 2, GridLayer.Building, BuildingCategory.Shop, VisualForm.Cabinet, Traits("Shop", FacilityEvolutionTerms.Luxury, FacilityEvolutionTerms.Security), phase: 2),
            Support("S04", "잡화 상자", 1, GridLayer.Building, BuildingCategory.Shop, VisualForm.Crates, Traits(FacilityEvolutionTerms.Storage)),
            Support("S05", "무기 거치대", 1, GridLayer.WallFixture, BuildingCategory.Shop, VisualForm.WallRack, Traits(FacilityEvolutionTerms.Combat)),
            Support("S06", "갑옷 거치대", 1, GridLayer.Building, BuildingCategory.Shop, VisualForm.ArmorStand, Traits(FacilityEvolutionTerms.Combat, FacilityEvolutionTerms.Defense)),
            Support("S07", "무기 보관함", 1, GridLayer.Building, BuildingCategory.Shop, VisualForm.Cabinet, Traits(FacilityEvolutionTerms.Storage, FacilityEvolutionTerms.Security)),
            Core("S08", "대장 작업대", 2, GridLayer.Building, BuildingCategory.Shop, FacilityRole.None, FacilityWorkType.Operate | FacilityWorkType.Repair, VisualForm.Workbench, Traits(FacilityEvolutionTerms.Combat, FacilityEvolutionTerms.Service), phase: 2),

            Core("R01", "간이침대", 2, GridLayer.Building, BuildingCategory.Special, FacilityRole.Rest, FacilityWorkType.Rest | FacilityWorkType.Repair, VisualForm.Bed, Traits(FacilityEvolutionTerms.Rest)),
            Core("R02", "정식 침대", 2, GridLayer.Building, BuildingCategory.Special, FacilityRole.Rest, FacilityWorkType.Rest | FacilityWorkType.Repair, VisualForm.Bed, Traits(FacilityEvolutionTerms.Rest, FacilityEvolutionTerms.Luxury), phase: 2),
            Core("R03", "이층침대", 2, GridLayer.Building, BuildingCategory.Special, FacilityRole.Rest, FacilityWorkType.Rest | FacilityWorkType.Repair, VisualForm.BunkBed, Traits(FacilityEvolutionTerms.Rest, FacilityEvolutionTerms.Crowd), capacity: 2, phase: 2),
            Core("R04", "휴식용 소파", 2, GridLayer.Building, BuildingCategory.Special, FacilityRole.Rest, FacilityWorkType.Rest | FacilityWorkType.Repair, VisualForm.Sofa, Traits(FacilityEvolutionTerms.Rest), capacity: 2),
            Support("R05", "협탁", 1, GridLayer.Building, BuildingCategory.Special, VisualForm.Table, Traits(FacilityEvolutionTerms.Luxury, FacilityEvolutionTerms.Quiet), phase: 2),
            Support("R06", "옷장", 1, GridLayer.Building, BuildingCategory.Special, VisualForm.Cabinet, Traits(FacilityEvolutionTerms.Storage, FacilityEvolutionTerms.Luxury), phase: 2),
            Core("R07", "영주 집무책상", 2, GridLayer.Building, BuildingCategory.Special, FacilityRole.Administration, FacilityWorkType.Operate | FacilityWorkType.Repair, VisualForm.Desk, Traits("Administration", FacilityEvolutionTerms.Service)),
            Support("R08", "지휘 의자", 1, GridLayer.Building, BuildingCategory.Special, VisualForm.Chair, Traits("Administration", FacilityEvolutionTerms.Noble, FacilityEvolutionTerms.Luxury)),
            Support("R09", "개인 보관함", 1, GridLayer.Building, BuildingCategory.Special, VisualForm.Cabinet, Traits(FacilityEvolutionTerms.Storage)),
            Support("R10", "침실용 책장", 1, GridLayer.Building, BuildingCategory.Special, VisualForm.Shelf, Traits(FacilityEvolutionTerms.Quiet, FacilityEvolutionTerms.Research), phase: 2),

            Core("Q01", "연구 책상", 2, GridLayer.Building, BuildingCategory.Crafting, FacilityRole.Research, FacilityWorkType.Research | FacilityWorkType.Repair, VisualForm.Desk, Traits(FacilityEvolutionTerms.Research)),
            Core("Q02", "연금술 작업대", 2, GridLayer.Building, BuildingCategory.Crafting, FacilityRole.Research | FacilityRole.Mana, FacilityWorkType.Research | FacilityWorkType.Operate | FacilityWorkType.Repair, VisualForm.Workbench, Traits(FacilityEvolutionTerms.Research, FacilityEvolutionTerms.Mana, FacilityEvolutionTerms.Ritual)),
            Support("Q03", "연구용 책장", 1, GridLayer.Building, BuildingCategory.Crafting, VisualForm.Shelf, Traits(FacilityEvolutionTerms.Research, FacilityEvolutionTerms.Storage)),
            Support("Q04", "시약 선반", 1, GridLayer.Building, BuildingCategory.Crafting, VisualForm.Shelf, Traits(FacilityEvolutionTerms.Research, FacilityEvolutionTerms.Mana)),
            Support("Q05", "표본 보관장", 1, GridLayer.Building, BuildingCategory.Crafting, VisualForm.Cabinet, Traits(FacilityEvolutionTerms.Research, FacilityEvolutionTerms.Fear), phase: 2),
            Support("Q06", "설계판", 1, GridLayer.WallFixture, BuildingCategory.Crafting, VisualForm.Board, Traits(FacilityEvolutionTerms.Research, FacilityEvolutionTerms.Service), phase: 2),

            Core("M01", "마력 수정 선반", 1, GridLayer.Building, BuildingCategory.Resource, FacilityRole.Mana, FacilityWorkType.Operate | FacilityWorkType.Repair, VisualForm.Shelf, Traits(FacilityEvolutionTerms.Mana, FacilityEvolutionTerms.Storage)),
            Core("M02", "마력 저장조", 2, GridLayer.Building, BuildingCategory.Resource, FacilityRole.Mana, FacilityWorkType.Operate | FacilityWorkType.Repair, VisualForm.Rune, Traits(FacilityEvolutionTerms.Mana, FacilityEvolutionTerms.Storage), capacity: 2),
            Support("M03", "룬 안정기", 1, GridLayer.Building, BuildingCategory.Resource, VisualForm.Rune, Traits(FacilityEvolutionTerms.Mana, FacilityEvolutionTerms.Security)),
            Core("M04", "의식 초점석", 1, GridLayer.Building, BuildingCategory.Resource, FacilityRole.Mana, FacilityWorkType.Operate | FacilityWorkType.Research | FacilityWorkType.Repair, VisualForm.Rune, Traits(FacilityEvolutionTerms.Mana, FacilityEvolutionTerms.Ritual), phase: 2),

            Core("T01", "훈련 허수아비", 1, GridLayer.Building, BuildingCategory.Special, FacilityRole.Training, FacilityWorkType.Operate | FacilityWorkType.Repair, VisualForm.Dummy, Traits(FacilityEvolutionTerms.Training, FacilityEvolutionTerms.Combat)),
            Core("T02", "사격 과녁", 1, GridLayer.Building, BuildingCategory.Special, FacilityRole.Training, FacilityWorkType.Operate | FacilityWorkType.Repair, VisualForm.Target, Traits(FacilityEvolutionTerms.Training, FacilityEvolutionTerms.Combat)),
            Core("T03", "중량 훈련석", 1, GridLayer.Building, BuildingCategory.Special, FacilityRole.Training, FacilityWorkType.Operate | FacilityWorkType.Repair, VisualForm.Weight, Traits(FacilityEvolutionTerms.Training)),
            Support("T04", "대련 매트", 2, GridLayer.FloorOverlay, BuildingCategory.Special, VisualForm.Mat, Traits(FacilityEvolutionTerms.Training, FacilityEvolutionTerms.Combat)),

            Core("G01", "경비 초소 책상", 2, GridLayer.Building, BuildingCategory.Special, FacilityRole.Security, FacilityWorkType.Guard | FacilityWorkType.Operate | FacilityWorkType.Repair, VisualForm.Desk, Traits("Guard", FacilityEvolutionTerms.Defense, FacilityEvolutionTerms.Security)),
            Core("G02", "경보 종", 1, GridLayer.WallFixture, BuildingCategory.Special, FacilityRole.Security, FacilityWorkType.Guard | FacilityWorkType.Repair, VisualForm.Bell, Traits(FacilityEvolutionTerms.Defense, FacilityEvolutionTerms.Security)),
            Support("G03", "순찰 상황판", 1, GridLayer.WallFixture, BuildingCategory.Special, VisualForm.Board, Traits(FacilityEvolutionTerms.Security, FacilityEvolutionTerms.Service), phase: 2),
            Support("G04", "전술 지도 탁자", 2, GridLayer.Building, BuildingCategory.Special, VisualForm.MapTable, Traits(FacilityEvolutionTerms.Combat, FacilityEvolutionTerms.Defense, FacilityEvolutionTerms.Service), phase: 2),
            Support("G05", "전투 깃발", 1, GridLayer.WallFixture, BuildingCategory.Special, VisualForm.Flag, Traits(FacilityEvolutionTerms.Combat, FacilityEvolutionTerms.Brutal), phase: 2),
            Support("G06", "전리품 거치대", 1, GridLayer.WallFixture, BuildingCategory.Special, VisualForm.WallRack, Traits(FacilityEvolutionTerms.Combat, FacilityEvolutionTerms.Luxury, FacilityEvolutionTerms.Fear), phase: 3),

            Core("L01", "대형 보관 선반", 2, GridLayer.Building, BuildingCategory.Production, FacilityRole.Logistics, FacilityWorkType.Restock | FacilityWorkType.Repair, VisualForm.Shelf, Traits(FacilityEvolutionTerms.Logistics, FacilityEvolutionTerms.Storage), capacity: 1, internalStockMax: 60),
            Support("L02", "상자 더미", 1, GridLayer.Building, BuildingCategory.Production, VisualForm.Crates, Traits(FacilityEvolutionTerms.Storage)),
            Support("L03", "통 더미", 1, GridLayer.Building, BuildingCategory.Production, VisualForm.Barrels, Traits(FacilityEvolutionTerms.Storage)),
            Support("L04", "자루 더미", 1, GridLayer.Building, BuildingCategory.Production, VisualForm.Sacks, Traits(FacilityEvolutionTerms.Storage)),
            Support("L05", "식재료 저장함", 1, GridLayer.Building, BuildingCategory.Production, VisualForm.Crates, Traits(FacilityEvolutionTerms.Logistics, "Cooking"), phase: 2),
            Support("L06", "무기 로커", 1, GridLayer.Building, BuildingCategory.Production, VisualForm.Cabinet, Traits(FacilityEvolutionTerms.Logistics, FacilityEvolutionTerms.Combat, FacilityEvolutionTerms.Security), phase: 2),
            Support("L07", "마력 보관함", 1, GridLayer.Building, BuildingCategory.Production, VisualForm.Cabinet, Traits(FacilityEvolutionTerms.Logistics, FacilityEvolutionTerms.Mana), phase: 2),

            Core("H01", "변기", 1, GridLayer.Building, BuildingCategory.Special, FacilityRole.Toilet, FacilityWorkType.Clean | FacilityWorkType.Repair, VisualForm.Toilet, Traits("Sanitation")),
            Support("H02", "화장실 칸막이", 1, GridLayer.Building, BuildingCategory.Special, VisualForm.Partition, Traits("Toilet", FacilityEvolutionTerms.Luxury)),
            Core("H03", "세면대", 1, GridLayer.Building, BuildingCategory.Special, FacilityRole.Hygiene, FacilityWorkType.Clean | FacilityWorkType.Repair, VisualForm.Sink, Traits(FacilityEvolutionTerms.Hygiene)),
            Core("H04", "목욕통", 2, GridLayer.Building, BuildingCategory.Special, FacilityRole.Hygiene | FacilityRole.Rest, FacilityWorkType.Clean | FacilityWorkType.Repair, VisualForm.Bath, Traits(FacilityEvolutionTerms.Hygiene, FacilityEvolutionTerms.Rest), phase: 2),
            Support("H05", "수건걸이", 1, GridLayer.WallFixture, BuildingCategory.Special, VisualForm.WallRack, Traits(FacilityEvolutionTerms.Hygiene, FacilityEvolutionTerms.Service)),
            Support("H06", "청소도구함", 1, GridLayer.Building, BuildingCategory.Special, VisualForm.Cabinet, Traits(FacilityEvolutionTerms.Hygiene, FacilityEvolutionTerms.Storage)),
            Support("H07", "바닥 배수구", 1, GridLayer.FloorOverlay, BuildingCategory.Special, VisualForm.Drain, Traits(FacilityEvolutionTerms.Hygiene), phase: 2),

            Support("E01", "벽 횃불", 1, GridLayer.WallFixture, BuildingCategory.Resource, VisualForm.Torch, Traits("Light")),
            Support("E02", "바닥 화로", 1, GridLayer.Building, BuildingCategory.Resource, VisualForm.Hearth, Traits("Light", FacilityEvolutionTerms.Rest), phase: 2),
            Support("E03", "샹들리에", 2, GridLayer.CeilingFixture, BuildingCategory.Resource, VisualForm.Chandelier, Traits("Light", FacilityEvolutionTerms.Luxury), phase: 2),
            Support("E04", "소형 러그", 2, GridLayer.FloorOverlay, BuildingCategory.Resource, VisualForm.Rug, Traits(FacilityEvolutionTerms.Luxury, FacilityEvolutionTerms.Quiet)),
            Support("E05", "세력 깃발", 1, GridLayer.WallFixture, BuildingCategory.Resource, VisualForm.Flag, Traits(FacilityEvolutionTerms.Combat)),
            Support("E06", "액자 초상화", 1, GridLayer.WallFixture, BuildingCategory.Resource, VisualForm.Portrait, Traits(FacilityEvolutionTerms.Luxury, FacilityEvolutionTerms.Noble), phase: 2),
            Support("E07", "촛대", 1, GridLayer.Building, BuildingCategory.Resource, VisualForm.Candlestick, Traits(FacilityEvolutionTerms.Quiet, FacilityEvolutionTerms.Luxury)),
            Support("E08", "해골 피 장식", 1, GridLayer.WallFixture, BuildingCategory.Resource, VisualForm.Skull, Traits(FacilityEvolutionTerms.Brutal, FacilityEvolutionTerms.Fear), phase: 3),
            Support("E09", "방 표지판", 1, GridLayer.WallFixture, BuildingCategory.Resource, VisualForm.Sign, Array.Empty<string>(), contributesToRoom: false, phase: 2)
        };
    }

    private static FacilityPartSpec Core(
        string code,
        string displayName,
        int width,
        GridLayer layer,
        BuildingCategory category,
        FacilityRole roles,
        FacilityWorkType workTypes,
        VisualForm form,
        string[] traits,
        FacilityEvolutionValue[] metrics = null,
        int capacity = 1,
        int internalStockMax = 0,
        int phase = 1,
        Type runtimeType = null)
    {
        return new FacilityPartSpec(
            code,
            displayName,
            width,
            layer,
            category,
            true,
            roles,
            workTypes,
            runtimeType ?? typeof(Facility),
            form,
            traits,
            metrics,
            capacity,
            internalStockMax,
            phase,
            true);
    }

    private static FacilityPartSpec Support(
        string code,
        string displayName,
        int width,
        GridLayer layer,
        BuildingCategory category,
        VisualForm form,
        string[] traits,
        FacilityEvolutionValue[] metrics = null,
        int phase = 1,
        bool contributesToRoom = true)
    {
        return new FacilityPartSpec(
            code,
            displayName,
            width,
            layer,
            category,
            false,
            FacilityRole.None,
            FacilityWorkType.None,
            typeof(Facility),
            form,
            traits,
            metrics,
            0,
            0,
            phase,
            contributesToRoom);
    }

    private static string[] Traits(params string[] values)
    {
        return values ?? Array.Empty<string>();
    }

    private static FacilityEvolutionValue[] Metrics(params (string key, float value)[] values)
    {
        return values?.Select((value) => new FacilityEvolutionValue(value.key, value.value)).ToArray()
            ?? Array.Empty<FacilityEvolutionValue>();
    }

    private static void DrawProp(PixelCanvas canvas, FacilityPartSpec spec)
    {
        Palette palette = Palette.ForCode(spec.Code);
        int variant = spec.Code[1] * 31 + spec.Code[2];
        int left = 2;
        int right = canvas.Width - 3;
        int center = canvas.Width / 2;

        if (spec.Layer == GridLayer.FloorOverlay)
        {
            canvas.FillRect(left, 3, right - left + 1, 8, palette.Dark);
            canvas.FillRect(left + 1, 4, right - left - 1, 6, palette.Accent);
        }

        switch (spec.Form)
        {
            case VisualForm.Hearth:
                DrawCabinet(canvas, left + 2, 5, right - left - 3, 16, palette);
                canvas.FillRect(center - 4, 8, 9, 8, palette.Dark);
                DrawFlame(canvas, center, 13, palette);
                break;
            case VisualForm.Grill:
                DrawCabinet(canvas, left + 1, 5, right - left - 1, 14, palette);
                canvas.FillRect(left + 3, 20, right - left - 5, 3, palette.Metal);
                canvas.Line(left + 5, 23, right - 5, 23, palette.Light);
                for (int x = left + 7; x < right - 4; x += 7) DrawFlame(canvas, x, 16, palette);
                break;
            case VisualForm.Counter:
                DrawCabinet(canvas, left + 1, 5, right - left - 1, 22, palette);
                canvas.FillRect(left, 27, right - left + 1, 4, palette.Light);
                canvas.FillRect(center - 2, 15, 5, 4, palette.Accent);
                break;
            case VisualForm.Table:
                canvas.FillRect(left + 1, 20, right - left - 1, 6, palette.Light);
                canvas.FillRect(left + 3, 17, right - left - 5, 3, palette.Wood);
                canvas.FillRect(left + 5, 6, 4, 14, palette.Dark);
                canvas.FillRect(right - 8, 6, 4, 14, palette.Dark);
                break;
            case VisualForm.Chair:
                canvas.FillRect(center - 7, 21, 14, 5, palette.Accent);
                canvas.FillRect(center - 7, 8, 4, 16, palette.Dark);
                canvas.FillRect(center + 3, 8, 4, 16, palette.Dark);
                canvas.OutlineRect(center - 8, 25, 16, 14, palette.Dark, palette.Light);
                break;
            case VisualForm.Bench:
                canvas.FillRect(left + 2, 19, right - left - 3, 6, palette.Light);
                canvas.FillRect(left + 5, 7, 4, 15, palette.Dark);
                canvas.FillRect(right - 8, 7, 4, 15, palette.Dark);
                canvas.FillRect(left + 3, 27, right - left - 5, 8, palette.Accent);
                break;
            case VisualForm.Shelf:
                DrawShelf(canvas, left + 2, 5, right - left - 3, 36, palette, variant);
                break;
            case VisualForm.WallRack:
                canvas.FillRect(left + 3, 34, right - left - 5, 4, palette.Wood);
                for (int x = left + 6; x < right - 3; x += 6)
                {
                    canvas.Line(x, 33, x - 2 + (variant % 3), 13, palette.Metal);
                    canvas.FillRect(x - 2, 10, 5, 5, palette.Accent);
                }
                break;
            case VisualForm.Cabinet:
                DrawCabinet(canvas, left + 3, 5, right - left - 5, 36, palette);
                canvas.Line(center, 7, center, 38, palette.Dark);
                canvas.FillRect(center - 3, 22, 2, 2, palette.Light);
                canvas.FillRect(center + 2, 22, 2, 2, palette.Light);
                break;
            case VisualForm.Workbench:
                canvas.FillRect(left + 1, 22, right - left - 1, 6, palette.Light);
                DrawCabinet(canvas, left + 4, 5, right - left - 7, 18, palette);
                canvas.Line(left + 6, 31, center - 2, 38, palette.Metal);
                canvas.Circle(right - 9, 33, 5, palette.Accent);
                break;
            case VisualForm.ArmorStand:
                canvas.FillRect(center - 2, 7, 5, 31, palette.Wood);
                canvas.FillRect(center - 8, 7, 17, 4, palette.Dark);
                canvas.OutlineRect(center - 8, 23, 17, 14, palette.Dark, palette.Metal);
                canvas.FillRect(center - 5, 36, 11, 5, palette.Accent);
                break;
            case VisualForm.Bed:
                canvas.FillRect(left + 1, 6, right - left - 1, 8, palette.Dark);
                canvas.FillRect(left + 2, 14, right - left - 3, 14, palette.Accent);
                canvas.FillRect(left + 4, 23, 12, 6, palette.Light);
                canvas.FillRect(left + 1, 5, 4, 25, palette.Wood);
                canvas.FillRect(right - 4, 5, 4, 12, palette.Wood);
                break;
            case VisualForm.BunkBed:
                DrawBedTier(canvas, left + 2, right - 2, 7, palette);
                DrawBedTier(canvas, left + 2, right - 2, 25, palette);
                canvas.FillRect(left, 4, 4, 39, palette.Wood);
                canvas.FillRect(right - 3, 4, 4, 39, palette.Wood);
                break;
            case VisualForm.Sofa:
                canvas.OutlineRect(left + 2, 10, right - left - 3, 18, palette.Dark, palette.Accent);
                canvas.FillRect(left + 4, 23, right - left - 7, 12, palette.Light);
                canvas.FillRect(left, 10, 6, 20, palette.Dark);
                canvas.FillRect(right - 5, 10, 6, 20, palette.Dark);
                break;
            case VisualForm.Desk:
                canvas.FillRect(left + 1, 21, right - left - 1, 6, palette.Light);
                DrawCabinet(canvas, left + 4, 5, 14, 18, palette);
                DrawCabinet(canvas, right - 17, 5, 14, 18, palette);
                canvas.FillRect(center - 6, 29, 12, 7, palette.Accent);
                break;
            case VisualForm.Rune:
                canvas.OutlineRect(left + 3, 5, right - left - 5, 31, palette.Dark, palette.Metal);
                canvas.Circle(center, 23, Mathf.Min(10, canvas.Width / 4), palette.Accent);
                canvas.Circle(center, 23, Mathf.Min(5, canvas.Width / 7), palette.Light);
                canvas.Line(center, 34, center, 42, palette.Light);
                break;
            case VisualForm.Dummy:
                canvas.FillRect(center - 2, 5, 5, 34, palette.Wood);
                canvas.FillRect(center - 9, 29, 19, 5, palette.Wood);
                canvas.Circle(center, 38, 6, palette.Accent);
                canvas.FillRect(center - 8, 5, 17, 4, palette.Dark);
                break;
            case VisualForm.Target:
                canvas.FillRect(center - 2, 5, 5, 28, palette.Wood);
                canvas.Circle(center, 31, 11, palette.Light);
                canvas.Circle(center, 31, 7, palette.Accent);
                canvas.Circle(center, 31, 3, palette.Dark);
                canvas.FillRect(center - 9, 5, 19, 4, palette.Dark);
                break;
            case VisualForm.Weight:
                canvas.FillRect(center - 2, 7, 5, 25, palette.Metal);
                canvas.FillRect(center - 10, 26, 21, 5, palette.Light);
                canvas.FillRect(center - 11, 18, 5, 18, palette.Dark);
                canvas.FillRect(center + 7, 18, 5, 18, palette.Dark);
                canvas.FillRect(center - 10, 5, 21, 4, palette.Dark);
                break;
            case VisualForm.Mat:
            case VisualForm.Rug:
                canvas.OutlineRect(left + 1, 4, right - left - 1, 12, palette.Dark, palette.Accent);
                for (int x = left + 5; x < right - 3; x += 7) canvas.FillRect(x, 8, 3, 3, palette.Light);
                break;
            case VisualForm.Bell:
                canvas.FillRect(center - 2, 28, 5, 13, palette.Wood);
                canvas.FillRect(center - 9, 38, 19, 4, palette.Wood);
                canvas.Circle(center, 25, 10, palette.Light);
                canvas.FillRect(center - 11, 18, 23, 5, palette.Accent);
                canvas.FillRect(center - 2, 14, 5, 6, palette.Dark);
                break;
            case VisualForm.Board:
            case VisualForm.Portrait:
                canvas.OutlineRect(left + 3, 15, right - left - 5, 25, palette.Dark, palette.Wood);
                canvas.FillRect(left + 6, 18, right - left - 11, 19, palette.Accent);
                canvas.Line(left + 8, 20, right - 8, 34, palette.Light);
                canvas.Line(right - 8, 20, left + 8, 34, palette.Light);
                break;
            case VisualForm.MapTable:
                canvas.FillRect(left + 1, 18, right - left - 1, 8, palette.Wood);
                canvas.FillRect(left + 4, 22, right - left - 7, 6, palette.Accent);
                canvas.Line(left + 8, 23, center, 27, palette.Light);
                canvas.Line(center, 27, right - 8, 23, palette.Light);
                canvas.FillRect(left + 6, 5, 4, 15, palette.Dark);
                canvas.FillRect(right - 9, 5, 4, 15, palette.Dark);
                break;
            case VisualForm.Flag:
                canvas.FillRect(left + 4, 9, 3, 34, palette.Metal);
                canvas.FillRect(left + 7, 23, right - left - 9, 18, palette.Accent);
                canvas.Triangle(right - 2, 23, right - 2, 41, right - 10, 32, palette.Dark);
                canvas.FillRect(center - 2, 29, 5, 5, palette.Light);
                break;
            case VisualForm.Crates:
                DrawCrate(canvas, left + 2, 5, Mathf.Min(18, canvas.Width - 5), 18, palette);
                if (canvas.Width > 30) DrawCrate(canvas, center, 8, Mathf.Min(20, right - center), 20, palette);
                DrawCrate(canvas, center - 8, 23, 18, 17, palette);
                break;
            case VisualForm.Barrels:
                DrawBarrel(canvas, center - (canvas.Width > 30 ? 12 : 0), 5, palette);
                if (canvas.Width > 30) DrawBarrel(canvas, center + 12, 5, palette);
                break;
            case VisualForm.Sacks:
                canvas.Circle(center - 5, 15, 10, palette.Wood);
                canvas.Circle(center + 6, 13, 9, palette.Accent);
                canvas.FillRect(center - 8, 23, 7, 5, palette.Dark);
                canvas.FillRect(center + 3, 21, 6, 5, palette.Dark);
                break;
            case VisualForm.Toilet:
                canvas.OutlineRect(center - 7, 7, 15, 13, palette.Dark, palette.Light);
                canvas.OutlineRect(center - 9, 19, 19, 8, palette.Dark, palette.Metal);
                canvas.FillRect(center - 7, 27, 15, 12, palette.Light);
                canvas.FillRect(center - 9, 37, 19, 4, palette.Dark);
                break;
            case VisualForm.Partition:
                canvas.OutlineRect(center - 9, 5, 19, 37, palette.Dark, palette.Accent);
                canvas.FillRect(center - 1, 8, 3, 31, palette.Light);
                canvas.FillRect(center + 4, 22, 3, 3, palette.Dark);
                break;
            case VisualForm.Sink:
                canvas.FillRect(center - 2, 5, 5, 20, palette.Metal);
                canvas.OutlineRect(center - 10, 22, 21, 9, palette.Dark, palette.Light);
                canvas.Line(center, 31, center, 39, palette.Metal);
                canvas.Line(center, 39, center + 6, 39, palette.Metal);
                canvas.FillRect(center + 4, 35, 3, 5, palette.Accent);
                break;
            case VisualForm.Bath:
                canvas.OutlineRect(left + 1, 7, right - left - 1, 18, palette.Dark, palette.Wood);
                canvas.FillRect(left + 4, 12, right - left - 7, 10, palette.Accent);
                canvas.Line(right - 8, 25, right - 8, 36, palette.Metal);
                canvas.Line(right - 8, 36, right - 3, 36, palette.Metal);
                break;
            case VisualForm.Drain:
                canvas.OutlineRect(center - 8, 4, 17, 10, palette.Dark, palette.Metal);
                for (int x = center - 5; x <= center + 5; x += 5) canvas.Line(x, 6, x, 11, palette.Dark);
                break;
            case VisualForm.Torch:
                canvas.Line(center, 13, center, 33, palette.Wood);
                canvas.Line(center - 5, 18, center + 5, 28, palette.Metal);
                DrawFlame(canvas, center, 37, palette);
                break;
            case VisualForm.Chandelier:
                canvas.Line(center, 45, center, 31, palette.Metal);
                canvas.Line(left + 7, 25, right - 7, 25, palette.Metal);
                canvas.Line(left + 7, 25, center, 31, palette.Metal);
                canvas.Line(right - 7, 25, center, 31, palette.Metal);
                DrawFlame(canvas, left + 7, 29, palette);
                DrawFlame(canvas, right - 7, 29, palette);
                DrawFlame(canvas, center, 34, palette);
                break;
            case VisualForm.Candlestick:
                canvas.FillRect(center - 7, 5, 15, 4, palette.Metal);
                canvas.FillRect(center - 2, 8, 5, 24, palette.Light);
                DrawFlame(canvas, center, 37, palette);
                break;
            case VisualForm.Skull:
                canvas.Circle(center, 29, 10, palette.Light);
                canvas.FillRect(center - 7, 17, 15, 10, palette.Light);
                canvas.FillRect(center - 6, 29, 4, 5, palette.Dark);
                canvas.FillRect(center + 3, 29, 4, 5, palette.Dark);
                canvas.FillRect(center - 1, 24, 3, 4, palette.Dark);
                canvas.Line(left + 4, 12, right - 4, 39, palette.Accent);
                break;
            case VisualForm.Sign:
                canvas.FillRect(center - 2, 9, 5, 30, palette.Wood);
                canvas.OutlineRect(left + 3, 23, right - left - 5, 17, palette.Dark, palette.Light);
                canvas.Triangle(right - 5, 26, right - 5, 36, right, 31, palette.Accent);
                break;
        }

        if (spec.Layer == GridLayer.Building)
        {
            canvas.FillRect(left, 3, right - left + 1, 2, palette.Shadow);
        }
        canvas.Set(2 + variant % Mathf.Max(1, canvas.Width - 4), 44, palette.Accent);
    }

    private static void DrawShelf(PixelCanvas canvas, int x, int y, int width, int height, Palette palette, int variant)
    {
        canvas.OutlineRect(x, y, width, height, palette.Dark, palette.Wood);
        for (int shelfY = y + 10; shelfY < y + height - 2; shelfY += 10)
        {
            canvas.FillRect(x + 2, shelfY, width - 4, 3, palette.Dark);
            for (int itemX = x + 4; itemX < x + width - 4; itemX += 5)
            {
                Color32 color = ((itemX + shelfY + variant) & 1) == 0 ? palette.Accent : palette.Light;
                canvas.FillRect(itemX, shelfY + 3, 3, 5 + ((itemX + variant) % 4), color);
            }
        }
    }

    private static void DrawCabinet(PixelCanvas canvas, int x, int y, int width, int height, Palette palette)
    {
        canvas.OutlineRect(x, y, width, height, palette.Dark, palette.Wood);
        canvas.FillRect(x + 2, y + height - 5, width - 4, 3, palette.Light);
        canvas.FillRect(x + width / 2 - 1, y + height / 2, 3, 3, palette.Accent);
    }

    private static void DrawFlame(PixelCanvas canvas, int x, int y, Palette palette)
    {
        canvas.Triangle(x, y + 7, x - 5, y - 2, x + 5, y - 2, palette.Accent);
        canvas.Triangle(x, y + 4, x - 2, y - 2, x + 3, y - 2, palette.Light);
    }

    private static void DrawBedTier(PixelCanvas canvas, int left, int right, int y, Palette palette)
    {
        canvas.FillRect(left, y, right - left + 1, 5, palette.Dark);
        canvas.FillRect(left + 1, y + 5, right - left - 1, 9, palette.Accent);
        canvas.FillRect(left + 3, y + 9, 9, 5, palette.Light);
    }

    private static void DrawCrate(PixelCanvas canvas, int x, int y, int width, int height, Palette palette)
    {
        canvas.OutlineRect(x, y, width, height, palette.Dark, palette.Wood);
        canvas.Line(x + 2, y + 2, x + width - 3, y + height - 3, palette.Light);
        canvas.Line(x + width - 3, y + 2, x + 2, y + height - 3, palette.Light);
    }

    private static void DrawBarrel(PixelCanvas canvas, int center, int y, Palette palette)
    {
        canvas.OutlineRect(center - 8, y + 3, 17, 29, palette.Dark, palette.Wood);
        canvas.FillRect(center - 9, y + 8, 19, 4, palette.Metal);
        canvas.FillRect(center - 9, y + 24, 19, 4, palette.Metal);
        canvas.FillRect(center - 6, y + 4, 13, 3, palette.Light);
        canvas.FillRect(center - 6, y + 29, 13, 3, palette.Light);
    }

    private static void EnsureFolder(string assetPath)
    {
        string[] parts = assetPath.Split('/');
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

    private sealed class FacilityPartSpec
    {
        public FacilityPartSpec(
            string code,
            string displayName,
            int width,
            GridLayer layer,
            BuildingCategory category,
            bool core,
            FacilityRole roles,
            FacilityWorkType workTypes,
            Type runtimeType,
            VisualForm form,
            string[] traits,
            FacilityEvolutionValue[] metrics,
            int capacity,
            int internalStockMax,
            int phase,
            bool contributesToRoom)
        {
            Code = code;
            DisplayName = displayName;
            AssetName = displayName.Replace(" ", string.Empty);
            Width = width;
            Layer = layer;
            Category = category;
            Core = core;
            Roles = roles;
            WorkTypes = workTypes;
            RuntimeType = runtimeType;
            Form = form;
            Traits = traits;
            Metrics = metrics;
            Capacity = capacity;
            InternalStockMax = internalStockMax;
            Phase = phase;
            ContributesToRoom = contributesToRoom;
            UseDuration = core ? 1.5f : 0f;
        }

        public string Code { get; }
        public string DisplayName { get; }
        public string AssetName { get; }
        public int Width { get; }
        public GridLayer Layer { get; }
        public BuildingCategory Category { get; }
        public bool Core { get; }
        public FacilityRole Roles { get; }
        public FacilityWorkType WorkTypes { get; }
        public Type RuntimeType { get; }
        public VisualForm Form { get; }
        public string[] Traits { get; }
        public FacilityEvolutionValue[] Metrics { get; }
        public int Capacity { get; }
        public int InternalStockMax { get; }
        public int Phase { get; }
        public bool ContributesToRoom { get; }
        public float UseDuration { get; }
    }

    private enum VisualForm
    {
        Hearth,
        Grill,
        Counter,
        Table,
        Chair,
        Bench,
        Shelf,
        WallRack,
        Cabinet,
        Workbench,
        ArmorStand,
        Bed,
        BunkBed,
        Sofa,
        Desk,
        Rune,
        Dummy,
        Target,
        Weight,
        Mat,
        Bell,
        Board,
        MapTable,
        Flag,
        Crates,
        Barrels,
        Sacks,
        Toilet,
        Partition,
        Sink,
        Bath,
        Drain,
        Torch,
        Chandelier,
        Rug,
        Portrait,
        Candlestick,
        Skull,
        Sign
    }

    private readonly struct Palette
    {
        public Palette(Color32 dark, Color32 shadow, Color32 wood, Color32 metal, Color32 accent, Color32 light)
        {
            Dark = dark;
            Shadow = shadow;
            Wood = wood;
            Metal = metal;
            Accent = accent;
            Light = light;
        }

        public Color32 Dark { get; }
        public Color32 Shadow { get; }
        public Color32 Wood { get; }
        public Color32 Metal { get; }
        public Color32 Accent { get; }
        public Color32 Light { get; }

        public static Palette ForCode(string code)
        {
            Color32 accent = code[0] switch
            {
                'D' => new Color32(190, 84, 48, 255),
                'S' => new Color32(197, 154, 56, 255),
                'R' => new Color32(66, 137, 147, 255),
                'Q' => new Color32(59, 151, 112, 255),
                'M' => new Color32(117, 79, 177, 255),
                'T' => new Color32(170, 66, 58, 255),
                'G' => new Color32(92, 109, 135, 255),
                'L' => new Color32(123, 107, 66, 255),
                'H' => new Color32(63, 157, 171, 255),
                _ => new Color32(186, 136, 73, 255)
            };

            return new Palette(
                new Color32(27, 25, 37, 255),
                new Color32(13, 16, 25, 180),
                new Color32(103, 71, 54, 255),
                new Color32(105, 113, 126, 255),
                accent,
                new Color32(218, 202, 164, 255));
        }
    }

    private sealed class PixelCanvas
    {
        private readonly Color32[] pixels;

        public PixelCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            pixels = new Color32[width * height];
        }

        public int Width { get; }
        public int Height { get; }
        public Color32[] Pixels => pixels;

        public void Set(int x, int y, Color32 color)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return;
            }

            pixels[y * Width + x] = color;
        }

        public void FillRect(int x, int y, int width, int height, Color32 color)
        {
            for (int py = y; py < y + height; py++)
            {
                for (int px = x; px < x + width; px++)
                {
                    Set(px, py, color);
                }
            }
        }

        public void OutlineRect(int x, int y, int width, int height, Color32 outline, Color32 fill)
        {
            FillRect(x, y, width, height, outline);
            FillRect(x + 2, y + 2, Mathf.Max(0, width - 4), Mathf.Max(0, height - 4), fill);
        }

        public void Line(int x0, int y0, int x1, int y1, Color32 color)
        {
            int dx = Mathf.Abs(x1 - x0);
            int sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0);
            int sy = y0 < y1 ? 1 : -1;
            int error = dx + dy;
            while (true)
            {
                Set(x0, y0, color);
                if (x0 == x1 && y0 == y1) break;
                int twice = 2 * error;
                if (twice >= dy)
                {
                    error += dy;
                    x0 += sx;
                }

                if (twice <= dx)
                {
                    error += dx;
                    y0 += sy;
                }
            }
        }

        public void Circle(int centerX, int centerY, int radius, Color32 color)
        {
            int radiusSquared = radius * radius;
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y <= radiusSquared)
                    {
                        Set(centerX + x, centerY + y, color);
                    }
                }
            }
        }

        public void Triangle(int x0, int y0, int x1, int y1, int x2, int y2, Color32 color)
        {
            int minX = Mathf.Min(x0, Mathf.Min(x1, x2));
            int maxX = Mathf.Max(x0, Mathf.Max(x1, x2));
            int minY = Mathf.Min(y0, Mathf.Min(y1, y2));
            int maxY = Mathf.Max(y0, Mathf.Max(y1, y2));
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int a = (x1 - x0) * (y - y0) - (y1 - y0) * (x - x0);
                    int b = (x2 - x1) * (y - y1) - (y2 - y1) * (x - x1);
                    int c = (x0 - x2) * (y - y2) - (y0 - y2) * (x - x2);
                    if ((a >= 0 && b >= 0 && c >= 0) || (a <= 0 && b <= 0 && c <= 0))
                    {
                        Set(x, y, color);
                    }
                }
            }
        }
    }
}
