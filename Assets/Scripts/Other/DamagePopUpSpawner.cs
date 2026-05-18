using UnityEngine;
using TMPro;

public class DamagePopUpSpawner : MonoBehaviour
{
    public static DamagePopUpSpawner Instance;
    
    [SerializeField] GameObject damagePopPrefab;
    Canvas damageCanvas;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    void Start()
    {
        GameObject canvasObj = GameObject.FindGameObjectWithTag("WorldCan");
        if (canvasObj != null)
        damageCanvas = canvasObj.GetComponent<Canvas>();
    }
    public void ShowDamage(Vector3 position, float damageAmount)
{
    if (damagePopPrefab == null || damageCanvas == null) return;
    Vector3 popupPos = position + new Vector3(0, 0.5f, 0);
    GameObject popup = Instantiate(damagePopPrefab, popupPos, Quaternion.identity, damageCanvas.transform);
    popup.GetComponent<RectTransform>().position = popupPos;
    popup.GetComponent<DamagePop>().Setup(damageAmount);
}
}