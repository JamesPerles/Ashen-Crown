using UnityEngine;
using System;
using System.Collections.Generic;
public class ShieldProjectile : MonoBehaviour
{
    public Transform player;
    public Rigidbody2D rb;
    float speed;
    public int damage = 1;
    public string enemyTag = "Enemy";
    public bool destroyOnHit = false;
    Action onReturnComplete;
    Vector2 launchDirection;
    bool returning = false;
    float maxDistance = 8f;
    Vector2 startPos;
    bool isDestroyed = false;
    HashSet<GameObject> hitEnemies = new HashSet<GameObject>();
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
        if (isDestroyed) return;
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
                ReturnAndDestroy();
        }
    }
    void StartReturn()
    {
        returning = true;
        hitEnemies.Clear();
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDestroyed) return;
        GameObject root = collision.transform.root.gameObject;
        if (hitEnemies.Contains(root)) return;
        if (!collision.CompareTag(enemyTag) && !root.CompareTag(enemyTag)) return;
        hitEnemies.Add(root);
        EnemyHP enemy = root.GetComponent<EnemyHP>() ?? collision.GetComponent<EnemyHP>();
        if (enemy != null)
            enemy.TakeDamage(damage);
        if (destroyOnHit)
            ReturnAndDestroy();
    }
    void ReturnAndDestroy()
    {
        if (isDestroyed) return;
        isDestroyed = true;
        onReturnComplete?.Invoke();
        Destroy(gameObject);
    }
    void OnDestroy()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            onReturnComplete?.Invoke();
        }
    }
}