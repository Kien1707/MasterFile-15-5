using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StateBasedPostProcess : MonoBehaviour
{
    [Header("Volume")]
    public Volume postProcessVolume;

    [Header("Normal Settings (state 0)")]
    [Range(0f, 1f)] public float normalVignette = 0f;
    [Range(-2f, 2f)] public float normalExposure = 0f;  // Exposure compensation
    [Range(0f, 2f)] public float normalBloomIntensity = 0f;

    [Header("Decay Settings (state -1)")]
    [Range(0f, 1f)] public float decayVignette = 0.4f;
    [Range(-2f, 2f)] public float decayExposure = -0.5f; // darker
    [Range(0f, 2f)] public float decayBloomIntensity = 0f;

    [Header("Flourish Settings (state 1)")]
    [Range(0f, 1f)] public float flourishVignette = 0f;
    [Range(-2f, 2f)] public float flourishExposure = 0.8f; // brighter
    [Range(0f, 2f)] public float flourishBloomIntensity = 0.5f;

    [Header("Transition")]
    public float transitionSpeed = 3f;

    private Vignette vignette;
    private Bloom bloom;
    private ColorAdjustments colorAdjustments;
    private float currentVignette;
    private float currentExposure;
    private float currentBloomIntensity;

    private int currentPriorityState = 0; // 0=normal, -1=decay, 1=flourish

    void Start()
    {
        if (postProcessVolume == null)
            postProcessVolume = FindObjectOfType<Volume>();

        if (postProcessVolume == null)
        {
            Debug.LogError("No Volume found!");
            return;
        }

        // Get or add effects
        if (!postProcessVolume.profile.TryGet(out vignette))
        {
            vignette = postProcessVolume.profile.Add<Vignette>(true);
            vignette.active = true;
        }
        if (!postProcessVolume.profile.TryGet(out bloom))
        {
            bloom = postProcessVolume.profile.Add<Bloom>(true);
            bloom.active = true;
        }
        if (!postProcessVolume.profile.TryGet(out colorAdjustments))
        {
            colorAdjustments = postProcessVolume.profile.Add<ColorAdjustments>(true);
            colorAdjustments.active = true;
        }

        // Set initial values
        currentVignette = normalVignette;
        currentExposure = normalExposure;
        currentBloomIntensity = normalBloomIntensity;
        UpdateEffects();
    }

    void Update()
    {
        if (postProcessVolume == null) return;

        // Determine the highest priority state the player is inside
        int priorityState = 0; // default normal
        GroworWilt[] clusters = FindObjectsOfType<GroworWilt>();
        foreach (var cluster in clusters)
        {
            if (cluster.IsPlayerInRange())
            {
                int state = cluster.currentState;
                if (state == 1) { priorityState = 1; break; } // flourish overrides everything
                else if (state == -1) priorityState = -1; // decay, but continue to check for flourish
            }
        }

        if (priorityState != currentPriorityState)
        {
            currentPriorityState = priorityState;
            // Optional: play sound or particle on transition
        }

        float targetVignette, targetExposure, targetBloom;
        switch (currentPriorityState)
        {
            case -1:
                targetVignette = decayVignette;
                targetExposure = decayExposure;
                targetBloom = decayBloomIntensity;
                break;
            case 1:
                targetVignette = flourishVignette;
                targetExposure = flourishExposure;
                targetBloom = flourishBloomIntensity;
                break;
            default:
                targetVignette = normalVignette;
                targetExposure = normalExposure;
                targetBloom = normalBloomIntensity;
                break;
        }

        // Smooth transition
        currentVignette = Mathf.Lerp(currentVignette, targetVignette, Time.deltaTime * transitionSpeed);
        currentExposure = Mathf.Lerp(currentExposure, targetExposure, Time.deltaTime * transitionSpeed);
        currentBloomIntensity = Mathf.Lerp(currentBloomIntensity, targetBloom, Time.deltaTime * transitionSpeed);

        UpdateEffects();
    }

    void UpdateEffects()
    {
        if (vignette != null)
            vignette.intensity.value = currentVignette;

        if (bloom != null)
            bloom.intensity.value = currentBloomIntensity;

        if (colorAdjustments != null)
            colorAdjustments.postExposure.value = currentExposure;
    }
}