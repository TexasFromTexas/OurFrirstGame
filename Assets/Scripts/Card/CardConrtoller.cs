using TMPro; // 推荐用TextMeshPro，需导入包
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CardData;
using static UnityEditor.Progress;

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

        // 根据卡牌类型设置数值和图标
        switch (data.cardType)
        {
            case CardData.CardType.Buff:
                Type.color = Color.red; // 改变数值类型牌红色
                break;
            case CardData.CardType.Cost:
                Type.color = Color.blue; // 费用相关蓝色
                break;
            case CardData.CardType.Item:
                Type.color = Color.yellow; // 道具黄色
                break;
        }
    }
    private string GetEffectDescription(CardData data)
    {
        switch (data.effectType)
        {
            case CardEffect.AddHealth:
                return $"恢复 {data.effectValue} 生命值";
            case CardEffect.ReduceHealth:
                return $"受到 {data.effectValue} 伤害";
            case CardEffect.IncreaseSpeed:
                return $"速度提升 {data.effectValue}";
            case CardEffect.DecreaseSpeed:
                return $"速度降低 {data.effectValue}";
            case CardEffect.EnlargeBodytype:
                return $"体积增大 {data.effectValue}";
            case CardEffect.ShrinkBodytype:
                return $"体积缩小 {data.effectValue}";
            case CardEffect.AddCurrentCost:
                return $"增加 {data.effectValue} 点当前费用";
            case CardEffect.IncreaseMaxCost:
                return $"增加 {data.effectValue} 点最大费用";
            default:
                return "无特殊效果";
        }
    }

    // 点击卡牌（使用卡牌）
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isDraggable) return;
        Debug.Log($"使用卡牌：{_cardData.CardName}");
        // 替换原代码
        if (CardEffectManager.Instance == null)
        {
            Debug.LogError("CardEffectManager 未初始化！请在场景中添加该物体并挂载脚本");
            return;
        }
        if (_cardData == null)
        {
            Debug.LogError("卡牌数据 _cardData 为空！请检查 Init 方法是否正确赋值");
            return;
        }

        if (GameManager.Instance.SpendCost(_cardData.Cost))
        {
            // 费用足够：执行效果
            Debug.Log($"使用卡牌：{_cardData.CardName}（消耗{_cardData.Cost}费用）");
            CardEffectManager.Instance.ExecuteEffect(_cardData);

            // 后续逻辑：移至弃牌堆等
            GameManager.Instance.DiscardPile.AddCard(this);
            Destroy(gameObject);
        }
        else
        {
            // 费用不足：可添加提示（如UI闪烁、音效等）
            Debug.Log("费用不足，无法使用该卡牌");
            // 示例：播放错误提示
            // UIManager.Instance.ShowNotEnoughManaTip();
        }

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
