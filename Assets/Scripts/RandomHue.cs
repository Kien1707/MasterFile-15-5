using UnityEngine;
using System.Collections.Generic;

public class RandomHue : MonoBehaviour
{
    [Header("Hue Shift Range")]
    public float minHue = -0.15f;  // slight blue/green shift
    public float maxHue = 0.15f;   // slight orange/purple shift

    void Start()
    {
        // Find all renderers in children
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer rend in renderers)
        {
            // Skip particle systems
            if (rend is ParticleSystemRenderer) continue;

            // Get all materials on this renderer
            Material[] mats = rend.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null && mats[i].HasProperty("_HueShift"))
                {
                    float randomHue = Random.Range(minHue, maxHue);
                    mats[i].SetFloat("_HueShift", randomHue);
                    Debug.Log($"Set _HueShift to {randomHue} on {rend.gameObject.name}");
                }
            }
        }
    }
}