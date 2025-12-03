using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public int damage = 1; // Damage dealt to enemy
    public string enemyTag = "Enemy"; // Tag your enemies with this
    public bool destroyOnHit = true; // Should projectile disappear after hitting?

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collider has the correct tag
        if (collision.CompareTag(enemyTag))
        {
            // Try to find an EnemyHealth or similar component
            EnemyHP enemy = collision.GetComponent<EnemyHP>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // Optionally destroy the projectile
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }
}
