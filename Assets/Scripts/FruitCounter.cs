using UnityEngine;
using TMPro;

public class FruitCounter : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text fruitCounterText;

    [Header("Fruit Limits")]
    public int maxFruits = 4;

    private int goodFruitCount = 0;
    private int badFruitCount = 0;

    // Public read‑only properties
    public int GoodCount => goodFruitCount;
    public int BadCount => badFruitCount;
    public bool EndingTriggered => endingTriggered;

    [Header("Ending Cameras")]
    public Camera neutralEndingCamera;   // good == bad
    public Camera finalEndingCamera;     // good != bad

    // Particle systems (auto‑found by name)
    private ParticleSystem goodEndingParticle;
    private ParticleSystem badEndingParticle;

    private bool endingTriggered = false;

    void Start()
    {
        UpdateUI();

        // Find particle systems by exact name
        GameObject goodObj = GameObject.Find("ParticleEndingGood");
        if (goodObj != null) goodEndingParticle = goodObj.GetComponent<ParticleSystem>();
        else Debug.LogWarning("No GameObject named 'ParticleEnding1' found");

        GameObject badObj = GameObject.Find("ParticleEndingBad");
        if (badObj != null) badEndingParticle = badObj.GetComponent<ParticleSystem>();
        else Debug.LogWarning("No GameObject named 'ParticleEnding2' found");

        // Ensure they are stopped at start
        if (goodEndingParticle != null) goodEndingParticle.Stop();
        if (badEndingParticle != null) badEndingParticle.Stop();
    }

    // Called from PickableFruit when the fruit actually triggers its unfold animation
    public void AddFruit(bool isGood)
    {
        if (endingTriggered) return;

        int total = goodFruitCount + badFruitCount;
        if (total >= maxFruits) return;   // already full

        if (isGood)
            goodFruitCount++;
        else
            badFruitCount++;

        UpdateUI();

        // Check if we reached the limit
        if (goodFruitCount + badFruitCount >= maxFruits)
        {
            TriggerEnding();
        }
    }

    void UpdateUI()
    {
        if (fruitCounterText != null)
        {
            int total = goodFruitCount + badFruitCount;
            fruitCounterText.text = total + " / " + maxFruits;
        }
    }

    void TriggerEnding()
    {
        endingTriggered = true;

        // Play particle effect based on outcome
        if (goodFruitCount > badFruitCount)
        {
            if (goodEndingParticle != null)
                goodEndingParticle.Play();
            else
                Debug.LogWarning("Good ending particle not found!");
        }
        else if (badFruitCount > goodFruitCount)
        {
            if (badEndingParticle != null)
                badEndingParticle.Play();
            else
                Debug.LogWarning("Bad ending particle not found!");
        }
        else
        {
            Debug.Log("Neutral ending → no particle burst");
        }

        // Switch camera
        if (goodFruitCount == badFruitCount)
        {
            if (neutralEndingCamera != null)
                ActivateCamera(neutralEndingCamera);
        }
        else
        {
            if (finalEndingCamera != null)
                ActivateCamera(finalEndingCamera);
        }
    }

    void ActivateCamera(Camera cam)
    {
        Camera[] allCams = FindObjectsOfType<Camera>();
        foreach (Camera c in allCams)
            c.enabled = false;

        cam.enabled = true;
    }
}