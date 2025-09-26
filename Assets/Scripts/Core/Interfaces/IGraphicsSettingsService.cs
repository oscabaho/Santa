using UnityEngine;

/// <summary>
/// Interfaz para exponer operaciones de configuración gráfica de forma desacoplada.
/// </summary>
public interface IGraphicsSettingsService
{
    Resolution[] AvailableResolutions { get; }
    int GetCurrentResolutionIndex();
    void SetQuality(int qualityIndex);
    void SetResolution(int resolutionIndex);
    void SetFullscreen(bool isFullscreen);
    void SetVSync(bool isEnabled);
}
