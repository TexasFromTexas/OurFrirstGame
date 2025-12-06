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
            Debug.LogError("ExecuteEffect: 未找到 Player（GameManager.Instance.Player 为 null）");
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
        if (GameManager.Instance == null) return;
        GameManager.Instance.currentCost += amount;
        // 确保不超过最大费用
        GameManager.Instance.currentCost = Mathf.Min(GameManager.Instance.currentCost, GameManager.Instance.maxCost);
        Debug.Log($"当前费用增加{amount}，剩余：{GameManager.Instance.currentCost}");
        GameManager.Instance.UpdateCostUI(); // 刷新费用UI
    }
    private void IncreaseMaxCost(int amount)
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.maxCost += amount;
        // 同步更新当前费用（可选：最大费用增加时，当前费用也同步增加）
        GameManager.Instance.currentCost += amount;
        Debug.Log($"最大费用永久增加{amount}，当前最大：{GameManager.Instance.maxCost}");
        GameManager.Instance.UpdateCostUI(); // 刷新费用UI
    }
    public void ExecuteDrawCardEffect(int count)
    {
        // 防止被滥用/一次性创建大量对象导致崩溃：限制上限
        const int MaxDrawPerEffect = 10;
        if (count <= 0) return;
        if (count > MaxDrawPerEffect)
        {
            Debug.LogWarning($"ExecuteDrawCardEffect: 请求抽取 {count} 张，已限制为 {MaxDrawPerEffect} 张以防止瞬时大量实例化。");
            count = MaxDrawPerEffect;
        }

        if (Deck.Instance == null)
        {
            Debug.LogError("ExecuteDrawCardEffect: Deck.Instance 为 null");
            return;
        }
        if (GameManager.Instance == null)
        {
            Debug.LogError("ExecuteDrawCardEffect: GameManager.Instance 为 null");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Card drawnCard = Deck.Instance.DrawCard();
            if (drawnCard != null)
            {
                // 优先通过 Hand 的接口添加卡牌（Hand 会处理父对象、排列等）
                if (GameManager.Instance.Hand != null)
                {
                    GameManager.Instance.Hand.AddCard(drawnCard);
                }
                else
                {
                    // 兜底：把卡牌放到 handTransform（若存在）
                    if (GameManager.Instance.handTransform != null)
                    {
                        drawnCard.transform.SetParent(GameManager.Instance.handTransform, false);
                        drawnCard.transform.localScale = Vector3.one;
                    }
                    else
                    {
                        Debug.LogWarning("ExecuteDrawCardEffect: 无 Hand 和 handTransform，卡牌将保持场景根节点。");
                    }
                }
            }
            else
            {
                Debug.Log("抽牌数量超过剩余牌数或 DrawCard 返回 null，停止抽卡");
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