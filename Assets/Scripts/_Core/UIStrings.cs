namespace Santa.Core.Config
{
    // Centralized user-facing UI strings and templates (localization-ready)
    public static class UIStrings
    {
        // Labels
        public const string SelectTarget = "Select a target";

        // Formats
        public const string ActionPointsLabelFormat = "PA: {0}";

        // Upgrade UI
        public const string UpgradeTitle = "Choose Your Upgrade";

        // Common Titles
        public const string PauseTitle = "Paused";
        public const string SettingsTitle = "Settings";
        public const string AudioTitle = "Audio";
        public const string GraphicsTitle = "Graphics";

        // Common Buttons
        public const string ConfirmButtonText = "Confirm";
        public const string CancelButtonText = "Cancel";
        public const string ApplyButtonText = "Apply";
        public const string CloseButtonText = "Close";

        // (Intentionally no numeric health or level strings; health uses sliders, and levels are decorative.)
    }
}
