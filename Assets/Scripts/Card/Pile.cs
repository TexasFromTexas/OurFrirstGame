using System.Collections.Generic;
using UnityEngine;

public class Pile : MonoBehaviour
{
    // 存储卡牌数据（仅保留数据，UI实例已销毁）
    private List<CardData> _pileData = new List<CardData>();

    // 添加卡牌到堆中（接收Card实例，提取数据）
    public void AddCard(Card card)
    {
        _pileData.Add(card.GetCardData());
        Debug.Log($"卡牌加入{gameObject.name}：{card.GetCardData().CardName}");
    }

    // 获取堆中所有卡牌数据
    public List<CardData> GetAllCards() => new List<CardData>(_pileData);

    // 清空堆
    public void ClearPile() => _pileData.Clear();

    // 获取堆中卡牌数量
    public int GetPileSize() => _pileData.Count;
}