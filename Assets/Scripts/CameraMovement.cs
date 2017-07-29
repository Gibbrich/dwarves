using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private const string HORIZONTAL_AXIS = "Horizontal";
    private const string VERTICAL_AXIS = "Vertical";
    
    public float MovementSpeed = 3f;
    private GameObject cameraRig;
    
    // Use this for initialization
    void Start()
    {
        cameraRig = GameObject.Find("CameraRig");
    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxisRaw(HORIZONTAL_AXIS);
        float v = Input.GetAxisRaw(VERTICAL_AXIS);
        if (v > 0)
        {
            Debug.Log(Camera.main.transform.forward);
            Move(Camera.main.transform.forward);
        }
        else if (v < 0)
        {
            Move(-Camera.main.transform.forward);
        }

        if (h > 0)
        {
            Move(Camera.main.transform.right);
        }
        else if (h < 0)
        {
            Move(-Camera.main.transform.right);
        }

        if (Input.GetKey(KeyCode.X))
        {
            Move(Vector3.down);
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            Move(Vector3.up);
        }
    }

    private void Move(Vector3 direction)
    {
        transform.position = transform.position + direction * MovementSpeed * Time.deltaTime;
    }
}