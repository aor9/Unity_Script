using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Heal,
    Attack,
    Buff,
    Debuff
}

public enum SkillTarget
{
    Single,
    Multi
}

public enum SkillUser
{
    Marshall,
    Ranger,
    Skillmonkey,
    Cleaner
}

public enum SkillPool
{
    Basic,
    Growth
}

[CreateAssetMenu]
public class Skill : ScriptableObject
{
    // skill type : heal, attack, buff, debuff
    // target : single, multi
    // skill user : marshall, ranger, skillmonkey, cleaner
    // skill pool : basic, growth
    
    public string skillname;
    public SkillType skillType;
    public SkillTarget target;
    public SkillUser skillUser;
    public SkillPool skillPool;
    public string skillInfo;
    public string skillEffectName;
    public string hitEffectName;
    public string skillAttackType;
    public int sp;
    
    public int damage;
    public float percentDamage;
    
    // range type : cross, diagonal, square
    // center point : self, clicked
    // specialEffect : none, drained
    public int range;
    public string rangeType;
    public string centerPoint;
    public string specialEffect;
    
    // buff, debuff 일때 영향을주는 stat
    public string targetStat;

    public string animationName;
    public Sprite icon,rangeIcon;
}
