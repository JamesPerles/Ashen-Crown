using UnityEngine;
using UnityEngine.UI;
public class PlayerHeadUI : MonoBehaviour
{
    [Header("References")]
    public Image headImage;
    public PlayerHealth playerHealth;
    [Header("Head Sprites")]
    public Sprite headFullHP;
    public Sprite headThreeHP;
    public Sprite headTwoHP;
    public Sprite headOneHP;
    public Sprite headZeroHP;
    [Header("Flash Settings")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.2f;
    private float flashTimer = 0f;
    [Header("Other")]
    [SerializeField] string playerTag = "Player";
    Sprite originalSprite;
    void Start()
    {
        if (headImage != null)
            originalSprite = headImage.sprite;
        FindPlayer();
    }
    void Update()
    {
        if (playerHealth == null)
        {
            FindPlayer();
        }
        if (playerHealth == null || headImage == null) return;
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
    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
    }
    public void FlashHead()
    {
        flashTimer = flashDuration;
    }
    void UpdateHeadSprite()
    {
        int hp = Mathf.Max(0, Mathf.RoundToInt(playerHealth.currentHP));
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