using UnityEngine;

[RequireComponent(typeof(AudioListener))]
public class PitchShifter : MonoBehaviour
{
    // Pitch factor, where 1.0 is the normal pitch, >1 is higher pitch, <1 is lower pitch
    public float pitch = 1.0f;

    // Store the timeScale value
    private float currentPitch;
    private float smoothedPitch;
    public float smoothingSpeed = 10.0f; // Higher value for faster smoothing

    void Start()
    {
        // Initialize current pitch based on Time.timeScale
        currentPitch = Time.timeScale;
        smoothedPitch = currentPitch;
    }

    void Update()
    {
        // Update current pitch from Time.timeScale
        currentPitch = Time.timeScale;

        // Smoothly transition the pitch to avoid abrupt changes
        smoothedPitch = Mathf.Lerp(smoothedPitch, currentPitch, smoothingSpeed * Time.deltaTime);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        // Use smoothedPitch for interpolation instead of the raw currentPitch
        pitch = smoothedPitch;

        // Loop through the incoming audio data
        for (int i = 0; i < data.Length; i += channels)
        {
            // Basic pitch-shifting by resampling the audio based on pitch value
            float sample = InterpolatePitchShift(i / channels, data, channels);

            // Store the shifted sample back in the audio buffer for output
            for (int j = 0; j < channels; j++)
            {
                data[i + j] = sample;
            }
        }
    }

    // Cubic interpolation for smoother audio data processing
    float InterpolatePitchShift(int sampleIndex, float[] data, int channels)
    {
        // Calculate where the sample should come from based on the pitch
        float samplePosition = sampleIndex * pitch;

        // Ensure samplePosition is within bounds
        if (samplePosition >= data.Length / channels - 2)
        {
            return data[(data.Length / channels - 1) * channels]; // Return last valid sample
        }

        if (samplePosition < 1)
        {
            return data[0]; // Return first sample if out of bounds
        }

        // Get surrounding indices for cubic interpolation
        int sampleInt = Mathf.FloorToInt(samplePosition);
        int prevSample = Mathf.Max(sampleInt - 1, 0);
        int nextSample = Mathf.Min(sampleInt + 1, data.Length / channels - 1);
        int nextNextSample = Mathf.Min(sampleInt + 2, data.Length / channels - 1);

        // Calculate interpolation weight
        float interpolationWeight = samplePosition - sampleInt;

        // Cubic interpolation for smoother results
        float sampleA = data[prevSample * channels];
        float sampleB = data[sampleInt * channels];
        float sampleC = data[nextSample * channels];
        float sampleD = data[nextNextSample * channels];

        return CubicInterpolate(sampleA, sampleB, sampleC, sampleD, interpolationWeight);
    }

    // Cubic interpolation formula
    float CubicInterpolate(float a, float b, float c, float d, float t)
    {
        float t2 = t * t;
        float a0 = d - c - a + b;
        float a1 = a - b - a0;
        float a2 = c - a;
        float a3 = b;

        return a0 * t * t2 + a1 * t2 + a2 * t + a3;
    }
}
