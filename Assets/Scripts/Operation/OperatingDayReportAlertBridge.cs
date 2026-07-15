using UnityEngine;

public class OperatingDayReportAlertBridge : MonoBehaviour, UtilEventListener<OperatingDayReportEvent>
{
    public void OnTriggerEvent(OperatingDayReportEvent eventType)
    {
        OperatingDayReport report = eventType.report;
        if (report == null)
        {
            return;
        }

        EventAlertService.Raise(
            $"Day {report.day} 정산",
            report.ToDetailText(),
            EventAlertImportance.Medium,
            "정산");

        if (report.staffComplaintEvents.Count > 0)
        {
            EventAlertService.RaiseStaffComplaint(
                string.Join("\n", report.staffComplaintEvents),
                report.staffComplaintEvents.Count >= 3 ? EventAlertImportance.High : EventAlertImportance.Medium);
        }
    }

    private void OnEnable()
    {
        this.EventStartListening<OperatingDayReportEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<OperatingDayReportEvent>();
    }
}
