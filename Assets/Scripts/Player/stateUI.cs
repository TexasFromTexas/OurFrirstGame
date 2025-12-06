using UnityEngine;
using UnityEngine.UI;

public class stateUI : MonoBehaviour
{
    [Header("UI文本组件绑定")]
    [SerializeField] private Text targetHealthText;
    [SerializeField] private Text currentHealthText;
    [SerializeField] private Text minDamageText;
    [SerializeField] private Text currentSizeText;
    [SerializeField] private Text speedMultiplierText;

    [Header("目标引用")]
    [SerializeField] private GameObject target;

    private HealthSystem_New targetHealth;
    private SpeedAndSize targetSpeedAndSize;

    private void Awake()
    {
        if (target != null)
        {
            targetHealth = target.GetComponent<HealthSystem_New>();
            targetSpeedAndSize = target.GetComponent<SpeedAndSize>();
        }
        else
        {
            Debug.LogWarning("【UI脚本】未指定目标GameObject！");
        }
    }

    private void Update()
    {
        UpdateHealthUI();
        UpdateMinDamageUI();
        UpdateCurrentSizeUI();
        UpdateSpeedMultiplierUI();
    }

    private void UpdateHealthUI()
    {
        if (targetHealth == null)
        {
            targetHealthText.text = "目标生命值：-/-";
            currentHealthText.text = "当前生命值：-";
            return;
        }

        targetHealthText.text = $"目标生命值：{targetHealth.GetCurrentHealth()}/{targetHealth.GetMaxHealth()}";
        currentHealthText.text = $"当前生命值：{targetHealth.GetCurrentHealth()}";
    }

    private void UpdateMinDamageUI()
    {
        // 显示来自 SpeedAndSize 的最低攻击力（若有）
        if (targetSpeedAndSize != null)
            minDamageText.text = $"最低攻击力：{targetSpeedAndSize.MinDamage}";
        else
            minDamageText.text = $"最低攻击力：-";
    }

    private void UpdateCurrentSizeUI()
    {
        if (target == null)
        {
            currentSizeText.text = "当前大小（r）：-";
            return;
        }

        float r = Mathf.Max(target.transform.localScale.x, target.transform.localScale.y);
        currentSizeText.text = $"当前大小（r）：{r:F2}";
    }

    private void UpdateSpeedMultiplierUI()
    {
        if (targetSpeedAndSize == null)
        {
            speedMultiplierText.text = "速度倍率：-";
            return;
        }

        speedMultiplierText.text = $"速度倍率：{targetSpeedAndSize.DamageMultiplier:F2}";
    }
}