using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectSpawnDespawnTrigger : MonoBehaviour
{
    [Header("Assign Player Here")]
    public Transform playerTransform;

    [Header("Spawn Area (Collider)")]
    public Collider spawnArea;

    [Header("Tree Settings")]
    public GameObject prefabToSpawn;
    public int numberOfInitialSpawns = 10;

    [Header("Height Offset")]
    public float globalYOffset = 0f;   // Adjust spawn height

    [Header("Sink Settings")]
    public float sinkDistance = 5f;
    public float sinkDuration = 2f;
    public float delayBetweenTrees = 0.3f;

    private List<GameObject> spawnedTrees = new List<GameObject>();
    private bool isDespawning = false;

    void Start()
    {
        if (spawnArea == null)
        {
            Debug.LogError("Spawn area collider is not assigned.");
            return;
        }

        SpawnTreesInsideCollider();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isDespawning)
        {
            if (playerTransform == null)
            {
                Debug.LogError("Player transform is not assigned.");
                return;
            }

            StartCoroutine(SinkTreesInOrder());
        }
    }

    // ---------------------------------------------------------
    // SPAWN TREES INSIDE COLLIDER (TRUE INSIDE CHECK)
    // ---------------------------------------------------------
    void SpawnTreesInsideCollider()
    {
        float baseY = spawnArea.bounds.min.y + globalYOffset;

        for (int i = 0; i < numberOfInitialSpawns; i++)
        {
            Vector3 pos = GetRandomPointInsideCollider(spawnArea);

            // Lock Y height
            pos.y = baseY;

            GameObject tree = Instantiate(prefabToSpawn, pos, Quaternion.identity);
            spawnedTrees.Add(tree);
        }
    }

    // TRUE inside-collider check using ClosestPoint
    Vector3 GetRandomPointInsideCollider(Collider col)
    {
        Vector3 min = col.bounds.min;
        Vector3 max = col.bounds.max;

        Vector3 point;

        // Loop until point is truly inside collider
        do
        {
            point = new Vector3(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y),
                Random.Range(min.z, max.z)
            );
        }
        while (col.ClosestPoint(point) != point); // TRUE inside check

        return point;
    }

    // ---------------------------------------------------------
    // SINK TREES BASED ON DISTANCE TO PLAYER
    // ---------------------------------------------------------
   IEnumerator SinkTreesInOrder()
{
    isDespawning = true;

    // FIRST: Remove any destroyed/null trees from the list
    spawnedTrees.RemoveAll(tree => tree == null);

    // THEN: Sort by distance to player (only if there are trees left)
    if (spawnedTrees.Count > 0)
    {
        spawnedTrees.Sort((a, b) =>
            Vector3.Distance(a.transform.position, playerTransform.position)
            .CompareTo(Vector3.Distance(b.transform.position, playerTransform.position)));
    }

    // Sink one by one
    foreach (GameObject tree in spawnedTrees)
    {
        if (tree != null)
            StartCoroutine(SinkTree(tree));

        yield return new WaitForSeconds(delayBetweenTrees);
    }

    isDespawning = false;
}


    IEnumerator SinkTree(GameObject tree)
    {
        Vector3 startPos = tree.transform.position;
        Vector3 endPos = startPos + Vector3.down * sinkDistance;

        float elapsed = 0f;

        while (elapsed < sinkDuration)
        {
            if (tree == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / sinkDuration;

            tree.transform.position = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        Destroy(tree);
    }
}
