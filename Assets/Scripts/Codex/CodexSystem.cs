using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CodexEntryCategory
{
    Monster,
    Invasion,
    Facility
}

public enum CodexInfoSource
{
    System,
    Observation,
    Research,
    Synthesis
}

public readonly struct CodexInfoLine
{
    public CodexInfoLine(string text, CodexInfoSource source)
    {
        Text = text ?? string.Empty;
        Source = source;
    }

    public string Text { get; }
    public CodexInfoSource Source { get; }
}

public sealed class CodexEntrySnapshot
{
    public CodexEntryCategory category;
    public string entryId;
    public string title;
    public bool discovered;
    public CodexInfoLine[] lines = Array.Empty<CodexInfoLine>();

    public string ToDisplayText()
    {
        List<string> result = new List<string>
        {
            string.IsNullOrWhiteSpace(title) ? entryId : title
        };

        if (lines == null || lines.Length == 0)
        {
            result.Add("- 정보 없음");
        }
        else
        {
            result.AddRange(lines.Select((line) => $"- {line.Text}"));
        }

        return string.Join("\n", result);
    }
}

public sealed class CodexEntryRecord
{
    private readonly List<CodexInfoLine> lines = new List<CodexInfoLine>();
    private readonly HashSet<string> lineSet = new HashSet<string>();

    public CodexEntryRecord(CodexEntryCategory category, string entryId, string title)
    {
        Category = category;
        EntryId = entryId ?? string.Empty;
        Title = string.IsNullOrWhiteSpace(title) ? EntryId : title;
        Discovered = true;
    }

    public CodexEntryCategory Category { get; }
    public string EntryId { get; }
    public string Title { get; private set; }
    public bool Discovered { get; private set; }
    public IReadOnlyList<CodexInfoLine> Lines => lines;

    public void Rename(string title)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            Title = title;
        }
    }

    public bool AddInfo(string text, CodexInfoSource source)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string normalized = text.Trim();
        if (!lineSet.Add(normalized))
        {
            return false;
        }

        lines.Add(new CodexInfoLine(normalized, source));
        Discovered = true;
        return true;
    }

    public CodexEntrySnapshot ToSnapshot()
    {
        return new CodexEntrySnapshot
        {
            category = Category,
            entryId = EntryId,
            title = Title,
            discovered = Discovered,
            lines = lines.ToArray()
        };
    }
}

public sealed class CodexState
{
    private readonly Dictionary<string, CodexEntryRecord> entries = new Dictionary<string, CodexEntryRecord>();

    public IReadOnlyCollection<CodexEntryRecord> Entries => entries.Values;

    public CodexEntryRecord GetOrCreate(CodexEntryCategory category, string entryId, string title)
    {
        string key = GetKey(category, entryId);
        if (!entries.TryGetValue(key, out CodexEntryRecord entry))
        {
            entry = new CodexEntryRecord(category, entryId, title);
            entries[key] = entry;
        }
        else
        {
            entry.Rename(title);
        }

        return entry;
    }

    public bool AddInfo(CodexEntryCategory category, string entryId, string title, string info, CodexInfoSource source)
    {
        return GetOrCreate(category, entryId, title).AddInfo(info, source);
    }

    public bool HasInfo(CodexEntryCategory category, string entryId, string info)
    {
        string key = GetKey(category, entryId);
        return entries.TryGetValue(key, out CodexEntryRecord entry)
            && entry.Lines.Any((line) => line.Text == info);
    }

    public IReadOnlyList<CodexEntrySnapshot> GetSnapshots(CodexEntryCategory category)
    {
        return entries.Values
            .Where((entry) => entry.Category == category)
            .OrderBy((entry) => entry.Title)
            .Select((entry) => entry.ToSnapshot())
            .ToList();
    }

    public CodexEntrySnapshot GetSnapshot(CodexEntryCategory category, string entryId)
    {
        return entries.TryGetValue(GetKey(category, entryId), out CodexEntryRecord entry)
            ? entry.ToSnapshot()
            : null;
    }

