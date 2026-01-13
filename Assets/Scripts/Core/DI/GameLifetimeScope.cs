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

    [SerializeField]
    private TurnBasedCombatManager turnBasedCombatManagerInstance;

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

    [SerializeField]
    private GameplayUIManager gameplayUIManagerInstance;

    [SerializeField]
    private LevelManager levelManagerInstance;

    [SerializeField]
    private CombatCameraManager combatCameraManagerInstance;

    // TODO: Uncomment when the VFX system is implemented
    // [SerializeField]
    // private VFXManager vfxManagerInstance;

    protected override void Awake()
    {
        // First, run base Awake logic (LifetimeScope)
        base.Awake();

        // Then mark this GameObject as persistent across scene loads
        DontDestroyOnLoad(this.gameObject);
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

        // Register services using helper method to reduce duplication
        RegisterService<IUIManager, UIManager>(builder, uiManagerInstance);
        RegisterService<ICombatService, TurnBasedCombatManager>(builder, turnBasedCombatManagerInstance);

        // TODO: Uncomment when the audio system is implemented
        // RegisterService<IAudioService, AudioManager>(builder, audioManagerInstance);

        RegisterService<ICombatTransitionService, CombatTransitionManager>(builder, combatTransitionManagerInstance);
        RegisterService<ICombatEncounterManager, CombatEncounterManager>(builder, combatEncounterManagerInstance);
        RegisterServiceWithMultipleInterfaces(builder, upgradeManagerInstance);

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

        RegisterService<IGameStateService, GameStateManager>(builder, gameStateManagerInstance);
        RegisterService<IGameplayUIService, GameplayUIManager>(builder, gameplayUIManagerInstance);
        RegisterService<ILevelService, LevelManager>(builder, levelManagerInstance);
        // CombatCameraManager is critical for CombatTransitionManager; provide Null object if missing.
        if (combatCameraManagerInstance != null)
        {
            builder.RegisterComponent(combatCameraManagerInstance).As<ICombatCameraManager>().AsSelf();
        }
        else
        {
            var found = FindFirstObjectByType<CombatCameraManager>(FindObjectsInactive.Include);
            if (found != null)
            {
                builder.RegisterComponent(found).As<ICombatCameraManager>().AsSelf();
                GameLog.Log("GameLifetimeScope: CombatCameraManager found in hierarchy.");
            }
            else
            {
                builder.Register<NullCombatCameraManager>(Lifetime.Singleton).As<ICombatCameraManager>().AsSelf();
                GameLog.LogWarning("GameLifetimeScope: CombatCameraManager missing. Registered NullCombatCameraManager placeholder.");
            }
        }

        // TODO: Uncomment when the VFX system is implemented
        // RegisterService<IVFXService, VFXManager>(builder, vfxManagerInstance);

        // --- Dynamic UI via Addressables ---
        // UpgradeUI loads via Addressables like other UIs.
        // Its instance is managed by UpgradeUILoader registered as IUpgradeUI.
        builder.Register<UpgradeUILoader>(Lifetime.Singleton)
            .As<IUpgradeUI>()
            .WithParameter(typeof(ILevelService), resolver => resolver.Resolve<ILevelService>())
            .WithParameter(typeof(ICombatTransitionService), resolver => resolver.Resolve<ICombatTransitionService>())
            .AsSelf();

        // Register lifecycle manager (OPTIONAL) for automatic preload & release.
        // Comment out if you prefer manual preload/release control.
        builder.RegisterEntryPoint<UpgradeUILifecycleManager>();
        // Preload frequently used panels like CombatUI at startup
        builder.RegisterEntryPoint<PreloadUIPanelsEntryPoint>();

        // --- Pooling Service ---
        builder.Register<PoolService>(Lifetime.Singleton).As<IPoolService>();

        // --- Combat Log Service ---
        builder.Register<CombatLogService>(Lifetime.Singleton).As<ICombatLogService>();

        // --- Player Reference ---
        // Prefer component in hierarchy so designers can assign the player explicitly.
        var playerRef = FindFirstObjectByType<PlayerReference>(FindObjectsInactive.Include);
        if (playerRef != null)
        {
            builder.RegisterComponent(playerRef).As<IPlayerReference>().AsSelf();
        }
        else
        {
            // Create a scene GameObject with PlayerReference if missing
            var go = new GameObject("[Auto] PlayerReference");
            playerRef = go.AddComponent<PlayerReference>();
            DontDestroyOnLoad(go);
            builder.RegisterComponent(playerRef).As<IPlayerReference>().AsSelf();
            GameLog.Log("GameLifetimeScope: PlayerReference component not found. Created auto-discovery instance. Consider adding PlayerReference to the base scene for reliability.");
        }

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
        TryRegisterOptionalComponent<PlayerInteraction>(builder);

        // PauseMenuController as IPauseMenuService (required for exploration pause)
        var pauseMenuController = FindFirstObjectByType<Santa.UI.PauseMenuController>(FindObjectsInactive.Include);
        GameLog.Log($"GameLifetimeScope: Searching for PauseMenuController... Found: {pauseMenuController != null}");
        if (pauseMenuController != null)
        {
            GameLog.Log($"GameLifetimeScope: Registering PauseMenuController '{pauseMenuController.gameObject.name}' as IPauseMenuService");
            builder.RegisterComponent(pauseMenuController).As<Santa.Core.IPauseMenuService>().AsSelf();
        }
        else
        {
            GameLog.LogWarning("GameLifetimeScope: PauseMenuController not found. Pause functionality disabled.");
        }

        // Legacy binder removed; Pause is driven by IPauseMenuService + Addressables
        // TryRegisterOptionalComponent<Santa.UI.VirtualPauseMenuBinder>(builder);
        TryRegisterOptionalComponent<Santa.Presentation.HUD.VirtualPauseButton>(builder);

        // NOTE: CombatUI and UpgradeUI are instantiated dynamically via Addressables (see UIManager)
        // They should not be registered here nor in the base scene.

        TryRegisterOptionalComponent<CombatScenePool>(builder);

        // GraphicsSettings components - PC and Mobile
#if UNITY_STANDALONE
        var graphicsSettingsManager = FindFirstObjectByType<GraphicsSettingsManager>(FindObjectsInactive.Include);
        if (graphicsSettingsManager != null)
        {
            builder.RegisterComponent(graphicsSettingsManager).As<IGraphicsSettingsService>().AsSelf();
        }
        else
        {
            builder.Register<IGraphicsSettingsService>(_ => new NullGraphicsSettingsService(), Lifetime.Singleton);
        }
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