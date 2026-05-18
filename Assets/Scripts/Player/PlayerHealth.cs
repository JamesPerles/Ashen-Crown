using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Flash Settings")]
    [SerializeField] int HP = 3;
    public float currentHP;
    [SerializeField] Color normalColor = Color.green;
    [SerializeField] Color flashColor = Color.red;
    [SerializeField] float flashDuration = 0.2f;
    [SerializeField] float flashTimer = 0f;
    [Header("Damage Popup")]
    public GameObject damagePopPrefab;
    [Header("Sound Effects")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip damageSFX;
    [Header("Other")]
    [SerializeField] float deathDelay = 1.5f;
    PlayerKnockback playerKnockback;
    PlayerAttack playerAttack;
    PlayerController playerController;
    TransformedMoveset transformedMoveset;
    Image healthFill;
    Animator animator;
    bool hasDied = false;
    [SerializeField] Collider2D playerCollider;
    void Start()
    {
        if (currentHP == 0)
        {
            currentHP = HP;
        }
        animator = GetComponent<Animator>();
        playerKnockback = GetComponent<PlayerKnockback>();
        playerAttack = GetComponent<PlayerAttack>();
        playerController = GetComponent<PlayerController>();
        transformedMoveset = GetComponent<TransformedMoveset>();
        GameObject healthObj = GameObject.Find("Health");
        if (healthObj != null)
        {
            healthFill = healthObj.GetComponent<Image>();
        }
    }
    public void TakeDamage(float damageAmount)
    {
        if (playerKnockback != null && playerKnockback.IsInvincible) return;
        currentHP -= damageAmount;
        currentHP = Mathf.Clamp(currentHP, 0, HP);
        flashTimer = flashDuration;
        if (audioSource != null && damageSFX != null)
            audioSource.PlayOneShot(damageSFX);
            if (DamagePopUpSpawner.Instance != null)
        DamagePopUpSpawner.Instance.ShowDamage(transform.position, damageAmount);
        PlayerHeadUI headUI = FindFirstObjectByType<PlayerHeadUI>();
        if (headUI != null)
            headUI.FlashHead();
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.otherCollider.gameObject != gameObject) return;
        if (!other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Enemy2")) return;
        EnemyDamage enemyDamage = other.gameObject.GetComponent<EnemyDamage>()
        ?? other.gameObject.GetComponentInParent<EnemyDamage>();
        float dmg = (enemyDamage != null) ? enemyDamage.damage : 1f;
        TakeDamage(dmg);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
    if (!other.CompareTag("Enemy")|| other.gameObject.CompareTag("Enemy2") ) return;
    if (!playerCollider.IsTouching(other)) return;
        EnemyDamage enemyDamage = other.gameObject.GetComponent<EnemyDamage>()
        ?? other.gameObject.GetComponentInParent<EnemyDamage>();
        float dmg = (enemyDamage != null) ? enemyDamage.damage : 1f;
        TakeDamage(dmg);
    }
    void Update()
    {
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
                transformedMoveset.enabled = false;
            Invoke(nameof(LoadGameOver), deathDelay);
        }
    }
   void LoadGameOver()
{
    SceneController.Instance.GoToGameOver();
}
    }
