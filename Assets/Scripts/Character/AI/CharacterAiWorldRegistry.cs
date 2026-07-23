using System.Collections.Generic;
using UnityEngine;

public static class CharacterAiWorldRegistry
{
    private static readonly List<CharacterActor> characters = new List<CharacterActor>();
    private static readonly List<WildlifeActor> wildlife = new List<WildlifeActor>();
    private static readonly List<BuildableObject> buildings = new List<BuildableObject>();
    private static readonly List<IWarehouseFacility> warehouses = new List<IWarehouseFacility>();
    private static Grid cachedGrid;
    private static GameManager cachedGameManager;

    public static int Version { get; private set; }
    public static int CharacterVersion { get; private set; }
    public static int WildlifeVersion { get; private set; }
    public static int BuildingVersion { get; private set; }
    public static int WarehouseVersion { get; private set; }

    public static IReadOnlyList<CharacterActor> Characters
    {
        get
        {
            PruneCharacters();
            return characters;
        }
    }

    public static IReadOnlyList<WildlifeActor> Wildlife
    {
        get
        {
            PruneWildlife();
            return wildlife;
        }
    }

    public static IReadOnlyList<BuildableObject> Buildings
    {
        get
        {
            PruneBuildings();
            return buildings;
        }
    }

    public static IReadOnlyList<IWarehouseFacility> Warehouses
    {
        get
        {
            PruneWarehouses();
            return warehouses;
        }
    }

    public static void RegisterCharacter(CharacterActor actor)
    {
        if (actor == null || characters.Contains(actor))
        {
            return;
        }

        characters.Add(actor);
        BumpCharacters();
    }

    public static void UnregisterCharacter(CharacterActor actor)
    {
        if (actor == null)
        {
            return;
        }

        if (characters.Remove(actor))
        {
            BumpCharacters();
        }
    }

    public static void RegisterWildlife(WildlifeActor actor)
    {
        if (actor == null || wildlife.Contains(actor))
        {
            return;
        }

        wildlife.Add(actor);
        BumpWildlife();
    }

    public static void UnregisterWildlife(WildlifeActor actor)
    {
        if (actor == null)
        {
            return;
        }

        if (wildlife.Remove(actor))
        {
            BumpWildlife();
        }
    }

    public static void RegisterBuilding(BuildableObject building)
    {
        if (building == null || buildings.Contains(building))
        {
            return;
        }

        buildings.Add(building);
        if (building is IWarehouseFacility warehouse)
        {
            RegisterWarehouse(warehouse);
        }

        BumpBuildings();
    }

    public static void UnregisterBuilding(BuildableObject building)
    {
        if (building == null)
        {
            return;
        }

        bool changed = buildings.Remove(building);
        if (building is IWarehouseFacility warehouse && warehouses.Remove(warehouse))
        {
            BumpWarehouses();
        }

        if (changed)
        {
            BumpBuildings();
        }
    }

    public static void RegisterWarehouse(IWarehouseFacility warehouse)
    {
        if (warehouse == null || warehouses.Contains(warehouse))
        {
            return;
        }

        warehouses.Add(warehouse);
        BumpWarehouses();
    }

    public static void UnregisterWarehouse(IWarehouseFacility warehouse)
    {
        if (warehouse == null)
        {
            return;
        }

        if (warehouses.Remove(warehouse))
        {
            BumpWarehouses();
        }
    }

    public static void SetGrid(Grid grid)
    {
        if (ReferenceEquals(cachedGrid, grid))
        {
            return;
        }

        cachedGrid = grid;
        unchecked
        {
            Version++;
        }
    }

    public static bool TryGetGrid(out Grid grid)
    {
        grid = cachedGrid;
        if (grid != null)
        {
            return true;
        }

        GridSystemManager manager = Object.FindFirstObjectByType<GridSystemManager>();
        cachedGrid = manager != null ? manager.grid : null;
        grid = cachedGrid;
        return grid != null;
    }

    public static bool TryGetGameData(out GameData data)
    {
        if (cachedGameManager == null)
        {
            cachedGameManager = Object.FindFirstObjectByType<GameManager>();
        }

        data = cachedGameManager != null ? cachedGameManager.gameData : null;
        return data != null;
    }

    public static void ClearSceneCache()
    {
        characters.Clear();
        wildlife.Clear();
        buildings.Clear();
        warehouses.Clear();
        cachedGrid = null;
        cachedGameManager = null;
        unchecked
        {
            Version++;
            CharacterVersion++;
            WildlifeVersion++;
            BuildingVersion++;
            WarehouseVersion++;
        }
    }

    private static void BumpCharacters()
    {
        unchecked
        {
            Version++;
            CharacterVersion++;
        }
    }

    private static void BumpWildlife()
    {
        unchecked
        {
            Version++;
            WildlifeVersion++;
        }
    }

    private static void BumpBuildings()
    {
        unchecked
        {
            Version++;
            BuildingVersion++;
        }
    }

    private static void BumpWarehouses()
    {
        unchecked
        {
            Version++;
            WarehouseVersion++;
        }
    }

    private static void PruneCharacters()
    {
        for (int i = characters.Count - 1; i >= 0; i--)
        {
            if (characters[i] == null)
            {
                characters.RemoveAt(i);
                BumpCharacters();
            }
        }
    }

    private static void PruneWildlife()
    {
        for (int i = wildlife.Count - 1; i >= 0; i--)
        {
            if (wildlife[i] == null)
            {
                wildlife.RemoveAt(i);
                BumpWildlife();
            }
        }
    }

    private static void PruneBuildings()
    {
        for (int i = buildings.Count - 1; i >= 0; i--)
        {
            BuildableObject building = buildings[i];
            if (building == null || building.isDestroy)
            {
                buildings.RemoveAt(i);
                BumpBuildings();
            }
        }
    }

    private static void PruneWarehouses()
    {
        for (int i = warehouses.Count - 1; i >= 0; i--)
        {
            if (warehouses[i] == null)
            {
                warehouses.RemoveAt(i);
                BumpWarehouses();
            }
        }
    }
}
