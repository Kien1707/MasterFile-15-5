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

    [Header("Jiggle settings")]
    public float duration = 0.7f;
    public float speed = 10f;
    public float amplitude = 0.15f;

    [Header("Sound")]
    public PlayerSound sound;

    [Header("Crack Settings")]
    public Transform crackVolume;
    public Vector3 crackTargetScale = new Vector3(2f, 2f, 2f);
    public float crackGrowDuration = 2f;
    public float crackGroundY = 0f;

    private bool playerInRange = false;
    private Transform player;
    private bool isOnGround = false;
    private bool isAnimating = false;
    private bool isDisappearing = false;
    private bool isJiggling = false;

    public bool isHeld = false;

    private Rigidbody rb;
    private Vector3 animationLockPosition;

    public static GameObject currentlyHeldFruit = null;
    public static bool AnyFruitHeld;

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
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isHeld && !isOnGround && currentlyHeldFruit == null)
            HoldFruit();

        else if (isHeld && Input.GetKeyDown(KeyCode.E) && !isAnimating && !isDisappearing)
            DropFruit();

        else if (isOnGround && playerInRange && Input.GetKeyDown(KeyCode.E) && currentlyHeldFruit == null)
            PickupFromGround();

        else if (isHeld && Input.GetKeyDown(KeyCode.F) && !isAnimating && !isDisappearing)
        {
            bool block = false;
            GameObject[] clusters = GameObject.FindGameObjectsWithTag("Cluster");
            foreach (GameObject clusterObj in clusters)
            {
                GroworWilt gw = clusterObj.GetComponent<GroworWilt>();
                if (gw != null && gw.IsPlayerInRange() && gw.currentState != 0)
                {
                    block = true;
                    break;
                }
            }

            if (block)
            {
                StartCoroutine(JiggleFruit());
            }
            else
            {
                if (sound != null)
                {
                    Glow glow = GetComponent<Glow>();
                    if (glow != null)
                    {
                        if (glow.isGoodFruit)
                            sound.Play(PlayerAction.UnfoldGood);
                        else
                            sound.Play(PlayerAction.UnfoldBad);
                    }
                }

                TriggerAnimation();
            }
        }

        if (isHeld && !isAnimating && !isDisappearing && !isJiggling)
            HoverAbovePlayer();
    }

    public void HoldFruit()
    {
        if (sound != null)
            sound.Play(PlayerAction.PickTheFruit);

        isHeld = true;
        currentlyHeldFruit = this.gameObject;
        AnyFruitHeld = true;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void PickupFromGround()
    {
        if (sound != null)
            sound.Play(PlayerAction.PickTheFruit);

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

    private IEnumerator JiggleFruit()
    {
        isJiggling = true;
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float offset = Mathf.Sin(elapsed * speed * Mathf.PI * 2f) * amplitude;
            transform.position = startPos + transform.right * offset;
            yield return null;
        }

        transform.position = startPos;
        isJiggling = false;
    }

    private void TriggerAnimation()
    {
        if (fruitAnimator == null)
            return;

        isAnimating = true;
        animationLockPosition = transform.position;

        fruitAnimator.SetTrigger(animationTriggerName);

        NotifyCluster();

        FruitCounter counter = FindFirstObjectByType<FruitCounter>();
        if (counter != null)
        {
            Glow glow = GetComponent<Glow>();
            if (glow != null)
                counter.AddFruit(glow.isGoodFruit);
        }

        Glow fruitGlow = GetComponent<Glow>();
        if (fruitGlow != null && !fruitGlow.isGoodFruit)
            StartCoroutine(GrowCrack());

        StartCoroutine(DelayedParticle());
        StartCoroutine(WaitForAnimation());
    }

    private IEnumerator GrowCrack()
    {
        if (crackVolume == null) yield break;

        // snap to ground at fruit's XZ position
        crackVolume.position = new Vector3(transform.position.x, crackGroundY, transform.position.z);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / crackGrowDuration;
            crackVolume.localScale = Vector3.Lerp(Vector3.zero, crackTargetScale, t);
            yield return null;
        }
        crackVolume.localScale = crackTargetScale;
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
            playerHeight = capsule.height;
        else
        {
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
                playerHeight = controller.height;
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

    void NotifyCluster()
    {
        GroworWilt[] clusters = FindObjectsOfType<GroworWilt>();
        foreach (GroworWilt cluster in clusters)
        {
            if (cluster.IsPlayerInRange())
            {
                cluster.OnFruitAnimationTriggered(this.gameObject);
                return;
            }
        }
    }
}