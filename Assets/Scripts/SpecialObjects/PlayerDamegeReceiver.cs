using UnityEngine;

// 玩家专属扣血监听，仅处理被敌人撞击扣血
public class PlayerDamageReceiver : MonoBehaviour
{
    private HealthSystem_New health; // 自身血量组件
    private Round Round; // 回合管理器（判断敌人回合）

    private void Awake()
    {
        // 获取组件（无需修改原有脚本，直接查找）
        health = GetComponent<HealthSystem_New>();
        Round = FindObjectOfType<Round>();

        // 日志1：检查组件获取结果
        Debug.Log($"【玩家扣血监听】初始化：");
        Debug.Log($"→ 血量组件是否获取到：{health != null}");
        Debug.Log($"→ 回合管理器是否获取到：{Round != null}");
        Debug.Log($"→ 玩家小球标签：{gameObject.tag}");
    }

    // 监听碰撞（与原有SlingshotBall的碰撞逻辑共存，互不干扰）
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 日志2：碰撞触发总开关（确认碰撞是否检测到）
        Debug.Log($"【玩家扣血监听】碰撞触发！撞到了：{collision.gameObject.name} | 对方标签：{collision.gameObject.tag}");

        // 条件1：有血量组件+回合管理器
        if (health == null)
        {
            Debug.LogError("【玩家扣血监听】失败：未挂载HealthSystem_New组件！");
            return;
        }
        if (Round == null)
        {
            Debug.LogError("【玩家扣血监听】失败：场景中找不到Round！");
            return;
        }

        // 日志3：打印当前回合状态
        Debug.Log($"【玩家扣血监听】当前回合：{Round.currentTurnState} | 需要EnemyRound");

        // 条件2：当前是敌人回合（EnemyRound）
        if (Round.currentTurnState != Round.TurnState.EnemyRound)
        {
            Debug.LogError($"【玩家扣血监听】失败：当前不是敌人回合！");
            return;
        }

        // 条件3：碰撞对象是敌人（标签为Enemy）
        if (!collision.gameObject.CompareTag("Enemy"))
        {
            Debug.LogError($"【玩家扣血监听】失败：碰撞对象不是敌人！对方标签：{collision.gameObject.tag}");
            return;
        }

        // 执行扣血（能走到这说明所有条件满足）
        health.TakeDamage(1);
        Debug.Log($"✅【玩家扣血监听】成功！玩家扣1血，剩余血量：{health.GetCurrentHealth()}");
    }
}