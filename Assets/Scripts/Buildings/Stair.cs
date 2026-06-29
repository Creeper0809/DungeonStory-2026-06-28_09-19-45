using System.Collections;
using UnityEngine;

public class Stair : BuildableObject, IInteractable, IGridMovementHandler
{
    private WaitForSeconds delay;
    public override GridMoveType GridMoveType => GridMoveType.Stair;

    public override void Start()
    {
        base.Start();
        delay = new WaitForSeconds(0.15f);
    }

    public IEnumerator Traverse(Character character, GridMoveStep step)
    {
        if (character == null || step == null) yield break;

        EnsureDelay();
        AbilityMove moveable = character.GetAbility<AbilityMove>();
        if (moveable == null || grid == null) yield break;

        Vector3 fromAnchor = GetMovementWorldPosition(step.From);
        Vector3 toAnchor = GetMovementWorldPosition(step.To);
        float stairCenterX = Mathf.Approximately(fromAnchor.x, toAnchor.x)
            ? fromAnchor.x
            : (fromAnchor.x + toAnchor.x) * 0.5f;

        yield return moveable.Move2PosBySpeed(new Vector2(stairCenterX, character.transform.position.y), 0.7f);
        yield return delay;
        character.transform.position = new Vector3(stairCenterX, toAnchor.y, character.transform.position.z);
        yield return delay;
    }

    public IEnumerator Interact(Character character)
    {
        if (character == null || grid == null) yield break;

        Vector2Int from = grid.GetXY(character.transform.position);
        Vector2Int to = from;
        GridCell cell = grid.GetGridCell(from);
        if (cell != null)
        {
            foreach (GridTraversalLink link in cell.TraversalLinks)
            {
                if (ReferenceEquals(link.Through, this))
                {
                    to = link.To;
                    break;
                }
            }
        }

        if (to == from)
        {
            to = new Vector2Int(from.x, Mathf.Clamp(from.y + 1, 0, grid.height - 1));
        }

        yield return Traverse(character, new GridMoveStep(from, to, this, this, GridMoveType.Stair));
    }

    private void EnsureDelay()
    {
        delay ??= new WaitForSeconds(0.15f);
    }
}
