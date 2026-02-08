using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DialogueInteract : MonoBehaviour, Iinteractable
{
    public string DialogueID { get; private set; }
    [SerializeField] private DialogueLineSO DialogueSO;
    private DialogueUI dialogueUI;
    [SerializeField] private bool interactable = false;
    private bool dialogueFinished = false;
    [SerializeField] private bool playDialogueOnce = false;


    // Store the current playing dialogue SO
    private DialogueLineSO currentPlayingDialogue;


    private bool isDirty = false;



    void Start()
    {
        dialogueUI = FindFirstObjectByType<DialogueUI>();

        if (dialogueUI != null)
        {
            dialogueUI.onDialogueStart.AddListener(OnDialogueStart);
            dialogueUI.onDialogueEnd.AddListener(OnDialogueEnd);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            Interact();
        }
    }

    void OnDestroy()
    {
        // Clean up event listeners
        if (dialogueUI != null)
        {
            dialogueUI.onDialogueStart.RemoveListener(OnDialogueStart);
            dialogueUI.onDialogueEnd.RemoveListener(OnDialogueEnd);
        }
    }

    private void OnDialogueStart()
    {
        if (currentPlayingDialogue != DialogueSO)
            return;
        // You could add logic here if this specific object's dialogue started
        Debug.Log("Dialogue started from: " + gameObject.name);

    }

    private void OnDialogueEnd()
    {
        // Only react if THIS object's dialogue just ended
        if (currentPlayingDialogue == DialogueSO)
        {
            if (playDialogueOnce)
                dialogueFinished = true;

            currentPlayingDialogue = null; // Clear reference
            StartCoroutine(InteractionCooldownRoutine());
        }

    }

    private IEnumerator InteractionCooldownRoutine()
    {

        isDirty = true;
        Debug.Log("now dirty!");
        yield return new WaitForSeconds(0.2f); // 200ms cooldown
        isDirty = false;
        Debug.Log("now not dirty!");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            interactable = true;
            return;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        interactable = false;
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        if (!interactable)
            return;
        currentPlayingDialogue = DialogueSO;

        if (dialogueFinished || isDirty)
            return;

        if (currentPlayingDialogue != DialogueSO)
            return;

        dialogueUI.StartDialogue(DialogueSO);

    }
}