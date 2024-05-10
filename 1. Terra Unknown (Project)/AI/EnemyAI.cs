 using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
 using System.Runtime.CompilerServices;
 using Assets.PixelFantasy.PixelHeroes.Common.Scripts.ExampleScripts;
 using Unity.VisualScripting;
 using UnityEngine;
 using AnimationState = Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts.AnimationState;
 using Random = UnityEngine.Random;

 // TODO: Enemy의 유형에 따른 AI를 구분해야할 필요가 있음.
 // TODO: 기본이되는 enemy ai를 만들고 적의 type에 따라서 기본 enemy ai를 상속받는 다양한 enemy ai를 만들어야 할 것 같다.
 // TODO: 근거리 type, 원거리 type, 보스 등 다양한 AI
public partial class EnemyAI : MonoBehaviour
{
    public enum ActionState
    {
        Default,
        Detect,
        Danger
    }
    
    private Dictionary<Vector3Int, OverlayTile> map;
    private OverlayTile currentTile;
    private OverlayTile destinationTile;
    private List<OverlayTile> movableTiles;
    private PathFinder pathFinder;
    private RangeFinder rangeFinder;
    private List<OverlayTile> inRangeTiles;
    private List<OverlayTile> path;
    private ActionState actionState;
    private bool isOverlapping = false;
    private bool isSetDestTile = false;
    private bool actionFlag = false;
    private bool isMove = false;
    
    public PlayerController playerController;
    public BattleSystem battleSystem;
    public TurnOrderUI turnOrderUI;
    public SkillEffectController skillEffectController;
    public bool isCharacterDoAttack = false;
    public const float Speed = 4;

    private void Start()
    {
        pathFinder = new PathFinder();
        rangeFinder = new RangeFinder();
        path = new List<OverlayTile>();
        inRangeTiles = new List<OverlayTile>();
        map = MapManager.Instance.map;
        actionState = ActionState.Default;
    }

    public void LateUpdate()
    {
        // ENEMY ACTION UPDATE
        if (battleSystem.battleState == BattleState.ENEMYTURN && battleSystem.TurnOrderList is not null)
        {
            if (!isSetDestTile && isMove == false)
            {
                ChooseEnemyAction(battleSystem.TurnOrderList[battleSystem.TurnOrderIdx]);
            }
            
            // actionState에 따라 이동
            if (!isOverlapping && path.Count > 0)
            {
                isSetDestTile = true;
                MoveToDestinationTile(battleSystem.TurnOrderList[battleSystem.TurnOrderIdx]);
            }
        }
    }

    private void ChooseEnemyAction(CharacterInfo enemy)
    {
        isMove = true;
        
        if (SkipTurn())
        {
            return;
        }
        
        EnemyInfo enemyInfo;
        enemy.TryGetComponent<EnemyInfo>(out enemyInfo);
        
        if (enemyInfo.rank == EnemyInfo.Rank.Normal)
        {
            NormalEnemyAttack(enemy);
        } 
        else if (enemyInfo.rank == EnemyInfo.Rank.Elite)
        {
            PerformEliteAction(enemy);
        }

        if (actionFlag == false)
        {
            ChooseDestinationTile(enemy);
        }
    }
    
    
    private void ChooseDestinationTile(CharacterInfo enemy)
    {
        DetectPlayer(enemy);
        SetActionState(enemy);
        switch (actionState)
        {
            case ActionState.Default :
                DefaultAction(enemy);
                break;
            case ActionState.Detect :
                DetectAction(enemy);
                break;
            case ActionState.Danger :
                DangerAction(enemy);
                break;
        }
    }

