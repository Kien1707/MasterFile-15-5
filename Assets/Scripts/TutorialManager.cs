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

    private int tutorialStep = 0;
    private bool isTransitioning = false;

    void Start()
    {
        HideInstant(zoomText);
        HideInstant(pickupFruitText);
        HideInstant(openFruitText);

        StartCoroutine(FadeIn(zoomText));
    }

    void Update()
    {
        if (isTransitioning) return;

        switch (tutorialStep)
        {
            case 0: // Zoom instruction – wait for scroll wheel
                if (Input.GetAxis("Mouse ScrollWheel") != 0f)
                {
                    StartCoroutine(TransitionTutorial(zoomText, pickupFruitText));
                    tutorialStep++;
                }
                break;

            case 1: // Pick up fruit instruction – wait for fruit held
                if (PickableFruit.AnyFruitHeld)
                {
                    StartCoroutine(TransitionTutorial(pickupFruitText, openFruitText));
                    tutorialStep++;
                }
                break;

            case 2: // Open fruit instruction – wait for F key
                if (Input.GetKeyDown(KeyCode.F))
                {
                    StartCoroutine(FadeOut(openFruitText));
                    tutorialStep++;
                }
                break;
        }
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