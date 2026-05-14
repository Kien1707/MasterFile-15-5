using UnityEngine;

public class SpawnAndFollowCube : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject cubePrefab;      // Prefab cube để spawn
    public Transform player;           // Player transform
    public Vector3 offset = new Vector3(1f, 0f, 0f); // vị trí follow

    [Header("State")]
    public bool hasTriggered = false;  // Đánh dấu player đã chạm trigger

    private GameObject spawnedCube;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Đánh dấu player đã chạm trigger này
        hasTriggered = true;

        // Spawn instance nếu chưa spawn
        if (spawnedCube == null)
        {
            Vector3 spawnPos = player.position + offset;
            spawnedCube = Instantiate(cubePrefab, spawnPos, Quaternion.identity);

            DisableCollision(spawnedCube);
        }
    }

    private void Update()
    {
        // Nếu cube đã spawn → follow player
        if (spawnedCube != null)
        {
            spawnedCube.transform.position = player.position + offset;
        }
    }

    void DisableCollision(GameObject obj)
    {
        // Xóa collider nếu có
        Collider col = obj.GetComponent<Collider>();
        if (col != null)
            Destroy(col);

        // Nếu có rigidbody → tắt collision
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
    }

    // Hàm cho script khác kiểm tra
    public bool PlayerHasTriggered()
    {
        return hasTriggered;
    }

    // Hàm lấy cube clone
    public GameObject GetSpawnedCube()
    {
        return spawnedCube;
    }

    // Hàm xóa cube clone
    public void ClearSpawnedCube()
    {
        if (spawnedCube != null)
        {
            Destroy(spawnedCube);
            spawnedCube = null;
        }
    }
}
