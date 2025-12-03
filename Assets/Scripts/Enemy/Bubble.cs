using UnityEngine;

public class Bubble : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    private Vector2 moveDirection;

    [Header("Damage Settings")]
    [SerializeField] private float damageAmount = 1f;
    [SerializeField] private string playerTag = "Player";

    [Header("Collision Settings")]
    [SerializeField] public LayerMask groundLayer; // ✅ Layer to detect ground
    [SerializeField] private float groundCheckRadius = 0.05f; // radius for overlap check

    [Header("Sound Effects")]
    [SerializeField] private AudioClip destroySFX;
    private AudioSource audioSource;
    private bool isDestroying = false;

    void Awake()
    {
        // Ensure AudioSource exists
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public void Initialize(Vector3 targetPosition)
    {
        moveDirection = (targetPosition - transform.position).normalized;

        // Rotate bubble toward target
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void Update()
    {
        if (!isDestroying)
            transform.position += (Vector3)moveDirection * speed * Time.deltaTime;

        // ✅ Ground check using OverlapCircle to detect inside ground
        if (!isDestroying)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, groundCheckRadius, groundLayer);
            if (hit != null)
                DestroyBubble();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDestroying) return;

        if (collision.CompareTag(playerTag))
        {
            var playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(damageAmount);

            DestroyBubble();
        }
        else if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            DestroyBubble();
        }
    }

    private void DestroyBubble()
    {
        if (isDestroying) return;

        isDestroying = true;

        // Play destroy SFX
        if (destroySFX != null)
            audioSource.PlayOneShot(destroySFX);

        // Disable visuals and collider
        var sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) sprite.enabled = false;

        var collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        // Destroy after SFX finishes
        Destroy(gameObject, destroySFX != null ? destroySFX.length : 0.05f);
    }
}
