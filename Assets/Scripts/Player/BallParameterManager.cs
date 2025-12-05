using UnityEngine;

/// <summary>
/// 小球参数管理脚本：自动同步 + 支持外部脚本直接修改
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

    // ------------------- 私有字段（存储当前值） -------------------
    [Header("当前生命值（可直接修改）")]
    [SerializeField] private int _currentHealth; // 私有字段，用于Inspector显示和存储

    [Header("最低攻击力（可直接修改）")]
    [SerializeField] private int _minDamage = 1; // 私有字段，用于Inspector显示和存储

    [Header("小球大小（可直接修改，自动同步碰撞体）")]
    [SerializeField] private float _ballSize = 1f; // 私有字段，用于Inspector显示和存储

    // ------------------- 只读参数（实时获取，Inspector中显示） -------------------
    [Header("只读参数（实时更新）")]
    [SerializeField] private int maxHealth; // 最大生命值
    [SerializeField] private float baseDamage; // 基础伤害（速度系数）
    [SerializeField] private float currentRadius; // 当前碰撞体半径
    [SerializeField] private Vector3 currentScale; // 当前缩放

    // ------------------- 公开属性（支持外部脚本直接修改，自动同步） -------------------
    /// <summary>
    /// 当前生命值（外部可直接修改，自动同步到HealthSystem_New）
    /// </summary>
    public int CurrentHealth
    {
        get => _currentHealth; // 返回存储的值
        set
        {
            // 1. 限制生命值范围
            int clampedHealth = Mathf.Clamp(value, 0, maxHealth);

            // 2. 同步到HealthSystem_New
            if (healthSystem != null)
            {
                healthSystem.SetCurrentHealth(clampedHealth);
                Debug.Log($"✅【外部修改】CurrentHealth被修改为{clampedHealth}，同步到HealthSystem_New");
            }

            // 3. 更新存储的值（同步Inspector显示）
            _currentHealth = clampedHealth;
        }
    }

    /// <summary>
    /// 最低攻击力（外部可直接修改，自动同步到SpeedAndSize）
    /// </summary>
    public int MinDamage
    {
        get => _minDamage; // 返回存储的值
        set
        {
            // 1. 限制最低攻击力范围（至少为1）
            int clampedMinDamage = Mathf.Max(1, value);

            // 2. 同步到SpeedAndSize
            if (speedAndSize != null)
            {
                speedAndSize.minDamage = clampedMinDamage;
                Debug.Log($"✅【外部修改】MinDamage被修改为{clampedMinDamage}，同步到SpeedAndSize");
            }

            // 3. 更新存储的值（同步Inspector显示）
            _minDamage = clampedMinDamage;
        }
    }

    /// <summary>
    /// 小球大小（外部可直接修改，自动同步碰撞体和缩放）
    /// </summary>
    public float BallSize
    {
        get => _ballSize; // 返回存储的值
        set
        {
            // 1. 限制大小范围（至少为0.1）
            float clampedBallSize = Mathf.Max(0.1f, value);

            // 2. 更新缩放
            transform.localScale = Vector3.one * clampedBallSize;

            // 3. 同步碰撞体半径
            SyncColliderRadius();

            // 4. 更新存储的值（同步Inspector显示）
            _ballSize = clampedBallSize;
            Debug.Log($"✅【外部修改】BallSize被修改为{clampedBallSize}，同步缩放和碰撞体");
        }
    }

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

        // 初始化参数：从组件读取初始值
        InitializeParameters();
    }

    /// <summary>
    /// 初始化参数（仅在Awake时执行一次）
    /// </summary>
    private void InitializeParameters()
    {
        if (healthSystem != null)
        {
            // 从HealthSystem_New读取初始生命值和最大生命值
            _currentHealth = healthSystem.GetCurrentHealth();
            maxHealth = healthSystem.GetMaxHealth();
            Debug.Log($"✅【初始化】从HealthSystem_New读取初始生命值：{_currentHealth}/{maxHealth}");
        }

        if (speedAndSize != null)
        {
            // 从SpeedAndSize读取初始最低攻击力和基础伤害
            _minDamage = speedAndSize.minDamage;
            baseDamage = speedAndSize.damageMultiplier;
            Debug.Log($"✅【初始化】从SpeedAndSize读取初始最低攻击力：{_minDamage}，基础伤害：{baseDamage}");
        }

        // 初始化大小参数
        _ballSize = Mathf.Max(0.1f, Mathf.Max(transform.localScale.x, transform.localScale.y));
        currentScale = transform.localScale;
        currentRadius = GetCurrentRadius();
        Debug.Log($"✅【初始化】小球初始大小：{_ballSize}，半径：{currentRadius}");
    }

    // ------------------- 实时更新参数（每帧） -------------------
    private void Update()
    {
        UpdateParametersFromComponents();
    }

    /// <summary>
    /// 从组件读取最新参数（自动同步到属性和Inspector）
    /// </summary>
    private void UpdateParametersFromComponents()
    {
        if (healthSystem != null)
        {
            // 实时同步生命值（组件变化时，自动更新属性和Inspector）
            _currentHealth = healthSystem.GetCurrentHealth();
            maxHealth = healthSystem.GetMaxHealth();
        }

        if (speedAndSize != null)
        {
            // 实时同步基础伤害（速度系数）
            baseDamage = speedAndSize.damageMultiplier;
        }

        // 实时同步大小参数
        currentScale = transform.localScale;
        currentRadius = GetCurrentRadius();
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
            float defaultRadius = 0.5f;
            circleCollider.radius = defaultRadius; // 保持基础radius不变，缩放由transform控制
        }
        // 方形碰撞体：根据缩放调整size（保持实际边长与缩放匹配）
        else if (boxCollider != null)
        {
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
        // 确保组件已获取（仅在编辑器模式下有效）
        if (healthSystem == null) healthSystem = GetComponent<HealthSystem_New>();
        if (speedAndSize == null) speedAndSize = GetComponent<SpeedAndSize>();
        if (circleCollider == null) circleCollider = GetComponent<CircleCollider2D>();
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();

        // 同步Inspector修改到组件（通过属性的setter自动触发同步）
        CurrentHealth = _currentHealth; // 触发CurrentHealth的setter
        MinDamage = _minDamage; // 触发MinDamage的setter
        BallSize = _ballSize; // 触发BallSize的setter
    }
}