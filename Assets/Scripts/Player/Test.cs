using UnityEngine;

/// <summary>
/// 外部脚本示例：直接修改小球参数
/// </summary>
public class Test : MonoBehaviour
{
    [Header("小球参数管理器引用")]
    [SerializeField] private BallParameterManager ballParameterManager;

    private void Start()
    {
        // 示例1：直接修改生命值（自动同步到HealthSystem_New）
        ballParameterManager.CurrentHealth = 10;

        // 示例2：直接修改最低攻击力（自动同步到SpeedAndSize）
        ballParameterManager.MinDamage = 1;

        // 示例3：直接修改小球大小（自动同步缩放和碰撞体）
        ballParameterManager.BallSize = 1.0f;
    }

    private void Update()
    {
        // 示例4：按空格键增加生命值
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ballParameterManager.CurrentHealth += 1;
            Debug.Log($"空格键增加生命值，当前生命值：{ballParameterManager.CurrentHealth}");
        }

        // 示例5：按M键增加最低攻击力
        if (Input.GetKeyDown(KeyCode.M))
        {
            ballParameterManager.MinDamage += 1;
            Debug.Log($"M键增加最低攻击力，当前最低攻击力：{ballParameterManager.MinDamage}");
        }

        // 示例6：按S键增大小球
        if (Input.GetKeyDown(KeyCode.S))
        {
            ballParameterManager.BallSize += 0.2f;
            Debug.Log($"S键增大小球，当前大小：{ballParameterManager.BallSize}");
        }
    }
}