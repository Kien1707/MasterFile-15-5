using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VisualStateController : MonoBehaviour
{
    [Header("Saturation per State")]
    public float satMinus1 = 0.2f;
    public float sat0 = 1f;
    public float sat1 = 1.6f;

    [Header("Brightness per State")]
    public float brightMinus1 = 0f;
    public float bright0 = 0.7f;
    public float bright1 = 1.5f;

    [Header("Lerp Speed")]
    public float lerpDuration = 1.5f;

    private List<Material> materials = new List<Material>();
    private GroworWilt growWilt;
    private int lastState = -999; // impossible initial value
    private int currentState = 0;
    private float currentSat = 1f;
    private float currentBright = 1f;
    private Coroutine activeLerp = null;

    void Start()
    {
        // Find GroworWilt on the same GameObject
        growWilt = GetComponent<GroworWilt>();
        if (growWilt == null)
        {
            Debug.LogError("VisualStateController: No GroworWilt component found on this GameObject!");
            enabled = false;
            return;
        }

        // Collect all materials
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer rend in renderers)
        {
            if (rend is ParticleSystemRenderer) continue;
            foreach (Material mat in rend.materials)
            {
                if (mat != null && !materials.Contains(mat))
                    materials.Add(mat);
            }
        }
        Debug.Log($"Found {materials.Count} materials");

        // Get initial visual values
        if (materials.Count > 0)
        {
            Material m = materials[0];
            currentSat = m.HasProperty("_Saturation") ? m.GetFloat("_Saturation") : 1f;
            currentBright = m.HasProperty("_Brightness") ? m.GetFloat("_Brightness") : 1f;
        }

        // Sync initial state
        lastState = growWilt.currentState;
        UpdateVisualsForState(lastState);
    }

    void Update()
    {
        if (growWilt == null) return;

        // Detect state change
        if (growWilt.currentState != lastState)
        {
            lastState = growWilt.currentState;
            UpdateVisualsForState(lastState);
        }
    }

    void UpdateVisualsForState(int state)
    {
        float targetSat, targetBright;
        switch (state)
        {
            case -1:
                targetSat = satMinus1;
                targetBright = brightMinus1;
                break;
            case 1:
                targetSat = sat1;
                targetBright = bright1;
                break;
            default:
                targetSat = sat0;
                targetBright = bright0;
                break;
        }
        StartLerp(targetSat, targetBright);
        currentState = state;
    }

    void StartLerp(float targetSat, float targetBright)
    {
        if (activeLerp != null)
            StopCoroutine(activeLerp);
        activeLerp = StartCoroutine(LerpVisuals(targetSat, targetBright));
    }

    IEnumerator LerpVisuals(float targetSat, float targetBright)
    {
        float startSat = currentSat;
        float startBright = currentBright;
        float elapsed = 0f;

        while (elapsed < lerpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lerpDuration;
            float newSat = Mathf.Lerp(startSat, targetSat, t);
            float newBright = Mathf.Lerp(startBright, targetBright, t);

            foreach (Material mat in materials)
                SetMaterialProperties(mat, newSat, newBright);

            currentSat = newSat;
            currentBright = newBright;
            yield return null;
        }

        foreach (Material mat in materials)
            SetMaterialProperties(mat, targetSat, targetBright);
        currentSat = targetSat;
        currentBright = targetBright;
        activeLerp = null;
    }

    void SetMaterialProperties(Material mat, float sat, float bright)
    {
        if (mat == null) return;

        if (mat.HasProperty("_Saturation"))
            mat.SetFloat("_Saturation", sat);
        if (mat.HasProperty("_Brightness"))
            mat.SetFloat("_Brightness", bright);

        // Fallback
        if (!mat.HasProperty("_Saturation") && !mat.HasProperty("_Brightness"))
        {
            Color col = mat.color;
            Color.RGBToHSV(col, out float h, out float s, out float v);
            mat.color = Color.HSVToRGB(h, sat, bright);
        }
    }
}