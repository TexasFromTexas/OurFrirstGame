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

    public void AddCost(int amount)
    {
        currentCost = Mathf.Min(currentCost + amount, maxCost); // 不超过最大费用
        UpdateCostUI();
    }

    public void UpdateCostUI()
    {
        if (CostUIManager.Instance != null)
        {
            CostUIManager.Instance.UpdateCostUI(currentCost, maxCost);
        }
    }

    public void StartTurn() => StartTurn(defaultDrawCount);

    public void StartTurn(int drawCount)
    {
        Debug.Log($"=== 回合开始 === drawCount={drawCount}");
        var gmInst = Instance;
        Debug.Log($"GameManager.Instance != null: {gmInst != null}");
        Debug.Log($"Deck reference set: {Deck != null} | Deck.Instance: {Deck?.GetRemainingCards() ?? -1}");
        if (Deck != null)
        {
            Debug.Log($"Deck.initialCards count: {Deck.initialCards?.Count ?? -1} | Deck pool: {Deck.GetRemainingCards()}");
        }

        if (Instance == null)
        {
            Debug.LogError("StartTurn 调用时 GameManager.Instance 为 null。确保 GameManager 已在场景中并启用。");
            return;
        }
        if (Deck == null)
        {
            Debug.LogError("StartTurn：Deck 引用为 null。请在 Inspector 绑定 Deck。");
            return;
        }
        if (Hand == null)
        {
            Debug.LogError("StartTurn：Hand 引用为 null。请在 Inspector 绑定 Hand。");
            return;
        }

        ResetCost(); // 会打印费用重置日志

        for (int i = 0; i < drawCount; i++)
        {
            Debug.Log($"StartTurn: 尝试抽牌 i={i}");
            Card drawnCard = Deck.DrawCard();
            Debug.Log($"StartTurn: DrawCard 返回 {(drawnCard == null ? "null" : drawnCard.name)}");
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

        // 诊断：列出 handTransform / HandPanel 下的子对象
        if (handTransform != null)
        {
            Debug.Log($"StartTurn: handTransform childCount = {handTransform.childCount}");
            for (int i = 0; i < handTransform.childCount; i++)
            {
                Debug.Log($" - hand child[{i}] = {handTransform.GetChild(i).name}");
            }
        }
        if (Hand != null && Hand.HandPanel != null)
        {
            Debug.Log($"StartTurn: Hand.HandPanel childCount = {Hand.HandPanel.childCount}");
            for (int i = 0; i < Hand.HandPanel.childCount; i++)
            {
                Debug.Log($" - HandPanel child[{i}] = {Hand.HandPanel.GetChild(i).name}");
            }
        }

        Debug.Log("StartTurn: 抽牌循环结束");
    }

    public void EndTurn()
    {
        Debug.Log("=== 回合结束 ===");
        Hand.DiscardAllCards();
    }
}