    private static string GetKey(CodexEntryCategory category, string entryId)
    {
        return $"{category}:{entryId ?? string.Empty}";
    }
}

public struct CodexUpdatedEvent
{
    public CodexEntryCategory category;
    public string entryId;

    public CodexUpdatedEvent(CodexEntryCategory category, string entryId)
    {
        this.category = category;
        this.entryId = entryId ?? string.Empty;
    }

    private static CodexUpdatedEvent e;

    public static void Trigger(CodexEntryCategory category, string entryId)
    {
        e.category = category;
        e.entryId = entryId ?? string.Empty;
        EventObserver.TriggerEvent(e);
    }
}

public static class CodexService
{
    public const string BreakthroughIntruderId = "intruder_breakthrough";

    public static void ImportReferenceData(CodexState state, BlueprintResearchState researchState)
    {
        if (state == null)
        {
            return;
        }

        foreach (CharacterSpeciesSO species in Resources.LoadAll<CharacterSpeciesSO>("SO/Character/Species"))
        {
            ObserveSpecies(state, species, CodexInfoSource.System);
        }

        foreach (BuildingSO building in Resources.LoadAll<BuildingSO>("SO/Building/P1"))
        {
            ObserveFacility(state, building, CodexInfoSource.System);
        }

        ImportSynthesisRecipes(state, researchState);
        SeedBreakthroughIntruder(state);
    }

    public static void ObserveCharacter(CodexState state, Character character)
    {
        if (state == null || character == null)
        {
            return;
        }

        ObserveSpecies(state, character.data != null ? character.data.species : null, CodexInfoSource.Observation);
        if (!string.IsNullOrWhiteSpace(character.SpeciesTag))
        {
            CodexEntryRecord entry = state.GetOrCreate(
                CodexEntryCategory.Monster,
                GetMonsterEntryId(character.SpeciesTag),
                character.SpeciesTag);
            entry.AddInfo($"관찰: {character.name} 방문", CodexInfoSource.Observation);
        }
    }

    public static void ObserveSpecies(CodexState state, CharacterSpeciesSO species, CodexInfoSource source)
    {
        if (state == null || species == null)
        {
            return;
        }

        string entryId = GetMonsterEntryId(species.speciesTag);
        string title = !string.IsNullOrWhiteSpace(species.displayName)
            ? species.displayName
            : species.speciesTag;
        CodexEntryRecord entry = state.GetOrCreate(CodexEntryCategory.Monster, entryId, title);

        AddIfNotBlank(entry, species.shortDescription, source);
        foreach (string preferred in species.preferredFacilityLabels ?? Array.Empty<string>())
        {
            AddIfNotBlank(entry, $"선호: {preferred}", source);
        }

        foreach (string disliked in species.dislikedEnvironmentLabels ?? Array.Empty<string>())
        {
            AddIfNotBlank(entry, $"기피: {disliked}", source);
        }

        AddIfNotBlank(entry, $"사고 위험: {species.incidentName}", source);
        AddIfNotBlank(entry, species.incidentDescription, source);
        if (species.incidentMitigatingRoles != FacilityRole.None)
        {
            entry.AddInfo($"완화 역할: {FormatFacilityRoles(species.incidentMitigatingRoles)}", source);
        }
    }

    public static void ObserveFacility(CodexState state, BuildableObject facility, CodexInfoSource source)
    {
        if (facility == null)
        {
            return;
        }

        ObserveFacility(state, facility.BuildingData, source);
    }

