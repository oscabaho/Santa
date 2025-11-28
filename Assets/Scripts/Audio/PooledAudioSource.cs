using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Component for an AudioSource managed by an ObjectPool.
/// Returns itself to the pool when audio playback finishes.
/// Its sole responsibility is to play a configured sound and manage
/// its lifecycle for returning to the pool.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PooledAudioSource : MonoBehaviour
{
    private AudioSource _audioSource;
    private Coroutine _returnToPoolCoroutine;

    /// <summary>
    /// The native AudioSource component. Useful for advanced configurations (e.g., 3D).
    /// </summary>
    public AudioSource Source => _audioSource;

    /// <summary>
    /// The object pool this instance belongs to.
    /// </summary>
    public IObjectPool<PooledAudioSource> Pool { get; set; }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    /// <summary>
    /// Starts playback of the audio clip with specific configuration.
    /// </summary>
    /// <returns>True if playback started, false if it failed (e.g. null clip).</returns>
    public bool Play(AudioData audioData, float spatialBlend = 1.0f, bool forceLoop = false)
    {
        if (audioData == null || audioData.GetClip() == null) // Use GetClip to validate at least one clip exists
        {
            GameLog.LogWarning("Attempted to play a null AudioClip. Returning to pool immediately.");
            ReturnToPool();
            return false;
        }

        gameObject.name = $"Pooled Audio - {audioData.name}";

        _audioSource.clip = audioData.GetClip();
        _audioSource.volume = audioData.GetVolume();
        _audioSource.pitch = audioData.GetPitch();
        _audioSource.loop = forceLoop || audioData.loop;
        _audioSource.outputAudioMixerGroup = audioData.outputMixerGroup;
        _audioSource.spatialBlend = spatialBlend;
        
        _audioSource.Play();

        if (!_audioSource.loop)
        {
            _returnToPoolCoroutine = StartCoroutine(ReturnToPoolWhenFinished());
        }

        return true;
    }

    /// <summary>
    /// Stops playback and immediately returns to the pool.
    /// </summary>
    public void Stop()
    {
        _audioSource.Stop();
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (_returnToPoolCoroutine != null)
        {
            StopCoroutine(_returnToPoolCoroutine);
            _returnToPoolCoroutine = null;
        }
        
        Pool?.Release(this);
    }

    private IEnumerator ReturnToPoolWhenFinished()
    {
        yield return new WaitWhile(() => _audioSource.isPlaying);
        ReturnToPool();
    }
}
