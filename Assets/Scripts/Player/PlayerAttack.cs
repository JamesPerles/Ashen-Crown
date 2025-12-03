using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private Collider2D[] attackHitboxes;
    [SerializeField] private float attackDamage = 1f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip attackSFX;

    private Animator animator;
    private SpriteRenderer sr;
    private Collider2D playerCollider;
    public AudioSource audioSource;
    private Vector3[] originalHitboxPositions;

    void Awake()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();

        // ✅ Try to get AudioSource from PlayerHealth first (shared source)
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null && playerHealth.GetComponent<AudioSource>() != null)
        {
            audioSource = playerHealth.GetComponent<AudioSource>();
        }
        else
        {
            // 🔸 If no shared one found, create a fallback AudioSource
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.outputAudioMixerGroup = null;
                audioSource.playOnAwake = false;
                Debug.LogWarning("⚠️ PlayerAttack: No shared AudioSource found! Created a new one as fallback.");
            }
        }

        // Setup hitboxes
        originalHitboxPositions = new Vector3[attackHitboxes.Length];
        for (int i = 0; i < attackHitboxes.Length; i++)
        {
            originalHitboxPositions[i] = attackHitboxes[i].transform.localPosition;
            attackHitboxes[i].enabled = false;

            if (!attackHitboxes[i].isTrigger)
                attackHitboxes[i].isTrigger = true;

            // Ignore collisions with player
            Physics2D.IgnoreCollision(attackHitboxes[i], playerCollider);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Attack();
        }

        // Flip hitbox positions with sprite
        for (int i = 0; i < attackHitboxes.Length; i++)
        {
            Vector3 pos = originalHitboxPositions[i];
            pos.x = Mathf.Abs(pos.x) * (sr.flipX ? -1 : 1);
            attackHitboxes[i].transform.localPosition = pos;
        }
    }

    public void Attack()
    {
        // Play attack animation and SFX
        animator.SetTrigger("Attack");

        if (audioSource != null && attackSFX != null)
        {
            audioSource.PlayOneShot(attackSFX);
        }
    }

    // Called via animation event
    public void EnableHitbox(int index)
    {
        if (index < 0 || index >= attackHitboxes.Length) return;

        attackHitboxes[index].enabled = true;
        Invoke(nameof(DisableHitboxes), attackDuration);
    }

    public void DisableHitboxes()
    {
        foreach (var hitbox in attackHitboxes)
        {
            if (hitbox != null)
                hitbox.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only affect enemies
        if (!other.CompareTag("Enemy")) return;

        EnemyHP enemyHealth = other.GetComponent<EnemyHP>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(attackDamage);
        }
    }
}
