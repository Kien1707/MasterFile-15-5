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
        if (fruitMaterial == null || micInput == null)
            return;

        float loudness = MicInput.Loudness;
        Color targetEmission = Color.black;

        PickableFruit fruitScript = GetComponent<PickableFruit>();
        bool isHeldFruit = fruitScript != null && fruitScript.isHeld;

        bool inRange = player != null &&
                       Vector3.Distance(player.position, transform.position) <= activationRadius;

        if (isHeldFruit && inRange && loudness > 0f)
        {
            Color glowColor = isGoodFruit ? micInput.goodColor : micInput.badColor;
            float intensity = Mathf.Lerp(0f, maxEmission, loudness);
            targetEmission = glowColor * intensity;
        }

        currentEmission = Color.Lerp(currentEmission, targetEmission, Time.deltaTime * smoothSpeed);
        fruitMaterial.SetColor("_EmissionColor", currentEmission);
    }
}
