using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    // 初始卡牌列表（在Inspector中赋值）
    public List<CardData> initialCards;
    // 实际牌库（存储卡牌数据，洗牌用）
    private List<CardData> _cardPool = new List<CardData>();

    private void Awake()
    {
        // 初始化牌库：将初始卡牌添加到牌池
        _cardPool.AddRange(initialCards);
        ShuffleDeck(); // 初始洗牌
    }

    // 洗牌（Fisher-Yates 洗牌算法，公平随机）
    public void ShuffleDeck()
    {
        int n = _cardPool.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            (_cardPool[k], _cardPool[n]) = (_cardPool[n], _cardPool[k]);
        }
        Debug.Log("牌库已洗牌");
    }

    // 抽牌（返回卡牌实例，添加到手牌）
    public Card DrawCard()
    {
        // 牌库为空时，将弃牌堆洗牌加入牌库
        if (_cardPool.Count == 0)
        {
            Debug.Log("牌库为空，洗牌弃牌堆");
            RefillFromDiscardPile();
            if (_cardPool.Count == 0)
            {
                Debug.Log("无牌可抽！");
                return null;
            }
        }

        // 取出牌池第一张牌
        CardData drawnData = _cardPool[0];
        _cardPool.RemoveAt(0);

        // 实例化卡牌并添加到手牌
        GameObject cardObj = Instantiate(GameManager.Instance.CardPrefab);
        cardObj.name = $"Card_{drawnData.CardName}";

        Card card = cardObj.GetComponent<Card>();
        if (card != null)
        {
            card.Init(drawnData);
        }
        else
        {
            Debug.LogError($"Deck：卡牌预制体 {cardObj.name} 未挂载 Card 脚本！");
            Destroy(cardObj);
            return null;
        }

        Debug.Log($"抽牌：{drawnData.CardName}");
        return card;
    }

    // 从弃牌堆补充牌库
    private void RefillFromDiscardPile()
    {
        // 将弃牌堆的所有卡牌数据加入牌库
        foreach (CardData data in GameManager.Instance.DiscardPile.GetAllCards())
        {
            _cardPool.Add(data);
        }
        // 清空弃牌堆
        GameManager.Instance.DiscardPile.ClearPile();
        // 洗牌
        ShuffleDeck();
    }

    // 向牌库添加新卡牌（如商店购买、奖励）
    public void AddCardToDeck(CardData newCard)
    {
        _cardPool.Add(newCard);
        Debug.Log($"添加卡牌到牌库：{newCard.CardName}");
    }

    // 获取牌库剩余卡牌数
    public int GetRemainingCards() => _cardPool.Count;
}