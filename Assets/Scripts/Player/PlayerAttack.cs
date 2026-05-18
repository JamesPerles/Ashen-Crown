using UnityEngine;
public class PlayerAttack : MonoBehaviour
{
    [SerializeField] float attackDuration = 0.2f;
    [SerializeField] Collider2D[] attackHitboxes;
    [SerializeField] float attackDamage = 1f;
    [SerializeField] AudioClip attackSFX;
    public AudioSource audioSource;
    public bool isHitboxActive = false;
    Animator animator;
    Collider2D playerCollider;
    public PlayerController playerController;
    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        for (int i = 0; i < attackHitboxes.Length; i++)
        {
            attackHitboxes[i].enabled = false;
            if (!attackHitboxes[i].isTrigger)
                attackHitboxes[i].isTrigger = true;
            Physics2D.IgnoreCollision(attackHitboxes[i], playerCollider);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Attack();
        }
    }
    public void Attack()
    {
        animator.SetTrigger("Attack");
        if (audioSource != null && attackSFX != null)
        {
            audioSource.PlayOneShot(attackSFX);
        }
    }
    public void EnableHitbox(int index)
    {
        if (index < 0 || index >= attackHitboxes.Length) return;
        attackHitboxes[index].enabled = true;
        isHitboxActive = true;
        Invoke(nameof(DisableHitboxes), attackDuration);
    }
    public void DisableHitboxes()
    {
        foreach (var hitbox in attackHitboxes)
        {
            if (hitbox != null)
                hitbox.enabled = false;
        }
        isHitboxActive = false;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy") && !other.CompareTag("Enemy2")) return;
        if (other.CompareTag("Enemy2"))
        {
            Destroy(other.gameObject);
            return;
        }
        EnemyHP enemyHealth = other.GetComponent<EnemyHP>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(attackDamage);
        }
        if(playerController.isDiving == true)
        {
            playerController.rb.linearVelocity = new Vector2(0f, 15);
            playerController.isDiving = false;
        }
    }
}