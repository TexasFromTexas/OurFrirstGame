using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	// Start is called before the first frame update
	Rigidbody rb;
	private Camera mainCamera;
	Renderer rend;
	void Start()
    {
		rend = GetComponent<Renderer>();

		rend.material.SetFloat("_Metallic", 1f);
		rend.material.SetColor("_EmissionColor", Color.green);

		rb = GetComponent<Rigidbody>();
		mainCamera = Camera.main;
	}

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
