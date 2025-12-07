using UnityEngine;

/// <summary>
/// 独立血量管理系统：支持扣血、血条更新、死亡逻辑、伤害数字显示
/// 挂载对象：需要血量管理的角色（玩家/敌人）
/// </summary>
public class HealthSystem_New : MonoBehaviour
{
    [Header("血量基础配置")]
    [SerializeField] private int maxHealth = 5; // 最大血量
    [SerializeField] private bool isPlayer = false; // true=玩家，false=敌人
    private int currentHealth; // 当前血量

    [Header("音效/音乐配置")]
    [SerializeField] private SceneBGM bgm; // 场景BGM组件（Boss死亡时切换音乐）
    [SerializeField] private bool isBoss = false; // 是否为Boss（死亡时有特殊逻辑）

    [Header("血条UI绑定")]
    [SerializeField] private BloodBarUI_New bloodBar; // 绑定对应血条UI

    // 新增：伤害数字管理器引用（用于显示跳伤害）
    private DamageTextManager damageTextManager;


    private void Awake()
    {
        // 1. 初始化血量
        currentHealth = maxHealth;
        // 2. 初始化血条
        UpdateBloodBar();
        // 3. 自动查找场景BGM（如果未手动赋值）
        if (bgm == null)
        {
            bgm = FindObjectOfType<SceneBGM>();
        }
        // 4. 新增：获取DamageTextManager实例（用于显示伤害数字）
        damageTextManager = FindObjectOfType<DamageTextManager>();
    }


    /// <summary>
    /// 外部调用扣血（核心方法）
    /// </summary>
    /// <param name="damage">扣血量，默认1</param>
    public void TakeDamage(int damage = 1)
    {
        // 防止重复扣血（已死亡时直接返回）
        if (currentHealth <= 0) return;

        // 执行扣血逻辑
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // 确保血量不小于0

        // 调试日志（方便排查问题）
        Debug.Log($"{(isPlayer ? "玩家" : "敌人")} [{gameObject.name}] 扣血！剩余血量：{currentHealth}/{maxHealth}");

        // 新增：显示伤害数字（调用DamageTextManager）
        if (damageTextManager != null)
        {
            damageTextManager.ShowDamage(
                damage: damage,
                worldPosition: transform.position // 伤害从角色位置飘出
            );
        }
        else
        {
            Debug.LogWarning($"【HealthSystem】{gameObject.name} 扣血时，DamageTextManager未找到！请检查是否挂载了DamageTextManager脚本。");
        }

        // 更新血条UI
        UpdateBloodBar();

        // 血量为0触发死亡逻辑
        if (currentHealth == 0)
        {
            // Boss死亡时切换回正常BGM
            if (isBoss && bgm != null)
            {
                bgm.FadeBackToNormal();
            }
            // 执行死亡逻辑
            OnDeath();
        }
    }


    /// <summary>
    /// 更新血条UI
    /// </summary>
    private void UpdateBloodBar()
    {
        if (bloodBar != null)
        {
            // 计算血量百分比（当前血量/最大血量）
            float bloodPercent = (float)currentHealth / maxHealth;
            // 调用血条UI的SetBloodValue方法更新显示
            bloodBar.SetBloodValue(bloodPercent);
        }
        else if (currentHealth < maxHealth)
        {
            // 调试提示：如果血条未绑定，输出警告（仅在非满血时提示）
            Debug.LogWarning($"{gameObject.name} 未绑定血条UI！");
        }
    }


    /// <summary>
    /// 死亡逻辑（根据角色类型执行不同操作）
    /// </summary>
    private void OnDeath()
    {
        // 敌人死亡逻辑
        if (!isPlayer)
        {
            // 1. 隐藏血条
            if (bloodBar != null)
            {
                bloodBar.gameObject.SetActive(false);
            }

            // 2. 通知Round系统：该敌人已死亡（用于回合管理）
            Round round = FindObjectOfType<Round>();
            if (round != null)
            {
                EnemyAI enemy = GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    round.OnEnemyDead(enemy);
                }
            }

            // 3. 延迟销毁（可用于播放死亡动画）
            Destroy(gameObject, 0.5f);
        }
        // 玩家死亡逻辑
        else
        {
            // 停止玩家移动（清空速度）
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }

            // 禁用碰撞体（防止继续受击）
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
            }

            // 可扩展：添加玩家死亡动画、游戏结束逻辑等
        }
    }


    #region 外部访问方法（供其他脚本调用）
    /// <summary>
    /// 获取当前血量
    /// </summary>
    public int GetCurrentHealth() => currentHealth;

    /// <summary>
    /// 设置当前血量（用于外部调整，如治疗）
    /// </summary>
    public void SetCurrentHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth); // 限制在0~maxHealth之间
        UpdateBloodBar(); // 更新血条
    }

    /// <summary>
    /// 获取最大血量
    /// </summary>
    public int GetMaxHealth() => maxHealth;

    /// <summary>
    /// 设置最大血量（用于外部调整，如升级）
    /// </summary>
    public void SetMaxHealth(int value)
    {
        maxHealth = Mathf.Max(value, 1); // 确保最大血量至少为1
        currentHealth = Mathf.Min(currentHealth, maxHealth); // 确保当前血量不超过新的最大血量
        UpdateBloodBar(); // 更新血条
    }
    #endregion
}