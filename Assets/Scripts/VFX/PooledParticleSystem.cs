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
        var main = _particleSystem.main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    private void OnParticleSystemStopped()
    {
        // Return to pool when the particle system stops
        Pool?.Release(this);
    }
}