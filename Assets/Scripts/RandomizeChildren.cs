using UnityEngine;

public class RandomizeChildren : MonoBehaviour
{
    [Header("Rotation")]
    public bool randomizeRotation = true;

    [Header("Scale")]
    public bool randomizeScale = true;
    public float minScale = 1f;
    public float maxScale = 2f;

    void Start()
    {
        foreach (Transform child in transform)
        {
            if (randomizeRotation)
                child.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            if (randomizeScale)
            {
                float scale = Random.Range(minScale, maxScale);
                child.localScale = Vector3.one * scale;
            }
        }
    }
}