    public static void ObserveFacility(CodexState state, BuildingSO building, CodexInfoSource source)
    {
        if (state == null || building == null)
        {
            return;
        }

        CodexEntryRecord entry = state.GetOrCreate(
            CodexEntryCategory.Facility,
            GetFacilityEntryId(building),
            FacilityShopService.GetBuildingName(building));
        FacilityData facility = building.Facility;
        if (facility != null && facility.roles != FacilityRole.None)
        {
            entry.AddInfo($"역할: {FormatFacilityRoles(facility.roles)}", source);
        }

        if (facility != null && facility.supportedWorkTypes != FacilityWorkType.None)
        {
            entry.AddInfo($"작업: {FormatWorkTypes(facility.supportedWorkTypes)}", source);
        }

        if (facility != null && facility.capacity > 0)
        {
            entry.AddInfo($"수용: {facility.capacity}", source);
        }

        if (facility != null && facility.requiresStock)
        {
            entry.AddInfo($"재고 필요: 내부 재고 {facility.internalStockMax}", source);
        }

        if (facility != null && facility.preferredSpeciesTags != null && facility.preferredSpeciesTags.Length > 0)
        {
            entry.AddInfo($"시너지 대상: {string.Join(", ", facility.preferredSpeciesTags)}", source);
        }

        DefenseFacilityData defense = building.Defense;
        if (defense != null && defense.IsDefenseFacility)
        {
            entry.AddInfo($"별 등급: {defense.star}성", source);
            entry.AddInfo($"공격 컨셉: {FormatDefenseConcept(defense.concept)}", source);
            entry.AddInfo($"발동 조건: {FormatTriggerTimings(defense.triggerTimings)}", source);
            entry.AddInfo($"대상: {FormatTargetRule(defense.targetRule)}", source);
            if (defense.SupportsTrigger(DefenseTriggerTiming.GuardResponse)
                || defense.concept == DefenseAttackConcept.Guard)
            {
                entry.AddInfo("시너지 대상: 경비 직원", source);
            }

            foreach (string effect in FormatDefenseEffects(defense))
            {
                entry.AddInfo($"효과: {effect}", source);
            }
        }
    }

    public static void RecordDefenseObservation(CodexState state, DefenseActivationReport report)
    {
        if (state == null || report == null)
        {
            return;
        }

        ObserveFacility(state, report.Facility, CodexInfoSource.Observation);
        SeedBreakthroughIntruder(state);
        foreach (string tag in report.EffectTags)
        {
            AddIntruderInfoFromEffectTag(state, tag);
        }

        if (report.MovementDelaySeconds > 0f)
        {
            AddInvasionInfo(state, "약점: 감속", CodexInfoSource.Observation);
        }

        if (report.TotalDamage > 0f)
        {
            AddInvasionInfo(state, "약점: 피해 누적", CodexInfoSource.Observation);
        }
    }

    public static void RecordCombatReport(CodexState state, InvasionCombatReport report)
    {
        if (state == null || report == null)
        {
            return;
        }

        SeedBreakthroughIntruder(state);
        foreach (string observation in report.Observations ?? Array.Empty<string>())
        {
            AddInvasionInfo(state, NormalizeObservation(observation), CodexInfoSource.Observation);
        }

        foreach (BuildableObject facility in report.DamagedFacilities ?? Array.Empty<BuildableObject>())
        {
            ObserveFacility(state, facility, CodexInfoSource.Observation);
            AddInvasionInfo(state, "성향: 시설 파괴 우선", CodexInfoSource.Observation);
        }
    }

    public static void RecordFacilityDamage(CodexState state, BuildableObject facility)
    {
        ObserveFacility(state, facility, CodexInfoSource.Observation);
        AddInvasionInfo(state, "성향: 시설 파괴 우선", CodexInfoSource.Observation);
    }

    public static void RecordResearch(CodexState state, BlueprintResearchUnlockResult unlockResult, BlueprintResearchState researchState)
    {
        if (state == null)
        {
            return;
        }

        FacilityBlueprintSO blueprint = unlockResult.Blueprint;
        if (blueprint != null)
        {
            foreach (int buildingId in blueprint.unlockBuildingIds ?? Array.Empty<int>())
            {
                ObserveFacility(state, FacilityShopService.FindBuildingById(buildingId), CodexInfoSource.Research);
            }

            foreach (int buildingId in blueprint.unlockBasicPurchaseBuildingIds ?? Array.Empty<int>())
            {
                BuildingSO building = FacilityShopService.FindBuildingById(buildingId);
                ObserveFacility(state, building, CodexInfoSource.Research);
                AddFacilityInfo(state, building, "기본 구매: 연구 완료 후 구매 가능", CodexInfoSource.Research);
            }
        }

        ImportSynthesisRecipes(state, researchState);
    }

