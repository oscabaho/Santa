using UnityEngine;

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
        // We want this to persist across scenes.
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ApplyPlatformSpecificSettings();
    }

    private void ApplyPlatformSpecificSettings()
    {
#if UNITY_STANDALONE
        // --- PC SPECIFIC LOGIC ---
        Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
        Application.targetFrameRate = 60;
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log("Platform: PC. Applied default graphics settings (1920x1080, 60 FPS, Windowed).");
        #endif

#elif UNITY_ANDROID || UNITY_IOS
        // --- MOBILE SPECIFIC LOGIC ---
        var availableResolutions = Screen.resolutions;
        Resolution selectedResolution;

        if (availableResolutions != null && availableResolutions.Length > 0)
        {
            selectedResolution = availableResolutions[availableResolutions.Length - 1];
        }
        else
        {
            selectedResolution = Screen.currentResolution;

            if (selectedResolution.width == 0 || selectedResolution.height == 0)
            {
                selectedResolution = new Resolution
                {
                    width = Screen.width,
                    height = Screen.height
                };
            }
        }

        double refreshRate = Screen.currentResolution.refreshRateRatio.value;
        if (refreshRate <= 0d)
        {
            refreshRate = selectedResolution.refreshRateRatio.value;
            if (refreshRate <= 0d)
            {
                refreshRate = 60d;
            }
        }

        Application.targetFrameRate = (int)System.Math.Round(refreshRate);
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, true);
        ConfigureMobileOrientation();
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"Platform: Mobile. Applied graphics settings ({selectedResolution.width}x{selectedResolution.height} @ {Application.targetFrameRate} FPS).");
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
