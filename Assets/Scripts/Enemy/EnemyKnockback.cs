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
    [Header("References")]
    [SerializeField] Collider2D enemyCollider;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    public bool isKnockedback = false;
    public bool isInvincible = false;
    Vector2 knockbackVelocity;
    float knockbackTimer = 0f;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (enemyCollider == null)
            enemyCollider = GetComponent<Collider2D>();
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
        if (isKnockedback)
        {
            float decayFactor = Mathf.Clamp01(knockbackTimer / knockbackDuration);
            Vector2 targetVelocity = new Vector2(
                knockbackVelocity.x * decayFactor,
                rb.linearVelocity.y
            );
            rb.linearVelocity = targetVelocity;
        }
    }
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (!collider.gameObject.CompareTag("Attack")) return;
        if (!enemyCollider.IsTouching(collider)) return;
        ApplyKnockback(collider.transform);
    }
    public void ApplyKnockback(Transform source)
    {
        if (isInvincible) return;
        isKnockedback = true;
        knockbackTimer = knockbackDuration;
        Vector2 direction = ((Vector2)transform.position - (Vector2)source.position).normalized;
        knockbackVelocity = new Vector2(direction.x * knockbackForce, 0f);
        rb.linearVelocity = new Vector2(direction.x * knockbackForce, upwardForce);
        StartCoroutine(InvincibilityFlash());
    }
    IEnumerator InvincibilityFlash()
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
}