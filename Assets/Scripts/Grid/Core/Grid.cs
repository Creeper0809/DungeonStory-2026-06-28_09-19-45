using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GridLayer
{
    Hallway = 0,
    Building = 1,
    Character = 2
}

public enum GridMoveType
{
    Walk = 0,
    Instant = 1,
    Stair = 2,
    Elevator = 3,
    Teleport = 4
}

public interface IGridOccupant
{
    int GridId { get; }
    bool IsGridDestroyed { get; }
    bool IsGridVisitable { get; }
    bool IsGridMovement { get; }
}

public interface IGridMovementOccupant
{
    GridMoveType GridMoveType { get; }
}

public class GridTraversalLink
{
    public Vector2Int To { get; }
    public IGridOccupant Through { get; }
    public GridMoveType MoveType { get; }

    public GridTraversalLink(Vector2Int to, IGridOccupant through, GridMoveType moveType)
    {
        To = to;
        Through = through;
        MoveType = moveType;
    }
}

public class GridMoveStep
{
    public Vector2Int From { get; }
    public Vector2Int To { get; }
    public IGridOccupant DestinationOccupant { get; }
    public IGridOccupant MovementOccupant { get; }
    public GridMoveType MoveType { get; }

    public bool IsSpecialMove => MoveType != GridMoveType.Walk;

    public GridMoveStep(
        Vector2Int from,
        Vector2Int to,
        IGridOccupant destinationOccupant,
        IGridOccupant movementOccupant,
        GridMoveType moveType)
    {
        From = from;
        To = to;
        DestinationOccupant = destinationOccupant;
        MovementOccupant = movementOccupant;
        MoveType = moveType;
    }

    public GridMoveStep WithDestination(IGridOccupant destination)
    {
        return new GridMoveStep(From, To, destination, MovementOccupant, MoveType);
    }
}

public class GridPathSearchResult
{
    public Grid sourceGrid { get; private set; }
    public Vector2Int start { get; private set; }
    public int gridVersion { get; private set; }

    private readonly Dictionary<Vector2Int, GridMoveStep> parentStep;
    private readonly List<Vector2Int> searchOrder;
    private readonly List<IGridOccupant> visitableOccupants;

    public GridPathSearchResult(
        Grid sourceGrid,
        Vector2Int start,
        int gridVersion,
        Dictionary<Vector2Int, GridMoveStep> parentStep,
        List<Vector2Int> searchOrder,
        List<IGridOccupant> visitableOccupants)
    {
        this.sourceGrid = sourceGrid;
        this.start = start;
        this.gridVersion = gridVersion;
        this.parentStep = parentStep;
        this.searchOrder = searchOrder;
        this.visitableOccupants = visitableOccupants;
    }

    public List<IGridOccupant> GetAllVisitableOccupants()
    {
        return new List<IGridOccupant>(visitableOccupants);
    }

    public List<IGridOccupant> GetAllReachableOccupants()
    {
        List<IGridOccupant> result = new List<IGridOccupant>();
        foreach (Vector2Int pos in searchOrder)
        {
            GridCell cell = sourceGrid.GetGridCell(pos);
            if (cell == null) continue;

            foreach (IGridOccupant occupant in cell.GetAllOccupants())
            {
                if (occupant != null && !occupant.IsGridDestroyed && !result.Contains(occupant))
                {
                    result.Add(occupant);
                }
            }
        }

        return result;
    }

    public Queue<IGridOccupant> GetOccupantPathTo(IGridOccupant destination)
    {
        if (destination == null || destination.IsGridDestroyed) return new Queue<IGridOccupant>();

        foreach (Vector2Int pos in searchOrder)
        {
            GridCell cell = sourceGrid.GetGridCell(pos);
            if (cell != null && cell.GetAllOccupants().Contains(destination))
            {
                return BuildOccupantPath(pos, destination);
            }
        }

        return new Queue<IGridOccupant>();
    }

    public Queue<IGridOccupant> GetOccupantPath(Func<Vector2Int, bool> terminateEndCondition)
    {
        if (terminateEndCondition == null) return new Queue<IGridOccupant>();

        foreach (Vector2Int pos in searchOrder)
        {
            if (terminateEndCondition(pos))
            {
                return BuildOccupantPath(pos);
            }
        }

        return new Queue<IGridOccupant>();
    }

    public Queue<GridMoveStep> GetMovePathTo(IGridOccupant destination)
    {
        if (destination == null || destination.IsGridDestroyed) return new Queue<GridMoveStep>();

        foreach (Vector2Int pos in searchOrder)
        {
            GridCell cell = sourceGrid.GetGridCell(pos);
            if (cell != null && cell.GetAllOccupants().Contains(destination))
            {
                return BuildMovePath(pos, destination);
            }
        }

        return new Queue<GridMoveStep>();
    }

