using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum BattleState
{
    START,
    PLAYERTURN,
    ENEMYTURN,
    WON,
    LOST,
    SELECTING_TARGET,
    LEVEL_UP
}

public class BattleSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject BattleUI;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private BattleHUD playerHUD;
    [SerializeField] private BattleHUD enemyHUD;
    [SerializeField] private GameObject ActionBox;
    [SerializeField] private GameObject skillPanel;
    [SerializeField] private Button[] skillButtons;
    [SerializeField] private GameObject targetSelectionPanel;
    [SerializeField] private Button[] targetButtons;

    [Header("Level Up UI")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private TMP_Text levelUpText;
    [SerializeField] private TMP_Text statIncreaseText;
    [SerializeField] private GameObject skillLearnedPanel;
    [SerializeField] private TMP_Text skillLearnedText;
    [SerializeField] private Image skillLearnedIcon;
    [SerializeField] private float levelUpDisplayTime = 3f;

    [Header("Battle Settings")]
    [SerializeField] private float StartTransitionTime = 1f;
    [SerializeField] private float AttackBufferTime = 1f;

    [Header("Defense Settings")]
    [SerializeField, Range(0, 1)] private float defenseDamageReduction = 0.5f;
    [SerializeField] private int defenseDuration = 1;

    [Header("Sound Effects")]
    [SerializeField] private bool enableSFX = true;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip healSound;
    [SerializeField] private AudioClip defenseSound;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip defeatSound;
    [SerializeField] private AudioClip levelUpSound;

    [Header("Visual Effects")]
    [SerializeField] private bool enableVisualEffects = true;

    // Private members
    private Unit playerUnit;
    private Unit enemyUnit;
    private BattleState currentstate;

    // Skill selection
    private Skill selectedSkill;
    private List<Unit> availableTargets = new List<Unit>();

    // Experience tracking
    private int expGainedThisBattle = 0;
    private bool isShowingLevelUp = false;
    private Queue<LevelUpResult> levelUpQueue = new Queue<LevelUpResult>();

    // Audio
    private AudioSource audioSource;

    // Events
    public UnityEvent OnBattleStart;
    public UnityEvent OnBattleEnd;
    public UnityEvent OnPlayerWin;
    public UnityEvent OnPlayerLose;
    public UnityEvent<LevelUpResult> OnLevelUp;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        // Setup button listeners
        if (targetButtons != null)
        {
            for (int i = 0; i < targetButtons.Length; i++)
            {
                int index = i;
                targetButtons[i].onClick.AddListener(() => OnTargetSelected(index));
            }
        }

        if (skillButtons != null)
        {
            for (int i = 0; i < skillButtons.Length; i++)
            {
                int index = i;
                skillButtons[i].onClick.AddListener(() => OnSkillSelected(index));
            }
        }

        HideTargetSelection();
        HideSkillPanel();
        HideLevelUpPanels();
    }

    public void StartBattle(Unit player, Unit enemy)
    {
        playerUnit = player;
        enemyUnit = enemy;

        playerUnit.ResetUnit();
        enemyUnit.ResetUnit();

        // Subscribe to level up event
        playerUnit.OnLevelUp += OnPlayerLevelUp;

        ShiftBattleState(BattleState.START);
        BattleUI.SetActive(true);
        OnBattleStart?.Invoke();
    }

    private IEnumerator SetupBattle()
    {
        dialogueText.text = "A wild " + enemyUnit.unitName + " appears!";

        playerHUD.SetHUD(playerUnit);
        enemyHUD.SetHUD(enemyUnit);

        UpdateSkillButtons();

        yield return new WaitForSeconds(StartTransitionTime);

        ShiftBattleState(BattleState.PLAYERTURN);
    }

    private void ShiftBattleState(BattleState newState)
    {
        currentstate = newState;

        switch (currentstate)
        {
            case BattleState.START:
                StartCoroutine(SetupBattle());
                break;

            case BattleState.PLAYERTURN:
                StartPlayerTurn();
                break;

            case BattleState.ENEMYTURN:
                StartCoroutine(EnemyTurn());
                break;

            case BattleState.SELECTING_TARGET:
                ShowTargetSelection();
                break;

            case BattleState.WON:
                StartCoroutine(EndBattle(true));
                break;

            case BattleState.LOST:
                StartCoroutine(EndBattle(false));
                break;

            case BattleState.LEVEL_UP:
                ProcessNextLevelUp();
                break;
        }
    }

    private void StartPlayerTurn()
    {
       
        dialogueText.text = "Choose an action:";
        ActionBox.SetActive(true);
        UpdateSkillButtons();
    }

    private IEnumerator EnemyTurn()
    {
        ActionBox.SetActive(false);
        HideSkillPanel();

        dialogueText.text = enemyUnit.unitName + " attacks!";
        yield return new WaitForSeconds(AttackBufferTime / 2);

        if (enableSFX && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        bool isDead = playerUnit.TakeDamage(enemyUnit.BaseAttack);

        if (enableVisualEffects)
        {
            playerHUD.TriggerDamageEffect();
        }

        if (enableSFX && damageSound != null && !playerUnit.IsDefending)
        {
            audioSource.PlayOneShot(damageSound);
        }

        playerHUD.SetHP(playerUnit.HealthPoints);

        yield return new WaitForSeconds(AttackBufferTime / 2);

        if (playerUnit.IsDefending)
        {
            playerUnit.EndDefense();
            playerHUD.SetDefenseState(false);
        }

        if (isDead)
        {
            ShiftBattleState(BattleState.LOST);
        }
        else
        {
            ShiftBattleState(BattleState.PLAYERTURN);
        }
    }

    private IEnumerator EndBattle(bool won)
    {
        ActionBox.SetActive(false);
        HideSkillPanel();
        HideTargetSelection();

        if (won)
        {
            dialogueText.text = "You won the battle!";

            // Award experience
            int expReward = enemyUnit.inheritedStats.GetExpReward();
            expGainedThisBattle = expReward;
            playerUnit.AddExperience(expReward);

            // Show experience gained
            yield return StartCoroutine(ShowExpGained(expReward));

            // Check if we need to show level ups
            if (levelUpQueue.Count > 0)
            {
                ShiftBattleState(BattleState.LEVEL_UP);
            }
            else
            {
                if (enableSFX && victorySound != null)
                {
                    audioSource.PlayOneShot(victorySound);
                }
                OnPlayerWin?.Invoke();
                yield return new WaitForSeconds(2f);
                BattleUI.SetActive(false);
                OnBattleEnd?.Invoke();
            }
        }
        else
        {
            dialogueText.text = "You were defeated...";
            if (enableSFX && defeatSound != null)
            {
                audioSource.PlayOneShot(defeatSound);
            }
            OnPlayerLose?.Invoke();
            yield return new WaitForSeconds(2f);
            BattleUI.SetActive(false);
            OnBattleEnd?.Invoke();
        }
    }

    private IEnumerator ShowExpGained(int expAmount)
    {
        string originalText = dialogueText.text;
        dialogueText.text = $"{originalText}\nGained {expAmount} EXP!";


        yield return new WaitForSeconds(1.5f);

        if (!isShowingLevelUp && levelUpQueue.Count == 0)
        {
            dialogueText.text = originalText;
        }
    }

    // ==================== LEVEL UP HANDLING ====================

    private void OnPlayerLevelUp(Unit unit, LevelUpResult result)
    {
        levelUpQueue.Enqueue(result);
    }

    private void ProcessNextLevelUp()
    {
        if (levelUpQueue.Count > 0)
        {
            LevelUpResult result = levelUpQueue.Dequeue();
            StartCoroutine(ShowLevelUpSequence(result));
        }
        else
        {
            // No more level ups, finish battle
            if (enableSFX && victorySound != null)
            {
                audioSource.PlayOneShot(victorySound);
            }
            OnPlayerWin?.Invoke();
            StartCoroutine(FinishBattleAfterLevelUps());
        }
    }

    private IEnumerator ShowLevelUpSequence(LevelUpResult result)
    {
        isShowingLevelUp = true;

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);

            levelUpText.text = $"{playerUnit.unitName} reached Level {result.newLevel}!";

            // Show stat increases
            StatIncreases increases = result.GetStatIncreases();
            string statText = "";

            if (increases.healthIncrease > 0)
                statText += $"HP +{increases.healthIncrease}\n";
            if (increases.manaIncrease > 0)
                statText += $"MP +{increases.manaIncrease}\n";
            if (increases.attackIncrease > 0)
                statText += $"ATK +{increases.attackIncrease}\n";
            if (increases.defenseIncrease > 0)
                statText += $"DEF +{increases.defenseIncrease}\n";
            if (increases.accuracyIncrease > 0)
                statText += $"ACC +{increases.accuracyIncrease}\n";

            statIncreaseText.text = statText;

            if (enableSFX && levelUpSound != null)
            {
                audioSource.PlayOneShot(levelUpSound);
            }

            if (enableVisualEffects && playerHUD != null)
            {
                playerHUD.TriggerLevelUpEffect();
            }

            OnLevelUp?.Invoke(result);

            yield return new WaitForSeconds(levelUpDisplayTime);

            // Show learned skills
            if (result.learnedSkills != null && result.learnedSkills.Count > 0)
            {
                foreach (Skill skill in result.learnedSkills)
                {
                    yield return StartCoroutine(ShowSkillLearned(skill));
                }
            }

            levelUpPanel.SetActive(false);

            // Update HUD
            if (playerHUD != null)
            {
                playerHUD.SetHUD(playerUnit);
                playerHUD.UpdateExpBar(playerUnit.GetExpPercentage());
            }

            UpdateSkillButtons();
        }

        isShowingLevelUp = false;

        // Process next level up if there are more
        if (levelUpQueue.Count > 0)
        {
            ShiftBattleState(BattleState.LEVEL_UP);
        }
        else
        {
            // All level ups shown, finish battle
            ShiftBattleState(BattleState.WON);
        }
    }

    private IEnumerator ShowSkillLearned(Skill skill)
    {
        if (skillLearnedPanel != null)
        {
            skillLearnedPanel.SetActive(true);
            skillLearnedText.text = $"Learned {skill.skillName}!";

            if (skillLearnedIcon != null && skill.icon != null)
            {
                skillLearnedIcon.sprite = skill.icon;
            }

            yield return new WaitForSeconds(2f);
            skillLearnedPanel.SetActive(false);
        }
    }

    private IEnumerator FinishBattleAfterLevelUps()
    {
        yield return new WaitForSeconds(1f);
        dialogueText.text = "Battle Complete!";
        yield return new WaitForSeconds(2f);
        BattleUI.SetActive(false);
        OnBattleEnd?.Invoke();
    }

    private void HideLevelUpPanels()
    {
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
        if (skillLearnedPanel != null) skillLearnedPanel.SetActive(false);
    }

    // ==================== PLAYER ACTIONS ====================

    public void OnAttackButton()
    {
        if (currentstate != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerAttack());
    }

    private IEnumerator PlayerAttack()
    {
        ActionBox.SetActive(false);

        dialogueText.text = playerUnit.unitName + " attacks!";
        yield return new WaitForSeconds(AttackBufferTime / 2);

        if (enableSFX && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        bool isDead = enemyUnit.TakeDamage(playerUnit.BaseAttack);

        if (enableVisualEffects)
        {
            enemyHUD.TriggerDamageEffect();
        }

        if (enableSFX && damageSound != null && !enemyUnit.IsDefending)
        {
            audioSource.PlayOneShot(damageSound);
        }

        enemyHUD.SetHP(enemyUnit.HealthPoints);

        yield return new WaitForSeconds(AttackBufferTime / 2);

        if (enemyUnit.IsDefending)
        {
            enemyUnit.EndDefense();
            enemyHUD.SetDefenseState(false);
        }

        if (isDead)
        {
            ShiftBattleState(BattleState.WON);
        }
        else
        {
            ShiftBattleState(BattleState.ENEMYTURN);
        }
    }

    public void OnDefendButton()
    {
        if (currentstate != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerDefend());
    }

    private IEnumerator PlayerDefend()
    {
        ActionBox.SetActive(false);

        dialogueText.text = playerUnit.unitName + " defends!";

        playerUnit.StartDefending(defenseDamageReduction, defenseDuration);
        playerHUD.SetDefenseState(true);

        if (enableSFX && defenseSound != null)
        {
            audioSource.PlayOneShot(defenseSound);
        }

        yield return new WaitForSeconds(AttackBufferTime);

        ShiftBattleState(BattleState.ENEMYTURN);
    }

    public void OnHealButton()
    {
        if (currentstate != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerHeal());
    }

    private IEnumerator PlayerHeal()
    {
        ActionBox.SetActive(false);

        int healAmount = Mathf.RoundToInt(playerUnit.MaxHealthPoints * 0.3f);
        dialogueText.text = playerUnit.unitName + " uses a healing potion!";

        playerUnit.Heal(healAmount);
        playerHUD.SetHP(playerUnit.HealthPoints);

        if (enableVisualEffects)
        {
            playerHUD.TriggerHealEffect();
        }

        if (enableSFX && healSound != null)
        {
            audioSource.PlayOneShot(healSound);
        }

        yield return new WaitForSeconds(AttackBufferTime);

        ShiftBattleState(BattleState.ENEMYTURN);
    }

    public void OnSkillsButton()
    {
        if (currentstate != BattleState.PLAYERTURN)
            return;

        ActionBox.SetActive(false);
        ShowSkillPanel();
    }

    private void OnSkillSelected(int skillIndex)
    {
        if (playerUnit.EquippedSkills == null || skillIndex >= playerUnit.EquippedSkills.Count)
            return;

        selectedSkill = playerUnit.EquippedSkills[skillIndex];

        if (selectedSkill == null)
            return;

        if (!playerUnit.CanUseSkill(selectedSkill))
        {
            dialogueText.text = "Cannot use " + selectedSkill.skillName + "!";
            return;
        }

        if (selectedSkill.targetType == TargetType.SINGLE_ENEMY)
        {
            availableTargets.Clear();
            availableTargets.Add(enemyUnit);
            ShiftBattleState(BattleState.SELECTING_TARGET);
        }
        else if (selectedSkill.targetType == TargetType.SELF)
        {
            HideSkillPanel();
            StartCoroutine(UseSkill(playerUnit));
        }
    }

    private void OnTargetSelected(int targetIndex)
    {
        if (targetIndex >= 0 && targetIndex < availableTargets.Count)
        {
            Unit target = availableTargets[targetIndex];
            HideTargetSelection();
            StartCoroutine(UseSkill(target));
        }
    }

    private IEnumerator UseSkill(Unit target)
    {
        dialogueText.text = playerUnit.unitName + " uses " + selectedSkill.skillName + "!";
        yield return new WaitForSeconds(AttackBufferTime / 2);

        SkillResult result = playerUnit.UseSkill(selectedSkill, target);

        if (result.damageDealt > 0)
        {
            if (target == enemyUnit)
            {
                enemyHUD.SetHP(enemyUnit.HealthPoints);
                if (enableVisualEffects)
                {
                    enemyHUD.TriggerDamageEffect();
                }
            }
            else if (target == playerUnit)
            {
                playerHUD.SetHP(playerUnit.HealthPoints);
                if (enableVisualEffects)
                {
                    playerHUD.TriggerDamageEffect();
                }
            }
        }

        if (result.healingDone > 0)
        {
            if (target == playerUnit)
            {
                playerHUD.SetHP(playerUnit.HealthPoints);
                if (enableVisualEffects)
                {
                    playerHUD.TriggerHealEffect();
                }
            }
        }

        if (!string.IsNullOrEmpty(result.message))
        {
            dialogueText.text = result.message;
            yield return new WaitForSeconds(AttackBufferTime / 2);
        }

        if (result.targetKilled)
        {
            if (target == enemyUnit)
            {
                ShiftBattleState(BattleState.WON);
            }
            else if (target == playerUnit)
            {
                ShiftBattleState(BattleState.LOST);
            }
            yield break;
        }

        yield return new WaitForSeconds(AttackBufferTime / 2);

        ShiftBattleState(BattleState.ENEMYTURN);
    }

    // ==================== UI MANAGEMENT ====================

    private void UpdateSkillButtons()
    {
        if (skillButtons == null || playerUnit == null || playerUnit.EquippedSkills == null)
            return;

        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (i < playerUnit.EquippedSkills.Count)
            {
                Skill skill = playerUnit.EquippedSkills[i];
                skillButtons[i].gameObject.SetActive(true);

                TMP_Text buttonText = skillButtons[i].GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.text = skill.skillName;
                }

                skillButtons[i].interactable = playerUnit.CanUseSkill(skill);
            }
            else
            {
                skillButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void ShowSkillPanel()
    {
        if (skillPanel != null)
        {
            skillPanel.SetActive(true);
        }
    }

    private void HideSkillPanel()
    {
        if (skillPanel != null)
        {
            skillPanel.SetActive(false);
        }
    }

    private void ShowTargetSelection()
    {
        if (targetSelectionPanel != null)
        {
            targetSelectionPanel.SetActive(true);

            for (int i = 0; i < targetButtons.Length; i++)
            {
                if (i < availableTargets.Count)
                {
                    targetButtons[i].gameObject.SetActive(true);

                    TMP_Text buttonText = targetButtons[i].GetComponentInChildren<TMP_Text>();
                    if (buttonText != null)
                    {
                        buttonText.text = availableTargets[i].unitName;
                    }
                }
                else
                {
                    targetButtons[i].gameObject.SetActive(false);
                }
            }

            dialogueText.text = "Select a target for " + selectedSkill.skillName + ":";
        }
    }

    private void HideTargetSelection()
    {
        if (targetSelectionPanel != null)
        {
            targetSelectionPanel.SetActive(false);
        }
    }

    public ExpInfo GetPlayerExpInfo()
    {
        if (playerUnit == null) return new ExpInfo();

        return new ExpInfo
        {
            currentLevel = playerUnit.Level,
            currentExp = playerUnit.CurrentExp,
            expToNextLevel = playerUnit.ExpToNextLevel,
            expPercentage = playerUnit.GetExpPercentage(),
            totalExpGained = expGainedThisBattle
        };
    }

    public void DebugBattle()
    {
        if (playerUnit == null || enemyUnit == null)
        {
            Debug.LogWarning("Units not assigned!");
            return;
        }

        StartBattle(playerUnit, enemyUnit);
    }
}

public struct ExpInfo
{
    public int currentLevel;
    public int currentExp;
    public int expToNextLevel;
    public float expPercentage;
    public int totalExpGained;
}