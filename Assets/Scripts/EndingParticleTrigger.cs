using UnityEngine;

public class EndingParticleTrigger : MonoBehaviour
{
    [Header("Particle Systems")]
    public ParticleSystem goodEndingParticle;
    public ParticleSystem badEndingParticle;

    [Header("Sound (PlayerSound)")]
    public PlayerSound sound;

    private FruitCounter fruitCounter;
    private bool hasTriggered = false;

    void Start()
    {
        fruitCounter = FindFirstObjectByType<FruitCounter>();
        if (fruitCounter == null)
            Debug.LogError("FruitCounter not found!");

        if (sound == null)
            Debug.LogError("PlayerSound NOT assigned in EndingParticleTrigger!");
    }

    void Update()
    {
        if (fruitCounter == null || hasTriggered) return;

        int total = fruitCounter.GoodCount + fruitCounter.BadCount;

        if (total >= fruitCounter.maxFruits)
        {
            hasTriggered = true;

            Debug.Log($"ENDING TRIGGERED — Good={fruitCounter.GoodCount}, Bad={fruitCounter.BadCount}");

            // GOOD ENDING
            if (fruitCounter.GoodCount > fruitCounter.BadCount)
            {
                if (goodEndingParticle != null)
                    goodEndingParticle.Play();

                if (sound != null)
                {
                    Debug.Log("Playing GOOD ENDING sound");
                    sound.Play(PlayerAction.GoodEnding);
                }
            }
            // BAD ENDING
            else if (fruitCounter.BadCount > fruitCounter.GoodCount)
            {
                if (badEndingParticle != null)
                    badEndingParticle.Play();

                if (sound != null)
                {
                    Debug.Log("Playing BAD ENDING sound");
                    sound.Play(PlayerAction.BadEnding);
                }
            }
            // NEUTRAL
            else
            {
                Debug.Log("Neutral ending → no sound");
            }
        }
    }
}
