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

    [Header("Auto Follow")]
    [SerializeField] private bool autoFollow = true;
    [SerializeField] private float followSpeedThreshold = 7f; // camera follows only above this speed (walk = 6, sprint = 10). Use 0.5 to follow while walking too
    [SerializeField] private float followSpeed = 2.5f;        // higher = camera swings behind the player faster
    [SerializeField] private float mouseIdleDelay = 0.5f;     // seconds after moving the mouse before auto follow kicks in

    private float yaw;
    private float pitch = 15f;
    private float lastMouseTime;
    private CharacterController targetController;

    private void Start()
    {
        yaw = transform.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked; // hides the mouse; press Esc to get it back
        Cursor.visible = false;

        if (target != null) targetController = target.GetComponent<CharacterController>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            if (mouseDelta.sqrMagnitude > 0.01f) lastMouseTime = Time.time;

            yaw += mouseDelta.x * mouseSensitivity;
            pitch -= mouseDelta.y * mouseSensitivity;
        }
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Auto follow: swing the camera toward the direction the player faces while moving fast
        if (autoFollow && targetController != null && Time.time - lastMouseTime > mouseIdleDelay)
        {
            Vector3 vel = targetController.velocity;
            vel.y = 0f;

            if (vel.magnitude > followSpeedThreshold)
                yaw = Mathf.LerpAngle(yaw, target.eulerAngles.y, followSpeed * Time.deltaTime);
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focusPoint = target.position + Vector3.up * height;

        transform.position = focusPoint - rotation * Vector3.forward * distance;
        transform.rotation = rotation;
    }
}