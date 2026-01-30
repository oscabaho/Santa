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
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [Header("Shared Assets - Global/Persistent Services")]
    [SerializeField]
    private InputReader inputReaderAsset;

    // TODO: Uncomment when the audio system is implemented
    // [SerializeField]
    // private AudioManager audioManagerInstance;

    // TODO: Uncomment when the VFX system is implemented
    // [SerializeField]
    // private VFXManager vfxManagerInstance;

    // NOTE: Services below have been moved to GameplayLifetimeScope
    // as they are only used during Gameplay, not in Menu.
    // If they are needed globally, uncomment and assign in Menu scene.
    // [SerializeField] private CombatTransitionManager combatTransitionManagerInstance;
    // [SerializeField] private CombatEncounterManager combatEncounterManagerInstance;
    // [SerializeField] private UpgradeManager upgradeManagerInstance;
    // [SerializeField] private GameStateManager gameStateManagerInstance;

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

    /// <summary>
    /// CRITICAL FOR MOBILE: Initialize the UI Event System with proper input module configuration.
    /// This must run before any UI components are initialized to ensure mobile touch input works.
    /// 
    /// Configures:
    /// - EventSystem (creates if missing)
    /// - InputSystemUIInputModule (replaces legacy StandaloneInputModule)
    /// - GraphicRaycaster on Canvas (if needed)
    /// - Camera.main for ScreenSpaceCamera Canvas
    /// </summary>
    private void InitializeUIEventSystem()
    {
        // Step 1: Ensure EventSystem exists
        var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            GameLog.LogError("GameLifetimeScope: EventSystem NOT FOUND! Creating one now.", this);
            var eventSystemGO = new GameObject("EventSystem");
            eventSystem = eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogVerbose("GameLifetimeScope: EventSystem verified.", this);
#endif

        // Step 2: Remove legacy StandaloneInputModule if present (Old Input System)
        var legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (legacyModule != null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning("GameLifetimeScope: Removing legacy StandaloneInputModule (Old Input System).", this);
#endif
            Destroy(legacyModule);
        }

        // Step 3: Ensure InputSystemUIInputModule exists (New Input System - CRITICAL for mobile)
        var uiInputModule = eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        if (uiInputModule == null)
        {
            uiInputModule = eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("GameLifetimeScope: Added InputSystemUIInputModule (New Input System) for mobile touch support.", this);
#else
            GameLog.Log("GameLifetimeScope: InputSystemUIInputModule configured for mobile input.");
#endif
        }

        // Step 4: Verify Canvas configuration
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            // Ensure Canvas has GraphicRaycaster
            var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster == null)
            {
                canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose("GameLifetimeScope: Added GraphicRaycaster to Canvas.", this);
#endif
            }

            // Validate Camera for ScreenSpaceCamera mode
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
            {
                var mainCam = UnityEngine.Camera.main;
                if (mainCam != null)
                {
                    canvas.worldCamera = mainCam;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.LogVerbose("GameLifetimeScope: Assigned Camera.main to Canvas.worldCamera.", this);
#endif
                }
                else
                {
                    GameLog.LogError("GameLifetimeScope: Canvas is ScreenSpaceCamera but Camera.main is NULL! Ensure main camera is tagged 'MainCamera'.", this);
                }
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log("GameLifetimeScope: UI Event System initialized successfully.", this);
#else
        GameLog.Log("GameLifetimeScope: UI Event System ready for mobile input.");
#endif
    }

    protected override void Configure(IContainerBuilder builder)
    {
        // CRITICAL: Initialize UI Event System first (before any UI components)
        // This ensures EventSystem and InputSystemUIInputModule are ready for mobile
        InitializeUIEventSystem();

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

        // NOTE: GameStateManager moved to GameplayScope

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