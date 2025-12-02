using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card/Card Data")]
public class CardData : ScriptableObject
{
    // 卡牌名称
    public string CardName;
    // 卡牌描述
    public string Description;
    // 法力消耗
    public int Cost;
    // 卡牌类型（攻击/技能/能力/消耗）
    public CardType cardType;
    // 卡牌精灵（显示用）
    public Sprite CardSprite;
    // 体型数值
    public int Bodytype;
    // 硬度数值
    public int Hardness;

    public enum CardType
    {
        Bodytype,
        Hardness,
        Item//道具
    }
}