using System.Linq;
using UnityEngine;

/// <summary>
/// Manages the game's graphics settings. Behaves differently based on the platform.
/// Ensures it is created and registered before any scene loads.
/// </summary>
public class GraphicsSettingsManager : MonoBehaviour, IGraphicsSettingsService
{
    private static GraphicsSettingsManager _instance;

    /// <summary>
    /// Gets the singleton instance of the GraphicsSettingsManager.
    /// </summary>
    public static GraphicsSettingsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // This will be handled by the RuntimeInitializeOnLoadMethod,
                // but as a fallback, we can try to find it.
                _instance = FindFirstObjectByType<GraphicsSettingsManager>();
                if (_instance == null)
                {
                    GameLog.LogError("GraphicsSettingsManager instance is required in the scene, but none was found and couldn't be created automatically.");
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Gets the list of resolutions supported by the current display.
    /// </summary>
    public Resolution[] AvailableResolutions => Screen.resolutions;

    // This method is called by the Unity runtime before the first scene is loaded.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (_instance == null)
        {
            _instance = FindFirstObjectByType<GraphicsSettingsManager>();
            if (_instance == null)
            {
                GameObject go = new GameObject("GraphicsSettingsManager");
                _instance = go.AddComponent<GraphicsSettingsManager>();
            }
        }
    }

    private void Awake()
    {
        // Singleton pattern enforcement
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
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
        if (_instance == this)
        {
            ServiceLocator.Unregister<IGraphicsSettingsService>();
        }
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
