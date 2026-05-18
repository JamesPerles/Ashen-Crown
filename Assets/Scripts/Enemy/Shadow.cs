using UnityEngine;
public class Shadow : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float speed = 3f;
    [SerializeField] float walkRange = 3f;
    [SerializeField] float Detection = 5f;
    [SerializeField] float jumpHeight = 5f;
    [SerializeField] float jumpCooldown = 2f;
    [SerializeField] float attackCooldown = 3f;
    [SerializeField] float attackRange = 1.5f;
    [SerializeField] float preventFlipping = 1f;
    [SerializeField] string playerTag = "Player";
    [Header("Hitbox Checks")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckDistance = 6f;
    [SerializeField] Transform wallCheck;
    [SerializeField] float wallCheckDistance = 0.3f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Collider2D[] attackHitboxes;
    [SerializeField] float attackDuration = 0.2f;
    [SerializeField] float edgeCheckDelay = 0.1f;
    float edgeCheckTimer = 0f;
    [Header("Other")]
    Animator animator;
    Rigidbody2D rb;
    Transform Player;
    Vector3 startPos;
    EnemyKnockback enemyKnockback;
    float jumpTimer = 0f;
    float attackTimer = 0f;
    bool isChasing = false;
    bool isAttacking = false;
    int direction = 1;
    int lastDirection = 1;
    Vector3 originalScale;
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
        edgeCheckTimer -= Time.deltaTime;
 
        bool grounded = IsGrounded();
 
        if (grounded && rb.linearVelocity.y <= 0.01f)
            jumpCooldown = 0;
 
        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
 
        if (enemyKnockback != null && enemyKnockback.isKnockedback)
        {
            if (isAttacking)
            {
                isAttacking = false;
                animator.SetBool("isAttacking", false);
            }
            return;
        }

        if (!isAttacking)
        {
            if (isChasing)
            {
                if (distanceToPlayer > Detection + preventFlipping)
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
        animator.SetBool("isJumping", !grounded);
 
        if (direction != lastDirection)
            FlipSprite();
        lastDirection = direction;
    }
    void FlipSprite()
    {
        transform.localScale = new Vector3(originalScale.x * direction, originalScale.y, originalScale.z);
    }
    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            Player = playerObj.transform;
    }
    void HandlePatrol(bool grounded)
    {
        if (isAttacking) return;
        StopHittingWall(grounded);
        float distanceFromStart = transform.position.x - startPos.x;
        if (Mathf.Abs(distanceFromStart) > walkRange)
            direction = distanceFromStart > 0 ? -1 : 1;
        transform.position += new Vector3(direction * speed * Time.deltaTime, 0f, 0f);
        Jumping();
    }
    void HandleChase(float distanceToPlayer, bool grounded)
    {
        if (isAttacking) return;
        float horizontalDistance = Player.position.x - transform.position.x;
        direction = horizontalDistance > 0 ? 1 : -1;
        StopHittingWall(grounded);
        transform.position += new Vector3(direction * speed * Time.deltaTime, 0f, 0f);
        Jumping();
        if (distanceToPlayer <= attackRange && attackTimer <= 0f)
        {
            isAttacking = true;
            direction = (int)Mathf.Sign(horizontalDistance);
            animator.SetBool("isAttacking", true);
            attackTimer = attackCooldown;
            FlipSprite();
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
    public void EndAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isAttacking) return;
        if (!collision.gameObject.CompareTag(playerTag))
            direction *= -1;
    }
        }
