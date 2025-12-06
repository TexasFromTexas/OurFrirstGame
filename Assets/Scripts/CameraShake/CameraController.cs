using UnityEngine;

/// <summary>
/// 相机控制器：支持右键拖动平移、滚轮缩放
/// 挂载对象：MainCamera
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("平移设置")]
    [Tooltip("右键拖动时的平移速度")]
    public float panSpeed = 10f; // 平移速度，可根据需求调整

    [Header("缩放设置")]
    [Tooltip("滚轮缩放速度")]
    public float zoomSpeed = 5f; // 缩放速度
    [Tooltip("相机最小缩放值（正交相机Size）")]
    public float minZoom = 1f; // 最小缩放限制（防止缩太小）
    [Tooltip("相机最大缩放值（正交相机Size）")]
    public float maxZoom = 20f; // 最大缩放限制（防止缩太大）

    private bool isRightMouseDown = false; // 右键是否按下
    private Vector3 lastMousePosition; // 上一帧鼠标位置

    private Camera mainCamera; // 主相机引用

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        // 确保相机是正交模式（2D场景常用）
        mainCamera.orthographic = true;
    }

    private void Update()
    {
        // 1. 监听右键拖动（平移）
        HandlePan();

        // 2. 监听滚轮缩放
        HandleZoom();
    }

    /// <summary>
    /// 处理右键拖动平移
    /// </summary>
    private void HandlePan()
    {
        // 检测右键按下/抬起
        if (Input.GetMouseButtonDown(1)) // 1=右键
        {
            isRightMouseDown = true;
            lastMousePosition = Input.mousePosition; // 记录初始鼠标位置
        }
        if (Input.GetMouseButtonUp(1))
        {
            isRightMouseDown = false;
        }

        // 右键按下时，根据鼠标移动量平移相机
        if (isRightMouseDown)
        {
            Vector3 currentMousePosition = Input.mousePosition;
            Vector3 mouseDelta = currentMousePosition - lastMousePosition; // 鼠标移动差值

            // 将屏幕坐标差值转换为世界坐标差值（正交相机下，屏幕移动量与世界移动量的关系）
            Vector3 moveDelta = new Vector3(-mouseDelta.x, -mouseDelta.y, 0) * panSpeed * Time.deltaTime;
            // 注意：x/y方向取反，因为鼠标向右移动，相机应该向左平移（才能看到右侧内容）

            // 应用平移
            transform.position += moveDelta;

            // 更新上一帧鼠标位置
            lastMousePosition = currentMousePosition;
        }
    }

    /// <summary>
    /// 处理滚轮缩放
    /// </summary>
    private void HandleZoom()
    {
        // 获取滚轮输入（正数=放大，负数=缩小）
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // 正交相机：调整orthographicSize（值越小，视野越小，相当于放大）
            float newSize = mainCamera.orthographicSize - scroll * zoomSpeed;
            // 限制缩放范围
            mainCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }
}
