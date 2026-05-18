using UnityEngine;
public class FallingPlatform : MonoBehaviour
{
    float fallSpeed = -10f;
    bool activate;
    Rigidbody2D rb;
    Vector3 originalPosition;
    [SerializeField] float shakeDuration = 0.8f;
    [SerializeField] float shakeMagnitude = 0.05f;
    float shakeTimer;
    bool isShaking;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalPosition = transform.position;
    }
    void Update()
    {
        if (isShaking)
        {
            shakeTimer -= Time.deltaTime;

            transform.position = originalPosition + new Vector3(
                Random.Range(-shakeMagnitude, shakeMagnitude),
                Random.Range(-shakeMagnitude, shakeMagnitude),
                0f
            );

            if (shakeTimer <= 0)
            {
                isShaking = false;
                activate = true;
                transform.position = originalPosition;
            }
        }
        if (activate)
        {
            rb.linearVelocity = new Vector2(0f, fallSpeed);
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isShaking && !activate)
        {
            originalPosition = transform.position;
            shakeTimer = shakeDuration;
            isShaking = true;
        }
    }
}