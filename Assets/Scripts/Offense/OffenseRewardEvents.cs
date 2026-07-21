using System.Collections.Generic;

public readonly struct OffenseRewardGrantedEvent
{
    public OffenseRewardGrantedEvent(
        OffenseExpeditionResult expeditionResult,
        IReadOnlyList<OffenseRewardGrantResult> grantResults)
    {
        this.expeditionResult = expeditionResult;
        this.grantResults = EventPayloadSnapshot.Copy(grantResults);
    }

    public OffenseExpeditionResult expeditionResult { get; }
    public IReadOnlyList<OffenseRewardGrantResult> grantResults { get; }

    public static void Trigger(
        OffenseExpeditionResult expeditionResult,
        IReadOnlyList<OffenseRewardGrantResult> grantResults)
    {
        EventObserver.TriggerEvent(new OffenseRewardGrantedEvent(expeditionResult, grantResults));
    }
}
