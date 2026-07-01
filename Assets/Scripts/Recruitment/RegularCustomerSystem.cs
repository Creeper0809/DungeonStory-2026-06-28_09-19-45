using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum RegularCustomerStatus
{
    Visitor,
    Regular,
    RecruitCandidate,
    Recruited
}

[Flags]
public enum RecruitCapability
{
    None = 0,
    Staff = 1 << 0,
    Defense = 1 << 1,
    Expedition = 1 << 2,
    All = Staff | Defense | Expedition
}

[Serializable]
public class RegularCustomerRules
{
    public int regularVisitThreshold = 3;
    public float regularAverageSatisfactionThreshold = 65f;
    public int recruitCandidateVisitThreshold = 4;
    public float recruitCandidateAverageSatisfactionThreshold = 75f;
    public RecruitCapability defaultRecruitCapabilities = RecruitCapability.All;

    public static RegularCustomerRules CreateDefault()
    {
        return new RegularCustomerRules();
    }
}

public sealed class RegularCustomerSnapshot
{
    public int customerId;
    public string displayName;
    public string speciesTag;
    public int visitCount;
    public float averageSatisfaction;
    public RegularCustomerStatus status;
    public RecruitCapability recruitCapabilities;

    public string ToSummaryText()
    {
        return $"{displayName} / {speciesTag} / 방문 {visitCount}회 / 만족도 {averageSatisfaction:0.#} / {status}";
    }
}

public sealed class RegularCustomerRecord
{
    private float satisfactionTotal;

    public RegularCustomerRecord(int customerId, Character customer, RecruitCapability recruitCapabilities)
    {
        CustomerId = customerId;
        DisplayName = RegularCustomerService.GetCustomerDisplayName(customer, customerId);
        SpeciesTag = RegularCustomerService.GetCustomerSpeciesTag(customer);
        SourceData = customer != null ? customer.data : null;
        RecruitCapabilities = recruitCapabilities == RecruitCapability.None
            ? RecruitCapability.All
            : recruitCapabilities;
    }

    public int CustomerId { get; }
    public string DisplayName { get; private set; }
    public string SpeciesTag { get; private set; }
    public CharacterSO SourceData { get; private set; }
    public int VisitCount { get; private set; }
    public float AverageSatisfaction => VisitCount > 0 ? satisfactionTotal / VisitCount : 0f;
    public bool IsRegular { get; private set; }
    public bool IsRecruitCandidate { get; private set; }
    public bool IsRecruited { get; private set; }
    public RecruitCapability RecruitCapabilities { get; }
    public RegularCustomerStatus Status
    {
        get
        {
            if (IsRecruited) return RegularCustomerStatus.Recruited;
            if (IsRecruitCandidate) return RegularCustomerStatus.RecruitCandidate;
            if (IsRegular) return RegularCustomerStatus.Regular;
            return RegularCustomerStatus.Visitor;
        }
    }

    public void RecordVisit(Character customer, float satisfaction, RegularCustomerRules rules)
    {
        if (customer != null)
        {
            DisplayName = RegularCustomerService.GetCustomerDisplayName(customer, CustomerId);
            SpeciesTag = RegularCustomerService.GetCustomerSpeciesTag(customer);
            SourceData = customer.data != null ? customer.data : SourceData;
        }

        VisitCount++;
        satisfactionTotal += Mathf.Clamp(satisfaction, 0f, 100f);

        if (!IsRegular && RegularCustomerService.MeetsRegularCondition(this, rules))
        {
            IsRegular = true;
        }

        if (!IsRecruitCandidate && RegularCustomerService.MeetsRecruitCandidateCondition(this, rules))
        {
            IsRegular = true;
            IsRecruitCandidate = true;
        }
    }

    public bool MarkRecruited()
    {
        if (IsRecruited || !IsRecruitCandidate)
        {
            return false;
        }

        IsRecruited = true;
        IsRegular = true;
        return true;
    }

