using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum OffenseTargetKind
{
    RivalDungeon,
    HumanOutpost,
    ResourceSite,
    SpecialEvent
}

public enum OffenseRewardCategory
{
    Money,
    Stock,
    RareFacility,
    Blueprint,
    FactionWeakening,
    RecruitCandidate,
    Prisoner
}

[Serializable]
public class OffenseRewardPreview
{
    public OffenseRewardCategory category;
    public string label;
    public int amount;

    public string ToSummaryText()
    {
        string name = string.IsNullOrWhiteSpace(label) ? category.ToString() : label;
        return amount > 0 ? $"{name} x{amount}" : name;
    }
}

[Serializable]
public class OffenseTargetDefinition
{
    public string id;
    public string title;
    [TextArea] public string description;
    public OffenseTargetKind kind;
    [Min(0f)] public float distance;
    [Min(0f)] public float danger;
    [Min(1f)] public float durationSeconds = 90f;
    [Min(1)] public int requiredMembers = 1;
    [Min(0f)] public float requiredPower;
    public OffenseRewardPreview[] rewards = Array.Empty<OffenseRewardPreview>();

    public bool IsValid => !string.IsNullOrWhiteSpace(id)
        && !string.IsNullOrWhiteSpace(title)
        && distance >= 0f
        && requiredMembers > 0;

    public OffenseTargetSnapshot ToSnapshot(bool preciseIntel)
    {
        return new OffenseTargetSnapshot
        {
            id = id,
            title = title,
            description = description,
            kind = kind,
            distance = distance,
            danger = preciseIntel ? danger : Mathf.Max(1f, Mathf.Round(danger / 5f) * 5f),
            durationSeconds = durationSeconds,
            requiredMembers = requiredMembers,
            requiredPower = preciseIntel ? requiredPower : Mathf.Max(0f, Mathf.Round(requiredPower / 5f) * 5f),
            rewards = rewards?
                .Where((reward) => reward != null)
                .Select((reward) => reward.ToSummaryText())
                .ToArray()
                ?? Array.Empty<string>()
        };
    }
}

[Serializable]
public class OffenseTargetSnapshot
{
    public string id;
    public string title;
    public string description;
    public OffenseTargetKind kind;
    public float distance;
    public float danger;
    public float durationSeconds;
    public int requiredMembers;
    public float requiredPower;
    public string[] rewards = Array.Empty<string>();

    public string ToDetailText()
    {
        List<string> lines = new List<string>
        {
            string.IsNullOrWhiteSpace(title) ? "원정 대상" : title,
            $"종류: {GetKindName(kind)}",
            $"거리: {distance:0.#}",
            $"위험도: {danger:0.#}",
            $"소요 시간: {FormatDuration(durationSeconds)}",
            $"필요 인력: {requiredMembers}",
            $"권장 전투력: {requiredPower:0.#}"
        };

        if (!string.IsNullOrWhiteSpace(description))
        {
            lines.Add(string.Empty);
            lines.Add(description);
        }

        if (rewards != null && rewards.Length > 0)
        {
            lines.Add(string.Empty);
            lines.Add("예상 보상:");
            foreach (string reward in rewards)
            {
                if (!string.IsNullOrWhiteSpace(reward))
                {
                    lines.Add($"- {reward}");
                }
            }
        }

        return string.Join("\n", lines);
    }

    private static string FormatDuration(float seconds)
    {
        int safeSeconds = Mathf.Max(1, Mathf.RoundToInt(seconds));
        int minutes = safeSeconds / 60;
        int remainSeconds = safeSeconds % 60;
        return minutes > 0 ? $"{minutes}분 {remainSeconds:00}초" : $"{remainSeconds}초";
    }

    private static string GetKindName(OffenseTargetKind kind)
    {
        return kind switch
        {
            OffenseTargetKind.HumanOutpost => "인간 거점",
            OffenseTargetKind.ResourceSite => "자원지",
            OffenseTargetKind.SpecialEvent => "특수 지점",
            OffenseTargetKind.RivalDungeon => "경쟁 던전",
            _ => kind.ToString()
        };
    }
}

