using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class OffenseExpeditionMemberSnapshot
{
    public string name;
    public string speciesTag;
    public float power;
    public bool survived;
    public float damageTaken;

    public string ToSummaryText()
    {
        string state = survived ? "복귀" : "사망";
        string species = string.IsNullOrWhiteSpace(speciesTag) ? "미상" : speciesTag;
        return $"{name} / {species} / 전투력 {power:0.#} / 피해 {damageTaken:0.#} / {state}";
    }
}

public sealed class OffenseExpeditionResult
{
    public string expeditionId;
    public string targetId;
    public string targetTitle;
    public bool success;
    public float totalPower;
    public float requiredPower;
    public float danger;
    public float elapsedSeconds;
    public OffenseExpeditionMemberSnapshot[] members = Array.Empty<OffenseExpeditionMemberSnapshot>();
    public string[] rewardSummaries = Array.Empty<string>();
    public OffenseRewardGrantResult[] grantedRewards = Array.Empty<OffenseRewardGrantResult>();

    public string ToDetailText()
    {
        List<string> lines = new List<string>
        {
            success ? "원정 성공" : "원정 실패",
            $"대상: {targetTitle}",
            $"전투력: {totalPower:0.#} / 권장 {requiredPower:0.#}",
            $"위험도: {danger:0.#}",
            $"소요 시간: {elapsedSeconds:0.#}초"
        };

        if (members != null && members.Length > 0)
        {
            lines.Add(string.Empty);
            lines.Add("원정대:");
            foreach (OffenseExpeditionMemberSnapshot member in members)
            {
                if (member != null)
                {
                    lines.Add($"- {member.ToSummaryText()}");
                }
            }
        }

        if (success && grantedRewards != null && grantedRewards.Length > 0)
        {
            lines.Add(string.Empty);
            lines.Add("지급 결과:");
            foreach (OffenseRewardGrantResult reward in grantedRewards)
            {
                if (reward != null)
                {
                    lines.Add($"- {reward.ToSummaryText()}");
                }
            }
        }
        else if (success && rewardSummaries != null && rewardSummaries.Length > 0)
        {
            lines.Add(string.Empty);
            lines.Add("획득 보상:");
            foreach (string reward in rewardSummaries)
            {
                if (!string.IsNullOrWhiteSpace(reward))
                {
                    lines.Add($"- {reward}");
                }
            }
        }

        return string.Join("\n", lines);
    }
}

public sealed class OffenseExpeditionRun
{
    private readonly List<Character> members;

    public OffenseExpeditionRun(
        string expeditionId,
        OffenseTargetDefinition target,
        IEnumerable<Character> members,
        float totalPower)
    {
        ExpeditionId = string.IsNullOrWhiteSpace(expeditionId)
            ? Guid.NewGuid().ToString("N")
            : expeditionId;
        Target = target;
        this.members = members?.Where((member) => member != null).Distinct().ToList()
            ?? new List<Character>();
        TotalPower = Mathf.Max(0f, totalPower);
        RemainingSeconds = target != null ? Mathf.Max(1f, target.durationSeconds) : 1f;
        TotalDurationSeconds = RemainingSeconds;
    }

    public string ExpeditionId { get; }
    public OffenseTargetDefinition Target { get; }
    public IReadOnlyList<Character> Members => members;
    public float TotalPower { get; }
    public float RemainingSeconds { get; private set; }
    public float TotalDurationSeconds { get; }
    public bool IsComplete => RemainingSeconds <= 0f;

    public void Tick(float deltaTime)
    {
        RemainingSeconds = Mathf.Max(0f, RemainingSeconds - Mathf.Max(0f, deltaTime));
    }
}

public struct OffenseExpeditionStartedEvent
{
    public OffenseExpeditionRun expedition;

    public OffenseExpeditionStartedEvent(OffenseExpeditionRun expedition)
    {
        this.expedition = expedition;
    }

    private static OffenseExpeditionStartedEvent e;

    public static void Trigger(OffenseExpeditionRun expedition)
    {
        e.expedition = expedition;
        EventObserver.TriggerEvent(e);
    }
}

