using UnityEngine;
using System.Collections.Generic;

public class ZonePopulator : MonoBehaviour
{
    [Header("Prefabs")]
    public List<GameObject> prefabs;

    [Header("Spawn Settings")]
    public int count = 20;
    public float radius = 10f;
    public LayerMask groundLayer;

    [Header("Scale")]
    public float minScale = 0.8f;
    public float maxScale = 1.5f;

    [Header("Raycast")]
    public float raycastHeight = 50f;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Start()
    {
        Populate();
        StartCoroutine(RegisterAfterFrame());
    }

    private System.Collections.IEnumerator RegisterAfterFrame()
    {
        yield return null; // wait one frame for GroworWilt to initialize
        GroworWilt gw = GetComponentInParent<GroworWilt>();
        if (gw != null)
            gw.RegisterAnimators(GetComponentsInChildren<Animator>());
    }

    void Populate()
    {
        if (prefabs == null || prefabs.Count == 0) return;

        int attempts = 0;
        int spawned = 0;

        while (spawned < count && attempts < count * 5)
        {
            attempts++;

            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 rayOrigin = transform.position + new Vector3(randomCircle.x, raycastHeight, randomCircle.y);

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundLayer))
            {
                GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
                Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                GameObject obj = Instantiate(prefab, hit.point, rotation, transform);

                float scale = Random.Range(minScale, maxScale);
                obj.transform.localScale = Vector3.one * scale;

                spawnedObjects.Add(obj);
                spawned++;
            }
        }
    }
}