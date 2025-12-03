using System.Collections;
using UnityEngine;

public class EnemyKnockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] public float knockbackForce = 15f;
    [SerializeField] public float knockbackDuration = 0.35f;
    [SerializeField] public float upwardForce = 6f;

    [Header("Invincibility Settings")]
    [SerializeField] public float invincibilityDuration = 0.8f;
    [SerializeField] public float flashInterval = 0.1f;
    [SerializeField] public float flashAlpha = 0.4f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // ✅ Public so AI scripts can check
    public bool isKnockedback { get; private set; } = false;
    public bool isInvincible { get; private set; } = false;

    // Knockback velocity tracking
    private Vector2 knockbackVelocity;
    private float knockbackTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Ensure no drag interferes with knockback
        if (rb != null)
        {
            rb.linearDamping = 0f;
        }
    }

    void Update()
    {
        if (isKnockedback)
        {
            knockbackTimer -= Time.deltaTime;

            if (knockbackTimer <= 0f)
            {
                isKnockedback = false;
                knockbackVelocity = Vector2.zero;
            }
        }
    }

    void FixedUpdate()
    {
        // Aggressively maintain knockback velocity in physics update
        if (isKnockedback)
        {
            // Calculate decay
            float decayFactor = Mathf.Clamp01(knockbackTimer / knockbackDuration);
            
            // FORCE the velocity every physics frame
            Vector2 targetVelocity = new Vector2(
                knockbackVelocity.x * decayFactor, 
                rb.linearVelocity.y  // Keep natural gravity
            );
            
            rb.linearVelocity = targetVelocity;
        }
    }

    public void ApplyKnockback(Transform source)
    {
        if (isInvincible) return;

        // ✅ Start knockback
        isKnockedback = true;
        knockbackTimer = knockbackDuration;

        // Calculate knockback direction (horizontal)
        Vector2 direction = ((Vector2)transform.position - (Vector2)source.position).normalized;
        
        // Store horizontal knockback velocity
        knockbackVelocity = new Vector2(direction.x * knockbackForce, 0f);

        // Apply immediate impulse for knockback with upward force
        rb.linearVelocity = new Vector2(direction.x * knockbackForce, upwardForce);

        StartCoroutine(InvincibilityFlash());
    }

    private IEnumerator InvincibilityFlash()
    {
        isInvincible = true;
        float elapsed = 0f;
        Color originalColor = spriteRenderer.color;

        while (elapsed < invincibilityDuration)
        {
            Color flashColor = originalColor;
            flashColor.a = flashAlpha;
            spriteRenderer.color = flashColor;

            yield return new WaitForSeconds(flashInterval);

            spriteRenderer.color = originalColor;

            yield return new WaitForSeconds(flashInterval);

            elapsed += flashInterval * 2;
        }

        spriteRenderer.color = originalColor;
        isInvincible = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Attack"))
        {
            ApplyKnockback(collision.transform);
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Attack"))
        {
            ApplyKnockback(collider.transform);
        }
    }
}