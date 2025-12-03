using UnityEngine;
using System;

public class ShieldProjectile : MonoBehaviour
{
    public Transform player;
    public Rigidbody2D rb;
    private float speed;
    private Action onReturnComplete;

    private Vector2 launchDirection;
    private bool returning = false;
    private float maxDistance = 8f;
    private Vector2 startPos;

    [Header("Damage Settings")]
    public int damage = 1; // How much damage the shield does
    public string enemyTag = "Enemy"; // Tag used for enemies
    public bool destroyOnHit = false; // Whether to destroy on hitting an enemy

    // Initialize with direction and speed
    public void Init(Transform playerTransform, float projectileSpeed, Action onReturn, float facingDir)
    {
        player = playerTransform;
        speed = projectileSpeed;
        onReturnComplete = onReturn;
        rb = GetComponent<Rigidbody2D>();

        startPos = transform.position;
        launchDirection = new Vector2(facingDir, 0).normalized;
        rb.linearVelocity = launchDirection * speed;
    }

    void Update()
    {
        if (!returning)
        {
            if (Vector2.Distance(startPos, transform.position) >= maxDistance)
                StartReturn();
        }
        else
        {
            Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * speed;

            if (Vector2.Distance(transform.position, player.position) < 1f)
            {
                onReturnComplete?.Invoke();
                Destroy(gameObject);
            }
        }
    }

    void StartReturn()
    {
        returning = true;
    }

    // Detect collisions with enemies (uses tag instead of layer)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(enemyTag))
        {
            var enemy = collision.GetComponent<EnemyHP>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            if (destroyOnHit)
            {
                onReturnComplete?.Invoke();
                Destroy(gameObject);
            }
        }
    }
}
