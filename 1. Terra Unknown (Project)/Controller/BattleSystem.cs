using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;
using DG.Tweening;

// 현재 누구의 턴인지, 어떤 상태인지 구분하는 enum
public enum BattleState
{
    START,
    PLAYERTURN,
    ENEMYTURN,
    SPAWNTURN,
}

// 현재 플레이어가 어떤 action을 하고있는지 구분하는 enum
public enum ActionState
{
    DEFAULT,
    MOVE,
    ATTACK
}

public class BattleSystem : MonoBehaviour
{
    private OverlayTile snottiteTile;//콧물석 타일
    private int camIdx = 0;
    private GameObject camObj;
    public int TurnOrderIdx { get; set; } = 0;

    public List<CharacterInfo> TurnOrderList { get; set; }

    public int Turn { get; set; } = 0;
    public BattleState battleState;
    public ActionState actionState;
    public  DungeonManager dungeonManager;
    public PlayerController playerController;
    public EnemyAI enemyAI;
    public TurnOrderUI turnOrderUI;
    public BasicCameraMovement camMovement;
    public CampaignUI campainUI;
    public TraitEffectManager traitManager;
    public UIPopupManager uiPopupManager;
    public SkillEffectController skillEffectController;
    public int playerCount; //생존 용병 수 확인용
    public GameObject stagePopup;
    public EffectPoolManager effectPoolManager;
    public DownArrowMove downArrowMove;
    public ItemEffectManager itemEffectManager;
    public InventoryList BagList;


    private GameObject warnObject;
    private DropObject dropObject;
    private Vector3 stagePopupPos;
    private int stageNum;
    private WaitForSeconds wfs;
    // Start is called before the first frame update
    void Start()
    {
        battleState = BattleState.START;
        actionState = ActionState.DEFAULT;
        dropObject = new DropObject();
        stageNum = 1;
        stagePopupPos = stagePopup.transform.position;
        uiPopupManager.DOTPopUpUI(stagePopup,"Stage"+stageNum,stagePopupPos);
        
        snottiteTile = null;
        dropObject.SplitMap();
        effectPoolManager.Init(TurnOrderList);
        
        wfs = new WaitForSeconds(1.1f);
        StartCoroutine(SetupBattle());
    }
    
    // 전투를 시작하기전 해야하는 것들을 하는 코루틴
    // TODO: InitBattle() 로 이름을 바꿀까
    IEnumerator SetupBattle()
    {
        Debug.Log(turnOrderUI);
        StartCoroutine(turnOrderUI.GetPortrait());
        uiPopupManager.DOTPopDownUI(stagePopup);
        if (TurnOrderList[TurnOrderIdx].type == "Player")
        {
            SetCamObj();
            camMovement.ChangeVcamFollow(camObj, camIdx);
            yield return wfs;
            camMovement.updateFreeCamPos = true;
            
            battleState = BattleState.PLAYERTURN;
            Debug.Log($"{TurnOrderIdx}번마");
            PlayerTurn();
        } 
        else if (TurnOrderList[TurnOrderIdx].type == "Enemy")
        {
            SetCamObj();
            camMovement.ChangeVcamFollow(camObj, camIdx);
            yield return wfs;
            camMovement.updateFreeCamPos = true;
            
            battleState = BattleState.ENEMYTURN;
            //cursor.SetActive(false);
            playerController.enabled = false;
            StartCoroutine(EnemyTurn());
        }
    }

    // TODO: 카메라 설정하는 메소드인데 이릉이 명확하지가 않아서 어떤 기능인지 헷갈림. 수정 필요.
    private void SetCamObj()
    {
        camObj = TurnOrderList[TurnOrderIdx].gameObject;
        camIdx++;
    }

