using UnityEngine;

namespace Santa.Core.Config
{
    public static class UIColors
    {
        // Centralized UI color palette (avoid scattered hard-coded colors)
        public static readonly Color InfoText = new Color(1f, 1f, 1f, 0.9f);
        public static readonly Color Success = new Color32(56, 198, 85, 255); // green
        public static readonly Color Warning = new Color32(255, 193, 7, 255); // amber
        public static readonly Color Error = new Color32(244, 67, 54, 255);   // red

        public static readonly Color ToastBackground = new Color(0f, 0f, 0f, 0.6f);
    }
}
