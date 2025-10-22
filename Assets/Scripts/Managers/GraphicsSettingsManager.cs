using System.Linq;
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
        GameLog.Log("Platform: PC. Applied default graphics settings (1920x1080, 60 FPS, Windowed).");

#elif UNITY_ANDROID || UNITY_IOS
        // --- MOBILE SPECIFIC LOGIC ---
        Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
        var bestResolution = Screen.resolutions.Last();
        Screen.SetResolution(bestResolution.width, bestResolution.height, true);
        GameLog.Log($"Platform: Mobile. Applied native graphics settings ({bestResolution.width}x{bestResolution.height} @ {Application.targetFrameRate} FPS).");
#endif
    }

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
            GameLog.LogWarning($"GraphicsSettingsManager: Invalid resolution index {resolutionIndex}.");
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
