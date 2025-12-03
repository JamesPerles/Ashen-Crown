using UnityEngine;
using UnityEngine.UI;

public class TransformationGauge : MonoBehaviour
{
    [Header("Gauge Settings")]
    [SerializeField] private Slider gaugeSlider;
    [SerializeField] private float maxEnergy = 100f;
    private float currentEnergy;

    [Header("Color Settings")]
    [SerializeField] private Image fillImage;        // Fill image (TransFill)
    [SerializeField] private Image backgroundImage;  // Background image (TransBar)
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color glowColor = new Color(0.7f, 0.2f, 1f); // Purple

    [Header("Glow Settings")]
    [SerializeField] private float glowIntensity = 2f;
    [SerializeField] private float glowSpeed = 3f;
    private bool isGlowing;

    void Awake()
    {
        // Automatically find UI references if missing
        if (gaugeSlider == null)
        {
            GameObject gaugeObj = GameObject.Find("TransGauge");
            if (gaugeObj != null)
                gaugeSlider = gaugeObj.GetComponent<Slider>();
            else
                Debug.LogWarning("TransGauge (Slider) not found in scene!");
        }

        if (fillImage == null)
        {
            GameObject fillObj = GameObject.Find("TransFill");
            if (fillObj != null)
                fillImage = fillObj.GetComponent<Image>();
            else
                Debug.LogWarning("TransFill (Fill Image) not found in scene!");
        }

        if (backgroundImage == null)
        {
            GameObject bgObj = GameObject.Find("TransBar");
            if (bgObj != null)
                backgroundImage = bgObj.GetComponent<Image>();
            else
                Debug.LogWarning("TransBar (Background Image) not found in scene!");
        }
    }

    void Start()
    {
        currentEnergy = 0f;

        if (gaugeSlider != null)
        {
            gaugeSlider.maxValue = maxEnergy;
            gaugeSlider.value = currentEnergy;
        }

        // Set starting color
        if (fillImage != null)
            fillImage.color = normalColor;

        if (backgroundImage != null)
            backgroundImage.color = normalColor;
    }

    void Update()
    {
        if (IsFull())
        {
            if (!isGlowing)
                isGlowing = true;

            float pulse = (Mathf.Sin(Time.time * glowSpeed) + 1f) / 2f;
            Color glow = Color.Lerp(glowColor * glowIntensity, glowColor, pulse);

            if (fillImage != null)
                fillImage.color = glow;

            if (backgroundImage != null)
                backgroundImage.color = glow;
        }
        else if (isGlowing)
        {
            isGlowing = false;

            if (fillImage != null)
                fillImage.color = normalColor;

            if (backgroundImage != null)
                backgroundImage.color = normalColor;
        }
    }

    public void AddEnergy(float amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0f, maxEnergy);
        if (gaugeSlider != null)
            gaugeSlider.value = currentEnergy;
    }

    public bool IsFull()
    {
        return currentEnergy >= maxEnergy;
    }
}
