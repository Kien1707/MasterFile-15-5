using UnityEngine;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial UI")]
    public CanvasGroup zoomText;
    public CanvasGroup pickupFruitText;
    public CanvasGroup moveToClusterText;
    public CanvasGroup openFruitText;

    [Header("Settings")]
    public float fadeDuration = 1f;
    public float delayBetweenTutorials = 0.5f;
    public float clusterRange = 3f;

    private int tutorialStep = 0;
    private bool isTransitioning = false;
    private bool hasReachedCluster = false;

    private Transform player;
    private GameObject[] clusters;

    void Start()
    {
        HideInstant(zoomText);
        HideInstant(pickupFruitText);
        HideInstant(moveToClusterText);
        HideInstant(openFruitText);

        // CACHE ONCE (IMPORTANT)
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
        else
            Debug.LogError("NO PLAYER FOUND. Check tag 'Player'.");

        clusters = GameObject.FindGameObjectsWithTag("Cluster");

        if (clusters.Length == 0)
            Debug.LogError("NO CLUSTERS FOUND. Check tag 'Cluster'.");

        StartCoroutine(FadeIn(zoomText));
    }

    void Update()
    {
        if (isTransitioning)
            return;

        switch (tutorialStep)
        {
            case 0:
                if (Input.GetAxis("Mouse ScrollWheel") != 0f)
                {
                    StartCoroutine(TransitionTutorial(zoomText, pickupFruitText));
                    tutorialStep++;
                }
                break;

            case 1:
                if (PickableFruit.AnyFruitHeld)
                {
                    StartCoroutine(TransitionTutorial(pickupFruitText, moveToClusterText));
                    tutorialStep++;
                }
                break;

            case 2:
                hasReachedCluster = false; // reset every frame
                CheckClusters();

                if (hasReachedCluster)
                {
                    StartCoroutine(TransitionTutorial(moveToClusterText, openFruitText));
                    tutorialStep++;
                }
                break;

            case 3:
                if (Input.GetKeyDown(KeyCode.F))
                {
                    StartCoroutine(FadeOut(openFruitText));
                    tutorialStep++;
                }
                break;
        }
    }

        void CheckClusters()
    {

        Collider[] hits = Physics.OverlapSphere(player.position, clusterRange);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Cluster"))
            {
                if (!hasReachedCluster)
                    Debug.Log("CLUSTER REACHED");

                hasReachedCluster = true;
                return;
            }
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