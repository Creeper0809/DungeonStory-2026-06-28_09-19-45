using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TMPro;
using UnityEngine;
using VContainer;

public class EventAlertRuntime : MonoBehaviour, UtilEventListener<EventAlertRequestedEvent>
{
    [SerializeField] private Transform buttonRoot;
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private TMP_Text detailText;

    private readonly List<EventAlertRecord> eventLog = new List<EventAlertRecord>();
    private ReadOnlyCollection<EventAlertRecord> eventLogView;
    private readonly EventAlertSelectionState selectionState = new EventAlertSelectionState();
    private int nextId = 1;
    private IEventAlertViewPresenterFactory viewPresenterFactory;
    private IEventAlertViewPresenter viewPresenter;

    public IReadOnlyList<EventAlertRecord> EventLog => eventLogView ??= eventLog.AsReadOnly();
    public bool IsDetailVisible => viewPresenter != null
        ? viewPresenter.IsDetailVisible
        : detailPanel != null && detailPanel.activeSelf;
    public EventAlertRecord SelectedRecord => selectionState.SelectedRecord;

    [Inject]
    public void Construct(IEventAlertViewPresenterFactory viewPresenterFactory)
    {
        this.viewPresenterFactory = viewPresenterFactory
            ?? throw new System.ArgumentNullException(nameof(viewPresenterFactory));
        if (TryResolveViewPresenter(out IEventAlertViewPresenter presenter))
        {
            presenter.EnsureRuntimeUI();
        }
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
        if (TryResolveViewPresenter(out IEventAlertViewPresenter presenter))
        {
            presenter.OpenDetail(record);
        }
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

    public void RestoreHistory(IEnumerable<EventAlertRecordSnapshot> records)
    {
        viewPresenter?.DestroyRuntimeUI();
        viewPresenter = null;
        selectionState.Clear();
        eventLog.Clear();
        nextId = 1;

        foreach (EventAlertRecordSnapshot snapshot in records ?? System.Array.Empty<EventAlertRecordSnapshot>())
        {
            if (snapshot == null)
            {
                continue;
            }

            EventAlertRecord record = new EventAlertRecord(
                snapshot.Id,
                snapshot.Title,
                snapshot.Detail,
                snapshot.Importance,
                snapshot.Category,
                snapshot.Count,
                snapshot.Choices.Select(choice => new EventAlertChoice(choice.Label, choice.Description)));
            eventLog.Add(record);
            nextId = System.Math.Max(nextId, record.Id + 1);
            CreateButton(record);
        }
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
        if (TryResolveViewPresenter(out IEventAlertViewPresenter presenter))
        {
            presenter.CreateButton(record);
        }
    }

    private void UpdateButton(EventAlertRecord record)
    {
        if (TryResolveViewPresenter(out IEventAlertViewPresenter presenter))
        {
            presenter.UpdateButton(record);
        }
    }

    private bool TryResolveViewPresenter(out IEventAlertViewPresenter presenter)
    {
        if (viewPresenter != null)
        {
            presenter = viewPresenter;
            return true;
        }

        if (viewPresenterFactory == null)
        {
            presenter = null;
            return false;
        }

        viewPresenter = viewPresenterFactory.Create(new EventAlertViewPresenterContext(
            buttonRoot,
            detailPanel,
            detailText,
            Open,
            ExecuteChoice,
            CloseDetail));
        presenter = viewPresenter;
        return presenter != null;
    }
}
