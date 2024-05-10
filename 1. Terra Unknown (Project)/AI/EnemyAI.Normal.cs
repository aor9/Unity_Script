using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.ExampleScripts;using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using AnimationState = Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts.AnimationState;
using Random = UnityEngine.Random;

public partial class EnemyAI
{
    [SerializeField] 
    private CinemachineBrain cinemachineBrain;
    
    private void NormalEnemyAttack(CharacterInfo enemy)
    {
        CharacterInfo target = null;
        
        target = SetNormalEnemyTarget(enemy);
        Debug.Log(target);
        
        if (target != null)
        {
            EnemyAttack(enemy, target);
        }
        else
        {
            Debug.Log("공격 가능한 적이 없음.");
        }
    }

    private CharacterInfo SetNormalEnemyTarget(CharacterInfo enemy)
    {
        CharacterInfo target = new CharacterInfo();
        
        inRangeTiles = rangeFinder.GetTilesInRange(enemy.standingOnTile, enemy.range);
        
        foreach (var player in DungeonManager.Instance.PlayerList)
        {
            foreach (var tile in inRangeTiles)
            {
                if (player.standingOnTile == tile)
                {
                    target = player;
                }
            }
        }

        return target;
    }
    
    private void EnemyAttack(CharacterInfo enemy, CharacterInfo player = null, List<CharacterInfo> playerList = null)
    {
        isSetDestTile = true;
        bool isAllDead = true;
        if (playerList == null)
        {
            playerList = new List<CharacterInfo>();
            playerList.Add(player);
        }

        if (player != null)
        {
            foreach (var unit in playerList)
            {
                StartCoroutine(AttackPlayer(enemy, unit));
                StartCoroutine(skillEffectController.EffectTimingControl("none","none","normal",false,enemy.damage, 
                    enemy.transform.gameObject, player.transform.gameObject));
                player.GetComponent<CharacterControl>().Character.Blink();
                isCharacterDoAttack = true;
            }
        }
        
        isSetDestTile = false;
        StartCoroutine(EnemyTurnEnd());
    }
    
    private IEnumerator AttackPlayer(CharacterInfo enemy, CharacterInfo player)
    {
        bool isHit = IsHit(enemy, player);
        if (isHit)
        {
            Debug.Log("적이 플레이어를 공격했습니다.");
            switch (enemy.motion)
            {
                case Motion.Slash :
                    enemy.GetOrAddComponent<CharacterControl>().Slash((int)Math.Round(enemy.transform.position.x));
                    break;
                case Motion.Jab :
                    enemy.GetOrAddComponent<CharacterControl>().Jab((int)Math.Round(enemy.transform.position.x));
                    break;
                case Motion.Shot :
                    enemy.GetOrAddComponent<CharacterControl>().Shot((int)Math.Round(enemy.transform.position.x));
                    break;
                default:
                    Debug.Log("정의되지 않은 모션");
                    break;
            }
            
            cinemachineBrain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CameraShake>().ShakeCamera();
            
            bool isDodge = IsDodge(player);
            if (isDodge == false)
            {
                StartCoroutine(skillEffectController.EffectTimingControl("none","none","normal",false,enemy.damage, 
                    enemy.transform.gameObject, player.transform.gameObject));
                player.GetComponent<CharacterControl>().Character.Blink();
                yield return new WaitForSeconds(0.5f);
                player.hp -= CalculateDamage(enemy, player);
            }

            // 플레이어 사망 처리
            if (player.hp <= 0)
            {
                enemy.GetComponent<CharacterControl>().Character.SetState(AnimationState.Dead);
                yield return new WaitForSeconds(0.5f);
                Debug.Log("플레이어 사망");
                player.isDead = true;
                player.gameObject.SetActive(false);
                
                int idx = battleSystem.TurnOrderList.IndexOf(player);
                turnOrderUI.transform.GetChild(idx).GetComponent<Image>().sprite = Resources.Load<Sprite>("Img/dead");

                // 모든 용병이 사망했는지 확인
                bool isAllDead = true;
                foreach (var unit in battleSystem.TurnOrderList)
                {
                    if (unit.type == "Mercenary")
                    {
                        isAllDead = false;
                        break;
                    }
                }

                if (isAllDead)
                {
                    Debug.Log("용병단 전원 사망. 캠페인 종료.");
                }
            }
        }
        else
        {
            Debug.Log($"{enemy.name}의 공격이 빗나감.");
        }
    }
    
    private bool IsHit(CharacterInfo enemy, CharacterInfo player)
    {
        int random = Random.Range(1, 100);
        return random <= enemy.accuracy;
    }

    private int CalculateDamage(CharacterInfo enemy, CharacterInfo player)
    {
        int damage = enemy.damage - player.defense;
    
        int crit = Random.Range(0, 100);
        if (enemy.criticalPct < crit)
        {
            damage = (int)(enemy.damage * enemy.criticalDmg - player.defense);
        }

        return damage;
    }

    bool IsDodge(CharacterInfo player)
    {
        int rand = Random.Range(0, 100);
        if (player.dodge > rand)
        {
            Debug.Log($"{player.name}가 공격을 회피함.");
            player.GetComponent<CharacterControl>().Character.Blink();
            return true;
        }

        return false;
    }

    bool SkipTurn()
    {
        return false;
    }
}
