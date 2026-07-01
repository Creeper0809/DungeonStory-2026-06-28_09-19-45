using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class CodexDebugScenarios
{
    [MenuItem("DungeonStory/Debug/Codex/Run P1 Codex Scenarios")]
    public static void RunFromMenu()
    {
        bool success = RunAll(true);
        if (!success)
        {
            Debug.LogError("P1 codex scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        P1FacilityShopAssetBuilder.EnsureP1FacilityShopAssets();
        P1FacilitySynthesisAssetBuilder.EnsureP1SynthesisAssets();

        List<string> errors = new List<string>();
        RunScenario("도감 기준 데이터", VerifyReferenceCodexData, errors);
        RunScenario("특수 조합식 힌트와 연구 해금", VerifySpecialRecipeHintAndResearchReveal, errors);
        RunScenario("방어 관찰 침략 도감", VerifyDefenseObservationUpdatesInvasionCodex, errors);
        RunScenario("손님 방문 몬스터 도감", VerifyFacilityVisitUpdatesMonsterCodex, errors);
        RunScenario("도감 UI 렌더", VerifyCodexPanelRendering, errors);

        if (errors.Count > 0)
        {
            foreach (string error in errors)
            {
                Debug.LogError(error);
            }

            return false;
        }

        if (logSuccess)
        {
            Debug.Log("P1 codex scenarios passed.");
        }

        return true;
    }

    private static void RunScenario(string name, Func<bool> scenario, List<string> errors)
    {
        try
        {
            if (scenario()) return;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        errors.Add(name);
    }

    private static bool VerifyReferenceCodexData()
    {
        using CodexScenarioWorld world = new CodexScenarioWorld();
        CodexRuntime runtime = world.CreateRuntime();

        CodexEntrySnapshot slime = runtime.State.GetSnapshot(CodexEntryCategory.Monster, "monster:Slime");
        CodexEntrySnapshot orc = runtime.State.GetSnapshot(CodexEntryCategory.Monster, "monster:Orc");
        CodexEntrySnapshot intruder = runtime.State.GetSnapshot(CodexEntryCategory.Invasion, CodexService.BreakthroughIntruderId);
        CodexEntrySnapshot spike = runtime.State.GetSnapshot(CodexEntryCategory.Facility, $"facility:{LoadBuilding("P1_SpikeTrap").id}");
        CodexEntrySnapshot guard = runtime.State.GetSnapshot(CodexEntryCategory.Facility, $"facility:{LoadBuilding("P1_GuardRoom").id}");

        return slime != null
            && orc != null
            && intruder != null
            && ContainsLine(intruder, "주의: 사장 캐릭터 처치")
            && spike != null
            && ContainsLinePart(spike, "공격 컨셉: 물리")
            && ContainsLinePart(spike, "효과: 피해")
            && guard != null
            && ContainsLinePart(guard, "시너지 대상: 경비 직원");
    }

    private static bool VerifySpecialRecipeHintAndResearchReveal()
    {
        using CodexScenarioWorld world = new CodexScenarioWorld();
        CodexRuntime runtime = world.CreateRuntime();
        CodexEntrySnapshot hint = runtime.GetEntries(CodexEntryCategory.Facility)
            .FirstOrDefault((entry) => entry.entryId == "special_recipe_hint:recipe_storm_fire_3");

        bool hiddenAsHint = hint != null
            && ContainsLinePart(hint, "특수 조합식 힌트")
            && !hint.lines.Any((line) => line.Text.Contains("경보 코일 + 화염 분사구"));

        BlueprintResearchState researchState = new BlueprintResearchState();
        researchState.UnlockRecipe("recipe_trap_chain_3");
        CodexService.ImportSynthesisRecipes(runtime.State, researchState);
        BuildingSO stormFire = LoadBuilding("P1_StormFireTrap");
        CodexEntrySnapshot stormFireEntry = runtime.State.GetSnapshot(CodexEntryCategory.Facility, $"facility:{stormFire.id}");

        return hiddenAsHint
            && stormFireEntry != null
            && ContainsLinePart(stormFireEntry, "조합식: 2성 경보 코일 + 1성 화염 분사구 -> 3성 폭뢰 분사구");
    }

    private static bool VerifyDefenseObservationUpdatesInvasionCodex()
    {
        using CodexScenarioWorld world = new CodexScenarioWorld();
        CodexRuntime runtime = world.CreateRuntime();
        DefenseFacility iceVent = world.CreateDefenseFacility("P1_IceVent");
        Character intruder = world.CreateCharacter("Intruder_Breakthrough");
        DefenseActivationReport report = new DefenseActivationReport(iceVent, intruder, DefenseTriggerTiming.OnEnter);
        report.AddMovementDelay(0.7f);
        report.AddEffectTag("감속");

        runtime.OnTriggerEvent(new DefenseFacilityTriggeredEvent(report));
        CodexEntrySnapshot invasion = runtime.State.GetSnapshot(CodexEntryCategory.Invasion, CodexService.BreakthroughIntruderId);
        CodexEntrySnapshot ice = runtime.State.GetSnapshot(CodexEntryCategory.Facility, $"facility:{LoadBuilding("P1_IceVent").id}");

        return invasion != null
            && ContainsLine(invasion, "약점: 감속")
            && ice != null
            && ContainsLinePart(ice, "공격 컨셉: 냉기");
    }

    private static bool VerifyFacilityVisitUpdatesMonsterCodex()
    {
        using CodexScenarioWorld world = new CodexScenarioWorld();
        CodexRuntime runtime = world.CreateRuntime();
        Character orc = world.CreateCharacter("Owner_Orc");
        BuildableObject meatRestaurant = world.CreateFacility("P1_MeatRestaurant");

        runtime.OnTriggerEvent(new FacilityVisitEvent(orc, meatRestaurant));
        CodexEntrySnapshot orcEntry = runtime.State.GetSnapshot(CodexEntryCategory.Monster, "monster:Orc");
        CodexEntrySnapshot restaurantEntry = runtime.State.GetSnapshot(CodexEntryCategory.Facility, $"facility:{LoadBuilding("P1_MeatRestaurant").id}");

        return orcEntry != null
            && ContainsLinePart(orcEntry, "관찰:")
            && restaurantEntry != null
            && ContainsLinePart(restaurantEntry, "역할: 식사");
    }

    private static bool VerifyCodexPanelRendering()
    {
        using CodexScenarioWorld world = new CodexScenarioWorld();
        CodexRuntime runtime = world.CreateRuntime();
        runtime.State.AddInfo(
            CodexEntryCategory.Invasion,
            CodexService.BreakthroughIntruderId,
            "돌파형 침입자",
            "약점: 감속",
            CodexInfoSource.Observation);
        CodexPanel panel = CodexPanel.CreateDefaultPanel(runtime);
        world.TrackObject(panel.transform.root.gameObject);

        return panel.LastRenderedText.Contains("몬스터 도감")
            && panel.LastRenderedText.Contains("침략 도감")
            && panel.LastRenderedText.Contains("시설 도감")
            && panel.LastRenderedText.Contains("약점: 감속");
    }

    private static bool ContainsLine(CodexEntrySnapshot entry, string line)
    {
        return entry != null
            && entry.lines != null
            && entry.lines.Any((candidate) => candidate.Text == line);
    }

    private static bool ContainsLinePart(CodexEntrySnapshot entry, string text)
    {
        return entry != null
            && entry.lines != null
            && entry.lines.Any((candidate) => candidate.Text.Contains(text));
    }

    private static BuildingSO LoadBuilding(string assetName)
    {
        return AssetDatabase.LoadAssetAtPath<BuildingSO>($"Assets/Resources/SO/Building/P1/{assetName}.asset");
    }

    private static CharacterSO LoadCharacter(string assetName)
    {
        CharacterSO character = AssetDatabase.LoadAssetAtPath<CharacterSO>($"Assets/Resources/SO/Character/Owners/{assetName}.asset");
        if (character != null)
        {
            return character;
        }

        return AssetDatabase.LoadAssetAtPath<CharacterSO>($"Assets/Resources/SO/Character/Intruders/{assetName}.asset");
    }

    private sealed class CodexScenarioWorld : IDisposable
    {
        private readonly List<GameObject> objects = new List<GameObject>();

        public CodexRuntime CreateRuntime()
        {
            GameObject obj = new GameObject("CodexRuntime_Test");
            objects.Add(obj);
            CodexRuntime runtime = obj.AddComponent<CodexRuntime>();
            runtime.ImportReferenceData();
            return runtime;
        }

        public BuildableObject CreateFacility(string assetName)
        {
            BuildingSO building = LoadBuilding(assetName);
            GameObject obj = new GameObject(assetName);
            objects.Add(obj);
            BuildableObject facility = obj.AddComponent(building != null && building.type != null ? building.type : typeof(BuildableObject)) as BuildableObject;
            if (facility == null)
            {
                throw new InvalidOperationException($"{assetName} is not a BuildableObject.");
            }

            facility.Initialization(building, Vector2Int.zero);
            return facility;
        }

        public DefenseFacility CreateDefenseFacility(string assetName)
        {
            return CreateFacility(assetName) as DefenseFacility;
        }

        public Character CreateCharacter(string assetName)
        {
            CharacterSO characterData = LoadCharacter(assetName);
            GameObject obj = new GameObject(assetName);
            objects.Add(obj);
            Character character = obj.AddComponent<Character>();
            character.data = characterData;
            return character;
        }

        public void TrackObject(GameObject obj)
        {
            if (obj != null && !objects.Contains(obj))
            {
                objects.Add(obj);
            }
        }

        public void Dispose()
        {
            foreach (GameObject obj in objects.Where((obj) => obj != null))
            {
                Object.DestroyImmediate(obj);
            }
        }
    }
}
