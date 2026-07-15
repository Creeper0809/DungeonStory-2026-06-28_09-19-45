using System.Collections;
using System.Linq;
using UnityEngine;

public class Facility : BuildableObject, IInteractable, IWorkableFacility, IWarehouseFacility
{
    private CharacterActor worker;
    private WarehouseInventory warehouseInventory;

    public WarehouseInventory Inventory => warehouseInventory;
    public bool HasWarehouseInventory => warehouseInventory != null;

    public override void Initialization(BuildingSO buildingSO, Vector2Int buildPos)
    {
        base.Initialization(buildingSO, buildPos);

        if (Operational != null && Operational.storageCapacity > 0)
        {
            bool restrictCategory = !Operational.HasFunction(FacilityFunction.Logistics);
            warehouseInventory = new WarehouseInventory(
                Operational.storageCapacity,
                Operational.storageCategory,
                restrictCategory);
            if (Operational.HasFunction(FacilityFunction.Logistics))
            {
                warehouseInventory.ApplySnapshot(
                    WarehouseInventory.CreateSeeded(Operational.storageCapacity).CreateSnapshot());
            }
        }
        else
        {
            warehouseInventory = Facility != null
                && Facility.SupportsRole(FacilityRole.Logistics)
                && Facility.internalStockMax > 0
                    ? WarehouseInventory.CreateSeeded(Facility.internalStockMax)
                    : null;
        }
    }

    public IEnumerator Interact(CharacterActor actor)
    {
        if (!TryBeginUse(actor, out string failureReason))
        {
            actor?.AddLog($"{objectNameOrDefault()} 이용 실패: {failureReason}");
            yield break;
        }

        AbilityMove moveable = actor != null ? actor.GetAbility<AbilityMove>() : null;
        if (moveable == null)
        {
            EndUse(actor);
            yield break;
        }

        AIAction currentAction = actor != null && actor.Brain != null
            ? actor.Brain.bestAction
            : null;
        Vector3 usePosition = GetFacilityAnchorWorldPosition(FacilityAnchorKind.Use, actor.transform.position);
        actor?.Brain?.SetActionPhase("\uC2DC\uC124 \uC811\uADFC", this);
        yield return moveable.Move2PosBySpeed(usePosition, 0.7f, currentAction);
        actor?.Brain?.SetActionPhase("\uC790\uB9AC \uC7A1\uAE30", this);
        yield return Linger(actor, 0.12f, currentAction);

        float duration = Facility != null ? Facility.useDuration : 1f;
        if (actor != null && actor.Stats != null)
        {
            duration *= actor.Stats.GetStayDurationMultiplier();
        }

        actor?.Brain?.SetActionPhase("\uC2DC\uC124 \uC774\uC6A9", this, $"{duration:0.#}s");
        if (duration > 0f)
        {
            yield return new WaitForSeconds(duration);
        }

        ApplyConfiguredUseRecovery(actor);
        ModularFacilityRuntimeEffects.ApplyUseCompleted(actor, this);
        RoomEnvironmentExperienceEvent.Trigger(
            actor,
            this,
            RoomExperienceActivity.FacilityUse);
        actor?.Brain?.SetActionPhase("\uC774\uC6A9 \uC815\uB9AC", this);
        yield return Linger(actor, 0.12f, currentAction);
        actor?.AddLog($"{objectNameOrDefault()} 이용 완료");
        EndUse(actor);
    }

    public bool CanAssignWorker(CharacterActor actor, out string failureReason)
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

        if (worker != null && worker != actor)
        {
            failureReason = "이미 근무자가 있음";
            return false;
        }

        if (HasWorkerReservationForOther(actor))
        {
            failureReason = "이미 작업 예약됨";
            return false;
        }

