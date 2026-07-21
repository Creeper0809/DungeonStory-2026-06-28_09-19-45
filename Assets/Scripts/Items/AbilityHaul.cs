using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class AbilityHaul : MonoBehaviour
{
    private CharacterActor actor;
    private AbilityMove move;
    private Coroutine haulingRoutine;
    private WorldItemHaulJob activeJob;

    public bool IsHauling => haulingRoutine != null;

    private void Awake()
    {
        CacheReferences();
    }

    public static AbilityHaul Ensure(CharacterActor actor)
    {
        if (actor == null)
        {
            return null;
        }

        AbilityHaul ability = actor.GetComponent<AbilityHaul>();
        if (ability == null && Application.isPlaying)
        {
            ability = actor.gameObject.AddComponent<AbilityHaul>();
        }

        ability?.CacheReferences();
        return ability;
    }

    public bool CanStartHauling(out string failureReason)
    {
        failureReason = string.Empty;
        CacheReferences();
        return actor != null
            && move != null
            && WorldItemStackRuntime.Active != null
            && WorldItemStackRuntime.Active.HasAvailableHaulJob(actor);
    }

    public void StartHauling()
    {
        CacheReferences();
        if (actor == null || move == null || WorldItemStackRuntime.Active == null)
        {
            EndAiAction();
            return;
        }

        StopHauling("restart");
        if (!WorldItemStackRuntime.Active.TryReserveBestHaulJob(
                actor,
                out WorldItemHaulJob reservedJob,
                out string reason))
        {
            actor.Brain?.SetActionPhase("운반 대기", null, reason);
            EndAiAction();
            return;
        }

        activeJob = reservedJob;
        haulingRoutine = StartCoroutine(HaulRoutine(activeJob));
    }

    public void StopHauling(string reason)
    {
        if (haulingRoutine != null)
        {
            StopCoroutine(haulingRoutine);
            haulingRoutine = null;
        }

        if (activeJob.IsValid && actor != null && WorldItemStackRuntime.Active != null)
        {
            string actorId = actor.Identity != null ? actor.Identity.PersistentId : string.Empty;
            WorldItemStackRuntime.Active.ReleaseReservation(activeJob.StackId, actorId);
        }

        activeJob = default;
    }

    private IEnumerator HaulRoutine(WorldItemHaulJob job)
    {
        CharacterCarryInventory carry = CharacterCarryInventory.Ensure(actor);
        if (carry == null || WorldItemStackRuntime.Active == null)
        {
            EndAiAction();
            yield break;
        }

        move.CancelActiveMovement();
        if (!TryGetGrid(out Grid grid))
        {
            actor.Brain?.SetActionPhase("운반 실패", null, "no grid");
            EndAiAction();
            yield break;
        }

        AIAction expectedAction = actor.Brain != null ? actor.Brain.bestAction : null;
        actor.Brain?.SetActionPhase("물건 가지러 가는 중", null, job.ItemPosition.ToString());
        Queue<GridMoveStep> pickupPath = grid.GetMovePath(actor.GetNowXY(), pos => pos == job.PickupStandPosition);
        yield return move.MoveByPath(pickupPath, expectedAction);
        if (IsActionCancelled(expectedAction)
            || (move.LastGridMoveWasBlocked && !IsActorAt(job.PickupStandPosition)))
        {
            EndAiAction();
            yield break;
        }

        actor.Brain?.SetActionPhase("물건 줍는 중", null, job.ItemPosition.ToString());
        if (!WorldItemStackRuntime.Active.TryPickupReservedStack(actor, carry, job, out string pickupReason))
        {
            actor.Brain?.SetActionPhase("운반 실패", null, pickupReason);
            EndAiAction();
            yield break;
        }

        yield return new WaitForSeconds(0.1f);
        actor.Brain?.SetActionPhase("창고로 옮기는 중", null, job.DeliveryPosition.ToString());
        Queue<GridMoveStep> deliveryPath = grid.GetMovePath(actor.GetNowXY(), pos => pos == job.DeliveryPosition);
        yield return move.MoveByPath(deliveryPath, expectedAction);
        if (IsActionCancelled(expectedAction)
            || (move.LastGridMoveWasBlocked && !IsActorAt(job.DeliveryPosition)))
        {
            EndAiAction();
            yield break;
        }

        actor.Brain?.SetActionPhase("창고 입고", null, job.DeliveryPosition.ToString());
        string depositReason;
        bool deposited;
        if (job.DestinationKind == WorldItemHaulDestinationKind.FacilityBuffer)
        {
            deposited = WorldItemStackRuntime.Active.TryDepositCarriedItemsToFacility(
                actor,
                carry,
                job.DropPosition,
                job.DestinationId,
                out depositReason);
        }
        else
        {
            deposited = WorldItemStackRuntime.Active.TryDepositCarriedItems(
                actor,
                carry,
                job.Warehouse,
                out depositReason);
        }

        if (!deposited && string.IsNullOrWhiteSpace(depositReason))
        {
            depositReason = "deposit failed";
        }

        if (!string.IsNullOrWhiteSpace(depositReason))
        {
            actor.AddLog("운반 정리: " + depositReason);
        }
        else
        {
            actor.AddLog("바닥 물건을 창고에 정리했다.");
        }

        haulingRoutine = null;
        activeJob = default;
        EndAiAction();
    }

    private bool TryGetGrid(out Grid grid)
    {
        grid = null;
        GridSystemManager manager = FindFirstObjectByType<GridSystemManager>();
        if (manager != null && manager.grid != null)
        {
            grid = manager.grid;
            return true;
        }

        return false;
    }

    private bool IsActionCancelled(AIAction expectedAction)
    {
        return expectedAction != null
            && (actor == null || actor.Brain == null || actor.Brain.bestAction != expectedAction);
    }

    private bool IsActorAt(Vector2Int gridPosition)
    {
        return actor != null && actor.GetNowXY() == gridPosition;
    }

    private void EndAiAction()
    {
        if (actor != null && actor.Brain != null)
        {
            actor.Brain.isBestActionEnd = true;
            actor.Brain.RequestImmediateReplan(clearFailures: true);
        }

        haulingRoutine = null;
    }

    private void CacheReferences()
    {
        actor = actor != null ? actor : GetComponent<CharacterActor>();
        move = move != null ? move : GetComponent<AbilityMove>();
    }
}
