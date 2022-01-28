using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDrrawLine : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 5.0f);
    }
}
