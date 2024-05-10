using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.ExampleScripts;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using AnimationState = Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts.AnimationState;
using Random = UnityEngine.Random;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField]
    private DungeonManager dungeonManager;
    [SerializeField] 
    private BattleSystem battleSystem;
    [SerializeField]
    private PlayerController playerController;
    [SerializeField] 
    private TurnOrderUI turnOrderUI;
    [SerializeField] 
    private BasicCameraMovement cameraMovement;
    [SerializeField] 
    private FadeInOut fadeInOut;
    [SerializeField] 
    private CinemachineBrain cinemachineBrain;
    
    private PlayerInfo currentPlayer;

    public Skill currentSkill;
    public List<OverlayTile> targetTiles;
    public List<OverlayTile> attackRangeTiles;
    public SkillEffectController skillEffectController;
    public UIPopupManager uiPopupManager;
    public WaitForSeconds wfs;


    private void Start()
    {
        attackRangeTiles = new List<OverlayTile>();
        targetTiles = new List<OverlayTile>();
        wfs = new WaitForSeconds(0.1f);
    }

    public List<OverlayTile> SetAttack(CharacterInfo player, Skill skill, List<OverlayTile> inRangeTiles)
    {
        attackRangeTiles = inRangeTiles;
        currentPlayer = (PlayerInfo)player;

        if (skill is not null)
        {
            currentSkill = skill;
            if (skill.centerPoint == "self")
            {
                if (battleSystem.actionState == ActionState.ATTACK)
                {
                    attackRangeTiles = dungeonManager.rangeFinder.GetTilesByCoordinate(player.standingOnTile,
                        skill.range,
                        skill.rangeType);
                }
            }
            else
            {
                foreach (var item in attackRangeTiles)
                {
                    item.HideTile();
                }

                if (battleSystem.actionState == ActionState.ATTACK)
                {
                    attackRangeTiles = dungeonManager.rangeFinder.GetTilesInRange(player.standingOnTile, skill.range);
                }
            }
        }
        else
        {
            foreach (var item in attackRangeTiles)
            {
                item.HideTile();
            }

            if (battleSystem.actionState == ActionState.ATTACK)
            {
                attackRangeTiles = dungeonManager.rangeFinder.GetTilesInRange(player.standingOnTile, player.range);
            }
        }

        foreach (var item in attackRangeTiles)
        {
            item.ShowTile(battleSystem.actionState);
        }
        
        return attackRangeTiles;
    }

    public List<OverlayTile> SetSkillRangeTiles(OverlayTile clickedTile)
    {
        if (battleSystem.actionState == ActionState.ATTACK)
        {
            attackRangeTiles =
                dungeonManager.rangeFinder.GetTilesByCoordinate(clickedTile, currentSkill.range,
                    currentSkill.rangeType);
        }

        foreach (var item in attackRangeTiles)
        {
            item.ShowSkillTile();
        }

        return attackRangeTiles;
    }

    public void HandleSingleTargetSkill(OverlayTile clickedTile)
    {
        targetTiles.Clear();
        
        targetTiles.Add(clickedTile);
        
        ShowTargetTiles();
    }

    public void HandleMultiTargetSkill(List<OverlayTile> skillRangeTiles)
    {
        targetTiles.Clear();
        
        targetTiles.AddRange(skillRangeTiles);
        
        ShowTargetTiles();
    }
    
    public IEnumerator PerformAttack()
    {
        var enemiesOnTile = battleSystem.TurnOrderList.Where(enemy => enemy.type != "Player" && targetTiles.Contains(enemy.standingOnTile)).ToList();
        
        foreach (var spawner in dungeonManager.SpawnerList)
        {
            if (targetTiles.Contains(spawner.standingOnTile))
            {
                enemiesOnTile.Add(spawner);
            }
        }

        if (enemiesOnTile.Count > 0)
        {
            StartCoroutine(HideHud());
            yield return wfs;
            cameraMovement.isActionStart = true;
            yield return new WaitForSeconds(0.5f);
        }

        foreach (var enemy in enemiesOnTile)
        {
            int random = Random.Range(0, 100);
            string skillEffectname, hitEffectName, effectType;
            bool isCrit = false;

            switch (currentPlayer.motion)
            {
                case Motion.Slash :
                    currentPlayer.GetOrAddComponent<CharacterControl>().Slash((int)Math.Round(enemy.transform.position.x));
                    break;
                case Motion.Jab :
                    currentPlayer.GetOrAddComponent<CharacterControl>().Jab((int)Math.Round(enemy.transform.position.x));
                    break;
                case Motion.Shot :
                    currentPlayer.GetOrAddComponent<CharacterControl>().Shot((int)Math.Round(enemy.transform.position.x));
                    break;
                default:
                    Debug.Log("정의되지 않은 모션");
                    break;
            }
            
            cinemachineBrain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CameraShake>().ShakeCamera();

            if (random <= currentPlayer.accuracy)
            {
                double attackDamage = 0;

                //일반 공격시
                if (currentSkill is null)
                {
                    //TODO: 치명타 여부 판별하는 코드 만들기
                    if (random <= 0)
                    {
                        isCrit = true;
                        attackDamage = currentPlayer.criticalDmg;
                    }
                    else
                    {
                        attackDamage = currentPlayer.damage;
                    }

                    effectType = currentPlayer.attackType;
                    skillEffectname = "none";
                    hitEffectName = "none";
                }
                //스킬 사용시
                else
                {
                    Debug.Log("스킬 사용 !!");
                    if (random <= 50)
                    {
                        isCrit = true;
                        attackDamage = currentSkill.damage * 1.5;
                    }
                    else
                    {
                        attackDamage = currentSkill.damage;
                    }

                    effectType = currentSkill.skillAttackType;
                    skillEffectname = currentSkill.skillEffectName;
                    hitEffectName = currentSkill.hitEffectName;
                }
                
                //공격,데미지 표시 이펙트
                StartCoroutine(skillEffectController.EffectTimingControl(skillEffectname,hitEffectName,effectType,isCrit,attackDamage, 
                    currentPlayer.transform.gameObject, enemy.transform.gameObject));
                enemy.GetComponent<CharacterControl>().Character.Blink();

                Debug.Log("공격 성공.");
                
                yield return new WaitForSeconds(0.5f);
                
                enemy.hp -= (float)attackDamage;
                
                if (enemy.hp <= 0)
                {
                    enemy.GetComponent<CharacterControl>().Character.SetState(AnimationState.Dead);
                    yield return new WaitForSeconds(0.5f);
                    enemy.gameObject.SetActive(false);
                    enemy.isDead = true;
                    
                    Debug.Log("적 사망");
                }
            }
            else
            {
                Debug.Log("공격 실패.");
            }
        }
        
        yield return new WaitForSeconds(0.2f);
        cameraMovement.isActionEnd = true;
        StartCoroutine(ShowHUD());
    }

    public void PerformHeal()
    {
        string skillEffectname, hitEffectName, effectType;
        effectType = currentSkill.skillAttackType;
        skillEffectname = currentSkill.skillEffectName;
        hitEffectName = currentSkill.hitEffectName;
        //힐 이펙트
        StartCoroutine(skillEffectController.EffectTimingControl(skillEffectname,hitEffectName,effectType,false,-1, 
            currentPlayer.transform.gameObject, null));
        //힐 효과
        if (currentPlayer.hp < currentPlayer.maxHp)
        {
            if (currentPlayer.maxHp - currentPlayer.hp > currentSkill.damage)
            {
                currentPlayer.hp += currentSkill.damage;
            }
            else
            {
                currentPlayer.hp = currentPlayer.maxHp;
            }
        } 

    }

    public void PerformBuff()
    {
        string skillEffectname, hitEffectName, effectType;
        effectType = currentSkill.skillAttackType;
        skillEffectname = currentSkill.skillEffectName;
        hitEffectName = currentSkill.hitEffectName;
        //버프 이펙트
        StartCoroutine(skillEffectController.EffectTimingControl(skillEffectname,hitEffectName,effectType,false,-1, 
            currentPlayer.transform.gameObject, null));
        //버프 효과
        currentPlayer.damage += 5;
    }

    public void PerformDeBuff()
    {
        
    }
    
    private void ShowTargetTiles()
    {
        if (targetTiles == null)
        {
            Debug.Log("targetTiles Null !!");
        }
        else
        {
            foreach (var item in playerController.inRangeTiles)
            {
                item.ShowTile(ActionState.ATTACK);
            }
            
            foreach (var item in targetTiles)
            {
                item.ShowTile(ActionState.DEFAULT);
            }
        }
    }

    IEnumerator ShowHUD()
    {
        yield return wfs;
        fadeInOut.FadeIn(0.1f);
    }

    IEnumerator HideHud()
    {
        yield return wfs;
        fadeInOut.FadeOut(0.1f);
    }

}

    

