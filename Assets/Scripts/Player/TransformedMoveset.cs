using UnityEngine;
using System.Collections;

public class TransformedMoveset : MonoBehaviour
{
    [Header("General Settings")]
    public Animator animator;
    public Rigidbody2D rb;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject shieldProjectile;
    [SerializeField] private GameObject slashProjectile;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float slashLifetime = 2f;

    [Header("Block Settings")]
    [SerializeField] private GameObject blockCollider;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip slashSpawnSFX;
    [SerializeField] private AudioClip shieldThrowSFX;
    [SerializeField] private AudioClip blockActivateSFX;

    public AudioSource audioSource;
    private bool isBlocking = false;
    private bool isCharging = false;
    private bool isThrowingShield = false;

    private PlayerController playerController;
    private float lastFacingDir = 1f; // -1 = left, 1 = right

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();

        // Ensure AudioSource exists
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = null;
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        HandleInput();
        UpdateFacingDirection();
    }

    // --- Handle Player Input ---
    private void HandleInput()
    {
        bool up = Input.GetKey(KeyCode.UpArrow);
        bool down = Input.GetKey(KeyCode.DownArrow);
        bool left = Input.GetKey(KeyCode.LeftArrow);
        bool right = Input.GetKey(KeyCode.RightArrow);
        bool directional = up || down || left || right;

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (directional)
            {
                if (up) StartChargeAttack();
                else if (down) StartBlock();
                else if (left || right) StartThrowShield();
            }
        }

        if (Input.GetKeyUp(KeyCode.X))
        {
            if (isCharging) ReleaseChargeAttack();
            if (isBlocking) StopBlock();
        }
    }

    // --- Track facing direction ---
    private void UpdateFacingDirection()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
            lastFacingDir = -1f;
        else if (Input.GetKey(KeyCode.RightArrow))
            lastFacingDir = 1f;
    }

    // ------------------ SPECIAL MOVES ------------------

    // --- SHIELD THROW ---
    private void StartThrowShield()
    {
        if (isThrowingShield) return;

        isThrowingShield = true;
        if (playerController != null) playerController.SetMovementEnabled(false);
        animator.SetBool("ThrowingShield", true);
    }

    // Called from animation event
    public void SpawnShield()
    {
        if (!isThrowingShield) return;

        // Play SFX
        if (shieldThrowSFX != null)
            audioSource.PlayOneShot(shieldThrowSFX);

        float facingDir = lastFacingDir;
        Vector3 spawnPos = transform.position + new Vector3(
            projectileSpawnPoint.localPosition.x * facingDir,
            projectileSpawnPoint.localPosition.y,
            projectileSpawnPoint.localPosition.z
        );

        Quaternion spawnRot = facingDir < 0 ? Quaternion.Euler(0, 180f, 0) : Quaternion.identity;

        GameObject shield = Instantiate(shieldProjectile, spawnPos, spawnRot);

        // Initialize shield behavior
        ShieldProjectile shieldScript = shield.GetComponent<ShieldProjectile>();
        if (shieldScript != null)
        {
            shieldScript.Init(transform, projectileSpeed, () =>
            {
                isThrowingShield = false;
                if (playerController != null) playerController.SetMovementEnabled(true);
                animator.SetBool("ThrowingShield", false);
            }, facingDir);
        }
    }

    // --- BLOCK ---
    private void StartBlock()
    {
        if (isBlocking) return;

        isBlocking = true;
        if (playerController != null) playerController.SetMovementEnabled(false);

        // Play SFX
        if (blockActivateSFX != null)
            audioSource.PlayOneShot(blockActivateSFX);

        animator.SetBool("Blocking", true);
        if (blockCollider != null)
            blockCollider.SetActive(true);
    }

    private void StopBlock()
    {
        isBlocking = false;
        if (playerController != null) playerController.SetMovementEnabled(true);

        animator.SetBool("Blocking", false);
        if (blockCollider != null)
            blockCollider.SetActive(false);
    }

    // --- CHARGE ATTACK ---
    private void StartChargeAttack()
    {
        if (isCharging) return;

        isCharging = true;
        if (playerController != null) playerController.SetMovementEnabled(false);
        animator.SetBool("Charging", true);
        rb.linearVelocity = Vector2.zero;
    }

    private void ReleaseChargeAttack()
    {
        if (!isCharging) return;

        isCharging = false;
        animator.SetBool("Charging", false);
        animator.SetTrigger("Slash");
        if (playerController != null) playerController.SetMovementEnabled(true);
    }

    // --- SLASH SPAWN (animation event) ---
    public void SpawnSlash()
    {
        // Play SFX
        if (slashSpawnSFX != null)
            audioSource.PlayOneShot(slashSpawnSFX);

        float facingDir = lastFacingDir;
        Vector3 spawnPos = transform.position + new Vector3(
            projectileSpawnPoint.localPosition.x * facingDir,
            projectileSpawnPoint.localPosition.y,
            projectileSpawnPoint.localPosition.z
        );

        Quaternion spawnRot = facingDir < 0 ? Quaternion.Euler(0, 180f, 0) : Quaternion.identity;

        GameObject slash = Instantiate(slashProjectile, spawnPos, spawnRot);
        Rigidbody2D rbSlash = slash.GetComponent<Rigidbody2D>();

        if (rbSlash != null)
            rbSlash.linearVelocity = new Vector2(facingDir * projectileSpeed, 0f);

        Destroy(slash, slashLifetime);
    }
}
