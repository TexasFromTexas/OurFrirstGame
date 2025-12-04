using UnityEngine;

/// <summary>
/// 速度+半径双因素伤害脚本：半径每多1，多造成0.5伤害，内置调试半径
/// 挂载到玩家/敌人小球上，兼容现有HealthSystem_New
/// </summary>
public class SpeedAndSize : MonoBehaviour
{
    [Header("基础伤害配置")]
    [SerializeField] private float damageMultiplier = 0.5f; // 速度系数
    [SerializeField] private int baseDamage = 1; // 保底伤害

    [Header("半径伤害配置")]
    [SerializeField] private float radiusDamageMultiplier = 0.5f; // 半径每差1，伤害变化量（可调整）

    [Header("调试半径（仅调试用）")]
    [SerializeField] private bool debugMode = false; // 开启调试模式（使用自定义半径）
    [SerializeField] private float debugRadius = 1f; // 调试用的自定义半径（可在Inspector实时调整）

    [Header("目标过滤")]
    [SerializeField] private bool damagePlayerOnly = false;
    [SerializeField] private bool damageEnemyOnly = false;

    private Rigidbody2D rb;
    private Round roundManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        roundManager = FindObjectOfType<Round>();

        // 自动设置过滤
        if (gameObject.CompareTag("Player"))
        {
            damageEnemyOnly = true;
        }
        else if (gameObject.CompareTag("Enemy"))
        {
            damagePlayerOnly = true;
        }

        Debug.Log($"【速度半径伤害】{gameObject.name} 初始化完成 | 速度系数：{damageMultiplier} | 半径系数：{radiusDamageMultiplier}");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. 检查刚体
        if (rb == null) return;

        // 2. 检查回合
        if (roundManager != null)
        {
            bool isPlayerTurn = roundManager.currentTurnState == Round.TurnState.BallRound;
            bool isEnemyTurn = roundManager.currentTurnState == Round.TurnState.EnemyRound;

            if ((gameObject.CompareTag("Player") && !isPlayerTurn) ||
                (gameObject.CompareTag("Enemy") && !isEnemyTurn))
            {
                return;
            }
        }

        // 3. 目标过滤
        string targetTag = collision.gameObject.tag;
        bool isTargetPlayer = targetTag == "Player";
        bool isTargetEnemy = targetTag == "Enemy";

        if (damagePlayerOnly && damageEnemyOnly)
        {
            // 不过滤
        }
        else if (damagePlayerOnly && !isTargetPlayer)
        {
            return;
        }
        else if (damageEnemyOnly && !isTargetEnemy)
        {
            return;
        }

        // 4. 获取目标血量组件
        HealthSystem_New targetHealth = collision.gameObject.GetComponent<HealthSystem_New>();
        if (targetHealth == null) return;

        // 5. 计算速度伤害
        float currentSpeed = rb.velocity.magnitude;
        float speedDamage = baseDamage + currentSpeed * damageMultiplier;

        // 6. 计算半径伤害加成（核心功能）
        float myRadius = GetMyRadius(); // 获取自身半径（调试模式用自定义值，否则用碰撞体实际半径）
        float targetRadius = GetTargetRadius(collision.gameObject); // 获取对方半径
        float radiusDiff = myRadius - targetRadius; // 半径差值（我-对方）
        float radiusBonus = radiusDiff * radiusDamageMultiplier; // 半径伤害加成（差值×系数）

        // 7. 总伤害 = 速度伤害 + 半径加成，四舍五入
        int totalDamage = Mathf.RoundToInt(speedDamage + radiusBonus);
        totalDamage = Mathf.Max(1, totalDamage); // 确保最低1点伤害

        // 8. 执行扣血
        targetHealth.TakeDamage(totalDamage);

        // 调试日志
        Debug.Log($"✅【速度半径伤害】{gameObject.name} 碰撞 {collision.gameObject.name}！");
        Debug.Log($"→ 速度：{currentSpeed:F2} | 速度伤害：{speedDamage:F2}");
        Debug.Log($"→ 我的半径：{myRadius:F2} | 对方半径：{targetRadius:F2} | 半径差：{radiusDiff:F2} | 半径加成：{radiusBonus:F2}");
        Debug.Log($"→ 总伤害：{totalDamage} | 目标剩余血量：{targetHealth.GetCurrentHealth()}/{targetHealth.GetMaxHealth()}");
    }

    /// <summary>
    /// 获取自身半径（调试模式用自定义值，否则用碰撞体实际半径）
    /// </summary>
    private float GetMyRadius()
    {
        if (debugMode)
        {
            return debugRadius; // 调试模式返回自定义半径
        }

        // 非调试模式，获取碰撞体实际半径
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            return circleCollider.radius;
        }

        // 如果不是CircleCollider2D，取BoxCollider2D的平均边长作为半径
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            float avgSize = (boxCollider.size.x + boxCollider.size.y) / 4f; // 平均边长的1/4作为半径
            return avgSize;
        }

        Debug.LogWarning($"【速度半径伤害】{gameObject.name} 没有CircleCollider2D或BoxCollider2D，使用默认半径1！");
        return 1f;
    }

    /// <summary>
    /// 获取目标半径
    /// </summary>
    private float GetTargetRadius(GameObject target)
    {
        CircleCollider2D circleCollider = target.GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            return circleCollider.radius;
        }

        BoxCollider2D boxCollider = target.GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            float avgSize = (boxCollider.size.x + boxCollider.size.y) / 4f;
            return avgSize;
        }

        return 1f; // 目标没有碰撞体，使用默认半径1
    }

    /// <summary>
    /// （可选）Gizmos绘制调试半径（场景视图可视化）
    /// </summary>
    private void OnDrawGizmos()
    {
        if (debugMode)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, debugRadius); // 绘制调试半径
        }
    }
}