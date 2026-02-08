using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleSystemUI : MonoBehaviour
{
    public TMP_Text name_text;
    public Image portrait;
    public BarTracker barTracker;

    // Add SFX references
    [Header("SFX Settings")]
    [SerializeField] private BattleSFXData sfxData;
    [SerializeField] private bool enableSFX = true;

    private RectTransform portraitRectTransform;
    private Vector3 originalPortraitPosition;
    private Color originalPortraitColor = Color.white;

    [Header("Flash Settings")]
    [SerializeField] private Color damageFlashColor = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color defenseColor = new Color(0.3f, 0.5f, 1f, 1f);
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private int flashCount = 2;
    [SerializeField] private bool useFlash = true;

    [Header("Mana Display")]
    [SerializeField] private BarTracker manaTracker;
    [SerializeField] private TMP_Text manaText;

    [Header("Status Effect Display")]
    [SerializeField] private Transform statusEffectContainer;
    [SerializeField] private GameObject statusEffectIconPrefab;

    private List<StatusEffectIcon> activeStatusIcons = new List<StatusEffectIcon>();

    // Defense state tracking
    private bool isInDefenseState = false;
    private Coroutine currentFlashRoutine;

    // Shake tracking
    private Coroutine currentShakeRoutine;
    private Vector3 currentShakeOffset = Vector3.zero;

    private void Awake()
    {
        if (portrait != null)
        {
            portraitRectTransform = portrait.GetComponent<RectTransform>();
            originalPortraitPosition = portraitRectTransform.anchoredPosition;          
        }
    }

    // Initialize with mana
    public void InitialiseHUD(Unit unit)
    {
        barTracker.InitialiseBar(unit.HealthPoints, unit.MaxHealthPoints);

        // Initialize mana
        if (manaTracker != null)
        {
            manaTracker.InitialiseBar(unit.ManaPoints, unit.MaxManaPoints);
        }

        if (manaText != null)
        {
            manaText.text = $"{unit.ManaPoints}/{unit.MaxManaPoints}";
        }

        name_text.text = unit.unitName;
        portrait.sprite = unit.unitSprite;
        ResetVisuals();

        // Clear status effects
        ClearStatusEffects();
    }

    public void InitialiseEnemyHUD(Unit unit)
    {
        barTracker.InitialiseBar(unit.HealthPoints, unit.MaxHealthPoints);
        portrait.sprite = unit.unitSprite;
        name_text.text = unit.unitName;
        ResetVisuals();
    }

    public void SetHealthUItoAmount(int amount)
    {
        barTracker.SetHealthToValue(amount);
    }

    // Combined shake and flash effect with SFX
    public void TriggerDamageEffect(bool isCritical = false, bool isBlocked = false)
    {
        // Play appropriate SFX
        if (enableSFX && sfxData != null)
        {
            if (isCritical)
            {
                SFXManager.Instance?.PlaySFX("CriticalHit", 1.2f);
            }
            else if (isBlocked)
            {
                SFXManager.Instance?.PlaySFX("DefenseBlock", 0.8f);
            }
            else
            {
                SFXManager.Instance?.PlaySFX("Damage");
            }
        }

        // Visual effects
        if (isInDefenseState && !isBlocked)
        {
            StartCoroutine(DamageWhileDefending());
        }
        else
        {
            if (useFlash)
            {
                StartCoroutine(FlashPortrait(isCritical ? Color.red : damageFlashColor));
            }
            StartCoroutine(ShakePortrait(isCritical ? 15f : 10f));
        }
    }

    private IEnumerator DamageWhileDefending()
    {
        // Play defense block SFX
        if (enableSFX && sfxData != null && sfxData.defenseBlockSound != null)
        {
            SFXManager.Instance?.PlaySFX("DefenseBlock");
        }

        Color currentDefenseColor = portrait.color;

        // Flash red for damage
        portrait.color = damageFlashColor;
        yield return new WaitForSeconds(flashDuration / 2);

        // Return to defense blue
        portrait.color = currentDefenseColor;
        yield return new WaitForSeconds(flashDuration / 4);

        // Quick white flash to show damage was mitigated
        portrait.color = Color.white;
        yield return new WaitForSeconds(flashDuration / 4);

        // Return to defense blue
        portrait.color = currentDefenseColor;

        // Also shake
        StartCoroutine(ShakePortrait(8f));
    }

    private IEnumerator FlashPortrait(Color flashColor)
    {
        if (portrait == null) yield break;

        Color originalColor = portrait.color;

        for (int i = 0; i < flashCount; i++)
        {
            portrait.color = flashColor;
            yield return new WaitForSeconds(flashDuration / (flashCount * 2));

            portrait.color = originalColor;
            yield return new WaitForSeconds(flashDuration / (flashCount * 2));
        }
    }

    // Set defense state with blue color and SFX
    public void SetDefenseState(bool defending, bool playSound = true)
    {
        isInDefenseState = defending;

        if (defending)
        {
            // Play defense start SFX
            if (playSound && enableSFX && sfxData != null && sfxData.defenseStartSound != null)
            {
                SFXManager.Instance?.PlaySFX("DefenseStart");
            }

            // Stop any ongoing flash routines
            if (currentFlashRoutine != null)
            {
                StopCoroutine(currentFlashRoutine);
            }

            // Set to defense blue
            portrait.color = defenseColor;

            // Add a subtle pulsing effect for defense
            currentFlashRoutine = StartCoroutine(PulseDefenseColor());
        }
        else
        {
            // Stop pulsing
            if (currentFlashRoutine != null)
            {
                StopCoroutine(currentFlashRoutine);
                currentFlashRoutine = null;
            }

            // Return to original color
            portrait.color = originalPortraitColor;
        }
    }

    private IEnumerator PulseDefenseColor()
    {
        float pulseSpeed = 2f;
        float pulseIntensity = 0.1f;

        while (true)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity + 1f;
            Color pulseColor = new Color(
                defenseColor.r * pulse,
                defenseColor.g * pulse,
                defenseColor.b,
                defenseColor.a
            );

            portrait.color = pulseColor;
            yield return null;
        }
    }

    private IEnumerator ShakePortrait(float intensity)
    {
        if (portraitRectTransform == null) yield break;

        float shakeDuration = 0.3f;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;

            // Left-right shake using sine wave
            float shakeOffset = Mathf.Sin(elapsedTime * 30f) * intensity * (1f - elapsedTime / shakeDuration);
            Vector3 shakePosition = originalPortraitPosition + new Vector3(shakeOffset, 0, 0);

            portraitRectTransform.anchoredPosition = shakePosition;

            yield return null;
        }

        portraitRectTransform.anchoredPosition = originalPortraitPosition;
    }

    // Stronger shake for critical hits with SFX
    public void TriggerCriticalHitEffect()
    {
        if (enableSFX && sfxData != null)
        {
            SFXManager.Instance?.PlaySFX("CriticalHit", 1.3f);
        }

        StartCoroutine(CriticalHitEffect());
    }

    private IEnumerator CriticalHitEffect()
    {
        Color originalColor = portrait.color;

        // Flash red
        portrait.color = Color.red;

        // Strong shake
        float shakeDuration = 0.5f;
        float maxShake = 15f;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float decay = 1f - (elapsed / shakeDuration);
            float shakeX = Mathf.Sin(elapsed * 50f) * maxShake * decay;
            float shakeY = Mathf.Cos(elapsed * 45f) * maxShake * 0.5f * decay;

            portraitRectTransform.anchoredPosition = originalPortraitPosition + new Vector3(shakeX, shakeY, 0);

            // Pulsing color effect
            float pulse = Mathf.Sin(elapsed * 20f) * 0.3f + 0.7f;
            portrait.color = new Color(1f, pulse * 0.3f, pulse * 0.3f, 1f);

            yield return null;
        }

        portraitRectTransform.anchoredPosition = originalPortraitPosition;
        portrait.color = isInDefenseState ? defenseColor : originalColor;
    }

    // Healing flash with SFX
    public void TriggerHealEffect(bool usePotion = false)
    {
        if (enableSFX && sfxData != null)
        {
            if (usePotion)
            {
                SFXManager.Instance?.PlaySFX("Potion");
            }
            else
            {
                SFXManager.Instance?.PlaySFX("Heal");
            }
        }

        StartCoroutine(HealFlash(usePotion));
    }

    private IEnumerator HealFlash(bool usePotion)
    {
        if (portrait == null) yield break;

        Color healColor = usePotion ? new Color(0.8f, 0.3f, 1f, 1f) : new Color(0.3f, 1f, 0.3f, 1f);
        Color currentColor = portrait.color;

        // Flash healing color
        portrait.color = healColor;
        yield return new WaitForSeconds(0.2f);

        // Pulse effect
        for (int i = 0; i < 3; i++)
        {
            portrait.color = healColor;
            yield return new WaitForSeconds(0.05f);
            portrait.color = currentColor;
            yield return new WaitForSeconds(0.05f);
        }

        portrait.color = isInDefenseState ? defenseColor : currentColor;
    }

    // Attack sound (call this when attack starts)
    public void PlayAttackSound(bool isStrongAttack = false)
    {
        if (!enableSFX || sfxData == null) return;

        if (isStrongAttack)
        {
            SFXManager.Instance?.PlaySFX("StrongAttack");
        }
        else
        {
            SFXManager.Instance?.PlaySFX("Attack");
        }
    }

    // UI Button sounds
    public void PlayButtonClickSound()
    {
        if (enableSFX && sfxData != null)
        {
            SFXManager.Instance?.PlayUISound("ButtonClick");
        }
    }

    public void PlayButtonHoverSound()
    {
        if (enableSFX && sfxData != null)
        {
            SFXManager.Instance?.PlayUISound("ButtonHover");
        }
    }

    // Reset to original state
    public void ResetVisuals()
    {
        if (portraitRectTransform != null)
        {
            portraitRectTransform.anchoredPosition = originalPortraitPosition;
        }
        if (portrait != null)
        {
            portrait.color = originalPortraitColor;
        }
        isInDefenseState = false;

        if (currentFlashRoutine != null)
        {
            StopCoroutine(currentFlashRoutine);
            currentFlashRoutine = null;
        }
    }

  

    public void UpdateManaUI(int currentMana, int maxMana)
    {
        if (manaTracker != null)
        {
            manaTracker.SetHealthToValue(currentMana);
        }

        if (manaText != null)
        {
            manaText.text = $"{currentMana}/{maxMana}";
        }
    }

    public void UpdateStatusEffects(List<ActiveStatus> statuses)
    {
        // Clear existing icons
        ClearStatusEffects();

        // Create new icons
        foreach (ActiveStatus status in statuses)
        {
            GameObject iconObj = Instantiate(statusEffectIconPrefab, statusEffectContainer);
            StatusEffectIcon icon = iconObj.GetComponent<StatusEffectIcon>();

            if (icon != null)
            {
                icon.Initialize(status.effect, status.remainingTurns, status.stacks);
                activeStatusIcons.Add(icon);
            }
        }
    }

    private void ClearStatusEffects()
    {
        foreach (StatusEffectIcon icon in activeStatusIcons)
        {
            Destroy(icon.gameObject);
        }
        activeStatusIcons.Clear();
    }
}