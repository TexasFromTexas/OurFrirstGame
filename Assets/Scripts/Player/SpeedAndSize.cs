using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SpeedAndSize : MonoBehaviour
{
    [Header("基础伤害配置")]
    [SerializeField] private float damageMultiplier = 0.5f; // 速度系数（保持私有，供Inspector编辑）
    [SerializeField] private int baseDamage = 1; // 保底伤害

    [Header("最低攻击力")]
    [SerializeField] private int minDamage = 1; // 最低攻击力（供 Inspector 编辑）

    [Header("半径伤害配置")]
    [SerializeField] private float radiusDamageMultiplier = 0.5f; // 半径每差1，伤害变化量（可调整）

    [Header("调试半径（仅调试用）")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private float debugRadius = 1f;

    [Header("目标过滤")]
    [SerializeField] private bool damagePlayerOnly = false;
    [SerializeField] private bool damageEnemyOnly = false;

    private Rigidbody2D rb;
    private Round roundManager;

    // 公开访问器：保持封装同时允许外部读取/设置
    public float DamageMultiplier
    {
        get => damageMultiplier;
        set => damageMultiplier = Mathf.Max(0f, value);
    }

    public int MinDamage
    {
        get => minDamage;
        set => minDamage = Mathf.Max(1, value);
    }

    public void ModifyDamageMultiplier(float delta)
    {
        DamageMultiplier = DamageMultiplier + delta;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        roundManager = FindObjectOfType<Round>();

        if (gameObject.CompareTag("Player"))
        {
            damageEnemyOnly = true;
        }
        else if (gameObject.CompareTag("Enemy"))
        {
            damagePlayerOnly = true;
        }

        Debug.Log($"【速度半径伤害】{gameObject.name} 初始化完成 | 速度系数：{damageMultiplier} | 半径系数：{radiusDamageMultiplier}");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (rb == null) return;

        if (roundManager != null)
        {
            bool isPlayerTurn = roundManager.currentTurnState == Round.TurnState.BallRound;
            bool isEnemyTurn = roundManager.currentTurnState == Round.TurnState.EnemyRound;

            if ((gameObject.CompareTag("Player") && !isPlayerTurn) ||
                (gameObject.CompareTag("Enemy") && !isEnemyTurn))
            {
                return;
            }
        }

        string targetTag = collision.gameObject.tag;
        bool isTargetPlayer = targetTag == "Player";
        bool isTargetEnemy = targetTag == "Enemy";

        if (damagePlayerOnly && !isTargetPlayer) return;
        if (damageEnemyOnly && !isTargetEnemy) return;

        HealthSystem_New targetHealth = collision.gameObject.GetComponent<HealthSystem_New>();
        if (targetHealth == null) return;

        float currentSpeed = rb.velocity.magnitude;
        float speedDamage = baseDamage + currentSpeed * damageMultiplier;

        float myRadius = GetMyRadius();
        float targetRadius = GetTargetRadius(collision.gameObject);
        float radiusDiff = myRadius - targetRadius;
        float radiusBonus = radiusDiff * radiusDamageMultiplier;

        int totalDamage = Mathf.RoundToInt(speedDamage + radiusBonus);
        totalDamage = Mathf.Max(MinDamage, totalDamage);

        targetHealth.TakeDamage(totalDamage);

        Debug.Log($"✅【速度半径伤害】{gameObject.name} 碰撞 {collision.gameObject.name}！");
    }

    private float GetMyRadius()
    {
        if (debugMode) return debugRadius;

        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            float scale = Mathf.Max(transform.localScale.x, transform.localScale.y);
            return circleCollider.radius * scale;
        }

        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            float actualSizeX = boxCollider.size.x * transform.localScale.x;
            float actualSizeY = boxCollider.size.y * transform.localScale.y;
            float avgSize = (actualSizeX + actualSizeY) / 4f;
            return avgSize;
        }

        Debug.LogWarning($"【速度半径伤害】{gameObject.name} 没有碰撞体，使用默认半径1！");
        return 1f;
    }

    private float GetTargetRadius(GameObject target)
    {
        CircleCollider2D circleCollider = target.GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            float targetScale = Mathf.Max(target.transform.localScale.x, target.transform.localScale.y);
            return circleCollider.radius * targetScale;
        }

        BoxCollider2D boxCollider = target.GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            float actualSizeX = boxCollider.size.x * target.transform.localScale.x;
            float actualSizeY = boxCollider.size.y * target.transform.localScale.y;
            float avgSize = (actualSizeX + actualSizeY) / 4f;
            return avgSize;
        }

        return 1f;
    }

    private void OnDrawGizmos()
    {
        if (debugMode)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, debugRadius);
        }
    }
}