    public static void RecordSynthesis(CodexState state, FacilitySynthesisResult result, BlueprintResearchState researchState)
    {
        if (state == null || result.Recipe == null)
        {
            return;
        }

        ObserveFacility(state, result.ResultBuilding, CodexInfoSource.Synthesis);
        AddRecipeInfo(state, result.Recipe, true, CodexInfoSource.Synthesis);
        ImportSynthesisRecipes(state, researchState);
    }

    public static void ImportSynthesisRecipes(CodexState state, BlueprintResearchState researchState)
    {
        if (state == null)
        {
            return;
        }

        foreach (FacilitySynthesisRecipeSO recipe in FacilitySynthesisService.LoadAllRecipes())
        {
            bool visible = FacilitySynthesisService.IsRecipeVisible(recipe, researchState);
            if (visible)
            {
                AddRecipeInfo(state, recipe, true, CodexInfoSource.System);
            }
            else if (recipe.IsSpecial)
            {
                AddSpecialRecipeHint(state, recipe);
            }
        }
    }

    public static void SeedBreakthroughIntruder(CodexState state)
    {
        if (state == null)
        {
            return;
        }

        CodexEntryRecord entry = state.GetOrCreate(
            CodexEntryCategory.Invasion,
            BreakthroughIntruderId,
            "돌파형 침입자");
        entry.AddInfo("주의: 사장 캐릭터 처치", CodexInfoSource.System);
        entry.AddInfo("주의: 사장방 돌파", CodexInfoSource.System);
        entry.AddInfo("성향: 시간이 지날수록 사장 위치 추적", CodexInfoSource.System);
        entry.AddInfo("저항: 공포 효과", CodexInfoSource.System);
    }

    private static void AddRecipeInfo(
        CodexState state,
        FacilitySynthesisRecipeSO recipe,
        bool reveal,
        CodexInfoSource source)
    {
        if (state == null || recipe == null || !recipe.HasValidData)
        {
            return;
        }

        string materials = string.Join(" + ", recipe.materialBuildings.Select(FacilityShopService.GetBuildingName));
        string resultName = FacilityShopService.GetBuildingName(recipe.resultBuilding);
        string line = reveal
            ? $"조합식: {materials} -> {resultName}"
            : BuildSpecialRecipeHint(recipe);

        AddFacilityInfo(state, recipe.resultBuilding, line, source);
        foreach (BuildingSO material in recipe.materialBuildings)
        {
            AddFacilityInfo(state, material, line, source);
        }
    }

    private static void AddSpecialRecipeHint(CodexState state, FacilitySynthesisRecipeSO recipe)
    {
        if (state == null || recipe == null || !recipe.HasValidData)
        {
            return;
        }

        string hintId = $"special_recipe_hint:{recipe.recipeId}";
        CodexEntryRecord entry = state.GetOrCreate(
            CodexEntryCategory.Facility,
            hintId,
            "미확인 특수 조합식");
        entry.AddInfo(BuildSpecialRecipeHint(recipe), CodexInfoSource.System);
    }

    private static string BuildSpecialRecipeHint(FacilitySynthesisRecipeSO recipe)
    {
        string concept = recipe.resultBuilding != null && recipe.resultBuilding.Defense != null
            ? FormatDefenseConcept(recipe.resultBuilding.Defense.concept)
            : "특수";
        return $"특수 조합식 힌트: {concept} 계열 연구 필요";
    }

    private static void AddFacilityInfo(CodexState state, BuildingSO building, string info, CodexInfoSource source)
    {
        if (building == null)
        {
            return;
        }

        state.AddInfo(
            CodexEntryCategory.Facility,
            GetFacilityEntryId(building),
            FacilityShopService.GetBuildingName(building),
            info,
            source);
    }

    private static void AddInvasionInfo(CodexState state, string info, CodexInfoSource source)
    {
        if (state == null || string.IsNullOrWhiteSpace(info))
        {
            return;
        }

        SeedBreakthroughIntruder(state);
        state.AddInfo(
            CodexEntryCategory.Invasion,
            BreakthroughIntruderId,
            "돌파형 침입자",
            info,
            source);
    }

