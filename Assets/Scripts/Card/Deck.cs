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
                // 过滤掉可能的 null 条目，避免运行时取出 null 引发 NRE
                int before = initialCards.Count;
                var valid = initialCards.FindAll(c => c != null);
                _cardPool.AddRange(valid);
                if (valid.Count != before)
                {
                    Debug.LogWarning("Deck：initialCards 中存在 null 条目，已自动过滤。");
                }
                ShuffleDeck(); // 初始洗牌
            }
            else
            {
                Debug.LogWarning("Deck：初始卡牌列表（initialCards）未赋值！");
            }
        }
        else
        {
            Destroy(gameObject); // 确保场景中只有一个Deck实例
        }
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
        // 清理可能的 null 条目（防止后续访问 null.CardName）
        _cardPool.RemoveAll(d => d == null);

        // 牌库为空时，将弃牌堆洗牌加入牌库
        if (_cardPool.Count == 0)
        {
            Debug.Log("牌库为空，尝试从弃牌堆补充");
            // 在调用补充前确保 GameManager 可用
            if (GameManager.Instance == null)
            {
                Debug.LogError("Deck：无法补充，GameManager.Instance 为 null！");
                return null;
            }
            RefillFromDiscardPile();
            // 再次移除 null 并检查
            _cardPool.RemoveAll(d => d == null);
            if (_cardPool.Count == 0)
            {
                Debug.Log("无牌可抽！");
                return null;
            }
        }

        // 取出牌池第一张牌
        CardData drawnData = _cardPool[0];
        _cardPool.RemoveAt(0);

        if (drawnData == null)
        {
            Debug.LogError("Deck：抽到的 CardData 为 null，跳过并尝试下一张。");
            return DrawCard(); // 递归尝试下一张（安全，因为我们已清理 null）
        }

        // 实例化卡牌并添加到手牌
        if (GameManager.Instance == null)
        {
            Debug.LogError("Deck：GameManager.Instance 为 null！");
            return null;
        }
        if (GameManager.Instance.CardPrefab == null)
        {
            Debug.LogError("Deck：GameManager 的 CardPrefab 未赋值！");
            return null;
        }

        // 如果是 UI 卡牌，推荐把实例放入手牌父物体，确保 Canvas/RectTransform 正常显示
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

        // 对 UI prefab 确保缩放正确（避免预制体保存了非 1 的缩放）
        cardObj.transform.localScale = Vector3.one;

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
        if (GameManager.Instance == null)
        {
            Debug.LogError("RefillFromDiscardPile：GameManager.Instance 为 null，无法补充牌库。");
            return;
        }

        if (GameManager.Instance.DiscardPile == null)
        {
            Debug.LogWarning("RefillFromDiscardPile：DiscardPile 为 null，无法补充牌库。");
            return;
        }

        var discarded = GameManager.Instance.DiscardPile.GetAllCards();
        if (discarded == null || discarded.Count == 0)
        {
            Debug.Log("RefillFromDiscardPile：弃牌堆为空或返回 null。");
            return;
        }

        // 只加入非 null 的卡牌数据
        int added = 0;
        foreach (CardData data in discarded)
        {
            if (data != null)
            {
                _cardPool.Add(data);
                added++;
            }
        }

        // 清空弃牌堆并洗牌（只在确实添加了卡牌时洗牌）
        GameManager.Instance.DiscardPile.ClearPile();

        if (added > 0)
        {
            ShuffleDeck();
            Debug.Log($"从弃牌堆补充了 {added} 张卡牌到牌库");
        }
    }

    // 向牌库添加新卡牌（如商店购买、奖励）
    public void AddCardToDeck(CardData newCard)
    {
        if (newCard == null)
        {
            Debug.LogWarning("AddCardToDeck: 试图添加 null 卡牌，已忽略。");
            return;
        }
        _cardPool.Add(newCard);
        Debug.Log($"添加卡牌到牌库：{newCard.CardName}");
    }

    // 获取牌库剩余卡牌数
    public int GetRemainingCards() => _cardPool.Count;
}