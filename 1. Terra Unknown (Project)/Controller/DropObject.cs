using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.DemiLib;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;


public class DropObject: MonoBehaviour
{
    public BattleSystem battleSystem;
    public TurnOrderUI turnOrderUI;
    private List<List<OverlayTile>> splitedMaps;
    private List<OverlayTile> diagonalTiles;
    private Transform root;
    
    
    
    // 맵을 5x5 9개로 나누는 메소드
    public List<List<OverlayTile>> SplitMap()
    {
        splitedMaps = new List<List<OverlayTile>>();
        root = new GameObject { name = "@Drop_Root" }.transform;
        for (int i = 0; i < 9; i++)
        {
            splitedMaps.Add(new List<OverlayTile>());
        }
        
        foreach (var map in MapManager.Instance.map)
        {
            if (map.Key.z >= 1) continue;
            
            int x = map.Key.x;
            int y = map.Key.y;

            int rowIndex = GetRowIndex(x);
            int colIndex = GetColIndex(y);

            if (rowIndex != -1 && colIndex != -1)
            {
                OverlayTile dropTile;
                MapManager.Instance.map.TryGetValue(map.Key, out dropTile);
                splitedMaps[rowIndex + colIndex * 3].Add(dropTile);
                
                // 테스트용 좌표 표시
                if (x == -8)
                {
                    UI_Coordinate ui = Managers.UI.MakeWorldSpaceUI<UI_Coordinate>(dropTile.transform);
                    ui.GetComponentInChildren<TextMeshProUGUI>().text = y.ToString();
                    DOTweenAnimation anim = ui.transform.GetChild(0).GetComponent<DOTweenAnimation>();
                    anim.DORestartById("x");
                }

                if (y == -8)
                {
                    UI_Coordinate ui = Managers.UI.MakeWorldSpaceUI<UI_Coordinate>(dropTile.transform);
                    ui.GetComponentInChildren<TextMeshProUGUI>().text = x.ToString();
                    DOTweenAnimation anim = ui.transform.GetChild(0).GetComponent<DOTweenAnimation>();
                    anim.DORestartById("y");
                    
                }
            }
            else
            {
                Debug.Log("잘못된 범위에 접근");
            }
        }
        
        int GetRowIndex(int x)
        {
            if (x >= -8 && x <= -4) return 0;
            else if (x >= -3 && x <= 1) return 1;
            else if (x >= 2 && x <= 6) return 2;
            else return -1;
        }

        int GetColIndex(int y)
        {
            if (y >= 2 && y <= 6) return 0;
            else if (y >= -3 && y <= 1) return 1;
            else if (y >= -8 && y <= -4) return 2;
            else return -1;
        }

        return splitedMaps;
    }

    // Drop 기능을 수행하는 메소드
    public void Drop(OverlayTile droptile)
    {
        GameObject drop = Managers.Resource.Instantiate("object/snottite");
        drop.transform.parent = root;
        drop.transform.position = new Vector3(droptile.transform.position.x, droptile.transform.position.y + 10, droptile.transform.position.z + 1);
    }
    
    // Drop의 위치를 설정하는 메소드
    public OverlayTile PositionDropOnTile()
    {
        HashSet<int> uniqueSections = new HashSet<int>();
        int sectionCnt = 0;

            int randomSection = UnityEngine.Random.Range(0, 8);
            if (!uniqueSections.Contains(randomSection))
            {
                uniqueSections.Add(randomSection);
                sectionCnt = splitedMaps[randomSection].Count;
            }

            int random = UnityEngine.Random.Range(0, sectionCnt);
            OverlayTile dropTile = splitedMaps[randomSection][random];

            return dropTile;
    }


    public void GetEffectTiles(OverlayTile tile)//콧물석 효과 범위 표시하는 메소드

