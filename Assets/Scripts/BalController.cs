using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalController : MonoBehaviour
{
    public float strength = 10f;    //施加力的大小(暂定为定值,实现其他性质后再进行计算更改)
    Rigidbody2D rb;                 //获取物体2D刚体
    private Camera mainCamera;      
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main; 
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;                                       //获取物体世界坐标
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);  //获取鼠标世界坐标
        Debug.Log(mouseWorldPos);
        mouseWorldPos.z = 0;    //确保位于2D平面
        Vector3 dir = (mouseWorldPos - pos).normalized;
        
        if (Input.GetMouseButtonDown(0))
        {
            rb.AddForce(dir*strength, ForceMode2D.Impulse);
        }
    }
}
