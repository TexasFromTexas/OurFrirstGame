using UnityEngine;

/// <summary>
/// 碰撞扣血触发器：修复“同时勾选仅玩家/仅敌人时无伤害”的问题
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

    private void Awake()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider == null)
        {
            Debug.LogError($"【扣血触发器】{gameObject.name} 未添加Collider2D！自动添加BoxCollider2D");
            triggerCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        

        // 提示：同时勾选“仅玩家”和“仅敌人”时，会对所有目标生效
        if (damagePlayerOnly && damageEnemyOnly)
        {
            Debug.LogWarning($"【扣血触发器】{gameObject.name} 同时勾选了“仅玩家”和“仅敌人”，将对所有目标扣血");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Time.time - lastTriggerTime < triggerCooldown)
        {
            Debug.Log($"【扣血触发器】{gameObject.name} 冷却中，跳过扣血（目标：{collision.gameObject.name}）");
            return;
        }

        // 从碰撞体获取目标的HealthSystem_New组件
        HealthSystem_New targetHealth = collision.gameObject.GetComponent<HealthSystem_New>();
        if (targetHealth == null)
        {
            Debug.LogWarning($"【扣血触发器】{gameObject.name} 检测到无血量组件的目标：{collision.gameObject.name}，跳过扣血");
            return;
        }

        // 原有的目标过滤逻辑保持不变（复制过来）
        string targetTag = collision.gameObject.tag;
        bool isPlayer = targetTag == "Player";
        bool isEnemy = targetTag == "Enemy";

        if (damagePlayerOnly && damageEnemyOnly)
        {
            // 同时勾选时，对所有目标生效
        }
        else if (damagePlayerOnly && !isPlayer)
        {
            Debug.Log($"【扣血触发器】{gameObject.name} 仅扣玩家血，跳过敌人目标：{collision.gameObject.name}");
            return;
        }
        else if (damageEnemyOnly && !isEnemy)
        {
            Debug.Log($"【扣血触发器】{gameObject.name} 仅扣敌人血，跳过玩家目标：{collision.gameObject.name}");
            return;
        }

        // 执行扣血
        targetHealth.TakeDamage(damageAmount);
        lastTriggerTime = Time.time;
        Debug.Log($"✅【扣血触发器】{gameObject.name} 扣血成功！");
        Debug.Log($"→ 目标：{collision.gameObject.name} | 扣血量：{damageAmount} | 剩余血量：{targetHealth.GetCurrentHealth()}/{targetHealth.GetMaxHealth()}");
    }

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