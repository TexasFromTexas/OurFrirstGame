using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// UI显示脚本：显示生命值、最低攻击力、当前大小、速度倍率
/// </summary>
public class stateUI : MonoBehaviour
{
    [Header("UI文本组件绑定")]
    [SerializeField] private Text targetHealthText; // 目标生命值文本（格式：当前/最大）
    [SerializeField] private Text currentHealthText; // 当前生命值文本（格式：当前）
    [SerializeField] private Text minDamageText; // 最低攻击力文本
    [SerializeField] private Text currentSizeText; // 当前大小（r）文本
    [SerializeField] private Text speedMultiplierText; // 速度倍率文本

    [Header("目标引用")]
    [SerializeField] private GameObject target; // 要显示的目标（玩家/敌人）

    // 组件缓存
    private HealthSystem_New targetHealth;
    private SpeedAndSize targetSpeedAndSize;

    private void Awake()
    {
        // 初始化目标组件引用
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
        // 更新UI显示
        UpdateHealthUI();
        UpdateMinDamageUI();
        UpdateCurrentSizeUI();
        UpdateSpeedMultiplierUI();
    }

    /// <summary>
    /// 更新生命值UI
    /// </summary>
    private void UpdateHealthUI()
    {
        if (targetHealth == null)
        {
            targetHealthText.text = "目标生命值：-/-";
            currentHealthText.text = "当前生命值：-";
            return;
        }

        // 目标生命值：当前/最大
        targetHealthText.text = $"目标生命值：{targetHealth.GetCurrentHealth()}/{targetHealth.GetMaxHealth()}";
        // 当前生命值：当前
        currentHealthText.text = $"当前生命值：{targetHealth.GetCurrentHealth()}";
    }

    /// <summary>
    /// 更新最低攻击力UI（固定为1）
    /// </summary>
    private void UpdateMinDamageUI()
    {
        minDamageText.text = $"最低攻击力：1";
    }

    /// <summary>
    /// 更新当前大小UI（r）：缩放为(1,1)时，r=1
    /// </summary>
    private void UpdateCurrentSizeUI()
    {
        if (target == null)
        {
            currentSizeText.text = "当前大小（r）：-";
            return;
        }

        // 计算当前大小r：取缩放的最大值（2D物体通常x/y缩放一致）
        float r = Mathf.Max(target.transform.localScale.x, target.transform.localScale.y);
        currentSizeText.text = $"当前大小（r）：{r:F2}";
    }

    /// <summary>
    /// 更新速度倍率UI
    /// </summary>
    private void UpdateSpeedMultiplierUI()
    {
        if (targetSpeedAndSize == null)
        {
            speedMultiplierText.text = "速度倍率：-";
            return;
        }

        // 从SpeedAndSize脚本获取速度倍率（需将damageMultiplier设为public或添加getter）
        // 注意：需修改SpeedAndSize脚本，将damageMultiplier设为public，或添加公共方法获取
        speedMultiplierText.text = $"速度倍率：{targetSpeedAndSize.damageMultiplier:F2}";
    }
}