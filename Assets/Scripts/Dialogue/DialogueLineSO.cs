using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "DialogueObject", menuName = "Dialogue/DialogueObject")]
public class DialogueLineSO : ScriptableObject
{
    [field: SerializeField][TextArea] public string[] dialogueText;
    [field: SerializeField] public string charactername;
    [field: SerializeField] public Sprite portrait;
    [field: SerializeField] public AudioClip audioClip;
    [field: SerializeField] public UnityAction[] events;
}