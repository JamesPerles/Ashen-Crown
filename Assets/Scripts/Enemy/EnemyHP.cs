using UnityEngine;
public class EnemyHP : MonoBehaviour
{
    [SerializeField] public float HP = 3f;
    public GameObject orbPrefab;
    public GameObject damagePopPrefab; 
    [SerializeField]  AudioSource audioSource; 
    [SerializeField]  AudioClip damageSFX;
    [SerializeField]  AudioClip deathSFX; 
    Animator animator;
    bool isDying = false;
    void Start()
    {
        animator = GetComponent<Animator>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
    }
    public void TakeDamage(float damageAmount)
    {
        if (isDying) return;
        HP -= damageAmount;
        if (DamagePopUpSpawner.Instance != null)
        DamagePopUpSpawner.Instance.ShowDamage(transform.position, damageAmount);
        if (audioSource != null && damageSFX != null)
            audioSource.PlayOneShot(damageSFX);
        if (HP <= 0f)
            Die();
    }
    void Die()
    {
        isDying = true;
        if (audioSource != null && deathSFX != null)
            audioSource.PlayOneShot(deathSFX);
        SpawnOrbs(transform.position, 3);
        animator.SetTrigger("Dead");
        var shadowScript = GetComponent<Shadow>();
        if (shadowScript != null) shadowScript.enabled = false;
        var frogScript = GetComponent<Frog>();
        if (frogScript != null) frogScript.enabled = false;
        var batScript = GetComponent<Bat>();
        if (batScript != null) batScript.enabled = false;
        var collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; 
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        gameObject.tag = "Untagged";
    }
    public void FinishDeath()
    {
        Destroy(gameObject);
    }
    public void SpawnOrbs(Vector3 position, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject orb = Instantiate(orbPrefab, position, Quaternion.identity);
            orb.transform.SetParent(null);
            orb.transform.position = position + new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(0f, 0.5f),
                0f
            );
            orb.GetComponent<TransformOrb>().Activate();
            orb.SetActive(true);
        }
    }
}
