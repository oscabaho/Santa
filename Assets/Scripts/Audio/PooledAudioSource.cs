using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
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
    private CancellationTokenSource _playbackCTS;

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
            _playbackCTS?.Cancel();
            _playbackCTS = new CancellationTokenSource();
            ReturnToPoolWhenFinishedAsync(_playbackCTS.Token).Forget();
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
        if (_playbackCTS != null)
        {
            _playbackCTS.Cancel();
            _playbackCTS = null;
        }

        Pool?.Release(this);
    }

    private async UniTaskVoid ReturnToPoolWhenFinishedAsync(CancellationToken token)
    {
        // Wait while playing, checking every frame (PlayerLoopTiming.Update)
        // If cancelled (Stop called), this throws OperationCanceledException which is handled by UniTaskVoid (logs if not handled, but here it just stops)
        // Actually UniTaskVoid.Forget() swallows exceptions usually, but cancellation is fine.
        // To be safe against cancellation throwing, we can try/catch or use SuppressCancellationThrow

        bool canceled = await UniTask.WaitWhile(() => _audioSource != null && _audioSource.isPlaying, PlayerLoopTiming.Update, token).SuppressCancellationThrow();

        if (!canceled)
        {
            ReturnToPool();
        }
    }
}
