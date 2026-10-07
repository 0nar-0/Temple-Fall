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

    [Header("Crouch")]
    [SerializeField] private float crouchSpeed = 3f;
    [SerializeField] private float crouchHeight = 1f;            // capsule height while crouching
    [SerializeField] private float crouchTransitionSpeed = 8f;   // how fast the capsule shrinks/grows

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float coyoteTime = 0.15f;      // can still jump shortly after leaving a ledge
    [SerializeField] private float jumpBufferTime = 0.15f;  // jump pressed slightly before landing still counts

    [Header("Animation")]
    [SerializeField] private float airAnimDelay = 0.1f;     // must be airborne this long before the jump animation plays

    private CharacterController controller;
    private Animator animator;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private float turnVelocity;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private float airTime;

    private float standingHeight;
    private float standingCenterY;
    private bool isCrouching;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        standingHeight = controller.height;
        standingCenterY = controller.center.y;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.applyRootMotion = false;
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

        // --- Crouch (hold C) ---
        if (kb.cKey.isPressed) isCrouching = true;
        else if (isCrouching && CanStandUp()) isCrouching = false; // stay crouched if something is above you

        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        controller.height = Mathf.MoveTowards(controller.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        // keep the bottom of the capsule in the same place while the height changes
        Vector3 c = controller.center;
        c.y = standingCenterY - (standingHeight - controller.height) / 2f;
        controller.center = c;

        // --- Timers (coyote + jump buffer + air time) ---
        if (controller.isGrounded)
        {
            coyoteTimer = coyoteTime;
            airTime = 0f;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
            airTime += Time.deltaTime;
        }

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

            float speed = isCrouching ? crouchSpeed : (sprint ? sprintSpeed : walkSpeed);
            targetVelocity = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward * speed;
        }

        // Less steering in the air, full control on the ground
        float control = controller.isGrounded ? 1f : airControl;
        horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetVelocity, control * 10f * Time.deltaTime);

        // --- Gravity & jump ---
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f; // small push keeps the player stuck to the ground

        if (!isCrouching && jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            airTime = airAnimDelay; // start the jump animation right away
        }

        verticalVelocity += gravity * Time.deltaTime;

        // --- Apply ---
        Vector3 finalVelocity = horizontalVelocity + Vector3.up * verticalVelocity;
        controller.Move(finalVelocity * Time.deltaTime);

        // --- Animation ---
        if (animator != null)
        {
            animator.SetFloat("Speed", horizontalVelocity.magnitude);
            // Only counts as "in the air" after a short delay, so tiny isGrounded flickers don't trigger the jump animation
            animator.SetBool("Grounded", airTime < airAnimDelay);
            animator.SetBool("Crouching", isCrouching);
        }
    }

    // Checks for a ceiling above the crouched capsule
    private bool CanStandUp()
    {
        float bottomY = transform.position.y + standingCenterY - standingHeight / 2f;
        Vector3 origin = new Vector3(transform.position.x, bottomY + crouchHeight - 0.1f, transform.position.z);
        float distance = (standingHeight - crouchHeight) + 0.15f;
        return !Physics.Raycast(origin, Vector3.up, distance, ~0, QueryTriggerInteraction.Ignore);
    }
}