using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BattleHUD : MonoBehaviour
{
    public TMP_Text nameText;
    public Image portrait;
    public BarTracker healthBar;

    // Visual effects
    private RectTransform portraitRect;
    private Vector3 originalPosition;
    private Color originalColor = Color.white;
        

    // Defense state
    private bool isDefending = false;
    private Coroutine defensePulseRoutine;

   
    [Header("Experience Display")]
    [SerializeField] private BarTracker expBar;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private GameObject levelUpEffect;

    // Current unit reference
    private Unit currentUnit;

    private void Awake()
    {
        if (portrait != null)
        {
            portraitRect = portrait.GetComponent<RectTransform>();
            originalPosition = portraitRect.anchoredPosition;          
            originalColor = portrait.color;
        }
    }

    public void SetHUD(Unit unit)
    {
        currentUnit = unit;

        nameText.text = unit.unitName;
        portrait.sprite = unit.unitSprite;

        // Set health bar
        healthBar.InitialiseBar(unit.HealthPoints, unit.MaxHealthPoints);

        // Set level and exp
        if (levelText != null)
        {
            levelText.text = $"Lv. {unit.Level}";
        }

        if (expBar != null)
        {
            expBar.InitialiseBar(unit.CurrentExp, unit.ExpToNextLevel);
            expBar.gameObject.SetActive(unit.Level < unit.inheritedStats.maxLevel);
        }

        if (expText != null)
        {
            expText.text = $"{unit.CurrentExp}/{unit.ExpToNextLevel} EXP";
        }

        ResetVisuals();
    }

    /// <summary>
    /// Update the experience bar
    /// </summary>
    public void UpdateExpBar(float percentage)
    {
        if (expBar != null && currentUnit != null)
        {
            // Animate exp gain
            StartCoroutine(AnimateExpGain(percentage));

            // Update exp text
            if (expText != null)
            {
                expText.text = $"{currentUnit.CurrentExp}/{currentUnit.ExpToNextLevel} EXP";
            }
        }
    }

    private IEnumerator AnimateExpGain(float targetPercentage)
    {
        if (expBar == null) yield break;

        float currentFill = expBar.GetCurrentValue() / (float)expBar.GetMaxValue();
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float currentValue = Mathf.Lerp(currentFill, targetPercentage, t);

            // Update exp bar display
            expBar.SetHealthPercentage(currentValue * 100f);

            yield return null;
        }

        expBar.SetHealthPercentage(targetPercentage * 100f);
    }

    /// <summary>
    /// Trigger level up visual effect
    /// </summary>
    public void TriggerLevelUpEffect()
    {
        if (levelUpEffect != null)
        {
            StartCoroutine(LevelUpAnimation());
        }

        // Update level text
        if (levelText != null && currentUnit != null)
        {
            levelText.text = $"Lv. {currentUnit.Level}";

            // Pulse animation for level text
            StartCoroutine(PulseLevelText());
        }
    }

    private IEnumerator LevelUpAnimation()
    {
        if (levelUpEffect != null)
        {
            levelUpEffect.SetActive(true);

            // Golden flash effect
            if (portrait != null)
            {
                Color originalColor = portrait.color;
                portrait.color = Color.yellow;

                yield return new WaitForSeconds(0.3f);

                // Pulse golden color
                for (int i = 0; i < 3; i++)
                {
                    portrait.color = Color.yellow;
                    yield return new WaitForSeconds(0.1f);
                    portrait.color = originalColor;
                    yield return new WaitForSeconds(0.1f);
                }

                portrait.color = originalColor;
            }

            yield return new WaitForSeconds(1f);
            levelUpEffect.SetActive(false);
        }
    }

    private IEnumerator PulseLevelText()
    {
        if (levelText == null) yield break;

        Color originalColor = levelText.color;
        float originalSize = levelText.fontSize;

        // Grow and change color
        for (int i = 0; i < 2; i++)
        {
            levelText.color = Color.yellow;
            levelText.fontSize = originalSize * 1.5f;
            yield return new WaitForSeconds(0.1f);

            levelText.color = originalColor;
            levelText.fontSize = originalSize;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void SetHP(int hp)
    {
        healthBar.SetHealthToValue(hp);
    }

    public void SetDefenseState(bool isdefending)
    {
        isDefending = isdefending;

        if (isdefending)
        {
            // Start blue pulse effect
            if (defensePulseRoutine != null)
                StopCoroutine(defensePulseRoutine);
            defensePulseRoutine = StartCoroutine(PulseDefenseColor());
        }
        else
        {
            // Stop pulse and return to normal
            if (defensePulseRoutine != null)
            {
                StopCoroutine(defensePulseRoutine);
                defensePulseRoutine = null;
            }
            portrait.color = originalColor;
        }
    }

    private IEnumerator PulseDefenseColor()
    {
        Color defenseColor = new Color(0.3f, 0.5f, 1f, 1f);

        while (true)
        {
            float pulse = Mathf.Sin(Time.time * 2f) * 0.1f + 0.9f;
            portrait.color = new Color(
                defenseColor.r * pulse,
                defenseColor.g * pulse,
                defenseColor.b,
                defenseColor.a
            );
            yield return null;
        }
    }

    public void TriggerDamageEffect()
    {
        StartCoroutine(DamageEffect());
    }

    private IEnumerator DamageEffect()
    {
        if (portraitRect == null) yield break;

        // Flash red
        portrait.color = Color.red;

        // Shake
        float elapsed = 0f;
        Vector3 basePos = originalPosition;

        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;

            float decay = 1f - (elapsed / 0.3f);
            float shakeX = Mathf.Sin(elapsed * 30f) * 10f * decay;

            portraitRect.anchoredPosition = basePos + new Vector3(shakeX, 0, 0);
            yield return null;
        }

        // Return to position and color
        portraitRect.anchoredPosition = originalPosition;
        portrait.color = isDefending ? new Color(0.3f, 0.5f, 1f, 1f) : originalColor;
        
    }

    public void TriggerHealEffect()
    {
        StartCoroutine(HealEffect());
    }

    private IEnumerator HealEffect()
    {
        if (portrait == null) yield break;

        Color original = portrait.color;

        // Flash green
        portrait.color = Color.green;
        yield return new WaitForSeconds(0.2f);

        // Pulse
        for (int i = 0; i < 3; i++)
        {
            portrait.color = Color.green;
            yield return new WaitForSeconds(0.05f);
            portrait.color = original;
            yield return new WaitForSeconds(0.05f);
        }

        portrait.color = isDefending ? new Color(0.3f, 0.5f, 1f, 1f) : original;
    }

    public void ResetVisuals()
    {
        if (portraitRect != null)
        {
            portraitRect.anchoredPosition = originalPosition;
        }

        if (portrait != null)
        {
            portrait.color = originalColor;
        }

        isDefending = false;

        if (defensePulseRoutine != null)
        {
            StopCoroutine(defensePulseRoutine);
            defensePulseRoutine = null;
        }
    }
}