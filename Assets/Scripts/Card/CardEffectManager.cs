using System.Collections;
using UnityEngine;
using static CardData;

public class CardEffectManager : MonoBehaviour
{
    public static CardEffectManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 对外调用的统一接口：执行传入的卡牌效果
    public void ExecuteEffect(CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogWarning("ExecuteEffect: cardData 为 null");
            return;
        }

        var player = GameManager.Instance?.Player;
        if (player == null)
        {
            Debug.LogError("ExecuteEffect: 未找到 Player（GameManager.Instance.PlayerBall 为 null）");
            return;
        }

        var bpm = player.GetComponent<BallParameterManager>();
        var health = player.GetComponent<HealthSystem_New>();
        var speedComp = player.GetComponent<SpeedAndSize>();

        switch (cardData.effectType)
        {
            case CardData.CardEffect.AddHealth:
                if (bpm != null && health != null)
                    bpm.CurrentHealth = health.GetCurrentHealth() + (int)cardData.effectValue;
                break;

            case CardData.CardEffect.ReduceHealth:
                if (bpm != null && health != null)
                    bpm.CurrentHealth = health.GetCurrentHealth() - (int)cardData.effectValue;
                break;

            case CardData.CardEffect.EnlargeBodytype:
                {
                    // 将 effectValue 视为大小增量（与 transform.localScale 单位一致）
                    float cur = player.transform.localScale.x;
                    float target = Mathf.Max(0.1f, cur + cardData.effectValue);
                    if (bpm != null) bpm.BallSize = target;
                }
                break;

            case CardData.CardEffect.ShrinkBodytype:
                {
                    float cur = player.transform.localScale.x;
                    float target = Mathf.Max(0.1f, cur - cardData.effectValue);
                    if (bpm != null) bpm.BallSize = target;
                }
                break;

            case CardData.CardEffect.IncreaseSpeed:
                if (speedComp != null)
                {
                    // 根据 SpeedAndSize 的真实 API 调整，这里示例直接修改 damageMultiplier
                    speedComp.damageMultiplier += cardData.effectValue;
                }
                break;

            case CardData.CardEffect.DecreaseSpeed:
                if (speedComp != null)
                {
                    speedComp.damageMultiplier = Mathf.Max(0f, speedComp.damageMultiplier - cardData.effectValue);
                }
                break;

            case CardEffect.IncreaseMaxCost:
                IncreaseMaxCost((int)cardData.effectValue);
                break;

            case CardData.CardEffect.AddCurrentCost:
                AddCurrentCost((int)cardData.effectValue);
                break;
            case CardData.CardEffect.DrawCards:
                ExecuteDrawCardEffect((int)cardData.effectValue);
                break;

            case CardData.CardEffect.None:
            default:
                Debug.Log($"未处理的卡牌效果：{cardData.effectType}");
                break;
        }
    }
    private void AddCurrentCost(int amount)
    {
        GameManager.Instance.currentCost += amount;
        // 确保不超过最大费用
        GameManager.Instance.currentCost = Mathf.Min(GameManager.Instance.currentCost, GameManager.Instance.maxCost);
        Debug.Log($"当前费用增加{amount}，剩余：{GameManager.Instance.currentCost}");
        GameManager.Instance.UpdateCostUI(); // 刷新费用UI
    }
    private void IncreaseMaxCost(int amount)
    {
        GameManager.Instance.maxCost += amount;
        // 同步更新当前费用（可选：最大费用增加时，当前费用也同步增加）
        GameManager.Instance.currentCost += amount;
        Debug.Log($"最大费用永久增加{amount}，当前最大：{GameManager.Instance.maxCost}");
        GameManager.Instance.UpdateCostUI(); // 刷新费用UI
    }
    public void ExecuteDrawCardEffect(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Card drawnCard = Deck.Instance.DrawCard();
            if (drawnCard != null)
            {
                // 加入手牌容器
                drawnCard.transform.SetParent(GameManager.Instance.HandPanel);
                drawnCard.transform.localScale = Vector3.one;
                drawnCard.transform.localPosition = Vector3.zero;
            }
            else
            {
                Debug.Log("抽牌数量超过剩余牌数，停止抽卡");
                break;
            }
        }
    }

    private IEnumerator TemporaryInvincible(GameObject player, float duration)
    {
        // 占位：如果玩家有无敌接口，在此调用
        Debug.Log($"使玩家无敌 {duration} 秒（占位）");
        yield return new WaitForSeconds(duration);
        Debug.Log("无敌结束（占位）");
    }
}