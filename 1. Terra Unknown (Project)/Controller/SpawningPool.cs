using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor.Overlays;
using UnityEditor.PackageManager;
using UnityEngine;
using Random = UnityEngine.Random;

// 스포너 관련 클래스
public class SpawningPool : MonoBehaviour
{
    [SerializeField] private int monsterCount = 0;
    
    [SerializeField] private int keepMonsterCount = 0;

    [SerializeField] private float spawnTurn = 3;
    
    private CharacterInfo enemy;
    private RangeFinder rangeFinder;
    private OverlayTile spawnTile;
    private Dictionary<Vector3Int, OverlayTile> map;
    private BattleSystem battleSystem;
    private CharacterInfo escapeway;
    private Transform enemyRoot;
    
    public OverlayTile BaseTile { get; set; }
    public List<OverlayTile> SpawnTiles { get; set; }
    public int SpawnRange { get; set; }
    public GameObject battleSystemObject;
    public Vector3Int baseTileLocation;
    public Vector2Int baseTile2DLocation;


    void Start()
    {
        SpawnTiles = new List<OverlayTile>();
        rangeFinder = new RangeFinder();
        GameObject.Find("@BattleSystem").TryGetComponent(out battleSystem);
        map = MapManager.Instance.map;
        enemyRoot = GameObject.Find("@Enemy_Root").transform;
    }

    public void Spawn()
    {
        if (SpawnTiles.Count == 0)
        {
            SpawnRange = 2;
            SpawnTiles = rangeFinder.GetTilesInRange(BaseTile, SpawnRange);
        }
        
        if (monsterCount < keepMonsterCount)
        {
            int random = Random.Range(0, SpawnTiles.Count);
            spawnTile = SpawnTiles[random];
            
            if (!checkOverlapping(spawnTile))
            {
                // 현재는 몬스터가 blue slime 만 있어서 blue slime 만 소환함.
                // 나중에는 현재 맵에서 소환할 몬스터들을 받아서 랜덤 생성하던 다른 방식을 사용해아함.
                enemy = Managers.Resource.Instantiate("enemy/Skeleton").GetComponent<CharacterInfo>();
                enemy.transform.parent = enemyRoot;
                PositionEnemyOnTile(spawnTile, enemy);
                
                // TODO: enemy.transform.GetChild(0).GetComponent<DOTweenAnimation>(); 로 수정해야함.
                // TODO: 블루 슬라임을 제외하면 BODY에 애니메이션을 적용해야 적용되기때문.
                DOTweenAnimation turnOrder = enemy.transform.GetComponent<DOTweenAnimation>();
                turnOrder.DORestartById("FadeIn");
                
                battleSystem.TurnOrderList.Add(enemy);
                monsterCount++;
            }
            else
            {
                // 재귀 호출로 인한 무한 루프의 위험이 있음.
                // 예를 들어 스폰 타일 위가 꽉찬 상황에서 스폰을 하게 될 때
                Spawn();
            }
        }
    }
    

    public void Expand()
    {
        SpawnRange++;
    }

    bool checkOverlapping(OverlayTile spawnTile)
    {
        foreach (var enemy in MapManager.Instance.EnemyList)
        {
            // 임의로 넣은 -1, -1 삭제 해야함.
            if (enemy.standingOnTile == spawnTile || spawnTile.grid2DLocation == BaseTile.grid2DLocation)
            {
                return true;
            }
        }
        
        return false;
    }
    
    private void PositionEnemyOnTile(OverlayTile tile, CharacterInfo enemy)
    {
        enemy.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z + 2);
        enemy.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
        enemy.standingOnTile = tile;
    }
    
}
