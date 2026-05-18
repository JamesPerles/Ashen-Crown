using UnityEngine;
using UnityEngine.UI;
public class TransformationGauge : MonoBehaviour
{
    [Header("Gauge Settings")]
    [SerializeField] Slider gaugeSlider;
    [SerializeField] float maxEnergy = 100f;
    public float currentEnergy;
    [Header("Color Settings")]
    [SerializeField] Image fillImage;
    [SerializeField] Image backgroundImage;
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color glowColor = new Color(0.7f, 0.2f, 1f);
    [SerializeField] float glowIntensity = 2f;
    [SerializeField] float glowSpeed = 3f;
    bool isGlowing;
    void Awake()
    {
        if (gaugeSlider == null)
        {
            GameObject gaugeObj = GameObject.Find("TransGauge");
            if (gaugeObj != null)
                gaugeSlider = gaugeObj.GetComponent<Slider>();
        }
        if (fillImage == null)
        {
            GameObject fillObj = GameObject.Find("TransFill");
            if (fillObj != null)
                fillImage = fillObj.GetComponent<Image>();
        }
        if (backgroundImage == null)
        {
            GameObject bgObj = GameObject.Find("TransBar");
            if (bgObj != null)
                backgroundImage = bgObj.GetComponent<Image>();
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
