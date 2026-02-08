using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    //references
    [SerializeField] private GameObject QuestUI; //UI elements for the Quest
    [SerializeField] private GameObject quest;


    //variables
    [SerializeField] private List<QuestSO> questList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddQuest(QuestSO questToAdd)
    {
        questList.Add(questToAdd);
    }

    public void RemoveQuest(QuestSO questToRemove)
    {
        questList.Remove(questToRemove);
    }

    private void UpdateQuestUI()
    {
        foreach (QuestSO quest in questList)
        {

        }
    }

    private void CreateQuestUI(QuestSO quest)
    {

    }
}
