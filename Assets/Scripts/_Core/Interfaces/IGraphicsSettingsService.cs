using UnityEngine;

/// <summary>
/// Interface exposing graphics configuration operations in a decoupled way.
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
