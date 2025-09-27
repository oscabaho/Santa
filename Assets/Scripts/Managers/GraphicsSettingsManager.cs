using System.Linq;
using UnityEngine;

/// <summary>
/// Manages the game's graphics settings. Behaves differently based on the platform.
/// </summary>
public class GraphicsSettingsManager : MonoBehaviour, IGraphicsSettingsService
{
    public static GraphicsSettingsManager Instance { get; private set; }

    /// <summary>
    /// Gets the list of resolutions supported by the current display.
    /// </summary>
    public Resolution[] AvailableResolutions => Screen.resolutions;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Register with the Service Locator
        ServiceLocator.Register<IGraphicsSettingsService>(this);
    }

    private void Start()
    {
        ApplyPlatformSpecificSettings();
    }

    private void OnDestroy()
    {
        // Unregister from the Service Locator when destroyed
        if (Instance == this)
        {
            ServiceLocator.Unregister<IGraphicsSettingsService>();
        }
    }

    private void ApplyPlatformSpecificSettings()
    {
#if UNITY_STANDALONE
        // --- PC SPECIFIC LOGIC ---
        // This is where you would load user preferences and apply them.
        // For now, we'll set a default windowed resolution.
        Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
        Application.targetFrameRate = 60; // A sensible default for PC
        Debug.Log("Platform: PC. Applied default graphics settings (1920x1080, 60 FPS, Windowed).");

#elif UNITY_ANDROID || UNITY_IOS
        // --- MOBILE SPECIFIC LOGIC ---
        // On mobile, we want to use the device's native capabilities.
        
        // Set target frame rate to the device's refresh rate for smoothest performance.
        // Screen.currentResolution.refreshRateRatio provides a more precise value on modern devices.
        Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;

        // Set the resolution to the highest available resolution for the device.
        // This is generally the best default for a crisp image.
        var bestResolution = Screen.resolutions.Last();
        Screen.SetResolution(bestResolution.width, bestResolution.height, true);

        Debug.Log($"Platform: Mobile. Applied native graphics settings ({bestResolution.width}x{bestResolution.height} @ {Application.targetFrameRate} FPS).");
#endif
    }

    /// <summary>
    /// Sets the game to be fullscreen or windowed.
    /// </summary>
    /// <param name="isFullscreen">True for fullscreen, false for windowed.</param>
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    /// <summary>
    /// Gets the index of the current resolution in the list of supported resolutions.
    /// </summary>
    /// <returns>The index of the current resolution.</returns>
    public int GetCurrentResolutionIndex()
    {
        Resolution[] resolutions = Screen.resolutions;
        Resolution currentResolution = Screen.currentResolution;

        // Find the index of the resolution that matches the current screen width and height.
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == currentResolution.width && 
                resolutions[i].height == currentResolution.height)
            {
                // Note: This returns the first match and may not be unique if multiple
                // refresh rates exist for the same resolution.
                return i;
            }
        }

        // Fallback, should ideally not be reached.
        return resolutions.Length - 1; 
    }

    /// <summary>
    /// Sets the graphics quality level.
    /// </summary>
    /// <param name="qualityIndex">The index of the quality level to set.</param>
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex, true);
    }

    /// <summary>
    /// Sets the screen resolution based on an index from the AvailableResolutions array.
    /// </summary>
    /// <param name="resolutionIndex">The index of the resolution to set.</param>
    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex < 0 || resolutionIndex >= AvailableResolutions.Length)
        {
            Debug.LogWarning($"GraphicsSettingsManager: Invalid resolution index {resolutionIndex}.");
            return;
        }
        Resolution resolution = AvailableResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    /// <summary>
    /// Enables or disables VSync.
    /// </summary>
    /// <param name="isVsyncOn">True to enable VSync, false to disable.</param>
    public void SetVSync(bool isVsyncOn)
    {
        // 0 = off, 1 = on (every frame)
        QualitySettings.vSyncCount = isVsyncOn ? 1 : 0;
    }
}
