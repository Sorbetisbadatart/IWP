using UnityEngine;

[CreateAssetMenu(fileName = "New BattleStats", menuName = "Battle/Battle Stats")]
public class BattleStats : ScriptableObject
{
    [Header("Level & Experience")]
    public int Level = 1;
    public int maxLevel = 100;
    public int currentExp = 0;
    public int expToNextLevel = 100;

    [Header("Base Stats")]
    public int MaxHealthPoints = 100;
    public int MaxManaPoints = 50;
    public int BaseAttack = 10;
    public int BaseDefense = 5;
    public int BaseAccuracy = 95;

    [Header("Growth Rates (per level)")]
    [Range(0.5f, 2f)] public float healthGrowth = 1.1f;
    [Range(0.5f, 2f)] public float manaGrowth = 1.05f;
    [Range(0.5f, 2f)] public float attackGrowth = 1.1f;
    [Range(0.5f, 2f)] public float defenseGrowth = 1.05f;
    [Range(0.5f, 2f)] public float accuracyGrowth = 1.01f;

    [Header("Experience Curve")]
    [Range(1f, 2f)] public float expGrowthMultiplier = 1.5f;
    public int baseExpReward = 200;

    [Header("Skills Learned by Level")]
    public LevelUpSkill[] skillsLearnedAtLevel;

    /// <summary>
    /// Calculate experience needed for the next level
    /// </summary>
    public int CalculateExpForNextLevel(int currentLevel)
    {
        return Mathf.RoundToInt(expToNextLevel * Mathf.Pow(expGrowthMultiplier, currentLevel - 1));
    }

    /// <summary>
    /// Get experience reward for defeating this unit
    /// </summary>
    public int GetExpReward()
    {
        return Mathf.RoundToInt(baseExpReward * (1 + (Level * 0.1f)));
    }
}

[System.Serializable]
public class LevelUpSkill
{
    public Skill skill;
    public int learnAtLevel;
}