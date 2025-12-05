using UnityEngine;

/// <summary>
/// 速度+半径双因素伤害脚本：考虑物体缩放，获取实际碰撞半径
/// </summary>
public class SpeedAndSize : MonoBehaviour
{
    [Header("基础伤害配置")]
    [SerializeField] public float damageMultiplier = 0.5f; // 速度系数
    [SerializeField] private int baseDamage = 1; // 保底伤害
                                                 // 在SpeedAndSize脚本中添加
    [Header("最低攻击力")]
    public int minDamage = 1; // 公开字段，允许外部修改

    [Header("半径伤害配置")]
    [SerializeField] private float radiusDamageMultiplier = 0.5f; // 半径每差1，伤害变化量

    [Header("调试半径（仅调试用）")]
    [SerializeField] private bool debugMode = false; // 开启调试模式（使用自定义半径）
    [SerializeField] private float debugRadius = 1f; // 调试用的自定义半径

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

        // 6. 计算半径伤害加成（核心修改：获取实际碰撞半径，考虑缩放）
        float myRadius = GetMyRadius(); // 获取自身实际碰撞半径（考虑缩放）
        float targetRadius = GetTargetRadius(collision.gameObject); // 获取目标实际碰撞半径（考虑缩放）
        float radiusDiff = myRadius - targetRadius; // 半径差值（我-对方）
        float radiusBonus = radiusDiff * radiusDamageMultiplier; // 半径伤害加成

        // 7. 总伤害 = 速度伤害 + 半径加成，四舍五入
        int totalDamage = Mathf.RoundToInt(speedDamage + radiusBonus);
        totalDamage = Mathf.Max(minDamage, totalDamage); // 使用修改后的minDamage

        // 8. 执行扣血
        targetHealth.TakeDamage(totalDamage);

        // 调试日志（新增：显示实际半径和缩放）
        Debug.Log($"✅【速度半径伤害】{gameObject.name} 碰撞 {collision.gameObject.name}！");
        Debug.Log($"→ 速度：{currentSpeed:F2} | 速度伤害：{speedDamage:F2}");
       
       
        Debug.Log($"→ 目标剩余血量：{targetHealth.GetCurrentHealth()}/{targetHealth.GetMaxHealth()}");
    }

    /// <summary>
    /// 获取自身实际碰撞半径（考虑物体缩放）
    /// </summary>
    private float GetMyRadius()
    {
        if (debugMode)
        {
            return debugRadius; // 调试模式：直接返回自定义半径（无需缩放）
        }

        // 非调试模式：计算实际碰撞半径（考虑物体缩放）
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            // 圆形碰撞体：实际半径 = collider.radius × 物体缩放（取最大缩放轴，确保均匀缩放时正确）
            float scale = Mathf.Max(transform.localScale.x, transform.localScale.y);
            return circleCollider.radius * scale;
        }

        // 方形碰撞体
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            // 计算实际边长（考虑缩放）
            float actualSizeX = boxCollider.size.x * transform.localScale.x;
            float actualSizeY = boxCollider.size.y * transform.localScale.y;
            // 等效半径 = 平均边长的1/4（与原逻辑一致，但使用实际边长）
            float avgSize = (actualSizeX + actualSizeY) / 4f;
            return avgSize;
        }

        Debug.LogWarning($"【速度半径伤害】{gameObject.name} 没有碰撞体，使用默认半径1！");
        return 1f;
    }

    /// <summary>
    /// 获取目标实际碰撞半径（考虑目标物体缩放）
    /// </summary>
    private float GetTargetRadius(GameObject target)
    {
        CircleCollider2D circleCollider = target.GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            // 目标圆形碰撞体：实际半径 = collider.radius × 目标缩放
            float targetScale = Mathf.Max(target.transform.localScale.x, target.transform.localScale.y);
            return circleCollider.radius * targetScale;
        }

        BoxCollider2D boxCollider = target.GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            // 目标方形碰撞体：计算实际边长（考虑目标缩放）
            float actualSizeX = boxCollider.size.x * target.transform.localScale.x;
            float actualSizeY = boxCollider.size.y * target.transform.localScale.y;
            // 等效半径 = 平均边长的1/4
            float avgSize = (actualSizeX + actualSizeY) / 4f;
            return avgSize;
        }

        return 1f; // 目标没有碰撞体，使用默认半径1
    }

    /// <summary>
    /// 调试绘制：显示实际碰撞半径（考虑缩放）
    /// </summary>
    private void OnDrawGizmos()
    {
        if (debugMode)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, debugRadius); // 调试模式：绘制自定义半径
        }
        else
        {
            // 非调试模式：绘制实际碰撞半径（考虑缩放）
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, GetMyRadius());
        }
    }
}