using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EnemyAI
{
    private bool isElitePerforming = false;
    
    private void PerformEliteAction(CharacterInfo character)
    {
        EnemyInfo enemy = (EnemyInfo)character;
        isElitePerforming = false;
        
        if (enemy.sp >= enemy.skill.sp)
        {
            switch (enemy.skill.skillType)
            {
                case SkillType.Attack:
                    isElitePerforming = HandleAttackSkill(enemy);
                    break;
                case SkillType.Heal:
                    isElitePerforming = HandleHealSkill(enemy);
                    break;
                case SkillType.Buff:
                    isElitePerforming = HandleBuffSkill(enemy);
                    break;
                case SkillType.Debuff:
                    isElitePerforming = HandleDebuffSkill(enemy);
                    break;
            }
        }
        else
        {
            CharacterInfo target = SetEliteEnemyTarget(enemy, false);
            EnemyAttack(enemy, target);
            if (target != null)
            {
                EnemyAttack(enemy, target);
                isElitePerforming = true;
            }
        }
        
        
        if (isElitePerforming)
        {
            actionFlag = true;
        }
        else
        {
            Debug.Log("공격을 하거나 스킬을 사용하지 않음.");
        }
    }

    private bool HandleAttackSkill(EnemyInfo enemy)
    {
        // 적에게 공격스킬이 닿는지 판별하고 닿는다면 공격스킬을 사용
        if (enemy.skill.target == SkillTarget.Single)
        {
            CharacterInfo target = SetEliteEnemyTarget(enemy, true);
            if (target != null)
            {
                EnemyAttack(enemy, target);
                return true;
            }
            
        }
        else if (enemy.skill.target == SkillTarget.Multi)
        {
            List<CharacterInfo> targets = SetEliteEnemyTargets(enemy);
            if (targets.Count > 0)
            {
                EnemyAttack(enemy, null, targets);
                return true;
            }
        }
        
        return false;
    }
    private bool HandleHealSkill(EnemyInfo enemy)
    {
        if (enemy.hp <= enemy.maxHp * 0.3)
        {
            enemy.hp += enemy.skill.damage;
            enemy.sp -= enemy.skill.sp;
            return true;
        }

        return false;
    }
    private bool HandleBuffSkill(EnemyInfo enemy)
    {
        // buff 를 사용할 상태인지 판별
        
        return false;
    }
    private bool HandleDebuffSkill(EnemyInfo enemy)
    {
        // 적에게 Debuff를 걸지 말지 판별
        
        return false;
    }
    

    private CharacterInfo SetEliteEnemyTarget(CharacterInfo enemy, bool isSkill)
    {
        // 공격 범위 내에서 가장 피가 적은 타겟 하나 선정
        List<CharacterInfo> inRangePlayers = new List<CharacterInfo>();
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

    private List<CharacterInfo> SetEliteEnemyTargets(CharacterInfo enemy)
    {
        // 범위 공격은 무조건 스킬. 공격 범위 내의 타겟들을 모두 선정
        return null;
    }
}
