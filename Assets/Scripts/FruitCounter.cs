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

    public int GoodCount => goodFruitCount;
    public int BadCount => badFruitCount;
    public bool EndingTriggered => endingTriggered;

    [Header("Ending Cameras")]
    public Camera neutralEndingCamera;
    public Camera finalEndingCamera;

    [Header("Sound")]
    public PlayerSound sound;

    private ParticleSystem goodEndingParticle;
    private ParticleSystem badEndingParticle;

    private bool endingTriggered = false;

    void Start()
    {
        UpdateUI();

        GameObject goodObj = GameObject.Find("ParticleEndingGood");
        if (goodObj != null) goodEndingParticle = goodObj.GetComponent<ParticleSystem>();

        GameObject badObj = GameObject.Find("ParticleEndingBad");
        if (badObj != null) badEndingParticle = badObj.GetComponent<ParticleSystem>();

        if (goodEndingParticle != null) goodEndingParticle.Stop();
        if (badEndingParticle != null) badEndingParticle.Stop();
    }

    public void AddFruit(bool isGood)
    {
        if (endingTriggered) return;

        int total = goodFruitCount + badFruitCount;
        if (total >= maxFruits) return;

        if (isGood) goodFruitCount++;
        else badFruitCount++;

        UpdateUI();

        if (goodFruitCount + badFruitCount >= maxFruits)
            TriggerEnding();
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

        // GOOD ENDING
        if (goodFruitCount > badFruitCount)
        {
            if (goodEndingParticle != null)
                goodEndingParticle.Play();

            if (sound != null)
                sound.Play(PlayerAction.GoodEnding);
        }
        // BAD ENDING
        else if (badFruitCount > goodFruitCount)
        {
            if (badEndingParticle != null)
                badEndingParticle.Play();

            if (sound != null)
                sound.Play(PlayerAction.BadEnding);
        }
        // NEUTRAL ENDING
        else
        {
            Debug.Log("Neutral ending → playing neutral sound");

            if (sound != null)
                sound.Play(PlayerAction.NeutralEnding);
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
        RenderSettings.fog = false;
    }
}
