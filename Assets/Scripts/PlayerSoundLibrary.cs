using System.Collections.Generic;
using UnityEngine;

public enum PlayerAction
{
    Decaying1,
    Decaying2,
    FootstepsGrass,
    Growing1,
    Growing2,
    PickTheFruit,
    UnfoldBad,
    UnfoldGood,
    GoodEnding,
    BadEnding,
    NeutralEnding
}

[System.Serializable]
public class SoundEntry
{
    public PlayerAction action;
    public AudioClip clip;
}

public class PlayerSoundLibrary : MonoBehaviour
{
    public List<SoundEntry> soundEntries = new List<SoundEntry>();
    private Dictionary<PlayerAction, AudioClip> soundDict;

    void Awake()
    {
        soundDict = new Dictionary<PlayerAction, AudioClip>();

        foreach (var entry in soundEntries)
        {
            if (!soundDict.ContainsKey(entry.action))
            {
                soundDict.Add(entry.action, entry.clip);
            }
            else
            {
                Debug.LogWarning("Duplicate action found: " + entry.action);
            }
        }
    }

    public AudioClip GetClip(PlayerAction action)
    {
        if (soundDict.TryGetValue(action, out AudioClip clip))
        {
            return clip;
        }

        Debug.LogWarning("No clip found for action: " + action);
        return null;
    }
}
