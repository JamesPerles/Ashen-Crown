using UnityEngine;
public class SpawnBook : MonoBehaviour
{
    [SerializeField] GameObject bookPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] CutsceneController cutsceneController; 
     bool hasSpawned = false;
    void Start()
    {
        if (cutsceneController != null && cutsceneController.dialogueSystem != null)
        {
            cutsceneController.dialogueSystem.OnDialogueComplete += OnCutsceneComplete;
        }
        else
        {
            SpawnBookPrefab();
        }
    }
     void OnCutsceneComplete()
    {
        SpawnBookPrefab();
    }
    void SpawnBookPrefab()
    {
        if (bookPrefab != null && !hasSpawned)
        {
            Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
            Instantiate(bookPrefab, spawnPosition, Quaternion.identity);
            hasSpawned = true;
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Book"))
        {
            Destroy(collision.gameObject);
        }
}
}