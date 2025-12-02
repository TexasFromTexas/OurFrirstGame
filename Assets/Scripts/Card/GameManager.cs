using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 单例实例
    public static GameManager Instance;

    [Header("核心模块引用")]
    public Deck Deck;
    public Hand Hand;
    public Pile DiscardPile;

    [Header("预制体引用")]
    public GameObject CardPrefab;

    [Header("UI父容器")]
    public Transform handTransform; // 手牌的父对象（UGUI的Canvas子物体）

    private void Awake()
    {
        // 单例初始化
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // 开始回合：抽指定数量的牌（默认5张）
    public void StartTurn(int drawCount = 5)
    {
        Debug.Log("=== 回合开始 ===");
        for (int i = 0; i < drawCount; i++)
        {
            Card drawnCard = Deck.DrawCard();
            if (drawnCard != null)
                Hand.AddCard(drawnCard);
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
