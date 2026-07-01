using UnityEngine;

public class OwnerCommandController : MonoBehaviour, UtilEventListener<InfoFeedEvent>
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private LayerMask commandTargetMask = ~0;

    private Character selectedCharacter;

    public Character SelectedCharacter => selectedCharacter;

    private void Update()
    {
        if (selectedCharacter == null || selectedCharacter.IsDead)
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            TryIssuePriorityWorkCommand();
        }
    }

    private void TryIssuePriorityWorkCommand()
    {
        Camera camera = targetCamera != null ? targetCamera : Camera.main;
        if (camera == null) return;

        Vector3 mouseWorld = camera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero, 0f, commandTargetMask);
        if (!hit.collider) return;

        Character targetCharacter = hit.collider.GetComponentInParent<Character>();
        if (targetCharacter != null && targetCharacter != selectedCharacter)
        {
            TryIssueSuppressCommand(targetCharacter);
            return;
        }

        BuildableObject target = hit.collider.GetComponentInParent<BuildableObject>();
        if (target == null) return;

        if (!selectedCharacter.TryGetAbility(out AbilityWork work))
        {
            NoticeFeedEvent.Trigger("선택한 캐릭터가 작업 능력을 가지고 있지 않습니다.", NoticeFeedEvent.Grade.WARNING);
            return;
        }

        if (!WorkCommandResolver.TryResolveFacilityCommand(selectedCharacter, target, out FacilityWorkType workType, out string errorMessage))
        {
            selectedCharacter.AddLog($"우선 지정 실패: {errorMessage}");
            NoticeFeedEvent.Trigger(errorMessage, NoticeFeedEvent.Grade.WARNING);
            return;
        }

        GridPathSearchResult searchResult = selectedCharacter.ai != null
            ? selectedCharacter.ai.GetPathSearch(selectedCharacter)
            : null;

        if (!work.TrySetPriorityWorkTarget(target, workType, searchResult, out errorMessage))
        {
            selectedCharacter.AddLog($"우선 지정 실패: {errorMessage}");
            NoticeFeedEvent.Trigger(errorMessage, NoticeFeedEvent.Grade.WARNING);
            return;
        }

        NoticeFeedEvent.Trigger(
            $"{selectedCharacter.name}: {target.name} {WorkTaskCatalog.GetDisplayName(workType)} 우선 지정",
            NoticeFeedEvent.Grade.NONE);
        selectedCharacter.ai?.ClearPathSearchCache();
    }

    private void TryIssueSuppressCommand(Character target)
    {
        if (!WorkCommandResolver.IsSuppressTarget(target))
        {
            return;
        }

        if (!selectedCharacter.TryGetAbility(out AbilityWork work))
        {
            string message = "선택한 캐릭터가 작업 능력을 가지고 있지 않습니다.";
            selectedCharacter.AddLog($"우선 지정 실패: {message}");
            NoticeFeedEvent.Trigger(message, NoticeFeedEvent.Grade.WARNING);
            return;
        }

        GridPathSearchResult searchResult = selectedCharacter.ai != null
            ? selectedCharacter.ai.GetPathSearch(selectedCharacter)
            : null;

        if (!work.TrySetPrioritySuppressTarget(target, searchResult, out string errorMessage))
        {
            selectedCharacter.AddLog($"우선 지정 실패: {errorMessage}");
            NoticeFeedEvent.Trigger(errorMessage, NoticeFeedEvent.Grade.WARNING);
            return;
        }

        NoticeFeedEvent.Trigger(
            $"{selectedCharacter.name}: {target.name} 제압 우선 지정",
            NoticeFeedEvent.Grade.NONE);
        selectedCharacter.ai?.ClearPathSearchCache();
    }

    public void OnTriggerEvent(InfoFeedEvent eventType)
    {
        if (eventType.infoable is Character character && character.TryGetAbility(out AbilityWork _))
        {
            selectedCharacter = character;
        }
    }

    private void OnEnable()
    {
        this.EventStartListening<InfoFeedEvent>();
    }

    private void OnDisable()
    {
        this.EventStopListening<InfoFeedEvent>();
    }
}
