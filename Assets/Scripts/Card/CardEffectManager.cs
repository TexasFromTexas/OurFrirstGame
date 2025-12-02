using Unity.VisualScripting;
using UnityEngine;

public class CardEffectManager : MonoBehaviour
{
    public static CardEffectManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // 执行卡牌效果
    public void ExecuteEffect(CardData cardData)
    {
        switch (cardData.cardType)
        {
            case CardData.CardType.Bodytype:
                Debug.Log($"增加{cardData.Bodytype}点体型！");
                // 这里可以添加球体型变大函数
                break;
            case CardData.CardType.Hardness:
                Debug.Log($"获得{cardData.Hardness}点硬度！");
                // 这里可以添加球硬度增加函数
                break;
            case CardData.CardType.Item:
                Debug.Log("激活道具效果");
                // 这里可以添加道具实现函数
                break;
        }
    }
}