public sealed class OffenseWorldMapState
{
    private readonly HashSet<string> knownTargetIds = new HashSet<string>();

    public int ReconLevel { get; private set; }
    public string SelectedTargetId { get; private set; }
    public IReadOnlyCollection<string> KnownTargetIds => knownTargetIds;

    public void Reset(int reconLevel = 0)
    {
        ReconLevel = Mathf.Max(0, reconLevel);
        SelectedTargetId = string.Empty;
        knownTargetIds.Clear();
    }

    public bool KnowTarget(string targetId)
    {
        return !string.IsNullOrWhiteSpace(targetId) && knownTargetIds.Contains(targetId);
    }

    public bool AddKnownTarget(string targetId)
    {
        return !string.IsNullOrWhiteSpace(targetId) && knownTargetIds.Add(targetId);
    }

    public void SetSelectedTarget(string targetId)
    {
        SelectedTargetId = targetId ?? string.Empty;
    }

    public bool TryUpgradeRecon(int maxLevel)
    {
        int safeMax = Mathf.Max(0, maxLevel);
        if (ReconLevel >= safeMax)
        {
            return false;
        }

        ReconLevel++;
        return true;
    }
}

public static class OffenseWorldMapService
{
    public const int MaxReconLevel = 3;

    public static float GetScanRange(int reconLevel)
    {
        return Mathf.Max(0, reconLevel) switch
        {
            0 => 8f,
            1 => 14f,
            2 => 22f,
            _ => 999f
        };
    }

    public static IReadOnlyList<OffenseTargetDefinition> CreateDefaultTargets()
    {
        return new[]
        {
            CreateTarget(
                "food_farm",
                "외곽 식재료 농장",
                "던전 근처의 작은 보급지입니다. 초반 식재료 수급을 노리기 쉽습니다.",
                OffenseTargetKind.ResourceSite,
                4f,
                8f,
                55f,
                1,
                10f,
                Reward(OffenseRewardCategory.Stock, "식재료", 40),
                Reward(OffenseRewardCategory.Money, "약탈금", 80)),
            CreateTarget(
                "merchant_road",
                "상단 길목",
                "인간 상단이 자주 지나는 길목입니다. 잡화와 돈을 노릴 수 있습니다.",
                OffenseTargetKind.HumanOutpost,
                6f,
                14f,
                70f,
                1,
                16f,
                Reward(OffenseRewardCategory.Stock, "잡화", 30),
                Reward(OffenseRewardCategory.Money, "약탈금", 120)),
            CreateTarget(
                "old_armory",
                "낡은 무기고",
                "방치된 무기고입니다. 방어 시설 합성 재료와 무기 재고를 기대할 수 있습니다.",
                OffenseTargetKind.HumanOutpost,
                12f,
                28f,
                100f,
                2,
                32f,
                Reward(OffenseRewardCategory.Stock, "무기", 25),
                Reward(OffenseRewardCategory.Blueprint, "방어 설계도", 1)),
            CreateTarget(
                "mana_ruins",
                "마력 유적",
                "마력 흔적이 강한 폐허입니다. 위험하지만 연구와 합성 보상이 좋습니다.",
                OffenseTargetKind.SpecialEvent,
                18f,
                38f,
                130f,
                2,
                42f,
                Reward(OffenseRewardCategory.Stock, "마력", 35),
                Reward(OffenseRewardCategory.Blueprint, "특수 설계도", 1)),
            CreateTarget(
                "rival_dungeon",
                "경쟁 던전 전초기지",
                "다른 몬스터 세력의 전초기지입니다. 성공하면 희귀 시설과 세력 약화를 기대할 수 있습니다.",
                OffenseTargetKind.RivalDungeon,
                26f,
                55f,
                180f,
                3,
                60f,
                Reward(OffenseRewardCategory.RareFacility, "희귀 시설", 1),
                Reward(OffenseRewardCategory.FactionWeakening, "인간 세력 약화", 1),
                Reward(OffenseRewardCategory.RecruitCandidate, "직원 후보", 1),
                Reward(OffenseRewardCategory.Prisoner, "특수 몬스터", 1))
        };
    }

