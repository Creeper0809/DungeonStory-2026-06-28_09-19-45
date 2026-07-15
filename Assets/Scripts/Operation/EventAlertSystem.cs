using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VContainer;

public class EventAlertRuntime : MonoBehaviour, UtilEventListener<EventAlertRequestedEvent>
{
    [SerializeField] private Transform buttonRoot;
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private TMP_Text detailText;

    private readonly List<EventAlertRecord> eventLog = new List<EventAlertRecord>();
    private readonly EventAlertSelectionState selectionState = new EventAlertSelectionState();
    private int nextId = 1;
    private IEventAlertViewPresenterFactory viewPresenterFactory;
    private IEventAlertViewPresenter viewPresenter;

    public IReadOnlyList<EventAlertRecord> EventLog => eventLog;
    public bool IsDetailVisible => viewPresenter != null
        ? viewPresenter.IsDetailVisible
        : detailPanel != null && detailPanel.activeSelf;
    public EventAlertRecord SelectedRecord => selectionState.SelectedRecord;

    [Inject]
    public void Construct(IEventAlertViewPresenterFactory viewPresenterFactory)
    {
        this.viewPresenterFactory = viewPresenterFactory
            ?? throw new System.ArgumentNullException(nameof(viewPresenterFactory));
        ResolveViewPresenter().EnsureRuntimeUI();
    }

    public void OnTriggerEvent(EventAlertRequestedEvent eventType)
    {
        if (eventType.request == null)
        {
            return;
        }

        EventAlertRecord record = EventAlertMergePolicy.FindMergeTarget(eventLog, eventType.request);
        if (record == null)
        {
            record = new EventAlertRecord(nextId++, eventType.request);
            eventLog.Add(record);
            CreateButton(record);
        }
        else
        {
            record.Increment();
            UpdateButton(record);
        }

        EventAlertLoggedEvent.Trigger(record);
    }

    public void Open(EventAlertRecord record)
    {
        if (record == null)
        {
            return;
        }

        selectionState.Select(record);
        ResolveViewPresenter().OpenDetail(record);
    }

    public void CloseDetail()
    {
        viewPresenter?.CloseDetail();
    }

    public bool ExecuteChoice(int index)
    {
        if (!selectionState.ExecuteChoice(index))
        {
            return false;
        }

        CloseDetail();
        return true;
    }

    private void OnEnable()
    {
        this.EventStartListening<EventAlertRequestedEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<EventAlertRequestedEvent>();
    }

    private void OnDestroy()
    {
        viewPresenter?.DestroyRuntimeUI();
    }

    private void CreateButton(EventAlertRecord record)
    {
        ResolveViewPresenter().CreateButton(record);
    }

    private void UpdateButton(EventAlertRecord record)
    {
        ResolveViewPresenter().UpdateButton(record);
    }

    private IEventAlertViewPresenter ResolveViewPresenter()
    {
        if (viewPresenter != null)
        {
            return viewPresenter;
        }

        if (viewPresenterFactory == null)
        {
            throw new System.InvalidOperationException(
                $"{nameof(EventAlertRuntime)} requires {nameof(IEventAlertViewPresenterFactory)} injection.");
        }

        viewPresenter = viewPresenterFactory.Create(new EventAlertViewPresenterContext(
            buttonRoot,
            detailPanel,
            detailText,
            Open,
            ExecuteChoice,
            CloseDetail));
        return viewPresenter;
    }
}
