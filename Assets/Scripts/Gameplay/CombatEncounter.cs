using UnityEngine;
using System.Threading.Tasks;
using VContainer;

/// <summary>
/// A data container for a specific combat encounter.
/// This component holds references to the scene and camera for a combat encounter.
/// </summary>
public class CombatEncounter : MonoBehaviour, ICombatEncounter
{
    [Header("Scene Setup")]
    [Tooltip("The parent GameObject that contains the entire pre-staged combat scene. This should have a CombatArena component.")]
    [SerializeField] private GameObject combatSceneParent;
    [Tooltip("Optional prefab to instantiate for this combat encounter. If assigned, it will be instantiated at runtime instead of using a pre-placed object.")]
    [SerializeField] private GameObject combatScenePrefab;

    [Tooltip("Optional Resources path to load the combat scene prefab (e.g. 'Combat/CombatScene_01'). If assigned and prefab is null, the prefab will be loaded from Resources.")]
    [SerializeField] private string combatSceneResourcePath;

    [Tooltip("The virtual camera that will be used for this combat encounter.")]
    [SerializeField] private GameObject combatCamera;

    public GameObject CombatSceneParent => combatSceneParent;
    public GameObject CombatScenePrefab => combatScenePrefab;
    public string CombatSceneResourcePath => combatSceneResourcePath;
    [Tooltip("Addressable address for this combat scene prefab. If assigned, the pool will load/instantiate via Addressables.")]
    [SerializeField] private string combatSceneAddress;
    public string CombatSceneAddress => combatSceneAddress;
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
    /// Create an instance of the combat scene for this encounter. Prefab -> Resources -> pre-placed fallback.
    /// The returned instance will be inactive when possible so transition sequences can control activation/animation.
    /// </summary>
    public string GetPoolKey()
    {
        if (!string.IsNullOrEmpty(combatSceneAddress)) return combatSceneAddress;
        if (combatScenePrefab != null) return combatScenePrefab.name;
        if (!string.IsNullOrEmpty(combatSceneResourcePath)) return combatSceneResourcePath;
        if (combatSceneParent != null) return combatSceneParent.name;
        return null;
    }

    // NOTE: Instantiation now handled via CombatScenePool to support Addressables + pooling.
    public Task<GameObject> InstantiateCombatSceneFallbackAsync()
    {
        GameObject instance = null;

        if (combatScenePrefab != null)
        {
            instance = Instantiate(combatScenePrefab);
            instance.SetActive(false);
            return Task.FromResult(instance);
        }

        if (!string.IsNullOrEmpty(combatSceneResourcePath))
        {
            var loaded = Resources.Load<GameObject>(combatSceneResourcePath);
            if (loaded != null)
            {
                instance = Instantiate(loaded);
                instance.SetActive(false);
                return Task.FromResult(instance);
            }
            else
            {
                GameLog.LogError($"CombatEncounter: Resources.Load failed for path '{combatSceneResourcePath}'");
            }
        }

        // Fallback to using a pre-placed scene parent if set.
        if (combatSceneParent != null)
        {
            return Task.FromResult(combatSceneParent);
        }

        return Task.FromResult<GameObject>(null);
    }
}