using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 범위 찾는 클래스 (공격, 이동 범위)
public class RangeFinder
{
    public List<OverlayTile> GetTilesInRange(OverlayTile startingTile, int range)
    {
        var inRangeTiles = new List<OverlayTile>();
        int stepCount = 0;

        inRangeTiles.Add(startingTile);
        
        var tilesForPreviousStep = new List<OverlayTile>();
        tilesForPreviousStep.Add(startingTile);
        
        while (stepCount < range)
        {
            var surroundingTiles = new List<OverlayTile>();

            foreach (var item in tilesForPreviousStep)
            {
                if (item.isBlocked && item.isSpawnerOn is false)
                {
                    continue;
                }
                
                surroundingTiles.AddRange(MapManager.Instance.GetSurroundingTiles(item, new List<OverlayTile>()));
            }

            inRangeTiles.AddRange(surroundingTiles);
            tilesForPreviousStep = surroundingTiles.Distinct().ToList();
            stepCount++;
        }
        
        return inRangeTiles.Distinct().ToList();
    }

    public List<OverlayTile> GetTilesByCoordinate(OverlayTile startingTile, int range, string type)
    {
        var inRangeTiles = new List<OverlayTile>();
        var _map = MapManager.Instance.map;
        
        inRangeTiles.Add(startingTile);

        if (type == "cross")//대각선 범위
        {
            var crossTiles = new List<OverlayTile>();
            
            for (int i = 0; i < range; i++)
            {
                var keys = new List<Vector3Int>();
                keys.Add(new Vector3Int(startingTile.gridLocation.x + i + 1, startingTile.gridLocation.y, startingTile.gridLocation.z));
                keys.Add(new Vector3Int(startingTile.gridLocation.x - i - 1, startingTile.gridLocation.y, startingTile.gridLocation.z));
                keys.Add(new Vector3Int(startingTile.gridLocation.x, startingTile.gridLocation.y + i + 1, startingTile.gridLocation.z));
                keys.Add(new Vector3Int(startingTile.gridLocation.x, startingTile.gridLocation.y - i - 1, startingTile.gridLocation.z));

                foreach (var key in keys)
                {
                    if (_map.ContainsKey(key))
                    {
                        crossTiles.Add(_map[key]);
                    }
                }
            }
            
            inRangeTiles.AddRange(crossTiles);
        } 
        else if (type == "diagonal")//수직 범위
        {
            var diagonalTiles = new List<OverlayTile>();
            
            for (int i = 0; i < range; i++)
            {
                var keys = new List<Vector3Int>();
                keys.Add(new Vector3Int(startingTile.gridLocation.x - i - 1, startingTile.gridLocation.y + i + 1, startingTile.gridLocation.z));
                keys.Add(new Vector3Int(startingTile.gridLocation.x + i + 1, startingTile.gridLocation.y + i + 1, startingTile.gridLocation.z));
                keys.Add(new Vector3Int(startingTile.gridLocation.x - i - 1, startingTile.gridLocation.y - i - 1, startingTile.gridLocation.z));
                keys.Add(new Vector3Int(startingTile.gridLocation.x + i + 1, startingTile.gridLocation.y - i - 1, startingTile.gridLocation.z));

                foreach (var key in keys)
                {
                    if (_map.ContainsKey(key))
                    {
                        diagonalTiles.Add(_map[key]);
                    }
                }
            }
            
            inRangeTiles.AddRange(diagonalTiles);
        }
        else if (type == "square")//사각형 범위
        {
            var squareTiles = new List<OverlayTile>();
            var keys = new List<Vector3Int>();
            keys.Add(new Vector3Int(startingTile.gridLocation.x, startingTile.gridLocation.y, startingTile.gridLocation.z));//캐릭터 위치
            for (int i = startingTile.gridLocation.x - (range - 1); i <= startingTile.gridLocation.x + (range - 1); i++)
            {
                for (int j = startingTile.gridLocation.y - (range - 1); j <= startingTile.gridLocation.y + (range - 1); j++)
                {
                    keys.Add(new Vector3Int(i, j, startingTile.gridLocation.z));
                    if (_map.ContainsKey(new Vector3Int(i, j, startingTile.gridLocation.z)))
                    {
                        squareTiles.Add(_map[new Vector3Int(i, j, startingTile.gridLocation.z)]);
                    }
                }
            }

            inRangeTiles.AddRange(squareTiles);
        }
        return inRangeTiles.Distinct().ToList();
    }
}
