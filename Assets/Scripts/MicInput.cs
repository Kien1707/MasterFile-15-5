using UnityEngine;
public class MicInput : MonoBehaviour
{
    public static float Loudness; //GLOBAL ACCESS
    public int micIndex = 0;

    public  Color goodColor = Color.green;
    public  Color badColor = Color.red;

    public float sensitivity = 100f;
    public float noiseThreshold = 0.05f;

    private AudioClip micClip;
    private string micDevice;
    private float[] samples = new float[128];

    void Start()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone found!");
            return;
        }

        micIndex = Mathf.Clamp(micIndex, 0, Microphone.devices.Length - 1);

        micDevice = Microphone.devices[micIndex];
        micClip = Microphone.Start(micDevice, true, 1, 44100);

    }

    void Update()
    {
        float loudness = GetMicLoudness() * sensitivity;

        if (loudness < noiseThreshold)
            loudness = 0;

        Loudness = loudness; //store it globally
    }

    float GetMicLoudness()
    {
        int micPosition = Microphone.GetPosition(micDevice) - samples.Length;

        if (micPosition < 0)
            return 0;

        micClip.GetData(samples, micPosition);

        float levelMax = 0;

        for (int i = 0; i < samples.Length; i++)
        {
            float wavePeak = Mathf.Abs(samples[i]);
            if (wavePeak > levelMax)
                levelMax = wavePeak;
        }

        return levelMax;
    }
}