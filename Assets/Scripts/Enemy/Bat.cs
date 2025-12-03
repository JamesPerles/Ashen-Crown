using UnityEngine;

public class Bat : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float speed = 3f;
    [SerializeField] float walk = 3f;
    [SerializeField] float Detection = 5f;
    [SerializeField] float swoopSpeed = 5f;
    [SerializeField] float swoopCooldown = 3f;
    [SerializeField] float maxSwoopTime = 3f;
    [SerializeField] float detectionHysteresis = 1f; // ✅ Prevents flickering at detection edge

    [Header("References")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private SpriteRenderer sr;

    private Transform Player;
    private Vector3 startPos;
    private int direction = 1;
    private int lastDirection = 1;
    private bool isChasing = false;
    private bool isReturning = false;
    private float swoopTimer = 0f;
    private float swoopDuration = 0f;

    private Animator animator;
    private Rigidbody2D rb;

    void Start()
    {
        startPos = transform.position;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        FindPlayer();
    }

    void Update()
    {
        if (Player == null || !Player.gameObject.activeInHierarchy)
        {
            FindPlayer();
            if (Player == null) return;
        }

        if (swoopTimer > 0f)
            swoopTimer -= Time.deltaTime;

        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);

        // ✅ Start chase with hysteresis to prevent flickering
        if (distanceToPlayer <= Detection && swoopTimer <= 0f && !isChasing && !isReturning)
        {
            isChasing = true;
            swoopDuration = 0f;
        }

        if (isChasing)
        {
            HandleSwoop();

            swoopDuration += Time.deltaTime;
            
            // ✅ Stop if max duration OR if player is too far (with hysteresis)
            if (swoopDuration >= maxSwoopTime || distanceToPlayer > Detection + detectionHysteresis)
            {
                isChasing = false;
                isReturning = true;
                swoopTimer = swoopCooldown;
            }
        }
        else if (isReturning)
        {
            ReturnToStart();
        }
        else
        {
            HandlePatrol();
        }
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            Player = playerObj.transform;
    }

    void HandlePatrol()
    {
        transform.position += new Vector3(direction * speed * Time.deltaTime, 0f, 0f);

        float distanceFromStart = transform.position.x - startPos.x;
        
        // ✅ Fixed: Only flip if moving away from center AND past boundary
        // This prevents rapid back-and-forth flipping
        if (Mathf.Abs(distanceFromStart) > walk)
        {
            // Only flip if we're moving in the same direction as our offset from start
            if (Mathf.Sign(direction) == Mathf.Sign(distanceFromStart))
            {
                direction *= -1;
            }
        }

        UpdateFlip(direction);
    }

    void HandleSwoop()
    {
        if (Player == null) return;

        Vector3 dir = (Player.position - transform.position).normalized;
        transform.position += dir * swoopSpeed * Time.deltaTime;

        int newDirection = dir.x >= 0 ? 1 : -1;
        UpdateFlip(newDirection);
    }

    void ReturnToStart()
    {
        Vector3 dir = (startPos - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        int newDirection = dir.x >= 0 ? 1 : -1;
        UpdateFlip(newDirection);

        if (Vector3.Distance(transform.position, startPos) < 0.5f)
            isReturning = false;
    }

    void UpdateFlip(int newDirection)
    {
        if (newDirection != lastDirection)
        {
            sr.flipX = newDirection < 0;
            lastDirection = newDirection;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(playerTag))
        {
            isChasing = false;
            isReturning = true;
            swoopTimer = swoopCooldown;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {
            isChasing = false;
            isReturning = true;
            swoopTimer = swoopCooldown;
        }
    }
}