using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDrrawLine : MonoBehaviour
{
    public float fov = 50;
    public float far = 500;
    public float near = 10;
    public float aspect = 1920f / 1080f;

    public float fovMult = 1.0f;
    public float divide = 2.0f;
    public float angle = 22.5f;

    public bool draw = true;

    static float FindAngle(float aspect, float fovRads)
    {
        return Mathf.Acos(1 - (2 * Mathf.Pow(aspect, 2) * Mathf.Pow(Mathf.Tan(fovRads / 2.0f), 2) / (Mathf.Pow(aspect, 2) + 1)));
    }

    private void OnDrawGizmos()
    {
        if (!draw)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 5.0f);

        // Gizmos.matrix = transform.localToWorldMatrix;
        Camera camera = GetComponent<Camera>();
        //Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(camera.aspect, 1.0f, 1.0f));
        //Gizmos.DrawFrustum(Vector3.zero, camera.fieldOfView, far, near, 1.0f);


        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawFrustum(Vector3.zero, fov, far, near, aspect);

        var viewMat = Matrix4x4.LookAt(transform.position, transform.position + transform.forward, Vector3.up);
        Gizmos.matrix = viewMat;
        Gizmos.DrawFrustum(transform.forward, fov, far, near, aspect / 2.0f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawFrustum(transform.forward, fov / 2.0f, far, near, aspect);


        var rotateMat = Matrix4x4.Rotate(Quaternion.Euler(0, angle, 0));
        var right = rotateMat.MultiplyPoint(transform.forward);
        var rightViewMat = Matrix4x4.LookAt(transform.position, transform.position + right, Vector3.up);
        Gizmos.color = Color.blue;
        Gizmos.matrix = rightViewMat;
        Gizmos.DrawFrustum(Vector3.zero, fov * fovMult, far, near, aspect / divide);
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.DrawLine(transform.position, transform.position + right * far);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * far);

        var left = Quaternion.Euler(0, -fov / 4.0f, 0) * transform.forward;
        var leftViewMat = Matrix4x4.LookAt(transform.position, transform.position + left, Vector3.up);
        Gizmos.color = Color.green;
        Gizmos.matrix = leftViewMat;
        // fov / 2.0f * divide
        Gizmos.DrawFrustum(Vector3.zero, fov * fovMult, far, near, aspect / divide);
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.DrawLine(transform.position, transform.position + left * far);
    }
}
