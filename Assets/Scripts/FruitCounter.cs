using UnityEngine;
using TMPro;
using System.Collections;

public class FruitCounter : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text fruitCounterText;

    [Header("Fruit Limits")]
    public int maxFruits = 8;

    private int goodFruitCount = 0;
    private int badFruitCount = 0;

    // PUBLIC READ-ONLY
    public int GoodCount => goodFruitCount;
    public int BadCount => badFruitCount;
    public bool EndingTriggered => endingTriggered;

    [Header("Ending Cameras")]
    public Camera neutralEndingCamera;   // 4 good – 4 bad
    public Camera finalEndingCamera;     // good > bad hoặc bad > good

    private bool endingTriggered = false;

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        if (endingTriggered)
            return;

        if (PickableFruit.AnyFruitHeld && Input.GetKeyDown(KeyCode.F))
        {
            AddFruit();
        }
    }

    void AddFruit()
    {
        int total = goodFruitCount + badFruitCount;
        if (total >= maxFruits)
            return;

        GameObject fruitObj = PickableFruit.currentlyHeldFruit;
        if (fruitObj == null)
            return;

        Glow glow = fruitObj.GetComponent<Glow>();
        if (glow == null)
            return;

        if (glow.isGoodFruit)
            goodFruitCount++;
        else
            badFruitCount++;

        UpdateUI();

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
