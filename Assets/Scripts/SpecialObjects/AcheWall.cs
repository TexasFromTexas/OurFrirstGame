using UnityEngine;

/// <summary>
/// 钉枪碰撞扣血：支持两种模式
/// 1. 触发器（isTrigger=true）：陷阱模式，穿过即扣血（OnTriggerEnter2D）
/// 2. 非触发器（isTrigger=false）：碰撞模式，接触即扣血（OnCollisionEnter2D）
/// </summary>
public class AcheWall : MonoBehaviour
{
    [Header("扣血配置")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float triggerCooldown = 1f;

    [Header("目标过滤")]
    [SerializeField] private bool damagePlayerOnly = true;
    [SerializeField] private bool damageEnemyOnly = false;

    private float lastTriggerTime;
    private Collider2D collider2D; // 缓存碰撞体引用

    private void Awake()
    {
        // 获取碰撞体（不强制修改isTrigger，由用户手动设置）
        collider2D = GetComponent<Collider2D>();
        if (collider2D == null)
        {
            Debug.LogError($"【钉枪】{gameObject.name} 未添加Collider2D！自动添加BoxCollider2D");
            collider2D = gameObject.AddComponent<BoxCollider2D>();
        }

        // 提示：同时勾选“仅玩家”和“仅敌人”时，会对所有目标生效
        if (damagePlayerOnly && damageEnemyOnly)
        {
            Debug.LogWarning($"【钉枪】{gameObject.name} 同时勾选了“仅玩家”和“仅敌人”，将对所有目标扣血");
        }
    }

    /// <summary>
    /// 非触发器模式：碰撞触发扣血（接触即扣）
    /// 注意：目标物体需挂载Rigidbody2D（Body Type非Static），否则不会触发
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        DealDamage(collision.gameObject); // 调用公共扣血方法
    }

    /// <summary>
    /// 触发器模式：穿过触发扣血（穿过即扣）
    /// 注意：至少一个物体需挂载Rigidbody2D，否则不会触发
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        DealDamage(other.gameObject); // 调用公共扣血方法
    }

    /// <summary>
    /// 公共扣血逻辑（核心复用，避免重复代码）
    /// </summary>
    /// <param name="targetObj">被检测的目标物体</param>
    private void DealDamage(GameObject targetObj)
    {
        // 1. 冷却时间检查
        if (Time.time - lastTriggerTime < triggerCooldown)
        {
            Debug.Log($"【钉枪】{gameObject.name} 冷却中，跳过扣血（目标：{targetObj.name}）");
            return;
        }

        // 2. 检查目标是否有血量组件
        HealthSystem_New targetHealth = targetObj.GetComponent<HealthSystem_New>();
        if (targetHealth == null)
        {
            Debug.LogWarning($"【钉枪】{gameObject.name} 检测到无血量组件的目标：{targetObj.name}，跳过扣血");
            return;
        }

        // 3. 目标标签过滤
        string targetTag = targetObj.tag;
        bool isPlayer = targetTag == "Player";
        bool isEnemy = targetTag == "Enemy";

        // 情况1：同时勾选“仅玩家”和“仅敌人”→ 对所有目标生效（不过滤）
        if (damagePlayerOnly && damageEnemyOnly)
        {
            // 不执行过滤，直接扣血
        }
        // 情况2：仅勾选“仅玩家”→ 过滤非玩家
        else if (damagePlayerOnly && !isPlayer)
        {
            Debug.Log($"【钉枪】{gameObject.name} 仅扣玩家血，跳过敌人目标：{targetObj.name}");
            return;
        }
        // 情况3：仅勾选“仅敌人”→ 过滤非敌人
        else if (damageEnemyOnly && !isEnemy)
        {
            Debug.Log($"【钉枪】{gameObject.name} 仅扣敌人血，跳过玩家目标：{targetObj.name}");
            return;
        }
        // 情况4：都没勾选→ 对所有目标生效

        // 4. 执行扣血
        targetHealth.TakeDamage(damageAmount);
        lastTriggerTime = Time.time;

        // 5. 日志输出（区分模式，方便调试）
        string mode = collider2D.isTrigger ? "陷阱模式（穿过）" : "碰撞模式（接触）";
        Debug.Log($"✅【钉枪】{gameObject.name} {mode} 扣血成功！");
        Debug.Log($"→ 目标：{targetObj.name} | 扣血量：{damageAmount} | 剩余血量：{targetHealth.GetCurrentHealth()}/{targetHealth.GetMaxHealth()}");
    }

    // 绘制Gizmos辅助线（红色线框，方便编辑时查看碰撞体范围）
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.DrawWireCube(transform.position, collider.bounds.size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 0));
        }
    }
}