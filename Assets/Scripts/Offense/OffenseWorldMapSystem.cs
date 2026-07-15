using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using VContainer;

public class OffenseWorldMapRuntime : MonoBehaviour
{
    [SerializeField] private List<OffenseTargetDefinition> configuredTargets = new List<OffenseTargetDefinition>();
    [SerializeField] private bool preciseIntel;

    private readonly OffenseWorldMapState state = new OffenseWorldMapState();
    private List<OffenseTargetDefinition> targets;
    private IOffensePanelService panelService;

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

    [Inject]
    public void Construct(IOffensePanelService panelService)
    {
        this.panelService = panelService
            ?? throw new ArgumentNullException(nameof(panelService));
    }

    private void Awake()
    {
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
        return ResolvePanelService().ShowWorldMap(this);
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

    private IOffensePanelService ResolvePanelService()
    {
        return panelService
            ?? throw new InvalidOperationException($"{nameof(OffenseWorldMapRuntime)} requires {nameof(IOffensePanelService)} injection.");
    }
}

public class OffenseWorldMapPanel : MonoBehaviour
{
    private OffenseWorldMapRuntime runtime;
    private TMP_Text headerText;
    private TMP_Text detailText;
    private RectTransform targetButtonRoot;
    private readonly List<GameObject> spawnedButtons = new List<GameObject>();
    private IOffensePanelButtonFactory buttonFactory;

    public void Bind(OffenseWorldMapRuntime source, IOffensePanelButtonFactory buttonFactory)
    {
        runtime = source
            ?? throw new ArgumentNullException(nameof(source));
        this.buttonFactory = buttonFactory
            ?? throw new ArgumentNullException(nameof(buttonFactory));
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
            GameObject buttonObject = RequireButtonFactory().CreateButton(
                targetButtonRoot,
                target.title,
                17f,
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

        GameObject upgradeButton = RequireButtonFactory().CreateButton(
            targetButtonRoot,
            "정찰 강화",
            17f,
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
            RequireButtonFactory().Release(button);
        }

        spawnedButtons.Clear();
    }

    internal void BindGeneratedView(
        TMP_Text headerText,
        TMP_Text detailText,
        RectTransform targetButtonRoot)
    {
        this.headerText = headerText != null
            ? headerText
            : throw new ArgumentNullException(nameof(headerText));
        this.detailText = detailText != null
            ? detailText
            : throw new ArgumentNullException(nameof(detailText));
        this.targetButtonRoot = targetButtonRoot != null
            ? targetButtonRoot
            : throw new ArgumentNullException(nameof(targetButtonRoot));
    }

    private IOffensePanelButtonFactory RequireButtonFactory()
    {
        return buttonFactory
            ?? throw new InvalidOperationException(
                $"{nameof(OffenseWorldMapPanel)} requires {nameof(IOffensePanelButtonFactory)} binding.");
    }
}
