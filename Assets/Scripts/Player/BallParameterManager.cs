using UnityEngine;

/// <summary>
/// 小球参数管理脚本：获取和修改生命值、最低攻击力、大小
/// </summary>
[RequireComponent(typeof(HealthSystem_New), typeof(SpeedAndSize), typeof(Rigidbody2D))]
public class BallParameterManager : MonoBehaviour
{
    // ------------------- 组件引用 -------------------
    private HealthSystem_New healthSystem;
    private SpeedAndSize speedAndSize;
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private BoxCollider2D boxCollider;

    // ------------------- 可修改参数（Inspector中显示） -------------------
    [Header("当前生命值（可直接修改）")]
    [SerializeField] private int currentHealth;

    [Header("最低攻击力（可直接修改）")]
    [SerializeField] private int minDamage = 1;

    [Header("小球大小（可直接修改，自动同步碰撞体）")]
    [SerializeField] private float ballSize = 1f; // 大小系数（1=默认大小）

    // ------------------- 只读参数（实时获取，Inspector中显示） -------------------
    [Header("只读参数（实时更新）")]
    [SerializeField] private int maxHealth; // 最大生命值
    [SerializeField] private float baseDamage; // 基础伤害
    [SerializeField] private float currentRadius; // 当前碰撞体半径
    [SerializeField] private Vector3 currentScale; // 当前缩放
    


    // ------------------- 初始化 -------------------
    private void Awake()
    {
        // 获取组件引用
        healthSystem = GetComponent<HealthSystem_New>();
        speedAndSize = GetComponent<SpeedAndSize>();
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        // 校验组件是否获取成功
        if (healthSystem == null)
        {
            Debug.LogError($"❌【初始化失败】小球未挂载HealthSystem_New脚本！");
        }
        if (speedAndSize == null)
        {
            Debug.LogError($"❌【初始化失败】小球未挂载SpeedAndSize脚本！");
        }

        // 初始化参数
        UpdateParametersFromComponents();
    }

    // ------------------- 实时更新参数（每帧） -------------------
    private void Update()
    {
        UpdateParametersFromComponents();
    }

    // ------------------- 参数更新逻辑 -------------------
    /// <summary>
    /// 更新所有参数（从组件中读取最新值，避免覆盖手动修改的currentHealth）
    /// </summary>
    private void UpdateParametersFromComponents()
    {
        // 1. 更新生命值（仅读取maxHealth，currentHealth由组件控制，避免覆盖手动修改）
        maxHealth = healthSystem.GetMaxHealth();
        // 注意：移除currentHealth的自动更新，避免覆盖手动修改
         currentHealth = healthSystem.GetCurrentHealth();

        // 2. 更新伤害参数
        baseDamage = speedAndSize.damageMultiplier;

        // 3. 更新大小参数（缩放和半径）
        currentScale = transform.localScale;
        currentRadius = GetCurrentRadius();
    }

    // ------------------- 参数修改逻辑 -------------------
    /// <summary>
    /// 修改当前生命值（外部可调用）
    /// </summary>
    /// <param name="newHealth">新的生命值</param>
    public void SetCurrentHealth(int newHealth)
    {
        // 1. 限制生命值范围
        int clampedHealth = Mathf.Clamp(newHealth, 0, maxHealth);

        // 2. 同步到HealthSystem_New（核心：取消注释！）
        if (healthSystem != null)
        {
            healthSystem.SetCurrentHealth(clampedHealth);
            Debug.Log($"✅【生命值同步成功】BallParameterManager将currentHealth修改为{clampedHealth}，同步到HealthSystem_New");
        }
        else
        {
            Debug.LogError($"❌【生命值同步失败】healthSystem引用为null！");
        }

        // 3. 更新本地变量（用于Inspector显示）
        currentHealth = clampedHealth;
    }

    /// <summary>
    /// 修改最低攻击力（外部可调用）
    /// </summary>
    /// <param name="newMinDamage">新的最低攻击力</param>
    public void SetMinDamage(int newMinDamage)
    {
        minDamage = Mathf.Max(1, newMinDamage);
        if (speedAndSize != null)
        {
            speedAndSize.minDamage = minDamage;
            Debug.Log($"✅【最低伤害同步成功】BallParameterManager将minDamage修改为{minDamage}，同步到SpeedAndSize");
        }
        else
        {
            Debug.LogError($"❌【最低伤害同步失败】speedAndSize引用为null！");
        }
    }

    /// <summary>
    /// 修改小球大小（外部可调用，自动同步碰撞体和缩放）
    /// </summary>
    /// <param name="newSize">新的大小系数（1=默认大小）</param>
    public void SetBallSize(float newSize)
    {
        ballSize = Mathf.Max(0.1f, newSize); // 限制最小大小为0.1

        // 1. 更新缩放（基于默认大小1对应缩放1）
        transform.localScale = Vector3.one * ballSize;

        // 2. 更新碰撞体半径（基于缩放同步）
        SyncColliderRadius();
    }

    // ------------------- 辅助方法 -------------------
    /// <summary>
    /// 获取当前碰撞体半径（考虑缩放）
    /// </summary>
    private float GetCurrentRadius()
    {
        if (circleCollider != null)
        {
            return circleCollider.radius * Mathf.Max(transform.localScale.x, transform.localScale.y);
        }
        else if (boxCollider != null)
        {
            return (boxCollider.size.x * transform.localScale.x) / 2f;
        }
        return 0.5f; // 默认半径
    }

    /// <summary>
    /// 同步碰撞体半径与缩放（确保碰撞体大小与视觉一致）
    /// </summary>
    private void SyncColliderRadius()
    {
        // 圆形碰撞体：根据缩放调整radius（保持实际半径与缩放匹配）
        if (circleCollider != null)
        {
            // 假设默认缩放1时，radius为0.5（根据你的实际设置调整）
            float defaultRadius = 0.5f;
            circleCollider.radius = defaultRadius; // 保持基础radius不变，缩放由transform控制
        }
        // 方形碰撞体：根据缩放调整size（保持实际边长与缩放匹配）
        else if (boxCollider != null)
        {
            // 假设默认缩放1时，size为1x1（根据你的实际设置调整）
            Vector2 defaultSize = Vector2.one;
            boxCollider.size = defaultSize; // 保持基础size不变，缩放由transform控制
        }
    }

    // ------------------- Inspector修改同步 -------------------
    /// <summary>
    /// 当在Inspector中修改参数时，自动同步到组件
    /// </summary>
    private void OnValidate()
    {
        // 确保组件已获取
        if (healthSystem == null) healthSystem = GetComponent<HealthSystem_New>();
        if (speedAndSize == null) speedAndSize = GetComponent<SpeedAndSize>();
        if (circleCollider == null) circleCollider = GetComponent<CircleCollider2D>();
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();

        // 同步修改到组件（仅在Inspector修改时执行）
        SetCurrentHealth(currentHealth);
        SetMinDamage(minDamage);
        SetBallSize(ballSize);
    }
}