namespace Santa.Core.Config
{
    public static class GameConstants
    {
        public static class Tags
        {
            public const string Player = "Player";
            public const string Enemy = "Enemy";
            public const string MainCombatCamera = "MainCombatCamera";
            public const string TargetSelectionCamera = "TargetSelectionCamera";
        }

    public static class Scenes
    {
        // Add scene names here if needed
    }

    /// <summary>
    /// Contains constants for scene hierarchy GameObject names.
    /// </summary>
    public static class Hierarchy
    {
        public const string Managers = "Managers";
        public const string Services = "Services";
        public const string UI = "UI";
        public const string Cameras = "Cameras";
        public const string Actors = "Actors";
        public const string Environment = "Environment";
        public const string Pools = "Pools";
        public const string DynamicPanels = "DynamicPanels";
        public const string CombatCameras = "CombatCameras";
        public const string StaticCanvas = "StaticCanvas";
    }

    /// <summary>
    /// Contains constants for PlayerPrefs key names.
    /// </summary>
    public static class PlayerPrefsKeys
    {
        public const string MusicVolume = "MusicVolume";
        public const string SfxVolume = "SfxVolume";
    }

    public static class Graphics
    {
        // PC default graphics settings
        public const int DefaultPCWidth = 1920;
        public const int DefaultPCHeight = 1080;
        public const int DefaultTargetFrameRate = 60;
        public const double DefaultMobileRefreshRate = 60d;
    }

    public static class Addressables
    {
        // Addressable keys
    }

    public static class CombatScene
    {
        // Combat scene pool configuration
        public const float OffsetY = -20f;
    }

    public static class PlayerStats
    {
        // Default player stats (fallback values when config is missing)
        public const int DefaultDirectAttackDamage = 25;
        public const int DefaultAreaAttackDamage = 10;
        public const int DefaultSpecialAttackDamage = 75;
        public const float DefaultSpecialAttackMissChance = 0.2f;
        public const int DefaultAPRecoveryAmount = 34;
        public const int DefaultMaxActionPoints = 100;
        public const int DefaultMaxHealth = 100;
        public const int DefaultGlobalAPCostReduction = 0;
        public const int DefaultGlobalActionSpeedBonus = 0;
        public const float DefaultBaseCriticalHitChance = 0.1f;

        // Validation constants
        public const float MaxPlayerPositionMagnitude = 10000f;
    }
    }
}
