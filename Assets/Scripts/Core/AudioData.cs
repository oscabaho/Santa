using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Un ScriptableObject que contiene la configuración para un evento de sonido.
/// Esto permite a los diseñadores de sonido crear y ajustar sonidos como assets
/// en el proyecto, desacoplando los datos de audio del código.
/// </summary>
[CreateAssetMenu(menuName = "ProyectSecret/Audio/Audio Data", fileName = "NewAudioData")]
public class AudioData : ScriptableObject
{
    [Header("Sound Definition")]
    [Tooltip("El/los clip(s) de audio a reproducir. Si hay más de uno, se elegirá uno al azar.")]
    public AudioClip[] clips;

    [Header("Configuration")]
    [Tooltip("El grupo de mezcla de audio al que se enviará el sonido.")]
    public AudioMixerGroup outputMixerGroup;

    [Tooltip("Si el sonido debe reproducirse en bucle.")]
    public bool loop = false;

    [Tooltip("Volumen base del sonido.")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [Tooltip("Pitch (tono) base del sonido.")]
    [Range(0.1f, 3f)]
    public float pitch = 1f;

    [Header("Randomization")]
    [Tooltip("Variación aleatoria del volumen (0 = sin variación).")]
    [Range(0f, 1f)]
    public float volumeVariation = 0f;

    [Tooltip("Variación aleatoria del pitch (0 = sin variación).")]
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
