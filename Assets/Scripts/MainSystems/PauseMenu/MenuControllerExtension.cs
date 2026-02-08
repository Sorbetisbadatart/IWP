using UnityEngine;

public class MenuControllerExtension : MonoBehaviour
{
    [SerializeField] private RPGMakerOptionsMenu optionsMenu;

    [Header("Alternative Close Methods")]
    [SerializeField] private bool enableBackButtonClose = true;
    [SerializeField] private bool enableAnywhereClickClose = false;
    [SerializeField] private bool enableTimeoutClose = false;
    [SerializeField] private float inactivityTimeout = 30f;

    [Header("Close Confirmation")]
    [SerializeField] private bool requireCloseConfirmation = false;
    [SerializeField] private GameObject closeConfirmationPanel;
    [SerializeField] private string closeConfirmationMessage = "Close menu?";

    private float inactivityTimer = 0f;
    private bool isConfirmingClose = false;

    private void Update()
    {
        if (!optionsMenu.IsMenuOpen) return;

        HandleAlternativeCloseMethods();
        HandleInactivityTimeout();
    }

    private void HandleAlternativeCloseMethods()
    {
        // Android/Unity back button
        if (enableBackButtonClose && Input.GetKeyDown(KeyCode.Escape))
        {
            if (requireCloseConfirmation && !isConfirmingClose)
            {
                ShowCloseConfirmation();
            }
            else
            {
                optionsMenu.CloseMenu();
            }
        }

        // Click anywhere to close (outside menu)
        if (enableAnywhereClickClose && Input.GetMouseButtonDown(0))
        {
            if (!IsMouseOverMenu() && !isConfirmingClose)
            {
                optionsMenu.CloseMenu();
            }
        }
    }

    private void HandleInactivityTimeout()
    {
        if (!enableTimeoutClose) return;

        // Reset timer on any input
        if (Input.anyKeyDown || Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f ||
            Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f)
        {
            inactivityTimer = 0f;
            return;
        }

        inactivityTimer += Time.unscaledDeltaTime;

        if (inactivityTimer >= inactivityTimeout)
        {
            optionsMenu.CloseMenu();
            inactivityTimer = 0f;
        }
    }

    private bool IsMouseOverMenu()
    {
        if (!optionsMenu.IsMenuOpen) return false;

        // Check if mouse is over any UI element in the menu
        // This is a simple implementation - you might want to use EventSystem.RaycastAll
        RectTransform menuRect = optionsMenu.transform as RectTransform;
        if (menuRect == null) return false;

        Vector2 mousePos = Input.mousePosition;
        Vector2 localMousePos = menuRect.InverseTransformPoint(mousePos);

        return menuRect.rect.Contains(localMousePos);
    }

    private void ShowCloseConfirmation()
    {
        isConfirmingClose = true;

        if (closeConfirmationPanel != null)
        {
            closeConfirmationPanel.SetActive(true);

            // You can add UI elements to show the message and Yes/No buttons
            // For simplicity, we'll use a simple approach
        }

        Debug.Log(closeConfirmationMessage);
        // In a real implementation, you would show a UI panel with Yes/No buttons
    }

    public void ConfirmClose()
    {
        optionsMenu.CloseMenu();
        HideCloseConfirmation();
    }

    public void CancelClose()
    {
        HideCloseConfirmation();
    }

    private void HideCloseConfirmation()
    {
        isConfirmingClose = false;

        if (closeConfirmationPanel != null)
        {
            closeConfirmationPanel.SetActive(false);
        }
    }

    // Public methods for UI buttons
    public void CloseMenuWithDelay(float delay)
    {
        Invoke(nameof(CloseMenuDelayed), delay);
    }

    private void CloseMenuDelayed()
    {
        optionsMenu.CloseMenu();
    }

    public void CloseMenuAndSave()
    {
        // Add save functionality before closing
        Debug.Log("Saving settings before closing menu...");
        optionsMenu.CloseMenu();
    }

    public void CloseMenuAndReturnToMainMenu()
    {
        optionsMenu.CloseMenu();
        // Load main menu scene or show main menu
        Debug.Log("Returning to main menu...");
    }
}