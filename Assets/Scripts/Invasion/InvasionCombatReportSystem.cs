using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct InvasionCombatFeedbackEvent
{
    public string message;
    public DefenseActivationReport defenseReport;

    public InvasionCombatFeedbackEvent(string message, DefenseActivationReport defenseReport)
    {
        this.message = message ?? string.Empty;
        this.defenseReport = defenseReport;
    }

    private static InvasionCombatFeedbackEvent e;

    public static void Trigger(string message, DefenseActivationReport defenseReport)
    {
        e.message = message ?? string.Empty;
        e.defenseReport = defenseReport;
        EventObserver.TriggerEvent(e);
    }
}

public struct InvasionCombatReportReadyEvent
{
    public InvasionCombatReport report;

    public InvasionCombatReportReadyEvent(InvasionCombatReport report)
    {
        this.report = report;
    }

    private static InvasionCombatReportReadyEvent e;

    public static void Trigger(InvasionCombatReport report)
    {
        e.report = report;
        EventObserver.TriggerEvent(e);
    }
}

public class InvasionCombatReport
{
    private readonly List<DefenseContribution> defenseContributions = new List<DefenseContribution>();
    private readonly List<BuildableObject> damagedFacilities = new List<BuildableObject>();
    private readonly List<string> observations = new List<string>();
    private readonly List<string> synergyLogs = new List<string>();
    private readonly List<string> activationLogs = new List<string>();

    public InvasionCombatReport(InvasionThreatSnapshot snapshot)
    {
        ThreatSnapshot = snapshot;
        StartedAt = Time.time;
    }

    public InvasionThreatSnapshot ThreatSnapshot { get; }
    public float StartedAt { get; }
    public Character Intruder { get; private set; }
    public Character FinalCombatOwner { get; private set; }
    public bool FinalCombatStarted { get; private set; }
    public bool Defended { get; private set; }
    public float ResidualRisk { get; private set; }
    public bool IsResolved { get; private set; }

    public IReadOnlyList<DefenseContribution> DefenseContributions => defenseContributions;
    public IReadOnlyList<BuildableObject> DamagedFacilities => damagedFacilities;
    public IReadOnlyList<string> Observations => observations;
    public IReadOnlyList<string> SynergyLogs => synergyLogs;
    public IReadOnlyList<string> ActivationLogs => activationLogs;

    public DefenseContribution TopDamageContribution => defenseContributions
        .Where((item) => item.TotalDamage > 0f)
        .OrderByDescending((item) => item.TotalDamage)
        .FirstOrDefault();

    public DefenseContribution TopDelayContribution => defenseContributions
        .Where((item) => item.MaxDelaySeconds > 0f)
        .OrderByDescending((item) => item.MaxDelaySeconds)
        .FirstOrDefault();

    public IReadOnlyList<BuildableObject> BrokenFacilities => damagedFacilities
        .Where((facility) => facility != null && facility.IsDamaged)
        .ToList();

    public void SetIntruder(Character intruder)
    {
        Intruder = intruder;
    }

    public void RecordDefenseActivation(DefenseActivationReport report)
    {
        if (report == null || report.Facility == null)
        {
            return;
        }

        DefenseContribution contribution = FindOrCreateContribution(report.Facility);
        contribution.Add(report);

        string activation = InvasionCombatReportFormatter.FormatActivation(report);
        if (!string.IsNullOrWhiteSpace(activation))
        {
            activationLogs.Add(activation);
        }

        foreach (string tag in report.EffectTags)
        {
            AddObservation(InvasionCombatReportFormatter.FormatObservation(tag));
        }

        string synergy = InvasionCombatReportFormatter.FormatSynergy(report);
        if (!string.IsNullOrWhiteSpace(synergy) && !synergyLogs.Contains(synergy))
        {
            synergyLogs.Add(synergy);
        }
    }

    public void RecordFacilityDamage(BuildableObject facility)
    {
        if (facility == null || damagedFacilities.Contains(facility))
        {
            return;
        }

        damagedFacilities.Add(facility);
    }

