using System.Collections.Generic;

public static class EventAlertService
{
    public static void Raise(
        string title,
        string detail,
        EventAlertImportance importance,
        string category = "",
        IEnumerable<EventAlertChoice> choices = null)
    {
        EventAlertRequestedEvent.Trigger(new EventAlertRequest(title, detail, importance, category, choices));
    }

    public static void RaiseInvasionResult(string detail, EventAlertImportance importance = EventAlertImportance.High)
    {
        Raise("침입 결과", detail, importance, "침입");
    }

    public static void RaiseStaffComplaint(string detail, EventAlertImportance importance = EventAlertImportance.Medium)
    {
        Raise("직원 불만", detail, importance, "직원");
    }

    public static void RaiseBlueprintAcquired(string detail, EventAlertImportance importance = EventAlertImportance.Medium)
    {
        Raise("설계도 획득", detail, importance, "설계도");
    }
}
