using UnityEngine;
using System.Collections.Generic;

public class SlingshotBall : MonoBehaviour
{
    [Header("发射核心参数")]
    [SerializeField] private float maxDragDistance = 5f;
    [SerializeField] private float maxLaunchForce = 20f;

    [Header("引导线参数")]
    [SerializeField] private float guideLineWidth = 0.05f;
    [SerializeField] private Color guideLineColor = Color.green;
    [SerializeField] private float maxGuideLineLength = 8f;

    [Header("动量碰撞核心参数【必配】")]
    [SerializeField] private float ballMass = 1f; // 小球质量（）
    [SerializeField] private bool isElasticCollision = true; // true=完全弹性（动量+动能守恒）
    [SerializeField] private float restitution = 1f; // 恢复系数（1=无能量损失，0=完全非弹性）

    // 调试用：显示速度（保留原有）
    [Header("速度显示（调试）")]
    [SerializeField] private bool showSpeedOnScreen = true;

    // 【轨迹功能】轨迹线参数配置
    [Header("轨迹线参数（含碰撞反弹）")]
    [SerializeField] private Color trajectoryColor = new Color(1, 0.8f, 0, 0.8f); // 轨迹颜色（半透明黄）
    [SerializeField] private float trajectoryWidth = 0.04f; // 轨迹宽度
    [SerializeField] private float recordInterval = 0.02f; // 轨迹点记录间隔（越小越密）
    [SerializeField] private float minMoveDistance = 0.01f; // 最小移动距离（避免重复记录同位置）

    // 原有变量
    private float currentLaunchSpeed;
    private Vector2 collisionSelfVelBefore, collisionOtherVelBefore;
    private Vector2 collisionSelfVelAfter, collisionOtherVelAfter;
    private bool hasCollided;
    private Rigidbody2D rb;
    private Vector2 dragStartPos;
    private bool isDragging;
    private Camera mainCamera;
    private LineRenderer guideLine;

    // 【轨迹功能】轨迹核心变量
    private LineRenderer trajectoryLine; // 轨迹渲染器
    private List<Vector3> trajectoryPoints = new List<Vector3>(); // 轨迹点容器
    private float lastRecordTime; // 上次记录时间（控制频率）
    private Vector3 lastRecordPos; // 上次记录位置（去重）
    private bool isRecordingTrajectory = false; // 是否正在记录轨迹

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        mainCamera = Camera.main;
        InitGuideLine();

        // 【动量守恒修正】清空所有物理干扰项（必须）
        rb.mass = ballMass; // 强制赋值质量
        rb.gravityScale = 0; // 关闭重力（2D平面碰撞无需重力）
        rb.freezeRotation = true;
        rb.drag = 2f; // 阻尼=2，避免速度衰减
        rb.angularDrag = 0f;
        rb.interpolation = RigidbodyInterpolation2D.None; // 关闭插值，提升精度
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 防穿透

