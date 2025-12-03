using UnityEngine;

// 敌人专属扣血监听，仅处理被玩家撞击扣血
public class EnemyDamageReceiver : MonoBehaviour
{
    private HealthSystem_New health; // 自身血量组件
    private TurnManager turnManager; // 回合管理器（判断玩家回合）

    private void Awake()
    {
        // 获取组件（无需修改原有脚本，直接查找）
        health = GetComponent<HealthSystem_New>();
        turnManager = FindObjectOfType<TurnManager>();
    }

    // 监听碰撞（与原有EnemyBallPhysics的碰撞逻辑共存，互不干扰）
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 条件1：有血量组件+回合管理器
        if (health == null || turnManager == null) return;

        // 条件2：当前是玩家回合（BallRound）
        if (turnManager.currentTurnState != TurnManager.TurnState.BallRound) return;

        // 条件3：碰撞对象是玩家（标签为Player）
        if (collision.gameObject.CompareTag("Player"))
        {
            // 执行扣血（核心）
            health.TakeDamage(1);
        }
    }
}