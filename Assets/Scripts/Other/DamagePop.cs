using UnityEngine;
using TMPro;

public class DamagePop : MonoBehaviour
{
    [SerializeField] float speed = 3f;
    [SerializeField] float lifetime = 1f;
    
    private TextMeshProUGUI textMesh;
    private Color textColor;

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
        
        // Display as negative number
        if (damageAmount % 1 == 0)
            textMesh.text = $"-{damageAmount:F0}"; // No decimal if whole number
        else
            textMesh.text = $"-{damageAmount:F1}"; // 1 decimal place
        
        textColor = textMesh.color;
    }

    void Update()
    {
        if (textMesh == null) return;
        
        // Move upward
        transform.position += new Vector3(0, speed * Time.deltaTime, 0);
        
        // Fade out
        textColor.a -= Time.deltaTime / lifetime;
        textMesh.color = textColor;
        
        // Destroy when fully transparent
        if (textColor.a <= 0)
            Destroy(gameObject);
    }
}