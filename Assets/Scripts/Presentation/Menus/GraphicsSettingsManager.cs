using Santa.Core;
using Santa.Core.Config;
using UnityEngine;

namespace Santa.Presentation.Menus
{

    /// <summary>
    /// Manages the game's graphics settings. Behaves differently based on the platform.
    /// </summary>
    public class GraphicsSettingsManager : MonoBehaviour, IGraphicsSettingsService
    {
        /// <summary>
        /// Gets the list of resolutions supported by the current display.
        /// </summary>
        public Resolution[] AvailableResolutions => Screen.resolutions;

        private void Awake()
        {
            // Persist across scenes only when this is a root object
            // (Unity requires DontDestroyOnLoad targets to be at the scene root).
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            ApplyPlatformSpecificSettings();
        }

        private void ApplyPlatformSpecificSettings()
        {
#if UNITY_STANDALONE
        // --- PC SPECIFIC LOGIC ---
        Screen.SetResolution(GameConstants.Graphics.DefaultPCWidth, GameConstants.Graphics.DefaultPCHeight, FullScreenMode.Windowed);
        Application.targetFrameRate = GameConstants.Graphics.DefaultTargetFrameRate;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"Platform: PC. Applied default graphics settings ({GameConstants.Graphics.DefaultPCWidth}x{GameConstants.Graphics.DefaultPCHeight}, {GameConstants.Graphics.DefaultTargetFrameRate} FPS, Windowed).");
#endif

#elif UNITY_ANDROID || UNITY_IOS
            // --- MOBILE SPECIFIC LOGIC ---
            // On mobile, we generally want to respect the device's native resolution and orientation.
            // Forcing a specific resolution from Screen.resolutions can cause stretching if the aspect ratio doesn't match
            // the current device orientation, or if the list contains portrait resolutions while we are in landscape.

            ConfigureMobileOrientation();

            Resolution current = Screen.currentResolution;
            double refreshRate = current.refreshRateRatio.value;

            // Fallback for invalid refresh rates
            if (refreshRate <= 0d)
            {
                refreshRate = GameConstants.Graphics.DefaultMobileRefreshRate;
            }

            Application.targetFrameRate = (int)System.Math.Round(refreshRate);

            // Force highest quality available on mobile as requested
            int maxQuality = QualitySettings.names.Length - 1;
            QualitySettings.SetQualityLevel(maxQuality, true);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"Platform: Mobile. config: {current.width}x{current.height} @ {Application.targetFrameRate} FPS. Quality Level: {QualitySettings.names[maxQuality]} (Max). Orientation set to Auto (Landscape).");
#endif
#endif
        }

#if UNITY_ANDROID || UNITY_IOS
        private void ConfigureMobileOrientation()
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }
#endif

        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }

        public int GetCurrentResolutionIndex()
        {
            Resolution[] resolutions = Screen.resolutions;
            Resolution currentResolution = Screen.currentResolution;
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == currentResolution.width &&
                    resolutions[i].height == currentResolution.height)
                {
                    return i;
                }
            }
            return resolutions.Length - 1;
        }

        public void SetQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex, true);
        }

        public void SetResolution(int resolutionIndex)
        {
            if (resolutionIndex < 0 || resolutionIndex >= AvailableResolutions.Length)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"GraphicsSettingsManager: Invalid resolution index {resolutionIndex}.");
#endif
                return;
            }
            Resolution resolution = AvailableResolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }

        public void SetVSync(bool isVsyncOn)
        {
            QualitySettings.vSyncCount = isVsyncOn ? 1 : 0;
        }
    }
}
