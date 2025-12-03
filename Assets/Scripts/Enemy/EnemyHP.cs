using UnityEngine;

public class EnemyHP : MonoBehaviour
{
    [SerializeField] float HP = 3f;
    public GameObject orbPrefab;
    [SerializeField] private string playerTag = "Player"; // Tag for the player

    [Header("Damage Popup")]
    public GameObject damagePopPrefab; // Assign your prefab in Inspector
    [SerializeField] private string damageCanvasTag = "WorldCan"; // Tag for the canvas

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource; // Assign in Inspector
    [SerializeField] private AudioClip damageSFX;
    [SerializeField] private AudioClip deathSFX; // Sound when enemy dies

    private Animator animator;
    private Canvas damageCanvas;
    private Transform playerTransform;
    private bool isDying = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (audioSource == null)
            Debug.LogWarning($"⚠️ No AudioSource assigned on {gameObject.name}! Please assign one in the Inspector.");

        // Find damage canvas
        GameObject canvasObj = GameObject.FindGameObjectWithTag(damageCanvasTag);
        if (canvasObj != null)
            damageCanvas = canvasObj.GetComponent<Canvas>();

        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            playerTransform = playerObj.transform;
    }

    [System.Obsolete]
    public void TakeDamage(float damageAmount)
    {
        if (isDying) return;

        HP -= damageAmount;

        // Play damage SFX
        if (audioSource != null && damageSFX != null)
            audioSource.PlayOneShot(damageSFX);

        // Spawn damage popup
        SpawnDamagePopup(damageAmount);

        if (HP <= 0f)
            Die();
    }

    [System.Obsolete]
    void Die()
    {
        isDying = true;

        // Play death sound
        if (audioSource != null && deathSFX != null)
            audioSource.PlayOneShot(deathSFX);

        // Spawn orbs
        SpawnOrbs(transform.position, 3);

        // Trigger death animation
        animator.SetTrigger("Dead");

        // ✅ Fully disable all enemy behavior scripts
        var shadowScript = GetComponent<Shadow>();
        if (shadowScript != null) shadowScript.enabled = false;

        var frogScript = GetComponent<Frog>();
        if (frogScript != null) frogScript.enabled = false;

        var batScript = GetComponent<Bat>();
        if (batScript != null) batScript.enabled = false;

        // Disable collider and Rigidbody2D to freeze physics
        var collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Stop movement
            rb.isKinematic = true;      // Freeze physics
        }

        // Remove Enemy tag
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

            // Send orb toward player
            orb.GetComponent<TransformOrb>().Activate(playerTransform);
            orb.SetActive(true);
        }
    }

    private void SpawnDamagePopup(float damageAmount)
    {
        if (damagePopPrefab == null || damageCanvas == null) return;

        GameObject popup = Instantiate(damagePopPrefab, damageCanvas.transform);
        popup.transform.position = transform.position + new Vector3(0, 1f, 0);

        DamagePop damagePop = popup.GetComponent<DamagePop>();
        if (damagePop != null)
        {
            damagePop.Setup(damageAmount);
        }
        else
        {
            TMPro.TextMeshProUGUI text = popup.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (text != null)
                text.text = "-" + Mathf.RoundToInt(damageAmount).ToString();
        }

        Destroy(popup, 1f);
    }
}
