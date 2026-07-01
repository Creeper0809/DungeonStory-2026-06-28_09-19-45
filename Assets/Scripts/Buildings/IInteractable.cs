using System.Collections;

public interface IInteractable
{
    public IEnumerator Interact(Character character);
}

public interface IGridMovementHandler
{
    public IEnumerator Traverse(Character character, GridMoveStep step);
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
    public bool CanAssignWorker(Character character, out string failureReason);
    public IEnumerator AllocateWorker(Character character);
    public void DeallocateWorker(Character character);
}
