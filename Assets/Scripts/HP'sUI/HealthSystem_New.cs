using UnityEngine;
using UnityEngine.UI;

// 独立血量管理，无需修改原有脚本
public class HealthSystem_New : MonoBehaviour
{
    [Header("血量基础配置")]
    [SerializeField] private int maxHealth = 5; // 最大血量
    [SerializeField] private bool isPlayer = false; // true=玩家，false=敌人
    private int currentHealth; // 当前血量

    [Header("血条UI绑定")]
    [SerializeField] private BloodBarUI_New bloodBar; // 绑定对应血条UI

    [Header("伤害数字管理")]
    [SerializeField] private DamageTextManager damageTextManager; // 伤害数字管理器引用

    private void Awake()
    {
        // 初始化血量
        currentHealth = maxHealth;
        // 初始化血条
        UpdateBloodBar();
    }

    /// <summary>
    /// 外部调用扣血（核心方法）
    /// </summary>
    /// <param name="damage">扣血量，默认1</param>
    public void TakeDamage(int damage = 1)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        // 调试日志（方便排查）
        Debug.Log($"{(isPlayer ? "玩家" : "敌人")} [{gameObject.name}] 收到伤害：{damage}！");
        Debug.Log($"{(isPlayer ? "玩家" : "敌人")} [{gameObject.name}] 扣血！剩余血量：{currentHealth}/{maxHealth}");

        // 显示伤害数字
        if (damageTextManager != null)
        {
            damageTextManager.ShowDamage(damage, transform.position); // 传入伤害值和当前位置
        }

        // 更新血条
        UpdateBloodBar();

        // 血量为0触发死亡（可选扩展）
        if (currentHealth == 0)
        {
            OnDeath();
        }
    }

    /// <summary>
    /// 设置当前生命值（供外部修改）
    /// </summary>
    /// <param name="newHealth">新的生命值</param>
    public void SetCurrentHealth(int newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        UpdateBloodBar(); // 更新血条UI
        Debug.Log($"{(isPlayer ? "玩家" : "敌人")} [{gameObject.name}] 生命值被设置为：{currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// 更新血条UI
    /// </summary>
    private void UpdateBloodBar()
    {
        if (bloodBar != null)
        {
            bloodBar.SetBloodValue((float)currentHealth / maxHealth);
        }
        else if (bloodBar == null && currentHealth < maxHealth)
        {
            Debug.LogWarning($"{gameObject.name} 未绑定血条UI！");
        }
    }

    /// <summary>
    /// 死亡逻辑（可选，按需扩展）
    /// </summary>
    private void OnDeath()
    {
        Debug.Log($"{(isPlayer ? "玩家" : "敌人")} [{gameObject.name}] 血量为0！");
        // 示例：敌人死亡销毁，玩家死亡禁用操作
        if (!isPlayer)
        {
            Destroy(gameObject, 0.5f);
        }
        else
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            GetComponent<Collider2D>().enabled = false; // 禁用碰撞
        }
    }

    // 外部获取当前血量（供调试）
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}