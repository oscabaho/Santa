using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class GraphicsSettingsController : MonoBehaviour
{
    [Header("UI Elements")]
    // Using `Dropdown` as a fallback when TextMeshPro is not available.
    // If you want prettier dropdowns via TextMeshPro, replace these with `TMP_Dropdown`
    // and install the TextMeshPro package via the Unity Package Manager.
    [SerializeField] private Dropdown qualityDropdown;
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vsyncToggle;

    private void Start()
    {
        var graphicsService = ServiceLocator.Get<IGraphicsSettingsService>();
        if (graphicsService == null)
        {
            Debug.LogError("GraphicsSettingsController: IGraphicsSettingsService no registrado. El panel de opciones no funcionar\u00e1.", this);
            gameObject.SetActive(false);
            return;
        }

        SetupQualityDropdown();
        SetupResolutionDropdown(graphicsService);
        SetupToggles();
        AddListeners(graphicsService);
    }

    private void SetupQualityDropdown()
    {
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(QualitySettings.names.ToList());
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();
    }

    private void SetupResolutionDropdown(IGraphicsSettingsService graphicsService)
    {
        #if UNITY_STANDALONE
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (var res in graphicsService.AvailableResolutions)
        {
            options.Add($"{res.width} x {res.height}");
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = graphicsService.GetCurrentResolutionIndex();
        resolutionDropdown.RefreshShownValue();
        #else
        // On mobile, hide the resolution dropdown as it's not needed.
        if (resolutionDropdown != null) resolutionDropdown.gameObject.SetActive(false);
        #endif
    }

    private void SetupToggles()
    {
        #if UNITY_STANDALONE
        // Fullscreen toggle is only relevant on PC.
        if (fullscreenToggle != null) fullscreenToggle.isOn = Screen.fullScreen;
        #else
        if (fullscreenToggle != null) fullscreenToggle.gameObject.SetActive(false);
        #endif

        // VSync can be relevant on mobile too.
        if (vsyncToggle != null) vsyncToggle.isOn = QualitySettings.vSyncCount > 0;
    }

    private void AddListeners(IGraphicsSettingsService graphicsService)
    {
        if (graphicsService == null)
            return;

        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(index => graphicsService.SetQuality(index));

        if (vsyncToggle != null)
            vsyncToggle.onValueChanged.AddListener(isEnabled => graphicsService.SetVSync(isEnabled));

        #if UNITY_STANDALONE
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(index => graphicsService.SetResolution(index));

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(isFullscreen => graphicsService.SetFullscreen(isFullscreen));
        #endif
    }
}