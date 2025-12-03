using UnityEngine;

public class Frog : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float speed = 3f;
    [SerializeField] float walk = 3f;
    [SerializeField] float Detection = 5f;
    [SerializeField] float attackRange = 3f;
    [SerializeField] float jumpHeight = 5f;
    [SerializeField] float jumpCooldown = 2f;
    [SerializeField] float chaseCooldown = 5f;
    [SerializeField] float attackCooldown = 3f;
    [SerializeField] float landingCooldown = 0.15f;
    [SerializeField] float maxAttackDuration = 2f; // ✅ Safety timeout for attack

    [Header("References")]
    [SerializeField] GameObject bubble;
    [SerializeField] Transform spawnPoint;
    [SerializeField] private string playerTag = "Player";
    public SpriteRenderer sr;

    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckDistance = 6f;
    [SerializeField] Transform wallCheck;
    [SerializeField] float wallCheckDistance = 0.3f;
    [SerializeField] LayerMask groundLayer;

    private Transform Player;
    private Rigidbody2D rb;
    private Animator animator;
    private EnemyKnockback enemyKnockback;
    private Vector3 startPos;

    private float jumpTimer = 0f;
    private float attackTimer = 0f;
    private float chaseCooldownTimer = 0f;
    private float landingTimer = 0f;
    private float edgeCheckTimer = 0f;
    private float attackStateTimer = 0f; // ✅ Tracks how long we've been in attack state

    private int direction = 1;
    private int lastDirection = 1;
    private bool isChasing = false;
    private bool hasFired = false;
    private bool isAttacking = false;
    private int attackDirection = 1;

    private Vector3 spawnPointOriginalLocalPos;
    private Vector3 groundCheckOriginalLocalPos;
    private Vector3 wallCheckOriginalLocalPos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemyKnockback = GetComponent<EnemyKnockback>();
        startPos = transform.position;

        spawnPointOriginalLocalPos = spawnPoint.localPosition;

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

        // Timers
        jumpTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;
        chaseCooldownTimer -= Time.deltaTime;
        landingTimer -= Time.deltaTime;
        edgeCheckTimer -= Time.deltaTime;

        bool grounded = IsGrounded();
        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);

        // ===== KNOCKBACK OVERRIDE =====
        if (enemyKnockback != null && enemyKnockback.isKnockedback)
        {
            // Cancel attack - ✅ Also reset attack state timer
            if (isAttacking)
            {
                isAttacking = false;
                attackStateTimer = 0f;
                animator.ResetTrigger("isAttacking");
                animator.SetBool("isAttacking", false);
            }

            // Stop AI movement; Rigidbody handles knockback
            return; // Skip rest of Update
        }
        // ================================

        // ✅ Safety timeout: Force end attack if it's been too long
        if (isAttacking)
        {
            attackStateTimer += Time.deltaTime;
            if (attackStateTimer >= maxAttackDuration)
            {
                Debug.LogWarning("Frog attack state timed out - forcing EndAttack()");
                EndAttack();
            }
        }

        // Detect landing
        if (!IsGrounded() && grounded)
            landingTimer = landingCooldown;

        // Update chasing state
        if (!isAttacking)
        {
            bool shouldChase = distanceToPlayer <= Detection && chaseCooldownTimer <= 0f;
            if (shouldChase != isChasing)
            {
                isChasing = shouldChase;
                hasFired = false;
            }
        }

        int currentDirection = isAttacking ? attackDirection : direction;

        // Movement
        if (!isAttacking)
        {
            if (isChasing)
                HandleChase();
            else
                HandlePatrol(grounded);
        }

        // Flip sprite & spawn point
        if (currentDirection != lastDirection)
        {
            sr.flipX = currentDirection < 0;

            Vector3 localPos = spawnPointOriginalLocalPos;
            localPos.x *= currentDirection;
            spawnPoint.localPosition = localPos;

            FlipGroundAndWallChecks();

            lastDirection = currentDirection;
        }
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            Player = playerObj.transform;
    }

    void HandleChase()
    {
        float horizontalDistance = Player.position.x - transform.position.x;
        direction = horizontalDistance > 0 ? 1 : -1;

        // Move
        transform.position += new Vector3(direction * speed * Time.deltaTime, 0f, 0f);

        // Jump if grounded
        if (jumpTimer <= 0f && Mathf.Abs(rb.linearVelocity.y) < 0.01f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpHeight);
            animator.SetTrigger("isJumping");
            jumpTimer = jumpCooldown;
        }

        // Attack
        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
        if (!hasFired && distanceToPlayer <= attackRange && attackTimer <= 0f)
        {
            attackDirection = (int)Mathf.Sign(Player.position.x - transform.position.x);

            animator.SetTrigger("isAttacking");
            isAttacking = true;
            attackStateTimer = 0f; // ✅ Reset timer when starting attack
            hasFired = true;
            attackTimer = attackCooldown;
            chaseCooldownTimer = chaseCooldown;
        }
    }

    void HandlePatrol(bool grounded)
    {
        // Edge flip
        if (grounded && edgeCheckTimer <= 0f && landingTimer <= 0f && IsAboutToFall())
        {
            direction *= -1;
            edgeCheckTimer = 0.1f;
        }

        // Wall flip
        if (IsHittingWall())
        {
            direction *= -1;
            edgeCheckTimer = 0.1f;
        }

        // Patrol within range
        float distanceFromStart = transform.position.x - startPos.x;
        if (Mathf.Abs(distanceFromStart) > walk)
            direction = distanceFromStart > 0 ? -1 : 1;

        transform.position += new Vector3(direction * speed * Time.deltaTime, 0f, 0f);

        // Jump
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

    public void FireProjectile()
    {
        if (Player == null) return;

        GameObject bubbleObj = Instantiate(bubble, spawnPoint.position, Quaternion.identity);
        Bubble bubbleScript = bubbleObj.GetComponent<Bubble>();
        if (bubbleScript != null)
            bubbleScript.Initialize(Player.position);
    }

    public void EndAttack()
    {
        isAttacking = false;
        attackStateTimer = 0f; // ✅ Reset timer when attack ends normally
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag(playerTag))
            direction *= -1;
    }
}