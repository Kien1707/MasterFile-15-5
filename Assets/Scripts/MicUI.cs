using UnityEngine;

public class MicUI : MonoBehaviour
{
    [Header("Bars (UI)")]
    public RectTransform[] bars;

    [Header("World Objects (7 slots)")]
    public Transform[] yAxisObjects = new Transform[7];

    [Header("UI Fade")]
    public CanvasGroup canvasGroup;
    public float idleAlpha = 0.5f;      // opacity when not holding fruit
    public float activeAlpha = 1f;      // opacity when holding fruit
    public float fadeSpeed = 5f;

    [Header("Bar Heights")]
    public float minHeight = 10f;
    public float maxHeight = 150f;

    [Header("Y‑Axis Manipulation")]
    public float minY = 0.2f;
    public float maxY = 2.0f;

    [Header("Animation Speeds")]
    public float attackSpeed = 25f;   // rise fast
    public float releaseSpeed = 6f;   // fall slow

    [Header("Loudness Boost")]
    public float loudnessMultiplier = 3f;

    [Header("UI Scale (make mic smaller)")]
    public float uiScale = 0.5f;      // reduces maxHeight and maxY

    private bool wasActiveLastFrame = false;
    private FruitCounter fruitCounter;
    private bool endingTriggered = false;

    void Start()
    {
        fruitCounter = FindFirstObjectByType<FruitCounter>();
        if (fruitCounter != null)
            endingTriggered = fruitCounter.EndingTriggered;
        else
            Debug.LogWarning("FruitCounter not found, ending detection disabled.");

        canvasGroup.alpha = idleAlpha;
        ResetBars();
        ResetObjects();
    }

    void Update()
    {
        // Update ending flag
        if (fruitCounter != null)
            endingTriggered = fruitCounter.EndingTriggered;

        // If ending triggered, fade out and disable completely
        if (endingTriggered)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed);
            return; // skip all further updates
        }

        bool isActive = PickableFruit.AnyFruitHeld;

        // Fade UI based on fruit held state
        float targetAlpha = isActive ? activeAlpha : idleAlpha;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

        // Reset when just grabbed fruit
        if (isActive && !wasActiveLastFrame)
        {
            ResetBars();
            ResetObjects();
        }
        // Reset when just released fruit
        if (!isActive && wasActiveLastFrame)
        {
            ResetBars();
            ResetObjects();
        }

        wasActiveLastFrame = isActive;

        // Only animate bars and objects if a fruit is being held
        if (isActive)
        {
            float loudness = Mathf.Clamp01(MicInput.Loudness * loudnessMultiplier);

            // Apply UI scale to bar heights
            float currentMaxHeight = maxHeight * uiScale;
            float currentMinHeight = minHeight * uiScale;
            float currentMinY = minY;
            float currentMaxY = maxY * uiScale;

            // --- UI Bars ---
            for (int i = 0; i < bars.Length; i++)
            {
                if (bars[i] == null) continue;

                float noise = Mathf.PerlinNoise(i * 0.4f, Time.time * 8f) * 1.4f;
                float targetHeight = Mathf.Lerp(currentMinHeight, currentMaxHeight, loudness * noise);

                Vector2 size = bars[i].sizeDelta;
                float speed = targetHeight > size.y ? attackSpeed : releaseSpeed;
                size.y = Mathf.Lerp(size.y, targetHeight, Time.deltaTime * speed);
                bars[i].sizeDelta = size;
            }

            // --- World Objects (Y‑scale) ---
            for (int i = 0; i < yAxisObjects.Length; i++)
            {
                if (yAxisObjects[i] == null) continue;

                float noise = Mathf.PerlinNoise(i * 0.3f, Time.time * 6f) * 1.2f;
                float targetY = Mathf.Lerp(currentMinY, currentMaxY, loudness * noise);

                Vector3 scale = yAxisObjects[i].localScale;
                float speed = targetY > scale.y ? attackSpeed : releaseSpeed;
                scale.y = Mathf.Lerp(scale.y, targetY, Time.deltaTime * speed);
                yAxisObjects[i].localScale = scale;
            }
        }
        else
        {
            // When not holding fruit, keep everything at minimum
            // (ResetBars/ResetObjects already called on state change, but ensure they stay low)
            // Optionally, you can also set them to min every frame – but that's wasteful.
            // The reset on state change is enough.
        }
    }

    private void ResetBars()
    {
        float currentMinHeight = minHeight * uiScale;
        foreach (var bar in bars)
        {
            if (bar == null) continue;
            Vector2 size = bar.sizeDelta;
            size.y = currentMinHeight;
            bar.sizeDelta = size;
        }
    }

    private void ResetObjects()
    {
        float currentMinY = minY;
        foreach (var obj in yAxisObjects)
        {
            if (obj == null) continue;
            Vector3 scale = obj.localScale;
            scale.y = currentMinY;
            obj.localScale = scale;
        }
    }
}