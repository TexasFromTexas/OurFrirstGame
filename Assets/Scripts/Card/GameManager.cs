using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 单例实例
    public static GameManager Instance;

    [Header("核心模块引用")]
    public Deck Deck;
    public Hand Hand;
    public Pile DiscardPile;
    public GameObject Player;

    [Header("预制体引用")]
    public GameObject CardPrefab;

    [Header("UI父容器")]
    public Transform handTransform; // 手牌的父对象（UGUI的Canvas子物体）

    [Header("回合设置")]
    public int defaultDrawCount = 5; // 在 Inspector 中调整默认抽牌数

    [Header("费用配置")]
    public int maxCost = 3; // 最大费用（每回合上限）
    public int currentCost;  // 当前可用费用

    private void Awake()
    {
        // 单例初始化
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ResetCost()
    {
        currentCost = maxCost;
        Debug.Log($"回合开始，费用重置为：{currentCost}/{maxCost}");
        // 可选：更新UI显示费用（调用UI刷新方法）
        UpdateCostUI();
    }

    // 扣除费用（返回是否扣除成功）
    public bool SpendCost(int cost)
    {
        if (currentCost >= cost)
        {
            currentCost -= cost;
            Debug.Log($"扣除费用：{cost}，剩余费用：{currentCost}");
            UpdateCostUI(); // 更新UI
            return true;
        }
        else
        {
            Debug.LogWarning($"费用不足！当前：{currentCost}，需要：{cost}");
            return false;
        }
    }

    // 增加费用（可选，如通过卡牌/道具临时增加）
    public void AddCost(int amount)
    {
        currentCost = Mathf.Min(currentCost + amount, maxCost); // 不超过最大费用
        UpdateCostUI();
    }

    // 更新费用UI显示（需结合你的UI逻辑实现）
    public void UpdateCostUI()
    {
        // 调用CostUIManager更新UI
        if (CostUIManager.Instance != null)
        {
            CostUIManager.Instance.UpdateCostUI(currentCost, maxCost);
        }
        else
        {
           // CostUIManager 不存在时不打印过多日志，避免噪声
        }
    }

    // 无参重载：使用 Inspector 中的默认值
    public void StartTurn() => StartTurn(defaultDrawCount);

    // 开始回合：抽指定数量的牌（带诊断日志）
    public void StartTurn(int drawCount)
    {
        Debug.Log($"=== 回合开始 === drawCount={drawCount}");

        if (Instance == null)
        {
            Debug.LogError("StartTurn 调用时 GameManager.Instance 为 null。确保 GameManager 已在场景中并启用。");
            return;
        }

        if (Deck == null)
        {
            Debug.LogError("StartTurn：Deck 引用为 null。请在 Inspector 将带有 Deck 脚本的 GameObject 拖到 GameManager 的 Deck 字段上。");
            return;
        }

        if (Hand == null)
        {
            Debug.LogError("StartTurn：Hand 引用为 null。请在 Inspector 绑定 Hand。");
            return;
        }

        GameManager.Instance.ResetCost(); // 重置费用

        for (int i = 0; i < drawCount; i++)
        {
            Debug.Log($"StartTurn: 尝试抽牌 i={i}");
            Card drawnCard = null;
            try
            {
                drawnCard = Deck.DrawCard();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"StartTurn: 调用 Deck.DrawCard() 时抛出异常：{ex.Message}\n{ex.StackTrace}");
                return;
            }

            if (drawnCard == null)
            {
                Debug.LogWarning($"StartTurn: 第 {i} 次抽牌返回 null（可能牌库为空或预制体/脚本配置有误）");
            }
            else
            {
                Hand.AddCard(drawnCard);
                Debug.Log($"StartTurn: 第 {i} 次抽到卡片 {drawnCard.name}");
            }
        }
    }

    // 结束回合：弃置所有手牌，准备下回合
    public void EndTurn()
    {
        Debug.Log("=== 回合结束 ===");
        Hand.DiscardAllCards();
        // 下回合可调用StartTurn()开始
    }
}
