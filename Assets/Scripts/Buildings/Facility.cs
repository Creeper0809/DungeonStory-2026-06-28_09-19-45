using System.Collections;
using System.Linq;
using UnityEngine;

public class Facility : BuildableObject, IInteractable, IWorkableFacility, IWarehouseFacility
{
    private Character worker;
    private WarehouseInventory warehouseInventory;

    public WarehouseInventory Inventory => warehouseInventory;
    public bool HasWarehouseInventory => warehouseInventory != null;

    public override void Initialization(BuildingSO buildingSO, Vector2Int buildPos)
    {
        base.Initialization(buildingSO, buildPos);

        warehouseInventory = Facility != null
            && Facility.SupportsRole(FacilityRole.Logistics)
            && Facility.internalStockMax > 0
                ? WarehouseInventory.CreateSeeded(Facility.internalStockMax)
                : null;
    }

    public IEnumerator Interact(Character character)
    {
        if (!TryBeginUse(character, out string failureReason))
        {
            character?.AddLog($"{objectNameOrDefault()} ?댁슜 ?ㅽ뙣: {failureReason}");
            yield break;
        }

        AbilityMove moveable = character != null ? character.GetAbility<AbilityMove>() : null;
        if (moveable == null)
        {
            EndUse(character);
            yield break;
        }

        yield return moveable.Move2PosBySpeed(GetRandomUsePosition(), 0.7f);

        float duration = Facility != null ? Facility.useDuration : 1f;
        if (character != null)
        {
            duration *= character.GetStayDurationMultiplier();
        }

        if (duration > 0f)
        {
            yield return new WaitForSeconds(duration);
        }

        ApplyUseRecovery(character);
        character?.AddLog($"{objectNameOrDefault()} ?댁슜 ?꾨즺");
        EndUse(character);
    }

    public bool CanAssignWorker(Character character, out string failureReason)
    {
        PruneInvalidWorker();
        bool hasAssignableWork = false;
        failureReason = "지원하지 않는 작업";
        foreach (FacilityWorkType workType in WorkTaskCatalog.GetSingleTypes(Facility != null ? Facility.supportedWorkTypes : FacilityWorkType.None))
        {
            if (CanAssignWork(workType, out failureReason))
            {
                hasAssignableWork = true;
                break;
            }
        }

        if (!hasAssignableWork)
        {
            return false;
        }

        if (worker != null && worker != character)
        {
            failureReason = "이미 근무자가 있음";
            return false;
        }

        if (HasWorkerReservationForOther(character))
        {
            failureReason = "이미 작업 예약됨";
            return false;
        }

        return true;
    }

    public IEnumerator AllocateWorker(Character character)
    {
        PruneInvalidWorker();
        if (!CanAssignWorker(character, out _))
        {
            yield break;
        }

        worker = character;
        ReleaseWorkerReservation(character);
        AbilityMove moveable = character.GetAbility<AbilityMove>();
        if (moveable == null) yield break;

        yield return moveable.Move2PosBySpeed(GetWorkerPosition());
        character.ChangeLayer("DungeonMiddleObject");
        character.transform.position = GetWorkerPosition() + new Vector2(0f, 0.15f);
        character.Flip(Character.Facing.RIGHT);
    }

    public void DeallocateWorker(Character character)
    {
        PruneInvalidWorker();
        if (worker != character) return;

        worker = null;
        character.transform.position -= new Vector3(0f, 0.15f);
        if (TryGetNearestWalkableWorldPosition(character.transform.position, out Vector3 exitPosition))
        {
            character.transform.position = exitPosition;
        }
        character.ChangeLayer("Default");
    }

    private void PruneInvalidWorker()
    {
        if (worker == null)
        {
            return;
        }

        try
        {
            if (worker.gameObject == null
                || !worker.gameObject.scene.IsValid()
                || !worker.gameObject.activeInHierarchy)
            {
                worker = null;
            }
        }
        catch (MissingReferenceException)
        {
            worker = null;
        }
    }

    private Vector2 GetRandomUsePosition()
    {
        if (grid == null || buildPoses == null || buildPoses.Count == 0)
        {
            return transform.position;
        }

        int minX = buildPoses.Min((pos) => pos.x);
        int maxX = buildPoses.Max((pos) => pos.x);
        Vector2 left = grid.GetWorldPos(new Vector2Int(maxX, centerPos.y));
        Vector2 right = grid.GetWorldPos(new Vector2Int(minX, centerPos.y));
        return new Vector2(Random.Range(left.x, right.x), left.y);
    }

    private Vector2 GetWorkerPosition()
    {
        if (grid == null || buildPoses == null || buildPoses.Count == 0)
        {
            return transform.position;
        }

        float endX = buildPoses.Max((pos) => pos.x) - 0.2f;
        return grid.GetWorldPos(new Vector2(endX, centerPos.y));
    }

    private void ApplyUseRecovery(Character character)
    {
        if (character == null || Facility == null)
        {
            return;
        }

        float sleep = 0f;
        float mood = 0f;
        float fun = 0f;
        float hunger = 0f;

        if (Facility.SupportsRole(FacilityRole.Rest))
        {
            sleep += 35f;
            mood += 12f;
        }

        if (Facility.SupportsRole(FacilityRole.Training))
        {
            fun += 15f;
            mood += 5f;
        }

        if (Facility.SupportsRole(FacilityRole.Research))
        {
            fun += 10f;
            mood += 8f;
        }

        if (Facility.SupportsRole(FacilityRole.Mana))
        {
            mood += 10f;
        }

        if (Facility.SupportsRole(FacilityRole.Logistics))
        {
            mood += 3f;
        }

        if (Facility.SupportsRole(FacilityRole.Meal))
        {
            hunger += 35f;
            mood += 5f;
        }

        if (sleep == 0f && mood == 0f && fun == 0f && hunger == 0f)
        {
            return;
        }

        if (character.TryGetAbility(out AbilityWork work))
        {
            work.RecoverOffDuty(sleep, mood, fun, hunger);
            return;
        }

        if (sleep != 0f) character.ChangesStat(Character.Condition.SLEEP, sleep);
        if (mood != 0f) character.ChangesStat(Character.Condition.MOOD, mood);
        if (fun != 0f) character.ChangesStat(Character.Condition.FUN, fun);
        if (hunger != 0f) character.ChangesStat(Character.Condition.HUNGER, hunger);
    }

    private string objectNameOrDefault()
    {
        return BuildingData != null && !string.IsNullOrWhiteSpace(BuildingData.objectName)
            ? BuildingData.objectName
            : name;
    }
}
