using UnityEngine;
using System.Collections;
public class TransformedMoveset : MonoBehaviour
{
    [Header("Moveset Settings")]
    [SerializeField] GameObject shieldProjectile;
    [SerializeField] GameObject slashProjectile;
    [SerializeField] Transform projectileSpawnPoint;
    [SerializeField] float projectileSpeed = 12f;
    [SerializeField] float slashLifetime = 2f;
    [SerializeField] GameObject blockCollider;
    [Header("Sound Effects")]
    [SerializeField] AudioClip slashSpawnSFX;
    [SerializeField] AudioClip shieldThrowSFX;
    [SerializeField] AudioClip blockActivateSFX;
    public AudioSource audioSource;
    bool isBlocking = false;
    bool isCharging = false;
    bool isThrowingShield = false;
    public Animator animator;
    public Rigidbody2D rb;
    PlayerController playerController;
    void Awake()
    {
        playerController = GetComponent<PlayerController>();
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
    }
    void HandleInput()
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
    void StartThrowShield()
    {
        if (isThrowingShield) return;
        isThrowingShield = true;
        if (playerController != null) playerController.SetMoveEnabled(false);
        animator.SetBool("ThrowingShield", true);
    }
    public void SpawnShield()
    {
        if (!isThrowingShield) return;
        if (shieldThrowSFX != null)
            audioSource.PlayOneShot(shieldThrowSFX);
        float facingDir = transform.localScale.x > 0 ? 1f : -1f;
        Vector3 spawnPos = projectileSpawnPoint.position;
        Quaternion spawnRot = facingDir < 0 ? Quaternion.Euler(0, 180f, 0) : Quaternion.identity;
        GameObject shield = Instantiate(shieldProjectile, spawnPos, spawnRot);
        ShieldProjectile shieldScript = shield.GetComponent<ShieldProjectile>();
        if (shieldScript != null)
        {
            shieldScript.Init(transform, projectileSpeed, OnShieldReturned, facingDir);
        }
        else
        {
            OnShieldReturned();
        }
    }
    void OnShieldReturned()
    {
        if (!isThrowingShield) return;
        isThrowingShield = false;
        if (playerController != null) playerController.SetMoveEnabled(true);
        animator.SetBool("ThrowingShield", false);
    }
    void StartBlock()
    {
        if (isBlocking) return;
        isBlocking = true;
        if (playerController != null) playerController.SetMoveEnabled(false);
        if (blockActivateSFX != null)
            audioSource.PlayOneShot(blockActivateSFX);
        animator.SetBool("Blocking", true);
        if (blockCollider != null)
            blockCollider.SetActive(true);
    }
    void StopBlock()
    {
        isBlocking = false;
        if (playerController != null) playerController.SetMoveEnabled(true);
        animator.SetBool("Blocking", false);
        if (blockCollider != null)
            blockCollider.SetActive(false);
    }
    void StartChargeAttack()
    {
        if (isCharging) return;
        isCharging = true;
        if (playerController != null) playerController.SetMoveEnabled(false);
        animator.SetBool("Charging", true);
        rb.linearVelocity = Vector2.zero;
    }
    void ReleaseChargeAttack()
    {
        if (!isCharging) return;
        isCharging = false;
        animator.SetBool("Charging", false);
        animator.SetTrigger("Slash");
        if (playerController != null) playerController.SetMoveEnabled(true);
    }
    public void SpawnSlash()
    {
        if (slashSpawnSFX != null)
            audioSource.PlayOneShot(slashSpawnSFX);
        float facingDir = transform.localScale.x > 0 ? 1f : -1f;
        Vector3 spawnPos = projectileSpawnPoint.position;
        Quaternion spawnRot = facingDir < 0 ? Quaternion.Euler(0, 180f, 0) : Quaternion.identity;
        GameObject slash = Instantiate(slashProjectile, spawnPos, spawnRot);
        Rigidbody2D rbSlash = slash.GetComponent<Rigidbody2D>();
        if (rbSlash != null)
            rbSlash.linearVelocity = new Vector2(facingDir * projectileSpeed, 0f);
        Destroy(slash, slashLifetime);
    }
}