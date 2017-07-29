using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    private const int MOUSE_RIGHT_BUTTON = 1;
    
    public float speedH = 2.0f;
    public float speedV = 2.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

        // Use this for initialization
        void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(MOUSE_RIGHT_BUTTON))
        {
            float h = Input.GetAxis("Mouse X");
            float v = Input.GetAxis("Mouse Y");

            yaw += speedH * h;
            pitch -= speedV * v;

            transform.eulerAngles = new Vector3(pitch, yaw, 0);
        }
    }
}