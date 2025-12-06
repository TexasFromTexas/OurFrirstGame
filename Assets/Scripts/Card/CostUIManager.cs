using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CostUIManager : MonoBehaviour
{
    // 单例实例
    public static CostUIManager Instance;

    [Header("费用UI组件")]
    [Tooltip("显示费用文本（如：5/10）")]
    public TextMeshProUGUI costText;


    private void Awake()
    {
        // 单例初始化（确保场景中只有一个CostUIManager）
        if (Instance == null)
        {
            Instance = this;
            // 可选：防止场景切换时被销毁
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 实时更新费用UI显示
    /// </summary>
    /// <param name="currentCost">当前可用费用</param>
    /// <param name="maxCost">最大费用上限</param>
    public void UpdateCostUI(int currentCost, int maxCost)
    {
        // 更新文本显示
        if (costText != null)
        {
            costText.text = $"Cost: {currentCost}/{maxCost}";
        }
    }
}