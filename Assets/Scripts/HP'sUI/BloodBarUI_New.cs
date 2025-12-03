using UnityEngine;


using UnityEngine.UI;

// 独立血条UI更新，无需修改原有脚本
public class BloodBarUI_New : MonoBehaviour
{
    [SerializeField] private Slider bloodSlider; // 绑定自身的Slider组件

    private void Awake()
    {
        // 初始化Slider参数
        if (bloodSlider == null)
        {
            bloodSlider = GetComponent<Slider>();
        }
        if (bloodSlider != null)
        {
            bloodSlider.maxValue = 1;
            bloodSlider.minValue = 0;
            bloodSlider.value = 1; // 初始满血
            bloodSlider.interactable = false; // 禁止手动拖动
        }
    }

    /// <summary>
    /// 外部调用更新血条（0~1的比例）
    /// </summary>
    public void SetBloodValue(float value)
    {
        if (bloodSlider == null) return;
        value = Mathf.Clamp01(value); // 限制0~1
        bloodSlider.value = value;
    }
}