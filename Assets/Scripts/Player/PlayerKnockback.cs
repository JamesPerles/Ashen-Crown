using System.Collections;
using UnityEngine;
public class PlayerKnockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.3f;
    [Header("Invincibility Settings")]
    [SerializeField] float invincibilityDuration = 1.5f;
    [SerializeField] float flashInterval = 0.1f;
    [SerializeField] float flashAlpha = 0.3f;
    [Header("References")]
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] Collider2D playerCollider;
    [SerializeField] PlayerAttack playerAttack;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Animator animator;
    float knockbackTimer = 0f;
    public bool IsKnockedback { get; private set; } = false;
    int invincibilityStack = 0;
    public bool IsInvincible => invincibilityStack > 0;
    public void AddInvincibility() => invincibilityStack++;
    public void RemoveInvincibility() => invincibilityStack = Mathf.Max(0, invincibilityStack - 1);
    public void ClearInvincibility() => invincibilityStack = 0;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();
        if (playerAttack == null)
            playerAttack = GetComponent<PlayerAttack>();
    }
    void Update()
    {
        if (playerHealth != null && playerHealth.currentHP <= 0 && IsKnockedback)
        {
            EndKnockback();
            return;
        }
        if (IsKnockedback)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
                EndKnockback();
        }
    }
    public void ApplyKnockback(Transform source)
    {
        if (IsInvincible || (playerHealth != null && playerHealth.currentHP <= 0))
            return;
        IsKnockedback = true;
        knockbackTimer = knockbackDuration;
        if (animator != null)
            animator.SetBool("isKnockedback", true);
        Vector2 direction = (transform.position - source.position).normalized;
        rb.linearVelocity = direction * knockbackForce;
        StartCoroutine(InvincibilityFlash());
    }
    IEnumerator InvincibilityFlash()
    {
        AddInvincibility();
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
        RemoveInvincibility();
    }
    void EndKnockback()
    {
        IsKnockedback = false;
        rb.linearVelocity = Vector2.zero;
        if (animator != null)
            animator.SetBool("isKnockedback", false);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy2"))
        {
            if (playerAttack != null && playerAttack.isHitboxActive) return;
            ApplyKnockback(other.transform);
            return;
        }
        if (!Physics2D.IsTouching(playerCollider, other)) return;
        if (other.CompareTag("Enemy"))
            ApplyKnockback(other.transform);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.otherCollider != playerCollider) return;
        if (collision.collider.CompareTag("Enemy"))
            ApplyKnockback(collision.transform);
    }
}