using Santa.Core;
using Santa.Core.Player;
using Santa.Infrastructure.Camera;
using Santa.Infrastructure.Combat;
using Santa.Infrastructure.Level;
using Santa.Presentation.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Santa.Core.DI
{
    /// <summary>
    /// LifetimeScope specific to the Gameplay scene.
    /// Manages dependencies that only exist during gameplay (Combat, Level, Player, Camera).
    /// Inherits from the parent GameLifetimeScope automatically if set up correctly in the Inspector.
    /// </summary>
    public class GameplayLifetimeScope : LifetimeScope
    {
        [Header("Gameplay Specific Assignments")]
        [SerializeField]
        private TurnBasedCombatManager turnBasedCombatManagerInstance;

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
