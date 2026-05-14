using UnityEngine;
using UnityEngine.UI;

public class MicUI : MonoBehaviour
{
    [Header("Bars (UI)")]
    public RectTransform[] bars;

    [Header("World Objects (7 slots)")]
    public Transform[] yAxisObjects = new Transform[7];

    [Header("UI Fade")]
    public CanvasGroup canvasGroup;
    public float idleAlpha = 0.5f;      // opacity khi không cầm fruit
    public float activeAlpha = 1f;      // opacity khi đang cầm fruit
    public float fadeSpeed = 5f;

    [Header("Bar Heights")]
    public float minHeight = 10f;
    public float maxHeight = 150f;

    [Header("Y‑Axis Manipulation")]
    public float minY = 0.2f;
    public float maxY = 2.0f;

    [Header("Animation Speeds")]
    public float attackSpeed = 25f;   // đi lên nhanh
    public float releaseSpeed = 6f;   // đi xuống chậm

    [Header("Loudness Boost")]
    public float loudnessMultiplier = 3f;

    private bool wasActiveLastFrame = false;

    void Start()
    {
        canvasGroup.alpha = idleAlpha;
        ResetBars();
        ResetObjects();
    }

    void Update()
    {
        bool isActive = PickableFruit.AnyFruitHeld;

        // Fade UI theo trạng thái cầm fruit
        float targetAlpha = isActive ? activeAlpha : idleAlpha;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

        // Vừa mới cầm fruit
        if (isActive && !wasActiveLastFrame)
        {
            ResetBars();
            ResetObjects();
        }

        // Vừa mới thả fruit
        if (!isActive && wasActiveLastFrame)
        {
            ResetBars();
            ResetObjects();
            canvasGroup.alpha = idleAlpha;
        }

        wasActiveLastFrame = isActive;

        // --- Loudness từ mic ---
        // Nếu MicInput của bạn dùng MicLoudness thì đổi dòng dưới cho đúng:
        // float loudness = Mathf.Clamp01(MicInput.MicLoudness * loudnessMultiplier);
        float loudness = Mathf.Clamp01(MicInput.Loudness * loudnessMultiplier);

        // --- UI Bars ---
        for (int i = 0; i < bars.Length; i++)
        {
            if (bars[i] == null) continue;

            float noise = Mathf.PerlinNoise(i * 0.4f, Time.time * 8f) * 1.4f;
            float targetHeight = Mathf.Lerp(minHeight, maxHeight, loudness * noise);

            Vector2 size = bars[i].sizeDelta;
            float speed = targetHeight > size.y ? attackSpeed : releaseSpeed;

            size.y = Mathf.Lerp(size.y, targetHeight, Time.deltaTime * speed);
            bars[i].sizeDelta = size;
        }

        // --- 7 World Objects ---
        for (int i = 0; i < yAxisObjects.Length; i++)
        {
            if (yAxisObjects[i] == null) continue;

            float noise = Mathf.PerlinNoise(i * 0.3f, Time.time * 6f) * 1.2f;
            float targetY = Mathf.Lerp(minY, maxY, loudness * noise);

            Vector3 scale = yAxisObjects[i].localScale;
            float speed = targetY > scale.y ? attackSpeed : releaseSpeed;

            scale.y = Mathf.Lerp(scale.y, targetY, Time.deltaTime * speed);
            yAxisObjects[i].localScale = scale;
        }
    }

    private void ResetBars()
    {
        foreach (var bar in bars)
        {
            if (bar == null) continue;
            Vector2 size = bar.sizeDelta;
            size.y = minHeight;
            bar.sizeDelta = size;
        }
    }

    private void ResetObjects()
    {
        foreach (var obj in yAxisObjects)
        {
            if (obj == null) continue;
            Vector3 scale = obj.localScale;
            scale.y = minY;
            obj.localScale = scale;
        }
    }
}
