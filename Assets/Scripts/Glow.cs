using UnityEngine;

public class Glow : MonoBehaviour
{
    [Header("Player Reference")]
    public Transform player;

    [Header("Activation Distance")]
    public float activationRadius = 5f;

    [Header("Glow Settings")]
    public float maxEmission = 2f;
    public float smoothSpeed = 5f;

    [Header("Fruit Type")]
    public bool isGoodFruit = true;

    [Header("Mic Input Reference")]
    public MicInput micInput;

    private Material fruitMaterial;
    private Color currentEmission;

    void Start()
    {
        Renderer rend = GetComponentInChildren<Renderer>();

        if (rend != null)
        {
            fruitMaterial = rend.material;
            fruitMaterial.EnableKeyword("_EMISSION");

            currentEmission = Color.black;
            fruitMaterial.SetColor("_EmissionColor", Color.black);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log($"M pressed - Mic loudness: {MicInput.Loudness}, MicInput exists: {micInput != null}");
        }
        
        if (fruitMaterial == null || micInput == null)
        {
            return;
        }

        float loudness = MicInput.Loudness;
        Color targetEmission = Color.black;

        // FIXED: Uses isHeld from PickableFruit
        bool isHeldFruit = false;
        PickableFruit fruitScript = GetComponent<PickableFruit>();
        if (fruitScript != null)
        {
            isHeldFruit = fruitScript.isHeld;
        }
        
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"FRUIT: {gameObject.name}, isHeld={isHeldFruit}, loudness={loudness:F2}");
        }
        
        bool inRange = player != null && Vector3.Distance(player.position, transform.position) <= activationRadius;

        if (isHeldFruit && inRange && loudness > 0f)
        {
            Color glowColor = isGoodFruit ? micInput.goodColor : micInput.badColor;
            float intensity = Mathf.Lerp(0f, maxEmission, loudness);
            targetEmission = glowColor * intensity;
            Debug.Log($"✓ GLOWING! isGood={isGoodFruit}, intensity={intensity:F2}");
        }

        currentEmission = Color.Lerp(currentEmission, targetEmission, Time.deltaTime * smoothSpeed);
        fruitMaterial.SetColor("_EmissionColor", currentEmission);
    }
}