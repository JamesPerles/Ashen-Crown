using UnityEngine;
public class FallingTrap : MonoBehaviour
{
    public float fallSpeed = 10f;
    public float returnSpeed = 3f;
    public float coolDown = 3f;
    public float maxFallDistance = 3f;
    Vector3 startPosition;
    bool activate;
    bool returning;
    Rigidbody2D rb;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        startPosition = transform.position;
    }
    void FixedUpdate()
    {
        if (activate && !returning)
        {
            if (transform.position.y <= startPosition.y - maxFallDistance)
            {
                rb.linearVelocity = Vector2.zero;
                Invoke(nameof(ReturnTrap), coolDown);
                activate = false;
            }
            else
            {
                rb.linearVelocity = new Vector2(0f, -fallSpeed);
            }
        }
        if (returning)
        {
            rb.linearVelocity = new Vector2(0f, returnSpeed);
            if (Vector2.Distance(transform.position, startPosition) < 0.1f)
            {
                transform.position = startPosition;
                rb.linearVelocity = Vector2.zero;
                returning = false;
                activate = false;
            }
        }
    }
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") && !activate && !returning)
            activate = true;
    }
    void ReturnTrap()
    {
        returning = true;
    }
}