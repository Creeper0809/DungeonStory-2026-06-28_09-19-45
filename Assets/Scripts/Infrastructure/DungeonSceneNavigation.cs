using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer.Unity;

public enum DungeonGameplayLaunchMode
{
    None,
    NewRun,
    LoadSlot
}

public readonly struct DungeonGameplayLaunchRequest
{
    public DungeonGameplayLaunchRequest(
        DungeonGameplayLaunchMode mode,
        string slotId = "",
        DungeonDifficulty difficulty = DungeonDifficulty.Normal)
    {
        Mode = mode;
        SlotId = slotId ?? string.Empty;
        Difficulty = difficulty;
    }

    public DungeonGameplayLaunchMode Mode { get; }
    public string SlotId { get; }
    public DungeonDifficulty Difficulty { get; }
}

public interface IDungeonSceneNavigator
{
    bool IsTransitioning { get; }
    bool StartNewGame();
    bool StartNewGame(DungeonDifficulty difficulty);
    bool LoadGame(string slotId);
    bool LoadTitle(string message = "");
    bool TryConsumeGameplayLaunch(out DungeonGameplayLaunchRequest request);
    string ConsumeTitleMessage();
}

public sealed class DungeonSceneNavigator : IDungeonSceneNavigator
{
    public const string TitleSceneName = "TitleScene";
    public const string GameplaySceneName = "SampleScene";

    private static DungeonSceneTransitionHost transitionHost;
    private static DungeonGameplayLaunchRequest? pendingGameplayLaunch;
    private static string pendingTitleMessage = string.Empty;
    private static bool isTransitioning;

    public bool IsTransitioning => isTransitioning;

    public bool StartNewGame()
    {
        return StartNewGame(DungeonDifficulty.Normal);
    }

    public bool StartNewGame(DungeonDifficulty difficulty)
    {
        return BeginGameplayTransition(new DungeonGameplayLaunchRequest(
            DungeonGameplayLaunchMode.NewRun,
            difficulty: difficulty));
    }

    public bool LoadGame(string slotId)
    {
        if (string.IsNullOrWhiteSpace(slotId))
        {
            return false;
        }

        return BeginGameplayTransition(new DungeonGameplayLaunchRequest(
            DungeonGameplayLaunchMode.LoadSlot,
            slotId.Trim()));
    }

    public bool LoadTitle(string message = "")
    {
        if (!BeginTransition(TitleSceneName, HandleTitleTransitionFailure))
        {
            return false;
        }

        pendingGameplayLaunch = null;
        pendingTitleMessage = message?.Trim() ?? string.Empty;
        return true;
    }

    public bool TryConsumeGameplayLaunch(out DungeonGameplayLaunchRequest request)
    {
        if (!pendingGameplayLaunch.HasValue)
        {
            request = default;
            return false;
        }

        request = pendingGameplayLaunch.Value;
        pendingGameplayLaunch = null;
        return request.Mode != DungeonGameplayLaunchMode.None;
    }

    public string ConsumeTitleMessage()
    {
        string message = pendingTitleMessage;
        pendingTitleMessage = string.Empty;
        return message;
    }

    private static bool BeginGameplayTransition(DungeonGameplayLaunchRequest request)
    {
        if (!BeginTransition(GameplaySceneName, HandleGameplayTransitionFailure))
        {
            return false;
        }

        pendingTitleMessage = string.Empty;
        pendingGameplayLaunch = request;
        return true;
    }

    private static bool BeginTransition(string targetScene, Action<string> onFailure)
    {
        if (isTransitioning || string.IsNullOrWhiteSpace(targetScene))
        {
            return false;
        }

        if (!Application.CanStreamedLevelBeLoaded(targetScene))
        {
            onFailure?.Invoke($"Scene '{targetScene}' is not available in build settings.");
            return false;
        }

        Time.timeScale = 1f;
        isTransitioning = true;
        GameObject hostObject = new GameObject("DungeonSceneTransitionHost");
        UnityEngine.Object.DontDestroyOnLoad(hostObject);
        transitionHost = hostObject.AddComponent<DungeonSceneTransitionHost>();
        transitionHost.Begin(targetScene, CompleteTransition, onFailure);
        return true;
    }

    private static void CompleteTransition()
    {
        isTransitioning = false;
        transitionHost = null;
    }

    private static void HandleGameplayTransitionFailure(string message)
    {
        pendingGameplayLaunch = null;
        pendingTitleMessage = string.IsNullOrWhiteSpace(message)
            ? "게임 화면을 불러오지 못했습니다."
            : message;
        CompleteTransition();
    }

    private static void HandleTitleTransitionFailure(string message)
    {
        pendingTitleMessage = string.IsNullOrWhiteSpace(message)
            ? "타이틀 화면을 불러오지 못했습니다."
            : message;
        CompleteTransition();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        transitionHost = null;
        pendingGameplayLaunch = null;
        pendingTitleMessage = string.Empty;
        isTransitioning = false;
    }
}

