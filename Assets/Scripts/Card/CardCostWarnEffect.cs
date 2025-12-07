using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardCostWarnEffect : MonoBehaviour
{
    [Header("警告参数")]
    public float shakeDistance = 5f; // 抖动距离
    public float shakeDuration = 0.2f; // 抖动时长
    public Color warnColor = Color.red; // 警告颜色

    private Image cardImage; // 卡牌图片
    private Vector3 originalPos; // 原始位置
    private Color originalColor; // 原始颜色

    private void Awake()
    {
        cardImage = GetComponent<Image>();
        originalPos = transform.localPosition;
        originalColor = cardImage.color;
    }

    // 外部调用：播放费用不足警告
    public IEnumerator PlayCostWarnEffect()
    {
        // 1. 抖动动画
        float elapsedTime = 0f;
        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;
            // 随机抖动（左右/上下）
            float x = Random.Range(-shakeDistance, shakeDistance);
            float y = Random.Range(-shakeDistance, shakeDistance);
            transform.localPosition = originalPos + new Vector3(x, y, 0);
            yield return null;
        }
        transform.localPosition = originalPos; // 恢复位置

        // 2. 闪烁红色（两次）
        for (int i = 0; i < 2; i++)
        {
            cardImage.color = warnColor;
            yield return new WaitForSeconds(0.1f);
            cardImage.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
