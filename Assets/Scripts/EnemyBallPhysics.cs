using UnityEngine;

public class EnemyBallPhysics : MonoBehaviour
{
    [Header("动量碰撞核心参数")]
    [SerializeField] private float ballMass = 1f; // 敌人小球质量
    [SerializeField] private bool isElasticCollision = true; // 是否弹性碰撞（默认开启）
    [SerializeField] private float restitution = 1f; // 恢复系数（1=完全弹性，0=完全非弹性）

    // 碰撞相关核心变量
    private Rigidbody2D rb;
    private bool hasCollided; // 标记是否发生过碰撞（可选，可删除）

    private void Awake()
    {
        // 初始化刚体，配置物理参数（适配敌人小球）
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>(); // 自动添加刚体（防止遗漏）
        }

        // 核心物理参数配置（确保碰撞逻辑生效）
        rb.mass = ballMass;
        rb.gravityScale = 0; // 2D物理场景通常关闭重力（可根据需求调整）
        rb.freezeRotation = true; // 禁止旋转，保持小球朝向
        rb.drag = 0f; // 无阻尼，避免速度衰减
        rb.angularDrag = 0f;
        rb.interpolation = RigidbodyInterpolation2D.None; // 关闭插值提升精度
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 连续碰撞检测防穿透
    }

    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 仅处理标签为"Enemy"（或你自定义的敌人/玩家小球标签）的碰撞体
        
        if (!collision.gameObject.CompareTag("Enemy")) return;

        // 获取碰撞对象的刚体
        Rigidbody2D otherRb = collision.rigidbody;
        if (otherRb == null) return;

        // 临时关闭物理，避免Unity默认逻辑干扰手动速度赋值
        rb.isKinematic = true;
        otherRb.isKinematic = true;

        // 定义碰撞前的动量参数
        float m1 = rb.mass; // 自身质量
        float m2 = otherRb.mass; // 对方质量
        Vector2 v1 = rb.velocity; // 自身碰撞前速度
        Vector2 v2 = otherRb.velocity; // 对方碰撞前速度
        Vector2 collisionNormal = (transform.position - collision.transform.position).normalized; // 碰撞法线

        // 计算碰撞后的速度（分弹性/非弹性）
        Vector2 v1Final, v2Final;
        if (isElasticCollision)
        {
            // 弹性碰撞：质量相等时直接交换速度（敌人小球常用场景）
            if (Mathf.Approximately(m1, m2))
            {
                v1Final = v2;
                v2Final = v1;
            }
            else
            {
                // 非等质量弹性碰撞（完整动量守恒公式）
                float numerator1 = (m1 - m2) * Vector2.Dot(v1, collisionNormal) + 2 * m2 * Vector2.Dot(v2, collisionNormal);
                float numerator2 = (m2 - m1) * Vector2.Dot(v2, collisionNormal) + 2 * m1 * Vector2.Dot(v1, collisionNormal);

                Vector2 v1Normal = Vector2.Dot(v1, collisionNormal) * collisionNormal;
                Vector2 v1Tangent = v1 - v1Normal;
                Vector2 v2Normal = Vector2.Dot(v2, collisionNormal) * collisionNormal;
                Vector2 v2Tangent = v2 - v2Normal;

                v1Final = (numerator1 / (m1 + m2)) * collisionNormal + v1Tangent;
                v2Final = (numerator2 / (m1 + m2)) * collisionNormal + v2Tangent;
            }
        }
        else
        {
            // 非弹性碰撞：碰撞后共速（动能损失）
            Vector2 totalMomentum = m1 * v1 + m2 * v2;
            float totalMass = m1 + m2;
            v1Final = v2Final = totalMomentum / totalMass;
        }

        // 强制赋值碰撞后速度（核心：覆盖Unity默认物理）
        rb.velocity = v1Final;
        otherRb.velocity = v2Final;

        // 防穿透：微小偏移避免小球重叠
        float pushBack = 0.001f;
        transform.position += (Vector3)collisionNormal * pushBack;
        collision.transform.position -= (Vector3)collisionNormal * pushBack;

        // 标记碰撞状态（可选，不需要可删除）
        hasCollided = true;

        // 恢复物理状态，让小球继续运动
        rb.isKinematic = false;
        otherRb.isKinematic = false;
        rb.WakeUp();
        otherRb.WakeUp();

        // 可选：碰撞日志（调试用，发布时删除）
        Debug.Log($"敌人小球碰撞 | 自身速度：{v1Final.magnitude:F2} | 对方速度：{v2Final.magnitude:F2}");
    }

    // 简化版重置方法（敌人小球重置位置/速度时调用）
    public void ResetEnemyBall()
    {
        if (rb == null) return;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        hasCollided = false;
    }
}