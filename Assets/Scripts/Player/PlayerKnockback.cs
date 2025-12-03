using System.Collections;
using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] public float knockbackForce = 10f;
    [SerializeField] public float knockbackDuration = 0.3f;

    [Header("Invincibility Settings")]
    [SerializeField] private float invincibilityDuration = 1.5f;
    [SerializeField] private float flashInterval = 0.1f;
    [SerializeField] private float flashAlpha = 0.3f;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [Tooltip("Colliders that should NOT cause player knockback when they overlap an enemy (typically your player's attack hitboxes).")]
    [SerializeField] private Collider2D[] ignoreHitboxes;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float knockbackTimer = 0f;
    public bool isKnockedback { get; private set; } = false;
    public bool isInvincible { get; private set; } = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (playerHealth != null && playerHealth.currentHP <= 0 && isKnockedback)
        {
            isKnockedback = false;
            rb.linearVelocity = Vector2.zero;
            if (animator != null)
                animator.SetBool("isKnockedback", false);
            return;
        }

        if (isKnockedback)
        {
            knockbackTimer -= Time.deltaTime;

            if (knockbackTimer <= 0f)
            {
                isKnockedback = false;
                rb.linearVelocity = Vector2.zero;

                if (animator != null)
                    animator.SetBool("isKnockedback", false);
            }
        }
    }

    public void ApplyKnockback(Transform source)
    {
        if (isInvincible || (playerHealth != null && playerHealth.currentHP <= 0))
            return;

        isKnockedback = true;
        knockbackTimer = knockbackDuration;

        if (animator != null)
            animator.SetBool("isKnockedback", true);

        Vector2 direction = (transform.position - source.position).normalized;
        rb.linearVelocity = direction * knockbackForce;

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
        Collider2D other = collision.collider;
        if (ShouldApplyKnockback(other))
            ApplyKnockback(collision.transform);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (ShouldApplyKnockback(collider))
            ApplyKnockback(collider.transform);
    }

    /// <summary>
    /// Returns true if knockback SHOULD be applied for this other collider.
    /// If other is not an Enemy -> false.
    /// If any inspector-assigned ignoreHitbox is currently touching 'other' -> false (do not apply).
    /// Otherwise true.
    /// </summary>
    private bool ShouldApplyKnockback(Collider2D other)
    {
        if (other == null) return false;

        // Only consider enemies
        if (!other.CompareTag("Enemy")) return false;

        // If any of the configured ignore-hitboxes is touching this collider right now,
        // we do NOT apply knockback.
        if (ignoreHitboxes != null)
        {
            foreach (var hb in ignoreHitboxes)
            {
                if (hb == null) continue;

                // Only consider enabled hitboxes (so you can disable them when not in use)
                if (!hb.enabled) continue;

                // If the hitbox is currently touching the other collider, ignore knockback
                // IsTouching checks collision/overlap between two colliders
                if (hb.IsTouching(other))
                    return false;
            }
        }

        // Otherwise, apply knockback for real enemy contacts
        return true;
    }
}