    public Queue<GridMoveStep> GetMovePath(Func<Vector2Int, bool> terminateEndCondition)
    {
        if (terminateEndCondition == null) return new Queue<GridMoveStep>();

        foreach (Vector2Int pos in searchOrder)
        {
            if (terminateEndCondition(pos))
            {
                return BuildMovePath(pos);
            }
        }

        return new Queue<GridMoveStep>();
    }

    private Queue<IGridOccupant> BuildOccupantPath(Vector2Int end, IGridOccupant destination = null)
    {
        Queue<IGridOccupant> path = new Queue<IGridOccupant>();
        foreach (GridMoveStep step in BuildMovePath(end, destination))
        {
            IGridOccupant occupant = step.IsSpecialMove
                ? step.MovementOccupant
                : step.DestinationOccupant;
            if (occupant != null && !path.Contains(occupant))
            {
                path.Enqueue(occupant);
            }
        }

        return path;
    }

    private Queue<GridMoveStep> BuildMovePath(Vector2Int end, IGridOccupant destination = null)
    {
        List<GridMoveStep> path = new List<GridMoveStep>();
        if (end == start) return new Queue<GridMoveStep>();

        Vector2Int current = end;
        while (current != start && parentStep.ContainsKey(current))
        {
            GridMoveStep step = parentStep[current];
            if (current == end && destination != null)
            {
                step = step.WithDestination(destination);
            }

            path.Add(step);
            current = step.From;
        }

        if (current != start) return new Queue<GridMoveStep>();

        path.Reverse();
        return new Queue<GridMoveStep>(path);
    }
}

public class Grid
{
    private const int DefaultCellWorldHeight = 3;

    public int width { get; private set; }
    public int height { get; private set; }
    public int version { get; private set; }

    private readonly GridCell[,] gridArray;
    private Vector3 originPos;
    private int cellWorldHeight;

    public Grid(int gridWidth, int gridHeight)
        : this(gridWidth, gridHeight, Vector3.zero, DefaultCellWorldHeight)
    {
    }

