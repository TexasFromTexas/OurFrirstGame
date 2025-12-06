using System.Collections;
using UnityEngine;

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

        var playerBall = GameManager.Instance?.PlayerBall;
        if (playerBall == null)
        {
            Debug.LogError("ExecuteEffect: 未找到 PlayerBall（GameManager.Instance.PlayerBall 为 null）");
            return;
        }

        var bpm = playerBall.GetComponent<BallParameterManager>();
        var health = playerBall.GetComponent<HealthSystem_New>();
        var speedComp = playerBall.GetComponent<SpeedAndSize>();

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
                    float cur = playerBall.transform.localScale.x;
                    float target = Mathf.Max(0.1f, cur + cardData.effectValue);
                    if (bpm != null) bpm.BallSize = target;
                }
                break;

            case CardData.CardEffect.ShrinkBodytype:
                {
                    float cur = playerBall.transform.localScale.x;
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

            case CardData.CardEffect.MakeInvincible:
                StartCoroutine(TemporaryInvincible(playerBall, cardData.effectDuration));
                break;

            case CardData.CardEffect.None:
            default:
                Debug.Log($"未处理的卡牌效果：{cardData.effectType}");
                break;
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