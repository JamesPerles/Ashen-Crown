using UnityEngine;
public class MovingPlatform : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float returnSpeed = 5f;
    [SerializeField] float maxDistanceX = 5f;
    [SerializeField] float maxDistanceY = 0f;
    Vector3 endPosition;
    Vector3 startPosition;
    bool returning;
    bool moving = true;
    Rigidbody2D rb;
    Vector2 lastPosition;
    Vector2 platformVelocity;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        startPosition = transform.position;
        endPosition = startPosition + new Vector3(maxDistanceX, maxDistanceY, 0f);
        lastPosition = rb.position;
    }
    void FixedUpdate()
    {
        Vector3 target = returning ? startPosition : endPosition;
        float speed = returning ? returnSpeed : moveSpeed;
        Vector2 newPos = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
        platformVelocity = (rb.position - lastPosition) / Time.fixedDeltaTime;
        lastPosition = rb.position;
        if (Vector3.Distance(transform.position, endPosition) < 0.1f)
        {
            returning = true;
            moving = false;
        }
        if (Vector3.Distance(transform.position, startPosition) < 0.1f)
        {
            moving = true;
            returning = false;
        }
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Enemy"))
        {
            Physics2D.IgnoreCollision(
                col.collider,
                GetComponent<Collider2D>()
            );
        }
    }
    void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Enemy")) return;
        PlayerController player = col.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.rb.position += platformVelocity * Time.fixedDeltaTime;
        }
    }
}