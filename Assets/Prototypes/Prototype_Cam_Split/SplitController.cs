using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitController : MonoBehaviour
{
    [SerializeField]
    private KeyCode splitKey = KeyCode.Z;

    [SerializeField]
    private KeyCode leftCameraActivationKey = KeyCode.Keypad1;

    [SerializeField]
    private KeyCode rightCameraActivationKey = KeyCode.Keypad2;

    [SerializeField]
    private float restoreTime = 1.0f;

    [SerializeField]
    private Camera wholeCamera = default;

    [SerializeField]
    private Camera leftCamera = default;

    [SerializeField]
    private Camera rightCamera = default;

    [SerializeField]
    private GameObject splitLine = default;

    private bool isSplit = false;
    private bool isLeftCameraActive = false;

    private void Awake()
    {
        isSplit = false;

        // Disable split cameras
        leftCamera.enabled = false;
        leftCamera.GetComponent<SimpleCameraController>().enabled = false;

        rightCamera.enabled = false;
        rightCamera.GetComponent<SimpleCameraController>().enabled = false;

        // Enable whole one
        wholeCamera.enabled = true;
        wholeCamera.GetComponent<SimpleCameraController>().enabled = true;

        // Deactivate split line
        splitLine.SetActive(false);
    }

    private void Update()
    {
        if (!isSplit)
        {
            if (Input.GetKeyDown(splitKey))
            {
                // Immediate state change
                isSplit = true;

                // Move cameras and rotate
                leftCamera.transform.position = transform.position;
                leftCamera.transform.rotation = transform.rotation;

                rightCamera.transform.position = transform.position;
                rightCamera.transform.rotation = transform.rotation;

                // Disable whole
                wholeCamera.enabled = false;
                wholeCamera.GetComponent<SimpleCameraController>().enabled = false;

                // Enable split ones
                // Always start with left
                isLeftCameraActive = true;
                leftCamera.enabled = true;
                leftCamera.GetComponent<SimpleCameraController>().enabled = true;

                rightCamera.enabled = true;
                rightCamera.GetComponent<SimpleCameraController>().enabled = false;

                // Activate split line
                splitLine.SetActive(true);
            }
        } else
        {
            if (Input.GetKeyDown(splitKey))
            {
                // (For now) immediate state change
                isSplit = false;

                // Move and rotate
                wholeCamera.transform.position = isLeftCameraActive ? leftCamera.transform.position : rightCamera.transform.position;
                wholeCamera.transform.rotation = isLeftCameraActive ? leftCamera.transform.rotation : rightCamera.transform.rotation;

                // Disable split cameras
                leftCamera.enabled = false;
                leftCamera.GetComponent<SimpleCameraController>().enabled = false;

                rightCamera.enabled = false;
                rightCamera.GetComponent<SimpleCameraController>().enabled = false;

                // Enable whole one
                wholeCamera.enabled = true;
                wholeCamera.GetComponent<SimpleCameraController>().enabled = true;

                // Deactivate split line
                splitLine.SetActive(false);
            }

            if (isLeftCameraActive)
            {
                if (Input.GetKeyDown(rightCameraActivationKey))
                {
                    isLeftCameraActive = false;
                    leftCamera.GetComponent<SimpleCameraController>().enabled = false;
                    rightCamera.GetComponent<SimpleCameraController>().enabled = true;
                }
            } else
            {
                if (Input.GetKeyDown(leftCameraActivationKey))
                {
                    isLeftCameraActive = true;
                    leftCamera.GetComponent<SimpleCameraController>().enabled = true;
                    rightCamera.GetComponent<SimpleCameraController>().enabled = false;
                }
            }
        }
    }
}
