using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuHandler : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("UI References")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject loadingScreen;

    [SerializeField] private TMP_Text versionText;

    [Header("Loading Settings")]
    [SerializeField] private float minLoadingTime = 2f;
    [SerializeField] private bool simulateSlowLoad = false;
    [SerializeField] private float simulatedLoadDelay = 0.5f;

    [Header("Navigation Settings")]
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private float navigationCooldown = 0.2f;
    [SerializeField] private bool wrapNavigation = true;
    [SerializeField] private AudioClip navigationSound;
    [SerializeField] private AudioClip selectSound;

    private SceneSwitcher sceneSwitcher;
    private bool isLoading = false;
    private List<Button> buttons = new List<Button>();
    private int currentIndex = 0;
    private bool isInitialized = false;
    private float lastNavigationTime = 0f;
    private AudioSource audioSource;

    // Input action names (for new Input System if you're using it)
    private const string VERTICAL_AXIS = "Vertical";
    private const string SUBMIT_BUTTON = "Submit";

    private void Awake()
    {
        // Initialize references
        sceneSwitcher = gameObject.AddComponent<SceneSwitcher>();

        // Add AudioSource for navigation sounds
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Set up button listeners
        playButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);

        // Ensure settings panel is hidden on start
        //settingsPanel.SetActive(false);
        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        // Prevent double-clicks
        playButton.interactable = true;

        InitializeButtons();
    }

    private void Update()
    {
        if (!isInitialized || isLoading) return;

        HandleKeyboardNavigation();
        HandleMouseHover();
    }

    private void InitializeButtons()
    {
        if (buttonContainer == null)
        {
            Debug.LogWarning("You forgot to reference the ButtonContainer");
            return;
        }

        // Get all buttons from container
        buttons.Clear();
        foreach (Transform child in buttonContainer)
        {
            Button button = child.GetComponent<Button>();
            if (button != null)
            {
                buttons.Add(button);

                // Add hover effect events
                AddHoverEvents(button);
            }
        }

        if (buttons.Count > 0)
        {
            currentIndex = 0;
            SelectButton(currentIndex);
            isInitialized = true;
        }
        else
        {
            Debug.LogWarning("No buttons found in container!");
        }
    }

    private void AddHoverEvents(Button button)
    {
        // Add EventTrigger for mouse hover
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // Create entry for pointer enter event
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { OnButtonHover(button); });
        trigger.triggers.Add(entryEnter);
    }

    private void OnButtonHover(Button hoveredButton)
    {
        if (!isInitialized || isLoading) return;

        // Find the index of the hovered button
        int newIndex = buttons.IndexOf(hoveredButton);
        if (newIndex >= 0 && newIndex != currentIndex)
        {
            currentIndex = newIndex;
            SelectButton(currentIndex);
            PlayNavigationSound();
        }
    }

    private void HandleKeyboardNavigation()
    {
        // Check for navigation cooldown
        if (Time.time - lastNavigationTime < navigationCooldown)
            return;

        float verticalInput = Input.GetAxisRaw(VERTICAL_AXIS);
        bool navigationPressed = false;
        int direction = 0;

        // Check for arrow keys or W/S
        if (verticalInput > 0.5f || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            direction = -1;
            navigationPressed = true;
        }
        else if (verticalInput < -0.5f || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            direction = 1;
            navigationPressed = true;
        }

        if (navigationPressed)
        {
            Navigate(direction);
            lastNavigationTime = Time.time;
        }

        // Check for Enter/Space to click current button
        if (Input.GetButtonDown(SUBMIT_BUTTON) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            ClickCurrentButton();
        }
    }

    private void Navigate(int direction)
    {
        if (buttons.Count == 0) return;

        int newIndex = currentIndex + direction;

        // Handle wrapping
        if (wrapNavigation)
        {
            if (newIndex < 0) newIndex = buttons.Count - 1;
            else if (newIndex >= buttons.Count) newIndex = 0;
        }
        else
        {
            // Clamp to bounds without wrapping
            newIndex = Mathf.Clamp(newIndex, 0, buttons.Count - 1);
        }

        // Only change if index is different
        if (newIndex != currentIndex)
        {
            currentIndex = newIndex;
            SelectButton(currentIndex);
            PlayNavigationSound();
        }
    }

    private void HandleMouseHover()
    {
        // If mouse moves, we might want to clear keyboard selection
        // or you can keep both systems working together
        // This is optional depending on your design preferences
        
    }

    private void SelectButton(int index)
    {
        if (index < 0 || index >= buttons.Count) return;

        // Deselect all buttons (optional visual feedback)
        foreach (var button in buttons)
        {
            // You could add visual state changes here
            // Example: button.GetComponent<Image>().color = Color.white;
            button.gameObject.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        }

        // Select the current button
        buttons[index].Select();
        buttons[index].gameObject.GetComponent<Image>().color = Color.white;

        // Optional: Add visual feedback
        // buttons[index].GetComponent<Image>().color = Color.yellow;

        // Update EventSystem's current selected
        EventSystem.current.SetSelectedGameObject(buttons[index].gameObject);

        Debug.Log($"Selected Button: {buttons[index].name} (Index: {index})");
    }

    private void ClickCurrentButton()
    {
        if (buttons.Count == 0 || currentIndex < 0 || currentIndex >= buttons.Count)
            return;

        PlaySelectSound();
        buttons[currentIndex].onClick.Invoke();
    }

    private void PlayNavigationSound()
    {
        if (navigationSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(navigationSound);
        }
    }

    private void PlaySelectSound()
    {
        if (selectSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(selectSound);
        }
    }

    private void StartGame()
    {
        if (isLoading) return;

        PlayNavigationSound();

        // Disable button to prevent multiple clicks
        playButton.interactable = false;
        isLoading = true;

        // Start loading 
        StartCoroutine(LoadGameScene());
    }

    private void QuitGame()
    {
        quitButton.interactable = false;

        // Save any game data
        //SaveSystem.SaveGame();

#if UNITY_EDITOR
        Debug.Log("<color=red>Application quit requested</color>");
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private IEnumerator LoadGameScene()
    {
        // Show loading screen
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        float startTime = Time.time;
        float progress = 0f;

        // Initialize scene loading
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameSceneName);
        asyncLoad.allowSceneActivation = false;

        // Fake load screen
        if (simulateSlowLoad)
        {
            yield return new WaitForSeconds(simulatedLoadDelay);
        }

        // Loading progress loop
        while (!asyncLoad.isDone)
        {
            // Calculate progress (0-0.9 for loading, 0.9-1.0 for activation)
            progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // Update UI
            // loadingBar.value = progress;

            // Check if loading is complete
            if (asyncLoad.progress >= 0.9f)
            {
                // Enforce minimum loading time
                if (Time.time - startTime < minLoadingTime)
                {
                    // Continue showing loading screen        
                }
                else
                {
                    // Allow scene activation
                    asyncLoad.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }
}