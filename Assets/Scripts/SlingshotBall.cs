using UnityEngine;

public class SlingshotBall : MonoBehaviour
{
    [Header("发射参数")]
    public float launchForce = 10f; // 发射力度（可在Inspector调整，越大越快）
    private Rigidbody2D rb;
    private Vector2 dragStartPos; // 拖拽起点（世界坐标）
    private bool isDragging = false;

    private void Awake()
    {
        // 获取小球的2D刚体组件（需提前挂载）
        rb = GetComponent<Rigidbody2D>();
        // 初始状态冻结速度，避免误动
        rb.velocity = Vector2.zero;
    }

    // 鼠标按下小球时触发
    private void OnMouseDown()
    {
        isDragging = true;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        // 将鼠标屏幕坐标转换为2D世界坐标（Z轴与相机一致，避免错位）
        dragStartPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.orthographicSize));
    }

    // 拖拽过程中持续触发（小球不移动，仅记录鼠标位置）
    private void OnMouseDrag()
    {
        if (!isDragging) return;
        // 实时更新鼠标当前世界坐标（仅用于后续计算，不改变小球位置）
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.orthographicSize));
    }

    // 鼠标松开时触发（核心发射逻辑）
    private void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        // 1. 计算鼠标松开时的世界坐标
        Vector2 dragEndPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.orthographicSize));
        // 2. 计算发射方向（起点 - 终点 = 拖拽反方向，即发射方向）
        Vector2 launchDirection = (dragStartPos - dragEndPos).normalized;
        // 3. 计算发射力（方向 × 力度系数）
        Vector2 launchForceVector = launchDirection * launchForce;

        // 4. 施加力发射小球（ForceMode2D.Impulse：瞬时冲量，适合台球类快速发射）
        rb.AddForce(launchForceVector, ForceMode2D.Impulse);
    }

    // 可选：重置小球状态（比如点击UI按钮调用）
    public void ResetBall()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        // 可添加小球回到初始位置的逻辑
    }
}