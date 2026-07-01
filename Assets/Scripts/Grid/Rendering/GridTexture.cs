using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DrawWithUnity]
public class GridTexture : SerializedMonoBehaviour, IGridBuildingVisual
{
    public enum TilemapLayer
    {
        HALLWAY,
        FRONT,
        MIDDLE,
        BACK
    }
    public Tilemap wallTilemap;
    public Dictionary<TilemapLayer, Tilemap> buildingTilemapEven;
    public Dictionary<TilemapLayer, Tilemap> buildingTilemapOdd;
    public Tile wall;
    public Tile floor;
    private readonly GridWallTileCalculator wallTileCalculator = new GridWallTileCalculator();
    private readonly Dictionary<Sprite, Tile> spriteTiles = new Dictionary<Sprite, Tile>();

    private static GridTexture instance;

    public static GridTexture Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GridTexture>();
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (!Application.isPlaying) return;

        instance = this;
    }

    public void DrawBuilding(Dictionary<TilemapLayer, Tile> tiles,Vector2Int selectPos,bool isEven)
    {
        if (tiles == null || tiles.Count == 0) return;

        foreach(var tile in tiles)
        {
            if (tile.Value == null || !TryGetTilemap(tile.Key, isEven, out Tilemap targetTilemap)) continue;

            targetTilemap.SetTile(GetTilePosition(selectPos),tile.Value);
        }
    }

    public void DrawBuilding(BuildingSO buildingData, Vector2Int selectPos)
    {
        if (buildingData == null) return;

        if (HasTileVisual(buildingData))
        {
            DrawBuilding(buildingData.tiles, selectPos, buildingData.IsEvenWidth);
            return;
        }

        DrawSpriteBuilding(buildingData, selectPos);
    }

    public void DeleteBuilding(Dictionary<TilemapLayer, Tile> tiles,Vector2Int selectPos,bool isEven)
    {
        if (tiles == null || tiles.Count == 0) return;

        foreach(var key in tiles.Keys)
        {
            if (!TryGetTilemap(key, isEven, out Tilemap targetTilemap)) continue;

            targetTilemap.SetTile(GetTilePosition(selectPos),null);
        }
    }

    public void DeleteBuilding(BuildingSO buildingData, Vector2Int selectPos)
    {
        if (buildingData == null) return;

        if (HasTileVisual(buildingData))
        {
            DeleteBuilding(buildingData.tiles, selectPos, buildingData.IsEvenWidth);
            return;
        }

        DeleteSpriteBuilding(buildingData, selectPos);
    }

    private void DrawSpriteBuilding(BuildingSO buildingData, Vector2Int selectPos)
    {
        if (buildingData.sprite == null) return;
        if (!TryGetTilemap(GetSpriteTilemapLayer(buildingData), buildingData.IsEvenWidth, out Tilemap targetTilemap)) return;

        targetTilemap.SetTile(GetTilePosition(selectPos), GetSpriteTile(buildingData.sprite));
    }

    private void DeleteSpriteBuilding(BuildingSO buildingData, Vector2Int selectPos)
    {
        if (buildingData.sprite == null) return;
        if (!TryGetTilemap(GetSpriteTilemapLayer(buildingData), buildingData.IsEvenWidth, out Tilemap targetTilemap)) return;

        targetTilemap.SetTile(GetTilePosition(selectPos), null);
    }

    private Tile GetSpriteTile(Sprite sprite)
    {
        if (!spriteTiles.TryGetValue(sprite, out Tile tile) || tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = $"{sprite.name}_RuntimeTile";
            tile.sprite = sprite;
            spriteTiles[sprite] = tile;
        }

        return tile;
    }

    private bool TryGetTilemap(TilemapLayer layer, bool isEven, out Tilemap targetTilemap)
    {
        targetTilemap = null;
        Dictionary<TilemapLayer, Tilemap> tilemaps = isEven ? buildingTilemapEven : buildingTilemapOdd;
        return tilemaps != null
            && tilemaps.TryGetValue(layer, out targetTilemap)
            && targetTilemap != null;
    }

    private static bool HasTileVisual(BuildingSO buildingData)
    {
        return buildingData.tiles != null && buildingData.tiles.Count > 0;
    }

    private static TilemapLayer GetSpriteTilemapLayer(BuildingSO buildingData)
    {
        GridBuildingPlacement placement = buildingData.Placement;
        return placement.Layer == GridLayer.Hallway || placement.IsMovement
            ? TilemapLayer.HALLWAY
            : TilemapLayer.BACK;
    }

    private static Vector3Int GetTilePosition(Vector2Int selectPos)
    {
        return new Vector3Int(-selectPos.x, 3 * selectPos.y, 0);
    }

    public void DrawWall(Grid grid)
    {
        if (grid == null || wallTilemap == null) return;

        int gridHeight = grid.height;
        int gridWidth = grid.width;

        // 변경된 타일 위치를 추적하기 위한 임시 집합
        HashSet<Vector3Int> updatedTiles = new HashSet<Vector3Int>();

        HashSet<Vector2Int> poses = wallTileCalculator.GetWallTilePositions(grid);

        // 변경된 타일만 업데이트하기 위한 임시 타일맵 생성
        var tempTilemap = new HashSet<Vector3Int>();

        // 현재 벽 타일맵에서 모든 타일 위치를 임시 타일맵에 추가
        foreach (var pos in wallTilemap.cellBounds.allPositionsWithin)
        {
            if (wallTilemap.HasTile(pos) && wallTilemap.GetTile(pos) == wall)
            {
                tempTilemap.Add(pos);
            }
        }

        // 새로운 벽 위치 설정
        foreach (Vector2Int pos in poses)
        {
            Vector3Int tilePos = new Vector3Int(-pos.x, pos.y, 0);
            if (!tempTilemap.Contains(tilePos))
            {
                wallTilemap.SetTile(tilePos, wall);
            }
            tempTilemap.Remove(tilePos);
        }

        // 기존에 있던 타일 중에서 더 이상 필요 없는 타일 제거
        foreach (var pos in tempTilemap)
        {
            wallTilemap.SetTile(pos, null);
        }

        // 바닥 타일 업데이트 최적화
        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                Vector2Int gridPos = new Vector2Int(j, i);
                Vector3Int tilePos = new Vector3Int(-j, i*3 + 2, 0);
                // 이전에 업데이트된 타일은 건너뜀
                if (updatedTiles.Contains(tilePos)) continue;

                if (grid.GetGridCell(gridPos).HasOccupant())
                {
                    wallTilemap.SetTile(tilePos, floor);
                    // 업데이트된 타일 집합에 추가
                    updatedTiles.Add(tilePos);
                }
                else if (wallTilemap.GetTile(tilePos) == floor)
                {
                    // 바닥 타일이지만 현재 건물이 없는 경우, 타일 제거
                    wallTilemap.SetTile(tilePos, null);
                    updatedTiles.Add(tilePos);
                }
            }
        }

    }
}

public class GridWallTileCalculator
{
    private readonly int cellTileHeight;

    public GridWallTileCalculator(int cellTileHeight = 3)
    {
        this.cellTileHeight = cellTileHeight;
    }

    public HashSet<Vector2Int> GetWallTilePositions(Grid grid)
    {
        HashSet<Vector2Int> wallTilePositions = new HashSet<Vector2Int>();
        if (grid == null) return wallTilePositions;

        Vector2Int[] horizontalDirections = { Vector2Int.left, Vector2Int.right };
        foreach (GridCell cell in grid.GetCells())
        {
            if (cell.HasOccupant()) continue;

            foreach (Vector2Int direction in horizontalDirections)
            {
                Vector2Int nextPos = direction + cell.Position;
                if (!grid.IsValidGridPos(nextPos) || !grid.GetGridCell(nextPos).HasOccupant()) continue;

                Vector2Int floor = new Vector2Int(cell.Position.x, cell.Position.y * cellTileHeight);
                for (int y = 0; y < cellTileHeight; y++)
                {
                    wallTilePositions.Add(floor + (Vector2Int.up * y));
                }
                break;
            }
        }

        return wallTilePositions;
    }
}
