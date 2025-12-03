namespace Santa.Core.Addressables
{
    /// <summary>
    /// Centralized Addressable keys for all assets loaded dynamically.
    /// Prevents hardcoded strings and provides compile-time safety.
    /// IMPORTANT: Ensure these keys match your Addressable Group addresses exactly.
    /// </summary>
    public static class AddressableKeys
    {
        /// <summary>
        /// UI Panel prefabs loaded via Addressables
        /// </summary>
        public static class UIPanels
        {
            /// <summary>Mobile virtual gamepad UI</summary>
            public const string VirtualGamepad = "VirtualGamepad";

            /// <summary>Combat UI overlay with stats and buttons</summary>
            public const string CombatUI = "CombatUI";

            /// <summary>Post-combat upgrade selection screen</summary>
            public const string UpgradeUI = "UpgradeUI";

            /// <summary>Pause menu overlay</summary>
            public const string PauseMenu = "PauseMenu";
        }

        /// <summary>
        /// Ability ScriptableObjects loaded via Addressables
        /// </summary>
        public static class Abilities
        {
            /// <summary>Direct single-target attack</summary>
            public const string Direct = "Direct";

            /// <summary>Area-of-effect attack</summary>
            public const string Area = "Area";

            /// <summary>Special powerful attack</summary>
            public const string Special = "Special";

            /// <summary>Action Point recovery ability</summary>
            public const string GainAP = "GainAP";
        }

        /// <summary>
        /// Targeting strategy ScriptableObjects loaded via Addressables
        /// </summary>
        public static class Targeting
        {
            /// <summary>Single enemy selection targeting</summary>
            public const string SingleEnemy = "SingleEnemyTargeting";

            /// <summary>All enemies targeting</summary>
            public const string AllEnemies = "AllEnemiesTargeting";

            /// <summary>Self-targeting</summary>
            public const string Self = "SelfTargeting";
        }

        /// <summary>
        /// Combat arena scene prefabs loaded via Addressables
        /// NOTE: Actual addresses are configured per-encounter in CombatEncounter component
        /// These are example addresses for documentation.
        /// </summary>
        public static class CombatArenas
        {
            // Example addresses (actual values set in CombatEncounter components):
            // "CombatArena_Forest"
            // "CombatArena_Desert"
            // "CombatArena_Cave"

            public const string DefaultArena = "CombatArena_Default";
        }

        /// <summary>
        /// Audio clips and music tracks (if loaded via Addressables)
        /// Currently music is loaded as Resources, but migration to Addressables is recommended
        /// </summary>
        public static class Audio
        {
            // Future: Migrate AudioManager to use Addressables instead of Resources
            // public const string CombatMusic = "Combat_Theme";
            // public const string ExplorationMusic = "Exploration_Theme";
        }

        /// <summary>
        /// VFX prefabs loaded via Addressables (if applicable)
        /// </summary>
        public static class VFX
        {
            // public const string HitEffect = "VFX_Hit";
            // public const string ExplosionEffect = "VFX_Explosion";
        }
    }
}
