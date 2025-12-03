using UnityEngine;
using TMPro;

public class DamagePopUpManager : MonoBehaviour
{
    public static DamagePopUpManager Instance;
    
    [SerializeField] GameObject damagePopPrefab;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowDamage(Vector3 position, float damageAmount)
    {
        if (damagePopPrefab != null)
        {
            Vector3 popupPos = position + new Vector3(0, 0.5f, 0);
            GameObject popup = Instantiate(damagePopPrefab, popupPos, Quaternion.identity);
            popup.GetComponent<DamagePop>().Setup(damageAmount);
        }
    }
}