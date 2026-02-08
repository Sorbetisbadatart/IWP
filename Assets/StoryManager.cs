using UnityEngine;

public class StoryManager : MonoBehaviour
{
    [SerializeField] private DialogueLineSO[] Area1;
    private DialogueUI dialogueUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueUI = FindFirstObjectByType<DialogueUI>();
    }

    public void StartDialogue(DialogueLineSO dialogue)
    {
        dialogueUI.StartDialogue(dialogue);
    }
}
