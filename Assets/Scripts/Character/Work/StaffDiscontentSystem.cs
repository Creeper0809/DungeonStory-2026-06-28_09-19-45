using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public enum StaffDiscontentStage
{
    Stable,
    LowSatisfaction,
    EfficiencyDrop,
    WorkDisruption,
    Departure,
    LocalRebellion
}

public enum StaffDiscontentOutcome
{
    None,
    Warning,
    EfficiencyPenalty,
    WorkDisruption,
    PermanentDeparture,
    LocalRebellion,
    OwnerThreat
}

public enum StaffRebellionResponseType
{
    AutoSuppress,
    SuppressCommand,
    Isolate,
    Calm
}

[Serializable]
public class StaffDiscontentRules
{
    public float lowMoodThreshold = 50f;
    public float efficiencyDropMoodThreshold = 35f;
    public float workDisruptionMoodThreshold = 25f;
    public float departureMoodThreshold = 15f;
    public float rebellionMoodThreshold = 8f;
    public int sustainedLowMoodForEfficiencyDrop = 2;
    public int sustainedLowMoodForWorkDisruption = 3;
    public int sustainedLowMoodForDeparture = 4;
    public int ownerThreatEscalationDays = 2;
    public float calmMoodRecovery = 25f;
    public float lowSatisfactionMultiplier = 0.95f;
    public float efficiencyDropMultiplier = 0.8f;
    public float workDisruptionMultiplier = 0.6f;

    public static StaffDiscontentRules CreateDefault()
    {
        return new StaffDiscontentRules();
    }
}

public sealed class StaffDiscontentSnapshot
{
    public int staffId;
    public string displayName;
    public StaffDiscontentStage stage;
    public StaffDiscontentOutcome outcome;
    public float mood;
    public int lowMoodDays;
    public bool permanentLoss;
    public bool departed;
    public bool localRebellion;
    public bool ownerThreat;
    public bool isolated;
    public bool suppressed;

    public string ToSummaryText()
    {
        return $"{displayName} / {stage} / 기분 {mood:0.#} / 저기분 {lowMoodDays}일";
    }
}

public sealed class StaffDiscontentRecord
{
    public StaffDiscontentRecord(int staffId, CharacterActor staff)
    {
        StaffId = staffId;
        DisplayName = StaffDiscontentService.GetStaffDisplayName(staff, staffId);
    }

    public int StaffId { get; }
    public string DisplayName { get; private set; }
    public StaffDiscontentStage Stage { get; private set; } = StaffDiscontentStage.Stable;
    public float LastMood { get; private set; } = 100f;
    public int LowMoodDays { get; private set; }
    public int LocalRebellionDays { get; private set; }
    public bool IsPermanentLoss { get; private set; }
    public bool IsDeparted { get; private set; }
    public bool IsInLocalRebellion { get; private set; }
    public bool IsOwnerThreat { get; private set; }
    public bool IsIsolated { get; private set; }
    public bool IsSuppressed { get; private set; }

    public StaffDiscontentOutcome Update(CharacterActor staff, StaffDiscontentRules rules)
    {
        rules ??= StaffDiscontentRules.CreateDefault();
        if (staff != null)
        {
            DisplayName = StaffDiscontentService.GetStaffDisplayName(staff, StaffId);
        }

        if (IsDeparted || IsSuppressed)
        {
            return StaffDiscontentOutcome.None;
        }

        LastMood = StaffDiscontentService.GetMood(staff);
        LowMoodDays = LastMood <= rules.lowMoodThreshold ? LowMoodDays + 1 : 0;
        StaffDiscontentStage previousStage = Stage;
        Stage = StaffDiscontentService.EvaluateStage(LastMood, LowMoodDays, rules);

        if (IsInLocalRebellion)
        {
            Stage = StaffDiscontentStage.LocalRebellion;
            if (IsIsolated)
            {
                return StaffDiscontentOutcome.None;
            }

            LocalRebellionDays++;
            if (!IsOwnerThreat && LocalRebellionDays >= Mathf.Max(1, rules.ownerThreatEscalationDays))
            {
                IsOwnerThreat = true;
                return StaffDiscontentOutcome.OwnerThreat;
            }

            return StaffDiscontentOutcome.None;
        }

        if (Stage == StaffDiscontentStage.LocalRebellion)
        {
            IsPermanentLoss = true;
            IsInLocalRebellion = true;
            LocalRebellionDays = 1;
            return StaffDiscontentOutcome.LocalRebellion;
        }

        if (Stage == StaffDiscontentStage.Departure)
        {
            IsPermanentLoss = true;
            IsDeparted = true;
            return StaffDiscontentOutcome.PermanentDeparture;
        }

        if (Stage == previousStage)
        {
            return StaffDiscontentOutcome.None;
        }

        return Stage switch
        {
            StaffDiscontentStage.LowSatisfaction => StaffDiscontentOutcome.Warning,
            StaffDiscontentStage.EfficiencyDrop => StaffDiscontentOutcome.EfficiencyPenalty,
            StaffDiscontentStage.WorkDisruption => StaffDiscontentOutcome.WorkDisruption,
            _ => StaffDiscontentOutcome.None
        };
    }

