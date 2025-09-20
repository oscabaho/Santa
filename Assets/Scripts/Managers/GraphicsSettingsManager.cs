using UnityEngine;
using System.Linq;

public class GraphicsSettingsManager : MonoBehaviour
{
    public static GraphicsSettingsManager Instance { get; private set; }

    public Resolution[] AvailableResolutions { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        #if UNITY_STANDALONE
        // For PC builds, populate the resolution list for the UI dropdown.
        AvailableResolutions = Screen.resolutions.Where(res => res.refreshRateRatio.value > 59).ToArray();
        #else
        // For mobile, the list is not needed as we should run at native resolution.
        AvailableResolutions = new Resolution[0];
        #endif
    }

    public int GetCurrentResolutionIndex()
    {
        #if UNITY_STANDALONE
        Resolution currentResolution = Screen.currentResolution;
        for (int i = 0; i < AvailableResolutions.Length; i++)
        {
            if (AvailableResolutions[i].width == currentResolution.width &&
                AvailableResolutions[i].height == currentResolution.height)
            {
                return i;
            }
        }
        return AvailableResolutions.Length - 1;
        #else
        return 0; // Not applicable for mobile
        #endif
    }

    public void SetQuality(int qualityIndex)
    {
        if (qualityIndex < 0 || qualityIndex >= QualitySettings.names.Length) return;
        
        QualitySettings.SetQualityLevel(qualityIndex, true);
        Debug.Log($"Quality level set to: {QualitySettings.names[qualityIndex]}");
    }

    public void SetResolution(int resolutionIndex)
    {
        #if UNITY_STANDALONE
        if (resolutionIndex < 0 || resolutionIndex >= AvailableResolutions.Length) return;

        Resolution resolution = AvailableResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        Debug.Log($"Resolution set to: {resolution.width}x{resolution.height}");
        #endif
        // On mobile, this function will do nothing.
    }

    public void SetFullscreen(bool isFullscreen)
    {
        #if UNITY_STANDALONE
        Screen.fullScreen = isFullscreen;
        Debug.Log($"Fullscreen set to: {isFullscreen}");
        #endif
        // Fullscreen is handled differently on mobile, often not user-configurable.
    }

    public void SetVSync(bool isEnabled)
    {
        QualitySettings.vSyncCount = isEnabled ? 1 : 0;
        Debug.Log($"VSync set to: {isEnabled}");
    }
}