using UnityEngine;
using UnityEngine.InputSystem;

// Attach to the Main Camera (NOT as a child of the player).
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;          // drag the Player here
    [SerializeField] private float distance = 6f;       // how far behind the player
    [SerializeField] private float height = 1.5f;       // look-at point above the player's feet
    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float minPitch = -20f;
    [SerializeField] private float maxPitch = 70f;

    private float yaw;
    private float pitch = 15f;

    private void Start()
    {
        yaw = transform.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked; // hides the mouse; press Esc to get it back
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            yaw += mouseDelta.x * mouseSensitivity;
            pitch -= mouseDelta.y * mouseSensitivity;
        }
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focusPoint = target.position + Vector3.up * height;

        transform.position = focusPoint - rotation * Vector3.forward * distance;
        transform.rotation = rotation;
    }
}