    public bool MarkIsolated()
    {
        if (IsDeparted || IsSuppressed || !IsInLocalRebellion)
        {
            return false;
        }

        IsIsolated = true;
        IsOwnerThreat = false;
        return true;
    }

    public bool MarkSuppressed()
    {
        if (IsDeparted || IsSuppressed || !IsInLocalRebellion)
        {
            return false;
        }

        IsSuppressed = true;
        IsInLocalRebellion = false;
        IsOwnerThreat = false;
        IsPermanentLoss = true;
        return true;
    }

    public bool TryCalm(CharacterActor staff, StaffDiscontentRules rules, out string failureReason)
    {
        rules ??= StaffDiscontentRules.CreateDefault();
        failureReason = string.Empty;

        if (IsDeparted)
        {
            failureReason = "이미 이탈했습니다";
            return false;
        }

        if (IsPermanentLoss || IsInLocalRebellion)
        {
            failureReason = "이미 영구 손실 상태입니다";
            return false;
        }

        if (Stage == StaffDiscontentStage.Stable)
        {
            failureReason = "진정이 필요하지 않습니다";
            return false;
        }

        staff?.Stats?.ApplyMoodFactor(
            "management:calmed",
            "상담으로 진정됨",
            Mathf.Max(0f, rules.calmMoodRecovery),
            240f,
            1);
        LastMood = StaffDiscontentService.GetMood(staff);
        LowMoodDays = 0;
        Stage = StaffDiscontentService.EvaluateStage(LastMood, LowMoodDays, rules);
        return true;
    }

    public StaffDiscontentSnapshot ToSnapshot(StaffDiscontentOutcome outcome = StaffDiscontentOutcome.None)
    {
        return new StaffDiscontentSnapshot
        {
            staffId = StaffId,
            displayName = DisplayName,
            stage = Stage,
            outcome = outcome,
            mood = LastMood,
            lowMoodDays = LowMoodDays,
            permanentLoss = IsPermanentLoss,
            departed = IsDeparted,
            localRebellion = IsInLocalRebellion,
            ownerThreat = IsOwnerThreat,
            isolated = IsIsolated,
            suppressed = IsSuppressed
        };
    }
}

public readonly struct StaffRebellionResponseResult
{
    public StaffRebellionResponseResult(
        bool success,
        StaffRebellionResponseType responseType,
        StaffDiscontentSnapshot snapshot,
        CharacterActor actor,
        string message)
    {
        Success = success;
        ResponseType = responseType;
        Snapshot = snapshot;
        Actor = actor;
        Message = message ?? string.Empty;
    }

    public bool Success { get; }
    public StaffRebellionResponseType ResponseType { get; }
    public StaffDiscontentSnapshot Snapshot { get; }
    public CharacterActor Actor { get; }
    public string Message { get; }
}

public sealed class StaffDiscontentState
{
    private readonly Dictionary<int, StaffDiscontentRecord> records = new Dictionary<int, StaffDiscontentRecord>();

    public IReadOnlyCollection<StaffDiscontentRecord> Records => records.Values;

