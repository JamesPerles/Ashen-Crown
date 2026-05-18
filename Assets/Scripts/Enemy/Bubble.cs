using UnityEngine;

public class Bubble : MonoBehaviour
{
    [Header("Bubble Settings")]
    public float speed = 0.1f;
    public float lifetime = 5f;
    Vector2 moveDirection;
    public float floatAmplitude = 10f;
    public float floatFrequency = 10f;
    float floatTimer = 0f;
    [SerializeField] float damageAmount = 1f;
    [SerializeField] string playerTag = "Player";
    [SerializeField] string enemyTag = "Enemy";
    [SerializeField] AudioClip destroySFX;
    AudioSource audioSource;
    bool isDestroying = false;
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    public void Initialize(Vector3 targetPosition)
    {
        moveDirection = new Vector2(targetPosition.x > transform.position.x ? 1f : -1f, 0f);
    }
    void Update()
    {
        if (!isDestroying)
        {
            floatTimer += Time.deltaTime;
            float verticalOffset = Mathf.PingPong(floatTimer * floatFrequency, floatAmplitude) - floatAmplitude / 2f;
            transform.position += new Vector3(moveDirection.x * speed * Time.deltaTime, verticalOffset * Time.deltaTime, 0);
            lifetime -= Time.deltaTime;
            if (lifetime <= 0)
                DestroyBubble();
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDestroying) return;
        if (collision.CompareTag(playerTag))
        {
            if (collision.TryGetComponent(out PlayerHealth playerHealth))
                playerHealth.TakeDamage(damageAmount);
            DestroyBubble();
        }
        else if (!collision.CompareTag(playerTag) && !collision.CompareTag(enemyTag))
        {
            DestroyBubble();
        }
    }
    void DestroyBubble()
    {
        if (isDestroying) return;
        isDestroying = true;
        if (destroySFX != null)
            audioSource.PlayOneShot(destroySFX);
        if (TryGetComponent(out SpriteRenderer sprite))
            sprite.enabled = false;
        if (TryGetComponent(out Collider2D col))
            col.enabled = false;
        Destroy(gameObject, destroySFX != null ? destroySFX.length : 0.05f);
    }
}