    public static IReadOnlyList<OffenseTargetDefinition> NormalizeTargets(IEnumerable<OffenseTargetDefinition> targets)
    {
        List<OffenseTargetDefinition> source = targets?
            .Where((target) => target != null && target.IsValid)
            .OrderBy((target) => target.distance)
            .ThenBy((target) => target.id)
            .ToList()
            ?? new List<OffenseTargetDefinition>();

        return source.Count > 0 ? source : CreateDefaultTargets();
    }

    public static int RevealTargetsInRange(
        OffenseWorldMapState state,
        IEnumerable<OffenseTargetDefinition> targets)
    {
        if (state == null)
        {
            return 0;
        }

        float scanRange = GetScanRange(state.ReconLevel);
        int added = 0;
        foreach (OffenseTargetDefinition target in NormalizeTargets(targets))
        {
            if (target.distance <= scanRange && state.AddKnownTarget(target.id))
            {
                added++;
            }
        }

        return added;
    }

    public static IReadOnlyList<OffenseTargetSnapshot> GetVisibleTargetSnapshots(
        OffenseWorldMapState state,
        IEnumerable<OffenseTargetDefinition> targets,
        bool preciseIntel)
    {
        if (state == null)
        {
            return Array.Empty<OffenseTargetSnapshot>();
        }

        return NormalizeTargets(targets)
            .Where((target) => state.KnowTarget(target.id))
            .Select((target) => target.ToSnapshot(preciseIntel))
            .ToList();
    }

    public static OffenseTargetDefinition FindKnownTarget(
        OffenseWorldMapState state,
        IEnumerable<OffenseTargetDefinition> targets,
        string targetId)
    {
        if (state == null || string.IsNullOrWhiteSpace(targetId) || !state.KnowTarget(targetId))
        {
            return null;
        }

        return NormalizeTargets(targets).FirstOrDefault((target) => target.id == targetId);
    }

    private static OffenseTargetDefinition CreateTarget(
        string id,
        string title,
        string description,
        OffenseTargetKind kind,
        float distance,
        float danger,
        float durationSeconds,
        int requiredMembers,
        float requiredPower,
        params OffenseRewardPreview[] rewards)
    {
        return new OffenseTargetDefinition
        {
            id = id,
            title = title,
            description = description,
            kind = kind,
            distance = distance,
            danger = danger,
            durationSeconds = durationSeconds,
            requiredMembers = requiredMembers,
            requiredPower = requiredPower,
            rewards = rewards ?? Array.Empty<OffenseRewardPreview>()
        };
    }

    private static OffenseRewardPreview Reward(OffenseRewardCategory category, string label, int amount)
    {
        return new OffenseRewardPreview
        {
            category = category,
            label = label,
            amount = amount
        };
    }
}

public struct OffenseWorldMapChangedEvent
{
    public OffenseWorldMapState state;
    public IReadOnlyList<OffenseTargetSnapshot> visibleTargets;

    public OffenseWorldMapChangedEvent(
        OffenseWorldMapState state,
        IReadOnlyList<OffenseTargetSnapshot> visibleTargets)
    {
        this.state = state;
        this.visibleTargets = visibleTargets ?? Array.Empty<OffenseTargetSnapshot>();
    }

    private static OffenseWorldMapChangedEvent e;

    public static void Trigger(
        OffenseWorldMapState state,
        IReadOnlyList<OffenseTargetSnapshot> visibleTargets)
    {
        e.state = state;
        e.visibleTargets = visibleTargets ?? Array.Empty<OffenseTargetSnapshot>();
        EventObserver.TriggerEvent(e);
    }
}

