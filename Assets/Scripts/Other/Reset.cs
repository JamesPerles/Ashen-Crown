using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string sceneName = "Level1"; // Set your target scene here

    void Update()
    {
        // Optional: allow pressing Space to load the scene
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoadScene();
        }
    }

    // This can be linked to a UI button
    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene name not set in SceneTransition!");
        }
    }
}
