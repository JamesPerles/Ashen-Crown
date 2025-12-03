using UnityEngine;

public class Shadow : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float speed = 3f;
    [SerializeField] float walk = 3f;
    [SerializeField] float Detection = 5f;
    [SerializeField] float jumpHeight = 5f;
    [SerializeField] float jumpCooldown = 2f;
    [SerializeField] float attackCooldown = 3f;
    [SerializeField] float attackRange = 1.5f;
    [SerializeField] float detectionHysteresis = 1f;

    [Header("References")]
    [SerializeField] private string playerTag = "Player";
    public SpriteRenderer sr;
    private Animator animator;
    private Rigidbody2D rb;
    private Transform Player;
    private Vector3 startPos;
    private EnemyKnockback enemyKnockback; // Knockback controller

    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckDistance = 6f;
    [SerializeField] Transform wallCheck;
    [SerializeField] float wallCheckDistance = 0.3f;
    [SerializeField] LayerMask groundLayer;

    [Header("Attack")]
    [SerializeField] private Collider2D[] attackHitboxes;
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private float damageAmount = 1f;

    private Vector3[] hitboxOriginalLocalPos;
    private float jumpTimer = 0f;
    private float attackTimer = 0f;
    private bool isChasing = false;
    private bool isAttacking = false;
    private int direction = 1;
    private int lastDirection = 1;

    [SerializeField] float edgeCheckDelay = 0.1f;
    private float edgeCheckTimer = 0f;

    [SerializeField] float landingCooldown = 0.15f;
    private float landingTimer = 0f;

    private Vector3 groundCheckOriginalLocalPos;
    private Vector3 wallCheckOriginalLocalPos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemyKnockback = GetComponent<EnemyKnockback>(); // Reference knockback
        startPos = transform.position;

        hitboxOriginalLocalPos = new Vector3[attackHitboxes.Length];
        for (int i = 0; i < attackHitboxes.Length; i++)
        {
            attackHitboxes[i].enabled = false;
            hitboxOriginalLocalPos[i] = attackHitboxes[i].transform.localPosition;
        }

        if (groundCheck != null)
            groundCheckOriginalLocalPos = groundCheck.localPosition;
        if (wallCheck != null)
            wallCheckOriginalLocalPos = wallCheck.localPosition;

        FindPlayer();
    }

    void Update()
    {
        if (Player == null || !Player.gameObject.activeInHierarchy)
        {
            FindPlayer();
            if (Player == null) return;
        }

        jumpTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;
        edgeCheckTimer -= Time.deltaTime;
        landingTimer -= Time.deltaTime;

        bool grounded = IsGrounded();

        if (grounded && rb.linearVelocity.y <= 0.01f)
            landingTimer = landingCooldown;

        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);

        // ===== KNOCKBACK OVERRIDE =====
        if (enemyKnockback != null && enemyKnockback.isKnockedback)
        {
            // Cancel attack
            if (isAttacking)
            {
                isAttacking = false;
                animator.ResetTrigger("isAttacking");
                animator.SetBool("isAttacking", false);
            }

            // Stop AI movement; Rigidbody handles knockback
            return; // Skip rest of Update until knockback ends
        }
        // ================================

        // Handle chasing / idle
        if (!isAttacking)
        {
            if (isChasing)
            {
                if (distanceToPlayer > Detection + detectionHysteresis)
                    isChasing = false;
            }
            else
            {
                if (distanceToPlayer <= Detection)
                    isChasing = true;
            }
        }

        if (!isAttacking)
        {
            if (isChasing)
                HandleChase(distanceToPlayer, grounded);
            else
                HandlePatrol(grounded);
        }

        // Flip sprite & helpers only if direction changed
        if (direction != lastDirection)
        {
            sr.flipX = direction < 0;
            FlipHitboxes();
            FlipGroundAndWallChecks();
            lastDirection = direction;
        }
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            Player = playerObj.transform;
    }

    void HandleChase(float distanceToPlayer, bool grounded)
    {
        if (isAttacking) return;

        float horizontalDistance = Player.position.x - transform.position.x;
        direction = horizontalDistance > 0 ? 1 : -1;

        if (grounded && edgeCheckTimer <= 0f && landingTimer <= 0f && IsAboutToFall())
        {
            direction *= -1;
            edgeCheckTimer = edgeCheckDelay;
        }

        if (IsHittingWall())
        {
            direction *= -1;
            edgeCheckTimer = edgeCheckDelay;
        }

        transform.position += new Vector3(direction * speed * Time.deltaTime, 0f, 0f);

        if (jumpTimer <= 0f && Mathf.Abs(rb.linearVelocity.y) < 0.01f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpHeight);
            animator.SetTrigger("isJumping");
            jumpTimer = jumpCooldown;
        }

        if (distanceToPlayer <= attackRange && attackTimer <= 0f)
        {
            isAttacking = true;
            direction = (int)Mathf.Sign(horizontalDistance);
            sr.flipX = direction < 0;
            FlipHitboxes();
            FlipGroundAndWallChecks();
            animator.SetTrigger("isAttacking");
            attackTimer = attackCooldown;
        }
    }

    void HandlePatrol(bool grounded)
    {
        if (isAttacking) return;

        if (grounded && edgeCheckTimer <= 0f && landingTimer <= 0f && IsAboutToFall())
        {
            direction *= -1;
            edgeCheckTimer = edgeCheckDelay;
        }

        if (IsHittingWall())
        {
            direction *= -1;
            edgeCheckTimer = edgeCheckDelay;
        }

        float distanceFromStart = transform.position.x - startPos.x;
        if (Mathf.Abs(distanceFromStart) > walk)
            direction = distanceFromStart > 0 ? -1 : 1;

        transform.position += new Vector3(direction * speed * Time.deltaTime, 0f, 0f);

        if (jumpTimer <= 0f && Mathf.Abs(rb.linearVelocity.y) < 0.01f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpHeight);
            animator.SetTrigger("isJumping");
            jumpTimer = jumpCooldown;
        }
    }

    bool IsGrounded() => Mathf.Abs(rb.linearVelocity.y) < 0.01f;

    bool IsAboutToFall()
    {
        if (groundCheck == null) return false;
        Vector2 checkPos = groundCheck.position + Vector3.right * direction * 0.2f + Vector3.up * 0.05f;
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider == null;
    }

    bool IsHittingWall()
    {
        if (wallCheck == null) return false;
        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, Vector2.right * direction, wallCheckDistance, groundLayer);
        return hit.collider != null && !hit.collider.CompareTag(playerTag);
    }

    void FlipGroundAndWallChecks()
    {
        if (groundCheck != null)
        {
            Vector3 gcPos = groundCheckOriginalLocalPos;
            gcPos.x *= direction;
            groundCheck.localPosition = gcPos;
        }
        if (wallCheck != null)
        {
            Vector3 wcPos = wallCheckOriginalLocalPos;
            wcPos.x *= direction;
            wallCheck.localPosition = wcPos;
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    public void EnableHitbox(int index)
    {
        if (index < 0 || index >= attackHitboxes.Length) return;
        attackHitboxes[index].enabled = true;
        Invoke(nameof(DisableHitboxes), attackDuration);
    }

    public void DisableHitboxes()
    {
        foreach (var hitbox in attackHitboxes)
            hitbox.enabled = false;
    }

    private void FlipHitboxes()
    {
        for (int i = 0; i < attackHitboxes.Length; i++)
        {
            Vector3 localPos = hitboxOriginalLocalPos[i];
            localPos.x *= direction;
            attackHitboxes[i].transform.localPosition = localPos;
        }
    }

    [System.Obsolete]
    void OnTriggerEnter2D(Collider2D collision)
    {
        bool anyHitboxActive = false;
        foreach (var hitbox in attackHitboxes)
        {
            if (hitbox.enabled)
            {
                anyHitboxActive = true;
                break;
            }
        }

        if (anyHitboxActive && collision.CompareTag(playerTag))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(damageAmount);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isAttacking) return;
        if (!collision.gameObject.CompareTag(playerTag))
            direction *= -1;
    }
}