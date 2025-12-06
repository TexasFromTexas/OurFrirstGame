using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake: MonoBehaviour
{
    private Vector3 CameraPos;
    float ShakeRange;
    float ShakeTime;
    private Camera cam;
    
    // Start is called before the first frame update
    void Awake()
    {
        cam = GetComponent<Camera>();
        CameraPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (ShakeTime > 0)
        {
            transform.position = CameraPos + Random.insideUnitSphere * ShakeRange;
            CameraPos.z = -10;
            ShakeTime -= Time.deltaTime;
        }
        else
        {
            transform.position = CameraPos;
        }
    }

    public void Trigger(float range,float time)
    {
        ShakeRange =  range;
        ShakeTime = time;
    }
}
