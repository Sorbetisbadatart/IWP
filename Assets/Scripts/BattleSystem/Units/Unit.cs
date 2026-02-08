using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public string unitName;
    public Sprite unitSprite;
    public BattleStats inheritedStats;

    // Current stats
    public int Level { get; private set; }
    private int currentHealth;
    public int HealthPoints
    {
        get => currentHealth;
        set => currentHealth = Mathf.Clamp(value, 0, MaxHealthPoints);
    }

    public int MaxHealthPoints { get; private set; }
    public int ManaPoints { get; private set; }
    public int MaxManaPoints { get; private set; }
    public int BaseAttack { get; private set; }
    public int BaseDefense { get; private set; }
    public int BaseAccuracy { get; private set; }

    // Experience
    public int CurrentExp { get; private set; }
    public int ExpToNextLevel { get; private set; }

    // Skills
    public List<Skill> availableSkills = new List<Skill>();
    public List<Skill> equippedSkills = new List<Skill>();
    private Dictionary<Skill, int> skillCooldowns = new Dictionary<Skill, int>();

    // Defense
    private bool isDefending = false;
    private float defenseMultiplier = 1f;
    private int defenseDuration = 0;

    // Status effects
    private List<ActiveStatus> activeStatuses = new List<ActiveStatus>();

    // Properties
    public bool IsDead => HealthPoints <= 0;
    public bool IsDefending => isDefending;
    public List<Skill> EquippedSkills => equippedSkills;
    public List<ActiveStatus> ActiveStatuses => activeStatuses;

    // Level up event
    public event System.Action<Unit, LevelUpResult> OnLevelUp;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        Level = inheritedStats.Level;
        CurrentExp = inheritedStats.currentExp;
        ExpToNextLevel = inheritedStats.CalculateExpForNextLevel(Level);

        CalculateStatsFromLevel();
        HealthPoints = MaxHealthPoints;
        ManaPoints = MaxManaPoints;

        // Initialize skills
        if (equippedSkills.Count == 0 && availableSkills.Count > 0)
        {
            for (int i = 0; i < Mathf.Min(4, availableSkills.Count); i++)
            {
                equippedSkills.Add(availableSkills[i]);
            }
        }

        LearnSkillsForCurrentLevel();
    }

    private void CalculateStatsFromLevel()
    {
        MaxHealthPoints = Mathf.RoundToInt(inheritedStats.MaxHealthPoints *
            Mathf.Pow(inheritedStats.healthGrowth, Level - 1));

        MaxManaPoints = Mathf.RoundToInt(inheritedStats.MaxManaPoints *
            Mathf.Pow(inheritedStats.manaGrowth, Level - 1));

        BaseAttack = Mathf.RoundToInt(inheritedStats.BaseAttack *
            Mathf.Pow(inheritedStats.attackGrowth, Level - 1));

        BaseDefense = Mathf.RoundToInt(inheritedStats.BaseDefense *
            Mathf.Pow(inheritedStats.defenseGrowth, Level - 1));

        BaseAccuracy = Mathf.RoundToInt(inheritedStats.BaseAccuracy *
            Mathf.Pow(inheritedStats.accuracyGrowth, Level - 1));

        BaseAccuracy = Mathf.Clamp(BaseAccuracy, 1, 100);
    }

    private void LearnSkillsForCurrentLevel()
    {
        if (inheritedStats.skillsLearnedAtLevel == null) return;

        foreach (var levelSkill in inheritedStats.skillsLearnedAtLevel)
        {
            if (levelSkill.learnAtLevel <= Level && !availableSkills.Contains(levelSkill.skill))
            {
                LearnSkill(levelSkill.skill);
            }
        }
    }

    public void AddExperience(int expAmount)
    {
        if (Level >= inheritedStats.maxLevel) return;

        CurrentExp += expAmount;

        while (CurrentExp >= ExpToNextLevel && Level < inheritedStats.maxLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        CurrentExp -= ExpToNextLevel;
        Level++;

        var oldStats = new LevelUpResult.OldStats
        {
            level = Level - 1,
            maxHealth = MaxHealthPoints,
            maxMana = MaxManaPoints,
            attack = BaseAttack,
            defense = BaseDefense,
            accuracy = BaseAccuracy
        };

        CalculateStatsFromLevel();
        HealthPoints = MaxHealthPoints;
        ManaPoints = MaxManaPoints;
        ExpToNextLevel = inheritedStats.CalculateExpForNextLevel(Level);

        List<Skill> newlyLearnedSkills = new List<Skill>();
        if (inheritedStats.skillsLearnedAtLevel != null)
        {
            foreach (var levelSkill in inheritedStats.skillsLearnedAtLevel)
            {
                if (levelSkill.learnAtLevel == Level && !availableSkills.Contains(levelSkill.skill))
                {
                    LearnSkill(levelSkill.skill);
                    newlyLearnedSkills.Add(levelSkill.skill);
                }
            }
        }

        LevelUpResult result = new LevelUpResult
        {
            newLevel = Level,
            oldStats = oldStats,
            newMaxHealth = MaxHealthPoints,
            newMaxMana = MaxManaPoints,
            newAttack = BaseAttack,
            newDefense = BaseDefense,
            newAccuracy = BaseAccuracy,
            learnedSkills = newlyLearnedSkills
        };

        OnLevelUp?.Invoke(this, result);
    }

    public void LearnSkill(Skill skill)
    {
        if (!availableSkills.Contains(skill))
        {
            availableSkills.Add(skill);
        }
    }

    public float GetExpPercentage()
    {
        if (Level >= inheritedStats.maxLevel) return 1f;
        return (float)CurrentExp / ExpToNextLevel;
    }

    // ==================== COMBAT METHODS ====================

    public bool TakeDamage(int dmg)
    {
        if (IsDead) return true;

        int finalDamage = dmg;

        if (isDefending)
        {
            int damageAfterBaseDefense = Mathf.Max(1, dmg - BaseDefense);
            float damageAfterMultiplier = damageAfterBaseDefense * defenseMultiplier;
            finalDamage = Mathf.CeilToInt(damageAfterMultiplier);
        }
        else
        {
            finalDamage = Mathf.Max(1, dmg - BaseDefense);
        }

        HealthPoints -= finalDamage;

        if (isDefending)
        {
            defenseDuration--;

            if (defenseDuration <= 0)
            {
                EndDefense();
            }
        }
        return IsDead;
    }

    public void Heal(int amount)
    {
        if (!IsDead)
        {
            HealthPoints += amount;
        }
    }

    public void StartDefending(float reductionPercent = 0.5f, int duration = 1)
    {
        isDefending = true;
        defenseMultiplier = 1f - reductionPercent;
        defenseDuration = duration;
    }

    public void EndDefense()
    {
        isDefending = false;
        defenseMultiplier = 1f;
        defenseDuration = 0;
    }

    public bool CanUseSkill(Skill skill)
    {
        if (IsDead) return false;
        if (skill.manaCost > ManaPoints) return false;
        if (skill.healthCost > HealthPoints) return false;
        if (skillCooldowns.ContainsKey(skill) && skillCooldowns[skill] > 0) return false;

        return true;
    }

    public SkillResult UseSkill(Skill skill, Unit target = null)
    {
        SkillResult result = new SkillResult();

        if (!CanUseSkill(skill))
        {
            result.success = false;
            result.message = "Cannot use skill!";
            return result;
        }

        ManaPoints -= skill.manaCost;
        if (skill.healthCost > 0)
        {
            HealthPoints -= skill.healthCost;
        }

        if (skill.cooldownTurns > 0)
        {
            skillCooldowns[skill] = skill.cooldownTurns;
        }

        switch (skill.skillType)
        {
            case SkillType.DAMAGE:
                result = ExecuteDamageSkill(skill, target);
                break;

            case SkillType.HEAL:
                result = ExecuteHealSkill(skill, target);
                break;

            default:
                result.success = true;
                result.message = $"{skill.skillName} was used!";
                break;
        }

        return result;
    }

    private SkillResult ExecuteDamageSkill(Skill skill, Unit target)
    {
        SkillResult result = new SkillResult();

        if (Random.Range(0, 100) > skill.accuracy)
        {
            result.message = $"{skill.skillName} missed!";
            return result;
        }

        int damage = skill.CalculateDamage(this, target);
        bool isCritical = Random.Range(0, 100) < skill.criticalChance;

        if (isCritical)
        {
            damage = Mathf.RoundToInt(damage * 1.5f);
            result.message = $"{skill.skillName} hit critically!";
        }

        bool isDead = target.TakeDamage(damage);

        result.damageDealt = damage;
        result.wasCritical = isCritical;
        result.targetKilled = isDead;

        return result;
    }

    private SkillResult ExecuteHealSkill(Skill skill, Unit target)
    {
        SkillResult result = new SkillResult();

        int healing = skill.CalculateHealing(this, target);
        target.Heal(healing);

        result.healingDone = healing;
        result.message = $"{skill.skillName} healed {healing} HP!";

        return result;
    }

    public void ResetUnit()
    {
        Initialize();
        isDefending = false;
        defenseMultiplier = 1f;
        defenseDuration = 0;
        activeStatuses.Clear();
        skillCooldowns.Clear();
    }
}

public class LevelUpResult
{
    public struct OldStats
    {
        public int level;
        public int maxHealth;
        public int maxMana;
        public int attack;
        public int defense;
        public int accuracy;
    }

    public int newLevel;
    public OldStats oldStats;
    public int newMaxHealth;
    public int newMaxMana;
    public int newAttack;
    public int newDefense;
    public int newAccuracy;
    public List<Skill> learnedSkills;

    public StatIncreases GetStatIncreases()
    {
        return new StatIncreases
        {
            healthIncrease = newMaxHealth - oldStats.maxHealth,
            manaIncrease = newMaxMana - oldStats.maxMana,
            attackIncrease = newAttack - oldStats.attack,
            defenseIncrease = newDefense - oldStats.defense,
            accuracyIncrease = newAccuracy - oldStats.accuracy
        };
    }
}

public struct StatIncreases
{
    public int healthIncrease;
    public int manaIncrease;
    public int attackIncrease;
    public int defenseIncrease;
    public int accuracyIncrease;
}