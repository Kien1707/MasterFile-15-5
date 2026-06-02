using UnityEngine;
using System.Collections;

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

    [Header("Fade Settings")]
    public Renderer charonRenderer;   // assign MeshRenderer
    public float fadeOutDuration = 5f;

    [Header("Player Ride Settings")]
    public PlayerMovement playerMovement;
    public Transform playerRidePoint;
    public Transform playerSpawnPoint;

    private int state;
    private float angle;
    private bool released = false;

    void Start()
    {
        transform.position = startPoint.position;
        transform.rotation = startPoint.rotation;
        state = 1;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;

            // Camera luôn parent vào player
            if (playerMovement.playerCamera != null)
                playerMovement.playerCamera.SetParent(playerMovement.transform, true);

            playerMovement.transform.position = playerRidePoint.position;
            playerMovement.transform.rotation = playerRidePoint.rotation;

            playerMovement.transform.SetParent(transform, true);
        }
    }

    void Update()
    {
        if (state == 1)
            MoveToEntryPoint();
        else if (state == 2)
            Orbit();
    }

    void MoveToEntryPoint()
    {
        Vector3 toPos = waterwayEndPoint.position;
        Vector3 dir = toPos - transform.position;

        if (dir.magnitude <= reachDistance)
        {
            transform.position = waterwayEndPoint.position;

            // Snap rotation hướng vào đảo
            Vector3 tangentDir = Vector3.Cross(
                Vector3.up,
                (transform.position - islandCenter.position).normalized
            );
            if (tangentDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(tangentDir);

            // Thả player
            ReleasePlayer();

            // 🔥 Dừng 3 giây → fade out 5 giây → orbit
            StartCoroutine(FadeOutThenOrbit());

            return;
        }

        Vector3 moveDir = dir.normalized;
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        Quaternion targetRot =
            Quaternion.LookRotation(moveDir) * Quaternion.Euler(0, -90f, 0);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            turnSpeed * Time.deltaTime
        );
    }

    IEnumerator FadeOutThenOrbit()
    {
        // 🔥 1. DỪNG 3 GIÂY
        yield return new WaitForSeconds(3f);

        // 🔥 2. FADE OUT 5 GIÂY
        yield return StartCoroutine(FadeOutCharon());

        // 🔥 3. BẮT ĐẦU XOAY
        state = 2;
    }

    IEnumerator FadeOutCharon()
    {
        float t = 0f;
        Material mat = charonRenderer.material;

        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeOutDuration);

            Color c = mat.color;
            c.a = alpha;
            mat.color = c;

            yield return null;
        }
    }

    void Orbit()
    {
        angle += orbitSpeed * Time.deltaTime;

        Vector3 offset = new Vector3(
            Mathf.Cos(angle),
            0,
            Mathf.Sin(angle)
        ) * orbitRadius;

        Vector3 targetPos = islandCenter.position + offset;

        Vector3 moveDir = (targetPos - transform.position).normalized;

        transform.position = targetPos;

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot =
                Quaternion.LookRotation(moveDir) * Quaternion.Euler(0, 90f, 0);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                turnSpeed * Time.deltaTime
            );
        }
    }

    void ReleasePlayer()
    {
        if (released) return;
        released = true;

        playerMovement.transform.SetParent(null);

        playerMovement.transform.position = playerSpawnPoint.position;
        playerMovement.transform.rotation = playerSpawnPoint.rotation;

        playerMovement.ResetCameraPitch();
        playerMovement.enabled = true;
    }
}