public struct OffenseTargetSelectedEvent
{
    public OffenseTargetSnapshot target;

    public OffenseTargetSelectedEvent(OffenseTargetSnapshot target)
    {
        this.target = target;
    }

    private static OffenseTargetSelectedEvent e;

    public static void Trigger(OffenseTargetSnapshot target)
    {
        e.target = target;
        EventObserver.TriggerEvent(e);
    }
}

public struct OffenseReconUpgradedEvent
{
    public int reconLevel;
    public float scanRange;
    public int newlyRevealedCount;

    public OffenseReconUpgradedEvent(int reconLevel, float scanRange, int newlyRevealedCount)
    {
        this.reconLevel = reconLevel;
        this.scanRange = scanRange;
        this.newlyRevealedCount = newlyRevealedCount;
    }

    private static OffenseReconUpgradedEvent e;

    public static void Trigger(int reconLevel, float scanRange, int newlyRevealedCount)
    {
        e.reconLevel = reconLevel;
        e.scanRange = scanRange;
        e.newlyRevealedCount = newlyRevealedCount;
        EventObserver.TriggerEvent(e);
    }
}

public class OffenseWorldMapRuntime : MonoBehaviour
{
    [SerializeField] private List<OffenseTargetDefinition> configuredTargets = new List<OffenseTargetDefinition>();
    [SerializeField] private bool preciseIntel;

    private readonly OffenseWorldMapState state = new OffenseWorldMapState();
    private List<OffenseTargetDefinition> targets;
    private static OffenseWorldMapRuntime instance;

    public static OffenseWorldMapRuntime Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<OffenseWorldMapRuntime>();
            }

            return instance;
        }
    }

    public OffenseWorldMapState State => state;
    public IReadOnlyList<OffenseTargetDefinition> Targets
    {
        get
        {
            EnsureInitialized();
            return targets;
        }
    }

    public IReadOnlyList<OffenseTargetSnapshot> VisibleTargets
    {
        get
        {
            EnsureInitialized();
            return OffenseWorldMapService.GetVisibleTargetSnapshots(state, targets, preciseIntel);
        }
    }

    public float CurrentScanRange => OffenseWorldMapService.GetScanRange(state.ReconLevel);

    private void Awake()
    {
        instance = this;
        StartWorldMap();
    }

    public void StartWorldMap(int reconLevel = 0)
    {
        state.Reset(reconLevel);
        targets = OffenseWorldMapService.NormalizeTargets(configuredTargets).ToList();
        OffenseWorldMapService.RevealTargetsInRange(state, targets);
        RaiseChanged();
    }

    public bool TryUpgradeRecon(out string message)
    {
        EnsureInitialized();
        if (!state.TryUpgradeRecon(OffenseWorldMapService.MaxReconLevel))
        {
            message = "정찰 범위가 이미 최대입니다";
            return false;
        }

        int newlyRevealed = OffenseWorldMapService.RevealTargetsInRange(state, targets);
        message = $"정찰 Lv.{state.ReconLevel}: 새 원정 대상 {newlyRevealed}개 발견";
        OffenseReconUpgradedEvent.Trigger(state.ReconLevel, CurrentScanRange, newlyRevealed);
        EventAlertService.Raise("정찰 강화", message, EventAlertImportance.Medium, "오펜스");
        RaiseChanged();
        return true;
    }

    public bool TrySelectTarget(string targetId, out OffenseTargetSnapshot snapshot, out string message)
    {
        EnsureInitialized();
        OffenseTargetDefinition target = OffenseWorldMapService.FindKnownTarget(state, targets, targetId);
        if (target == null)
        {
            snapshot = null;
            message = "발견되지 않은 원정 대상입니다";
            return false;
        }

        state.SetSelectedTarget(target.id);
        snapshot = target.ToSnapshot(preciseIntel);
        message = $"{snapshot.title} 선택";
        OffenseTargetSelectedEvent.Trigger(snapshot);
        RaiseChanged();
        return true;
    }

    public bool TryGetKnownTargetSnapshot(string targetId, out OffenseTargetSnapshot snapshot)
    {
        EnsureInitialized();
        OffenseTargetDefinition target = OffenseWorldMapService.FindKnownTarget(state, targets, targetId);
        if (target == null)
        {
            snapshot = null;
            return false;
        }

        snapshot = target.ToSnapshot(preciseIntel);
        return true;
    }

    public OffenseWorldMapPanel ShowWorldMap()
    {
        EnsureInitialized();
        return OffenseWorldMapPanel.Show(this);
    }

    public void SetPreciseIntelForDebug(bool value)
    {
        preciseIntel = value;
        RaiseChanged();
    }

    public void SetTargetsForDebug(IEnumerable<OffenseTargetDefinition> debugTargets)
    {
        configuredTargets = debugTargets?.Where((target) => target != null).ToList()
            ?? new List<OffenseTargetDefinition>();
        StartWorldMap(state.ReconLevel);
    }

    private void EnsureInitialized()
    {
        if (targets != null) return;

        targets = OffenseWorldMapService.NormalizeTargets(configuredTargets).ToList();
        if (state.KnownTargetIds.Count == 0)
        {
            OffenseWorldMapService.RevealTargetsInRange(state, targets);
        }
    }

    private void RaiseChanged()
    {
        if (targets == null)
        {
            return;
        }

        OffenseWorldMapChangedEvent.Trigger(state, VisibleTargets);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}

