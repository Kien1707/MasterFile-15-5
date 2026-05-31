using UnityEngine;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Player

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0f, 2f, -12f);  // increased base distance
    public float mouseSensitivity = 120f;
    public float smoothSpeed = 0.15f;

    [Header("Zoom")]
    public float zoomSpeed = 5f;
    public float minDistance = 8f;      // closest zoom
    public float maxDistance = 25f;     // furthest zoom (increased)
    public float zoomSmoothness = 0.2f;

    [Header("Ground Clamp")]
    public float minCameraHeight = 0.5f; // camera will never go below this Y value

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

        HandleMouseLook();
        HandleZoom();
        FollowTarget();
        RotatePlayerWithCamera();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -30f, 60f);
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            currentZoomTarget -= scroll * zoomSpeed;
            currentZoomTarget = Mathf.Clamp(currentZoomTarget, minDistance, maxDistance);
        }
    }

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

        // Ground clamp – prevent camera from going underground
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