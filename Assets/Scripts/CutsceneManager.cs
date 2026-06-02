using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    [Header("Cameras")]
    public Camera cutsceneCamera;
    public Camera playerCamera;

    [Header("Fade Settings")]
    public float fadeDuration = 5f;          // fade in/out duration
    public float cutsceneDelay = 5f;         // how long the cutscene plays (after fade‑in)

    [Header("Boat Teleport (after cutscene)")]
    public bool teleportBoat = true;
    public CharonController charonBoat;
    public Transform boatOrbitStartPoint;    // where the boat should appear (on the shore, facing orbit direction)

    [Header("Rotation Offset (for Charon)")]
    public Vector3 rotationOffset = new Vector3(0f, -90f, 0f);   // adjust in Inspector

    private CanvasGroup fadeCanvasGroup;

    void Start()
    {
        // Create full‑screen fade canvas
        GameObject fadeObj = new GameObject("FadeCanvas");
        Canvas canvas = fadeObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        fadeCanvasGroup = fadeObj.AddComponent<CanvasGroup>();
        CanvasScaler scaler = fadeObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        Image image = fadeObj.AddComponent<Image>();
        image.color = Color.black;

        DontDestroyOnLoad(fadeObj);

        StartCoroutine(CutsceneSequence());
    }

    IEnumerator CutsceneSequence()
    {
        // Activate cutscene camera, disable player camera
        if (cutsceneCamera != null) cutsceneCamera.enabled = true;
        if (playerCamera != null) playerCamera.enabled = false;

        // Fade in from black
        fadeCanvasGroup.alpha = 1f;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;

        // Wait for the cutscene to play (boat moves, camera flies, etc.)
        yield return new WaitForSeconds(cutsceneDelay);

        // Fade out to black
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;

        // --- Teleport boat to the shore orbit start ---
        if (teleportBoat && charonBoat != null && boatOrbitStartPoint != null)
        {
            charonBoat.transform.position = boatOrbitStartPoint.position;
            charonBoat.transform.rotation = boatOrbitStartPoint.rotation;
            charonBoat.SetState(2);   // switch to orbiting state
        }

        // Switch cameras
        if (cutsceneCamera != null) cutsceneCamera.enabled = false;
        if (playerCamera != null) playerCamera.enabled = true;

        // Fade back in to reveal game world
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;

        Destroy(fadeCanvasGroup.gameObject);
    }
    
}