using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Win : MonoBehaviour
{

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene("WinScreen");
        }

    }
}
