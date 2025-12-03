using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] float speed = 3f;
    [SerializeField] float walk = 3f;
    [SerializeField] float Detection = 5f;
    [SerializeField] float jumpHeight = 3f;
    [SerializeField] float jumpCooldown;
    public Transform Player;
    private Vector3 startPos;
    private int direction = 1;
    private bool isChasing = false;
    private float jumpTimer = 0f;
    public SpriteRenderer sr;
    Rigidbody2D rb;
    void Start()
    {//keeps track of start position
        startPos = transform.position;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        //detection
        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
        if (distanceToPlayer <= Detection)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }
        if (isChasing)
        { //movement
            Vector3 dir = (Player.position - transform.position).normalized;
            Vector2 jump = new Vector2(dir.x * speed, jumpHeight);
            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

        }
        else
        {
            //turns around
            if (Mathf.Abs(transform.position.x - startPos.x) > walk)
            {
                direction *= -1;
            }
        }
        jumpTimer -= Time.deltaTime;
        if (jumpTimer <= 0f)
        {
            if (isChasing)
            {
                Vector2 dir = (Player.position - transform.position).normalized;
                rb.linearVelocity = new Vector2(dir.x * speed, rb.linearVelocity.y);
            }
            else
            {
                Vector2 jump = new Vector2(direction * speed, jumpHeight);
                rb.linearVelocity = new Vector2(direction * jump.x, jump.y);
            }
            jumpTimer = jumpCooldown;
        }
        }
        
        
}
