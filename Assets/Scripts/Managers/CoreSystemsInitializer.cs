using UnityEngine;

/// <summary>
/// Deprecated bootstrapper now kept only to warn if a scene still references it.
/// </summary>
[DisallowMultipleComponent]
public class CoreSystemsInitializer : MonoBehaviour
{
    private void Awake()
    {
        GameLog.LogWarning("CoreSystemsInitializer is deprecated. Remove this component from scenes; GameLifetimeScope now bootstraps services.", this);
        Destroy(this);
    }
}
