using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using VContainer;

public class OffenseExpeditionRuntime : MonoBehaviour
{
    private const int MaxResultHistory = 20;

    private readonly List<OffenseExpeditionRun> activeExpeditions = new List<OffenseExpeditionRun>();
    private readonly List<OffenseExpeditionResult> resultHistory = new List<OffenseExpeditionResult>();
    private IOffenseExpeditionMemberQuery memberQuery;
    private IOffenseWorldMapRuntimeProvider worldMapProvider;
    private IOffenseRewardRuntimeProvider rewardProvider;
    private IMetaProgressionRuntimeProvider metaProgressionProvider;
    private IOffensePanelService panelService;

    public IReadOnlyList<OffenseExpeditionRun> ActiveExpeditions => activeExpeditions;
    public IReadOnlyList<OffenseExpeditionResult> ResultHistory => resultHistory;

    [Inject]
    public void Construct(
        IOffenseExpeditionMemberQuery memberQuery,
        IOffenseWorldMapRuntimeProvider worldMapProvider,
        IOffenseRewardRuntimeProvider rewardProvider,
        IMetaProgressionRuntimeProvider metaProgressionProvider,
        IOffensePanelService panelService)
    {
        this.memberQuery = memberQuery
            ?? throw new ArgumentNullException(nameof(memberQuery));
        this.worldMapProvider = worldMapProvider
            ?? throw new ArgumentNullException(nameof(worldMapProvider));
        this.rewardProvider = rewardProvider
            ?? throw new ArgumentNullException(nameof(rewardProvider));
        this.metaProgressionProvider = metaProgressionProvider
            ?? throw new ArgumentNullException(nameof(metaProgressionProvider));
        this.panelService = panelService
            ?? throw new ArgumentNullException(nameof(panelService));
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    public IReadOnlyList<CharacterActor> GetAvailableMemberActors()
    {
        return ResolveMemberQuery().GetAvailableMemberActors();
    }

    public bool TryStartExpedition(
        string targetId,
        IEnumerable<CharacterActor> members,
        out OffenseExpeditionRun expedition,
        out string message)
    {
        expedition = null;
        if (!ResolveWorldMapProvider().TryGetRuntime(out OffenseWorldMapRuntime worldMap))
        {
            message = "월드맵이 초기화되지 않았습니다";
            return false;
        }

        OffenseTargetDefinition target = OffenseWorldMapService.FindKnownTarget(
            worldMap.State,
            worldMap.Targets,
            targetId);
        if (target == null)
        {
            message = "발견되지 않은 원정 대상입니다";
            return false;
        }

        List<CharacterActor> party = members?
            .Where((member) => member != null)
            .Distinct()
            .ToList()
            ?? new List<CharacterActor>();
        if (party.Count < target.requiredMembers)
        {
            message = $"필요 인력 부족: {party.Count}/{target.requiredMembers}";
            return false;
        }

        foreach (CharacterActor member in party)
        {
            if (!OffenseExpeditionService.CanJoinExpedition(member, out string reason))
            {
                message = $"{member.name}: {reason}";
                return false;
            }
        }

        float totalPower = OffenseExpeditionService.CalculatePartyPower(party);
        expedition = new OffenseExpeditionRun(Guid.NewGuid().ToString("N"), target, party, totalPower);
        foreach (CharacterActor member in party)
        {
            member.BeginExpedition();
        }

        activeExpeditions.Add(expedition);
        message = $"{target.title} 원정 출발: {party.Count}명 / 전투력 {totalPower:0.#}";
        OffenseExpeditionStartedEvent.Trigger(expedition);
        EventAlertService.Raise("원정 출발", message, EventAlertImportance.Medium, "오펜스");
        return true;
    }

    public void Tick(float deltaTime)
    {
        for (int i = activeExpeditions.Count - 1; i >= 0; i--)
        {
            OffenseExpeditionRun expedition = activeExpeditions[i];
            expedition.Tick(deltaTime);
            if (expedition.IsComplete)
            {
                CompleteExpeditionAt(i, null);
            }
        }
    }

    public bool CompleteExpeditionForDebug(
        string expeditionId,
        bool? forceSuccess,
        out OffenseExpeditionResult result)
    {
        int index = activeExpeditions.FindIndex((expedition) => expedition.ExpeditionId == expeditionId);
        if (index < 0)
        {
            result = null;
            return false;
        }

        result = CompleteExpeditionAt(index, forceSuccess);
        return result != null;
    }

    private OffenseExpeditionResult CompleteExpeditionAt(int index, bool? forceSuccess)
    {
        if (index < 0 || index >= activeExpeditions.Count)
        {
            return null;
        }

        OffenseExpeditionRun expedition = activeExpeditions[index];
        activeExpeditions.RemoveAt(index);
        OffenseExpeditionResult result = OffenseExpeditionService.Resolve(expedition, forceSuccess);
        if (result == null)
        {
            return null;
        }

        if (result.success
            && ResolveMetaProgressionProvider().TryGetRuntime(out MetaProgressionRuntime metaProgression))
        {
            metaProgression.RecordOffenseSuccess();
        }

        if (result.success
            && ResolveRewardProvider().TryGetRuntime(out OffenseRewardRuntime rewards))
        {
            IReadOnlyList<OffenseRewardGrantResult> grantedRewards =
                rewards.ApplyExpeditionRewards(expedition, result);
            result.grantedRewards = grantedRewards?.ToArray() ?? Array.Empty<OffenseRewardGrantResult>();
            if (result.grantedRewards.Length > 0)
            {
                result.rewardSummaries = result.grantedRewards
                    .Where((reward) => reward != null)
                    .Select((reward) => reward.ToSummaryText())
                    .ToArray();
            }
        }

        resultHistory.Insert(0, result);
        if (resultHistory.Count > MaxResultHistory)
        {
            resultHistory.RemoveRange(MaxResultHistory, resultHistory.Count - MaxResultHistory);
        }

        OffenseExpeditionCompletedEvent.Trigger(result);
        EventAlertService.Raise("원정 결과", result.ToDetailText(), result.success ? EventAlertImportance.Medium : EventAlertImportance.High, "오펜스");
        return result;
    }

    public OffenseExpeditionPanel ShowExpeditionPanel()
    {
        return ResolvePanelService().ShowExpedition(this);
    }

    private IOffenseExpeditionMemberQuery ResolveMemberQuery()
    {
        return memberQuery
            ?? throw new InvalidOperationException($"{nameof(OffenseExpeditionRuntime)} requires {nameof(IOffenseExpeditionMemberQuery)} injection.");
    }

    private IOffenseWorldMapRuntimeProvider ResolveWorldMapProvider()
    {
        return worldMapProvider
            ?? throw new InvalidOperationException($"{nameof(OffenseExpeditionRuntime)} requires {nameof(IOffenseWorldMapRuntimeProvider)} injection.");
    }

    private IOffenseRewardRuntimeProvider ResolveRewardProvider()
    {
        return rewardProvider
            ?? throw new InvalidOperationException($"{nameof(OffenseExpeditionRuntime)} requires {nameof(IOffenseRewardRuntimeProvider)} injection.");
    }

    private IMetaProgressionRuntimeProvider ResolveMetaProgressionProvider()
    {
        return metaProgressionProvider
            ?? throw new InvalidOperationException($"{nameof(OffenseExpeditionRuntime)} requires {nameof(IMetaProgressionRuntimeProvider)} injection.");
    }

    private IOffensePanelService ResolvePanelService()
    {
        return panelService
            ?? throw new InvalidOperationException($"{nameof(OffenseExpeditionRuntime)} requires {nameof(IOffensePanelService)} injection.");
    }
}

public class OffenseExpeditionPanel : MonoBehaviour
{
    private OffenseExpeditionRuntime runtime;
    private TMP_Text headerText;
    private TMP_Text detailText;
    private RectTransform memberButtonRoot;
    private readonly List<GameObject> spawnedButtons = new List<GameObject>();
    private readonly HashSet<CharacterActor> selectedMembers = new HashSet<CharacterActor>();
    private OffenseWorldMapRuntime worldMap;
    private string statusMessage;
    private IOffensePanelButtonFactory buttonFactory;

