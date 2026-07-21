using System;
using UnityEngine;
using VContainer;

public class FacilityEvolutionRecordRuntime :
    MonoBehaviour,
    UtilEventListener<FacilityVisitEvent>,
    UtilEventListener<FacilityRevenueEvent>,
    UtilEventListener<FacilityStockConsumedEvent>,
    UtilEventListener<FacilityCrimeEvent>,
    UtilEventListener<FacilityRestockEvent>,
    UtilEventListener<DefenseFacilityTriggeredEvent>,
    UtilEventListener<InvasionFacilityDamagedEvent>,
    UtilEventListener<OperatingDayEndedEvent>
{
    [SerializeField, Min(1)] private int highTurnoverVisitStep = 5;
    [SerializeField, Min(1)] private int cleanServiceMinVisits = 3;
    [SerializeField, Min(1)] private int highValueRevenueThreshold = 30;

    private IFacilityEvolutionRecordEventRecorder recordEventRecorder;

    [Inject]
    public void Construct(IFacilityEvolutionRecordEventRecorder recordEventRecorder)
    {
        this.recordEventRecorder = recordEventRecorder
            ?? throw new ArgumentNullException(nameof(recordEventRecorder));
    }

    public void OnTriggerEvent(FacilityVisitEvent eventType)
    {
        if (TryResolveRecordEventRecorder(out IFacilityEvolutionRecordEventRecorder recorder))
        {
            recorder.RecordVisit(eventType, highTurnoverVisitStep);
        }
    }

    public void OnTriggerEvent(FacilityRevenueEvent eventType)
    {
        if (TryResolveRecordEventRecorder(out IFacilityEvolutionRecordEventRecorder recorder))
        {
            recorder.RecordRevenue(eventType, highValueRevenueThreshold);
        }
    }

    public void OnTriggerEvent(FacilityStockConsumedEvent eventType)
    {
        if (TryResolveRecordEventRecorder(out IFacilityEvolutionRecordEventRecorder recorder))
        {
            recorder.RecordStockConsumed(eventType);
        }
    }

    public void OnTriggerEvent(FacilityCrimeEvent eventType)
    {
        if (TryResolveRecordEventRecorder(out IFacilityEvolutionRecordEventRecorder recorder))
        {
            recorder.RecordCrime(eventType);
        }
    }

    public void OnTriggerEvent(FacilityRestockEvent eventType)
    {
        if (TryResolveRecordEventRecorder(out IFacilityEvolutionRecordEventRecorder recorder))
        {
            recorder.RecordRestock(eventType);
        }
    }

    public void OnTriggerEvent(DefenseFacilityTriggeredEvent eventType)
    {
        if (TryResolveRecordEventRecorder(out IFacilityEvolutionRecordEventRecorder recorder))
        {
            recorder.RecordDefenseTriggered(eventType);
        }
    }

    public void OnTriggerEvent(InvasionFacilityDamagedEvent eventType)
    {
        if (TryResolveRecordEventRecorder(out IFacilityEvolutionRecordEventRecorder recorder))
        {
            recorder.RecordInvasionDamage(eventType);
        }
    }

    public void OnTriggerEvent(OperatingDayEndedEvent eventType)
    {
        if (TryResolveRecordEventRecorder(out IFacilityEvolutionRecordEventRecorder recorder))
        {
            recorder.CompleteOperatingDay(cleanServiceMinVisits);
        }
    }

    private void OnEnable()
    {
        this.EventStartListening<FacilityVisitEvent>();
        this.EventStartListening<FacilityRevenueEvent>();
        this.EventStartListening<FacilityStockConsumedEvent>();
        this.EventStartListening<FacilityCrimeEvent>();
        this.EventStartListening<FacilityRestockEvent>();
        this.EventStartListening<DefenseFacilityTriggeredEvent>();
        this.EventStartListening<InvasionFacilityDamagedEvent>();
        this.EventStartListening<OperatingDayEndedEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<FacilityVisitEvent>();
        this.EventStopListening<FacilityRevenueEvent>();
        this.EventStopListening<FacilityStockConsumedEvent>();
        this.EventStopListening<FacilityCrimeEvent>();
        this.EventStopListening<FacilityRestockEvent>();
        this.EventStopListening<DefenseFacilityTriggeredEvent>();
        this.EventStopListening<InvasionFacilityDamagedEvent>();
        this.EventStopListening<OperatingDayEndedEvent>();
    }

    private bool TryResolveRecordEventRecorder(out IFacilityEvolutionRecordEventRecorder recorder)
    {
        recorder = recordEventRecorder;
        return recorder != null;
    }
}