    private void DetectPlayer(CharacterInfo character)
    {
        EnemyInfo enemy = (EnemyInfo)character;
        int range = 0;
        
        if (enemy.rank == EnemyInfo.Rank.Normal)
        {
            range = 5;
        } 
        else if (enemy.rank == EnemyInfo.Rank.Elite)
        {
            range = 6;
        }

        if (enemy.actionState != EnemyInfo.ActionState.Danger)
        {
            List<OverlayTile> detectTiles = rangeFinder.GetTilesInRange(enemy.standingOnTile, range);
            
            enemy.actionState = EnemyInfo.ActionState.Default;
            enemy.destinationTile = null;
            
            
            foreach (var player in DungeonManager.Instance.PlayerList)
            {
                if (detectTiles.Contains(player.standingOnTile))
                {
                    Debug.Log("플레이어를 감지했다!");
                    enemy.actionState = EnemyInfo.ActionState.Detect;
                    enemy.destinationTile = player.standingOnTile;
                    break;
                }
            }
        }
    }

    private void SetActionState(CharacterInfo enemy)
    {
        if (enemy.transform.GetComponent<EnemyInfo>().actionState == EnemyInfo.ActionState.Default)
        {
            actionState = ActionState.Default;
        } else if (enemy.transform.GetComponent<EnemyInfo>().actionState == EnemyInfo.ActionState.Detect)
        {
            actionState = ActionState.Detect;
        } else if (enemy.transform.GetComponent<EnemyInfo>().actionState == EnemyInfo.ActionState.Danger)
        {
            actionState = ActionState.Danger;
        }
    }
    private void DefaultAction(CharacterInfo character)
    {
        EnemyInfo enemy = (EnemyInfo)character;
        
        Debug.Log("기본 상태에서의 이동");

        if (enemy.rank == EnemyInfo.Rank.Normal)
        {
            inRangeTiles = rangeFinder.GetTilesInRange(enemy.standingOnTile, 1);
        } 
        else if (enemy.rank == EnemyInfo.Rank.Elite)
        {
            inRangeTiles = rangeFinder.GetTilesInRange(enemy.standingOnTile, 2);
        }

        List<OverlayTile> movableTiles = inRangeTiles.Where(x => !x.isBlocked && enemy.standingOnTile != x && !x.isUnitOn).ToList();
        
        int idx = Random.Range(0, movableTiles.Count);
        
        destinationTile = movableTiles[idx];
        path = pathFinder.FindPath(battleSystem.TurnOrderList[battleSystem.TurnOrderIdx].standingOnTile, destinationTile, movableTiles, enemy);

        if (destinationTile.isUnitOn = true)
        {
            Debug.Log("왜 유닛이 서 있는 곳을 destination TilE 로 정한거임?? ");
        }

        enemy.standingOnTile.isUnitOn = false;
        destinationTile.isUnitOn = true;
    }
    
    private void DetectAction(CharacterInfo character)
    {
        EnemyInfo enemy = (EnemyInfo)character;
        int range = 0;

        if (enemy.rank == EnemyInfo.Rank.Normal)
        {
            range = 5;
        } 
        else if (enemy.rank == EnemyInfo.Rank.Elite)
        {
            range = 6;
        }
        
        destinationTile = enemy.destinationTile;
        inRangeTiles = rangeFinder.GetTilesInRange(enemy.standingOnTile, range);
        path = pathFinder.FindPath(enemy.standingOnTile, destinationTile, inRangeTiles, enemy);
        Debug.Log("발견 상태에서의 이동");
        
        if (path.Count > 2)
        {
            destinationTile = path[1];
            path.RemoveRange(2, path.Count - 2);
        }
        else if (path.Count == 2)
        {
            destinationTile = path[0];
            path.RemoveAt(1);
        }
        
        enemy.standingOnTile.isUnitOn = false;
        destinationTile.isUnitOn = true;
    }
    
