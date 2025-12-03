using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Santa.Core;
using Santa.Core.Addressables;
using Santa.Domain.Combat;
using AbilityUpgrade = Santa.Domain.Combat.AbilityUpgrade;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;
using VContainer.Unity;

namespace Santa.Presentation.Upgrades
{

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
    /// Matches IUpgradeUI signature (void) and runs async internally.
    /// </summary>
    public void ShowUpgrades(AbilityUpgrade upgrade1, AbilityUpgrade upgrade2)
    {
        ShowUpgradesAsync(upgrade1, upgrade2).Forget();
    }

    private async UniTaskVoid ShowUpgradesAsync(AbilityUpgrade upgrade1, AbilityUpgrade upgrade2)
    {
        try
        {
            _showCancellation?.Cancel();
            _showCancellation?.Dispose();
            _showCancellation = new CancellationTokenSource();
            var ct = _showCancellation.Token;

            if (_isLoaded && _upgradeUIInstance != null)
            {
                _upgradeUIInstance.ShowUpgrades(upgrade1, upgrade2);
                return;
            }

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
        catch (System.Exception ex)
        {
            GameLog.LogError($"UpgradeUILoader.ShowUpgrades: Exception while showing upgrades: {ex.Message}");
            GameLog.LogException(ex);
        }
    }

    /// <summary>
    /// Preloads the upgrade UI in the background without showing it.
    /// Useful to call at the start of a combat level to avoid later delay.
    /// </summary>
    public async UniTask PreloadAsync()
    {
        // Do not overwrite current show cancellation; separate preload lifecycle
        if (_isLoaded)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("UpgradeUILoader: UI already loaded, no need to preload.");
#endif
            return;
        }

        if (_isLoading)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("UpgradeUILoader: Already loading, waiting...");
#endif
            await WaitForLoad();
            return;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log("UpgradeUILoader: Preloading UpgradeUI in background...");
#endif
        await LoadUpgradeUI();

        if (_isLoaded)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("UpgradeUILoader: Preload completed successfully.");
#endif
        }
    }

    private UniTask _loadingTask;

    /// <summary>
    /// Loads the UpgradeUI prefab via Addressables.
    /// </summary>
    private async UniTask LoadUpgradeUI()
    {
        if (_isLoaded) return;

        // If already loading, return the existing task
        if (_isLoading && _loadingTask.Status == UniTaskStatus.Pending)
        {
            await _loadingTask;
            return;
        }

        _isLoading = true;
        _loadingTask = LoadInternal();
        await _loadingTask;
    }

    private async UniTask LoadInternal()
    {
        try
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"UpgradeUILoader: Loading UpgradeUI from Addressables ('{UPGRADE_UI_ADDRESS}')...");
#endif

            // Load and instantiate via Addressables
            _loadHandle = Addressables.InstantiateAsync(UPGRADE_UI_ADDRESS);
            await _loadHandle.ToUniTask();

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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.Log("UpgradeUILoader: UpgradeUI loaded successfully via Addressables.");
#endif
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
            _loadingTask = UniTask.CompletedTask;
        }
    }

    /// <summary>
    /// Waits for the in-progress load to finish.
    /// </summary>
    private async UniTask WaitForLoad()
    {
        if (_isLoading && _loadingTask.Status == UniTaskStatus.Pending)
        {
            await _loadingTask;
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("UpgradeUILoader: UpgradeUI resources released.");
#endif
        }
    }
}
}
