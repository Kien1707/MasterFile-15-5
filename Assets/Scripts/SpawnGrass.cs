using UnityEngine;
using System.Collections.Generic;

public class GrassSpawner : MonoBehaviour
{
    [Header("Grass Settings")]
    public GameObject grassPrefab;
    public int grassCount = 50;
    public float spawnRadius = 5f;
    public float raycastHeight = 10f;
    public LayerMask groundLayer;

    [Header("Variation")]
    public float minScale = 0.8f;
    public float maxScale = 1.2f;

    private List<GameObject> spawnedGrass = new List<GameObject>();

    void Start()
    {
        SpawnGrass();
    }

    void SpawnGrass()
    {
        int attempts = 0;
        int spawned = 0;

        while (spawned < grassCount && attempts < grassCount * 5)
        {
            attempts++;

            // Random point within radius
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 rayOrigin = transform.position + new Vector3(randomCircle.x, raycastHeight, randomCircle.y);

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundLayer))
            {
                // Spawn aligned to surface normal
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                rotation *= Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                GameObject grass = Instantiate(grassPrefab, hit.point, rotation, transform);

                // Random scale variation
                float scale = Random.Range(minScale, maxScale);
                grass.transform.localScale = Vector3.one * scale;

                spawnedGrass.Add(grass);
                spawned++;
            }
        }

        Debug.Log($"Spawned {spawned} grass on {gameObject.name}");
    }
}