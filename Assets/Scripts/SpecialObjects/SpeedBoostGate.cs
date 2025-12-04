using UnityEngine;

/// <summary>
/// 加速门触发器：球穿过时按当前速度倍数加速，完全独立不修改原有脚本
/// 挂载到加速门物体上，需给加速门添加Collider2D并勾选Is Trigger
/// </summary>
public class SpeedBoostGate : MonoBehaviour
{
    [Header("加速配置")]
    [SerializeField] private float speedMultiplier = 2f; // 加速倍数（当前速度×该值）
    [SerializeField] private float boostDuration = 0f; // 加速持续时间（0=永久加速）
    [SerializeField] private float triggerCooldown = 0.5f; // 触发冷却（防止重复加速）

    [Header("目标过滤")]
    [SerializeField] private bool boostPlayerOnly = false; // 仅加速玩家球
    [SerializeField] private bool boostEnemyOnly = false; // 仅加速敌人球

    private float lastTriggerTime; // 上次触发时间（防重复）

    private void Awake()
    {
        // 验证加速门碰撞体配置
        Collider2D gateCollider = GetComponent<Collider2D>();
        if (gateCollider == null)
        {
            Debug.LogError($"【加速门】{gameObject.name} 未添加Collider2D组件！");
            // 自动添加BoxCollider2D（兜底）
            gateCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        // 强制设置为触发器（穿过门需要Trigger）
        gateCollider.isTrigger = true;
        Debug.Log($"【加速门】{gameObject.name} 初始化完成，加速倍数：{speedMultiplier}");
    }

    /// <summary>
    /// 触发器核心逻辑：球穿过时触发加速
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 冷却检查：防止短时间重复触发
        if (Time.time - lastTriggerTime < triggerCooldown)
        {
            Debug.Log($"【加速门】{gameObject.name} 触发冷却中，跳过加速");
            return;
        }

        // 2. 获取球的刚体（必须有Rigidbody2D才生效）
        Rigidbody2D ballRb = other.GetComponent<Rigidbody2D>();
        if (ballRb == null)
        {
            Debug.LogWarning($"【加速门】{gameObject.name} 检测到非刚体对象：{other.gameObject.name}，跳过");
            return;
        }

        // 3. 目标过滤：仅加速指定类型的球
        string ballTag = other.gameObject.tag;
        bool isPlayerBall = ballTag == "Player";
        bool isEnemyBall = ballTag == "Enemy";

        // 过滤逻辑：同时勾选仅玩家/仅敌人时，默认仅玩家
        if (boostPlayerOnly && !isPlayerBall)
        {
            Debug.Log($"【加速门】{gameObject.name} 仅加速玩家球，跳过敌人球：{other.gameObject.name}");
            return;
        }
        if (boostEnemyOnly && !isEnemyBall)
        {
            Debug.Log($"【加速门】{gameObject.name} 仅加速敌人球，跳过玩家球：{other.gameObject.name}");
            return;
        }

        // 4. 执行加速逻辑
        BoostBallSpeed(ballRb, other.gameObject.name);
        lastTriggerTime = Time.time; // 更新触发时间
    }

    /// <summary>
    /// 核心加速方法：给球施加当前速度倍数的速度
    /// </summary>
    private void BoostBallSpeed(Rigidbody2D rb, string ballName)
    {
        // 保存原速度（用于日志）
        Vector2 originalVelocity = rb.velocity;
        float originalSpeed = originalVelocity.magnitude;

        // 计算新速度：当前速度 × 加速倍数
        Vector2 newVelocity = originalVelocity * speedMultiplier;
        rb.velocity = newVelocity;

        // 调试日志
        Debug.Log($"【加速门】{gameObject.name} 加速成功！");
        Debug.Log($"→ 目标球：{ballName}");
        Debug.Log($"→ 原速度：{originalSpeed:F2} | 新速度：{newVelocity.magnitude:F2}（×{speedMultiplier}）");

        // 可选：加速持续时间（到时间恢复原速度）
        if (boostDuration > 0)
        {
            Invoke(nameof(ResetBallSpeed), boostDuration);
            void ResetBallSpeed()
            {
                rb.velocity = originalVelocity;
                Debug.Log($"【加速门】{gameObject.name} 加速结束，{ballName} 恢复原速度：{originalSpeed:F2}");
            }
        }
    }

    // 可选：Gizmos绘制加速门范围（场景视图可视化）
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.DrawWireCube(transform.position, collider.bounds.size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(2, 1, 0)); // 默认大小
        }
    }
}