    public void RecordFinalCombat(Character owner)
    {
        FinalCombatStarted = true;
        FinalCombatOwner = owner;
    }

    public void Resolve(bool defended, float residualRisk)
    {
        Defended = defended;
        ResidualRisk = Mathf.Max(0f, residualRisk);
        IsResolved = true;
    }

    public string ToDetailText()
    {
        List<string> lines = new List<string>
        {
            Defended ? "방어 결과: 방어 성공" : "방어 결과: 방어 실패",
            $"잔여 위험도: {ResidualRisk:0.#}"
        };

        AddContributionLine(lines, "가장 많은 피해를 준 시설", TopDamageContribution, true);
        AddContributionLine(lines, "가장 오래 지연시킨 시설", TopDelayContribution, false);

        lines.Add($"결정적 방어: {GetDecisiveDefenseText()}");
        lines.Add($"피해를 입은 시설: {FormatFacilities(damagedFacilities)}");
        lines.Add($"파손 시설: {FormatFacilities(BrokenFacilities)}");
        lines.Add($"획득한 관찰 정보: {FormatObservations()}");

        if (synergyLogs.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("작동한 연계:");
            foreach (string synergy in synergyLogs.Take(5))
            {
                lines.Add($"- {synergy}");
            }
        }

        if (activationLogs.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("전투 중 발동:");
            foreach (string activation in activationLogs.Skip(Mathf.Max(0, activationLogs.Count - 8)))
            {
                lines.Add($"- {activation}");
            }
        }

        return string.Join("\n", lines);
    }

    private DefenseContribution FindOrCreateContribution(DefenseFacility facility)
    {
        DefenseContribution contribution = defenseContributions.FirstOrDefault((item) => item.Facility == facility);
        if (contribution != null)
        {
            return contribution;
        }

        contribution = new DefenseContribution(facility);
        defenseContributions.Add(contribution);
        return contribution;
    }

    private void AddObservation(string observation)
    {
        if (!string.IsNullOrWhiteSpace(observation) && !observations.Contains(observation))
        {
            observations.Add(observation);
        }
    }

    private string GetDecisiveDefenseText()
    {
        if (FinalCombatStarted)
        {
            string ownerName = FinalCombatOwner != null ? FinalCombatOwner.name : "사장";
            return Defended ? $"{ownerName} 최종 교전 생존" : $"{ownerName} 최종 교전 패배";
        }

        DefenseContribution topDamage = TopDamageContribution;
        if (Defended && topDamage != null)
        {
            return $"{topDamage.FacilityName} 피해 {topDamage.TotalDamage:0.#}";
        }

        DefenseContribution topDelay = TopDelayContribution;
        if (topDelay != null)
        {
            return $"{topDelay.FacilityName} 지연 {topDelay.MaxDelaySeconds:0.#}초";
        }

        return Defended ? "침입자 제압" : "기록된 결정적 방어 없음";
    }

    private string FormatFacilities(IEnumerable<BuildableObject> facilities)
    {
        List<string> names = facilities?
            .Where((facility) => facility != null)
            .Select(InvasionCombatReportFormatter.GetBuildingName)
            .Distinct()
            .ToList()
            ?? new List<string>();

        return names.Count > 0 ? string.Join(", ", names) : "없음";
    }

    private string FormatObservations()
    {
        return observations.Count > 0 ? string.Join(", ", observations) : "없음";
    }

    private static void AddContributionLine(
        List<string> lines,
        string label,
        DefenseContribution contribution,
        bool damage)
    {
        if (contribution == null)
        {
            lines.Add($"{label}: 없음");
            return;
        }

        string value = damage
            ? $"{contribution.TotalDamage:0.#} 피해"
            : $"{contribution.MaxDelaySeconds:0.#}초 지연";
        lines.Add($"{label}: {contribution.FacilityName} ({value})");
    }
}

public class DefenseContribution
{
    private readonly List<string> effectTags = new List<string>();

    public DefenseContribution(DefenseFacility facility)
    {
        Facility = facility;
    }

