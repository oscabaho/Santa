using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// ScriptableObject containing configuration for a sound event.
/// Lets sound designers create and tweak sounds as assets,
/// decoupling audio data from code.
/// </summary>
[CreateAssetMenu(menuName = "ProyectSecret/Audio/Audio Data", fileName = "NewAudioData")]
public class AudioData : ScriptableObject
{
    [Header("Sound Definition")]
    [Tooltip("Audio clip(s) to play. If more than one, one will be chosen at random.")]
    public AudioClip[] clips;

    [Header("Configuration")]
    [Tooltip("Audio mixer group the sound will route to.")]
    public AudioMixerGroup outputMixerGroup;

    [Tooltip("Whether the sound should loop.")]
    public bool loop = false;

    [Tooltip("Base sound volume.")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [Tooltip("Base sound pitch.")]
    [Range(0.1f, 3f)]
    public float pitch = 1f;

    [Header("Randomization")]
    [Tooltip("Random volume variation (0 = none).")]
    [Range(0f, 1f)]
    public float volumeVariation = 0f;

    [Tooltip("Random pitch variation (0 = none).")]
    [Range(0f, 1f)]
    public float pitchVariation = 0f;

    public AudioClip GetClip()
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[UnityEngine.Random.Range(0, clips.Length)];
    }

    public float GetVolume()
    {
        if (volumeVariation <= 0f) return volume;
        return volume * (1 + UnityEngine.Random.Range(-volumeVariation / 2f, volumeVariation / 2f));
    }

    public float GetPitch()
    {
        if (pitchVariation <= 0f) return pitch;
        return pitch * (1 + UnityEngine.Random.Range(-pitchVariation / 2f, pitchVariation / 2f));
    }
}
