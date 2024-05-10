using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class MapManager : MonoBehaviour
{
    private static MapManager _instance;
    private BattleSystem battleSystem;
    private List<Vector2Int> wallCoords;
    private Transform root;
    
    public static MapManager Instance
    {
        get { return _instance; }
    }

    public OverlayTile overlayTilePrefab;
    public GameObject overlayContainer;
    public Dictionary<Vector3Int, OverlayTile> map;
    public GameObject enemyPrefab;
    public GameObject battleSystemObject;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    void Start()
    {
        var tileMap = transform.GetChild(0).GetComponent<Tilemap>();
        var wallMap = transform.GetChild(1).GetComponent<Tilemap>();
        
        map = new Dictionary<Vector3Int, OverlayTile>();
        wallCoords = new List<Vector2Int>();
        overlayContainer = GameObject.Find("OverlayContainer");
        
        // wallmap에 overlaytile 배치
        //TODO: 부서지는벽, 안부서지는 벽을 속성으로 나눠야함.
        BoundsInt bounds = wallMap.cellBounds;

        for (int z = bounds.max.z; z >= bounds.min.z; z--)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                for (int x = bounds.min.x; x < bounds.max.x; x++)
                {
                    var tileLocation = new Vector3Int(x, y, z);
                    var tileKey = new Vector3Int(x, y, z);
                    
                    if (wallMap.HasTile(tileLocation) && !map.ContainsKey(tileKey))
                    {
                        var overlayTile = Instantiate(overlayTilePrefab, overlayContainer.transform);
                        var cellWorldPosition = wallMap.GetCellCenterWorld(tileLocation);
                        
                        overlayTile.transform.position = new Vector3(cellWorldPosition.x, cellWorldPosition.y, cellWorldPosition.z + 0.0001f);
                        overlayTile.gridLocation = tileLocation;
                        overlayTile.isBlocked = true;
                        wallCoords.Add(new Vector2Int(tileLocation.x, tileLocation.y));

                        map.Add(tileKey, overlayTile);
                    }
                }
            }
        }
        
        // map 바닥에 overlaytile을 배치
        bounds = tileMap.cellBounds;

        for (int z = bounds.max.z; z >= bounds.min.z; z--)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                for (int x = bounds.min.x; x < bounds.max.x; x++)
                {
                    var tileLocation = new Vector3Int(x, y, z);
                    var tileKey = new Vector3Int(x, y, z);
                    if (tileMap.HasTile(tileLocation) && !map.ContainsKey(tileKey))
                    {
                        var overlayTile = Instantiate(overlayTilePrefab, overlayContainer.transform);
                        var cellWorldPosition = tileMap.GetCellCenterWorld(tileLocation);
                        
                        overlayTile.transform.position = new Vector3(cellWorldPosition.x, cellWorldPosition.y, cellWorldPosition.z + 0.0001f);
                        overlayTile.gridLocation = tileLocation;
                        // sorting order를 벽이냐 아니냐에 따라 나눠놓음 -> 캐릭터가 이동할때 맵이랑 sorting order가 같으면 겹쳐서 보이는 버그 때문에
                        if (overlayTile.gridLocation.z > 0)
                        {
                            overlayTile.GetComponent<SpriteRenderer>().sortingOrder = tileMap.GetComponent<TilemapRenderer>().sortingOrder;
                            overlayTile.isBlocked = true;
                            wallCoords.Add(new Vector2Int(tileLocation.x, tileLocation.y));
                        }
                        else
                        {
                            overlayTile.GetComponent<SpriteRenderer>().sortingOrder = tileMap.GetComponent<TilemapRenderer>().sortingOrder;
                        }
                        map.Add(tileKey, overlayTile);
                    }
                }
            }
        }
        
        // blocked 타일 아래 타일들의 속성 관리
        foreach (var coord in wallCoords)
        {
            OverlayTile overlayTile;
            map.TryGetValue(new Vector3Int(coord.x, coord.y, 0), out overlayTile);
            if (overlayTile is not null)
            {
                overlayTile.isBlocked = true;
            }
        }
    }
    
    private void PositionStructureOnTile(OverlayTile tile, CharacterInfo structure)
    {
        structure.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z + 2);
        structure.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
        structure.standingOnTile = tile;
    }
    
    // 특정 타일을 기준으로 주변 타일을 탐색하여 리스트로 반환하는 메소드
    public List<OverlayTile> GetSurroundingTiles(OverlayTile currentOverlayTile, List<OverlayTile> searchableTiles)
    {
        Dictionary<Vector3Int, OverlayTile> tileToSearch = new Dictionary<Vector3Int, OverlayTile>();

        if (searchableTiles.Count > 0)
        {
            foreach (var item in searchableTiles)
            {
                tileToSearch.Add(item.gridLocation, item);
            }
        }
        else
        {
            tileToSearch = map;
        }
        
        List<OverlayTile> surroundingTiles = new List<OverlayTile>();

        
        //TODO: 맵 위의 OBJECT(z 값이 1 이상인 것들)에 관한 것들을 처리하는 방안 마련
        //top
        Vector3Int locationToCheck = new Vector3Int(
            currentOverlayTile.gridLocation.x,
            currentOverlayTile.gridLocation.y + 1,
            currentOverlayTile.gridLocation.z
        );
        if (tileToSearch.ContainsKey(locationToCheck))
        {
            if (Mathf.Abs(currentOverlayTile.gridLocation.z - tileToSearch[locationToCheck].gridLocation.z) <= 1)
            {
                if(checkOverlapping(surroundingTiles, tileToSearch[locationToCheck]) == false)
                    surroundingTiles.Add(tileToSearch[locationToCheck]);   
            }
        }
        //bottom
        locationToCheck = new Vector3Int(
            currentOverlayTile.gridLocation.x,
            currentOverlayTile.gridLocation.y - 1,
            currentOverlayTile.gridLocation.z
        );
        if (tileToSearch.ContainsKey(locationToCheck))
        {
            if(Mathf.Abs(currentOverlayTile.gridLocation.z - tileToSearch[locationToCheck].gridLocation.z) <= 1)
            {
                if(checkOverlapping(surroundingTiles, tileToSearch[locationToCheck]) == false)
                    surroundingTiles.Add(tileToSearch[locationToCheck]);   
            }
        }
        //right
        locationToCheck = new Vector3Int(
            currentOverlayTile.gridLocation.x + 1,
            currentOverlayTile.gridLocation.y,
            currentOverlayTile.gridLocation.z
        );
        if (tileToSearch.ContainsKey(locationToCheck))
        {
            if(Mathf.Abs(currentOverlayTile.gridLocation.z - tileToSearch[locationToCheck].gridLocation.z) <= 1)
            {
                if(checkOverlapping(surroundingTiles, tileToSearch[locationToCheck]) == false)
                    surroundingTiles.Add(tileToSearch[locationToCheck]);   
            }
        }
        //left
        locationToCheck = new Vector3Int(
            currentOverlayTile.gridLocation.x - 1,
            currentOverlayTile.gridLocation.y,
            currentOverlayTile.gridLocation.z
        );
        if (tileToSearch.ContainsKey(locationToCheck))
        {
            if(Mathf.Abs(currentOverlayTile.gridLocation.z - tileToSearch[locationToCheck].gridLocation.z) <= 1)
            {
                if(checkOverlapping(surroundingTiles, tileToSearch[locationToCheck]) == false)
                    surroundingTiles.Add(tileToSearch[locationToCheck]);   
            }
        }
        
        return surroundingTiles;
    }
    
    public bool checkOverlapping(List<OverlayTile> surroundingTiles, OverlayTile checkTile)
    {
        foreach (var tile in surroundingTiles)
        {
            if (tile.grid2DLocation == checkTile.grid2DLocation)
            {
                if (battleSystem.actionState == ActionState.MOVE)
                {
                    surroundingTiles.Remove(tile);
                }
                return true;
            }
        }

        return false;
    }
}
