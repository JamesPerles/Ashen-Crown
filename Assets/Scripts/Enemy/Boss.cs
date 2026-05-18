using System.Collections;
using UnityEngine;
public class Boss : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] float activationRange = 15f;
    [Header("Movement")]
    [SerializeField] float moveSpeed = 16f;
    [SerializeField] float chargeWindupDuration = 0.6f;
    [Header("Player X Offset")]
    [SerializeField] float playerXOffset = 5f;
    [Header("Attack")]
    [SerializeField] Collider2D[] attackHitboxes;
    [Header("Jump")]
    [SerializeField] Transform[] jumpPoints;
    [SerializeField] float jumpRiseDuration = 0.5f;
    [SerializeField] float jumpHangTime = 0.6f;
    [SerializeField] float jumpSlamDuration = 0.2f;
    [SerializeField] float jumpHeight = 4f;
    [SerializeField] int jumpCount = 3;
    [Header("Spike Spawning")]
    [SerializeField] GameObject spikePrefab;
    [SerializeField] Transform[] spikeSpawnPoints;
    [Header("References")]
    [SerializeField] EnemyHP enemyHP;
    [SerializeField] Transform spawnPoint;
    [SerializeField] GameObject goal;
    Animator animator;
    Rigidbody2D rb;
    Transform player;
    Vector3 originalScale;
    bool isActive;
    bool isDead;
    int direction = 1;
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        if (enemyHP == null) enemyHP = GetComponent<EnemyHP>();
        StartCoroutine(BossLoop());
    }
    void Update()
    {
        if (isDead || player == null) return;
        if (!isActive && Vector2.Distance(transform.position, player.position) <= activationRange)
            isActive = true;
        if (enemyHP.HP <= 0) Die();
    }
    void Die()
    {
        isDead = true;
        animator.SetBool("isWalking", false);
        Instantiate(goal, spawnPoint.position, Quaternion.identity);
    }
    IEnumerator BossLoop()
    {
        yield return new WaitUntil(() => isActive);
        while (!isDead)
        {
            yield return PerformChargeAttack();
            yield return PerformJumpSequence();
        }
    }
    void FaceDirection(int dir)
    {
        direction = dir;
        transform.localScale = new Vector3(originalScale.x * direction, originalScale.y, originalScale.z);
    }
    IEnumerator PerformChargeAttack()
    {
        float distToPlayer = Mathf.Abs(player.position.x - transform.position.x);
        if (distToPlayer <= playerXOffset)
        {
            FaceDirection(player.position.x > transform.position.x ? 1 : -1);
            animator.SetTrigger("Attack");
            yield return new WaitForSeconds(0.5f);
            yield break;
        }
        float approachDir = player.position.x > transform.position.x ? 1f : -1f;
        float targetX = player.position.x - approachDir * playerXOffset;
        int chargeDirection = (int)approachDir;
        FaceDirection(chargeDirection);
        animator.SetBool("isWalking", false);
        float windupElapsed = 0f;
        Vector3 baseScale = new(originalScale.x * direction, originalScale.y, originalScale.z);
        while (windupElapsed < chargeWindupDuration)
        {
            windupElapsed += Time.deltaTime;
            float shake = Mathf.Sin(windupElapsed * 40f) * 0.04f;
            transform.localScale = new Vector3(baseScale.x + shake, baseScale.y, baseScale.z);
            yield return null;
        }
        transform.localScale = baseScale;
        animator.SetBool("isWalking", true);
        bool hasAttacked = false;
        float postAttackElapsed = 0f;
        float postAttackTravel = 0.6f;
        float maxChargeDuration = 3f;
        float chargeElapsed = 0f;
        while (true)
        {
            if (isDead) yield break;
            chargeElapsed += Time.deltaTime;
            if (chargeElapsed >= maxChargeDuration) break;
            rb.MovePosition(rb.position + Vector2.right * chargeDirection * moveSpeed * Time.deltaTime);
            if (!hasAttacked)
            {
                bool pastTarget = chargeDirection == 1
                    ? transform.position.x >= targetX
                    : transform.position.x <= targetX;
                if (pastTarget)
                {
                    animator.SetTrigger("Attack");
                    hasAttacked = true;
                }
            }
            else
            {
                postAttackElapsed += Time.deltaTime;
                if (postAttackElapsed >= postAttackTravel) break;
            }
            yield return null;
        }
        animator.SetBool("isWalking", false);
        yield return new WaitForSeconds(0.5f);
    }
    IEnumerator PerformJumpSequence()
    {
        if (jumpPoints.Length == 0) yield break;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        for (int i = 0; i < jumpCount; i++)
        {
            if (isDead) yield break;
            Transform target = jumpPoints[Random.Range(0, jumpPoints.Length)];
            Vector3 startPos = transform.position;
            Vector3 landPos = target.position;
            int jumpDir = landPos.x > startPos.x ? 1 : -1;
            FaceDirection(jumpDir);
            animator.SetBool("isJumping", true);
            Vector3 risePos = new(startPos.x, startPos.y + jumpHeight, startPos.z);
            float elapsed = 0f;
            while (elapsed < jumpRiseDuration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, risePos,
                    Mathf.SmoothStep(0, 1, elapsed / jumpRiseDuration));
                yield return null;
            }
            transform.position = risePos;
            Vector3 arcStart = risePos;
            Vector3 arcEnd = new(landPos.x, risePos.y, landPos.z);
            float arcDuration = Mathf.Abs(arcEnd.x - arcStart.x) / (moveSpeed * 1.2f);
            arcDuration = Mathf.Clamp(arcDuration, 0.2f, 0.8f);
            elapsed = 0f;
            while (elapsed < arcDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / arcDuration);
                transform.position = Vector3.Lerp(arcStart, arcEnd, t);
                yield return null;
            }
            transform.position = arcEnd;
            yield return new WaitForSeconds(jumpHangTime);
            elapsed = 0f;
            while (elapsed < jumpSlamDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / jumpSlamDuration;
                Vector3 next = Vector3.Lerp(arcEnd, landPos, t * t);
                if (next.y <= landPos.y + 0.5f && rb.bodyType == RigidbodyType2D.Kinematic)
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.gravityScale = 1f;
                    rb.linearVelocity = Vector2.zero;
                    break;
                }
                transform.position = next;
                yield return null;
            }
            yield return new WaitForSeconds(0.15f);
            animator.SetBool("isJumping", false);
            yield return new WaitForSeconds(0.3f);
            bool isFinalJump = i == jumpCount - 1;
            if (!isFinalJump)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
            }
        }
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        SpawnSpikes();
        yield return new WaitForSeconds(0.5f);
    }
    void SpawnSpikes()
    {
        if (spikePrefab == null || spikeSpawnPoints.Length == 0) return;
        int spawnAmount = Mathf.Min(3, spikeSpawnPoints.Length);
        bool[] used = new bool[spikeSpawnPoints.Length];
        for (int i = 0; i < spawnAmount; i++)
        {
            int randomIndex;
            do { randomIndex = Random.Range(0, spikeSpawnPoints.Length); } while (used[randomIndex]);
            used[randomIndex] = true;
            Transform point = spikeSpawnPoints[randomIndex];
            Instantiate(spikePrefab, point.position, point.rotation);
        }
    }
    public void EnableHitbox(int index)
    {
        if (index >= 0 && index < attackHitboxes.Length)
            attackHitboxes[index].enabled = true;
    }
    public void DisableHitboxes()
    {
        foreach (Collider2D hitbox in attackHitboxes)
            hitbox.enabled = false;
    }
}