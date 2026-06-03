using UnityEngine;
public class Bat : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float speed = 3f;
    [SerializeField] float walkRange = 3f;
    [SerializeField] float Detection = 5f;
    [SerializeField] float swoopSpeed = 5f;
    [SerializeField] float swoopCooldown = 3f;
    [SerializeField] float maxSwoopDuration = 3f;
    [SerializeField] float preventFlipping = 1f; //Stops Bat infinitely flipping back and forth getting stuck until player enters range
    [Header("References")]
    [SerializeField] string playerTag = "Player";
    [SerializeField] SpriteRenderer sr;
     Transform Player;
     Vector3 startPos;
    int direction = 1;
    int lastDirection = 1;
    bool isChasing = false;
    bool isReturning = false;
    float swoopCooldownTimer = 0f;
    float swoopDuration = 0f;
    Animator animator;
    Rigidbody2D rb;

    void Start()
    {
        startPos = transform.position;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
            FindPlayer();
            if (Player == null) return;
        if (swoopCooldownTimer > 0f)
        {
          swoopCooldownTimer -= Time.deltaTime;  
        }
            float distanceToPlayer = Vector3.Distance(transform.position, Player.position);

        if (distanceToPlayer <= Detection && swoopCooldownTimer <= 0f && !isChasing && !isReturning)
        {
            isChasing = true;
            swoopDuration = 0f;
        }
        if (isChasing)
        {
            Chase(distanceToPlayer);
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
    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            Player = playerObj.transform;
    }
    void HandlePatrol()
    {
        transform.position += new Vector3(direction * speed * Time.deltaTime, 0f, 0f);

        float distanceFromStart = transform.position.x - startPos.x;

        if (Mathf.Abs(distanceFromStart) > walkRange)
        {
            if (Mathf.Sign(direction) == Mathf.Sign(distanceFromStart))
            {
                direction *= -1;
            }
        }

        UpdateFlip(direction);
    }
    void Chase(float distanceToPlayer)
    {
        HandleSwoop();
        swoopDuration += Time.deltaTime;

        if (swoopDuration >= maxSwoopDuration || distanceToPlayer > Detection + preventFlipping)
        {
            StartReturning();
        }
    }
    void HandleSwoop()
    {
        if (Player == null) return;

        Vector3 dir = (Player.position - transform.position).normalized;
        transform.position += dir * swoopSpeed * Time.deltaTime;

        int newDirection = ChangeDirection(dir);
        UpdateFlip(newDirection);
    }
    void StartReturning()
    {
        isChasing = false;
        isReturning = true;
        swoopCooldownTimer = swoopCooldown;
    }
    void ReturnToStart()
    {
        Vector3 dir = (startPos - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        int newDirection = ChangeDirection(dir);
        UpdateFlip(newDirection);

        if (Vector3.Distance(transform.position, startPos) < 0.5f)
            isReturning = false;
    }

    static int ChangeDirection(Vector3 dir)
    {
        return dir.x >= 0 ? 1 : -1;
    }

    void UpdateFlip(int newDirection)
    {
        if (newDirection != lastDirection)
        {
            sr.flipX = newDirection < 0;
            lastDirection = newDirection;
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(playerTag))
        {
            StartReturning();
        }
    }
}