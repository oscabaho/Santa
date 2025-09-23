using UnityEngine;

public interface IAudioService
{
    void PlaySound2D(AudioData audioData);
    void PlaySound3D(AudioData audioData, Vector3 position);
    void PlayLoopingSoundOnObject(AudioData audioData, GameObject target);
    void StopLoopingSoundOnObject(GameObject target);
    void PlayMusic(AudioClip musicClip, bool loop = true);
}