    public Grid(int gridWidth, int gridHeight, Vector3 originPos, int cellWorldHeight = DefaultCellWorldHeight)
    {
        width = Mathf.Max(1, gridWidth);
        height = Mathf.Max(1, gridHeight);
        this.originPos = originPos;
        this.cellWorldHeight = cellWorldHeight <= 0 ? DefaultCellWorldHeight : cellWorldHeight;
        version = 0;

        gridArray = new GridCell[height, width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                gridArray[y, x] = new GridCell(pos);
            }
        }
    }

    public void SetUnityCoordinates(Vector3 originPos, int cellWorldHeight = DefaultCellWorldHeight)
    {
        this.originPos = originPos;
        this.cellWorldHeight = cellWorldHeight <= 0 ? DefaultCellWorldHeight : cellWorldHeight;
    }

    public Vector2Int GetXY(Vector3 worldPosition)
    {
        int x = -Mathf.FloorToInt((worldPosition - originPos).x);
        int y = Mathf.FloorToInt((worldPosition - originPos).y) / cellWorldHeight;
        return new Vector2Int(x, y);
    }

    public Vector3 GetWorldPos(Vector2Int gridPosition)
    {
        return GetWorldPos((Vector2)gridPosition);
    }

    public Vector3 GetWorldPos(Vector2 gridPosition)
    {
        return new Vector3(
            originPos.x - gridPosition.x,
            originPos.y + (gridPosition.y * cellWorldHeight))
            + new Vector3(0.5f, 0);
    }

    public bool IsValidGridPos(Vector2Int gridPos)
    {
        return gridPos.x >= 0
            && gridPos.y >= 0
            && gridPos.x < width
            && gridPos.y < height;
    }

    public GridCell GetGridCell(Vector2Int pos)
    {
        if (IsValidGridPos(pos)) return gridArray[pos.y, pos.x];

        return null;
    }

    public IEnumerable<GridCell> GetCells()
    {
        foreach (GridCell cell in gridArray)
        {
            yield return cell;
        }
    }

    public Grid TryExpandGrid(int x, int y)
    {
        int newWidth = width + x;
        int newHeight = height + y;
        if (newWidth <= 0 || newHeight <= 0) return null;

        Grid newGrid = new Grid(newWidth, newHeight, originPos, cellWorldHeight);
        int copyHeight = Mathf.Min(height, newHeight);
        int copyWidth = Mathf.Min(width, newWidth);
        for (int j = 0; j < copyHeight; j++)
        {
            for (int i = 0; i < copyWidth; i++)
            {
                newGrid.gridArray[j, i] = GetGridCell(new Vector2Int(i, j));
            }
        }

        return newGrid;
    }

    public bool RegisterOccupant(IGridOccupant occupant, GridLayer layer, IReadOnlyList<Vector2Int> positions, bool connectPositions)
    {
        if (occupant == null || positions == null) return false;

        List<Vector2Int> targetPositions = positions.Distinct().ToList();
        if (!targetPositions.Any()) return false;

        foreach (Vector2Int tempPos in targetPositions)
        {
            GridCell cell = GetGridCell(tempPos);
            if (cell == null || !cell.CanOccupy(layer)) return false;
        }

        foreach (Vector2Int tempPos in targetPositions)
        {
            GridCell cell = GetGridCell(tempPos);
            if (!cell.TrySetOccupant(layer, occupant)) return false;
        }

        if (connectPositions)
        {
            RegisterTraversalLinks(occupant, targetPositions);
        }

        version++;
        return true;
    }

    public bool RemoveOccupant(GridLayer layer, IReadOnlyList<Vector2Int> positions, bool disconnectPositions)
    {
        if (positions == null) return false;

        List<Vector2Int> targetPositions = positions.Distinct().ToList();
        if (!targetPositions.Any()) return false;

        foreach (Vector2Int tempPos in targetPositions)
        {
            GridCell cell = GetGridCell(tempPos);
            if (cell == null) return false;
        }

        bool changed = false;
        foreach (Vector2Int tempPos in targetPositions)
        {
            GridCell cell = GetGridCell(tempPos);
            changed = changed || cell.HasOccupantInLayer(layer) || (disconnectPositions && cell.TraversalLinks.Any());
            cell.RemoveOccupantByLayer(layer);
            if (disconnectPositions)
            {
                cell.SetTraversalLinks(null);
            }
        }

        if (changed)
        {
            version++;
        }

        return changed;
    }

    public GridPathSearchResult SearchPath(Vector2Int start)
    {
        return SearchPath(start, null);
    }

    private GridPathSearchResult SearchPath(Vector2Int start, Func<Vector2Int, bool> stopCondition)
    {
        Dictionary<Vector2Int, GridMoveStep> parentStep = new Dictionary<Vector2Int, GridMoveStep>();
        List<Vector2Int> searchOrder = new List<Vector2Int>();
        List<IGridOccupant> visitableOccupants = new List<IGridOccupant>();
        if (!IsValidGridPos(start))
        {
            return new GridPathSearchResult(this, start, version, parentStep, searchOrder, visitableOccupants);
        }

        bool[,] visited = new bool[height, width];
        Vector2Int[] dir = { Vector2Int.left, Vector2Int.right };
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        List<GridMoveStep> nextSteps = new List<GridMoveStep>();

        queue.Enqueue(start);
        visited[start.y, start.x] = true;

        while (queue.Count > 0)
        {
            Vector2Int pos = queue.Dequeue();
            searchOrder.Add(pos);
            nextSteps.Clear();

            GridCell cell = GetGridCell(pos);
            if (cell == null) continue;

            foreach (IGridOccupant occupant in cell.GetAllOccupants())
            {
                if (occupant != null && occupant.IsGridVisitable && !visitableOccupants.Contains(occupant))
                {
                    visitableOccupants.Add(occupant);
                }
            }

            if (stopCondition != null && stopCondition(pos))
            {
                break;
            }

            foreach (GridTraversalLink link in cell.TraversalLinks)
            {
                AddMoveStep(nextSteps, pos, link.To, link.Through, link.MoveType);
            }

            foreach (Vector2Int nextPos in dir)
            {
                AddMoveStep(nextSteps, pos, nextPos + pos, null, GridMoveType.Walk);
            }

            foreach (GridMoveStep step in nextSteps)
            {
                Vector2Int nextPos = step.To;
                GridCell nextCell = GetGridCell(nextPos);
                if (nextCell != null
                    && !visited[nextPos.y, nextPos.x]
                    && (IsWalkable(nextPos) || (stopCondition != null && stopCondition(nextPos))))
                {
                    queue.Enqueue(nextPos);
                    visited[nextPos.y, nextPos.x] = true;
                    parentStep[nextPos] = step;
                }
            }
        }

        return new GridPathSearchResult(this, start, version, parentStep, searchOrder, visitableOccupants);
    }

    public Queue<IGridOccupant> GetOccupantPath(Vector2Int start, Func<Vector2Int, bool> terminateEndCondition)
    {
        return SearchPath(start, terminateEndCondition).GetOccupantPath(terminateEndCondition);
    }

    public Queue<GridMoveStep> GetMovePath(Vector2Int start, Func<Vector2Int, bool> terminateEndCondition)
    {
        return SearchPath(start, terminateEndCondition).GetMovePath(terminateEndCondition);
    }

    public List<IGridOccupant> GetAllVisitableOccupants(Vector2Int start)
    {
        return SearchPath(start).GetAllVisitableOccupants();
    }

    public List<IGridOccupant> GetAllReachableOccupants(Vector2Int start)
    {
        return SearchPath(start).GetAllReachableOccupants();
    }

    public Queue<IGridOccupant> SmoothOccupantPath(Queue<IGridOccupant> gridPath)
    {
        Queue<IGridOccupant> result = new Queue<IGridOccupant>();
        if (gridPath == null || !gridPath.Any()) return result;

        while (gridPath.Count > 1)
        {
            IGridOccupant occupant = gridPath.Dequeue();
            if (occupant.IsGridMovement)
            {
                result.Enqueue(occupant);
            }
        }

        result.Enqueue(gridPath.Dequeue());
        return result;
    }

    public bool IsWalkable(Vector2Int pos)
    {
        GridCell cell = GetGridCell(pos);
        if (cell == null) return false;

        if (cell.HasOccupantInLayer(GridLayer.Hallway)) return true;

        return cell.GetAllOccupants().Any((occupant) => occupant != null && occupant.IsGridMovement);
    }

    public bool TryFindNearestWalkablePosition(Vector2Int start, out Vector2Int walkablePosition)
    {
        if (IsValidGridPos(start) && IsWalkable(start))
        {
            walkablePosition = start;
            return true;
        }

        bool found = false;
        Vector2Int best = default;
        int bestDistance = int.MaxValue;
        foreach (GridCell cell in GetCells())
        {
            if (cell == null || !IsWalkable(cell.Position)) continue;

            int distance = Mathf.Abs(cell.Position.x - start.x) + Mathf.Abs(cell.Position.y - start.y);
            if (found && distance >= bestDistance) continue;

            found = true;
            best = cell.Position;
            bestDistance = distance;
        }

        walkablePosition = best;
        return found;
    }

    public bool IsConnectedWithAny(IReadOnlyCollection<Vector2Int> end)
    {
        if (end == null) return false;

        return GetOccupantPath(Vector2Int.zero, (pos) => end.Contains(pos)).Any();
    }

    public List<IGridOccupant> FindAllOccupants(Func<IGridOccupant, bool> predicate)
    {
        List<IGridOccupant> result = new List<IGridOccupant>();
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                foreach (IGridOccupant occupant in gridArray[i, j].GetAllOccupants())
                {
                    if (occupant != null && !result.Contains(occupant) && (predicate == null || predicate(occupant)))
                    {
                        result.Add(occupant);
                    }
                }
            }
        }

        return result;
    }

    private void RegisterTraversalLinks(IGridOccupant occupant, IReadOnlyList<Vector2Int> positions)
    {
        GridMoveType moveType = ResolveMoveType(occupant);
        foreach (Vector2Int from in positions)
        {
            GridCell cell = GetGridCell(from);
            if (cell == null) continue;

            List<GridTraversalLink> links = new List<GridTraversalLink>();
            foreach (Vector2Int to in positions)
            {
                if (from == to || !CanConnectMovementCells(from, to, moveType)) continue;

                links.Add(new GridTraversalLink(to, occupant, moveType));
            }

            cell.SetTraversalLinks(links);
        }
    }

    private bool CanConnectMovementCells(Vector2Int from, Vector2Int to, GridMoveType moveType)
    {
        switch (moveType)
        {
            case GridMoveType.Stair:
                return from.x == to.x && Mathf.Abs(from.y - to.y) == 1;
            case GridMoveType.Elevator:
                return from.x == to.x && from.y != to.y;
            case GridMoveType.Teleport:
                return true;
            case GridMoveType.Instant:
                return from.x == to.x && from.y != to.y;
            default:
                return false;
        }
    }

    private GridMoveType ResolveMoveType(IGridOccupant occupant)
    {
        if (occupant is IGridMovementOccupant movementOccupant)
        {
            return movementOccupant.GridMoveType;
        }

        return GridMoveType.Instant;
    }

    private void AddMoveStep(
        List<GridMoveStep> steps,
        Vector2Int from,
        Vector2Int to,
        IGridOccupant movementOccupant,
        GridMoveType moveType)
    {
        GridCell nextCell = GetGridCell(to);
        IGridOccupant destinationOccupant = nextCell?.GetTopOccupant();
        steps.Add(new GridMoveStep(from, to, destinationOccupant, movementOccupant, moveType));
    }
}
