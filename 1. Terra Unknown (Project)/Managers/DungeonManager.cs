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

public partial class DungeonManager : MonoBehaviour
{
    [SerializeField] private BattleSystem battleSystem;
    public TraitEffectManager traitEffectManager;
    
    private DropObject dropObject;
    private MercenaryList squadList;
    private int ActionCost { get; set; } = 0;
    private OverlayTile[] spawnTiles = new OverlayTile[4];
    private GameObject[] squadPrefabs = new GameObject[4];
    private Transform mercenaryRoot;
    private Transform enemyRoot;
    private Transform spawningPoolRoot;
    private string jsonPath;
    private List<List<OverlayTile>> splitedMaps;
    private GameObject wallObject;
    private Tilemap wallTilemap;
    private Tilemap floorTilemap;
    private static DungeonManager _instance;
    
    [SerializeField]private Vector3Int baseSpawnCoord = new Vector3Int(-7, -7, 0);
    [SerializeField] private TileBase darkWaterTile;
    
    public List<CharacterInfo> TurnOrderList { get; set; }
    public List<CharacterInfo> PlayerList { get; set; }
    public List<CharacterInfo> EnemyList { get; set; }
    public List<CharacterInfo> SpawnerList { get; set; }
    
    public RangeFinder rangeFinder;
    
    public static DungeonManager Instance
    {
        get { return _instance; }
    }
    
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

    private void Start()
    {
        mercenaryRoot = new GameObject { name = "@Mercenary_Root" }.transform;
        enemyRoot = new GameObject { name = "@Enemy_Root" }.transform;
        spawningPoolRoot = GameObject.Find("@SpawningPool").transform;
        jsonPath = Path.Combine(Application.dataPath, "mySquad.json");
        squadList = LoadData();
        PlayerList = new List<CharacterInfo>();
        EnemyList = new List<CharacterInfo>();
        SpawnerList = new List<CharacterInfo>();
        TurnOrderList = new List<CharacterInfo>();
        dropObject = new DropObject();
        rangeFinder = new RangeFinder();
        
        wallObject = GameObject.Find("Wall");
        wallTilemap = wallObject.GetComponentInChildren<Tilemap>();
        floorTilemap = GameObject.Find("Movable Map").GetComponentInChildren<Tilemap>();
        
        splitedMaps = dropObject.SplitMap();
        GenerateProceduralMap();
        SpawnSquad();
        SpawnEnemy();
        ApplyTraitsToStats();

    }

    private void GenerateProceduralMap()
    {
        // TODO: 스포너가 소환되어야하는 맵에서만 소환하게 하기
        GenerateSpawner();
        GenerateWall();
    }

    private MercenaryList LoadData()
    {
        string json = File.ReadAllText(jsonPath);
        return JsonUtility.FromJson<MercenaryList>(json); 
    }
    
    // Squad를 소환하는 메서드
    private void SpawnSquad()
    {
        int i = 0;
        CharacterInfo character;
        squadList = LoadData();
        battleSystem.playerCount = squadList.mercenaries.Count;//투입 용병 수 업데이트

        void InItMercenary(Mercenary mercenary, OverlayTile spawnTile)
        {
            List<Trait> traitList = new List<Trait>();
            
            foreach (var trait in mercenary.traitnames)
            {
                traitList.Add((Trait)(Resources.Load($"Trait/{trait}")));
            }
            
            character = Managers.Resource.Instantiate($"mercenaries/{mercenary.name}").GetComponent<CharacterInfo>();
            character.transform.parent = mercenaryRoot;
            character.GetComponent<PlayerInfo>().traits = traitList;
            ApplyMercenaryStats(mercenary);
            
            PositionCharacterOnTile(spawnTile, character);
            character.standingOnTile.isUnitOn = true;
            Managers.UI.MakeWorldSpaceUI<UI_HPBar>(character.transform);
            PlayerList.Add(character);
        }
        
        foreach (var mercenary in squadList.mercenaries)
        {
            OverlayTile spawnTile;
            switch (mercenary.idx)
            {
                case 0:
                    MapManager.Instance.map.TryGetValue(baseSpawnCoord, out spawnTile);
                    InItMercenary(mercenary, spawnTile);
                    break;
                case 1:
                    MapManager.Instance.map.TryGetValue(new Vector3Int(baseSpawnCoord.x, baseSpawnCoord.y-1, 0), out spawnTile);
                    InItMercenary(mercenary, spawnTile);
                    break;
                case 2:
                    MapManager.Instance.map.TryGetValue(new Vector3Int(baseSpawnCoord.x-1, baseSpawnCoord.y, 0), out spawnTile);
                    InItMercenary(mercenary, spawnTile);
                    break;
                case 3:
                    MapManager.Instance.map.TryGetValue(new Vector3Int(baseSpawnCoord.x-1, baseSpawnCoord.y-1, 0), out spawnTile);
                    InItMercenary(mercenary, spawnTile);
                    break;
            }    
        }
        
    }
    
