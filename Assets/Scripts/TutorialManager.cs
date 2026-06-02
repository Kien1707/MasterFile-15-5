using UnityEngine;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial UI")]
    public CanvasGroup zoomText;
    public CanvasGroup pickupFruitText;
    public CanvasGroup openFruitText;

    [Header("Settings")]
    public float fadeDuration = 1f;
    public float delayBetweenTutorials = 0.5f;
    public float initialDelay = 5f;       // ← NEW: delay before showing first tip

    private int tutorialStep = 0;
    private bool isTransitioning = false;
    private bool tutorialActive = false;   // ← NEW: prevents update checks before delay

    void Start()
    {
        HideInstant(zoomText);
        HideInstant(pickupFruitText);
        HideInstant(openFruitText);

        StartCoroutine(DelayedTutorialStart());
    }

    IEnumerator DelayedTutorialStart()
    {
        yield return new WaitForSeconds(initialDelay);
        tutorialActive = true;
        StartCoroutine(FadeIn(zoomText));
    }

    void Update()
    {
        if (!tutorialActive || isTransitioning) return;

        switch (tutorialStep)
        {
            // ============================
            // STEP 0 — ZOOM
            // ============================
            case 0:
                if (DidZoom())
                {
                    StartCoroutine(TransitionTutorial(zoomText, pickupFruitText));
                    tutorialStep++;
                }
                break;

            // ============================
            // STEP 1 — PICK FRUIT
            // ============================
            case 1:
                if (PickableFruit.AnyFruitHeld)
                {
                    StartCoroutine(TransitionTutorial(pickupFruitText, openFruitText));
                    tutorialStep++;
                }
                break;

            // ============================
            // STEP 2 — OPEN FRUIT
            // ============================
            case 2:
                if (DidOpenFruit())
                {
                    StartCoroutine(FadeOut(openFruitText));
                    tutorialStep++;
                }
                break;
        }
    }

    // ============================
    // DETECT ZOOM (Mouse + Controller)
    // ============================
    bool DidZoom()
    {
        // Chuột
        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
            return true;

        // Controller (LT/RT → scroll override)
        if (Input.GetAxis("LT") > 0.2f) return true;
        if (Input.GetAxis("RT") > 0.2f) return true;

        return false;
    }

    // ============================
    // DETECT OPEN FRUIT (F + B)
    // ============================
    bool DidOpenFruit()
    {
        // Bàn phím
        if (Input.GetKeyDown(KeyCode.F))
            return true;

        // Controller (B = JoystickButton1)
        if (Input.GetKeyDown(KeyCode.JoystickButton1))
            return true;

        return false;
    }

    IEnumerator TransitionTutorial(CanvasGroup current, CanvasGroup next)
    {
        isTransitioning = true;

        yield return FadeOut(current);
        yield return new WaitForSeconds(delayBetweenTutorials);
        yield return FadeIn(next);

        isTransitioning = false;
    }

    IEnumerator FadeIn(CanvasGroup cg)
    {
        cg.gameObject.SetActive(true);
        float t = 0;
        cg.alpha = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 1;
    }

    IEnumerator FadeOut(CanvasGroup cg)
    {
        float t = 0;
        cg.alpha = 1;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 0;
        cg.gameObject.SetActive(false);
    }

    void HideInstant(CanvasGroup cg)
    {
        cg.alpha = 0;
        cg.gameObject.SetActive(false);
    }
}
