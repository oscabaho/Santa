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
        // Registramos UIManager (preferimos instancia asignada, si no, buscamos en jerarquía)
        if (uiManagerInstance != null)
        {
            builder.RegisterComponent(uiManagerInstance).As<IUIManager>().AsSelf();
        }
        else
        {
            builder.RegisterComponentInHierarchy<UIManager>().As<IUIManager>().AsSelf();
            GameLog.Log("GameLifetimeScope: UIManager instance not assigned in inspector. Registered UIManager found in hierarchy.");
        }

        // Registramos TurnBasedCombatManager (instancia o jerarquía)
        if (turnBasedCombatManagerInstance != null)
        {
            builder.RegisterComponent(turnBasedCombatManagerInstance).As<ICombatService>().AsSelf();
        }
        else
        {
            builder.RegisterComponentInHierarchy<TurnBasedCombatManager>().As<ICombatService>().AsSelf();
            GameLog.Log("GameLifetimeScope: TurnBasedCombatManager not assigned. Registered from hierarchy.");
        }

        // TODO: Descomentar cuando el sistema de audio esté implementado
    // Registramos AudioManager (instancia o jerarquía)
    // if (audioManagerInstance != null)
    // {
    //     builder.RegisterComponent(audioManagerInstance).As<IAudioService>().AsSelf();
    // }
    // else
    // {
    //     builder.RegisterComponentInHierarchy<AudioManager>().As<IAudioService>().AsSelf();
    //     GameLog.Log("GameLifetimeScope: AudioManager not assigned. Registered from hierarchy.");
    // }

        // Registramos CombatTransitionManager (instancia o jerarquía)
        if (combatTransitionManagerInstance != null)
        {
            builder.RegisterComponent(combatTransitionManagerInstance).As<ICombatTransitionService>().AsSelf();
        }
        else
        {
            builder.RegisterComponentInHierarchy<CombatTransitionManager>().As<ICombatTransitionService>().AsSelf();
            GameLog.Log("GameLifetimeScope: CombatTransitionManager not assigned. Registered from hierarchy.");
        }

        // Registramos UpgradeManager (instancia o jerarquía)
        if (upgradeManagerInstance != null)
        {
            builder.RegisterComponent(upgradeManagerInstance).As<IUpgradeService>().As<IUpgradeTarget>().AsSelf();
        }
        else
        {
            builder.RegisterComponentInHierarchy<UpgradeManager>().As<IUpgradeService>().As<IUpgradeTarget>().AsSelf();
            GameLog.Log("GameLifetimeScope: UpgradeManager not assigned. Registered from hierarchy.");
        }

        // Registramos GameEventBus como Singleton
        builder.Register<GameEventBus>(Lifetime.Singleton).As<IEventBus>();

        // Registramos GameStateManager (instancia o jerarquía)
        if (gameStateManagerInstance != null)
        {
            builder.RegisterComponent(gameStateManagerInstance).As<IGameStateService>().AsSelf();
        }
        else
        {
            builder.RegisterComponentInHierarchy<GameStateManager>().As<IGameStateService>().AsSelf();
            GameLog.Log("GameLifetimeScope: GameStateManager not assigned. Registered from hierarchy.");
        }

        // Registramos GameplayUIManager (instancia o jerarquía)
        if (gameplayUIManagerInstance != null)
        {
            builder.RegisterComponent(gameplayUIManagerInstance).As<IGameplayUIService>().AsSelf();
        }
        else
        {
            builder.RegisterComponentInHierarchy<GameplayUIManager>().As<IGameplayUIService>().AsSelf();
            GameLog.Log("GameLifetimeScope: GameplayUIManager not assigned. Registered from hierarchy.");
        }

        // Registramos LevelManager (instancia o jerarquía)
        if (levelManagerInstance != null)
        {
            builder.RegisterComponent(levelManagerInstance).As<ILevelService>().AsSelf();
        }
        else
        {
            builder.RegisterComponentInHierarchy<LevelManager>().As<ILevelService>().AsSelf();
            GameLog.Log("GameLifetimeScope: LevelManager not assigned. Registered from hierarchy.");
        }

        // TODO: Descomentar cuando el sistema de VFX esté implementado
    // Registramos VFXManager (instancia o jerarquía)
    // if (vfxManagerInstance != null)
    // {
    //     builder.RegisterComponent(vfxManagerInstance).As<IVFXService>().AsSelf();
    // }
    // else
    // {
    //     builder.RegisterComponentInHierarchy<VFXManager>().As<IVFXService>().AsSelf();
    //     GameLog.Log("GameLifetimeScope: VFXManager not assigned. Registered from hierarchy.");
    // }

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
        
        builder.RegisterComponentInHierarchy<CombatCameraManager>().As<ICombatCameraManager>().AsSelf();
        builder.RegisterComponentInHierarchy<CombatScenePool>().AsSelf();
        builder.RegisterComponentInHierarchy<ScreenFade>().AsSelf();
        builder.RegisterComponentInHierarchy<GraphicsSettingsManager>().As<IGraphicsSettingsService>().AsSelf();
        builder.RegisterComponentInHierarchy<GraphicsSettingsController>().AsSelf();

        GameLog.Log("GameLifetimeScope CONFIGURED!");
    }
}