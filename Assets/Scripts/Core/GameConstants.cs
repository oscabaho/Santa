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

    public static class Addressables
    {
<<<<<<< Updated upstream
        // Add addressable keys here if needed, though UIPanelAddresses exists
=======
        // PC default graphics settings
        public const int DefaultPCWidth = 1920;
        public const int DefaultPCHeight = 1080;
        public const int DefaultTargetFrameRate = 60;
        public const double DefaultMobileRefreshRate = 60d;
    }

    public static class CombatScene
    {
        // Combat scene pool configuration
        public const float OffsetY = -2000f;
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
>>>>>>> Stashed changes
    }
}
