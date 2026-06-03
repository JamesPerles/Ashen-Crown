using UnityEngine;
public class DeathPit : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public PlayerKnockback playerKnockback;
    public PlayerController playerController;
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerKnockback.ClearInvincibility();
            playerController.rb.linearVelocity = new Vector2(0f, 30f);
            playerHealth.TakeDamage(3f);
        }
    }
}