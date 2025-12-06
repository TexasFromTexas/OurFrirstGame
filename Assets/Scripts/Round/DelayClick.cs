using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DelayMouseInput : MonoBehaviour
{
    public GameObject panel;  // 你的 Panel 对象
    public float delayTime = 1f;  // 延迟的时间，单位为秒
    private bool canClick = true;  // 标志是否可以进行鼠标点击

    void Update()
    {
        // 如果 Panel 被关闭并且延迟时间已过，则允许读取鼠标点击
        if (!panel.activeSelf && !canClick)
        {
            canClick = Time.time >= delayTime;
        }

        // 检测鼠标左键点击
        if (canClick && Input.GetMouseButtonDown(0))
        {
            // 在这里处理鼠标点击事件
            Debug.Log("鼠标点击事件处理");

            // 处理点击后, 如果需要再开始延迟计时, 可以重置 canClick
            canClick = false;  // 禁止立刻响应下一次点击
            delayTime = Time.time + 1f;  // 设置延迟时间
        }
    }
}
