using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using TMPro;

public class RPGMakerOptionsMenu : MonoBehaviour
{
    [System.Serializable]
    public class TabData
    {
        public string tabName;
        public Sprite icon;
        public GameObject contentPanel;
        public KeyCode quickKey = KeyCode.None;
    }

    [Header("UI Components")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private RectTransform windowRect;
    [SerializeField] private Transform tabButtonContainer;
    [SerializeField] private GameObject tabButtonPrefab;
    [SerializeField] private float tabHeight = 60f;
    [SerializeField] private float windowPadding = 20f;
    [SerializeField] private Color selectedTabColor = new Color(0.2f, 0.5f, 0.8f);
    [SerializeField] private Color normalTabColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private Color highlightedTabColor = new Color(0.4f, 0.6f, 1f);

    [Header("Tab Configuration")]
    [SerializeField]
    private List<TabData> tabs = new List<TabData>
    {
        new TabData { tabName = "System", quickKey = KeyCode.F1 },
        new TabData { tabName = "Items", quickKey = KeyCode.F2 },
        new TabData { tabName = "Skills", quickKey = KeyCode.F3 },
        new TabData { tabName = "Equip", quickKey = KeyCode.F4 },
        new TabData { tabName = "Status", quickKey = KeyCode.F5 }
    };

    [Header("Navigation Settings")]
    [SerializeField] private float navigationCooldown = 0.2f;
    [SerializeField] private bool wrapNavigation = true;
    [SerializeField] private bool playNavigationSounds = true;
    [SerializeField] private AudioClip navigationSound;
    [SerializeField] private AudioClip selectionSound;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private List<Button> tabButtons = new List<Button>();
    private int selectedTabIndex = 0;
    private int highlightedTabIndex = 0;
    private bool isMenuOpen = false;
    private float navigationTimer = 0f;

    // Input states
    private bool upPressed = false;
    private bool downPressed = false;
    private bool submitPressed = false;
    private bool cancelPressed = false;

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;
    [SerializeField] private KeyCode submitKey = KeyCode.Return;
    [SerializeField] private KeyCode secondarySubmitKey = KeyCode.Space;
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape;
    [SerializeField] private KeyCode nextTabKey = KeyCode.Tab;
    [SerializeField] private KeyCode previousTabKey = KeyCode.LeftShift;

    private AudioSource audioSource;
    private EventSystem eventSystem;

    // Public properties
    public bool IsMenuOpen { get { return isMenuOpen; } }
    public int SelectedTabIndex { get { return selectedTabIndex; } }
    public string SelectedTabName
    {
        get
        {
            return (tabs.Count > 0 && selectedTabIndex >= 0 && selectedTabIndex < tabs.Count) ?
                   tabs[selectedTabIndex].tabName : "No Tab Selected";
        }
    }

    private void Awake()
    {
        // Initialize components before Start
        if (menuPanel == null)
        {
            Debug.LogError("MenuPanel is not assigned!");
        }

        if (windowRect == null)
        {
            Debug.LogError("WindowRect is not assigned!");
        }

        if (tabButtonContainer == null)
        {
            Debug.LogError("TabButtonContainer is not assigned!");
        }

        if (tabButtonPrefab == null)
        {
            Debug.LogError("TabButtonPrefab is not assigned!");
        }
    }

    private void Start()
    {
        InitializeMenu();
        CloseMenu(); // Start with menu closed

        // Get or add AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && playNavigationSounds)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Ensure we have an EventSystem
        eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            eventSystem = es.GetComponent<EventSystem>();
        }
    }

    private void InitializeMenu()
    {
        // Clear existing tabs safely
        if (tabButtonContainer != null)
        {
            foreach (Transform child in tabButtonContainer)
            {
                if (child != null)
                    Destroy(child.gameObject);
            }
        }

        tabButtons.Clear();

        // Check if we have valid references
        if (tabButtonContainer == null || tabButtonPrefab == null)
        {
            Debug.LogError("Cannot initialize menu: Missing required references");
            return;
        }

        // Create tabs if we have any
        if (tabs.Count > 0)
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                CreateTabButton(i);
            }

            // Adjust window size
            UpdateWindowSize();

            // Set initial navigation
            UpdateTabNavigation();

