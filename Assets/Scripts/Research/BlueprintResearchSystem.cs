using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

[Serializable]
public class BlueprintResearchTask
{
    [SerializeField] private FacilityBlueprintSO blueprint;
    [SerializeField] private float progress;

    public BlueprintResearchTask(FacilityBlueprintSO blueprint)
    {
        this.blueprint = blueprint;
        progress = 0f;
    }

    public FacilityBlueprintSO Blueprint => blueprint;
    public float Progress => progress;
    public float RequiredWork => blueprint != null ? Mathf.Max(1f, blueprint.researchWorkRequired) : 1f;
    public float ProgressRatio => Mathf.Clamp01(progress / RequiredWork);
    public bool IsCompleted => blueprint != null && progress >= RequiredWork;

    public float AddProgress(float amount)
    {
        if (blueprint == null || IsCompleted)
        {
            return 0f;
        }

        float before = progress;
        progress = Mathf.Min(RequiredWork, progress + Mathf.Max(0f, amount));
        return progress - before;
    }
}

public class BlueprintResearchState
{
    private readonly List<BlueprintResearchTask> tasks = new List<BlueprintResearchTask>();
    private readonly HashSet<int> completedBlueprintIds = new HashSet<int>();
    private readonly HashSet<string> unlockedRecipeIds = new HashSet<string>();

    public IReadOnlyList<BlueprintResearchTask> Tasks => tasks;
    public IReadOnlyCollection<int> CompletedBlueprintIds => completedBlueprintIds;
    public IReadOnlyCollection<string> UnlockedRecipeIds => unlockedRecipeIds;

    public bool HasActiveTask => TryGetActiveTask(out _);

    public bool EnqueueBlueprint(FacilityBlueprintSO blueprint)
    {
        if (blueprint == null || completedBlueprintIds.Contains(blueprint.id))
        {
            return false;
        }

        if (tasks.Any((task) => task.Blueprint == blueprint || task.Blueprint?.id == blueprint.id))
        {
            return false;
        }

        tasks.Add(new BlueprintResearchTask(blueprint));
        return true;
    }

    public bool TryGetActiveTask(out BlueprintResearchTask task)
    {
        task = tasks.FirstOrDefault((candidate) => candidate != null && !candidate.IsCompleted);
        return task != null;
    }

    public bool IsCompleted(FacilityBlueprintSO blueprint)
    {
        return blueprint != null && completedBlueprintIds.Contains(blueprint.id);
    }

    public bool TryCancelBlueprint(FacilityBlueprintSO blueprint)
    {
        if (blueprint == null)
        {
            return false;
        }

        BlueprintResearchTask task = tasks.FirstOrDefault((candidate) =>
            candidate != null
            && candidate.Blueprint != null
            && candidate.Blueprint.id == blueprint.id
            && !candidate.IsCompleted);
        return task != null && tasks.Remove(task);
    }

    public void MarkCompleted(FacilityBlueprintSO blueprint)
    {
        if (blueprint == null)
        {
            return;
        }

        completedBlueprintIds.Add(blueprint.id);
    }

    public bool UnlockRecipe(string recipeId)
    {
        return !string.IsNullOrWhiteSpace(recipeId) && unlockedRecipeIds.Add(recipeId);
    }
}

public readonly struct BlueprintResearchUnlockResult
{
    public BlueprintResearchUnlockResult(
        FacilityBlueprintSO blueprint,
        IReadOnlyList<string> unlockedBuildings,
        IReadOnlyList<string> unlockedBasicPurchases,
        IReadOnlyList<string> unlockedRecipes)
    {
        Blueprint = blueprint;
        UnlockedBuildings = unlockedBuildings ?? Array.Empty<string>();
        UnlockedBasicPurchases = unlockedBasicPurchases ?? Array.Empty<string>();
        UnlockedRecipes = unlockedRecipes ?? Array.Empty<string>();
    }

    public FacilityBlueprintSO Blueprint { get; }
    public IReadOnlyList<string> UnlockedBuildings { get; }
    public IReadOnlyList<string> UnlockedBasicPurchases { get; }
    public IReadOnlyList<string> UnlockedRecipes { get; }
}

public readonly struct BlueprintResearchWorkResult
{
    public BlueprintResearchWorkResult(
        bool success,
        FacilityBlueprintSO blueprint,
        float addedProgress,
        float totalProgress,
        float requiredWork,
        bool completed,
        string message)
    {
        Success = success;
        Blueprint = blueprint;
        AddedProgress = Mathf.Max(0f, addedProgress);
        TotalProgress = Mathf.Max(0f, totalProgress);
        RequiredWork = Mathf.Max(1f, requiredWork);
        Completed = completed;
        Message = message ?? string.Empty;
    }

    public bool Success { get; }
    public FacilityBlueprintSO Blueprint { get; }
    public float AddedProgress { get; }
    public float TotalProgress { get; }
    public float RequiredWork { get; }
    public float ProgressRatio => Mathf.Clamp01(TotalProgress / RequiredWork);
    public bool Completed { get; }
    public string Message { get; }
}

