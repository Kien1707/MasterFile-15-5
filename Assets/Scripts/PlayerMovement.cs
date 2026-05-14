using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 2f;

    [Header("Double Tap Settings")]
    public float doubleTapTime = 0.3f;

    [Header("Jump")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Camera Control")]
    public Transform playerCamera;
    public float rotationSmoothTime = 0.1f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 120f;
    public float mouseSmoothTime = 0.05f;

    private CharacterController controller;
    private float verticalVelocity;
    private float currentVelocity;

    private float lastWPressTime = -1f;
    private bool isSprinting = false;

    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;
    private float cameraPitch = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleCameraLook();
        HandleDoubleTapSprint();
        RotatePlayerToCamera();
        MovePlayer();
    }

    // ---------------------------------------------------------
    // SMOOTH CAMERA LOOK (NO SHAKE, NO SPRING)
    // ---------------------------------------------------------
    void HandleCameraLook()
    {
        Vector2 targetMouseDelta = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );

        currentMouseDelta = Vector2.SmoothDamp(
            currentMouseDelta,
            targetMouseDelta,
            ref currentMouseDeltaVelocity,
            mouseSmoothTime
        );

        cameraPitch -= currentMouseDelta.y * mouseSensitivity * Time.deltaTime;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);

        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

        transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSensitivity * Time.deltaTime);
    }

    // ---------------------------------------------------------
    // DOUBLE TAP W → SPRINT
    // ---------------------------------------------------------
    void HandleDoubleTapSprint()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (Time.time - lastWPressTime <= doubleTapTime)
                isSprinting = true;

            lastWPressTime = Time.time;
        }

        if (!Input.GetKey(KeyCode.W))
            isSprinting = false;
    }

    // ---------------------------------------------------------
    // PLAYER ROTATION FOLLOWS CAMERA
    // ---------------------------------------------------------
    void RotatePlayerToCamera()
    {
        Vector3 camForward = playerCamera.forward;
        camForward.y = 0f;

        if (camForward.sqrMagnitude < 0.01f)
            return;

        float targetAngle = Mathf.Atan2(camForward.x, camForward.z) * Mathf.Rad2Deg;

        float angle = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetAngle,
            ref currentVelocity,
            rotationSmoothTime
        );

        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    // ---------------------------------------------------------
    // MOVEMENT (WASD theo hướng camera)
    // ---------------------------------------------------------
    void MovePlayer()
    {
        bool grounded = controller.isGrounded;

        if (grounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        if (Input.GetButtonDown("Jump") && grounded)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVelocity += gravity * Time.deltaTime;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 forward = playerCamera.forward;
        Vector3 right = playerCamera.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * z + right * x;

        float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        Vector3 finalMove = move.normalized * speed;
        finalMove.y = verticalVelocity;

        controller.Move(finalMove * Time.deltaTime);
    }
}
