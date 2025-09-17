using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class GraphicsSettingsController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vsyncToggle;

    private void Start()
    {
        if (GraphicsSettingsManager.Instance == null)
        {
            Debug.LogError("GraphicsSettingsController: GraphicsSettingsManager no encontrado. El panel de opciones no funcionará.", this);
            gameObject.SetActive(false);
            return;
        }

        SetupQualityDropdown();
        SetupResolutionDropdown();
        SetupToggles();
        AddListeners();
    }

    private void SetupQualityDropdown()
    {
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(QualitySettings.names.ToList());
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();
    }

    private void SetupResolutionDropdown()
    {
        #if UNITY_STANDALONE
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (var res in GraphicsSettingsManager.Instance.AvailableResolutions)
        {
            options.Add($"{res.width} x {res.height}");
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = GraphicsSettingsManager.Instance.GetCurrentResolutionIndex();
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

    private void AddListeners()
    {
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(index => GraphicsSettingsManager.Instance.SetQuality(index));

        if (vsyncToggle != null)
            vsyncToggle.onValueChanged.AddListener(isEnabled => GraphicsSettingsManager.Instance.SetVSync(isEnabled));

        #if UNITY_STANDALONE
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(index => GraphicsSettingsManager.Instance.SetResolution(index));

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(isFullscreen => GraphicsSettingsManager.Instance.SetFullscreen(isFullscreen));
        #endif
    }
}
