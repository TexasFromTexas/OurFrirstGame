using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public RectTransform HandPanel; 
    // 手牌最大数量（默认10张）
    public int maxHandSize = 10;
    // 卡牌间距
    public float cardSpacing = 80f;
    // 手牌中心偏移（适配屏幕）
    public Vector2 centerOffset = new Vector2(0, -200f);

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
        if (_currentHand.Count >= maxHandSize)
        {
            Debug.Log("手牌已满，无法抽牌！");
            // 手牌满时，新抽的牌直接进入弃牌堆
            GameManager.Instance.DiscardPile.AddCard(card);
            Destroy(card.gameObject);
            return;
        }

        _currentHand.Add(card);
        // 关键：直接用 handPanel 作为父容器（不依赖 GameManager）
        card.transform.SetParent(HandPanel);
        // 强制重置卡牌变换（清除所有异常）
        card.transform.localPosition = Vector3.zero;
        card.transform.localRotation = Quaternion.identity;
        card.transform.localScale = Vector3.one;
        // 确保卡牌是 UI 元素（避免渲染异常）
        CanvasRenderer renderer = card.GetComponent<CanvasRenderer>();
        if (renderer == null)
        {
            card.gameObject.AddComponent<CanvasRenderer>();
        }

        Debug.Log($"卡牌 {card.GetCardData().CardName} 已添加到 HandPanel 下");
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
        int cardCount = _currentHand.Count;
        if (cardCount == 0) return;

        // 直接获取 HandPanel 的实际宽度（无需通过 GameManager）
        float panelWidth = HandPanel.rect.width;
        float maxAllowedSpacing = (panelWidth - 150) / (cardCount - 1); // 150=单张卡牌宽
        float actualSpacing = Mathf.Min(cardSpacing, maxAllowedSpacing);
        float totalWidth = (cardCount - 1) * actualSpacing;
        float startX = -totalWidth / 2 + centerOffset.x;

        for (int i = 0; i < cardCount; i++)
        {
            Card card = _currentHand[i];
            RectTransform cardRect = card.GetComponent<RectTransform>();
            if (cardRect == null)
            {
                Debug.LogError($"卡牌 {card.name} 没有 RectTransform 组件！");
                continue;
            }
            cardRect.anchoredPosition = new Vector2(startX + i * actualSpacing, centerOffset.y);
            cardRect.sizeDelta = new Vector2(150, 220); // 强制卡牌宽高
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