public struct OffenseExpeditionCompletedEvent
{
    public OffenseExpeditionResult result;

    public OffenseExpeditionCompletedEvent(OffenseExpeditionResult result)
    {
        this.result = result;
    }

    private static OffenseExpeditionCompletedEvent e;

    public static void Trigger(OffenseExpeditionResult result)
    {
        e.result = result;
        EventObserver.TriggerEvent(e);
    }
}

public static class OffenseExpeditionService
{
    public static bool CanJoinExpedition(Character character, out string reason)
    {
        if (character == null)
        {
            reason = "캐릭터 없음";
            return false;
        }

        if (character.IsOwner)
        {
            reason = "사장은 원정에 보낼 수 없습니다";
            return false;
        }

        if (character.IsDead)
        {
            reason = "사망한 캐릭터입니다";
            return false;
        }

        if (character.IsOnExpedition)
        {
            reason = "이미 원정 중입니다";
            return false;
        }

        if (character.CurrentLifecycleState != Character.LifecycleState.Active)
        {
            reason = "현재 던전에서 활동 중인 캐릭터가 아닙니다";
            return false;
        }

        if (character.characterType != CharacterType.NPC)
        {
            reason = "직원이나 방어 몬스터만 원정에 보낼 수 있습니다";
            return false;
        }

        if (!character.TryGetAbility(out AbilityWork _))
        {
            reason = "원정 가능한 작업/전투 능력이 없습니다";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public static float CalculateMemberPower(Character character)
    {
        if (character == null || character.IsDead)
        {
            return 0f;
        }

        float basePower =
            character.GetCharacterStat(CharacterStatType.Attack) * 1.4f
            + character.GetCharacterStat(CharacterStatType.Strength) * 0.8f
            + character.GetCharacterStat(CharacterStatType.Toughness) * 0.6f
            + character.GetCharacterStat(CharacterStatType.Endurance) * 0.4f
            + character.GetCharacterStat(CharacterStatType.MoveSpeed) * 0.25f;
        return Mathf.Max(0f, basePower * character.GetCombatPowerMultiplier());
    }

    public static float CalculatePartyPower(IEnumerable<Character> members)
    {
        return members?.Where((member) => member != null).Sum(CalculateMemberPower) ?? 0f;
    }

    public static bool ShouldSucceed(OffenseExpeditionRun expedition)
    {
        if (expedition == null || expedition.Target == null)
        {
            return false;
        }

        float requiredPower = Mathf.Max(1f, expedition.Target.requiredPower);
        return expedition.TotalPower >= requiredPower;
    }

    public static OffenseExpeditionResult Resolve(
        OffenseExpeditionRun expedition,
        bool? forceSuccess = null)
    {
        if (expedition == null || expedition.Target == null)
        {
            return null;
        }

        bool success = forceSuccess ?? ShouldSucceed(expedition);
        float requiredPower = Mathf.Max(1f, expedition.Target.requiredPower);
        float powerRatio = expedition.TotalPower / requiredPower;
        float danger = Mathf.Max(0f, expedition.Target.danger);
        int memberCount = Mathf.Max(1, expedition.Members.Count);
        float damageMultiplier = success ? 0.16f : Mathf.Lerp(0.75f, 1.35f, Mathf.Clamp01(1f - powerRatio));
        float damagePerMember = danger * damageMultiplier / memberCount;
        List<OffenseExpeditionMemberSnapshot> memberSnapshots = new List<OffenseExpeditionMemberSnapshot>();

        foreach (Character member in expedition.Members)
        {
            if (member == null)
            {
                continue;
            }

            float memberPower = CalculateMemberPower(member);
            member.EndExpedition(alive: true);
            member.ChangesStat(Character.Condition.SLEEP, success ? -12f : -24f);
            member.ChangesStat(Character.Condition.MOOD, success ? -4f : -12f);
            if (damagePerMember > 0f)
            {
                member.ApplyDamage(damagePerMember, "원정 피해");
            }

            memberSnapshots.Add(new OffenseExpeditionMemberSnapshot
            {
                name = member.data != null ? member.data.characterName : member.name,
                speciesTag = member.SpeciesTag,
                power = memberPower,
                survived = !member.IsDead,
                damageTaken = damagePerMember
            });
        }

        string[] rewards = success
            ? expedition.Target.rewards?
                .Where((reward) => reward != null)
                .Select((reward) => reward.ToSummaryText())
                .ToArray() ?? Array.Empty<string>()
            : Array.Empty<string>();

        return new OffenseExpeditionResult
        {
            expeditionId = expedition.ExpeditionId,
            targetId = expedition.Target.id,
            targetTitle = expedition.Target.title,
            success = success,
            totalPower = expedition.TotalPower,
            requiredPower = requiredPower,
            danger = danger,
            elapsedSeconds = expedition.TotalDurationSeconds,
            members = memberSnapshots.ToArray(),
            rewardSummaries = rewards
        };
    }
}

public class OffenseExpeditionRuntime : MonoBehaviour
{
    private readonly List<OffenseExpeditionRun> activeExpeditions = new List<OffenseExpeditionRun>();
    private static OffenseExpeditionRuntime instance;

    public static OffenseExpeditionRuntime Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<OffenseExpeditionRuntime>();
            }

            return instance;
        }
    }

    public IReadOnlyList<OffenseExpeditionRun> ActiveExpeditions => activeExpeditions;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    public IReadOnlyList<Character> GetAvailableMembers()
    {
        return FindObjectsByType<Character>(FindObjectsSortMode.None)
            .Where((character) => OffenseExpeditionService.CanJoinExpedition(character, out _))
            .OrderByDescending(OffenseExpeditionService.CalculateMemberPower)
            .ToList();
    }

    public bool TryStartExpedition(
        string targetId,
        IEnumerable<Character> members,
        out OffenseExpeditionRun expedition,
        out string message)
    {
        expedition = null;
        OffenseWorldMapRuntime worldMap = OffenseWorldMapRuntime.Instance;
        if (worldMap == null)
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

        List<Character> party = members?
            .Where((member) => member != null)
            .Distinct()
            .ToList()
            ?? new List<Character>();
        if (party.Count < target.requiredMembers)
        {
            message = $"필요 인력 부족: {party.Count}/{target.requiredMembers}";
            return false;
        }

        foreach (Character member in party)
        {
            if (!OffenseExpeditionService.CanJoinExpedition(member, out string reason))
            {
                message = $"{member.name}: {reason}";
                return false;
            }
        }

        float totalPower = OffenseExpeditionService.CalculatePartyPower(party);
        expedition = new OffenseExpeditionRun(Guid.NewGuid().ToString("N"), target, party, totalPower);
        foreach (Character member in party)
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

        if (result.success && MetaProgressionRuntime.Instance != null)
        {
            MetaProgressionRuntime.Instance.RecordOffenseSuccess();
        }

        if (result.success && OffenseRewardRuntime.Instance != null)
        {
            IReadOnlyList<OffenseRewardGrantResult> grantedRewards =
                OffenseRewardRuntime.Instance.ApplyExpeditionRewards(expedition, result);
            result.grantedRewards = grantedRewards?.ToArray() ?? Array.Empty<OffenseRewardGrantResult>();
            if (result.grantedRewards.Length > 0)
            {
                result.rewardSummaries = result.grantedRewards
                    .Where((reward) => reward != null)
                    .Select((reward) => reward.ToSummaryText())
                    .ToArray();
            }
        }

        OffenseExpeditionCompletedEvent.Trigger(result);
        EventAlertService.Raise("원정 결과", result.ToDetailText(), result.success ? EventAlertImportance.Medium : EventAlertImportance.High, "오펜스");
        return result;
    }

    public OffenseExpeditionPanel ShowExpeditionPanel()
    {
        return OffenseExpeditionPanel.Show(this);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}

