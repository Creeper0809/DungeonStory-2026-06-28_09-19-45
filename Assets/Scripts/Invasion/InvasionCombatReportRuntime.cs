using UnityEngine;

public class InvasionCombatReportRuntime : MonoBehaviour,
    UtilEventListener<InvasionStartedEvent>,
    UtilEventListener<InvasionSpawnedEvent>,
    UtilEventListener<DefenseFacilityTriggeredEvent>,
    UtilEventListener<InvasionFacilityDamagedEvent>,
    UtilEventListener<InvasionFinalCombatStartedEvent>,
    UtilEventListener<InvasionResolvedEvent>
{
    private const int MaxReportHistory = 20;

    [SerializeField] private bool showActivationNotice = true;
    [SerializeField] private int maxActivationNoticeLength = 64;

    private InvasionCombatReport currentReport;
    private InvasionCombatReportSnapshot currentReportView;
    private bool isRecording;
    private readonly System.Collections.Generic.List<InvasionCombatReportSnapshot> reportHistory = new System.Collections.Generic.List<InvasionCombatReportSnapshot>();
    private System.Collections.Generic.IReadOnlyList<InvasionCombatReportSnapshot> reportHistoryView;

    public InvasionCombatReportSnapshot CurrentReport => currentReportView;
    public System.Collections.Generic.IReadOnlyList<InvasionCombatReportSnapshot> ReportHistory
    {
        get
        {
            if (reportHistoryView == null)
            {
                reportHistoryView = reportHistory.AsReadOnly();
            }

            return reportHistoryView;
        }
    }

    public void OnTriggerEvent(InvasionStartedEvent eventType)
    {
        currentReport = new InvasionCombatReport(eventType.snapshot);
        isRecording = true;
        RefreshCurrentReportView();
    }

    public void OnTriggerEvent(InvasionSpawnedEvent eventType)
    {
        EnsureReport(eventType.threatSnapshot).SetIntruder(eventType.intruderActor);
        isRecording = true;
        RefreshCurrentReportView();
    }

    public void OnTriggerEvent(DefenseFacilityTriggeredEvent eventType)
    {
        if (!isRecording || currentReport == null || eventType.report == null)
        {
            return;
        }

        currentReport.RecordDefenseActivation(eventType.report);
        RefreshCurrentReportView();
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
        RefreshCurrentReportView();
    }

    public void OnTriggerEvent(InvasionFinalCombatStartedEvent eventType)
    {
        if (!isRecording || currentReport == null)
        {
            return;
        }

        currentReport.RecordFinalCombat(eventType.ownerActor);
        RefreshCurrentReportView();
    }

    public void OnTriggerEvent(InvasionResolvedEvent eventType)
    {
        if (currentReport == null)
        {
            return;
        }

        currentReport.Resolve(eventType.defended, eventType.residualRisk);
        isRecording = false;
        InvasionCombatReportSnapshot completedReport = currentReport.CreateSnapshot();
        currentReportView = completedReport;
        reportHistory.Insert(0, completedReport);
        if (reportHistory.Count > MaxReportHistory)
        {
            reportHistory.RemoveRange(MaxReportHistory, reportHistory.Count - MaxReportHistory);
        }

        InvasionCombatReportReadyEvent.Trigger(completedReport);
        EventAlertService.RaiseInvasionResult(
            completedReport.ToDetailText(),
            eventType.defended ? EventAlertImportance.Medium : EventAlertImportance.High);
        currentReport = null;
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

    private void RefreshCurrentReportView()
    {
        currentReportView = currentReport != null ? currentReport.CreateSnapshot() : currentReportView;
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
