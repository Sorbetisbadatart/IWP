using Unity.Cinemachine;
using UnityEngine;

public class MapTransition : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D mapBoundary;
    private CinemachineConfiner2D confiner;
    [SerializeField] private Direction direction;
    [SerializeField] private float teleportMagnitude = 0.25f;
    [SerializeField] private bool TeleportToGameObject = false;
    [SerializeField] private GameObject GameObjectToTeleportTo;

    private enum Direction
    {
        UP,
        DOWN,
        LEFT,
        RIGHT
    }

    private void Start()
    {
        confiner = FindFirstObjectByType<CinemachineConfiner2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
       

        if (collision.gameObject.CompareTag("Player"))
        {
            if (!TeleportToGameObject)
            {
                confiner.BoundingShape2D = mapBoundary;
                UpdatePlayerDirection(collision.gameObject);
            }
            else
            {
                UpdatePlayerPosition(collision.gameObject,GameObjectToTeleportTo);
                confiner.BoundingShape2D = mapBoundary;
            }
           
        }
    }

    //shove the player in the direction so that it bypasses the boundary
    private void UpdatePlayerDirection(GameObject player)
    {
        Vector3 newPos = player.transform.position;

        switch (direction)
        {
            case Direction.UP:
                newPos.y += teleportMagnitude;
                break;
            case Direction.DOWN:
                newPos.y -= teleportMagnitude;
                break;
            case Direction.LEFT:
                newPos.x -= teleportMagnitude; break;
            case Direction.RIGHT:
                newPos.x += teleportMagnitude;
                break;
        }

        player.transform.position = newPos;

    }

    private void UpdatePlayerPosition(GameObject player, GameObject teleportLocation)
    {
        player.transform.position = teleportLocation.transform.position;
    }
}
