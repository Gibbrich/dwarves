using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private const int MOUSE_RIGHT_BUTTON = 1;
    private const string MOUSE_SCROLL_WHEEL_AXIS = "Mouse ScrollWheel";
    private const string HORIZONTAL_AXIS = "Horizontal";
    private const string VERTICAL_AXIS = "Vertical";

    // A mouselook behaviour with constraints which operate relative to
    // this gameobject's initial rotation.
    // Only rotates around local X and Y.
    // Works in local coordinates, so if this object is parented
    // to another moving gameobject, its local constraints will
    // operate correctly
    // (Think: looking out the side window of a car, or a gun turret
    // on a moving spaceship with a limited angular range)
    // to have no constraints on an axis, set the rotationRange to 360 or greater.
    private Camera mainCamera;

    public Vector2 RotationRange = new Vector3(60, 60);

    // variable that determine the speed of rotation
    public float RotationSpeed = 5;

    public float DampingTime = 0.2f;
    public bool IsRelative = true;

    private Vector3 targetAngles;
    private Vector3 followAngles;
    private Vector3 followVelocity;
    private Quaternion originalRotation;

    public float MovementSpeed = 3f;

    public float MinZoom = 5.0f;
    public float MaxZoom = 80.0f;
    public float ZoomSpeed = 30f;
    public float ZoomSmoothing = 0.07f;
    private float zoom;

    [Range(0, 1)] public float CameraRigRotationSpeed = 0.25f;
    private Quaternion targetRotation;
    private bool isRotating;

    private List<Transform> overviewPoints;
    private Vector3 targetPosition;
    private bool isMoving;

    private OverviewRotation overviewRotation;
    private List<MeshRenderer> meshes;

    // variable that determine the speed of lerping (deceleration)
    public float LerpSpeed = 2.0f;

    // variable that holds the current rotation speed
    private float speed;

    // timer to check whether the touch is a valid rotation, to prevent camera jerking on mobile device
    private float holdTimer = 0.0f;

    // variable to hold the x-axis from mouse
    private float xAxis = 0.0f;

    private int lastTouch = 0;

    private void Awake()
    {
        mainCamera = GetComponentInChildren<Camera>();
        overviewRotation = OverviewRotation.Northward;

        // get all objects, which can be faded
        meshes = new List<MeshRenderer>();
        foreach (GameObject environmentObject in GameObject.FindGameObjectsWithTag("Hidable"))
        {
            meshes.AddRange(environmentObject.GetComponentsInChildren<MeshRenderer>());
        }

        overviewPoints =
            new List<Transform>(GameObject.Find("TargetCameraRigMovement").GetComponentsInChildren<Transform>());
    }

    // Use this for initialization
    private void Start()
    {
        originalRotation = transform.localRotation;
        zoom = mainCamera.fieldOfView;
    }

    // Update is called once per frame
    private void Update()
    {
        horizontalRotation();
        
        // center camera
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Keypad5))
        {
            float rotationY;
            switch (overviewRotation)
            {
                case OverviewRotation.Northward:
                    rotationY = 0;
                    break;
                case OverviewRotation.Eastward:
                    rotationY = 90;
                    break;
                case OverviewRotation.Westward:
                    rotationY = -90;
                    break;
                case OverviewRotation.Southward:
                    rotationY = 180;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // reset cameraRotation angles
            targetAngles = Vector3.zero;
            followAngles = Vector3.zero;
            transform.localEulerAngles = new Vector3(10, rotationY, 0);
        }

        // camera zoom in/out
        if (Math.Abs(Input.GetAxis(MOUSE_SCROLL_WHEEL_AXIS)) > 0)
        {
            zoom -= Input.GetAxis(MOUSE_SCROLL_WHEEL_AXIS) * ZoomSpeed;
            zoom = Mathf.Clamp(zoom, MinZoom, MaxZoom);
        }

        // overview rotation
        // todo change rotation trigger from key to UI button
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RotateOverview(90);
            MoveOverviewPoint();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            RotateOverview(-90);
            MoveOverviewPoint();
        }

        if (isRotating)
        {
            if (Quaternion.Angle(targetRotation, transform.rotation) > 0.01f)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, CameraRigRotationSpeed);
            }
            else
            {
                transform.rotation = targetRotation;
                isRotating = false;
            }
        }

        if (isMoving)
        {
            if (Vector3.Distance(targetPosition, transform.position) > 0.01f)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, CameraRigRotationSpeed);
            }
            else
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }

    private void RotateOverview(float angle)
    {
        // change limitations for camera rotation according to changed overview
        Vector3 originalRotationVector = originalRotation.eulerAngles;
        originalRotationVector.y += angle;
        originalRotation = Quaternion.Euler(originalRotationVector);

        if (isRotating)
        {
            Vector3 targetRotationVector = targetRotation.eulerAngles;
            targetRotation = Quaternion.Euler(targetRotationVector.x, targetRotationVector.y + angle,
                targetRotationVector.z);
        }
        else
        {
            isRotating = true;
            Vector3 currentRotationVector = transform.rotation.eulerAngles;
            targetRotation = Quaternion.Euler(currentRotationVector.x, currentRotationVector.y + angle,
                currentRotationVector.z);
        }

        // assume that rotate we can only by 90 degrees in every direction
        switch (overviewRotation)
        {
            case OverviewRotation.Northward:
                overviewRotation = angle > 0 ? OverviewRotation.Eastward : OverviewRotation.Westward;
                break;
            case OverviewRotation.Eastward:
                overviewRotation = angle > 0 ? OverviewRotation.Southward : OverviewRotation.Northward;
                break;
            case OverviewRotation.Westward:
                overviewRotation = angle > 0 ? OverviewRotation.Northward : OverviewRotation.Southward;
                break;
            case OverviewRotation.Southward:
                overviewRotation = angle > 0 ? OverviewRotation.Westward : OverviewRotation.Eastward;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void MoveOverviewPoint()
    {
        // move overview point
        // todo list of predefined overview points - temporal decision. Need to implement algorithm for
        // searching nearest overview point
        float nearestDistance = float.NaN;
        Vector3 tempTargetPosition = new Vector3();
        foreach (Transform point in overviewPoints)
        {
            if (float.IsNaN(nearestDistance) || Vector3.Distance(transform.position, point.position) < nearestDistance)
            {
                tempTargetPosition = point.position;
                nearestDistance = Vector3.Distance(transform.position, point.transform.position);
            }
        }

        targetPosition = tempTargetPosition;
        isMoving = true;
    }

    private void horizontalRotation()
    {
        if (Input.touchCount < 2 && lastTouch < 2)
        {
            if (Input.touchCount == 0)
            {
                lastTouch = 0;
            }
            // adds up the timer everytime the user holds the left mouse button/one finger touch in mobile device
            if (Input.GetMouseButton(MOUSE_RIGHT_BUTTON))
            {
                holdTimer++;
            }

            // if the user hold for more than 3 frame, record the mouse x-axis
            if (Input.GetMouseButton(MOUSE_RIGHT_BUTTON) && holdTimer > 3)
            {
                holdTimer++;
                xAxis = Input.GetAxis("Mouse X");
                speed = xAxis;
            }
            // else the user is not holding the mouse click anymore, begin calculating the lerp speed 
            else
            {
                var i = Time.deltaTime * LerpSpeed;
                speed = Mathf.Lerp(speed, 0, i);
            }

            // if the user release the mouse/touch, reset the timer
            if (Input.GetMouseButtonUp(MOUSE_RIGHT_BUTTON))
            {
                holdTimer = 0;
            }
            // rotate the object
            transform.Rotate(0.0f, speed * LerpSpeed, 0.0f, Space.World);
        }
        else
        {
            lastTouch = Input.touchCount;
            speed = 0;
        }
    }

    private void LateUpdate()
    {
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, zoom, ZoomSmoothing);
    }

    private enum OverviewRotation
    {
        Northward,
        Eastward,
        Westward,
        Southward
    }
}