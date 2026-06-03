using UnityEngine;
public class BossRoomSpawner : MonoBehaviour
{
public GameObject wall;

void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            wall.SetActive(true);
        }
    }
}
