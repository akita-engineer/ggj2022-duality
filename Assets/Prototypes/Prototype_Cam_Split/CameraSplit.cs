using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSplit : MonoBehaviour
{
	[SerializeField]
	private bool renderWithOffset = default;

	[SerializeField]
	private float movementYOffset = default;
	public Vector3 MoveDirection => Quaternion.Euler(0, movementYOffset, 0) * transform.forward;
	public Vector3 MoveDirectionRight => Quaternion.Euler(0, movementYOffset, 0) * transform.right;

	private Camera targetCamera;

    private void Awake()
    {
		targetCamera = GetComponent<Camera>();
    }

    public Rect scissorRect = new Rect(0, 0, 1, 1);

	// https://answers.unity.com/questions/1709397/urp-cameras-create-a-custom-projection-matrix-give.html
	public void SetScissorRect(Camera cam, Rect r)
	{
		if (r.x < 0)
		{
			r.width += r.x;
			r.x = 0;
		}

		if (r.y < 0)
		{
			r.height += r.y;
			r.y = 0;
		}

		r.width = Mathf.Min(1 - r.x, r.width);
		r.height = Mathf.Min(1 - r.y, r.height);

		cam.rect = new Rect(0, 0, 1, 1);
		cam.ResetProjectionMatrix();
		Matrix4x4 m = cam.projectionMatrix;
		cam.rect = r;

		if (renderWithOffset)
        {
            Matrix4x4 m2 = Matrix4x4.TRS(new Vector3((1 / r.width - 1), (1 / r.height - 1), 0), Quaternion.identity, new Vector3(1 / r.width, 1 / r.height, 1));
            Matrix4x4 m3 = Matrix4x4.TRS(new Vector3(-r.x * 2 / r.width, -r.y * 2 / r.height, 0), Quaternion.identity, Vector3.one);
			cam.projectionMatrix = m3 * m2 * m;
		} else
        {
			Matrix4x4 m2 = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(1 / r.width, 1 / r.height, 1));
			Matrix4x4 m3 = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, Vector3.one);
			cam.projectionMatrix = m3 * m2 * m;
		}
	}

	// Update is called once per frame
	void OnPreRender()
	{
		SetScissorRect(targetCamera, scissorRect);
	}

    private void OnDrawGizmos()
    {
		Gizmos.color = Color.yellow;

		// Rotate forward vector

		Quaternion directionRotation = Quaternion.Euler(0, movementYOffset, 0);
		Vector3 newDirection = directionRotation * transform.forward;

		Debug.DrawLine(transform.position, transform.position + newDirection * 5, Color.red);
    }
}