    public DefenseFacility Facility { get; }
    public float TotalDamage { get; private set; }
    public float MaxDelaySeconds { get; private set; }
    public int TriggerCount { get; private set; }
    public IReadOnlyList<string> EffectTags => effectTags;
    public string FacilityName => InvasionCombatReportFormatter.GetBuildingName(Facility);

    public void Add(DefenseActivationReport report)
    {
        if (report == null)
        {
            return;
        }

        TriggerCount++;
        TotalDamage += Mathf.Max(0f, report.TotalDamage);
        MaxDelaySeconds = Mathf.Max(MaxDelaySeconds, report.MovementDelaySeconds);

        foreach (string tag in report.EffectTags)
        {
            if (!string.IsNullOrWhiteSpace(tag) && !effectTags.Contains(tag))
            {
                effectTags.Add(tag);
            }
        }
    }
}

public static class InvasionCombatReportFormatter
{
    public static string FormatActivation(DefenseActivationReport report)
    {
        if (report == null || report.Facility == null)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>
        {
            $"{GetBuildingName(report.Facility)} 발동"
        };

        if (report.TotalDamage > 0f)
        {
            parts.Add($"피해 {report.TotalDamage:0.#}");
        }

        if (report.MovementDelaySeconds > 0f)
        {
            parts.Add($"지연 {report.MovementDelaySeconds:0.#}초");
        }

        if (report.EffectTags.Count > 0)
        {
            parts.Add(string.Join(", ", report.EffectTags));
        }

        return string.Join(" / ", parts);
    }

    public static string FormatObservation(string effectTag)
    {
        if (string.IsNullOrWhiteSpace(effectTag))
        {
            return string.Empty;
        }

        string normalized = effectTag.Trim();
        if (normalized.Contains("축전 방전"))
        {
            return "관찰: 축전 방전";
        }

        if (normalized.Contains("축전"))
        {
            return "관찰: 축전 누적";
        }

        if (normalized.Contains("감속") || normalized.Contains("속박"))
        {
            return "관찰: 감속 적용";
        }

        if (normalized.Contains("부식"))
        {
            return "관찰: 부식 적용";
        }

        if (normalized.Contains("연소"))
        {
            return "관찰: 연소 적용";
        }

        if (normalized.Contains("경비"))
        {
            return "관찰: 경비 교전";
        }

        if (normalized.Contains("피해") || normalized.Contains("가시"))
        {
            return "관찰: 직접 피해";
        }

        return $"관찰: {normalized}";
    }

    public static string FormatSynergy(DefenseActivationReport report)
    {
        if (report == null)
        {
            return string.Empty;
        }

        string facilityName = GetBuildingName(report.Facility);
        IReadOnlyList<string> tags = report.EffectTags;
        if (report.Concept == DefenseAttackConcept.Guard || tags.Any((tag) => tag.Contains("경비")))
        {
            return $"{facilityName}: 직원/경비 교전";
        }

        if (tags.Any((tag) => tag.Contains("축전 방전")))
        {
            return $"{facilityName}: 번개 축전 방전";
        }

        if (tags.Any((tag) => tag.Contains("부식")) && report.TotalDamage > 0f)
        {
            return $"{facilityName}: 독 피해와 부식";
        }

        if (tags.Any((tag) => tag.Contains("연소")) && report.TotalDamage > 0f)
        {
            return $"{facilityName}: 화염 피해와 연소";
        }

        if (report.MovementDelaySeconds > 0f)
        {
            return $"{facilityName}: 냉기 피해와 이동 지연";
        }

        if (report.TotalDamage > 0f)
        {
            return $"{facilityName}: 직접 피해";
        }

        return string.Empty;
    }

    public static string GetBuildingName(BuildableObject building)
    {
        if (building == null)
        {
            return "시설";
        }

        if (building.BuildingData != null && !string.IsNullOrWhiteSpace(building.BuildingData.objectName))
        {
            return building.BuildingData.objectName;
        }

        return string.IsNullOrWhiteSpace(building.name) ? "시설" : building.name;
    }
}

