using UnityEngine;
public class Arrow : MonoBehaviour
{
    [SerializeField] float arrowSpeed = -10f;
    [SerializeField] float arrowSpeedY = 0f;
    float arrowLifeTime = 2f;
    public Rigidbody2D rb;
    PlayerHealth playerHealth;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = new Vector2(arrowSpeed, arrowSpeedY);
    }
    void Update()
    {
        arrowLifeTime -= Time.deltaTime;
        if(arrowLifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            playerHealth = collision.GetComponent<PlayerHealth>();
            playerHealth.TakeDamage(1f);
            Destroy(gameObject);
        }
    }
}
