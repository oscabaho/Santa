using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using ProyectSecret.Managers;
using TMPro; // Usaremos TextMeshPro para mejor calidad de texto

namespace ProyectSecret.UI
{
    /// <summary>
    /// Gestiona los elementos de la UI para los ajustes gráficos.
    /// </summary>
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
            resolutionDropdown.ClearOptions();
            List<string> options = new List<string>();
            foreach (var res in GraphicsSettingsManager.Instance.AvailableResolutions)
            {
                options.Add($"{res.width} x {res.height}");
            }
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = GraphicsSettingsManager.Instance.GetCurrentResolutionIndex();
            resolutionDropdown.RefreshShownValue();
        }

        private void SetupToggles()
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            vsyncToggle.isOn = QualitySettings.vSyncCount > 0;
        }

        private void AddListeners()
        {
            qualityDropdown.onValueChanged.AddListener(index => GraphicsSettingsManager.Instance.SetQuality(index));
            resolutionDropdown.onValueChanged.AddListener(index => GraphicsSettingsManager.Instance.SetResolution(index));
            fullscreenToggle.onValueChanged.AddListener(isFullscreen => GraphicsSettingsManager.Instance.SetFullscreen(isFullscreen));
            vsyncToggle.onValueChanged.AddListener(isEnabled => GraphicsSettingsManager.Instance.SetVSync(isEnabled));
        }
    }
}