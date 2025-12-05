using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 伤害数字管理脚本：生成伤害数字，向上飘并消失
/// </summary>
public class DamageTextManager : MonoBehaviour
{
    [Header("伤害数字配置")]
    [SerializeField] private Text damageTextPrefab; // 伤害数字预制体
    [SerializeField] private float moveSpeed = 50f; // 向上移动速度（像素/秒）
    [SerializeField] private float duration = 1f; // 显示持续时间（秒）
    [SerializeField] private float fadeSpeed = 2f; // 透明度降低速度（1/秒）

    /// <summary>
    /// 显示伤害数字（外部调用）
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="worldPosition">伤害发生的世界位置</param>
    public void ShowDamage(int damage, Vector3 worldPosition)
    {
        // 1. 将世界坐标转换为屏幕坐标（Canvas为Screen Space - Overlay时，z轴无效）
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        // 2. 实例化伤害数字Text
        Text damageText = Instantiate(damageTextPrefab, transform);
        // 设置Text的位置（屏幕坐标转换为Canvas局部坐标）
        damageText.rectTransform.position = screenPosition;
        // 设置伤害值文本
        damageText.text = damage.ToString();
        Debug.Log($"显示伤害：{damage}，来自对象：{transform.root.name}，位置：{worldPosition}");
        // 3. 启动伤害数字动画协程
        StartCoroutine(DamageTextAnimation(damageText));
    }

    /// <summary>
    /// 伤害数字动画协程：向上移动 + 逐渐消失
    /// </summary>
    private System.Collections.IEnumerator DamageTextAnimation(Text damageText)
    {
        float elapsedTime = 0f; // 已流逝时间
        Color originalColor = damageText.color; // 原始颜色（用于恢复透明度）

        while (elapsedTime < duration)
        {
            // 计算当前进度（0~1）
            float progress = elapsedTime / duration;

            // 1. 向上移动（基于时间的平滑移动）
            damageText.rectTransform.anchoredPosition += new Vector2(0, moveSpeed * Time.deltaTime);

            // 2. 逐渐降低透明度（线性衰减）
            float alpha = Mathf.Lerp(1f, 0f, progress * fadeSpeed);
            damageText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            // 更新已流逝时间
            elapsedTime += Time.deltaTime;
            yield return null; // 等待下一帧
        }

        // 3. 动画结束，销毁伤害数字
        Destroy(damageText.gameObject);
    }
}
