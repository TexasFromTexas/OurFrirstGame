using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("特效参数")]
    public float hoverScale = 1.05f; // 悬停缩放比例
    public float selectScale = 1.1f; // 选中缩放比例
    public Color hoverColor = new Color(1, 1, 1, 1); // 悬停边框色
    public Color selectColor = new Color(0, 1, 0, 1); // 选中边框色（绿色）

    private Outline cardOutline; // 卡牌边框
    private Vector3 originalScale; // 原始缩放
    private bool isSelected = false; // 是否选中

    private void Awake()
    {
        // 获取组件
        cardOutline = GetComponent<Outline>();
        if (cardOutline == null) cardOutline = gameObject.AddComponent<Outline>();
        cardOutline.enabled = false; // 默认隐藏边框
        originalScale = transform.localScale;
    }

    // 鼠标进入卡牌
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected)
        {
            transform.localScale = originalScale * hoverScale;
            cardOutline.effectColor = hoverColor;
            cardOutline.enabled = true;
        }
    }

    // 鼠标离开卡牌
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected)
        {
            transform.localScale = originalScale;
            cardOutline.enabled = false;
        }
    }

    // 鼠标点击卡牌（选中/取消选中）
    public void OnPointerClick(PointerEventData eventData)
    {
        isSelected = !isSelected;
        if (isSelected)
        {
            transform.localScale = originalScale * selectScale;
            cardOutline.effectColor = selectColor;
            cardOutline.enabled = true;
        }
        else
        {
            transform.localScale = originalScale;
            cardOutline.enabled = false;
        }
    }
}
