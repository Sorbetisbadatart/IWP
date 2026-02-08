using Unity.VisualScripting;
using UnityEngine;

public class RestArea : MonoBehaviour, Iinteractable
{
    private bool interactable;
    [SerializeField] private Unit player;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
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
        throw new System.NotImplementedException();
    }

    public void Interact()
    {
        if (interactable) 
        player.Heal(player.MaxHealthPoints);
    }
}
