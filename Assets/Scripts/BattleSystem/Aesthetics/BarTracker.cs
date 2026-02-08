using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BarTracker : MonoBehaviour
{
    [SerializeField] private Image bar;
    [SerializeField] private int currentBarFill = 100;
    [SerializeField] private int barFillMax = 100;

    [Header("Animation")]
    [SerializeField, Range(0, 0.5f)] private float animationTime = 0.25f;
    private Coroutine fillRoutine;

    [Header("Text Display")]
    [SerializeField] private DisplayType howToDisplayValueText = DisplayType.LongValue;
    [SerializeField] private TMP_Text barValueTextField;

    public enum DisplayType
    {
        LongValue,      // 50/100
        ShortValue,     // 50
        Percentage,     // 50%
        None
    }

    public void InitialiseBar(int currentAmt, int maxAmt)
    {
        barFillMax = maxAmt;
        currentBarFill = currentAmt;
        UpdateBar();
    }

    public void SetHealthToValue(int newValue)
    {
        newValue = Mathf.Clamp(newValue, 0, barFillMax);

        if (newValue == currentBarFill)
            return;

        currentBarFill = newValue;
        TriggerFillAnimation();
    }

    public void SetMax(int maxAmt)
    {
        barFillMax = maxAmt;
        currentBarFill = Mathf.Clamp(currentBarFill, 0, maxAmt);
        UpdateBar();
    }

    public bool ReduceResourceByAmount(int amount)
    {
        currentBarFill -= amount;
        currentBarFill = Mathf.Clamp(currentBarFill, 0, barFillMax);

        TriggerFillAnimation();
        return true;
    }

    public bool IncreaseResourceByAmount(int amount)
    {
        currentBarFill += amount;
        currentBarFill = Mathf.Clamp(currentBarFill, 0, barFillMax);

        TriggerFillAnimation();
        return true;
    }

    private void UpdateBar()
    {
        if (barFillMax <= 0)
        {
            bar.fillAmount = 0;
            return;
        }

        float fillAmount = (float)currentBarFill / barFillMax;
        bar.fillAmount = fillAmount;
        SetCurrentBarValueText();
    }

    private void TriggerFillAnimation()
    {
        float targetFill = (float)currentBarFill / barFillMax;

        if (Mathf.Approximately(bar.fillAmount, targetFill))
            return;

        if (fillRoutine != null)
            StopCoroutine(fillRoutine);

        fillRoutine = StartCoroutine(SmoothTransitionToNewValue(targetFill));
    }

    [Header("Experience Bar Settings")]
    [SerializeField] private bool isExpBar = false;
    [SerializeField] private Gradient expGradient;

    // Add to SetHealthPercentage method:
    public void SetHealthPercentage(float percentage)
    {
        percentage = Mathf.Clamp(percentage, 0f, 100f);
        int newValue = Mathf.RoundToInt((percentage / 100f) * barFillMax);
        SetHealthToValue(newValue);

        // Special handling for exp bar gradient
        if (isExpBar && expGradient != null && bar != null)
        {
            bar.color = expGradient.Evaluate(percentage / 100f);
        }
    }

    // Add to SmoothTransitionToNewValue coroutine:
    private IEnumerator SmoothTransitionToNewValue(float targetFill)
    {
        float originalFill = bar.fillAmount;
        float elapsedTime = 0.0f;

        while (elapsedTime < animationTime)
        {
            elapsedTime += Time.deltaTime;
            float time = elapsedTime / animationTime;
            float currentFill = Mathf.Lerp(originalFill, targetFill, time);
            bar.fillAmount = currentFill;

            // Update gradient for exp bar
            if (isExpBar && expGradient != null)
            {
                bar.color = expGradient.Evaluate(currentFill);
            }

            yield return null;
        }

        bar.fillAmount = targetFill;

        // Final gradient update
        if (isExpBar && expGradient != null)
        {
            bar.color = expGradient.Evaluate(targetFill);
        }

        SetCurrentBarValueText();
    }

    private void SetCurrentBarValueText()
    {
        if (barValueTextField == null) return;

        switch (howToDisplayValueText)
        {
            case DisplayType.LongValue:
                barValueTextField.text = $"{currentBarFill}/{barFillMax}";
                break;
            case DisplayType.ShortValue:
                barValueTextField.text = $"{currentBarFill}";
                break;
            case DisplayType.Percentage:
                float percentage = ((float)currentBarFill / barFillMax) * 100f;
                barValueTextField.text = $"{percentage:F0}%";
                break;
            case DisplayType.None:
                barValueTextField.text = "";
                break;
        }
    }

    public int GetCurrentValue() => currentBarFill;
    public int GetMaxValue() => barFillMax;
}