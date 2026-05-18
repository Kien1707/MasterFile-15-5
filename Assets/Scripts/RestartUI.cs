using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class RestartUI : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup overlayPanel;   // Black panel with 0.5 alpha
    public Button restartButton;

    [Header("Fade Settings")]
    public float fadeDuration = 1f;
    public float delayBeforeShow = 5f;

    private FruitCounter fruitCounter;
    private bool isEnding = false;
    private bool hasShown = false;

    void Start()
    {
        fruitCounter = FindFirstObjectByType<FruitCounter>();
        if (fruitCounter == null)
            Debug.LogError("RestartUI: FruitCounter not found!");

        if (overlayPanel != null)
        {
            overlayPanel.alpha = 0f;
            overlayPanel.gameObject.SetActive(false);
            // Ensure panel doesn't block clicks if we want button to work (it will be over panel anyway)
            overlayPanel.blocksRaycasts = false; // panel shouldn't block; button will handle clicks
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
            restartButton.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (fruitCounter == null || isEnding) return;

        if (fruitCounter.EndingTriggered && !hasShown)
        {
            hasShown = true;
            isEnding = true;
            StartCoroutine(ShowAfterDelay());
        }
    }

    IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeShow);
        ShowRestartUI();
    }

    void ShowRestartUI()
    {
        // Unlock the cursor so the player can click the button
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (overlayPanel != null)
        {
            overlayPanel.gameObject.SetActive(true);
            StartCoroutine(FadeInOverlay());
        }

        if (restartButton != null)
            restartButton.gameObject.SetActive(true);
    }

    IEnumerator FadeInOverlay()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            overlayPanel.alpha = Mathf.Lerp(0f, 0.5f, t / fadeDuration);
            yield return null;
        }
        overlayPanel.alpha = 0.5f;
    }

    void RestartGame()
    {
        // Reset static variables
        PickableFruit.currentlyHeldFruit = null;

        // Stop any playing particles
        var allParticles = FindObjectsOfType<ParticleSystem>();
        foreach (var ps in allParticles)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}