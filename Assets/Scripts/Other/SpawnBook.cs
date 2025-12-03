using UnityEngine;

public class SpawnBook : MonoBehaviour
{
    [SerializeField] GameObject bookPrefab;
    [SerializeField] Transform spawnPoint; // ✅ Optional spawn point
    [SerializeField] CutsceneController cutsceneController; // ✅ Reference to cutscene
    
    private bool hasSpawned = false;
    private bool cutsceneComplete = false;

    void Start()
    {
        // ✅ Register to cutscene's dialogue complete callback
        if (cutsceneController != null && cutsceneController.dialogueManager != null)
        {
            cutsceneController.dialogueManager.RegisterDialogueComplete(OnCutsceneComplete);
        }
        else
        {
            // If no cutscene, spawn immediately
            SpawnBookPrefab();
        }
    }

    private void OnCutsceneComplete()
    {
        cutsceneComplete = true;
        SpawnBookPrefab();
    }

    private void SpawnBookPrefab()
    {
        if (bookPrefab != null && !hasSpawned)
        {
            // Use spawn point if assigned, otherwise use this object's position
            Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
            Instantiate(bookPrefab, spawnPosition, Quaternion.identity);
            hasSpawned = true;
        }
        else if (bookPrefab == null)
        {
            Debug.LogWarning("⚠️ Book prefab is not assigned!");
        }
    }
}