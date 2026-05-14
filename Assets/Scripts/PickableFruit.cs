using UnityEngine;
using System.Collections;

public class PickableFruit : MonoBehaviour
{
    [Header("Hold Settings")]
    public float holdDistance = 1.5f;
    public float hoverHeight = 0.5f;
    public float hoverSpeed = 2f;
    public float rotateSpeed = 60f;
    public float heightOffset = 0.2f;

    [Header("Animation")]
    public string animationTriggerName = "Trigger";

    [Header("References")]
    public Animator fruitAnimator;

    [Header("Particle Effects")]
    public ParticleSystem unfoldParticle;
    public float particleDelay = 2f;

    [Header("Disappear Settings")]
    public float disappearDelay = 3f;
    public GameObject poofEffect;

    private bool playerInRange = false;
    private Transform player;
    private bool isOnGround = false;
    private bool isAnimating = false;
    private bool isDisappearing = false;

    public bool isHeld = false;  // ← CHANGED from isHeldFruit

    private Rigidbody rb;
    private Vector3 animationLockPosition;

    // REQUIRED BY MicUI + Glow
    public static GameObject currentlyHeldFruit = null;
    public static bool AnyFruitHeld { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (fruitAnimator == null)
            fruitAnimator = GetComponentInChildren<Animator>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        // Debug auto-pickup
        if (playerInRange && !isHeld && currentlyHeldFruit == null)
        {
            Debug.Log($"Player in range, isOnGround={isOnGround}, E key down={Input.GetKeyDown(KeyCode.E)}");
        }

        // Pick up (FIXED: uses isHeld not isHeldFruit)
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isHeld && !isOnGround && currentlyHeldFruit == null)
            HoldFruit();

        // Drop
        else if (isHeld && Input.GetKeyDown(KeyCode.E) && !isAnimating && !isDisappearing)
            DropFruit();

        // Pick up from ground
        else if (isOnGround && playerInRange && Input.GetKeyDown(KeyCode.E) && currentlyHeldFruit == null)
            PickupFromGround();

        // Trigger animation
        else if (isHeld && Input.GetKeyDown(KeyCode.F) && !isAnimating && !isDisappearing)
            TriggerAnimation();

        if (isHeld && !isAnimating && !isDisappearing)
            HoverAbovePlayer();
    }

    void NotifyCluster()
    {
        Debug.Log($"=== NotifyCluster CALLED from {gameObject.name} ===");
        
        ClusterStates[] allClusters = FindObjectsOfType<ClusterStates>();
        Debug.Log($"Found {allClusters.Length} clusters in scene");
        GroworWilt[] newClusters = FindObjectsOfType<GroworWilt>();
        
        foreach (GroworWilt cluster in newClusters)
        {
            if (cluster.IsPlayerInRange())
            {
                cluster.OnFruitAnimationTriggered(this.gameObject);
                return;
            }
        }
        
        foreach (ClusterStates cluster in allClusters)
        {
            bool isPlayerInRange = cluster.IsPlayerInRange();
            Debug.Log($"Cluster {cluster.name}: IsPlayerInRange = {isPlayerInRange}");
            
            if (isPlayerInRange)
            {
                Debug.Log($"Found cluster with player in range! Sending to {cluster.name}");
                cluster.OnFruitAnimationTriggered(this.gameObject);
                return;
            }
        }
        
        Debug.Log("No cluster with player in range found!");
    }

    public void HoldFruit()
    {    
        Debug.Log($"=== HOLD FRUIT: {gameObject.name} ===");
        isHeld = true;
        currentlyHeldFruit = this.gameObject;
        AnyFruitHeld = true;
        Debug.Log($"currentlyHeldFruit set to: {currentlyHeldFruit?.name}");

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void PickupFromGround()
    {
        isHeld = true;
        isOnGround = false;
        currentlyHeldFruit = this.gameObject;
        AnyFruitHeld = true;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void DropFruit()
    {
        isHeld = false;
        currentlyHeldFruit = null;
        AnyFruitHeld = false;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    private void TriggerAnimation()
    {
        if (fruitAnimator == null)
            return;

        isAnimating = true;
        animationLockPosition = transform.position;

        fruitAnimator.SetTrigger(animationTriggerName);

        NotifyCluster();

        StartCoroutine(DelayedParticle());
        StartCoroutine(WaitForAnimation());
    }

    private IEnumerator DelayedParticle()
    {
        yield return new WaitForSeconds(particleDelay);

        if (unfoldParticle != null)
            unfoldParticle.Play();
    }

    private IEnumerator WaitForAnimation()
    {
        yield return null;

        AnimatorStateInfo info = fruitAnimator.GetCurrentAnimatorStateInfo(0);
        float length = info.length;

        float t = 0f;
        while (t < length)
        {
            t += Time.deltaTime;
            transform.position = animationLockPosition;
            yield return null;
        }

        isAnimating = false;
        StartCoroutine(Disappear());
    }

    private IEnumerator Disappear()
    {
        isDisappearing = true;

        if (currentlyHeldFruit == this.gameObject)
        {
            currentlyHeldFruit = null;
            AnyFruitHeld = false;
            isHeld = false;
        }

        yield return new WaitForSeconds(disappearDelay);

        if (poofEffect != null)
        {
            GameObject fx = Instantiate(poofEffect, transform.position, Quaternion.identity);
            Destroy(fx, 3f);
        }

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        gameObject.SetActive(false);
        isDisappearing = false;
    }

    private void HoverAbovePlayer()
    {
        if (player == null) return;

        float playerHeight = 1f;
        
        CapsuleCollider capsule = player.GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            playerHeight = capsule.height;
        }
        else
        {
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                playerHeight = controller.height;
            }
        }
        
        Vector3 target = player.position;
        target.y += playerHeight + hoverHeight + heightOffset;
        
        float hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * 0.1f;
        target.y += hoverOffset;
        
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 10f);
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isHeld && !isOnGround && collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = true;

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }
    }

    void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player")) 
        {
            Debug.Log("TRIGGER ENTERED");
            playerInRange = true;
            player = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (!isHeld)
                player = null;
        }
    }
}