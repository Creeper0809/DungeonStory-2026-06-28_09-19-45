using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class GridFoundationDebugScenarios
{
    [MenuItem("DungeonStory/Debug/Grid Foundation/Run 0 Foundation Scenarios")]
    public static void RunFromMenu()
    {
        bool success = RunAll(true);
        if (!success)
        {
            Debug.LogError("Grid foundation scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        List<string> errors = new List<string>();

        RunScenario("일반 복도 이동", VerifyWalkPath, errors);
        RunScenario("계단 이동", VerifyStairPath, errors);
        RunScenario("입장/퇴장 경로", VerifyEntryExitPath, errors);
        RunScenario("도달 불가 이동 후보 제외", VerifyUnreachableMovementIsExcluded, errors);

        if (errors.Count > 0)
        {
            foreach (string error in errors)
            {
                Debug.LogError(error);
            }

            return false;
        }

        if (logSuccess)
        {
            Debug.Log("Grid foundation scenarios passed.");
        }

        return true;
    }

    private static void RunScenario(string name, System.Func<bool> scenario, List<string> errors)
    {
        for (int i = 0; i < 10; i++)
        {
            if (scenario()) continue;

            errors.Add($"{name} 실패: 반복 {i + 1}");
            return;
        }
    }

    private static bool VerifyWalkPath()
    {
        Grid grid = new Grid(4, 1);
        AddHallway(grid, new Vector2Int(0, 0));
        AddHallway(grid, new Vector2Int(1, 0));
        AddHallway(grid, new Vector2Int(2, 0));

        Queue<GridMoveStep> path = grid.GetMovePath(new Vector2Int(0, 0), (pos) => pos == new Vector2Int(2, 0));
        return path.Count == 2 && path.All((step) => step.MoveType == GridMoveType.Walk);
    }

    private static bool VerifyStairPath()
    {
        Grid grid = new Grid(3, 2);
        AddHallway(grid, new Vector2Int(0, 0));
        AddHallway(grid, new Vector2Int(1, 0));
        AddHallway(grid, new Vector2Int(1, 1));
        AddHallway(grid, new Vector2Int(2, 1));
        AddMovement(grid, new Vector2Int(1, 0), new Vector2Int(1, 1), GridMoveType.Stair);

        Queue<GridMoveStep> path = grid.GetMovePath(new Vector2Int(0, 0), (pos) => pos == new Vector2Int(2, 1));
        return path.Any((step) => step.MoveType == GridMoveType.Stair)
            && path.Last().To == new Vector2Int(2, 1);
    }

    private static bool VerifyEntryExitPath()
    {
        Grid grid = new Grid(4, 1);
        AddHallway(grid, new Vector2Int(0, 0));
        AddHallway(grid, new Vector2Int(1, 0));
        AddHallway(grid, new Vector2Int(2, 0));
        AddHallway(grid, new Vector2Int(3, 0));

        Queue<GridMoveStep> entryPath = grid.GetMovePath(new Vector2Int(0, 0), (pos) => pos == new Vector2Int(3, 0));
        Queue<GridMoveStep> exitPath = grid.GetMovePath(new Vector2Int(3, 0), (pos) => pos == new Vector2Int(0, 0));
        return entryPath.Count == 3
            && exitPath.Count == 3
            && entryPath.All((step) => step.MoveType == GridMoveType.Walk)
            && exitPath.All((step) => step.MoveType == GridMoveType.Walk);
    }

    private static bool VerifyUnreachableMovementIsExcluded()
    {
        Grid grid = new Grid(6, 1);
        TestOccupant reachable = AddHallway(grid, new Vector2Int(0, 0));
        AddHallway(grid, new Vector2Int(1, 0));
        TestOccupant unreachable = AddHallway(grid, new Vector2Int(5, 0));

        List<IGridOccupant> occupants = grid.SearchPath(new Vector2Int(0, 0)).GetAllReachableOccupants();
        return occupants.Contains(reachable) && !occupants.Contains(unreachable);
    }

    private static TestOccupant AddHallway(Grid grid, Vector2Int position)
    {
        TestOccupant occupant = new TestOccupant(1, true, GridMoveType.Instant);
        grid.RegisterOccupant(occupant, GridLayer.Hallway, new List<Vector2Int> { position }, false);
        return occupant;
    }

    private static void AddMovement(Grid grid, Vector2Int from, Vector2Int to, GridMoveType moveType)
    {
        TestOccupant occupant = new TestOccupant(2, true, moveType);
        grid.RegisterOccupant(occupant, GridLayer.Building, new List<Vector2Int> { from, to }, true);
    }

    private sealed class TestOccupant : IGridOccupant, IGridMovementOccupant
    {
        public TestOccupant(int id, bool isMovement, GridMoveType moveType)
        {
            GridId = id;
            IsGridMovement = isMovement;
            GridMoveType = moveType;
        }

        public int GridId { get; }
        public bool IsGridDestroyed => false;
        public bool IsGridVisitable => false;
        public bool IsGridMovement { get; }
        public GridMoveType GridMoveType { get; }
    }
}
