using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EmotionController : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private Image emotionImage;

    [Header("玩家目标引用")]
    [SerializeField] private HealthSystem_New playerHealth;

    [Header("表情配置")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hitSprite;
    [SerializeField] private Sprite attackSuccessSprite;
    [SerializeField] private float emotionRecoverTime = 0.5f; // 表情恢复时间（秒）

    // 调试开关：开启后输出详细日志，方便定位问题
    [Header("调试选项")]
    [SerializeField] private bool enableDebugLog = true;

    private int lastPlayerHealth;
    private Dictionary<HealthSystem_New, int> enemyHealthRecords = new Dictionary<HealthSystem_New, int>();

    private enum EmotionState { Normal, Hit, AttackSuccess }
    private EmotionState currentState = EmotionState.Normal;
    private float stateTimer;

    private void Start()
    {
        // 1. 检查必要引用
        if (emotionImage == null)
        {
            Debug.LogError("【表情控制器】未赋值Emotion Image！");
            return;
        }
        if (playerHealth == null)
        {
            Debug.LogError("【表情控制器】未赋值Player Health！");
            return;
        }
        if (normalSprite == null)
        {
            Debug.LogError("【表情控制器】未赋值Normal Sprite！");
            return;
        }

        // 2. 初始化状态
        emotionImage.sprite = normalSprite;
        lastPlayerHealth = playerHealth.GetCurrentHealth();
        UpdateEnemyHealthRecords();

        if (enableDebugLog)
            Debug.Log("【表情控制器】初始化完成，当前状态：Normal");
    }

    private void Update()
    {
        if (playerHealth == null) return;

        UpdateEnemyHealthRecords();
        CheckPlayerHit();
        CheckEnemyHit();
        UpdateEmotionState(); // 关键：每帧更新表情状态（恢复逻辑）
    }

    /// <summary>
    /// 更新敌人血量记录
    /// </summary>
    private void UpdateEnemyHealthRecords()
    {
        HealthSystem_New[] allHealthSystems = FindObjectsOfType<HealthSystem_New>();

        // 添加新敌人
        foreach (HealthSystem_New health in allHealthSystems)
        {
            if (health.gameObject.CompareTag("Player")) continue;
            if (!enemyHealthRecords.ContainsKey(health))
            {
                enemyHealthRecords.Add(health, health.GetCurrentHealth());
                if (enableDebugLog)
                    Debug.Log($"【表情控制器】添加新敌人：{health.gameObject.name}");
            }
        }

        // 移除已销毁敌人
        List<HealthSystem_New> enemiesToRemove = new List<HealthSystem_New>();
        foreach (var pair in enemyHealthRecords)
        {
            if (pair.Key == null || pair.Key.gameObject == null)
            {
                enemiesToRemove.Add(pair.Key);
            }
        }
        foreach (var enemy in enemiesToRemove)
        {
            enemyHealthRecords.Remove(enemy);
            if (enableDebugLog)
                Debug.Log($"【表情控制器】移除已销毁敌人");
        }
    }

    /// <summary>
    /// 检测玩家受击
    /// </summary>
    private void CheckPlayerHit()
    {
        int currentPlayerHealth = playerHealth.GetCurrentHealth();
        if (currentPlayerHealth < lastPlayerHealth && currentPlayerHealth > 0)
        {
            SetEmotionState(EmotionState.Hit);
        }
        lastPlayerHealth = currentPlayerHealth;
    }

    /// <summary>
    /// 检测攻击敌人成功
    /// </summary>
    private void CheckEnemyHit()
    {
        bool isAttackSuccess = false;

        // 1. 检测是否有敌人被攻击
        foreach (var pair in enemyHealthRecords)
        {
            HealthSystem_New enemyHealth = pair.Key;
            if (enemyHealth == null) continue;

            int currentHealth = enemyHealth.GetCurrentHealth();
            int lastHealth = pair.Value;

            if (currentHealth < lastHealth && currentHealth > 0)
            {
                isAttackSuccess = true;
                break; // 优化：找到一个受击敌人即可跳出
            }
        }

        // 2. 攻击成功时切换表情
        if (isAttackSuccess)
        {
            SetEmotionState(EmotionState.AttackSuccess);
        }

        // 3. 更新所有敌人的血量记录
        foreach (var pair in enemyHealthRecords)
        {
            HealthSystem_New enemyHealth = pair.Key;
            if (enemyHealth == null) continue;
            enemyHealthRecords[enemyHealth] = enemyHealth.GetCurrentHealth();
        }
    }

    /// <summary>
    /// 设置表情状态（简化逻辑，确保恢复不受影响）
    /// </summary>
    private void SetEmotionState(EmotionState newState)
    {
        // 无论当前状态如何，都允许切换（简化优先级逻辑）
        currentState = newState;
        stateTimer = emotionRecoverTime; // 强制重置恢复计时器

        // 切换表情图片
        switch (currentState)
        {
            case EmotionState.Hit:
                emotionImage.sprite = hitSprite;
                if (enableDebugLog)
                    Debug.Log($"【表情控制器】切换到受击表情，恢复时间：{emotionRecoverTime}秒");
                break;
            case EmotionState.AttackSuccess:
                emotionImage.sprite = attackSuccessSprite;
                if (enableDebugLog)
                    Debug.Log($"【表情控制器】切换到攻击成功表情，恢复时间：{emotionRecoverTime}秒");
                break;
        }
    }

    /// <summary>
    /// 更新表情状态（核心恢复逻辑，添加详细日志）
    /// </summary>
    private void UpdateEmotionState()
    {
        if (currentState != EmotionState.Normal)
        {
            // 递减计时器
            stateTimer -= Time.deltaTime;
            if (enableDebugLog)
                Debug.Log($"【表情控制器】当前状态：{currentState}，剩余恢复时间：{stateTimer:F2}秒");

            // 计时器归零时恢复正常表情
            if (stateTimer <= 0)
            {
                currentState = EmotionState.Normal;
                emotionImage.sprite = normalSprite;
                if (enableDebugLog)
                    Debug.Log("【表情控制器】恢复正常表情");
            }
        }
    }
}