    public StaffDiscontentRecord ProcessStaff(CharacterActor staff, StaffDiscontentRules rules, out StaffDiscontentOutcome outcome)
    {
        outcome = StaffDiscontentOutcome.None;
        if (!StaffDiscontentService.IsTrackableStaff(staff))
        {
            return null;
        }

        int staffId = StaffDiscontentService.GetStaffId(staff);
        StaffDiscontentRecord record = GetOrCreate(staffId, staff);
        outcome = record.Update(staff, rules);
        return record;
    }

    public bool TryGetRecord(CharacterActor staff, out StaffDiscontentRecord record)
    {
        record = null;
        if (!StaffDiscontentService.IsTrackableStaff(staff))
        {
            return false;
        }

        return records.TryGetValue(StaffDiscontentService.GetStaffId(staff), out record);
    }

    public bool IsPermanentLoss(CharacterActor staff)
    {
        return TryGetRecord(staff, out StaffDiscontentRecord record) && record.IsPermanentLoss;
    }

    private StaffDiscontentRecord GetOrCreate(int staffId, CharacterActor staff)
    {
        if (!records.TryGetValue(staffId, out StaffDiscontentRecord record))
        {
            record = new StaffDiscontentRecord(staffId, staff);
            records[staffId] = record;
        }

        return record;
    }
}

public struct StaffDiscontentChangedEvent
{
    public StaffDiscontentSnapshot snapshot;

    public StaffDiscontentChangedEvent(StaffDiscontentSnapshot snapshot)
    {
        this.snapshot = snapshot;
    }

    private static StaffDiscontentChangedEvent e;

    public static void Trigger(StaffDiscontentSnapshot snapshot)
    {
        e.snapshot = snapshot;
        EventObserver.TriggerEvent(e);
    }
}

public struct StaffPermanentLossEvent
{
    public StaffDiscontentSnapshot snapshot;

    public StaffPermanentLossEvent(StaffDiscontentSnapshot snapshot)
    {
        this.snapshot = snapshot;
    }

    private static StaffPermanentLossEvent e;

    public static void Trigger(StaffDiscontentSnapshot snapshot)
    {
        e.snapshot = snapshot;
        EventObserver.TriggerEvent(e);
    }
}

public struct StaffRebellionResponseEvent
{
    public StaffRebellionResponseResult result;

    public StaffRebellionResponseEvent(StaffRebellionResponseResult result)
    {
        this.result = result;
    }

    private static StaffRebellionResponseEvent e;

    public static void Trigger(StaffRebellionResponseResult result)
    {
        e.result = result;
        EventObserver.TriggerEvent(e);
    }
}

public static class StaffDiscontentService
{
    public static bool IsTrackableStaff(CharacterActor staff)
    {
        CharacterIdentity identity = staff != null ? staff.Identity : null;
        return staff != null
            && identity != null
            && !identity.IsOwner
            && identity.CharacterType == CharacterType.NPC
            && staff.TryGetAbility(out AbilityWork _);
    }

    public static int GetStaffId(CharacterActor staff)
    {
        if (staff == null)
        {
            return -1;
        }

        CharacterIdentity identity = staff.Identity;
        return identity != null ? identity.StableId : staff.GetInstanceID();
    }

    public static string GetStaffDisplayName(CharacterActor staff, int staffId)
    {
        CharacterIdentity identity = staff != null ? staff.Identity : null;
        if (!string.IsNullOrWhiteSpace(identity != null ? identity.DisplayName : null))
        {
            return identity.DisplayName;
        }

        if (!string.IsNullOrWhiteSpace(staff != null ? staff.name : null))
        {
            return staff.name;
        }

        return $"Staff {staffId}";
    }

    public static float GetMood(CharacterActor staff)
    {
        CharacterStats stats = staff != null ? staff.Stats : null;
        if (stats == null)
        {
            return 100f;
        }

        return stats.Stats.TryGetValue(CharacterCondition.MOOD, out float mood)
            ? Mathf.Clamp(mood, 0f, 100f)
            : 100f;
    }

