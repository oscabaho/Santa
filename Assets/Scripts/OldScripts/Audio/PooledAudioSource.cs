using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace ProyectSecret.Audio
{
    /// <summary>
    /// Componente para un AudioSource gestionado por un ObjectPool.
    /// Se encarga de devolverse al pool cuando termina de reproducir el audio.
    /// Su única responsabilidad es reproducir un sonido configurado y gestionar su
    /// ciclo de vida para volver al pool.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class PooledAudioSource : MonoBehaviour
    {
        private AudioSource _audioSource;
        private Coroutine _returnToPoolCoroutine;

        /// <summary>
        /// El componente AudioSource nativo. Útil para configuraciones avanzadas (ej. 3D).
        /// </summary>
        public AudioSource Source => _audioSource;

        /// <summary>
        /// El pool de objetos al que pertenece esta instancia.
        /// </summary>
        public IObjectPool<PooledAudioSource> Pool { get; set; }

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        /// <summary>
        /// Inicia la reproducción del clip de audio con una configuración específica.
        /// </summary>
        /// <returns>True si la reproducción comenzó, false si falló (ej. clip nulo).</returns>
        public bool Play(AudioData audioData, float spatialBlend = 1.0f, bool forceLoop = false)
        {
            if (audioData == null || audioData.GetClip() == null) // Usar GetClip para validar que haya al menos un clip
            {
                Debug.LogWarning("Se intentó reproducir un AudioClip nulo. Devolviendo al pool de inmediato.");
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
        /// Detiene la reproducción del sonido y lo devuelve inmediatamente al pool.
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
}