    {
        var _map = MapManager.Instance.map;
        OverlayTile startingTile = tile;
        int range = 1;

        diagonalTiles = new List<OverlayTile>();
        var keys = new List<Vector3Int>();
        keys.Add(new Vector3Int(startingTile.gridLocation.x, startingTile.gridLocation.y, startingTile.gridLocation.z));//캐릭터 위치
        for (int i = 0; i < range; i++)
        //캐릭터 기준 range 두께의 사각형 범위
        {
            keys.Add(new Vector3Int(startingTile.gridLocation.x - i - 1, startingTile.gridLocation.y + i + 1, startingTile.gridLocation.z));
            keys.Add(new Vector3Int(startingTile.gridLocation.x + i + 1, startingTile.gridLocation.y + i + 1, startingTile.gridLocation.z));
            keys.Add(new Vector3Int(startingTile.gridLocation.x - i - 1, startingTile.gridLocation.y - i - 1, startingTile.gridLocation.z));
            keys.Add(new Vector3Int(startingTile.gridLocation.x + i + 1, startingTile.gridLocation.y - i - 1, startingTile.gridLocation.z));
            keys.Add(new Vector3Int(startingTile.gridLocation.x + i + 1, startingTile.gridLocation.y, startingTile.gridLocation.z));
            keys.Add(new Vector3Int(startingTile.gridLocation.x - i - 1, startingTile.gridLocation.y, startingTile.gridLocation.z));
            keys.Add(new Vector3Int(startingTile.gridLocation.x, startingTile.gridLocation.y + i + 1, startingTile.gridLocation.z));
            keys.Add(new Vector3Int(startingTile.gridLocation.x, startingTile.gridLocation.y - i - 1, startingTile.gridLocation.z));

            foreach (var key in keys)
            {
                if (_map.ContainsKey(key))
                {
                    diagonalTiles.Add(_map[key]);
                }
            }
        }
    }
    //TODO : RangeFinder로 연결해서 사용하는게 더 깔끔할거같음



    // 확률에 따라 Drop을 수행할지 안할지 결정하는 메소드
    public OverlayTile CheckDrop()
    {
        OverlayTile droptile;

        if (UnityEngine.Random.Range(0f, 1f) > 0f)
        {
            droptile = PositionDropOnTile();
            Debug.Log($"{droptile.grid2DLocation.x}, {droptile.grid2DLocation.y} 로의 drop 수행");
            return droptile;
        }
        else
        {
            Debug.Log("drop 수행하지 않음.");
            return null;
        }
    }
    
    public void DropAttack(CharacterInfo character, int playerIdx)
    {
        bool isAllDead = true;

        var player = (PlayerInfo)character;

        if (player.standingOnTile == diagonalTiles[0])
        {
            player.hp -= 3;
            player.cool -= 5;
            Debug.Log("콧물석에 맞았습니다. 체력: " + player.hp + "냉정: " + player.cool);
        }
        else
        {
            player.cool -= 5;
            Debug.Log("콧물석이 근처에 떨어졌습니다." + "체력: " + player.hp + "냉정: " + player.cool);
        }

        if (player.hp <= 0)
        {
            Destroy(player.gameObject);
            battleSystem.TurnOrderList.RemoveAt(playerIdx);

            if (playerIdx < battleSystem.TurnOrderIdx) battleSystem.TurnOrderIdx -= 1;
            
            Debug.Log("플레이어 사망");

            foreach (var unit in battleSystem.TurnOrderList)
            {
                if (unit.type == "Mercenary")
                {
                    isAllDead = false;
                }
            }
            
            if (isAllDead) Debug.Log("용병단 전원 사망. 캠페인 종료.");
        }

        foreach (var tile in diagonalTiles) tile.HideTile();
    }

    public void DropObjectEffect(List<CharacterInfo> turnorderlist)
    {
        //bool hide = false;
        foreach (var tile in diagonalTiles)
        {
            int idx = 0;

            foreach (var player in turnorderlist)
            {
                if (player.type != "Player")
                {
                    idx++;
                    continue;
                }

                if (tile == player.standingOnTile)
                {
                    DropAttack(player, idx);
                }

                idx++;
            }
        }
    }
}
