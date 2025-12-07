using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public static Deck Instance;
    // ��ʼ�����б����Inspector�и�ֵ��
    public List<CardData> initialCards;
    // ʵ���ƿ⣨�洢�������ݣ�ϴ���ã�
    private List<CardData> _cardPool = new List<CardData>();


    private void Awake()
    {
        // ������ʼ��
        if (Instance == null)
        {
            Instance = this;
            // ��ʼ���ƿ⣺����ʼ������ӵ��Ƴ�
            if (initialCards != null)
            {
                // ���˵���ܵ� null ��Ŀ����������ʱȡ�� null ���� NRE
                int before = initialCards.Count;
                var valid = initialCards.FindAll(c => c != null);
                _cardPool.AddRange(valid);
                if (valid.Count != before)
                {
                    Debug.LogWarning("Deck��initialCards �д��� null ��Ŀ�����Զ����ˡ�");
                }
                ShuffleDeck(); // ��ʼϴ��
            }
            else
            {
                Debug.LogWarning("Deck����ʼ�����б��initialCards��δ��ֵ��");
            }
        }
        else
        {
            Destroy(gameObject); // ȷ��������ֻ��һ��Deckʵ��
        }
    }


    // ϴ�ƣ�Fisher-Yates ϴ���㷨����ƽ�����
    public void ShuffleDeck()
    {
        int n = _cardPool.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            (_cardPool[k], _cardPool[n]) = (_cardPool[n], _cardPool[k]);
        }
        Debug.Log("�ƿ���ϴ��");
    }

    // ���ƣ����ؿ���ʵ������ӵ����ƣ�
    public Card DrawCard()
    {
        // ������ܵ� null ��Ŀ����ֹ�������� null.CardName��
        _cardPool.RemoveAll(d => d == null);

        // �ƿ�Ϊ��ʱ�������ƶ�ϴ�Ƽ����ƿ�
        if (_cardPool.Count == 0)
        {
            Debug.Log("�ƿ�Ϊ�գ����Դ����ƶѲ���");
            // �ڵ��ò���ǰȷ�� GameManager ����
            if (GameManager.Instance == null)
            {
                Debug.LogError("Deck���޷����䣬GameManager.Instance Ϊ null��");
                return null;
            }
            RefillFromDiscardPile();
            // �ٴ��Ƴ� null �����
            _cardPool.RemoveAll(d => d == null);
            if (_cardPool.Count == 0)
            {
                Debug.Log("���ƿɳ飡");
                return null;
            }
        }

        // ȡ���Ƴص�һ����
        CardData drawnData = _cardPool[0];
        _cardPool.RemoveAt(0);

        if (drawnData == null)
        {
            Debug.LogError("Deck���鵽�� CardData Ϊ null��������������һ�š�");
            return DrawCard(); // �ݹ鳢����һ�ţ���ȫ����Ϊ���������� null��
        }

        // ʵ�������Ʋ���ӵ�����
        if (GameManager.Instance == null)
        {
            Debug.LogError("Deck��GameManager.Instance Ϊ null��");
            return null;
        }
        if (GameManager.Instance.CardPrefab == null)
        {
            Debug.LogError("Deck��GameManager �� CardPrefab δ��ֵ��");
            return null;
        }

        // ����� UI ���ƣ��Ƽ���ʵ���������Ƹ����壬ȷ�� Canvas/RectTransform ������ʾ
        Transform parent = GameManager.Instance.handTransform;
        GameObject cardObj;
        if (parent != null)
        {
            cardObj = Instantiate(GameManager.Instance.CardPrefab, parent, false);
        }
        else
        {
            cardObj = Instantiate(GameManager.Instance.CardPrefab);
        }

        string safeName = string.IsNullOrEmpty(drawnData.CardName) ? "Unknown" : drawnData.CardName;
        cardObj.name = $"Card_{safeName}";

        // �� UI prefab ȷ��������ȷ������Ԥ���屣���˷� 1 �����ţ�
        cardObj.transform.localScale = Vector3.one;

        Card card = cardObj.GetComponent<Card>();
        if (card != null)
        {
            card.Init(drawnData);
        }
        else
        {
            Debug.LogError($"Deck������Ԥ���� {cardObj.name} δ���� Card �ű���");
            Destroy(cardObj);
            return null;
        }

        Debug.Log($"���ƣ�{drawnData.CardName}");
        return card;
    }

    // �����ƶѲ����ƿ�
    private void RefillFromDiscardPile()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("RefillFromDiscardPile��GameManager.Instance Ϊ null���޷������ƿ⡣");
            return;
        }

        if (GameManager.Instance.DiscardPile == null)
        {
            Debug.LogWarning("RefillFromDiscardPile��DiscardPile Ϊ null���޷������ƿ⡣");
            return;
        }

        var discarded = GameManager.Instance.DiscardPile.GetAllCards();
        if (discarded == null || discarded.Count == 0)
        {
            Debug.Log("RefillFromDiscardPile�����ƶ�Ϊ�ջ򷵻� null��");
            return;
        }

        // ֻ����� null �Ŀ�������
        int added = 0;
        foreach (CardData data in discarded)
        {
            if (data != null)
            {
                _cardPool.Add(data);
                added++;
            }
        }

        // ������ƶѲ�ϴ�ƣ�ֻ��ȷʵ����˿���ʱϴ�ƣ�
        GameManager.Instance.DiscardPile.ClearPile();

        if (added > 0)
        {
            ShuffleDeck();
            Debug.Log($"�����ƶѲ����� {added} �ſ��Ƶ��ƿ�");
        }
    }

    // ���ƿ�����¿��ƣ����̵깺�򡢽�����
    public void AddCardToDeck(CardData newCard)
    {
        if (newCard == null)
        {
            Debug.LogWarning("AddCardToDeck: ��ͼ��� null ���ƣ��Ѻ��ԡ�");
            return;
        }
        _cardPool.Add(newCard);
        Debug.Log($"��ӿ��Ƶ��ƿ⣺{newCard.CardName}");
    }

    // ��ȡ�ƿ�ʣ�࿨����
    public int GetRemainingCards() => _cardPool.Count;
}