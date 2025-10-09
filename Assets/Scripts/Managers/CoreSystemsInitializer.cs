using UnityEngine;

/// <summary>
/// Initializes core game systems. This component should be placed on a persistent GameObject
/// in the first scene of your game.
/// </summary>
public class CoreSystemsInitializer : MonoBehaviour
{
    [Header("System Prefabs")]
    [SerializeField] private GameObject _graphicsSettingsManagerPrefab;

    private static bool _isInitialized = false;

    private void Awake()
    {
        // Ensure this initializer runs only once.
        if (_isInitialized)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        InitializeSystems();
        _isInitialized = true;
    }

    private void InitializeSystems()
    {
        // Instantiate managers from the prefabs assigned in the Inspector.
        InstantiateManager(_graphicsSettingsManagerPrefab);
    }

    /// <summary>
    /// Instantiates a manager prefab if an instance of its component doesn't already exist.
    /// </summary>
    /// <param name="prefab">The manager prefab to instantiate.</param>
    private void InstantiateManager(GameObject prefab)
    {
        if (prefab == null)
        {
            #if UNITY_EDITOR
            GameLog.LogWarning($"CoreSystemsInitializer: A manager prefab has not been assigned in the Inspector.");
            #endif
            return;
        }

        // We need a component type to check for existence, so we assume the prefab's name
        // matches the main component's name. This is a common convention.
        var componentType = System.Type.GetType(prefab.name);
        if (componentType == null || !(typeof(Component).IsAssignableFrom(componentType)))
        {
            #if UNITY_EDITOR
            GameLog.LogError($"CoreSystemsInitializer: Could not find a component type named '{prefab.name}' on the prefab. Ensure the prefab name matches its main component script name.");
            #endif
            return;
        }

        if (Object.FindFirstObjectByType(componentType) == null)
        {
            Instantiate(prefab);
            // The Awake() method of the manager itself should handle DontDestroyOnLoad and service registration.
        }
    }
}
