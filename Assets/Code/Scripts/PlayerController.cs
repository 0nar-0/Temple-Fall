using UnityEngine;
using UnityEngine.InputSystem;

// Attach to a Capsule/Cube with a CharacterController component.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform; // drag Main Camera here

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    [SerializeField] private float airControl = 0.6f; // 1 = full control in air

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float coyoteTime = 0.15f;      // can still jump shortly after leaving a ledge
    [SerializeField] private float jumpBufferTime = 0.15f;  // jump pressed slightly before landing still counts

    private CharacterController controller;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private float turnVelocity;
    private float coyoteTimer;
    private float jumpBufferTimer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        // --- Input ---
        Keyboard kb = Keyboard.current;
        if (kb == null) return; // no keyboard connected

        float h = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float v = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
        Vector3 input = new Vector3(h, 0f, v).normalized;
        bool sprint = kb.leftShiftKey.isPressed;

        // --- Timers (coyote + jump buffer) ---
        if (controller.isGrounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.deltaTime;

        if (kb.spaceKey.wasPressedThisFrame) jumpBufferTimer = jumpBufferTime;
        else jumpBufferTimer -= Time.deltaTime;

        // --- Horizontal movement (relative to camera) ---
        Vector3 targetVelocity = Vector3.zero;
        if (input.sqrMagnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg;
            if (cameraTransform != null) targetAngle += cameraTransform.eulerAngles.y;

            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            float speed = sprint ? sprintSpeed : walkSpeed;
            targetVelocity = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward * speed;
        }

        // Less steering in the air, full control on the ground
        float control = controller.isGrounded ? 1f : airControl;
        horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetVelocity, control * 10f * Time.deltaTime);

        // --- Gravity & jump ---
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f; // small push keeps the player stuck to the ground

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        // --- Apply ---
        Vector3 finalVelocity = horizontalVelocity + Vector3.up * verticalVelocity;
        controller.Move(finalVelocity * Time.deltaTime);
    }
}