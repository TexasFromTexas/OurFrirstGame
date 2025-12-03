using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Touch : MonoBehaviour
{
    private CameraShake cameraShake;
    public float shakeTime = 0.2f;
    public float shakeRange = 0.1f;

    void Start()
    {
        cameraShake = Camera.main.GetComponent<CameraShake>();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player")|| other.gameObject.CompareTag("Enemy"))
        {
            cameraShake.Trigger(shakeRange,shakeTime);
        }
    }
}
