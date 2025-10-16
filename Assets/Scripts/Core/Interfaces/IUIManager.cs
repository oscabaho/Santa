using System.Threading.Tasks;

/// <summary>
/// Interface for a UI Manager that can show, hide, and switch between UI panels.
/// </summary>
public interface IUIManager
{
    /// <summary>
    /// Shows a specific panel without hiding others.
    /// </summary>
    /// <param name="panelId">The ID of the panel to show.</param>
    Task ShowPanel(string panelId);

    /// <summary>
    /// Hides a specific panel.
    /// </summary>
    /// <param name="panelId">The ID of the panel to hide.</param>
    void HidePanel(string panelId);

    /// <summary>
    /// Hides all other panels and shows the specified one.
    /// </summary>
    /// <param name="panelId">The ID of the panel to switch to.</param>
    Task SwitchToPanel(string panelId);
}