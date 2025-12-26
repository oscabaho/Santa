using System.Collections.Generic;
using Santa.Core;
using Santa.Core.Config;
using Santa.Domain.Combat;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

namespace Santa.Infrastructure.Audio
{

/// <summary>
/// Manages playback of all game sounds and music.
/// Uses an object pool for AudioSources for efficiency.
/// </summary>
public class AudioManager : MonoBehaviour, IAudioService
{
    // PlayerPrefs keys exposed publicly for use by other scripts.
    public const string MusicVolumeKey = GameConstants.PlayerPrefsKeys.MusicVolume;
    public const string SfxVolumeKey = GameConstants.PlayerPrefsKeys.SfxVolume;

    [Header("Pool Configuration")]
    [SerializeField] private PooledAudioSource audioSourcePrefab;
    [SerializeField] private int defaultPoolSize = 20;
    [SerializeField] private int maxPoolSize = 50;

    [Header("Mixer")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;

    private ObjectPool<PooledAudioSource> _pool;
    private readonly Dictionary<GameObject, PooledAudioSource> _loopingSources = new Dictionary<GameObject, PooledAudioSource>();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (audioSourcePrefab == null)
        {
            GameLog.LogError("AudioManager: 'Audio Source Prefab' is not assigned in the Inspector. The audio system will not function. Please assign the PooledAudioSource prefab.", this);
            enabled = false;
            return;
        }

        InitializePool();
    }

    private void Start()
    {
        // On start, load saved volumes and apply them.
        LoadAndApplyInitialVolumes();
    }

    private void InitializePool()
    {
        _pool = new ObjectPool<PooledAudioSource>(
            CreatePooledObject,
            OnTakeFromPool,
            OnReturnedToPool,
            OnDestroyObject,
            true,
            defaultPoolSize,
            maxPoolSize
        );
    }

    public void PlaySound2D(AudioData audioData)
    {
        var pooledSource = _pool.Get();
        pooledSource.Play(audioData, 0f); // 2D sound
    }

    public void PlaySound3D(AudioData audioData, Vector3 position)
    {
        var pooledSource = _pool.Get();
        pooledSource.transform.position = position;
        pooledSource.Play(audioData, 1f); // 3D sound
    }

    public void PlayLoopingSoundOnObject(AudioData audioData, GameObject target)
    {
        if (_loopingSources.ContainsKey(target)) return;

        var pooledSource = _pool.Get();
        pooledSource.transform.SetParent(target.transform);
        pooledSource.transform.localPosition = Vector3.zero;

        // --- KEY LOGIC ---
        // Attempt to play sound. If it fails, PooledAudioSource already returned to pool.
        // Do not add to dictionary to avoid inconsistent state.
        bool success = pooledSource.Play(audioData, 1.0f, true);
        if (success)
        {
            _loopingSources[target] = pooledSource;
        }
        else
        {
            // If it failed, ensure it's unparented so it doesn't remain attached
            // to the target object while inactive in the pool.
            pooledSource.transform.SetParent(transform);
        }
    }

    public void StopLoopingSoundOnObject(GameObject target)
    {
        if (_loopingSources.TryGetValue(target, out var pooledSource))
        {
            pooledSource.Stop(); // Stop will handle returning it to the pool
            _loopingSources.Remove(target);
        }
    }

    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicSource == null || musicClip == null) return;
        musicSource.clip = musicClip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    /// <summary>
    /// Sets the volume of an exposed parameter in the AudioMixer.
    /// </summary>
    /// <param name="parameterName">Exposed parameter name (e.g. "MusicVolume").</param>
    /// <param name="volume">Volume on a linear scale (0.0 to 1.0).</param>
    public void SetVolume(string parameterName, float volume)
    {
        if (mainMixer == null) return;

        // Convert linear volume (0-1) to decibels (logarithmic).
        // A value of 0.0001f is effectively silence (-80dB).
        float dbVolume = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20;
        mainMixer.SetFloat(parameterName, dbVolume);
    }

    /// <summary>
    /// Gets the volume of an exposed parameter in the AudioMixer.
    /// </summary>
    /// <param name="parameterName">Exposed parameter name (e.g. "MusicVolume").</param>
    /// <returns>Volume on a linear scale (0.0 to 1.0).</returns>
    public float GetVolume(string parameterName)
    {
        if (mainMixer != null && mainMixer.GetFloat(parameterName, out float dbVolume))
        {
            // Convert decibels back to linear scale.
            return Mathf.Pow(10, dbVolume / 20);
        }
        // If retrieval fails, return 0 as a safe value.
        return 0f;
    }

    private void LoadAndApplyInitialVolumes()
    {
        // Load music volume or use 0.75f default.
        float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.75f);
        SetVolume(MusicVolumeKey, musicVolume);

        // Load SFX volume or use 1.0f default.
        float sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1.0f);
        SetVolume(SfxVolumeKey, sfxVolume);
    }

    #region Pool Callbacks
    private PooledAudioSource CreatePooledObject()
    {
        var newSource = Instantiate(audioSourcePrefab, transform);
        newSource.Pool = _pool;
        return newSource;
    }

    private void OnTakeFromPool(PooledAudioSource pooledSource)
    {
        pooledSource.gameObject.SetActive(true);
    }

    private void OnReturnedToPool(PooledAudioSource pooledSource)
    {
        // Limpiar el estado antes de devolverlo
        pooledSource.transform.SetParent(transform);
        pooledSource.Source.clip = null;
        pooledSource.gameObject.SetActive(false);
    }

    private void OnDestroyObject(PooledAudioSource pooledSource)
    {
        Destroy(pooledSource.gameObject);
    }
    #endregion
}
}