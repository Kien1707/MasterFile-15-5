using UnityEngine;

public class CharonController : MonoBehaviour
{
    [Header("Path Points")]
    public Transform startPoint;
    public Transform waterwayEndPoint;
    public Transform islandCenter;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float turnSpeed = 5f;
    public float reachDistance = 0.3f;

    [Header("Orbit")]
    public float orbitRadius = 10f;
    public float orbitSpeed = 1f;

    [Header("Rotation Correction")]
    public Vector3 rotationOffset = new Vector3(0f, -90f, 0f);   // adjust until model faces forward

    private int state;
    private float angle;
    private bool angleInitialised = false;

    void Start()
    {
        transform.position = startPoint.position;
        transform.rotation = startPoint.rotation;
        state = 1;
    }

    void Update()
    {
        if (state == 1)
        {
            MoveToEntryPoint();
        }
        else if (state == 2)
        {
            Orbit();
        }
    }

    void MoveToEntryPoint()
    {
        Vector3 toPos = waterwayEndPoint.position;
        Vector3 dir = toPos - transform.position;
        float distance = dir.magnitude;

        if (distance <= reachDistance)
        {
            transform.position = toPos;

            // Snap rotation to correct orbit‑facing direction
            Vector3 tangentDir = Vector3.Cross(Vector3.up, (transform.position - islandCenter.position).normalized);
            if (tangentDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(tangentDir) * Quaternion.Euler(rotationOffset);

            state = 2;
            angleInitialised = false;   // will be recalculated in Orbit()
            return;
        }

        Vector3 moveDir = dir.normalized;
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        Quaternion targetRot = Quaternion.LookRotation(moveDir) * Quaternion.Euler(rotationOffset);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
    }

    void Orbit()
    {
        // Initialise angle based on current position if not already done
        if (!angleInitialised)
        {
            Vector3 offsetFromCenter = transform.position - islandCenter.position;
            angle = Mathf.Atan2(offsetFromCenter.z, offsetFromCenter.x);
            angleInitialised = true;
        }

        angle += orbitSpeed * Time.deltaTime;

        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * orbitRadius;
        Vector3 targetPos = islandCenter.position + offset;

        Vector3 moveDir = (targetPos - transform.position).normalized;
        transform.position = targetPos;

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir) * Quaternion.Euler(rotationOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }
    }

    // Public methods for CutsceneManager
    public int GetState() { return state; }

    public void SetState(int newState)
    {
        if (newState == 2)
        {
            angleInitialised = false;   // force recalc on next Orbit()
        }
        state = newState;
    }
}