public struct BlueprintResearchQueuedEvent
{
    public FacilityBlueprintSO blueprint;

    public BlueprintResearchQueuedEvent(FacilityBlueprintSO blueprint)
    {
        this.blueprint = blueprint;
    }

    private static BlueprintResearchQueuedEvent e;

    public static void Trigger(FacilityBlueprintSO blueprint)
    {
        e.blueprint = blueprint;
        EventObserver.TriggerEvent(e);
    }
}

public struct BlueprintResearchProgressEvent
{
    public BlueprintResearchWorkResult result;

    public BlueprintResearchProgressEvent(BlueprintResearchWorkResult result)
    {
        this.result = result;
    }

    private static BlueprintResearchProgressEvent e;

    public static void Trigger(BlueprintResearchWorkResult result)
    {
        e.result = result;
        EventObserver.TriggerEvent(e);
    }
}

public struct BlueprintResearchCompletedEvent
{
    public FacilityBlueprintSO blueprint;
    public BlueprintResearchUnlockResult unlockResult;

    public BlueprintResearchCompletedEvent(FacilityBlueprintSO blueprint, BlueprintResearchUnlockResult unlockResult)
    {
        this.blueprint = blueprint;
        this.unlockResult = unlockResult;
    }

    private static BlueprintResearchCompletedEvent e;

    public static void Trigger(FacilityBlueprintSO blueprint, BlueprintResearchUnlockResult unlockResult)
    {
        e.blueprint = blueprint;
        e.unlockResult = unlockResult;
        EventObserver.TriggerEvent(e);
    }
}

public static class BlueprintResearchService
{
    private const float BaseResearchWorkPerSecond = 4f;

    public static float CalculateResearchWork(CharacterActor researcher, BuildableObject researchFacility, float seconds)
    {
        float characterMultiplier = researcher != null
            ? Mathf.Max(0.05f, researcher.GetWorkSpeedMultiplier(FacilityWorkType.Research))
            : 1f;
        float facilityMultiplier = GetFacilityResearchMultiplier(researchFacility);
        return Mathf.Max(0f, seconds) * BaseResearchWorkPerSecond * characterMultiplier * facilityMultiplier;
    }

    public static float GetFacilityResearchMultiplier(BuildableObject researchFacility)
    {
        if (researchFacility == null || researchFacility.Facility == null)
        {
            return 1f;
        }

        float multiplier = 1f;
        if (researchFacility.Facility.SupportsRole(FacilityRole.Research))
        {
            multiplier += 0.15f;
        }

        if (researchFacility.Facility.SupportsRole(FacilityRole.Mana))
        {
            multiplier += 0.05f;
        }

        if (researchFacility.Facility.requiredWorkers > 0)
        {
            multiplier += Mathf.Min(0.1f, researchFacility.Facility.requiredWorkers * 0.05f);
        }

        return multiplier;
    }

    public static BlueprintResearchUnlockResult ApplyCompletion(
        FacilityBlueprintSO blueprint,
        BlueprintResearchState state,
        FacilityShopUnlockState shopUnlockState,
        IFacilityShopCatalog facilityShopCatalog)
    {
        if (blueprint == null)
        {
            return new BlueprintResearchUnlockResult(null, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
        }

        if (facilityShopCatalog == null)
        {
            throw new ArgumentNullException(nameof(facilityShopCatalog));
        }

        state?.MarkCompleted(blueprint);

        List<string> unlockedBuildings = new List<string>();
        foreach (int buildingId in blueprint.unlockBuildingIds ?? Array.Empty<int>())
        {
            BuildingSO building = FacilityShopService.FindBuildingById(facilityShopCatalog, buildingId);
            if (building == null)
            {
                continue;
            }

            building.unlocked = true;
            unlockedBuildings.Add(FacilityShopService.GetBuildingName(building));
        }

        List<string> unlockedBasicPurchases = new List<string>();
        foreach (int buildingId in blueprint.unlockBasicPurchaseBuildingIds ?? Array.Empty<int>())
        {
            BuildingSO building = FacilityShopService.FindBuildingById(facilityShopCatalog, buildingId);
            if (building == null)
            {
                continue;
            }

            if (shopUnlockState != null && shopUnlockState.UnlockBasicPurchase(building))
            {
                unlockedBasicPurchases.Add(FacilityShopService.GetBuildingName(building));
            }
        }

        List<string> unlockedRecipes = new List<string>();
        foreach (string recipeId in blueprint.unlockRecipeIds ?? Array.Empty<string>())
        {
            if (state != null && state.UnlockRecipe(recipeId))
            {
                unlockedRecipes.Add(recipeId);
            }
        }

        return new BlueprintResearchUnlockResult(
            blueprint,
            unlockedBuildings,
            unlockedBasicPurchases,
            unlockedRecipes);
    }
}

public class BlueprintResearchRuntime : MonoBehaviour, UtilEventListener<FacilityShopPurchasedEvent>
{
    [SerializeField] private bool raiseAlertOnResearchComplete = true;

    private readonly BlueprintResearchState state = new BlueprintResearchState();
    private IFacilityShopUnlockStateService shopUnlockStateService;
    private IFacilityShopCatalog facilityShopCatalog;

