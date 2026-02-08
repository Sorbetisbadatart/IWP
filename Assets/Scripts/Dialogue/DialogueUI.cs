using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("In-Scene References")]
    [SerializeField] private GameObject DialogueBox;
    [SerializeField] private TMP_Text Text;
    [SerializeField] private Image portraitImage;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject characterNameContainer;
    [SerializeField] private TMP_Text characterName_text;
    [SerializeField] private DialogueLineSO CurrentSO;

    private TypewriterEffect typewriterEffect;
    private bool isDialogueActive = false;
    private bool isWaitingForFinalSpace = false;
    private Coroutine currentDialogueCoroutine;

    //events
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;

    private void Start()
    {
        // Safe component creation
        typewriterEffect = GetComponent<TypewriterEffect>();
        if (typewriterEffect == null)
        {
            typewriterEffect = gameObject.AddComponent<TypewriterEffect>();
        }
    }

    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.O)) OpenDialogue();
        if (Input.GetKeyDown(KeyCode.P)) CloseDialogue();
        if (Input.GetKeyDown(KeyCode.T)) StartDialogue(CurrentSO);
#endif
    }

    public void OpenDialogue()
    {
        DialogueBox.SetActive(true);
    }

    public void CloseDialogue()
    {
        // Stop any running coroutine
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
            currentDialogueCoroutine = null;
        }

        // Reset all state
        isWaitingForFinalSpace = false;
        DialogueBox.SetActive(false);

        // Only invoke end event if dialogue was active
        if (isDialogueActive)
        {
            isDialogueActive = false;
            onDialogueEnd?.Invoke();
        }

        // Clear audio
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }

    public void StartDialogue(DialogueLineSO DialogueSO)
    {
        if (isDialogueActive) return;

        // Clean up any existing coroutine
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
        }

        DialogueBox.SetActive(true);
        isDialogueActive = true;

        // Clear text from previous dialogue
        Text.text = "";

        onDialogueStart?.Invoke();
        UpdateDialogueUI(DialogueSO);
        currentDialogueCoroutine = StartCoroutine(StepThroughLine(DialogueSO));
    }

    public void UpdateDialogueUI(DialogueLineSO dialogueObject)
    {
        UpdatePortrait(dialogueObject);
        UpdateCharacterName(dialogueObject);
        PlayAudioClip(dialogueObject);
    }

    private IEnumerator StepThroughLine(DialogueLineSO dialogueObject)
    {
        for (int i = 0; i < dialogueObject.dialogueText.Length; i++)
        {
            string dialogue = dialogueObject.dialogueText[i];
            yield return RunTypingEffect(dialogue);

            // Clear input buffer to prevent carry-over
            if (i < dialogueObject.dialogueText.Length - 1)
            {
                yield return null;
                Input.ResetInputAxes(); // Clear buffered Space presses
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            }
            else
            {
                // Last line - wait for final input
                isWaitingForFinalSpace = true;
                Input.ResetInputAxes();

                yield return new WaitUntil(() =>
                    Input.GetKeyDown(KeyCode.Space) ||
                    Input.GetKeyDown(KeyCode.E) ||
                    Input.GetKeyDown(KeyCode.Return));

                CloseDialogue();
                yield break;
            }
        }
    }

    private bool UpdatePortrait(DialogueLineSO dialogueObject)
    {
        if (portraitImage == null)
        {
            Debug.LogWarning("No Portrait is assigned");
            return false;
        }

        if (dialogueObject.portrait != null)
        {
            Sprite portraitSprite = dialogueObject.portrait;
            portraitImage.sprite = portraitSprite;
            portraitImage.gameObject.SetActive(true);
            return true;
        }

        portraitImage.gameObject.SetActive(false);
        return false;
    }

    private bool UpdateCharacterName(DialogueLineSO dialogueObject)
    {
        if (dialogueObject.charactername != null && characterName_text != null)
        {
            string characterName = dialogueObject.charactername;
            characterName_text.text = characterName;

            if (characterNameContainer != null)
                characterNameContainer.SetActive(true);

            return true;
        }

        if (characterNameContainer != null)
            characterNameContainer.SetActive(false);

        return false;
    }

    private void PlayAudioClip(DialogueLineSO dialogueObject)
    {
        if (audioSource == null) return;

        audioSource.Stop();
        audioSource.clip = null;

        if (dialogueObject.audioClip != null)
        {
            audioSource.clip = dialogueObject.audioClip;
            audioSource.Play();
        }
    }

    private IEnumerator RunTypingEffect(string dialogue)
    {
        Text.text = "";
        typewriterEffect.Run(dialogue, Text);

        while (typewriterEffect.IsRunning)
        {
            yield return null;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Manually complete the text before stopping
                Text.text = dialogue; // Show full text immediately

                // Then stop the typewriter effect
                typewriterEffect.Stop();

                // Clear input buffer
                Input.ResetInputAxes();
                break; // Exit the loop
            }
        }

        // Wait a tiny moment to ensure the typewriter is fully stopped
        yield return null;
    }

    // Helper property for other scripts
    public bool IsDialogueActive => isDialogueActive;
}