            // Default to first tab
            if (tabButtons.Count > 0)
            {
                selectedTabIndex = 0;
                highlightedTabIndex = 0;
                UpdateTabVisual(0, true);
            }
        }
        else
        {
            // No tabs, set to empty state
            selectedTabIndex = -1;
            highlightedTabIndex = -1;
            UpdateWindowSize();
        }
    }

    private void CreateTabButton(int index)
    {
        if (index < 0 || index >= tabs.Count) return;

        GameObject tabGO = Instantiate(tabButtonPrefab, tabButtonContainer);
        if (tabGO == null)
        {
            Debug.LogError("Failed to instantiate tab button!");
            return;
        }

        tabGO.name = $"Tab_{tabs[index].tabName}";
     

        Button button = tabGO.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("Tab button prefab is missing Button component!");
            Destroy(tabGO);
            return;
        }

        // Find UI components
        TMP_Text buttonText = tabGO.GetComponentInChildren<TMP_Text>();
        Image buttonIcon = tabGO.transform.Find("Icon")?.GetComponent<Image>();
        TMP_Text shortcutText = tabGO.transform.Find("ShortcutText")?.GetComponent<TMP_Text>();

        if (buttonText != null)
            buttonText.text = tabs[index].tabName;

        if (buttonIcon != null && tabs[index].icon != null)
            buttonIcon.sprite = tabs[index].icon;

        if (shortcutText != null && tabs[index].quickKey != KeyCode.None)
            shortcutText.text = GetKeyCodeString(tabs[index].quickKey);

        int tabIndex = index;
        button.onClick.AddListener(() =>
        {
            SelectTab(tabIndex);
            PlaySound(selectionSound);
        });

        // Add navigation component
        TabNavigation tabNav = tabGO.AddComponent<TabNavigation>();
        if (tabNav != null)
        {
            tabNav.Initialize(this, tabIndex);
        }

        tabButtons.Add(button);
    }

    private void UpdateTabNavigation()
    {
        // Update navigation for all tabs
        for (int i = 0; i < tabButtons.Count; i++)
        {
            Button button = tabButtons[i];
            if (button == null) continue;

            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.Explicit;

            // Set up vertical navigation
            if (i > 0 && tabButtons[i - 1] != null)
                nav.selectOnUp = tabButtons[i - 1];

            if (i < tabButtons.Count - 1 && tabButtons[i + 1] != null)
                nav.selectOnDown = tabButtons[i + 1];

            button.navigation = nav;
        }
    }

    private void Update()
    {
        // Always listen for toggle key
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMenu();
        }

        if (!isMenuOpen) return;

        // Handle input cooldown
        navigationTimer -= Time.unscaledDeltaTime;
        bool canNavigate = navigationTimer <= 0f;

        // Check for quick key shortcuts
        CheckQuickKeys();

        // Check for navigation input
        float verticalInput = Input.GetAxisRaw("Vertical");
        bool upInput = verticalInput > 0.5f;
        bool downInput = verticalInput < -0.5f;
        bool submitInput = Input.GetKeyDown(submitKey) || Input.GetKeyDown(secondarySubmitKey);
        bool cancelInput = Input.GetKeyDown(cancelKey);
        bool tabInput = Input.GetKeyDown(nextTabKey);
        bool shiftTabInput = Input.GetKeyDown(previousTabKey);

        // Handle keyboard navigation
        if (canNavigate)
        {
            if (upInput && !upPressed)
            {
                NavigateUp();
                navigationTimer = navigationCooldown;
            }
            else if (downInput && !downPressed)
            {
                NavigateDown();
                navigationTimer = navigationCooldown;
            }
            else if (tabInput && !shiftTabInput)
            {
                NavigateDown();
                navigationTimer = navigationCooldown * 0.5f;
            }
            else if (shiftTabInput)
            {
                NavigateUp();
                navigationTimer = navigationCooldown * 0.5f;
            }
        }

        // Handle selection
        if (submitInput && !submitPressed)
        {
            if (highlightedTabIndex >= 0 && highlightedTabIndex < tabButtons.Count)
            {
                SelectTab(highlightedTabIndex);
                PlaySound(selectionSound);
            }
            submitPressed = true;
        }
        else if (cancelInput && !cancelPressed)
        {
            CloseMenu();
            cancelPressed = true;
        }

        // Update input states
        upPressed = upInput;
        downPressed = downInput;
        submitPressed = submitInput;
        cancelPressed = cancelInput;

        // Update EventSystem selection
        UpdateEventSystemSelection();
    }

    public void NavigateUp()
    {
        if (tabButtons.Count == 0) return;

        int newIndex = highlightedTabIndex - 1;

        if (wrapNavigation && newIndex < 0)
            newIndex = tabButtons.Count - 1;

        if (newIndex >= 0)
        {
            HighlightTab(newIndex);
            PlaySound(navigationSound);
        }
    }

    public void NavigateDown()
    {
        if (tabButtons.Count == 0) return;

        int newIndex = highlightedTabIndex + 1;

        if (wrapNavigation && newIndex >= tabButtons.Count)
            newIndex = 0;

        if (newIndex < tabButtons.Count)
        {
            HighlightTab(newIndex);
            PlaySound(navigationSound);
        }
    }

    private void CheckQuickKeys()
    {
        if (navigationTimer > 0 || tabs.Count == 0) return;

        for (int i = 0; i < tabs.Count; i++)
        {
            if (tabs[i].quickKey != KeyCode.None && Input.GetKeyDown(tabs[i].quickKey))
            {
                if (i < tabButtons.Count)
                {
                    SelectTab(i);
                    HighlightTab(i);
                    PlaySound(selectionSound);
                    navigationTimer = navigationCooldown;
                    break;
                }
            }
        }
    }

    public void HighlightTab(int index)
    {
        if (index < 0 || index >= tabButtons.Count || tabButtons.Count == 0) return;

        // Remove highlight from previous tab
        if (highlightedTabIndex >= 0 && highlightedTabIndex < tabButtons.Count)
        {
            UpdateTabVisual(highlightedTabIndex, highlightedTabIndex == selectedTabIndex);
        }

        // Highlight new tab
        highlightedTabIndex = index;
        UpdateTabVisual(index, index == selectedTabIndex, true);
    }

    public void SelectTab(int index)
    {
      
        if (index < 0 || index >= tabButtons.Count || tabButtons.Count == 0) return;

        // Deselect previous tab
        if (selectedTabIndex >= 0 && selectedTabIndex < tabButtons.Count)
        {
            UpdateTabVisual(selectedTabIndex, false);
            if (tabs[selectedTabIndex].contentPanel != null)
                tabs[selectedTabIndex].contentPanel.SetActive(false);


        }

     

        // Select new tab
        selectedTabIndex = index;
        highlightedTabIndex = index;
        UpdateTabVisual(index, true);

        // Here you would show the tab's specific content screen
        Debug.Log($"Selected tab: {tabs[index].tabName}");
        if (tabs[index].contentPanel != null)
            tabs[index].contentPanel.SetActive(true);


        // Focus the selected button for keyboard navigation
        if (eventSystem != null && tabButtons[index] != null)
        {
            eventSystem.SetSelectedGameObject(tabButtons[index].gameObject);
        }
    }

    private void UpdateTabVisual(int index, bool isSelected, bool isHighlighted = false)
    {
        if (index < 0 || index >= tabButtons.Count || tabButtons[index] == null) return;

        Image tabImage = tabButtons[index].GetComponent<Image>();
        Text tabText = tabButtons[index].GetComponentInChildren<Text>();

        if (tabImage != null)
        {
            if (isSelected)
                tabImage.color = selectedTabColor;
            else if (isHighlighted)
                tabImage.color = highlightedTabColor;
            else
                tabImage.color = normalTabColor;
        }

        if (tabText != null)
            tabText.color = isSelected || isHighlighted ? Color.white : new Color(0.8f, 0.8f, 0.8f);
    }

    private void UpdateEventSystemSelection()
    {
        if (eventSystem == null || tabButtons.Count == 0) return;

        // If nothing is selected, select the highlighted tab
        if (eventSystem.currentSelectedGameObject == null)
        {
            if (highlightedTabIndex >= 0 && highlightedTabIndex < tabButtons.Count)
            {
                eventSystem.SetSelectedGameObject(tabButtons[highlightedTabIndex].gameObject);
            }
        }
        else
        {
            // Update highlighted index based on current selection
            GameObject currentSelected = eventSystem.currentSelectedGameObject;
            for (int i = 0; i < tabButtons.Count; i++)
            {
                if (tabButtons[i] != null && tabButtons[i].gameObject == currentSelected)
                {
                    if (i != highlightedTabIndex)
                    {
                        HighlightTab(i);
                    }
                    break;
                }
            }
        }
    }

    private void UpdateWindowSize()
    {
        if (windowRect == null) return;

        float totalHeight = (tabHeight * tabs.Count) + (windowPadding * 2);
        windowRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

        windowRect.anchoredPosition = new Vector2(windowPadding, -windowPadding);
    }

    // Public methods for dynamic tab management
    public void AddTab(string tabName, Sprite icon = null, KeyCode quickKey = KeyCode.None)
    {
        TabData newTab = new TabData { tabName = tabName, icon = icon, quickKey = quickKey };
        tabs.Add(newTab);

        CreateTabButton(tabs.Count - 1);
        UpdateTabNavigation();
        UpdateWindowSize();

        // If this is the first tab, select it
        if (tabs.Count == 1)
        {
            SelectTab(0);
            HighlightTab(0);
        }
    }

    public void RemoveTab(int index)
    {
        if (index < 0 || index >= tabs.Count) return;

        tabs.RemoveAt(index);

        if (index < tabButtons.Count && tabButtons[index] != null)
        {
            Destroy(tabButtons[index].gameObject);
            tabButtons.RemoveAt(index);
        }

        // Reinitialize menu
        InitializeMenu();

        // Select a valid tab if any remain
        if (tabs.Count > 0)
        {
            if (selectedTabIndex >= tabs.Count)
                selectedTabIndex = Mathf.Max(0, tabs.Count - 1);

            if (selectedTabIndex >= 0 && selectedTabIndex < tabButtons.Count)
            {
                SelectTab(selectedTabIndex);
                HighlightTab(selectedTabIndex);
            }
        }
        else
        {
            // No tabs left
            selectedTabIndex = -1;
            highlightedTabIndex = -1;
            CloseMenu();
        }
    }

    public void ToggleMenu()
    {
        if (isMenuOpen)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    public void OpenMenu()
    {
        if (isMenuOpen) return;

        isMenuOpen = true;
        if (menuPanel != null)
            menuPanel.SetActive(true);

        Time.timeScale = 0f;
        PlaySound(openSound);

        // Reset selection if we have tabs
        if (tabs.Count > 0 && tabButtons.Count > 0)
        {
            if (selectedTabIndex < 0 || selectedTabIndex >= tabButtons.Count)
                selectedTabIndex = 0;

            SelectTab(selectedTabIndex);
            HighlightTab(selectedTabIndex);

            // Set initial focus
            if (eventSystem != null && tabButtons[selectedTabIndex] != null)
            {
                eventSystem.SetSelectedGameObject(tabButtons[selectedTabIndex].gameObject);
            }
        }
    }

    public void CloseMenu()
    {
      

        isMenuOpen = false;
        if (menuPanel != null)
            menuPanel.SetActive(false);

        Time.timeScale = 1f;
        PlaySound(closeSound);

        // Clear selection
        if (eventSystem != null)
        {
            eventSystem.SetSelectedGameObject(null);
        }
    }

    public void ClearAllTabs()
    {
        tabs.Clear();
        InitializeMenu();
        CloseMenu();
    }

    private void PlaySound(AudioClip clip)
    {
        if (playNavigationSounds && audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private string GetKeyCodeString(KeyCode key)
    {
        if (key >= KeyCode.F1 && key <= KeyCode.F12)
            return key.ToString().Substring(1);
        if (key == KeyCode.Return)
            return "Enter";
        if (key == KeyCode.Escape)
            return "Esc";
        return key.ToString();
    }

    // Getters for external scripts
    public int GetTabCount() => tabs.Count;
    public TabData GetTabData(int index) => (index >= 0 && index < tabs.Count) ? tabs[index] : null;
    public List<string> GetAllTabNames()
    {
        List<string> names = new List<string>();
        foreach (var tab in tabs)
        {
            names.Add(tab.tabName);
        }
        return names;
    }
}