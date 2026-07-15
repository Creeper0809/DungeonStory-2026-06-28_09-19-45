using System;
using System.Collections.Generic;

public struct OffenseRewardGrantedEvent
{
    public OffenseExpeditionResult expeditionResult;
    public IReadOnlyList<OffenseRewardGrantResult> grantResults;

    public OffenseRewardGrantedEvent(
        OffenseExpeditionResult expeditionResult,
        IReadOnlyList<OffenseRewardGrantResult> grantResults)
    {
        this.expeditionResult = expeditionResult;
        this.grantResults = grantResults ?? Array.Empty<OffenseRewardGrantResult>();
    }

    private static OffenseRewardGrantedEvent e;

    public static void Trigger(
        OffenseExpeditionResult expeditionResult,
        IReadOnlyList<OffenseRewardGrantResult> grantResults)
    {
        e.expeditionResult = expeditionResult;
        e.grantResults = grantResults ?? Array.Empty<OffenseRewardGrantResult>();
        EventObserver.TriggerEvent(e);
    }
}
