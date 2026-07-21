using System;
using UnityEngine;
using VContainer;

public class OwnerCommandController : MonoBehaviour, UtilEventListener<InfoFeedEvent>
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private LayerMask commandTargetMask = ~0;

    private IStaffDiscontentRuntimeService staffDiscontentRuntimeService;
    private IMainCameraProvider mainCameraProvider;
    private IPlayerInputReader inputReader;
    private IWorldPointerRaycaster pointerRaycaster;
    private IUiPointerBlocker uiPointerBlocker;
    private CharacterActor selectedActor;

    public CharacterActor SelectedActor => selectedActor != null ? selectedActor : null;

    private IStaffDiscontentRuntimeService StaffDiscontentRuntimeService => staffDiscontentRuntimeService
        ?? throw new InvalidOperationException($"{nameof(OwnerCommandController)} requires {nameof(IStaffDiscontentRuntimeService)} injection.");

    [Inject]
    public void ConstructOwnerCommandController(
        IStaffDiscontentRuntimeService staffDiscontentRuntimeService,
        IMainCameraProvider mainCameraProvider,
        IPlayerInputReader inputReader,
        IWorldPointerRaycaster pointerRaycaster,
        IUiPointerBlocker uiPointerBlocker)
    {
        this.staffDiscontentRuntimeService = staffDiscontentRuntimeService
            ?? throw new ArgumentNullException(nameof(staffDiscontentRuntimeService));
        this.mainCameraProvider = mainCameraProvider
            ?? throw new ArgumentNullException(nameof(mainCameraProvider));
        this.inputReader = inputReader
            ?? throw new ArgumentNullException(nameof(inputReader));
        this.pointerRaycaster = pointerRaycaster
            ?? throw new ArgumentNullException(nameof(pointerRaycaster));
        this.uiPointerBlocker = uiPointerBlocker
            ?? throw new ArgumentNullException(nameof(uiPointerBlocker));
    }

    private void Update()
    {
        if (selectedActor == null)
        {
            selectedActor = null;
            return;
        }

        if (selectedActor.Stats != null && selectedActor.Stats.IsDead)
        {
            return;
        }

        if (RequireInputReader().GetMouseButtonDown(1) && !RequireUiPointerBlocker().IsPointerOverUi())
        {
            TryIssuePriorityWorkCommand();
        }
    }

    private void TryIssuePriorityWorkCommand()
    {
        Camera camera = targetCamera != null ? targetCamera : RequireMainCameraProvider().Camera;
        if (camera == null) return;

        if (!RequirePointerRaycaster().TryRaycast(camera, commandTargetMask, out RaycastHit2D hit))
        {
            return;
        }

        CharacterActor targetActor = hit.collider.GetComponentInParent<CharacterActor>();
        if (targetActor != null && targetActor != selectedActor)
        {
            TryIssueSuppressCommand(targetActor, out _);
            return;
        }

        BuildableObject target = hit.collider.GetComponentInParent<BuildableObject>();
        if (target != null)
        {
            TryIssuePriorityWorkCommand(target, out _);
        }
    }

    public bool TrySelectActor(CharacterActor actor, out string message)
    {
        if (actor == null || (actor.Stats != null && actor.Stats.IsDead))
        {
            message = "명령할 직원을 선택할 수 없습니다.";
            return false;
        }

        if (!actor.TryGetAbility(out AbilityWork _))
        {
            message = "선택한 캐릭터는 작업 능력이 없습니다.";
            return false;
        }

        selectedActor = actor;
        message = $"명령 대상 선택: {actor.name}";
        return true;
    }

    public bool TryIssuePriorityWorkCommand(BuildableObject target, out string message)
    {
        if (selectedActor == null || (selectedActor.Stats != null && selectedActor.Stats.IsDead))
        {
            message = "먼저 명령할 직원을 선택하세요.";
            return false;
        }

        if (target == null || target.isDestroy)
        {
            message = "명령할 시설을 선택할 수 없습니다.";
            return false;
        }

        if (!selectedActor.TryGetAbility(out AbilityWork work))
        {
            message = "선택한 캐릭터는 작업 능력이 없습니다.";
            NoticeFeedEvent.Trigger(message, NoticeFeedEvent.Grade.WARNING);
            return false;
        }

        if (!WorkCommandResolver.TryResolveFacilityCommand(selectedActor, target, out FacilityWorkType workType, out string errorMessage))
        {
            selectedActor.AddActivity(CharacterActivityEvent.Facility(
                CharacterActivityKinds.Command,
                CharacterActivityOutcomes.Failed,
                $"우선 지정 실패: {errorMessage}",
                target,
                actionId: "command:priority-work",
                reasonCode: "unsupported-facility-command",
                bubbleEligible: true));
            NoticeFeedEvent.Trigger(errorMessage, NoticeFeedEvent.Grade.WARNING);
            message = errorMessage;
            return false;
        }

        GridPathSearchResult searchResult = selectedActor.Brain != null
            ? selectedActor.Brain.GetPathSearch(selectedActor)
            : null;

        if (!work.TrySetPriorityWorkTarget(target, workType, searchResult, out errorMessage))
        {
            selectedActor.AddActivity(CharacterActivityEvent.Facility(
                CharacterActivityKinds.Command,
                CharacterActivityOutcomes.Failed,
                $"우선 지정 실패: {errorMessage}",
                target,
                actionId: "command:priority-work",
                reasonCode: "priority-target-rejected",
                bubbleEligible: true));
            NoticeFeedEvent.Trigger(errorMessage, NoticeFeedEvent.Grade.WARNING);
            message = errorMessage;
            return false;
        }

        message = $"{selectedActor.name}: {target.name} {WorkTaskCatalog.GetDisplayName(workType)} 우선 지정";
        NoticeFeedEvent.Trigger(message, NoticeFeedEvent.Grade.NONE);
        selectedActor.Brain?.ClearPathSearchCache();
        return true;
    }

    public bool TryIssueSuppressCommand(CharacterActor target, out string message)
    {
        if (selectedActor == null || (selectedActor.Stats != null && selectedActor.Stats.IsDead))
        {
            message = "먼저 명령할 직원을 선택하세요.";
            return false;
        }

        if (!WorkCommandResolver.IsSuppressTarget(target, StaffDiscontentRuntimeService.IsRebellionTarget))
        {
            message = "제압할 수 있는 반란 대상이 아닙니다.";
            return false;
        }

        if (!selectedActor.TryGetAbility(out AbilityWork work))
        {
            message = "선택한 캐릭터는 작업 능력이 없습니다.";
            selectedActor.AddActivity(CreateSuppressCommandFailure(
                target,
                message,
                "missing-work-ability"));
            NoticeFeedEvent.Trigger(message, NoticeFeedEvent.Grade.WARNING);
            return false;
        }

        GridPathSearchResult searchResult = selectedActor.Brain != null
            ? selectedActor.Brain.GetPathSearch(selectedActor)
            : null;

        if (!work.TrySetPrioritySuppressTarget(target, searchResult, out string errorMessage))
        {
            selectedActor.AddActivity(CreateSuppressCommandFailure(
                target,
                errorMessage,
                "priority-target-rejected"));
            NoticeFeedEvent.Trigger(errorMessage, NoticeFeedEvent.Grade.WARNING);
            message = errorMessage;
            return false;
        }

        message = $"{selectedActor.name}: {target.name} 제압 우선 지정";
        NoticeFeedEvent.Trigger(message, NoticeFeedEvent.Grade.NONE);
        selectedActor.Brain?.ClearPathSearchCache();
        return true;
    }

    private static CharacterActivityEvent CreateSuppressCommandFailure(
        CharacterActor target,
        string message,
        string reasonCode)
    {
        return CharacterActivityEvent.Create(
            CharacterActivityKinds.Command,
            CharacterActivityOutcomes.Failed,
            $"우선 지정 실패: {message}",
            actionId: "command:priority-suppress",
            targetId: target != null ? $"character:{target.GetInstanceID()}" : string.Empty,
            targetName: target != null ? target.name : string.Empty,
            reasonCode: reasonCode,
            sentiment: -0.7f,
            bubbleEligible: true);
    }

    public void OnTriggerEvent(InfoFeedEvent eventType)
    {
        CharacterActor actor = eventType.Target as CharacterActor;
        if (actor != null)
        {
            TrySelectActor(actor, out _);
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

    private IMainCameraProvider RequireMainCameraProvider()
    {
        return mainCameraProvider
            ?? throw new InvalidOperationException($"{nameof(OwnerCommandController)} requires {nameof(IMainCameraProvider)} injection.");
    }

    private IPlayerInputReader RequireInputReader()
    {
        return inputReader
            ?? throw new InvalidOperationException($"{nameof(OwnerCommandController)} requires {nameof(IPlayerInputReader)} injection.");
    }

    private IWorldPointerRaycaster RequirePointerRaycaster()
    {
        return pointerRaycaster
            ?? throw new InvalidOperationException($"{nameof(OwnerCommandController)} requires {nameof(IWorldPointerRaycaster)} injection.");
    }

    private IUiPointerBlocker RequireUiPointerBlocker()
    {
        return uiPointerBlocker
            ?? throw new InvalidOperationException($"{nameof(OwnerCommandController)} requires {nameof(IUiPointerBlocker)} injection.");
    }
}
