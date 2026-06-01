using UnityEngine;

public class GhostDissolve : MonoBehaviour
{
    [Header("References")]
    public Renderer ghostRenderer;

    [Header("Dissolve Settings")]
    public float minDissolve = 0.3f;
    public float maxDissolve = 0.8f;
    public float speed = 1f;

    private MaterialPropertyBlock propBlock;

    void Start()
    {
        propBlock = new MaterialPropertyBlock();
        if (ghostRenderer == null)
            ghostRenderer = GetComponent<Renderer>();
    }

    void Update()
{
    float dissolve = Mathf.Lerp(minDissolve, maxDissolve, (Mathf.Sin(Time.time * speed) + 1f) / 2f);
    ghostRenderer.material.SetFloat("_Dissolve", dissolve);
}
}