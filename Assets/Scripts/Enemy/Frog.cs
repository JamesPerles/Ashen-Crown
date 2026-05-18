using UnityEngine;
 
public class Frog : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float speed = 3f;
    [SerializeField] float walkRange = 3f;
    [SerializeField] float Detection = 5f;
    [SerializeField] float attackRange = 3f;
    [SerializeField] float jumpHeight = 5f;
    [SerializeField] float jumpCooldown = 2f;
    [SerializeField] float chaseCooldown = 5f;
    [SerializeField] float attackCooldown = 3f;
    [SerializeField] float maxAttackDuration = 2f;
    [SerializeField] string playerTag = "Player";
    [Header("References")]
    [SerializeField] GameObject bubble;
    [SerializeField] Transform spawnPoint;
    [Header("Hitbox Checks")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckDistance = 6f;
    [SerializeField] Transform wallCheck;
    [SerializeField] float wallCheckDistance = 0.3f;
    [SerializeField] LayerMask groundLayer;
    [Header("Other")]
    Rigidbody2D rb;
    Animator animator;
    EnemyKnockback enemyKnockback;
    Transform Player;
    Vector3 startPos;
    Vector3 originalScale;
    float jumpTimer = 0f;
    float attackTimer = 0f;
    float chaseCooldownTimer = 0f;
    float edgeCheckTimer = 0f;
    float attackStateTimer = 0f;
    [SerializeField] float edgeCheckDelay = 0.1f;
    int direction = 1;
    int lastDirection = 1;
    int attackDirection = 1;
    bool isChasing = false;
    bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemyKnockback = GetComponent<EnemyKnockback>();
        startPos = transform.position;
        originalScale = transform.localScale;
    }

    void Update()
    {
            FindPlayer();
            if (Player == null) return;
        jumpTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;
        chaseCooldownTimer -= Time.deltaTime;
        edgeCheckTimer -= Time.deltaTime;

        bool grounded = IsGrounded();
        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);

        if (enemyKnockback != null && enemyKnockback.isKnockedback)
        {
            if (isAttacking)
            {
                isAttacking = false;
                attackStateTimer = 0f;
                animator.SetBool("isAttacking", false);
            }
            return;
        }

        if (isAttacking)
        {
            attackStateTimer += Time.deltaTime;
            if (attackStateTimer >= maxAttackDuration)
            {
                EndAttack();
            }
        }

        if (!isAttacking)
        {
            bool shouldChase = distanceToPlayer <= Detection && chaseCooldownTimer <= 0f;
            isChasing = shouldChase;
        }

        int currentDirection = isAttacking ? attackDirection : direction;

        if (!isAttacking)
        {
            if (isChasing)
                HandleChase();
            else
                HandlePatrol(grounded);
        }

        animator.SetBool("isJumping", !grounded);

        if (currentDirection != lastDirection)
        {
            FlipSprite(currentDirection);
            lastDirection = currentDirection;
        }
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            Player = playerObj.transform;
    }
    void FlipSprite(int currentDirection)
    {
        transform.localScale = new Vector3(originalScale.x * currentDirection, 
        originalScale.y, originalScale.z);
    }

    void HandlePatrol(bool grounded)
    {
        StopHittingWall(grounded);

        float distanceFromStart = transform.position.x - startPos.x;
        if (Mathf.Abs(distanceFromStart) > walkRange)
            direction = distanceFromStart > 0 ? -1 : 1;

        transform.position += new Vector3(direction * speed * Time.deltaTime, 0f, 0f);

        Jumping();
    }

    void HandleChase()
    {
        float horizontalDistance = Player.position.x - transform.position.x;
        direction = horizontalDistance > 0 ? 1 : -1;

        transform.position += new Vector3(direction * speed * Time.deltaTime, 0f, 0f);

        Jumping();

        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
        if (distanceToPlayer <= attackRange && attackTimer <= 0f)
        {
            attackDirection = (int)Mathf.Sign(horizontalDistance);
            animator.SetBool("isAttacking", true);
            isAttacking = true;
            attackStateTimer = 0f;
            attackTimer = attackCooldown;
            chaseCooldownTimer = chaseCooldown;
        }
    }

    void Jumping()
    {
        if (jumpTimer <= 0f && Mathf.Abs(rb.linearVelocity.y) < 0.01f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpHeight);
            animator.SetBool("isJumping", true);
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
    void StopHittingWall(bool grounded)
    {
        if (grounded && edgeCheckTimer <= 0f && IsAboutToFall())
        {
            direction *= -1;
            edgeCheckTimer = edgeCheckDelay;
        }

        if (IsHittingWall())
        {
            direction *= -1;
            edgeCheckTimer = edgeCheckDelay;
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
        attackStateTimer = 0f;
        animator.SetBool("isAttacking", false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag(playerTag))
            direction *= -1;
    }
}