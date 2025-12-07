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

    // ������õ�ͳһ�ӿڣ�ִ�д���Ŀ���Ч��
    public void ExecuteEffect(CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogWarning("ExecuteEffect: cardData Ϊ null");
            return;
        }

        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("ExecuteEffect: GameManager.Instance Ϊ null���޷�ִ�п���Ч����");
            return;
        }

        GameObject player = gm.Player;
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
            if (player != null)
                Debug.LogWarning("ExecuteEffect: GameManager.Player δ���ã���ͨ�� Tag �ҵ���Ҷ�����ʱ���ˣ������� GameManager Inspector ��ֵ Player �ֶΡ�");
            else
            {
                Debug.LogError("ExecuteEffect: δ�ҵ� Player��GameManager.Player Ϊ null���ҳ������� Tag Ϊ \"Player\" �Ķ���");
                return;
            }
        }

        // ��ϣ���ӡ��Һ����״̬�����ڵ��ԣ�
        var bpm = player.GetComponent<BallParameterManager>();
        var health = player.GetComponent<HealthSystem_New>();
        var speedComp = player.GetComponent<SpeedAndSize>();

        Debug.Log($"ExecuteEffect: Ŀ�����={player.name} | BallParam={(bpm != null)} | Health={(health != null)} | SpeedAndSize={(speedComp != null)} | effect={cardData.effectType}");

        switch (cardData.effectType)
        {
            case CardData.CardEffect.AddHealth:
                if (health != null)
                {
                    int newHp = health.GetCurrentHealth() + (int)cardData.effectValue;
                    health.SetCurrentHealth(newHp);
                    Debug.Log($"AddHealth: �����������Ϊ {newHp}���� {cardData.effectValue} ���ӣ�");
                    // ͬ����ʾ�� BallParameterManager �� inspector ֵ��������ڣ�
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
                    Debug.Log($"ReduceHealth: �����������Ϊ {newHp}������ {cardData.effectValue}��");
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
                    Debug.Log($"EnlargeBodytype: Ŀ������ -> {target}");
                }
                break;

            case CardData.CardEffect.ShrinkBodytype:
                {
                    float cur = player.transform.localScale.x;
                    float target = Mathf.Max(0.1f, cur - cardData.effectValue);
                    if (bpm != null) bpm.BallSize = target;
                    else player.transform.localScale = Vector3.one * target;
                    Debug.Log($"ShrinkBodytype: Ŀ������ -> {target}");
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
                Debug.Log($"δ����Ŀ���Ч����{cardData.effectType}");
                break;
        }
    }

    private void AddCurrentCost(int amount)
    {
        var gm = GameManager.Instance;
        if (gm == null) return;
        gm.currentCost = Mathf.Min(gm.currentCost + amount, gm.maxCost);
        Debug.Log($"��ǰ��������{amount}��ʣ�ࣺ{gm.currentCost}");
        gm.UpdateCostUI();
    }

    private void IncreaseMaxCost(int amount)
    {
        var gm = GameManager.Instance;
        if (gm == null) return;
        gm.maxCost += amount;
        gm.currentCost += amount;
        Debug.Log($"��������������{amount}����ǰ���{gm.maxCost}");
        gm.UpdateCostUI();
    }

    public void ExecuteDrawCardEffect(int count)
    {
        const int MaxDrawPerEffect = 10;
        if (count <= 0) return;
        if (count > MaxDrawPerEffect)
        {
            Debug.LogWarning($"ExecuteDrawCardEffect: �����ȡ {count} �ţ�������Ϊ {MaxDrawPerEffect} �š�");
            count = MaxDrawPerEffect;
        }

        if (Deck.Instance == null)
        {
            Debug.LogError("ExecuteDrawCardEffect: Deck.Instance Ϊ null");
            return;
        }
        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("ExecuteDrawCardEffect: GameManager.Instance Ϊ null");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Card drawnCard = Deck.Instance.DrawCard();
            if (drawnCard == null)
            {
                Debug.Log("������������ʣ�������� DrawCard ���� null��ֹͣ�鿨");
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
                Debug.LogWarning("ExecuteDrawCardEffect: �� Hand �� handTransform�����ƽ����ֳ������ڵ㡣");
            }
        }
    }

    private IEnumerator TemporaryInvincible(float duration)
    {
        Debug.Log($"ʹ����޵� {duration} �루ռλ��");
        yield return new WaitForSeconds(duration);
        Debug.Log("�޵н�����ռλ��");
    }
}