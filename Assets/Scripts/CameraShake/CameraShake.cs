using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("抖动参数")]
    [SerializeField] private float defaultRange = 0.1f; // 默认抖动幅度
    [SerializeField] private float defaultTime = 0.5f; // 默认抖动时长

    private float shakeRange;
    private float shakeTime;
    private Camera cam;
    private CameraController camController; // 引用平移脚本

    private void Awake()
    {
        cam = GetComponent<Camera>();
        // 获取平移脚本引用（需挂载在同一相机上）
        camController = GetComponent<CameraController>();
        if (camController == null)
        {
            Debug.LogError("CameraShake: 未找到CameraController脚本！");
        }
    }

    private void LateUpdate() // 用LateUpdate确保在平移更新后执行
    {
        if (shakeTime > 0)
        {
            // 计算抖动偏移（只影响x/y轴，z轴保持基准位置）
            Vector3 shakeOffset = Random.insideUnitSphere * shakeRange;
            shakeOffset.z = 0; // 固定z轴，避免影响深度

            // 最终位置 = 平移基准位置 + 抖动偏移
            transform.position = camController.targetPosition + shakeOffset;
            shakeTime -= Time.deltaTime;
        }
        else
        {
            // 抖动结束，回到平移基准位置
            transform.position = camController.targetPosition;
        }
    }

    /// <summary>
    /// 触发抖动（外部调用）
    /// </summary>
    public void Trigger(float range = -1, float time = -1)
    {
        // 使用默认值或传入值
        shakeRange = range > 0 ? range : defaultRange;
        shakeTime = time > 0 ? time : defaultTime;
    }
}