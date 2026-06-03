using UnityEngine;
using TMPro;
public class DamagePop : MonoBehaviour
{
    [SerializeField] float speed = 3f;
    [SerializeField] float lifetime = 1f;
    TextMeshProUGUI textMesh;
    Color textColor;
    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }
    void Start()
    {
        if (textMesh != null)
        {
            textColor = textMesh.color;
        }
        Destroy(gameObject, lifetime);
    }
    public void Setup(float damageAmount)
    {
        if (textMesh == null) return;
        
        if (damageAmount % 1 == 0)
            textMesh.text = $"-{damageAmount:F0}"; 
        else
            textMesh.text = $"-{damageAmount:F1}"; 

        textColor = textMesh.color;
    }
    void Update()
    {
        if (textMesh == null) return;
        transform.position += new Vector3(0, speed * Time.deltaTime, 0);
        textColor.a -= Time.deltaTime / lifetime;
        textMesh.color = textColor;
        if (textColor.a <= 0)
            Destroy(gameObject);
    }
}