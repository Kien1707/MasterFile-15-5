using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GroworWilt : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    [Header("Additional Animators")]
    public List<Animator> zoneAnimators;

    [Header("State Settings")]
    public int currentState = 0; // -1, 0, 1
    public float detectionRadius = 2f;

    private bool playerInRange = false;

    void Start()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
            else Debug.LogError("No player found!");
        }

        if (zoneAnimators == null || zoneAnimators.Count == 0)
        {
            Debug.LogError("No zone animators assigned!");
        }
        else
        {
            foreach (Animator a in zoneAnimators) a.SetInteger("State", currentState);
        }
    }

    public bool IsPlayerInRange()
    {
        return playerInRange;
    }

    public void OnFruitAnimationTriggered(GameObject fruit)
    {
        Glow fruitScript = fruit.GetComponent<Glow>();
        if (fruitScript == null) { Debug.LogError("No Glow script on fruit!"); return; }

        if (fruitScript.isGoodFruit && currentState < 1)
            StartCoroutine(DelayedStateChange(+1));
        else if (!fruitScript.isGoodFruit && currentState > -1)
            StartCoroutine(DelayedStateChange(-1));
    }

    private IEnumerator DelayedStateChange(int direction)
    {
        yield return new WaitForSeconds(2f);
        currentState = Mathf.Clamp(currentState + direction, -1, 1);
        foreach (Animator a in zoneAnimators) a.SetInteger("State", currentState);
        Debug.Log($"State changed to {currentState}");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}