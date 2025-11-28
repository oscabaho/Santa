using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Component for a particle system managed by an ObjectPool.
/// Returns itself to the pool when its effect ends.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class PooledParticleSystem : MonoBehaviour
{
    private ParticleSystem _particleSystem;

    /// <summary>
    /// The object pool this instance belongs to.
    /// </summary>
    public IObjectPool<PooledParticleSystem> Pool { get; set; }

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        // When activated from the pool, start the coroutine that will return it.
        StartCoroutine(ReturnToPoolWhenFinished());
    }

    private IEnumerator ReturnToPoolWhenFinished()
    {
        // Wait until the particle system (and all children) has finished.
        yield return new WaitWhile(() => _particleSystem.IsAlive(true));

        // Once finished, return to the pool.
        Pool?.Release(this);
    }
}
