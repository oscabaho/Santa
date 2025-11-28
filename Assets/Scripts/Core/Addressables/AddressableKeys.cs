namespace Santa.Core.Addressables
{
    // Centralized Addressables keys with logical grouping.
    public static class AddressableKeys
    {
        public static class UIPanels
        {
            public const string VirtualGamepad = global::UIPanelAddresses.VirtualGamepad;
            public const string CombatUI = global::UIPanelAddresses.CombatUI;
            public const string UpgradeUI = global::UIPanelAddresses.UpgradeUI;
            public const string PauseMenu = "PauseMenu"; // Ensure prefab Addressable has this key
        }

        public static class Abilities
        {
            public const string Direct = global::AbilityAddresses.Direct;
            public const string Area = global::AbilityAddresses.Area;
            public const string Special = global::AbilityAddresses.Special;
            public const string GainAP = global::AbilityAddresses.GainAP;
            public const string SingleEnemyTargeting = global::AbilityAddresses.SingleEnemyTargeting;
        }
    }
}
