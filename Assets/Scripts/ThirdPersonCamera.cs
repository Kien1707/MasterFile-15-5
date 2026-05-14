using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Player

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0f, 2f, -4f);
    public float mouseSensitivity = 120f;
    public float smoothSpeed = 0.15f;

    [Header("Zoom")]
    public float zoomSpeed = 5f;
    public float minDistance = 2f;
    public float maxDistance = 10f;

    private float currentDistance;
    private float xRotation = 20f;
    private float yRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currentDistance = offset.magnitude;
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleMouseLook();
        HandleZoom();
        FollowTarget();
        RotatePlayerWithCamera();
    }

    // ---------------------------------------------------------
    // 1. MOUSE LOOK
    // ---------------------------------------------------------
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -30f, 60f);
    }

    // ---------------------------------------------------------
    // 2. ZOOM
    // ---------------------------------------------------------
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            currentDistance -= scroll * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }
    }

    // ---------------------------------------------------------
    // 3. CAMERA FOLLOW
    // ---------------------------------------------------------
    void FollowTarget()
    {
        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        Vector3 zoomOffset = offset.normalized * currentDistance;

        Vector3 desiredPos = target.position + rotation * zoomOffset;

        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed);
        transform.LookAt(target);
    }

    // ---------------------------------------------------------
    // 4. CAMERA → PLAYER ROTATION (Y‑AXIS ONLY)
    // ---------------------------------------------------------
    void RotatePlayerWithCamera()
    {
        Vector3 camForward = transform.forward;
        camForward.y = 0f;

        if (camForward.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(camForward);
            target.rotation = Quaternion.Slerp(
                target.rotation,
                targetRot,
                10f * Time.deltaTime
            );
        }
    }
}
