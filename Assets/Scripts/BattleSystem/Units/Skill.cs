using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Skill", menuName = "Battle/Skill")]
public class Skill : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    public string description;
    public Sprite icon;
    public SkillType skillType;
    public TargetType targetType;

    [Header("Requirements")]
    public int manaCost = 0;
    public int healthCost = 0;
    public int cooldownTurns = 0;

    [Header("Effects")]
    public int basePower = 10;
    [Range(0, 100)] public int accuracy = 100;
    [Range(0, 100)] public int criticalChance = 5;

    public int CalculateDamage(Unit user, Unit target)
    {
        float damage = basePower + user.BaseAttack;

        // Apply target defense
        damage = Mathf.Max(1, damage - target.BaseDefense);

        // Random variation
        damage *= Random.Range(0.9f, 1.1f);

        return Mathf.RoundToInt(damage);
    }

    public int CalculateHealing(Unit user, Unit target)
    {
        float healing = basePower + user.BaseAttack;
        healing *= Random.Range(0.9f, 1.1f);
        return Mathf.RoundToInt(healing);
    }
}

public enum SkillType
{
    DAMAGE,
    HEAL,
    BUFF,
    DEBUFF
}

public enum TargetType
{
    SELF,
    SINGLE_ENEMY,
    SINGLE_ALLY,
    ALL_ENEMIES,
    ALL_ALLIES
}

public class SkillResult
{
    public bool success = false;
    public string message = "";
    public int damageDealt = 0;
    public int healingDone = 0;
    public bool wasCritical = false;
    public bool targetKilled = false;
}