    // 턴이 끝나고 난 후 다음턴은 누구인지, 설정하는 메소드
    public IEnumerator TurnEnd()
    {
        StartCoroutine(turnOrderUI.GetPortrait());
        dungeonManager.GetComponent<EnemyAI>().enabled = false;
        playerController.enabled = false;
        TurnOrderIdx++;
        
        uiPopupManager.DOTPopDownUI(stagePopup);
        
        if (TurnOrderIdx >= TurnOrderList.Count)
        {
            TurnOrderIdx = -1;
            StartCoroutine(SpawnTrun());
        }
        else if(TurnOrderList[TurnOrderIdx].isDead == true)
        {
            Debug.Log("죽은놈 스킵");
            StartCoroutine(TurnEnd());
        }
        else if (TurnOrderList[TurnOrderIdx].type == "Player")
        { 
            if (snottiteTile != null) 
            { 
                //dropObject.Drop(snottiteTile);
                dropObject.DropObjectEffect(TurnOrderList);
                StartCoroutine(skillEffectController.DropEffectTimingControl
                    ("DropEffect01","WaterEffect",snottiteTile.transform.GameObject()));
                snottiteTile = null;
                if (warnObject != null)
                {
                    skillEffectController.ReleaseWarnEffect(warnObject);
                    warnObject = null;
                }
            }
            //드랍 하는 턴이면 콧물석 드랍
            snottiteTile = dropObject.CheckDrop();
            //콧물석 드랍 판정
            if (snottiteTile != null)
            {
                //콧물석 경고 표시
                warnObject = skillEffectController.SpawnWarnEffect(snottiteTile.transform.position);
                dropObject.GetEffectTiles(snottiteTile);
            }
            SetCamObj();
            battleState = BattleState.PLAYERTURN;
            Debug.Log($"{TurnOrderIdx}번마");
            yield return wfs;
            camMovement.ChangeVcamFollow(camObj, camIdx);
            yield return wfs;
            PlayerTurn();
        } 
        else if (TurnOrderList[TurnOrderIdx].type == "Enemy")
        {
            if (snottiteTile != null)
            {
                //dropObject.Drop(snottiteTile);
                dropObject.DropObjectEffect(TurnOrderList);
                StartCoroutine(skillEffectController.DropEffectTimingControl
                    ("DropEffect01","WaterEffect",snottiteTile.transform.GameObject()));
                snottiteTile = null;
                if (warnObject != null)
                {
                    skillEffectController.ReleaseWarnEffect(warnObject);
                    warnObject = null;
                }
            }
            SetCamObj();
            battleState = BattleState.ENEMYTURN;
            Debug.Log($"{TurnOrderIdx}번마");
            yield return wfs;
            camMovement.ChangeVcamFollow(camObj, camIdx);
            yield return wfs;
            StartCoroutine(EnemyTurn());
        }
    }
    
    // 플레이어 턴에 해야할 행동들을 수행하는 메소드
    void PlayerTurn()
    {
        Debug.Log("PlayerTurn");
        
        // 해당 턴 플레이어 화살표 표시
        TurnOrderList[TurnOrderIdx].gameObject.transform.GetChild(3).TryGetComponent<DownArrowMove>(out downArrowMove);
        downArrowMove.ShowDownArrow();
            
        CountPlayer();
        traitManager.SpecialTraitUpdate((PlayerInfo)TurnOrderList[TurnOrderIdx]);//특수 특성 효과 업데이트
        camMovement.updateFreeCamPos = true;
        Turn++;
        
        playerController.enabled = true;
    }
    
    // 적 턴에 해야할 행동들을 수행하는 메소드
    IEnumerator EnemyTurn()
    {
        Debug.Log("EnemyTurn");
        yield return new WaitForSeconds(0.4f);
        camMovement.updateFreeCamPos = true;
        enemyAI.isCharacterDoAttack = false;
        dungeonManager.GetComponent<EnemyAI>().enabled = true;
        yield return wfs;
    }

    // 스폰 턴에 해야할 행동들을 수행하는 메소드
    IEnumerator SpawnTrun()
    {
        Debug.Log("SpawnTurn");
        uiPopupManager.DOTPopDownUI(stagePopup);
        ClearDeadUnit();
        SpawningPoolManager.Instance.CallSpawn();
        TurnOrderList = TurnOrderList.OrderByDescending(i => i.turnSpeed).ToList();
        
        yield return new WaitForSeconds(1f);
        
        stageNum++;
        uiPopupManager.DOTPopUpUI(stagePopup,"Stage"+stageNum,stagePopupPos);
        TurnOrderIdx = 0;
        StartCoroutine(SetupBattle());
    }

    private void ClearDeadUnit()
    {
        for (int i = TurnOrderList.Count - 1; i >= 0; i--)
        {
            if (TurnOrderList[i].isDead == true)
            {
                Destroy(TurnOrderList[i]);
                TurnOrderList.RemoveAt(i);
            }
        }
    }
    
    private void CountPlayer()
    {
        playerCount = 0;
        foreach (var character in TurnOrderList)
        {
            if (character.type == "Player")
            {
                playerCount++;
            }
        }
    }
    
    public List<PlayerInfo> GetPlayerList(List<CharacterInfo> List)
    {
        List<PlayerInfo> playerInfo = new List<PlayerInfo>();
        
        foreach (var character in List)
        {
            if (character.type == "Player")
            {
                playerInfo.Add((PlayerInfo)character);
            }
        }

        return playerInfo;
    }
   
}
