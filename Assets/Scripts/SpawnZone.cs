using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeZoneSpawner : MonoBehaviour
{
    // ======================
    // ENUM
    // ======================
    public enum RotationMode
    {
        Fixed,
        Random,
        TowardPlayer,
        FollowPivot
    }

    [Header("Assign")]
    public Transform player;
    public GameObject treePrefab;

    // 👉 EMPTY PIVOT CONTROLLER
    public Transform rotationCenter;

    [Header("Spawn")]
    public int treeCount = 50;

    [Header("Scale")]
    public Vector2 scaleRange = new Vector2(1f, 1f);
    public float globalScale = 1f;

    [Header("Position")]
    public float yOffset = 0.2f;

    [Header("Rotation Settings")]
    public RotationMode rotationMode = RotationMode.Random;
    public float yRotationOffset = 0f;
    public Vector2 yRotationRange = new Vector2(0f, 360f);

    [Header("Sink")]
    public float sinkDepth = 5f;
    public float sinkDuration = 1f;
    public float waveDelay = 0.05f;

    private List<Transform> trees = new List<Transform>();
    private BoxCollider box;
    private bool triggered = false;

    void Start()
    {
        box = GetComponent<BoxCollider>();
        if (box == null) return;

        SpawnTrees();
    }

    void SpawnTrees()
    {
        Bounds bounds = box.bounds;

        for (int i = 0; i < treeCount; i++)
        {
            Vector3 pos = GetRandomPoint(bounds);

            GameObject tree = Instantiate(treePrefab, pos, Quaternion.identity);

            // ======================
            // SCALE
            // ======================
            Vector3 baseScale = treePrefab.transform.localScale;
            float s = Random.Range(scaleRange.x, scaleRange.y) * globalScale;
            tree.transform.localScale = baseScale * s;

            // ======================
            // POSITION
            // ======================
            tree.transform.position += Vector3.up * yOffset;

            // ======================
            // ROTATION (NEW SYSTEM)
            // ======================
            ApplyRotation(tree);

            tree.transform.SetParent(transform, true);
            trees.Add(tree.transform);
        }
    }

    // 🔥 NEW CLEAN ROTATION SYSTEM
    void ApplyRotation(GameObject tree)
    {
        float baseY = 0f;

        switch (rotationMode)
        {
            case RotationMode.Fixed:
                baseY = yRotationOffset;
                break;

            case RotationMode.Random:
                baseY = Random.Range(yRotationRange.x, yRotationRange.y) + yRotationOffset;
                break;

            case RotationMode.TowardPlayer:
                if (player != null)
                    baseY = LookY(tree.transform.position, player.position);
                break;

            case RotationMode.FollowPivot:
                if (rotationCenter != null)
                    baseY = rotationCenter.eulerAngles.y + yRotationOffset;
                break;
        }

        // 👉 APPLY PIVOT ROTATION
        Quaternion pivotRot = rotationCenter != null
            ? rotationCenter.rotation
            : Quaternion.identity;

        Quaternion finalRot = pivotRot * Quaternion.Euler(0f, baseY, 0f);

        tree.transform.rotation = finalRot;
    }

    float LookY(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        dir.y = 0;

        if (dir.sqrMagnitude < 0.001f)
            return 0f;

        return Quaternion.LookRotation(dir).eulerAngles.y;
    }

    Vector3 GetRandomPoint(Bounds b)
    {
        return new Vector3(
            Random.Range(b.min.x, b.max.x),
            b.min.y,
            Random.Range(b.min.z, b.max.z)
        );
    }

    void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(SinkTrees());
        }
    }

    IEnumerator SinkTrees()
    {
        Vector3 playerPos = player.position;

        trees.Sort((a, b) =>
            Vector3.Distance(playerPos, a.position)
            .CompareTo(Vector3.Distance(playerPos, b.position))
        );

        foreach (Transform tree in trees)
        {
            StartCoroutine(SinkSingle(tree));
            yield return new WaitForSeconds(waveDelay);
        }
    }

    IEnumerator SinkSingle(Transform tree)
    {
        Vector3 start = tree.position;
        Vector3 end = start + Vector3.down * sinkDepth;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / sinkDuration;
            tree.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
    }
}