using TMPro; // 推荐用TextMeshPro，需导入包
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CardData;

public class Card : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI引用")]
    public Image cardImage;
    public Image Cardname;
    public Image Type;

    // 卡牌数据
    private CardData _cardData;
    // 父容器（手牌/牌库/弃牌堆）
    private Transform _originalParent;
    // 是否可拖拽
    private bool _isDraggable = true;

    // 初始化卡牌数据
    public void Init(CardData data)
    {
        _cardData = data;
        cardImage.sprite = data.CardSprite;
    }

    // 点击卡牌（使用卡牌）
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isDraggable) return;
        Debug.Log($"使用卡牌：{_cardData.CardName}");
        if (CardEffectManager.Instance == null)
        {
            Debug.LogError("CardEffectManager 未初始化！");
            return;
        }
        if (_cardData == null)
        {
            Debug.LogError("卡牌数据 _cardData 为空！");
            return;
        }

        if (GameManager.Instance.SpendCost(_cardData.Cost))
        {
            // 执行卡牌效果
            Debug.Log($"使用卡牌：{_cardData.CardName}（消耗{_cardData.Cost}）");
            CardEffectManager.Instance.ExecuteEffect(_cardData);

            // 从手牌中移除（防止 Hand 持有已销毁引用）
            var hand = GameManager.Instance.Hand;
            if (hand != null)
            {
                hand.RemoveCard(this);
            }

            // 加入弃牌堆并销毁自身
            GameManager.Instance.DiscardPile.AddCard(this);
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("费用不足，无法使用该卡牌");
        }
    }

    // 开始拖拽
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_isDraggable) return;
        _originalParent = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        cardImage.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDraggable) return;
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDraggable) return;
        transform.SetParent(_originalParent);
        cardImage.raycastTarget = true;
        GameManager.Instance.Hand.RearrangeCards();
    }

    public CardData GetCardData() => _cardData;
}
