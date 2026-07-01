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

        RunScenario("?쇰컲 蹂듬룄 ?대룞", VerifyWalkPath, errors);
        RunScenario("怨꾨떒 ?대룞", VerifyStairPath, errors);
        RunScenario("?낆옣/?댁옣 寃쎈줈", VerifyEntryExitPath, errors);
        RunScenario("?꾨떖 遺덇? ?대룞 ?꾨낫 ?쒖쇅", VerifyUnreachableMovementIsExcluded, errors);

        RunScenario("????녿뒗 嫄대Ъ sprite generated visual ?뚮뜑", VerifyTilelessBuildingDoesNotCreateGeneratedSprite, errors);
        RunScenario("?쒕옒洹?ghost sprite 諛섎났 ?뚮뜑", VerifyDraggedGhostUsesRepeatedSprites, errors);

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

            errors.Add($"{name} ?ㅽ뙣: 諛섎났 {i + 1}");
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

    private static bool VerifyTilelessBuildingDoesNotCreateGeneratedSprite()
    {
        BuildingSO lab = AssetDatabase.LoadAssetAtPath<BuildingSO>(
            "Assets/Resources/SO/Building/P1/P1_ResearchLab.asset");
        if (lab == null
            || lab.sprite == null
            || (lab.tiles != null && lab.tiles.Count > 0))
        {
            return false;
        }

        Grid grid = new Grid(10, 1);
        for (int x = 0; x < grid.width; x++)
        {
            AddHallway(grid, new Vector2Int(x, 0));
        }

        GridBuildingPlacementService service = new GridBuildingPlacementService(
            grid,
            null,
            null,
            new GridBuildingFactory(),
            new BuildingPlacementValidator());
        bool placed = service.TryPlaceBuilding(lab, new Vector2Int(4, 0), out _);
        BuildableObject building = grid.GetGridCell(new Vector2Int(4, 0)).GetBuilding();
        SpriteRenderer renderer = building != null
            ? building.GetComponentInChildren<SpriteRenderer>()
            : null;

        bool valid = placed
            && building != null
            && renderer == null;

        if (building != null)
        {
            Object.DestroyImmediate(building.gameObject);
        }

        return valid;
    }

    private static bool VerifyDraggedGhostUsesRepeatedSprites()
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Images/Placeholders/Facilities/facility_research_lab.png");
        if (sprite == null)
        {
            return false;
        }

        GameObject root = new GameObject("GhostRoot");
        GameObject mainObject = new GameObject("Ghost");
        mainObject.transform.SetParent(root.transform, false);

        SpriteRenderer mainRenderer = mainObject.AddComponent<SpriteRenderer>();
        mainRenderer.sortingLayerName = "UI";
        mainRenderer.sortingOrder = 100;

        GridGhostObject ghost = root.AddComponent<GridGhostObject>();
        ghost.Initialize(mainObject);

        List<Vector3> positions = new List<Vector3>
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0f, 0f)
        };
        List<bool> buildableStates = new List<bool> { true, false, true };

        ghost.ShowRepeated(sprite, positions, new Vector2(1f, 3f), buildableStates);

        List<SpriteRenderer> activeRenderers = root
            .GetComponentsInChildren<SpriteRenderer>(false)
            .OrderBy((renderer) => renderer.bounds.center.x)
            .ToList();

        bool repeatedValid = activeRenderers.Count == 3
            && activeRenderers.All((renderer) => renderer.sprite == sprite)
            && activeRenderers.All((renderer) => Mathf.Abs(renderer.bounds.size.x - 1f) <= 0.05f)
            && activeRenderers.All((renderer) => Mathf.Abs(renderer.bounds.size.y - 3f) <= 0.05f)
            && Mathf.Abs(activeRenderers[0].bounds.center.x - 0f) <= 0.05f
            && Mathf.Abs(activeRenderers[1].bounds.center.x - 1f) <= 0.05f
            && Mathf.Abs(activeRenderers[2].bounds.center.x - 2f) <= 0.05f
            && activeRenderers[0].color == Color.green
            && activeRenderers[1].color == Color.red
            && activeRenderers[2].color == Color.green;

        ghost.Show(sprite);
        bool singleValid = root.GetComponentsInChildren<SpriteRenderer>(false).Length == 1;

        Object.DestroyImmediate(root);
        return repeatedValid && singleValid;
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
