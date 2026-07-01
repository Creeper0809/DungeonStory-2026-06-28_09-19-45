using System;
using System.Linq;
using UnityEngine;

public class OwnerRunManager : UtilSingleton<OwnerRunManager>, UtilEventListener<CharacterDeathEvent>
{
    [SerializeField] private CharacterSO[] ownerCandidates = Array.Empty<CharacterSO>();
    [SerializeField] private CharacterSO defaultOwner;
    [SerializeField] private GameObject ownerPrefab;
    [SerializeField] private Transform ownerSpawnPoint;
    [SerializeField] private Vector2Int ownerSpawnGridPosition = Vector2Int.zero;
    [SerializeField] private bool autoSpawnDefaultOwner = true;

    public Data<CharacterSO> selectedOwnerData = new Data<CharacterSO>();

    public Character CurrentOwner { get; private set; }
    public bool IsRunEnded { get; private set; }
    public CharacterSO[] OwnerCandidates => ownerCandidates;

    public event Action<CharacterSO> OnOwnerSelected;
    public event Action<Character, string> OnRunEnded;

    protected override void Awake()
    {
        base.Awake();
        EnsureOwnerCandidates();
        selectedOwnerData ??= new Data<CharacterSO>();
    }

    private void Start()
    {
        if (autoSpawnDefaultOwner && CurrentOwner == null)
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

        if (CurrentOwner != null && !CurrentOwner.IsDead)
        {
            Destroy(CurrentOwner.gameObject);
        }

        selectedOwnerData.Value = ownerData;
        CurrentOwner = SpawnOwner(ownerData);
        OnOwnerSelected?.Invoke(ownerData);
        NoticeFeedEvent.Trigger($"{ownerData.characterName} 사장으로 시작", NoticeFeedEvent.Grade.NONE);
    }

    public void HandleOwnerDeath(Character owner, string reason)
    {
        if (owner == null || owner != CurrentOwner || IsRunEnded)
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

    private Character SpawnOwner(CharacterSO ownerData)
    {
        GameObject ownerObject = ownerPrefab != null
            ? Instantiate(ownerPrefab)
            : new GameObject("OwnerCharacter");

        ownerObject.name = ownerData.characterName;
        ownerObject.transform.position = ResolveOwnerSpawnPosition();

        Character owner = EnsureOwnerComponents(ownerObject);
        owner.SetLifecycleState(Character.LifecycleState.Active);
        owner.Initialization(ownerData);
        if (owner.ai != null)
        {
            owner.ai.UseOwnerWorkActions();
        }

        return owner;
    }

    private Character EnsureOwnerComponents(GameObject ownerObject)
    {
        EnsureCharacterVisual(ownerObject);

        if (!ownerObject.TryGetComponent(out AIBrain _))
        {
            ownerObject.AddComponent<AIBrain>();
        }

        if (!ownerObject.TryGetComponent(out Character owner))
        {
            owner = ownerObject.AddComponent<Character>();
        }

        if (!ownerObject.TryGetComponent(out AbilityMove _))
        {
            ownerObject.AddComponent<AbilityMove>();
        }

        if (!ownerObject.TryGetComponent(out AbilityWork _))
        {
            ownerObject.AddComponent<AbilityWork>();
        }

        owner.RefreshAbilityCache();
        return owner;
    }

    private static SpriteRenderer EnsureCharacterVisual(GameObject characterObject)
    {
        Transform visual = characterObject.transform.Find("Visual");
        if (visual == null)
        {
            GameObject visualObject = new GameObject("Visual");
            visual = visualObject.transform;
            visual.SetParent(characterObject.transform, false);
        }

        if (!visual.TryGetComponent(out SpriteRenderer renderer))
        {
            renderer = visual.gameObject.AddComponent<SpriteRenderer>();
        }

        return renderer;
    }

    private Vector3 ResolveOwnerSpawnPosition()
    {
        if (ownerSpawnPoint != null)
        {
            return ownerSpawnPoint.position;
        }

        Grid grid = GridSystemManager.Instance != null ? GridSystemManager.Instance.grid : null;
        if (grid == null)
        {
            return Vector3.zero;
        }

        if (grid.IsValidGridPos(ownerSpawnGridPosition) && grid.IsWalkable(ownerSpawnGridPosition))
        {
            return grid.GetWorldPos(ownerSpawnGridPosition);
        }

        return grid.TryFindNearestWalkablePosition(ownerSpawnGridPosition, out Vector2Int walkablePosition)
            ? grid.GetWorldPos(walkablePosition)
            : Vector3.zero;
    }

    private void EnsureOwnerCandidates()
    {
        ownerCandidates = ownerCandidates?
            .Where((candidate) => candidate != null)
            .Distinct()
            .ToArray() ?? Array.Empty<CharacterSO>();

        if (ownerCandidates.Length == 0)
        {
            ownerCandidates = Resources.LoadAll<CharacterSO>("SO/Character/Owners")
                .Where((candidate) => candidate != null && candidate.IsOwnerCandidate)
                .OrderBy((candidate) => candidate.id)
                .ToArray();
        }
    }

    public void OnTriggerEvent(CharacterDeathEvent eventType)
    {
        if (eventType.Character != null && eventType.Character.IsOwner)
        {
            HandleOwnerDeath(eventType.Character, eventType.Reason);
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
    public Character Owner;
    public string Reason;

    public OwnerRunEndedEvent(Character owner, string reason)
    {
        Owner = owner;
        Reason = reason;
    }

    private static OwnerRunEndedEvent e;

    public static void Trigger(Character owner, string reason)
    {
        e.Owner = owner;
        e.Reason = reason;
        EventObserver.TriggerEvent(e);
    }
}
