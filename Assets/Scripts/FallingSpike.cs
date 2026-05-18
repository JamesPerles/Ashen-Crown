using UnityEngine;
public class FallingSpike : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy")) return;
        if (collision.gameObject.CompareTag("Ground"))
            Destroy(gameObject);
    }
}
 