    public RegularCustomerSnapshot ToSnapshot()
    {
        return new RegularCustomerSnapshot
        {
            customerId = CustomerId,
            displayName = DisplayName,
            speciesTag = SpeciesTag,
            visitCount = VisitCount,
            averageSatisfaction = AverageSatisfaction,
            status = Status,
            recruitCapabilities = RecruitCapabilities
        };
    }
}

public readonly struct RegularCustomerVisitResult
{
    public RegularCustomerVisitResult(
        bool success,
        RegularCustomerRecord record,
        bool becameRegular,
        bool becameRecruitCandidate,
        string message)
    {
        Success = success;
        Record = record;
        BecameRegular = becameRegular;
        BecameRecruitCandidate = becameRecruitCandidate;
        Message = message ?? string.Empty;
    }

    public bool Success { get; }
    public RegularCustomerRecord Record { get; }
    public bool BecameRegular { get; }
    public bool BecameRecruitCandidate { get; }
    public string Message { get; }
}

public readonly struct RegularCustomerRecruitResult
{
    public RegularCustomerRecruitResult(bool success, RegularCustomerRecord record, string message)
    {
        Success = success;
        Record = record;
        Message = message ?? string.Empty;
    }

    public bool Success { get; }
    public RegularCustomerRecord Record { get; }
    public string Message { get; }
    public CharacterSO SourceData => Record != null ? Record.SourceData : null;
    public CharacterType ResultType => CharacterType.NPC;
    public RecruitCapability Capabilities => Record != null ? Record.RecruitCapabilities : RecruitCapability.None;
}

public sealed class RegularCustomerState
{
    private readonly Dictionary<int, RegularCustomerRecord> records = new Dictionary<int, RegularCustomerRecord>();
    private readonly List<RegularCustomerRecruitResult> recruitedCharacters = new List<RegularCustomerRecruitResult>();

    public IReadOnlyCollection<RegularCustomerRecord> Records => records.Values;
    public IReadOnlyList<RegularCustomerRecruitResult> RecruitedCharacters => recruitedCharacters;

    public RegularCustomerVisitResult RecordVisit(Character customer, RegularCustomerRules rules)
    {
        rules ??= RegularCustomerRules.CreateDefault();
        if (!RegularCustomerService.IsTrackableCustomer(customer))
        {
            return new RegularCustomerVisitResult(false, null, false, false, "추적 가능한 손님이 아닙니다");
        }

        int customerId = RegularCustomerService.GetCustomerId(customer);
        RegularCustomerRecord record = GetOrCreate(customerId, customer, rules);
        if (record.IsRecruited)
        {
            return new RegularCustomerVisitResult(false, record, false, false, "이미 영입된 손님입니다");
        }

        bool wasRegular = record.IsRegular;
        bool wasRecruitCandidate = record.IsRecruitCandidate;
        record.RecordVisit(customer, RegularCustomerService.GetSatisfaction(customer), rules);

        bool becameRegular = !wasRegular && record.IsRegular;
        bool becameRecruitCandidate = !wasRecruitCandidate && record.IsRecruitCandidate;
        return new RegularCustomerVisitResult(true, record, becameRegular, becameRecruitCandidate, "방문 기록 갱신");
    }

    public bool TryGetRecord(int customerId, out RegularCustomerRecord record)
    {
        return records.TryGetValue(customerId, out record);
    }

    public bool IsRecruited(int customerId)
    {
        return records.TryGetValue(customerId, out RegularCustomerRecord record) && record.IsRecruited;
    }

