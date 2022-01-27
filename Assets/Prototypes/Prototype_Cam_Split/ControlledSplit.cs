using UnityEngine;

public class ControlledSplit : MonoBehaviour
{
    [SerializeField]
    private Transform leftCam = default;

    [SerializeField]
    private Transform rightCam = default;

    [SerializeField]
    [Range(-5, 5)]
    private float offset = default;

    [SerializeField]
    [Range(-180, 180)]
    private float yRotation = default;

    private void Update()
    {
        leftCam.localPosition = new Vector3(-offset, 0, 0);
        rightCam.localPosition = new Vector3(offset, 0, 0);

        leftCam.localRotation = Quaternion.Euler(new Vector3(leftCam.localRotation.eulerAngles.x, -yRotation, 0));
        rightCam.localRotation = Quaternion.Euler(new Vector3(leftCam.localRotation.eulerAngles.x, yRotation, 0));
    }
}
