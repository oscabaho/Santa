using Santa.Core;
using Santa.Core.Player;
using Santa.Core.Pooling;
using Santa.Domain.Upgrades;
using Santa.Infrastructure.Camera;
using Santa.Infrastructure.Combat;
using Santa.Infrastructure.Input;
using Santa.Infrastructure.Level;
using Santa.Infrastructure.State;
using Santa.Presentation.HUD;
using Santa.Presentation.Menus;
using Santa.Presentation.UI;
using Santa.Presentation.Upgrades;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [Header("Shared Assets")]
    [SerializeField]
    private InputReader inputReaderAsset;
    [SerializeField]
    private UIManager uiManagerInstance;

    // TODO: Uncomment when the audio system is implemented
    // [SerializeField]
    // private AudioManager audioManagerInstance;

    [SerializeField]
    private CombatTransitionManager combatTransitionManagerInstance;

    [SerializeField]
    private CombatEncounterManager combatEncounterManagerInstance;

    [SerializeField]
    private UpgradeManager upgradeManagerInstance;

    [SerializeField]
    private GameStateManager gameStateManagerInstance;

    // TODO: Uncomment when the VFX system is implemented
    // [SerializeField]
    // private VFXManager vfxManagerInstance;

    private static GameLifetimeScope _instance;
    public static GameLifetimeScope Instance => _instance;

    protected override void Awake()
    {
        // Singleton check to prevent duplicates when reloading scenes
        if (_instance != null && _instance != this)
        {
            GameLog.LogWarning("GameLifetimeScope: Duplicate detected. Destroying new instance.");
            Destroy(gameObject);
            return; // IMPORTANT: Do not call base.Awake() to prevent VContainer from building this duplicate scope
        }

        _instance = this;

        // Ensure critical managers persist by reparenting them to this DDOL object
        // if they are currently separate root objects.
        // Managers moved to GameplayScope, so no need to reparent here.

        // First, run base Awake logic (LifetimeScope)
        base.Awake();

        // Then mark this GameObject as persistent across scene loads
        DontDestroyOnLoad(this.gameObject);
    }

    private void ReparentToPreserve(Component component)
    {
        if (component != null && component.transform.parent != transform)
        {
            component.transform.SetParent(transform);
        }
    }

    protected override void Configure(IContainerBuilder builder)
    {
        // Register shared InputReader if assigned (prevents desync between UI and gameplay)
        if (inputReaderAsset != null)
        {
            builder.RegisterInstance(inputReaderAsset).AsSelf();
        }
        else
        {
            GameLog.Log("GameLifetimeScope: InputReader asset not assigned. Ensure consumers reference the same asset.");
        }

        // TODO: Uncomment when the audio system is implemented
        // RegisterService<IAudioService, AudioManager>(builder, audioManagerInstance);

        // Registramos GameEventBus como Singleton
        builder.Register<GameEventBus>(Lifetime.Singleton).As<IEventBus>();

        // Register SecureStorage (non-MonoBehaviour service)
        builder.Register<Santa.Core.Save.SecureStorageService>(Lifetime.Singleton)
            .As<Santa.Core.Save.ISecureStorageService>()
            .AsSelf();

        // Register SaveContributorRegistry as singleton
        builder.Register<Santa.Core.Save.SaveContributorRegistry>(Lifetime.Singleton)
            .As<Santa.Core.Save.ISaveContributorRegistry>()
            .AsSelf();

        // GameStateManager moved to GameplayScope

        // TODO: Uncomment when the VFX system is implemented
        // RegisterService<IVFXService, VFXManager>(builder, vfxManagerInstance);

        // --- Dynamic UI via Addressables ---
        // UpgradeUI moved to GameplayScope.
        // See GameplayLifetimeScope.cs for registration.

        // PreloadUIPanelsEntryPoint moved to GameplayScope

        // PreloadUIPanelsEntryPoint moved to GameplayScope

        // --- Pooling Service ---
        // Register as component (creates one on this GameObject if not found in scene)
        var poolService = FindFirstObjectByType<Santa.Core.Pooling.PoolService>();
        if (poolService == null)
        {
            poolService = gameObject.AddComponent<Santa.Core.Pooling.PoolService>();
        }
        builder.RegisterComponent(poolService).As<IPoolService>().AsSelf();

        // --- Combat Log Service ---
        // --- Combat Log Service ---
        // Register as component (creates one on this GameObject if not found in scene)
        var combatLog = FindFirstObjectByType<Santa.Infrastructure.Combat.CombatLogService>();
        if (combatLog == null)
        {
            combatLog = gameObject.AddComponent<Santa.Infrastructure.Combat.CombatLogService>();
        }
        builder.RegisterComponent(combatLog).As<ICombatLogService>().AsSelf();

        // Register SaveService from hierarchy only if it exists (optional for test scenes)
        var saveService = FindFirstObjectByType<Santa.Core.Save.SaveService>(FindObjectsInactive.Include);
        if (saveService != null)
        {
            builder.RegisterComponent(saveService).As<Santa.Core.Save.ISaveService>().AsSelf();
        }
        else
        {
            GameLog.LogWarning("GameLifetimeScope: SaveService not found in scene. Save functionality disabled.");
        }

        // --- Hierarchy Components (Optional Registrations) ---
        // These components are optional and will be registered only if found in the scene
        TryRegisterOptionalComponent<GameInitializer>(builder);
        TryRegisterOptionalComponent<Santa.Presentation.HUD.VirtualPauseButton>(builder);

        // GraphicsSettings components - PC and Mobile
#if UNITY_STANDALONE || UNITY_EDITOR
        var graphicsSettingsManager = FindFirstObjectByType<Santa.Presentation.Menus.GraphicsSettingsManager>(FindObjectsInactive.Include);
        if (graphicsSettingsManager == null)
        {
            graphicsSettingsManager = gameObject.AddComponent<Santa.Presentation.Menus.GraphicsSettingsManager>();
        }
        builder.RegisterComponent(graphicsSettingsManager).As<IGraphicsSettingsService>().AsSelf();
        TryRegisterOptionalComponent<GraphicsSettingsController>(builder);
#else
        // Mobile platforms: register null graphics service
        builder.Register<IGraphicsSettingsService>(_ => new NullGraphicsSettingsService(), Lifetime.Singleton);
        TryRegisterOptionalComponent<GraphicsSettingsController>(builder);
#endif

        // Save-related optional components
        var decorState = FindFirstObjectByType<Santa.Core.Save.EnvironmentDecorState>(FindObjectsInactive.Include);
        if (decorState != null)
        {
            builder.RegisterComponent(decorState).AsSelf();
        }
        // If not found, LevelManager will use FindFirstObjectByType fallback

        GameLog.Log("GameLifetimeScope CONFIGURED!");
    }

    // Null object implementations to avoid DI chain failures
    private class NullCombatCameraManager : ICombatCameraManager
    {
        public void SwitchToMainCamera() { }
        public void SwitchToTargetSelectionCamera() { }
        public void SetCombatCameras(Unity.Cinemachine.CinemachineCamera main, Unity.Cinemachine.CinemachineCamera target) { }
        public void DeactivateCameras() { }
    }

    private class NullGraphicsSettingsService : IGraphicsSettingsService
    {
        public Resolution[] AvailableResolutions => System.Array.Empty<Resolution>();
        public int GetCurrentResolutionIndex() => 0;
        public void SetQuality(int qualityIndex) { }
        public void SetResolution(int resolutionIndex) { }
        public void SetFullscreen(bool isFullscreen) { }
        public void SetVSync(bool isEnabled) { }
    }

    /// <summary>
    /// Helper method to register a service with fallback to hierarchy search.
    /// Reduces code duplication for standard service registration pattern.
    /// </summary>
    private void RegisterService<TService, TImplementation>(IContainerBuilder builder, TImplementation component)
        where TService : class
        where TImplementation : Component, TService
    {
        if (component != null)
        {
            builder.RegisterComponent(component).As<TService>().AsSelf();
        }
        else
        {
            builder.RegisterComponentInHierarchy<TImplementation>().As<TService>().AsSelf();
            GameLog.Log($"GameLifetimeScope: {typeof(TImplementation).Name} not assigned. Registered from hierarchy.");
        }
    }

    /// <summary>
    /// Special case for UpgradeManager which implements multiple interfaces.
    /// </summary>
    private void RegisterServiceWithMultipleInterfaces(IContainerBuilder builder, UpgradeManager component)
    {
        if (component != null)
        {
            builder.RegisterComponent(component).As<IUpgradeService>().As<IUpgradeTarget>().AsSelf();
        }
        else
        {
            builder.RegisterComponentInHierarchy<UpgradeManager>().As<IUpgradeService>().As<IUpgradeTarget>().AsSelf();
            GameLog.Log("GameLifetimeScope: UpgradeManager not assigned. Registered from hierarchy.");
        }
    }

    /// <summary>
    /// Try to register an optional component from the hierarchy.
    /// If not found, logs a warning but does not fail.
    /// </summary>
    private void TryRegisterOptionalComponent<T>(IContainerBuilder builder) where T : Component
    {
        var component = FindFirstObjectByType<T>(FindObjectsInactive.Include);
        if (component != null)
        {
            builder.RegisterComponent(component).AsSelf();
        }
        else
        {
            // Optional components don't need a warning, just a debug log if needed
            // GameLog.Log($"GameLifetimeScope: {typeof(T).Name} not found in scene (optional).");
        }
    }
}