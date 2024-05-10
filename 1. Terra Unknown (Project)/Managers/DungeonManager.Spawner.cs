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
    private List<OverlayTile> spawnRange = new List<OverlayTile>();
    private List<OverlayTile> spawnTileList = new List<OverlayTile>();
    private List<OverlayTile> spawnerTileList = new List<OverlayTile>();
    
    private void GenerateSpawner()
    {
        List<int> sections = new List<int>();
        Random random = new Random();

        // 랜덤한 위치에 spawner 3개 추가
        while (sections.Count < 3)
        {
            int randomSection = random.Next(0, 9);
            if (randomSection != 6 && sections.Contains(randomSection) is false &&
                IsNotAdjacent(sections, randomSection) is true)
            {
                sections.Add(randomSection);
            }
        }

        foreach (int section in sections)
        {
            OverlayTile spawnTile;
            GameObject basicSpawner = Managers.Resource.Instantiate("object/BasicSpawner");
            Managers.UI.MakeWorldSpaceUI<UI_HPBar>(basicSpawner.transform);
            SpawningPool spawningPool = basicSpawner.GetComponent<SpawningPool>();
            spawningPool.transform.parent = spawningPoolRoot;
            
            switch (section)
            {
                case 0:
                    MapManager.Instance.map.TryGetValue(new Vector3Int(-6, 4, 0), out spawnTile);
                    SetSpawner(spawningPool, spawnTile);
                    break;
                case 1:
                    MapManager.Instance.map.TryGetValue(new Vector3Int(-1, 4, 0), out spawnTile);
                    SetSpawner(spawningPool, spawnTile);
                    break;
                case 2:
                    MapManager.Instance.map.TryGetValue(new Vector3Int(4, 4, 0), out spawnTile);
                    SetSpawner(spawningPool, spawnTile);
                    break;
                case 3:
                    MapManager.Instance.map.TryGetValue(new Vector3Int(-6, -1, 0), out spawnTile);
                    SetSpawner(spawningPool, spawnTile);
                    break;
                case 4:
                    MapManager.Instance.map.TryGetValue(new Vector3Int(-1, -1, 0), out spawnTile);
                    SetSpawner(spawningPool, spawnTile);
                    break;
                case 5:
                    MapManager.Instance.map.TryGetValue(new Vector3Int(4, -1, 0), out spawnTile);
                    SetSpawner(spawningPool, spawnTile);
                    break;
                case 7:
                    MapManager.Instance.map.TryGetValue(new Vector3Int(-1, -6, 0), out spawnTile);
                    SetSpawner(spawningPool, spawnTile);
                    break;
                case 8:
                    MapManager.Instance.map.TryGetValue(new Vector3Int(4, -6, 0), out spawnTile);
                    SetSpawner(spawningPool, spawnTile);
                    break;
                default:
                    Debug.Log("스포너 생성 에러");
                    break;
            }
        }
    }
    
    private bool IsNotAdjacent(List<int> sections, int newSection)
    {
        foreach (int section in sections)
        {
            if (Math.Abs(section - newSection) == 1 || Math.Abs(section - newSection) == 3)
            {
                return false;
            }
        }
        return true;
    }

    private void SetSpawner(SpawningPool spawner, OverlayTile tile)
    {
        spawnerTileList.Add(tile);
        spawner.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z + 2);
        
        Tile spawnerTile = Managers.Resource.Load<Tile>($"Img/map3/spawner");
        wallTilemap.SetTile(tile.gridLocation, spawnerTile);

        SetSpawnerInfo(spawner, tile);
        
        spawnRange = rangeFinder.GetTilesInRange(tile, 2);
        spawnTileList.AddRange(spawnRange);
        
        Random random = new Random();
        
        foreach (var spawnTile in spawnRange)
        {
            int distance = Mathf.Abs(tile.gridLocation.x - spawnTile.gridLocation.x) + Mathf.Abs(tile.gridLocation.y - spawnTile.gridLocation.y);
            if (distance == 1)
            {
                floorTilemap.SetTile(spawnTile.gridLocation, darkWaterTile);
            }   
            else
            {
                floorTilemap.SetTile(spawnTile.gridLocation, darkWaterTile);
            }
        }
        
        // 오브젝트 아래 있는 타일 isBlocked 속성을 true로 설정
        OverlayTile floorTile = MapManager.Instance.map[new Vector3Int(tile.gridLocation.x, tile.gridLocation.y, 0)];
        floorTile.isBlocked = true;
        floorTile.isSpawnerOn = true;
        
        spawner.BaseTile = tile;
        spawner.baseTileLocation = tile.gridLocation;
        spawner.baseTile2DLocation = tile.grid2DLocation;
    }

    private void SetSpawnerInfo(SpawningPool spawner, OverlayTile tile)
    {
        CharacterInfo spawnerInfo;
        spawner.gameObject.TryGetComponent<CharacterInfo>(out spawnerInfo);
        spawnerInfo.standingOnTile = tile;
        spawnerInfo.hp = 10;
        spawnerInfo.maxHp = 10;
        SpawnerList.Add(spawnerInfo);
    }
}
