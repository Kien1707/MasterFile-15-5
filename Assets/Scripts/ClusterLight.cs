using UnityEngine;

public class LightByClusterState : MonoBehaviour
{
    // Intensity per state: -2, -1, 0, 1, 2
    private float[] intensityByState = new float[] { 0f, 25f, 50f, 300f, 600f };
    
    private Light myLight;
    private ClusterStates cluster;

    void Start()
    {
        myLight = GetComponent<Light>();
        
        // Find the parent cluster
        cluster = GetComponentInParent<ClusterStates>();
        
        if (cluster == null)
        {
            Debug.LogWarning($"No ClusterStates found on parent of {gameObject.name}");
        }
    }

    void Update()
    {
        if (cluster == null || myLight == null) return;
        
        int state = cluster.currentState; // 0=-2, 1=-1, 2=0, 3=1, 4=2
        
        if (state >= 0 && state < intensityByState.Length)
        {
            myLight.intensity = intensityByState[state];
        }
    }
}