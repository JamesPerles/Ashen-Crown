using UnityEngine;
public class FireTrap : MonoBehaviour
{
    [SerializeField] Collider2D[] damageHitbox;
    float damageDuration = 5f;
 float activationTime = 3f;
 Animator animator;
 void Awake()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        activationTime -= Time.deltaTime;
        if(activationTime <= 0)
        {
            animator.SetBool("Fire", true);
        }
    }
    public void EnableHitbox(int index)
    {
        if (index < 0 || index >= damageHitbox.Length) return;
        damageHitbox[index].enabled = true;
        Invoke(nameof(DisableHitboxes), damageDuration);
    }
    public void DisableHitboxes()
    {
        foreach (var hitbox in damageHitbox)
        {
            if (hitbox != null)
                hitbox.enabled = false;
                activationTime = 5f;
                animator.SetBool("Fire", false);
        }
    }
}
