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
        /// Audio clips and music tracks (if loaded via Addressables)
        /// Currently music is loaded as Resources, but migration to Addressables is recommended
        /// </summary>
        public static class Audio
        {
            // Music tracks
            public const string CombatMusic = "Combat_Theme";
            public const string ExplorationMusic = "Exploration_Theme";
            public const string BossMusic = "Boss_Theme";
            public const string MenuMusic = "Menu_Theme";
            
            // Sound effects categories
            public const string PlayerAttackSFX = "SFX_Player_Attack";
            public const string EnemyHitSFX = "SFX_Enemy_Hit";
            public const string UISFX = "SFX_UI_Click";
            public const string AmbientSFX = "SFX_Ambient";
        }

        /// <summary>
        /// VFX prefabs loaded via Addressables (if applicable)
        /// </summary>
        public static class VFX
        {
            public const string HitEffect = "VFX_Hit";
            public const string ExplosionEffect = "VFX_Explosion";
            public const string HealEffect = "VFX_Heal";
            public const string LevelUpEffect = "VFX_LevelUp";
            public const string SpawnEffect = "VFX_Spawn";
            public const string DeathEffect = "VFX_Death";
        }

        /// <summary>
        /// Enemy prefabs loaded via Addressables
        /// </summary>
        public static class Enemies
        {
            public const string CombatEnemy_01 = "CombatEnemy_01";
            public const string CombatEnemy_02 = "CombatEnemy_02";
            public const string CombatEnemy_03 = "CombatEnemy_03";
            public const string ExplorationEnemy_01 = "ExplorationEnemy_01";
            public const string ExplorationEnemy_02 = "ExplorationEnemy_02";
            public const string ExplorationEnemy_03 = "ExplorationEnemy_03";
        }

        /// <summary>
        /// Level-specific assets loaded via Addressables
        /// </summary>
        public static class Environments
        {
            public const string Exploration = "Exploration";
            public const string CombatArena_01 = "CombatArena_01";
            public const string CombatArena_02 = "CombatArena_02";
            public const string CombatArena_03 = "CombatArena_03";
        }
        /// <summary>
        /// Decorative assets loaded via Addressables
        /// </summary>
        public static class Derorations
        {
            public const string Decor_American_01 = "Decor_American_01";
            public const string Decor_American_02 = "Decor_American_02";
            public const string Decor_American_03 = "Decor_American_03";
            public const string Decor_Christmas_01 = "Decor_Christmas_01";
            public const string Decor_Christmas_02 = "Decor_Christmas_02";
            public const string Decor_Christmas_03 = "Decor_Christmas_03";
        }

        /// <summary>
        /// Large texture assets and sprites loaded via Addressables
        /// </summary>
        public static class Materials
        {
            public const string Amarillo_01 = "Amarillo_01";
            public const string Amarillo_02 = "Amarillo_02";
            public const string Azul_01 = "Azul_01";
            public const string Casa_01 = "Casa_01";
            public const string Casa_02 = "Casa_02";
            public const string Casa_03 = "Casa_03";
            public const string Casa_04 = "Casa_04";
            public const string Casa_05 = "Casa_05";
            public const string Casa_06 = "Casa_06";
            public const string Casa_07 = "Casa_07";
            public const string Cemento_01 = "Cemento_01";
            public const string Edificio_01 = "Edificio_01";
            public const string Edificio_02 = "Edificio_02";
            public const string Grid = "Grid";
            public const string Lazo_01 = "Lazo_01";
            public const string Luz_01 = "Luz_01";
            public const string Marco_01 = "Marco_01";
            public const string Matera_01 = "Matera_01";
            public const string Piso_01 = "Piso_01";
            public const string Piso_02 = "Piso_02";
            public const string Puerta_01 = "Puerta_01";
            public const string Regalo_01 = "Regalo_01";
            public const string Rojo_01 = "Rojo_01";
            public const string Rosa_01 = "Rosa_01";
            public const string Semaforo_01 = "Semaforo_01";
            public const string Semaforo_02 = "Semaforo_02";
            public const string Semaforo_03 = "Semaforo_03";
            public const string Techo_01 = "Techo_01";
            public const string Techo_02 = "Techo_02";
            public const string Techo_03 = "Techo_03";
            public const string Techo_04 = "Techo_04";
            public const string Techo_05 = "Techo_05";
            public const string Ventanas_01 = "Ventanas_01";
            public const string Ventanas_02 = "Ventanas_02";
            public const string Ventanas_03 = "Ventanas_03";
            public const string Verde_01 = "Verde_01";
        }
        /// <summary>
        /// Data assets and large databases loaded via Addressables
        /// </summary>
        public static class Upgrades
        {
            public const string AllUpgrades = "Data_AllUpgrades";
            public const string EnemyDatabase = "Data_EnemyDatabase";
            public const string DialogueDatabase = "Data_Dialogues";
        }

        public static class Changeables
        {
            public const string Decor_Enemy_01 = "Decor_Enemy_01";
            public const string Decor_Enemy_02 = "Decor_Enemy_02";
            public const string Decor_Enemy_03 = "Decor_Enemy_03";
        }

        public static class Stats
        {
            public const string NormalStats = "NormalStats";
        }

        public static class Strategies
        {
            public const string Speed = "Speed";
            public const string Critical = "Critical";
            public const string Damage = "Damage";
            public const string MaxHealth = "MaxHealth";
            public const string CostAP_Reduce = "CostAP_Reduce";
            public const string MissChance_Reduce = "MissChance_Reduce";
        }
    }
}
