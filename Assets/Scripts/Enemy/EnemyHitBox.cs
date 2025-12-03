using UnityEngine;

public class EnemyHitBox : MonoBehaviour
{
    [SerializeField] private float damageAmount = 1f;
    private bool active = false;

    public void Activate() 
    {
        active = true;
        Debug.Log("🔴 Enemy hitbox ACTIVATED");
    }
    
    public void Deactivate() 
    {
        active = false;
        Debug.Log("⚪ Enemy hitbox DEACTIVATED");
    }

    [System.Obsolete]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"🎯 Trigger detected with: {collision.gameObject.name} (Active: {active})");
        
        if (!active) return;

        if (collision.CompareTag("Player"))
        {
            Debug.Log("✅ Enemy hitbox collided with player!");

            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Debug.Log("💥 Enemy hitbox dealt " + damageAmount + " damage to player!");
                playerHealth.TakeDamage(damageAmount);
            }
            else
            {
                Debug.LogWarning("⚠️ No PlayerHealth component on player!");
            }
        }
    }
}