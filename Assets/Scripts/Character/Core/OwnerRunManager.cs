using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

public class OwnerRunManager : SerializedMonoBehaviour, UtilEventListener<CharacterDeathEvent>
{
    [SerializeField] private CharacterSO[] ownerCandidates = Array.Empty<CharacterSO>();
    [SerializeField] private CharacterSO defaultOwner;
    [SerializeField] private GameObject ownerPrefab;
    [SerializeField] private Transform ownerSpawnPoint;
    [SerializeField] private Vector2Int ownerSpawnGridPosition = Vector2Int.zero;
    [SerializeField] private bool autoSpawnDefaultOwner = true;

    public Data<CharacterSO> selectedOwnerData = new Data<CharacterSO>();

    private CharacterActor currentOwnerActor;
    private IOwnerCandidateCatalog ownerCandidateCatalog;
    private IOwnerCharacterFactory ownerCharacterFactory;

    public CharacterActor CurrentOwnerActor => currentOwnerActor;
    public bool IsRunEnded { get; private set; }
    public CharacterSO[] OwnerCandidates => ownerCandidates;

    public event Action<CharacterSO> OnOwnerSelected;
    public event Action<CharacterActor, string> OnRunEnded;

    private void Awake()
    {
        NormalizeOwnerCandidates();
        selectedOwnerData ??= new Data<CharacterSO>();
    }

    [Inject]
    public void ConstructOwnerRunManager(
        IOwnerCandidateCatalog ownerCandidateCatalog,
        IOwnerCharacterFactory ownerCharacterFactory)
    {
        this.ownerCandidateCatalog = ownerCandidateCatalog
            ?? throw new ArgumentNullException(nameof(ownerCandidateCatalog));
        this.ownerCharacterFactory = ownerCharacterFactory
            ?? throw new ArgumentNullException(nameof(ownerCharacterFactory));
        EnsureOwnerCandidates();
    }

    private void Start()
    {
        if (autoSpawnDefaultOwner && currentOwnerActor == null)
        {
            CharacterSO owner = defaultOwner != null ? defaultOwner : ownerCandidates.FirstOrDefault();
            if (owner != null)
            {
                SelectOwner(owner);
            }
        }
    }

    public void SelectOwnerByIndex(int index)
    {
        EnsureOwnerCandidates();
        if (index < 0 || index >= ownerCandidates.Length)
        {
            NoticeFeedEvent.Trigger("선택할 사장 후보가 없습니다.", NoticeFeedEvent.Grade.WARNING);
            return;
        }

        SelectOwner(ownerCandidates[index]);
    }

    public void SelectOwner(CharacterSO ownerData)
    {
        if (ownerData == null)
        {
            NoticeFeedEvent.Trigger("사장 데이터가 없습니다.", NoticeFeedEvent.Grade.DANGER);
            return;
        }

        if (!ownerData.IsOwnerCandidate)
        {
            NoticeFeedEvent.Trigger($"{ownerData.characterName}은 사장 후보가 아닙니다.", NoticeFeedEvent.Grade.WARNING);
            return;
        }

        if (currentOwnerActor != null && !currentOwnerActor.IsDead)
        {
            Destroy(currentOwnerActor.gameObject);
        }

        selectedOwnerData.Value = ownerData;
        currentOwnerActor = SpawnOwner(ownerData);
        OnOwnerSelected?.Invoke(ownerData);
        NoticeFeedEvent.Trigger($"{ownerData.characterName} 사장으로 시작", NoticeFeedEvent.Grade.NONE);
    }

    public void HandleOwnerDeath(CharacterActor owner, string reason)
    {
        if (owner == null || owner != currentOwnerActor || IsRunEnded)
        {
            return;
        }

        IsRunEnded = true;
        string message = string.IsNullOrWhiteSpace(reason)
            ? "사장이 사망해 런이 종료되었습니다."
            : $"사장이 사망해 런이 종료되었습니다. 원인: {reason}";
        NoticeFeedEvent.Trigger(message, NoticeFeedEvent.Grade.DANGER);
        OnRunEnded?.Invoke(owner, reason);
        OwnerRunEndedEvent.Trigger(owner, reason);
    }

    public CharacterSO GetDefaultOwner()
    {
        EnsureOwnerCandidates();
        return defaultOwner != null ? defaultOwner : ownerCandidates.FirstOrDefault();
    }

    private CharacterActor SpawnOwner(CharacterSO ownerData)
    {
        return ResolveOwnerCharacterFactory().CreateOwner(
            ownerData,
            ownerPrefab,
            ownerSpawnPoint,
            ownerSpawnGridPosition);
    }

    private void EnsureOwnerCandidates()
    {
        NormalizeOwnerCandidates();

        if (ownerCandidates.Length == 0)
        {
            IOwnerCandidateCatalog catalog = ownerCandidateCatalog
                ?? throw new InvalidOperationException($"{nameof(OwnerRunManager)} requires {nameof(IOwnerCandidateCatalog)} injection before loading owner candidates.");
            ownerCandidates = catalog.OwnerCandidates
                .Where((candidate) => candidate != null)
                .Distinct()
                .ToArray();
        }
    }

    private void NormalizeOwnerCandidates()
    {
        ownerCandidates = ownerCandidates?
            .Where((candidate) => candidate != null)
            .Distinct()
            .ToArray() ?? Array.Empty<CharacterSO>();
    }

    private IOwnerCharacterFactory ResolveOwnerCharacterFactory()
    {
        return ownerCharacterFactory
            ?? throw new InvalidOperationException($"{nameof(OwnerRunManager)} requires {nameof(IOwnerCharacterFactory)} injection.");
    }

    public void OnTriggerEvent(CharacterDeathEvent eventType)
    {
        if (eventType.Actor != null && eventType.Actor.IsOwner)
        {
            HandleOwnerDeath(eventType.Actor, eventType.Reason);
        }
    }

    private void OnEnable()
    {
        this.EventStartListening<CharacterDeathEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<CharacterDeathEvent>();
    }
}

public struct OwnerRunEndedEvent
{
    public CharacterActor OwnerActor;
    public string Reason;

    public OwnerRunEndedEvent(CharacterActor owner, string reason)
    {
        OwnerActor = owner;
        Reason = reason;
    }

    private static OwnerRunEndedEvent e;

    public static void Trigger(CharacterActor owner, string reason)
    {
        e.OwnerActor = owner;
        e.Reason = reason;
        EventObserver.TriggerEvent(e);
    }
}
