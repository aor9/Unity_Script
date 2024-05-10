using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 길찾기 클래스
public class PathFinder
{
    public List<OverlayTile> FindPath(OverlayTile start, OverlayTile end, List<OverlayTile> searchableTiles, CharacterInfo character)
    {
        List<OverlayTile> openList = new List<OverlayTile>();
        List<OverlayTile> closedList = new List<OverlayTile>();

        openList.Add(start);

        while (openList.Count > 0)
        {
            OverlayTile currentOverlayTile = openList.OrderBy(x => x.F).First();

            openList.Remove(currentOverlayTile);
            closedList.Add(currentOverlayTile);

            if (currentOverlayTile == end)
            {
                return GetFinishedList(start, end);
            }

            if (searchableTiles == null)
            {
                Debug.Log("null 값이 들어왔다고합니다.");
            }
            var Tiles = MapManager.Instance.GetSurroundingTiles(currentOverlayTile, searchableTiles);
            
            foreach (var tile in Tiles)
            {
                if (tile.isBlocked || closedList.Contains(tile) || Mathf.Abs(currentOverlayTile.transform.position.z - tile.transform.position.z) > 1)
                {
                    continue;
                }

                if (character.transform.gameObject.CompareTag("Enemy") && tile.isUnitOn && tile != end)
                {
                    continue;
                }

                tile.G = GetManhattenDistance(start, tile);
                tile.H = GetManhattenDistance(end, tile);
                tile.previous = currentOverlayTile;

                if (!openList.Contains(tile))
                {
                    openList.Add(tile);
                }
            }
        }

        return new List<OverlayTile>();
    }
    private List<OverlayTile> GetFinishedList(OverlayTile start, OverlayTile end)
    {
        List<OverlayTile> finishedList = new List<OverlayTile>();
        OverlayTile currentTile = end;

        while (currentTile != start)
        {
            finishedList.Add(currentTile);
            currentTile = currentTile.previous;
        }

        finishedList.Reverse();

        return finishedList;
    }

    private int GetManhattenDistance(OverlayTile start, OverlayTile tile)
    {
        return Mathf.Abs(start.gridLocation.x - tile.gridLocation.x) + Mathf.Abs(start.gridLocation.y - tile.gridLocation.y);
    }
}