        // 【轨迹功能】初始化轨迹线
        InitTrajectory();
    }

    private void InitGuideLine()
    {
        if (guideLine == null)
        {
            guideLine = gameObject.AddComponent<LineRenderer>();
        }
        guideLine.positionCount = 0;
        guideLine.loop = false;
        guideLine.useWorldSpace = true;
        guideLine.startWidth = guideLineWidth;
        guideLine.endWidth = guideLineWidth;

        Gradient gradient = new Gradient();
        gradient.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(guideLineColor, 0f),
            new GradientColorKey(guideLineColor, 1f)
        };
        guideLine.colorGradient = gradient;
        guideLine.enabled = false;
    }

    // 【轨迹功能】初始化轨迹线渲染器
    private void InitTrajectory()
    {
        // 自动创建轨迹LineRenderer（若无）
        if (trajectoryLine == null)
        {
            GameObject trajObj = new GameObject($"Trajectory_{gameObject.name}");
            trajObj.transform.SetParent(transform.root); // 不跟随小球移动，避免轨迹偏移
            trajectoryLine = trajObj.AddComponent<LineRenderer>();

            // 轨迹渲染器基础配置
            trajectoryLine.widthMultiplier = trajectoryWidth;
            trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
            trajectoryLine.startColor = trajectoryColor;
            trajectoryLine.endColor = trajectoryColor;
            trajectoryLine.useWorldSpace = true;
            trajectoryLine.positionCount = 0;
        }
    }

    private void Update()
    {
        // 【轨迹功能】实时记录轨迹（含碰撞反弹）
        UpdateTrajectoryRecord();

        // 拖拽发射逻辑（保留原有，仅重置碰撞状态）
        if (Input.GetMouseButtonDown(0) && !isDragging)
        {
            if (IsMouseOverBall())
            {
                isDragging = true;
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                dragStartPos = transform.position;
                guideLine.enabled = true;

                // 重置碰撞状态
                hasCollided = false;
                collisionSelfVelBefore = collisionOtherVelBefore = Vector2.zero;
                collisionSelfVelAfter = collisionOtherVelAfter = Vector2.zero;
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            Vector2 currentMousePos = mainCamera.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.orthographicSize)
            );

            Vector2 dragVector = currentMousePos - dragStartPos;
            float dragDistance = dragVector.magnitude;
            float clampedDragDistance = Mathf.Clamp(dragDistance, 0f, maxDragDistance);
            Vector2 clampedDragVector = dragVector.normalized * clampedDragDistance;

            Vector2 launchDirection = -clampedDragVector.normalized;
            UpdateGuideLine(launchDirection, clampedDragDistance);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;

            Vector2 dragEndPos = mainCamera.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.orthographicSize)
            );

            Vector2 dragVector = dragEndPos - dragStartPos;
            float dragDistance = dragVector.magnitude;
            float clampedDragDistance = Mathf.Clamp(dragDistance, 0f, maxDragDistance);

            float currentLaunchForce = (clampedDragDistance / maxDragDistance) * maxLaunchForce;
            Vector2 launchDirection = -dragVector.normalized;
            Vector2 launchForceVector = launchDirection * currentLaunchForce;

            rb.AddForce(launchForceVector, ForceMode2D.Impulse);

            // 记录发射速度
            currentLaunchSpeed = rb.velocity.magnitude;
            Debug.Log($"发射速度：{currentLaunchSpeed:F2} 单位/秒");

            // 【轨迹功能】开始记录全流程轨迹（含碰撞反弹）
            StartRecordTrajectory();

            guideLine.positionCount = 0;
            guideLine.enabled = false;
        }
    }

    // 【动量守恒核心：重构碰撞逻辑 - 修复质量相等时速度不变问题】
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 步骤1：仅处理小球碰撞（必须给所有小球打"Player"标签）
        if (!collision.gameObject.CompareTag("Player")) return;

        // 步骤2：获取碰撞对象刚体（无刚体则跳过）
        Rigidbody2D otherRb = collision.rigidbody;
        if (otherRb == null)
        {
            Debug.LogError("碰撞对象缺少Rigidbody2D组件！");
            return;
        }

        // 步骤3：临时关闭物理，避免Unity默认逻辑干扰
        rb.isKinematic = true;
        otherRb.isKinematic = true;

        // 步骤4：定义动量守恒参数
        float m1 = rb.mass; // 自身质量
        float m2 = otherRb.mass; // 对方质量
        Vector2 v1 = rb.velocity; // 碰撞前自身速度
        Vector2 v2 = otherRb.velocity; // 碰撞前对方速度

        // 步骤5：计算碰撞法线（两球中心连线，绝对准确）
        Vector2 collisionNormal = (transform.position - collision.transform.position).normalized;

        // 步骤6：严格动量守恒计算（分弹性/非弹性）
        Vector2 v1Final, v2Final;
        if (isElasticCollision)
        {
            // 【修复1：质量相等时，强制直接交换速度（极简无误差）】
            if (Mathf.Approximately(m1, m2))
            {
                v1Final = v2; // 球1速度 = 球2原来的速度（静止则为0）
                v2Final = v1; // 球2速度 = 球1原来的速度
            }
            else
            {
                // 非等质量时，用原有弹性碰撞公式（不影响其他场景）
                float numerator1 = (m1 - m2) * Vector2.Dot(v1, collisionNormal) + 2 * m2 * Vector2.Dot(v2, collisionNormal);
                float numerator2 = (m2 - m1) * Vector2.Dot(v2, collisionNormal) + 2 * m1 * Vector2.Dot(v1, collisionNormal);

                Vector2 v1Normal = Vector2.Dot(v1, collisionNormal) * collisionNormal;
                Vector2 v1Tangent = v1 - v1Normal;
                Vector2 v2Normal = Vector2.Dot(v2, collisionNormal) * collisionNormal;
                Vector2 v2Tangent = v2 - v2Normal;

                v1Final = (numerator1 / (m1 + m2)) * collisionNormal + v1Tangent;
                v2Final = (numerator2 / (m1 + m2)) * collisionNormal + v2Tangent;
            }

            // 【修复2：删除冗余的恢复系数修正代码（避免覆盖速度交换结果）】
        }
        else
        {
            // 非弹性碰撞：仅动量守恒（动能损失）
            Vector2 totalMomentum = m1 * v1 + m2 * v2;
            float totalMass = m1 + m2;
            v1Final = v2Final = totalMomentum / totalMass; // 碰撞后共速
        }

        // 步骤7：强制赋值速度（核心，覆盖Unity默认物理）
        rb.velocity = v1Final;
        otherRb.velocity = v2Final;

        // 步骤8：防穿透（极小偏移，不干扰速度）
        float pushBack = 0.001f;
        transform.position += (Vector3)collisionNormal * pushBack;
        collision.transform.position -= (Vector3)collisionNormal * pushBack;

        // 记录碰撞速度（调试用）
        collisionSelfVelBefore = v1;
        collisionOtherVelBefore = v2;
        collisionSelfVelAfter = v1Final;
        collisionOtherVelAfter = v2Final;
        hasCollided = true;

       

        // 【轨迹功能】碰撞时强制记录轨迹点（确保反弹轨迹连续）
        RecordTrajectoryOnCollision();

        // 恢复物理状态
        rb.isKinematic = false;
        otherRb.isKinematic = false;
        rb.WakeUp();
        otherRb.WakeUp();
    }

    // 【轨迹功能】开始记录轨迹（发射时调用）
    private void StartRecordTrajectory()
    {
        // 清空旧轨迹（下次发射自动消除）
        ClearTrajectory();

        // 初始化记录状态
        isRecordingTrajectory = true;
        lastRecordTime = Time.time;
        lastRecordPos = transform.position;
        trajectoryPoints.Clear();

        // 记录第一个点（发射起点）
        AddTrajectoryPoint(transform.position);
    }

    // 【轨迹功能】停止记录轨迹（小球静止时调用）
    private void StopRecordTrajectory()
    {
        isRecordingTrajectory = false;
    }

    // 【轨迹功能】清空轨迹（下次发射/重置时调用）
    private void ClearTrajectory()
    {
        trajectoryPoints.Clear();
        if (trajectoryLine != null)
        {
            trajectoryLine.positionCount = 0;
        }
        isRecordingTrajectory = false;
    }

    // 【轨迹功能】添加轨迹点（去重+控频）
    private void AddTrajectoryPoint(Vector3 pos)
    {
        // 过滤：移动距离不足/时间间隔不够 → 不记录
        if (Vector3.Distance(pos, lastRecordPos) < minMoveDistance) return;
        if (Time.time - lastRecordTime < recordInterval) return;

        trajectoryPoints.Add(pos);
        lastRecordPos = pos;
        lastRecordTime = Time.time;

        // 更新轨迹渲染
        UpdateTrajectoryLine();
    }

    // 【轨迹功能】更新轨迹LineRenderer
    private void UpdateTrajectoryLine()
    {
        if (trajectoryLine == null || trajectoryPoints.Count == 0) return;

        trajectoryLine.positionCount = trajectoryPoints.Count;
        trajectoryLine.SetPositions(trajectoryPoints.ToArray());

        // 轨迹渐隐（尾部透明，视觉更自然）
        Gradient gradient = new Gradient();
        gradient.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(trajectoryColor, 0f),
            new GradientColorKey(new Color(trajectoryColor.r, trajectoryColor.g, trajectoryColor.b, 0.1f), 1f)
        };
        trajectoryLine.colorGradient = gradient;
    }

    // 【轨迹功能】帧更新：实时记录轨迹
    private void UpdateTrajectoryRecord()
    {
        if (!isRecordingTrajectory) return;

        // 记录当前位置（碰撞反弹后会自动记录新位置）
        AddTrajectoryPoint(transform.position);

        // 检测小球是否静止 → 停止记录
        if (rb != null && rb.velocity.magnitude < 0.01f && !rb.isKinematic)
        {
            StopRecordTrajectory();
        }
    }

    // 【轨迹功能】碰撞时强制记录轨迹点
    private void RecordTrajectoryOnCollision()
    {
        if (!isRecordingTrajectory) return;
        AddTrajectoryPoint(transform.position); // 碰撞点强制记录
    }

    // 其余原有方法（射线检测、引导线、OnGUI显示、重置）保留不变
    private bool IsMouseOverBall()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
        return hit.collider != null && hit.collider.gameObject == gameObject;
    }

    private void UpdateGuideLine(Vector2 launchDirection, float clampedDragDistance)
    {
        guideLine.positionCount = 2;
        float currentGuideLineLength = (clampedDragDistance / maxDragDistance) * maxGuideLineLength;
        Vector3 startPoint = transform.position;
        Vector3 endPoint = startPoint + (Vector3)launchDirection * currentGuideLineLength;
        guideLine.SetPosition(0, startPoint);
        guideLine.SetPosition(1, endPoint);
    }

    private void OnGUI()
    {
        if (!showSpeedOnScreen) return;
        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        style.normal.background = MakeTex(400, 120, new Color(0, 0, 0, 0.7f));

        GUI.Label(new Rect(10, 10, 400, 30), $"发射速度：{currentLaunchSpeed:F2} 单位/秒", style);
        if (hasCollided)
        {
            GUI.Label(new Rect(10, 50, 400, 30),
                $"碰撞前：自身{collisionSelfVelBefore:F2} | 对方{collisionOtherVelBefore:F2}", style);
            GUI.Label(new Rect(10, 90, 400, 30),
                $"碰撞后：自身{collisionSelfVelAfter:F2} | 对方{collisionOtherVelAfter:F2}", style);
        }
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    public void ResetBall()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        isDragging = false;
        guideLine.positionCount = 0;
        guideLine.enabled = false;
        currentLaunchSpeed = 0;
        hasCollided = false;

        // 【轨迹功能】重置时清空轨迹
        ClearTrajectory();
    }
}