using UnityEngine;

public class QuestSO : ScriptableObject
{
    public string questName;
   
    public string questDescription;

    public enum questType
    {
        FETCH,
        KILL,
        NONE,
    }

}
