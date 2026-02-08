using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StatusEffectIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text durationText;
    [SerializeField] private TMP_Text stackText;
    [SerializeField] private Image background;

    private StatusEffect statusEffect;
    private int remainingTurns;
    private int stacks;

    public void Initialize(StatusEffect effect, int duration, int stackCount)
    {
        statusEffect = effect;
        remainingTurns = duration;
        stacks = stackCount;

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (statusEffect == null) return;

        iconImage.sprite = statusEffect.icon;
        background.color = statusEffect.color;

        if (durationText != null)
        {
            durationText.text = remainingTurns.ToString();
            durationText.gameObject.SetActive(remainingTurns > 0);
        }

        if (stackText != null && stacks > 1)
        {
            stackText.text = $"x{stacks}";
            stackText.gameObject.SetActive(true);
        }
        else if (stackText != null)
        {
            stackText.gameObject.SetActive(false);
        }
    }

    public void UpdateStatus(int newDuration, int newStacks)
    {
        remainingTurns = newDuration;
        stacks = newStacks;
        UpdateDisplay();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Show tooltip with status effect info
        // TooltipManager.Instance.ShowStatusTooltip(statusEffect, remainingTurns, stacks);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Hide tooltip
        // TooltipManager.Instance.HideTooltip();
    }
}