    public void Bind(
        OffenseExpeditionRuntime source,
        OffenseWorldMapRuntime worldMap,
        IOffensePanelButtonFactory buttonFactory)
    {
        runtime = source
            ?? throw new ArgumentNullException(nameof(source));
        this.worldMap = worldMap;
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
        string selectedTargetId = worldMap != null ? worldMap.State.SelectedTargetId : string.Empty;
        OffenseTargetSnapshot target = null;
        if (worldMap != null && !string.IsNullOrWhiteSpace(selectedTargetId))
        {
            worldMap.TryGetKnownTargetSnapshot(selectedTargetId, out target);
        }

        headerText.text = target != null
            ? $"원정 편성 / 대상: {target.title} / 필요 {target.requiredMembers}명 / 권장 {target.requiredPower:0.#}"
            : "원정 편성 / 선택된 대상 없음";

        ClearButtons();
        foreach (CharacterActor member in runtime.GetAvailableMemberActors())
        {
            CharacterActor captured = member;
            string label = BuildMemberLabel(captured);
            GameObject buttonObject = RequireButtonFactory().CreateButton(
                memberButtonRoot,
                selectedMembers.Contains(captured) ? $"[선택] {label}" : label,
                16f,
                () =>
                {
                    if (!selectedMembers.Add(captured))
                    {
                        selectedMembers.Remove(captured);
                    }

                    Render();
                });
            spawnedButtons.Add(buttonObject);
        }

        GameObject startButton = RequireButtonFactory().CreateButton(
            memberButtonRoot,
            "원정 출발",
            16f,
            () =>
            {
                if (string.IsNullOrWhiteSpace(selectedTargetId))
                {
                    statusMessage = "선택된 원정 대상이 없습니다.";
                    Render();
                    return;
                }

                if (runtime.TryStartExpedition(selectedTargetId, selectedMembers, out OffenseExpeditionRun expedition, out string message))
                {
                    selectedMembers.Clear();
                    statusMessage = $"{message}\n남은 시간: {expedition.RemainingSeconds:0.#}초";
                }
                else
                {
                    statusMessage = message;
                }

                Render();
            });
        spawnedButtons.Add(startButton);

        float selectedPower = OffenseExpeditionService.CalculatePartyPower(selectedMembers);
        string detail = target != null
            ? $"{target.ToDetailText()}\n\n선택 인원: {selectedMembers.Count}\n선택 전투력: {selectedPower:0.#}"
            : $"선택 인원: {selectedMembers.Count}\n선택 전투력: {selectedPower:0.#}";
        detailText.text = string.IsNullOrWhiteSpace(statusMessage)
            ? detail
            : $"{detail}\n\n{statusMessage}";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private static string BuildMemberLabel(CharacterActor member)
    {
        if (member == null)
        {
            return "알 수 없음";
        }

        member.EnsureRuntimeState();
        CharacterIdentity identity = member.Identity;
        string name = identity != null ? identity.DisplayName : member.name;
        string speciesTag = identity != null ? identity.SpeciesTag : string.Empty;
        return $"{name} / {speciesTag} / 전투력 {OffenseExpeditionService.CalculateMemberPower(member):0.#}";
    }

    private void EnsureView()
    {
        if (headerText != null && detailText != null && memberButtonRoot != null) return;

        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        headerText = texts.FirstOrDefault((text) => text.name == "OffenseExpeditionHeader");
        detailText = texts.FirstOrDefault((text) => text.name == "OffenseExpeditionDetail");
        memberButtonRoot = GetComponentsInChildren<RectTransform>(true)
            .FirstOrDefault((rect) => rect.name == "OffenseExpeditionMembers");
    }

    private void ClearButtons()
    {
        foreach (GameObject button in spawnedButtons)
        {
            if (button != null)
            {
                RequireButtonFactory().Release(button);
            }
        }

        spawnedButtons.Clear();
    }

    internal void BindGeneratedView(
        TMP_Text headerText,
        TMP_Text detailText,
        RectTransform memberButtonRoot)
    {
        this.headerText = headerText != null
            ? headerText
            : throw new ArgumentNullException(nameof(headerText));
        this.detailText = detailText != null
            ? detailText
            : throw new ArgumentNullException(nameof(detailText));
        this.memberButtonRoot = memberButtonRoot != null
            ? memberButtonRoot
            : throw new ArgumentNullException(nameof(memberButtonRoot));
    }

    private IOffensePanelButtonFactory RequireButtonFactory()
    {
        return buttonFactory
            ?? throw new InvalidOperationException(
                $"{nameof(OffenseExpeditionPanel)} requires {nameof(IOffensePanelButtonFactory)} binding.");
    }
}
