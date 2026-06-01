using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StateBasedPostProcess : MonoBehaviour
{
    [Header("Volume")]
    public Volume postProcessVolume;

    [Header("Offset per State")]
    // Offsets are ADDED to the original volume values
    [Header("Normal (state 0) – typically zero offset")]
    [Range(-1f, 1f)] public float normalVignetteOffset = 0f;
    [Range(-2f, 2f)] public float normalExposureOffset = 0f;
    [Range(-2f, 2f)] public float normalBloomOffset = 0f;

    [Header("Decay (state -1)")]
    [Range(-1f, 1f)] public float decayVignetteOffset = 0.4f;
    [Range(-2f, 2f)] public float decayExposureOffset = -0.5f;
    [Range(-2f, 2f)] public float decayBloomOffset = 0f;

    [Header("Flourish (state 1)")]
    [Range(-1f, 1f)] public float flourishVignetteOffset = 0f;
    [Range(-2f, 2f)] public float flourishExposureOffset = 0.8f;
    [Range(-2f, 2f)] public float flourishBloomOffset = 0.5f;

    [Header("Transition")]
    public float transitionSpeed = 3f;

    // Current target offsets (blended)
    private float targetVignetteOffset;
    private float targetExposureOffset;
    private float targetBloomOffset;

    // Current actual values (blended)
    private float currentVignetteOffset;
    private float currentExposureOffset;
    private float currentBloomOffset;

    // Baseline values from the Volume (read once at start)
    private float baselineVignette;
    private float baselineExposure;
    private float baselineBloom;

    // References to effects
    private Vignette vignette;
    private Bloom bloom;
    private ColorAdjustments colorAdjustments;

    private int currentPriorityState = 0; // 0=normal, -1=decay, 1=flourish

    void Start()
    {
        if (postProcessVolume == null)
            postProcessVolume = FindObjectOfType<Volume>();

        if (postProcessVolume == null)
        {
            Debug.LogError("StateBasedPostProcess: No Volume found!");
            enabled = false;
            return;
        }

        // Get or add effects, but also read their current values
        if (!postProcessVolume.profile.TryGet(out vignette))
        {
            vignette = postProcessVolume.profile.Add<Vignette>(true);
            vignette.active = true;
            vignette.intensity.value = 0f; // default if not present
        }
        baselineVignette = vignette.intensity.value;

        if (!postProcessVolume.profile.TryGet(out bloom))
        {
            bloom = postProcessVolume.profile.Add<Bloom>(true);
            bloom.active = true;
            bloom.intensity.value = 0f;
        }
        baselineBloom = bloom.intensity.value;

        if (!postProcessVolume.profile.TryGet(out colorAdjustments))
        {
            colorAdjustments = postProcessVolume.profile.Add<ColorAdjustments>(true);
            colorAdjustments.active = true;
            colorAdjustments.postExposure.value = 0f;
        }
        baselineExposure = colorAdjustments.postExposure.value;

        // Start with normal offsets
        currentVignetteOffset = normalVignetteOffset;
        currentExposureOffset = normalExposureOffset;
        currentBloomOffset = normalBloomOffset;
        ApplyCurrentOffsets();
    }

    void Update()
    {
        if (postProcessVolume == null) return;

        // Determine highest priority state player is in (flourish > decay > normal)
        int priorityState = 0;
        GroworWilt[] clusters = FindObjectsOfType<GroworWilt>();
        foreach (var cluster in clusters)
        {
            if (cluster.IsPlayerInRange())
            {
                int state = cluster.currentState;
                if (state == 1) { priorityState = 1; break; }
                if (state == -1) priorityState = -1;
            }
        }

        if (priorityState != currentPriorityState)
        {
            currentPriorityState = priorityState;
            // Optionally play sound or particle on transition
        }

        // Determine target offsets based on state
        switch (currentPriorityState)
        {
            case -1:
                targetVignetteOffset = decayVignetteOffset;
                targetExposureOffset = decayExposureOffset;
                targetBloomOffset = decayBloomOffset;
                break;
            case 1:
                targetVignetteOffset = flourishVignetteOffset;
                targetExposureOffset = flourishExposureOffset;
                targetBloomOffset = flourishBloomOffset;
                break;
            default:
                targetVignetteOffset = normalVignetteOffset;
                targetExposureOffset = normalExposureOffset;
                targetBloomOffset = normalBloomOffset;
                break;
        }

        // Smoothly blend offsets
        currentVignetteOffset = Mathf.Lerp(currentVignetteOffset, targetVignetteOffset, Time.deltaTime * transitionSpeed);
        currentExposureOffset = Mathf.Lerp(currentExposureOffset, targetExposureOffset, Time.deltaTime * transitionSpeed);
        currentBloomOffset = Mathf.Lerp(currentBloomOffset, targetBloomOffset, Time.deltaTime * transitionSpeed);

        ApplyCurrentOffsets();
    }

    void ApplyCurrentOffsets()
    {
        // Apply baseline + offset
        if (vignette != null)
            vignette.intensity.value = baselineVignette + currentVignetteOffset;

        if (bloom != null)
            bloom.intensity.value = baselineBloom + currentBloomOffset;

        if (colorAdjustments != null)
            colorAdjustments.postExposure.value = baselineExposure + currentExposureOffset;
    }
}