    // 캐릭터의 위치를 정하는 method
    public void PositionCharacterOnTile(OverlayTile tile, CharacterInfo mercenary)
    {
        mercenary.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y + 0.2f, tile.transform.position.z + 5);
        mercenary.standingOnTile = tile;
        mercenary.standingOnTile.isUnitOn = true;
        // test 코드
        Random random = new Random();
        if (mercenary.type == "Player")
        {
            mercenary.turnSpeed = random.Next(2, 5);   
        }
        
        if (mercenary.type == "Enemy")
        {
            mercenary.turnSpeed = 10;
        }
    }

    // json 파일에서 해당 용병의 스탯을 가져와서 적용하는 메서드 
    public void ApplyMercenaryStats(Mercenary mercenary)
    {
        
    }
  

    // 적을 맵에 소환하는 메서드
    public void SpawnEnemy()
    {
        //enemy 소환 test 코드
        foreach (var tile in spawnerTileList)
        {
            List<OverlayTile> spawnRange = rangeFinder.GetTilesInRange(tile, 2);
            HashSet<int> randomIndex = new HashSet<int>();
            while (randomIndex.Count < 3)
            {
                randomIndex.Add(random.Next(1, spawnRange.Count));
            }
            
            
            CharacterInfo enemy = Managers.Resource.Instantiate("enemy/Zombie01").GetComponent<CharacterInfo>();
            enemy.transform.parent = enemyRoot;
            PositionCharacterOnTile(spawnRange[2], enemy);
            EnemyList.Add(enemy);
            
            //CharacterInfo enemy2 = Managers.Resource.Instantiate("enemy/Skeleton").GetComponent<CharacterInfo>();
            //enemy2.transform.parent = enemyRoot;
            //PositionCharacterOnTile(spawnRange[3], enemy2);
            //EnemyList.Add(enemy2);

            /*foreach (var index in randomIndex)
            {
                CharacterInfo enemy = Managers.Resource.Instantiate("enemy/Skeleton").GetComponent<CharacterInfo>();
                enemy.transform.parent = enemyRoot;
                PositionCharacterOnTile(spawnRange[index], enemy);
                EnemyList.Add(enemy);
            }*/
        }
        
        BuildTurnOrder();
    }

    // turn 순서를 turnspeed 오름차순으로 정렬하는 메소드
    private void BuildTurnOrder()
    {
        TurnOrderList.AddRange(PlayerList);
        TurnOrderList.AddRange(EnemyList);
        TurnOrderList = TurnOrderList.OrderByDescending(i => i.turnSpeed).ToList();
        battleSystem.TurnOrderList = TurnOrderList;
    }
    
    // TODO: new 키워드 줄이는 방안 생각해보기, 상속 형변환 문제 해결
    //특성 효과 스탯에 적용하는 메소드
    private void ApplyTraitsToStats()

    {
        List<PlayerInfo> playerInfo = new List<PlayerInfo>();
        
        foreach (var character in TurnOrderList)
        {
            if (character.type == "Player")
            {
                playerInfo.Add((PlayerInfo)character);
            }
        }

        foreach (var player in playerInfo)
        {
            traitEffectManager.CheckSpecialTraits(player);
            traitEffectManager.SpecialTraitEffect(player);
            
            for (int i = 0; i < player.traits.Count; i++)
            {
                traitEffectManager.traitEffect(player, player.traits[i]);
            }   
            
            player.hp = player.maxHp;//체력 초기화
            player.cool = player.Maxcool;//냉정 초기화
            player.criticalDmg = (float)1.5 * player.damage;//치명타 데미지 계산
        }
    }
    
}
