using UnityEngine;
using UnityEngine.UI;

public class PlayerHeadUI : MonoBehaviour
{
    [Header("References")]
    public Image headImage;           // The small head UI image
    public PlayerHealth playerHealth; // Reference to player health

    [Header("Head Sprites")]
    public Sprite headFullHP;         // 4 HP
    public Sprite headThreeHP;        // 3 HP
    public Sprite headTwoHP;          // 2 HP
    public Sprite headOneHP;          // 1 HP
    public Sprite headZeroHP;         // 0 HP

    [Header("Flash Settings")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.2f;
    private float flashTimer = 0f;

    [Header("Dynamic Tracking")]
    [SerializeField] private string playerTag = "Player"; // ✅ Tag to find player

    private Sprite originalSprite;

    void Start()
    {
        if (headImage != null)
            originalSprite = headImage.sprite;

        // ✅ Find player at start
        FindPlayer();
    }

    void Update()
    {
        // ✅ Check if player is still valid, find new one if not
        if (playerHealth == null || !playerHealth.gameObject.activeInHierarchy)
        {
            FindPlayer();
        }

        if (playerHealth == null || headImage == null) return;

        // Flashing when damaged
        if (flashTimer > 0)
        {
            headImage.color = flashColor;
            flashTimer -= Time.deltaTime;
        }
        else
        {
            headImage.color = Color.white;
        }

        UpdateHeadSprite();
    }

    // ✅ Find the active player in the scene
    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerHealth = playerObj.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogWarning("⚠️ PlayerHeadUI: Found player but no PlayerHealth component!");
            }
        }
    }

    public void FlashHead()
    {
        flashTimer = flashDuration;
    }

    void UpdateHeadSprite()
    {
        int hp = Mathf.Max(0, Mathf.RoundToInt(playerHealth.currentHP)); // Clamp to 0+

        switch (hp)
        {
            case 4:
                headImage.sprite = headFullHP ?? originalSprite;
                break;
            case 3:
                headImage.sprite = headThreeHP ?? originalSprite;
                break;
            case 2:
                headImage.sprite = headTwoHP ?? originalSprite;
                break;
            case 1:
                headImage.sprite = headOneHP ?? originalSprite;
                break;
            case 0:
                headImage.sprite = headZeroHP ?? originalSprite;
                break;
            default:
                headImage.sprite = originalSprite;
                break;
        }
    }
}