using UnityEngine;
using System.Collections;
using System.Collections;

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

    [Header("Sound")]
    public PlayerSound sound;

    [Header("Ghost Animation")]
    public Animator ghostAnimator;

    private CharacterController controller;
    private float verticalVelocity;
    private float currentVelocity;

    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;
    private float cameraPitch = 0f;

    private bool isFootstepPlaying = false;

    // Sprint logic
    private float lastWPressTime = -1f;
    private bool isSprintingKeyboard = false;
    private bool isSprintingController = false;

    // Animation flags
    private bool isJumping = false;
    private bool isPickingUp = false;

    private Coroutine jumpRoutine;
    private Coroutine pickupRoutine;

    private static readonly int IdleState = Animator.StringToHash("Base Layer.idle");
    private static readonly int MoveState = Animator.StringToHash("Base Layer.move");
    private static readonly int SurprisedState = Animator.StringToHash("Base Layer.surprised");
    private static readonly int AttackState = Animator.StringToHash("Base Layer.attack");

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleCameraLook();
<<<<<<< HEAD
        HandleSprintLogic();
        RotatePlayerToCamera();
=======
        HandleDoubleTapSprint();
        // RotatePlayerToCamera();
>>>>>>> 1245ed6b7c4e71318d928ff5dd9c6c2b7d39b34e
        MovePlayer();
        HandleFootstep();
        HandleGhostAnimation();
    }

    // ---------------------------------------------------------
    // CAMERA LOOK
    // ---------------------------------------------------------
    void HandleCameraLook()
    {
        float mouseX = InputOverride.AxisOverride("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = InputOverride.AxisOverride("Mouse Y") * mouseSensitivity * Time.deltaTime;

        currentMouseDelta = Vector2.SmoothDamp(
            currentMouseDelta,
            new Vector2(mouseX, mouseY),
            ref currentMouseDeltaVelocity,
            mouseSmoothTime
        );

        cameraPitch -= currentMouseDelta.y;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);

        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
<<<<<<< HEAD
        transform.Rotate(Vector3.up * currentMouseDelta.x);
=======

        // Remove this line ↓
        // transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSensitivity * Time.deltaTime);
>>>>>>> 1245ed6b7c4e71318d928ff5dd9c6c2b7d39b34e
    }

    // ---------------------------------------------------------
    // SPRINT LOGIC
    // ---------------------------------------------------------
    void HandleSprintLogic()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (Time.time - lastWPressTime <= doubleTapTime)
                isSprintingKeyboard = true;

            lastWPressTime = Time.time;
        }

        if (!Input.GetKey(KeyCode.W))
            isSprintingKeyboard = false;

        float zController = InputOverride.AxisOverride("Vertical");
        bool LB = Input.GetKey(KeyCode.JoystickButton4);

        isSprintingController = (zController > 0.7f && LB);
    }

    // ---------------------------------------------------------
    // PLAYER ROTATION
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
    // MOVEMENT
    // ---------------------------------------------------------
    void MovePlayer()
    {
        bool grounded = controller.isGrounded;

        if (grounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        bool jumpPressed =
            Input.GetKeyDown(KeyCode.Space) ||
            InputOverride.KeyDownOverride(KeyCode.Space);

        if (jumpPressed && grounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
<<<<<<< HEAD
            PlayJumpAnimation();
        }
=======
            GetComponent<Sample.GhostScript>()?.OnJump();
>>>>>>> 1245ed6b7c4e71318d928ff5dd9c6c2b7d39b34e

        verticalVelocity += gravity * Time.deltaTime;

        float xKeyboard = Input.GetAxis("Horizontal");
        float zKeyboard = Input.GetAxis("Vertical");

        float xController = InputOverride.AxisOverride("Horizontal");
        float zController = InputOverride.AxisOverride("Vertical");

        float x = Mathf.Abs(xController) > 0.1f ? xController : xKeyboard;
        float z = Mathf.Abs(zController) > 0.1f ? zController : zKeyboard;

        Vector3 forward = playerCamera.forward;
        Vector3 right = playerCamera.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * z + right * x;

        bool sprinting = isSprintingKeyboard || isSprintingController;

        float speed = sprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        Vector3 finalMove = move.normalized * speed;
        finalMove.y = verticalVelocity;

        controller.Move(finalMove * Time.deltaTime);
    }

    // ---------------------------------------------------------
    // FOOTSTEP
    // ---------------------------------------------------------
    void HandleFootstep()
    {
        bool isMovingKeyboard =
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.D);

        bool isMovingController =
            Mathf.Abs(InputOverride.AxisOverride("Horizontal")) > 0.2f ||
            Mathf.Abs(InputOverride.AxisOverride("Vertical")) > 0.2f;

        bool isMoving = isMovingKeyboard || isMovingController;

        if (isMoving && controller.isGrounded)
        {
            if (!isFootstepPlaying)
            {
                AudioClip clip = sound.library.GetClip(PlayerAction.FootstepsGrass);

                sound.audioSource.clip = clip;
                sound.audioSource.loop = true;

                float randomStart = Random.Range(0f, clip.length);
                sound.audioSource.time = randomStart;

                sound.audioSource.Play();
                isFootstepPlaying = true;
            }
        }
        else
        {
            if (isFootstepPlaying)
            {
                sound.audioSource.Stop();
                isFootstepPlaying = false;
            }
        }
    }

    // ---------------------------------------------------------
    // GHOST ANIMATION
    // ---------------------------------------------------------
    void HandleGhostAnimation()
    {
        if (isJumping || isPickingUp)
            return;

        bool moving =
            Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f ||
            Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f ||
            Mathf.Abs(InputOverride.AxisOverride("Horizontal")) > 0.1f ||
            Mathf.Abs(InputOverride.AxisOverride("Vertical")) > 0.1f;

        if (moving)
            ghostAnimator.CrossFade(MoveState, 0.1f);
        else
            ghostAnimator.CrossFade(IdleState, 0.1f);
    }

    public void PlayJumpAnimation()
    {
        if (jumpRoutine != null) StopCoroutine(jumpRoutine);

        isJumping = true;
        ghostAnimator.CrossFade(AttackState, 0.1f);

        jumpRoutine = StartCoroutine(JumpAnim());
    }

    IEnumerator JumpAnim()
    {
        yield return new WaitForSeconds(0.6f);
        isJumping = false;
    }

    public void PlayPickupAnimation()
    {
        if (pickupRoutine != null) StopCoroutine(pickupRoutine);

        isPickingUp = true;
        ghostAnimator.CrossFade(SurprisedState, 0.1f);

        pickupRoutine = StartCoroutine(PickupAnim());
    }

    IEnumerator PickupAnim()
    {
        yield return new WaitForSeconds(1f);
        isPickingUp = false;
    }
    public void ResetCameraPitch()
{
    cameraPitch = 0f;

    if (playerCamera != null)
        playerCamera.localRotation = Quaternion.Euler(0f, 0f, 0f);
}

}