    public BlueprintResearchState State => state;
    public bool HasActiveResearch => state.HasActiveTask;
    public FacilityShopUnlockState ShopUnlockState => ResolveShopUnlockStateService().GetUnlockState();

    [Inject]
    public void Construct(
        IFacilityShopUnlockStateService shopUnlockStateService,
        IFacilityShopCatalog facilityShopCatalog)
    {
        this.shopUnlockStateService = shopUnlockStateService
            ?? throw new ArgumentNullException(nameof(shopUnlockStateService));
        this.facilityShopCatalog = facilityShopCatalog
            ?? throw new ArgumentNullException(nameof(facilityShopCatalog));
    }

    public bool EnqueueBlueprint(FacilityBlueprintSO blueprint)
    {
        bool queued = state.EnqueueBlueprint(blueprint);
        if (queued)
        {
            BlueprintResearchQueuedEvent.Trigger(blueprint);
            EventAlertService.Raise(
                "연구 대기",
                $"{blueprint.DisplayName} 분석 가능",
                EventAlertImportance.Low,
                "연구");
        }

        return queued;
    }

    public BlueprintResearchWorkResult ApplyResearchWork(CharacterActor researcher, BuildableObject researchFacility, float seconds)
    {
        if (researchFacility == null || !researchFacility.SupportsWork(FacilityWorkType.Research))
        {
            return new BlueprintResearchWorkResult(false, null, 0f, 0f, 1f, false, "연구 가능한 시설이 아닙니다");
        }

        if (!state.TryGetActiveTask(out BlueprintResearchTask task))
        {
            return new BlueprintResearchWorkResult(false, null, 0f, 0f, 1f, false, "연구할 설계도가 없습니다");
        }

        float added = task.AddProgress(BlueprintResearchService.CalculateResearchWork(researcher, researchFacility, seconds));
        bool completed = task.IsCompleted;
        BlueprintResearchWorkResult result = new BlueprintResearchWorkResult(
            true,
            task.Blueprint,
            added,
            task.Progress,
            task.RequiredWork,
            completed,
            completed ? "연구 완료" : "연구 진행");

        BlueprintResearchProgressEvent.Trigger(result);

        if (completed)
        {
            CompleteTask(task.Blueprint);
        }

        return result;
    }

    public bool TryCancelBlueprint(FacilityBlueprintSO blueprint, out string message)
    {
        if (blueprint == null)
        {
            message = "설계도 정보가 없습니다";
            return false;
        }

        bool cancelled = state.TryCancelBlueprint(blueprint);
        message = cancelled
            ? $"{blueprint.DisplayName} 연구를 취소했습니다"
            : "취소할 수 있는 진행 중 연구가 없습니다";
        return cancelled;
    }

    public void OnTriggerEvent(FacilityShopPurchasedEvent eventType)
    {
        if (!eventType.result.success || eventType.result.blueprint == null)
        {
            return;
        }

        EnqueueBlueprint(eventType.result.blueprint);
    }

    private void CompleteTask(FacilityBlueprintSO blueprint)
    {
        BlueprintResearchUnlockResult unlockResult = BlueprintResearchService.ApplyCompletion(
            blueprint,
            state,
            ShopUnlockState,
            ResolveFacilityShopCatalog());
        BlueprintResearchCompletedEvent.Trigger(blueprint, unlockResult);

        if (raiseAlertOnResearchComplete)
        {
            EventAlertService.Raise(
                "연구 완료",
                FormatUnlockResult(unlockResult),
                EventAlertImportance.Medium,
                "연구");
        }
    }

    private static string FormatUnlockResult(BlueprintResearchUnlockResult result)
    {
        if (result.Blueprint == null)
        {
            return "연구 완료";
        }

        List<string> lines = new List<string> { $"{result.Blueprint.DisplayName} 분석 완료" };
        AddLines(lines, "기본 구매", result.UnlockedBasicPurchases);
        AddLines(lines, "시설 해금", result.UnlockedBuildings);
        AddLines(lines, "조합식", result.UnlockedRecipes);
        return string.Join("\n", lines);
    }

    private static void AddLines(List<string> lines, string title, IReadOnlyList<string> values)
    {
        if (values == null || values.Count == 0)
        {
            return;
        }

        lines.Add($"{title}: {string.Join(", ", values)}");
    }

    private IFacilityShopUnlockStateService ResolveShopUnlockStateService()
    {
        return shopUnlockStateService
            ?? throw new InvalidOperationException($"{nameof(BlueprintResearchRuntime)} requires VContainer injection of {nameof(IFacilityShopUnlockStateService)}.");
    }

    private IFacilityShopCatalog ResolveFacilityShopCatalog()
    {
        return facilityShopCatalog
            ?? throw new InvalidOperationException($"{nameof(BlueprintResearchRuntime)} requires VContainer injection of {nameof(IFacilityShopCatalog)}.");
    }

    private void OnEnable()
    {
        this.EventStartListening<FacilityShopPurchasedEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<FacilityShopPurchasedEvent>();
    }
}
