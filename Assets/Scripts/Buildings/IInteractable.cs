using System.Collections;

public interface IInteractable
{
    public IEnumerator Interact(CharacterActor actor);
}

public interface IGridMovementHandler
{
    public IEnumerator Traverse(CharacterActor actor, GridMoveStep step);
}

public interface IStockedFacility
{
    public int CurrentStock { get; }
    public bool HasAvailableStock { get; }
}

public interface IWarehouseFacility
{
    public WarehouseInventory Inventory { get; }
    public bool HasWarehouseInventory { get; }
}

public interface IWorkableFacility
{
    public bool CanAssignWorker(CharacterActor actor, out string failureReason);
    public IEnumerator AllocateWorker(CharacterActor actor);
    public void DeallocateWorker(CharacterActor actor);
}
