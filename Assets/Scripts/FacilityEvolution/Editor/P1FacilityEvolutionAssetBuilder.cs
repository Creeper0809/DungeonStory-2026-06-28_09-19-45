using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class P1FacilityEvolutionAssetBuilder
{
    private const string BuildingFolder = "Assets/Resources/SO/Building/P1";
    private const string RecipeFolder = "Assets/Resources/SO/FacilityEvolution/P1";
    private const string TokenDefinitionFolder = "Assets/Resources/SO/FacilityEvolution/RecordTokens/P1";

    [MenuItem("DungeonStory/Debug/Facility Evolution/Ensure P1 Evolution Assets")]
    public static void EnsureP1EvolutionAssetsFromMenu()
    {
        EnsureP1EvolutionAssets();
    }

    public static void EnsureP1EvolutionAssets()
    {
        AssetDatabase.Refresh();
        P1FacilitySynthesisAssetBuilder.EnsureP1SynthesisAssets();
        EnsureFolder(RecipeFolder);
        EnsureFolder(TokenDefinitionFolder);
        ApplyFacilityContributions();
        EnsureRecordTokenDefinitionAssets();

        foreach (EvolutionRecipeSpec spec in CreateRecipeSpecs())
        {
            EnsureRecipeAsset(spec);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void ApplyFacilityContributions()
    {
        ApplyContribution(
            "P1_MeatRestaurant",
            tags: new[] { FacilityEvolutionTerms.Dining, FacilityEvolutionTerms.Cooking, FacilityEvolutionTerms.Meat },
            scores: new[]
            {
                Value(FacilityEvolutionTerms.Dining, 30f),
                Value(FacilityEvolutionTerms.Cooking, 24f),
                Value(FacilityEvolutionTerms.Meat, 24f),
                Value(FacilityEvolutionTerms.Service, 6f)
            },
            metrics: new[]
            {
                Value(FacilityEvolutionTerms.SeatCount, 4f),
                Value(FacilityEvolutionTerms.TableCount, 1f)
            });

        ApplyContribution(
            "P1_BattleDining",
            tags: new[] { FacilityEvolutionTerms.Dining, FacilityEvolutionTerms.Combat, FacilityEvolutionTerms.Meat },
            scores: new[]
            {
                Value(FacilityEvolutionTerms.Dining, 34f),
                Value(FacilityEvolutionTerms.Cooking, 20f),
                Value(FacilityEvolutionTerms.Meat, 24f),
                Value(FacilityEvolutionTerms.Combat, 18f),
                Value(FacilityEvolutionTerms.Training, 10f)
            },
            metrics: new[]
            {
                Value(FacilityEvolutionTerms.SeatCount, 4f),
                Value(FacilityEvolutionTerms.TableCount, 1f),
                Value(FacilityEvolutionTerms.LargeTableCount, 1f)
            });

        ApplyContribution(
            "P1_PremiumMeatRestaurant",
            tags: new[] { FacilityEvolutionTerms.Dining, FacilityEvolutionTerms.Meat, FacilityEvolutionTerms.Luxury, FacilityEvolutionTerms.Quiet },
            scores: new[]
            {
                Value(FacilityEvolutionTerms.Dining, 32f),
                Value(FacilityEvolutionTerms.Cooking, 18f),
                Value(FacilityEvolutionTerms.Meat, 18f),
                Value(FacilityEvolutionTerms.Luxury, 24f),
                Value(FacilityEvolutionTerms.Service, 14f),
                Value(FacilityEvolutionTerms.Rest, 10f)
            },
            metrics: new[]
            {
                Value(FacilityEvolutionTerms.SeatCount, 3f),
                Value(FacilityEvolutionTerms.TableCount, 1f),
                Value(FacilityEvolutionTerms.PrivateSeatCount, 2f)
            });

        ApplyContribution(
            "P1_BattlefieldDining",
            tags: new[] { FacilityEvolutionTerms.Dining, FacilityEvolutionTerms.Combat, FacilityEvolutionTerms.Defense, FacilityEvolutionTerms.Meat },
            scores: new[]
            {
                Value(FacilityEvolutionTerms.Dining, 38f),
                Value(FacilityEvolutionTerms.Cooking, 22f),
                Value(FacilityEvolutionTerms.Combat, 32f),
                Value(FacilityEvolutionTerms.Defense, 18f),
                Value(FacilityEvolutionTerms.Training, 18f)
            },
            metrics: new[]
            {
                Value(FacilityEvolutionTerms.SeatCount, 5f),
                Value(FacilityEvolutionTerms.TableCount, 1f),
                Value(FacilityEvolutionTerms.LargeTableCount, 1f)
            });

        ApplyContribution(
            "P1_NobleDining",
            tags: new[] { FacilityEvolutionTerms.Dining, FacilityEvolutionTerms.Luxury, FacilityEvolutionTerms.Noble, FacilityEvolutionTerms.Mana },
            scores: new[]
            {
                Value(FacilityEvolutionTerms.Dining, 36f),
                Value(FacilityEvolutionTerms.Luxury, 38f),
                Value(FacilityEvolutionTerms.Service, 20f),
                Value(FacilityEvolutionTerms.Rest, 16f),
                Value(FacilityEvolutionTerms.Mana, 18f)
            },
            metrics: new[]
            {
                Value(FacilityEvolutionTerms.SeatCount, 3f),
                Value(FacilityEvolutionTerms.TableCount, 1f),
                Value(FacilityEvolutionTerms.PrivateSeatCount, 3f)
            });

        ApplyContribution(
            "P1_TrainingRoom",
            tags: new[] { FacilityEvolutionTerms.Training, FacilityEvolutionTerms.Combat },
            scores: new[]
            {
                Value(FacilityEvolutionTerms.Training, 30f),
                Value(FacilityEvolutionTerms.Combat, 16f)
            },
            metrics: Array.Empty<FacilityEvolutionValue>());

        ApplyContribution(
            "P1_Barracks",
            tags: new[] { FacilityEvolutionTerms.Training, FacilityEvolutionTerms.Combat, FacilityEvolutionTerms.Defense, FacilityEvolutionTerms.Security },
            scores: new[]
            {
                Value(FacilityEvolutionTerms.Training, 24f),
                Value(FacilityEvolutionTerms.Combat, 24f),
                Value(FacilityEvolutionTerms.Defense, 18f)
            },
            metrics: Array.Empty<FacilityEvolutionValue>());

        ApplyContribution(
            "P1_RestRoom",
            tags: new[] { FacilityEvolutionTerms.Rest, FacilityEvolutionTerms.Quiet, FacilityEvolutionTerms.Service },
            scores: new[]
            {
                Value(FacilityEvolutionTerms.Rest, 26f),
                Value(FacilityEvolutionTerms.Service, 10f),
                Value(FacilityEvolutionTerms.Luxury, 8f)
            },
            metrics: Array.Empty<FacilityEvolutionValue>());

        ApplyContribution(
            "P1_ManaStorage",
            tags: new[] { FacilityEvolutionTerms.Mana, FacilityEvolutionTerms.Luxury },
            scores: new[]
            {
                Value(FacilityEvolutionTerms.Mana, 24f),
                Value(FacilityEvolutionTerms.Luxury, 10f)
            },
            metrics: Array.Empty<FacilityEvolutionValue>());
    }

    private static void ApplyContribution(
        string assetName,
        string[] tags,
        FacilityEvolutionValue[] scores,
        FacilityEvolutionValue[] metrics)
    {
        BuildingSO building = LoadBuilding(assetName);
        if (building == null)
        {
            return;
        }

        building.evolution = new FacilityEvolutionContributionData
        {
            contributesToRoomProfile = true,
            tags = tags ?? Array.Empty<string>(),
            scores = scores ?? Array.Empty<FacilityEvolutionValue>(),
            metrics = metrics ?? Array.Empty<FacilityEvolutionValue>()
        };
        EditorUtility.SetDirty(building);
    }

    private static void EnsureRecordTokenDefinitionAssets()
    {
        foreach (RecordTokenDefinitionSpec spec in CreateRecordTokenDefinitionSpecs())
        {
            string assetPath = $"{TokenDefinitionFolder}/{spec.assetName}.asset";
            FacilityEvolutionRecordTokenDefinitionSO definition =
                AssetDatabase.LoadAssetAtPath<FacilityEvolutionRecordTokenDefinitionSO>(assetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<FacilityEvolutionRecordTokenDefinitionSO>();
                AssetDatabase.CreateAsset(definition, assetPath);
            }

            definition.id = spec.id;
            definition.tokenId = spec.tokenId;
            definition.displayName = spec.displayName;
            definition.description = spec.description;
            definition.sourceMetric = spec.sourceMetric;
            definition.threshold = spec.threshold;
            definition.decayPolicy = spec.decayPolicy;
            definition.consumePolicy = spec.consumePolicy;
            definition.recipeTags = spec.recipeTags ?? Array.Empty<string>();
            definition.uiHint = spec.uiHint;
            EditorUtility.SetDirty(definition);
        }
    }

    private static void EnsureRecipeAsset(EvolutionRecipeSpec spec)
    {
        string assetPath = $"{RecipeFolder}/{spec.assetName}.asset";
        FacilityEvolutionRecipeSO recipe = AssetDatabase.LoadAssetAtPath<FacilityEvolutionRecipeSO>(assetPath);
        if (recipe == null)
        {
            recipe = ScriptableObject.CreateInstance<FacilityEvolutionRecipeSO>();
            AssetDatabase.CreateAsset(recipe, assetPath);
        }

        recipe.id = spec.id;
        recipe.evolutionId = spec.evolutionId;
        recipe.displayName = spec.displayName;
        recipe.description = spec.description;
        recipe.resultBuilding = LoadBuilding(spec.resultAssetName);
        recipe.fromFacilities = spec.sourceAssetNames
            .Select(LoadBuilding)
            .Where((building) => building != null)
            .ToArray();
        recipe.fromLineageTags = spec.sourceLineageTags ?? Array.Empty<string>();
        recipe.requiredStarGrade = spec.requiredStarGrade;
        recipe.resultStarGrade = spec.resultStarGrade;
        recipe.publicByDefault = spec.publicByDefault;
        recipe.requiredResearchRecipeId = spec.requiredResearchRecipeId;
        recipe.requireUsableRoom = true;
        recipe.requiredRoomScores = spec.requiredRoomScores ?? Array.Empty<FacilityEvolutionMetricRequirement>();
        recipe.requiredRoomMetrics = spec.requiredRoomMetrics ?? Array.Empty<FacilityEvolutionMetricRequirement>();
        recipe.requiredRoomTags = spec.requiredRoomTags ?? Array.Empty<string>();
        recipe.requiredUniqueFixtures = spec.requiredUniqueFixtureAssetNames
            .Select(LoadBuilding)
            .Where((building) => building != null)
            .ToArray();
        recipe.requiredRecordTokens = spec.requiredRecordTokens ?? Array.Empty<FacilityEvolutionTokenRequirement>();
        recipe.requiredMaterials = spec.requiredMaterials ?? Array.Empty<FacilityEvolutionMaterialRequirement>();
        recipe.allowedMutationTags = spec.allowedMutationTags ?? Array.Empty<string>();
        recipe.consumeRecordTokens = spec.consumeRecordTokens;
        recipe.identityPressureWeights = spec.identityPressureWeights ?? Array.Empty<FacilityEvolutionValue>();
        recipe.minimumIdentityScore = spec.minimumIdentityScore;
        EditorUtility.SetDirty(recipe);
    }

    private static EvolutionRecipeSpec[] CreateRecipeSpecs()
    {
        return new[]
        {
            new EvolutionRecipeSpec(
                9101,
                "EV_BattleDining",
                "evolve_meat_restaurant_to_battle_dining",
                "전투 식당 진화",
                "고기 식당이 훈련/전투 맥락과 용병 이용 기록을 받아 전투 식당 계보로 진화한다.",
                "P1_BattleDining",
                new[] { "P1_MeatRestaurant" },
                requiredStarGrade: 1,
                resultStarGrade: 2,
                publicByDefault: true,
                requiredResearchRecipeId: string.Empty,
                requiredRoomScores: new[]
                {
                    Min(FacilityEvolutionTerms.Dining, 40f),
                    Min(FacilityEvolutionTerms.Training, 15f)
                },
                requiredRoomMetrics: new[]
                {
                    Min(FacilityEvolutionTerms.SeatDensity, 0.25f),
                    Max(FacilityEvolutionTerms.SeatDensity, 1.4f)
                },
                requiredRecordTokens: new[]
                {
                    Token(FacilityEvolutionTerms.MercenaryHangout, 1)
                },
                allowedMutationTags: new[] { FacilityEvolutionTerms.Brutal, FacilityEvolutionTerms.Combat },
                identityPressureWeights: new[]
                {
                    Value(FacilityEvolutionTerms.Combat, 0.6f),
                    Value(FacilityEvolutionTerms.Crowd, 0.15f),
                    Value(FacilityEvolutionTerms.Service, 0.1f),
                    Value(FacilityEvolutionTerms.Luxury, -0.2f)
                },
                minimumIdentityScore: 0.35f),

            new EvolutionRecipeSpec(
                9102,
                "EV_PremiumMeatRestaurant",
                "evolve_meat_restaurant_to_premium_meat_restaurant",
                "고급 고기 식당 진화",
                "고기 식당이 여유로운 좌석, 휴식/서비스 맥락, 귀족 이용 기록을 받아 고급 식당 계보로 진화한다.",
                "P1_PremiumMeatRestaurant",
                new[] { "P1_MeatRestaurant" },
                requiredStarGrade: 1,
                resultStarGrade: 2,
                publicByDefault: true,
                requiredResearchRecipeId: string.Empty,
                requiredRoomScores: new[]
                {
                    Min(FacilityEvolutionTerms.Dining, 35f),
                    Min(FacilityEvolutionTerms.Luxury, 12f)
                },
                requiredRoomMetrics: new[]
                {
                    Max(FacilityEvolutionTerms.SeatDensity, 0.75f),
                    Min(FacilityEvolutionTerms.LuxuryPerSeat, 1.2f)
                },
                requiredRecordTokens: new[]
                {
                    Token(FacilityEvolutionTerms.NoblePatronage, 1)
                },
                allowedMutationTags: new[] { FacilityEvolutionTerms.Luxury, FacilityEvolutionTerms.Noble },
                identityPressureWeights: new[]
                {
                    Value(FacilityEvolutionTerms.Luxury, 0.55f),
                    Value(FacilityEvolutionTerms.Service, 0.2f),
                    Value(FacilityEvolutionTerms.Rest, 0.15f),
                    Value(FacilityEvolutionTerms.Crowd, -0.2f),
                    Value(FacilityEvolutionTerms.Outlaw, -0.2f)
                },
                minimumIdentityScore: 0.35f),

            new EvolutionRecipeSpec(
                9103,
                "EV_BattlefieldDining",
                "evolve_battle_dining_to_battlefield_dining",
                "전장의 식당 진화",
                "전투 식당이 경비 집결과 침입 방어 기록을 쌓아 전장의 식당 계보로 진화한다.",
                "P1_BattlefieldDining",
                new[] { "P1_BattleDining" },
                requiredStarGrade: 2,
                resultStarGrade: 3,
                publicByDefault: true,
                requiredResearchRecipeId: string.Empty,
                requiredRoomScores: new[]
                {
                    Min(FacilityEvolutionTerms.Combat, 34f),
                    Min(FacilityEvolutionTerms.Defense, 12f)
                },
                requiredRoomMetrics: Array.Empty<FacilityEvolutionMetricRequirement>(),
                requiredRecordTokens: new[]
                {
                    Token(FacilityEvolutionTerms.GuardRallyPoint, 1),
                    Token(FacilityEvolutionTerms.IntruderBloodied, 1)
                },
                allowedMutationTags: new[] { FacilityEvolutionTerms.Brutal, FacilityEvolutionTerms.Security },
                identityPressureWeights: new[]
                {
                    Value(FacilityEvolutionTerms.Combat, 0.4f),
                    Value(FacilityEvolutionTerms.Security, 0.45f),
                    Value(FacilityEvolutionTerms.Service, 0.1f)
                },
                minimumIdentityScore: 0.4f),

            new EvolutionRecipeSpec(
                9104,
                "EV_NobleDining",
                "evolve_premium_meat_restaurant_to_noble_dining",
                "귀족의 식당 진화",
                "고급 고기 식당이 마력/서비스/귀족 후원 기록을 받아 귀족의 식당 계보로 진화한다.",
                "P1_NobleDining",
                new[] { "P1_PremiumMeatRestaurant" },
                requiredStarGrade: 2,
                resultStarGrade: 3,
                publicByDefault: true,
                requiredResearchRecipeId: string.Empty,
                requiredRoomScores: new[]
                {
                    Min(FacilityEvolutionTerms.Luxury, 32f),
                    Min(FacilityEvolutionTerms.Mana, 12f)
                },
                requiredRoomMetrics: new[]
                {
                    Min(FacilityEvolutionTerms.LuxuryPerSeat, 2f)
                },
                requiredRecordTokens: new[]
                {
                    Token(FacilityEvolutionTerms.NoblePatronage, 2),
                    Token(FacilityEvolutionTerms.CleanServiceStreak, 1)
                },
                allowedMutationTags: new[] { FacilityEvolutionTerms.Luxury, FacilityEvolutionTerms.Noble },
                identityPressureWeights: new[]
                {
                    Value(FacilityEvolutionTerms.Luxury, 0.5f),
                    Value(FacilityEvolutionTerms.Service, 0.2f),
                    Value(FacilityEvolutionTerms.Rest, 0.15f),
                    Value(FacilityEvolutionTerms.Ritual, 0.1f),
                    Value(FacilityEvolutionTerms.Outlaw, -0.25f)
                },
                minimumIdentityScore: 0.42f)
        };
    }

    private static RecordTokenDefinitionSpec[] CreateRecordTokenDefinitionSpecs()
    {
        return new[]
        {
            new RecordTokenDefinitionSpec(
                9201,
                "RT_MercenaryHangout",
                FacilityEvolutionTerms.MercenaryHangout,
                "용병 단골화",
                "용병 손님이 반복해서 이용한 기록이다. 전투/연회 계보의 역사 증거로 남긴다.",
                FacilityEvolutionTerms.CombatVisitorRatio,
                0.25f,
                "recent+long-term",
                FacilityEvolutionRecordTokenConsumePolicy.Preserve,
                new[] { FacilityEvolutionTerms.Combat, FacilityEvolutionTerms.Dining },
                "용병 손님이 이 시설의 분위기를 바꾸고 있습니다."),

            new RecordTokenDefinitionSpec(
                9202,
                "RT_HighMeatConsumption",
                FacilityEvolutionTerms.HighMeatConsumption,
                "고기 소비 과다",
                "고기 재고를 많이 소비한 기록이다. 식당, 연회, 전투 식당 계보에 영향을 준다.",
                FacilityEvolutionTerms.StockCostPerVisit,
                1f,
                "daily",
                FacilityEvolutionRecordTokenConsumePolicy.Preserve,
                new[] { FacilityEvolutionTerms.Meat, FacilityEvolutionTerms.Dining },
                "고기 소비가 시설의 정체성으로 굳어지고 있습니다."),

            new RecordTokenDefinitionSpec(
                9203,
                "RT_FrequentBrawls",
                FacilityEvolutionTerms.FrequentBrawls,
                "잦은 소란",
                "싸움과 소란이 반복된 기록이다. 전투/무법 계보의 변이 후보를 강화한다.",
                FacilityEvolutionTerms.BrawlCount,
                1f,
                "recent-weighted",
                FacilityEvolutionRecordTokenConsumePolicy.Preserve,
                new[] { FacilityEvolutionTerms.Combat, FacilityEvolutionTerms.Outlaw },
                "소란이 단순 사고가 아니라 이 시설의 문화가 되고 있습니다."),

            new RecordTokenDefinitionSpec(
                9204,
                "RT_NoblePatronage",
                FacilityEvolutionTerms.NoblePatronage,
                "귀족 후원",
                "귀족 손님이 반복 방문하거나 큰돈을 지불한 기록이다. 고급/귀족 계보의 역사 증거로 남긴다.",
                FacilityEvolutionTerms.NobleVisitorRatio,
                0.25f,
                "long-term",
                FacilityEvolutionRecordTokenConsumePolicy.Preserve,
                new[] { FacilityEvolutionTerms.Luxury, FacilityEvolutionTerms.Noble },
                "귀족층이 이 시설을 알아보기 시작했습니다."),

            new RecordTokenDefinitionSpec(
                9205,
                "RT_GuardRallyPoint",
                FacilityEvolutionTerms.GuardRallyPoint,
                "경비 집결지",
                "경비나 전투 인원이 이 시설을 집결지처럼 이용한 기록이다.",
                FacilityEvolutionTerms.ZoneSafetyScore,
                0.4f,
                "recent+long-term",
                FacilityEvolutionRecordTokenConsumePolicy.Preserve,
                new[] { FacilityEvolutionTerms.Security, FacilityEvolutionTerms.Combat },
                "이 시설 주변이 경비 동선의 기준점이 되고 있습니다."),

            new RecordTokenDefinitionSpec(
                9206,
                "RT_IntruderBloodied",
                FacilityEvolutionTerms.IntruderBloodied,
                "침입자 유혈 기록",
                "침입자에게 피해를 입히거나 침입을 저지한 기록이다.",
                FacilityEvolutionTerms.IntruderDamageDealt,
                1f,
                "event-memory",
                FacilityEvolutionRecordTokenConsumePolicy.Preserve,
                new[] { FacilityEvolutionTerms.Security, FacilityEvolutionTerms.Brutal },
                "침입자와의 충돌이 시설의 평판에 남았습니다."),

            new RecordTokenDefinitionSpec(
                9207,
                "RT_CleanServiceStreak",
                FacilityEvolutionTerms.CleanServiceStreak,
                "청결한 서비스 연속",
                "높은 만족도와 낮은 실패율이 이어진 기록이다.",
                FacilityEvolutionTerms.ServiceQuality,
                0.7f,
                "streak",
                FacilityEvolutionRecordTokenConsumePolicy.Preserve,
                new[] { FacilityEvolutionTerms.Service, FacilityEvolutionTerms.Luxury },
                "안정적인 서비스가 고급화의 근거가 되고 있습니다."),

            new RecordTokenDefinitionSpec(
                9208,
                "RT_HighTurnoverService",
                FacilityEvolutionTerms.HighTurnoverService,
                "빠른 회전 서비스",
                "많은 손님을 빠르게 처리한 기록이다.",
                FacilityEvolutionTerms.TurnoverRate,
                0.65f,
                "daily",
                FacilityEvolutionRecordTokenConsumePolicy.Preserve,
                new[] { FacilityEvolutionTerms.Crowd, FacilityEvolutionTerms.Service },
                "손님을 빠르게 돌리는 운영이 자리 잡고 있습니다."),

            new RecordTokenDefinitionSpec(
                9209,
                "RT_OutlawRumor",
                FacilityEvolutionTerms.OutlawRumor,
                "무법자 소문",
                "범죄, 절도, 부정적 소문이 시설에 붙은 기록이다.",
                FacilityEvolutionTerms.NegativeMentionCount,
                1f,
                "rumor-decay",
                FacilityEvolutionRecordTokenConsumePolicy.Preserve,
                new[] { FacilityEvolutionTerms.Outlaw, FacilityEvolutionTerms.Fear },
                "나쁜 소문이 다른 가능성의 그림자를 만들고 있습니다.")
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

    private static FacilityEvolutionValue Value(string key, float value)
    {
        return new FacilityEvolutionValue(key, value);
    }

    private static FacilityEvolutionMetricRequirement Min(string key, float value)
    {
        return new FacilityEvolutionMetricRequirement
        {
            key = key,
            requireMin = true,
            minValue = value
        };
    }

    private static FacilityEvolutionMetricRequirement Max(string key, float value)
    {
        return new FacilityEvolutionMetricRequirement
        {
            key = key,
            requireMax = true,
            maxValue = value
        };
    }

    private static FacilityEvolutionTokenRequirement Token(string key, int count)
    {
        return new FacilityEvolutionTokenRequirement
        {
            key = key,
            minCount = count
        };
    }

    private static void EnsureFolder(string folder)
    {
        string normalized = folder.Replace('\\', '/');
        string[] parts = normalized.Split('/');
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

    private sealed class EvolutionRecipeSpec
    {
        public EvolutionRecipeSpec(
            int id,
            string assetName,
            string evolutionId,
            string displayName,
            string description,
            string resultAssetName,
            string[] sourceAssetNames,
            int requiredStarGrade,
            int resultStarGrade,
            bool publicByDefault,
            string requiredResearchRecipeId,
            FacilityEvolutionMetricRequirement[] requiredRoomScores,
            FacilityEvolutionMetricRequirement[] requiredRoomMetrics,
            FacilityEvolutionTokenRequirement[] requiredRecordTokens,
            string[] allowedMutationTags,
            string[] sourceLineageTags = null,
            string[] requiredRoomTags = null,
            string[] requiredUniqueFixtureAssetNames = null,
            FacilityEvolutionMaterialRequirement[] requiredMaterials = null,
            FacilityEvolutionValue[] identityPressureWeights = null,
            float minimumIdentityScore = 0f,
            bool consumeRecordTokens = false)
        {
            this.id = id;
            this.assetName = assetName;
            this.evolutionId = evolutionId;
            this.displayName = displayName;
            this.description = description;
            this.resultAssetName = resultAssetName;
            this.sourceAssetNames = sourceAssetNames ?? Array.Empty<string>();
            this.requiredStarGrade = requiredStarGrade;
            this.resultStarGrade = resultStarGrade;
            this.publicByDefault = publicByDefault;
            this.requiredResearchRecipeId = requiredResearchRecipeId ?? string.Empty;
            this.requiredRoomScores = requiredRoomScores ?? Array.Empty<FacilityEvolutionMetricRequirement>();
            this.requiredRoomMetrics = requiredRoomMetrics ?? Array.Empty<FacilityEvolutionMetricRequirement>();
            this.requiredRecordTokens = requiredRecordTokens ?? Array.Empty<FacilityEvolutionTokenRequirement>();
            this.allowedMutationTags = allowedMutationTags ?? Array.Empty<string>();
            this.sourceLineageTags = sourceLineageTags ?? Array.Empty<string>();
            this.requiredRoomTags = requiredRoomTags ?? Array.Empty<string>();
            this.requiredUniqueFixtureAssetNames = requiredUniqueFixtureAssetNames ?? Array.Empty<string>();
            this.requiredMaterials = requiredMaterials ?? Array.Empty<FacilityEvolutionMaterialRequirement>();
            this.identityPressureWeights = identityPressureWeights ?? Array.Empty<FacilityEvolutionValue>();
            this.minimumIdentityScore = Mathf.Clamp01(minimumIdentityScore);
            this.consumeRecordTokens = consumeRecordTokens;
        }

        public readonly int id;
        public readonly string assetName;
        public readonly string evolutionId;
        public readonly string displayName;
        public readonly string description;
        public readonly string resultAssetName;
        public readonly string[] sourceAssetNames;
        public readonly int requiredStarGrade;
        public readonly int resultStarGrade;
        public readonly bool publicByDefault;
        public readonly string requiredResearchRecipeId;
        public readonly FacilityEvolutionMetricRequirement[] requiredRoomScores;
        public readonly FacilityEvolutionMetricRequirement[] requiredRoomMetrics;
        public readonly FacilityEvolutionTokenRequirement[] requiredRecordTokens;
        public readonly string[] allowedMutationTags;
        public readonly string[] sourceLineageTags;
        public readonly string[] requiredRoomTags;
        public readonly string[] requiredUniqueFixtureAssetNames;
        public readonly FacilityEvolutionMaterialRequirement[] requiredMaterials;
        public readonly FacilityEvolutionValue[] identityPressureWeights;
        public readonly float minimumIdentityScore;
        public readonly bool consumeRecordTokens;
    }

    private sealed class RecordTokenDefinitionSpec
    {
        public RecordTokenDefinitionSpec(
            int id,
            string assetName,
            string tokenId,
            string displayName,
            string description,
            string sourceMetric,
            float threshold,
            string decayPolicy,
            FacilityEvolutionRecordTokenConsumePolicy consumePolicy,
            string[] recipeTags,
            string uiHint)
        {
            this.id = id;
            this.assetName = assetName;
            this.tokenId = tokenId;
            this.displayName = displayName;
            this.description = description;
            this.sourceMetric = sourceMetric;
            this.threshold = threshold;
            this.decayPolicy = decayPolicy;
            this.consumePolicy = consumePolicy;
            this.recipeTags = recipeTags ?? Array.Empty<string>();
            this.uiHint = uiHint;
        }

        public readonly int id;
        public readonly string assetName;
        public readonly string tokenId;
        public readonly string displayName;
        public readonly string description;
        public readonly string sourceMetric;
        public readonly float threshold;
        public readonly string decayPolicy;
        public readonly FacilityEvolutionRecordTokenConsumePolicy consumePolicy;
        public readonly string[] recipeTags;
        public readonly string uiHint;
    }
}
