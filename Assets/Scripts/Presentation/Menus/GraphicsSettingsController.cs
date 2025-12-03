using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Santa.Core;
using VContainer;

namespace Santa.Presentation.Menus
{

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

    private IGraphicsSettingsService _graphicsService;

    [Inject]
    public void Construct(IGraphicsSettingsService graphicsService)
    {
        _graphicsService = graphicsService;
    }

    private void Start()
    {
        if (_graphicsService == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("GraphicsSettingsController: IGraphicsSettingsService not registered. Options panel will not function.", this);
#endif
            gameObject.SetActive(false);
            return;
        }

        // Verify whether UI elements are assigned.
        // If none are present this component does nothing (useful for test or mobile scenes without options UI).
        if (qualityDropdown == null && resolutionDropdown == null && fullscreenToggle == null && vsyncToggle == null)
        {
            // On mobile or simple scenes, it's valid to omit these UI elements.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("GraphicsSettingsController: No UI elements assigned. Disabling component (intended on mobile/test scenes).", this);
#endif
            enabled = false;
            return;
        }

        SetupQualityDropdown();
        SetupResolutionDropdown(_graphicsService);
        SetupToggles();
        AddListeners(_graphicsService);
    }

    private void SetupQualityDropdown()
    {
        if (qualityDropdown == null) return;
        
        qualityDropdown.ClearOptions();
        // Avoid ToList allocation: AddOptions accepts List<string>
        var names = QualitySettings.names;
        var namesList = new List<string>(names.Length);
        for (int i = 0; i < names.Length; i++)
        {
            namesList.Add(names[i]);
        }
        qualityDropdown.AddOptions(namesList);
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();
    }

    private void SetupResolutionDropdown(IGraphicsSettingsService graphicsService)
    {
        if (resolutionDropdown == null) return;
        
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
        resolutionDropdown.gameObject.SetActive(false);
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
}