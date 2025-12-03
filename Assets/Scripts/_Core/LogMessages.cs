namespace Santa.Core.Config
{
    /// <summary>
    /// Centralized log messages to avoid hardcoded strings and ensure consistency.
    /// </summary>
    public static class LogMessages
    {
        public static class Upgrades
        {
            public const string ConfigNotAssigned = "UpgradeManager: PlayerStatsConfig is not assigned! Using centralized defaults.";
            public const string NotEnoughUpgrades = "Not enough upgrades defined in UpgradeManager to offer a choice.";
            public const string NotEnoughUniqueUpgrades = "Not enough unique upgrades to offer a choice. Only {0} available.";
            public const string OfferingUpgrades = "Offering upgrades: {0} vs {1}";
            public const string ApplyingUpgrade = "Applying upgrade: {0}";
            public const string LastUpgradeLoaded = "Last selected upgrade loaded: {0}";
            public const string ResetLastUpgrade = "Reset last selected upgrade.";
        }

        public static class CombatUI
        {
            public const string AwakeCalled = "CombatUI.Awake called.";
            public const string ConstructCalled = "CombatUI.Construct called.";
            public const string WaitingForService = "CombatUI: Waiting for DI to complete (ICombatService not yet injected).";
            public const string ActionButtonsNotAssigned = "ActionButtonsPanel is not assigned in the inspector.";
            public const string PlayerNotAvailable = "CombatUI: Player is not available yet; will subscribe when combat initializes.";
            public const string PlayerNull = "CombatUI: Service returned null player; deferring player subscription.";
            public const string PlayerMissingRegistry = "Player is missing an IComponentRegistry.";
            public const string TargetSubmitFailed = "Cannot submit target: CombatService not in Targeting phase or is null.";
        }

        public static class CombatCamera
        {
            public const string FailedToResolveCombatService = "CombatCameraManager: Failed to resolve ICombatService. {0}";
        }

        public static class CombatTransition
        {
            public const string ExplorationPlayerNotFound = "CombatTransitionManager: Could not find the exploration player via the ExplorationPlayerIdentifier component.";
            public const string MainCameraNotFound = "CombatTransitionManager: Could not find the main camera.";
            public const string CombatPlayerNotFound = "CombatTransitionManager: Could not find the combat player via the CombatPlayerIdentifier component within {0}.";
            public const string CameraManagerNotInjected = "CombatTransitionManager: ICombatCameraManager not injected; cameras will rely on fallback tag search inside CombatCameraManager.";
            public const string NoArenaSettings = "CombatTransitionManager: No CombatArenaSettings found on {0}. CombatCameraManager will attempt fallback tag search.";
            public const string EndCombatNoContext = "EndCombat was called but there is no active combat or context.";
            public const string PlayerDefeatedRespawning = "Player defeated. Respawning at {0}.";
            public const string PlayerDefeatedNoRespawn = "Player defeated but no Respawn Point assigned (or player null).";
        }

        public static class CombatEncounter
        {
            public const string NullEncounter = "CombatEncounterManager: Cannot start null encounter.";
            public const string PoolInstanceFailed = "CombatEncounterManager: Failed to get instance from pool.";
            public const string NoArenaFound = "CombatEncounterManager: No CombatArena found in '{0}'.";
            public const string NoParticipants = "CombatEncounterManager: No participants found in arena.";
            public const string EncounterException = "CombatEncounterManager: Exception during encounter: {0}";
        }

        public static class TurnBasedCombat
        {
            public const string ExecutorNotFound = "Could not find IActionExecutor in children of {0}.";
            public const string AIManagerNotFound = "Could not find IAIManager in children of {0}.";
            public const string CombatStarted = "═══ COMBAT STARTED ═══";
            public const string ExecutionPhaseFinished = "Execution phase finished.";
            public const string CombatVictory = "═══ COMBAT ENDED: VICTORY ═══";
            public const string CombatDefeat = "═══ COMBAT ENDED: DEFEAT ═══";
        }

        public static class UI
        {
            public const string PanelAddressNull = "UIManager: Panel address is null or empty.";
            public const string CachedPanelMissingComponent = "UIManager: Cached panel with address '{0}' is missing its UIPanel component. The panel will not be shown.";
            public const string PanelLoadedAndShown = "UIManager: Panel with address '{0}' loaded and shown.";
            public const string PrefabMissingComponent = "UIManager: Prefab with address '{0}' does not have a UIPanel component on its root object. The panel will not be shown.";
            public const string LoadFailed = "UIManager: Failed to load panel with address '{0}'. Status: {1}";
            public const string LoadException = "UIManager: Exception loading panel '{0}': {1}";
            public const string PanelAddressNullHide = "UIManager: Panel address is null or empty for hiding.";
            public const string PanelHidden = "UIManager: Panel with address '{0}' hidden.";
            public const string PanelNotFound = "UIManager: Panel with address '{0}' not found or not instantiated.";
            public const string PanelAddressNullSwitch = "UIManager: Panel address is null or empty for switching.";
            public const string PanelAddressNullPreload = "UIManager: Panel address is null or empty for preloading.";
            public const string PanelPreloaded = "UIManager: Panel '{0}' preloaded and hidden.";
            public const string PreloadMissingComponent = "UIManager: Prefab with address '{0}' is missing UIPanel component on root.";
            public const string PreloadFailed = "UIManager: Failed to preload panel with address '{0}'. Status: {1}";
            public const string PreloadException = "UIManager: Exception preloading panel '{0}': {1}";
        }
    }
}