public class InvasionCombatReportRuntime : MonoBehaviour,
    UtilEventListener<InvasionStartedEvent>,
    UtilEventListener<InvasionSpawnedEvent>,
    UtilEventListener<DefenseFacilityTriggeredEvent>,
    UtilEventListener<InvasionFacilityDamagedEvent>,
    UtilEventListener<InvasionFinalCombatStartedEvent>,
    UtilEventListener<InvasionResolvedEvent>
{
    [SerializeField] private bool showActivationNotice = true;
    [SerializeField] private int maxActivationNoticeLength = 64;

    private InvasionCombatReport currentReport;
    private bool isRecording;

    public InvasionCombatReport CurrentReport => currentReport;

    public void OnTriggerEvent(InvasionStartedEvent eventType)
    {
        currentReport = new InvasionCombatReport(eventType.snapshot);
        isRecording = true;
    }

    public void OnTriggerEvent(InvasionSpawnedEvent eventType)
    {
        EnsureReport(eventType.threatSnapshot).SetIntruder(eventType.intruder);
        isRecording = true;
    }

    public void OnTriggerEvent(DefenseFacilityTriggeredEvent eventType)
    {
        if (!isRecording || currentReport == null || eventType.report == null)
        {
            return;
        }

        currentReport.RecordDefenseActivation(eventType.report);
        string message = InvasionCombatReportFormatter.FormatActivation(eventType.report);
        InvasionCombatFeedbackEvent.Trigger(message, eventType.report);

        if (showActivationNotice && !string.IsNullOrWhiteSpace(message))
        {
            NoticeFeedEvent.Trigger(ClampLine(message), NoticeFeedEvent.Grade.NONE);
        }
    }

    public void OnTriggerEvent(InvasionFacilityDamagedEvent eventType)
    {
        if (!isRecording || currentReport == null)
        {
            return;
        }

        currentReport.RecordFacilityDamage(eventType.facility);
    }

    public void OnTriggerEvent(InvasionFinalCombatStartedEvent eventType)
    {
        if (!isRecording || currentReport == null)
        {
            return;
        }

        currentReport.RecordFinalCombat(eventType.owner);
    }

    public void OnTriggerEvent(InvasionResolvedEvent eventType)
    {
        if (currentReport == null)
        {
            return;
        }

        currentReport.Resolve(eventType.defended, eventType.residualRisk);
        isRecording = false;
        InvasionCombatReportReadyEvent.Trigger(currentReport);
        EventAlertService.RaiseInvasionResult(
            currentReport.ToDetailText(),
            eventType.defended ? EventAlertImportance.Medium : EventAlertImportance.High);
    }

    private void OnEnable()
    {
        this.EventStartListening<InvasionStartedEvent>();
        this.EventStartListening<InvasionSpawnedEvent>();
        this.EventStartListening<DefenseFacilityTriggeredEvent>();
        this.EventStartListening<InvasionFacilityDamagedEvent>();
        this.EventStartListening<InvasionFinalCombatStartedEvent>();
        this.EventStartListening<InvasionResolvedEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<InvasionStartedEvent>();
        this.EventStopListening<InvasionSpawnedEvent>();
        this.EventStopListening<DefenseFacilityTriggeredEvent>();
        this.EventStopListening<InvasionFacilityDamagedEvent>();
        this.EventStopListening<InvasionFinalCombatStartedEvent>();
        this.EventStopListening<InvasionResolvedEvent>();
    }

    private InvasionCombatReport EnsureReport(InvasionThreatSnapshot snapshot)
    {
        currentReport ??= new InvasionCombatReport(snapshot);
        return currentReport;
    }

    private string ClampLine(string message)
    {
        int maxLength = Mathf.Max(12, maxActivationNoticeLength);
        if (string.IsNullOrWhiteSpace(message) || message.Length <= maxLength)
        {
            return message;
        }

        return message.Substring(0, maxLength - 1) + "...";
    }
}
