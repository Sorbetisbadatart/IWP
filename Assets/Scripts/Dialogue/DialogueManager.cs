using NUnit.Framework;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public DialogueUI dialogueUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueUI = GetComponent<DialogueUI>();
        if (dialogueUI == null)
            Debug.LogWarning("Failed to get DialogueUI");       
    }

    public void StartDialogue(DialogueLineSO dialogueSO)
    {    
        dialogueUI.StartDialogue(dialogueSO);
    }

    public void EndDialogue()
    {
        dialogueUI.CloseDialogue();
    }
}
