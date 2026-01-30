using Santa.Core;
using Santa.Core.Player;
using Santa.Infrastructure.Camera;
using Santa.Infrastructure.Combat;
using Santa.Infrastructure.Input;
using Santa.Infrastructure.Level;
using Santa.Presentation.UI;
using Santa.Presentation.Upgrades;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Santa.Core.DI
{
    /// <summary>
    /// LifetimeScope specific to the Gameplay scene.
    /// Manages dependencies that only exist during gameplay (Combat, Level, Player, Camera).
    /// 
    /// Also ensures critical UI systems are initialized if Gameplay is loaded independently.
    /// 
    /// NOTE: UIManager is NOW registered here (Gameplay) since the UI is specific to Gameplay.
    /// Menu scene has its own independent UI.
    /// 
    /// Inherits from the parent GameLifetimeScope automatically if set up correctly in the Inspector.
    /// </summary>
    public class GameplayLifetimeScope : LifetimeScope
    {
        [Header("Gameplay Combat")]
        [SerializeField]
        private TurnBasedCombatManager turnBasedCombatManagerInstance;

        [Header("Gameplay UI Management")]
        [SerializeField]
        private UIManager uiManagerInstance;

        [SerializeField]
        private LevelManager levelManagerInstance;

        [SerializeField]
        private CombatCameraManager combatCameraManagerInstance;

        [SerializeField]
        private GameplayUIManager gameplayUIManagerInstance;

        // PlayerReference can be assigned or found in scene
        [SerializeField]
        private PlayerReference playerReferenceInstance;

        [SerializeField]
        private CombatScenePool combatScenePoolInstance;

        [SerializeField]
        private Santa.UI.PauseMenuController pauseMenuControllerInstance;

        protected override void Awake()
        {
            // If Gameplay is loaded independently (not from Menu), ensure UI Event System is initialized
            EnsureUIEventSystemInitialized();
            
            base.Awake();
        }

        /// <summary>
        /// Ensures EventSystem is configured even if Gameplay loads independently.
        /// This replicates the initialization from GameLifetimeScope if needed.
        /// </summary>
        private void EnsureUIEventSystemInitialized()
        {
            var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("GameplayLifetimeScope: EventSystem not found. Creating one for independent Gameplay load.", this);
#endif
                var eventSystemGO = new GameObject("EventSystem");
                eventSystem = eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            }

            // Ensure InputSystemUIInputModule
            var uiInputModule = eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            if (uiInputModule == null)
            {
                uiInputModule = eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose("GameplayLifetimeScope: Added InputSystemUIInputModule for mobile support.", this);
#endif
            }
        }

        protected override void Configure(IContainerBuilder builder)
        {
            // --- Combat Service ---
            if (turnBasedCombatManagerInstance != null)
            {
                builder.RegisterComponent(turnBasedCombatManagerInstance).As<ICombatService>().AsSelf();
            }
            else
            {
                builder.RegisterComponentInHierarchy<TurnBasedCombatManager>().As<ICombatService>().AsSelf();
            }

            // --- Level Service ---
            if (levelManagerInstance != null)
            {
                builder.RegisterComponent(levelManagerInstance).As<ILevelService>().AsSelf();
            }
            else
            {
                builder.RegisterComponentInHierarchy<LevelManager>().As<ILevelService>().AsSelf();
            }

            // --- Combat Camera ---
            // Critical for transitions.
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
                }
                else
                {
                    // If missing in Gameplay, we still provide a Null implementation to prevent crashes,
                    // though gameplay will be broken.
                    builder.Register<NullCombatCameraManager>(Lifetime.Singleton).As<ICombatCameraManager>().AsSelf();
                    GameLog.LogWarning("GameplayLifetimeScope: CombatCameraManager missing!");
                }
            }

            // --- Gameplay UI ---
            if (gameplayUIManagerInstance != null)
            {
                builder.RegisterComponent(gameplayUIManagerInstance).As<IGameplayUIService>().AsSelf();
            }
            else
            {
                builder.RegisterComponentInHierarchy<GameplayUIManager>().As<IGameplayUIService>().AsSelf();
            }

            // Register Main UIManager (Moved from Global Scope)
            // It manages dynamic panels like Pause, HUD, etc.
            var mainUIManager = FindFirstObjectByType<UIManager>(FindObjectsInactive.Include);
            if (mainUIManager != null)
            {
                builder.RegisterComponent(mainUIManager).As<IUIManager>().AsSelf();
            }
            else
            {
                // Should exist in scene
                GameLog.LogWarning("GameplayLifetimeScope: UIManager not found in scene!");
            }

            // Preload panels when Gameplay starts

            builder.RegisterEntryPoint<PreloadUIPanelsEntryPoint>();

            // --- Player Reference ---
            if (playerReferenceInstance != null)
            {
                builder.RegisterComponent(playerReferenceInstance).As<IPlayerReference>().AsSelf();
            }
            else
            {
                var foundPlayer = FindFirstObjectByType<PlayerReference>(FindObjectsInactive.Include);
                if (foundPlayer != null)
                {
                    builder.RegisterComponent(foundPlayer).As<IPlayerReference>().AsSelf();
                }
                else
                {
                    // Create auto-discovery instance if missing (fallback)
                    var go = new GameObject("[Auto] PlayerReference");
                    var playerRef = go.AddComponent<PlayerReference>();
                    builder.RegisterComponent(playerRef).As<IPlayerReference>().AsSelf();
                    GameLog.Log("GameplayLifetimeScope: Created auto-discovery PlayerReference.");
                }
            }

            // --- Combat Scene Pool ---
            if (combatScenePoolInstance != null)
            {
                builder.RegisterComponent(combatScenePoolInstance).AsSelf();
            }
            else
            {
                var foundPool = FindFirstObjectByType<CombatScenePool>(FindObjectsInactive.Include);
                if (foundPool != null)
                {
                    builder.RegisterComponent(foundPool).AsSelf();
                }
            }

            // --- Combat Transition Manager (Moved from Global) ---
            var combatTransition = FindFirstObjectByType<Santa.Infrastructure.Combat.CombatTransitionManager>(FindObjectsInactive.Include);
            if (combatTransition != null)
            {
                builder.RegisterComponent(combatTransition).As<ICombatTransitionService>().AsSelf();
            }
            else
            {
                GameLog.LogWarning("GameplayLifetimeScope: CombatTransitionManager not found in scene!");
            }

            // Optional: Player Interaction
            var playerInteraction = FindFirstObjectByType<PlayerInteraction>(FindObjectsInactive.Include);
            if (playerInteraction != null)
            {
                builder.RegisterComponent(playerInteraction).AsSelf();
            }

            // Game Initializer (Entry Point for Gameplay Scene)
            var gameInitializer = FindFirstObjectByType<GameInitializer>(FindObjectsInactive.Include);
            if (gameInitializer != null)
            {
                builder.RegisterComponent(gameInitializer).AsSelf();
            }

            // --- Pause Menu ---
            if (pauseMenuControllerInstance != null)
            {
                builder.RegisterComponent(pauseMenuControllerInstance).As<IPauseMenuService>().AsSelf();
            }
            else
            {
                var foundPause = FindFirstObjectByType<Santa.UI.PauseMenuController>(FindObjectsInactive.Include);
                if (foundPause != null)
                {
                    builder.RegisterComponent(foundPause).As<IPauseMenuService>().AsSelf();
                }
            }

            // --- Game State Manager (Gameplay-specific state management) ---
            var gameState = FindFirstObjectByType<Santa.Infrastructure.State.GameStateManager>(FindObjectsInactive.Include);
            if (gameState != null)
            {
                builder.RegisterComponent(gameState).As<IGameStateService>().AsSelf();
            }
            else
            {
                GameLog.LogWarning("GameplayLifetimeScope: GameStateManager not found in scene!");
            }

            // --- Combat Encounter Manager (Manages combat encounters) ---
            var combatEncounter = FindFirstObjectByType<Santa.Infrastructure.Combat.CombatEncounterManager>(FindObjectsInactive.Include);
            if (combatEncounter != null)
            {
                builder.RegisterComponent(combatEncounter).AsSelf();
            }
            else
            {
                GameLog.LogWarning("GameplayLifetimeScope: CombatEncounterManager not found in scene!");
            }

            // --- Upgrade Manager ---
            var upgradeManager = FindFirstObjectByType<Santa.Presentation.Upgrades.UpgradeManager>(FindObjectsInactive.Include);
            if (upgradeManager != null)
            {
                builder.RegisterComponent(upgradeManager).As<IUpgradeService>().AsSelf();
            }
            else
            {
                GameLog.LogWarning("GameplayLifetimeScope: UpgradeManager not found in scene!");
            }

            GameLog.Log("GameplayLifetimeScope CONFIGURED!");
        }

        // Null implementation for safety
        private class NullCombatCameraManager : ICombatCameraManager
        {
            public void SwitchToMainCamera() { }
            public void SwitchToTargetSelectionCamera() { }
            public void SetCombatCameras(Unity.Cinemachine.CinemachineCamera main, Unity.Cinemachine.CinemachineCamera target) { }
            public void DeactivateCameras() { }
        }
    }
}
