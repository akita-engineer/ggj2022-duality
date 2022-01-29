using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// just a basic camera controller but using Unity's new input system
[RequireComponent(typeof(Camera))]
public class InputSystemCameraController : MonoBehaviour
{
    private Camera mCamera;

    private Vector2 mMovementInput;

    private Vector2 mLookInput;

    [SerializeField]
    private float xAxisTurnSpeed = default;

    [SerializeField]
    private float yAxisTurnSpeed = default;

    [SerializeField]
    private float movementSpeed = default;

    public void OnMove(InputAction.CallbackContext context)
    {
        mMovementInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        mLookInput = context.ReadValue<Vector2>();
    }

    private void Awake()
    {
        mCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        float webRotationSpeedDampener = 1.0f;

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            webRotationSpeedDampener = 0.15f;
        }

        Vector3 angle = mCamera.transform.rotation.eulerAngles + new Vector3(-mLookInput.y * yAxisTurnSpeed, mLookInput.x * xAxisTurnSpeed, 0) * Time.deltaTime * webRotationSpeedDampener;
        angle.z = 0f;
        transform.eulerAngles = angle;

        mCamera.transform.Translate(new Vector3(mMovementInput.x, 0, mMovementInput.y) * movementSpeed * Time.deltaTime);
    }
}
