using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Slider))]
public sealed class HealthBarView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;

    private void Awake()
    {
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
        }

        if (healthSlider == null)
        {
            Debug.LogError("血量 UI 找不到 Slider 元件。", this);
            enabled = false;
            return;
        }

        healthSlider.interactable = false;
        healthSlider.wholeNumbers = false;
    }

    public void SetHealth(float currentHealth, float maxHealth)
    {
        if (healthSlider == null)
        {
            return;
        }

        float safeMaxHealth = Mathf.Max(0f, maxHealth);
        float safeCurrentHealth = Mathf.Clamp(currentHealth, 0f, safeMaxHealth);

        healthSlider.minValue = 0f;
        healthSlider.maxValue = Mathf.Max(1f, safeMaxHealth);
        healthSlider.SetValueWithoutNotify(safeCurrentHealth);

        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(safeCurrentHealth)} / {Mathf.CeilToInt(safeMaxHealth)}";
        }
    }
}
