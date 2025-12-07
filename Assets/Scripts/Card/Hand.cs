using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public RectTransform HandPanel;
    public int maxHandSize = 10;
    public float cardSpacing = 100f;
    public Vector2 centerOffset = new Vector2(0, -90f);

    public Vector2 defaultCardSize = new Vector2(200, 300);

    private List<Card> _currentHand = new List<Card>();
    private void Awake()
    {
        if (HandPanel == null)
        {
            Debug.LogError("Hand: Inspector 中 HandPanel 未绑定");
            return;
        }
        if (HandPanel.GetComponentInParent<Canvas>() == null)
        {
            Debug.LogError("Hand: HandPanel 未找到父 Canvas");
        }
    }

    // 添加卡到手牌（增强诊断）
    public void AddCard(Card card)
    {
        // 防御性：清理已被销毁的引用，保证计数反映真实活卡
        _currentHand.RemoveAll(c => c == null);

        if (card == null)
        {
            Debug.LogError("Hand.AddCard: card 为 null");
            return;
        }
        if (HandPanel == null)
        {
            Debug.LogError("HandPanel 未赋值，无法添加卡牌");
            return;
        }

        if (_currentHand.Count >= maxHandSize)
        {
            Debug.Log("手牌已满，无法加入新卡，直接丢弃");
            GameManager.Instance.DiscardPile.AddCard(card);
            Destroy(card.gameObject);
            return;
        }

        // 防止重复添加同一张卡
        if (_currentHand.Contains(card))
        {
            Debug.LogWarning($"Hand.AddCard: 卡牌 {card.name} 已在手牌中，跳过添加。");
            return;
        }

        _currentHand.Add(card);

        // 把卡牌放到 HandPanel 下
        card.transform.SetParent(HandPanel, false);

        RectTransform cardRect = card.GetComponent<RectTransform>();
        if (cardRect == null)
        {
            Debug.LogError($"Hand.AddCard: {card.name} 缺少 RectTransform，无法正确显示");
            return;
        }

        // 初始化 UI 变换
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.localRotation = Quaternion.identity;
        cardRect.localScale = Vector3.one;
        cardRect.sizeDelta = defaultCardSize;

        if (card.GetComponent<CanvasRenderer>() == null)
            card.gameObject.AddComponent<CanvasRenderer>();

        // 诊断日志：父对象/active/层级/HandPanel 子数
        Debug.Log($"Hand: 将 {card.GetCardData()?.CardName ?? "未知"} 添加到 HandPanel");
        Debug.Log($" - card.activeInHierarchy={card.gameObject.activeInHierarchy}, parent={card.transform.parent?.name}");
        Debug.Log($" - cardRect.anchoredPosition={cardRect.anchoredPosition}, sizeDelta={cardRect.sizeDelta}");
        Debug.Log($" - HandPanel childCount={HandPanel.childCount}");

        RearrangeCards();
    }

    // 从手牌中移除卡（外部在销毁前应调用）
    public void RemoveCard(Card card)
    {
        if (card == null) return;
        _currentHand.Remove(card);
        RearrangeCards(); // 更新布局
    }

    // 布局手牌（先清理空引用）
    public void RearrangeCards()
    {
        if (HandPanel == null) return;

        // 先移除已被销毁的 entry，避免 MissingReferenceException
        _currentHand.RemoveAll(c => c == null);

        int cardCount = _currentHand.Count;
        if (cardCount == 0) return;

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

        for (int i = 0; i < cardCount; i++)
        {
            Card card = _currentHand[i];
            if (card == null) continue; // 再保险检查

            RectTransform cardRect = card.GetComponent<RectTransform>();
            if (cardRect == null)
            {
                Debug.LogError($"Hand.RearrangeCards: 卡牌 {card.name} 缺少 RectTransform，跳过布局。");
                continue;
            }

            cardRect.anchoredPosition = new Vector2(startX + i * actualSpacing, centerOffset.y);
            cardRect.sizeDelta = defaultCardSize;
            card.transform.SetSiblingIndex(i);
        }
    }

    public void DiscardAllCards()
    {
        foreach (Card card in new List<Card>(_currentHand))
        {
            GameManager.Instance.DiscardPile.AddCard(card);
            Destroy(card.gameObject);
        }
        _currentHand.Clear();
        Debug.Log("已弃置所有手牌");
    }

    public int GetHandSize() => _currentHand.Count;
}
