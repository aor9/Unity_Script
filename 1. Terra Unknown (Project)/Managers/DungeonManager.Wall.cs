using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;
using Random = System.Random;

public partial class DungeonManager
{
    private List<OverlayTile> baseWallTileList = new List<OverlayTile>();
    private List<OverlayTile> wallTileList = new List<OverlayTile>();
    private List<OverlayTile> connectedTileList = new List<OverlayTile>();
    
    Random random = new Random();
    
    private void GenerateWall()
    {
        GenerateBaseWall();
    }

    // 기본 벽에 이어지는 벽 생성
    private void GenerateConnectingWall()
    {
        OverlayTile currentWall;
        OverlayTile wall;
        Vector3Int nextPosition = new Vector3Int();
        
        foreach (var baseWall in baseWallTileList)
        {
            connectedTileList.Clear();
            connectedTileList.Add(baseWall);
            currentWall = baseWall;
            int connectingWallCount = random.Next(2, 4);
            int i = 0;
            
            //TODO: 무한루프 위험이 있을 수 있음
            while (i < connectingWallCount)
            {
                int direction = random.Next(1, 5);
                switch (direction)
                {
                    case 1:
                        nextPosition = new Vector3Int(currentWall.gridLocation.x + 1, currentWall.gridLocation.y, currentWall.gridLocation.z);
                        break;
                    case 2:
                        nextPosition = new Vector3Int(currentWall.gridLocation.x - 1, currentWall.gridLocation.y, currentWall.gridLocation.z);
                        break;
                    case 3:
                        nextPosition = new Vector3Int(currentWall.gridLocation.x, currentWall.gridLocation.y + 1, currentWall.gridLocation.z);
                        break;
                    case 4:
                        nextPosition = new Vector3Int(currentWall.gridLocation.x, currentWall.gridLocation.y - 1, currentWall.gridLocation.z);
                        break;
                }
                
                if (nextPosition.x < -4 && nextPosition.y < -4) continue;

                if (MapManager.Instance.map.TryGetValue(nextPosition, out wall) && spawnTileList.Contains(wall) is false)
                {
                    if (!IsWallInRange(wall))
                    {
                        SetWall(wall, false);
                        currentWall = wall;
                        i++;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }
    }

    public bool IsWallInRange(OverlayTile wall)
    {
        List<OverlayTile> surrondingTiles = new List<OverlayTile>();
        surrondingTiles = rangeFinder.GetTilesByCoordinate(wall, 1, "cross");
        bool isWallInRange = surrondingTiles.Except(connectedTileList).Any(x => wallTileList.Contains(x));

        if (isWallInRange)
        {
            return true;
        }
        
        return false;
    }

    // 기본 뼈대가 되는 벽 생성
    private void GenerateBaseWall()
    {
        // TODO: 맵 타입에 따라서 벽 생성 규칙이 다르면 수정
        int baseWallCount = 0;
        
        foreach (var tile in MapManager.Instance.map.Values)
        {
            if (baseWallCount == 10) break;
            if (spawnTileList.Contains(tile) || tile.grid2DLocation.x < -4 && tile.grid2DLocation.y < -4) continue;
            
            // 아군 스폰 지역일 때 벽 생성 안하게 하기
            float probability = random.Next(0, 101);
            bool check = false;
            
            if (probability <= 5 && baseWallCount < 10)
            {
                List<OverlayTile> surrondingTiles = new List<OverlayTile>();
                surrondingTiles = rangeFinder.GetTilesByCoordinate(tile, 3, "square");
                bool isBaseWallContained = surrondingTiles.Any(x => baseWallTileList.Contains(x));

                if (isBaseWallContained is false)
                {
                    SetWall(tile, true);
                    baseWallCount++;
                }
                else
                {
                    check = true;
                }
            }
            else
            {
                if (check is true)
                {
                    List<OverlayTile> surrondingTiles = new List<OverlayTile>();
                    surrondingTiles = rangeFinder.GetTilesInRange(tile, 2);
                    bool isBaseWallContained = surrondingTiles.Any(x => baseWallTileList.Contains(x));

                    if (isBaseWallContained is false)
                    {
                        SetWall(tile, true);
                        baseWallCount++;
                    }
                    
                    check = false;
                }
            }
        }
        
        GenerateConnectingWall();
    }

    private void SetWall(OverlayTile tile, bool isBaseWall)
    {
        if (isBaseWall is true)
        {
            baseWallTileList.Add(tile);
        }
        else
        {
            connectedTileList.Add(tile);
        }
        
        Tile wallTile = Managers.Resource.Load<Tile>($"Img/map4/stalactite");
        wallTilemap.SetTile(tile.gridLocation, wallTile);
        wallTileList.Add(tile);
        
        OverlayTile floorTile = MapManager.Instance.map[new Vector3Int(tile.gridLocation.x, tile.gridLocation.y, 0)];
        floorTile.isBlocked = true;
    }
}
