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
        ResolveRecordEventRecorder().RecordVisit(eventType, highTurnoverVisitStep);
    }

    public void OnTriggerEvent(FacilityRevenueEvent eventType)
    {
        ResolveRecordEventRecorder().RecordRevenue(eventType, highValueRevenueThreshold);
    }

    public void OnTriggerEvent(FacilityStockConsumedEvent eventType)
    {
        ResolveRecordEventRecorder().RecordStockConsumed(eventType);
    }

    public void OnTriggerEvent(FacilityCrimeEvent eventType)
    {
        ResolveRecordEventRecorder().RecordCrime(eventType);
    }

    public void OnTriggerEvent(FacilityRestockEvent eventType)
    {
        ResolveRecordEventRecorder().RecordRestock(eventType);
    }

    public void OnTriggerEvent(DefenseFacilityTriggeredEvent eventType)
    {
        ResolveRecordEventRecorder().RecordDefenseTriggered(eventType);
    }

    public void OnTriggerEvent(InvasionFacilityDamagedEvent eventType)
    {
        ResolveRecordEventRecorder().RecordInvasionDamage(eventType);
    }

    public void OnTriggerEvent(OperatingDayEndedEvent eventType)
    {
        ResolveRecordEventRecorder().CompleteOperatingDay(cleanServiceMinVisits);
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

    private IFacilityEvolutionRecordEventRecorder ResolveRecordEventRecorder()
    {
        return recordEventRecorder
            ?? throw new InvalidOperationException($"{nameof(FacilityEvolutionRecordRuntime)} requires {nameof(IFacilityEvolutionRecordEventRecorder)} injection.");
    }
}
