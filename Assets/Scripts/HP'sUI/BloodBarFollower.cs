using UnityEngine;

/// <summary>
/// 血条跟随脚本：独立实现血条跟随小球，无需修改原有脚本
/// 挂载到血条物体上，自动跟随目标小球
/// </summary>
public class BloodBarFollower : MonoBehaviour
{
    [Header("跟随配置")]
    [SerializeField] private Transform target; // 跟随的目标（小球的Transform）
    [SerializeField] private Vector3 offset = new Vector3(0, 0.5f, 0); // 血条在小球上方的偏移量
    [SerializeField] private bool autoSetCanvasMode = true; // 自动将Canvas设置为World Space

    private Canvas parentCanvas;

    private void Awake()
    {
        // 自动设置Canvas为World Space模式（如果需要）
        parentCanvas = GetComponentInParent<Canvas>();
        if (autoSetCanvasMode && parentCanvas != null)
        {
            parentCanvas.renderMode = RenderMode.WorldSpace;
            // 调整Canvas缩放（World Space模式下需要小缩放）
            parentCanvas.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        }
    }

    private void Update()
    {
        // 实时跟随目标位置（如果有目标）
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    /// <summary>
    /// 外部设置跟随目标（用于动态绑定）
    /// </summary>
    public void SetFollowTarget(Transform newTarget)
    {
        target = newTarget;
    }
}