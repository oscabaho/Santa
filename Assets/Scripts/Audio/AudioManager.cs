using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

/// <summary>
/// Gestiona la reproducción de todos los sonidos y música del juego.
/// Utiliza un pool de objetos para los AudioSources para ser más eficiente.
/// </summary>
public class AudioManager : MonoBehaviour
{
    // Claves para PlayerPrefs, públicas para que otros scripts puedan usarlas.
    public const string MusicVolumeKey = "MusicVolume";
    public const string SfxVolumeKey = "SFXVolume";

    public static AudioManager Instance { get; private set; }

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
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSourcePrefab == null)
        {
            Debug.LogError("AudioManager: 'Audio Source Prefab' no está asignado en el Inspector. El sistema de audio no funcionará. Por favor, asigna el prefab de PooledAudioSource.", this);
            enabled = false;
            return;
        }

        InitializePool();
    }

    private void Start()
    {
        // Al iniciar, cargamos los volúmenes guardados y los aplicamos.
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

        // --- LÓGICA CLAVE ---
        // Intentamos reproducir el sonido. Si falla, el PooledAudioSource ya se ha devuelto al pool.
        // No lo añadimos a nuestro diccionario para evitar un estado inconsistente.
        bool success = pooledSource.Play(audioData, 1.0f, true);
        if (success)
        {
            _loopingSources[target] = pooledSource;
        }
        else
        {
            // Si falló, nos aseguramos de quitarle el parentesco para que no se quede "pegado"
            // al objeto target mientras está inactivo en el pool.
            pooledSource.transform.SetParent(transform);
        }
    }

    public void StopLoopingSoundOnObject(GameObject target)
    {
        if (_loopingSources.TryGetValue(target, out var pooledSource))
        {
            pooledSource.Stop(); // Stop se encargará de devolverlo al pool
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
    /// Establece el volumen de un parámetro expuesto en el AudioMixer.
    /// </summary>
    /// <param name="parameterName">El nombre del parámetro expuesto (ej. "MusicVolume").</param>
    /// <param name="volume">El volumen en una escala lineal (0.0 a 1.0).</param>
    public void SetVolume(string parameterName, float volume)
    {
        if (mainMixer == null) return;

        // Convertimos el volumen lineal (0-1) a decibelios (logarítmico).
        // Un valor de 0.0001f es efectivamente silencio (-80dB).
        float dbVolume = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20;
        mainMixer.SetFloat(parameterName, dbVolume);
    }

    /// <summary>
    /// Obtiene el volumen de un parámetro expuesto en el AudioMixer.
    /// </summary>
    /// <param name="parameterName">El nombre del parámetro expuesto (ej. "MusicVolume").</param>
    /// <returns>El volumen en una escala lineal (0.0 a 1.0).</returns>
    public float GetVolume(string parameterName)
    {
        if (mainMixer != null && mainMixer.GetFloat(parameterName, out float dbVolume))
        {
            // Convertimos los decibelios de vuelta a una escala lineal.
            return Mathf.Pow(10, dbVolume / 20);
        }
        // Si no se puede obtener, devolvemos 0 como valor seguro.
        return 0f;
    }

    private void LoadAndApplyInitialVolumes()
    {
        // Carga el volumen de la música o usa 0.75f si no hay nada guardado.
        float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.75f);
        SetVolume(MusicVolumeKey, musicVolume);

        // Carga el volumen de los SFX o usa 1.0f si no hay nada guardado.
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
