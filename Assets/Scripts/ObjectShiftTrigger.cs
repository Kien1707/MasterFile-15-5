using UnityEngine;

public class ObjectShiftTrigger : MonoBehaviour
{
    public Transform[] objectsToShift;
    public Vector3 shiftOffset = new Vector3(0.2f, 0f, 0f);

    private Vector3[] originalPositions;
    private bool shifted = false;

    void Start()
    {
        originalPositions = new Vector3[objectsToShift.Length];

        for (int i = 0; i < objectsToShift.Length; i++)
        {
            originalPositions[i] = objectsToShift[i].position;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (shifted) return;

        for (int i = 0; i < objectsToShift.Length; i++)
        {
            objectsToShift[i].position = originalPositions[i] + shiftOffset;
        }

        shifted = true;
    }

    void OnTriggerExit(Collider other)
    {
        for (int i = 0; i < objectsToShift.Length; i++)
        {
            objectsToShift[i].position = originalPositions[i];
        }

        shifted = false;
    }
}