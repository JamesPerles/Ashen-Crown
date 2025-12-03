using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float runSpeed = 2f;
    [SerializeField] float jump = 5f;
    [SerializeField] float rayLength = 0.5f;
    [SerializeField] int HP = 5;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public Transform projectileSpawnPoint; // Assign from inspector

    bool isGrounded = false;
    bool isWalking = false;
    bool isRunning = false;
    private bool isDead = false;

    private float baseSpeed;
    public Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;

    private PlayerKnockback knockback;

    [HideInInspector] public bool canMove = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        baseSpeed = moveSpeed;

        knockback = GetComponent<PlayerKnockback>();
    }

    void Update()
    {
        if (isDead) return;
        if (knockback != null && knockback.isKnockedback) return;

        // Horizontal movement
        float moveX = Input.GetAxis("Horizontal");
        if (canMove)
            rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Walking/Running animations
        isWalking = moveX != 0;
        animator.SetBool("isWalking", isWalking);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = baseSpeed * runSpeed;
            isRunning = true;
        }
        else
        {
            moveSpeed = baseSpeed;
            isRunning = false;
        }
        animator.SetBool("isRunning", isRunning);

        // Flip sprite
        if (moveX > 0) sr.flipX = false;
        else if (moveX < 0) sr.flipX = true;

        // Make projectile spawn point follow player facing
        if (projectileSpawnPoint != null)
        {
            projectileSpawnPoint.localPosition = new Vector3(
                sr.flipX ? -Mathf.Abs(projectileSpawnPoint.localPosition.x) : Mathf.Abs(projectileSpawnPoint.localPosition.x),
                projectileSpawnPoint.localPosition.y,
                projectileSpawnPoint.localPosition.z
            );
        }

        // Ground check
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, rayLength, groundLayer);

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump);
            animator.SetBool("isJumping", true);
        }

        if (!wasGrounded && isGrounded)
        {
            animator.SetBool("isJumping", false);
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            HP -= 1;
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
    }

    public void OnPlayerDeath()
    {
        isDead = true;
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isJumping", false);
        rb.linearVelocity = Vector2.zero;
    }
}
