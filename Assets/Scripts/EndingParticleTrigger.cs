using UnityEngine;

public class EndingParticleTrigger : MonoBehaviour
{
    [Header("Particle Systems (assign in inspector)")]
    public ParticleSystem goodEndingParticle; // "ParticleEnding1"
    public ParticleSystem badEndingParticle;  // "ParticleEnding2"

    [Header("Boundary (optional – not used here, kept for compatibility)")]
    public Collider boundaryCollider; // can be left empty

    private FruitCounter fruitCounter;
    private bool hasTriggered = false;

    void Start()
    {
        fruitCounter = FindFirstObjectByType<FruitCounter>();
        if (fruitCounter == null)
            Debug.LogError("FruitCounter not found!");
    }

    void Update()
    {
        if (fruitCounter == null || hasTriggered) return;

        int total = fruitCounter.GoodCount + fruitCounter.BadCount;
        if (total >= fruitCounter.maxFruits)
        {
            hasTriggered = true;

            if (fruitCounter.GoodCount > fruitCounter.BadCount)
            {
                if (goodEndingParticle != null)
                    goodEndingParticle.Play();
                else
                    Debug.LogWarning("Good ending particle not assigned!");
            }
            else if (fruitCounter.BadCount > fruitCounter.GoodCount)
            {
                if (badEndingParticle != null)
                    badEndingParticle.Play();
                else
                    Debug.LogWarning("Bad ending particle not assigned!");
            }
            else
            {
                Debug.Log("Neutral ending → no particle burst");
            }
        }
    }
}