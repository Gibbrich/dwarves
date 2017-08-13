using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalCameraRotation : MonoBehaviour
{
    private const int MOUSE_RIGHT_BUTTON = 1;

    // variable that determine the speed of rotation
    public float RotationSpeed = 5.0f;

    // variable that determine the speed of lerping (deceleration)
    public float LerpSpeed = 2.0f;

    // variable that holds the current rotation speed
    private float speed;

    // timer to check whether the touch is a valid rotation, to prevent camera jerking on mobile device
    private float holdTimer = 0.0f;

    // variable to hold the x-axis from mouse
    private float xAxis = 0.0f;
    private int lastTouch = 0;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount < 2 && lastTouch < 2)
        {
            if (Input.touchCount == 0)
            {
                lastTouch = 0;
            }
            // adds up the timer everytime the user holds the left mouse button/one finger touch in mobile device
            if (Input.GetMouseButton(MOUSE_RIGHT_BUTTON) && !Input.GetKey(KeyCode.LeftControl))
            {
                holdTimer++;
            }

            // if the user hold for more than 3 frame, record the mouse x-axis
            if (Input.GetMouseButton(MOUSE_RIGHT_BUTTON) && !Input.GetKey(KeyCode.LeftControl) && holdTimer > 3)
            {
                holdTimer++;
                xAxis = Input.GetAxis("Mouse X");
                speed = xAxis;
            }
            // else the user is not holding the mouse click anymore, begin calculating the lerp speed 
            else
            {
                float i = Time.deltaTime * LerpSpeed;
                speed = Mathf.Lerp(speed, 0, i);
            }

            // if the user release the mouse/touch, reset the timer
            if (Input.GetMouseButtonUp(MOUSE_RIGHT_BUTTON))
            {
                holdTimer = 0;
            }
            // rotate the object
            transform.Rotate(0.0f, speed * RotationSpeed, 0.0f, Space.World);
        }
        else
        {
            lastTouch = Input.touchCount;
            speed = 0;
        }
    }
}