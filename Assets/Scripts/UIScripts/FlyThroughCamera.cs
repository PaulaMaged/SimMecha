using UnityEngine;
using UnityEngine.EventSystems;

public class FlyThroughCamera : MonoBehaviour
{
    public float movementSpeed = 10.0f;
    public float lookSpeed = 2.0f;
    public float zoomSpeed = 2.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    void Update()
    {
        // Movement
        float translation = Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime; // Forward/Backward
        float strafe = Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime; // Left/Right
        float vertical = 0;

        if (Input.GetKey(KeyCode.E)) // Up
        {
            vertical = movementSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.Q)) // Down
        {
            vertical = -movementSpeed * Time.deltaTime;
        }

        transform.Translate(strafe, vertical, translation);

        // Rotation
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            yaw += lookSpeed * Input.GetAxis("Mouse X");
            pitch -= lookSpeed * Input.GetAxis("Mouse Y");
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }

        // Zoom
        if (!IsMouseOverUI())
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            transform.Translate(0, 0, scroll);
        }
    }

    // Method to check if the mouse is over a UI element
    private bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
