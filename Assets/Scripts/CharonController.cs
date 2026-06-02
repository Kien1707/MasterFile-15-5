<<<<<<< HEAD
﻿using UnityEngine;
using System.Collections;
=======
using UnityEngine;
>>>>>>> 1245ed6b7c4e71318d928ff5dd9c6c2b7d39b34e

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

<<<<<<< HEAD
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
=======
    [Header("Rotation Correction")]
    public Vector3 rotationOffset = new Vector3(0f, -90f, 0f);   // adjust until model faces forward

    private int state;
    private float angle;
    private bool angleInitialised = false;
>>>>>>> 1245ed6b7c4e71318d928ff5dd9c6c2b7d39b34e

    void Start()
    {
        transform.position = startPoint.position;
        transform.rotation = startPoint.rotation;
        state = 1;
<<<<<<< HEAD

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
=======
>>>>>>> 1245ed6b7c4e71318d928ff5dd9c6c2b7d39b34e
    }

    void Update()
    {
        if (state == 1)
<<<<<<< HEAD
            MoveToEntryPoint();
        else if (state == 2)
            Orbit();
=======
        {
            MoveToEntryPoint();
        }
        else if (state == 2)
        {
            Orbit();
        }
>>>>>>> 1245ed6b7c4e71318d928ff5dd9c6c2b7d39b34e
    }

    void MoveToEntryPoint()
    {
        Vector3 toPos = waterwayEndPoint.position;
        Vector3 dir = toPos - transform.position;
<<<<<<< HEAD

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

=======
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
>>>>>>> 1245ed6b7c4e71318d928ff5dd9c6c2b7d39b34e
            return;
        }

        Vector3 moveDir = dir.normalized;
        transform.position += moveDir * moveSpeed * Time.deltaTime;

<<<<<<< HEAD
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
=======
        Quaternion targetRot = Quaternion.LookRotation(moveDir) * Quaternion.Euler(rotationOffset);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
>>>>>>> 1245ed6b7c4e71318d928ff5dd9c6c2b7d39b34e
    }

    void Orbit()
    {
<<<<<<< HEAD
        angle += orbitSpeed * Time.deltaTime;

        Vector3 offset = new Vector3(
            Mathf.Cos(angle),
            0,
            Mathf.Sin(angle)
        ) * orbitRadius;

        Vector3 targetPos = islandCenter.position + offset;

        Vector3 moveDir = (targetPos - transform.position).normalized;

=======
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
>>>>>>> 1245ed6b7c4e71318d928ff5dd9c6c2b7d39b34e
        transform.position = targetPos;

        if (moveDir != Vector3.zero)
        {
<<<<<<< HEAD
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
=======
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
>>>>>>> 1245ed6b7c4e71318d928ff5dd9c6c2b7d39b34e
