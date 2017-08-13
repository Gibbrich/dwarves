using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalCameraRotation : MonoBehaviour
{
    private const int MOUSE_RIGHT_BUTTON = 1;
    
    // Define the limit of camera position as well as rotation
    public float CameraTopLimit;
    public float CameraBottomLimit;

    // Position period that the camera won't rotate
    public float ThresholdTop;
    public float ThresholdBottom;

    // variables to define the camera movement speed, camera rotation speed, and deceleration(lerp) speed respectively
    public float MoveSpeed;
    public float RotationSpeed;
    public float LerpSpeed;

    // hold timer to prevent the camera from jerking in mobile device
    private float holdTimer = 0f;

    // holds the y-axis retrieved from mouse
    private float yAxis;

    // holds the current 
    private float speed;

    public float MinimumTilt = -30F;
    public float MaximumTilt = 30F;
    private float rotationY = 0F;
    private Quaternion originalRotation;
    private float originalXRot;
    private float originalYRot;
    private int lastTouch = 0;

    // Use this for initialization
    void Start()
    {
        originalRotation = transform.localRotation;
        originalXRot = transform.localEulerAngles.x;
        originalYRot = transform.localEulerAngles.y;
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
            if (Input.GetMouseButton(MOUSE_RIGHT_BUTTON) && Input.GetKey(KeyCode.LeftControl))
            {
                holdTimer++;
            }

            // if the user hold for more than 3 frame, record the mouse y-axis
            if (Input.GetMouseButton(MOUSE_RIGHT_BUTTON) && Input.GetKey(KeyCode.LeftControl) && holdTimer > 3)
            {
                // reverse the received y-axis to create inverse-axis movement (remove minus sign if you want normal-axis movement)
                yAxis = -Input.GetAxis("Mouse Y");
                speed = yAxis;
            }
            // else the user is not holding the mouse click anymore, begin calculating the lerp speed 
            else
            {
                float ix = Time.deltaTime * LerpSpeed;
                speed = Mathf.Lerp(speed, 0, ix);
            }

            // if the user release the mouse/touch, reset the timer
            if (Input.GetMouseButtonUp(MOUSE_RIGHT_BUTTON))
            {
                holdTimer = 0;
            }

            // calculate the movement of the camera, clamp it so that it won't exceed the limit
            float limitY = Mathf.Clamp(transform.position.y + (speed * MoveSpeed), CameraBottomLimit, CameraTopLimit);
            transform.position = new Vector3(transform.position.x, limitY, transform.position.z);

            // if the camera pos still inside the limit, rotate the camera as well
            if (!(transform.position.y < ThresholdTop && transform.position.y > ThresholdBottom))
            {
                rotationY += speed * -RotationSpeed * 0.8f;
                if (transform.position.y > ThresholdTop)
                {
                    rotationY = ClampAngle(rotationY, -MaximumTilt, 1F);
                }
                else if (transform.position.y < ThresholdBottom)
                {
                    rotationY = ClampAngle(rotationY, 1F, -MinimumTilt);
                }
                
                Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);
                transform.localRotation = originalRotation * yQuaternion;
            }
        }
        else
        {
            lastTouch = Input.touchCount;
            speed = 0;
        }
    }

    // function to clamp the angle based on given min and max
    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
        {
            angle += 360;
        }

        if (angle > 360)
        {
            angle -= 360;
        }

        return Mathf.Clamp(angle, min, max);
    }
}