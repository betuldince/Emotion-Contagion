using UnityEngine;

public class CameraControls : MonoBehaviour
{
    public float moveSpeed = 10.0f; // Speed of movement
    public float lookSpeed = 2.0f;  // Speed of looking around
    public float zoomSpeed = 5.0f;  // Speed of zoom

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    void Start()
    {
        // Lock the cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Handle camera movement
        HandleMovement();

        // Handle camera rotation
        HandleLook();

        // Handle zoom (optional)
        HandleZoom();
    }

    void HandleMovement()
    {
        Vector3 move = Vector3.zero;

        // Check if Shift is being held down to increase move speed
        float currentMoveSpeed = Input.GetKey(KeyCode.LeftShift) ? moveSpeed * 2 : moveSpeed;

        // Move forward with W
        if (Input.GetKey(KeyCode.W))
        {
            move += transform.forward;
        }

        // Move backward with S
        if (Input.GetKey(KeyCode.S))
        {
            move -= transform.forward;
        }

        // Move right with D
        if (Input.GetKey(KeyCode.D))
        {
            move += transform.right;
        }

        // Move left with A
        if (Input.GetKey(KeyCode.A))
        {
            move -= transform.right;
        }

        // Apply movement with the adjusted speed
        transform.position += move * currentMoveSpeed * Time.deltaTime;

        // Ascend with Q and descend with E
        if (Input.GetKey(KeyCode.E))
        {
            transform.position += Vector3.up * currentMoveSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            transform.position -= Vector3.up * currentMoveSpeed * Time.deltaTime;
        }
    }

    void HandleLook()
    {
        yaw += lookSpeed * Input.GetAxis("Mouse X");
        pitch -= lookSpeed * Input.GetAxis("Mouse Y");

        pitch = Mathf.Clamp(pitch, -90f, 90f); // Prevent flipping the camera upside down

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.position += transform.forward * scroll * zoomSpeed * Time.deltaTime;
    }

    void OnDisable()
    {
        // Unlock and show cursor when the script is disabled or the game ends
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
