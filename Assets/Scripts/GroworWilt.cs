using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GroworWilt : MonoBehaviour
{
    [Header("Player Proximity")]
    public Transform player;
    public float detectionRadius = 2f;
    private bool playerInRange = false;

    [Header("State Settings")]
    public int currentState = 0; // -1, 0, 1

    private List<Animator> zoneAnimators;
    private List<ParticleSystem> goodParticles = new List<ParticleSystem>();
    private List<ParticleSystem> badParticles = new List<ParticleSystem>();

    [Header("Sound")]
    public PlayerSound sound;   // ← thêm PlayerSound để phát âm

    void Start()
    {
        // Find all animators in children
        zoneAnimators = new List<Animator>(GetComponentsInChildren<Animator>(true));
        if (zoneAnimators.Count == 0)
            Debug.LogWarning("No animators found in children!");
        else
        {
            foreach (Animator a in zoneAnimators)
                a.SetInteger("State", currentState);
        }

        // Auto‑collect particles by tag
        CollectParticlesByTag("GoodParticle", goodParticles);
        CollectParticlesByTag("BadParticle", badParticles);

        Debug.Log($"Found {goodParticles.Count} good particles, {badParticles.Count} bad particles in {gameObject.name}");

        // Find player if not assigned
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
            else Debug.LogError("No player found!");
        }
    }

    void CollectParticlesByTag(string tag, List<ParticleSystem> targetList)
    {
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in taggedObjects)
        {
            if (obj.transform.IsChildOf(transform))
            {
                ParticleSystem ps = obj.GetComponent<ParticleSystem>();
                if (ps != null) targetList.Add(ps);
            }
        }
    }

    public bool IsPlayerInRange() { return playerInRange; }

    // ---------------------------------------------------------
    // FRUIT TRIGGER
    // ---------------------------------------------------------
    public void OnFruitAnimationTriggered(GameObject fruit)
    {
        Glow fruitScript = fruit.GetComponent<Glow>();
        if (fruitScript == null)
        {
            Debug.LogError("No Glow script on fruit!");
            return;
        }

        if (fruitScript.isGoodFruit && currentState < 1)
            StartCoroutine(DelayedStateChange(+1));
        else if (!fruitScript.isGoodFruit && currentState > -1)
            StartCoroutine(DelayedStateChange(-1));
    }

    // ---------------------------------------------------------
    // STATE CHANGE + RANDOM SOUND
    // ---------------------------------------------------------
    private IEnumerator DelayedStateChange(int direction)
    {
        yield return new WaitForSeconds(2f);

        int newState = Mathf.Clamp(currentState + direction, -1, 1);
        if (newState == currentState) yield break;

        currentState = newState;

        // Update all animators
        foreach (Animator a in zoneAnimators)
            if (a != null && a.isActiveAndEnabled)
                a.SetInteger("State", currentState);

        // PLAY RANDOM SOUND
        if (sound != null)
        {
            if (direction > 0) // GROWING
            {
                PlayerAction growSound = (Random.value < 0.5f)
                    ? PlayerAction.Growing1
                    : PlayerAction.Growing2;

                sound.Play(growSound);
            }
            else if (direction < 0) // DECAYING
            {
                PlayerAction decaySound = (Random.value < 0.5f)
                    ? PlayerAction.Decaying1
                    : PlayerAction.Decaying2;

                sound.Play(decaySound);
            }
        }

        // Spawn if fully grown
        if (currentState == 1)
        {
            TriggerSpawn spawner = GetComponentInChildren<TriggerSpawn>();
            if (spawner != null)
                spawner.SpawnObjects();
            else
                Debug.LogWarning("No TriggerSpawn found on this GameObject");
        }

        // Play particles
        if (direction > 0)
        {
            foreach (ParticleSystem ps in goodParticles)
                if (ps != null) ps.Play();
        }
        else if (direction < 0)
        {
            foreach (ParticleSystem ps in badParticles)
                if (ps != null) ps.Play();
        }

        Debug.Log($"State changed to {currentState}");
    }

    // ---------------------------------------------------------
    // PLAYER RANGE
    // ---------------------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player entered cluster zone");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player exited cluster zone");
        }
    }

    // ---------------------------------------------------------
    // GIZMOS
    // ---------------------------------------------------------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