    private void DangerAction(CharacterInfo enemy)
    {
        Debug.Log("위험 상태에서의 이동");
        destinationTile = enemy.transform.GetComponent<EnemyInfo>().destinationTile;
        inRangeTiles = rangeFinder.GetTilesInRange(battleSystem.TurnOrderList[battleSystem.TurnOrderIdx].standingOnTile, 5);
        path = pathFinder.FindPath(battleSystem.TurnOrderList[battleSystem.TurnOrderIdx].standingOnTile, destinationTile, inRangeTiles, enemy);
        
        if (path.Count > 2)
        {
            destinationTile = path[1];
            path.RemoveRange(2, path.Count - 2);
        }
        else if (path.Count == 2)
        {
            destinationTile = path[0];
            path.RemoveAt(1);
        }
        
        enemy.standingOnTile.isUnitOn = false;
        destinationTile.isUnitOn = true;
    }
    
    private bool CheckCharacterOverlap(OverlayTile destinationTile)
    {
        foreach (var unit in battleSystem.TurnOrderList)
        {
            if (unit.standingOnTile == destinationTile)
            {
                return true;
            }
        }

        return false;
    }
    
    private void MoveToDestinationTile(CharacterInfo enemy)
    {
        var step = Speed * Time.deltaTime;
        
        float zIndex = path[0].transform.position.z;
        Vector2 pathPosition = new Vector2(path[0].transform.position.x, path[0].transform.position.y + 0.2f);
        enemy.transform.position = Vector2.MoveTowards(enemy.transform.position, pathPosition, step);
        enemy.transform.position = new Vector3(enemy.transform.position.x, enemy.transform.position.y, zIndex + 5);
        
        CharacterControl characterControl = enemy.gameObject.GetOrAddComponent<CharacterControl>();
        characterControl.Character.SetState(AnimationState.Running);
        characterControl.MoveDust.Play();
        
        // 캐릭터 바라보는 방향 설정
        if (enemy.transform.position.x < pathPosition.x)
        {
            if (enemy.transform.localScale.x < 0.0f)
            {
                var scale = enemy.transform.localScale;
                var velocityModule = characterControl.MoveDust.GetComponent<ParticleSystem>().velocityOverLifetime;
                velocityModule.x = new ParticleSystem.MinMaxCurve(-2.0f);
                scale.x = -1 * scale.x;
                enemy.transform.localScale = scale;
            }
        }
        else if(enemy.transform.position.x > pathPosition.x)
        {
            if (enemy.transform.localScale.x > 0.0f)
            {
                var scale = enemy.transform.localScale;
                var velocityModule = characterControl.MoveDust.GetComponent<ParticleSystem>().velocityOverLifetime;
                velocityModule.x = new ParticleSystem.MinMaxCurve(2.0f);
                scale.x = Mathf.Sign(-1) * scale.x;
                enemy.transform.localScale = scale;
            }
        }
        
        if(Vector2.Distance(enemy.transform.position, pathPosition) < 0.00001f)
        {
            playerController.PositionCharacterOnTile(path[0], enemy);
            path.RemoveAt(0);
        }
        if (path.Count == 0)
        {
            isSetDestTile = false;
            characterControl.Character.SetState(AnimationState.Idle);
            characterControl.MoveDust.Stop();
            CheckAttackPossible(enemy);
        }
    }

    private void CheckAttackPossible(CharacterInfo character)
    {
        EnemyInfo enemy = (EnemyInfo)character;

        if (enemy.rank == EnemyInfo.Rank.Normal)
        {
            NormalEnemyAttack(enemy);
        } 
        else if (enemy.rank == EnemyInfo.Rank.Elite)
        {
            PerformEliteAction(enemy);
        }

        if (isCharacterDoAttack == false)
        {
            Debug.Log("이동후 공격을 하지 않고 턴 종료");
            StartCoroutine(EnemyTurnEnd());
        }
    }
    
    IEnumerator EnemyTurnEnd()
    {
        yield return new WaitForSeconds(0.35f);
        isSetDestTile = false;
        isMove = false;
        StartCoroutine(battleSystem.TurnEnd());
    }

    
}
