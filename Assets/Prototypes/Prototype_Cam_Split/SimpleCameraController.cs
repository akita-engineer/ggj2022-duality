using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SimpleCameraController : MonoBehaviour
{
    private Camera mCamera;

    private Vector3 prevMouseDirection;

    private Vector3 prevMousePosition;

    [SerializeField]
    private CameraSplit cameraSplit = default;
        
    [SerializeField]
    private bool allowMovement = default;
    
    [SerializeField]
    private float movementSpeed = default;
    
    [SerializeField]
    private float xAxisTurnSpeed = default;
    
    [SerializeField]
    private float yAxisTurnSpeed = default;

    private int firstTicksToSkip = 50;

    private void Awake()
    {
        mCamera = GetComponent<Camera>();
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (firstTicksToSkip > 0)
        {
            firstTicksToSkip--;
            return;
        }

        Vector3 mousePositionDelta = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0f);
        float yAxisTurn = mousePositionDelta.x;
        float xAxisTurn = -mousePositionDelta.y;

        float webRotationSpeedDampener = 1.0f;

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            webRotationSpeedDampener = 0.15f;
        }

        Vector3 angle = mCamera.transform.rotation.eulerAngles + new Vector3(xAxisTurn*xAxisTurnSpeed, yAxisTurn*yAxisTurnSpeed, 0) * Time.deltaTime * webRotationSpeedDampener;
        angle.z = 0f;
        mCamera.transform.rotation = Quaternion.Euler(angle);


        //Vector3 moveDirectionForward = mCamera.transform.forward;
        //Vector3 moveDirectionRight = mCamera.transform.right;
        Vector3 moveDirectionForward = cameraSplit.MoveDirection;
        Vector3 moveDirectionRight = cameraSplit.MoveDirectionRight;



        if (allowMovement)
        {
            if (Input.GetKey(KeyCode.W))
            {
                mCamera.transform.Translate(moveDirectionForward*movementSpeed*Time.deltaTime, Space.World);
            }

            if (Input.GetKey(KeyCode.S))
            {
                mCamera.transform.Translate(-moveDirectionForward*movementSpeed*Time.deltaTime, Space.World);
            }

            if (Input.GetKey(KeyCode.A))
            {
                mCamera.transform.Translate(-moveDirectionRight*movementSpeed*Time.deltaTime, Space.World);
            }

            if (Input.GetKey(KeyCode.D))
            {
                mCamera.transform.Translate(moveDirectionRight*movementSpeed*Time.deltaTime, Space.World);
            }
        }
    }
}