    public static StaffDiscontentStage EvaluateStage(float mood, int lowMoodDays, StaffDiscontentRules rules)
    {
        rules ??= StaffDiscontentRules.CreateDefault();
        mood = Mathf.Clamp(mood, 0f, 100f);

        if (mood <= rules.rebellionMoodThreshold)
        {
            return StaffDiscontentStage.LocalRebellion;
        }

        if (mood <= rules.departureMoodThreshold
            || lowMoodDays >= Mathf.Max(1, rules.sustainedLowMoodForDeparture))
        {
            return StaffDiscontentStage.Departure;
        }

        if (mood <= rules.workDisruptionMoodThreshold
            || lowMoodDays >= Mathf.Max(1, rules.sustainedLowMoodForWorkDisruption))
        {
            return StaffDiscontentStage.WorkDisruption;
        }

        if (mood <= rules.efficiencyDropMoodThreshold
            || lowMoodDays >= Mathf.Max(1, rules.sustainedLowMoodForEfficiencyDrop))
        {
            return StaffDiscontentStage.EfficiencyDrop;
        }

        if (mood <= rules.lowMoodThreshold)
        {
            return StaffDiscontentStage.LowSatisfaction;
        }

        return StaffDiscontentStage.Stable;
    }

    public static float GetWorkEfficiencyMultiplier(StaffDiscontentStage stage, StaffDiscontentRules rules)
    {
        rules ??= StaffDiscontentRules.CreateDefault();
        return stage switch
        {
            StaffDiscontentStage.LowSatisfaction => Mathf.Clamp(rules.lowSatisfactionMultiplier, 0.1f, 1f),
            StaffDiscontentStage.EfficiencyDrop => Mathf.Clamp(rules.efficiencyDropMultiplier, 0.1f, 1f),
            StaffDiscontentStage.WorkDisruption => Mathf.Clamp(rules.workDisruptionMultiplier, 0.05f, 1f),
            StaffDiscontentStage.Departure => 0f,
            StaffDiscontentStage.LocalRebellion => 0f,
            _ => 1f
        };
    }

    public static bool ShouldBlockWork(StaffDiscontentStage stage)
    {
        return stage == StaffDiscontentStage.WorkDisruption
            || stage == StaffDiscontentStage.Departure
            || stage == StaffDiscontentStage.LocalRebellion;
    }

    public static string GetBlockReason(StaffDiscontentStage stage)
    {
        return stage switch
        {
            StaffDiscontentStage.WorkDisruption => "태업/결근",
            StaffDiscontentStage.Departure => "이탈",
            StaffDiscontentStage.LocalRebellion => "반란",
            _ => string.Empty
        };
    }
}

public class StaffDiscontentRuntime : MonoBehaviour, UtilEventListener<OperatingDayEndedEvent>
{
    [SerializeField] private StaffDiscontentRules rules = StaffDiscontentRules.CreateDefault();

    private readonly StaffDiscontentState state = new StaffDiscontentState();
    private IDungeonSceneComponentQuery sceneQuery;

    public StaffDiscontentState State => state;
    public StaffDiscontentRules Rules => rules;

    [Inject]
    public void Construct(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery
            ?? throw new ArgumentNullException(nameof(sceneQuery));
    }

    public void OnTriggerEvent(OperatingDayEndedEvent eventType)
    {
        ProcessAllStaff();
    }

    public StaffDiscontentRecord ProcessStaff(CharacterActor staff, out StaffDiscontentOutcome outcome)
    {
        StaffDiscontentRecord record = state.ProcessStaff(staff, rules, out outcome);
        if (record == null)
        {
            return null;
        }

        ApplyOutcome(staff, record, outcome);
        if (record.IsInLocalRebellion && !record.IsIsolated && !record.IsSuppressed)
        {
            DispatchAutoSuppress(staff);
        }
        return record;
    }

    public void ProcessAllStaff()
    {
        IReadOnlyList<CharacterActor> actors = RequireSceneQuery().All<CharacterActor>();
        foreach (CharacterActor staff in actors)
        {
            ProcessStaff(staff, out _);
        }
    }

    public float GetWorkEfficiencyMultiplier(CharacterActor staff)
    {
        if (!StaffDiscontentService.IsTrackableStaff(staff))
        {
            return 1f;
        }

        StaffDiscontentStage stage = state.TryGetRecord(staff, out StaffDiscontentRecord record)
            ? record.Stage
            : StaffDiscontentService.EvaluateStage(StaffDiscontentService.GetMood(staff), 0, rules);
        return StaffDiscontentService.GetWorkEfficiencyMultiplier(stage, rules);
    }

