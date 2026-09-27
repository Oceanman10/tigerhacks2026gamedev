using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Syncs one Slider to one exposed AudioMixer parameter.
/// Reuse this same script for Master, Music, and SFX - just change
/// "exposedParameter" and assign a different Slider each time.
/// </summary>
public class MixerVolumeControl : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string exposedParameter = "MasterVolume"; // or "MusicVolume" / "SFXVolume"
    [SerializeField] private Slider slider;

    private void Awake()
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;

        slider.onValueChanged.AddListener(OnSliderChanged);

        // Read whatever the mixer's current value is so the slider starts in sync.
        if (audioMixer.GetFloat(exposedParameter, out float currentDb))
        {
            slider.SetValueWithoutNotify(DecibelsToNormalized(currentDb));
        }
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        float dB = NormalizedToDecibels(value);
        bool success = audioMixer.SetFloat(exposedParameter, dB);
        Debug.Log($"[{exposedParameter}] slider={value}, dB={dB}, SetFloat success={success}");
    }

    private static float NormalizedToDecibels(float normalized)
    {
        // Log scale sounds more natural to human hearing than linear.
        return Mathf.Log10(Mathf.Max(normalized, 0.0001f)) * 20f;
    }

    private static float DecibelsToNormalized(float dB)
    {
        return Mathf.Pow(10f, dB / 20f);
    }
}
