using UnityEngine;

/// <summary>
/// 带方向的成对传送门脚本
/// </summary>
public class TPdoor : MonoBehaviour
{
    [Header("传送门配置")]
    [SerializeField] private TPdoor targetPortal; // 对应的另一个传送门
    [SerializeField] private float teleportOffset = 0.5f; // 传送位置偏移（避免卡在触发器）
    [SerializeField] private float cooldownTime = 0.2f; // 传送冷却时间（防无限传送）

    [Header("调试选项")]
    [SerializeField] private bool showDirectionGizmo = true; // 显示方向 gizmo

    private Collider2D portalCollider;
    private Rigidbody2D portalRb;

    // 用于记录上次传送的物体和时间（防无限传送）
    private GameObject lastTeleportedObject;
    private float lastTeleportTime;

    private void Awake()
    {
        // 获取组件
        portalCollider = GetComponent<Collider2D>();
        portalRb = GetComponent<Rigidbody2D>();

        // 校验配置
        if (targetPortal == null)
        {
            Debug.LogError($"【传送门】{gameObject.name} 未设置对应传送门！");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 跳过自身、无刚体的物体、冷却中的物体
        if (other.gameObject == gameObject || other.attachedRigidbody == null) return;
        if (other.gameObject == lastTeleportedObject && Time.time - lastTeleportTime < cooldownTime) return;

        // 执行传送
        Teleport(other.attachedRigidbody);
    }

    /// <summary>
    /// 核心传送逻辑
    /// </summary>
    private void Teleport(Rigidbody2D incomingRb)
    {
        if (targetPortal == null) return;

        // 1. 计算原速度大小（保持不变）
        float originalSpeed = incomingRb.velocity.magnitude;

        // 2. 获取目标传送门的方向（基于目标传送门的旋转，right 向量即为传送方向）
        Vector2 targetDirection = targetPortal.transform.right.normalized;

        // 3. 计算传送后的位置（目标传送门位置 + 方向偏移，避免卡在触发器）
        Vector2 teleportPosition = (Vector2)targetPortal.transform.position + targetDirection * teleportOffset;

        // 4. 执行传送：设置位置和新速度
        incomingRb.transform.position = teleportPosition;
        incomingRb.velocity = targetDirection * originalSpeed;

        // 5. 记录传送信息（防无限传送）
        lastTeleportedObject = incomingRb.gameObject;
        lastTeleportTime = Time.time;

        Debug.Log($"✅【传送门】{gameObject.name} → {targetPortal.gameObject.name}");
        Debug.Log($"→ 原速度：{originalSpeed:F2} | 新方向：{targetDirection} | 新速度：{incomingRb.velocity.magnitude:F2}");
    }

    /// <summary>
    /// 绘制方向 gizmo（便于调试）
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showDirectionGizmo) return;

        // 绘制传送门方向箭头（长度为2，颜色为蓝色）
        Gizmos.color = Color.blue;
        Vector2 direction = transform.right.normalized;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + direction * 2f);

        // 绘制箭头尖端
        Vector2 arrowTip = (Vector2)transform.position + direction * 2f;
        Vector2 arrowLeft = arrowTip - direction * 0.3f + new Vector2(-direction.y, direction.x) * 0.2f;
        Vector2 arrowRight = arrowTip - direction * 0.3f + new Vector2(direction.y, -direction.x) * 0.2f;
        Gizmos.DrawLine(arrowTip, arrowLeft);
        Gizmos.DrawLine(arrowTip, arrowRight);

        // 绘制到对应传送门的连线（颜色为绿色）
        if (targetPortal != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetPortal.transform.position);
        }
    }
}