    public bool TryRecruit(int customerId, out RegularCustomerRecruitResult result)
    {
        if (!records.TryGetValue(customerId, out RegularCustomerRecord record))
        {
            result = new RegularCustomerRecruitResult(false, null, "단골 기록이 없습니다");
            return false;
        }

        if (record.IsRecruited)
        {
            result = new RegularCustomerRecruitResult(false, record, "이미 영입된 손님입니다");
            return false;
        }

        if (!record.IsRecruitCandidate)
        {
            result = new RegularCustomerRecruitResult(false, record, "영입 후보가 아닙니다");
            return false;
        }

        if (!record.MarkRecruited())
        {
            result = new RegularCustomerRecruitResult(false, record, "영입할 수 없습니다");
            return false;
        }

        result = new RegularCustomerRecruitResult(true, record, "영입 완료");
        recruitedCharacters.Add(result);
        return true;
    }

    private RegularCustomerRecord GetOrCreate(int customerId, Character customer, RegularCustomerRules rules)
    {
        if (!records.TryGetValue(customerId, out RegularCustomerRecord record))
        {
            record = new RegularCustomerRecord(customerId, customer, rules.defaultRecruitCapabilities);
            records[customerId] = record;
        }

        return record;
    }
}

public struct RegularCustomerUpdatedEvent
{
    public RegularCustomerVisitResult result;

    public RegularCustomerUpdatedEvent(RegularCustomerVisitResult result)
    {
        this.result = result;
    }

    private static RegularCustomerUpdatedEvent e;

    public static void Trigger(RegularCustomerVisitResult result)
    {
        e.result = result;
        EventObserver.TriggerEvent(e);
    }
}

public struct RegularCustomerBecameRegularEvent
{
    public RegularCustomerSnapshot snapshot;

    public RegularCustomerBecameRegularEvent(RegularCustomerSnapshot snapshot)
    {
        this.snapshot = snapshot;
    }

    private static RegularCustomerBecameRegularEvent e;

    public static void Trigger(RegularCustomerSnapshot snapshot)
    {
        e.snapshot = snapshot;
        EventObserver.TriggerEvent(e);
    }
}

public struct RecruitCandidateDiscoveredEvent
{
    public RegularCustomerSnapshot snapshot;

    public RecruitCandidateDiscoveredEvent(RegularCustomerSnapshot snapshot)
    {
        this.snapshot = snapshot;
    }

    private static RecruitCandidateDiscoveredEvent e;

    public static void Trigger(RegularCustomerSnapshot snapshot)
    {
        e.snapshot = snapshot;
        EventObserver.TriggerEvent(e);
    }
}

public struct CustomerRecruitedEvent
{
    public RegularCustomerRecruitResult result;

    public CustomerRecruitedEvent(RegularCustomerRecruitResult result)
    {
        this.result = result;
    }

    private static CustomerRecruitedEvent e;

    public static void Trigger(RegularCustomerRecruitResult result)
    {
        e.result = result;
        EventObserver.TriggerEvent(e);
    }
}

public static class RegularCustomerService
{
    public static bool IsTrackableCustomer(Character customer)
    {
        return customer != null
            && customer.characterType == CharacterType.Customer
            && customer.data != null;
    }

    public static int GetCustomerId(Character customer)
    {
        if (customer == null)
        {
            return -1;
        }

        return customer.data != null ? customer.data.id : customer.GetInstanceID();
    }

    public static string GetCustomerDisplayName(Character customer, int customerId)
    {
        if (!string.IsNullOrWhiteSpace(customer?.data?.characterName))
        {
            return customer.data.characterName;
        }

        if (!string.IsNullOrWhiteSpace(customer?.name))
        {
            return customer.name;
        }

        return $"Customer {customerId}";
    }

    public static string GetCustomerSpeciesTag(Character customer)
    {
        if (!string.IsNullOrWhiteSpace(customer?.SpeciesTag))
        {
            return customer.SpeciesTag;
        }

        return "Unknown";
    }

    public static float GetSatisfaction(Character customer)
    {
        if (customer == null || customer.stats == null)
        {
            return 0f;
        }

        return customer.stats.TryGetValue(Character.Condition.MOOD, out float mood)
            ? Mathf.Clamp(mood, 0f, 100f)
            : 0f;
    }