    public bool ShouldBlockWork(CharacterActor staff, out string reason)
    {
        reason = string.Empty;
        if (!StaffDiscontentService.IsTrackableStaff(staff))
        {
            return false;
        }

        StaffDiscontentStage stage = state.TryGetRecord(staff, out StaffDiscontentRecord record)
            ? record.Stage
            : StaffDiscontentService.EvaluateStage(StaffDiscontentService.GetMood(staff), 0, rules);
        if (!StaffDiscontentService.ShouldBlockWork(stage))
        {
            return false;
        }

        reason = StaffDiscontentService.GetBlockReason(stage);
        return true;
    }

    public bool IsRebellionTarget(CharacterActor target)
    {
        return state.TryGetRecord(target, out StaffDiscontentRecord record)
            && record.IsInLocalRebellion
            && !record.IsDeparted
            && !record.IsSuppressed;
    }

    public int DispatchAutoSuppress(CharacterActor rebel)
    {
        if (!IsRebellionTarget(rebel))
        {
            return 0;
        }

        IReadOnlyList<CharacterActor> characters = RequireSceneQuery().All<CharacterActor>();
        int assignedCount = 0;
        foreach (CharacterActor candidate in characters)
        {
            if (candidate == null
                || candidate == rebel
                || (candidate.Stats != null && candidate.Stats.IsDead)
                || !candidate.TryGetAbility(out AbilityWork work)
                || work.HasPrioritySuppressTarget
                || !work.WorkPriorities.IsEnabled(FacilityWorkType.Guard))
            {
                continue;
            }

            if (!WorkCommandResolver.TryResolveSuppressCommand(candidate, rebel, IsRebellionTarget, out _))
            {
                continue;
            }

            GridPathSearchResult searchResult = candidate.Brain != null
                ? candidate.Brain.GetPathSearch(candidate)
                : null;
            if (!work.TrySetPrioritySuppressTarget(rebel, searchResult, out _))
            {
                continue;
            }

            assignedCount++;
        }

        if (assignedCount > 0 && state.TryGetRecord(rebel, out StaffDiscontentRecord record))
        {
            StaffRebellionResponseResult result = new StaffRebellionResponseResult(
                true,
                StaffRebellionResponseType.AutoSuppress,
                record.ToSnapshot(),
                null,
                $"자동 제압 배정: {assignedCount}명");
            StaffRebellionResponseEvent.Trigger(result);
            EventAlertService.RaiseStaffComplaint(
                $"{record.DisplayName}: 자동 제압 {assignedCount}명 배정",
                EventAlertImportance.Medium);
        }

        return assignedCount;
    }

    public bool TryIsolateRebel(CharacterActor rebel, CharacterActor actor, out StaffRebellionResponseResult result)
    {
        if (!state.TryGetRecord(rebel, out StaffDiscontentRecord record)
            || !record.IsInLocalRebellion)
        {
            result = new StaffRebellionResponseResult(false, StaffRebellionResponseType.Isolate, null, actor, "격리할 반란 대상이 없습니다");
            return false;
        }

        if (!record.MarkIsolated())
        {
            result = new StaffRebellionResponseResult(false, StaffRebellionResponseType.Isolate, record.ToSnapshot(), actor, "격리할 수 없습니다");
            return false;
        }

        rebel?.AddLog("반란 대응: 격리");
        result = new StaffRebellionResponseResult(true, StaffRebellionResponseType.Isolate, record.ToSnapshot(), actor, "격리 완료");
        StaffRebellionResponseEvent.Trigger(result);
        EventAlertService.RaiseStaffComplaint($"{record.DisplayName}: 격리", EventAlertImportance.Medium);
        return true;
    }

