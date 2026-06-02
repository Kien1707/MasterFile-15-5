using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class RestartUI : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup overlayPanel;
    public Button restartButton;

    [Header("Fade Settings")]
    public float fadeDuration = 2f;
    public float delayBeforeShow = 5f;
    public float targetAlpha = 0.8f;

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
            overlayPanel.blocksRaycasts = false;
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
            restartButton.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (fruitCounter == null) return;

        // Khi ending xảy ra → chuẩn bị hiện UI
        if (fruitCounter.EndingTriggered && !hasShown)
        {
            hasShown = true;
            isEnding = true;
            StartCoroutine(ShowAfterDelay());
        }

        // Khi UI đã hiện → bấm A để restart
        if (hasShown)
        {
            if (Input.GetKeyDown(KeyCode.JoystickButton0)) // A button
            {
                RestartGame();
            }
        }
    }

    IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeShow);
        ShowRestartUI();
    }

    void ShowRestartUI()
    {
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
            overlayPanel.alpha = Mathf.Lerp(0f, targetAlpha, t / fadeDuration);
            yield return null;
        }
        overlayPanel.alpha = targetAlpha;
    }

    void RestartGame()
    {
        PickableFruit.currentlyHeldFruit = null;
        PickableFruit.AnyFruitHeld = false;
        RenderSettings.fog = true;

        var allParticles = FindObjectsOfType<ParticleSystem>();
        foreach (var ps in allParticles)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene("Canvas UI", LoadSceneMode.Additive);
    }
}
