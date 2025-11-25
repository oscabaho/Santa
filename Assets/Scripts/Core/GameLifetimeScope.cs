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

    // TODO: Descomentar cuando el sistema de audio esté implementado
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

    // TODO: Descomentar cuando el sistema de VFX esté implementado
    // [SerializeField]
    // private VFXManager vfxManagerInstance;

    protected override void Awake()
    {
        // Primero, ejecuta la lógica de Awake de la clase base (LifetimeScope)
        base.Awake();
        
        // Luego, le decimos a Unity que este GameObject no debe ser destruido al cargar nuevas escenas
        DontDestroyOnLoad(this.gameObject);
    }

    protected override void Configure(IContainerBuilder builder)
    {
        // Registrar InputReader compartido si está asignado (evita desincronización entre UI y gameplay)
        if (inputReaderAsset != null)
        {
            builder.RegisterInstance(inputReaderAsset).AsSelf();
        }
        else
        {
            GameLog.Log("GameLifetimeScope: InputReader asset not assigned. Ensure consumers reference the same asset.");
        }

        // Registrar servicios usando helper method para reducir duplicación
        RegisterService<IUIManager, UIManager>(builder, uiManagerInstance);
        RegisterService<ICombatService, TurnBasedCombatManager>(builder, turnBasedCombatManagerInstance);

        // TODO: Descomentar cuando el sistema de audio esté implementado
        // RegisterService<IAudioService, AudioManager>(builder, audioManagerInstance);

        RegisterService<ICombatTransitionService, CombatTransitionManager>(builder, combatTransitionManagerInstance);
        RegisterServiceWithMultipleInterfaces(builder, upgradeManagerInstance);
        
        // Registramos GameEventBus como Singleton
        builder.Register<GameEventBus>(Lifetime.Singleton).As<IEventBus>();

        RegisterService<IGameStateService, GameStateManager>(builder, gameStateManagerInstance);
        RegisterService<IGameplayUIService, GameplayUIManager>(builder, gameplayUIManagerInstance);
        RegisterService<ILevelService, LevelManager>(builder, levelManagerInstance);
        // CombatCameraManager es crítico para CombatTransitionManager; proporcionar Null object si falta.
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

        // TODO: Descomentar cuando el sistema de VFX esté implementado
        // RegisterService<IVFXService, VFXManager>(builder, vfxManagerInstance);

        // --- UI Dinámica con Addressables ---
        // UpgradeUI se carga via Addressables, igual que las otras UIs del proyecto
        // La instancia la maneja UpgradeUILoader que se registra como IUpgradeUI
        builder.Register<UpgradeUILoader>(Lifetime.Singleton)
            .As<IUpgradeUI>()
            .WithParameter(typeof(ILevelService), resolver => resolver.Resolve<ILevelService>())
            .WithParameter(typeof(ICombatTransitionService), resolver => resolver.Resolve<ICombatTransitionService>())
            .AsSelf();

            // Registrar el lifecycle manager (OPCIONAL) para preload y release automático
            // Comenta esta línea si prefieres controlar manualmente el preload/release
            builder.RegisterEntryPoint<UpgradeUILifecycleManager>();
            // Preload frequently used panels like CombatUI at startup
            builder.RegisterEntryPoint<PreloadUIPanelsEntryPoint>();

        // --- Componentes en la jerarquía ---
        builder.RegisterComponentInHierarchy<GameInitializer>();
        builder.RegisterComponentInHierarchy<PlayerInteraction>().AsSelf();
        
        // NOTA: CombatUI y UpgradeUI se instancian dinámicamente via Addressables (ver UIManager)
        // No deben estar registrados aquí ni en la escena base
        
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