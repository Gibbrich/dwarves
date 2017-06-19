﻿using System;
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
    public float RotationSpeed = 10;
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

    [Range(0, 1)]
    public float CameraRigRotationSpeed = 0.25f;
    private Quaternion targetRotation;
    private bool isRotating;

    private void Awake()
    {
        mainCamera = GetComponentInChildren<Camera>();
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
        // camera movement
        float x = Input.GetAxis(HORIZONTAL_AXIS);
        float y = Input.GetAxis(VERTICAL_AXIS);
        if (Math.Abs(x) > 0 || Math.Abs(y) > 0)
        {
            Vector3 rigMovement = new Vector3(x, y).normalized * MovementSpeed * Time.deltaTime;
            transform.Translate(rigMovement, Space.World);
        }

        #region CameraRotation
        // camera rotation
        // todo при попытке изменить угол обзора после того, как overview был изменён, overview возвращается к начальному значению - поправить
        if (Input.GetMouseButton(MOUSE_RIGHT_BUTTON))
        {
            // we make initial calculations from the original local rotation
            transform.localRotation = originalRotation;

            // read input from mouse or mobile controls
            float inputH;
            float inputV;
            if (IsRelative)
            {
                inputH = Input.GetAxis("Mouse X");
                inputV = Input.GetAxis("Mouse Y");

                // wrap values to avoid springing quickly the wrong way from positive to negative
                if (targetAngles.y > 180)
                {
                    targetAngles.y -= 360;
                    followAngles.y -= 360;
                }
                if (targetAngles.x > 180)
                {
                    targetAngles.x -= 360;
                    followAngles.x -= 360;
                }
                if (targetAngles.y < -180)
                {
                    targetAngles.y += 360;
                    followAngles.y += 360;
                }
                if (targetAngles.x < -180)
                {
                    targetAngles.x += 360;
                    followAngles.x += 360;
                }

                // with mouse input, we have direct control with no springback required.
                targetAngles.y += inputH * RotationSpeed;
                targetAngles.x += inputV * RotationSpeed;

                // clamp values to allowed range
                targetAngles.y = Mathf.Clamp(targetAngles.y, -RotationRange.y * 0.5f, RotationRange.y * 0.5f);
                targetAngles.x = Mathf.Clamp(targetAngles.x, -RotationRange.x * 0.5f, RotationRange.x * 0.5f);
            }
            else
            {
                inputH = Input.mousePosition.x;
                inputV = Input.mousePosition.y;

                // set values to allowed range
                targetAngles.y = Mathf.Lerp(-RotationRange.y * 0.5f, RotationRange.y * 0.5f, inputH / Screen.width);
                targetAngles.x = Mathf.Lerp(-RotationRange.x * 0.5f, RotationRange.x * 0.5f, inputV / Screen.height);
            }

            // smoothly interpolate current values to target angles
            followAngles = Vector3.SmoothDamp(followAngles, targetAngles, ref followVelocity, DampingTime);

            // update the actual gameobject's rotation
            transform.localRotation = originalRotation * Quaternion.Euler(-followAngles.x, followAngles.y, 0);
        }
        #endregion

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
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, CameraRigRotationSpeed);
            }
            else
            {
                transform.rotation = targetRotation;
                isRotating = false;
            }
        }
    }

    private void rotateOverview(float angle)
    {
        if (isRotating)
        {
            Vector3 targetRotationVector = targetRotation.eulerAngles;
            targetRotation = Quaternion.Euler(targetRotationVector.x, targetRotationVector.y + angle, targetRotationVector.z);
        }
        else
        {
            isRotating = true;
            Vector3 currentRotationVector = transform.rotation.eulerAngles;
            targetRotation = Quaternion.Euler(currentRotationVector.x, currentRotationVector.y + angle, currentRotationVector.z);
        }
    }

    private void LateUpdate()
    {
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, zoom, ZoomSmoothing);
    }
}