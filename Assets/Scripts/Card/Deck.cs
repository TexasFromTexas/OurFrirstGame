using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public static Deck Instance;
    // 初始卡牌列表（在Inspector中赋值）
    public List<CardData> initialCards;
    // 实际牌库（存储卡牌数据，洗牌用）
    private List<CardData> _cardPool = new List<CardData>();

    private void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
            // 初始化牌库：将初始卡牌添加到牌池
            if (initialCards != null)
            {
                // 过滤 null，避免运行时 NRE
                _cardPool.AddRange(initialCards.FindAll(c => c != null));
                ShuffleDeck(); // 初始洗牌
            }
            else
            {
                Debug.LogWarning("Deck：initialCards 未赋值（Inspector），牌库为空。");
            }
        }
        else
        {
            Destroy(gameObject); // 确保场景中只有一个Deck实例
        }
    }

    // 洗牌（Fisher-Yates）
    public void ShuffleDeck()
    {
        int n = _cardPool.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            (_cardPool[k], _cardPool[n]) = (_cardPool[n], _cardPool[k]);
        }
        Debug.Log("Deck: 牌库已洗牌，剩余 " + _cardPool.Count + " 张。");
    }

    // 抽牌（返回 Card 实例）
    public Card DrawCard()
    {
        // 清理可能的 null 条目
        _cardPool.RemoveAll(d => d == null);

        if (_cardPool.Count == 0)
        {
            Debug.Log("Deck.DrawCard: 牌库为空，尝试从弃牌堆补充。");
            if (GameManager.Instance == null)
            {
                Debug.LogError("Deck.DrawCard: 无法补牌，GameManager.Instance 为 null。");
                return null;
            }
            RefillFromDiscardPile();
            _cardPool.RemoveAll(d => d == null);
            if (_cardPool.Count == 0)
            {
                Debug.LogWarning("Deck.DrawCard: 补牌后仍无牌可抽。");
                return null;
            }
        }

        CardData drawnData = _cardPool[0];
        _cardPool.RemoveAt(0);

        if (drawnData == null)
        {
            Debug.LogError("Deck.DrawCard: 抽到 null CardData（已过滤但仍发生），返回 null。");
            return null;
        }

        // 确保 GameManager 与 CardPrefab 可用
        if (GameManager.Instance == null)
        {
            Debug.LogError("Deck.DrawCard: GameManager.Instance 为 null，无法实例化卡牌预制体。");
            return null;
        }
        if (GameManager.Instance.CardPrefab == null)
        {
            Debug.LogError("Deck.DrawCard: GameManager.CardPrefab 未设置！请在 Inspector 指定项目内的卡牌 Prefab（不可为场景对象）。");
            return null;
        }

        // 实例化：优先放到 handTransform（UI 父物体）下，确保显示与缩放正确
        Transform parent = GameManager.Instance.handTransform;
        GameObject cardObj = parent != null
            ? Instantiate(GameManager.Instance.CardPrefab, parent, false)
            : Instantiate(GameManager.Instance.CardPrefab);

        // 容错：确保实例化结果非 null
        if (cardObj == null)
        {
            Debug.LogError("Deck.DrawCard: Instantiate 返回 null（请检查 CardPrefab 是否为有效 Prefab）。");
            return null;
        }

        string safeName = string.IsNullOrEmpty(drawnData.CardName) ? "Unknown" : drawnData.CardName;
        cardObj.name = $"Card_{safeName}";

        // 强制重置缩放（UI 预制体避免异常缩放）
        cardObj.transform.localScale = Vector3.one;

        Card card = cardObj.GetComponent<Card>();
        if (card == null)
        {
            Debug.LogError($"Deck.DrawCard: 预制体 {cardObj.name} 未挂载 Card 脚本，销毁实例并返回 null。");
            Destroy(cardObj);
            return null;
        }

        card.Init(drawnData);
        Debug.Log($"Deck.DrawCard: 抽到卡片 {drawnData.CardName}，实例化成功。");
        return card;
    }

    // 从弃牌堆补充牌库
    private void RefillFromDiscardPile()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("RefillFromDiscardPile: GameManager.Instance 为 null，无法补牌。");
            return;
        }
        if (GameManager.Instance.DiscardPile == null)
        {
            Debug.LogWarning("RefillFromDiscardPile: DiscardPile 未设置，无法补牌。");
            return;
        }

        var discarded = GameManager.Instance.DiscardPile.GetAllCards();
        if (discarded == null || discarded.Count == 0)
        {
            Debug.Log("RefillFromDiscardPile: 弃牌堆为空。");
            return;
        }

        int added = 0;
        foreach (var data in discarded)
        {
            if (data != null)
            {
                _cardPool.Add(data);
                added++;
            }
        }

        GameManager.Instance.DiscardPile.ClearPile();
        if (added > 0) ShuffleDeck();
        Debug.Log($"RefillFromDiscardPile: 从弃牌堆补充了 {added} 张牌到牌库。");
    }

    public void AddCardToDeck(CardData newCard)
    {
        if (newCard == null)
        {
            Debug.LogWarning("Deck.AddCardToDeck: 试图添加 null 卡牌，已忽略。");
            return;
        }
        _cardPool.Add(newCard);
        Debug.Log($"Deck.AddCardToDeck: 添加卡牌 {newCard.CardName} 到牌库，当前数量 {_cardPool.Count}。");
    }

    public int GetRemainingCards() => _cardPool.Count;
}