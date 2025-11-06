using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using VContainer;

/// <summary>
/// Loader que gestiona la carga y acceso de la UpgradeUI via Addressables.
/// Implementa IUpgradeUI para ser inyectado en UpgradeManager.
/// Sigue el patrón establecido por UIManager en el proyecto.
/// </summary>
public class UpgradeUILoader : IUpgradeUI
{
    private const string UPGRADE_UI_ADDRESS = "UpgradeUI"; // Nombre del addressable

    private UpgradeUI _upgradeUIInstance;
    private AsyncOperationHandle<GameObject> _loadHandle;
    private bool _isLoading;
    private bool _isLoaded;

    private IUpgradeService _upgradeService;
    private ILevelService _levelService;
    private ICombatTransitionService _combatTransitionService;

    [Inject]
    public void Construct(
        IUpgradeService upgradeService, 
        ILevelService levelService, 
        ICombatTransitionService combatTransitionService)
    {
        _upgradeService = upgradeService;
        _levelService = levelService;
        _combatTransitionService = combatTransitionService;
    }

    /// <summary>
    /// Muestra la UI de upgrades. Carga el prefab via Addressables si es necesario.
    /// </summary>
    public async void ShowUpgrades(AbilityUpgrade upgrade1, AbilityUpgrade upgrade2)
    {
        // Si ya está cargada, solo mostrarla
        if (_isLoaded && _upgradeUIInstance != null)
        {
            _upgradeUIInstance.ShowUpgrades(upgrade1, upgrade2);
            return;
        }

        // Si está en proceso de carga, esperar
        if (_isLoading)
        {
            await WaitForLoad();
            if (_upgradeUIInstance != null)
            {
                _upgradeUIInstance.ShowUpgrades(upgrade1, upgrade2);
            }
            return;
        }

        // Cargar por primera vez
        await LoadUpgradeUI();
        
        if (_upgradeUIInstance != null)
        {
            _upgradeUIInstance.ShowUpgrades(upgrade1, upgrade2);
        }
        else
        {
            GameLog.LogError("UpgradeUILoader: Failed to load UpgradeUI. Cannot show upgrades.");
        }
    }

        /// <summary>
        /// Precarga la UI de upgrades en background sin mostrarla.
        /// Útil para llamar al inicio de un nivel de combate para evitar delay posterior.
        /// </summary>
        public async Task PreloadAsync()
        {
            if (_isLoaded)
            {
                GameLog.Log("UpgradeUILoader: UI already loaded, no need to preload.");
                return;
            }

            if (_isLoading)
            {
                GameLog.Log("UpgradeUILoader: Already loading, waiting...");
                await WaitForLoad();
                return;
            }

            GameLog.Log("UpgradeUILoader: Preloading UpgradeUI in background...");
            await LoadUpgradeUI();
        
            if (_isLoaded)
            {
                GameLog.Log("UpgradeUILoader: Preload completed successfully.");
            }
        }

    /// <summary>
    /// Carga el prefab de UpgradeUI via Addressables.
    /// </summary>
    private async Task LoadUpgradeUI()
    {
        if (_isLoading || _isLoaded)
            return;

        _isLoading = true;

        try
        {
            GameLog.Log($"UpgradeUILoader: Loading UpgradeUI from Addressables ('{UPGRADE_UI_ADDRESS}')...");

            // Cargar e instanciar via Addressables
            _loadHandle = Addressables.InstantiateAsync(UPGRADE_UI_ADDRESS);
            await _loadHandle.Task;

            if (_loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject instantiatedObject = _loadHandle.Result;
                _upgradeUIInstance = instantiatedObject.GetComponent<UpgradeUI>();

                if (_upgradeUIInstance != null)
                {
                    // Inyectar dependencias manualmente
                    _upgradeUIInstance.Construct(_upgradeService, _levelService, _combatTransitionService);

                    // Marcar como persistente entre escenas
                    Object.DontDestroyOnLoad(instantiatedObject);

                    _isLoaded = true;
                    GameLog.Log("UpgradeUILoader: UpgradeUI loaded successfully via Addressables.");
                }
                else
                {
                    GameLog.LogError($"UpgradeUILoader: Prefab '{UPGRADE_UI_ADDRESS}' does not have UpgradeUI component.");
                    Addressables.ReleaseInstance(instantiatedObject);
                }
            }
            else
            {
                GameLog.LogError($"UpgradeUILoader: Failed to load '{UPGRADE_UI_ADDRESS}' from Addressables. " +
                                $"Status: {_loadHandle.Status}. Make sure the prefab is marked as Addressable.");
            }
        }
        catch (System.Exception ex)
        {
            GameLog.LogError($"UpgradeUILoader: Exception while loading UpgradeUI: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    /// <summary>
    /// Espera a que termine la carga en progreso.
    /// </summary>
    private async Task WaitForLoad()
    {
        while (_isLoading)
        {
            await Task.Yield();
        }
    }

    /// <summary>
    /// Limpia los recursos de Addressables cuando ya no se necesiten.
    /// </summary>
    public void Release()
    {
        if (_isLoaded && _loadHandle.IsValid())
        {
            Addressables.ReleaseInstance(_loadHandle.Result);
            _upgradeUIInstance = null;
            _isLoaded = false;
            GameLog.Log("UpgradeUILoader: UpgradeUI resources released.");
        }
    }
}
