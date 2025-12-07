using TMPro; // �Ƽ���TextMeshPro���赼���
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CardData;

public class Card : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI����")]
    public Image cardImage;
    public Image Cardname;
    public Image Type;

    // ��������
    private CardData _cardData;
    // ������������/�ƿ�/���ƶѣ�
    private Transform _originalParent;
    // �Ƿ����ק
    private bool _isDraggable = true;

    // ��ʼ����������
    public void Init(CardData data)
    {
        _cardData = data;
        cardImage.sprite = data.CardSprite;
    }

    // ������ƣ�ʹ�ÿ��ƣ�
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isDraggable) return;
        Debug.Log($"ʹ�ÿ��ƣ�{_cardData.CardName}");
        if (CardEffectManager.Instance == null)
        {
            Debug.LogError("CardEffectManager δ��ʼ����");
            return;
        }
        if (_cardData == null)
        {
            Debug.LogError("�������� _cardData Ϊ�գ�");
            return;
        }

        if (GameManager.Instance.SpendCost(_cardData.Cost))
        {
            // ִ�п���Ч��
            Debug.Log($"ʹ�ÿ��ƣ�{_cardData.CardName}������{_cardData.Cost}��");
            CardEffectManager.Instance.ExecuteEffect(_cardData);

            // ���������Ƴ�����ֹ Hand �������������ã�
            var hand = GameManager.Instance.Hand;
            if (hand != null)
            {
                hand.RemoveCard(this);
            }

            // �������ƶѲ���������
            GameManager.Instance.DiscardPile.AddCard(this);
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("���ò��㣬�޷�ʹ�øÿ���");
        }
    }

    // ��ʼ��ק
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
