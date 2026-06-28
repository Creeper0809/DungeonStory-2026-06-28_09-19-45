using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridTexture : UtilSingleton<GridTexture>
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
    public void DrawBuilding(Dictionary<TilemapLayer, Tile> tiles,Vector2Int selectPos,bool isEven)
    {
        var tilemap = isEven ? buildingTilemapEven : buildingTilemapOdd;
        foreach(var tile in tiles)
        {
            tilemap[tile.Key].SetTile(new Vector3Int(-selectPos.x,3 * selectPos.y),tile.Value);
        }
    }
    public void DeleteBuilding(BuildingSO SO,Vector2Int selectPos)
    {
        var tilemap = SO.width % 2 == 0 ? buildingTilemapEven : buildingTilemapOdd;
        foreach(var key in SO.tiles.Keys)
        {
            tilemap[key].SetTile(new Vector3Int(-selectPos.x, 3 * selectPos.y),null);
        }
    }
    public void DrawWall()
    {
        Grid grid = GridSystemManager.Instance.grid;
        int gridHeight = grid.height;
        int gridWidth = grid.width;

        // 변경된 타일 위치를 추적하기 위한 임시 집합
        HashSet<Vector3Int> updatedTiles = new HashSet<Vector3Int>();

        HashSet<Vector2Int> poses = grid.CheckWallsAndBuildable();

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

                if (grid.GetGridCell(gridPos).HasBuilding())
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
    private void OnEnable()
    {
        GridSystemManager.Instance.onGridObjectChange += DrawWall;
    }
    private void OnDisable()
    {
        GridSystemManager.Instance.onGridObjectChange -= DrawWall;
    }
}