public class OffenseWorldMapPanel : MonoBehaviour
{
    private OffenseWorldMapRuntime runtime;
    private TMP_Text headerText;
    private TMP_Text detailText;
    private RectTransform targetButtonRoot;
    private readonly List<GameObject> spawnedButtons = new List<GameObject>();

    public static OffenseWorldMapPanel Show(OffenseWorldMapRuntime runtime)
    {
        OffenseWorldMapPanel panel = FindFirstObjectByType<OffenseWorldMapPanel>();
        if (panel == null)
        {
            panel = CreateDefaultPanel();
        }

        panel.Bind(runtime);
        return panel;
    }

    public void Bind(OffenseWorldMapRuntime source)
    {
        runtime = source != null ? source : OffenseWorldMapRuntime.Instance;
        EnsureView();
        gameObject.SetActive(true);
        Render();
    }

    public void Render()
    {
        if (runtime == null)
        {
            return;
        }

        EnsureView();
        headerText.text = $"월드맵 / 정찰 Lv.{runtime.State.ReconLevel} / 범위 {runtime.CurrentScanRange:0.#}";
        ClearButtons();

        foreach (OffenseTargetSnapshot target in runtime.VisibleTargets)
        {
            GameObject buttonObject = CreateButton(
                targetButtonRoot,
                target.title,
                () =>
                {
                    if (runtime.TrySelectTarget(target.id, out OffenseTargetSnapshot selected, out _))
                    {
                        detailText.text = selected.ToDetailText();
                    }

                    Render();
                });
            spawnedButtons.Add(buttonObject);
        }

        GameObject upgradeButton = CreateButton(
            targetButtonRoot,
            "정찰 강화",
            () =>
            {
                runtime.TryUpgradeRecon(out string message);
                detailText.text = message;
                Render();
            });
        spawnedButtons.Add(upgradeButton);

        if (runtime.VisibleTargets.Count == 0)
        {
            detailText.text = "발견된 원정 대상이 없습니다.";
        }
        else if (!string.IsNullOrWhiteSpace(runtime.State.SelectedTargetId)
            && runtime.TryGetKnownTargetSnapshot(runtime.State.SelectedTargetId, out OffenseTargetSnapshot selected))
        {
            detailText.text = selected.ToDetailText();
        }
        else
        {
            detailText.text = runtime.VisibleTargets[0].ToDetailText();
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void EnsureView()
    {
        if (headerText != null && detailText != null && targetButtonRoot != null) return;

        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        headerText = texts.FirstOrDefault((text) => text.name == "OffenseWorldMapHeader");
        detailText = texts.FirstOrDefault((text) => text.name == "OffenseWorldMapDetail");
        targetButtonRoot = GetComponentsInChildren<RectTransform>(true)
            .FirstOrDefault((rect) => rect.name == "OffenseWorldMapTargets");
    }

    private void ClearButtons()
    {
        foreach (GameObject button in spawnedButtons)
        {
            if (button != null)
            {
                Destroy(button);
            }
        }

        spawnedButtons.Clear();
    }

    private static OffenseWorldMapPanel CreateDefaultPanel()
    {
        GameObject canvasObject = new GameObject("OffenseWorldMapCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 420;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);

        GameObject panelObject = new GameObject("OffenseWorldMapPanel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.12f, 0.12f);
        panelRect.anchorMax = new Vector2(0.88f, 0.88f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panelObject.GetComponent<Image>().color = new Color(0.035f, 0.04f, 0.05f, 0.94f);

        GameObject header = CreateText(panelObject.transform, "OffenseWorldMapHeader", 25f, TextAlignmentOptions.Left);
        RectTransform headerRect = header.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.offsetMin = new Vector2(24f, -66f);
        headerRect.offsetMax = new Vector2(-24f, -18f);

        GameObject buttonRootObject = new GameObject("OffenseWorldMapTargets", typeof(RectTransform), typeof(VerticalLayoutGroup));
        buttonRootObject.transform.SetParent(panelObject.transform, false);
        RectTransform buttonRootRect = buttonRootObject.GetComponent<RectTransform>();
        buttonRootRect.anchorMin = new Vector2(0f, 0f);
        buttonRootRect.anchorMax = new Vector2(0.36f, 0.86f);
        buttonRootRect.offsetMin = new Vector2(24f, 24f);
        buttonRootRect.offsetMax = new Vector2(-12f, -24f);
        VerticalLayoutGroup layout = buttonRootObject.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandHeight = false;

        GameObject detail = CreateText(panelObject.transform, "OffenseWorldMapDetail", 20f, TextAlignmentOptions.TopLeft);
        RectTransform detailRect = detail.GetComponent<RectTransform>();
        detailRect.anchorMin = new Vector2(0.38f, 0f);
        detailRect.anchorMax = new Vector2(1f, 0.86f);
        detailRect.offsetMin = new Vector2(12f, 24f);
        detailRect.offsetMax = new Vector2(-24f, -24f);

        OffenseWorldMapPanel panel = panelObject.AddComponent<OffenseWorldMapPanel>();
        panel.headerText = header.GetComponent<TMP_Text>();
        panel.detailText = detail.GetComponent<TMP_Text>();
        panel.targetButtonRoot = buttonRootObject.GetComponent<RectTransform>();
        return panel;
    }

    private static GameObject CreateText(Transform parent, string name, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        TMPKoreanFont.Apply(text);
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        return textObject;
    }

    private static GameObject CreateButton(RectTransform parent, string label, Action callback)
    {
        GameObject buttonObject = new GameObject($"Button_{label}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);
        buttonObject.GetComponent<Image>().color = new Color(0.16f, 0.18f, 0.22f, 0.96f);
        buttonObject.GetComponent<LayoutElement>().preferredHeight = 42f;

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(() => callback?.Invoke());

        GameObject textObject = CreateText(buttonObject.transform, "Label", 17f, TextAlignmentOptions.Center);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 4f);
        textRect.offsetMax = new Vector2(-8f, -4f);
        textObject.GetComponent<TMP_Text>().text = label;
        return buttonObject;
    }
}
