using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.ExampleScripts;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEditor.Overlays;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using DG.Tweening;
using Unity.VisualScripting;

public partial class PlayerController : MonoBehaviour
{
    private GameObject distanceCalculator;
    private Dictionary<Vector3Int, OverlayTile> map;

    [SerializeField] private DungeonManager dungeonManager;
    private Tilemap tileMap;
    private OverlayTile cursorTile;
    private OverlayTile clickedTile;
    private PathFinder pathFinder;
    private RangeFinder rangeFinder;
    private PlayerAttack playerAttack;
    private List<OverlayTile> path;
    private List<OverlayTile> skillRangeTiles;
    private PathTranslator pathTranslator;
    private bool isPaused = false;
    private bool isMove = false;
    private bool isOverlapping = false;
    private GameObject currentPlayer;
    Vector3 cursorPosition;
    private RaycastHit2D? focusedTileHit;
    private bool isShowDownArrow = false;

    public int ActionCost { get; set; } = 0;
    public List<OverlayTile> inRangeTiles;
    public bool isConfirmed = false;
    public GameObject cursor;
    public MapManager mapManager;
    public BattleSystem battleSystem;
    public TurnOrderUI turnOrderUI;
    public CampaignUI campainUI;
    public AudioManager audioManager;
    public bool playingFootStep;

    public const float Speed = 2f;
    public Vector3Int baseSpawnCoord;

    void Start()
    {
        pathFinder = new PathFinder();
        rangeFinder = new RangeFinder();
        path = new List<OverlayTile>();
        inRangeTiles = new List<OverlayTile>();
        skillRangeTiles = new List<OverlayTile>();
        pathTranslator = new PathTranslator();

        tileMap = mapManager.GetComponentInChildren<Tilemap>();
        playerAttack = GetComponent<PlayerAttack>();

        map = mapManager.map;

        //distanceCalculator = (GameObject)Instantiate(Resources.Load("Prefabs/calcDistance"));
        playingFootStep = false;
    }