public sealed class DungeonGameplayLaunchController : IStartable, ITickable
{
    private readonly IDungeonSceneNavigator sceneNavigator;
    private readonly IDungeonGameSaveSlotService slotService;
    private readonly IDungeonSceneComponentQuery sceneQuery;
    private readonly IInvasionThreatRuntimeProvider threatProvider;

    private DungeonGameplayLaunchRequest request;
    private bool pending;
    private string pendingTitleFailure = string.Empty;

    public DungeonGameplayLaunchController(
        IDungeonSceneNavigator sceneNavigator,
        IDungeonGameSaveSlotService slotService,
        IDungeonSceneComponentQuery sceneQuery,
        IInvasionThreatRuntimeProvider threatProvider)
    {
        this.sceneNavigator = sceneNavigator ?? throw new ArgumentNullException(nameof(sceneNavigator));
        this.slotService = slotService ?? throw new ArgumentNullException(nameof(slotService));
        this.sceneQuery = sceneQuery ?? throw new ArgumentNullException(nameof(sceneQuery));
        this.threatProvider = threatProvider ?? throw new ArgumentNullException(nameof(threatProvider));
    }

    public void Start()
    {
        pending = sceneNavigator.TryConsumeGameplayLaunch(out request);
    }

    public void Tick()
    {
        if (!string.IsNullOrWhiteSpace(pendingTitleFailure) && !sceneNavigator.IsTransitioning)
        {
            string message = pendingTitleFailure;
            pendingTitleFailure = string.Empty;
            sceneNavigator.LoadTitle(message);
            return;
        }

        if (!pending)
        {
            return;
        }

        pending = false;
        switch (request.Mode)
        {
            case DungeonGameplayLaunchMode.NewRun:
                DeleteRunSlots();
                ApplyNewRunDifficulty(request.Difficulty);
                RefreshOwnerSelection();
                break;
            case DungeonGameplayLaunchMode.LoadSlot:
                RestoreSlot(request.SlotId);
                break;
        }
    }

    private void RestoreSlot(string slotId)
    {
        if (slotService.TryLoad(slotId, out DungeonGameRestoreReport report))
        {
            RefreshOwnerSelection();
            return;
        }

        string reason = report?.Errors != null && report.Errors.Count > 0
            ? string.Join(" ", report.Errors)
            : "저장 데이터를 복원하지 못했습니다.";
        pendingTitleFailure = reason;
    }

    private void RefreshOwnerSelection()
    {
        foreach (OwnerSelectionPanel panel in sceneQuery.All<OwnerSelectionPanel>(includeInactive: true))
        {
            panel.RefreshVisibility();
        }
    }

    private void DeleteRunSlots()
    {
        slotService.Delete(DungeonGameSaveSlotService.AutoSaveSlot);
        slotService.Delete(DungeonGameSaveSlotService.QuickSaveSlot);
        slotService.Delete(DungeonGameSaveSlotService.ManualSaveSlot);
    }

    private void ApplyNewRunDifficulty(DungeonDifficulty difficulty)
    {
        if (threatProvider.TryGetRuntime(out InvasionThreatRuntime threat)
            && threat.Settings != null)
        {
            threat.Settings.difficulty = DungeonDifficultyRules.ToLegacy(difficulty);
        }
    }
}

public sealed class DungeonSceneTransitionHost : MonoBehaviour
{
    private const float FadeSeconds = 0.18f;

    private Image blocker;
    private Action onComplete;
    private Action<string> onFailure;

    public void Begin(string targetScene, Action complete, Action<string> failure)
    {
        onComplete = complete;
        onFailure = failure;
        CreateOverlay();
        StartCoroutine(Transition(targetScene));
    }

    private IEnumerator Transition(string targetScene)
    {
        yield return Fade(0f, 1f);

        AsyncOperation operation;
        try
        {
            operation = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Single);
        }
        catch (Exception exception)
        {
            Fail(exception.Message);
            yield break;
        }

        if (operation == null)
        {
            Fail($"Scene '{targetScene}' did not start loading.");
            yield break;
        }

        while (!operation.isDone)
        {
            yield return null;
        }

        yield return null;
        yield return Fade(1f, 0f);
        onComplete?.Invoke();
        Destroy(gameObject);
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < FadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / FadeSeconds)));
            yield return null;
        }

        SetAlpha(to);
    }

    private void CreateOverlay()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = short.MaxValue;
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        GameObject blockerObject = new GameObject("SceneTransitionInputBlocker", typeof(RectTransform), typeof(Image));
        blockerObject.transform.SetParent(transform, false);
        RectTransform rect = blockerObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        blocker = blockerObject.GetComponent<Image>();
        blocker.raycastTarget = true;
        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        if (blocker != null)
        {
            blocker.color = new Color(0.018f, 0.027f, 0.031f, Mathf.Clamp01(alpha));
        }
    }

    private void Fail(string message)
    {
        onFailure?.Invoke(message);
        Destroy(gameObject);
    }
}
