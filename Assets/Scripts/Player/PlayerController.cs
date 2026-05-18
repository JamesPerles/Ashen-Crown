using UnityEngine;
using System.Collections;
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float runMultiplier = 2f;
    [SerializeField] float jump = 5f;
    [Header("Ground Check")]
    [SerializeField] float groundCheckWidth = 0.5f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    [Header("Other")]
    public Transform projectileSpawnPoint;
    public Rigidbody2D rb;
    Animator animator;
    PlayerKnockback knockback;
    bool isGrounded;
    bool isWalking;
    bool isRunning;
    bool isDead;
    public bool canMove = true;
    float baseSpeed;
    int direction = 1;
    Vector3 originalScale;
    [SerializeField] float dive = -10f;
    [SerializeField] float diveMove = 10f;
    public bool isDiving;
    bool isDashing;
    [SerializeField] float dashDist = 5f;
    [SerializeField] float dashTime = 0.2f;
    public float coyoteTime = 0.2f;
    bool runLatched = false;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        knockback = GetComponent<PlayerKnockback>();
        baseSpeed = moveSpeed;
        originalScale = transform.localScale;
    }
    void Update()
    {
        if (isGrounded)
        {
            isDiving = false;
            animator.SetBool("isDiving", false);
        }
        if (isDead) return;
        if (knockback != null && knockback.IsKnockedback) return;
        float moveX = Input.GetAxisRaw("Horizontal");
        if (!isDiving && !isDashing)
        {
            if (canMove)
                rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
            else
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        isWalking = moveX != 0;
        animator.SetBool("isWalking", isWalking);
        if (Input.GetKeyDown(KeyCode.LeftShift))
            runLatched = true;
        if (Input.GetKeyUp(KeyCode.LeftShift))
            runLatched = false;
        isRunning = runLatched;
        moveSpeed = isRunning ? baseSpeed * runMultiplier : baseSpeed;
        animator.SetBool("isRunning", isRunning);
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapBox(
            groundCheck.position,
            new Vector2(groundCheckWidth * 2f, 0.1f),
            0f,
            groundLayer
        );
        if (Input.GetButtonDown("Jump") && isGrounded || Input.GetButtonDown("Jump") && coyoteTime > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump);
            animator.SetBool("isJumping", true);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) && !isGrounded)
        {
            DiveKick();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Dash();
        }
        if (!isGrounded)
        {
            coyoteTime -= Time.deltaTime;
        }
        if (!wasGrounded && isGrounded)
        {
            animator.SetBool("isJumping", false);
            coyoteTime = 0.2f;
        }
        if (moveX > 0)
            direction = 1;
        else if (moveX < 0)
            direction = -1;
        FlipSprite();
        if (direction == -1)
        {
            diveMove = -10;
            dashDist = -20;
        }
        if (direction == 1)
        {
            diveMove = 10;
            dashDist = 20;
        }
        if (dashTime <= 0)
        {
            isDashing = false;
            animator.SetBool("isDashing", false);
        }
        if (isDiving == true)
        {
            animator.SetBool("isDiving", true);
        }
        if (isDashing == false)
        {
            dashTime = 0.2f;
        }
        if (isDashing == true)
        {
            dashTime -= Time.deltaTime;
        }
    }
    void FlipSprite()
    {
        transform.localScale = new Vector3(originalScale.x * direction, originalScale.y, originalScale.z);
    }
    public void SetMoveEnabled(bool enabled)
    {
        canMove = enabled;
    }
    public void OnPlayerDeath()
    {
        isDead = true;
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isJumping", false);
        animator.SetBool("isDiving", false);
        rb.linearVelocity = Vector2.zero;
    }
    public void DiveKick()
    {
        isDiving = true;
        rb.linearVelocity = new Vector2(diveMove, dive);
        StartCoroutine(DiveInvincibility());
    }
    public void Dash()
    {
        isDashing = true;
        animator.SetBool("isDashing", true);
        rb.linearVelocity = new Vector2(dashDist, 0f);
        StartCoroutine(DashInvincibility());
    }
    IEnumerator DiveInvincibility()
    {
        knockback.AddInvincibility();
        yield return new WaitUntil(() => !isDiving);
        knockback.RemoveInvincibility();
    }
    IEnumerator DashInvincibility()
    {
        knockback.AddInvincibility();
        yield return new WaitUntil(() => !isDashing);
        knockback.RemoveInvincibility();
    }
    }