    public static bool MeetsRegularCondition(RegularCustomerRecord record, RegularCustomerRules rules)
    {
        rules ??= RegularCustomerRules.CreateDefault();
        return record != null
            && record.VisitCount >= Mathf.Max(1, rules.regularVisitThreshold)
            && record.AverageSatisfaction >= Mathf.Clamp(rules.regularAverageSatisfactionThreshold, 0f, 100f);
    }

    public static bool MeetsRecruitCandidateCondition(RegularCustomerRecord record, RegularCustomerRules rules)
    {
        rules ??= RegularCustomerRules.CreateDefault();
        return record != null
            && MeetsRegularCondition(record, rules)
            && record.VisitCount >= Mathf.Max(1, rules.recruitCandidateVisitThreshold)
            && record.AverageSatisfaction >= Mathf.Clamp(rules.recruitCandidateAverageSatisfactionThreshold, 0f, 100f);
    }

    public static bool CanSpawnAsCustomer(CharacterSO data, RegularCustomerState state)
    {
        if (data == null || state == null)
        {
            return true;
        }

        if (data.characterType != CharacterType.Customer)
        {
            return true;
        }

        return !state.IsRecruited(data.id);
    }

    public static string FormatCapabilities(RecruitCapability capabilities)
    {
        List<string> names = new List<string>();
        if ((capabilities & RecruitCapability.Staff) != 0) names.Add("직원");
        if ((capabilities & RecruitCapability.Defense) != 0) names.Add("방어");
        if ((capabilities & RecruitCapability.Expedition) != 0) names.Add("원정");
        return names.Count > 0 ? string.Join("/", names) : "없음";
    }
}

public class RegularCustomerRuntime : MonoBehaviour, UtilEventListener<FacilityVisitEvent>
{
    [SerializeField] private RegularCustomerRules rules = RegularCustomerRules.CreateDefault();

    private readonly RegularCustomerState state = new RegularCustomerState();

    public static RegularCustomerRuntime Instance => FindFirstObjectByType<RegularCustomerRuntime>();
    public RegularCustomerState State => state;
    public RegularCustomerRules Rules => rules;

    public void OnTriggerEvent(FacilityVisitEvent eventType)
    {
        RegularCustomerVisitResult result = state.RecordVisit(eventType.visitor, rules);
        if (!result.Success)
        {
            return;
        }

        RegularCustomerUpdatedEvent.Trigger(result);

        if (result.BecameRegular)
        {
            RegularCustomerSnapshot snapshot = result.Record.ToSnapshot();
            RegularCustomerBecameRegularEvent.Trigger(snapshot);
            EventAlertService.Raise(
                "단골 등장",
                $"{snapshot.displayName}이 단골이 되었습니다.\n{snapshot.ToSummaryText()}",
                EventAlertImportance.Low,
                "단골");
        }

        if (result.BecameRecruitCandidate)
        {
            RegularCustomerSnapshot snapshot = result.Record.ToSnapshot();
            RecruitCandidateDiscoveredEvent.Trigger(snapshot);
            EventAlertService.Raise(
                "영입 후보",
                $"{snapshot.displayName}을 영입할 수 있습니다.\n가능 역할: {RegularCustomerService.FormatCapabilities(snapshot.recruitCapabilities)}",
                EventAlertImportance.Medium,
                "영입");
        }
    }

    public bool TryRecruit(int customerId, out RegularCustomerRecruitResult result)
    {
        bool recruited = state.TryRecruit(customerId, out result);
        if (!recruited)
        {
            return false;
        }

        CustomerRecruitedEvent.Trigger(result);
        EventAlertService.Raise(
            "손님 영입",
            $"{result.Record.DisplayName} 영입 완료\n가능 역할: {RegularCustomerService.FormatCapabilities(result.Capabilities)}",
            EventAlertImportance.Medium,
            "영입");
        return true;
    }

    private void OnEnable()
    {
        this.EventStartListening<FacilityVisitEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<FacilityVisitEvent>();
    }
}
