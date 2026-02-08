using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabNavigation : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animation Settings")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float animationSpeed = 8f;
    [SerializeField] private bool enableHoverAnimation = true;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject selectionIndicator;
    [SerializeField] private GameObject highlightIndicator;

    private RPGMakerOptionsMenu menuManager;
    private int tabIndex;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isSelected = false;

    public void Initialize(RPGMakerOptionsMenu manager, int index)
    {
        menuManager = manager;
        tabIndex = index;
        originalScale = transform.localScale;
        targetScale = originalScale;

        // Initialize indicators
        if (selectionIndicator != null)
            selectionIndicator.SetActive(false);
        if (highlightIndicator != null)
            highlightIndicator.SetActive(false);
    }

    private void Update()
    {
        if (!enableHoverAnimation) return;

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale,
            Time.unscaledDeltaTime * animationSpeed);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (menuManager != null)
        {
            menuManager.HighlightTab(tabIndex);
        }

        if (enableHoverAnimation)
            targetScale = originalScale * hoverScale;

        if (highlightIndicator != null)
            highlightIndicator.SetActive(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (menuManager != null && menuManager.SelectedTabIndex != tabIndex)
        {
            if (enableHoverAnimation)
                targetScale = originalScale;

            if (highlightIndicator != null)
                highlightIndicator.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (menuManager != null && menuManager.IsMenuOpen)
        {
            menuManager.HighlightTab(tabIndex);
        }

        if (enableHoverAnimation)
            targetScale = originalScale * hoverScale;

        if (highlightIndicator != null && !isSelected)
            highlightIndicator.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (menuManager != null && menuManager.SelectedTabIndex != tabIndex)
        {
            if (enableHoverAnimation)
                targetScale = originalScale;

            if (highlightIndicator != null)
                highlightIndicator.SetActive(false);
        }
    }

    public void SetSelectedState(bool selected)
    {
        isSelected = selected;

        if (selectionIndicator != null)
            selectionIndicator.SetActive(selected);

        if (highlightIndicator != null && selected)
            highlightIndicator.SetActive(false);
    }

    private void OnDisable()
    {
        if (enableHoverAnimation)
        {
            targetScale = originalScale;
            transform.localScale = originalScale;
        }

        if (highlightIndicator != null)
            highlightIndicator.SetActive(false);
    }
}