using UnityEngine;
using UnityEngine.UI;

public class MenuTest : MonoBehaviour
{
    [SerializeField] private RPGMakerOptionsMenu optionsMenu;
    [SerializeField] private MenuControllerExtension menuController;

    [Header("Test UI")]
    [SerializeField] private Button addTabButton;
    [SerializeField] private Button removeTabButton;
    [SerializeField] private Button closeMenuButton;
    [SerializeField] private Button openMenuButton;
    [SerializeField] private InputField tabNameInput;
    [SerializeField] private Dropdown quickKeyDropdown;

    private void Start()
    {
        // Setup UI button listeners
        if (addTabButton != null)
            addTabButton.onClick.AddListener(AddTestTab);

        if (removeTabButton != null)
            removeTabButton.onClick.AddListener(RemoveLastTab);

        if (closeMenuButton != null)
            closeMenuButton.onClick.AddListener(CloseMenuProgrammatically);

        if (openMenuButton != null)
            openMenuButton.onClick.AddListener(OpenMenuProgrammatically);

        // Setup quick key dropdown
        if (quickKeyDropdown != null)
        {
            quickKeyDropdown.ClearOptions();
            quickKeyDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "None", "F1", "F2", "F3", "F4", "F5", "F6"
            });
        }
    }

    private void Update()
    {
        // Test different ways to close the menu
        if (Input.GetKeyDown(KeyCode.C))
        {
            CloseMenuProgrammatically();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            OpenMenuProgrammatically();
        }

        // Test direct tab selection
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectTabByIndex(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectTabByIndex(1);
        }

        // Force close menu with condition
        if (Input.GetKeyDown(KeyCode.X))
        {
            ForceCloseMenu();
        }
    }

    public void AddTestTab()
    {
        string tabName = string.IsNullOrEmpty(tabNameInput.text) ?
                        $"Tab_{Random.Range(100, 999)}" : tabNameInput.text;

        KeyCode quickKey = KeyCode.None;
        if (quickKeyDropdown != null && quickKeyDropdown.value > 0)
        {
            string keyName = "F" + quickKeyDropdown.value;
            quickKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), keyName);
        }

        optionsMenu.AddTab(tabName, null, quickKey);

        Debug.Log($"Added tab: {tabName} with quick key: {quickKey}");
    }

    public void RemoveLastTab()
    {
        int tabCount = optionsMenu.GetTabCount();
        if (tabCount > 0)
        {
            optionsMenu.RemoveTab(tabCount - 1);
            Debug.Log($"Removed last tab. Remaining tabs: {optionsMenu.GetTabCount()}");
        }
    }

    public void CloseMenuProgrammatically()
    {
        if (optionsMenu.IsMenuOpen)
        {
            Debug.Log("Closing menu via script...");
            optionsMenu.CloseMenu();
        }
    }

    public void OpenMenuProgrammatically()
    {
        if (!optionsMenu.IsMenuOpen)
        {
            Debug.Log("Opening menu via script...");
            optionsMenu.OpenMenu();
        }
    }

    public void SelectTabByIndex(int index)
    {
        if (optionsMenu.IsMenuOpen && index < optionsMenu.GetTabCount())
        {
            optionsMenu.SelectTab(index);
            Debug.Log($"Selected tab {index} via script");
        }
    }

    public void ForceCloseMenu()
    {
        // This ensures menu closes regardless of its state
        if (optionsMenu.IsMenuOpen)
        {
            optionsMenu.CloseMenu();
            Debug.Log("Menu force-closed");
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 500));

        GUILayout.Label("=== Menu Controls Demo ===");
        GUILayout.Label($"Menu State: {(optionsMenu.IsMenuOpen ? "OPEN" : "CLOSED")}");
        GUILayout.Label($"Selected Tab: {optionsMenu.SelectedTabName}");
        GUILayout.Label($"Total Tabs: {optionsMenu.GetTabCount()}");
        GUILayout.Label("");

        GUILayout.Label("Keyboard Shortcuts:");
        GUILayout.Label("• ESC: Toggle Menu");
        GUILayout.Label("• Arrow Keys: Navigate Tabs");
        GUILayout.Label("• Enter/Space: Select Tab");
        GUILayout.Label("• F1-F5: Direct Tab Access");
        GUILayout.Label("• Tab/Shift+Tab: Quick Nav");
        GUILayout.Label("");

        GUILayout.Label("Test Controls:");
        GUILayout.Label("• O: Open Menu");
        GUILayout.Label("• C: Close Menu");
        GUILayout.Label("• X: Force Close");
        GUILayout.Label("• 1/2: Select Tab 1/2");

        GUILayout.EndArea();
    }
}