    void LateUpdate()
    {
        focusedTileHit = GetFocusedOnTile();

        if (focusedTileHit.HasValue)
        {
            focusedTileHit.Value.collider.gameObject.TryGetComponent(out cursorTile);
            if (cursorTile == null)
            {
                if (Input.GetMouseButtonDown(0) && battleSystem.actionState == ActionState.DEFAULT) //캐릭터 UI 팝업 조건
                {
                    campainUI.MouseClickDown();
                }
            }
            // 커서 위치 조정하는 코드
            else
            {
                cursorPosition = cursorTile.transform.position;
                cursor.transform.position = cursorPosition;

                // 타일 클릭 시 이벤트 발생시키는 코드
                if (!isPaused)
                {
                    // 마우스 클릭시
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (!EventSystem.current.IsPointerOverGameObject())
                        {
                            if (battleSystem.actionState == ActionState.DEFAULT)
                            {
                                DestroyCurrentPlayer();
                                campainUI.MouseClickDown(); //캐릭터 UI 팝업 조건
                                if (isShowDownArrow == false)
                                {
                                    battleSystem.downArrowMove.ShowDownArrow();
                                    isShowDownArrow = true;
                                }
                            }

                            if (battleSystem.actionState == ActionState.MOVE)
                            {
                                focusedTileHit.Value.collider.gameObject.TryGetComponent(out clickedTile);
                                
                                // 마우스 커서가 inRagngeTiles 위에 있다면 이동 경로와 예상 위치를 보여주는 코드
                                if (inRangeTiles.Contains(cursorTile) && !isMove && battleSystem.actionState == ActionState.MOVE && cursorTile.isBlocked == false && cursorTile.isUnitOn == false)
                                {
                                    path = pathFinder.FindPath(
                                        battleSystem.TurnOrderList[battleSystem.TurnOrderIdx].standingOnTile,
                                        cursorTile, inRangeTiles,
                                        battleSystem.TurnOrderList[battleSystem.TurnOrderIdx]);

                                    foreach (var overlayTile in inRangeTiles)
                                        overlayTile.SetPathSprite(PathTranslator.PathDirection.None);

                                    for (var i = 0; i < path.Count; i++)
                                    {
                                        var previousTile = i > 0 ? path[i - 1] : battleSystem.TurnOrderList[battleSystem.TurnOrderIdx].standingOnTile;
                                        var futureTile = i < path.Count - 1 ? path[i + 1] : null;

                                        var pathDir = pathTranslator.TranslateDirection(previousTile, path[i], futureTile);
                                        path[i].SetPathSprite(pathDir);
                                    }

                                    if (currentPlayer is null)
                                    {
                                        currentPlayer = Managers.Resource.Instantiate(
                                            $"mercenaries/{battleSystem.TurnOrderList[battleSystem.TurnOrderIdx].gameObject.name}");
                                        currentPlayer.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.7f);
                                        Destroy(currentPlayer.GetComponent<CapsuleCollider2D>());
                                    }

                                    currentPlayer.GetOrAddComponent<CharacterControl>().Turn(
                                        (int)Math.Round(cursorTile.transform.position.x - 
                                                        battleSystem.TurnOrderList[battleSystem.TurnOrderIdx].transform.position.x));
                                    currentPlayer.transform.position = new Vector3(cursorTile.transform.position.x, cursorTile.transform.position.y + 0.2f, cursorTile.transform.position.z + 2);
                                }

                                isOverlapping = CheckCharacterOverlapping(battleSystem.TurnOrderList[battleSystem.TurnOrderIdx], clickedTile);

                                if (isMove && !isOverlapping)
                                {
                                    ClearTaunt(battleSystem.TurnOrderList[battleSystem.TurnOrderIdx]);
                                    battleSystem.TurnOrderList[battleSystem.TurnOrderIdx].standingOnTile.isUnitOn = false;
                                }
                            }
                            else if (battleSystem.actionState == ActionState.ATTACK)
                            {
                                focusedTileHit.Value.collider.gameObject.TryGetComponent(out clickedTile);
                                
                                if (skillRangeTiles.Contains(clickedTile) && playerAttack.currentSkill is not null && playerAttack.currentSkill.centerPoint == "clicked")
                                {
                                    playerAttack.HandleMultiTargetSkill(skillRangeTiles);
                                    ActionCost++;
                                }

                                if (inRangeTiles.Contains(clickedTile))
                                {
                                    isMove = true;
                                    battleSystem.downArrowMove.HideDownArrow();
                                    isShowDownArrow = false;

                                    // skill의 종류에 따라 playerAttack 의 메소드를 호출하는 코드
                                    if (playerAttack.currentSkill is null)
                                    {
                                        playerAttack.HandleSingleTargetSkill(clickedTile);
                                    }
                                    else
                                    {
                                        if (playerAttack.currentSkill.target == SkillTarget.Single)
                                        {
                                            playerAttack.HandleSingleTargetSkill(clickedTile);
                                            ActionCost++;
                                        }
                                        else if (playerAttack.currentSkill.target == SkillTarget.Multi)
                                        {
                                            if (playerAttack.currentSkill.centerPoint == "self")
                                            {
                                                playerAttack.HandleMultiTargetSkill(inRangeTiles);
                                                ActionCost++;
                                            }
                                            else if (playerAttack.currentSkill.centerPoint == "clicked")
                                            {
                                                skillRangeTiles = playerAttack.SetSkillRangeTiles(clickedTile);
                                                inRangeTiles.Clear();
                                            }
                                        }
                                    }

                                    isMove = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        //빈 공간 클릭시 UI를 닫는 코드
        else
        {
            if (Input.GetMouseButtonDown(0) && battleSystem.actionState == ActionState.DEFAULT &&
                !EventSystem.current.IsPointerOverGameObject())
            {
                print("click");
                campainUI.MouseClickDown();
            }
        }

        if (path.Count > 0 && battleSystem.actionState == ActionState.MOVE && isMove == true && isConfirmed == true)
        {
            MoveAlongPath(battleSystem.TurnOrderList[battleSystem.TurnOrderIdx], clickedTile);
        }
    }

    bool SkipTurn()
    {
        return false;
    }

    public void TauntEnemy(CharacterInfo character)
    {
        int range = 5;
        List<OverlayTile> tauntTiles = rangeFinder.GetTilesInRange(character.standingOnTile, range);
        foreach (var enemy in battleSystem.TurnOrderList)
        {
            if (enemy.type != "Enemy")
            {
                continue;
            }

            if (tauntTiles.Contains(enemy.standingOnTile))
            {
                Debug.Log($"{enemy}는 도발에 걸렸다!");
                enemy.transform.GetComponent<EnemyInfo>().actionState = EnemyInfo.ActionState.Danger;
                enemy.transform.GetComponent<EnemyInfo>().destinationTile = character.standingOnTile;
            }
        }
    }

    private void ClearTaunt(CharacterInfo character)
    {
        foreach (var enemy in battleSystem.TurnOrderList)
        {
            if (enemy.type != "Enemy")
            {
                continue;
            }

            if (enemy.transform.GetComponent<EnemyInfo>().destinationTile == character.standingOnTile)
            {
                Debug.Log($"{enemy}는 도발에서 벗어났다!");
                enemy.transform.GetComponent<EnemyInfo>().actionState = EnemyInfo.ActionState.Default;
                enemy.transform.GetComponent<EnemyInfo>().destinationTile = null;
            }
        }
    }

    public void PositionCharacterOnTile(OverlayTile tile, CharacterInfo character)
    {
        character.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y + 0.15f, tile.transform.position.z + 5);
        character.standingOnTile = tile;
    }

    public RaycastHit2D? GetFocusedOnTile()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2d = new Vector2(mousePos.x, mousePos.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2d, Vector2.zero);

        if (hits.Length > 0)
        {
            return hits.OrderBy(i => i.collider.transform.position.z).First();
        }

        return null;
    }

    public void OnEndButton()
    {
        DestroyCurrentPlayer();
        battleSystem.downArrowMove.HideDownArrow();
        isShowDownArrow = false;

        HideCurrentTiles();

        if (GameObject.Find("BasicAttackButton") != null)
        {
            GameObject.Find("UICanvas").transform.Find("AttackButton").gameObject.SetActive(true);
            GameObject.Find("BasicAttackButton").SetActive(false);
            GameObject.Find("SkillButton1").SetActive(false);
            GameObject.Find("SkillButton2").SetActive(false);
        }

        playerAttack.currentSkill = null;

        if (battleSystem.battleState == BattleState.PLAYERTURN)
        {
            ActionCost = 0;
            StartCoroutine(battleSystem.TurnEnd());
        }
    }

    public void HideCurrentTiles()
    {
        foreach (var item in inRangeTiles)
        {
            item.HideTile();
        }
    }

    public void OnConfirmButton()
    {
        isConfirmed = true;
        if (battleSystem.actionState == ActionState.MOVE)
        {
            isMove = true; isPaused = true;
            // 캐릭터가 이동할 때 경로 및 예상 위치 및 화살표 삭제하기
            HideCurrentTiles();
            DestroyCurrentPlayer();
            battleSystem.downArrowMove.HideDownArrow();
        }
        
        else if (battleSystem.actionState == ActionState.ATTACK)
        {
            HideCurrentTiles();

            if (playerAttack.currentSkill == null)
            {
                StartCoroutine(playerAttack.PerformAttack());
            }
            else
            {
                switch (playerAttack.currentSkill.skillType)
                {
                    case SkillType.Attack:
                        StartCoroutine(playerAttack.PerformAttack());
                        break;
                    case SkillType.Heal:
                        playerAttack.PerformHeal();
                        break;
                    case SkillType.Buff:
                        playerAttack.PerformBuff();
                        break;
                    case SkillType.Debuff:
                        playerAttack.PerformDeBuff();
                        break;
                }
            }
            
            battleSystem.downArrowMove.HideDownArrow();
        }
    }

    public void OnMoveButton()
    {
        HideCurrentTiles();
        
        if (GameObject.Find("BasicAttackButton") != null)
        {
            GameObject.Find("UICanvas").transform.Find("AttackButton").gameObject.SetActive(true);
            GameObject.Find("BasicAttackButton").SetActive(false);
            GameObject.Find("SkillButton1").SetActive(false);
            GameObject.Find("SkillButton2").SetActive(false);
        }

        if (battleSystem.battleState == BattleState.PLAYERTURN && isMove is false)
        {
            battleSystem.actionState = ActionState.MOVE;
            if (battleSystem.TurnOrderList is null)
                Debug.Log("Null");
            else
                GetInRangeTiles(battleSystem.TurnOrderList[battleSystem.TurnOrderIdx]);
        }
    }

        //TODO : 해당 캐릭터의 스킬 버튼을 띄우는 애니메이션 추가
        public void OnAttackButton()
        {
            DestroyCurrentPlayer();
            HideCurrentTiles();

            if (battleSystem.battleState == BattleState.PLAYERTURN && isMove is false)
            {
                battleSystem.actionState = ActionState.ATTACK;
                if (battleSystem.TurnOrderList is null)
                    Debug.Log("Null");
                else
                    GetPlayerSkills(battleSystem.TurnOrderList[battleSystem.TurnOrderIdx]);
                //GetTestTiles(battleSystem.TurnOrderList[battleSystem.TurnOrderIdx]);
            }
        }

        public void OnBasicAttackButton()
        {
            HideCurrentTiles();
            
            playerAttack.currentSkill = null;
            playerAttack.targetTiles.Clear();
            var player = (PlayerInfo)battleSystem.TurnOrderList[battleSystem.TurnOrderIdx];
            inRangeTiles = playerAttack.SetAttack(player, null, inRangeTiles);
        }

        public void OnSkillButton1()
        {
            HideCurrentTiles();
            playerAttack.targetTiles.Clear();
            var player = (PlayerInfo)battleSystem.TurnOrderList[battleSystem.TurnOrderIdx];
            inRangeTiles = playerAttack.SetAttack(player, player.skills[0], inRangeTiles);
        }

        public void OnSkillButton2()
        {
            HideCurrentTiles();
            playerAttack.targetTiles.Clear();
            var player = (PlayerInfo)battleSystem.TurnOrderList[battleSystem.TurnOrderIdx];
            inRangeTiles = playerAttack.SetAttack(player, player.skills[1], inRangeTiles);
        }

        // Player 스킬을 받아오는 함수
        private void GetPlayerSkills(CharacterInfo player)
        {
            GameObject.Find("AttackButton").SetActive(false);
            GameObject.Find("UICanvas").transform.Find("BasicAttackButton").gameObject.SetActive(true);
            GameObject.Find("UICanvas").transform.Find("SkillButton1").gameObject.SetActive(true);
            GameObject.Find("UICanvas").transform.Find("SkillButton2").gameObject.SetActive(true);

            var skillImage1 = GameObject.Find("SkillImage1").GetComponent<Image>();
            var skillImage2 = GameObject.Find("SkillImage2").GetComponent<Image>();

            var curPlayer = (PlayerInfo)player;
            skillImage1.sprite = curPlayer.skills[0].icon;
            skillImage2.sprite = curPlayer.skills[1].icon;
        }

        private void DestroyCurrentPlayer()
        {
            Managers.Resource.Destroy(currentPlayer);
            currentPlayer = null;
        }
}
