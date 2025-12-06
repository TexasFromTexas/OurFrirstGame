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
    // 卡牌类型
    public CardType cardType;
    // 卡牌精灵（显示用）
    public Sprite CardSprite;
    // 效果类型（枚举）
    public CardEffect effectType; 
    // 效果数值（如+50生命值、-20速度）
    public float effectValue; 
    // 效果持续时间（如无敌持续3秒，0表示永久）
    public float effectDuration; 

    public enum CardType
    {
        Buff,
        Cost,
        Item//道具
    }
    public enum CardEffect
    {
        None,               // 无效果（默认）
        AddHealth,          // 增加生命值
        ReduceHealth,       // 减少生命值
        IncreaseSpeed,      // 增加移动速度
        DecreaseSpeed,      // 降低移动速度
        EnlargeBodytype,    // 增大体积
        ShrinkBodytype,     // 缩小体积
        AddCurrentCost,     // 增加费用
        IncreaseMaxCost,    // 增加最大费用
        DrawCards           // 抽取卡牌  
    }
}