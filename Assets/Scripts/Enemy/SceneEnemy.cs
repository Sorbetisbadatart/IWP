using UnityEngine;

public class SceneEnemy : MonoBehaviour
{
    private GameManager gameManager;
    [SerializeField] private Unit player;
    [SerializeField] private Unit Enemy;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();      
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            gameManager.TriggerBattle(player,Enemy);
            GetComponent<BoxCollider2D>().enabled = false;
            GetComponent<SpriteRenderer>().enabled = false;
            this.gameObject.SetActive(false);
        }
    }
}
