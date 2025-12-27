using Cysharp.Threading.Tasks;

namespace Santa.Core
{
    /// <summary>
    /// Interface for a UI Manager that can show, hide, and switch between UI panels.
    /// </summary>
    public interface IUIManager
{
    /// <summary>
    /// Shows a specific panel without hiding others.
    /// </summary>
    /// <param name="panelAddress">The addressable address of the panel prefab to show.</param>
    UniTask ShowPanel(string panelAddress);

    /// <summary>
    /// Hides a specific panel.
    /// </summary>
    /// <param name="panelAddress">The addressable address of the panel prefab to hide.</param>
    void HidePanel(string panelAddress);

    /// <summary>
    /// Hides all other panels and shows the specified one.
    /// </summary>
    /// <param name="panelAddress">The addressable address of the panel prefab to switch to.</param>
    UniTask SwitchToPanel(string panelAddress);

    /// <summary>
    /// Preloads a panel instance into cache without showing it.
    /// </summary>
    /// <param name="panelAddress">The addressable address of the panel prefab to preload.</param>
    UniTask PreloadPanel(string panelAddress);
    }
}