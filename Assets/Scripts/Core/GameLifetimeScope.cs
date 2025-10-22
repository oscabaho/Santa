using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField]
    private UIManager uiManagerInstance;

    [SerializeField]
    private TurnBasedCombatManager turnBasedCombatManagerInstance;

    [SerializeField]
    private AudioManager audioManagerInstance;

    [SerializeField]
    private CombatTransitionManager combatTransitionManagerInstance;

    [SerializeField]
    private UpgradeManager upgradeManagerInstance;

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

        // Registramos AudioManager
        builder.RegisterComponent(audioManagerInstance).As<IAudioService>().AsSelf();

        // Registramos CombatTransitionManager
        builder.RegisterComponent(combatTransitionManagerInstance).As<ICombatTransitionService>().AsSelf();

        // Registramos UpgradeManager
        builder.RegisterComponent(upgradeManagerInstance).As<IUpgradeService>().As<IUpgradeTarget>().AsSelf();

        // --- Componentes en la jerarquía ---
        builder.RegisterComponentInHierarchy<GameInitializer>();
        builder.RegisterComponentInHierarchy<PlayerInteraction>().AsSelf();
        builder.RegisterComponentInHierarchy<CombatTestInitiator>();
        builder.RegisterComponentInHierarchy<CombatUI>();
        builder.RegisterComponentInHierarchy<UpgradeUI>().As<IUpgradeUI>();
        builder.RegisterComponentInHierarchy<CombatCameraManager>().As<ICombatCameraManager>().AsSelf();
        builder.RegisterComponentInHierarchy<CombatScenePool>().AsSelf();
        builder.RegisterComponentInHierarchy<ScreenFade>().AsSelf();
        builder.RegisterComponentInHierarchy<GraphicsSettingsManager>().As<IGraphicsSettingsService>().AsSelf();
        builder.RegisterComponentInHierarchy<GraphicsSettingsController>().AsSelf();

        GameLog.Log("GameLifetimeScope CONFIGURED!");
    }
}