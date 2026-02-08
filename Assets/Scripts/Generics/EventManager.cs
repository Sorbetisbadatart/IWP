using UnityEngine;
using UnityEngine.Events;

public static class EventManager
{
    public static readonly DialogueEvent dialogueEvent = new();
    public class DialogueEvent
    {
        public UnityAction<DialogueEvent> action;
    }




    public static readonly SampleEventClass sampleEvent = new(); 
    public class SampleEventClass
    {
        //for variable Change
        public UnityAction<int> OnHealthChanged;
    }

   
    public class SampleEventUsage
    {
        public void OnEnable()
        {
            EventManager.sampleEvent.OnHealthChanged += UpdateHealth;
        }
        public void OnDisable()
        {
            EventManager.sampleEvent.OnHealthChanged -= UpdateHealth;
        }
        private void UpdateHealth(int health)
        {
            EventManager.sampleEvent.OnHealthChanged(health);
        }
    }       
}