public class OffenseExpeditionPanel : MonoBehaviour
{
    private OffenseExpeditionRuntime runtime;
    private TMP_Text headerText;
    private TMP_Text detailText;
    private RectTransform memberButtonRoot;
    private readonly List<GameObject> spawnedButtons = new List<GameObject>();
    private readonly HashSet<Character> selectedMembers = new HashSet<Character>();
    private string statusMessage;

    public static OffenseExpeditionPanel Show(OffenseExpeditionRuntime runtime)
    {
        OffenseExpeditionPanel panel = FindFirstObjectByType<OffenseExpeditionPanel>();
        if (panel == null)
        {
            panel = CreateDefaultPanel();
        }

        panel.Bind(runtime);
        return panel;
    }

    public void Bind(OffenseExpeditionRuntime source)
    {
        runtime = source != null ? source : OffenseExpeditionRuntime.Instance;
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
        OffenseWorldMapRuntime worldMap = OffenseWorldMapRuntime.Instance;
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
        foreach (Character member in runtime.GetAvailableMembers())
        {
            Character captured = member;
            string label = BuildMemberLabel(captured);
            GameObject buttonObject = CreateButton(
                memberButtonRoot,
                selectedMembers.Contains(captured) ? $"[선택] {label}" : label,
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

        GameObject startButton = CreateButton(
            memberButtonRoot,
            "원정 출발",
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

    private static string BuildMemberLabel(Character member)
    {
        if (member == null)
        {
            return "알 수 없음";
        }

        string name = member.data != null ? member.data.characterName : member.name;
        return $"{name} / {member.SpeciesTag} / 전투력 {OffenseExpeditionService.CalculateMemberPower(member):0.#}";
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
                Destroy(button);
            }
        }

        spawnedButtons.Clear();
    }

    private static OffenseExpeditionPanel CreateDefaultPanel()
    {
        GameObject canvasObject = new GameObject("OffenseExpeditionCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 430;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);

        GameObject panelObject = new GameObject("OffenseExpeditionPanel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.16f, 0.14f);
        panelRect.anchorMax = new Vector2(0.84f, 0.84f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panelObject.GetComponent<Image>().color = new Color(0.04f, 0.045f, 0.052f, 0.94f);

        GameObject header = CreateText(panelObject.transform, "OffenseExpeditionHeader", 24f, TextAlignmentOptions.Left);
        RectTransform headerRect = header.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.offsetMin = new Vector2(24f, -64f);
        headerRect.offsetMax = new Vector2(-24f, -18f);

        GameObject memberRootObject = new GameObject("OffenseExpeditionMembers", typeof(RectTransform), typeof(VerticalLayoutGroup));
        memberRootObject.transform.SetParent(panelObject.transform, false);
        RectTransform memberRootRect = memberRootObject.GetComponent<RectTransform>();
        memberRootRect.anchorMin = new Vector2(0f, 0f);
        memberRootRect.anchorMax = new Vector2(0.42f, 0.86f);
        memberRootRect.offsetMin = new Vector2(24f, 24f);
        memberRootRect.offsetMax = new Vector2(-12f, -24f);
        VerticalLayoutGroup layout = memberRootObject.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandHeight = false;

        GameObject detail = CreateText(panelObject.transform, "OffenseExpeditionDetail", 19f, TextAlignmentOptions.TopLeft);
        RectTransform detailRect = detail.GetComponent<RectTransform>();
        detailRect.anchorMin = new Vector2(0.44f, 0f);
        detailRect.anchorMax = new Vector2(1f, 0.86f);
        detailRect.offsetMin = new Vector2(12f, 24f);
        detailRect.offsetMax = new Vector2(-24f, -24f);

        OffenseExpeditionPanel panel = panelObject.AddComponent<OffenseExpeditionPanel>();
        panel.headerText = header.GetComponent<TMP_Text>();
        panel.detailText = detail.GetComponent<TMP_Text>();
        panel.memberButtonRoot = memberRootObject.GetComponent<RectTransform>();
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

        GameObject textObject = CreateText(buttonObject.transform, "Label", 16f, TextAlignmentOptions.Center);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 4f);
        textRect.offsetMax = new Vector2(-8f, -4f);
        textObject.GetComponent<TMP_Text>().text = label;
        return buttonObject;
    }
}
