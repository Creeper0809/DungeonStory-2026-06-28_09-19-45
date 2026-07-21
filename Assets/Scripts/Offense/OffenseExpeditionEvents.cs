using System.Collections.Generic;
using System.Linq;

public sealed class OffenseExpeditionStartedSnapshot
{
    public OffenseExpeditionStartedSnapshot(OffenseExpeditionRun expedition)
    {
        expeditionId = expedition?.ExpeditionId ?? string.Empty;
        targetId = expedition?.Target?.id ?? string.Empty;
        targetTitle = expedition?.Target?.title ?? string.Empty;
        totalPower = expedition?.TotalPower ?? 0f;
        durationSeconds = expedition?.TotalDurationSeconds ?? 0f;
        memberNames = EventPayloadSnapshot.Copy(
            expedition?.MemberActors
                .Where((member) => member != null)
                .Select((member) => member.Identity != null ? member.Identity.DisplayName : member.name)
                .ToArray());
    }

    public string expeditionId { get; }
    public string targetId { get; }
    public string targetTitle { get; }
    public float totalPower { get; }
    public float durationSeconds { get; }
    public IReadOnlyList<string> memberNames { get; }
}

public readonly struct OffenseExpeditionStartedEvent
{
    public OffenseExpeditionStartedEvent(OffenseExpeditionRun expedition)
    {
        this.expedition = new OffenseExpeditionStartedSnapshot(expedition);
    }

    public OffenseExpeditionStartedSnapshot expedition { get; }

    public static void Trigger(OffenseExpeditionRun expedition)
    {
        EventObserver.TriggerEvent(new OffenseExpeditionStartedEvent(expedition));
    }
}

public readonly struct OffenseExpeditionCompletedEvent
{
    public OffenseExpeditionCompletedEvent(OffenseExpeditionResult result)
    {
        this.result = result;
    }

    public OffenseExpeditionResult result { get; }

    public static void Trigger(OffenseExpeditionResult result)
    {
        EventObserver.TriggerEvent(new OffenseExpeditionCompletedEvent(result));
    }
}
