using UnityEngine;
using System.Collections;

// -Insert prefab/object to spawn
// -The spawn pattern for growing the trees at random inside the zone's radius
// -Set spawn radius for how spread out the objects will spawn from one another
// -Growth duration input
// -Spawn count for number of spawn objects
// -Min and Max height input. Random height spawns

public class TriggerSpawn : MonoBehaviour
{
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private int spawnCount = 1;
    [SerializeField] private KeyCode triggerKey = KeyCode.Space;
    [SerializeField] private SpawnPattern spawnPattern = SpawnPattern.AtPosition;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private bool randomizeYRotation = false;
    [SerializeField] private float growthDuration = 0.5f;
    [SerializeField] private Vector3 targetScale = Vector3.one;
    [SerializeField] private float minRandomHeight = 0.5f;
    [SerializeField] private float maxRandomHeight = 2f;
    
    [Header("Ground Adjustment")]
    [SerializeField] private float yOffset = 0f; // positive = higher, negative = lower

    private enum SpawnPattern
    {
        AtPosition,
        RandomInRadius,
        InCircle,
        InGrid
    }

    // private void Update()
    // {
    //     if (Input.GetKeyDown(triggerKey))
    //     {
    //         SpawnObjects();
    //     }
    // }

    public void SpawnObjects()
    {
        if (prefabToSpawn == null)
        {
            Debug.LogError("Prefab to spawn is not assigned in TriggerSpawn script!");
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPosition = GetSpawnPosition(i);
            spawnPosition.y = 0; // Ensure spawning at y=0 on the plane
            GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPosition, GetSpawnRotation());
            
            // Generate random height for this object
            float randomHeight = Random.Range(minRandomHeight, maxRandomHeight);
            Vector3 randomTargetScale = new Vector3(targetScale.x, randomHeight, targetScale.z);
            
            // Start with zero scale and grow
            spawnedObject.transform.localScale = Vector3.zero;
            StartCoroutine(GrowObject(spawnedObject, spawnPosition, randomTargetScale, growthDuration));
        }

        Debug.Log($"Spawned {spawnCount} object(s) at {transform.position}");
    }

    private Vector3 GetSpawnPosition(int index)
    {
        switch (spawnPattern)
        {
            case SpawnPattern.AtPosition:
                return transform.position;

            case SpawnPattern.RandomInRadius:
                return transform.position + Random.insideUnitSphere * spawnRadius;

            case SpawnPattern.InCircle:
                float angle = (360f / spawnCount) * index;
                float rad = angle * Mathf.Deg2Rad;
                return transform.position + new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * spawnRadius;

            case SpawnPattern.InGrid:
                int gridSize = Mathf.CeilToInt(Mathf.Sqrt(spawnCount));
                int x = index % gridSize;
                int z = index / gridSize;
                float spacing = spawnRadius / gridSize;
                return transform.position + new Vector3(x * spacing - spawnRadius / 2, 0, z * spacing - spawnRadius / 2);

            default:
                return transform.position;
        }
    }

    private Quaternion GetSpawnRotation()
    {
        if (randomizeYRotation)
        {
            float randomY = Random.Range(0f, 360f);
            return Quaternion.Euler(0, randomY, 0);
        }
        return Quaternion.identity;
    }

    private IEnumerator GrowObject(GameObject obj, Vector3 initialPosition, Vector3 targetScale, float duration)
    {
        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            Vector3 currentScale = Vector3.Lerp(startScale, targetScale, progress);
            obj.transform.localScale = currentScale;
            
            // Apply Y offset: base placement (half scale) + manual offset
            float yOffsetFromScale = currentScale.y / 2f;
            obj.transform.position = initialPosition + Vector3.up * (yOffsetFromScale + yOffset);
            
            yield return null;
        }

        obj.transform.localScale = targetScale;
        float finalYOffset = targetScale.y / 2f + yOffset;
        obj.transform.position = initialPosition + Vector3.up * finalYOffset;
    }
}