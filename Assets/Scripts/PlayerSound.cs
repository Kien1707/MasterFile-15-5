using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    public AudioSource audioSource;          // Nơi phát âm thanh
    public PlayerSoundLibrary library;       // Thư viện chứa các clip

    public void Play(PlayerAction action)
    {
        AudioClip clip = library.GetClip(action);

        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("Không có clip cho action: " + action);
        }
    }
}
