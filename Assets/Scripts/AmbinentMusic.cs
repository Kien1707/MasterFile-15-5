using UnityEngine;

public class AmbientMusic : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip ambientClip;

    void Start()
    {
        if (audioSource != null && ambientClip != null)
        {
            audioSource.clip = ambientClip;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AmbientMusic: Missing AudioSource or AudioClip!");
        }
    }
}
