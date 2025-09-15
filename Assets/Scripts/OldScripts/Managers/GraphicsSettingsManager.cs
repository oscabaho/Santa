using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProyectSecret.Managers
{
    /// <summary>
    /// Gestiona la carga, aplicación y guardado de los ajustes gráficos del juego.
    /// </summary>
    public class GraphicsSettingsManager : MonoBehaviour
    {
        public static GraphicsSettingsManager Instance { get; private set; }

        // Claves para PlayerPrefs
        public const string QualityLevelKey = "GraphicsQualityLevel";
        public const string ResolutionIndexKey = "GraphicsResolutionIndex";
        public const string IsFullscreenKey = "GraphicsIsFullscreen";
        public const string VSyncKey = "GraphicsVSync";

        public Resolution[] AvailableResolutions { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SetupResolutions();
        }

        private void Start()
        {
            LoadAndApplySettings();
        }

        private void SetupResolutions()
        {
            // Filtramos para evitar resoluciones duplicadas con diferentes tasas de refresco
            AvailableResolutions = Screen.resolutions.Select(res => new Resolution { width = res.width, height = res.height }).Distinct().ToArray();
        }

        public void SetQuality(int qualityIndex)
        {
            if (qualityIndex < 0 || qualityIndex >= QualitySettings.names.Length) return;
            QualitySettings.SetQualityLevel(qualityIndex, true);
            PlayerPrefs.SetInt(QualityLevelKey, qualityIndex);
            PlayerPrefs.Save();
        }

        public void SetResolution(int resolutionIndex)
        {
            if (resolutionIndex < 0 || resolutionIndex >= AvailableResolutions.Length) return;
            Resolution resolution = AvailableResolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            PlayerPrefs.SetInt(ResolutionIndexKey, resolutionIndex);
            PlayerPrefs.Save();
        }

        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt(IsFullscreenKey, isFullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetVSync(bool enabled)
        {
            QualitySettings.vSyncCount = enabled ? 1 : 0;
            PlayerPrefs.SetInt(VSyncKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void LoadAndApplySettings()
        {
            SetQuality(PlayerPrefs.GetInt(QualityLevelKey, QualitySettings.GetQualityLevel()));
            SetVSync(PlayerPrefs.GetInt(VSyncKey, 1) == 1);

        int defaultResolutionIndex = GetCurrentResolutionIndex();
        int resolutionIndex = PlayerPrefs.GetInt(ResolutionIndexKey, defaultResolutionIndex);
        bool isFullscreen = PlayerPrefs.GetInt(IsFullscreenKey, 1) == 1;

        // Sanity check: If the saved index is invalid for the current display (e.g. different monitor),
        // or if no valid default was found, fall back to a safe default.
        if (resolutionIndex < 0 || resolutionIndex >= AvailableResolutions.Length)
        {
            resolutionIndex = defaultResolutionIndex;
        }

        Screen.SetResolution(AvailableResolutions[resolutionIndex].width, AvailableResolutions[resolutionIndex].height, isFullscreen);
        }

        public int GetCurrentResolutionIndex()
        {
        int currentIndex = System.Array.FindIndex(AvailableResolutions, res => res.width == Screen.width && res.height == Screen.height);
        // If not found, return a safe index (the last one, which is usually the highest resolution).
        return currentIndex < 0 ? AvailableResolutions.Length - 1 : currentIndex;
        }
    }
}