    public bool TryCalmStaff(CharacterActor staff, CharacterActor actor, out StaffRebellionResponseResult result)
    {
        if (!state.TryGetRecord(staff, out StaffDiscontentRecord record))
        {
            result = new StaffRebellionResponseResult(false, StaffRebellionResponseType.Calm, null, actor, "진정할 직원 기록이 없습니다");
            return false;
        }

        if (!record.TryCalm(staff, rules, out string failureReason))
        {
            result = new StaffRebellionResponseResult(false, StaffRebellionResponseType.Calm, record.ToSnapshot(), actor, failureReason);
            return false;
        }

        staff?.AddLog("반란 대응: 진정");
        result = new StaffRebellionResponseResult(true, StaffRebellionResponseType.Calm, record.ToSnapshot(), actor, "진정 완료");
        StaffRebellionResponseEvent.Trigger(result);
        EventAlertService.RaiseStaffComplaint($"{record.DisplayName}: 진정", EventAlertImportance.Low);
        return true;
    }

    public bool ResolveSuppressedRebel(CharacterActor rebel, CharacterActor defender)
    {
        if (!state.TryGetRecord(rebel, out StaffDiscontentRecord record))
        {
            return false;
        }

        if (!record.MarkSuppressed())
        {
            return false;
        }

        StaffRebellionResponseResult result = new StaffRebellionResponseResult(
            true,
            StaffRebellionResponseType.SuppressCommand,
            record.ToSnapshot(),
            defender,
            "제압 완료");
        StaffRebellionResponseEvent.Trigger(result);
        EventAlertService.RaiseStaffComplaint($"{record.DisplayName}: 제압 완료", EventAlertImportance.Medium);
        return true;
    }

    private void ApplyOutcome(CharacterActor staff, StaffDiscontentRecord record, StaffDiscontentOutcome outcome)
    {
        if (outcome == StaffDiscontentOutcome.None)
        {
            return;
        }

        StaffDiscontentSnapshot snapshot = record.ToSnapshot(outcome);
        StaffDiscontentChangedEvent.Trigger(snapshot);

        switch (outcome)
        {
            case StaffDiscontentOutcome.Warning:
                staff?.AddLog("직원 불만: 만족도 낮음");
                EventAlertService.RaiseStaffComplaint($"{snapshot.displayName}: 만족도 낮음", EventAlertImportance.Low);
                break;
            case StaffDiscontentOutcome.EfficiencyPenalty:
                staff?.AddLog("직원 불만: 효율 저하");
                EventAlertService.RaiseStaffComplaint($"{snapshot.displayName}: 효율 저하", EventAlertImportance.Medium);
                break;
            case StaffDiscontentOutcome.WorkDisruption:
                staff?.AddLog("직원 불만: 태업/결근");
                EventAlertService.RaiseStaffComplaint($"{snapshot.displayName}: 태업/결근", EventAlertImportance.Medium);
                break;
            case StaffDiscontentOutcome.PermanentDeparture:
                staff?.AddLog("직원 이탈: 영구 손실");
                staff?.Lifecycle?.SetLifecycleState(CharacterLifecycleState.Despawned);
                StaffPermanentLossEvent.Trigger(snapshot);
                EventAlertService.RaiseStaffComplaint($"{snapshot.displayName}: 이탈", EventAlertImportance.High);
                break;
            case StaffDiscontentOutcome.LocalRebellion:
                staff?.AddLog("국지 반란: 주변 피해 시작");
                StaffPermanentLossEvent.Trigger(snapshot);
                EventAlertService.RaiseStaffComplaint($"{snapshot.displayName}: 국지 반란", EventAlertImportance.High);
                DispatchAutoSuppress(staff);
                break;
            case StaffDiscontentOutcome.OwnerThreat:
                staff?.AddLog("반란 확산: 사장 위협");
                EventAlertService.RaiseStaffComplaint($"{snapshot.displayName}: 반란 확산", EventAlertImportance.High);
                break;
        }
    }

    private IDungeonSceneComponentQuery RequireSceneQuery()
    {
        if (sceneQuery == null)
        {
            throw new InvalidOperationException($"{nameof(StaffDiscontentRuntime)} requires {nameof(IDungeonSceneComponentQuery)} injection.");
        }

        return sceneQuery;
    }

    private void OnEnable()
    {
        this.EventStartListening<OperatingDayEndedEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<OperatingDayEndedEvent>();
    }
}
