using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is a debug script to record camera positions, to assist in setting up levels.
public class DebugPositionRecorder : MonoBehaviour
{
    public Transform cameraLeft;
    public Transform cameraRight;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Debug.Log("Left:");
            Debug.Log(cameraLeft.position);
            Debug.Log(cameraLeft.rotation);
            Debug.Log("Right: " + cameraRight.position);
            Debug.Log(cameraRight.position);
            Debug.Log(cameraRight.rotation);
        }
    }
}
