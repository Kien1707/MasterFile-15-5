using UnityEngine;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0f, 2f, -12f);
    public float mouseSensitivity = 120f;
    public float smoothSpeed = 0.15f;

    [Header("Zoom")]
    public float zoomSpeed = 10f;
    public float scrollZoomSpeed = 5f;
    public float minDistance = 8f;
    public float maxDistance = 25f;
    public float zoomSmoothness = 0.2f;

    [Header("Ground Clamp")]
    public float minCameraHeight = 0.5f;

    private float currentZoomTarget;
    private float currentZoomVelocity;
    private float xRotation = 20f;
    private float yRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currentZoomTarget = offset.magnitude;
        StartCoroutine(InitialZoomOut());
    }

    IEnumerator InitialZoomOut()
    {
        float startDistance = 4f;
        float endDistance = currentZoomTarget;
        float duration = 3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            currentZoomTarget = Mathf.Lerp(startDistance, endDistance, t);
            yield return null;
        }

        currentZoomTarget = endDistance;
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleCameraLook();
        HandleZoom();
        FollowTarget();
        RotatePlayerWithCamera();
    }

    // ---------------------------------------------------------
    // CAMERA LOOK (Mouse ALWAYS works)
    // ---------------------------------------------------------
    void HandleCameraLook()
    {
        // Chuột luôn hoạt động
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Controller override (nếu có)
        float joyX = InputOverride.AxisOverride("Mouse X") * mouseSensitivity * Time.deltaTime;
        float joyY = InputOverride.AxisOverride("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Ưu tiên chuột nếu có input
        float finalX = Mathf.Abs(mouseX) > 0.001f ? mouseX : joyX;
        float finalY = Mathf.Abs(mouseY) > 0.001f ? mouseY : joyY;

        yRotation += finalX;
        xRotation -= finalY;
        xRotation = Mathf.Clamp(xRotation, -30f, 60f);
    }

    // ---------------------------------------------------------
    // ZOOM
    // ---------------------------------------------------------
    void HandleZoom()
    {
        float triggerZoom = InputOverride.ScrollOverride();
        float scrollZoom = Input.GetAxis("Mouse ScrollWheel");

        float zoomInput = 0f;

        if (Mathf.Abs(triggerZoom) > 0.1f)
            zoomInput += triggerZoom * zoomSpeed * Time.deltaTime;

        if (Mathf.Abs(scrollZoom) > 0.01f)
            zoomInput += scrollZoom * scrollZoomSpeed;

        if (Mathf.Abs(zoomInput) > 0.001f)
        {
            currentZoomTarget -= zoomInput;
            currentZoomTarget = Mathf.Clamp(currentZoomTarget, minDistance, maxDistance);
        }
    }

    // ---------------------------------------------------------
    // FOLLOW TARGET
    // ---------------------------------------------------------
    void FollowTarget()
    {
        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0f);

        float currentDistance = Mathf.SmoothDamp(
            GetCurrentDistance(),
            currentZoomTarget,
            ref currentZoomVelocity,
            zoomSmoothness
        );

        Vector3 zoomOffset = offset.normalized * currentDistance;
        Vector3 desiredPos = target.position + rotation * zoomOffset;

        if (desiredPos.y < minCameraHeight)
            desiredPos.y = minCameraHeight;

        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed);
        transform.LookAt(target);
    }

    float GetCurrentDistance()
    {
        if (target == null) return currentZoomTarget;
        return Vector3.Distance(transform.position, target.position);
    }

    // ---------------------------------------------------------
    // PLAYER ROTATION FOLLOWS CAMERA
    // ---------------------------------------------------------
    void RotatePlayerWithCamera()
    {
        Vector3 camForward = transform.forward;
        camForward.y = 0f;

        if (camForward.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(camForward);
            target.rotation = Quaternion.Slerp(target.rotation, targetRot, 10f * Time.deltaTime);
        }
    }
}
