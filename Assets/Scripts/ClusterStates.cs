using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections; 

public class ClusterStates : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public float detectionRadius = 2f;
    
    [Header("State Settings")]
    public int currentState = 2; // 0=-2, 1=-1, 2=0, 3=1, 4=2
    
    [Header("Mesh Names (Exact as in Hierarchy)")]
    public string[] stateMeshNames = new string[] 
    {
        "Cluster2.-2",
        "Cluster2.-1", 
        "Cluster2.0",
        "Cluster2.1",
        "Cluster2.2"
    };
    
    private GameObject[] stateMeshes = new GameObject[5];
    
    public bool IsPlayerInRange()
{
    return playerInRange;
}
    private bool playerInRange = false;
    
    void Start()
    {
        // Find player if not assigned
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
            else Debug.LogError("No player found! Assign player in Inspector or add 'Player' tag.");
        }
        
        // Find all meshes in children (including nested)
        FindAllMeshes();
        
        // Log what was found
        for (int i = 0; i < stateMeshes.Length; i++)
        {
            string stateName = GetStateName(i);
            Debug.Log($"State {stateName}: {(stateMeshes[i] != null ? "FOUND: " + stateMeshes[i].name : "NOT FOUND")}");
        }
        
        // Initialize - show starting state, hide others
        UpdateStateVisuals();
    }
    
    void FindAllMeshes()
    {
        // Search ALL children recursively
        for (int i = 0; i < stateMeshNames.Length; i++)
        {
            stateMeshes[i] = FindChildByNameRecursive(transform, stateMeshNames[i]);
        }
    }
    
    GameObject FindChildByNameRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
                return child.gameObject;
        }
        return null;
    }
    
    void Update()
    {
        
    // if (player != null)
    // {
    //     float dist = Vector3.Distance(transform.position, player.position);
        
    //     // Show distance every 60 frames (about once per second)
    //     if (Time.frameCount % 60 == 0)
    //     {
    //         Debug.Log($"Cluster {gameObject.name}: Distance to player = {dist:F2}, In range = {playerInRange}");
    //     }
    // }
        if (!playerInRange) return;
        
        // Increase state (L key)
        // if (Input.GetKeyDown(KeyCode.L))
        // {
        //     Debug.Log("L pressed - moving to NEXT state");
        //     if (currentState < 4)
        //     {
        //         ChangeState(currentState + 1);
        //     }
        //     else
        //     {
        //         Debug.Log("Already at max state (2)");
        //     }
        // }
        
        // // Decrease state (K key)
        // if (Input.GetKeyDown(KeyCode.K))
        // {
        //     Debug.Log("K pressed - moving to PREVIOUS state");
        //     if (currentState > 0)
        //     {
        //         ChangeState(currentState - 1);
        //     }
        //     else
        //     {
        //         Debug.Log("Already at min state (-2)");
        //     }
        // }
    }
    
    void ChangeState(int newState)
    {
        if (newState == currentState) return;
        
        string oldStateName = GetStateName(currentState);
        string newStateName = GetStateName(newState);
        
        Debug.Log($"Changing from {oldStateName} (index {currentState}) to {newStateName} (index {newState})");
        
        // Update current state
        currentState = newState;
        
        // Update visuals based on new state
        UpdateStateVisuals();
    }
    
    public void OnFruitAnimationTriggered(GameObject fruit)
{
    Debug.Log($"=== OnFruitAnimationTriggered CALLED! Fruit: {fruit.name} ===");
    
    // Try to get Glow component
    Glow fruitScript = fruit.GetComponent<Glow>();
    
    if (fruitScript == null)
    {
        Debug.LogError($"No Glow script found on {fruit.name}! Checking all components...");
        
        // List all components on the fruit
        Component[] allComponents = fruit.GetComponents<Component>();
        foreach (Component comp in allComponents)
        {
            Debug.Log($"  Component found: {comp.GetType().Name}");
        }
        return;
    }
    
    Debug.Log($"Glow script FOUND on {fruit.name}!");
    
    // Check if fruit is good or bad
    Debug.Log($"isGoodFruit value = {fruitScript.isGoodFruit}");
    
    if (fruitScript.isGoodFruit)
    {
        Debug.Log("Good fruit! Increasing state +1");
        StartCoroutine(DelayedStateChange(+1));
    }
    else
    {
        Debug.Log("Bad fruit! Decreasing state -1");
        StartCoroutine(DelayedStateChange(-1));
    }
}

    private IEnumerator DelayedStateChange(int direction)
{
    Debug.Log($"Waiting 2 seconds before changing state...");
    yield return new WaitForSeconds(2f);  // 2 second delay
    
    int newState = currentState + direction;
    newState = Mathf.Clamp(newState, 0, 4);
    
    Debug.Log($"Changing state from {GetStateName(currentState)} to {GetStateName(newState)}");
    
    if (newState != currentState)
    {
        currentState = newState;
        UpdateStateVisuals();
    }
}

    void UpdateStateVisuals()
    {
        // First, hide all meshes
        for (int i = 0; i < stateMeshes.Length; i++)
        {
            if (stateMeshes[i] != null)
            {
                stateMeshes[i].SetActive(false);
            }
        }
        
        // Then show meshes based on current state
        switch(currentState)
        {
            case 0: // State -2
                if (stateMeshes[0] != null) stateMeshes[0].SetActive(true);
                Debug.Log("State -2 active");
                break;
                
            case 1: // State -1
                if (stateMeshes[1] != null) stateMeshes[1].SetActive(true);
                Debug.Log("State -1 active");
                break;
                
            case 2: // State 0
                if (stateMeshes[2] != null) stateMeshes[2].SetActive(true);
                Debug.Log("State 0 active");
                break;
                
            case 3: // State 1
                if (stateMeshes[3] != null) stateMeshes[3].SetActive(true);
                Debug.Log("State 1 active");
                break;
                
            case 4: // State 2 
                if (stateMeshes[4] != null) stateMeshes[4].SetActive(true);
                Debug.Log("State 2 active");
                break;
        }
    }
    
    string GetStateName(int index)
    {
        switch(index)
        {
            case 0: return "-2";
            case 1: return "-1";
            case 2: return "0";
            case 3: return "1";
            case 4: return "2";
            default: return "unknown";
        }
    }
    
    // Visualize detection radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
    
    // Trigger detection
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered clump zone!");
            playerInRange = true;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited clump zone!");
            playerInRange = false;
        }
    }
}