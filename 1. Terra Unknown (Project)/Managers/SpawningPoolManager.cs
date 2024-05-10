using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 스포너 관리 클래스
public class SpawningPoolManager : MonoBehaviour
{
    private int numOfChild;
    private static SpawningPoolManager _instance;
    
    public static SpawningPoolManager Instance
    {
        get { return _instance; }
    }
    public int spawnCount = 0;
    
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

    public void CallSpawn()
    {
        
        numOfChild = this.transform.childCount;
        for (int i = 0; i < numOfChild; i++)
        {
            transform.GetChild(i).GetComponent<SpawningPool>().Spawn();
        }
        /*if (spawnCount == 4)
        {
            numOfChild = this.transform.childCount;
            for (int i = 0; i < numOfChild; i++)
            {
                transform.GetChild(i).GetComponent<SpawningPool>().Spawn();
            }
            
            ExpandSpawningPool();
            spawnCount = 0;
        }
        else
        {
            spawnCount++;
        }*/
    }

    public void ExpandSpawningPool()
    {
        numOfChild = this.transform.childCount;
        for (int i = 0; i < numOfChild; i++)
        {
            transform.GetChild(i).GetComponent<SpawningPool>().Expand();
        }
    }

    public bool CheckIsSpawningPool(Vector2Int clickedTileLocation)
    {
        OverlayTile overlayTile;
        for (int i = 0; i < numOfChild; i++)
        {
            SpawningPool spawningPool = new SpawningPool();
            transform.GetChild(i).TryGetComponent(out spawningPool);
            
            if (spawningPool.baseTile2DLocation == clickedTileLocation)
            {
                DestroySpawningPool(clickedTileLocation, spawningPool);
                return true;
            }
        }

        return false;
    }
    
    public void DestroySpawningPool(Vector2Int clickedTileLocation, SpawningPool spawningPool)
    {
        numOfChild = this.transform.childCount;
        OverlayTile overlayTile;

        MapManager.Instance.map.TryGetValue(new Vector3Int(clickedTileLocation.x, clickedTileLocation.y, 0),
            out overlayTile);
        overlayTile.isBlocked = false;
        Destroy(spawningPool.gameObject);
    }
    

    public bool IsPlayerOnSpawner(CharacterInfo player)
    {
        numOfChild = this.transform.childCount;
        for (int i = 0; i < numOfChild; i++)
        {
            SpawningPool spawningPool = new SpawningPool();
            transform.GetChild(i).TryGetComponent(out spawningPool);
            
            foreach (var tile in spawningPool.SpawnTiles)
            {
                if (player.standingOnTile == tile)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
