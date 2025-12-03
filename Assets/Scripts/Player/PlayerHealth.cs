using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] int HP = 3;
    public float currentHP;

    private Image healthFill; // 🔹 Found at runtime by name

    [Header("Health Flash Settings")]
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.2f;
    private float flashTimer = 0f;

    private Animator animator;

    [Header("Damage Popup")]
    public GameObject damagePopPrefab;
    [SerializeField] private string damageCanvasTag = "WorldCan"; // Tag for the canvas

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource; // ✅ Assign this in Inspector
    [SerializeField] private AudioClip damageSFX;

    [SerializeField] private float deathDelay = 2f;

    private PlayerKnockback playerKnockback;
    private PlayerAttack playerAttack;
    private PlayerController playerController;
    private TransformedMoveset transformedMoveset; // ✅ reference added
    private Canvas damageCanvas;
    private bool hasDied = false;

    void Start()
    {
        // Only initialize HP if it hasn't been set yet (for transformation support)
        if (currentHP == 0)
        {
            currentHP = HP;
        }
        
        animator = GetComponent<Animator>();
        playerKnockback = GetComponent<PlayerKnockback>();
        playerAttack = GetComponent<PlayerAttack>();
        playerController = GetComponent<PlayerController>();
        transformedMoveset = GetComponent<TransformedMoveset>(); // ✅ Get reference

        // ✅ AudioSource should now be assigned in Inspector
        if (audioSource == null)
        {
            Debug.LogWarning("⚠️ No AudioSource assigned in PlayerHealth! Please assign one in the Inspector.");
        }

        // Find the health bar UI Image by name
        GameObject healthObj = GameObject.Find("Health");
        if (healthObj != null)
        {
            healthFill = healthObj.GetComponent<Image>();
        }
        else
        {
            Debug.LogWarning("⚠️ Health UI Image named 'Health' not found in scene!");
        }

        // Find the damage canvas by tag
        GameObject canvasObj = GameObject.FindGameObjectWithTag(damageCanvasTag);
        if (canvasObj != null)
        {
            damageCanvas = canvasObj.GetComponent<Canvas>();
        }
    }

    [System.Obsolete]
    public void TakeDamage(float damageAmount)
    {
        if (playerKnockback != null && playerKnockback.isInvincible) return;

        currentHP -= damageAmount;
        currentHP = Mathf.Clamp(currentHP, 0, HP);
        flashTimer = flashDuration;

        if (audioSource != null && damageSFX != null)
            audioSource.PlayOneShot(damageSFX);

        SpawnDamagePopup(damageAmount);

        PlayerHeadUI headUI = FindObjectOfType<PlayerHeadUI>();
        if (headUI != null)
            headUI.FlashHead();
    }

    [System.Obsolete]
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.otherCollider.gameObject != gameObject) return;
        if (!other.gameObject.CompareTag("Enemy")) return;

        EnemyDamage enemyDamage = other.gameObject.GetComponent<EnemyDamage>()
                                   ?? other.gameObject.GetComponentInParent<EnemyDamage>();

        float dmg = (enemyDamage != null) ? enemyDamage.damage : 1f;
        TakeDamage(dmg);
    }

    [System.Obsolete]
    void OnTriggerEnter2D(Collider2D collision)
    {
        Collider2D[] playerColliders = GetComponents<Collider2D>();
        bool isPlayerCollider = false;

        foreach (Collider2D col in playerColliders)
        {
            if (col == collision)
            {
                isPlayerCollider = true;
                break;
            }
        }

        if (!isPlayerCollider) return;
        if (!collision.gameObject.CompareTag("Enemy")) return;

        EnemyDamage enemyDamage = collision.gameObject.GetComponent<EnemyDamage>()
                                   ?? collision.gameObject.GetComponentInParent<EnemyDamage>();

        float dmg = (enemyDamage != null) ? enemyDamage.damage : 1f;
        TakeDamage(dmg);
    }

    void Update()
    {
        // 🔹 Update health bar
        if (healthFill != null)
        {
            float targetFill = currentHP / (float)HP;
            healthFill.fillAmount = targetFill;

            if (flashTimer > 0)
            {
                healthFill.color = flashColor;
                flashTimer -= Time.deltaTime;
            }
            else
            {
                healthFill.color = normalColor;
            }
        }

        // 🔹 Handle death
        if (currentHP <= 0 && !hasDied)
        {
            hasDied = true;
            animator.SetTrigger("Dead");

            if (playerController != null)
                playerController.OnPlayerDeath();

            if (playerAttack != null)
                playerAttack.enabled = false;

            if (playerKnockback != null)
                playerKnockback.enabled = false;

            if (transformedMoveset != null)
                transformedMoveset.enabled = false; // ✅ Disable TransformedMoveset on death

            Invoke(nameof(LoadGameOver), deathDelay);
        }
    }

    void LoadGameOver()
    {
        SceneManager.LoadScene("GameOver");
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
            TMPro.TextMeshProUGUI text = popup.GetComponent<TMPro.TextMeshProUGUI>();
            if (text != null)
                text.text = "-" + damageAmount.ToString();
        }
    }
}