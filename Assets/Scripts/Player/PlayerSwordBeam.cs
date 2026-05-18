using UnityEngine;
public class PlayerSwordBeam : MonoBehaviour
{
    public int damage = 1;
    public string enemyTag = "Enemy";
    public bool destroyOnHit = true;
    float lifetime = 5f;
    void Update()
    {
        lifetime -= Time.deltaTime;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(enemyTag))
        {
            EnemyHP enemy = collision.GetComponent<EnemyHP>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
         if(lifetime == 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
