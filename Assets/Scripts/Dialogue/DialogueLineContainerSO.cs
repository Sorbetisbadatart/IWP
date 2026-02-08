using UnityEngine;

[CreateAssetMenu(fileName = "DialogueList", menuName = "Dialogue/DialogueList")]
public class DialogueLineContainerSO : ScriptableObject
{
    [field:SerializeField] public DialogueLineSO[] lines;
}