    private static void AddIntruderInfoFromEffectTag(CodexState state, string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return;
        }

        string normalized = tag.Trim();
        if (normalized.Contains("감속") || normalized.Contains("속박"))
        {
            AddInvasionInfo(state, "약점: 감속", CodexInfoSource.Observation);
        }

        if (normalized.Contains("경비"))
        {
            AddInvasionInfo(state, "약점: 근접 교전", CodexInfoSource.Observation);
        }

        if (normalized.Contains("부식"))
        {
            AddInvasionInfo(state, "약점: 방어력 감소", CodexInfoSource.Observation);
        }

        if (normalized.Contains("축전"))
        {
            AddInvasionInfo(state, "약점: 축전 연계", CodexInfoSource.Observation);
        }

        if (normalized.Contains("연소"))
        {
            AddInvasionInfo(state, "약점: 지속 피해", CodexInfoSource.Observation);
        }
    }

    private static string NormalizeObservation(string observation)
    {
        if (string.IsNullOrWhiteSpace(observation))
        {
            return string.Empty;
        }

        string normalized = observation.Trim();
        if (normalized.Contains("감속"))
        {
            return "약점: 감속";
        }

        if (normalized.Contains("경비"))
        {
            return "약점: 근접 교전";
        }

        if (normalized.Contains("부식"))
        {
            return "약점: 방어력 감소";
        }

        if (normalized.Contains("축전"))
        {
            return "약점: 축전 연계";
        }

        if (normalized.Contains("연소"))
        {
            return "약점: 지속 피해";
        }

        if (normalized.Contains("직접 피해"))
        {
            return "약점: 직접 피해";
        }

        return normalized.Replace("관찰:", "관찰:");
    }

    private static void AddIfNotBlank(CodexEntryRecord entry, string text, CodexInfoSource source)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            entry.AddInfo(text, source);
        }
    }

    private static string GetMonsterEntryId(string speciesTag)
    {
        return $"monster:{(string.IsNullOrWhiteSpace(speciesTag) ? "unknown" : speciesTag)}";
    }

    private static string GetFacilityEntryId(BuildingSO building)
    {
        return building != null ? $"facility:{building.id}" : "facility:unknown";
    }

    private static string FormatFacilityRoles(FacilityRole roles)
    {
        return string.Join(", ", Enum.GetValues(typeof(FacilityRole))
            .Cast<FacilityRole>()
            .Where((role) => role != FacilityRole.None && (roles & role) != 0)
            .Select(FormatFacilityRole));
    }

    private static string FormatWorkTypes(FacilityWorkType workTypes)
    {
        return string.Join(", ", Enum.GetValues(typeof(FacilityWorkType))
            .Cast<FacilityWorkType>()
            .Where((workType) => workType != FacilityWorkType.None && (workTypes & workType) != 0)
            .Select(FormatWorkType));
    }

    private static string FormatFacilityRole(FacilityRole role)
    {
        return role switch
        {
            FacilityRole.Meal => "식사",
            FacilityRole.Purchase => "구매",
            FacilityRole.Rest => "휴식",
            FacilityRole.Training => "훈련",
            FacilityRole.Research => "연구",
            FacilityRole.Mana => "마력",
            FacilityRole.Logistics => "물류",
            _ => role.ToString()
        };
    }

    private static string FormatWorkType(FacilityWorkType workType)
    {
        return workType switch
        {
            FacilityWorkType.Operate => "근무",
            FacilityWorkType.Restock => "보충",
            FacilityWorkType.Repair => "수리",
            FacilityWorkType.Clean => "청소",
            FacilityWorkType.Research => "연구",
            FacilityWorkType.Guard => "경비",
            FacilityWorkType.Rescue => "구조",
            FacilityWorkType.Rest => "휴식",
            _ => workType.ToString()
        };
    }

    private static string FormatDefenseConcept(DefenseAttackConcept concept)
    {
        return concept switch
        {
            DefenseAttackConcept.Physical => "물리",
            DefenseAttackConcept.Poison => "독",
            DefenseAttackConcept.Fire => "화염",
            DefenseAttackConcept.Lightning => "번개",
            DefenseAttackConcept.Ice => "냉기",
            DefenseAttackConcept.Guard => "경비",
            _ => "없음"
        };
    }

    private static string FormatTriggerTimings(DefenseTriggerTiming timings)
    {
        if (timings == DefenseTriggerTiming.None)
        {
            return "없음";
        }

        return string.Join(", ", Enum.GetValues(typeof(DefenseTriggerTiming))
            .Cast<DefenseTriggerTiming>()
            .Where((timing) => timing != DefenseTriggerTiming.None && (timings & timing) != 0)
            .Select((timing) => timing switch
            {
                DefenseTriggerTiming.OnEnter => "입장 시",
                DefenseTriggerTiming.Periodic => "머무는 동안",
                DefenseTriggerTiming.Cooldown => "쿨타임",
                DefenseTriggerTiming.GuardResponse => "경비 반응",
                _ => timing.ToString()
            }));
    }

    private static string FormatTargetRule(DefenseTargetRule targetRule)
    {
        return targetRule switch
        {
            DefenseTargetRule.EnteringIntruder => "입장한 침입자",
            DefenseTargetRule.IntrudersInRoom => "방 안 침입자",
            DefenseTargetRule.AllIntrudersInRoom => "방 안 모든 침입자",
            DefenseTargetRule.GuardTarget => "경비 대상",
            _ => targetRule.ToString()
        };
    }

    private static IEnumerable<string> FormatDefenseEffects(DefenseFacilityData defense)
    {
        IEnumerable<DefenseEffectData> effectData = defense.effects ?? Array.Empty<DefenseEffectData>();
        if (defense.effectAssets != null && defense.effectAssets.Length > 0)
        {
            return defense.effectAssets
                .Where((effect) => effect != null)
                .Select((effect) => FormatEffect(effect.Kind, effect.Amount, effect.Duration, effect.Stacks, effect.LogTag));
        }

        return effectData.Select((effect) => FormatEffect(effect.kind, effect.amount, effect.duration, effect.stacks, effect.logTag));
    }

    private static string FormatEffect(DefenseEffectKind kind, float amount, float duration, int stacks, string tag)
    {
        string name = kind switch
        {
            DefenseEffectKind.Damage => "피해",
            DefenseEffectKind.Corrosion => "방어력 감소",
            DefenseEffectKind.Burn => "지속 피해",
            DefenseEffectKind.Charge => "축전",
            DefenseEffectKind.Slow => "감속",
            DefenseEffectKind.GuardAttack => "근접 교전",
            _ => kind.ToString()
        };

        List<string> parts = new List<string> { name };
        if (amount > 0f)
        {
            parts.Add($"{amount:0.#}");
        }

        if (duration > 0f)
        {
            parts.Add($"{duration:0.#}초");
        }

        if (stacks > 1)
        {
            parts.Add($"{stacks}중첩");
        }

        return string.Join(" ", parts);
    }
}

