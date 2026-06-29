using System.Collections;

public interface IInteractable
{
    public IEnumerator Interact(Character character);
}

public interface IGridMovementHandler
{
    public IEnumerator Traverse(Character character, GridMoveStep step);
}
