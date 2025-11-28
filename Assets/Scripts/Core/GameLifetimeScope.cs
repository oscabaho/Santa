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
        RegisterServiceWithMultipleInterfaces(builder, upgradeManagerInstance);
        
        // Registramos GameEventBus como Singleton
        builder.Register<GameEventBus>(Lifetime.Singleton).As<IEventBus>();

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

        // --- Save System ---
        // Register SaveService from hierarchy or create a new GameObject-bound instance.
        builder.RegisterComponentInHierarchy<Santa.Core.Save.SaveService>().As<Santa.Core.Save.ISaveService>().AsSelf();

        // --- Hierarchy Components ---
        builder.RegisterComponentInHierarchy<GameInitializer>();
        builder.RegisterComponentInHierarchy<PlayerInteraction>().AsSelf();
        builder.RegisterComponentInHierarchy<Santa.UI.PauseMenuController>().AsSelf();
        builder.RegisterComponentInHierarchy<Santa.UI.VirtualPauseMenuBinder>().AsSelf();
        builder.RegisterComponentInHierarchy<Santa.UI.VirtualPauseButton>().AsSelf();
        
        // NOTE: CombatUI and UpgradeUI are instantiated dynamically via Addressables (see UIManager)
        // They should not be registered here nor in the base scene.
        
        builder.RegisterComponentInHierarchy<CombatScenePool>().AsSelf();
        builder.RegisterComponentInHierarchy<GraphicsSettingsManager>().As<IGraphicsSettingsService>().AsSelf();
        builder.RegisterComponentInHierarchy<GraphicsSettingsController>().AsSelf();

        GameLog.Log("GameLifetimeScope CONFIGURED!");
    }

    // Null object implementation to avoid DI chain failures
    private class NullCombatCameraManager : ICombatCameraManager
    {
        public void SwitchToMainCamera() { }
        public void SwitchToTargetSelectionCamera() { }
        public void SetCombatCameras(Unity.Cinemachine.CinemachineCamera main, Unity.Cinemachine.CinemachineCamera target) { }
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
}