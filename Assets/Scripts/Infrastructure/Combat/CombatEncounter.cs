using UnityEngine;
using VContainer;

/// <summary>
/// A data container for a specific combat encounter.
/// REQUIRED: Configure combatSceneAddress with a valid Addressable address.
/// Combat arenas MUST be loaded via Addressables for optimal performance and memory management.
/// </summary>
public class CombatEncounter : MonoBehaviour, ICombatEncounter
{
    [Header("Scene Setup - Addressables")]
    [Tooltip("Addressable address for this combat scene prefab. The pool will load/instantiate via Addressables. Example: 'CombatArena_Forest', 'CombatArena_Desert'")]
    [SerializeField] private string combatSceneAddress;
    public string CombatSceneAddress => combatSceneAddress;

    [Header("Combat Camera")]
    [Tooltip("The virtual camera that will be used for this combat encounter.")]
    [SerializeField] private GameObject combatCamera;
    public GameObject CombatCamera => combatCamera;

    [Header("Pooling")]
    [Tooltip("If enabled, this encounter will request the pool to prewarm instances on Start.")]
    [SerializeField] private bool autoPrewarm = false;
    [Tooltip("Number of instances to prewarm into the pool for this encounter.")]
    [SerializeField] private int prewarmCount = 1;
    [Tooltip("If true, Addressables-created instances for this encounter will be Released (Addressables.ReleaseInstance) when returned instead of pooled.")]
    [SerializeField] private bool releaseAddressablesInstances = false;

    public bool ReleaseAddressablesInstances => releaseAddressablesInstances;

    private CombatScenePool _combatScenePool;

    [Inject]
    public void Construct(CombatScenePool combatScenePool)
    {
        _combatScenePool = combatScenePool;
    }

    private void Start()
    {
        if (autoPrewarm && prewarmCount > 0)
        {
            var key = GetPoolKey();
            if (!string.IsNullOrEmpty(key))
            {
                // Start prewarming in background
                _ = _combatScenePool.PrewarmAsync(key, prewarmCount, this);
            }
        }
    }

    /// <summary>
    /// Gets the pool key for this combat encounter.
    /// Returns the combatSceneAddress which must be set to a valid Addressable address.
    /// </summary>
    public string GetPoolKey()
    {
        if (string.IsNullOrEmpty(combatSceneAddress))
        {
            GameLog.LogError($"CombatEncounter '{gameObject.name}': combatSceneAddress is not configured. Combat arenas MUST be loaded via Addressables.");
        }
        return combatSceneAddress;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!string.IsNullOrEmpty(combatSceneAddress))
        {
            // Simple validation to check if the key looks like a valid arena key
            // In a real scenario, we might check against the Addressables catalog, but that's async/complex in OnValidate.
            // Here we just check if it matches one of our known constants for safety.

            var fields = typeof(Santa.Core.Addressables.AddressableKeys.CombatArenas)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
            
            bool isKnown = false;
            for (int i = 0; i < fields.Length; i++)
            {
                var f = fields[i];
                if (f.IsLiteral && !f.IsInitOnly && (string)f.GetValue(null) == combatSceneAddress)
                {
                    isKnown = true;
                    break;
                }
            }

            if (!isKnown && !combatSceneAddress.StartsWith("CombatArena_"))
            {
                Debug.LogWarning($"CombatEncounter '{gameObject.name}': '{combatSceneAddress}' is not a known constant in AddressableKeys.CombatArenas and doesn't follow the 'CombatArena_' naming convention.");
            }
        }
    }
#endif
}