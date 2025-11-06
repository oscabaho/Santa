using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
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
        // Registramos UIManager
        builder.RegisterComponent(uiManagerInstance).As<IUIManager>().AsSelf();

        // Registramos TurnBasedCombatManager
        builder.RegisterComponent(turnBasedCombatManagerInstance).As<ICombatService>().AsSelf();

        // TODO: Descomentar cuando el sistema de audio esté implementado
        // Registramos AudioManager
        // builder.RegisterComponent(audioManagerInstance).As<IAudioService>().AsSelf();

        // Registramos CombatTransitionManager
        builder.RegisterComponent(combatTransitionManagerInstance).As<ICombatTransitionService>().AsSelf();

        // Registramos UpgradeManager
        builder.RegisterComponent(upgradeManagerInstance).As<IUpgradeService>().As<IUpgradeTarget>().AsSelf();

        // Registramos GameEventBus como Singleton
        builder.Register<GameEventBus>(Lifetime.Singleton).As<IEventBus>();

        // Registramos GameStateManager
        builder.RegisterComponent(gameStateManagerInstance).As<IGameStateService>().AsSelf();

        // Registramos GameplayUIManager
        builder.RegisterComponent(gameplayUIManagerInstance).As<IGameplayUIService>().AsSelf();

        // Registramos LevelManager
        builder.RegisterComponent(levelManagerInstance).As<ILevelService>().AsSelf();

        // TODO: Descomentar cuando el sistema de VFX esté implementado
        // Registramos VFXManager
        // builder.RegisterComponent(vfxManagerInstance).As<IVFXService>().AsSelf();

        // --- UI Dinámica con Addressables ---
        // UpgradeUI se carga via Addressables, igual que las otras UIs del proyecto
        // La instancia la maneja UpgradeUILoader que se registra como IUpgradeUI
        builder.Register<UpgradeUILoader>(Lifetime.Singleton)
            .As<IUpgradeUI>()
            .AsSelf();

            // Registrar el lifecycle manager (OPCIONAL) para preload y release automático
            // Comenta esta línea si prefieres controlar manualmente el preload/release
            builder.RegisterEntryPoint<UpgradeUILifecycleManager>();
            // Preload frequently used panels like CombatUI at startup
            builder.RegisterEntryPoint<PreloadUIPanelsEntryPoint>();

        // --- Componentes en la jerarquía ---
        builder.RegisterComponentInHierarchy<GameInitializer>();
    builder.RegisterComponentInHierarchy<PlayerInteraction>().AsSelf();
        builder.RegisterComponentInHierarchy<CombatUI>();
        // NOTA: UpgradeUI ahora se instancia dinámicamente desde el prefab (ver arriba)
        builder.RegisterComponentInHierarchy<CombatCameraManager>().As<ICombatCameraManager>().AsSelf();
        builder.RegisterComponentInHierarchy<CombatScenePool>().AsSelf();
        builder.RegisterComponentInHierarchy<ScreenFade>().AsSelf();
        builder.RegisterComponentInHierarchy<GraphicsSettingsManager>().As<IGraphicsSettingsService>().AsSelf();
        builder.RegisterComponentInHierarchy<GraphicsSettingsController>().AsSelf();

        GameLog.Log("GameLifetimeScope CONFIGURED!");
    }
}