        return true;
    }

    public IEnumerator AllocateWorker(CharacterActor actor)
    {
        PruneInvalidWorker();
        if (!CanAssignWorker(actor, out _))
        {
            yield break;
        }

        worker = actor;
        ReleaseWorkerReservation(actor);
        AbilityMove moveable = actor != null ? actor.GetAbility<AbilityMove>() : null;
        if (moveable == null) yield break;

        AIAction currentAction = actor != null && actor.Brain != null
            ? actor.Brain.bestAction
            : null;
        Vector3 workPosition = GetFacilityAnchorWorldPosition(FacilityAnchorKind.Work, actor.transform.position);
        actor?.Brain?.SetActionPhase("\uC791\uC5C5\uB300 \uC811\uADFC", this);
        yield return moveable.Move2PosBySpeed(workPosition, 1f, currentAction);
        actor.ChangeLayer("DungeonMiddleObject");
        yield return moveable.Move2PosBySpeed(workPosition + new Vector3(0f, 0.15f), 3f, currentAction);
        actor?.Brain?.SetActionPhase("\uC791\uC5C5 \uC790\uC138", this);
        actor.Flip(CharacterFacing.RIGHT);
    }

    public void DeallocateWorker(CharacterActor actor)
    {
        if (actor == null)
        {
            return;
        }

        PruneInvalidWorker();
        if (worker != actor) return;

        worker = null;
        actor.Brain?.SetActionPhase("\uC2DC\uC124 \uD1F4\uC7A5", this);
        actor.transform.position -= new Vector3(0f, 0.15f);
        Vector2Int actorGridPosition = grid != null
            ? grid.GetXY(actor.transform.position)
            : centerPos;
        if (!ContainsGridPosition(actorGridPosition)
            && TryGetFacilityOccupiedWorldPosition(actor.transform.position, out Vector3 exitPosition))
        {
            actor.transform.position = exitPosition;
        }
        actor.ChangeLayer("Default");
    }

    private static IEnumerator Linger(CharacterActor actor, float seconds, AIAction expectedAction)
    {
        if (seconds <= 0f)
        {
            yield break;
        }

        float timer = 0f;
        while (timer < seconds)
        {
            if (expectedAction != null
                && (actor == null
                    || actor.Brain == null
                    || actor.Brain.bestAction != expectedAction
                    || actor.Brain.isBestActionEnd))
            {
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }
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
        Vector2 minWorld = grid.GetWorldPos(new Vector2Int(minX, centerPos.y));
        Vector2 maxWorld = grid.GetWorldPos(new Vector2Int(maxX, centerPos.y));
        return new Vector2(Random.Range(minWorld.x, maxWorld.x), minWorld.y);
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

    public void ApplyConfiguredUseRecovery(CharacterActor actor)
    {
        if (actor == null || Facility == null)
        {
            return;
        }

        FacilityNeedRecoveryData configured = Operational != null ? Operational.recovery : default;
        float sleep = configured.sleep;
        float mood = configured.mood;
        float fun = configured.fun;
        float hunger = configured.hunger;
        float excretion = configured.excretion;
        float hygiene = configured.hygiene;

        if (configured.HasEffect)
        {
            ApplyRecovery(actor, sleep, mood, fun, hunger, excretion, hygiene);
            return;
        }

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

        if (Facility.SupportsRole(FacilityRole.Toilet))
        {
            excretion += 70f;
            mood += 2f;
        }

        if (Facility.SupportsRole(FacilityRole.Hygiene))
        {
            hygiene += 60f;
            mood += 4f;
        }

        ApplyRecovery(actor, sleep, mood, fun, hunger, excretion, hygiene);
    }

    private void ApplyRecovery(
        CharacterActor actor,
        float sleep,
        float mood,
        float fun,
        float hunger,
        float excretion,
        float hygiene)
    {
        if (sleep == 0f
            && mood == 0f
            && fun == 0f
            && hunger == 0f
            && excretion == 0f
            && hygiene == 0f)
        {
            return;
        }

        if (mood != 0f)
        {
            actor.ApplyMoodFactor(
                $"facility:{GetInstanceID()}",
                $"{objectNameOrDefault()} 이용",
                mood,
                180f,
                2);
        }

        if (actor.TryGetAbility(out AbilityWork work))
        {
            work.RecoverOffDuty(sleep, 0f, fun, hunger, excretion, hygiene);
            return;
        }

        if (sleep != 0f) actor.ChangesStat(CharacterCondition.SLEEP, sleep);
        if (fun != 0f) actor.ChangesStat(CharacterCondition.FUN, fun);
        if (hunger != 0f) actor.ChangesStat(CharacterCondition.HUNGER, hunger);
        if (excretion != 0f) actor.ChangesStat(CharacterCondition.EXCRETION, excretion);
        if (hygiene != 0f) actor.ChangesStat(CharacterCondition.HYGIENE, hygiene);
    }

    private string objectNameOrDefault()
    {
        return BuildingData != null && !string.IsNullOrWhiteSpace(BuildingData.objectName)
            ? BuildingData.objectName
            : name;
    }
}
