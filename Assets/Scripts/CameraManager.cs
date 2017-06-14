using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private static readonly int MOUSE_RIGHT_BUTTON = 1;
    private static readonly string MOUSE_SCROLL_WHEEL_AXIS = "Mouse ScrollWheel";
    private static readonly string HORIZONTAL_AXIS = "Horizontal";
    private static readonly string VERTICAL_AXIS = "Vertical";
    private static readonly Quaternion DEFAULT_ROTATION = new Quaternion(0, 0, 0, 0);
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

    public Vector2 rotationRange = new Vector3(60, 60);
    public float rotationSpeed = 10;
    public float dampingTime = 0.2f;
    public bool relative = true;

    private Vector3 m_TargetAngles;
    private Vector3 m_FollowAngles;
    private Vector3 m_FollowVelocity;
    private Quaternion m_OriginalRotation;

    public float movementSpeed = 3f;

    public float minZoom = 5.0f;
    public float maxZoom = 80.0f;
    public float zoomSpeed = 30f;
    public float zoomSmoothing = 0.07f;
    private float zoom;

    [Range(0, 1)]
    public float overviewRotationSpeed = 0.25f;
    private Quaternion targetRotation;
    private bool isRotating;

    void Awake()
    {
        mainCamera = GetComponentInChildren<Camera>();
    }

    // Use this for initialization
    void Start()
    {
        m_OriginalRotation = transform.localRotation;
        zoom = mainCamera.fieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        // camera movement
        float x = Input.GetAxis(HORIZONTAL_AXIS);
        float y = Input.GetAxis(VERTICAL_AXIS);
        if (Math.Abs(x) > 0 || Math.Abs(y) > 0)
        {
            Vector3 rigMovement = new Vector3(x, y).normalized * movementSpeed * Time.deltaTime;
            transform.Translate(rigMovement, Space.World);
        }

        #region CameraRotation
        // camera rotation
        // todo при попытке изменить угол обзора после того, как overview был изменён, overview возвращается к начальному значению - поправить
        if (Input.GetMouseButton(MOUSE_RIGHT_BUTTON))
        {
            // we make initial calculations from the original local rotation
            transform.localRotation = m_OriginalRotation;

            // read input from mouse or mobile controls
            float inputH;
            float inputV;
            if (relative)
            {
                inputH = Input.GetAxis("Mouse X");
                inputV = Input.GetAxis("Mouse Y");

                // wrap values to avoid springing quickly the wrong way from positive to negative
                if (m_TargetAngles.y > 180)
                {
                    m_TargetAngles.y -= 360;
                    m_FollowAngles.y -= 360;
                }
                if (m_TargetAngles.x > 180)
                {
                    m_TargetAngles.x -= 360;
                    m_FollowAngles.x -= 360;
                }
                if (m_TargetAngles.y < -180)
                {
                    m_TargetAngles.y += 360;
                    m_FollowAngles.y += 360;
                }
                if (m_TargetAngles.x < -180)
                {
                    m_TargetAngles.x += 360;
                    m_FollowAngles.x += 360;
                }

                // with mouse input, we have direct control with no springback required.
                m_TargetAngles.y += inputH * rotationSpeed;
                m_TargetAngles.x += inputV * rotationSpeed;

                // clamp values to allowed range
                m_TargetAngles.y = Mathf.Clamp(m_TargetAngles.y, -rotationRange.y * 0.5f, rotationRange.y * 0.5f);
                m_TargetAngles.x = Mathf.Clamp(m_TargetAngles.x, -rotationRange.x * 0.5f, rotationRange.x * 0.5f);
            }
            else
            {
                inputH = Input.mousePosition.x;
                inputV = Input.mousePosition.y;

                // set values to allowed range
                m_TargetAngles.y = Mathf.Lerp(-rotationRange.y * 0.5f, rotationRange.y * 0.5f, inputH / Screen.width);
                m_TargetAngles.x = Mathf.Lerp(-rotationRange.x * 0.5f, rotationRange.x * 0.5f, inputV / Screen.height);
            }

            // smoothly interpolate current values to target angles
            m_FollowAngles = Vector3.SmoothDamp(m_FollowAngles, m_TargetAngles, ref m_FollowVelocity, dampingTime);

            // update the actual gameobject's rotation
            transform.localRotation = m_OriginalRotation * Quaternion.Euler(-m_FollowAngles.x, m_FollowAngles.y, 0);
        }
        #endregion

        // camera zoom in/out
        if (Math.Abs(Input.GetAxis(MOUSE_SCROLL_WHEEL_AXIS)) > 0)
        {
            zoom -= Input.GetAxis(MOUSE_SCROLL_WHEEL_AXIS) * zoomSpeed;
            zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        }

        // overview rotation
        // todo change rotation trigger from key to UI button
        // todo при одновременном нажатии на кнопки вращения влево и вправо сбивается угол
        if (Input.GetKeyDown(KeyCode.Q))
        {
            rotateOverview(90);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            rotateOverview(-90);
        }

        if (isRotating)
        {
            if (Quaternion.Angle(targetRotation, transform.rotation) > 0.01f)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, overviewRotationSpeed);
            }
            else
            {
                transform.rotation = targetRotation;
                targetRotation = DEFAULT_ROTATION;
                isRotating = false;
            }
        }
    }

    private void rotateOverview(float angle)
    {
        isRotating = true;
        Vector3 baseRotationAxis = transform.rotation.eulerAngles;
        float delta = targetRotation.Equals(DEFAULT_ROTATION) ? 0 : Quaternion.Angle(targetRotation, transform.rotation);
        Debug.Log(delta);
        if (angle < 0)
        {
            delta = -delta;
        }
        targetRotation = Quaternion.Euler(baseRotationAxis.x, baseRotationAxis.y + angle + delta, baseRotationAxis.z);
    }

    void LateUpdate()
    {
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, zoom, zoomSmoothing);
    }
}