using TMPro; // 推荐用TextMeshPro，需导入包
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class Card : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI引用")]
    public Image cardImage;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI CostText;
    public TextMeshProUGUI Description;
    public TextMeshProUGUI Bodytype;
    public TextMeshProUGUI Hardness;
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
        NameText.text = data.CardName;
        CostText.text = data.Cost.ToString();
        Description.text = data.Description;

        // 根据卡牌类型设置数值和图标
        switch (data.cardType)
        {
            case CardData.CardType.Bodytype:
                Bodytype.text = $"+{data.Bodytype} 体型";
                Type.color = Color.red; // 体型红色
                break;
            case CardData.CardType.Hardness:
                Hardness.text = $"+{data.Hardness} 硬度";
                Type.color = Color.blue; // 硬度蓝色
                break;
            case CardData.CardType.Item:
                Type.color = Color.yellow; // 道具黄色
                break;
        }
    }

    // 点击卡牌（使用卡牌）
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isDraggable) return;
        Debug.Log($"使用卡牌：{_cardData.CardName}");
        // 调用卡牌效果（后续扩展）
        CardEffectManager.Instance.ExecuteEffect(_cardData);
        // 移除手牌，根据类型放入弃牌堆
            GameManager.Instance.DiscardPile.AddCard(this);
        Destroy(gameObject); // 先销毁UI实例，数据保留在堆中
    }

    // 开始拖拽
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_isDraggable) return;
        _originalParent = transform.parent;
        transform.SetParent(transform.root); // 临时移到UI根节点，避免被父容器裁剪
        transform.SetAsLastSibling(); // 拖拽时显示在最上层
        cardImage.raycastTarget = false; // 避免拖拽时触发点击
    }

    // 拖拽中
    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDraggable) return;
        transform.position = Input.mousePosition; // 跟随鼠标移动
    }

    // 结束拖拽
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDraggable) return;
        transform.SetParent(_originalParent); // 放回原父容器
        cardImage.raycastTarget = true;
        // 重置位置（手牌会自动排列，后续实现）
        GameManager.Instance.Hand.RearrangeCards();
    }

    // 获取卡牌数据
    public CardData GetCardData() => _cardData;
}
