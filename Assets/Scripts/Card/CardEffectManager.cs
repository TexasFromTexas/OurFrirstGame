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

        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("ExecuteEffect: GameManager.Instance 为 null，无法执行卡牌效果。");
            return;
        }

        GameObject player = gm.Player;
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
            if (player != null)
                Debug.LogWarning("ExecuteEffect: GameManager.Player 未设置，已通过 Tag 找到玩家对象（临时回退）。请在 GameManager Inspector 赋值 Player 字段。");
            else
            {
                Debug.LogError("ExecuteEffect: 未找到 Player（GameManager.Player 为 null，且场景中无 Tag 为 \"Player\" 的对象）");
                return;
            }
        }

        // 诊断：打印玩家和组件状态（便于调试）
        var bpm = player.GetComponent<BallParameterManager>();
        var health = player.GetComponent<HealthSystem_New>();
        var speedComp = player.GetComponent<SpeedAndSize>();

        Debug.Log($"ExecuteEffect: 目标玩家={player.name} | BallParam={(bpm!=null)} | Health={(health!=null)} | SpeedAndSize={(speedComp!=null)} | effect={cardData.effectType}");

        switch (cardData.effectType)
        {
            case CardData.CardEffect.AddHealth:
                if (health != null)
                {
                    int newHp = health.GetCurrentHealth() + (int)cardData.effectValue;
                    health.SetCurrentHealth(newHp);
                    Debug.Log($"AddHealth: 将玩家生命设为 {newHp}（由 {cardData.effectValue} 增加）");
                    // 同步显示到 BallParameterManager 的 inspector 值（如果存在）
                    if (bpm != null) bpm.CurrentHealth = health.GetCurrentHealth();
                }
                else if (bpm != null)
                {
                    bpm.CurrentHealth += (int)cardData.effectValue;
                    Debug.Log($"AddHealth (fallback via BPM): bpm.CurrentHealth -> {bpm.CurrentHealth}");
                }
                break;

            case CardData.CardEffect.ReduceHealth:
                if (health != null)
                {
                    int newHp = health.GetCurrentHealth() - (int)cardData.effectValue;
                    health.SetCurrentHealth(newHp);
                    Debug.Log($"ReduceHealth: 将玩家生命设为 {newHp}（减少 {cardData.effectValue}）");
                    if (bpm != null) bpm.CurrentHealth = health.GetCurrentHealth();
                }
                else if (bpm != null)
                {
                    bpm.CurrentHealth -= (int)cardData.effectValue;
                    Debug.Log($"ReduceHealth (fallback via BPM): bpm.CurrentHealth -> {bpm.CurrentHealth}");
                }
                break;

            case CardData.CardEffect.EnlargeBodytype:
                {
                    float cur = player.transform.localScale.x;
                    float target = Mathf.Max(0.1f, cur + cardData.effectValue);
                    if (bpm != null) bpm.BallSize = target;
                    else player.transform.localScale = Vector3.one * target;
                    Debug.Log($"EnlargeBodytype: 目标体型 -> {target}");
                }
                break;

            case CardData.CardEffect.ShrinkBodytype:
                {
                    float cur = player.transform.localScale.x;
                    float target = Mathf.Max(0.1f, cur - cardData.effectValue);
                    if (bpm != null) bpm.BallSize = target;
                    else player.transform.localScale = Vector3.one * target;
                    Debug.Log($"ShrinkBodytype: 目标体型 -> {target}");
                }
                break;

            case CardData.CardEffect.IncreaseSpeed:
                if (speedComp != null)
                {
                    speedComp.ModifyDamageMultiplier(cardData.effectValue);
                    Debug.Log($"IncreaseSpeed: DamageMultiplier -> {speedComp.DamageMultiplier}");
                }
                break;

            case CardData.CardEffect.DecreaseSpeed:
                if (speedComp != null)
                {
                    speedComp.ModifyDamageMultiplier(-cardData.effectValue);
                    Debug.Log($"DecreaseSpeed: DamageMultiplier -> {speedComp.DamageMultiplier}");
                }
                break;

            case CardData.CardEffect.IncreaseMaxCost:
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
        var gm = GameManager.Instance;
        if (gm == null) return;
        gm.currentCost = Mathf.Min(gm.currentCost + amount, gm.maxCost);
        Debug.Log($"当前费用增加{amount}，剩余：{gm.currentCost}");
        gm.UpdateCostUI();
    }

    private void IncreaseMaxCost(int amount)
    {
        var gm = GameManager.Instance;
        if (gm == null) return;
        gm.maxCost += amount;
        gm.currentCost += amount;
        Debug.Log($"最大费用永久增加{amount}，当前最大：{gm.maxCost}");
        gm.UpdateCostUI();
    }

    public void ExecuteDrawCardEffect(int count)
    {
        const int MaxDrawPerEffect = 10;
        if (count <= 0) return;
        if (count > MaxDrawPerEffect)
        {
            Debug.LogWarning($"ExecuteDrawCardEffect: 请求抽取 {count} 张，已限制为 {MaxDrawPerEffect} 张。");
            count = MaxDrawPerEffect;
        }

        if (Deck.Instance == null)
        {
            Debug.LogError("ExecuteDrawCardEffect: Deck.Instance 为 null");
            return;
        }
        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("ExecuteDrawCardEffect: GameManager.Instance 为 null");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Card drawnCard = Deck.Instance.DrawCard();
            if (drawnCard == null)
            {
                Debug.Log("抽牌数量超过剩余牌数或 DrawCard 返回 null，停止抽卡");
                break;
            }

            if (gm.Hand != null)
            {
                gm.Hand.AddCard(drawnCard);
            }
            else if (gm.handTransform != null)
            {
                drawnCard.transform.SetParent(gm.handTransform, false);
                drawnCard.transform.localScale = Vector3.one;
            }
            else
            {
                Debug.LogWarning("ExecuteDrawCardEffect: 无 Hand 和 handTransform，卡牌将保持场景根节点。");
            }
        }
    }

    private IEnumerator TemporaryInvincible(float duration)
    {
        Debug.Log($"使玩家无敌 {duration} 秒（占位）");
        yield return new WaitForSeconds(duration);
        Debug.Log("无敌结束（占位）");
    }
}