public class CodexRuntime :
    MonoBehaviour,
    UtilEventListener<FacilityVisitEvent>,
    UtilEventListener<DefenseFacilityTriggeredEvent>,
    UtilEventListener<InvasionCombatReportReadyEvent>,
    UtilEventListener<InvasionFacilityDamagedEvent>,
    UtilEventListener<InvasionSpawnedEvent>,
    UtilEventListener<BlueprintResearchCompletedEvent>,
    UtilEventListener<FacilitySynthesisCompletedEvent>
{
    [SerializeField] private bool importReferenceDataOnAwake = true;

    private readonly CodexState state = new CodexState();

    public static CodexRuntime Instance => FindFirstObjectByType<CodexRuntime>();
    public CodexState State => state;

    public BlueprintResearchState ResearchState
    {
        get
        {
            BlueprintResearchRuntime researchRuntime = BlueprintResearchRuntime.Instance;
            return researchRuntime != null ? researchRuntime.State : null;
        }
    }

    private void Awake()
    {
        if (importReferenceDataOnAwake)
        {
            ImportReferenceData();
        }
    }

    public void ImportReferenceData()
    {
        CodexService.ImportReferenceData(state, ResearchState);
        CodexUpdatedEvent.Trigger(CodexEntryCategory.Facility, "reference");
        CodexUpdatedEvent.Trigger(CodexEntryCategory.Monster, "reference");
        CodexUpdatedEvent.Trigger(CodexEntryCategory.Invasion, CodexService.BreakthroughIntruderId);
    }

    public IReadOnlyList<CodexEntrySnapshot> GetEntries(CodexEntryCategory category)
    {
        return state.GetSnapshots(category);
    }

    public void OnTriggerEvent(FacilityVisitEvent eventType)
    {
        CodexService.ObserveCharacter(state, eventType.visitor);
        CodexService.ObserveFacility(state, eventType.facility, CodexInfoSource.Observation);
        CodexUpdatedEvent.Trigger(CodexEntryCategory.Monster, eventType.visitor != null ? eventType.visitor.SpeciesTag : string.Empty);
        CodexUpdatedEvent.Trigger(CodexEntryCategory.Facility, eventType.facility != null ? eventType.facility.id.ToString() : string.Empty);
    }

    public void OnTriggerEvent(DefenseFacilityTriggeredEvent eventType)
    {
        CodexService.RecordDefenseObservation(state, eventType.report);
        CodexUpdatedEvent.Trigger(CodexEntryCategory.Facility, "defense");
        CodexUpdatedEvent.Trigger(CodexEntryCategory.Invasion, CodexService.BreakthroughIntruderId);
    }

    public void OnTriggerEvent(InvasionCombatReportReadyEvent eventType)
    {
        CodexService.RecordCombatReport(state, eventType.report);
        CodexUpdatedEvent.Trigger(CodexEntryCategory.Invasion, CodexService.BreakthroughIntruderId);
    }

    public void OnTriggerEvent(InvasionFacilityDamagedEvent eventType)
    {
        CodexService.RecordFacilityDamage(state, eventType.facility);
        CodexUpdatedEvent.Trigger(CodexEntryCategory.Invasion, CodexService.BreakthroughIntruderId);
    }

    public void OnTriggerEvent(InvasionSpawnedEvent eventType)
    {
        CodexService.SeedBreakthroughIntruder(state);
        CodexService.ObserveCharacter(state, eventType.intruder);
        CodexUpdatedEvent.Trigger(CodexEntryCategory.Invasion, CodexService.BreakthroughIntruderId);
    }

    public void OnTriggerEvent(BlueprintResearchCompletedEvent eventType)
    {
        CodexService.RecordResearch(state, eventType.unlockResult, ResearchState);
        CodexUpdatedEvent.Trigger(CodexEntryCategory.Facility, "research");
    }

    public void OnTriggerEvent(FacilitySynthesisCompletedEvent eventType)
    {
        CodexService.RecordSynthesis(state, eventType.result, ResearchState);
        CodexUpdatedEvent.Trigger(CodexEntryCategory.Facility, "synthesis");
    }

    private void OnEnable()
    {
        this.EventStartListening<FacilityVisitEvent>();
        this.EventStartListening<DefenseFacilityTriggeredEvent>();
        this.EventStartListening<InvasionCombatReportReadyEvent>();
        this.EventStartListening<InvasionFacilityDamagedEvent>();
        this.EventStartListening<InvasionSpawnedEvent>();
        this.EventStartListening<BlueprintResearchCompletedEvent>();
        this.EventStartListening<FacilitySynthesisCompletedEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<FacilityVisitEvent>();
        this.EventStopListening<DefenseFacilityTriggeredEvent>();
        this.EventStopListening<InvasionCombatReportReadyEvent>();
        this.EventStopListening<InvasionFacilityDamagedEvent>();
        this.EventStopListening<InvasionSpawnedEvent>();
        this.EventStopListening<BlueprintResearchCompletedEvent>();
        this.EventStopListening<FacilitySynthesisCompletedEvent>();
    }
}
