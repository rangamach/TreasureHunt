using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerView : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset;

    [Header("Input (Vector2 action)")]
    [SerializeField] private InputActionReference lookAction;   // Mouse delta / right stick
    [SerializeField] private InputActionReference movementAction;

    [Header("Orbit")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 70f;

    private Camera mainCamera;
    private float yaw;
    private float pitch;

    [Header("Animation")]
    [SerializeField] private Animator anim;
    [SerializeField] private float rotationSpeed = 540f;   // degrees per second
    [SerializeField] private float speedDampTime = 0.1f;   // smoothing for the blend tree

    private const float BackSpeed = 0f;
    private const float IdleSpeed = 0.5f;
    private const float ForwardSpeed = 1f;

    [Header("Locomotion")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float backwardMoveSpeed = 2f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private CharacterController controller;
    
    private float verticalVelocity;

    private void Awake()
    {
        mainCamera = Camera.main;

        Vector3 angles = mainCamera.transform.eulerAngles;
        yaw = angles.y;
        pitch = Mathf.Clamp(Mathf.DeltaAngle(0f, angles.x), minPitch, maxPitch);

        SetAnimSpeed(IdleSpeed);
    }

    private void OnEnable()
    {
        if (lookAction != null) lookAction.action.Enable();
        if (movementAction != null) movementAction.action.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        if (lookAction != null) lookAction.action.Disable();
        if (movementAction != null) movementAction.action.Disable();
    }

    private void Update()
    {
        PlayerMovement();
    }
    // LateUpdate so the camera moves after the player has moved this frame
    private void LateUpdate()
    {
        CameraMovement();
    }
    private void CameraMovement()
    {
        if (target == null || mainCamera == null) return;

        // 1) Read input
        if (lookAction != null)
        {
            Vector2 look = lookAction.action.ReadValue<Vector2>();
            yaw += look.x * mouseSensitivity;
            pitch -= look.y * mouseSensitivity;

            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // 2) Build rotation and position the camera behind the pivot
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pivot = target.position + targetOffset;
        Vector3 position = pivot - rotation * Vector3.forward;

        // 3) Apply
        mainCamera.transform.SetPositionAndRotation(position, rotation);
    }
    private void PlayerMovement()
    {
        if (movementAction == null || anim == null || mainCamera == null || controller == null) return;

        Vector2 input = movementAction.action.ReadValue<Vector2>();

        bool forward = input.y > 0.1f;
        bool backward = input.y < -0.1f;
        bool sideways = Mathf.Abs(input.x) > 0.1f;
        bool hasInput = forward || backward || sideways;

        float targetSpeed = IdleSpeed;
        if (backward) targetSpeed = BackSpeed;
        else if (forward || sideways) targetSpeed = ForwardSpeed;

        SetAnimSpeed(targetSpeed);

        Vector3 horizontalVelocity = Vector3.zero;

        if (hasInput)
        {
            Vector3 camForward = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(mainCamera.transform.right, Vector3.up).normalized;

            // Direction the player travels, relative to the camera
            Vector3 moveDir = Vector3.ClampMagnitude(camRight * input.x + camForward * input.y, 1f);

            // Going backwards: face the opposite way so the back animation looks right
            Vector3 faceDir = backward ? -moveDir : moveDir;

            if (faceDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(faceDir.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            float speed = backward ? backwardMoveSpeed : moveSpeed;
            horizontalVelocity = moveDir * speed;
        }

        // Gravity
        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }
    private void SetAnimSpeed(float speed) => anim.SetFloat("Speed", speed);
}