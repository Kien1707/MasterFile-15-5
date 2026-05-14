using UnityEngine;
using System.Collections;

public class BurstTest : MonoBehaviour
{
    [Header("GOOD Mesh Source")]
    public MeshFilter meshSourceGood;
    public Renderer materialSourceGood;

    [Header("BAD Mesh Source")]
    public MeshFilter meshSourceBad;
    public Renderer materialSourceBad;

    [Header("Boundary (assign empty with collider)")]
    public Collider boundaryCollider;

    [Header("Burst Settings")]
    public int burstCount = 150;
    public float lifetime = 2f;
    public float destroyAfter = 5f;

    [Header("Burst Timing")]
    public float burstDuration = 0.5f;
    public float burstInterval = 0.05f;

    private FruitCounter fruitCounter;
    private bool hasBurst = false;

    void Start()
    {
        fruitCounter = FindObjectOfType<FruitCounter>();
    }

    void Update()
    {
        if (fruitCounter == null || hasBurst)
            return;

        // CHỈ EMIT KHI ĐỦ 8 QUẢ
        int total = fruitCounter.GoodCount + fruitCounter.BadCount;

        if (total == 8)
        {
            hasBurst = true;

            if (fruitCounter.GoodCount > fruitCounter.BadCount)
            {
                StartCoroutine(BurstForDuration(meshSourceGood, materialSourceGood));
            }
            else if (fruitCounter.BadCount > fruitCounter.GoodCount)
            {
                StartCoroutine(BurstForDuration(meshSourceBad, materialSourceBad));
            }
            else
            {
                Debug.Log("Neutral ending → no burst");
            }
        }
    }

    IEnumerator BurstForDuration(MeshFilter meshSrc, Renderer matSrc)
    {
        float timer = 0f;

        while (timer < burstDuration)
        {
            EmitBurst(meshSrc, matSrc);
            timer += burstInterval;
            yield return new WaitForSeconds(burstInterval);
        }
    }

    void EmitBurst(MeshFilter meshSrc, Renderer matSrc)
    {
        if (meshSrc == null || matSrc == null || boundaryCollider == null)
        {
            Debug.LogWarning("Missing meshSource, materialSource, or boundaryCollider!");
            return;
        }

        for (int i = 0; i < burstCount; i++)
        {
            Vector3 randomPos = GetRandomPointInsideCollider(boundaryCollider);

            GameObject obj = new GameObject("MeshParticle");
            obj.transform.position = randomPos;

            ParticleSystem ps = obj.AddComponent<ParticleSystem>();

            // MAIN
            var main = ps.main;
            main.loop = false;
            main.startLifetime = lifetime;
            main.startSpeed = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            // RANDOM VELOCITY
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;

            Vector3 dir = Random.onUnitSphere;
            vel.x = dir.x * 2f;
            vel.y = Mathf.Abs(dir.y) * 3f;
            vel.z = dir.z * 2f;

            // EMISSION (1 particle)
            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, 1)
            });

            // RENDERER — dùng mesh + material tương ứng
            var renderer = obj.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.mesh = meshSrc.sharedMesh;
            renderer.material = matSrc.sharedMaterial;

            ps.Play();
            Destroy(obj, destroyAfter);
        }
    }

    Vector3 GetRandomPointInsideCollider(Collider col)
    {
        Vector3 min = col.bounds.min;
        Vector3 max = col.bounds.max;

        Vector3 point;

        do
        {
            point = new Vector3(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y),
                Random.Range(min.z, max.z)
            );
        }
        while (col.ClosestPoint(point) != point);

        return point;
    }
}
