using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;
using VContainer.Unity;

/// <summary>
/// Loader that manages loading and access to the UpgradeUI via Addressables.
/// Implements IUpgradeUI to be injected into UpgradeManager.
/// Follows the pattern established by UIManager in the project.
/// </summary>
public class UpgradeUILoader : IUpgradeUI
{
    private const string UPGRADE_UI_ADDRESS = Santa.Core.Addressables.AddressableKeys.UIPanels.UpgradeUI; // Addressable name

    private UpgradeUI _upgradeUIInstance;
    private AsyncOperationHandle<GameObject> _loadHandle;
    private bool _isLoading;
    private bool _isLoaded;
    private CancellationTokenSource _showCancellation;

    private ILevelService _levelService;
    private ICombatTransitionService _combatTransitionService;
    private IObjectResolver _resolver;

    [Inject]
    public void Construct(
        ILevelService levelService,
        ICombatTransitionService combatTransitionService,
        IObjectResolver resolver)
    {
        _levelService = levelService;
        _combatTransitionService = combatTransitionService;
        _resolver = resolver;
    }

    /// <summary>
    /// Shows the upgrade UI. Loads the prefab via Addressables if needed.
    /// </summary>
    public async void ShowUpgrades(AbilityUpgrade upgrade1, AbilityUpgrade upgrade2)
    {
        _showCancellation?.Cancel();
        _showCancellation?.Dispose();
        _showCancellation = new CancellationTokenSource();
        var ct = _showCancellation.Token;

        // If already loaded, just show it
        if (_isLoaded && _upgradeUIInstance != null)
        {
            _upgradeUIInstance.ShowUpgrades(upgrade1, upgrade2);
            return;
        }

        // If currently loading, wait
        if (_isLoading)
        {
            await WaitForLoad();
            if (ct.IsCancellationRequested) return;
            if (_upgradeUIInstance != null)
            {
                _upgradeUIInstance.ShowUpgrades(upgrade1, upgrade2);
            }
            return;
        }

        // First-time load
        await LoadUpgradeUI();
        if (ct.IsCancellationRequested) return;

        if (_upgradeUIInstance != null)
        {
            _upgradeUIInstance.ShowUpgrades(upgrade1, upgrade2);
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("UpgradeUILoader: Failed to load UpgradeUI. Cannot show upgrades.");
#endif
        }
    }

    /// <summary>
    /// Preloads the upgrade UI in the background without showing it.
    /// Useful to call at the start of a combat level to avoid later delay.
    /// </summary>
    public async Task PreloadAsync()
    {
        // Do not overwrite current show cancellation; separate preload lifecycle
        if (_isLoaded)
        {
            return;
        }

        if (_isLoading)
        {
            await WaitForLoad();
            return;
        }

        await LoadUpgradeUI();

        if (_isLoaded)
        {
        }
    }

    /// <summary>
    /// Loads the UpgradeUI prefab via Addressables.
    /// </summary>
    private async Task LoadUpgradeUI()
    {
        if (_isLoading || _isLoaded)
            return;

        _isLoading = true;

        try
        {
            // Load and instantiate via Addressables
            _loadHandle = Addressables.InstantiateAsync(UPGRADE_UI_ADDRESS);
            await _loadHandle.Task;

            if (_loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject instantiatedObject = _loadHandle.Result;
                _upgradeUIInstance = instantiatedObject.GetComponent<UpgradeUI>();

                if (_upgradeUIInstance != null)
                {
                    if (_resolver != null)
                    {
                        _resolver.InjectGameObject(instantiatedObject);
                    }
                    else
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        GameLog.LogWarning("UpgradeUILoader: IObjectResolver not available, dependencies will not be injected into UpgradeUI instance.");
#endif
                    }

                    Object.DontDestroyOnLoad(instantiatedObject);

                    _isLoaded = true;
                }
                else
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.LogError($"UpgradeUILoader: Prefab '{UPGRADE_UI_ADDRESS}' does not have UpgradeUI component.");
#endif
                    Addressables.ReleaseInstance(instantiatedObject);
                }
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError($"UpgradeUILoader: Failed to load '{UPGRADE_UI_ADDRESS}' from Addressables. " +
                                $"Status: {_loadHandle.Status}. Make sure the prefab is marked as Addressable.");
#endif
            }
        }
        catch (System.Exception ex)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError($"UpgradeUILoader: Exception while loading UpgradeUI: {ex.Message}");
#endif
        }
        finally
        {
            _isLoading = false;
        }
    }

    /// <summary>
    /// Waits for the in-progress load to finish.
    /// </summary>
    private async Task WaitForLoad()
    {
        while (_isLoading)
        {
            await Task.Yield();
        }
    }

    /// <summary>
    /// Releases Addressables resources when no longer needed.
    /// </summary>
    public void Release()
    {
        _showCancellation?.Cancel();
        _showCancellation?.Dispose();
        _showCancellation = null;
        if (_isLoaded && _loadHandle.IsValid())
        {
            Addressables.ReleaseInstance(_loadHandle.Result);
            _upgradeUIInstance = null;
            _isLoaded = false;
        }
    }
}
