using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public RectTransform HandPanel; 
    // 手牌最大数量（默认10张）
    public int maxHandSize = 10;
    // 卡牌间距
    public float cardSpacing = 100f;
    // 手牌中心偏移（适配屏幕）
    public Vector2 centerOffset = new Vector2(0, -90f);

    // 默认卡牌尺寸（可在 Inspector 调整）
    public Vector2 defaultCardSize = new Vector2(200, 300);

    // 手牌列表（存储当前手牌的Card实例）
    private List<Card> _currentHand = new List<Card>();
    private void Awake()
    {
        if (HandPanel == null)
        {
            Debug.LogError("Hand：请在 Inspector 绑定 HandPanel！");
            return;
        }
        // 确保 HandPanel 是 Canvas 的子物体
        if (HandPanel.GetComponentInParent<Canvas>() == null)
        {
            Debug.LogError("Hand：HandPanel 必须是 Canvas 的子物体！");
        }
    }

    // 添加卡牌到手牌
    public void AddCard(Card card)
    {
        if (card == null)
        {
            Debug.LogError("Hand.AddCard: card 为 null");
            return;
        }
        if (HandPanel == null)
        {
            Debug.LogError("HandPanel 未绑定，无法添加卡牌");
            return;
        }

        if (_currentHand.Count >= maxHandSize)
        {
            Debug.Log("手牌已满，无法抽牌！");
            GameManager.Instance.DiscardPile.AddCard(card);
            Destroy(card.gameObject);
            return;
        }

        _currentHand.Add(card);

        // 将父对象设为 HandPanel，并让本地坐标不变（避免缩放/位置跳动）
        card.transform.SetParent(HandPanel, false);

        // 确保是 UI 元素并初始化 RectTransform
        RectTransform cardRect = card.GetComponent<RectTransform>();
        if (cardRect == null)
        {
            Debug.LogError($"卡牌 {card.name} 缺少 RectTransform，无法作为 UI 使用");
            return;
        }

        cardRect.anchoredPosition = Vector2.zero;
        cardRect.localRotation = Quaternion.identity;
        cardRect.localScale = Vector3.one;
        cardRect.sizeDelta = defaultCardSize; // 可调：默认尺寸

        // CanvasRenderer 多为 Image 自动添加，若没有则补上
        if (card.GetComponent<CanvasRenderer>() == null)
            card.gameObject.AddComponent<CanvasRenderer>();

        Debug.Log($"卡牌 {card.GetCardData()?.CardName ?? "未知"} 已添加到 HandPanel 下");
        RearrangeCards();
    }

    // 从手牌移除卡牌（使用/丢弃时）
    public void RemoveCard(Card card)
    {
        _currentHand.Remove(card);
        RearrangeCards(); // 重新排列
    }

    // 重新排列手牌（扇形排列，适配屏幕）
    public void RearrangeCards()
    {
        if (HandPanel == null) return;

        // ---------- 1) 清理已被销毁的引用 ----------
        // 使用临时列表收集仍然有效的 Card 引用（避免在迭代中修改原列表）
        var liveCards = new List<Card>(capacity: _currentHand.Count);
        foreach (var c in _currentHand)
        {
            // Unity 对已销毁对象的 == 运算符会返回 true
            if (c == null) continue;
            // 额外防护：确保 GameObject 未被销毁（保险）
            if (c.gameObject == null) continue;
            liveCards.Add(c);
        }

        // 如果有引用被移除，替换原列表
        if (liveCards.Count != _currentHand.Count)
        {
            _currentHand = liveCards;
        }

        int cardCount = _currentHand.Count;
        if (cardCount == 0) return;

        // ---------- 2) 计算排布参数 ----------
        float panelWidth = HandPanel.rect.width;
        float cardWidth = defaultCardSize.x;

        float actualSpacing = 0f;
        if (cardCount > 1)
        {
            float maxAllowedSpacing = (panelWidth - cardWidth) / (cardCount - 1);
            actualSpacing = Mathf.Min(cardSpacing, maxAllowedSpacing);
        }

        float totalWidth = (cardCount - 1) * actualSpacing;
        float startX = -totalWidth / 2 + centerOffset.x;

        // ---------- 3) 布局活卡 ----------
        for (int i = 0; i < cardCount; i++)
        {
            Card card = _currentHand[i];
            if (card == null) continue; // 再次保险检查

            RectTransform cardRect = card.GetComponent<RectTransform>();
            if (cardRect == null)
            {
                Debug.LogError($"Hand.RearrangeCards: 卡牌 {card.name} 缺少 RectTransform，跳过布局。");
                continue;
            }

            cardRect.anchoredPosition = new Vector2(startX + i * actualSpacing, centerOffset.y);
            cardRect.sizeDelta = defaultCardSize;
            // 将 sibling index 设置为 i，保持顺序
            card.transform.SetSiblingIndex(i);
        }
    }

    // 回合结束：将所有手牌放入弃牌堆
    public void DiscardAllCards()
    {
        foreach (Card card in _currentHand)
        {
            GameManager.Instance.DiscardPile.AddCard(card);
            Destroy(card.gameObject);
        }
        _currentHand.Clear();
        Debug.Log("回合结束，手牌已弃置");
    }

    // 获取当前手牌数
    public int GetHandSize() => _currentHand.Count;
}
