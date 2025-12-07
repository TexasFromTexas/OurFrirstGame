using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("平移设置")]
    public float panSpeed = 10f;

    [Header("缩放设置")]
    public float zoomSpeed = 5f;
    public float minZoom = 1f;
    public float maxZoom = 20f;

    // 新增：基准位置（供抖动脚本使用）
    public Vector3 targetPosition; // 公开，让抖动脚本访问

    private bool isRightMouseDown = false;
    private Vector3 lastMousePosition;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        mainCamera.orthographic = true;
        // 初始化基准位置为当前相机位置
        targetPosition = transform.position;
    }

    private void Update()
    {
        HandlePan();
        HandleZoom();
    }

    /// <summary>
    /// 处理平移：修改基准位置，不再直接改transform
    /// </summary>
    private void HandlePan()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isRightMouseDown = true;
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(1))
        {
            isRightMouseDown = false;
        }

        if (isRightMouseDown)
        {
            Vector3 currentMousePosition = Input.mousePosition;
            Vector3 mouseDelta = currentMousePosition - lastMousePosition;
            Vector3 moveDelta = new Vector3(-mouseDelta.x, -mouseDelta.y, 0) * panSpeed * Time.deltaTime;
            // 修改基准位置，而非直接改transform
            targetPosition += moveDelta;
            lastMousePosition = currentMousePosition;
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float newSize = mainCamera.orthographicSize - scroll * zoomSpeed;
            mainCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }
}